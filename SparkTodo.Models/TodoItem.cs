using System;

namespace SparkTodo.Models
{
    public class TodoItem
    {
        public int TodoId { get;set; }

        public string TodoTitle {get;set;}

        public string TodoContent {get;set;}

        public bool IsCompleted {get;set;}

        public bool IsDeleted {get;set;}

        public DateTime CreatedTime {get;set;}

        public DateTime? CompletedTime {get;set;}

        public DateTime UpdatedTime {get;set;}
        
        public int CategoryId {get;set;}
        
        public int UserId {get;set;}

    }
}