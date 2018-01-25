namespace ReportBuilderAjax.Web.HttpHandlers
{
    public enum ReportRequestStatus
	{
		Completed,
        Failed,
        TimedOut,
        Cancelled	
    }

    public enum RunningReportStatus
    {
        Pending,
        Uploading,
        Rendering,
        Cancelled,
    }
}
