// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

// ReSharper disable once CheckNamespace
namespace SparkTodo.DataAccess;

public partial class UserAccountRepository
{
    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="userInfo">登录信息</param>
    /// <returns></returns>
    public async Task<bool> LoginAsync(UserAccount userInfo)
    {
        var user = await FetchAsync(u => u.Email == userInfo.Email && u.PasswordHash == userInfo.PasswordHash);
        return user != null;
    }
}
