using System.Web.Mvc;
using System.Web.Routing;
using BizSparkSupport.DAL;
using BizSparkSupport.MVC.Repositories;
using System.Linq;
using System;

namespace BizSparkSupport.MVC.Filters
{
    public class LoggedOutFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            GenericUnitOfWork unitOfWork;
            string controller = "Case";
            string action = "Dashboard";
            string area = "";
            if (filterContext.HttpContext.Session["USER"] != null)
            {
                bool IsEmployee = false;
                User currentUser = (User)filterContext.HttpContext.Session["USER"];

                unitOfWork = (GenericUnitOfWork)filterContext.HttpContext.Session["UNITOFWORK"];
                GenericRepository<Employee> empRepository = unitOfWork.GetRepoInstance<Employee>();

                if (empRepository.GetBy(e => e.EmployeeID == currentUser.UserID).Count() != 0)
                {
                    IsEmployee = true;
                }
                if (IsEmployee)
                {
                    controller = "Support";
                    action = "Dashboard";
                    area = "Admin";
                }
                filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary{
                    { "controller", controller },
                    { "action", action },
                    { "area" , area}
                });
            }

            //base.OnActionExecuting(filterContext);
        }
    }
}