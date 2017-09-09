using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BizSparkSupport.MVC.Areas.Admin.ViewModels
{
    public class CatgeroyView
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string CategoryDescription { get; set; }
        public string CategoryAdmin { get; set; }
        public int CasesNumber { get; set; }

    }
}