using System.Web;
using System.Web.SessionState;
using ReportBuilderAjax.Web.Common;

namespace ReportBuilderAjax.Web.HttpHandlers
{
    public class IntegrationHandler : IHttpHandler, IRequiresSessionState, IIntegrationInfo
    {   
        #region IHttpHandler Members

        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            if (UserContext != null)
            {
                context.Response.Redirect(UserContext.OpenAsMainPage ? "ReportBuilder.aspx" : string.Format("ReportBuilder.aspx?userId={0}", UserContext.UserID), true);
            }
        }

        #endregion

        public UserContext UserContext
        {
            get; set;
        }
    }
}