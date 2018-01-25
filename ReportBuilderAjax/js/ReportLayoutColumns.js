function ReportLayoutColumns(element, parentObject, data) {
    this.Element = element;
    this.ParentObject = parentObject;
    this.Data = data;
    this.IsBind = false;
    this.dragDropControl = null;
    this.GoupingTab = null;
    this.SortingTab = null;
};

ReportLayoutColumns.prototype.Bind = function () {

    if (this.IsBind) {
        this.Element.style.display = '';
    }
    else {

        this.Element.innerHTML = TemplateEngine.Format(TemplateManager.templates['ReportLayoutColumns_mainColumns'], {});

        var dragdropContent = document.getElementById('columnsDragDropFields');

        this.dragDropControl = new DragDropFieldsControl(dragdropContent.id, this, this.RefreshDragDropFields(), 'Columns Abailable To Use In Report', 'Columns Used In Report', 'layout');
        this.dragDropControl.Render();

        this.IsBind = true;
    }
};

ReportLayoutColumns.prototype.RefreshDragDropFields = function () {

    var sortedFields = new Array();
    var used = new Array();
    var available = new Array();

    for (var i in this.Data.ReportFields) {
        if (this.Data.ReportFields[i].ColumnOrder * 1 > -1) {
            used.push(this.Data.ReportFields[i]);
        }
        else if (this.Data.ReportFields[i].ColumnOrder * 1 < 0) {
            available.push(this.Data.ReportFields[i]);
        }
    }


    used.sort(function (a, b) {
        var nameA = a.ColumnOrder * 1, nameB = b.ColumnOrder * 1;
        if (nameA < nameB) //sort ascending
            return -1;
        if (nameA > nameB)
            return 1;
        return 0;
    });

    available.sort(function (a, b) {
        var nameA = a.Name.toLowerCase(), nameB = b.Name.toLowerCase();
        if (nameA < nameB) //sort string ascending
            return -1;
        if (nameA > nameB)
            return 1;
        return 0;
    });

    for (var i in used) {
        sortedFields.push(used[i]);
    }
    for (var i in available) {
        sortedFields.push(available[i]);
    }



    return sortedFields;
};