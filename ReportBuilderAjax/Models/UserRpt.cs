using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;


namespace ReportBuilderAjax.Web
{
	public partial class UserRpt
	{
		[Key]
		[ReadOnly(true)]
		public int UserReportID	// UserReport.UserReportID
		{
			get;
			set;
		}

        public int ReportID // Report.ReportID
        {
            get; 
            set;
        }

		public string Name	// Report.ReportName
		{
			get;
			set;
		}

        public string UserReportName  // UserReport.UserReportDescription
		{
			get;
			set;
		}

		public DateTime CreatedDate  // UserReport.CreatedByDate
		{
			get;
			set;
		}

		public DateTime ModifiedDate  // UserReport.ModifiedByDate
		{
			get;
			set;
		}

		public bool IsCustom   // UserReport.IsCustom
		{
			get;
			set;
		}

        public string ReportLayoutStyleName   // ReportLayoutStyle.ReportLayoutStyleName
		{
			get;
			set;
		}
        
        public string FilterSystemType
        {
            get; 
            set;
        }

        public int ReportLayoutStyleID
        {
            get;
            set;
        }

		public FormatTypeEnum FormatType
		{
			get;
			set;
		}

        public bool IsExistsSchedules
        {
            get; 
            set;
        }

        
        public bool IsSummaryOnly
        {
            get; 
            set;
        }

        public bool IsCustomDateRange
        {
            get; 
            set;
        }

	    public bool IsTurnOffPageBreak
	    {
	        get; 
            set;
	    }

        public bool IncludeTitlePage
        {
            get;
            set;
        }
        public string CoverageCode
        {
            get;
            set;
        }
	}
}
