using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;

namespace SparkTodo.Models
{
    /// <summary>
    /// UserAccount
    /// </summary>
    public class UserAccount : IdentityUser
    {
        public int UserId { get; set; }

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
}