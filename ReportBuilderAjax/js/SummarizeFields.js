function SummarizeFields(element, parentObject, data, values) {
    this.Element = element;
    this.ParentObject = parentObject;
    this.Data = data;
    this.Controls = new Array();
    this.LeftContainer = null;
    this.RightContainer = null;

    this.Values = values;
};

SummarizeFields.prototype.Render = function () {

    this.Element.innerHTML = TemplateEngine.Format(TemplateManager.templates['SummarizeFields_SummarizeContainer'], {});

    this.LeftContainer = $("#leftBlock", this.Element)[0];
    this.RightContainer = $("#rightBlock", this.Element)[0];

    for (var i in this.Data) {

        var itemContainerAttributes = {
            "SummarizeItemCheckBoxContainerId": this.getClientID("SummarizeItemCheckBoxContainerId") + "_" + this.Data[i].ID,
            "FieldName": this.Data[i].Name
        };

        var itemHTML = TemplateEngine.Format(TemplateManager.templates['SummarizeFields_SummarizeItem'], itemContainerAttributes);

        if (i * 1 % 2 == 0) {
            $(this.LeftContainer).append(itemHTML);
        }
        else {
            $(this.RightContainer).append(itemHTML);
        }

        var att = {
            "idCheckBox": this.getClientID("SummarizeItemCheckBoxId") + "_" + this.Data[i].ReportFieldID
        };
        var checkBoxTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_Checkbox'], att);
        $('#' + itemContainerAttributes.SummarizeItemCheckBoxContainerId).append(checkBoxTempl);

        this.Controls["SummarizeItemCheckBoxId" + "_" + this.Data[i].ReportFieldID] = new newCheckBoxControl(this, att["idCheckBox"], "", 0);
        this.Controls["SummarizeItemCheckBoxId" + "_" + this.Data[i].ReportFieldID].DataBind();
        this.Controls["SummarizeItemCheckBoxId" + "_" + this.Data[i].ReportFieldID].ID = this.Data[i].ReportFieldID;
    }

    this.ChangeValues();
};

SummarizeFields.prototype.ChangeValues = function () {
    for (var v in this.Values) {
        var value = this.Values[v];

        if (value) {
            for (var idx in this.Controls) {
                var control = this.Controls[idx];

                if (control && control.ID == value.ReportFieldID) {
                    control.SetValue(1);
                }
            }
        }
    }
};

SummarizeFields.prototype.GetCheckedSummarizeFieldIds = function () {
    var result = new Array();

    for (var i in this.Controls) {
        if (this.Controls[i].Element.checked) result[this.Controls[i].ID] = this.Controls[i].ID;
    }

    return result;
};

SummarizeFields.prototype.getClientID = function (name) {
    return this.Element.id + '_' + name;
};