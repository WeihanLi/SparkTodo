using SparkTodo.Models;

namespace SparkTodo.DataAccess.Repository
{
    public class CategoryRepository : BaseRepository<Models.Category>, ICategoryRepository
    {
        public CategoryRepository(SparkTodoEntity dbEntity) : base(dbEntity)
        {
        }
    }
}