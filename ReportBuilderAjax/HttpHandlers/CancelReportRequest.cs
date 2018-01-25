using System.Runtime.Serialization;
using System;

namespace ReportBuilderAjax.Web.HttpHandlers
{
    [DataContract]
    public class CancelReportRequest : DuplexMessage
    {
        [DataMember]
        public Guid ReportRequestGuid { get; set; }
    }
}