using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Activation;
using ReportBuilderAjax.Web.ReportBuilder;

namespace ReportBuilderAjax.Web.HttpHandlers
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class RdlService
    {
        [OperationContract]
        public byte[] CustomizeReportDefinition(byte[] reportDefinition, Dictionary<string, object> queryParameters)
        {
            ReportCustomizationExtensionManager reportCustomizationExtensionManager = new ReportCustomizationExtensionManager(reportDefinition, queryParameters);
            return reportCustomizationExtensionManager.CustomizeReport();
        }
    }
}
