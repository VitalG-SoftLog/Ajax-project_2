
namespace ReportBuilderSL.Ajax
{
    using Microsoft.Practices.EnterpriseLibrary.Logging;
    using System.ServiceModel.DomainServices.Hosting;
    using System.ServiceModel.DomainServices.Server;



	// TODO: Create methods containing your application logic.
	[EnableClientAccess()]
	public class LogDomainService : DomainService
	{
		[Invoke]
		public void Log(string message)
		{
			//TODO: Log error message somewhere

			Logger.Write(message , "SLLog");

		}
	}
}


