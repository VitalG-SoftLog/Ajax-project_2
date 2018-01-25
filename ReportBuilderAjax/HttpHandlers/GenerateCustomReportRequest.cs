using System.Runtime.Serialization;
using System;
using System.Collections.Generic;

namespace ReportBuilderAjax.Web.HttpHandlers
{
    [DataContract]
    public class GenerateCustomReportRequest : GenerateReportRequest
    {
        [DataMember]
        public int UserID { get; set; }

        [DataMember]
        public string AssociationNumber { get; set; }

        [DataMember]
        public int ReportID { get; set; }

        [DataMember]
        public int FormatTypeId { get; set; }

        [DataMember]
        public int ReportLayoutStyleID { get; set; }

        [DataMember]
		public List<RptField> Fields { get; set; }

        [DataMember]
		public List<RptField> UserSummarizeRptField { get; set; }

        [DataMember]
        public List<RptParameter> Parameters { get; set; }

        [DataMember]
        public bool IsSummaryOnly { get; set; }

        [DataMember]
        public bool IncludeTitlePage { get; set; }
    }
}