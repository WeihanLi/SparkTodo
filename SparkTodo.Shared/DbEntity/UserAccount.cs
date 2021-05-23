using System;
using Microsoft.AspNetCore.Identity;

namespace SparkTodo.Models
{
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

        public bool IsDisabled { get; set; }
    }

    public class UserRole : IdentityRole<int>
    {
    }
}
