using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ReportBuilderAjax.Web
{
    public partial class SubRptParameter
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

        public string SubReportParameterName
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }       

        public string DefaultValue
        {
            get;
            set;
        }

    }
}
