using BizSparkSupport.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BizSparkSupport.MVC.Areas.Admin.ViewModels
{
    public class StartUpDetails
    {
        public string StartUpName {get;set;}
        public string telephone { get; set; }
        public string email { get; set; }
        public string noCases { get; set; }
        public bool Active { get; set; }
        public string Date { get; set; }
        public long StartUpID { get; set; }
        public List<Case> listCases { get; set; }
    }
}