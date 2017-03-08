using System;
using System.Threading.Tasks;
using SparkTodo.Models;

namespace SparkTodo.DataAccess.Repository
{
    public class UserAccountRepository : BaseRepository<Models.UserAccount>, IUserAccountRepository
    {
        public UserAccountRepository(SparkTodoEntity dbEntity) : base(dbEntity)
        {
        }

        public Task<bool> LoginAsync(UserAccount userInfo)
        {
            throw new NotImplementedException();
        }
    }
}