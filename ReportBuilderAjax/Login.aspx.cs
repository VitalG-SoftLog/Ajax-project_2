using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using ReportBuilderAjax.Web.Common;

namespace ReportBuilderAjax.Web
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if(!IsPostBack)
            {
                
            }
        }

        protected void SingInButton_Click(object sender, EventArgs e)
        {
            AuthenticationService autService = new AuthenticationService();
            if (string.IsNullOrEmpty(loginTextBox.Text) || string.IsNullOrEmpty(passTextBox.Text))
            {
                ErrorMessage.Visible = true;
                return;
            }
            User user = autService.Login(loginTextBox.Text, passTextBox.Text, false, null);
            if (user != null)
            {
                Dictionary<string, object> authenticateData = new Dictionary<string, object>();
                authenticateData.Add("userId", user.ID);
                authenticateData.Add("userName", user.UserID);
                authenticateData.Add("associationNumber", user.AssociationNumber);
                authenticateData.Add("name", user.Name);
                UserContext userContext = new UserContext(authenticateData);
                HttpContext.Current.Session[user.UserID] = userContext;
                FormsAuthentication.SetAuthCookie(user.UserID, true);
                FormsAuthentication.RedirectFromLoginPage(user.UserID, false);
                setIntegrationInfo(HttpContext.Current, userContext);
                setAuthCookieValue(HttpContext.Current, user.UserID);
            }
            else
            {
                ErrorMessage.Visible = true;
            }
        }

        private void setAuthCookieValue(HttpContext context, string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            DateTime expiredDate = DateTime.Now.AddMinutes(Convert.ToInt32(ConfigurationManager.AppSettings[Const.INTEGRATION_SESSION_PERIOD]));
            string encryptValue = FormsAuthentication.Encrypt(new FormsAuthenticationTicket(1, "auth", DateTime.Now, expiredDate, true, value));

            context.Response.Cookies.Remove(Const.AUTH_COOKIE_NAME);
            HttpCookie responseUserCookie = new HttpCookie(Const.AUTH_COOKIE_NAME)
            {
                Expires = expiredDate,
                Value = encryptValue
            };
            context.Response.Cookies.Add(responseUserCookie);
        }

        private void setIntegrationInfo(HttpContext context, UserContext userContext)
        {
            IIntegrationInfo integrationInfo = context.Handler as IIntegrationInfo;
            if (integrationInfo != null)
            {
                integrationInfo.UserContext = userContext;
            }
        }
    }
}