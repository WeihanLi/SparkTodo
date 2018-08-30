using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SparkTodo.DataAccess;

namespace SparkTodo.API.Controllers
{
    [ApiVersion("2.0")]
    [Authorize]
    [Route("api/todo")]
    public class NewTodoController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ITodoItemRepository _todoItemRepository;

        public NewTodoController(ICategoryRepository categoryRepository, ITodoItemRepository todoItemRepository)
        {
            _categoryRepository = categoryRepository;
            _todoItemRepository = todoItemRepository;
        }

        [HttpGet]
        public IActionResult GetVersion()
        {
            return Json(new { Version = 123 });
        }
    }
}
