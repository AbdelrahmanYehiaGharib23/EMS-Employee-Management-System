using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EMS.DAL.Contracts.Repositories
{
    public interface IGenericRepository<TEntity> where TEntity : BaseEntity
    {
        int Count();
        System.Threading.Tasks.Task<int> CountAsync();
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);
        System.Threading.Tasks.Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
        void Add(TEntity entity);
        IEnumerable<TEntity> GetAll(bool WithTracking = false);
        System.Threading.Tasks.Task<IEnumerable<TEntity>> GetAllAsync(bool WithTracking = false);
        IEnumerable<TEntity> GetAll(Expression<Func<TEntity, bool>> predicate);
        System.Threading.Tasks.Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate);
        TEntity? GetById(int id);
        System.Threading.Tasks.Task<TEntity?> GetByIdAsync(int id);
        void Remove(TEntity entity);

        void Update(TEntity entity);
    }
}
