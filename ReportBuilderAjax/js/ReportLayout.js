function ReportLayout(element, parentObject, data) {
    this.Element = element;
    this.ParentObject = parentObject;
    this.CurrentTab = 1;
    this.Data = data;
    this.Groups = new Array();   
    
};

ReportLayout.prototype.Bind = function () {
    if (this.Element.innerHTML != '') return;
    
    this.Element.innerHTML = TemplateEngine.Format(TemplateManager.templates['ReportLayoutGroups_mainTabs'], {});

    var columnsTab = document.getElementById('groupMainTabColumns');
    var sortingTab = document.getElementById('groupMainTabSorting');
    var groupingTab = document.getElementById('groupMainTabGrouping');

    this.Groups[1] = new ReportLayoutColumns(columnsTab, this, this.Data);
    this.Groups[2] = new ReportLayoutSorting(sortingTab, this, this.Data);
    this.Groups[3] = new ReportLayoutGrouping(groupingTab, this, this.Data);

    this.Groups[1].SortingTab = this.Groups[2];
    this.Groups[1].GoupingTab = this.Groups[3];

    var parentObj = this;

    $('div[groupTabs="1"]', this.Element).each(function () {
        $(this).bind('click', function () {
            var tabId = this.getAttribute('groupTab');

            parentObj.inactive_tabs();
            parentObj.active_tab(tabId);

            parentObj.CurrentTab = tabId;
            parentObj.UnBindTabs();
            parentObj.BindTab(tabId);
        });
    });

    this.BindTab(this.CurrentTab);
};

ReportLayout.prototype.IsSummaryOnly = function () {
    return this.Groups[3].IsSummaryOnly();
};

ReportLayout.prototype.GetCheckedSummarizeFieldIds = function () {
    return this.Groups[3].GetCheckedSummarizeFieldIds();
};

ReportLayout.prototype.inactive_tabs = function () {
    $('div[groupTabs="1"]', this.Element).each(function () {
        $('td[corner="1"]', this).addClass('leftInactive');
        $('td[corner="2"]', this).addClass('inactive');
        $('td[corner="3"]', this).addClass('rightInactive');

        //$('span[shadow="1"]', this).style.display = '';
    });
};

ReportLayout.prototype.BindTab = function (tabId) {
    this.Groups[tabId].Bind();
};

ReportLayout.prototype.UnBindTabs = function () {
    for (var idx in this.Groups) {
        var group = this.Groups[idx];

        if (group && group.IsBind) {
            group.Element.style.display = 'none';
        }
    }
};

ReportLayout.prototype.active_tab = function (tabId) {
    var tabElement = $('div[groupTab="' + tabId + '"]', this.Element)[0];

    if (tabElement) {
        $('td[corner="1"]', tabElement).removeClass('leftInactive');
        $('td[corner="2"]', tabElement).removeClass('inactive');
        $('td[corner="3"]', tabElement).removeClass('rightInactive');

        //$('span[shadow="1"]', tabElement).style.display = 'none';

    }
};