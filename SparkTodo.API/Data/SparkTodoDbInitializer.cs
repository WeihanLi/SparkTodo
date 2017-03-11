using SparkTodo.Models;

namespace SparkTodo.API.Data
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