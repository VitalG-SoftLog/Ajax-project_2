using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ReportBuilderAjax.Web.Models
{
    public class UserRptOutput
    {
        [Key]
        [ReadOnly(true)]
        public int ID
        {
            get;
            set;
        }

        public int ParentId
        {
            get; 
            set;
        }

        public string FileName
        { 
            get; 
            set;
        }

        public DateTime CreatedDate
        { 
            get; 
            set;
        }

        public string Description
        { 
            get; 
            set;
        }
    }
}