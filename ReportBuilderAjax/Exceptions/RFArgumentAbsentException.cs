using System;

namespace ReportBuilderAjax.Web.Exceptions
{
    /// <summary>
    /// Базовый класс для исключений при обработке не правильных аргументов
    /// </summary>
    public class RFArgumentAbsentException : ArgumentException
    {
        public RFArgumentAbsentException(string message): base(message)
        {
        }
    }
}