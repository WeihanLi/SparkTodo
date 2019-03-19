using System.Security.Claims;
using SparkTodo.API.JWT;

namespace SparkTodo.API.Services
{
    public interface ITokenGenerator
    {
        /// <summary>
        /// 生成 Token
        /// </summary>
        /// <param name="claims">claims</param>
        /// <returns>token</returns>
        TokenEntity GenerateToken(params Claim[] claims);
    }
}
