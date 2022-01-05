// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using SparkTodo.API.JWT;
using System.Security.Claims;

namespace SparkTodo.API.Services;

public interface ITokenGenerator
{
    /// <summary>
    /// 生成 Token
    /// </summary>
    /// <param name="claims">claims</param>
    /// <returns>token</returns>
    TokenEntity GenerateToken(params Claim[] claims);
}
