// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace SparkTodo.API.Controllers;

[Authorize]
[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/[controller]")]
public class TodoController : ControllerBase
{
    private readonly ITodoItemRepository _todoItemRepository;

    /// <summary>
    /// TodoController .ctor
    /// </summary>
    /// <param name="todoItemRepository">todoItemRepository</param>
    public TodoController(ITodoItemRepository todoItemRepository)
    {
        _todoItemRepository = todoItemRepository;
    }

    [HttpGet("{todoId}")]
    public async Task<IActionResult> Get(int todoId)
    {
        if (todoId <= 0)
        {
            return BadRequest();
        }
        var todoItem = await _todoItemRepository.FetchAsync(t => t.TodoId == todoId);
        if (todoItem == null)
        {
            return NotFound();
        }
        return Ok(todoItem);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(int pageIndex = 1, int pageSize = 50, bool isOnlyNotDone = false, int categoryId = -1)
    {
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

        return Ok(todoList);
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] TodoItem todo)
    {
        todo.UserId = User.GetUserId();
        if (todo.UserId <= 0 || todo.CategoryId <= 0 || string.IsNullOrEmpty(todo.TodoTitle))
        {
            return new StatusCodeResult(StatusCodes.Status406NotAcceptable);
        }
        todo.UpdatedTime = DateTime.UtcNow;
        var item = await _todoItemRepository.UpdateAsync(todo,
            t => t.TodoTitle,
            t => t.TodoContent,
            t => t.IsCompleted,
            t => t.CompletedTime,
            t => t.UpdatedTime);
        return Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] TodoItem todo)
    {
        todo.UserId = User.GetUserId();
        if (todo.UserId <= 0 || todo.CategoryId <= 0 || string.IsNullOrEmpty(todo.TodoTitle))
        {
            return BadRequest();
        }
        todo.UpdatedTime = DateTime.UtcNow;
        await _todoItemRepository.InsertAsync(todo);
        return Ok(todo);
    }

    [HttpDelete("{todoId}")]
    public async Task<IActionResult> Delete(int todoId)
    {
        if (todoId <= 0)
        {
            return BadRequest();
        }
        var userId = User.GetUserId();
        if (!await _todoItemRepository.ExistAsync(_ => _.UserId == userId && _.TodoId == todoId))
        {
            return NotFound();
        }
        var todo = new TodoItem() { TodoId = todoId, IsDeleted = true };
        var result = await _todoItemRepository.UpdateAsync(todo, t => t.IsDeleted);
        if (result > 0)
        {
            return Ok();
        }

        return new StatusCodeResult(StatusCodes.Status501NotImplemented);
    }
}
