// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace SparkTodo.DataAccess;

public partial interface IUserAccountRepository
{
    Task<bool> LoginAsync(UserAccount userInfo);
}
