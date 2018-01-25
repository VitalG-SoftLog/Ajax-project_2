using System;
using ReportBuilderAjax.Web.Common;

namespace ReportBuilderAjax.Web.Exceptions
{
    /// <summary>
    /// ���������� ����������� ��� ��������� �� ������� ����� ������ (����� �� ��� ������)
    /// </summary>
    public class RFInvalidMethodNameException : ArgumentException
    {
        public RFInvalidMethodNameException(string methodName)
            : base(string.Format(Const.METHOD_NAME_NOT_FOUNDED, methodName))
        {
        }
    }
}