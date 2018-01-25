namespace ReportBuilderAjax.Web.Common
{
    public partial class Consts
    {
        public const string SP_NAME_MEMBERS_SELECT = "rb_Members_Select";
        public const string SP_NAME_LOCATIONS_SELECT = "rb_Locations_Select";

        public const string SP_NAME_GROUP1_CODES_SELECT = "rb_Group1Codes_Select";        
        public const string SP_NAME_GROUP2_CODES_SELECT = "rb_Group2Codes_Select";
        public const string SP_NAME_GROUP3_CODES_SELECT = "rb_Group3Codes_Select";
        public const string SP_NAME_GROUP4_CODES_SELECT = "rb_Group4Codes_Select";

        public const string SP_NAME_USER_AUTHENTICATE = "rb_ReportBuilderUser_Authenticate";
        public const string SP_NAME_USER_SELECT = "rb_ReportBuilderUser_Select";
        public const string SP_NAME_USER_SELECT_BY_ID = "rb_ReportBuilderUser_SelectByID";
        public const string SP_NAME_USER_CONFIGURE_AUTHORIZATION = "rb_ReportBuilderUser_ConfigureAuthorization";
        public const string SP_NAME_USER_ASSOCIATION_SELECT = "rb_UserAssociation_Select";
        public const string SP_NAME_ASSOCIATION_LATEST_LOGIN_SELECT = "rb_AssociationFromLatestLogin_Select";

        public const string SP_NAME_CLIENT_ANALYSIS_1_SELECT = "rb_ClientAnalysis1_Select";
        public const string SP_NAME_CLIENT_ANALYSIS_2_SELECT = "rb_ClientAnalysis2_Select";
        public const string SP_NAME_CLIENT_ANALYSIS_3_SELECT = "rb_ClientAnalysis3_Select";
        public const string SP_NAME_CLIENT_ANALYSIS_4_SELECT = "rb_ClientAnalysis4_Select";
        public const string SP_NAME_CLIENT_ANALYSIS_5_SELECT = "rb_ClientAnalysis5_Select";

        public const string SP_NAME_COVERAGE_CODES_SELECT = "rb_CoverageCodes_Select";
        public const string SP_NAME_DISCOUNTERS_SELECT = "rb_Discounters_Select";

        public const string SP_NAME_CLASS_CODES_SELECT = "rb_ClassCodes_Select";
        public const string SP_NAME_MEDICAL_SUBCLASS_CODES_SELECT = "rb_MedicalSubClassCodes_Select";
        public const string SP_NAME_SUBCLASS_CODES_SELECT = "rb_SubclassCodes_Select";

        public const string SP_NAME_CLIENT_CONFIGURATION_SELECT = "rb_ClientConfiguration_Select";

        public const string SP_NAME_STATE_TYPE_SELECT = "rb_StateType_Select";
        public const string SP_NAME_AGGREGATE_POLICY_BY_CLIENT__SELECT = "rb_AggregatePolicyByClient_Select";
        public const string SP_NAME_SIR_CASH_FLOW_PROTECTION_POLICY_BY_CLIENT__SELECT = "rb_SirCashFlowProtectionPolicyByClient_Select";
        public const string SP_NAME_POLICY_BY_CLIENT__SELECT = "rb_PolicyByClient_Select";
        public const string SP_NAME_POLICY_BY_DEDUCTIBLE_SELECT = "rb_PolicyByDeductible _Select";
        public const string SP_NAME_FUND_YEARS_SELECT = "rb_FundYears_Select";

        public const string SP_NAME_GET_CHECKPOINTS_LIST = "rb_ReportBuilderUser_GetICECheckpointsList";
        public const string SP_NAME_GET_USER_CHECKPOINTS = "rb_ReportBuilderUser_GetUserCheckpoints";

        public const string SP_NAME_GET_DATES_BY_PERIOD = "rb_Dates_GetByPeriod";
        public const string SP_NAME_GET_AS_OF_DATE_BY_RANGE = "rb_AsOfDate_GetByType";
        public const string SP_NAME_GET_USER_SSN_ACCESS_CHECKPOINT = "rb_ReportBuilderUser_GetUserSSNAccess_Checkpoint";
        public const string SP_NAME_GET_POLICY_NUMBER_LIST_BY_POLICY_ID_LIST = "rb_PolicyNumberListByPolicyIdList_Select";
        public const string SP_NAME_GET_RRE_NUMBER_LIST_BY_RRE_STORE_ID_LIST = "rb_RRENumberListByRREStoreIdList_Select";
        
        public const string SP_NAME_SEARCH_CCMSI_USER = "rb_CcmsiUser_Search";
        public const string SP_NAME_GET_CCMSI_USER = "rb_CcmsiUser_Select";
        public const string SP_NAME_GET_CCMSI_USERS = "rb_CcmsiUsers_SelectByUserIDs";
        public const string SP_NAME_GET_CCMSI_BY_USER = "rb_CcmsiUser_SelectByUserID";                

        public const string SP_NAME_CAST_POLICY_COVERAGE_CODES_SELECT = "rb_CastPolicyCoverageCodes_Select";
        public const string SP_NAME_CLIENT_SPECIFIC_SELECT = "rb_ClientSpecific_Select";
        public const string SP_NAME_PRIMARY_CARRIER_SELECT = "rb_PrimaryCarrier_Select";
        public const string SP_NAME_ISSUING_CARRIER_SELECT = "rb_IssuingCarrier_Select";
        public const string SP_NAME_VENUE_SELECT = "rb_Venue_Select";
        public const string SP_NAME_DEFENSE_FIRM_SELECT = "rb_DefenseFirm_Select";
        public const string SP_NAME_DEFENSE_ATTORNEY_SELECT = "rb_DefenseAttorney_Select";
        public const string SP_NAME_PLAINTIFF_FIRM_SELECT = "rb_PlaintiffFirm_Select";
        public const string SP_NAME_PLAINTIFF_ATTORNEY_SELECT = "rb_PlaintiffAttorney_Select";
        public const string SP_NAME_REGION_BY_CLIENT_SELECT = "rb_RegionByClient_Select";
        public const string SP_NAME_OFFICE_BY_CLIENT_SELECT = "rb_OfficeByClient_Select";
        public const string SP_NAME_GET_TRIAL_DATES_BY_PERIOD = "rb_TrialDates_GetByPeriod";
        public const string SP_NAME_GET_VENUE_NAME_LIST = "rb_VenueNameList_Select";
        public const string SP_NAME_GET_DEFENSE_FIRM_NAME_LIST = "rb_DefenseFirmNameList_Select";
        public const string SP_NAME_GET_PLAINTIFF_FIRM_NAME_LIST = "rb_PlaintiffFirmNameList_Select";
        public const string SP_NAME_GET_PRIMARY_CARRIER_NAME_LIST = "rb_PrimaryCarrierNameList_Select";
        public const string SP_NAME_GET_ISSUING_CARRIER_NAME_LIST = "rb_IssuingCarrierNameList_Select";
        public const string SP_NAME_GET_ADJUSTER_NAME_LIST = "rb_AdjusterNameList_Select";
        

        public const string SP_NAME_ADJUSTER_SELECT = "rb_Adjuster_Select";
        public const string SP_NAME_HANDLING_OFFICE_SELECT = "rb_HandlingOffice_Select";

        public const string SP_NAME_HIERARCHY_DESCRIPTION_SELECT = "rb_Hierarchy_Description_Select";
        public const string SP_NAME_GET_RRENUMBERS_LIST = "rb_RRENumbers_Select";
        public const string SP_NAME_COVERAGE_CODES_BY_INJURY_SELECT = "rb_CoverageCodes_ByInjury_Select";
        public const string SP_NAME_BANK_ACCOUNT_SELECT = "rb_BankAccount_Select";
        public const string SP_NAME_GET_BANK_ACCOUNT_NAME_LIST = "rb_BankAccountNameList_Select";

        public const string SP_NAME_PAYEE_FEIN_TAX_SELECT = "rb_PayeeFEINTax_Select";
        public const string SP_NAME_GET_PAYEE_FEIN_TAX_NUMBER_LIST = "rb_PayeeFEINTaxNumberList_Select";

        public const string SP_NAME_NOTE_TYPES_SELECT = "rb_NoteTypes_Select";
        public const string SP_NAME_NOTE_INPUT_BY_SELECT = "rb_NoteInputBy_Select";

        public const string SP_NAME_PERIOD_LIST_SELECT = "rb_ComparisonPeriod_Periods_Select";
    }
}
