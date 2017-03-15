using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SparkTodo.DataAccess;
using SparkTodo.Models;

namespace SparkTodo.API.Controllers
{
    [Filters.PermissionRequired]
    [Route("api/v1/[controller]")]
    public class CategoryController : Controller
    {
        readonly ICategoryRepository _categoryRepository;
        readonly ITodoItemRepository _todoItemRepository;

        public CategoryController(ICategoryRepository categoryRepository, ITodoItemRepository todoItemRepository)
        {
            _categoryRepository = categoryRepository;
            _todoItemRepository = todoItemRepository;
        }

        [HttpGet("{categoryId}")]
        public async Task<IActionResult> Get([FromQuery] int categoryId)
        {
            if(categoryId<=0)
            {
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }
            var list = await _todoItemRepository.SelectAsync(t => t.CategoryId == categoryId && t.IsDeleted == false,t=>t.CreatedTime);
            return Json(list);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _categoryRepository.SelectAsync(ca => !ca.IsDeleted,ca=>ca.CreatedTime,true);
            return Json(list);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Category category)
        {
            if(category.UserId <= 0 || String.IsNullOrEmpty(category.CategoryName))
            {
                return new StatusCodeResult(StatusCodes.Status406NotAcceptable);
            }
            category.CreatedTime = DateTime.Now;
            category.UpdatedTime = DateTime.Now;
            var item = await _categoryRepository.AddAsync(category);
            return Json(item);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Category category)
        {
            if(category.UserId <= 0 || String.IsNullOrEmpty(category.CategoryName))
            {
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }
            category.UpdatedTime = DateTime.Now;
            var item = await _categoryRepository.FetchAsync(category.CategoryId);
            if(item == null){
                category.CreatedTime = DateTime.Now;
                category = await _categoryRepository.AddAsync(category);
            }else{
                await _categoryRepository.UpdateAsync(category);
            }
            return Json(category);
        }

        [HttpDelete("{categoryId}")]
        public async Task<IActionResult> Delete(int categoryId)
        {
            if(categoryId <= 0)
            {
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }
            var category = new Category(){ CategoryId = categoryId , IsDeleted = true };
            var result = await _categoryRepository.UpdateAsync(t=>t.CategoryId == categoryId,"IsDeleted");
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