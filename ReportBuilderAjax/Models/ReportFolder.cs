using System;
using System.Runtime.Serialization;

namespace ReportBuilderAjax.Web
{
    public partial class ReportFolder
    {
        [DataMember]
        public bool IsUseWithCheckpoint { get; set; }
    }
}