using System;

namespace SparkTodo.Models
{
    public class Category
    {
        public int CategoryId { get;set; }

        public string CategoryName {get;set;}

        public int ParentId {get;set;}

        public bool IsDeleted {get;set;}

        public DateTime CreatedTime {get;set;}

        public DateTime UpdatedTime {get;set;}

        public int UserId {get;set;}

        public UserAccount User {get;set;}
    }
}