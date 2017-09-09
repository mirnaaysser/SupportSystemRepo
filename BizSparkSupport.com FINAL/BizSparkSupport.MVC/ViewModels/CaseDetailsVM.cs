using BizSparkSupport.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BizSparkSupport.MVC.ViewModels
{
    public class CaseDetailsVM
    {
        public long CaseID { get; set; }
        public virtual Case Case { get; set; }
        public MessagesVM MessagesModel { get; set; }

    }
}