using SparkTodo.Models;

namespace WebApplication.Data
{
    public class SparkTodoDbInitializer
    {
        readonly SparkTodoEntity _dbEntity;
        public SparkTodoDbInitializer(SparkTodoEntity entity)
        {
            _dbEntity = entity;
        }
    }
}