using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReportBuilderAjax.Web
{
	public class DeliveredReportInfo
	{
		public DateTime ExecutionDate
		{
			get;
			set;
		}

		public string Filename
		{
			get;
			set;
		}

		public string Extension
		{
			get;
			set;
		}


        public string ScheduleID
		{
			get;
			set;
		}        
	}
}
