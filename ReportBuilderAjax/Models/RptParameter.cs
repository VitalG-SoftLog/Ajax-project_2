using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ReportBuilderAjax.Web
{
	public partial class RptParameter
	{
		[Key]
		[ReadOnly(true)]
		public int ID
		{
			get;
			set;
		}

		public int ReportID
		{
			get;
			set;
		}

        public int UserReportParameterID
        {
            get;
            set;
        }

		public string Name
		{
			get;
			set;
		}

		public string Description
		{
			get;
			set;
		}

		public DataTypeEnum DataType
		{
			get;
			set;
		}

		public string DefaultValue
		{
			get;
			set;
		}

        public string FilterString
        {
            get; 
            set;
        }

        public bool IsReportParameter
        {
            get; 
            set;
        }

        public bool IsQueryParameter
        {
            get;
            set;
        }

        public int OrderIndex
        {
            get; 
            set;
        }	   
	}
}
