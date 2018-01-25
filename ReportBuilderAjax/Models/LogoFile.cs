using System.Runtime.Serialization;

namespace ReportBuilderAjax.Web
{
    [DataContract]
    public class LogoFile
    {
        [DataMember]
        public string ClientNumber
        {
            get;
            set;
        }

        [DataMember]
        public string Filename
        {
            get;
            set;
        }

        [DataMember]
        public byte[] File
        {
            get;
            set;
        }

    }
}