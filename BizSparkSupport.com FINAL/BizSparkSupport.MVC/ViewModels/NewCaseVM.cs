using BizSparkSupport.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BizSparkSupport.MVC.ViewModels
{
    public class NewCaseVM
    {
        public HttpPostedFileBase File { get; set; }
        public long FileID { get; set; }
        public byte[] Content { get; set; }
        public long CaseID { get; set; }
        public string FileName { get; set; }
        public string SavedFileName { get; set; }

        public virtual Case Case { get; set; }
    }
}