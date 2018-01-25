namespace ReportBuilderAjax.Web.Common
{ 
    public partial class Const
    {
/*
        public const int SEARCH_ROW_PER_PAGE = 10;

        #region Parameter Names
*/
        public const string RP_PARAMS_ARRAY = "params";
		
/*
        public const string RP_PARAMS_SAVED_FILE_NAME = "savedFileName";

        public const string RP_PARAMS_ORIGINAL_FILE_NAME = "originalFileName";

        public const string RP_PARAMS_IS_SUBMIT = "isSubmit";
		
        #endregion
*/
        #region Exception Messages

        public const string METHOD_NAME_ABSENT = "Method name is not specified!";
        public const string METHOD_NAME_NOT_FOUNDED = "Method \"{0}\" is not founded!";
        public const string PARAMETER_NOT_FOUNDED = "Parameter \"{0}\" is not specified!";
        public const string METHOD_IS_NOT_AJAX_ACCESSIBLE = "Method \"{0}\" is not ajax accessible!";
        public const string INVALID_PARAMETER_VALUE = "Parameter value in not valid type!";
        public const string ERRORS = "Errors";
        public const string ERROR_FORMAT = "{0}</br>";
      
        #endregion

        #region Misc
/*
        public const string EFFECTIVE_DATE = "EffectiveDate";
*/     
        #endregion
/*
        #region Intagartion

        public const string PARAM_DATABASE_SERVER = "databaseserver";
        public const string PARAM_QUOTE_ID = "quoteId";
*/
        public const string PARAM_LOGIN_PAGE = "Login.aspx";

        public const string PARAM_GUID = "guid";
/*
        public const string PARAM_OCCURRENCE_ID = "occurrenceid";
        public const string PARAM_EXPIRED_DATE = "expiredDate";
*/
        public const string AUTH_COOKIE_NAME = "authRBCookie";

        public const string LOGO_IMAGE_URL = "LogoImageURL";
/*
        public const string PATTERN_ACCIDENTRPT = "AccidentRpt";
        public const string PATTERN_ACCIDENTRPT_GATEKEEPER = "AccidentRptGateKeepr";
        public const string PATTERN_INITIAL_REPORT_FORM = "InitialReportForm";
        public const string PATTERN_CLAIMS_ANALYSIS_SCREEN = "ClaimsAnalysisScreen";
        public const string PATTERN_IS_EXTERNAL_USER = "Y";

        public const int FORBIDDEN_STATUS_CODE = 403;
        public const string FORBIDDEN_STATUS_DESCRIPTION = "403 Forbidden";
        
        #endregion


        public const string CACHE_RELEASE_MESSAGE = "__releaseMessage";
 */ 
        public const string TEMLATE_ID = "templateId";

        public const string TEMPLATE_EXT = ".htm";
/*
        public const string FILE_UPLOAD_PATH = "FileUploadPath";

        public const string WEB_FILE_PATH = "WebFilePath";

        public const string UPLOAD_LIST_REFRESH_TIMER = "UploadListRefreshTimer";

        public const string CULTURE_INFO = "en-US";

        public const string AUTO_SAVE_INTERVAL = "AutoSaveInterval";

        public const string SILVERLIGHT_PERIOD_REMINDER = "SilverlightPeriodReminder";

        public const string LOGIN_PAGE_URL = "LoginPageUrl";

        public const string DEFAULT_PAGE_URL = "DefaultPageUrl";
*/
        public const string INTEGRATION_SESSION_PERIOD = "IntegrationSessionPeriod";
        /*
                public const string WIN_AUTH_URL = "WinAuthURL";
        */

        #region Filter types

        public const string FT_LOSS_RUN_SUMMARY = "LossRunSummaryFilter";

        #endregion

    }

    public class RptFieldInfo
    {
        public int RptFieldInfoID { get; set; }
        public string Name { get; set; }
        public bool CanDrag { get; set; }
        public bool IsVisible { get; set; }
        public int Tag { get; set; }
        public int ReportFieldID { get; set; }
        public int ID { get; set; }
        public string HideName { get; set; }
        public int GroupOrder { get; set; }
        public int SortOrder { get; set; }
        public int ReportID { get; set; }
        public double ColumnWidthFactor { get; set; }
        public string SQLName { get; set; }
        public int ColumnOrder { get; set; }
        public string GroupSummaryExpression { get; set; }
        public bool IsGroupable { get; set; }
        public int DataType { get; set; }
        public int SortDirection { get; set; }
        public bool IncludePageBreak { get; set; }
        public bool IsGroupByDefault { get; set; }
        public bool IsDisplayInReport { get; set; }
        public bool IsSummarizable { get; set; }
        public bool IsUsed { get; set; }
        public string Category { get; set; }
        public string FieldValueExpression { get; set; }
        public bool IsCalculated { get; set; }
        public string CalculationDescription { get; set; }
        public bool UseSummaryExpresionGroup { get; set; }
        public string CoverageCode { get; set; }
        public bool IsClientSpecificCode { get; set; }
        public string DefaultName { get; set; }
    }
}