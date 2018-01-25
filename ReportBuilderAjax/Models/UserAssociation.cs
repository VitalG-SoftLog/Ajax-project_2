using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ReportBuilderAjax.Web
{
    public class UserAssociation
    {
        [Key]
        [ReadOnly(true)]
        public string AssnNum { get; set; }
        public string AssnName { get; set; }
        public string AssnLongName { get; set; }
        public bool IsStandAlone { get; set; }
        public bool IsAssociation { get; set; }
        public string AssnDisplay
        {
            get
            {
                return string.Format("{0} ({1})", this.AssnLongName, this.AssnNum);
            }
        }

    }
}