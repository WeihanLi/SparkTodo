// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace SparkTodo.Models;

public enum OperationType
{
    Add = 0,
    Delete = 1,
    Update = 2,
}

public class SyncTodoModel
{
    public long Version { get; set; }

    [Required]
    public required SyncTodoItemModel[] SyncTodoItems { get; set; }
}

public class SyncTodoItemModel
{
    /// <summary>
    /// 操作类型
    /// </summary>
    public required OperationType Type { get; set; }

    /// <summary>
    /// todoItem
    /// </summary>
    public required TodoItem TodoItem { get; set; }
}

