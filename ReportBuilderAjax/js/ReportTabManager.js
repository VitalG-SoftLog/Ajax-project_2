function ReportTabManager(element, parentObject, isCustom, data) {
    this.Element = element;
    this.Data = data;
    this.ParentObject = parentObject;
    this.IsCustom = isCustom;
    this.CurrentTab = 1;
    this.Tabs = new Array();
    this.init();
    
};

ReportTabManager.prototype.init = function () {
    this.BindTabs();

    if (!this.IsCustom) {
        this.CurrentTab = 2;

        this.active_tab(this.CurrentTab);
    }

    this.BindTab(this.CurrentTab);
};

ReportTabManager.prototype.BindTabs = function () {
    this.Element.innerHTML = '';

    var attributes = {
        'isCustom': (this.IsCustom) ? '' : 'none'
    };

    this.Element.innerHTML = TemplateEngine.Format(TemplateManager.templates['ReportTabs_mainTabs'], attributes);

    var reportLayoutTabContainer = document.getElementById('ReportLayoutTabContainer');
    var viewReportTabContainer = document.getElementById('ViewReportTabContainer');

    this.Tabs[1] = new ReportLayout(reportLayoutTabContainer, this, this.Data);
    this.Tabs[2] = new ViewReport(viewReportTabContainer, this);
    var parentObj = this;

    $('div[reportTabs="1"]', this.Element).each(function () {
        $(this).bind('click', function () {
            var tabId = this.getAttribute('tab');

            parentObj.inactive_tabs();
            parentObj.active_tab(tabId);

            if (tabId == 2) {
                if (document.getElementById("SlideLeftBttn").style.display != 'none')
                    parentObj.ParentObject.HideLeft();

                if (parentObj.Tabs[2] != null) {
                    parentObj.Tabs[2].refreshBtnClick(false);
                }
            }

            parentObj.CurrentTab = tabId;
            parentObj.BindTab(tabId);
        });
    });

};

ReportTabManager.prototype.IsSummaryOnly = function() {
    return this.Tabs[1].IsSummaryOnly();
};

ReportTabManager.prototype.GetCheckedSummarizeFieldIds = function () {
    return this.Tabs[1].GetCheckedSummarizeFieldIds();
};

ReportTabManager.prototype.inactive_tabs = function () {
    $('div[reportTabs="1"]', this.Element).each(function () {
        $('td[corner="1"]', this).addClass('leftInactive');
        $('td[corner="2"]', this).addClass('inactive');
        $('td[corner="3"]', this).addClass('rightInactive');

    });

    $('div[tabContainerIndex]', this.Element).css('display', 'none');
};

ReportTabManager.prototype.BindTab = function (tabId) {
    this.Tabs[tabId].Bind();
};

ReportTabManager.prototype.active_tab = function (tabId) {
    var tabElement = $('div[tab="' + tabId + '"]', this.Element)[0];
    var tabContainerElement = $('div[tabContainerIndex="' + tabId + '"]', this.Element).css('display', '');
    if (tabElement) {
        $('td[corner="1"]', tabElement).removeClass('leftInactive');
        $('td[corner="2"]', tabElement).removeClass('inactive');
        $('td[corner="3"]', tabElement).removeClass('rightInactive');

    }
};