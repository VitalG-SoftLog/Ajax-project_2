using System;
using System.IO;

namespace ReportBuilderAjax.Web
{
    public partial class GetFile : System.Web.UI.Page
    {
        private string FilePath
        {
            get
            {
                return System.Web.Hosting.HostingEnvironment.MapPath("~/Reports/");
            }
        }
        
        private string fileName
        {
            get { return Request.QueryString["fileName"]; }
        }
        
        protected void Page_Load(object sender, EventArgs e)
        {
            if (File.Exists(Path.Combine(FilePath, fileName)))
            {
                Response.AddHeader("content-disposition", string.Format("attachment;filename={0}", fileName));
                Response.ContentType = "application/octet-stream";
                Response.WriteFile(Path.Combine(FilePath, fileName));
                Response.End();
            }
        }
    }
}