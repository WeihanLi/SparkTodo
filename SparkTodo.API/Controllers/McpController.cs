// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using System.Text.Json;
using System.Linq.Expressions;
using SparkTodo.DataAccess;
using SparkTodo.Models;
using WeihanLi.Extensions;

namespace SparkTodo.API.Controllers;

/// <summary>
/// MCP (Model Context Protocol) Controller for exposing Todo API as standardized tools
/// </summary>
[ApiController]
[Route("api/mcp")]
public class McpController : ControllerBase
{
    private readonly ITodoItemRepository _todoItemRepository;
    private readonly ICategoryRepository _categoryRepository;

    public McpController(ITodoItemRepository todoItemRepository, ICategoryRepository categoryRepository)
    {
        _todoItemRepository = todoItemRepository;
        _categoryRepository = categoryRepository;
    }

    /// <summary>
    /// Get available MCP tools
    /// </summary>
    [HttpGet("tools")]
    public IActionResult GetTools()
    {
        var tools = new[]
        {
            new
            {
                name = "list_todos",
                description = "List todo items for the authenticated user with optional filtering",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        pageIndex = new { type = "integer", description = "Page index (default: 1)", minimum = 1 },
                        pageSize = new { type = "integer", description = "Page size (default: 50)", minimum = 1, maximum = 100 },
                        isOnlyNotDone = new { type = "boolean", description = "Filter only incomplete todos (default: false)" },
                        categoryId = new { type = "integer", description = "Filter by category ID (default: -1 for all)" }
                    }
                }
            },
            new
            {
                name = "get_todo",
                description = "Get a specific todo item by ID",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        todoId = new { type = "integer", description = "The ID of the todo item", minimum = 1 }
                    },
                    required = new[] { "todoId" }
                }
            },
            new
            {
                name = "create_todo",
                description = "Create a new todo item",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        title = new { type = "string", description = "The title of the todo item" },
                        content = new { type = "string", description = "The content/description of the todo item" },
                        categoryId = new { type = "integer", description = "The category ID for the todo item", minimum = 1 },
                        scheduledTime = new { type = "string", format = "date-time", description = "Optional scheduled time for the todo" }
                    },
                    required = new[] { "title", "categoryId" }
                }
            },
            new
            {
                name = "update_todo",
                description = "Update an existing todo item",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        todoId = new { type = "integer", description = "The ID of the todo item to update", minimum = 1 },
                        title = new { type = "string", description = "The title of the todo item" },
                        content = new { type = "string", description = "The content/description of the todo item" },
                        isCompleted = new { type = "boolean", description = "Whether the todo is completed" },
                        categoryId = new { type = "integer", description = "The category ID for the todo item", minimum = 1 },
                        scheduledTime = new { type = "string", format = "date-time", description = "Scheduled time for the todo" }
                    },
                    required = new[] { "todoId" }
                }
            },
            new
            {
                name = "delete_todo",
                description = "Delete a todo item",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        todoId = new { type = "integer", description = "The ID of the todo item to delete", minimum = 1 }
                    },
                    required = new[] { "todoId" }
                }
            },
            new
            {
                name = "list_categories",
                description = "List all categories for the authenticated user",
                inputSchema = new
                {
                    type = "object",
                    properties = new { }
                }
            },
            new
            {
                name = "create_category",
                description = "Create a new category",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        name = new { type = "string", description = "The name of the category" },
                        parentId = new { type = "integer", description = "The parent category ID (default: 0)", minimum = 0 }
                    },
                    required = new[] { "name" }
                }
            },
            new
            {
                name = "update_category",
                description = "Update an existing category",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        categoryId = new { type = "integer", description = "The ID of the category to update", minimum = 1 },
                        name = new { type = "string", description = "The name of the category" },
                        parentId = new { type = "integer", description = "The parent category ID", minimum = 0 }
                    },
                    required = new[] { "categoryId" }
                }
            },
            new
            {
                name = "delete_category",
                description = "Delete a category",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        categoryId = new { type = "integer", description = "The ID of the category to delete", minimum = 1 }
                    },
                    required = new[] { "categoryId" }
                }
            }
        };

        return Ok(new { tools });
    }

    /// <summary>
    /// Execute an MCP tool
    /// </summary>
    [HttpPost("tools/{toolName}")]
    [Authorize]
    public async Task<IActionResult> ExecuteTool(string toolName, [FromBody] JsonElement arguments)
    {
        try
        {
            var result = toolName switch
            {
                "list_todos" => await ExecuteListTodos(arguments),
                "get_todo" => await ExecuteGetTodo(arguments),
                "create_todo" => await ExecuteCreateTodo(arguments),
                "update_todo" => await ExecuteUpdateTodo(arguments),
                "delete_todo" => await ExecuteDeleteTodo(arguments),
                "list_categories" => await ExecuteListCategories(arguments),
                "create_category" => await ExecuteCreateCategory(arguments),
                "update_category" => await ExecuteUpdateCategory(arguments),
                "delete_category" => await ExecuteDeleteCategory(arguments),
                _ => throw new ArgumentException($"Unknown tool: {toolName}")
            };

            return Ok(new { success = true, result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    private async Task<object> ExecuteListTodos(JsonElement arguments)
    {
        var pageIndex = arguments.TryGetProperty("pageIndex", out var pageIndexProp) ? pageIndexProp.GetInt32() : 1;
        var pageSize = arguments.TryGetProperty("pageSize", out var pageSizeProp) ? pageSizeProp.GetInt32() : 50;
        var isOnlyNotDone = arguments.TryGetProperty("isOnlyNotDone", out var isOnlyNotDoneProp) && isOnlyNotDoneProp.GetBoolean();
        var categoryId = arguments.TryGetProperty("categoryId", out var categoryIdProp) ? categoryIdProp.GetInt32() : -1;

        var userId = User.GetUserId();
        Expression<Func<TodoItem, bool>> predict = t => t.UserId == userId;
        if (isOnlyNotDone)
        {
            predict = predict.And(t => !t.IsCompleted);
        }
        if (categoryId > 0)
        {
            predict = predict.And(t => t.CategoryId == categoryId);
        }

        var todoList = await _todoItemRepository.GetPagedListAsync(
            builder => builder.WithPredict(predict)
                                .WithOrderBy(q => q.OrderBy(_ => _.CreatedTime)),
            pageIndex,
            pageSize);

        return todoList;
    }

    private async Task<object> ExecuteGetTodo(JsonElement arguments)
    {
        if (!arguments.TryGetProperty("todoId", out var todoIdProp))
            throw new ArgumentException("todoId is required");

        var todoId = todoIdProp.GetInt32();
        if (todoId <= 0)
            throw new ArgumentException("todoId must be greater than 0");

        var todoItem = await _todoItemRepository.FetchAsync(t => t.TodoId == todoId);
        if (todoItem == null)
            throw new ArgumentException("Todo item not found");

        return todoItem;
    }

    private async Task<object> ExecuteCreateTodo(JsonElement arguments)
    {
        if (!arguments.TryGetProperty("title", out var titleProp))
            throw new ArgumentException("title is required");
        if (!arguments.TryGetProperty("categoryId", out var categoryIdProp))
            throw new ArgumentException("categoryId is required");

        var title = titleProp.GetString();
        var categoryId = categoryIdProp.GetInt32();
        var content = arguments.TryGetProperty("content", out var contentProp) ? contentProp.GetString() : string.Empty;

        if (string.IsNullOrEmpty(title))
            throw new ArgumentException("title cannot be empty");
        if (categoryId <= 0)
            throw new ArgumentException("categoryId must be greater than 0");

        var todo = new TodoItem
        {
            TodoTitle = title,
            TodoContent = content,
            CategoryId = categoryId,
            UserId = User.GetUserId(),
            CreatedTime = DateTime.UtcNow,
            UpdatedTime = DateTime.UtcNow,
            IsCompleted = false,
            IsDeleted = false
        };

        if (arguments.TryGetProperty("scheduledTime", out var scheduledTimeProp))
        {
            if (DateTime.TryParse(scheduledTimeProp.GetString(), out var scheduledTime))
            {
                todo.ScheduledTime = scheduledTime;
            }
        }

        await _todoItemRepository.InsertAsync(todo);
        return todo;
    }

    private async Task<object> ExecuteUpdateTodo(JsonElement arguments)
    {
        if (!arguments.TryGetProperty("todoId", out var todoIdProp))
            throw new ArgumentException("todoId is required");

        var todoId = todoIdProp.GetInt32();
        if (todoId <= 0)
            throw new ArgumentException("todoId must be greater than 0");

        var existingTodo = await _todoItemRepository.FetchAsync(t => t.TodoId == todoId);
        if (existingTodo == null)
            throw new ArgumentException("Todo item not found");

        if (arguments.TryGetProperty("title", out var titleProp))
            existingTodo.TodoTitle = titleProp.GetString();
        if (arguments.TryGetProperty("content", out var contentProp))
            existingTodo.TodoContent = contentProp.GetString();
        if (arguments.TryGetProperty("isCompleted", out var isCompletedProp))
        {
            existingTodo.IsCompleted = isCompletedProp.GetBoolean();
            if (existingTodo.IsCompleted && existingTodo.CompletedTime == null)
                existingTodo.CompletedTime = DateTime.UtcNow;
        }
        if (arguments.TryGetProperty("categoryId", out var categoryIdProp))
            existingTodo.CategoryId = categoryIdProp.GetInt32();
        if (arguments.TryGetProperty("scheduledTime", out var scheduledTimeProp))
        {
            var scheduledTimeStr = scheduledTimeProp.GetString();
            existingTodo.ScheduledTime = string.IsNullOrEmpty(scheduledTimeStr) 
                ? null 
                : DateTime.Parse(scheduledTimeStr);
        }

        existingTodo.UpdatedTime = DateTime.UtcNow;
        existingTodo.UserId = User.GetUserId();

        var updatedItem = await _todoItemRepository.UpdateAsync(existingTodo,
            t => t.TodoTitle,
            t => t.TodoContent,
            t => t.IsCompleted,
            t => t.CompletedTime,
            t => t.CategoryId,
            t => t.ScheduledTime,
            t => t.UpdatedTime);

        return updatedItem;
    }

    private async Task<object> ExecuteDeleteTodo(JsonElement arguments)
    {
        if (!arguments.TryGetProperty("todoId", out var todoIdProp))
            throw new ArgumentException("todoId is required");

        var todoId = todoIdProp.GetInt32();
        if (todoId <= 0)
            throw new ArgumentException("todoId must be greater than 0");

        var userId = User.GetUserId();
        if (!await _todoItemRepository.ExistAsync(_ => _.UserId == userId && _.TodoId == todoId))
            throw new ArgumentException("Todo item not found");

        var todo = new TodoItem() { TodoId = todoId };
        var result = await _todoItemRepository.DeleteAsync(todo);
        if (result <= 0)
            throw new InvalidOperationException("Failed to delete todo item");

        return new { message = "Todo item deleted successfully", todoId };
    }

    private async Task<object> ExecuteListCategories(JsonElement arguments)
    {
        var userId = User.GetUserId();
        var categories = await _categoryRepository.SelectAsync(ca => ca.UserId == userId && !ca.IsDeleted);
        return categories.OrderBy(t => t.CreatedTime);
    }

    private async Task<object> ExecuteCreateCategory(JsonElement arguments)
    {
        if (!arguments.TryGetProperty("name", out var nameProp))
            throw new ArgumentException("name is required");

        var name = nameProp.GetString();
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("name cannot be empty");

        var parentId = arguments.TryGetProperty("parentId", out var parentIdProp) ? parentIdProp.GetInt32() : 0;

        var category = new Category
        {
            CategoryName = name,
            ParentId = parentId,
            UserId = User.GetUserId(),
            CreatedTime = DateTime.UtcNow,
            UpdatedTime = DateTime.UtcNow,
            IsDeleted = false
        };

        await _categoryRepository.InsertAsync(category);
        return category;
    }

    private async Task<object> ExecuteUpdateCategory(JsonElement arguments)
    {
        if (!arguments.TryGetProperty("categoryId", out var categoryIdProp))
            throw new ArgumentException("categoryId is required");

        var categoryId = categoryIdProp.GetInt32();
        if (categoryId <= 0)
            throw new ArgumentException("categoryId must be greater than 0");

        if (!await _categoryRepository.ExistAsync(_ => _.CategoryId == categoryId))
            throw new ArgumentException("Category not found");

        var existingCategory = await _categoryRepository.FetchAsync(c => c.CategoryId == categoryId);
        if (existingCategory == null)
            throw new ArgumentException("Category not found");

        if (arguments.TryGetProperty("name", out var nameProp))
            existingCategory.CategoryName = nameProp.GetString();
        if (arguments.TryGetProperty("parentId", out var parentIdProp))
            existingCategory.ParentId = parentIdProp.GetInt32();

        existingCategory.UserId = User.GetUserId();
        existingCategory.UpdatedTime = DateTime.UtcNow;
        existingCategory.IsDeleted = false;

        var updatedItem = await _categoryRepository.UpdateAsync(existingCategory, 
            c => c.CategoryName, 
            c => c.ParentId, 
            c => c.UpdatedTime);
        return updatedItem;
    }

    private async Task<object> ExecuteDeleteCategory(JsonElement arguments)
    {
        if (!arguments.TryGetProperty("categoryId", out var categoryIdProp))
            throw new ArgumentException("categoryId is required");

        var categoryId = categoryIdProp.GetInt32();
        if (categoryId <= 0)
            throw new ArgumentException("categoryId must be greater than 0");

        var userId = User.GetUserId();
        if (!await _categoryRepository.ExistAsync(_ => _.CategoryId == categoryId && _.UserId == userId))
            throw new ArgumentException("Category not found");

        var category = new Category()
        {
            CategoryId = categoryId,
            IsDeleted = true
        };

        var result = await _categoryRepository.DeleteAsync(category);
        if (result <= 0)
            throw new InvalidOperationException("Failed to delete category");

        // Update todos that belonged to this category to have no category
        await _todoItemRepository.UpdateAsync(
            (x) => x.SetProperty(_ => _.CategoryId, _ => -1), 
            _ => _.WithPredict(t => t.CategoryId == categoryId));

        return new { message = "Category deleted successfully", categoryId };
    }
}