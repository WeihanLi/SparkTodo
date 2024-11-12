// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics.Metrics;
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
        {
            var meter = new Meter("test");
            var counter = meter.CreateCounter<int>("counter");
            counter.Add(1);
        }

        {
            var meter = new Meter("test1");
            var counter = meter.CreateCounter<int>("counter");
            counter.Add(1);
        }
        
        var responseText = await HttpHelper.HttpClient.GetStringAsync("https://reservation.weihanli.xyz/health");
        return Ok(Result.Success(responseText));
    }
}
