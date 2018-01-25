using ReportBuilderAjax.Web.Common;

namespace ReportBuilderAjax.Web.Exceptions
{
    /// <summary>
    /// Исключение, возникающее в случае, когда имя action не указано
    /// </summary>
    public class RFActionNameAbsentException : RFArgumentAbsentException
    {
        public RFActionNameAbsentException()
            : base(Const.METHOD_NAME_ABSENT)
        {
        } 
    }
}