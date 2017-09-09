using System.Web.Mvc;
using System.Web.Routing;
using BizSparkSupport.DAL;
using BizSparkSupport.MVC.Repositories;
using System.Web;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BizSparkSupport.MVC.Filters
{
    // Redirect support team member according to his/her role (Admin - Admin Category - Support).
    public class LoginFilter : AuthorizeAttribute
    {
        GenericUnitOfWork unitOfWork;
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            string controller = "Support";
            string action = "Dashboard";
            string area = "Admin";

            bool flag = false;
            if (filterContext.HttpContext.Session["USER"] != null)
            {
                User user = (User)filterContext.HttpContext.Session["USER"];

                unitOfWork = (GenericUnitOfWork)filterContext.HttpContext.Session["UNITOFWORK"];
                GenericRepository<Employee> empRepository = unitOfWork.GetRepoInstance<Employee>();
                var result = empRepository.GetBy(e => e.EmployeeID == user.UserID).FirstOrDefault();

                Employee currentEmp = null;
                if (result != null) {
                    currentEmp = result;
                }
                // Admin
                if (currentEmp.RoleID == 1)
                {
                    controller = "Admin";
                    action = "Dashboard";
                    area = "Admin";

                    flag = true;
                }
                // Category Admin
                else if (currentEmp.RoleID == 2)
                {
                    controller = "CategoryAdmin";
                    action = "CategoryAdminDashboard";
                    area = "Admin";

                    flag = true;
                }
                // Support Team
                else if (currentEmp.RoleID == 3)
                {
                    //controller = "Support";
                    //action = "Dashboard";
                    //area = "Admin";
                }
                else
                {
                    throw new HttpException(404, "Not found");
                }
            }
            if (flag == true)
            {
                filterContext.Result = new RedirectToRouteResult(
                            new RouteValueDictionary{
                            { "controller", controller },
                            { "action", action },
                            { "area" , area}
                            });
            }

        }
    }
}