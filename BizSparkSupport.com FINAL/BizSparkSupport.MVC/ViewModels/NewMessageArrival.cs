using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BizSparkSupport.MVC.ViewModels
{
    public class NewMessageArrival
    {
        public string MessageContent { get; set; }
        public long MessageID { get; set; }
        public string CaseSubject { get; set; }
        public string StartupName { get; set; }
        public long CaseID { get; set; }

    }
}