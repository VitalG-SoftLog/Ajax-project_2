function CheckpointsConfiguration(id) {
    this.Element = document.getElementById(id);
    this.Data = new Array();
    this.IsLoaded = false;
    this.CheckpointContainer = null;
    this.ReportFolderContainer = null;
    this.DescriptionContainer = null;
    this.Controls = new Array();
    this.SelectedCheckPoint = "";
};
CheckpointsConfiguration.prototype.Render = function (data) {
    this.Element.innerHTML = '';
    var parentObj = this;
    $(this.Element).append(TemplateEngine.Format(TemplateManager.templates['CheckpointsConfigurationTemplate_RootTemplate'], {}));
    this.CheckpointContainer = $("#leftCheckpointsConfigurationReportsContainerId", this.Element);
    this.ReportFolderContainer = $("#rightCheckpointsConfigurationReportsContainerId", this.Element);
    this.DescriptionContainer = $("#checkpointsConfigurationReportDescription", this.Element);
    this.Data = data;

    $('#checkpointsConfigurationSaveButtonId').click(function () {
        parentObj.SaveConfiguration.call(parentObj);
    });

    for (var i in this.Data.Checkpoints) {
        var attr = {
            "checkpointsConfigurationItemId": this.getClientID(this.Data.Checkpoints[i].CheckpointID.trim().replace('&', '_')),
            "checkpointId": this.Data.Checkpoints[i].CheckpointID.trim().replace('&', '_'),
            "checkpointsConfigurationName": this.Data.Checkpoints[i].CheckpointID.trim()
        };

        this.CheckpointContainer.append(TemplateEngine.Format(TemplateManager.templates['CheckpointsConfigurationTemplate_CheckpointsConfigurationItem'], attr));

        $("#" + this.getClientID(this.Data.Checkpoints[i].CheckpointID.trim().replace('&', '_')), this.Element).click(function () {
            var checkpointID = this.getAttribute("checkpointId");
            parentObj.GetReportFoldersByCheckPointID(checkpointID);
            parentObj.SelectedCheckPoint = checkpointID;
            $("div[checkpointId]").removeClass("checkpoints_item_selected");
            $("div[checkpointId]").addClass("checkpoints_item");
            this.className = "checkpoints_item_selected";

            for (var j in parentObj.Data.Checkpoints) {
                if (parentObj.Data.Checkpoints[j].CheckpointID.trim().replace('&', '_') == checkpointID) {
                    parentObj.DescriptionContainer.html(parentObj.Data.Checkpoints[j].CheckpointDescription.trim());
                    break;
                }
            }
        });
    }
    $.unblockUI();

};

CheckpointsConfiguration.prototype.Bind = function () {
    var params = {
        action: "GetCheckpointsConfigurationData"
    };

    var parentObj = this;
    this.sendAjax(Consts.HANDLERS.SEARCH_HANDLER, { type: 'POST', params: JSON.stringify(params) }, function (data) {
        parentObj.Render.call(parentObj, data);
    }, this.sendErrorCallback);
};


CheckpointsConfiguration.prototype.Load = function() {
    this.Bind();
};

CheckpointsConfiguration.prototype.GetReportFoldersByCheckPointID = function (checkPointId) {
    $.blockUI({ message: "Loading Data...", css: { marginLeft: '40%', marginRight: '30%', width: '20%', top: '30%'} });
    var params = {
        action: "GetReportsForCheckpoint",
        checkpointId : checkPointId
    };

    var parentObj = this;
    this.sendAjax(Consts.HANDLERS.SEARCH_HANDLER, { type: 'POST', params: JSON.stringify(params) }, function (data) {
        parentObj.SetReportFoldersByCheckPointID.call(parentObj, data);
    }, this.sendErrorCallback);
};

CheckpointsConfiguration.prototype.SetReportFoldersByCheckPointID = function (data) {
    this.Controls = new Array();
    var parentObj = this;
    this.ReportFolderContainer.html("");
    var countFolders = 0;
    for (var i in this.Data.ReportFolders) {

        if (this.Data.ReportFolders[i].ReportFolderID == 1) continue;
        countFolders++;
        var attr = {
            "checkpointsConfigurationReportFolderCheckboxId": this.getClientID(this.Data.ReportFolders[i].ReportFolderID),
            "checkpointsConfigurationReportCheckboxName": this.Data.ReportFolders[i].ReportFolderName,
            "folderID": this.Data.ReportFolders[i].ReportFolderID,
            "folderNumber": countFolders,
            'folderNameID': data.Reports[i].ReportFolderID
        };

        this.ReportFolderContainer.append(TemplateEngine.Format(TemplateManager.templates['CheckpointsConfigurationTemplate_CheckpointsConfigurationReportItem'], attr));
        if (countFolders == 1) {
            $('div[folderNumber = 1]').css('border-top', '1px solid transparent');
        }

        this.Controls[this.Data.ReportFolders[i].ReportFolderID] = new newCheckBoxControl(this, this.getClientID(this.Data.ReportFolders[i].ReportFolderID), "", 0);
        this.Controls[this.Data.ReportFolders[i].ReportFolderID].ReportFolderID = this.Data.ReportFolders[i].ReportFolderID;
        this.Controls[this.Data.ReportFolders[i].ReportFolderID].DataBind();

        this.Controls[this.Data.ReportFolders[i].ReportFolderID].EventSet.Click.List.reportCheck = function () {
            for (var control in parentObj.Controls) {
                if (parentObj.Controls[control].IsReport &&
                        parentObj.Controls[control].ReportFolderID == this.ReportFolderID) {

                    parentObj.Controls[control].SetValue(this.GetValue());
                }
            }

        };
    }

    for (var i in data.Reports) {

        var attr = {
            "checkpointsConfigurationReportCheckboxId": this.getClientID(data.Reports[i].ID),
            "checkpointsConfigurationReportCheckboxName": data.Reports[i].Name,
            "parentID": data.Reports[i].ReportFolderID
        };

        $('div[folderID=' + data.Reports[i].ReportFolderID + ']').after(TemplateEngine.Format(TemplateManager.templates['CheckpointsConfigurationTemplate_CheckpointsConfigurationReportSubItem'], attr));

        this.Controls[data.Reports[i].ID] = new newCheckBoxControl(this, this.getClientID(data.Reports[i].ID), "", data.Reports[i].IsUseWithCheckpoint ? 1 : 0);
        this.Controls[data.Reports[i].ID].ID = data.Reports[i].ID;
        this.Controls[data.Reports[i].ID].Name = data.Reports[i].Name;
        this.Controls[data.Reports[i].ID].Description = data.Reports[i].Description;
        this.Controls[data.Reports[i].ID].FilterSystemType = data.Reports[i].FilterSystemType;
        this.Controls[data.Reports[i].ID].ReportFolderID = data.Reports[i].ReportFolderID;
        this.Controls[data.Reports[i].ID].IsReport = true;
        this.Controls[data.Reports[i].ID].DataBind();
        this.Controls[data.Reports[i].ID].SetValue(data.Reports[i].IsUseWithCheckpoint ? 1 : 0);

        this.Controls[data.Reports[i].ID].EventSet.Click.List.reportCheck = function () {
            if (this.GetValue() == 1) {
                parentObj.Controls[this.ReportFolderID].SetValue(1);
            }
            else {
                var allUnchecked = true;
                for (var control in parentObj.Controls) {
                    if (parentObj.Controls[control].IsReport &&
                        parentObj.Controls[control].ReportFolderID == this.ReportFolderID &&
                        parentObj.Controls[control].GetValue() == 1) {

                        allUnchecked = false;
                        break;
                    }
                }

                if (allUnchecked) parentObj.Controls[this.ReportFolderID].SetValue(0);
            }
        };

        this.Controls[data.Reports[i].ReportFolderID].SetValue(data.Reports[i].IsUseWithCheckpoint ? 1 : 0); ///
    }

    $('div[folderNameID]').click(function () {

        var folderID = this.getAttribute("folderNameID");
        var children = $('div[parentID=' + folderID + ']');
        if (children.length > 0) {
            if (children[0].style.display == 'none') {
                this.className = 'active_checkpoints_checkbox_item_name';
                folderID++;
                $('div[folderID = ' + folderID + ']').css('border-top', '1px solid #c7c7c7');
                children.show();
            }
            else {
                this.className = 'checkpoints_checkbox_item_name';
                folderID++;
                $('div[folderID = ' + folderID + ']').css('border-top', '1px solid transparent');
                children.hide();
            }
        }

    });


    $('div[folderNumber]').mouseenter(function () {

        var folderNumber = this.getAttribute("folderNumber");
        if ($('.active_checkpoints_checkbox_item_name', this).length > 0) {
            $('div[folderNumber = ' + folderNumber + ']').css('border-top', '1px solid transparent');
        } else {
            $('div[folderNumber = ' + folderNumber + ']').css('border-top', '1px solid transparent');
            folderNumber++;
            $('div[folderNumber = ' + folderNumber + ']').css('border-top', '1px solid transparent');
        }

    }).mouseleave(function () {
        var folderNumber = this.getAttribute("folderNumber");
        if (folderNumber == 1) {
            folderNumber++;
            $('div[folderNumber = ' + folderNumber + ']').css('border-top', '1px solid #c7c7c7');
        } else {
            $('div[folderNumber = ' + folderNumber + ']').css('border-top', '1px solid #c7c7c7');
            folderNumber++;
            $('div[folderNumber = ' + folderNumber + ']').css('border-top', '1px solid #c7c7c7');
        }
    });

    $.unblockUI();
};


CheckpointsConfiguration.prototype.SaveConfiguration = function () {
    $.blockUI({ message: "Saving Data...", css: { marginLeft: '40%', marginRight: '30%', width: '20%', top: '30%'} });
    var reports = new Array();

    for (var i in this.Controls) {
        if (this.Controls[i].IsReport && this.Controls[i].GetValue() == 1) {
            var item = {
                "ID": this.Controls[i].ID,
                "Name": this.Controls[i].Name,
                "Description": this.Controls[i].Description,
                "FilterSystemType": this.Controls[i].FilterSystemType,
                "ReportFolderID": this.Controls[i].ReportFolderID,
                "IsUseWithCheckpoint": this.Controls[i].GetValue() == 1 ? true : false
            };

            reports.push(item);
        }
    }


    var params = {
        action: "SetReportsForCheckpoint",
        checkpointId: this.SelectedCheckPoint,
        reports: reports
    };

    var parentObj = this;
    this.sendAjax(Consts.HANDLERS.SEARCH_HANDLER, { type: 'POST', params: JSON.stringify(params) }, function () {
        parentObj.Bind();
    }, this.sendErrorCallback);
};


CheckpointsConfiguration.prototype.getClientID = function (name) {
    return this.Element.id + '_' + name;
};

CheckpointsConfiguration.prototype.sendAjax = function (url, params, callbackSuccess, callbackError) {
    $.ajax({
        type: "POST",
        dataType: 'json',
        url: url,
        data: params,
        success: callbackSuccess,
        error: callbackError
    });
};

CheckpointsConfiguration.prototype.sendErrorCallback = function (e) {
    alert('Sorry, internal server error.');
};