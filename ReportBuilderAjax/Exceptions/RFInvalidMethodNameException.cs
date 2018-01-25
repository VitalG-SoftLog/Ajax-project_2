using System;
using ReportBuilderAjax.Web.Common;

namespace ReportBuilderAjax.Web.Exceptions
{
    /// <summary>
    /// Исключение возникающее при обработке не верного имени метода (метод не был найден)
    /// </summary>
    public class RFInvalidMethodNameException : ArgumentException
    {
        public RFInvalidMethodNameException(string methodName)
            : base(string.Format(Const.METHOD_NAME_NOT_FOUNDED, methodName))
        {
        }
    }
}