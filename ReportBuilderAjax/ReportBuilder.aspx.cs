using System;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using ReportBuilderAjax.Web.Common;

namespace ReportBuilderAjax.Web
{
    public partial class _Default : BasePage
    {

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void SingOutButton_Click(object sender, EventArgs e)
        {
            string authCookieValue = getAuthCookieValue(HttpContext.Current);
            if (authCookieValue != null)
            {
                HttpContext.Current.Session[authCookieValue] = null;
            }
            FormsAuthentication.SignOut();
            FormsAuthentication.RedirectToLoginPage();
        }

        private string getAuthCookieValue(HttpContext context)
        {
            HttpCookie requestUserCookie = context.Request.Cookies[Const.AUTH_COOKIE_NAME];
            if (requestUserCookie == null || string.IsNullOrEmpty(requestUserCookie.Value)) return null;
            FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(requestUserCookie.Value);

            string guidValue = ticket.UserData;
            context.Request.Cookies.Remove(Const.AUTH_COOKIE_NAME);

            return (ticket.Expiration < DateTime.Now || string.IsNullOrEmpty(guidValue)) ? null : guidValue;
        }
    }
}
