using System;

namespace SparkTodo.Models
{
    public class UserAccount
    {
        public int UserId {get;set;}

        public string UserName {get;set;}

        public string UserEmailAddress {get;set;}

        public string UserPassword {get;set;}

        public DateTime CreatedTime { get;set; }

        public bool IsDisabled {get;set;}
    }
}