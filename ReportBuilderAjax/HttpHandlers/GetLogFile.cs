using System.Data;
using System.Text;
using System.Web;

namespace ReportBuilderAjax.Web.HttpHandlers
{
    public class GetLogFile : IHttpHandler
    {
        private HttpResponse Response;

        private HttpRequest Request;

        public void ProcessRequest(HttpContext context)
        {
            initilizing(context);
            writeFile();
        }

        private void initilizing(HttpContext context)
        {
            Response = context.Response;
            Request = context.Request;
        }

        private void writeFile()
        {
            string startDateString = Request.QueryString["startDate"];
            string endDateString = Request.QueryString["endDate"];

            /*DataTable errorsTable = DataAccess.DataProvider.GetLog(startDateString, endDateString);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (DataRow row in errorsTable.Rows)
            {
                stringBuilder.AppendLine(row["LogDate"].ToString());
                stringBuilder.AppendLine(row["LogMessage"].ToString());
            }
            
            Response.ContentType = "application/octet-stream";
            Response.AddHeader("content-disposition", "attachment; filename=log.txt");
            Response.Write(stringBuilder.ToString());*/
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}