//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;
//using System.Web.Optimization;
//using System.Web.Routing;
//using BizSparkSupport.DAL;
//using BizSparkSupport.MVC.Repositories;
//using System.Configuration;
//using System.Data.SqlClient;
//using BizSparkSupport.MVC.Notification;
//using ServiceBrokerListener.Domain;

//namespace BizSparkSupport.MVC
//{
//    public class MvcApplication : System.Web.HttpApplication
//    {
//        protected void Application_Start()
//        {
//            AreaRegistration.RegisterAllAreas();
//            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
//            RouteConfig.RegisterRoutes(RouteTable.Routes);
//            BundleConfig.RegisterBundles(BundleTable.Bundles);

//        }

//        protected void Application_End()
//        {
//            string con = ConfigurationManager.ConnectionStrings["sqlConString"].ConnectionString;
//            //here we will stop Sql Dependency
//            SqlDependency.Stop(con);

//        }

//        //void StopListeners()
//        //{
//        //    NotificationComponent.SharedInstance.ACListener.Stop();
//        //    NotificationComponent.SharedInstance.CSListener.Stop();
//        //    NotificationComponent.SharedInstance.MSGListener.Stop();

//        //}

//        protected void Session_Start()
//        {
//            Session["UNITOFWORK"] = new GenericUnitOfWork();

//            if (Request.Cookies["LoginCookie"] != null)
//            {
//                long id;
//                string token = Request.Cookies["LoginCookie"]["RemeberMe"];
//                Int64.TryParse(Request.Cookies["LoginCookie"]["UserID"].ToString(), out id);

//                GenericUnitOfWork unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];

//                GenericRepository<User> userRepository = unitOfWork.GetRepoInstance<User>();
//                User testedUser = userRepository.GetBy(u => u.UserID == id).FirstOrDefault();

//                if (testedUser != null)
//                {
//                    if (testedUser.RememberToken == token)
//                    {
//                        Session["USER"] = testedUser;
//                    }
//                }
//            }
//            else
//            {
//            }
//        }
//    }
//}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using BizSparkSupport.DAL;
using BizSparkSupport.MVC.Repositories;
using System.Configuration;
using System.Data.SqlClient;
using BizSparkSupport.MVC.Notification;
using ServiceBrokerListener.Domain;

namespace BizSparkSupport.MVC
{
    public class MvcApplication : System.Web.HttpApplication
    {
        string con = ConfigurationManager.ConnectionStrings["sqlConString"].ConnectionString;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

        }




        protected void Application_End()
        {
            //here we will stop Sql Dependency
            StopListeners();

        }
        void StopListeners()
        {

            SqlDependency.Stop(con);

        }

        protected void Session_Start()
        {
            Session["UNITOFWORK"] = new GenericUnitOfWork();

            //if (Request.Cookies["LoginCookie"] != null)
            //{
            //    long id;
            //    string token = Request.Cookies["LoginCookie"]["RemeberMe"];
            //    Int64.TryParse(Request.Cookies["LoginCookie"]["UserID"].ToString(), out id);

            //    GenericUnitOfWork unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];

            //    GenericRepository<User> userRepository = unitOfWork.GetRepoInstance<User>();
            //    User testedUser = userRepository.GetBy(u => u.UserID == id).FirstOrDefault();

            //    if (testedUser != null)
            //    {
            //        if (testedUser.RememberToken == token)
            //        {
            //            Session["USER"] = testedUser;
            //        }
            //    }
            //}
            //else
            //{
            //}
        }
    }
}

