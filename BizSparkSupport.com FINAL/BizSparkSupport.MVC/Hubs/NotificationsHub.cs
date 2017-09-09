//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using Microsoft.AspNet.SignalR;

//namespace BizSparkSupport.MVC.Hubs
//{
//    public class NotificationsHub : Hub
//    {
//        public void Hello()
//        {
//            Clients.All.hello();
//        }
//    }
//}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace BizSparkSupport.MVC.Hubs
{
    public class NotificationsHub : Hub
    {

        private static NotificationsHub instance;

        public static NotificationsHub SharedInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new NotificationsHub();
                }
                return instance;
            }
        }
        public static void notify(string message)
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<MessagesHub>();

            context.Clients.All.notify(message);
        }
    }
}