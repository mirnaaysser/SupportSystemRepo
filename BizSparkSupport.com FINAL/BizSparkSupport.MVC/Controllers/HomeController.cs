using BizSparkSupport.DAL;
using BizSparkSupport.MVC.Filters;
using BizSparkSupport.MVC.Notification;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BizSparkSupport.MVC.Controllers
{
    public class HomeController : Controller
    {
        [LoggedOutFilter]
        public ActionResult Index()
        {
            return View();
        }

        //public JsonResult GetMessages()
        //{
        //    Debug.WriteLine("Test");
        //    var notificationRegisterTime = Session["LastReceivedMsg"] != null ? Convert.ToDateTime(Session["LastReceivedMsg"]) : DateTime.Now;
        //    // NotificationComponent NC = new NotificationComponent();
        //    User u = (User)Session["USER"];
        //    var list = NotificationComponent.SharedInstance.GetNewMessages(notificationRegisterTime, 2);
        //    //update session here for get only new added contacts (notification)
        //    Session["LastReceivedMsg"] = DateTime.Now;
        //    return new JsonResult { Data = list, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        //}
    }
}