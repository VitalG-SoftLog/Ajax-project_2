using System;
using ReportBuilderAjax.Web.Common;

namespace ReportBuilderAjax.Web.Exceptions
{
    /// <summary>
    /// ����������, ����������� ��� ��������� ������, ������� �� �������� ��������� ����� AJAX ������
    /// </summary>
    public class RFNotAjaxAccessibleException : ArgumentException
    {
        public RFNotAjaxAccessibleException(string methodName)
            : base(string.Format(Const.METHOD_IS_NOT_AJAX_ACCESSIBLE, methodName))
        {
        }
    }
}