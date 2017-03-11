using System;
using Microsoft.AspNetCore.Mvc;
using SparkTodo.DataAccess;
using Microsoft.AspNetCore.Http;

namespace SparkTodo.API.Controllers
{
    [Route("api/v1/[controller]/[action]")]
    public class AccountController : Controller
    {
        readonly IUserAccountRepository _userRepository;
        public AccountController(IUserAccountRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpPost]
        public IActionResult Login(SparkTodo.Models.UserAccount userInfo)
        {
            if(String.IsNullOrWhiteSpace(userInfo.UserEmailAddress) || String.IsNullOrEmpty(userInfo.UserPassword))
            {
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }
            return Ok();
        }
    }
}