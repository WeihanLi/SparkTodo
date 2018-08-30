using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SparkTodo.DataAccess;
using SparkTodo.Models;

namespace SparkTodo.API.Controllers
{
    /// <summary>
    /// Todos
    /// </summary>
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
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
            var todoItem = await _todoItemRepository.FetchAsync(todoId);
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
        /// <param name="userId">userId</param>
        /// <param name="pageIndex">pageIndex</param>
        /// <param name="pageSize">pageSize</param>
        /// <param name="isOnlyNotDone">isOnlyNotDone</param>
        /// <returns></returns>
        [Route("GetAll")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int userId, int pageIndex = 1, int pageSize = 50, bool isOnlyNotDone = false)
        {
            if (userId <= 0)
            {
                return new StatusCodeResult(StatusCodes.Status406NotAcceptable);
            }
            var todoItem = await _todoItemRepository.SelectAsync(t => t.UserId == userId, t => t.CreatedTime);
            List<TodoItem> todoList = null;
            if (isOnlyNotDone)
            {
                todoList = await _todoItemRepository.SelectAsync(pageIndex, pageSize, t => t.UserId == userId && !t.IsDeleted && !t.IsCompleted, t => t.CreatedTime);
            }
            else
            {
                todoList = await _todoItemRepository.SelectAsync(pageIndex, pageSize, t => t.UserId == userId && !t.IsDeleted, t => t.CreatedTime);
            }
            return Json(todoItem);
        }

        /// <summary>
        /// update todos
        /// </summary>
        /// <param name="todo">todo</param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] TodoItem todo)
        {
            if (todo.UserId <= 0 || todo.CategoryId <= 0 || String.IsNullOrEmpty(todo.TodoTitle))
            {
                return new StatusCodeResult(StatusCodes.Status406NotAcceptable);
            }
            todo.UpdatedTime = DateTime.Now;
            var item = await _todoItemRepository.UpdateAsync(todo, "TodoTitle", "TodoContent", "IsCompleted", "CompletedTime", "CategoryId");
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
            if (todo.UserId <= 0 || todo.CategoryId <= 0 || String.IsNullOrEmpty(todo.TodoTitle))
            {
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }
            await _todoItemRepository.AddAsync(todo);
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
            var todo = new TodoItem() { TodoId = todoId, IsDeleted = true };
            var result = await _todoItemRepository.UpdateAsync(t => t.TodoId == todoId);
            if (result)
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
