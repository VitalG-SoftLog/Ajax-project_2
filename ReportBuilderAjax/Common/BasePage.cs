using System.Web.UI;

namespace ReportBuilderAjax.Web.Common
{
    public class BasePage : Page, IIntegrationInfo
    {
        public UserContext UserContext
        {
            get;
            set;
        }

        public bool IsMainWindow
        {
            get
            {
                if (UserContext != null)
                {
                    return UserContext.OpenAsMainPage;
                }
                return true;
            }
        }
    }
}
