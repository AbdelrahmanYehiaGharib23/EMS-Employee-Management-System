using EMS.DAL.Contracts.Repositories;
using EMS.DAL.Entities.DepartmentEntity;
using EMS.DAL.Persistence.Data.DbInitializer;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EMS.DAL.Persistence.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : BaseEntity
    {
        protected readonly ApplicationDbContext _dbContext;

        public GenericRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public int Count()
        {
            return _dbContext.Set<TEntity>().Count();
        }

        public async System.Threading.Tasks.Task<int> CountAsync()
        {
            return await _dbContext.Set<TEntity>().CountAsync();
        }

        public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
        {
            return _dbContext.Set<TEntity>().Where(predicate).ToList();
        }

        public async System.Threading.Tasks.Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbContext.Set<TEntity>().Where(predicate).ToListAsync();
        }

        public IEnumerable<TEntity> GetAll(bool withTracking = false)
        {
            if (withTracking)
                return _dbContext.Set<TEntity>().Where(E => E.IsDeleted != true).ToList();
            else
                return _dbContext.Set<TEntity>().Where(E => E.IsDeleted != true).AsNoTracking().ToList();
        }

        public async System.Threading.Tasks.Task<IEnumerable<TEntity>> GetAllAsync(bool withTracking = false)
        {
            if (withTracking)
                return await _dbContext.Set<TEntity>().Where(E => E.IsDeleted != true).ToListAsync();
            else
                return await _dbContext.Set<TEntity>().Where(E => E.IsDeleted != true).AsNoTracking().ToListAsync();
        }

        public IEnumerable<TEntity> GetAll(Expression<Func<TEntity, bool>> predicate)
        {
            return _dbContext.Set<TEntity>().Where(predicate).ToList();
        }

        public async System.Threading.Tasks.Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbContext.Set<TEntity>().Where(predicate).ToListAsync();
        }

        public TEntity? GetById(int id) => _dbContext.Set<TEntity>().Find(id);

        public async System.Threading.Tasks.Task<TEntity?> GetByIdAsync(int id) => 
            await _dbContext.Set<TEntity>().FindAsync(id);

        public void Update(TEntity entity)
        {
            _dbContext.Set<TEntity>().Update(entity);
        }

        public void Remove(TEntity entity)
        {
            var isDeletedProp = entity.GetType().GetProperty("IsDeleted");
            var deletedAtProp = entity.GetType().GetProperty("DeletedAt");

            if (isDeletedProp != null)
            {
                // Soft delete via property
                isDeletedProp.SetValue(entity, true);

                if (deletedAtProp != null)
                    deletedAtProp.SetValue(entity, DateTime.UtcNow);

                _dbContext.Set<TEntity>().Update(entity);
            }
            else
            {
                // Hard delete
                if (_dbContext.Entry(entity).State == EntityState.Detached)
                    _dbContext.Set<TEntity>().Attach(entity);

                _dbContext.Set<TEntity>().Remove(entity);
            }
        }

        public void Add(TEntity entity)
        {
            _dbContext.Set<TEntity>().Add(entity);
        }

        public void AddRange(IEnumerable<TEntity> entities)
        {
            _dbContext.Set<TEntity>().AddRange(entities);
        }
    }
}
