// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace SparkTodo.API.Controllers;

[Authorize]
[ApiVersion("1")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class CategoryController : ControllerBase
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

    [HttpGet("{categoryId}/todo")]
    public async Task<IActionResult> Get(int categoryId)
    {
        if (categoryId <= 0)
        {
            return BadRequest();
        }
        var list = await _todoItemRepository.SelectAsync(t => t.CategoryId == categoryId && t.IsDeleted == false);
        return Ok(list.OrderByDescending(_ => _.CreatedTime));
    }

    /// <summary>
    /// get all categories
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.GetUserId();
        var list = await _categoryRepository.SelectAsync(ca => ca.UserId == userId && !ca.IsDeleted);
        return Ok(list.OrderBy(t => t.CreatedTime));
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
            return BadRequest();
        }
        if (!await _categoryRepository.ExistAsync(_ => _.CategoryId == category.CategoryId))
        {
            return NotFound();
        }
        category.UserId = User.GetUserId();
        category.IsDeleted = false;
        category.UpdatedTime = DateTime.UtcNow;
        var item = await _categoryRepository.UpdateAsync(category, c => c.CreatedTime, c => c.UpdatedTime);
        return Ok(item);
    }

    /// <summary>
    /// create category
    /// </summary>
    /// <param name="category">category info</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Category category)
    {
        category.UserId = User.GetUserId();
        if (category.UserId <= 0 || string.IsNullOrEmpty(category.CategoryName))
        {
            return BadRequest();
        }

        category.IsDeleted = false;
        category.UpdatedTime = DateTime.UtcNow;
        category.CreatedTime = DateTime.UtcNow;

        await _categoryRepository.InsertAsync(category);
        return Ok(category);
    }

    /// <summary>
    /// delete category
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
            return NotFound();
        }
        var category = new Category()
        {
            CategoryId = categoryId,
            IsDeleted = true
        };
        var result = await _categoryRepository.UpdateAsync(category, t => t.IsDeleted);
        if (result > 0)
        {
            await _todoItemRepository.UpdateAsync((x) => x.SetProperty(_ => _.CategoryId, _ => -1), _ => _.WithPredict(t => t.CategoryId == categoryId));
            return Ok();
        }

        return new StatusCodeResult(StatusCodes.Status501NotImplemented);
    }
}
