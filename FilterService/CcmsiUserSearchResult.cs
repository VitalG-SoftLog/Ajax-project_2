using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReportBuilderAjax.Web.Services;

namespace ReportBuilderAjax.Web.Services
{
    public class CcmsiUserSearchResult
    {
        public int ItemsPerPage { get; set; }
        public int CountWithoutTruncate { get; set; }
        public List<CcmsiUser> UserList { get; set; }
    }
}
