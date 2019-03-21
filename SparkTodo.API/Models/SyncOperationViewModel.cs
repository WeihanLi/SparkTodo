using SparkTodo.Models;

namespace SparkTodo.API.Models
{
    public enum OperationType
    {
        Add = 0,
        Delete = 1,
        Update = 2,
    }

    public class SyncTodoModel
    {
        public long Version { get; set; }

        public SyncTodoItemModel[] SyncTodoItems { get; set; }
    }

    public class SyncTodoItemModel
    {
        /// <summary>
        /// 操作类型
        /// </summary>
        public OperationType Type { get; set; }

        /// <summary>
        /// todoItem
        /// </summary>
        public TodoItem TodoItem { get; set; }
    }
}
