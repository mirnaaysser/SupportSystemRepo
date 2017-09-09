using System.Web.Mvc;
using System.Web.Routing;

namespace BizSparkSupport.MVC.Filters
{
    public class LoggedInAdminFilter : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Session["USER"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary{
                    { "controller", "Login" },
                    { "action", "Login" },
                    { "area", "Admin" }
                });
            }

            //base.OnAuthorization(filterContext);
        }
    }
}