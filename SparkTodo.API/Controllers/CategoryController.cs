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
    /// todo
    /// </summary>
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ITodoItemRepository _todoItemRepository;

        /// <summary>
        /// CategoryController .ctor
        /// </summary>
        /// <param name="categoryRepository">categoryRepository</param>
        /// <param name="todoItemRepository">todoItemRepository</param>
        public CategoryController(ICategoryRepository categoryRepository, ITodoItemRepository todoItemRepository)
        {
            _categoryRepository = categoryRepository;
            _todoItemRepository = todoItemRepository;
        }

        /// <summary>
        /// Get todo
        /// </summary>
        /// <param name="categoryId">categoryId</param>
        /// <returns></returns>
        [HttpGet("{categoryId}")]
        public async Task<IActionResult> Get([FromQuery] int categoryId)
        {
            if (categoryId <= 0)
            {
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }
            var list = await _todoItemRepository.SelectAsync(t => t.CategoryId == categoryId && t.IsDeleted == false, t => t.CreatedTime);
            return Json(list);
        }

        /// <summary>
        /// GetAll to dos
        /// </summary>
        /// <param name="userId">userId</param>
        /// <returns></returns>
        [Route("GetAll")]
        [HttpGet]
        public async Task<IActionResult> GetAll(int userId)
        {
            if (userId <= 0)
            {
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }
            List<Category> list = await _categoryRepository.SelectAsync(ca => ca.UserId == userId && !ca.IsDeleted, ca => ca.CreatedTime, true);
            return Json(list);
        }

        /// <summary>
        /// Update CategoryInfo
        /// </summary>
        /// <param name="category">category info</param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Category category)
        {
            if (category.UserId <= 0 || category.CategoryId <= 0 || String.IsNullOrEmpty(category.CategoryName))
            {
                return new StatusCodeResult(StatusCodes.Status406NotAcceptable);
            }
            category.UpdatedTime = DateTime.Now;
            var item = await _categoryRepository.UpdateAsync(category, "CategoryName", "UpdatedTime");
            return Json(item);
        }

        /// <summary>
        /// create todo category
        /// </summary>
        /// <param name="category">category info</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Category category)
        {
            if (category.UserId <= 0 || String.IsNullOrEmpty(category.CategoryName))
            {
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }
            category.UpdatedTime = DateTime.Now;
            category.CreatedTime = DateTime.Now;
            category = await _categoryRepository.AddAsync(category);
            return Json(category);
        }

        /// <summary>
        /// delete todo category
        /// </summary>
        /// <param name="categoryId">categoryId</param>
        /// <returns></returns>
        [HttpDelete("{categoryId}")]
        public async Task<IActionResult> Delete(int categoryId)
        {
            if (categoryId <= 0)
            {
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }
            var category = new Category() { CategoryId = categoryId, IsDeleted = true };
            var result = await _categoryRepository.UpdateAsync(t => t.CategoryId == categoryId, "IsDeleted");
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
