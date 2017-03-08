using Microsoft.AspNetCore.Mvc;
using SparkTodo.DataAccess;

namespace WebApplication.Controllers
{
    [Route("api/v1/[controller]/[action]")]
    public class AccountController : Controller
    {
        readonly IUserAccountRepository _userRepository;
        public AccountController(IUserAccountRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public IActionResult Login()
        {
            return View();
        }
    }
}