var TabsManager = function (tabsPlaceHolder, tabsPanelTemplateName, contentPanelTemplateName, tabsSet) {
    this.tabs = new Array();
    this.activeTabIndex = 0;
    this.tabsPlaceHolder = null;    
    this.tabsPanel = null;    
    this.contentPanel = null;    
    this.tabManagerId = new Date().valueOf();
    this.Set_TabsPlaceHolder(tabsPlaceHolder);
    this.Init(tabsPanelTemplateName, contentPanelTemplateName);
    this.InitTabs(tabsSet);
};

TabsManager.prototype.InitTabs = function (tabsSet) {
    if (tabsSet && typeof tabsSet == 'array' && tabsSet.length > 0) {
        for (var idx in tabsSet) {
            var item = tabsSet[idx];
            if (item) {
                this.AddTab(item.tabName, item.tabTemplate, item.isActive, item.callback);
            }
        }
    }
};

TabsManager.prototype.Init = function (tabsPanelTemplateName, contentPanelTemplateName) {
    var tabsDiv = document.createElement('div');
    tabsDiv.id = this.getClientId('tabsPlaceHolder');
    var attr = {
        'id': this.getClientId('tabsPanel')
    };
    $(tabsDiv).append(TemplateEngine.Format(TemplateManager.templates[tabsPanelTemplateName], attr));

    var contentDiv = document.createElement('div');
    contentDiv.id = this.getClientId('contentPlaceHolder');
    attr.id = this.getClientId('contentPanel');
    $(contentDiv).append(TemplateEngine.Format(TemplateManager.templates[contentPanelTemplateName], attr));

    this.tabsPlaceHolder.appendChild(tabsDiv);
    this.tabsPlaceHolder.appendChild(contentDiv);

    this.tabsPanel = this.go(this.getClientId('tabsPanel'));
    this.contentPanel = this.go(this.getClientId('contentPanel'));
};

TabsManager.prototype.AddTab = function (tabName, tabTemplateName, isActive, callback) {
    var parentObj = this;
    var tabObj = { };
    tabObj.name = tabName;
    tabObj.isActive = isActive;
    tabObj.tabIndex = this.tabs.length;

    var attr = {
        'tabId': this.getClientId('tab:' + tabObj.tabIndex),
        'tabIndex': tabObj.tabIndex
    };
    $(this.tabsPanel).append(TemplateEngine.Format(TemplateManager.templates[tabTemplateName], attr));
    this.tabs.push(tabObj);

    var currentTabContentDiv = document.createElement('div');
    currentTabContentDiv.id = this.getClientId('tabContent:' + tabObj.tabIndex);
    currentTabContentDiv.style.display = 'none';
    $(this.contentPanel).append(currentTabContentDiv);

    if (callback) {
        var currentTab = this.go(this.getClientId('tab:' + tabObj.tabIndex));
        if (currentTab) {
            $(currentTab).bind('click', function (sender) {
                parentObj.HideContentPanels();
                var tabIndex = this.getAttribute('tabIndex');
                parentObj.ShowCurrentContentPanel(tabIndex);
                callback.call();
            });
        }
    }
};

TabsManager.prototype.HideContentPanels = function () {
    for (var i in this.tabs) {
        var tabObj = this.tabs[i];
        if (tabObj) {
            var el = this.go(this.getClientId('tabContent:' + i));
            if (el)
                el.style.display = 'none';
        }
    }
};

TabsManager.prototype.ShowCurrentContentPanel = function(panelIndex) {
    var tabObj = this.tabs[panelIndex];
    if (tabObj) {
        var el = this.go(this.getClientId('tabContent:' + panelIndex));
        if (el)
            el.style.display = '';
    }
};

TabsManager.prototype.go = function(name) {   
    var el = document.getElementById(name);
    if(el)
        return el;
    else
        throw 'Cannot access element with id='+name;
};

TabsManager.prototype.getClientId = function (name) {
    return 'tabsManager:' + this.tabManagerId + '_' + name;
};

TabsManager.prototype.Get_ActiveTabIndex = function () {
    return this.activeTabIndex;
};

TabsManager.prototype.Set_ActiveTabIndex = function (value) {
    this.activeTabIndex = (!value) ? 0 : value;
};

TabsManager.prototype.Get_TabsPlaceHolder = function () {
    return this.tabsPlaceHolder;
};

TabsManager.prototype.Set_TabsPlaceHolder = function (tabsPlaceHolder) {
    if (typeof tabsPlaceHolder == 'object')
        this.tabsPlaceHolder = tabsPlaceHolder;
    else if (typeof tabsPlaceHolder == 'string') {
        this.tabsPlaceHolder = this.go(tabsPlaceHolder);
    }

};

/*TabsManager.prototype.Get_ = function() {

};

TabsManager.prototype.Set_ = function() {

};*/