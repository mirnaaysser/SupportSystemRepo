using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BizSparkSupport.DAL;

namespace BizSparkSupport.MVC.Areas.Admin.ViewModels
{
    public class Dashboard
    {
        //Cases
        public List<Case> Assigned { get; set; }
        public List<Case> Open { get; set; }
        public List<Case> New { get; set; }

        //History
        public List<Case> Solved { get; set; }
    }
}