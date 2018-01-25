using System.ServiceModel;
using System.ServiceModel.Activation;
using ReportBuilderSL.Ajax;

namespace ReportBuilderAjax.Web.HttpHandlers
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class LogoService
    {
        LogoManager _logoManager = new LogoManager();
        
        [OperationContract]
        public bool Save(LogoFile file, string userId)
        {
            return _logoManager.Save(file, userId);
        }


        [OperationContract]
        public bool Delete(string assn, string userId)
        {
            return _logoManager.Delete(assn, userId);
        }


        [OperationContract]
        public bool Exists(string assn, string userId)
        {
            return _logoManager.Exists(assn, userId);
        }
    }
}
