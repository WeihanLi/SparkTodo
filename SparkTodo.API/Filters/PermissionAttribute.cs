using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace SparkTodo.API.Filters
{
    public class NoPermissionRequiredAttribute : ActionFilterAttribute
    {
    }

    public class PermissionRequiredAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Session.Keys.Contains("User"))
            {
                filterContext.Result = new RedirectResult("~/Admin/Account/Login");
            }
            base.OnActionExecuting(filterContext);
        }
    }
}