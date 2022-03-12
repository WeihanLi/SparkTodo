// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using System.Security.Claims;
using WeihanLi.Web.Extensions;

namespace SparkTodo.API.Extensions;

public static class IdentityExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal, string userIdClaimType = "nameid")
    {
        return principal.GetUserId<int>(userIdClaimType);
    }
}
