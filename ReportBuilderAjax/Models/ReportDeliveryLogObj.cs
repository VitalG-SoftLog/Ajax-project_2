using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ReportBuilderAjax.Web
{
    public class ReportDeliveryScheduleObj
    {
        public string DeliveryMethods { get; set; }

        public string Recipients { get; set; }

        public string ScheduleName { get; set; }

        public string Period { get; set; }

        public string ScheduleStart { get; set; }

        public string ScheduleStop { get; set; }

        public string TimeOfDay { get; set; }
    }
	
    public class ReportDeliveryLogObj
    {
        [Key]
        [ReadOnly(true)]
        public int ReportDeliveryLogID { get; set; }

        public string DeliveryDate { get; set; }

        public string ReportName { get; set; }

        public string ReportType { get; set; }

        public string DeliveryMethods { get; set; }

        public string Recipient { get; set; }

        public string ReportLink { get; set; }

        public List<ReportDeliveryScheduleObj> Schedules { get; set; }

        public int TotalCount { get; set; }
    }
}