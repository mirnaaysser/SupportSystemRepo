using System.Collections.Generic;
using BizSparkSupport.DAL;

namespace BizSparkSupport.MVC.ViewModels
{
    public class MessagesVM
    {
        public List<Message> Messages { get; set; }
        public Message CurrentMessage { get; set; }
    }
}