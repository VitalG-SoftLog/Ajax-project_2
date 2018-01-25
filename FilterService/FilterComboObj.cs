using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReportBuilderAjax.Web.Services
{
    public class FilterComboObj
    {
        public int MaxRecordCount { get; set; }
        public int CountWithoutTruncate { get; set; }
        public string FilterValue { get; set; }
        public bool IsTruncated { get; set; }
        public List<Members> MemberList { get; set; }
        public List<Location> LocationList { get; set; }
        public List<GroupCode> GroupCodeList { get; set; }
        public List<Coverage> CoverageList { get; set; }
        public List<PrimaryCarrier> PrimaryCarrierList { get; set; }
        public List<IssuingCarrier> IssuingCarrierList { get; set; }
        public List<Venue> VenueList { get; set; }
        public List<DefenseFirm> DefenseFirmList { get; set; }
        public List<DefenseAttorney> DefenseAttorneyList { get; set; }
        public List<PlaintiffFirm> PlaintiffFirmList { get; set; }
        public List<PlaintiffAttorney> PlaintiffAttorneyList { get; set; }
        public List<Region> RegionList { get; set; }
        public List<Office> OfficeList { get; set; }
        public List<StateOfJurisdiction> StateOfJurisdictionsList { get; set; }
        public List<ClientAnalysis> ClientAnalysesList { get; set; }
        public List<Adjuster> AdjusterList { get; set; }
        public List<HandlingOffice> HandlingOfficeList { get; set; }
        public List<RRENumber> RRENumberList { get; set; }
        public List<BankAccount> BankAccountList { get; set; }
        public List<PayeeFeinTax> PayeeFeinTaxList { get; set; }
        //public List<ClientSpecific> ClientSpecificList { get; set; }
        public List<MultiPeriod> MultiPeriodList { get; set; }
    }
}
