using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using ReportBuilderAjax.Web.Common;
using System.IO;
using System.Text;
using System.Linq;

namespace ReportBuilderAjax.Web.HttpHandlers
{
    public sealed class IntegrationModule : IHttpModule
    {
        public void Init(HttpApplication app)
        {
            app.AcquireRequestState += onEnter;
            app.EndRequest += onLeave;
        }

        public void Dispose()
        {
        }
        
        private void onEnter(object source, EventArgs eventArgs)
        {
            HttpApplication application = source as HttpApplication;
            if(application == null) return;
            HttpContext context = application.Context;
            if (context.Handler == null || !(context.Handler is IIntegrationInfo) || !(context.Handler is IRequiresSessionState)) return;

            invokeAction(context);
        }

        private void onLeave(object source, EventArgs eventArgs)
        {
            HttpApplication application = source as HttpApplication;
            if (application == null) return;

            HttpContext context = application.Context;
            if (context.Response.StatusCode == (int)HttpStatusCode.Forbidden)
            {
                context.Response.Clear();
            }
        }

        private void invokeAction(HttpContext context)
        {
            UserContext userContext = null;

            if (string.IsNullOrEmpty(context.Request.QueryString[Const.PARAM_GUID]))
            {
                string authCookieValue = getAuthCookieValue(context);
                if (authCookieValue == null || context.Session[authCookieValue] == null || !(context.Session[authCookieValue] is UserContext))
                {
                    setForbiddenStatus(context);
                    return;
                }

                setAuthCookieValue(context, authCookieValue);
                userContext = (UserContext)context.Session[authCookieValue];
                setIntegrationInfo(context, userContext);
            }
            else
            {
                string guid = context.Request.QueryString[Const.PARAM_GUID];
                Dictionary<string, object> authenticateData = getAuthenticateData(guid);
                userContext = new UserContext(authenticateData);
                context.Session[userContext.UserID] = userContext;
                setIntegrationInfo(context, userContext);
                setAuthCookieValue(context, userContext.UserID);
            }
        }

        private Dictionary<string, object> getAuthenticateData(string guid)
        {
            AuthenticationService autService = new AuthenticationService();
            User user = autService.Login(null, null, false, guid);
            Dictionary<string, object> authenticateData = null;
            if (user != null)
            {
                authenticateData = new Dictionary<string, object>();
                authenticateData.Add("userId", user.ID);
                authenticateData.Add("userName", user.UserID);
                authenticateData.Add("associationNumber", user.AssociationNumber);
                authenticateData.Add("name", user.Name);
            }       
            return authenticateData;
        }

        private void setForbiddenStatus(HttpContext context)
        {
            if (context.Request.Url.AbsolutePath.Contains(Const.PARAM_LOGIN_PAGE)) return;
            /*context.Response.ClearHeaders();
            context.Response.ClearContent();
            context.Response.Clear();
            context.Response.ContentType = "application/json";*/
            context.Response.StatusCode = (int) HttpStatusCode.Unused;
            context.Response.AddHeader("jsonerror", "true");
            using (StreamWriter writer = new StreamWriter(context.Response.OutputStream, new UTF8Encoding(false)))
            {
                /*string template =
                    "<html><body><script type=\"text/javascript\">alert('Authentication failed. Please try to login again.');</script></body></html>";
                writer.Write(template);
                writer.Flush(); */
                context.Response.Redirect("Login.aspx");
            }

            /*context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            context.Response.StatusDescription = HttpWorkerRequest.GetStatusDescription((int)HttpStatusCode.Forbidden);*/
            
            context.ApplicationInstance.CompleteRequest();
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

