using System.Threading.Tasks;

namespace SparkTodo.DataAccess
{
    public interface IUserAccountRepository:IBaseRepository<Models.UserAccount>
    {
        Task<bool> LoginAsync(Models.UserAccount userInfo);
    }
}