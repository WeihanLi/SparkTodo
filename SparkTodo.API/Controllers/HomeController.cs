using Microsoft.AspNetCore.Mvc;

namespace SparkTodo.API.Controllers
{
    /// <summary>
    /// Home
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// HomePage
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Index()
        {
            //return LocalRedirectPermanent("/swagger/");
            return View();
        }

        /// <summary>
        /// ErrorPage
        /// </summary>
        /// <returns></returns>
        public IActionResult Error()
        {
            return View();
        }
    }
}
