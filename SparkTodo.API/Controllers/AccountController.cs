using System;
using Microsoft.AspNetCore.Mvc;
using SparkTodo.DataAccess;
using Microsoft.AspNetCore.Http;
using SparkTodo.API.JWT;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace SparkTodo.API.Controllers
{
    [Route("api/v1/[controller]")]
    public class AccountController : Controller
    {
        private readonly IUserAccountRepository _userRepository;
        private readonly IOptions<Models.WebApiSettings> _apiSetting;
        private readonly UserManager<SparkTodo.Models.UserAccount> _userManager;
        private readonly SignInManager<SparkTodo.Models.UserAccount> _signInManager;
        private readonly ILogger _logger;

        public AccountController(UserManager<SparkTodo.Models.UserAccount> userManager,
            SignInManager<SparkTodo.Models.UserAccount> signInManager, IUserAccountRepository userRepository, IOptions<Models.WebApiSettings> apiSetting, ILoggerFactory loggerFactory)
        {
            _userRepository = userRepository;
            _apiSetting = apiSetting;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = loggerFactory.CreateLogger<AccountController>();
        }

        [Route("SignIn")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> SignInAsync([FromForm]Models.AccountViewModels.LoginViewModel loginModel)
        {
            if (!ModelState.IsValid)
            {
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }
            var userInfo = new SparkTodo.Models.UserAccount()
            {
                Email = loginModel.Email
            };
            var result = new Models.JsonResponseModel<JWT.TokenEntity>();
            Microsoft.AspNetCore.Identity.SignInResult signinResult = await _signInManager.PasswordSignInAsync(loginModel.Email, loginModel.Password, true, lockoutOnFailure: false);
            if(signinResult.Succeeded)
            {
                _logger.LogInformation(1, "User logged in.");
                var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_apiSetting.Value.SecretKey));
                var options = new JWT.TokenOptions
                {
                    Audience = "SparkTodoAudience",
                    Issuer = "SparkTodo",
                    SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
                };
                var token = new TokenProvider(options).GenerateToken(HttpContext, userInfo.Email);
                result = new Models.JsonResponseModel<JWT.TokenEntity> { Data = token, Msg = "登录成功", Status = Models.JsonResponseStatus.Success };
            }
            else
            {
                if(signinResult.IsLockedOut)
                {
                    result = new Models.JsonResponseModel<JWT.TokenEntity> { Data = null, Msg = "登录失败，账户已被锁定", Status = Models.JsonResponseStatus.RequestError };
                }
                else
                {
                    result = new Models.JsonResponseModel<JWT.TokenEntity> { Data = null, Msg = "登录失败", Status = Models.JsonResponseStatus.AuthFail };
                }
            }
            return Json(result);
        }

        
        [Route("SignUp")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> SignUpAsync([FromForm]Models.AccountViewModels.RegisterViewModel regModel)
        {
            if (!ModelState.IsValid)
            {
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }
            var userInfo = new SparkTodo.Models.UserAccount()
            {
                UserName = regModel.Email,
                Email = regModel.Email,
                EmailConfirmed = true,//默认不需要验证邮箱，注释以启用
                CreatedTime =DateTime.Now
            };
            var result = new Models.JsonResponseModel<JWT.TokenEntity>();
            var signupResult = await _userManager.CreateAsync(userInfo, regModel.Password);
            if(signupResult.Succeeded)
            {
                _logger.LogInformation(3, "User created a new account");
                var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_apiSetting.Value.SecretKey));
                var options = new JWT.TokenOptions
                {
                    Audience = "SparkTodoAudience",
                    Issuer = "SparkTodo",
                    SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
                };
                var token = new TokenProvider(options).GenerateToken(HttpContext, userInfo.Email);
                result = new Models.JsonResponseModel<JWT.TokenEntity> { Data = token, Msg = "注册成功", Status = Models.JsonResponseStatus.Success };
            }
            else
            {
                result = new Models.JsonResponseModel<JWT.TokenEntity> { Data = null, Msg = "sign up failed,"+String.Join(",",signupResult.Errors.Select(e => e.Description).ToArray()), Status = Models.JsonResponseStatus.ProcessFail };
            }
            return Json(result);
        }

        [Route("SignOut")]
        [Authorize]
        public async Task<IActionResult> SignOut()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation(4, "User logged out.");
            return Ok();
        }
    }
}