// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using SparkTodo.API.JWT;
using SparkTodo.API.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WeihanLi.Common.Models;

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
    private readonly UserManager<UserAccount> _userManager;
    private readonly SignInManager<UserAccount> _signInManager;
    private readonly ITokenGenerator _tokenGenerator;

    /// <summary>
    /// AccountController .ctor
    /// </summary>
    /// <param name="userManager">userManager</param>
    /// <param name="signInManager">signInManager</param>
    /// <param name="userRepository">userRepository</param>
    /// <param name="tokenGenerator"></param>
    public AccountController(UserManager<UserAccount> userManager,
        SignInManager<UserAccount> signInManager,
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
        Result result;
        var signinResult = await _signInManager.PasswordSignInAsync(loginModel.Email, loginModel.Password, true, lockoutOnFailure: false);
        if (signinResult.Succeeded)
        {
            var userInfo = await _userRepository.FetchAsync(u => u.Email == loginModel.Email);
            ArgumentNullException.ThrowIfNull(userInfo);
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
            result = Result.Success(userToken);
        }
        else
        {
            result = Result.Fail(signinResult.IsLockedOut ? "Account locked out" : "failed to authenticate");
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
        var userInfo = new UserAccount()
        {
            UserName = regModel.Email,
            Email = regModel.Email,
            EmailConfirmed = true,
            CreatedTime = DateTime.UtcNow
        };
        Result<UserTokenEntity> result;
        var signUpResult = await _userManager.CreateAsync(userInfo, regModel.Password);
        if (signUpResult.Succeeded)
        {
            userInfo = await _userRepository.FetchAsync(u => u.Email == regModel.Email);
            ArgumentNullException.ThrowIfNull(userInfo);
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
            result = Result.Success(userToken);
        }
        else
        {
            result = Result.Fail<UserTokenEntity>(
                $"sign up failed,{string.Join(",", signUpResult.Errors.Select(e => e.Description).ToArray())}");
        }
        return Ok(result);
    }
}
