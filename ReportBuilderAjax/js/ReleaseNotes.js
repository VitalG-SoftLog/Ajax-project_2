function ReleaseNotes(id) {
    this.Element = document.getElementById(id);
    this.Data = new Array();
    this.IsLoaded = false;  
};

ReleaseNotes.prototype.Load = function () {
    if (this.IsLoaded) return;
    this.Element.innerHTML = '';
    var params = {
        action: Consts.HANDLER_ACTIONS.GET_RELEASE_NOTES_DATA
    };

    var parentObj = this;
    $.blockUI({ message: "Please wait loading...", css: { marginLeft: '35%', marginRight: '35%', width: '30%', top: '45%'} });
    this.sendAjax(Consts.HANDLERS.SEARCH_HANDLER, { type: 'POST', params: JSON.stringify(params) }, function (data) {
        parentObj.successfullGetData.call(parentObj, data);
    }, function (e) { parentObj.sendErrorCallbackForGetData.call(parentObj, e); });
};

ReleaseNotes.prototype.openDocumentClick = function (releaseNoteId) {
    var params = {
        action: Consts.HANDLER_ACTIONS.UPDATE_DOCUMENT_STATUS,
        releaseNoteID: releaseNoteId
    };
    var parentObj = this;
    $.blockUI({ message: "Please wait loading...", css: { marginLeft: '35%', marginRight: '35%', width: '30%', top: '45%'} });
    this.sendAjax(Consts.HANDLERS.SEARCH_HANDLER, { type: 'POST', params: JSON.stringify(params) }, function (data) {
        parentObj.successfullUpdateDocumentStatus.call(parentObj, data);
    }, function (e) { parentObj.sendErrorCallbackForGetData.call(parentObj, e); });
};

ReleaseNotes.prototype.successfullUpdateDocumentStatus = function (data) {
    this.IsLoaded = false;
    this.Load();
    $.unblockUI();
};

ReleaseNotes.prototype.successfullGetData = function (data) {
    this.Data = data;
    this.IsLoaded = true;
    this.Bind();
    $.unblockUI();
};

ReleaseNotes.prototype.Bind = function () {
    var obj = this;
    var url = GetUrlForResource('DownloadFile.aspx');
    var rowsPlaceHolderTemplate = '';
    for (var idx in this.Data.ReleaseNotes) {
        var note = this.Data.ReleaseNotes[idx];
        var rowAttr = {
            'rowClass': (idx%2 != 0) ? 'even' : 'odd',
            'releaseNoteID' : note.ReleaseNoteID,
            'openDocumentLinkId' : this.getClientID("openDocumentLinkId") + note.ReleaseNoteID,
            'showUnRead': note.IsReleaseNoteRead ? 'display:none;' : '',
            'herf': url + "?" + note.DocumentLink,
            'release': note.ReleaseNoteNumber,
            'date': $.datepicker.formatDate('mm/dd/yy', new Date(note.ReleaseNoteDate)) + " " + returnTime(note.ReleaseNoteDate),
            'reliaseSummary': note.ReleaseNoteTitle
        };
        rowsPlaceHolderTemplate += TemplateEngine.Format(TemplateManager.templates['ReleaseNotesTemplate_RowTemplate'], rowAttr);
    }

    var rootAttr = {
        'rowsPlaceHolder': rowsPlaceHolderTemplate
    };
    var mainDivLeft = TemplateEngine.Format(TemplateManager.templates['ReleaseNotesTemplate_RootTemplate'], rootAttr);
    $(this.Element).append(mainDivLeft);

    for (var idx in this.Data.ReleaseNotes) {
        var note = this.Data.ReleaseNotes[idx];
        if (!note.IsReleaseNoteRead) {
            $('#' + this.getClientID("openDocumentLinkId") + note.ReleaseNoteID).bind("click", function () {
                obj.openDocumentClick($(this).attr("releaseNoteID"));
            });
        }        
    }
};

ReleaseNotes.prototype.sendAjax = function (url, params, callbackSuccess, callbackError) {
    $.ajax({
        type: "POST",
        dataType: 'json',
        url: url,
        data: params,
        success: callbackSuccess,
        error: callbackError
    });
};

ReleaseNotes.prototype.sendErrorCallbackForGetData = function (e) {
    $.unblockUI();
    this.sendErrorCallback(e);
    if (e.responseText.indexOf("timeout") > -1) {
        this.goToMain();
    }
};

ReleaseNotes.prototype.sendErrorCallback = function (e) {
    this.IsBusy = false;
    /*
    if (e.responseText.indexOf('MAPS Login Page') > -1) {
    returnToLogin(winAuthPath + '?ReturnURL=MAPSPage.aspx%3f' + 'QuoteId=' + this.QuoteId + '%26ActiveSectionNumber=' + this.ActiveSectionNumber);
    return;
    }
    */
    if (e.responseText.indexOf('Authentication failed') > -1) {
        alert('Authentication failed. Please try to login again.');
    }
    else if (e.status == 0 || e.status == 12029) {
        alert('Your connection has been lost. Please reconnect to the Internet before continuing with this form.');
    }
    else if (e.responseText.indexOf("timeout") > -1) {
        alert("Database timeout. Please try again later");
    }
    else {
        alert('Sorry, internal server error.');
    }
};


ReleaseNotes.prototype.goToMain = function () {
    /*try {
    window.location = Consts.PAGES.GRID_PAGE + '?currentPage=' + this.CurrentPage + '&filterId=' + this.FilterId + '&sortABC=' + this.SortABC + '&searchParam=' + ReplaceQuoteCharToUnicodeString(this.SearchParam);
    } catch(e) {

    }*/
};

ReleaseNotes.prototype.getClientID = function (name) {
    return this.Element.id + '_' + name;
};

