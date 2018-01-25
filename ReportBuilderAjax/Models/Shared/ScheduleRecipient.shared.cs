using System;
namespace ReportBuilderAjax.Web
{
    public partial class ScheduleRecipient
    {
        #region Enums

        public enum DeliveryMethod
        {
            Email = 1,
            Application = 2
        }
        
        #endregion

        public DeliveryMethod DeliveryMethodType
        {
            get { return (DeliveryMethod)this.DeliveryMethodTypeID; }
        }
    }
}