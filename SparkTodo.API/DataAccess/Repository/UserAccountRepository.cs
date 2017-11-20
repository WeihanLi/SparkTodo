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
        /// ��¼
        /// </summary>
        /// <param name="userInfo">��¼��Ϣ</param>
        /// <returns></returns>
        public async Task<bool> LoginAsync(UserAccount userInfo)
        {
            var user = await FetchAsync(u => u.Email == userInfo.Email && u.PasswordHash == userInfo.PasswordHash);
            return user != null;
        }
    }
}