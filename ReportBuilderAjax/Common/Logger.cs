using System;
using System.Reflection;
using System.Text;
using ReportBuilderAjax.Web.Services;

namespace ReportBuilderAjax.Web.Common
{
    public static class Logger
    {
        private static StringBuilder _exceptionsInfo = new StringBuilder();
       
        public static void Write(Exception exception, MethodBase methodBase, string message, string userId)
        {
            write(exception, methodBase, message, 0);
            try
            {
                ReportDomainService domainService = new ReportDomainService();
                domainService.InsertException(userId, _exceptionsInfo.ToString());
            }
            catch(Exception e)
            {
                //throw e;
            }
            _exceptionsInfo = new StringBuilder();
        }

        private static void write(Exception exception, MethodBase methodBase, string message, int tabCount)
        {
            try
            {
                string msg = getMsg(tabCount, message, methodBase, exception);
                _exceptionsInfo.Append(msg);
                
                if (exception != null)
                {
                    if (exception.InnerException != null)
                    {
                        tabCount++;
                        write(exception.InnerException, methodBase, exception.InnerException.Message, tabCount);
                    }
                }
            }
            catch
            {
                // we have no place to log the message to so we ignore it
            }
        }

        private static string getMsg(int tabCount, string message, MethodBase methodBase, Exception exception)
        {
            string tabString = "";
            for(int i = 0; i<tabCount; i++)
            {
                tabString += "\t";
            }
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("{0}[Message: {1}]", tabString, message);
            builder.AppendLine("   ");
            if (methodBase != null)
            {
                builder.AppendFormat("{0}[{1}: {2}.{3}]\r\n", tabString, Enum.GetName(typeof(MemberTypes), methodBase.MemberType), methodBase.DeclaringType, methodBase.Name);
            }
            if (exception != null && exception.Message != null && exception.StackTrace != null)
            {
                builder.AppendLine(string.Format("{0}[Error]\r\n{0}{1}\r\n{0}[Stack Trace]\r\n{2}",tabString, exception.Message, formatStackTrace(exception.StackTrace, tabString)));
            }

            builder.AppendLine(tabString + "-----------------------------------------------------------");
            builder.AppendLine();

            return builder.ToString();
        }

        private static string formatStackTrace(string stackTrace, string tabString)
        {
            string result = "";
            string replacedString = stackTrace.Replace("at ", "&");
            string[] splitResult = replacedString.Split(new[]{"&"}, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in splitResult)
            {
                if (!string.IsNullOrEmpty(s.Trim(' ')) && s != " ")
                {
                    result += string.Format("{0}at {1}", tabString, s.Trim(' '));
                }
            }
            return result;
        }
    }
}