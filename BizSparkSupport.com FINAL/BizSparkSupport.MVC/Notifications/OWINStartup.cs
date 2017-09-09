using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(BizSparkSupport.MVC.Notifications.OWINStartup))]

namespace BizSparkSupport.MVC.Notifications
{
    public class OWINStartup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();        }
    }
}
