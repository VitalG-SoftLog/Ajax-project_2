using System;
using System.Collections.Generic;
using System.Configuration;

namespace ReportBuilderAjax.Web.Common
{
    public class UserContext
    {
        public UserContext(Dictionary<string, object> initParams) 
        {
            UserID = initParams["userName"].ToString();
            ID = Convert.ToInt32(initParams["userId"]);
            AssociationNumber = initParams["associationNumber"].ToString();
            Name = initParams["name"].ToString();
        }

        public string UserID { get; private set; }
        public bool OpenAsMainPage { get; set; }
        public int ID { get; private set; }
        public string AssociationNumber { get; set; }
        public string Name { get; private set; }
    }
}
