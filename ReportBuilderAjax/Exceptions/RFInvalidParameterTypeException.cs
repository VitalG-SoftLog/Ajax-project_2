using System;
using ReportBuilderAjax.Web.Common;

namespace ReportBuilderAjax.Web.Exceptions
{
    /// <summary>
    /// ����������, �����������, ���� �������� ��������� �� ������� �������� � ���������� ���� ������
    /// </summary>
    public class RFInvalidParameterTypeException: InvalidCastException
    {
        public RFInvalidParameterTypeException(Exception e)
            : base(Const.INVALID_PARAMETER_VALUE, e)
        {
        }
    }
}