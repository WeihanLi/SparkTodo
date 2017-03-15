using System;
using Microsoft.AspNetCore.Mvc;
using SparkTodo.DataAccess;
using Microsoft.AspNetCore.Http;
using SparkTodo.API.JWT;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace SparkTodo.API.Controllers
{
    [Route("api/v1/[controller]")]
    public class AccountController : Controller
    {
        readonly IUserAccountRepository _userRepository;
        readonly IOptions<Models.WebApiSettings> _apiSetting;
        public AccountController(IUserAccountRepository userRepository, IOptions<Models.WebApiSettings> apiSetting)
        {
            _userRepository = userRepository;
            _apiSetting = apiSetting;
        }

        [Route("SignIn")]
        [HttpPost]
        public async Task<IActionResult> SignInAsync([FromForm]Models.AccountViewModels.LoginViewModel loginModel)
        {
            if (!ModelState.IsValid)
            {
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }
            var userInfo = new SparkTodo.Models.UserAccount()
            {
                UserEmailAddress = loginModel.Email,
                UserPassword = loginModel.Password
            };
            var result = new Models.JsonResponseModel<TokenEntity>();
            //验证账户，登录成功则设置Token
            //密码加密
            userInfo.UserPassword = Common.SecurityHelper.SHA256_Encrypt(userInfo.UserPassword);
            var loginResult = await _userRepository.LoginAsync(userInfo);
            if(loginResult)
            {
                var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_apiSetting.Value.SecretKey));
                var options = new TokenOptions
                {
                    Audience = "SparkTodoAudience",
                    Issuer = "SparkTodo",
                    SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
                };
                var token = new TokenProvider(options).GenerateToken(HttpContext, userInfo.UserEmailAddress);
                result = new Models.JsonResponseModel<TokenEntity> { Data = token, Msg = "登录成功", Status = Models.JsonResponseStatus.Success };
            }
            else
            {
                result = new Models.JsonResponseModel<TokenEntity> { Data = null, Msg = "登录失败", Status = Models.JsonResponseStatus.Success };
            }
            return Json(result);
        }

        [Route("SignUp")]
        [HttpPost]
        public async Task<IActionResult> SignUpAsync([FromForm]Models.AccountViewModels.RegisterViewModel regModel)
        {
            if (!ModelState.IsValid)
            {
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }
            var userInfo = new SparkTodo.Models.UserAccount()
            {
                UserEmailAddress = regModel.Email,
                UserPassword = regModel.Password
            };
            var result = new Models.JsonResponseModel<TokenEntity>();
            //用户密码加密
            userInfo.UserPassword = Common.SecurityHelper.SHA256_Encrypt(userInfo.UserPassword);
            userInfo.UserName = userInfo.UserEmailAddress;
            userInfo.IsDisabled = false;
            userInfo.CreatedTime = DateTime.Now;
            //注册账户，设置Token
            var regUser = await _userRepository.AddAsync(userInfo);
            if(regUser != null)
            {
                var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_apiSetting.Value.SecretKey));
                var options = new TokenOptions
                {
                    Audience = "SparkTodoAudience",
                    Issuer = "SparkTodo",
                    SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
                };
                var token = new TokenProvider(options).GenerateToken(HttpContext, userInfo.UserEmailAddress);
                result = new Models.JsonResponseModel<TokenEntity> { Data = token, Msg = "注册成功", Status = Models.JsonResponseStatus.Success };
            }
            else
            {
                result = new Models.JsonResponseModel<TokenEntity> { Data = null, Msg = "注册失败", Status = Models.JsonResponseStatus.ProcessFail };
            }
            return Json(result);
        }
    }
}