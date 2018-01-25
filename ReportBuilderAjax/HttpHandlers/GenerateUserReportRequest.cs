using System.Runtime.Serialization;
using System;

namespace ReportBuilderAjax.Web.HttpHandlers
{
    [DataContract]
    public class GenerateUserReportRequest : GenerateReportRequest
    {
        [DataMember]
        public int UserReportID { get; set; }
    }
}