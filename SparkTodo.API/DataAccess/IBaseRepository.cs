using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SparkTodo.DataAccess
{
    public interface IBaseRepository<T>
    {
        Task<T> AddAsync(T entity);

        Task<bool> UpdateAsync(T entity);

        Task<bool> UpdateAsync(T entity,params string[] propertyNames);

        Task<bool> UpdateAsync(Expression<Func<T,bool>> whereLamdba,params string[] propertyNames);

        Task<List<T>> SelectAsync<TKey>(Expression<Func<T,bool>> whereLamdba,Expression<Func<T,TKey>> orderbyLambda,bool isAsc = false);

        Task<List<T>> SelectAsync<TKey>(int pageIndex,int pageSize,Expression<Func<T,bool>> whereLamdba,Expression<Func<T,TKey>> orderbyLambda,bool isAsc = false);

        Task<T> FetchAsync(int id);
        Task<T> FetchAsync(Expression<Func<T,bool>> whereLamdba);

        Task<bool> ExistAsync(Expression<Func<T,bool>> whereLamdba);

        Task<int> QueryCountAsync(Expression<Func<T,bool>> whereLamdba);

        Task<bool> DeleteAsync(Expression<Func<T,bool>> whereLamdba);
    }
}
