function DeliveredReports(id) {
    this.Element = document.getElementById(id);
    this.Data = new Array();
    this.IsLoaded = false;
    this.OpenedIds = new Array();
    this.CurrentPage = 1;
    this.FirstPagerElement = 1;
    this.LastPagerElement = 1;
    this.AllNumberPager = null;
};
DeliveredReports.prototype.Load = function () {
    if (this.IsLoaded) return;
    this.Element.innerHTML = '';
    var params = {
        action: Consts.HANDLER_ACTIONS.GET_DELIVERED_REPORTS_DATA
    };

    var parentObj = this;
    $.blockUI({ message: "Please wait loading...", css: { marginLeft: '35%', marginRight: '35%', width: '30%', top: '45%'} });
    this.sendAjax(Consts.HANDLERS.SEARCH_HANDLER, { type: 'POST', params: JSON.stringify(params) }, function (data) {
        parentObj.successfullGetData.call(parentObj, data);
    }, function (e) { parentObj.sendErrorCallbackForGetData.call(parentObj, e); });
};


DeliveredReports.prototype.Render = function () {
    this.Element.innerHTML = '';
    $(this.Element).append(TemplateEngine.Format(TemplateManager.templates['DeliveredReportTemplate_RootTemplate'], {}));
    this.DeliveryReportsRender();
    this.DeliveryHistoryRender();
    this.DeliveryHistoryPagerRender();
};

DeliveredReports.prototype.DeliveryReportsRender = function () {

    var leftCustomReportContainerId = $('#leftCustomReportContainerId', this.Element);
    var rightCustomReportContainerId = $('#rightCustomReportContainerId', this.Element);
    leftCustomReportContainerId.html('');
    rightCustomReportContainerId.html('');
    var tempReportOutputs = this.Data.ReportOutputs;

    if (this.Data.ReportOutputs == '') {
        var tableContainerDeliveryReport = $('#tableContainerDeliveryReport', this.Element);
        tableContainerDeliveryReport.css("display", 'none');
    } else {
        for (var i in tempReportOutputs) {
            tempReportOutputs[i].isOpen = false;

            for (var j in this.OpenedIds) {
                if (tempReportOutputs[i].ParentId == this.OpenedIds[j]) tempReportOutputs[i].isOpen = true;
            }
        }

        var leftRightIndicator = true;
        for (var i in tempReportOutputs) {
            if (document.getElementById(this.getClientID('deliveredReportsFolderId_' + tempReportOutputs[i].ParentId)) == null) {
                var attr = {
                    'deliveredReportsFolderId': this.getClientID('deliveredReportsFolderId_' + tempReportOutputs[i].ParentId),
                    'deliveredReportsFolderRootId': this.getClientID('deliveredReportsFolderRootId_' + tempReportOutputs[i].ID),
                    'nameDescription': tempReportOutputs[i].Description,
                    'class': tempReportOutputs[i].isOpen ? "active_custom_report_item" : "custom_report_item",
                    'numberDeliveryFilesId': this.getClientID('numberDeliveryFilesId_' + tempReportOutputs[i].ParentId),
                    'fillingCustomReportItemId': this.getClientID('fillingCustomReportItemId_' + tempReportOutputs[i].ParentId)
                };


                leftRightIndicator == true ? leftCustomReportContainerId.append(TemplateEngine.Format(TemplateManager.templates['DeliveredReportTemplate_DeliveredReportsFolderTemplate'], attr))
                : rightCustomReportContainerId.append(TemplateEngine.Format(TemplateManager.templates['DeliveredReportTemplate_DeliveredReportsFolderTemplate'], attr));
                var parentObj = this;
                $('#' + this.getClientID('deliveredReportsFolderId_' + tempReportOutputs[i].ParentId)).click(function () {
                    var itemId = this.id.split('_')[this.id.split('_').length - 1];
                    if (this.parentNode.className == 'custom_report_item') {
                        this.parentNode.className = 'active_custom_report_item';
                        parentObj.OpenedIds.push(itemId);
                    } else {
                        for (var q in parentObj.OpenedIds) {
                            if (parentObj.OpenedIds[q] == itemId) {
                                var idx = q * 1;
                                parentObj.OpenedIds.splice(idx, 1);
                            }
                        }

                        this.parentNode.className = 'custom_report_item';
                    }

                });

                var idx = i;
                var numberfiles = 0;
                var url = GetUrlForResource('DownloadFile.aspx');
                for (var j in tempReportOutputs) {
                    if (tempReportOutputs[i].ParentId == tempReportOutputs[j].ParentId) {
                        var attFile = {
                            'numberFileClass': (numberfiles % 2 != 0) ? 'second_report' : 'first_report',
                            'nameDeliveredReportFile': tempReportOutputs[j].FileName.split('.')[0],
                            'typeReportFileClass': tempReportOutputs[j].FileName.split('.')[1].toUpperCase() + 'TypeReportFileClass',
                            'linkDeliveryReport': tempReportOutputs[j].FileName == '' ? '' : url + "?Folder=&File=" + tempReportOutputs[j].FileName,
                            'deleteDeliveredReportButtonId': this.getClientID('deleteDeliveredReportButtonId_' + tempReportOutputs[j].ID)
                        };

                        var tempfillingCustomReportItem = $('#' + this.getClientID('fillingCustomReportItemId_' + tempReportOutputs[j].ParentId));
                        $('.directory_tree_last', tempfillingCustomReportItem).each(function () {
                            this.className = 'directory_tree';
                        });

                        tempfillingCustomReportItem.append(TemplateEngine.Format(TemplateManager.templates['DeliveredReportTemplate_DeliveredReportsItemTemplate'], attFile));

                        $('#' + this.getClientID('deleteDeliveredReportButtonId_' + tempReportOutputs[j].ID)).click(function () {

                            var delId = this.id.split('_')[this.id.split('_').length - 1];

                            for (var q in parentObj.Data.ReportOutputs) {
                                if (parentObj.Data.ReportOutputs[q].ID == delId) {
                                    var idx = q * 1;
                                    var tempOutputs = parentObj.Data.ReportOutputs[q];
                                    delete tempOutputs['isOpen'];
                                    parentObj.deleteDeliveryReport(tempOutputs, idx);
                                    //parentObj.Data.ReportOutputs.splice(idx, 1);
                                    //parentObj.DeliveryReportsRender();
                                }
                            }
                        });
                        numberfiles++;
                    }
                }

                $('#' + this.getClientID('numberDeliveryFilesId_' + tempReportOutputs[i].ParentId)).html(' (' + numberfiles + ')');
                leftRightIndicator == true ? leftRightIndicator = false : leftRightIndicator = true;
            }
        }
        
    }
    
};

DeliveredReports.prototype.deleteDeliveryReport = function(reportDelete) {
    var params = {
        action: Consts.HANDLER_ACTIONS.DELETE_DELIVERED_REPORTS,
        deleteReport: reportDelete
    };

    var parentObj = this;
    $.blockUI({ message: "Please wait loading...", css: { marginLeft: '35%', marginRight: '35%', width: '30%', top: '45%'} });
    this.sendDelAjax(Consts.HANDLERS.SEARCH_HANDLER, { type: 'POST', params: JSON.stringify(params) }, function (data) {
        parentObj.successfullGetDataDelete.call(parentObj, data);
    }, function (e) { parentObj.sendErrorCallbackForGetDataDelete.call(parentObj, e); });
};


DeliveredReports.prototype.sendDelAjax = function (url, params, callbackSuccess, callbackError) {
    $.ajax({
        type: "POST",
        dataType: 'json',
        url: url,
        data: params,
        success: callbackSuccess,
        error: callbackError
    });
};

DeliveredReports.prototype.successfullGetDataDelete = function (element, data) {
  
        //this.Data = data;
    this.IsLoaded = true;
    //this.Data.ReportOutputs.splice(idx, 1);
        this.DeliveryReportsRender();
        //$.unblockUI();
    
};

DeliveredReports.prototype.sendErrorCallbackForGetDataDelete = function (element, e) {
    $.unblockUI();
    //this.sendErrorCallback(e);
    //if (e.responseText.indexOf("timeout") > -1) {
        //this.goToMain();
    //}
};


DeliveredReports.prototype.DeliveryHistoryRender = function () {
    var reportDeliveryHistoryContainerId = $('#reportDeliveryHistoryContainerId', this.Element);
    reportDeliveryHistoryContainerId.html('');
    var url = GetUrlForResource('DownloadFile.aspx');
    var tempReportHistory = this.Data.ReportDeliveryLog;
    for (var i in tempReportHistory) {
        var deliveryHistoryAttr = {
            'deliveredReportsHistoryId': this.getClientID('deliveredReportsHistoryId_' + tempReportHistory[i].ReportDeliveryLogID),
            'deliveryOpenReportId': this.getClientID('deliveryOpenReportId_' + tempReportHistory[i].ReportDeliveryLogID),
            'classActiveHistoryReportRow': i == 0 ? 'active_history_report_row' : "",
            'classActiveHistoryRow': i == 0 ? 'active_history_row' : "not_active_history_row",
            'classDeliveryOpenReport': tempReportHistory[i].ReportLink == '' ? 'not_visible_open_report' : 'visible_open_report',
            'linkDeliveryOpenReport': tempReportHistory[i].ReportLink == '' ? '' : url + "?" + tempReportHistory[i].ReportLink,
            'moreInfoItemContainerId': this.getClientID('moreInfoItemContainerId_' + tempReportHistory[i].ReportDeliveryLogID),
            'numberHistoryClass': (i % 2 != 0) ? 'second_history' : 'first_history',
            'deliveryDate': tempReportHistory[i].DeliveryDate,
            'reportDeliveryName': tempReportHistory[i].ReportName,
            'deliveryMethod': tempReportHistory[i].DeliveryMethods,
            'recipientsDelivery': tempReportHistory[i].Recipient,
            'reporDeliverytLink': tempReportHistory[i].ReportLink
        };
        reportDeliveryHistoryContainerId.append(TemplateEngine.Format(TemplateManager.templates['DeliveredReportTemplate_DeliveredReportsHistoryItemTemplate'], deliveryHistoryAttr));

        $('#' + this.getClientID('deliveredReportsHistoryId_' + tempReportHistory[i].ReportDeliveryLogID)).click(function () {
            var tempParent = this.parentNode.parentNode;
            if (this.className != 'active_history_report_row') {
                $('.active_history_report_row').each(function () {
                    this.className = '';
                });
                $('.more_info_container').each(function () {
                    $(this).hide();
                });
                this.className = 'active_history_report_row';
                tempParent.className = '';
                $('.more_info_container', tempParent).show();
            }
        });
        var moreInfoItemContainerId = $('#' + this.getClientID('moreInfoItemContainerId_' + tempReportHistory[i].ReportDeliveryLogID), this.Element);
        for (var j in tempReportHistory[i].Schedules) {
            var scheduleAttr = {
                'schedule_row': (j % 2 != 0) ? 'second_schedule_row' : 'first_schedule_row',
                'recipientsInfo': tempReportHistory[i].Schedules[j].Recipients,
                'scheduleNameInfo': tempReportHistory[i].Schedules[j].ScheduleName,
                'periodInfo': tempReportHistory[i].Schedules[j].Period,
                'scheduleStartInfo':tempReportHistory[i].Schedules[j].ScheduleStart,
                'scheduleStopInfo': tempReportHistory[i].Schedules[j].ScheduleStop,
                'timeInfo': tempReportHistory[i].Schedules[j].TimeOfDay
            };
            $(moreInfoItemContainerId).append(TemplateEngine.Format(TemplateManager.templates['DeliveredReportTemplate_DeliveredReportsHistoryMoreInfoItemTemplate'], scheduleAttr));
        };
    };
};

DeliveredReports.prototype.DeliveryHistoryPagerRender = function () {
    if (this.Data.ReportDeliveryLog[0] == null) {
        $('#reportDeliveryHistoryPagerId').html('');
        return;
    }
    if (this.Data.ReportDeliveryLog[0].TotalCount <= 10) {
        $('#reportDeliveryHistoryPagerId').html('');
        return;
    } else {
        var tempNumberPager = this.Data.ReportDeliveryLog[0].TotalCount;
        var allNumberPager = tempNumberPager / 10 + 1;
        this.AllNumberPager = parseInt(allNumberPager, 10);
        this.AllNumberPager > 5 ? this.LastPagerElement = this.FirstPagerElement + 4 : this.LastPagerElement = this.AllNumberPager;
        this.RenderHistoryPager();
        var parentObj = this;
        $('div.previous_page_delivered_history', this.Element).click(function () {
            if (parentObj.CurrentPage != 1) {
                if (parentObj.CurrentPage - 1 == parentObj.FirstPagerElement && parentObj.FirstPagerElement != 1) {
                    parentObj.CurrentPage--;
                    parentObj.FirstPagerElement--;
                    parentObj.LastPagerElement--;
                    parentObj.LoadHistory();
                } else {
                    parentObj.CurrentPage--;
                    parentObj.LoadHistory();
                }
            }
        });
        $('div.next_page_delivered_history', this.Element).click(function () {
            if (parentObj.CurrentPage != parentObj.AllNumberPager) {
                if (parentObj.CurrentPage + 1 == parentObj.LastPagerElement && parentObj.LastPagerElement != parentObj.AllNumberPager) {
                    parentObj.CurrentPage++;
                    parentObj.FirstPagerElement++;
                    parentObj.LastPagerElement++;
                    parentObj.LoadHistory();
                } else {
                    parentObj.CurrentPage++;
                    parentObj.LoadHistory();
                }
            }
        });
    }
};

DeliveredReports.prototype.RenderHistoryPager = function () {
    var reportDeliveryHistoryPagerId = $('#reportDeliveryHistoryNumberPagerId', this.Element);
    reportDeliveryHistoryPagerId.html('');
    var parentObj = this;
    for (var i = this.FirstPagerElement; i <= this.LastPagerElement; i++) {
        var deliveryHistoryPagerAttr = {
            'numberHistoryPage': i,
            'classNumderHistoryPage': this.CurrentPage == i ? 'active_numder_history_page' : 'numder_history_page',
            'numberHistoryPageId': this.getClientID('numberHistoryPageId_' + i)
        };
        reportDeliveryHistoryPagerId.append(TemplateEngine.Format(TemplateManager.templates['DeliveredReportTemplate_DeliveredHistoryPagerItemTemplate'], deliveryHistoryPagerAttr));
        
        $('#' + this.getClientID('numberHistoryPageId_' + i)).click(function () {
            var currId = this.id.split('_')[this.id.split('_').length - 1];
            if (parentObj.CurrentPage != currId) {
                if (currId == 1) {
                    parentObj.CurrentPage = currId;
                    parentObj.LoadHistory();
                }
                if (currId == parentObj.LastPagerElement) {
                    parentObj.CurrentPage = currId;
                    parentObj.LoadHistory();
                }
                if (currId == parentObj.LastPagerElement && parentObj.LastPagerElement < parentObj.AllNumberPager) {
                    parentObj.CurrentPage = currId;
                    parentObj.FirstPagerElement++;
                    parentObj.LastPagerElement++;
                    parentObj.LoadHistory();
                }
                if (currId == parentObj.FirstPagerElement && parentObj.FirstPagerElement > 1) {
                    parentObj.CurrentPage = currId;
                    parentObj.FirstPagerElement--;
                    parentObj.LastPagerElement--;
                    parentObj.LoadHistory();
                } else {

                    parentObj.CurrentPage = currId;
                    parentObj.LoadHistory();
                }
            }
        });
    }
};

DeliveredReports.prototype.sendAjax = function (url, params, callbackSuccess, callbackError) {
    $.ajax({
        type: "POST",
        dataType: 'json',
        url: url,
        data: params,
        success: callbackSuccess,
        error: callbackError
    });
};

DeliveredReports.prototype.successfullGetData = function (data) {
    this.Data = data;
    this.IsLoaded = true;
    this.Render();
    $.unblockUI();
};

DeliveredReports.prototype.sendErrorCallbackForGetData = function (e) {
    $.unblockUI();
    this.sendErrorCallback(e);
    if (e.responseText.indexOf("timeout") > -1) {
        this.goToMain();
    }
};

DeliveredReports.prototype.LoadHistory = function (e) {

    var params = {
        action: Consts.HANDLER_ACTIONS.GET_DELIVERED_REPORTS_DATA_BY_PAGE , 
        currentPage: this.CurrentPage 
    };

    var parentObj = this;
    $.blockUI({ message: "Please wait loading...", css: { marginLeft: '35%', marginRight: '35%', width: '30%', top: '45%'} });
    this.sendHistoryAjax(Consts.HANDLERS.SEARCH_HANDLER, { type: 'POST', params: JSON.stringify(params) }, function (data) {
        parentObj.successfullGetDataHistory.call(parentObj, data);
    }, function (e) { parentObj.sendErrorCallbackForGetDataHistory.call(parentObj, e); });
};

DeliveredReports.prototype.sendHistoryAjax = function (url, params, callbackSuccess, callbackError) {
    $.ajax({
        type: "POST",
        dataType: 'json',
        url: url,
        data: params,
        success: callbackSuccess,
        error: callbackError
    });
};


DeliveredReports.prototype.successfullGetDataHistory = function (data) {
    this.Data.ReportDeliveryLog = data.ReportDeliveryLog;
    this.DeliveryHistoryRender();
    this.RenderHistoryPager();
    $.unblockUI();
};

DeliveredReports.prototype.sendErrorCallbackForGetDataHistory = function (e) {
    $.unblockUI();
    this.sendErrorCallback(e);
    if (e.responseText.indexOf("timeout") > -1) {
        this.goToMain();
    }
};

DeliveredReports.prototype.getClientID = function (name) {
    return this.Element.id + '_' + name;
};