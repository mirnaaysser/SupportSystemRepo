using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BizSparkSupport.MVC.Filters
{
    public class LoggedInStartupFilter : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Session["USER"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary{
                    { "controller", "User" },
                    { "action", "Login" },
                    { "area", "" }
                });
            }

            //base.OnAuthorization(filterContext);
        }
    }
}