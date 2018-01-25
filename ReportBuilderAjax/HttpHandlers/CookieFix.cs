using System;
using System.Web;
using System.Web.Security;

namespace ReportBuilderAjax.Web.HttpHandlers
{
    public static class CookieFix
    {
        private static void UpdateCookie(string cookieName, string cookieValue)
        {
            var cookie = HttpContext.Current.Request.Cookies.Get(cookieName);
            if (cookie == null)
            {
                cookie = new HttpCookie(cookieName);
                HttpContext.Current.Request.Cookies.Add(cookie);
            }
            cookie.Value = cookieValue;
            HttpContext.Current.Request.Cookies.Set(cookie);

        }

        public static string GetUploaderQueryString()
        {
            var context = HttpContext.Current;
            var authCoockie = context.Request.Cookies[FormsAuthentication.FormsCookieName];
            return "?ASPSESSID=" + context.Session.SessionID + "&AUTHID=" +
                   (authCoockie == null ? "" : authCoockie.Value);
        }

        public static void FixCookiesForNonIeBrowsers(HttpResponse response)
        {
            /* Fix for the Flash Player Cookie bug in Non-IE browsers.
            * Since Flash Player always sends the IE cookies even in FireFox
            * we have to bypass the cookies by sending the values as part of the POST or GET
            * and overwrite the cookies with the passed in values.
            *
            * The theory is that at this point (BeginRequest) the cookies have not been read by
            * the Session and Authentication logic and if we update the cookies here we'll get our
            * Session and Authentication restored correctly
            */

            try
            {
                const string sessionParamName = "ASPSESSID";
                const string sessionCookieName = "ASP.NET_SESSIONID";

                if (HttpContext.Current.Request.Form[sessionParamName] != null)
                {
                    UpdateCookie(sessionCookieName, HttpContext.Current.Request.Form[sessionParamName]);
                }
                else if (HttpContext.Current.Request.QueryString[sessionParamName] != null)
                {
                    UpdateCookie(sessionCookieName, HttpContext.Current.Request.QueryString[sessionParamName]);
                }
            }
            catch (Exception exception)
            {
                response.StatusCode = 500;
                response.Write("Error Initializing Session");
            }

            try
            {
                const string authParamName = "AUTHID";
                var authCookieName = FormsAuthentication.FormsCookieName;

                if (HttpContext.Current.Request.Form[authParamName] != null)
                {
                    UpdateCookie(authCookieName, HttpContext.Current.Request.Form[authParamName]);
                }
                else if (HttpContext.Current.Request.QueryString[authParamName] != null)
                {
                    UpdateCookie(authCookieName, HttpContext.Current.Request.QueryString[authParamName]);
                }

            }
            catch (Exception exception)
            {
                response.StatusCode = 500;
                response.Write("Error Initializing Forms Authentication");
            }
        }
    }
}
