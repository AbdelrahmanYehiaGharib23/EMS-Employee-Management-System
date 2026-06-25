using EMS.BLL;
using EMS.BLL.Services.AttachementService;
using EMS.BLL.Services.AuditService;
using EMS.BLL.Services.DashboardServices;
using EMS.BLL.Services.LeaveServices;
using EMS.BLL.Services.DepartmentServices;
using EMS.BLL.Services.EmployeeServices;
using EMS.BLL.Services.Identity;
using EMS.DAL;
using EMS.DAL.Entities.IdentityModel;
using EMS.DAL.Interfaces;
using EMS.DAL.Persistence.Data.DbInitializer;
using EMS.DAL.Persistence.Repositories;
using EMS.PL.Extensions;
using EMS.PL.Mapping;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using EMS.BLL.Services.Notification;

namespace EMS.PL
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add core services
            builder.Services.AddControllersWithViews();
            builder.Services.AddPersistenceServices(builder.Configuration);
            builder.Services.AddHttpsRedirection(options =>
            {
                var configuredHttpsPort = builder.Configuration.GetValue<int?>("HttpsRedirection:HttpsPort");
                if (configuredHttpsPort.HasValue)
                    options.HttpsPort = configuredHttpsPort.Value;
            });
            
            // Register application services
            builder.Services.AddScoped<IDepartmentService, DepartmentService>();
            builder.Services.AddScoped<IEmployeeService, EmployeeService>();
            builder.Services.AddScoped<IAttachementService, AttachementService>();
            builder.Services.AddScoped<ILeaveRequestService, LeaveRequestService>();
            builder.Services.AddScoped<IEmailSender, EmailSender>();
            builder.Services.AddScoped<IPasswordResetManager, PasswordResetManager>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddScoped<INotificationService, Services.SignalRNotificationService>();
            builder.Services.AddScoped<IAuditService, AuditService>();

            builder.Services.AddMemoryCache();
            builder.Services.AddSignalR();

            // Configure AutoMapper
            builder.Services.AddAutoMapper(M => M.AddProfile(new MappingProfile()));
            builder.Services.AddAutoMapper(cfg => cfg.AddProfile<PresentationProfile>());

            // Configure Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>();

            // Configure authentication cookies
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = builder.Configuration["AuthCookieName"] ?? "EMS.Auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromHours(1);
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";

                options.Events.OnRedirectToLogin = context =>
                {
                    if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    }
                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };

                options.Events.OnRedirectToAccessDenied = context =>
                {
                    if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    }
                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
            });

            // Configure antiforgery token
            builder.Services.AddAntiforgery(opts =>
            {
                opts.Cookie.Name = "EMS.AntiForgery";
                opts.Cookie.HttpOnly = true;
                opts.Cookie.SameSite = SameSiteMode.Lax;
                opts.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            });

            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            // Apply pending migrations at startup
            using (var scope = app.Services.CreateScope())
            {
                try
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    logger.LogInformation("Applying pending migrations at startup");
                    db.Database.Migrate();
                }
                catch (Exception ex)
                {
                    try
                    {
                        scope.ServiceProvider.GetRequiredService<ILogger<Program>>()
                            .LogError(ex, "Database migration failed");
                    }
                    catch { }
                }
            }

            app.InitializeDatabase();

            // Configure HTTP request pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }

            // Development-only CSP relaxation for browser refresh and source maps
            if (app.Environment.IsDevelopment())
            {
                app.Use(async (context, next) =>
                {
                    context.Response.Headers["Content-Security-Policy"] =
                        "default-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com https://unpkg.com; " +
                        "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com https://unpkg.com; " +
                        "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com https://unpkg.com; " +
                        "img-src 'self' data: https://cdn.jsdelivr.net https://cdnjs.cloudflare.com https://unpkg.com; " +
                        "connect-src 'self' ws: wss: http://localhost:62203 http://localhost:62197 http://localhost:58068 https://localhost:7040 https://cdn.jsdelivr.net https://cdnjs.cloudflare.com https://unpkg.com https://nominatim.openstreetmap.org;";
                    await next();
                });
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            // Map SignalR hubs
            app.MapHub<Hubs.NotificationHub>("/hubs/notifications");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Register}/{id?}");

            app.MapControllerRoute(
                name: "employee_profile",
                pattern: "Employee/Profile",
                defaults: new { controller = "Employee", action = "Profile" });

            app.Run();
        }
    }
}
