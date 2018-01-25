var HierarchyControl = function (_parentObject, _id, _labels, _filterDataManager, _data) {
    this.Element = document.getElementById(_id);
    this.ParentObject = _parentObject;
    this.Data = _data;
    this.Controls = new Array();
    this.Index = 0;
    this.HierarchyLabels = _labels;
    this.FilterDataManager = _filterDataManager;
};

HierarchyControl.prototype.Render = function () {
    var parentObj = this;

    this.Element.innerHTML = '';
    this.Controls = new Array();

    this.CreateHierarchyItem(this.Index++, true);
};

HierarchyControl.prototype.CreateHierarchyItem = function (index, getMembers) {
    var parentObj = this;
    this.Controls[index] = {};

    var attributes = {
        'index': index,
        'menbersDropDownContainer': this.getClientID('menbersDropDownContainer_' + index),
        'locationDropDownContainer': this.getClientID('locationDropDownContainer_' + index),
        'groupCode1DropDownContainer': this.getClientID('groupCode1DropDownContainer_' + index),
        'groupCode2DropDownContainer': this.getClientID('groupCode2DropDownContainer_' + index),
        'groupCode3DropDownContainer': this.getClientID('groupCode3DropDownContainer_' + index),
        'groupCode4DropDownContainer': this.getClientID('groupCode4DropDownContainer_' + index),
        'clearHierarchy': this.getClientID('clearHierarchy_' + index),
        'addHierarchyItem': this.getClientID('addHierarchyItem_' + index),
        'removeHierarchyItem': this.getClientID('removeHierarchyItem_' + index),
        'memberLabel': capitalizeString(this.HierarchyLabels.MemberLabel) + ":",
        'locationLabel': capitalizeString(this.HierarchyLabels.LocationLabel) + ":",
        'groupCode1Label': capitalizeString(this.HierarchyLabels.Group1Label) + ":",
        'groupCode2Label': capitalizeString(this.HierarchyLabels.Group2Label) + ":",
        'groupCode3Label': capitalizeString(this.HierarchyLabels.Group3Label) + ":",
        'groupCode4Label': capitalizeString(this.HierarchyLabels.Group4Label) + ":",
        'groupCode1LabelId': this.getClientID('groupCode1LabelId_' + index),
        'groupCode2LabelId': this.getClientID('groupCode2LabelId_' + index),
        'groupCode3LabelId': this.getClientID('groupCode3LabelId_' + index),
        'groupCode4LabelId': this.getClientID('groupCode4LabelId_' + index)
    };

    $(this.Element).append(TemplateEngine.Format(TemplateManager.templates['Hierarchy_Item'], attributes));

    var menbersDropDownAttr = {
        "idDropDown": this.getClientID("menbersDropDown_" + index),
        "dropDownHide": "dropDownHide"
    };
    var menbersDropDownTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_DropDownControl'], menbersDropDownAttr);
    $(document.getElementById(this.getClientID('menbersDropDownContainer_' + index))).append(menbersDropDownTempl);
    this.Controls[index].MenbersDropDown = new newDropDownControl(this, menbersDropDownAttr["idDropDown"], "", "", ValueType.String, null, true, 205);


    var locationDropDownAttr = {
        "idDropDown": this.getClientID("locationDropDown_" + index),
        "dropDownHide": "dropDownHide"
    };
    var locationDropDownTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_DropDownControl'], locationDropDownAttr);
    $(document.getElementById(this.getClientID('locationDropDownContainer_' + index))).append(locationDropDownTempl);
    this.Controls[index].LocationDropDown = new newDropDownControl(this, locationDropDownAttr["idDropDown"], "", "", ValueType.String, null, false, 205);

    var groupCode1DropDownAttr = {
        "idDropDown": this.getClientID("groupCode1DropDown_" + index),
        "dropDownHide": "dropDownHide"
    };
    var groupCode1DropDownTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_DropDownControl'], groupCode1DropDownAttr);
    $(document.getElementById(this.getClientID('groupCode1DropDownContainer_' + index))).append(groupCode1DropDownTempl);
    this.Controls[index].GroupCode1DropDown = new newDropDownControl(this, groupCode1DropDownAttr["idDropDown"], "", "", ValueType.String, null, false, 205);

    var groupCode2DropDownAttr = {
        "idDropDown": this.getClientID("groupCode2DropDown_" + index),
        "dropDownHide": "dropDownHide"
    };
    var groupCode2DropDownTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_DropDownControl'], groupCode2DropDownAttr);
    $(document.getElementById(this.getClientID('groupCode2DropDownContainer_' + index))).append(groupCode2DropDownTempl);
    this.Controls[index].GroupCode2DropDown = new newDropDownControl(this, groupCode2DropDownAttr["idDropDown"], "", "", ValueType.String, null, false, 205);

    var groupCode3DropDownAttr = {
        "idDropDown": this.getClientID("groupCode3DropDown_" + index),
        "dropDownHide": "dropDownHide"
    };
    var groupCode3DropDownTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_DropDownControl'], groupCode3DropDownAttr);
    $(document.getElementById(this.getClientID('groupCode3DropDownContainer_' + index))).append(groupCode3DropDownTempl);
    this.Controls[index].GroupCode3DropDown = new newDropDownControl(this, groupCode3DropDownAttr["idDropDown"], "", "", ValueType.String, null, false, 205);

    var groupCode4DropDownAttr = {
        "idDropDown": this.getClientID("groupCode4DropDown_" + index),
        "dropDownHide": "dropDownHide"
    };
    var groupCode4DropDownTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_DropDownControl'], groupCode4DropDownAttr);
    $(document.getElementById(this.getClientID('groupCode4DropDownContainer_' + index))).append(groupCode4DropDownTempl);
    this.Controls[index].GroupCode4DropDown = new newDropDownControl(this, groupCode4DropDownAttr["idDropDown"], "", "", ValueType.String, null, false, 205);

    this.ConfigureControlsVisibility(index);

    var addActivityBtnTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_plusBtnTemplate'], {});
    $(document.getElementById(this.getClientID('addHierarchyItem_' + index))).append(addActivityBtnTempl);

    $(document.getElementById(this.getClientID('addHierarchyItem_' + index))).click(function () {
        parentObj.AddHierarchyItem();
    });

    var removeActivityBtnTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_minusBtnTemplate'], {});
    $(document.getElementById(this.getClientID('removeHierarchyItem_' + index))).append(removeActivityBtnTempl);

    $(document.getElementById(this.getClientID('removeHierarchyItem_' + index))).click(function () {
        parentObj.RemoveHierarchyItem(this.id.split('_')[2]);
        RightTabHeight();
    });

    $(document.getElementById(this.getClientID('clearHierarchy_' + index))).click(function () {
        parentObj.ClearHierarchy();
        RightTabHeight();
    });

    this.ConfigureButtons();

    if (getMembers) {
        this.FilterDataManager.GetMembers("", "", function (data) {

            parentObj.Controls[index].MenbersDropDown.AppendOption("ALL", "ALL");
            for (var item in data.ResultList) {
                parentObj.Controls[index].MenbersDropDown.AppendOption(data.ResultList[item].Value, data.ResultList[item].Description);
            }
            parentObj.Controls[index].MenbersDropDown.SetDropDownValue("ALL");

            if (data.IsTruncated) parentObj.Controls[index].MenbersDropDown.ShowFilter();
            else parentObj.Controls[index].MenbersDropDown.HideFilter();
        });

        this.SetDropDownsPropertyChagedEvent(index);

    }
};

HierarchyControl.prototype.SetDropDownsPropertyChagedEvent = function (index) {
    var parentObj = this;
    
    this.Controls[index].MenbersDropDown.PropertyChanged = function () {
        parentObj.Controls[index].LocationDropDown.ClearOptions();
        parentObj.Controls[index].LocationDropDown.AppendOption("ALL", "ALL");
        parentObj.Controls[index].LocationDropDown.SetDropDownValue("ALL");

        parentObj.ConfigureControlsVisibility(index);

        if (this.GetValue() == "ALL") {
            return;
        }

        parentObj.FilterDataManager.GetLocations("", "", this.GetOptionValue(this.GetValue()), function (data) {

            for (var item in data.ResultList) {
                parentObj.Controls[index].LocationDropDown.AppendOption(data.ResultList[item].Value, data.ResultList[item].Description);
            }

        });
    };

    this.Controls[index].LocationDropDown.PropertyChanged = function () {
        parentObj.Controls[index].GroupCode1DropDown.ClearOptions();
        parentObj.Controls[index].GroupCode1DropDown.AppendOption("ALL", "ALL");
        parentObj.Controls[index].GroupCode1DropDown.SetDropDownValue("ALL");

        parentObj.ConfigureControlsVisibility(index);

        if (this.GetValue() == "ALL") {
            return;
        }

        parentObj.FilterDataManager.GetGroupCode1s("", "", parentObj.Controls[index].MenbersDropDown.GetOptionValue(parentObj.Controls[index].MenbersDropDown.GetValue()),
                this.GetOptionValue(this.GetValue()),
                function (data) {
                    for (var item in data.ResultList) {
                        parentObj.Controls[index].GroupCode1DropDown.AppendOption(data.ResultList[item].Value, data.ResultList[item].Description);
                    }
                });
    };

    this.Controls[index].GroupCode1DropDown.PropertyChanged = function () {
        parentObj.Controls[index].GroupCode2DropDown.ClearOptions();
        parentObj.Controls[index].GroupCode2DropDown.AppendOption("ALL", "ALL");
        parentObj.Controls[index].GroupCode2DropDown.SetDropDownValue("ALL");

        parentObj.ConfigureControlsVisibility(index);

        if (this.GetValue() == "ALL") {
            return;
        }

        parentObj.FilterDataManager.GetGroupCode2s("", "", parentObj.Controls[index].MenbersDropDown.GetOptionValue(parentObj.Controls[index].MenbersDropDown.GetValue()),
                parentObj.Controls[index].LocationDropDown.GetOptionValue(parentObj.Controls[index].LocationDropDown.GetValue()),
                this.GetOptionValue(this.GetValue()),
                function (data) {
                    for (var item in data.ResultList) {
                        parentObj.Controls[index].GroupCode2DropDown.AppendOption(data.ResultList[item].Value, data.ResultList[item].Description);
                    }
                });
    };


    this.Controls[index].GroupCode2DropDown.PropertyChanged = function () {
        parentObj.Controls[index].GroupCode3DropDown.ClearOptions();
        parentObj.Controls[index].GroupCode3DropDown.AppendOption("ALL", "ALL");
        parentObj.Controls[index].GroupCode3DropDown.SetDropDownValue("ALL");

        parentObj.ConfigureControlsVisibility(index);

        if (this.GetValue() == "ALL") {
            return;
        }

        parentObj.FilterDataManager.GetGroupCode3s("", "", parentObj.Controls[index].MenbersDropDown.GetOptionValue(parentObj.Controls[index].MenbersDropDown.GetValue()),
                parentObj.Controls[index].LocationDropDown.GetOptionValue(parentObj.Controls[index].LocationDropDown.GetValue()),
                parentObj.Controls[index].GroupCode1DropDown.GetOptionValue(parentObj.Controls[index].GroupCode1DropDown.GetValue()),
                this.GetOptionValue(this.GetValue()),
                function (data) {
                    for (var item in data.ResultList) {
                        parentObj.Controls[index].GroupCode3DropDown.AppendOption(data.ResultList[item].Value, data.ResultList[item].Description);
                    }
                });
    };

    this.Controls[index].GroupCode3DropDown.PropertyChanged = function () {
        parentObj.Controls[index].GroupCode4DropDown.ClearOptions();
        parentObj.Controls[index].GroupCode4DropDown.AppendOption("ALL", "ALL");
        parentObj.Controls[index].GroupCode4DropDown.SetDropDownValue("ALL");

        parentObj.ConfigureControlsVisibility(index);

        if (this.GetValue() == "ALL") {
            return;
        }

        parentObj.FilterDataManager.GetGroupCode4s("", "", parentObj.Controls[index].MenbersDropDown.GetOptionValue(parentObj.Controls[index].MenbersDropDown.GetValue()),
                parentObj.Controls[index].LocationDropDown.GetOptionValue(parentObj.Controls[index].LocationDropDown.GetValue()),
                parentObj.Controls[index].GroupCode1DropDown.GetOptionValue(parentObj.Controls[index].GroupCode1DropDown.GetValue()),
                parentObj.Controls[index].GroupCode2DropDown.GetOptionValue(parentObj.Controls[index].GroupCode2DropDown.GetValue()),
                this.GetOptionValue(this.GetValue()),
                function (data) {
                    for (var item in data.ResultList) {
                        parentObj.Controls[index].GroupCode4DropDown.AppendOption(data.ResultList[item].Value, data.ResultList[item].Description);
                    }
                });
    };
};


HierarchyControl.prototype.AddHierarchyItem = function () {
    this.CreateHierarchyItem(this.Index++, true);
    this.ConfigureButtons();
};

HierarchyControl.prototype.RemoveHierarchyItem = function (index) {
    var parentObj = this;

    if ($('div.hierarchyItem', this.Element).length > 1) {
        $('div.hierarchyItem', this.Element).each(function () {
            var idx = this.getAttribute('index');
            if (idx == index) {
                $(this).remove();
            }

            parentObj.Controls[index] = null;
        });
    }

    this.ConfigureButtons();
};

HierarchyControl.prototype.ClearHierarchy = function () {
    this.Index = 0;
    this.Render();
    this.ConfigureButtons();
};


HierarchyControl.prototype.ConfigureButtons = function() {
    $('div.addHierarchy').css('display', 'none');
    $('div.clearHierarhy').css('display', 'none');


    $('div.hierarchyItem:last-child').each(function() {
        $('div.addHierarchy', this).css('display', '');
    });

    $('div.hierarchyItem:first-child').each(function() {
        $('div.clearHierarhy', this).css('display', '');
    });
};


HierarchyControl.prototype.ConfigureControlsVisibility = function (index) {
    $('.groupCodeContainer', $('div.hierarchyItem[index='+ index + ']')[0]).css('display', 'none');
    $('.groupCodeLabel', $('div.hierarchyItem[index=' + index + ']')[0]).css('display', 'none');

    if (this.Controls[index].LocationDropDown.GetValue().replace('ALL','') != '') {
        $('#' + this.getClientID('groupCode1LabelId_' + index)).css('display', '');
        $('#' + this.getClientID('groupCode1DropDownContainer_' + index)).css('display', '');
    }

    if (this.Controls[index].GroupCode1DropDown.GetValue().replace('ALL', '') != '') {
        $('#' + this.getClientID('groupCode2LabelId_' + index)).css('display', '');
        $('#' + this.getClientID('groupCode2DropDownContainer_' + index)).css('display', '');
    }

    if (this.Controls[index].GroupCode2DropDown.GetValue().replace('ALL', '') != '') {
        $('#' + this.getClientID('groupCode3LabelId_' + index)).css('display', '');
        $('#' + this.getClientID('groupCode3DropDownContainer_' + index)).css('display', '');
    }

    if (this.Controls[index].GroupCode3DropDown.GetValue().replace('ALL', '') != '') {
        $('#' + this.getClientID('groupCode4LabelId_' + index)).css('display', '');
        $('#' + this.getClientID('groupCode4DropDownContainer_' + index)).css('display', '');
    }
};

HierarchyControl.prototype.GetHierarchy = function () {
    var result = "";

    for (var i in this.Controls) {
        if (this.Controls[i] && this.Controls[i].MenbersDropDown.GetValue() != 'ALL') {
            result +=  this.Controls[i].MenbersDropDown.GetOptionValue(this.Controls[i].MenbersDropDown.GetValue()) + ',';
            result += (this.Controls[i].LocationDropDown.GetValue() == 'ALL' ? '' : this.Controls[i].LocationDropDown.GetOptionValue(this.Controls[i].LocationDropDown.GetValue())) + ',';
            result += (this.Controls[i].GroupCode1DropDown.GetValue() == 'ALL' ? '' : this.Controls[i].GroupCode1DropDown.GetOptionValue(this.Controls[i].GroupCode1DropDown.GetValue())) + ',';
            result += (this.Controls[i].GroupCode2DropDown.GetValue() == 'ALL' ? '' : this.Controls[i].GroupCode2DropDown.GetOptionValue(this.Controls[i].GroupCode2DropDown.GetValue())) + ',';
            result += (this.Controls[i].GroupCode3DropDown.GetValue() == 'ALL' ? '' : this.Controls[i].GroupCode3DropDown.GetOptionValue(this.Controls[i].GroupCode3DropDown.GetValue())) + ',';
            result += (this.Controls[i].GroupCode4DropDown.GetValue() == 'ALL' ? '' : this.Controls[i].GroupCode4DropDown.GetOptionValue(this.Controls[i].GroupCode4DropDown.GetValue())) + ',';
           
        }
    }

    if (result.length > 0) return result.substring(0, result.length - 1);

    return result;
};

HierarchyControl.prototype.FillHierarchy = function () {
    this.Index = 0;
    this.Controls = new Array();
    this.Element.innerHTML = '';

    var hierarhyArray = this.Data.split(',');

    for (var i = 0; i < hierarhyArray.length; i = i + 6) {
        if (hierarhyArray[i] != '') {
            this.CreateHierarchyItem(this.Index, false);
            this.getHierarchyItemData(hierarhyArray[i], hierarhyArray[i + 1], hierarhyArray[i + 2], hierarhyArray[i + 3], hierarhyArray[i + 4], hierarhyArray[i + 5], this.Index);
            this.Index++;
        } else {
            this.Render();
        }

    }
};

HierarchyControl.prototype.getHierarchyItemData = function (member, location, groupCode1, groupCode2, groupCode3, groupCode4, index) {
    var parentObj = this;

    if (member != '') {

        this.FilterDataManager.GetMembers("", "", function (data) {
            parentObj.Controls[index].MenbersDropDown.ClearOptions();
            parentObj.Controls[index].MenbersDropDown.AppendOption("ALL", "ALL");
            for (var item in data.ResultList) {
                parentObj.Controls[index].MenbersDropDown.AppendOption(data.ResultList[item].Value, data.ResultList[item].Description);
            }

            if (data.IsTruncated) parentObj.Controls[index].MenbersDropDown.ShowFilter();
            else parentObj.Controls[index].MenbersDropDown.HideFilter();

            parentObj.Controls[index].MenbersDropDown.SetDropDownValue(member);

            if (location != '') {
                parentObj.Controls[index].LocationDropDown.ClearOptions();
                parentObj.Controls[index].LocationDropDown.AppendOption("ALL", "ALL");
                parentObj.FilterDataManager.GetLocations("", "", member, function (data) {
                    for (var item in data.ResultList) {
                        parentObj.Controls[index].LocationDropDown.AppendOption(data.ResultList[item].Value, data.ResultList[item].Description);
                    }

                    parentObj.Controls[index].GroupCode1DropDown.ClearOptions();
                    parentObj.Controls[index].GroupCode1DropDown.AppendOption("ALL", "ALL");

                    parentObj.Controls[index].LocationDropDown.SetDropDownValue(location);

                    if (groupCode1 != '') {
                        parentObj.FilterDataManager.GetGroupCode1s("", "", member, location, function (data) {

                            for (var item in data.ResultList) {
                                parentObj.Controls[index].GroupCode1DropDown.AppendOption(data.ResultList[item].Value, data.ResultList[item].Description);
                            }

                            parentObj.Controls[index].GroupCode2DropDown.ClearOptions();
                            parentObj.Controls[index].GroupCode2DropDown.AppendOption("ALL", "ALL");

                            parentObj.Controls[index].GroupCode1DropDown.SetDropDownValue(groupCode1);

                            if (groupCode2 != '') {
                                parentObj.FilterDataManager.GetGroupCode2s("", "", member, location, groupCode1, function (data) {

                                    for (var item in data.ResultList) {
                                        parentObj.Controls[index].GroupCode2DropDown.AppendOption(data.ResultList[item].Value, data.ResultList[item].Description);
                                    }
                                    parentObj.Controls[index].GroupCode3DropDown.ClearOptions();
                                    parentObj.Controls[index].GroupCode3DropDown.AppendOption("ALL", "ALL");

                                    parentObj.Controls[index].GroupCode2DropDown.SetDropDownValue(groupCode2);
                                    
                                    if (groupCode3 != '') {
                                        parentObj.FilterDataManager.GetGroupCode3s("", "", member, location, groupCode1, groupCode2, function (data) {

                                            for (var item in data.ResultList) {
                                                parentObj.Controls[index].GroupCode3DropDown.AppendOption(data.ResultList[item].Value, data.ResultList[item].Description);
                                            }

                                            parentObj.Controls[index].GroupCode4DropDown.ClearOptions();
                                            parentObj.Controls[index].GroupCode4DropDown.AppendOption("ALL", "ALL");

                                            parentObj.Controls[index].GroupCode3DropDown.SetDropDownValue(groupCode3);

                                            if (groupCode4 != '') {
                                                parentObj.FilterDataManager.GetGroupCode4s("", "", member, location, groupCode1, groupCode2, groupCode3, function (data) {
                                                    for (var item in data.ResultList) {
                                                        parentObj.Controls[index].GroupCode4DropDown.AppendOption(data.ResultList[item].Value, data.ResultList[item].Description);
                                                    }

                                                    parentObj.Controls[index].GroupCode4DropDown.SetDropDownValue(groupCode4);
                                                });
                                            } else {
                                                parentObj.Controls[index].GroupCode4DropDown.SetDropDownValue('ALL');
                                            }

                                            parentObj.SetDropDownsPropertyChagedEvent(index);
                                            parentObj.ConfigureControlsVisibility(index);
                                        });

                                    } else {
                                        parentObj.Controls[index].GroupCode3DropDown.SetDropDownValue('ALL');
                                        parentObj.SetDropDownsPropertyChagedEvent(index);
                                        parentObj.ConfigureControlsVisibility(index);
                                    }
                                });
                            } else {
                                parentObj.Controls[index].GroupCode2DropDown.SetDropDownValue('ALL');
                                parentObj.SetDropDownsPropertyChagedEvent(index);
                                parentObj.ConfigureControlsVisibility(index);
                            }
                        });
                    } else {
                        parentObj.Controls[index].GroupCode1DropDown.SetDropDownValue('ALL');
                        parentObj.SetDropDownsPropertyChagedEvent(index);
                        parentObj.ConfigureControlsVisibility(index);
                    }
                });
            } else {
                parentObj.Controls[index].LocationDropDown.SetDropDownValue('ALL');
                parentObj.SetDropDownsPropertyChagedEvent(index);
            }
        });
    } else {
        this.Controls[index].MenbersDropDown.SetDropDownValue('ALL');
        this.SetDropDownsPropertyChagedEvent(index);
    }
};


HierarchyControl.prototype.getClientID = function (name) {
    return this.Element.id + '_' + name;
};
