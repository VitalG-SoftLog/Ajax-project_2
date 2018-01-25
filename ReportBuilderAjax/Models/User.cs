using System.Collections.Generic;

namespace ReportBuilderAjax.Web
{
	using System.Runtime.Serialization;
    using System.ServiceModel.DomainServices.Server.ApplicationServices;

    public partial class User : UserBase
    {
        //// NOTE: Profile properties can be added for use in Silverlight application.
        //// To enable profiles, edit the appropriate section of web.config file.
        ////
        //// [DataMember]
        //// public string MyProfileProperty { get; set; }

        //[DataMember]
        //public string FriendlyName
        //{
        //    get;
        //    set;
        //}

        [DataMember]
        public int ID
        {
            get;
            set;
        }

        [DataMember]
        public string UserID
        {
            get;
            set;
        }

        [DataMember]
        public string Password
        {
            get;
            set;
        }

        [DataMember]
        public string AssociationNumber
        {
            get;
            set;
        }

        [DataMember]
        public string AssociationName
        {
            get;
            set;
        }

        [DataMember]
        public int UserAuthorizationCheckTicks
        {
            get;
            set;
        }

        [DataMember]
        public string ReportFileStorage
        {
            get;
            set;
        }

        [DataMember]
        public string EmailAddress
        {
            get;
            set;
        }

        [DataMember]
        public bool IsAdmin
        {
            get;
            set;
        }

        [DataMember]
        public List<string> CheckpointIds
        {
            get;
            set;
        }

        [DataMember]
        public string LogoFileStorage
        {
            get;
            set;
        }

        [DataMember]
        public string LogoFileHandler
        {
            get;
            set;
        }

        [DataMember]
        public List<string> UserAssociationList
        {
            get; 
            set; 
        }
    }
}
