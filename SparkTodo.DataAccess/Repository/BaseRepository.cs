using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SparkTodo.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace SparkTodo.DataAccess.Repository
{
    public class BaseRepository<T> : IBaseRepository<T> where T:class
    {
        protected readonly SparkTodoEntity _dbEntity;

        public BaseRepository(SparkTodoEntity dbEntity)
        {
            _dbEntity = dbEntity;
        }

        public async Task<T> AddAsync(T entity)
        {
            _dbEntity.Set<T>().Add(entity);
            await _dbEntity.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> UpdateAsync(T entity)
        {
            _dbEntity.Update(entity);
            return await _dbEntity.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(T entity,params string[] propertyNames)
        {
            var entry =  _dbEntity.Entry(entity);
            entry.State = EntityState.Unchanged;
            foreach (string proName in propertyNames)
            {
                entry.Property(proName).IsModified = true;
            }
            return await _dbEntity.SaveChangesAsync()>0;
        }

        public async Task<bool> UpdateAsync(Expression<Func<T,bool>> whereLamdba,params string[] propertyNames)
        {
            var list = await  _dbEntity.Set<T>().AsNoTracking().Where(whereLamdba).ToListAsync();
            foreach(var item in list)
            {
                var entry =  _dbEntity.Entry(item);
                entry.State = EntityState.Unchanged;
                foreach (string proName in propertyNames)
                {
                    entry.Property(proName).IsModified = true;
                }
            }
            return await _dbEntity.SaveChangesAsync()>0;
        }

        public async Task<List<T>> SelectAsync<TKey>(Expression<Func<T,bool>> whereLamdba,Expression<Func<T,TKey>> orderbyLambda,bool isAsc = false)
        {
            if(isAsc)
            {
                return await _dbEntity.Set<T>().AsNoTracking().Where(whereLamdba).OrderBy(orderbyLambda).ToListAsync();
            }
            else
            {
                return await _dbEntity.Set<T>().AsNoTracking().Where(whereLamdba).OrderByDescending(orderbyLambda).ToListAsync();
            }
        }

        public async Task<List<T>> SelectAsync<TKey>(int pageIndex,int pageSize,Expression<Func<T,bool>> whereLamdba,Expression<Func<T,TKey>> orderbyLambda,bool isAsc)
        {
            int offset = (pageIndex-1)*pageSize;
            if(isAsc)
            {
                return await _dbEntity.Set<T>().AsNoTracking().Where(whereLamdba).Skip(offset).Take(pageSize).OrderBy(orderbyLambda).ToListAsync();
            }
            else
            {
                return await _dbEntity.Set<T>().AsNoTracking().Where(whereLamdba).Skip(offset).Take(pageSize).OrderByDescending(orderbyLambda).ToListAsync();
            }
        }

        public async Task<T> FetchAsync(int id)
        {
            return await _dbEntity.Set<T>().FindAsync(id);
        }

        public async Task<T> FetchAsync(Expression<Func<T,bool>> whereLamdba)
        {
            return await _dbEntity.Set<T>().AsNoTracking().FirstOrDefaultAsync(whereLamdba);
        }

        public async Task<bool> ExistAsync(Expression<Func<T,bool>> whereLamdba)
        {
            return await _dbEntity.Set<T>().AsNoTracking().AnyAsync(whereLamdba);
        }

        public async Task<int> QueryCountAsync(Expression<Func<T,bool>> whereLamdba)
        {
            return await _dbEntity.Set<T>().CountAsync(whereLamdba);
        }

        public async Task<bool> DeleteAsync(Expression<Func<T,bool>> whereLamdba)
        {
            var list = _dbEntity.Set<T>().AsNoTracking().Where(whereLamdba);
            if(list!=null)
            {
                _dbEntity.Set<T>().RemoveRange(list);
                return await _dbEntity.SaveChangesAsync()>0;
            }
            else
            {
                return false;
            }
        }
    }
}