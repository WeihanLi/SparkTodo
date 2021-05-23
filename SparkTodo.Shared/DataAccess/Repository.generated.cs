 
using SparkTodo.Models;
using Microsoft.Extensions.DependencyInjection;
using WeihanLi.EntityFramework;

namespace SparkTodo.DataAccess
{
	public partial interface ITodoItemRepository: IEFRepository<SparkTodoDbContext, TodoItem>{}

	public partial class TodoItemRepository : EFRepository<SparkTodoDbContext, TodoItem>,  ITodoItemRepository
    {
        public TodoItemRepository(SparkTodoDbContext dbContext) : base(dbContext)
        {
        }
    }
	public partial interface ICategoryRepository: IEFRepository<SparkTodoDbContext, Category>{}

	public partial class CategoryRepository : EFRepository<SparkTodoDbContext, Category>,  ICategoryRepository
    {
        public CategoryRepository(SparkTodoDbContext dbContext) : base(dbContext)
        {
        }
    }
	public partial interface IUserAccountRepository: IEFRepository<SparkTodoDbContext, UserAccount>{}

	public partial class UserAccountRepository : EFRepository<SparkTodoDbContext, UserAccount>,  IUserAccountRepository
    {
        public UserAccountRepository(SparkTodoDbContext dbContext) : base(dbContext)
        {
        }
    }
	public partial interface ISyncVersionRepository: IEFRepository<SparkTodoDbContext, SyncVersion>{}

	public partial class SyncVersionRepository : EFRepository<SparkTodoDbContext, SyncVersion>,  ISyncVersionRepository
    {
        public SyncVersionRepository(SparkTodoDbContext dbContext) : base(dbContext)
        {
        }
    }
}