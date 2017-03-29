using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace SparkTodo.API.Filters
{
    /// <summary>
    /// Custom NoPermissionRequiredAttribute
    /// </summary>
    public class NoPermissionRequiredAttribute : ActionFilterAttribute
    {
    }

    /// <summary>
    /// Custom PermissionRequiredAttribute
    /// </summary>
    public class PermissionRequiredAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// OnActionExecuting
        /// </summary>
        /// <param name="filterContext">filterContext</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Session.Keys.Contains("User"))
            {
                filterContext.Result = new JsonResult(new Models.JsonResponseModel<string>() { Status = Models.JsonResponseStatus.AuthFail , Msg = "需要登录" });
            }
            base.OnActionExecuting(filterContext);
        }
    }
}