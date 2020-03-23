using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SparkTodo.API.JWT;
using SparkTodo.API.Services;
using SparkTodo.DataAccess;
using WeihanLi.Common;

namespace SparkTodo.API.Controllers
{
    /// <summary>
    /// Account
    /// </summary>
    [ApiVersion("1")]
    [ApiVersion("2")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AccountController : Controller
    {
        private readonly IUserAccountRepository _userRepository;
        private readonly UserManager<SparkTodo.Models.UserAccount> _userManager;
        private readonly SignInManager<SparkTodo.Models.UserAccount> _signInManager;
        private readonly ILogger _logger;
        private readonly ITokenGenerator _tokenGenerator;

        /// <summary>
        /// AccountController .ctor
        /// </summary>
        /// <param name="userManager">userManager</param>
        /// <param name="signInManager">signInManager</param>
        /// <param name="userRepository">userRepository</param>
        /// <param name="tokenGenerator"></param>
        /// <param name="loggerFactory">loggerFactory</param>
        public AccountController(UserManager<SparkTodo.Models.UserAccount> userManager,
            SignInManager<SparkTodo.Models.UserAccount> signInManager,
            IUserAccountRepository userRepository,
            ITokenGenerator tokenGenerator,
            ILoggerFactory loggerFactory)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenGenerator = tokenGenerator;
            _logger = loggerFactory.CreateLogger<AccountController>();
        }

        /// <summary>
        /// SignIn
        /// </summary>
        /// <remarks>
        /// POST /api/v1/Account/SignIn
        /// {
        ///     "Email":"test0001@test.com",
        ///     "Password":"test001.com"
        /// }
        /// </remarks>
        [Route("SignIn")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> SignInAsync([FromBody]Models.AccountViewModels.LoginViewModel loginModel)
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
            var signinResult = await _signInManager.PasswordSignInAsync(loginModel.Email, loginModel.Password, true, lockoutOnFailure: false);
            if (signinResult.Succeeded)
            {
                _logger.LogInformation("User logged in.");

                userInfo = await _userRepository.FetchAsync(u => u.Email == loginModel.Email);
                var claims = new Claim[]
                {
                    new Claim(JwtRegisteredClaimNames.Jti, ObjectIdGenerator.Instance.NewId()),
                    new Claim(JwtRegisteredClaimNames.Sub, userInfo.Email),
                    new Claim(JwtRegisteredClaimNames.NameId, userInfo.Id.ToString()),
                };
                var token = _tokenGenerator.GenerateToken(claims);

                var userToken = new UserTokenEntity
                {
                    AccessToken = token.AccessToken,
                    ExpiresIn = token.ExpiresIn,
                    UserEmail = userInfo.Email,
                    UserId = userInfo.Id,
                    UserName = userInfo.UserName
                };
                result = new Models.JsonResponseModel<JWT.TokenEntity> { Data = userToken, Msg = "", Status = Models.JsonResponseStatus.Success };
            }
            else
            {
                if (signinResult.IsLockedOut)
                {
                    result = new Models.JsonResponseModel<JWT.TokenEntity> { Data = null, Msg = "Account locked out", Status = Models.JsonResponseStatus.RequestError };
                }
                else
                {
                    result = new Models.JsonResponseModel<JWT.TokenEntity> { Data = null, Msg = "failed to authenticate", Status = Models.JsonResponseStatus.AuthFail };
                }
            }
            return Json(result);
        }

        /// <summary>
        /// SignUp
        /// </summary>
        /// <remarks>
        /// POST /api/v1/Account/SignUp
        /// {
        ///     "Email":"test0001@test.com",
        ///     "Password":"test001.com"
        /// }
        /// </remarks>
        /// <returns></returns>
        [Route("SignUp")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> SignUpAsync([FromBody]Models.AccountViewModels.RegisterViewModel regModel)
        {
            if (!ModelState.IsValid)
            {
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }
            var userInfo = new SparkTodo.Models.UserAccount()
            {
                UserName = regModel.Email,
                Email = regModel.Email,
                EmailConfirmed = true,
                CreatedTime = DateTime.Now
            };
            var result = new Models.JsonResponseModel<JWT.TokenEntity>();
            var signupResult = await _userManager.CreateAsync(userInfo, regModel.Password);
            if (signupResult.Succeeded)
            {
                _logger.LogInformation(3, "User created a new account");
                userInfo = await _userRepository.FetchAsync(u => u.Email == regModel.Email);

                var claims = new Claim[]
                {
                    new Claim(JwtRegisteredClaimNames.Jti, ObjectIdGenerator.Instance.NewId()),
                    new Claim(JwtRegisteredClaimNames.Sub, userInfo.Email),
                    new Claim(ClaimTypes.Name, userInfo.UserName),
                };
                var token = _tokenGenerator.GenerateToken(claims);

                var userToken = new UserTokenEntity
                {
                    AccessToken = token.AccessToken,
                    ExpiresIn = token.ExpiresIn,
                    UserEmail = userInfo.Email,
                    UserId = userInfo.Id,
                    UserName = userInfo.UserName
                };
                result = new Models.JsonResponseModel<JWT.TokenEntity> { Data = userToken, Msg = "", Status = Models.JsonResponseStatus.Success };
            }
            else
            {
                result = new Models.JsonResponseModel<JWT.TokenEntity> { Data = null, Msg = "sign up failed," + string.Join(",", signupResult.Errors.Select(e => e.Description).ToArray()), Status = Models.JsonResponseStatus.ProcessFail };
            }
            return Json(result);
        }
    }
}
