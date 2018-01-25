using System.Collections.Generic;
using System.Runtime.Serialization;
using System;

namespace ReportBuilderAjax.Web.HttpHandlers
{
    [DataContract]
    public class GenerateReportResponse : DuplexMessage
    {
        [DataMember]
        public string ReportUrl { get; set; }

        [DataMember]
        public Guid ReportRequestGuid { get; set; }

        [DataMember]
        public ReportRequestStatus Status { get; set; }

        [DataMember]
        public string StatusMessage { get; set; }

        [DataMember]
        public string Content { get; set; }

        [DataMember]
        public List<Dictionary<string, object>> GridData { get; set; }

        [DataMember]
        public List<RptField> UserReportFields { get; set; }
    }
}