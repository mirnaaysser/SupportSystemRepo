using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BizSparkSupport.MVC.Areas.Admin.ViewModels
{
    public class CategoryCases
    {
        public long CaseID { get; set; }
        public string Subject { get; set; }
        public string CompanyName { get; set; }
        public string StatusName { get; set; }
        public System.DateTime SubmissionDate { get; set; }
        public System.String ClosedAt { get; set; }
    }
}