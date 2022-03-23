// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using WeihanLi.Web.Authorization.Token;

namespace SparkTodo.API.Response;

/// <summary>
/// 包含用户信息的Token
/// </summary>
public class UserTokenEntity : TokenEntityWithRefreshToken
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
