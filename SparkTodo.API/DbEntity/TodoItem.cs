using System;
using System.ComponentModel.DataAnnotations;

namespace SparkTodo.Models
{
    /// <summary>
    /// TodoItem
    /// </summary>
    public class TodoItem
    {
        public int TodoId { get; set; }

        [Required]
        public string TodoTitle { get; set; }

        public string TodoContent { get; set; }

        public bool IsCompleted { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime? CompletedTime { get; set; }

        public DateTime UpdatedTime { get; set; }

        public int CategoryId { get; set; }

        public int UserId { get; set; }
    }
}
