using SparkTodo.Models;

namespace SparkTodo.DataAccess.Repository
{
    public class TodoItemRepository : BaseRepository<Models.TodoItem>, ITodoItemRepository
    {
        public TodoItemRepository(SparkTodoEntity dbEntity) : base(dbEntity)
        {
        }
    }
}