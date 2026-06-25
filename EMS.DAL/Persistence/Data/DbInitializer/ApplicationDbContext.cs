using EMS.DAL.Entities;
using EMS.DAL.Entities.AttendanceEntity;
using EMS.DAL.Entities.AuditEntity;
using EMS.DAL.Entities.DepartmentEntity;
using EMS.DAL.Entities.EmployeeEntity;
using EMS.DAL.Entities.IdentityModel;
using EMS.DAL.Entities.Location;
using EMS.DAL.Entities.LeaveEntity;
using EMS.DAL.Entities.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;

namespace EMS.DAL.Persistence.Data.DbInitializer
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;
        private bool _isSavingAudit;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor? httpContextAccessor = null) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var isDeletedProp = entityType.ClrType.GetProperty("IsDeleted");

                if (isDeletedProp != null && isDeletedProp.PropertyType == typeof(bool))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var property = Expression.Property(parameter, "IsDeleted");
                    var isDeletedFalse = Expression.Not(property);
                    var lambda = Expression.Lambda(isDeletedFalse, parameter);

                    modelBuilder.Entity(entityType.ClrType)
                                .HasQueryFilter(lambda);
                }
            }

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            modelBuilder.Entity<ApplicationUser>()
                        .HasIndex(u => u.NormalizedEmail)
                        .IsUnique();

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Department> Departments { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<PasswordResetTokens> PasswordResetTokens { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<CompanyLocation> CompanyLocations { get; set; }
        public DbSet<EmployeeProfileRequest> EmployeeProfileRequests { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        public override int SaveChanges()
        {
            if (_isSavingAudit)
                return base.SaveChanges();

            var auditEntries = PrepareAuditEntries();
            var result = base.SaveChanges();
            if (result > 0)
                SaveAuditEntries(auditEntries);

            return result;
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (_isSavingAudit)
                return await base.SaveChangesAsync(cancellationToken);

            var auditEntries = PrepareAuditEntries();
            var result = await base.SaveChangesAsync(cancellationToken);
            if (result > 0)
                await SaveAuditEntriesAsync(auditEntries, cancellationToken);

            return result;
        }

        private List<PendingAuditEntry> PrepareAuditEntries()
        {
            ChangeTracker.DetectChanges();

            var auditEntries = new List<PendingAuditEntry>();
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is not BaseEntity entity ||
                    entry.Entity is AuditLog ||
                    entry.State is EntityState.Detached or EntityState.Unchanged)
                    continue;

                if (entry.State == EntityState.Added)
                    entity.CreatedOn ??= now;

                if (entry.State == EntityState.Modified)
                    entity.LastModifiedOn = now;

                var auditEntry = CreateAuditEntry(entry, now);
                if (auditEntry is not null)
                    auditEntries.Add(auditEntry);
            }

            return auditEntries;
        }

        private PendingAuditEntry? CreateAuditEntry(EntityEntry entry, DateTime changedDate)
        {
            var action = GetAuditAction(entry);
            if (action is null)
                return null;

            var auditEntry = new PendingAuditEntry(entry)
            {
                UserId = GetCurrentUserId(),
                UserEmail = GetCurrentUserEmail(),
                TableName = entry.Metadata.GetTableName() ?? entry.Metadata.ClrType.Name,
                Action = action,
                ChangedDate = changedDate,
                SourceIp = _httpContextAccessor?.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? string.Empty
            };

            var databaseValues = entry.State == EntityState.Added ? null : entry.GetDatabaseValues();

            foreach (var property in entry.Properties)
            {
                if (property.Metadata.IsPrimaryKey())
                    continue;

                var propertyName = property.Metadata.Name;

                if (entry.State == EntityState.Added)
                {
                    auditEntry.NewValues[propertyName] = property.CurrentValue;
                    continue;
                }

                var oldValue = databaseValues?[propertyName] ?? property.OriginalValue;

                if (entry.State == EntityState.Deleted)
                {
                    auditEntry.OldValues[propertyName] = oldValue;
                    continue;
                }

                var newValue = property.CurrentValue;
                if (!Equals(oldValue, newValue))
                {
                    auditEntry.OldValues[propertyName] = oldValue;
                    auditEntry.NewValues[propertyName] = newValue;
                }
            }

            if (entry.State == EntityState.Modified && auditEntry.OldValues.Count == 0 && auditEntry.NewValues.Count == 0)
                return null;

            return auditEntry;
        }

        private void SaveAuditEntries(List<PendingAuditEntry> auditEntries)
        {
            if (auditEntries.Count == 0)
                return;

            _isSavingAudit = true;
            try
            {
                AuditLogs.AddRange(auditEntries.Select(e => e.ToAuditLog()));
                base.SaveChanges();
            }
            finally
            {
                _isSavingAudit = false;
            }
        }

        private async Task SaveAuditEntriesAsync(List<PendingAuditEntry> auditEntries, CancellationToken cancellationToken)
        {
            if (auditEntries.Count == 0)
                return;

            _isSavingAudit = true;
            try
            {
                await AuditLogs.AddRangeAsync(auditEntries.Select(e => e.ToAuditLog()), cancellationToken);
                await base.SaveChangesAsync(cancellationToken);
            }
            finally
            {
                _isSavingAudit = false;
            }
        }

        private string? GetAuditAction(EntityEntry entry)
        {
            if (entry.State == EntityState.Added)
                return "Create";

            if (entry.State == EntityState.Deleted)
                return "Delete";

            if (entry.State == EntityState.Modified)
            {
                var isDeletedProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == nameof(BaseEntity.IsDeleted));
                if (isDeletedProperty?.CurrentValue is bool isDeleted && isDeleted)
                    return "Delete";

                return "Update";
            }

            return null;
        }

        private string GetCurrentUserId()
        {
            return _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? "System";
        }

        private string GetCurrentUserEmail()
        {
            return _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.Email)
                ?? _httpContextAccessor?.HttpContext?.User?.Identity?.Name
                ?? "System";
        }

        private sealed class PendingAuditEntry
        {
            private readonly EntityEntry _entry;

            public PendingAuditEntry(EntityEntry entry)
            {
                _entry = entry;
            }

            public string UserId { get; set; } = string.Empty;
            public string UserEmail { get; set; } = string.Empty;
            public string TableName { get; set; } = string.Empty;
            public string Action { get; set; } = string.Empty;
            public DateTime ChangedDate { get; set; }
            public string SourceIp { get; set; } = string.Empty;
            public Dictionary<string, object?> OldValues { get; } = new();
            public Dictionary<string, object?> NewValues { get; } = new();

            public AuditLog ToAuditLog()
            {
                return new AuditLog
                {
                    UserId = UserId,
                    UserEmail = UserEmail,
                    TableName = TableName,
                    RecordId = GetRecordId(),
                    Action = Action,
                    ChangedDate = ChangedDate,
                    OldValues = JsonSerializer.Serialize(OldValues),
                    NewValues = JsonSerializer.Serialize(NewValues),
                    ChangeDescription = $"{Action} on {TableName}",
                    SourceIp = SourceIp
                };
            }

            private string GetRecordId()
            {
                var keyValues = _entry.Properties
                    .Where(p => p.Metadata.IsPrimaryKey())
                    .Select(p => p.CurrentValue?.ToString() ?? p.OriginalValue?.ToString() ?? string.Empty);

                return string.Join(",", keyValues);
            }
        }
    }
}
