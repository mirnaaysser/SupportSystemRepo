using BizSparkSupport.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BizSparkSupport.MVC.Areas.Admin.ViewModels
{
    public class EmployeeCasesVM
    {
        public virtual Employee Employee { get; set; }

        public int CasesNumber { get; set; }
    }
}