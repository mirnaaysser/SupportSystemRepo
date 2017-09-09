using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BizSparkSupport.MVC.Areas.Admin.ViewModels
{
    public class ViewEmployee
    {
        public long UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string UserName { get; set; }
       // public int CategoryID { get; set; }
        public string CategoryName { get; set; }
       // public int RoleID { get; set; }
        public string RoleName { get; set; }
        public int TotalCases { get; set; }
        public int SolvedCases { get; set; }
        public int esclatedCases { get; set; }
        public int pendingCases { get; set; }
    }
}