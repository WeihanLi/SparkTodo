using System;

namespace SparkTodo.Models
{
    /// <summary>
    /// Category
    /// </summary>
    public class Category
    {
        public int CategoryId { get;set; }

        public string CategoryName {get;set;}

        public int ParentId {get;set;}

        public bool IsDeleted {get;set;}

        public DateTime CreatedTime {get;set;}

        public DateTime UpdatedTime {get;set;}

        public int UserId {get;set;}       
    }
}