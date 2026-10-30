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


        void Add(TEntity entity);
        IEnumerable<TEntity> GetAll(bool WithTracking = false);
        Task<IEnumerable<TEntity>> GetAllAsync(bool WithTracking = false);
        IEnumerable<TEntity> GetAll(Expression<Func<TEntity, bool>> predicate);
        Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate);
        TEntity? GetById(int id);
        Task<TEntity?> GetByIdAsync(int id);
        void Remove(TEntity entity);

        void Update(TEntity entity);
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
        int Count();
        Task<int> CountAsync();
    }
}
