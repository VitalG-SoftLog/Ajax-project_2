using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using ReportBuilderSL.Web.Services;

namespace ReportBuilderSL.Web
{
    public partial class DownloadFile : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                //string fileName = @"E:\Source\ReportingProject\ReportBuilderSL4\ReportBuilderSL.Web\Reports\69cc6261-94c5-4e87-b50c-f1b3e4756191-Transaction Register.pdf";

                //
                // Unpack query string parameters 
                //

                // for files stored on on the file share
                string folder = Request.QueryString["Folder"];
                string fileName = Request.QueryString["File"];

                // for files stored temporarily in the Reports folder
                string relativeFilePath = Request.QueryString["RelativeFilePath"];

                ReportDeliveryService rdService = null;
                FileInfo file = null;
                bool isImpersonating = false;
                if (relativeFilePath != null)
                {
                    // download request is for a transient report that was just run and is sitting in the web app's "Reports" folder 
                    //Logger.Write("Path of file being downloaded: " + Request.PhysicalApplicationPath + " | " + relativeFilePath, "SLLog");
                    file = new FileInfo(Path.Combine(Request.PhysicalApplicationPath, relativeFilePath));
                }
                else
                {
                    // download request is for a saved report stored on the file share

                    //
                    //  Impersonate user with access to file share
                    //
                    //string currentUser = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    rdService = new ReportDeliveryService();
                    string fileShare = rdService.FileServerPath;

                    if (rdService.EnableFileServerSecurity)
                    {
                        if (!rdService.ImpersonateUser(rdService.FileServerUserID, rdService.FileServerDomain, rdService.FileServerPassword))
                        {
                            DisplayErrorMessage("Impersonation failed");
                            return;
                        }
                        isImpersonating = true;
                    }

                    file = new FileInfo(Path.Combine(fileShare, folder, fileName));
                }

                //
                // Verify file exists
                //
                if (!file.Exists)
                {
                    Logger.Write("File does not exist", "SLLog");
                    DisplayErrorMessage("File does not exist");
                    return;
                }

                //
                // Stream the file to the browser as the response 
                //
                Response.ClearHeaders();

                //immediately prompt the user with a file download box and use the provided filename as the suggested name
                Response.AddHeader("content-disposition", string.Format("attachment; filename={0}", file.Name));

                Response.ClearContent();
                //Response.ContentEncoding = System.Text.Encoding.UTF8;
                Response.ContentType = GetContentType(file.Extension);
                Response.TransmitFile(file.FullName);
                Response.Flush();
                //Response.Close(); // this caused some files to be truncated

                if (isImpersonating)
                {
                    rdService.UndoImpersonation();
                }
            }
            catch (Exception ex)
            {
                //Logger.Write("Error downloading report: " + ex.ToString(), "SLLog");
                DisplayErrorMessage(ex.Message);
            }

            Response.End();  // this throws a thread abort exception (by design), so don't put this inside of the try/catch. See http://social.msdn.microsoft.com/Forums/en-US/vsdebug/thread/817776da-13da-43f5-a189-1727ce4f3b6b/
        }

        private void DisplayErrorMessage(string errMessage)
        {
            string startupScriptKey = "PopupErrorMessage";
            Type theType = this.GetType();
            ClientScriptManager csm = Page.ClientScript;

            if (!csm.IsStartupScriptRegistered(theType, startupScriptKey))
            {
                const string message = "There was an error downloading the requested file.  If you continue to receive this error message, please contact your system administrator.";
                //string debugMessage = message + " | " + errMessage;

                //errMessage = message + " | " + errMessage; 
                System.Text.StringBuilder builder = new System.Text.StringBuilder();
                builder.Append("<script type=text/javascript>");
                //builder.AppendLine("alert('" + debugMessage + "');");
                builder.AppendLine("alert('" + message + "');");
                builder.AppendLine("self.close();");
                builder.AppendLine("</script>");

                csm.RegisterStartupScript(theType, startupScriptKey, builder.ToString());
            }

        }

        #region GetContentType
        public string GetContentType(string strextension)
        {
            string contentType;
            switch (strextension.ToLower())
            {
                case ".gif":
                    contentType = "image/gif";
                    break;
                case ".jpg":
                case ".jpeg":
                    contentType = "image/jpeg";
                    break;
                case ".png":
                    contentType = "image/png";
                    break;
                case ".doc":
                    contentType = "application/ms-word";
                    break;
                case ".docx":
                    contentType = "application/vnd.ms-word.document.12";
                    break;
                case ".pdf":
                    contentType = "application/pdf";
                    break;
                case ".xls":
                    contentType = "application/vnd.ms-excel";
                    break;
                case ".ppt":
                    contentType = "application/vnd.ms-powerpoint";
                    break;
                case ".zip":
                    contentType = "application/zip";
                    break;
                case ".txt":
                    contentType = "text/plain";
                    break;
                default:
                    contentType = "application/octet-stream";
                    break;
            }
            return contentType;
        }
        #endregion
    }
}