
namespace ReportBuilderAjax.Web
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Data.Objects.DataClasses;
    using System.Linq;
    using System.ServiceModel.DomainServices.Hosting;
    using System.ServiceModel.DomainServices.Server;


    // The MetadataTypeAttribute identifies AdminUserNameMetadata as the class
    // that carries additional metadata for the AdminUserName class.
    [MetadataTypeAttribute(typeof(AdminUserName.AdminUserNameMetadata))]
    public partial class AdminUserName
    {

        // This class allows you to attach custom attributes to properties
        // of the AdminUserName class.
        //
        // For example, the following marks the Xyz property as a
        // required property and specifies the format for valid values:
        //    [Required]
        //    [RegularExpression("[A-Z][A-Za-z0-9]*")]
        //    [StringLength(32)]
        //    public string Xyz { get; set; }
        internal sealed class AdminUserNameMetadata
        {

            // Metadata classes are not meant to be instantiated.
            private AdminUserNameMetadata()
            {
            }

            public string userName { get; set; }
        }
    }

    // The MetadataTypeAttribute identifies AssnSpecificFieldDefinitionMetadata as the class
    // that carries additional metadata for the AssnSpecificFieldDefinition class.
    [MetadataTypeAttribute(typeof(AssnSpecificFieldDefinition.AssnSpecificFieldDefinitionMetadata))]
    public partial class AssnSpecificFieldDefinition
    {

        // This class allows you to attach custom attributes to properties
        // of the AssnSpecificFieldDefinition class.
        //
        // For example, the following marks the Xyz property as a
        // required property and specifies the format for valid values:
        //    [Required]
        //    [RegularExpression("[A-Z][A-Za-z0-9]*")]
        //    [StringLength(32)]
        //    public string Xyz { get; set; }
        internal sealed class AssnSpecificFieldDefinitionMetadata
        {

            // Metadata classes are not meant to be instantiated.
            private AssnSpecificFieldDefinitionMetadata()
            {
            }

            public int AssnSpecificFieldDefinitionId { get; set; }

            public string AssociationNumber { get; set; }

            public string coverage_code { get; set; }

            public int CreatedByUserID { get; set; }

            public DateTime CreatedDate { get; set; }

            public string FieldDescription { get; set; }

            public string FieldName { get; set; }
        }
    }

    // The MetadataTypeAttribute identifies ClientReportFieldMetadata as the class
    // that carries additional metadata for the ClientReportField class.
    [MetadataTypeAttribute(typeof(ClientReportField.ClientReportFieldMetadata))]
    public partial class ClientReportField
    {

        // This class allows you to attach custom attributes to properties
        // of the ClientReportField class.
        //
        // For example, the following marks the Xyz property as a
        // required property and specifies the format for valid values:
        //    [Required]
        //    [RegularExpression("[A-Z][A-Za-z0-9]*")]
        //    [StringLength(32)]
        //    public string Xyz { get; set; }
        internal sealed class ClientReportFieldMetadata
        {

            // Metadata classes are not meant to be instantiated.
            private ClientReportFieldMetadata()
            {
            }

            public string ClientNumber { get; set; }

            public int ClientReportFieldID { get; set; }

            public int CreatedByUserID { get; set; }

            public DateTime CreatedDate { get; set; }

            public ReportField ReportField { get; set; }

            public int ReportFieldID { get; set; }
        }
    }

    // The MetadataTypeAttribute identifies DataTypeMetadata as the class
    // that carries additional metadata for the DataType class.
    [MetadataTypeAttribute(typeof(DataType.DataTypeMetadata))]
    public partial class DataType
    {

        // This class allows you to attach custom attributes to properties
        // of the DataType class.
        //
        // For example, the following marks the Xyz property as a
        // required property and specifies the format for valid values:
        //    [Required]
        //    [RegularExpression("[A-Z][A-Za-z0-9]*")]
        //    [StringLength(32)]
        //    public string Xyz { get; set; }
        internal sealed class DataTypeMetadata
        {

            // Metadata classes are not meant to be instantiated.
            private DataTypeMetadata()
            {
            }

            public int CreatedByUserID { get; set; }

            public DateTime CreatedDate { get; set; }

            public int DataTypeID { get; set; }

            public string DataTypeName { get; set; }

            public string Description { get; set; }

            public EntityCollection<ReportField> ReportField { get; set; }

            public EntityCollection<ReportParameter> ReportParameter { get; set; }
        }
    }

    // The MetadataTypeAttribute identifies FilterMetadata as the class
    // that carries additional metadata for the Filter class.
    [MetadataTypeAttribute(typeof(Filter.FilterMetadata))]
    public partial class Filter
    {

        // This class allows you to attach custom attributes to properties
        // of the Filter class.
        //
        // For example, the following marks the Xyz property as a
        // required property and specifies the format for valid values:
        //    [Required]
        //    [RegularExpression("[A-Z][A-Za-z0-9]*")]
        //    [StringLength(32)]
        //    public string Xyz { get; set; }
        internal sealed class FilterMetadata
        {

            // Metadata classes are not meant to be instantiated.
            private FilterMetadata()
            {
            }

            public int FilterID { get; set; }

            public EntityCollection<Report> Report { get; set; }

            public string SystemType { get; set; }
        }
    }

    // The MetadataTypeAttribute identifies FormatTypeMetadata as the class
    // that carries additional metadata for the FormatType class.
    [MetadataTypeAttribute(typeof(FormatType.FormatTypeMetadata))]
    public partial class FormatType
    {

        // This class allows you to attach custom attributes to properties
        // of the FormatType class.
        //
        // For example, the following marks the Xyz property as a
        // required property and specifies the format for valid values:
        //    [Required]
        //    [RegularExpression("[A-Z][A-Za-z0-9]*")]
        //    [StringLength(32)]
        //    public string Xyz { get; set; }
        internal sealed class FormatTypeMetadata
        {

            // Metadata classes are not meant to be instantiated.
            private FormatTypeMetadata()
            {
            }

            public int CreatedByUserID { get; set; }

            public DateTime CreatedDate { get; set; }

            public string Description { get; set; }

            public string Extension { get; set; }

            public int FormatTypeID { get; set; }

            public string FormatTypeName { get; set; }

            public EntityCollection<ReportLayoutStyleRdl> ReportLayoutStyleRdl { get; set; }

            public EntityCollection<UserReport> UserReport { get; set; }
        }
    }

    // The MetadataTypeAttribute identifies RdlComponentMetadata as the class
    // that carries additional metadata for the RdlComponent class.
    [MetadataTypeAttribute(typeof(RdlComponent.RdlComponentMetadata))]
    public partial class RdlComponent
    {

        // This class allows you to attach custom attributes to properties
        // of the RdlComponent class.
        //
        // For example, the following marks the Xyz property as a
        // required property and specifies the format for valid values:
        //    [Required]
        //    [RegularExpression("[A-Z][A-Za-z0-9]*")]
        //    [StringLength(32)]
        //    public string Xyz { get; set; }
        internal sealed class RdlComponentMetadata
        {

            // Metadata classes are not meant to be instantiated.
            private RdlComponentMetadata()
            {
            }

            public int CreatedByUserID { get; set; }

            public Nullable<DateTime> CreatedDate { get; set; }

            public string Description { get; set; }

            public int RdlComponentID { get; set; }

            public RdlComponentType RdlComponentType { get; set; }

            public int RdlComponentTypeID { get; set; }

            public string RdlData { get; set; }

            public EntityCollection<ReportLayoutStyleRdl> ReportLayoutStyleRdl { get; set; }
        }
    }

    // The MetadataTypeAttribute identifies RdlComponentTypeMetadata as the class
    // that carries additional metadata for the RdlComponentType class.
    [MetadataTypeAttribute(typeof(RdlComponentType.RdlComponentTypeMetadata))]
    public partial class RdlComponentType
    {

        // This class allows you to attach custom attributes to properties
        // of the RdlComponentType class.
        //
        // For example, the following marks the Xyz property as a
        // required property and specifies the format for valid values:
        //    [Required]
        //    [RegularExpression("[A-Z][A-Za-z0-9]*")]
        //    [StringLength(32)]
        //    public string Xyz { get; set; }
        internal sealed class RdlComponentTypeMetadata
        {

            // Metadata classes are not meant to be instantiated.
            private RdlComponentTypeMetadata()
            {
            }

            public int CreatedByUserID { get; set; }

            public DateTime CreatedDate { get; set; }

            public EntityCollection<RdlComponent> RdlComponent { get; set; }

            public int RdlComponentTypeID { get; set; }

            public string RdlComponentTypeName { get; set; }
        }
    }

    // The MetadataTypeAttribute identifies ReleaseNoteMetadata as the class
    // that carries additional metadata for the ReleaseNote class.
    [MetadataTypeAttribute(typeof(ReleaseNote.ReleaseNoteMetadata))]
    public partial class ReleaseNote
    {

        // This class allows you to attach custom attributes to properties
        // of the ReleaseNote class.
        //
        // For example, the following marks the Xyz property as a
        // required property and specifies the format for valid values:
        //    [Required]
        //    [RegularExpression("[A-Z][A-Za-z0-9]*")]
        //    [StringLength(32)]
        //    public string Xyz { get; set; }
        internal sealed class ReleaseNoteMetadata
        {

            // Metadata classes are not meant to be instantiated.
            private ReleaseNoteMetadata()
            {
            }

            public DateTime CreatedDate { get; set; }

            public int CreatedUserID { get; set; }

            public string DOCUMENT { get; set; }

            public DateTime ModifiedDate { get; set; }

            public int ModifiedUserId { get; set; }

            public Nullable<DateTime> ReleaseDate { get; set; }

            public int ReleaseNoteID { get; set; }

            public string ReleaseNumber { get; set; }

            public string ReleaseTitle { get; set; }

            public EntityCollection<UserReleaseNote> UserReleaseNote { get; set; }
        }
    }

    // The MetadataTypeAttribute identifies ReportMetadata as the class
    // that carries additional metadata for the Report class.
    [MetadataTypeAttribute(typeof(Report.ReportMetadata))]
    public partial class Report
    {

        // This class allows you to attach custom attributes to properties
        // of the Report class.
        //
        // For example, the following marks the Xyz property as a
        // required property and specifies the format for valid values:
        //    [Required]
        //    [RegularExpression("[A-Z][A-Za-z0-9]*")]
        //    [StringLength(32)]
        //    public string Xyz { get; set; }
        internal sealed class ReportMetadata
        {

            // Metadata classes are not meant to be instantiated.
            private ReportMetadata()
            {
            }

            public int CreatedByUserID { get; set; }

            public DateTime CreatedDate { get; set; }

            public string Description { get; set; }

            public Filter Filters { get; set; }

            public string RdlFooterMessage { get; set; }

            public string RdlHeaderDate { get; set; }

            public EntityCollection<Report_ICESecurityCheckpoint> Report_ICESecurityCheckpoint { get; set; }

            public EntityCollection<ReportField> ReportField { get; set; }

            public ReportFolder ReportFolder { get; set; }

            public int ReportFolderID { get; set; }

            public int ReportID { get; set; }

            public EntityCollection<ReportLayoutStyle> ReportLayoutStyle { get; set; }

            public string ReportName { get; set; }

            public EntityCollection<ReportParameter> ReportParameter { get; set; }

            public EntityCollection<SubReport> SubReport { get; set; }

            public EntityCollection<UserReport> UserReport { get; set; }

            public bool VerifyRowCountBeforeExecuting { get; set; }

            public string VerifyRowCountSP { get; set; }
        }
    }

    // The MetadataTypeAttribute identifies Report_ICESecurityCheckpointMetadata as the class
    // that carries additional metadata for the Report_ICESecurityCheckpoint class.
    [MetadataTypeAttribute(typeof(Report_ICESecurityCheckpoint.Report_ICESecurityCheckpointMetadata))]
    public partial class Report_ICESecurityCheckpoint
    {

        // This class allows you to attach custom attributes to properties
        // of the Report_ICESecurityCheckpoint class.
        //
        // For example, the following marks the Xyz property as a
        // required property and specifies the format for valid values:
        //    [Required]
        //    [RegularExpression("[A-Z][A-Za-z0-9]*")]
        //    [StringLength(32)]
        //    public string Xyz { get; set; }
        internal sealed class Report_ICESecurityCheckpointMetadata
        {

            // Metadata classes are not meant to be instantiated.
            private Report_ICESecurityCheckpointMetadata()
            {
            }

            public string CheckpointID { get; set; }

            public Report Report { get; set; }

            public int Report_ICESecurityCheckpointID { get; set; }

            public int ReportID { get; set; }
        }
    }

    // The MetadataTypeAttribute identifies ReportDeliveryLogMetadata as the class
    // that carries additional metadata for the ReportDeliveryLog class.
    [MetadataTypeAttribute(typeof(ReportDeliveryLog.ReportDeliveryLogMetadata))]
    public partial class ReportDeliveryLog
    {

        // This class allows you to attach custom attributes to properties
        // of the ReportDeliveryLog class.
        //
        // For example, the following marks the Xyz property as a
        // required property and specifies the format for valid values:
        //    [Required]
        //    [RegularExpression("[A-Z][A-Za-z0-9]*")]
        //    [StringLength(32)]
        //    public string Xyz { get; set; }
        internal sealed class ReportDeliveryLogMetadata
        {

            // Metadata classes are not meant to be instantiated.
            private ReportDeliveryLogMetadata()
            {
            }

            public DateTime DeliveryDate { get; set; }

            public int ReportDeliveryLogID { get; set; }

            public Schedule Schedule { get; set; }

            public int ScheduleID { get; set; }

            public ScheduleRecipient ScheduleRecipient { get; set; }

            public Nullable<int> ScheduleRecipientID { get; set; }

            public UserReportOutput UserReportOutput { get; set; }

            public Nullable<int> UserReportOutputID { get; set; }
        }
    }

    // The MetadataTypeAttribute identifies ReportFieldMetadata as the class
    // that carries additional metadata for the ReportField class.
    [MetadataTypeAttribute(typeof(ReportField.ReportFieldMetadata))]
    public partial class ReportField
    {

        // This class allows you to attach custom attributes to properties
        // of the ReportField class.
        //
        // For example, the following marks the Xyz property as a
        // required property and specifies the format for valid values:
        //    [Required]
        //    [RegularExpression("[A-Z][A-Za-z0-9]*")]
        //    [StringLength(32)]
        //    public string Xyz { get; set; }
        internal sealed class ReportFieldMetadata
        {

            // Metadata classes are not meant to be instantiated.
            private ReportFieldMetadata()
            {
            }

            public string CalculationDescription { get; set; }

            public string Category { get; set; }

            public EntityCollection<ClientReportField> ClientReportField { get; set; }

            public int ColumnOrder { get; set; }

            public double ColumnWidthFactor { get; set; }

            public int CreatedByUserID { get; set; }

            public DateTime CreatedDate { get; set; }

            public DataType DataType { get; set; }

            public string FieldName { get; set; }

            public string FieldValueExpression { get; set; }

            public string GroupSummaryExpression { get; set; }

            public bool IncludePageBreak { get; set; }

            public bool IsCalculated { get; set; }

            public bool IsClientSpecific { get; set; }

            public bool isClientSpecificCode { get; set; }

            public bool IsDisplayInReport { get; set; }

            public bool IsGroupable { get; set; }

            public bool IsGroupByDefault { get; set; }

            public bool IsSensitive { get; set; }

            public bool IsSummarizable { get; set; }

            public Report Report { get; set; }

            public int ReportFieldID { get; set; }

            public string SQLFieldName { get; set; }

            public EntityCollection<UserReportField> UserReportField { get; set; }

            public bool UseSummaryExpresionGroup { get; set; }
        }
    }

    // The MetadataTypeAttribute identifies ReportFolderMetadata as the class
    // that carries additional metadata for the ReportFolder class.
    [MetadataTypeAttribute(typeof(ReportFolder.ReportFolderMetadata))]
    public partial class ReportFolder
    {

        // This class allows you to attach custom attributes to properties
        // of the ReportFolder class.
        //
        // For example, the following marks the Xyz property as a
        // required property and specifies the format for valid values:
        //    [Required]
        //    [RegularExpression("[A-Z][A-Za-z0-9]*")]
        //    [StringLength(32)]
        //    public string Xyz { get; set; }
        internal sealed class ReportFolderMetadata
        {

            // Metadata classes are not meant to be instantiated.
            private ReportFolderMetadata()
            {
            }

            public string Description { get; set; }

            public bool IsUseWithCheckpoint { get; set; }

            public Nullable<int> OrderIndex { get; set; }

            public EntityCollection<Report> Report { get; set; }

            public int ReportFolderID { get; set; }

            public string ReportFolderName { get; set; }
        }
    }

    // The MetadataTypeAttribute identifies ReportLayoutStyleMetadata as the class
    // that carries additional metadata for the ReportLayoutStyle class.
    [MetadataTypeAttribute(typeof(ReportLayoutStyle.ReportLayoutStyleMetadata))]
    public partial class ReportLayoutStyle
    {

        // This class allows you to attach custom attributes to properties
        // of the ReportLayoutStyle class.
        //
        // For example, the following marks the Xyz property as a
        // required property and specifies the format for valid values:
        //    [Required]
        //    [RegularExpression("[A-Z][A-Za-z0-9]*")]
        //    [StringLength(32)]
        //    public string Xyz { get; set; }
        internal sealed class ReportLayoutStyleMetadata
        {

            // Metadata classes are not meant to be instantiated.
            private ReportLayoutStyleMetadata()
            {
            }

            public int CreatedByUserID { get; set; }

            public DateTime CreatedDate { get; set; }

            public string Description { get; set; }

            public bool IsCustom { get; set; }

            public bool IsSensitive { get; set; }

            public bool IsUsed { get; set; }

            public int OrderIndex { get; set; }

            public string PreviewImagePath { get; set; }

            public Report Report { get; set; }

            public int ReportID { get; set; }

            public int ReportLayoutStyleID { get; set; }

            public string ReportLayoutStyleName { get; set; }

            public EntityCollection<ReportLayoutStyleRdl> ReportLayoutStyleRdl { get; set; }

            public string ReportLink { get; set; }

            public EntityCollection<UserReport> UserReport { get; set; }
        }
    }

    // The MetadataTypeAttribute identifies ReportLayoutStyleRdlMetadata as the class
    // that carries additional metadata for the ReportLayoutStyleRdl class.
    [MetadataTypeAttribute(typeof(ReportLayoutStyleRdl.ReportLayoutStyleRdlMetadata))]
    public partial class ReportLayoutStyleRdl
    {

        // This class allows you to attach custom attributes to properties
        // of the ReportLayoutStyleRdl class.
        //
        // For example, the following marks the Xyz property as a
        // required property and specifies the format for valid values:
        //    [Required]
        //    [RegularExpression("[A-Z][A-Za-z0-9]*")]
        //    [StringLength(32)]
        //    public string Xyz { get; set; }
        internal sealed class ReportLayoutStyleRdlMetadata
        {

            // Metadata classes are not meant to be instantiated.
            private ReportLayoutStyleRdlMetadata()
            {
            }

            public int CreatedByUserID { get; set; }

            public DateTime CreatedDate { get; set; }

            public FormatType FormatType { get; set; }

            public int FormatTypeID { get; set; }

            public RdlComponent RdlComponent { get; set; }

            public int RdlComponentID { get; set; }

            public ReportLayoutStyle ReportLayoutStyle { get; set; }

            public int ReportLayoutStyleID { get; set; }

            public int ReportLayoutStyleRdlID { get; set; }
        }
    }

    // The MetadataTypeAttribute identifies ReportParameterMetadata as the class
    // that carries additional metadata for the ReportParameter class.
    [MetadataTypeAttribute(typeof(ReportParameter.ReportParameterMetadata))]
    public partial class ReportParameter
    {

        // This class allows you to attach custom attributes to properties
        // of the ReportParameter class.
        //
        // For example, the following marks the Xyz property as a
        // required property and specifies the format for valid values:
        //    [Required]
        //    [RegularExpression("[A-Z][A-Za-z0-9]*")]
        //    [StringLength(32)]
        //    public string Xyz { get; set; }
        internal sealed class ReportParameterMetadata
        {

            // Metadata classes are not meant to be instantiated.
            private ReportParameterMetadata()
            {
            }

            public int CreatedByUserID { get; set; }

            public DateTime CreatedDate { get; set; }

            public DataType DataType { get; set; }

            public string DefaultValue { get; set; }

            public string Description { get; set; }

            public bool IsQueryParameter { get; set; }

            public bool IsReportParameter { get; set; }

            public int OrderIndex { get; set; }

            public Report Report { get; set; }

            public int ReportParameterID { get; set; }

            public string ReportParameterName { get; set; }

            public EntityCollection<UserReportParameter> UserReportParameter { get; set; }
        }
    }

    // The MetadataTypeAttribute identifies ScheduleMetadata as the class
    // that carries additional metadata for the Schedule class.
    [MetadataTypeAttribute(typeof(Schedule.ScheduleMetadata))]
    public partial class Schedule
    {

        // This class allows you to attach custom attributes to properties
        // of the Schedule class.
        //
        // For example, the following marks the Xyz property as a
        // required property and specifies the format for valid values:
        //    [Required]
        //    [RegularExpression("[A-Z][A-Za-z0-9]*")]
        //    [StringLength(32)]
        //    public string Xyz { get; set; }
        internal sealed class ScheduleMetadata
        {

            // Metadata classes are not meant to be instantiated.
            private ScheduleMetadata()
            {
            }

            public string AdditionalEmailAddresses { get; set; }

            public string CalendarDays { get; set; }

            public string Comment { get; set; }

            public int CreatedByUserID { get; set; }

            public DateTime CreatedDate { get; set; }

            public Nullable<byte> DaysOfWeekBitmask { get; set; }

            public bool IsActive { get; set; }

            public bool IsEveryWeekDay { get; set; }

            public bool IsNotSendWithNoData { get; set; }

            public Nullable<short> MonthsOfYearBitmask { get; set; }

            public Priority PriorityType { get; set; }

            public int PriorityTypeID { get; set; }

            public string RecurrenceDescription { get; set; }

            public Recurrence RecurrenceType { get; set; }

            public int RecurrenceTypeID { get; set; }

            public Nullable<int> RepeatAfterInterval { get; set; }

            public EntityCollection<ReportDeliveryLog> ReportDeliveryLog { get; set; }

            public int ScheduleID { get; set; }

            public EntityCollection<ScheduleRecipient> ScheduleRecipient { get; set; }

            public DateTime ScheduleStart { get; set; }

            public Nullable<DateTime> ScheduleStop { get; set; }

            public string Subject { get; set; }

            public string SubscriptionID { get; set; }

            public UserReport UserReport { get; set; }

            public int UserReportID { get; set; }

            public WeekOfMonth WeekOfMonthType { get; set; }

            public Nullable<int> WeekOfMonthTypeID { get; set; }
        }
    }

    // The MetadataTypeAttribute identifies ScheduleRecipientMetadata as the class
    // that carries additional metadata for the ScheduleRecipient class.
    [MetadataTypeAttribute(typeof(ScheduleRecipient.ScheduleRecipientMetadata))]
    public partial class ScheduleRecipient
    {

        // This class allows you to attach custom attributes to properties
        // of the ScheduleRecipient class.
        //
        // For example, the following marks the Xyz property as a
        // required property and specifies the format for valid values:
        //    [Required]
        //    [RegularExpression("[A-Z][A-Za-z0-9]*")]
        //    [StringLength(32)]
        //    public string Xyz { get; set; }
        internal sealed class ScheduleRecipientMetadata
        {

            // Metadata classes are not meant to be instantiated.
            private ScheduleRecipientMetadata()
            {
            }

            public int CreatedByUserID { get; set; }

            public DateTime CreatedDate { get; set; }

            public DeliveryMethod DeliveryMethodType { get; set; }

            public int DeliveryMethodTypeID { get; set; }

            public bool IsActive { get; set; }

            public EntityCollection<ReportDeliveryLog> ReportDeliveryLog { get; set; }

            public Schedule Schedule { get; set; }

            public int ScheduleID { get; set; }

            public int ScheduleRecipientID { get; set; }

            public int UserID { get; set; }
        }
    }

    // The MetadataTypeAttribute identifies SubReportMetadata as the class
    // that carries additional metadata for the SubReport class.
    [MetadataTypeAttribute(typeof(SubReport.SubReportMetadata))]
    public partial class SubReport
    {

        // This class allows you to attach custom attributes to properties
        // of the SubReport class.
        //
        // For example, the following marks the Xyz property as a
        // required property and specifies the format for valid values:
        //    [Required]
        //    [RegularExpression("[A-Z][A-Za-z0-9]*")]
        //    [StringLength(32)]
        //    public string Xyz { get; set; }
        internal sealed class SubReportMetadata
        {

            // Metadata classes are not meant to be instantiated.
            private SubReportMetadata()
            {
            }

            public int CreatedByUserID { get; set; }

            public DateTime CreatedDate { get; set; }

            public string Description { get; set; }

            public int OrderIndex { get; set; }

            public Report Report { get; set; }

            public int ReportID { get; set; }

            public string SPName { get; set; }

            public int SubReportID { get; set; }

            public string SubReportName { get; set; }

            public EntityCollection<SubReportParameter> SubReportParameter { get; set; }
        }
    }

    // The MetadataTypeAttribute identifies SubReportParameterMetadata as the class
    // that carries additional metadata for the SubReportParameter class.
    [MetadataTypeAttribute(typeof(SubReportParameter.SubReportParameterMetadata))]
    public partial class SubReportParameter
    {

        // This class allows you to attach custom attributes to properties
        // of the SubReportParameter class.
        //
        // For example, the following marks the Xyz property as a
        // required property and specifies the format for valid values:
        //    [Required]
        //    [RegularExpression("[A-Z][A-Za-z0-9]*")]
        //    [StringLength(32)]
        //    public string Xyz { get; set; }
        internal sealed class SubReportParameterMetadata
        {

            // Metadata classes are not meant to be instantiated.
            private SubReportParameterMetadata()
            {
            }

            public int CreatedByUserID { get; set; }

            public DateTime CreatedDate { get; set; }

            public string Description { get; set; }

            public string FieldValueExpression { get; set; }

            public SubReport SubReport { get; set; }

            public int SubReportID { get; set; }

            public int SubReportParameterID { get; set; }

            public string SubReportParameterName { get; set; }
        }
    }

    // The MetadataTypeAttribute identifies UserReleaseNoteMetadata as the class
    // that carries additional metadata for the UserReleaseNote class.
    [MetadataTypeAttribute(typeof(UserReleaseNote.UserReleaseNoteMetadata))]
    public partial class UserReleaseNote
    {

        // This class allows you to attach custom attributes to properties
        // of the UserReleaseNote class.
        //
        // For example, the following marks the Xyz property as a
        // required property and specifies the format for valid values:
        //    [Required]
        //    [RegularExpression("[A-Z][A-Za-z0-9]*")]
        //    [StringLength(32)]
        //    public string Xyz { get; set; }
        internal sealed class UserReleaseNoteMetadata
        {

            // Metadata classes are not meant to be instantiated.
            private UserReleaseNoteMetadata()
            {
            }

            public DateTime CreatedDate { get; set; }

            public int CreatedUserID { get; set; }

            public bool IsReleaseNoteNotificationViewed { get; set; }

            public bool IsReleaseNoteRead { get; set; }

            public DateTime ModifiedDate { get; set; }

            public int ModifiedUserId { get; set; }

            public ReleaseNote ReleaseNote { get; set; }

            public int ReleaseNoteID { get; set; }

            public int UserID { get; set; }
        }
    }

    // The MetadataTypeAttribute identifies UserReportMetadata as the class
    // that carries additional metadata for the UserReport class.
    [MetadataTypeAttribute(typeof(UserReport.UserReportMetadata))]
    public partial class UserReport
    {

        // This class allows you to attach custom attributes to properties
        // of the UserReport class.
        //
        // For example, the following marks the Xyz property as a
        // required property and specifies the format for valid values:
        //    [Required]
        //    [RegularExpression("[A-Z][A-Za-z0-9]*")]
        //    [StringLength(32)]
        //    public string Xyz { get; set; }
        internal sealed class UserReportMetadata
        {

            // Metadata classes are not meant to be instantiated.
            private UserReportMetadata()
            {
            }

            public string ClientNumber { get; set; }

            public int CreatedByUserID { get; set; }

            public DateTime CreatedDate { get; set; }

            public FormatType FormatType { get; set; }

            public int FormatTypeID { get; set; }

            public bool IncludeTitlePage { get; set; }

            public bool IsCustom { get; set; }

            public bool IsDeleted { get; set; }

            public bool IsSummaryOnly { get; set; }

            public bool IsTransient { get; set; }

            public bool IsTurnOffPageBreak { get; set; }

            public int ModifiedByUserID { get; set; }

            public DateTime ModifiedDate { get; set; }

            public Report Report { get; set; }

            public int ReportID { get; set; }

            public ReportLayoutStyle ReportLayoutStyle { get; set; }

            public int ReportLayoutStyleID { get; set; }

            public int RowsCount { get; set; }

            public EntityCollection<Schedule> Schedule { get; set; }

            public int UserID { get; set; }

            public EntityCollection<UserReportField> UserReportField { get; set; }

            public int UserReportID { get; set; }

            public string UserReportName { get; set; }

            public EntityCollection<UserReportOutput> UserReportOutput { get; set; }

            public EntityCollection<UserReportParameter> UserReportParameter { get; set; }

            public EntityCollection<UserReportSummarizeField> UserReportSummarizeField { get; set; }
        }
    }

    // The MetadataTypeAttribute identifies UserReportFieldMetadata as the class
    // that carries additional metadata for the UserReportField class.
    [MetadataTypeAttribute(typeof(UserReportField.UserReportFieldMetadata))]
    public partial class UserReportField
    {

        // This class allows you to attach custom attributes to properties
        // of the UserReportField class.
        //
        // For example, the following marks the Xyz property as a
        // required property and specifies the format for valid values:
        //    [Required]
        //    [RegularExpression("[A-Z][A-Za-z0-9]*")]
        //    [StringLength(32)]
        //    public string Xyz { get; set; }
        internal sealed class UserReportFieldMetadata
        {

            // Metadata classes are not meant to be instantiated.
            private UserReportFieldMetadata()
            {
            }

            public int ColumnOrder { get; set; }

            public string CoverageCode { get; set; }

            public int CreatedByUserID { get; set; }

            public DateTime CreatedDate { get; set; }

            public string CustomName { get; set; }

            public int GroupOrder { get; set; }

            public bool IncludePageBreak { get; set; }

            public ReportField ReportField { get; set; }

            public string SortDirection { get; set; }

            public int SortOrder { get; set; }

            public UserReport UserReport { get; set; }

            public int UserReportFieldID { get; set; }

            public EntityCollection<UserReportSummarizeField> UserReportSummarizeField { get; set; }
        }
    }

    // The MetadataTypeAttribute identifies UserReportOutputMetadata as the class
    // that carries additional metadata for the UserReportOutput class.
    [MetadataTypeAttribute(typeof(UserReportOutput.UserReportOutputMetadata))]
    public partial class UserReportOutput
    {

        // This class allows you to attach custom attributes to properties
        // of the UserReportOutput class.
        //
        // For example, the following marks the Xyz property as a
        // required property and specifies the format for valid values:
        //    [Required]
        //    [RegularExpression("[A-Z][A-Za-z0-9]*")]
        //    [StringLength(32)]
        //    public string Xyz { get; set; }
        internal sealed class UserReportOutputMetadata
        {

            // Metadata classes are not meant to be instantiated.
            private UserReportOutputMetadata()
            {
            }

            public string ClientNumber { get; set; }

            public int CreatedByUserID { get; set; }

            public DateTime CreatedDate { get; set; }

            public string FileName { get; set; }

            public EntityCollection<ReportDeliveryLog> ReportDeliveryLog { get; set; }

            public DateTime ReportRunDate { get; set; }

            public int UserID { get; set; }

            public UserReport UserReport { get; set; }

            public int UserReportID { get; set; }

            public int UserReportOutputID { get; set; }
        }
    }

    // The MetadataTypeAttribute identifies UserReportParameterMetadata as the class
    // that carries additional metadata for the UserReportParameter class.
    [MetadataTypeAttribute(typeof(UserReportParameter.UserReportParameterMetadata))]
    public partial class UserReportParameter
    {

        // This class allows you to attach custom attributes to properties
        // of the UserReportParameter class.
        //
        // For example, the following marks the Xyz property as a
        // required property and specifies the format for valid values:
        //    [Required]
        //    [RegularExpression("[A-Z][A-Za-z0-9]*")]
        //    [StringLength(32)]
        //    public string Xyz { get; set; }
        internal sealed class UserReportParameterMetadata
        {

            // Metadata classes are not meant to be instantiated.
            private UserReportParameterMetadata()
            {
            }

            public int CreatedByUserID { get; set; }

            public DateTime CreatedDate { get; set; }

            public string FilterString { get; set; }

            public string ParameterValue { get; set; }

            public ReportParameter ReportParameter { get; set; }

            public int ReportParameterID { get; set; }

            public UserReport UserReport { get; set; }

            public int UserReportID { get; set; }

            public int UserReportParameterID { get; set; }
        }
    }

    // The MetadataTypeAttribute identifies UserReportSummarizeFieldMetadata as the class
    // that carries additional metadata for the UserReportSummarizeField class.
    [MetadataTypeAttribute(typeof(UserReportSummarizeField.UserReportSummarizeFieldMetadata))]
    public partial class UserReportSummarizeField
    {

        // This class allows you to attach custom attributes to properties
        // of the UserReportSummarizeField class.
        //
        // For example, the following marks the Xyz property as a
        // required property and specifies the format for valid values:
        //    [Required]
        //    [RegularExpression("[A-Z][A-Za-z0-9]*")]
        //    [StringLength(32)]
        //    public string Xyz { get; set; }
        internal sealed class UserReportSummarizeFieldMetadata
        {

            // Metadata classes are not meant to be instantiated.
            private UserReportSummarizeFieldMetadata()
            {
            }

            public int CreatedByUserID { get; set; }

            public DateTime CreatedDate { get; set; }

            public UserReport UserReport { get; set; }

            public UserReportField UserReportField { get; set; }

            public int UserReportFieldID { get; set; }

            public int UserReportID { get; set; }

            public int UserReportSummarizeFieldID { get; set; }
        }
    }
}
