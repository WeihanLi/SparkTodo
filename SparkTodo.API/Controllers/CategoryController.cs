using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SparkTodo.API.Extensions;
using SparkTodo.DataAccess;
using SparkTodo.Models;
using WeihanLi.EntityFramework;

namespace SparkTodo.API.Controllers
{
    /// <summary>
    /// todo
    /// </summary>
    [Authorize]
    [ApiVersion("1")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
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
                return BadRequest();
            }
            var list = await _todoItemRepository.SelectAsync(t => t.CategoryId == categoryId && t.IsDeleted == false);
            return Json(list.OrderByDescending(_ => _.CreatedTime));
        }

        /// <summary>
        /// GetAll to dos
        /// </summary>
        /// <returns></returns>
        [Route("")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = User.GetUserId();
            if (userId <= 0)
            {
                return BadRequest();
            }
            var list = await _categoryRepository.SelectAsync(ca => ca.UserId == userId && !ca.IsDeleted);
            return Json(list.OrderBy(t => t.CreatedTime));
        }

        /// <summary>
        /// Update CategoryInfo
        /// </summary>
        /// <param name="category">category info</param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Category category)
        {
            if (category.UserId <= 0 || category.CategoryId <= 0 || string.IsNullOrEmpty(category.CategoryName))
            {
                return new StatusCodeResult(StatusCodes.Status406NotAcceptable);
            }
            if (!await _categoryRepository.ExistAsync(_ => _.CategoryId == category.CategoryId))
            {
                return new StatusCodeResult(StatusCodes.Status404NotFound);
            }
            category.UserId = User.GetUserId();
            category.IsDeleted = false;
            category.UpdatedTime = DateTime.Now;
            var item = await _categoryRepository.UpdateAsync(category, c => c.CreatedTime, c => c.UpdatedTime);
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
            category.UserId = User.GetUserId();
            if (category.UserId <= 0 || string.IsNullOrEmpty(category.CategoryName))
            {
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }

            category.IsDeleted = false;
            category.UpdatedTime = DateTime.Now;
            category.CreatedTime = DateTime.Now;
            var categoryId = await _categoryRepository.InsertAsync(category);
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
                return BadRequest();
            }
            var userId = User.GetUserId();
            if (!await _categoryRepository.ExistAsync(_ => _.CategoryId == categoryId && _.UserId == userId))
            {
                return BadRequest();
            }
            var category = new Category() { CategoryId = categoryId, IsDeleted = true };
            var result = await _categoryRepository.UpdateAsync(category, t => t.IsDeleted);

            if (result > 0)
            {
                await _todoItemRepository.UpdateAsync(_ => _.CategoryId == categoryId, _ => _.CategoryId, -1);
                return Ok();
            }
            else
            {
                return new StatusCodeResult(StatusCodes.Status501NotImplemented);
            }
        }
    }
}
