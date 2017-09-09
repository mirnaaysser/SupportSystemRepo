using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BizSparkSupport.MVC.ViewModels
{
    public class CaseArrival
    {
        public long CaseID { get; set; }
        public string Subject { get; set; }
        public string StartUpName { get; set; }
        public string PriorityColor { get; set; }
        public string listText { get; set; }
        public string caseMsg { get; set; }

    }
}