using SparkTodo.API.JWT;
using SparkTodo.API.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SparkTodo.API.Controllers;
/// <summary>
/// Account
/// </summary>
[ApiVersion("1")]
[ApiVersion("2")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IUserAccountRepository _userRepository;
    private readonly UserManager<SparkTodo.Models.UserAccount> _userManager;
    private readonly SignInManager<SparkTodo.Models.UserAccount> _signInManager;
    private readonly ITokenGenerator _tokenGenerator;

    /// <summary>
    /// AccountController .ctor
    /// </summary>
    /// <param name="userManager">userManager</param>
    /// <param name="signInManager">signInManager</param>
    /// <param name="userRepository">userRepository</param>
    /// <param name="tokenGenerator"></param>
    public AccountController(UserManager<SparkTodo.Models.UserAccount> userManager,
        SignInManager<SparkTodo.Models.UserAccount> signInManager,
        IUserAccountRepository userRepository,
        ITokenGenerator tokenGenerator)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenGenerator = tokenGenerator;
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
    public async Task<IActionResult> SignInAsync([FromBody] Models.AccountViewModels.LoginViewModel loginModel)
    {
        JsonResponseModel result;
        var signinResult = await _signInManager.PasswordSignInAsync(loginModel.Email, loginModel.Password, true, lockoutOnFailure: false);
        if (signinResult.Succeeded)
        {
            var userInfo = await _userRepository.FetchAsync(u => u.Email == loginModel.Email);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, GuidIdGenerator.Instance.NewId()),
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
                result = new Models.JsonResponseModel<JWT.TokenEntity> { Msg = "Account locked out", Status = Models.JsonResponseStatus.RequestError };
            }
            else
            {
                result = new Models.JsonResponseModel<JWT.TokenEntity> { Msg = "failed to authenticate", Status = Models.JsonResponseStatus.AuthFail };
            }
        }
        return Ok(result);
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
    public async Task<IActionResult> SignUpAsync([FromBody] Models.AccountViewModels.RegisterViewModel regModel)
    {
        var userInfo = new SparkTodo.Models.UserAccount()
        {
            UserName = regModel.Email,
            Email = regModel.Email,
            EmailConfirmed = true,
            CreatedTime = DateTime.UtcNow
        };
        JsonResponseModel<TokenEntity> result;
        var signUpResult = await _userManager.CreateAsync(userInfo, regModel.Password);
        if (signUpResult.Succeeded)
        {
            userInfo = await _userRepository.FetchAsync(u => u.Email == regModel.Email);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, GuidIdGenerator.Instance.NewId()),
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
            result = new Models.JsonResponseModel<JWT.TokenEntity> { Msg = "sign up failed," + string.Join(",", signUpResult.Errors.Select(e => e.Description).ToArray()), Status = Models.JsonResponseStatus.ProcessFail };
        }
        return Ok(result);
    }
}
