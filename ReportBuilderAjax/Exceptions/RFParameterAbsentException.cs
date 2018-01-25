using ReportBuilderAjax.Web.Common;

namespace ReportBuilderAjax.Web.Exceptions
{
    /// <summary>
    /// ����������, ����������� � ������, ����� �� ��� ��������� ���� �� ����������� ����������� ������
    /// </summary>
    public class RFParameterAbsentException : RFArgumentAbsentException
    {
        public RFParameterAbsentException(string argument)
            : base(string.Format(Const.PARAMETER_NOT_FOUNDED, argument))
        {
        }
    }
}