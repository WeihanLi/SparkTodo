// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using WeihanLi.Web.Authorization.Jwt;

namespace SparkTodo.API.JWT;

public class JwtTokenServiceConfigureOptions :
    IPostConfigureOptions<JwtBearerOptions>
{
    private readonly IOptions<JwtTokenOptions> _options;

    public JwtTokenServiceConfigureOptions(IOptions<JwtTokenOptions> options)
    {
        _options = options;
    }

    public void PostConfigure(string name, JwtBearerOptions options)
    {
        options.Audience = _options.Value.Audience;
        options.ClaimsIssuer = _options.Value.Issuer;
        options.TokenValidationParameters = _options.Value.GetTokenValidationParameters();
    }
}
