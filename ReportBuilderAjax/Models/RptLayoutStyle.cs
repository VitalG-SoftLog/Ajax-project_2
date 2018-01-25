using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;


namespace ReportBuilderAjax.Web
{
	public partial class RptLayoutStyle
	{
		[Key]
		[ReadOnly(true)]
		public int ID
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

        public string PreviewImagePath
        {
            get;
            set;
        }

        public bool IsCustom
        {
            get; 
            set;
        }
	}
}
