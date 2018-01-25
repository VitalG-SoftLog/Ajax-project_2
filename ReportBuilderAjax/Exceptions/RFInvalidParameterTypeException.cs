using System;
using ReportBuilderAjax.Web.Common;

namespace ReportBuilderAjax.Web.Exceptions
{
    /// <summary>
    /// »сключение, возникающее, если значение параметра не удалось привести к требуемому типу данных
    /// </summary>
    public class RFInvalidParameterTypeException: InvalidCastException
    {
        public RFInvalidParameterTypeException(Exception e)
            : base(Const.INVALID_PARAMETER_VALUE, e)
        {
        }
    }
}