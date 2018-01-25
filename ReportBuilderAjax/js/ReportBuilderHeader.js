function ReportBuilderHeader(id) {
    this.Element = document.getElementById(id);
    this.ReportBuilderForm = null;
    this.ReportSelectForm = null;
    this.ReleaseNotes = null;
    this.DeliveredReports = null;
    this.CheckpointsConfiguration = null;
    this.ReportCustomLogo = null;
    this.TabsContent = new Array();
    this.Data = null;
    this.ClientData = null;
    this.ClientTempData = null;
    this.Controls = new Array();
};

ReportBuilderHeader.prototype.Load = function () {
    if (this.Data == null) {
        var params = {
            action: Consts.HANDLER_ACTIONS.GET_HEADER_DATA
        };
        var parentObj = this;
        $.blockUI({ message: "Please wait loading...", css: { marginLeft: '35%', marginRight: '35%', width: '30%', top: '45%'} });
        this.sendAjax(Consts.HANDLERS.SEARCH_HANDLER, { type: 'POST', params: JSON.stringify(params) }, function (data) {
            parentObj.successfullGetData.call(parentObj, data);
        }, function (e) { parentObj.sendErrorCallbackForGetData.call(parentObj, e); });
    } else {
        this.Bind();
        this.ShowReportSelectForm();
    }
};

ReportBuilderHeader.prototype.SwitchClient = function (value) {
    var params = {
        action: Consts.HANDLER_ACTIONS.SWITCH_CLIENT,
        AssociationNumber: value
    };
    var parentObj = this;
    $.blockUI({ message: "Please wait loading...", css: { marginLeft: '35%', marginRight: '35%', width: '30%', top: '45%'} });
    this.sendAjax(Consts.HANDLERS.SEARCH_HANDLER, { type: 'POST', params: JSON.stringify(params) }, function (data) {
        window.location = window.location;
    }, function (e) {
        parentObj.sendErrorCallbackForGetData.call(parentObj, e);
    });
};

ReportBuilderHeader.prototype.successfullGetData = function (data) {
    this.Data = data;
    this.ClientData = data.ClientList;
    this.Bind();
    $.unblockUI();
    this.ShowReportSelectForm();
};


ReportBuilderHeader.prototype.sendErrorCallbackForGetData = function (e) {
    $.unblockUI();
    this.sendErrorCallback(e);
    if (e.responseText.indexOf("timeout") > -1) {
        this.goToMain();
    }
};

ReportBuilderHeader.prototype.sendErrorCallback = function (e) {
    this.IsBusy = false;

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

ReportBuilderHeader.prototype.goToMain = function () {

};

ReportBuilderHeader.prototype.sendAjax = function (url, params, callbackSuccess, callbackError) {
    $.ajax({
        type: "POST",
        dataType: 'json',
        url: url,
        data: params,
        success: callbackSuccess,
        error: callbackError
    });
};

ReportBuilderHeader.prototype.ShowReportSelectForm = function () {
    this.ReportSelectForm = new ReportSelectForm(this.getClientID('reportSelectFormPlaceHolder'), this);
    this.ReportSelectForm.Load();
    document.getElementById(this.getClientID('reportSelectFormPlaceHolder')).style.display = '';
    this.HideReportBuilderForm();
};

ReportBuilderHeader.prototype.HideReportSelectForm = function () {
    document.getElementById(this.getClientID('reportSelectFormPlaceHolder')).style.display = 'none';
};

ReportBuilderHeader.prototype.ShowReportBuilderForm = function (reportId, userReportId) {
    this.ReportBuilderForm = new ReportBuilderForm(this.getClientID('reportBuilderFormPlaceHolder'), this);
    this.ReportBuilderForm.Load(reportId, userReportId);
    document.getElementById(this.getClientID('reportBuilderFormPlaceHolder')).style.display = '';
    this.HideReportSelectForm();
};

ReportBuilderHeader.prototype.HideReportBuilderForm = function () {
    document.getElementById(this.getClientID('reportBuilderFormPlaceHolder')).style.display = 'none';
};

ReportBuilderHeader.prototype.Bind = function () {
    var obj = this;

    var assnNumber = this.Data["assnNumber"];

    var assnDisplay = '';

    for (var i = 0; i < this.ClientData.length; i++) {
        if (this.ClientData[i].AssnNum == assnNumber) {
            assnDisplay = this.ClientData[i].AssnDisplay;
        }
    }

    var attr = {
        'reportBuilderTabMainPlaceHolderId': this.getClientID('reportBuilderTabMainPlaceHolderId'),
        'userName': this.Data["userName"],
        'switchClientName': assnDisplay
    };

    var header = TemplateEngine.Format(TemplateManager.templates['ReportBuilderHeader_Header'], attr);
    $(this.Element).append(header);

    var reportBuilderRootAttr = {
        'reportBuilderTabPlaceHolder': this.getClientID('reportBuilderTabPlaceHolder'),
        'reportBuilderFormPlaceHolder': this.getClientID('reportBuilderFormPlaceHolder'),
        'reportSelectFormPlaceHolder': this.getClientID('reportSelectFormPlaceHolder')
    };
    var reportBuilderRootTemplate = TemplateEngine.Format(TemplateManager.templates['ReportBuilderHeader_ReportBuilderRootTemplate'], reportBuilderRootAttr);
    $(this.Element).append(reportBuilderRootTemplate);
    this.TabsContent.ReportBuilderTabPlaceHolder = document.getElementById(this.getClientID('reportBuilderTabPlaceHolder'));

    var releaseNotesRootAttr = {
        'releaseNotesTabPlaceHolder': this.getClientID('releaseNotesTabPlaceHolder')
    };
    var releaseNotesRootTemplate = TemplateEngine.Format(TemplateManager.templates['ReportBuilderHeader_ReleaseNotesRootTemplate'], releaseNotesRootAttr);
    $(this.Element).append(releaseNotesRootTemplate);
    this.TabsContent.ReleaseNotesTabPlaceHolder = document.getElementById(this.getClientID('releaseNotesTabPlaceHolder'));
    this.ReleaseNotes = new ReleaseNotes(this.getClientID('releaseNotesTabPlaceHolder'));

    var deliveredReportRootAttr = {
        'deliveredReportsTabPlaceHolder': this.getClientID('deliveredReportsTabPlaceHolder')
    };
    var deliveredReportsRootTemplate = TemplateEngine.Format(TemplateManager.templates['ReportBuilderHeader_DeliveredReportsRootTemplate'], deliveredReportRootAttr);
    $(this.Element).append(deliveredReportsRootTemplate);
    this.TabsContent.DeliveredReportsTabPlaceHolder = document.getElementById(this.getClientID('deliveredReportsTabPlaceHolder'));
    this.DeliveredReports = new DeliveredReports(this.getClientID('deliveredReportsTabPlaceHolder'));


    var customLogoAttr = {
        'customLogoPlaceHolder': this.getClientID('customLogoPlaceHolder')
    };

    var checkpointsConfigurationRootAttr = {
        'checkpointsConfigurationTabPlaceHolder': this.getClientID('checkpointsConfigurationTabPlaceHolder')
    };
    var checkpointsConfigurationRootRootTemplate = TemplateEngine.Format(TemplateManager.templates['ReportBuilderHeader_CheckpointsConfigurationRootTemplate'], checkpointsConfigurationRootAttr);
    $(this.Element).append(checkpointsConfigurationRootRootTemplate);
    this.TabsContent.CheckpointsConfigurationTabPlaceHolder = document.getElementById(this.getClientID('checkpointsConfigurationTabPlaceHolder'));
    this.CheckpointsConfiguration = new CheckpointsConfiguration(this.getClientID('checkpointsConfigurationTabPlaceHolder'));
    var customLogoRootTemplate = TemplateEngine.Format(TemplateManager.templates['ReportBuilderHeader_CustomLogoRootTemplate'], customLogoAttr);
    $(this.Element).append(customLogoRootTemplate);

    this.TabsContent.CustomLogoTabPlaceHolder = document.getElementById(this.getClientID('customLogoPlaceHolder'));
    this.ReportCustomLogo = new ReportCustomLogo(this.getClientID('customLogoPlaceHolder'));

    /*************/
    //Start Report Builder Tab
    var ReportBuilderTabAttr = {
        "idReportBuilderTab": this.getClientID("reportBuilder"),
        "idReportBuilderTabSkin": this.getClientID("reportBuilderSkin")
    };

    var ReportBuilderTabTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderHeader_ReportBuilderTabTemplate'], ReportBuilderTabAttr);
    $(document.getElementById("ReportBuilderTab")).append(ReportBuilderTabTempl);

    var reportBuilderTabObj = this;
    $('#' + this.getClientID("reportBuilder")).bind("click", function () {
        reportBuilderTabObj.ReportBuilderActiveTab();
    });
    //End Report Builder Tab

    //Start Delivered Reports Tab
    var DeliveredReportsTabAttr = {
        "idDeliveredReportsTab": this.getClientID("deliveredReports"),
        "idDeliveredReportsTabSkin": this.getClientID("deliveredReportsSkin")
    };

    var DeliveredReportsTabTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderHeader_DeliveredReportsTabTemplate'], DeliveredReportsTabAttr);
    $(document.getElementById("DeliveredReportsTab")).append(DeliveredReportsTabTempl);

    var deliveredReportsTabObj = this;
    $('#' + this.getClientID("deliveredReports")).bind("click", function () {
        deliveredReportsTabObj.DeliveredReportsActiveTab();
    });
    //End Delivered Reports Tab

    //Start Checkpoints Configuration Tab
    var CheckpointsConfigurationTabAttr = {
        "idCheckpointsConfigurationTab": this.getClientID("checkpointsConfiguration"),
        "idCheckpointsConfigurationTabSkin": this.getClientID("checkpointsConfigurationSkin")
    };

    var CheckpointsConfigurationTabTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderHeader_CheckpointsConfigurationTabTemplate'], CheckpointsConfigurationTabAttr);
    $(document.getElementById("CheckpointsConfigurationTab")).append(CheckpointsConfigurationTabTempl);

    var checkpointsConfigurationTabObj = this;
    $('#' + this.getClientID("checkpointsConfiguration")).bind("click", function () {
        checkpointsConfigurationTabObj.CheckpointsConfigurationActiveTab();
    });
    //End Checkpoints Configuration Tab

    //Start Custom Logo Tab
    var CustomLogoTabAttr = {
        "idCustomLogoTab": this.getClientID("customLogo"),
        "idCustomLogoTabSkin": this.getClientID("customLogoSkin")
    };

    var CustomLogoTabTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderHeader_CustomLogoTabTemplate'], CustomLogoTabAttr);
    $(document.getElementById("CustomLogoTab")).append(CustomLogoTabTempl);

    var customLogoTabObj = this;
    $('#' + this.getClientID("customLogo")).bind("click", function () {
        customLogoTabObj.CustomLogoActiveTab();
    });
    //End Custom Logo Tab

    //Start Release Notes Tab
    var ReleaseNotesTabAttr = {
        "idReleaseNotesTab": this.getClientID("releaseNotes"),
        "idReleaseNotesTabSkin": this.getClientID("releaseNotesSkin")
    };

    var ReleaseNotesTabTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderHeader_ReleaseNotesTabTemplate'], ReleaseNotesTabAttr);
    $(document.getElementById("ReleaseNotesTab")).append(ReleaseNotesTabTempl);

    var releaseNotesTabObj = this;
    $('#' + this.getClientID("releaseNotes")).bind("click", function () {
        releaseNotesTabObj.ReleaseNotesActiveTab();
    });
    //End Release Notes Tab

    //Start Logout button
    var logoutBttnAttr = {
        "idLogoutBttn": this.getClientID("logOut")
    };

    var logoutBttnTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderHeader_LogoutBttnTemplate'], logoutBttnAttr);
    $(document.getElementById("LogoutBttn")).append(logoutBttnTempl);
    $(document.getElementById("LogoutBttn")).bind("click", function () {
        document.getElementById(ASP_LOGOUT_BUTTON_ID).click();
    });
    //End Logout button

    //Start Switch Client button
    var SwitchClientBttnAttr = {
        "idDropDown": this.getClientID("switchClDropDown"),
        "dropDownHide": "dropDownHide",
        "idSwitchClient": this.getClientID("switchClient")
    };

    var SwitchClientBttnTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderHeader_SwitchClientBttnTemplate'], SwitchClientBttnAttr);
    $(document.getElementById("SwitchClient")).append(SwitchClientBttnTempl);
    $(document.getElementById("SwitchClient")).bind("click", function () {
        obj.SwitchClientClick();
    });

    var SwitchClientPopinTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderHeader_SwitchClientPopin'], SwitchClientBttnAttr);
    $(document.getElementById("SwitchClient_button")).append(SwitchClientPopinTempl);

    var SwitchClientDropDownTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_DropDownControl'], SwitchClientBttnAttr);
    $(document.getElementById("SCDropDown")).append(SwitchClientDropDownTempl);

    this.Controls.SwitchClientDropDownTempl = new newDropDownControl(this, SwitchClientBttnAttr["idDropDown"], "", "", ValueType.String, null, false, 319);
    $(document.getElementById("main_div_switchClDropDown_showDiv")).attr("readonly", "");

    $(document.getElementById("main_div_switchClDropDown_showDiv")).bind("keyup", function () {
        obj.updateClientList();
    });

    $(document.getElementById("SwitchClient_cancelButton")).bind("click", function () {
        document.getElementById("SwitchClientPopin").style.display = "none";
        //        $.unblockUI();
        document.getElementById("blockDisplay").style.display = "none";
    });

    $(document.getElementById("SwitchClient_switchButton")).bind("click", function () {
        obj.SwitchClientClickSwitch();
    });
    //End Switch Client button
};

ReportBuilderHeader.prototype.updateClientList = function () {

    this.ClientTempData = new Array();
    document.getElementById("main_div_switchClDropDown").innerHTML = "";
    var inputValue = document.getElementById("main_div_switchClDropDown_showDiv").value;
    for (var i = 0; i < this.ClientData.length; i++) {
        if (this.ClientData[i].AssnDisplay.toLowerCase().indexOf(inputValue.toLowerCase()) != -1) {
            this.ClientTempData[this.ClientTempData.length] = this.ClientData[i];
        }
    };
    for (var idx = 0; idx < this.ClientTempData.length; idx++) {
        this.Controls.SwitchClientDropDownTempl.AppendOption(this.ClientTempData[idx].AssnDisplay, this.ClientTempData[idx].AssnDisplay);
    }

    if (inputValue == '') {
        for (var ind = 0; ind < this.ClientData.length; ind++) {
            this.Controls.SwitchClientDropDownTempl.AppendOption(this.ClientData[ind].AssnDisplay, this.ClientData[ind].AssnDisplay);
        }        
    }

    if (this.ClientTempData.length <= 11)
        document.getElementById("main_div_switchClDropDown").style.height = "";
    if (this.ClientTempData.length == 0)
        document.getElementById("main_div_switchClDropDown_ImageTable").style.display = "none";
    else
        document.getElementById("main_div_switchClDropDown_ImageTable").style.display = "";
};

ReportBuilderHeader.prototype.ChangeSwitchClient = function (value) {
    var clientName = document.getElementById('switchClientName');
    clientName.innerHTML = value;
};

ReportBuilderHeader.prototype.SwitchClientClick = function () {
    this.updateClientList();
    document.getElementById("SwitchClientPopin").style.display = "";
    //$.blockUI({ message: null, css: { marginLeft: '35%', marginRight: '35%', width: '30%', top: '45%'} });

    document.getElementById("blockDisplay").style.height = $(document).height() + 'px';

    document.getElementById("blockDisplay").style.display = "";

    var parentObj = this;

//    this.Controls.SwitchClientDropDownTempl.SetSelect = function (sender) {
//        this.SetDropDownValue(sender);
//        parentObj.ChangeSwitchClient(sender);
//        this.ShowHideDropDown(this.Element.id);
//    };
};

ReportBuilderHeader.prototype.SwitchClientClickSwitch = function () {

    for (var i = 0; i < this.ClientData.length; i++) {
        if (document.getElementById("main_div_switchClDropDown_showDiv").value == this.ClientData[i].AssnDisplay) {
            this.SwitchClient(this.ClientData[i].AssnNum);
            return;
        }
    }
    alert('Under construction!');
    return;
    //        alert('Under construction!');
    //    SwitchClient();
};

ReportBuilderHeader.prototype.ReportBuilderActiveTab = function () {
    if ($('#' + this.getClientID("reportBuilderSkin")).css() != 'one') {
        $('#' + this.getClientID("reportBuilderSkin")).removeClass("two");
        $('#' + this.getClientID("reportBuilderSkin")).addClass("one");
        $('#' + this.getClientID("deliveredReportsSkin")).removeClass("one");
        $('#' + this.getClientID("deliveredReportsSkin")).addClass("two");
        $('#' + this.getClientID("checkpointsConfigurationSkin")).removeClass("one");
        $('#' + this.getClientID("checkpointsConfigurationSkin")).addClass("two");
        $('#' + this.getClientID("customLogoSkin")).removeClass("one");
        $('#' + this.getClientID("customLogoSkin")).addClass("two");
        $('#' + this.getClientID("releaseNotesSkin")).removeClass("one");
        $('#' + this.getClientID("releaseNotesSkin")).addClass("two");
        this.AllNotActiveTab();
        this.showTabContent('ReportBuilderTabPlaceHolder');
    }
};

ReportBuilderHeader.prototype.DeliveredReportsActiveTab = function () {
    if ($('#' + this.getClientID("deliveredReportsSkin")).css() != 'one') {
        $('#' + this.getClientID("reportBuilderSkin")).removeClass("one");
        $('#' + this.getClientID("reportBuilderSkin")).addClass("two");
        $('#' + this.getClientID("deliveredReportsSkin")).removeClass("two");
        $('#' + this.getClientID("deliveredReportsSkin")).addClass("one");
        $('#' + this.getClientID("checkpointsConfigurationSkin")).removeClass("one");
        $('#' + this.getClientID("checkpointsConfigurationSkin")).addClass("two");
        $('#' + this.getClientID("customLogoSkin")).removeClass("one");
        $('#' + this.getClientID("customLogoSkin")).addClass("two");
        $('#' + this.getClientID("releaseNotesSkin")).removeClass("one");
        $('#' + this.getClientID("releaseNotesSkin")).addClass("two");
        if(this.DeliveredReports.IsLoaded != true)
        {
            this.AllNotActiveTab();
        }
        this.DeliveredReports.Load();
        this.showTabContent('DeliveredReportsTabPlaceHolder');
    }
};

ReportBuilderHeader.prototype.CheckpointsConfigurationActiveTab = function () {
    if ($('#' + this.getClientID("checkpointsConfigurationSkin")).css() != 'one') {
        $('#' + this.getClientID("reportBuilderSkin")).removeClass("one");
        $('#' + this.getClientID("reportBuilderSkin")).addClass("two");
        $('#' + this.getClientID("deliveredReportsSkin")).removeClass("one");
        $('#' + this.getClientID("deliveredReportsSkin")).addClass("two");
        $('#' + this.getClientID("checkpointsConfigurationSkin")).removeClass("two");
        $('#' + this.getClientID("checkpointsConfigurationSkin")).addClass("one");
        $('#' + this.getClientID("customLogoSkin")).removeClass("one");
        $('#' + this.getClientID("customLogoSkin")).addClass("two");
        $('#' + this.getClientID("releaseNotesSkin")).removeClass("one");
        $('#' + this.getClientID("releaseNotesSkin")).addClass("two");
        this.AllNotActiveTab();
        this.CheckpointsConfiguration.Load();
        this.showTabContent('CheckpointsConfigurationTabPlaceHolder');
    }
};

ReportBuilderHeader.prototype.CustomLogoActiveTab = function () {
    if ($('#' + this.getClientID("customLogoSkin")).css() != 'one') {
        $('#' + this.getClientID("reportBuilderSkin")).removeClass("one");
        $('#' + this.getClientID("reportBuilderSkin")).addClass("two");
        $('#' + this.getClientID("deliveredReportsSkin")).removeClass("one");
        $('#' + this.getClientID("deliveredReportsSkin")).addClass("two");
        $('#' + this.getClientID("checkpointsConfigurationSkin")).removeClass("one");
        $('#' + this.getClientID("checkpointsConfigurationSkin")).addClass("two");
        $('#' + this.getClientID("customLogoSkin")).removeClass("two");
        $('#' + this.getClientID("customLogoSkin")).addClass("one");
        $('#' + this.getClientID("releaseNotesSkin")).removeClass("one");
        $('#' + this.getClientID("releaseNotesSkin")).addClass("two");
        this.AllNotActiveTab();
        this.ReportCustomLogo.Render();
        this.showTabContent('CustomLogoTabPlaceHolder');
    }
};

ReportBuilderHeader.prototype.ReleaseNotesActiveTab = function () {
    if ($('#' + this.getClientID("customLogoSkin")).css() != 'one') {
        $('#' + this.getClientID("reportBuilderSkin")).removeClass("one");
        $('#' + this.getClientID("reportBuilderSkin")).addClass("two");
        $('#' + this.getClientID("deliveredReportsSkin")).removeClass("one");
        $('#' + this.getClientID("deliveredReportsSkin")).addClass("two");
        $('#' + this.getClientID("checkpointsConfigurationSkin")).removeClass("one");
        $('#' + this.getClientID("checkpointsConfigurationSkin")).addClass("two");
        $('#' + this.getClientID("customLogoSkin")).removeClass("one");
        $('#' + this.getClientID("customLogoSkin")).addClass("two");
        $('#' + this.getClientID("releaseNotesSkin")).removeClass("two");
        $('#' + this.getClientID("releaseNotesSkin")).addClass("one");
        if (this.ReleaseNotes.IsLoaded != true) {
            this.AllNotActiveTab();
        }
        this.ReleaseNotes.Load();
        this.showTabContent('ReleaseNotesTabPlaceHolder');
    }
};
ReportBuilderHeader.prototype.AllNotActiveTab = function () {
    //this.DeliveredReports.IsLoaded = false;
    this.DeliveredReports.IsLoaded = false;
    //this.CheckpointsConfiguration.IsLoaded = false;
    //this.ReportCustomLogo.IsLoaded = false;
    this.ReleaseNotes.IsLoaded = false;
};

ReportBuilderHeader.prototype.getClientID = function (name) {
    return this.Element.id + '_' + name;
};

ReportBuilderHeader.prototype.showTabContent = function (name) {
    for (var tabName in this.TabsContent) {
        this.TabsContent[tabName].style.display = ((name == tabName) ? '' : 'none');
    }
};
