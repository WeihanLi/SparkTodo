// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Identity;
using WeihanLi.Common.Models;

namespace SparkTodo.Models;

/// <summary>
/// UserAccount
/// </summary>
public class UserAccount : IdentityUser<int>
{
    public UserAccount()
    {
    }

    public UserAccount(string userName)
    {
        base.UserName = userName;
    }

    public DateTime CreatedTime { get; set; }
}

public class UserRole : IdentityRole<int>
{
}
