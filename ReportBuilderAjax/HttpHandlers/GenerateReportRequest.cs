using System.Runtime.Serialization;
using System;

namespace ReportBuilderAjax.Web.HttpHandlers
{
    [DataContract]
    public class GenerateReportRequest : DuplexMessage
    {
        [DataMember]
        public Guid ReportRequestGuid { get; set; }

        // this is used on server side only for monitoring the progress of a request
        public RunningReportStatus Status { get; set; }
    }
}