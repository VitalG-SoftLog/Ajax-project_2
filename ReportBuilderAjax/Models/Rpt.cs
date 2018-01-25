using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;


namespace ReportBuilderAjax.Web
{
	public partial class Rpt
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

		public string Description
		{
			get;
			set;
		}

        public string FilterSystemType
        {
            get; 
            set;
        }

        public int ReportFolderID
        {
            get; 
            set;
        }

	    public bool IsUseWithCheckpoint
	    {
	        get; 
            set;
	    }
	}
}
