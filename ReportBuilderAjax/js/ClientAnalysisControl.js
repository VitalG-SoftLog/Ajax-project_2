var ClientAnalysisControl = function (_parentObject, _id, _labels, _filterDataManager, _data) {
    this.Element = document.getElementById(_id);
    this.ParentObject = _parentObject;
    this.Data = _data;
    this.Controls = new Array();
    this.Index = 0;
    this.HierarchyLabels = _labels;
    this.FilterDataManager = _filterDataManager;
};

ClientAnalysisControl.prototype.Render = function () {
    var parentObj = this;
    this.Controls[index] = {};

    var attributes = {
        'clientAnalysis1DropDownContainer': this.getClientID('clientAnalysis1DropDownContainer'),
        'clientAnalysis2DropDownContainer': this.getClientID('clientAnalysis2DropDownContainer'),
        'clientAnalysis3DropDownContainer': this.getClientID('clientAnalysis3DropDownContainer'),
        'clientAnalysis4DropDownContainer': this.getClientID('clientAnalysis4DropDownContainer'),
        'clientAnalysis5DropDownContainer': this.getClientID('clientAnalysis5DropDownContainer'),
        'clientAnalysis1Label': capitalizeString(this.HierarchyLabels.SpecialAnalysis1Label) + ":",
        'clientAnalysis2Label': capitalizeString(this.HierarchyLabels.SpecialAnalysis2Label) + ":",
        'clientAnalysis3Label': capitalizeString(this.HierarchyLabels.SpecialAnalysis3Label) + ":",
        'clientAnalysis4Label': capitalizeString(this.HierarchyLabels.SpecialAnalysis4Label) + ":",
        'clientAnalysis5Label': capitalizeString(this.HierarchyLabels.SpecialAnalysis5Label) + ":",
        'clientAnalysis1Visible': this.HierarchyLabels.SpecialAnalysis1LabelVisible ? "" : "none",
        'clientAnalysis2Visible': this.HierarchyLabels.SpecialAnalysis2LabelVisible ? "" : "none",
        'clientAnalysis3Visible': this.HierarchyLabels.SpecialAnalysis3LabelVisible ? "" : "none",
        'clientAnalysis4Visible': this.HierarchyLabels.SpecialAnalysis4LabelVisible ? "" : "none",
        'clientAnalysis5Visible': this.HierarchyLabels.SpecialAnalysis5LabelVisible ? "" : "none"
    };

    $(this.Element).append(TemplateEngine.Format(TemplateManager.templates['ClientAnalysis_Main'], attributes));
    this.getClientAnalysis1("");
    this.getClientAnalysis2("");
    this.getClientAnalysis3("");
    this.getClientAnalysis4("");
    this.getClientAnalysis5("");
};

ClientAnalysisControl.prototype.ClearControl = function () {
    for (var idx in this.Controls) {
        var control = this.Controls[idx];
        if (control && control.Data != undefined) {
            control.Set_Value(null);
        }
    }
};

ClientAnalysisControl.prototype.fillControlData = function (name, control) {
    var item = null;
    for (var key in this.Data) {
        if (this.Data[key].FilterName.toString().toLowerCase() == name) {
            item = this.Data[key];
        }
    }
    
    if (item == null || control == null) return;
    control.Set_ValueMode(item.Value.toString());
};

ClientAnalysisControl.prototype.getDataItemByName = function (name) {
    for (var key1 in this.Data) {
        if (this.Data[key1].FilterName.toString().toLowerCase() == name) {
            return this.Data[key1];
        }
    }
    return null;
};

ClientAnalysisControl.prototype.getClientAnalysis1 = function (filter) {
    var parentObj = this;
    this.FilterDataManager.GetClientAnalysis1(filter, function (data) {
        var newData = new Array();
        newData[newData.length] = { Value: "", Name: 'ALL' };

        for (var i in data.ResultList) {
            var item = { Value: data.ResultList[i].Value, Name: data.ResultList[i].Description };
            newData.push(item);
        }

        if (parentObj.Controls.SpecialAnalysis1DropDown == null) {
            parentObj.Controls.SpecialAnalysis1DropDown = new CustomDropDown(document.getElementById(parentObj.getClientID('clientAnalysis1DropDownContainer')), this, newData, 205, data.IsTruncated, data.FilterValue, true);
            parentObj.Controls.SpecialAnalysis1DropDown.OnClick = function () {
                parentObj.getClientAnalysis1(this.FilterValue);
            };
            parentObj.fillControlData("specialanalysis1", parentObj.Controls.SpecialAnalysis1DropDown);
        }
        else {
            parentObj.Controls.SpecialAnalysis1DropDown.ReBind(newData);
        }
    });
};

ClientAnalysisControl.prototype.getClientAnalysis2 = function (filter) {
    var parentObj = this;
    this.FilterDataManager.GetClientAnalysis2(filter, function (data) {
        var newData = new Array();
        newData[newData.length] = { Value: "", Name: 'ALL' };

        for (var i in data.ResultList) {
            var item = { Value: data.ResultList[i].Value, Name: data.ResultList[i].Description };
            newData.push(item);
        }

        if (parentObj.Controls.SpecialAnalysis2DropDown == null) {
            parentObj.Controls.SpecialAnalysis2DropDown = new CustomDropDown(document.getElementById(parentObj.getClientID('clientAnalysis2DropDownContainer')), this, newData, 205, data.IsTruncated, data.FilterValue, true);
            parentObj.Controls.SpecialAnalysis2DropDown.OnClick = function () {
                parentObj.getClientAnalysis2(this.FilterValue);
            };
            parentObj.fillControlData("specialanalysis2", parentObj.Controls.SpecialAnalysis2DropDown);
        }
        else {
            parentObj.Controls.SpecialAnalysis2DropDown.ReBind(newData);
        }
    });
};

ClientAnalysisControl.prototype.getClientAnalysis3 = function (filter) {
    var parentObj = this;
    this.FilterDataManager.GetClientAnalysis3(filter, function (data) {
        var newData = new Array();
        newData[newData.length] = { Value: "", Name: 'ALL' };

        for (var i in data.ResultList) {
            var item = { Value: data.ResultList[i].Value, Name: data.ResultList[i].Description };
            newData.push(item);
        }

        if (parentObj.Controls.SpecialAnalysis3DropDown == null) {
            parentObj.Controls.SpecialAnalysis3DropDown = new CustomDropDown(document.getElementById(parentObj.getClientID('clientAnalysis3DropDownContainer')), this, newData, 205, data.IsTruncated, data.FilterValue, true);
            parentObj.Controls.SpecialAnalysis3DropDown.OnClick = function () {
                parentObj.getClientAnalysis3(this.FilterValue);
            };
            parentObj.fillControlData("specialanalysis3", parentObj.Controls.SpecialAnalysis3DropDown);
        }
        else {
            parentObj.Controls.SpecialAnalysis3DropDown.ReBind(newData);
        }
    });
};

ClientAnalysisControl.prototype.getClientAnalysis4 = function (filter) {
    var parentObj = this;
    this.FilterDataManager.GetClientAnalysis4(filter, function (data) {
        var newData = new Array();
        newData[newData.length] = { Value: "", Name: 'ALL' };

        for (var i in data.ResultList) {
            var item = { Value: data.ResultList[i].Value, Name: data.ResultList[i].Description };
            newData.push(item);
        }

        if (parentObj.Controls.SpecialAnalysis4DropDown == null) {
            parentObj.Controls.SpecialAnalysis4DropDown = new CustomDropDown(document.getElementById(parentObj.getClientID('clientAnalysis4DropDownContainer')), this, newData, 205, data.IsTruncated, data.FilterValue, true);
            parentObj.Controls.SpecialAnalysis4DropDown.OnClick = function () {
                parentObj.getClientAnalysis4(this.FilterValue);
            };
            parentObj.fillControlData("specialanalysis4", parentObj.Controls.SpecialAnalysis4DropDown);
        }
        else {
            parentObj.Controls.SpecialAnalysis4DropDown.ReBind(newData);
        }
    });
};

ClientAnalysisControl.prototype.getClientAnalysis5 = function (filter) {
    var parentObj = this;
    this.FilterDataManager.GetClientAnalysis5(filter, function (data) {
        var newData = new Array();
        newData[newData.length] = { Value: "", Name: 'ALL' };

        for (var i in data.ResultList) {
            var item = { Value: data.ResultList[i].Value, Name: data.ResultList[i].Description };
            newData.push(item);
        }
        if (parentObj.Controls.SpecialAnalysis5DropDown == null) {
            parentObj.Controls.SpecialAnalysis5DropDown = new CustomDropDown(document.getElementById(parentObj.getClientID('clientAnalysis5DropDownContainer')), this, newData, 205, data.IsTruncated, data.FilterValue, true);
            parentObj.Controls.SpecialAnalysis5DropDown.OnClick = function () {
                parentObj.getClientAnalysis5(this.FilterValue);
            };
            parentObj.fillControlData("specialanalysis5", parentObj.Controls.SpecialAnalysis5DropDown);
        }
        else {
            parentObj.Controls.SpecialAnalysis5DropDown.ReBind(newData);
        }
    });
};

ClientAnalysisControl.prototype.getSpecialAnalysis1DropDownValue = function () {
    return this.Controls.SpecialAnalysis1DropDown.Get_Value();
};

ClientAnalysisControl.prototype.getSpecialAnalysis2DropDownValue = function () {
    return this.Controls.SpecialAnalysis2DropDown.Get_Value();
};

ClientAnalysisControl.prototype.getSpecialAnalysis3DropDownValue = function () {
    return this.Controls.SpecialAnalysis3DropDown.Get_Value();
};

ClientAnalysisControl.prototype.getSpecialAnalysis4DropDownValue = function () {
    return this.Controls.SpecialAnalysis4DropDown.Get_Value();
};

ClientAnalysisControl.prototype.getSpecialAnalysis5DropDownValue = function () {
    return this.Controls.SpecialAnalysis5DropDown.Get_Value();
};

ClientAnalysisControl.prototype.getClientID = function (name) {
    return this.Element.id + '_' + name;
};