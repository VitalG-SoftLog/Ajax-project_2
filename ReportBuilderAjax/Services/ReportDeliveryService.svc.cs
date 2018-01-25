using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.IO;
using System.Configuration;
using System.Text.RegularExpressions;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using ReportBuilderAjax.Web;
using ReportBuilderAjax.Web.Models;
using ReportBuilderAjax.Web.Services;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.Net.Mail;
using System.Text;
using System.Collections.Generic;


namespace ReportBuilderSL.Web.Services
{
    [MessageContract]
    public class ReportUploadRequest
    {
        [MessageHeader(MustUnderstand = true)]
        public DeliveredReportInfo DeliveredReportInfo;

        [MessageBodyMember(Order = 1)]
        public System.IO.Stream FileByteStream;
    }

    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class ReportDeliveryService
    {
        #region Config Settings
        string _FileServerPath = string.Empty;
        public string FileServerPath
        {
            get
            {
                if (string.IsNullOrEmpty(_FileServerPath))
                {
                    _FileServerPath = ConfigurationManager.AppSettings["FileServerPath"].ToString();
                }

                return _FileServerPath;
            }
        }

        public string FileServerUserID
        {
            get
            {
                return ConfigurationManager.AppSettings["FileServerUserID"];
            }
        }

        public string FileServerDomain
        {
            get
            {
                return ConfigurationManager.AppSettings["FileServerDomain"];
            }
        }

        public string FileServerPassword
        {
            get
            {
                return ConfigurationManager.AppSettings["FileServerPassword"];
            }
        }

        public bool EnableFileServerSecurity
        {
            get
            {
                return Convert.ToBoolean(ConfigurationManager.AppSettings["EnableFileServerSecurity"]);
            }
        }

        bool? _logExceptionsOnly;
        public bool LogExceptionsOnly
        {
            get
            {
                if (!_logExceptionsOnly.HasValue)
                {
                    if (ConfigurationManager.AppSettings["LogExceptionsOnly"] != null)
                    {
                        _logExceptionsOnly = Convert.ToBoolean(ConfigurationManager.AppSettings["LogExceptionsOnly"]);
                    }
                    else
                    {
                        // default to true
                        _logExceptionsOnly = true;
                    }
                }

                return _logExceptionsOnly.Value;
            }
        }

        private SmtpClient _SmtpClient;
        private SmtpClient SmtpClient
        {
            get
            {
                if (_SmtpClient == null)
                {
                    SmtpClient mailClient = new SmtpClient();
                    mailClient.Host = SmtpServer;
                    //mailClient.Credentials = new System.Net.NetworkCredential(SmtpUser, SmtpUserPassword);
                    _SmtpClient = mailClient;
                }
                return _SmtpClient;
            }
        }

        private bool IsEmailDeliveyEnabled
        {
            get
            {
                return Convert.ToBoolean(ConfigurationManager.AppSettings["IsEmailDeliveyEnabled"]);
            }
        }

        private string SmtpServer
        {
            get
            {
                return ConfigurationManager.AppSettings["SmtpServer"].ToString();
            }
        }

        private string SmtpUser
        {
            get
            {
                return ConfigurationManager.AppSettings["SmtpUserName"].ToString();
            }
        }

        private string SmtpUserPassword
        {
            get
            {
                return ConfigurationManager.AppSettings["SmtpUserPassword"].ToString();
            }
        }

        private string SmtpFromAddress
        {
            get
            {
                return ConfigurationManager.AppSettings["SmtpFromAddress"].ToString();
            }
        }

        private string ApplicationEnvironment
        {
            get
            {
                return ConfigurationManager.AppSettings["ApplicationEnvironment"].ToString();
            }
        }

        private bool IsProduction
        {
            get
            {
                return (ApplicationEnvironment.ToUpper() == "PRODUCTION");                
            }
        }

        private List<string> EnabledEmailAddresses
        {
            get
            {
                string enabledEmailAddresses = ConfigurationManager.AppSettings["EnabledEmailAddresses"].ToString().ToUpper();
                string[] addresses = enabledEmailAddresses.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                return addresses.ToList();
            }
        }
        #endregion

        #region Impersonation
        public const int LOGON32_LOGON_INTERACTIVE = 2;
        public const int LOGON32_PROVIDER_DEFAULT = 0;

        WindowsImpersonationContext _ImpersonationContext;

        [DllImport("advapi32.dll")]
        public static extern int LogonUserA(String lpszUserName,
            String lpszDomain,
            String lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            ref IntPtr phToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int DuplicateToken(IntPtr hToken,
            int impersonationLevel,
            ref IntPtr hNewToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool RevertToSelf();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool CloseHandle(IntPtr handle);

        public bool ImpersonateUser(String userName, String domain, String password)
        {
            WindowsIdentity tempWindowsIdentity;
            IntPtr token = IntPtr.Zero;
            IntPtr tokenDuplicate = IntPtr.Zero;

            if (RevertToSelf())
            {
                if (LogonUserA(userName, domain, password, LOGON32_LOGON_INTERACTIVE,
                    LOGON32_PROVIDER_DEFAULT, ref token) != 0)
                {
                    if (DuplicateToken(token, 2, ref tokenDuplicate) != 0)
                    {
                        tempWindowsIdentity = new WindowsIdentity(tokenDuplicate);
                        _ImpersonationContext = tempWindowsIdentity.Impersonate();
                        if (_ImpersonationContext != null)
                        {
                            CloseHandle(token);
                            CloseHandle(tokenDuplicate);
                            return true;
                        }
                    }
                }
            }
            if (token != IntPtr.Zero)
                CloseHandle(token);
            if (tokenDuplicate != IntPtr.Zero)
                CloseHandle(tokenDuplicate);
            return false;
        }

        public void UndoImpersonation()
        {
            if (_ImpersonationContext != null)
            {
                _ImpersonationContext.Undo();
            }
        }

        #endregion

        private int createUserReportOutputRecord(CcmsiUser user, UserReport userReport, DeliveredReportInfo info, string filename, ScheduleRecipient recipient)
        {          
            using (ReportProjectDBEntities entities = new ReportProjectDBEntities())
            {
                UserReportOutput rpt = new UserReportOutput()
                {
                    UserReport = entities.UserReport.Where(r => r.UserReportID == userReport.UserReportID).First(),
                    UserID = recipient.UserID,
                    FileName = filename,
                    ClientNumber = userReport.ClientNumber,
                    ReportRunDate = info.ExecutionDate,
                    CreatedDate = DateTime.Now,
                    CreatedByUserID = Convert.ToInt32(userReport.CreatedByUserID)
                };

                entities.AddToUserReportOutput(rpt);

                entities.SaveChanges();
                return rpt.UserReportOutputID;
            }
        }

        private void createReportDeliveryLogRecord(int scheduleID, int? scheduleRecipientID, int? userReportOutputID)
        {
            using (ReportProjectDBEntities entities = new ReportProjectDBEntities())
            {
                ReportDeliveryLog rdl = new ReportDeliveryLog()
                {
                    ScheduleID = scheduleID,
                    ScheduleRecipientID = scheduleRecipientID,
                    UserReportOutputID = userReportOutputID,
                    DeliveryDate = DateTime.Now
                };
                entities.AddToReportDeliveryLog(rdl);
                entities.SaveChanges();
            }
        }

        public void CreateReportEmptyNotSentDeliveryLog(Schedule schedule)
        {
            var filterService = new FilterService();
            foreach (ScheduleRecipient recipient in schedule.ScheduleRecipient.Where(sr => sr.IsActive).ToList())
            {
                createReportDeliveryLogRecord(recipient.ScheduleID, recipient.ScheduleRecipientID, null);
                
            }
            if (!String.IsNullOrEmpty(schedule.AdditionalEmailAddresses) && schedule.AdditionalEmailAddresses.Trim() != String.Empty)
            {
                createReportDeliveryLogRecord(schedule.ScheduleID, null, null);
            }  
        }

        public bool IsSendReport(Schedule schedule, UserReport userReport)
        {
            return (schedule.IsNotSendWithNoData && userReport.RowsCount > 0 || !schedule.IsNotSendWithNoData);
        }

        [OperationContract(Action = "UploadDeliveredReport", IsOneWay = true)]
        public void UploadDeliveredReport(ReportUploadRequest request)
        {
            //string currentUser = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

            MemoryStream sourceStream = null;
            DeliveredReportInfo info = request.DeliveredReportInfo;
            ReportProjectDBEntities entities = new ReportProjectDBEntities();
            bool success = true;

            try
            {
                // write the byte stream to a memory stream because it can be repositioned
                const int BUFFER_LENGTH = 4096;
                byte[] buffer = new byte[BUFFER_LENGTH];
                int byteCount = 0;
                sourceStream = new MemoryStream();
                while ((byteCount = request.FileByteStream.Read(buffer, 0, BUFFER_LENGTH)) > 0)
                {
                    sourceStream.Write(buffer, 0, byteCount);
                }

                int scheduleID = Convert.ToInt32(info.ScheduleID);
                Schedule schedule = (from s in entities.Schedule
                                         .Include("ScheduleRecipient")
                                     where s.ScheduleID == scheduleID
                                     orderby s.ScheduleID
                                     select s).FirstOrDefault();
                
                int userReportID = schedule.UserReportID;
                UserReport userReport = (from ur in entities.UserReport.Include("ReportLayoutStyle")
                                         where ur.UserReportID == userReportID
                                         select ur).FirstOrDefault();
                
                if (!LogExceptionsOnly)
                {
                    Logger.Write(string.Format("**** Processing Report Delivery. Client Number = {0}, User Report = {1}({2}), ScheduleID = {3}, RS File Name = {4} ****",
                        userReport.ClientNumber, userReport.UserReportName, userReport.UserReportID, scheduleID, info.Filename), "SLLog");
                }

                if (!IsSendReport(schedule, userReport))
                {
                    CreateReportEmptyNotSentDeliveryLog(schedule);
                }

                else
                {
                    Regex reg = new Regex("[^0-9a-zA-Z_]+?");
                    string fileName = reg.Replace(string.Format("{0}-{1}", userReport.UserReportName, DateTime.Now), "-");
                    fileName = string.Format("{0}.{1}", fileName, info.Extension);

                    bool isSensitive = false;

                    if (userReport.IsCustom)
                    {
                        List<UserReportField> userReportFields = (from urf in entities.UserReportField.Include("ReportField").Include("UserReport")
                                                                  where urf.UserReport.UserReportID == userReport.UserReportID
                                                                  select urf).ToList();

                        foreach (UserReportField userReportField in userReportFields)
                        {
                            if (userReportField.ReportField.IsSensitive)
                            {
                                isSensitive = true;
                                break;
                            }
                        }

                    }
                    else
                    {
                        if (userReport.ReportLayoutStyle.IsSensitive) isSensitive = true;
                    }




                    string subject = string.Empty;
                    var filterService = new FilterService();

                    var userId = filterService.GetCcmsiUser(Convert.ToInt32(userReport.CreatedByUserID));

                    if (isSensitive && filterService.GetUserHasSSNAccess(userId.UserID))
                    {
                        subject = string.Format("{0}{1}", "CCMSISecure: ", schedule.Subject);
                    }
                    else
                    {
                        subject = IsReportNeedSensitive(userReport.ReportID) ? string.Format("{0}{1}", "CCMSISecure: ", schedule.Subject) : schedule.Subject;
                    }

                    foreach (ScheduleRecipient recipient in schedule.ScheduleRecipient.Where(sr => sr.IsActive).ToList())
                    {
                        CcmsiUser user = filterService.GetCcmsiUser(recipient.UserID);

                        if (recipient.DeliveryMethodType == ScheduleRecipient.DeliveryMethod.Email)
                        {
                            // deliver the email to all recipients in prod, but only to the user who created the schedule in test
                            if (IsProduction || EnabledEmailAddresses.Contains(user.EmailAddress.Trim().ToUpper())) //recipient.UserID == schedule.CreatedByUserID)
                            {
                                string emailBody = GetEmailBody(schedule);
                                var ccmsiDomainPattern = new Regex(@"\w+([-+.]\w+)*@ccmsi.com", RegexOptions.Compiled);
                                DeliverViaEmail(ccmsiDomainPattern.IsMatch(user.EmailAddress) ? schedule.Subject : subject, emailBody, schedule.PriorityType, user.EmailAddress, info, fileName, sourceStream, recipient, scheduleID);

                                if (!LogExceptionsOnly)
                                {
                                    Logger.Write(string.Format("      Report emailed to user. UserID = {0}, User Name = {1}, Email Address = {2}",
                                        recipient.UserID, user.UserID, user.EmailAddress), "SLLog");
                                }
                            }
                        }
                        else
                        {
                            DeliverViaApplication(user, userReport, info, fileName, sourceStream, recipient);
                        }
                    }

                    // deliver to additional email recipients not in our database whose addresses were manually entered
                    if (IsProduction && !String.IsNullOrEmpty(schedule.AdditionalEmailAddresses) && schedule.AdditionalEmailAddresses.Trim() != String.Empty)
                    {
                        DeliverViaEmail(subject, schedule.Comment, schedule.PriorityType, schedule.AdditionalEmailAddresses.Trim(), info, fileName, sourceStream, null, scheduleID);
                        if (!LogExceptionsOnly)
                        {
                            Logger.Write(string.Format("      Report emailed to additonal recipients. Email Addresses = {0}", schedule.AdditionalEmailAddresses), "SLLog");
                        }
                    }
                }



            }
            catch (Exception ex)
            {
                success = false;
                Logger.Write(ex.ToString(), "SLLog");
                throw;
            }
            finally
            {
                UndoImpersonation();
                sourceStream.Close();

                if (!LogExceptionsOnly)
                {
                    Logger.Write(string.Format("**** Processing Report Delivery Completed. Success = {0}  ****", success), "SLLog");
                }

            }
        }

        [OperationContract]
        public bool DeleteDeliveredReport(string userId, UserRptOutput userReportOutput)
        {
            var file = Path.Combine(FileServerPath, userId, userReportOutput.FileName);

            try
            {
                if (File.Exists(file))
                    File.Delete(file);
            }
            catch 
            {
                return false;
            }

            try
            {
                ReportDomainService service = new ReportDomainService();
                service.DeleteDeliveredReport(userReportOutput);
            }
            catch 
            {
                return false;
            }

            return true;
        }

        private void DeliverViaApplication(CcmsiUser user, UserReport userReport, DeliveredReportInfo info, string fileName, MemoryStream sourceStream, ScheduleRecipient recipient)
        {
            try
            {
                if (EnableFileServerSecurity)
                {
                    if (!ImpersonateUser(FileServerUserID, FileServerDomain, FileServerPassword))
                    {
                        throw new ApplicationException("Failed to impersonate user with access to file share");
                    }
                }

                var recipientCcmsiUser = GetCcmsiUser(recipient.UserID);
                string fileFolder = Path.Combine(FileServerPath, recipientCcmsiUser.UserID);
                if (!Directory.Exists(fileFolder))
                {
                    Directory.CreateDirectory(fileFolder);
                }

                string filePath = Path.Combine(fileFolder, fileName);

                sourceStream.Position = 0;
                using (FileStream targetStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    //read from the input stream in 4K chunks and save to file stream
                    const int BUFFER_LENGTH = 4096;
                    byte[] buffer = new byte[BUFFER_LENGTH];
                    int byteCount = 0;
                    while ((byteCount = sourceStream.Read(buffer, 0, BUFFER_LENGTH)) > 0)
                    {
                        targetStream.Write(buffer, 0, byteCount);
                    }
                    targetStream.Close();
                }

                int userReportOutputId = createUserReportOutputRecord(user, userReport, info, fileName,  recipient);
                createReportDeliveryLogRecord(recipient.ScheduleID, recipient.ScheduleRecipientID, userReportOutputId);
                if (!LogExceptionsOnly)
                {
                    Logger.Write(string.Format("      Report delivered to file share. UserID = {0}, User Name = {1}, File Name = {2}",
                        user.ID, user.UserID, filePath), "SLLog");
                }
            }
            finally
            {
                UndoImpersonation();
            }
        }

        private void DeliverViaEmail(string subject, string body, Schedule.Priority priority, string toEmailAddresses, DeliveredReportInfo info, string fileName, MemoryStream reportStream, ScheduleRecipient recipient, int scheduleID)
        {
            // Build the email message
            MailMessage email = new MailMessage();
            email.IsBodyHtml = true;
            email.From = new MailAddress(SmtpFromAddress);
            email.Subject = subject;
            email.Body = body;
            switch (priority)
            {
                case Schedule.Priority.High:
                    email.Priority = MailPriority.High;
                    break;
                case Schedule.Priority.Low:
                    email.Priority = MailPriority.Low;
                    break;
                case Schedule.Priority.Normal:
                    email.Priority = MailPriority.Normal;
                    break;
            }

            reportStream.Position = 0;

            System.Net.Mime.ContentType contentType = new System.Net.Mime.ContentType(info.Extension.ToUpper() == "PDF" ? "application/pdf" : "application/vnd.ms-excel");
            contentType.Name = fileName;
            Attachment attachment = new Attachment(reportStream, contentType);                        
            email.Attachments.Add(attachment);

            string[] addresses = toEmailAddresses.Split(new string[]{";"}, StringSplitOptions.RemoveEmptyEntries);
            foreach (string address in addresses)
            {
                email.To.Add(address);
            }

            //// prepend security string
            //email.Subject = "[" + Environment.MachineName + "] " + email.Subject;

            if (IsEmailDeliveyEnabled)
            {
                SmtpClient.Send(email);
            }

            if (recipient != null)
            {
                createReportDeliveryLogRecord(scheduleID, recipient.ScheduleRecipientID, null);
            }
            else
            {
                createReportDeliveryLogRecord(scheduleID, null, null);
            }
        }

        private string GetEmailBody(Schedule schedule)
        {
            // for normal production use the email body is simply the comment the user entered for this schedule
            //  but when running locally or testing we will only send the email to the user who created the schedule and
            //  we will list in the body of the email the other users who would have received the email
            string emailBody = schedule.Comment;

            if (!IsProduction)
            {
                // get all the email recipients for this schedule
                var userIDs = schedule.ScheduleRecipient.Where(s => s.DeliveryMethodType == ScheduleRecipient.DeliveryMethod.Email).Select(s => s.UserID);
                FilterService filterService = new FilterService();
                List<CcmsiUser> emailRecipientList = filterService.GetCcmsiUsers(userIDs.ToList());

                // list the recipients in the email body
                StringBuilder sb = new StringBuilder();
                sb.Append("<BR/><BR/>Application Environment: ");
                sb.Append(ApplicationEnvironment);
                sb.Append("<BR/><BR/>In production each of the following system users would receive this email:");
                foreach (CcmsiUser emailRecipient in emailRecipientList)
                {
                    sb.Append("<BR/>");
                    sb.Append(string.Format("Name: {0} | UserID: {1} | ID: {2} | Email: {3}", emailRecipient.FullName, emailRecipient.UserID, emailRecipient.ID, emailRecipient.EmailAddress.Trim()));
                }
                sb.Append("<BR/><BR/>");
                sb.Append("In addition, this email would be sent to these email addresses: <BR/>");
                sb.Append(schedule.AdditionalEmailAddresses);
                emailBody += sb.ToString();
            }

            return emailBody;
        }

        private bool IsReportNeedSensitive(int reportID)
        {
            return reportID == (int) ReportEnum.ManagedCareSavingsAndFeesDetail;
        }

        private CcmsiUser GetCcmsiUser(int userID)
        {
            // delegate to the filter service which is set up to execute SPs against the Store1 DB
            FilterService filterService = new FilterService();
            return filterService.GetCcmsiUser(userID);
        }
    }
}
