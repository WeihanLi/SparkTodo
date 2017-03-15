using System;
using System.Threading.Tasks;
using SparkTodo.Models;
using System.Linq;

namespace SparkTodo.DataAccess.Repository
{
    public class UserAccountRepository : BaseRepository<Models.UserAccount>, IUserAccountRepository
    {
        public UserAccountRepository(SparkTodoEntity dbEntity) : base(dbEntity)
        {
        }

        /// <summary>
        /// µÇÂ¼
        /// </summary>
        /// <param name="userInfo">µÇÂ¼ÐÅÏ¢</param>
        /// <returns></returns>
        public async Task<bool> LoginAsync(UserAccount userInfo)
        {
            var user = await FetchAsync(u => u.UserEmailAddress == userInfo.UserEmailAddress && u.UserPassword == userInfo.UserPassword);
            return user != null;
        }
    }
}