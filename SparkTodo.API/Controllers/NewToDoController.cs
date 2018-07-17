using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SparkTodo.API.Controllers
{
    /// <summary>
    /// new Todo
    /// </summary>
    [Authorize]
    [Route("api/v2/[controller]")]
    public class SyncController : Controller
    {
    }
}