if (typeof Consts == "undefined") {
    var Consts = {};
};

Consts.HANDLERS = {
    SEARCH_HANDLER: 'HttpHandlers/ReportBuilderHandler.ashx'
};

Consts.TEMPLATE_ACTION = {
    GET_TEMPLATE: "GetTemplate",
    GET_ALL_TEMPLATES: "GetAllTemplates"
};

Consts.HANDLER_ACTIONS = {
    GET_HEADER_DATA: 'GetHeaderData',
    SWITCH_CLIENT: 'SwitchClient',
    DELETE_REPORT: 'DeleteReport',
    UPDATE_DOCUMENT_STATUS: 'UpdateDocumentStatus',
    GET_RELEASE_NOTES_DATA: "GetReleaseNotesData",
    GET_DELIVERED_REPORTS_DATA: "GetDeliveredReportsData",
    DELETE_DELIVERED_REPORTS: "DeleteDeliveredReport",
    GET_DELIVERED_REPORTS_DATA_BY_PAGE: "GetDeliveredReportsDataByPage",
    GET_SCHEDULES: "GetSchedules",
    GET_ALL_DATA_FOR_GRID: "GetAllDataForGrid",
    GET_FILTER_DATA: "GetFilterData",
    GET_ALL_DATA: "GetAllData",
    GET_COVERAGE_LIST: "GetCoverages",
    GET_STATE_OF_JURISDICTION: "GetStateOfJurisdiction",
    GET_CLIENT_ANALYSIS_1: "GetClientAnalysis1",
    GET_CLIENT_ANALYSIS_2: "GetClientAnalysis2",
    GET_CLIENT_ANALYSIS_3: "GetClientAnalysis3",
    GET_CLIENT_ANALYSIS_4: "GetClientAnalysis4",
    GET_CLIENT_ANALYSIS_5: "GetClientAnalysis5",
    GET_CLASSES: "GetClasses",
    GET_MEMBERS: "GetMembers",
    GET_LOCATIONS: "GetLocations",
    GET_GROUP_CODE1S: "GetGroupCode1s",
    GET_GROUP_CODE2S: "GetGroupCode2s",
    GET_GROUP_CODE3S: "GetGroupCode3s",
    GET_GROUP_CODE4S: "GetGroupCode4s",
    RUN_STANDART_REPORT: "RunStandartReport",
    RUN_CUSTOM_REPORT: "RunCustomReport",
    SAVE_STANDART_REPORT: "SaveStandartReport",
    SAVE_CUSTOM_REPORT: "SaveCustomReport",
    VALIDATE_PAGE_BREAK: "ValidatePageBreak"
};

Consts.FILTER_TYPE = {
    LOSS_RUN_SUMMARY: "LossRunSummaryFilter"
};

Consts.DATA_TYPE = {
   STRING: 1000,
   DATE: 1001,
   INTEGER: 1002,
   DECIMAL: 1003,
   BOOL: 1004,
   MONEY: 1005,
   PERCENTAGE: 1006,
   TIME: 1007
};

Consts.SHEDULE_TAB = {
    ONCE: 1,
    DAILY: 2,
    WEEKLY: 3,
    MONTHLY: 4,
    QUARTERLY: 5,
    YEARLY: 6
};

Consts.REPORT_TYPE = {
    UnknownReport : 1,
    CheckRegister : 1011,
    TransactionRegister : 1012,
    LossRunSummary : 1014,
    ManagedCareSavingsAndFees : 1015,
    AggregateReportDetailPaid : 1016,
    ManagedCareSavingsAndFeesDetail : 1017,
    MonthlyBillingReport: 1018,
    AggregateReportSummaryPaid : 1019,
    ManagedCareSavingsByState : 1021,
    LossRatioWithLossSummary : 1022,
    ManagedCareBillsAndChargesByProvider : 1023,
    ManagedCareSavingsByProviderType : 1024,
    LossRunDetailExpanded : 1025,
    StewardshipReport : 1028,
    ManagedCarePharmacyPBMSavings : 1027,
    AggregateReportDetailIncurred : 1029,
    AggregateReportSummaryIncurred : 1030,
    SIRDeductibleSpecificDetailIncurred : 1031,
    SIRDeductibleSpecificDetailPaid : 1032,
    SIRDeductibleSpecificSummaryIncurred: 1033,
    SIRDeductibleSpecificSummaryPaid : 1034,
    ClaimLag : 1026,
    ActivityReport: 1035,
    ReserveChangeReport : 1036,
    ManagedCareDiagnosticSavings : 1040,
    LossRunSummaryWithCurrentPaid : 1041,
    LitigationManagementBasic : 1045,
    LitigationManagementDetail : 1046,
    ClosingRatio : 1042,
    PAndIReimbursableReport : 1043,
    NoteAnalysis : 1044,
    LossSummaryByCoverageAndPaymentClass : 1047,
    ComparisonReport : 1048,
    CorridorDeductible : 1049,
    SummaryLossByClassEmplyeeInjury : 1050,
    MadicareQueryFucntion :1051,
    CFPEndorsementIncurredDetail : 1054,
    CFPEndorsementIncurredSummary :1055,
    CFPEndorsementPaidDetail :1056,
    CFPEndorsementPaidSummary :1057,
    LossRunSummaryForPowerUser : 1058
};

Consts.DATE_RANGE = {
    NONE : 0,
    LAST_MONTH : 1,
    LAST_QUARTER : 2,
    LAST_YEAR : 3,
    MONTH_TO_DATE : 4,
    YEAR_TO_DATE : 5,
    CUSTOM : 6,
    LAST_CALENDAR_WEEK : 7,
    LAST_2_WEEKS : 8,
    ALL_DATES : 9,
    LAST_BUSINESS_WEEK : 10,
    LAST_X_DAYS : 11,
    YESTERDAY : 12,
    LAST_WEEK : 13,
    PREVIOUS_BUSINESSDAY: 14
};

Consts.AS_OF_DATE_TYPE = {
    LAST_DAY_OF_LAST_WEEK : 1,
    LAST_DAY_OF_LAST_MONTH : 2,
    LAST_DAY_OF_LAST_QUARTER : 3,
    LAST_DAY_OF_LAST_YEAR : 4,
    CUSTOM : 5,
    YESTERDAY : 6
};

Consts.DATE_RANGE_CAPTIONS = {
    ITEM_CUSTOM: "Custom",
    ITEM_YESTERDAY: "Yesterday",
    ITEM_PREVIOUS_BUSINNES_DAY: "Previous Business Day",
    ITEM_LAST_WEEK_MONDAY_TO_SUNDAY: "Last Week (Monday to Sunday)",
    ITEM_LAST_CALENDAR_WEEK: "Last Calendar Week",
    ITEM_LAST_BUSINESS_WEEK: "Last Business Week",
    ITEM_LAST_X_DAYS: "Last X Days",
    ITEM_LAST_2_WEEKS: "Last 2 Weeks",
    ITEM_LAST_MONTH: "Last Month",
    ITEM_LAST_QUARTER: "Last Quarter",
    ITEM_LAST_YEAR: "Last Year",
    ITEM_MONTH_TO_DATE: "Month to Date",
    ITEM_YEAR_TO_DATE: "Year to Date",
    ITEM_ALL_DATES: "All Dates"
};

Consts.DAY_OF_WEEK = {
    SUNDAY: 7,
    MONDEY: 1,
    TUESDAY: 2,
    WEDNESDAY:3,
    THURSDAY:4,
    FRIDAY:5,
    SATURDAY:6
};

Consts.FORMAT_TYPE = {
    PDF : 1000,
    Excel : 1001,
    XML : 1002,
    CSV : 1003,
    Image : 1004,
    MSHTML: 1005,
    HTML: 1006,
    GRID:1007
};
