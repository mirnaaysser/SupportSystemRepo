using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BizSparkSupport.DAL;
using BizSparkSupport.MVC.ViewModels;

namespace BizSparkSupport.MVC.ViewModels
{
    public class Dashboard
    {
        //History
        public List<Case> Closed { get; set; }
        public List<Case> Current { get; set; }
        public List<Case> Escalated { get; set; }

        public NewCaseVM newCaseVM { get; set; }
    }
}