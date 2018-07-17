using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SparkTodo.DataAccess;
using SparkTodo.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SparkTodo.API.Controllers
{
    /// <summary>
    /// Todo
    /// </summary>
    [Authorize]
    [Route("api/v1/[controller]")]
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
        /// ��ȡTodo����
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
        /// ��ȡ todo �б�
        /// </summary>
        /// <param name="userId">�û�id</param>
        /// <param name="pageIndex">ҳ������</param>
        /// <param name="pageSize">ÿҳ������</param>
        /// <param name="isOnlyNotDone">�Ƿ�ֻ��ʾδ��ɵ�todo��Ĭ��ֻ��ѯδ��ɵ�todo</param>
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
        /// �޸�todo��Ϣ
        /// </summary>
        /// <param name="todo">todo��Ϣ</param>
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
        /// ����һ�� todo
        /// </summary>
        /// <param name="todo">todo��Ϣ</param>
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
        /// ɾ��ĳһ�� todo
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