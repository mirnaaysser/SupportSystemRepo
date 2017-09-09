using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BizSparkSupport.DAL;

namespace BizSparkSupport.MVC.Areas.Admin.ViewModels
{
    public class CatAdminDashboardVM
    {
        //Cases
        public List<Case> AssignedNotOpen { get; set; }
        public List<Case> AssignedOpen { get; set; }
        public List<Case> New { get; set; }
        public List<Case> Escalated { get; set; }

        //History
        public List<Case> Solved { get; set; }
    }
}