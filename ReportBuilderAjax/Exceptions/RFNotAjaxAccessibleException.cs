using System;
using ReportBuilderAjax.Web.Common;

namespace ReportBuilderAjax.Web.Exceptions
{
    /// <summary>
    /// »сключение, возникающее при обработке метода, который не €вл€етс€ доступным через AJAX запрос
    /// </summary>
    public class RFNotAjaxAccessibleException : ArgumentException
    {
        public RFNotAjaxAccessibleException(string methodName)
            : base(string.Format(Const.METHOD_IS_NOT_AJAX_ACCESSIBLE, methodName))
        {
        }
    }
}