using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BizSparkSupport.DAL;

namespace BizSparkSupport.MVC.Areas.Admin.ViewModels
{
    public class MessagesVM
    {
        public List<Message> Messages { get; set; }
        public Message CurrentMessage { get; set; }
    }
}