﻿namespace ReportBuilderAjax.Web
{
	/// <summary>
	///     Partial class extending the User type that adds shared properties and methods
	///     that will be available both to the server app and the client app
	/// </summary>
	public partial class User
	{
		/// <summary>
		///     Returns the user display name, which by default is its Friendly Name,
		///     and if that is not set, its User Name
		/// </summary>
//#if !SILVERLIGHT
//        [System.Web.Ria.ApplicationServices.ProfileUsage(IsExcluded = true)]
//#endif
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
