using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SparkTodo.API.JWT
{
    /// <summary>
    /// Token
    /// </summary>
    public class TokenEntity
    {
        /// <summary>
        /// AccessToken
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// ExpiresIn
        /// </summary>
        public int ExpiresIn { get; set; }
    }

    /// <summary>
    /// 包含用户信息的Token
    /// </summary>
    public class UserTokenEntity:TokenEntity
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 用户邮箱
        /// </summary>
        public string UserEmail { get; set; }
    }
}
