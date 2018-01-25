var LossRunSummaryFilter = function (id, parent) {
    this.Element = document.getElementById(id);
    this.ParentObject = parent;
    this.Type = Consts.FILTER_TYPE.LOSS_RUN_SUMMARY;
    this.Data = [];
    this.HierarchyLabels = [];
    this.HierarchyControl = null;
    this.Controls = new Array();
    this.TestDrop = new Array();
    this.FilterDataManager = new FilterDataManager();
    this.ActivityControl = null;
};

ClassInheritance.Standart(LossRunSummaryFilter, FilterBase);

LossRunSummaryFilter.prototype.Bind = function () {
    var parentObj = this;
    this.FilterDataManager.GetCoveragesList(function (data) {

        var coverageElement = document.getElementById('CoverageDropDown');

        var newData = new Array();
        newData[newData.length] = { Value: "", Name: 'ALL' };

        for (var i in data.ResultList) {
            var item = { Value: data.ResultList[i].Value, Name: data.ResultList[i].Description };
            newData.push(item);
        }

        var dataValues = new Array();
        dataValues.push({ Value: "", Name: 'ALL' });

        parentObj.Controls.CoverageControl = new CustomDropDown(coverageElement, this, newData, 203, data.IsTruncated, data.FilterValue); // container, this object, data elements, width container, isTruncated
        parentObj.Controls.CoverageControl.Set_Value(dataValues);
        parentObj.fillControlData("coverages", parentObj.Controls.CoverageControl);

        parentObj.Controls.CoverageControl.OnChange = function () {
            parentObj.ChangeCoverages(this);
        };
    });

    this.FilterDataManager.GetStateOfJurisdiction(false, function (data) {

        var stateOfJurisdictionElement = document.getElementById('StateOfJurisdictionDropDown');

        var newData = new Array();
        newData[newData.length] = { Value: "", Name: 'ALL' };

        for (var i in data.ResultList) {
            var item = { Value: data.ResultList[i].Value, Name: data.ResultList[i].Description };
            newData.push(item);
        }

        var dataValues = new Array();
        dataValues.push({ Value: "", Name: "ALL" });

        parentObj.Controls.StateOfJurisdictionControl = new CustomDropDown(stateOfJurisdictionElement, this, newData, 130, data.IsTruncated, data.filterValue); // container, this object, data elements, width container, isTruncated
        parentObj.Controls.StateOfJurisdictionControl.Set_Value(dataValues);
        parentObj.fillStateOfJurisdiction(true);
    });
    this.GetClasses("");

    this.CreateControls();
    this.BindControls();
    this.FillData();
};

LossRunSummaryFilter.prototype.GetClasses = function (coverages) {
    var parentObj = this;
    this.FilterDataManager.GetClasses(coverages, function (data) {
        var paimentClassesElement = document.getElementById('PaymantClassDropDown');

        var newData = new Array();
        newData[newData.length] = { Value: "", Name: 'ALL' };

        for (var i in data.ResultList) {
            var item = { Value: data.ResultList[i].Value, Name: data.ResultList[i].Value };
            newData.push(item);
        }

        var dataValues = new Array();
        dataValues.push({ Value: "", Name: "ALL" });
        if (parentObj.Controls.PaymantClassControl == null) {
            parentObj.Controls.PaymantClassControl = new CustomDropDown(paimentClassesElement, this, newData, 203, data.IsTruncated, data.filterValue); // container, this object, data elements, width container, isTruncated
            parentObj.Controls.PaymantClassControl.Set_Value(dataValues);
            parentObj.fillControlData("classes", parentObj.Controls.PaymantClassControl);
        }
        else {
            parentObj.Controls.PaymantClassControl.ReBind(newData);
        }
    });
};

LossRunSummaryFilter.prototype.ChangeCoverages = function (control) {

    var isDefaultClaimType = false;
    var isOnlyDefault = true;

    var isAll = false;

    $('div[class="checkboxChecked"]', control.OptionsElement).each(function () {
        var elementId = this.getAttribute('elementId');

        if (elementId == '') {
            isAll = true;
        }

        if (elementId != '') {
            switch (elementId) {
                case "EJA":
                case "XWC":
                case "JA":
                case "NSWC":
                case "USLH":
                case "WC":
                case "WCEL":
                    isDefaultClaimType = true;
                    break;
                default:
                    isOnlyDefault = false;
                    break;
            }
        }
    });

    if (!isAll && control.Input.value != '' && isDefaultClaimType && isOnlyDefault) {
        this.Controls.IndemnityCheckBoxControl.SetValue(1);
        this.Controls.MedicalCheckBoxControl.SetValue(1);
        this.Controls.IncedentCheckBoxControl.SetValue(0);
    }
    else {
        this.Controls.IndemnityCheckBoxControl.SetValue(1);
        this.Controls.MedicalCheckBoxControl.SetValue(1);
        this.Controls.IncedentCheckBoxControl.SetValue(1);
    }

    this.GetClasses(this.Controls.CoverageControl.Get_Value());
};

LossRunSummaryFilter.prototype.reportLayoutStylesChanged = function(isCustom) {
    if (isCustom) {
        $('#standartCheckBoxes').css('display', 'none');
    }
    else {
        $('#standartCheckBoxes').css('display', '');
    }
};

LossRunSummaryFilter.prototype.CreateControls = function () {
    var attributes = {
        "HierarchyControlContainerId": this.getClientID("HierarchyControlContainer")
    };
    var template = TemplateEngine.Format(TemplateManager.templates['LossRunSummaryFilterTemplate_LossRunSummaryFilterRoot'], attributes);
    $(this.Element).append(template);
};

LossRunSummaryFilter.prototype.BindControls = function () {
    var parentObj = this;

    var attr = {
        'MultiRadioBtn': this.getClientID("MultiRadioBtn"),
        'MultiRadioBtnClaimDenied': this.getClientID("MultiRadioBtnClaimDenied"),
        'dateRangeContainer': this.getClientID("dateRangeContainer"),
        'asOfDateRangeContainer': this.getClientID("asOfDateRangeContainer"),
        'PageBreakRadioBttn': this.getClientID("PageBreakRadioBttn"),
        'IncludePageTitleRadioBttn': this.getClientID("IncludePageTitleRadioBttn"),
        'IncludeRPORadio': this.getClientID("IncludeRPORadio")
    };
    $(this.Element).append(TemplateEngine.Format(TemplateManager.templates['LossRunSummaryFilterTemplate_Main'], attr));

    //End Report Layout Style dropdown menu

    this.Controls.DateRange = new DateRangeControl(this.getClientID("dateRangeContainer"), this.ParentObject.ReportId);
    this.Controls.DateRange.Data = this.Data;
    this.Controls.DateRange.Bind(TemplateManager.templates['ReportBuilderForm_DateRangeTemplate']);

    this.Controls.AsOfDate = new AsOfDateControl(this.getClientID("asOfDateRangeContainer"), this.ParentObject.ReportId);
    this.Controls.AsOfDate.Data = this.Data;
    this.Controls.AsOfDate.Bind(TemplateManager.templates['ReportBuilderForm_AsOfDateRangeTemplate']);

    this.Controls.DateOfLossRadioBtnControl = new MultiRadioButton(this.getClientID("MultiRadioBtn"), TemplateManager.templates['ReportBuilderForm_MultiRadio']);
    this.Controls.DateOfLossRadioBtnControl.Bind();
    this.Controls.DateOfLossRadioBtnControl.SetValue(1);

    this.Controls.TurnOffPageBreakControl = new MultiRadioButton(this.getClientID("PageBreakRadioBttn"), TemplateManager.templates['ReportBuilderForm_TurnOffPageBreakMultiRadio']);
    this.Controls.TurnOffPageBreakControl.Bind();
    this.Controls.TurnOffPageBreakControl.SetValue(1);

    this.Controls.IncludePageTitleControl = new MultiRadioButton(this.getClientID("IncludePageTitleRadioBttn"), TemplateManager.templates['ReportBuilderForm_IncludePageTitleMultiRadio']);
    this.Controls.IncludePageTitleControl.Bind();
    this.Controls.IncludePageTitleControl.SetValue(1);

    this.Controls.ClaimDeniedRadioBtnControl = new MultiRadioButton(this.getClientID("MultiRadioBtnClaimDenied"), TemplateManager.templates['ReportBuilderForm_MultiRadioHorizontal']);
    this.Controls.ClaimDeniedRadioBtnControl.Bind();
    this.Controls.ClaimDeniedRadioBtnControl.SetValue(3);

    this.Controls.IndemnityCheckBoxControl = new newCheckBoxControl(this, "IdemnityCheckBox", "", 1);
    this.Controls.IndemnityCheckBoxControl.DataBind();

    this.Controls.MedicalCheckBoxControl = new newCheckBoxControl(this, "MedicalCheckBox", "", 1);
    this.Controls.MedicalCheckBoxControl.DataBind();

    this.Controls.IncedentCheckBoxControl = new newCheckBoxControl(this, "IncedentCheckBox", "", 1);
    this.Controls.IncedentCheckBoxControl.DataBind();

    this.Controls.OpenCheckBoxControl = new newCheckBoxControl(this, "OpenCheckBox", "", 1);
    this.Controls.OpenCheckBoxControl.DataBind();

    this.Controls.ClosedCheckBoxControl = new newCheckBoxControl(this, "ClosedCheckBox", "", 1);
    this.Controls.ClosedCheckBoxControl.DataBind();

    this.Controls.PendingCheckBoxControl = new newCheckBoxControl(this, "PendingCheckBox", "", 1);
    this.Controls.PendingCheckBoxControl.DataBind();
    //-
    this.Controls.ShowOpenClaimsCheckBoxControl = new newCheckBoxControl(this, "ShowOpenClaimsCheckBox", "", 1);
    this.Controls.ShowOpenClaimsCheckBoxControl.DataBind();

    this.Controls.ShowClosedClaimsCheckBoxControl = new newCheckBoxControl(this, "ShowClosedClaimsCheckBox", "", 1);
    this.Controls.ShowClosedClaimsCheckBoxControl.DataBind();

    this.Controls.ShowNewClaimsCheckBoxControl = new newCheckBoxControl(this, "ShowNewClaimsCheckBox", "", 1);
    this.Controls.ShowNewClaimsCheckBoxControl.DataBind();

    this.Controls.IncludeOneLineClaimDetailCheckBoxControl = new newCheckBoxControl(this, "IncludeOneLineClaimDetailCheckBox", "", 1);
    this.Controls.IncludeOneLineClaimDetailCheckBoxControl.DataBind();

    this.Controls.TotalPaidCheckBoxControl = new newCheckBoxControl(this, "TotalPaidCheckBox", "", 1);
    this.Controls.TotalPaidCheckBoxControl.DataBind();

    this.Controls.OutstandingReserveCheckBoxControl = new newCheckBoxControl(this, "OutstandingReserveCheckBox", "", 1);
    this.Controls.OutstandingReserveCheckBoxControl.DataBind();

    this.Controls.TotalIncurredCheckBoxControl = new newCheckBoxControl(this, "TotalIncurredCheckBox", "", 1);
    this.Controls.TotalIncurredCheckBoxControl.DataBind();
    //-
    this.Controls.PageBreakControl = new RadioButtonSetControl(this, "PageBreakBttn", {}, TemplateManager.templates['ReportBuilderForm_YesNoButtonTemplate']);
    this.Controls.PageBreakControl.FindButtons();

    this.Controls.PageTitleControl = new RadioButtonSetControl(this, "PageTitleBttn", {}, TemplateManager.templates['ReportBuilderForm_YesNoButtonTemplate']);
    this.Controls.PageTitleControl.FindButtons();

    this.Controls.IncludeRPORadioControl = new MultiRadioButton(this.getClientID("IncludeRPORadio"), TemplateManager.templates['ReportBuilderForm_IncludeRPORadio']);
    this.Controls.IncludeRPORadioControl.Bind();
    this.Controls.IncludeRPORadioControl.SetValue(1);

    //this.Controls.DateRange.Controls.DataRangeDropDown.SetDropDownValue(Consts.DATE_RANGE.LAST_MONTH);
    //this.Controls.DateRange.Controls.LastXDays.SetValue(7);
    //this.Controls.AsOfDate.Controls.AsOfDateTypeDropDown.SetDropDownValue(Consts.AS_OF_DATE_TYPE.LAST_DAY_OF_LAST_MONTH);
    this.Controls.IndemnityCheckBoxControl.SetValue(1);
    this.Controls.MedicalCheckBoxControl.SetValue(1);
    this.Controls.IncedentCheckBoxControl.SetValue(1);
    this.Controls.OpenCheckBoxControl.SetValue(1);
    this.Controls.ClosedCheckBoxControl.SetValue(1);
    this.Controls.PendingCheckBoxControl.SetValue(0);
    this.Controls.ShowClosedClaimsCheckBoxControl.SetValue(0);
    this.Controls.ShowOpenClaimsCheckBoxControl.SetValue(0);
    this.Controls.ShowNewClaimsCheckBoxControl.SetValue(0);
    this.Controls.IncludeOneLineClaimDetailCheckBoxControl.SetValue(1);
    this.Controls.TotalPaidCheckBoxControl.SetValue(0);
    this.Controls.OutstandingReserveCheckBoxControl.SetValue(0);
    this.Controls.TotalIncurredCheckBoxControl.SetValue(0);

    this.ActivityControl = new ActivityControl('activities', this, '', null, null);
    this.HierarchyControl = new HierarchyControl(this, 'HierarchyContainer', this.HierarchyLabels, this.FilterDataManager, '');

    if (this.ParentObject.Data.IsNew) {
        this.HierarchyControl.Render();
        this.ActivityControl.Render();
    }


    this.Controls.ClientAnalysisControl = new ClientAnalysisControl(this, 'ClientAnalysisContainer', this.HierarchyLabels, this.FilterDataManager);
    this.Controls.ClientAnalysisControl.Data = this.Data;
    this.Controls.ClientAnalysisControl.Render();

    //Start Clear Filter button
    var clearFilterBttnAttr = {
        "idClearFilterBttn": this.getClientID("clearFilter")
    };

    var ClearFilterBttnTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_ClearFilterBttnTemplate'], clearFilterBttnAttr);
    $(document.getElementById("ClearFilterBttn")).append(ClearFilterBttnTempl);

    var clearFilterButton = document.getElementById(this.getClientID("clearFilter"));

    var parentObj = this;

    $(clearFilterButton).bind('click', function () {
        parentObj.ClearFilter();
    });

    //End Clear Filter button

    //Start View Report button
    var viewReportBttnAttr = {
        "idViewReportBttn": this.getClientID("viewReport")
    };

    var ViewReportBttnTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_ViewReportBttnTemplate'], viewReportBttnAttr);
    $(document.getElementById("ViewReportBttn")).append(ViewReportBttnTempl);

    $(document.getElementById("ViewReportBttn")).click(function () {
        parentObj.ParentObject.ReportBuilderTabs.inactive_tabs();
        parentObj.ParentObject.ReportBuilderTabs.active_tab(2);


        if (document.getElementById("SlideLeftBttn").style.display != 'none')
            parentObj.ParentObject.HideLeft();

        if (parentObj.ParentObject.ReportBuilderTabs.Tabs[2] != null) {
            parentObj.ParentObject.ReportBuilderTabs.Tabs[2].refreshBtnClick(false);
        }


        parentObj.ParentObject.ReportBuilderTabs.CurrentTab = 2;
        parentObj.ParentObject.ReportBuilderTabs.BindTab(2);

    });

    //    var viewReportButton = document.getElementById(this.getClientID("viewReport"));

    //End View Report button

    //Start Show All button
    var showAllBttnAttr = {
        "idShowAllBttn": this.getClientID("showAll")
    };

    var showAllBttnTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_ShowAllBttnTemplate'], showAllBttnAttr);
    $(document.getElementById("ShowAllBttn")).append(showAllBttnTempl);
    //End Show All button

    //Start Show Covered button
    var showCoveredBttnAttr = {
        "idShowCoveredBttn": this.getClientID("showCovered")
    };

    var showCoveredBttnTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_ShowCoveredBttnTemplate'], showCoveredBttnAttr);
    $(document.getElementById("ShowCoveredBttn")).append(showCoveredBttnTempl);
    //End Show Covered button
    $(document.getElementById("ShowCoveredBttn")).click(function () {
        $(this).css('display', 'none');

        parentObj.FilterDataManager.GetStateOfJurisdiction(false, function (data) {

            var stateOfJurisdictionElement = document.getElementById('StateOfJurisdictionDropDown');

            var newData = new Array();
            newData[newData.length] = { Value: "", Name: 'ALL' };

            for (var i in data.ResultList) {
                var item = { Value: data.ResultList[i].Value, Name: data.ResultList[i].Description };
                newData.push(item);
            }

            var dataValues = new Array();
            dataValues.push({ Value: "", Name: "ALL" });

            parentObj.Controls.StateOfJurisdictionControl = new CustomDropDown(stateOfJurisdictionElement, this, newData, 110, data.IsTruncated, data.filterValue); // container, this object, data elements, width container, isTruncated
            parentObj.Controls.StateOfJurisdictionControl.Set_Value(dataValues);
            parentObj.fillStateOfJurisdiction(true);

            $(document.getElementById("ShowAllBttn")).css('display', '');
        });
    });


    $(document.getElementById("ShowAllBttn")).click(function () {
        $(this).css('display', 'none');

        parentObj.FilterDataManager.GetStateOfJurisdiction(true, function (data) {

            var stateOfJurisdictionElement = document.getElementById('StateOfJurisdictionDropDown');

            var newData = new Array();
            newData[newData.length] = { Value: "", Name: 'ALL' };

            for (var i in data.ResultList) {
                var item = { Value: data.ResultList[i].Value, Name: data.ResultList[i].Description };
                newData.push(item);
            }

            var dataValues = new Array();
            dataValues.push({ Value: "", Name: "ALL" });

            parentObj.Controls.StateOfJurisdictionControl = new CustomDropDown(stateOfJurisdictionElement, this, newData, 110, data.IsTruncated, data.filterValue); // container, this object, data elements, width container, isTruncated
            parentObj.Controls.StateOfJurisdictionControl.Set_Value(dataValues);
            parentObj.fillStateOfJurisdiction(false);

            $(document.getElementById("ShowCoveredBttn")).css('display', '');
        });
    });

    var saveBasicFilterHeight = '';
    $(document.getElementById("CollapseBasicFilterBtn")).click(function () {
        if (document.getElementById("BasicFilterContainer").style.height >= "1px") {
            saveBasicFilterHeight = ($(document.getElementById("BasicFilterContainer")).height()).toString();
            $(document.getElementById("BasicFilterContainer")).animate({ height: "0px" }, "fast", function () {
                document.getElementById("CollapseBasicFilterBtn").style.backgroundImage = "url(./images/BasicFilterArreyUp.png)";
                RightTabHeight();
            });
        } else {
            $(document.getElementById("BasicFilterContainer")).animate({ height: saveBasicFilterHeight + 'px' }, "fast", function () {
                document.getElementById("BasicFilterContainer").style.height = "auto";
                document.getElementById("CollapseBasicFilterBtn").style.backgroundImage = "url(./images/BasicFilterArreyDown.png)";
                RightTabHeight();
            });
        }
    });

    var saveAdvancedFilterHeight = '';
    $(document.getElementById("CollapseAdvancedFilterBtn")).click(function () {
        if (document.getElementById("AdvancedFilterContainer").style.display == 'none') {
            saveAdvancedFilterHeight = ($(document.getElementById("AdvancedFilterContainer")).height()).toString();
            $(document.getElementById("AdvancedFilterContainer")).animate({ height: "0px" }, 0, function () {
                document.getElementById("AdvancedFilter").style.paddingBottom = "0px";
                document.getElementById("AdvancedFilterContainer").style.display = '';
                if (document.getElementById("AdvancedFilterContainer").style.height >= "1px") {
                    saveAdvancedFilterHeight = ($(document.getElementById("AdvancedFilterContainer")).height()).toString();
                    $(document.getElementById("AdvancedFilterContainer")).animate({ height: "0px" }, "fast", function () {
                        document.getElementById("CollapseAdvancedFilterBtn").style.backgroundImage = "url(./images/BasicFilterArreyUp.png)";
                        document.getElementById("AdvancedFilter").style.paddingBottom = "3px";
                        RightTabHeight();
                    });
                } else {
                    document.getElementById("AdvancedFilter").style.paddingBottom = "0px";
                    $(document.getElementById("AdvancedFilterContainer")).animate({ height: saveAdvancedFilterHeight + 'px' }, "fast", function () {
                        document.getElementById("AdvancedFilterContainer").style.height = "auto";
                        document.getElementById("CollapseAdvancedFilterBtn").style.backgroundImage = "url(./images/BasicFilterArreyDown.png)";
                        RightTabHeight();
                    });
                }
            });
        } else {
            if (document.getElementById("AdvancedFilterContainer").style.height >= "1px") {
                saveAdvancedFilterHeight = ($(document.getElementById("AdvancedFilterContainer")).height()).toString();
                $(document.getElementById("AdvancedFilterContainer")).animate({ height: "0px" }, "fast", function () {
                    document.getElementById("CollapseAdvancedFilterBtn").style.backgroundImage = "url(./images/BasicFilterArreyUp.png)";
                    document.getElementById("AdvancedFilter").style.paddingBottom = "3px";
                    RightTabHeight();
                });
            } else {
                document.getElementById("AdvancedFilter").style.paddingBottom = "0px";
                $(document.getElementById("AdvancedFilterContainer")).animate({ height: saveAdvancedFilterHeight + 'px' }, "fast", function () {
                    document.getElementById("AdvancedFilterContainer").style.height = "auto";
                    document.getElementById("CollapseAdvancedFilterBtn").style.backgroundImage = "url(./images/BasicFilterArreyDown.png)";
                    RightTabHeight();
                });
            }
        }
    });

};

LossRunSummaryFilter.prototype.ClearFilter = function () {
    this.Controls.DateRange.Controls.DataRangeDropDown.SetDropDownValue(Consts.DATE_RANGE.LAST_MONTH);
    this.Controls.DateRange.Controls.LastXDays.SetValue(7);
    this.Controls.AsOfDate.Controls.AsOfDateTypeDropDown.SetDropDownValue(Consts.AS_OF_DATE_TYPE.LAST_DAY_OF_LAST_MONTH);
    this.Controls.DateOfLossRadioBtnControl.SetValue(1);
    this.Controls.IndemnityCheckBoxControl.SetValue(1);
    this.Controls.MedicalCheckBoxControl.SetValue(1);
    this.Controls.IncedentCheckBoxControl.SetValue(1);
    this.Controls.OpenCheckBoxControl.SetValue(1);
    this.Controls.ClosedCheckBoxControl.SetValue(1);
    this.Controls.PendingCheckBoxControl.SetValue(0);
    this.Controls.IncludeRPORadioControl.SetValue(1);
    this.Controls.ShowClosedClaimsCheckBoxControl.SetValue(0);
    this.Controls.ShowOpenClaimsCheckBoxControl.SetValue(0);
    this.Controls.ShowNewClaimsCheckBoxControl.SetValue(0);
    this.Controls.IncludeOneLineClaimDetailCheckBoxControl.SetValue(1);
    this.Controls.TotalPaidCheckBoxControl.SetValue(0);
    this.Controls.OutstandingReserveCheckBoxControl.SetValue(0);
    this.Controls.TotalIncurredCheckBoxControl.SetValue(0);
    this.Controls.CoverageControl.Set_Value(null);
    this.HierarchyControl.ClearHierarchy();
    this.Controls.ClientAnalysisControl.ClearControl();
    this.ActivityControl.ClearFilter();
    this.Controls.ClaimDeniedRadioBtnControl.SetValue(3);

    this.Controls.TurnOffPageBreakControl.SetValue(1);
    this.Controls.IncludePageTitleControl.SetValue(1);
};

LossRunSummaryFilter.prototype.validateInteger = function (e) {
    var keyCode = e.keyCode;
    if ((keyCode >= 48 && keyCode <= 57) || (keyCode >= 35 && keyCode <= 40) || keyCode == 8 || keyCode == 46 || (keyCode >= 96 && keyCode <= 105) || keyCode == 144 || keyCode == 20) return;
    var str = this.value;
    var re = /[^0-9]/gi;
    this.value = str.replace(re, "");
};

LossRunSummaryFilter.prototype.fillControlData = function (name, control) {
    var item = null;
    for (var key in this.Data) {
        if (this.Data[key].FilterName.toString().toLowerCase() == name) {
            item = this.Data[key];
        }
    }
    if (item == null || control == null) return;
    var dataValues = new Array();
    var values = item.Value.split(',');
    for (var i in values) {
        dataValues.push({ Value: values[i], Name: '' });
    }

    if (dataValues.length > 0) {
        control.Set_Value(dataValues);
    }
};

LossRunSummaryFilter.prototype.fillStateOfJurisdiction = function (isAll) {
    var item = this.getDataItemByName("stateofjurisdiction");
    if (item == null) return;
    var str = item.Value.toString().substr(0, 2);
    if (str == "c;" && document.getElementById("ShowAllBttn").style.display == "" && isAll) {
        $(document.getElementById("ShowAllBttn")).trigger("click");
        return;
    }

    if (this.Controls.StateOfJurisdictionControl == null) return;

    str = item.Value.toString().substr(2, item.Value.toString().length - 2);
    var dataValues = new Array();
    var values = str.split(',');
    for (var i in values) {
        dataValues.push({ Value: values[i], Name: '' });
    }

    if (dataValues.length > 0) {
        this.Controls.StateOfJurisdictionControl.Set_Value(dataValues);
    }
};

LossRunSummaryFilter.prototype.getDataItemByName = function(name) {
    for (var key1 in this.Data) {
        if (this.Data[key1].FilterName.toString().toLowerCase() == name) {
            return this.Data[key1];
        }
    }
    return null;
};

LossRunSummaryFilter.prototype.FillData = function () {

    this.Controls.TurnOffPageBreakControl.SetValue(this.ParentObject.Data.IsTurnOffPageBreak ? 1 : 2);
    this.Controls.IncludePageTitleControl.SetValue(this.ParentObject.Data.IncludeTitlePage ? 1 : 2);
    
    for (var key in this.Data) {
        var item = this.Data[key];
        if (!item || item.Value == null) continue;
        switch (item.FilterName.toString().toLowerCase()) {
            case "asofdate":
                this.Controls.AsOfDate.Controls.AsOfDateControl.SetValue(returnShortDate(item.Value.toString()));
                break;
            case "asofdatetype":
                this.Controls.AsOfDate.Controls.AsOfDateTypeDropDown.SetDropDownValue(item.Value * 1);
                var asOfDateItem = this.getDataItemByName("asofdate");
                if (asOfDateItem != null && item.Value * 1 == Consts.AS_OF_DATE_TYPE.CUSTOM) {
                    this.Controls.AsOfDate.Controls.AsOfDateControl.SetValue(returnShortDate(asOfDateItem.Value.toString()));
                }
                break;
            case "claimtypes":
                this.Controls.IndemnityCheckBoxControl.SetValue(0);
                this.Controls.MedicalCheckBoxControl.SetValue(0);
                this.Controls.IncedentCheckBoxControl.SetValue(0);
                if (item.Value.toString() != "") {
                    var claimTypeValues = item.Value.toString().split(",");
                    for (var ctkey in claimTypeValues) {
                        switch (claimTypeValues[ctkey]) {
                            case "Ind": this.Controls.IndemnityCheckBoxControl.SetValue(1); break;
                            case "Med": this.Controls.MedicalCheckBoxControl.SetValue(1); break;
                            case "Inc": this.Controls.IncedentCheckBoxControl.SetValue(1); break;
                        }
                    }
                }
                break;
            case "claimstatuses":
                this.Controls.OpenCheckBoxControl.SetValue(0);
                this.Controls.ClosedCheckBoxControl.SetValue(0);
                this.Controls.PendingCheckBoxControl.SetValue(0);
                if (item.Value.toString() != "") {
                    var claimStatusValues = item.Value.toString().split(",");
                    for (var cskey in claimStatusValues) {
                        switch (claimStatusValues[cskey]) {
                            case "Closed": this.Controls.ClosedCheckBoxControl.SetValue(1); break;
                            case "Open": this.Controls.OpenCheckBoxControl.SetValue(1); break;
                            case "Pending": this.Controls.PendingCheckBoxControl.SetValue(1); break;
                        }
                    }
                }
                break;
            case "totalincurred":
                /*decimal totalIncurred = 0;
                decimal.TryParse(item.Value.ToString(), out totalIncurred);
                TotalIncurred = totalIncurred;*/
                break;
            case "totalincurredoperand":
                //TotalIncurredOperand = item.Value.ToString();
                break;
            case "totalrange":
                //TotalRange = item.Value.ToString();
                break;
            case "activity":
                this.ActivityControl.Data = item.Value;
                this.ActivityControl.Render();
                break;
            case "hierarchy":
                this.HierarchyControl.Data = item.Value.toString();
                this.HierarchyControl.FillHierarchy();
                break;
            case "showopenclaimcount":
                this.Controls.ShowOpenClaimsCheckBoxControl.SetValue(item.Value.toString().toLocaleLowerCase() == "true" ? 1 : 0);
                break;
            case "showclosedclaimcount":
                this.Controls.ShowClosedClaimsCheckBoxControl.SetValue(item.Value.toString().toLocaleLowerCase() == "true" ? 1 : 0);
                break;
            case "shownewclaimcount":
                this.Controls.ShowNewClaimsCheckBoxControl.SetValue(item.Value.toString().toLocaleLowerCase() == "true" ? 1 : 0);
                break;
            case "includeonelineclaimdetail":
                this.Controls.IncludeOneLineClaimDetailCheckBoxControl.SetValue(item.Value.toString().toLocaleLowerCase() == "true" ? 1 : 0);
                break;
            case "showpaidtotalbyclass":
                this.Controls.TotalPaidCheckBoxControl.SetValue(item.Value.toString().toLocaleLowerCase() == "true" ? 1 : 0);
                break;
            case "showreservedtotalbyclass":
                this.Controls.OutstandingReserveCheckBoxControl.SetValue(item.Value.toString().toLocaleLowerCase() == "true" ? 1 : 0);
                break;
            case "showincurredtotalbyclass":
                this.Controls.TotalIncurredCheckBoxControl.SetValue(item.Value.toString().toLocaleLowerCase() == "true" ? 1 : 0);
                break;
            case "datetype":
                this.Controls.DateOfLossRadioBtnControl.SetValue(item.Value.toString() * 1);
                break;
            case "claimdenied":
                this.Controls.ClaimDeniedRadioBtnControl.SetValue(item.Value.toString() * 1);
                break;
            case "coverages":
                this.fillControlData("coverages", this.Controls.CoverageControl);
                break;
            case "stateofjurisdiction":
                this.fillStateOfJurisdiction();
                break;
            case "classes":
                this.fillControlData("classes", this.Controls.PaymantClassControl);
                break;
            case "specialanalysis1":
                if (this.Controls.ClientAnalysisControl.Controls.SpecialAnalysis1DropDown) {
                    this.Controls.ClientAnalysisControl.Controls.SpecialAnalysis1DropDown.Set_ValueMode(item.Value.toString());
                }
                break;
            case "specialanalysis2":
                if (this.Controls.ClientAnalysisControl.Controls.SpecialAnalysis1DropDown) {
                    this.Controls.ClientAnalysisControl.Controls.SpecialAnalysis2DropDown.Set_ValueMode(item.Value.toString());
                }
                break;
            case "specialanalysis3":
                if (this.Controls.ClientAnalysisControl.Controls.SpecialAnalysis1DropDown) {
                    this.Controls.ClientAnalysisControl.Controls.SpecialAnalysis3DropDown.Set_ValueMode(item.Value.toString());
                }
                break;
            case "specialanalysis4":
                if (this.Controls.ClientAnalysisControl.Controls.SpecialAnalysis1DropDown) {
                    this.Controls.ClientAnalysisControl.Controls.SpecialAnalysis4DropDown.Set_ValueMode(item.Value.toString());
                }
                break;
            case "specialanalysis5":
                if (this.Controls.ClientAnalysisControl.Controls.SpecialAnalysis1DropDown) {
                    this.Controls.ClientAnalysisControl.Controls.SpecialAnalysis5DropDown.Set_ValueMode(item.Value.toString());
                }
                break;
            case "includerpo":
                if (item.Value.toString() == "False") {
                    this.Controls.IncludeRPORadioControl.SetValue(2);
                }
                else {
                    this.Controls.IncludeRPORadioControl.SetValue(1);
                }
                break;
        }
    }
};

LossRunSummaryFilter.prototype.getClaimTypes = function () {
    var resultString = "";
    if (this.Controls.IndemnityCheckBoxControl.GetStringValue() == "true") {
        resultString = "Ind";
    }
    if (this.Controls.MedicalCheckBoxControl.GetStringValue() == "true") {
        resultString = resultString + (resultString == "" ? "" : ",") + "Med";
    }
    if (this.Controls.IncedentCheckBoxControl.GetStringValue() == "true") {
        resultString = resultString + (resultString == "" ? "" : ",") + "Inc";
    }
    return resultString;
};

LossRunSummaryFilter.prototype.getClaimStatuses = function () {
    var resultString = "";
    if (this.Controls.ClosedCheckBoxControl.GetStringValue() == "true") {
        resultString = "Closed";
    }
    if (this.Controls.OpenCheckBoxControl.GetStringValue() == "true") {
        resultString = resultString + (resultString == "" ? "" : ",") + "Open";
    }
    if (this.Controls.PendingCheckBoxControl.GetStringValue() == "true") {
        resultString = resultString + (resultString == "" ? "" : ",") + "Pending";
    }
    return resultString;
};

LossRunSummaryFilter.prototype.getStateOfJurisdiction = function () {
    var resultString = "";
    resultString = resultString + ( document.getElementById("ShowAllBttn").style.display == "" ? "a;" : "c;") + this.Controls.StateOfJurisdictionControl.Get_Value();
    return resultString;
};

LossRunSummaryFilter.prototype.GetFilterData = function () {
    var resultArray = new Array();
    resultArray[resultArray.length] = { FilterName: "startDate", FilterType: Consts.DATA_TYPE.DATE, Value: this.Controls.DateRange.getStartDateValue() };
    resultArray[resultArray.length] = { FilterName: "endDate", FilterType: Consts.DATA_TYPE.DATE, Value: this.Controls.DateRange.getEndDateValue() };
    resultArray[resultArray.length] = { FilterName: "asOfDate", FilterType: Consts.DATA_TYPE.DATE, Value: this.Controls.AsOfDate.getAsOfDateValue() };
    resultArray[resultArray.length] = { FilterName: "AsOfDateType", FilterType: Consts.DATA_TYPE.INTEGER, Value: this.Controls.AsOfDate.getAsOfDateTypeValue() };
    resultArray[resultArray.length] = { FilterName: "DateType", FilterType: Consts.DATA_TYPE.BOOL, Value: this.Controls.DateOfLossRadioBtnControl.GetValue() };
    resultArray[resultArray.length] = { FilterName: "ClaimDenied", FilterType: Consts.DATA_TYPE.BOOL, Value: this.Controls.ClaimDeniedRadioBtnControl.GetValue() };
    resultArray[resultArray.length] = { FilterName: "DatePeriod", FilterType: Consts.DATA_TYPE.STRING, Value: this.Controls.DateRange.getDataRangeValue() };
    resultArray[resultArray.length] = { FilterName: "LastXDays", FilterType: Consts.DATA_TYPE.STRING, Value: this.Controls.DateRange.getLastXDaysValue() };
    resultArray[resultArray.length] = { FilterName: "Hierarchy", FilterType: Consts.DATA_TYPE.DATE, Value: this.HierarchyControl.GetHierarchy() };
    resultArray[resultArray.length] = { FilterName: "Classes", FilterType: Consts.DATA_TYPE.STRING, Value: this.Controls.PaymantClassControl.Get_Value() };
    resultArray[resultArray.length] = { FilterName: "Coverages", FilterType: Consts.DATA_TYPE.STRING, Value: this.Controls.CoverageControl.Get_Value() };
    resultArray[resultArray.length] = { FilterName: "ClaimTypes", FilterType: Consts.DATA_TYPE.STRING, Value: this.getClaimTypes() };
    resultArray[resultArray.length] = { FilterName: "IncludeRPO", FilterType: Consts.DATA_TYPE.BOOL, Value: this.Controls.IncludeRPORadioControl.GetBoolValue() };
    resultArray[resultArray.length] = { FilterName: "ClaimStatuses", FilterType: Consts.DATA_TYPE.STRING, Value: this.getClaimStatuses() };
    resultArray[resultArray.length] = { FilterName: "TotalIncurred", FilterType: Consts.DATA_TYPE.INTEGER, Value: "0" };
    resultArray[resultArray.length] = { FilterName: "TotalIncurredOperand", FilterType: Consts.DATA_TYPE.STRING, Value: "Any Value" };
    resultArray[resultArray.length] = { FilterName: "TotalRange", FilterType: Consts.DATA_TYPE.STRING, Value: "Total Incurred" };
    resultArray[resultArray.length] = { FilterName: "StateOfJurisdiction", FilterType: Consts.DATA_TYPE.STRING, Value: this.getStateOfJurisdiction() };
    resultArray[resultArray.length] = { FilterName: "Activity", FilterType: Consts.DATA_TYPE.STRING, Value: this.ActivityControl.GetActivities() };
    resultArray[resultArray.length] = { FilterName: "SpecialAnalysis1", FilterType: Consts.DATA_TYPE.STRING, Value: this.Controls.ClientAnalysisControl.getSpecialAnalysis1DropDownValue() };
    resultArray[resultArray.length] = { FilterName: "SpecialAnalysis2", FilterType: Consts.DATA_TYPE.STRING, Value: this.Controls.ClientAnalysisControl.getSpecialAnalysis2DropDownValue() };
    resultArray[resultArray.length] = { FilterName: "SpecialAnalysis3", FilterType: Consts.DATA_TYPE.STRING, Value: this.Controls.ClientAnalysisControl.getSpecialAnalysis3DropDownValue() };
    resultArray[resultArray.length] = { FilterName: "SpecialAnalysis4", FilterType: Consts.DATA_TYPE.STRING, Value: this.Controls.ClientAnalysisControl.getSpecialAnalysis4DropDownValue() };
    resultArray[resultArray.length] = { FilterName: "SpecialAnalysis5", FilterType: Consts.DATA_TYPE.STRING, Value: this.Controls.ClientAnalysisControl.getSpecialAnalysis5DropDownValue() };
    resultArray[resultArray.length] = { FilterName: "ShowClosedClaimCount", FilterType: Consts.DATA_TYPE.BOOL, Value: this.Controls.ShowClosedClaimsCheckBoxControl.GetStringValue() };
    resultArray[resultArray.length] = { FilterName: "ShowOpenClaimCount", FilterType: Consts.DATA_TYPE.BOOL, Value: this.Controls.ShowOpenClaimsCheckBoxControl.GetStringValue() };
    resultArray[resultArray.length] = { FilterName: "ShowNewClaimCount", FilterType: Consts.DATA_TYPE.BOOL, Value: this.Controls.ShowNewClaimsCheckBoxControl.GetStringValue() };
    resultArray[resultArray.length] = { FilterName: "IncludeOneLineClaimDetail", FilterType: Consts.DATA_TYPE.BOOL, Value: this.Controls.IncludeOneLineClaimDetailCheckBoxControl.GetStringValue() };
    resultArray[resultArray.length] = { FilterName: "ShowPaidTotalByClass", FilterType: Consts.DATA_TYPE.BOOL, Value: this.Controls.TotalPaidCheckBoxControl.GetStringValue() };
    resultArray[resultArray.length] = { FilterName: "ShowReservedTotalByClass", FilterType: Consts.DATA_TYPE.BOOL, Value: this.Controls.OutstandingReserveCheckBoxControl.GetStringValue() };
    resultArray[resultArray.length] = { FilterName: "ShowIncurredTotalByClass", FilterType: Consts.DATA_TYPE.BOOL, Value: this.Controls.TotalIncurredCheckBoxControl.GetStringValue() };
    resultArray[resultArray.length] = { FilterName: "ShowGroupingOption", FilterType: Consts.DATA_TYPE.INTEGER, Value: "2" };
    return resultArray;
};

LossRunSummaryFilter.prototype.IsTurnOfPageBreak = function () {
    return this.Controls.TurnOffPageBreakControl.GetValue() * 1 == 1;
};

LossRunSummaryFilter.prototype.IncludeTitlePage = function () {
    return this.Controls.IncludePageTitleControl.GetValue() * 1 == 1;
};

LossRunSummaryFilter.prototype.IsValid = function () {
    var startDate = Date.parse(this.Controls.DateRange.getStartDateValue());
    var endDate = Date.parse(this.Controls.DateRange.getEndDateValueToValidation());
    if (startDate > endDate) {
        alert("Error. End Date must be later than Start Date.");
        return false;
    }

    if (this.Controls.PaymantClassControl.Get_Value().toString() == "" || this.Controls.CoverageControl.Get_Value().toString() == "") {
        alert("Please select at least one option from the list.");
        return false;
    }
    return true;
};
