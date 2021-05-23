using System.Threading.Tasks;

namespace SparkTodo.DataAccess
{
    public partial interface IUserAccountRepository
    {
        Task<bool> LoginAsync(Models.UserAccount userInfo);
    }
}
