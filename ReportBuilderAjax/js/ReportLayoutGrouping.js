function ReportLayoutGrouping(element, parentObject, data) {
    this.Element = element;
    this.ParentObject = parentObject;
    this.Data = data;
    this.IsBind = false;
    this.dragDropControl = null;
    this.Controls = new Array();
    this.SummarizeControl = null;
    this.init();  
};


ReportLayoutGrouping.prototype.init = function () {
    this.Element.innerHTML = TemplateEngine.Format(TemplateManager.templates['ReportLayoutGrouping_mainGrouping'], {});
    this.Element.style.display = 'none';
    var dragdropContent = document.getElementById('groupingDragDropFields');
    this.dragDropControl = new DragDropFieldsControl(dragdropContent.id, this, this.RefreshDragDropFields(), 'Available Summary By Fields', 'Summary By Fields', 'grouping');
    this.dragDropControl.Render();
    this.IsBind = true;


    var att = {
        "idCheckBox": this.getClientID("pageBreakCheckBox")
    };
    var checkBoxTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_Checkbox'], att);
    $(document.getElementById("pageBreakCheckBox")).append(checkBoxTempl);

    this.Controls.pageBreakCheckBox = new newCheckBoxControl(this, att["idCheckBox"], "", 1);
    this.Controls.pageBreakCheckBox.DataBind();

    this.PageBreakChangeValue();

    att = {
        "idCheckBox": this.getClientID("removeDetailCheckBox")
    };
    checkBoxTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_Checkbox'], att);
    $(document.getElementById("removeDetailCheckBox")).append(checkBoxTempl);

    this.Controls.removeDetailCheckBox = new newCheckBoxControl(this, att["idCheckBox"], "", 0);
    this.Controls.removeDetailCheckBox.DataBind();

    if (this.Data.SummaryOnly) {
        this.Controls.removeDetailCheckBox.SetValue(1);
    }
    
    var sumFields = this.GetSummarizeFields();

    this.SummarizeControl = new SummarizeFields(document.getElementById('summarizeFields'), this, sumFields, this.Data.UserReportSummarizeFields);
    this.SummarizeControl.Render();

};

ReportLayoutGrouping.prototype.PageBreakChangeValue = function () {
    var isPageBreak = false;
    for (var idx in this.Data.ReportFields) {
        var item = this.Data.ReportFields[idx];

        if (item && item.IsUsed && item.IsPageBreak) {
            isPageBreak = true;
            break;
        }
    }

    if (isPageBreak) {
        this.Controls.pageBreakCheckBox.SetValue(1);
    }
};

ReportLayoutGrouping.prototype.IsSummaryOnly = function () {
    return this.Controls.removeDetailCheckBox.GetStringValue();
};

ReportLayoutGrouping.prototype.GetCheckedSummarizeFieldIds = function() {
    return this.SummarizeControl.GetCheckedSummarizeFieldIds();
};

ReportLayoutGrouping.prototype.Bind = function () {
    if (this.IsBind) {
        this.Element.style.display = '';
    }
    else {
        this.IsBind = true;
    }
};


ReportLayoutGrouping.prototype.GetSummarizeFields = function () {
    var result = new Array();

    for (var i in this.Data.ReportFields) {
        if (this.Data.ReportFields[i].IsSummarizable && this.Data.ReportFields[i].IsUsed) {
            result.push(this.Data.ReportFields[i]);
        }
    }

    $('#summarizeFieldsCount').html(' (' + result.length + ')');
    
    return result;
};

ReportLayoutGrouping.prototype.RefreshDragDropFields = function () {

    var usedFields = new Array();
    for (var i in this.Data.ReportFields) {
        if (this.Data.ReportFields[i].IsUsed && this.Data.ReportFields[i].IsGroupable) {
            usedFields.push(this.Data.ReportFields[i]);
        }
    }


    var usedInGrouping = new Array();
    var availableInGrouping = new Array();

    for (var i in usedFields) {
        if (usedFields.GroupOrder * 1 > -1 || usedFields.IsGroupByDefault) {
            usedInGrouping.push(usedFields[i]);
        }
        else {
            availableInGrouping.push(usedFields[i]);
        }
    }


    usedInGrouping.sort(function (a, b) {
        var nameA = a.GroupOrder * 1, nameB = b.GroupOrder * 1;
        if (nameA < nameB) //sort ascending
            return -1;
        if (nameA > nameB)
            return 1;
        return 0;
    });

    availableInGrouping.sort(function (a, b) {
        var nameA = a.Name.toLowerCase(), nameB = b.Name.toLowerCase();
        if (nameA < nameB) //sort string ascending
            return -1;
        if (nameA > nameB)
            return 1;
        return 0;
    });

    usedFields = new Array();

    for (var i in usedInGrouping) {
        usedFields.push(usedInGrouping[i]);
    }
    for (var i in availableInGrouping) {
        usedFields.push(availableInGrouping[i]);
    }
    
    return usedFields;
};


ReportLayoutGrouping.prototype.getClientID = function (name) {
    return this.Element.id + '_' + name;
};