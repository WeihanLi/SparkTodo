using System.Security.Claims;

namespace SparkTodo.API.Extensions
{
    public static class IdentityExtensions
    {
        public static int GetUserId(this ClaimsPrincipal principal, string userIdClaimType = "nameid")
        {
            var userId = 0;
            if (principal?.Claims != null && principal.Claims.Any(_ => _.Type == userIdClaimType))
            {
                int.TryParse(principal.FindFirst(userIdClaimType)?.Value, out userId);
            }
            return userId;
        }
    }
}
