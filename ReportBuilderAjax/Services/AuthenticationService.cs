using System;
using ReportBuilderAjax.Web.Common;

namespace ReportBuilderAjax.Web
{
    using System.Web.Security;
	using System.Collections.Generic;
	using System.Linq;
    using System.ServiceModel.DomainServices.Hosting;
    using System.ServiceModel.DomainServices.EntityFramework;
    using System.ServiceModel.DomainServices.Server.ApplicationServices;
    using System.Configuration;
    using System.Data;

	/// <summary>
	///    RIA Services DomainService responsible for authenticating users when
	///    they try to log on to the application.
	///    
	///    Most of the functionality is already provided by the base class
	///    AuthenticationBase
	/// </summary>
	[EnableClientAccess]
	public class AuthenticationService : LinqToEntitiesDomainService<MsiclaimEntities> , IAuthentication<User>
	{
		private static User DefaultUser = new User() {
			ID = -1,
			Name = string.Empty,
			Roles = new List<string>(),
			UserID = string.Empty,
            Password = string.Empty,
			AssociationNumber = string.Empty,
			AssociationName = string.Empty,
            EmailAddress = string.Empty,
            ReportFileStorage = ConfigurationManager.AppSettings["ReportFileStorage"],
            UserAuthorizationCheckTicks = ConfigurationManager.AppSettings["UserAuthorizationCheckTicks"] != null ? int.Parse(ConfigurationManager.AppSettings["UserAuthorizationCheckTicks"].ToString()) : 300000,
            IsAdmin = false,
            CheckpointIds = new List<string>(),
            LogoFileStorage = "",
            LogoFileHandler ="",
            UserAssociationList = new List<string>()
		};

		#region IAuthentication<User> Members
        
        private User GetUser(string userName)
        {
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_USER_SELECT, CommandType.StoredProcedure,
                                            new object[] { 
                                                Consts.SP_PARAM_USER_ID, userName
                                            });

            User user = null;
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count > 0)
            {
                DataRow row = dataTable.Rows[0];
                user = new User();
                user.ID = (int)row["ID"];
                user.Name = string.Format("{0} {1}", row["FirstName"].ToString().Trim(), row["LastName"].ToString().Trim());
                user.UserID = row["UserID"].ToString().Trim();
                user.AssociationNumber = row["AssociationNumber"].ToString().Trim();
                user.AssociationName = row["AssociationName"].ToString().Trim();
                user.EmailAddress = row["EmailAddress"].ToString().Trim();
                user.UserAuthorizationCheckTicks = ConfigurationManager.AppSettings["UserAuthorizationCheckTicks"] !=
                                                   null
                                                       ? int.Parse(
                                                           ConfigurationManager.AppSettings[
                                                               "UserAuthorizationCheckTicks"].ToString())
                                                       : 300000;
                user.ReportFileStorage = ConfigurationManager.AppSettings["ReportFileStorage"] ?? "";
                user.IsAdmin = isUserAdmin(user.UserID);
                user.LogoFileStorage = ConfigurationManager.AppSettings["LogoImageURL"] ?? "";
            }
            
            if (user != null)
            {
                string environment = ConfigurationManager.AppSettings["ApplicationEnvironment"];
                if (environment.ToUpper() == "SYSTEM TEST" || environment.ToUpper() == "LOCAL")
                {
                    // since we do not login via iCE, get the last association the user logged in with 
                    //  from the userlogin table from Store1 rather than using the one from above that came
                    //  from our authentication database (that is, our ice database)
                    dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
                    dataTable = dbHelper.GetDataTable(Consts.SP_NAME_ASSOCIATION_LATEST_LOGIN_SELECT, CommandType.StoredProcedure,
                                                    new object[] { 
                                                Consts.SP_PARAM_USER_ID, userName
                                            });

                    if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count > 0)
                    {
                        DataRow row = dataTable.Rows[0];
                        user.AssociationNumber = row["AssociationNumber"].ToString().Trim();
                        user.AssociationName = row["AssociationName"].ToString().Trim();
                    }
                }

                SetupUserAuthorization(user.UserID, user.AssociationNumber);
            }
            
            return user;
        }

        private User AuthenticateUser(string userName, string password, string assnNum, System.Guid loginGuid)
        {
            DBHelper dbHelper;
            DataTable dataTable;

            //if we do not login via iCE, get the last association the user logged in with from the userlogin table from Store1
            if (string.IsNullOrEmpty(assnNum))
            {
                dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
                dataTable = dbHelper.GetDataTable(Consts.SP_NAME_ASSOCIATION_LATEST_LOGIN_SELECT, CommandType.StoredProcedure,
                                                new object[] { 
                                                Consts.SP_PARAM_USER_ID, userName
                                            });

                if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count > 0)
                {
                    DataRow row = dataTable.Rows[0];                    
                    assnNum = row["AssociationNumber"].ToString().Trim();
                }
            }


            dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["AuthenticationConnection"].ConnectionString);
            dataTable = dbHelper.GetDataTable(Consts.SP_NAME_USER_AUTHENTICATE, CommandType.StoredProcedure,
                                            new object[] { 
                                                Consts.SP_PARAM_USER_ID, userName,
                                                Consts.SP_PARAM_PASSWORD, password,
                                                Consts.SP_PARAM_ASSN_NUM, assnNum == null ? DBNull.Value : (object)assnNum,
                                                Consts.SP_PARAM_LOGIN_GUID, (loginGuid == System.Guid.Empty) ? string.Empty : loginGuid.ToString() 
                                            });


            User user = null;
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count > 0)
            {
                DataRow row = dataTable.Rows[0];
                user = new User();
                user.ID = (int)row["ID"];
                user.Name = string.Format("{0} {1}", row["FirstName"].ToString().Trim(), row["LastName"].ToString().Trim());
                user.UserID = row["UserID"].ToString().Trim();
                //user.Password = row["Password"].ToString().Trim();  //why would we put this in the user object?
                user.AssociationNumber = row["AssociationNumber"].ToString().Trim();
                user.AssociationName = row["AssociationName"].ToString().Trim();
                user.EmailAddress = row["EmailAddress"].ToString().Trim();
                user.UserAuthorizationCheckTicks = ConfigurationManager.AppSettings["UserAuthorizationCheckTicks"] !=
                                                   null
                                                       ? int.Parse(
                                                           ConfigurationManager.AppSettings[
                                                               "UserAuthorizationCheckTicks"].ToString())
                                                       : 300000;
                user.ReportFileStorage = ConfigurationManager.AppSettings["ReportFileStorage"] ?? "";
                user.IsAdmin = isUserAdmin(user.UserID);
                user.LogoFileStorage = ConfigurationManager.AppSettings["LogoImageURL"] ?? "";
            }

            return user;
        }

        public void SetupUserAuthorization(string userName, string assnNum)
        {
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            dbHelper.ExecuteNonQuery(Consts.SP_NAME_USER_CONFIGURE_AUTHORIZATION, CommandType.StoredProcedure,
                                            new object[] { 
                                                Consts.SP_PARAM_USER_ID, userName,
                                                Consts.SP_PARAM_ASSN_NUM, assnNum
                                            });
        }

        public User GetUser()
		{
			if ((this.ServiceContext != null) &&
				(this.ServiceContext.User != null) &&
				this.ServiceContext.User.Identity.IsAuthenticated)
			{
				return this.GetUser(this.ServiceContext.User.Identity.Name);
			}
			return AuthenticationService.DefaultUser;
		}
        
        public User Login(string userName, string password, bool isPersistent, string customData)
        {
            System.Guid loginGuid = System.Guid.Empty;
            string assnNum = string.Empty;

            // if user name is provided, then expect a password also and customdata will be the association number;
            //  otherwise customdata will contain a GUID that can be used to look up the user name and password
            //  in the SecureDataTransfer table in MSIclaim.  This means the user is already authenticated in another
            //  app (iCE) and we don't want them to have to log in again.
            if (string.IsNullOrEmpty(userName))
            {
                loginGuid = new System.Guid(customData);
            }
            else
            {
                assnNum = customData;
            }

            // authenticate against SQLPROD.MSIClaim which is the system of record for user data
            User user = AuthenticateUser(userName, password, assnNum, loginGuid);

            if (user != null)
            {
                // configure the UserAccessDetail table in the Store1 database with the user's access
                //  rights for this association  
                SetupUserAuthorization(user.UserID, user.AssociationNumber);
                FormsAuthentication.SetAuthCookie(user.UserID, isPersistent);
                return user;
            }
            
            //if (GetUser() != AuthenticationService.DefaultUser)
            //{
            //    return GetUser();
            //}

            return null;
        }

        public User Logout()
		{
			FormsAuthentication.SignOut();
			return AuthenticationService.DefaultUser;
		}
        
        public void UpdateUser(User user)
        {
            User loggedUser = GetUser();
            if (user.ID != -1)
            {
                SetupUserAuthorization(loggedUser.UserID, user.AssociationNumber);
            }
        }

		#endregion

        public IQueryable<UserAssnView> GetUserAssnList()
        {
            string userid = this.ServiceContext.User.Identity.Name;

            var assns = this.ObjectContext.GetUserAssn(userid);

            return assns.AsQueryable();
        }

        public List<UserAssociation> GetUserAssociations(string userId)
        {
            //var userId = this.ServiceContext.User.Identity.Name;
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["Store1Connection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_USER_ASSOCIATION_SELECT,
                                                        CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_USER_ID, userId
                                                            });

            List<UserAssociation> assnList = new List<UserAssociation>();
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count > 0)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    UserAssociation ua = new UserAssociation();
                    ua.AssnNum = row["AssnNum"].ToString().Trim();
                    ua.AssnName = row["AssnName"].ToString().Trim();
                    ua.IsStandAlone = row["IsStandalone"].ToString().Trim() == "Y" ? true : false;
                    ua.IsAssociation = row["IsAssociation"].ToString().Trim() == "Y" ? true : false;
                    ua.AssnLongName = row["AssnLongName"].ToString().Trim();
                    assnList.Add(ua);
                }
            }
            return assnList;
        }

        private bool isUserAdmin(string userName)
        {
            ReportDomainService domainService = new ReportDomainService();
            return domainService.GetAdminUserName(userName).ToList().Count == 1;
        }

        public User GetUserCheckpoints(string userId)
        {
            List<string> list = new List<string>();
            DBHelper dbHelper = new DBHelper(ConfigurationManager.ConnectionStrings["AuthenticationConnection"].ConnectionString);
            DataTable dataTable = dbHelper.GetDataTable(Consts.SP_NAME_GET_USER_CHECKPOINTS,
                                                        CommandType.StoredProcedure,
                                                        new object[]
                                                            {
                                                                Consts.SP_PARAM_USER_ID, userId
                                                            });

            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count > 0)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    list.Add(row["CheckpointID"].ToString().Trim());
                }
            }
            return new User{CheckpointIds =  list};
        }     
	}
}
