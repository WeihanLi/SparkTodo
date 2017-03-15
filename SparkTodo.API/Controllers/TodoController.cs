using System;
using System.Threading.Tasks;
using SparkTodo.DataAccess;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SparkTodo.Models;

namespace SparkTodo.API.Controllers
{
    [Filters.PermissionRequired]
    [Route("api/v1/[controller]")]
    public class TodoController:Controller
    {
        private readonly ITodoItemRepository _todoItemRepository;

        public TodoController(ITodoItemRepository todoItemRepository)
        {
            _todoItemRepository = todoItemRepository;
        }

        [HttpGet("{todoId}")]
        public async Task<IActionResult> Get([FromQuery] int todoId)
        {
            if(todoId<=0)
            {
                return new StatusCodeResult(StatusCodes.Status406NotAcceptable);
            }
            var todoItem = await _todoItemRepository.FetchAsync(todoId);
            if(todoItem == null)
            {
                return new StatusCodeResult(StatusCodes.Status404NotFound);
            }
            else
            {
                return Json(todoItem);
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] TodoItem todo)
        {
            if(todo.UserId <= 0 || todo.CategoryId <=0 || String.IsNullOrEmpty(todo.TodoTitle))
            {
                return new StatusCodeResult(StatusCodes.Status406NotAcceptable);
            }
            todo.CreatedTime = DateTime.Now;
            todo.UpdatedTime = DateTime.Now;
            var item = await _todoItemRepository.AddAsync(todo);
            return Json(item);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TodoItem todo)
        {
            if(todo.UserId <= 0 || todo.CategoryId <=0 || String.IsNullOrEmpty(todo.TodoTitle))
            {
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }
            todo.UpdatedTime = DateTime.Now;
            if(todo.IsCompleted){
                todo.CompletedTime = DateTime.Now;
            }
            var todoItem = await _todoItemRepository.FetchAsync(todo.TodoId);
            if(todoItem == null){
                todo.CreatedTime = DateTime.Now;
                await _todoItemRepository.AddAsync(todo);
            }else{
                await _todoItemRepository.UpdateAsync(todo);
            }
            return Json(todo);
        }

        [HttpDelete("{todoId}")]
        public async Task<IActionResult> Delete(int todoId)
        {
            if(todoId <= 0)
            {
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }
            var todo = new TodoItem(){ TodoId = todoId , IsDeleted = true };
            var result = await _todoItemRepository.UpdateAsync(t=>t.TodoId == todoId);
            if(result)
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