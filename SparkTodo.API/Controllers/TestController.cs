// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using WeihanLi.Common.Helpers;
using WeihanLi.Common.Models;

namespace SparkTodo.API.Controllers;

[ApiController]
[ApiVersionNeutral]
[Route("api/[controller]/[action]")]
public class TestController : ControllerBase
{
    [HttpGet]
    public IActionResult Test()
    {
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> ExternalApiTest()
    {
        var responseText = await HttpHelper.HttpClient.GetStringAsync("https://reservation.weihanli.xyz/health");
        return Ok(Result.Success(responseText));
    }
}
