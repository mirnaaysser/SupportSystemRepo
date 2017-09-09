using BizSparkSupport.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BizSparkSupport.MVC.Areas.Admin.ViewModels
{
    public class CaseDetailsVM
    {
        public HttpPostedFileBase File { get; set; }
        public long FileID { get; set; }
        public byte[] Content { get; set; }
        public long CaseID { get; set; }
        public string FileName { get; set; }
        public string SavedFileName { get; set; }
        
        public List<Employee> AllEmployees { get; set; }

        public virtual Case Case { get; set; }

        //public virtual User user { get; set; }
        //public virtual Employee_Case EmployeeCase { get; set; }

        public List<Employee_Case> AllEmployeeCases { get; set; }
        public List<User> AllAssignedEmployees { get; set; }
        public MessagesVM MessagesModel { get; set; }
    }
}