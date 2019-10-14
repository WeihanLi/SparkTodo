using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SparkTodo.API.Extensions;
using SparkTodo.DataAccess;
using SparkTodo.Models;
using WeihanLi.EntityFramework;
using WeihanLi.Extensions;

namespace SparkTodo.API.Controllers
{
    /// <summary>
    /// Todos
    /// </summary>
    [Authorize]
    [ApiController]
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class TodoController : Controller
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

        /// <summary>
        /// get todo by id
        /// </summary>
        /// <param name="todoId">todo id</param>
        /// <returns></returns>
        [HttpGet("{todoId}")]
        public async Task<IActionResult> Get([FromQuery] int todoId)
        {
            if (todoId <= 0)
            {
                return new StatusCodeResult(StatusCodes.Status406NotAcceptable);
            }
            var todoItem = await _todoItemRepository.FetchAsync(t => t.TodoId == todoId);
            if (todoItem == null)
            {
                return new StatusCodeResult(StatusCodes.Status404NotFound);
            }
            else
            {
                return Json(todoItem);
            }
        }

        /// <summary>
        /// todos list
        /// </summary>
        /// <param name="pageIndex">pageIndex</param>
        /// <param name="pageSize">pageSize</param>
        /// <param name="isOnlyNotDone">isOnlyNotDone</param>
        /// <returns></returns>
        [Route("")]
        [HttpGet]
        public async Task<IActionResult> GetAll(int pageIndex = 1, int pageSize = 50, bool isOnlyNotDone = false)
        {
            var userId = User.GetUserId();
            if (userId <= 0)
            {
                return new StatusCodeResult(StatusCodes.Status406NotAcceptable);
            }
            Expression<Func<TodoItem, bool>> predict = t => t.UserId == userId;
            if (isOnlyNotDone)
            {
                predict = predict.And(t => !t.IsCompleted);
            }
            var todoList = await _todoItemRepository.GetPagedListAsync(
                builder => builder.WithPredict(predict)
                                    .WithOrderBy(q => q.OrderBy(_ => _.CreatedTime)),
                pageIndex,
                pageSize);

            return Json(todoList);
        }

        /// <summary>
        /// update todos
        /// </summary>
        /// <param name="todo">todo</param>
        /// <returns></returns>
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
            return Json(item);
        }

        /// <summary>
        /// create a todo
        /// </summary>
        /// <param name="todo">todo info</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TodoItem todo)
        {
            todo.UserId = User.GetUserId();
            if (todo.UserId <= 0 || todo.CategoryId <= 0 || string.IsNullOrEmpty(todo.TodoTitle))
            {
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }
            await _todoItemRepository.InsertAsync(todo);
            return Json(todo);
        }

        /// <summary>
        /// delete todo
        /// </summary>
        /// <param name="todoId">todo Id</param>
        /// <returns></returns>
        [HttpDelete("{todoId}")]
        public async Task<IActionResult> Delete(int todoId)
        {
            if (todoId <= 0)
            {
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }
            var userId = User.GetUserId();
            if (!await _todoItemRepository.ExistAsync(_ => _.UserId == userId && _.TodoId == todoId))
            {
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }
            var todo = new TodoItem() { TodoId = todoId, IsDeleted = true };
            var result = await _todoItemRepository.UpdateAsync(todo, t => t.IsDeleted);
            if (result > 0)
            {
                return Ok();
            }
            else
            {
                return new StatusCodeResult(StatusCodes.Status501NotImplemented);
            }
        }
    }
}
