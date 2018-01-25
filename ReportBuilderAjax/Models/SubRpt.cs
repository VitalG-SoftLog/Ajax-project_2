using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ReportBuilderAjax.Web
{
    public partial class SubRpt
    {
        [Key]
        [ReadOnly(true)]
        public int ID
        {
            get;
            set;
        }

        public int SubReportID
        {
            get;
            set;
        }

        public int ReportID
        {
            get;
            set;
        }


        public string SubReportName
        {
            get;
            set;
        }

        public string SPName
        {
            get;
            set;
        }

      

        public int OrderIndex
        {
            get;
            set;
        }
    }
}