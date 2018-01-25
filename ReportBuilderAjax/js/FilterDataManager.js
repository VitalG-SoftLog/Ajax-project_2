var FilterDataManager = function () {
    this.LayerCount = 0;
};

FilterDataManager.prototype.GetCoveragesList = function (callback) {
    var params = {
        action: Consts.HANDLER_ACTIONS.GET_COVERAGE_LIST
    };
    this.Send("Loading Coverages...", params, callback);
};

FilterDataManager.prototype.GetStateOfJurisdiction = function (isAll, callback) {
    var params = {
        action: Consts.HANDLER_ACTIONS.GET_STATE_OF_JURISDICTION,
        isAll : isAll
    };
    this.Send("Loading States...", params, callback);
};

FilterDataManager.prototype.GetClientAnalysis1 = function (filterValue, callback) {
    var params = {
        action: Consts.HANDLER_ACTIONS.GET_CLIENT_ANALYSIS_1,
        filterValue: filterValue
    };
    this.Send("Loading Client Analysis...", params, callback);
};

FilterDataManager.prototype.GetClientAnalysis2 = function (filterValue, callback) {
    var params = {
        action: Consts.HANDLER_ACTIONS.GET_CLIENT_ANALYSIS_2,
        filterValue: filterValue
    };
    this.Send("Loading Client Analysis...", params, callback);
};

FilterDataManager.prototype.GetClientAnalysis3 = function (filterValue, callback) {
    var params = {
        action: Consts.HANDLER_ACTIONS.GET_CLIENT_ANALYSIS_3,
        filterValue: filterValue
    };
    this.Send("Loading Client Analysis...", params, callback);
};

FilterDataManager.prototype.GetClientAnalysis4 = function (filterValue, callback) {
    var params = {
        action: Consts.HANDLER_ACTIONS.GET_CLIENT_ANALYSIS_4,
        filterValue: filterValue
    };
    this.Send("Loading Client Analysis...", params, callback);
};

FilterDataManager.prototype.GetClientAnalysis5 = function (filterValue, callback) {
    var params = {
        action: Consts.HANDLER_ACTIONS.GET_CLIENT_ANALYSIS_5,
        filterValue: filterValue
    };
    this.Send("Loading Client Analysis...", params, callback);
};

FilterDataManager.prototype.GetClasses = function (coverages, callback) {
    var params = {
        action: Consts.HANDLER_ACTIONS.GET_CLASSES,
        coverages: coverages
    };
    this.Send("Loading Classes...", params, callback);
};

FilterDataManager.prototype.HideBlockUI = function () {
    if (this.LayerCount == 0) {
        $.unblockUI();
    }
};

FilterDataManager.prototype.GetMembers = function (filterValue, selectedValue, callback) {
    var params = {
        action: Consts.HANDLER_ACTIONS.GET_MEMBERS,
        filterValue: filterValue,
        selectedValue: selectedValue
    };
    this.Send("Loading Members...", params, callback);
};

FilterDataManager.prototype.GetLocations = function (filterValue, selectedValue, memberNumber, callback) {
    var params = {
        action: Consts.HANDLER_ACTIONS.GET_LOCATIONS,
        filterValue: filterValue,
        selectedValue: selectedValue,
        memberNumber: memberNumber
    };
    this.Send("Loading Locations...", params, callback);
};

FilterDataManager.prototype.GetGroupCode1s = function (filterValue, selectedValue, memberNumber, location, callback) {
    var params = {
        action: Consts.HANDLER_ACTIONS.GET_GROUP_CODE1S,
        filterValue: filterValue,
        selectedValue: selectedValue,
        memberNumber: memberNumber,
        location: location
    };
    this.Send("Loading Group Code 1...", params, callback);
};

FilterDataManager.prototype.GetGroupCode2s = function (filterValue, selectedValue, memberNumber, location, group1Code, callback) {
    var params = {
        action: Consts.HANDLER_ACTIONS.GET_GROUP_CODE2S,
        filterValue: filterValue,
        selectedValue: selectedValue,
        memberNumber: memberNumber,
        location: location,
        group1Code: group1Code
    };
    this.Send("Loading Group Code 2...", params, callback);
};

FilterDataManager.prototype.GetGroupCode3s = function (filterValue, selectedValue, memberNumber, location, group1Code, group2Code, callback) {
    var params = {
        action: Consts.HANDLER_ACTIONS.GET_GROUP_CODE3S,
        filterValue: filterValue,
        selectedValue: selectedValue,
        memberNumber: memberNumber,
        location: location,
        group1Code: group1Code,
        group2Code: group2Code
    };
    this.Send("Loading Group Code 3...", params, callback);
};

FilterDataManager.prototype.GetGroupCode4s = function (filterValue, selectedValue, memberNumber, location, group1Code, group2Code, group3Code, callback) {
    var params = {
        action: Consts.HANDLER_ACTIONS.GET_GROUP_CODE4S,
        filterValue: filterValue,
        selectedValue: selectedValue,
        memberNumber: memberNumber,
        location: location,
        group1Code: group1Code,
        group2Code: group2Code,
        group3Code: group3Code
    };
    this.Send("Loading Group Code 4...", params, callback);
};

FilterDataManager.prototype.Send = function(caption, params, callback) {
    $.blockUI({ message: caption, css: { marginLeft: '35%', marginRight: '35%', width: '30%', top: '45%'} });
    this.LayerCount++;
    var parentObj = this;
    this.sendAjax(Consts.HANDLERS.SEARCH_HANDLER, { type: 'POST', params: JSON.stringify(params) },
        function (data) {
            parentObj.LayerCount--;
            callback.call(parentObj, data);
            parentObj.HideBlockUI();
        }, function (e) { }
    );
};

FilterDataManager.prototype.sendAjax = function (url, params, callbackSuccess, callbackError) {
    $.ajax({
        type: "POST",
        dataType: 'json',
        url: url,
        data: params,
        success: callbackSuccess,
        error: callbackError
    });
};