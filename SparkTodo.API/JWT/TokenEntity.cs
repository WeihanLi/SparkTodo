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
}
