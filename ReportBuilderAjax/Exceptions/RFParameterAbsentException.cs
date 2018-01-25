using ReportBuilderAjax.Web.Common;

namespace ReportBuilderAjax.Web.Exceptions
{
    /// <summary>
    /// Исключение, возникающее в случае, когда не был определен один из параметеров вызоваемого метода
    /// </summary>
    public class RFParameterAbsentException : RFArgumentAbsentException
    {
        public RFParameterAbsentException(string argument)
            : base(string.Format(Const.PARAMETER_NOT_FOUNDED, argument))
        {
        }
    }
}