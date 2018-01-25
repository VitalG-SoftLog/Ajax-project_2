function ReportLayoutSorting(element, parentObject, data) {
    this.Element = element;
    this.ParentObject = parentObject;
    this.Data = data;
    this.dragDropControl = null;
    this.IsBind = false;

    this.init();
};

ReportLayoutSorting.prototype.init = function () {
    this.Element.innerHTML = TemplateEngine.Format(TemplateManager.templates['ReportLayoutSorting_mainSorting'], {});
    this.Element.style.display = 'none';
    var dragdropContent = document.getElementById('sortingDragDropFields');

    this.dragDropControl = new DragDropFieldsControl(dragdropContent.id, this, this.RefreshDragDropFields(), 'Available Columns', 'Sorted Columns', 'sorting');
    this.dragDropControl.Render();

    this.IsBind = true;
};

ReportLayoutSorting.prototype.Bind = function () {
    if (this.IsBind) {
        this.Element.style.display = '';
    }
    else {

        this.IsBind = true; 
    }
};

ReportLayoutSorting.prototype.RefreshDragDropFields = function () {

    var sortedFields = new Array();
    var used = new Array();
    var available = new Array();

    for (var i in this.Data.ReportFields) {
        if (this.Data.ReportFields[i].SortOrder * 1 > -1 && this.Data.ReportFields[i].IsVisible) {
            used.push(this.Data.ReportFields[i]);
        }
        else if (this.Data.ReportFields[i].SortOrder * 1 < 0 && this.Data.ReportFields[i].IsVisible) {
            available.push(this.Data.ReportFields[i]);
        }
    }


    used.sort(function (a, b) {
        var nameA = a.SortOrder * 1, nameB = b.SortOrder * 1;
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