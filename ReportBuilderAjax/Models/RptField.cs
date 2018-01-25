using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel; 
    

namespace ReportBuilderAjax.Web
{
	public partial class RptField
	{
        //[Key]
        //[ReadOnly(true)]
        public int RptFieldID
        {
            get;
            set;
        }

        public int RptFieldInfoID
        {
            get;
            set;
        }

        [Key]
        [ReadOnly(true)]
		public int ID
		{
			get;
			set;
		}

		public int ReportFieldID
		{
			get;
			set;
		}

		public int ReportID
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

        public string HideName
        {
            get;
            set;
        }

		public string SQLName
		{
			get;
			set;
		}

		public DataTypeEnum DataType
		{
			get;
			set;
		}

		public int ColumnOrder
		{
			get;
			set;
		}

        public double ColumnWidthFactor
		{
			get;
			set;
		}

        public int SortOrder
        {
            get;
            set;
        }

        public SortDirectionEnum SortDirection
        {
            get;
            set;
        }

        public int GroupOrder
        {
            get;
            set;
        }

        public string GroupSummaryExpression
		{
			get;
			set;
		}

		public bool IsGroupable
		{
			get;
			set;
		}

        public bool IncludePageBreak
        {
            get;
            set;
        }

        public bool IsGroupByDefault
        {
            get;
            set;
        }

        public bool IsDisplayInReport
        {
            get;
            set;
        }

        public bool IsSummarizable
        {
            get;
            set;
        }

        public bool IsUsed
        {
            get;
            set;
        }

        public bool CanDrag
        {
            get;
            set;
        }

        public bool IsVisible
        {
            get;
            set;
        }

        public string Tag
        {
            get;
            set;
        } 

        public string Category
        {
            get;
            set;
        }

        public string FieldValueExpression
        {
            get;
            set;
        }

        public bool IsCalculated
        {
            get;
            set;
        }

        public string CalculationDescription
        {
            get;
            set;
        }
        public bool UseSummaryExpresionGroup
        {
            get;
            set;
        }
        public string CoverageCode
        {
            get;
            set;
        }
        public bool IsClientSpecificCode
        {
            get;
            set;
        }

	    public string DefaultName
	    {
	        get; 
            set;
	    }
	}                
}
