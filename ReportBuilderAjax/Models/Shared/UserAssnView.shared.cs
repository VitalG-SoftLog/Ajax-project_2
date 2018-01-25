using System;
using System.Collections.Generic;
using System.Linq;

namespace ReportBuilderAjax.Web
{
    public partial class UserAssnView
    {

        public string AssnDisplay
        {
            get
            {
                return string.Format("{0} ({1})", this.AssnLongName, this.AssnNum);
            }
        }

        //        public string DisplayName
        //        {
        //            get
        //            {
        //                if (!string.IsNullOrEmpty(this.FriendlyName))
        //                {
        //                    return this.FriendlyName;
        //                }
        //                else
        //                {
        //                    return this.Name;
        //                }
        //            }
        //        }
    }
}
