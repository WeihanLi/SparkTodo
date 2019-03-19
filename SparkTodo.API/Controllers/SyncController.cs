using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SparkTodo.API.Controllers
{
    /// <summary>
    /// new Todo
    /// </summary>
    [Authorize]
    [ApiVersion("2.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class SyncController : Controller
    {
    }
}
