function ReportTypesControl(id, parentObject) {
    this.ReportFolders = new Array();
    this.Reports = new Array();
    this.Element = document.getElementById(id);
    this.ParentObject = parentObject;
    this.Controls = new Array();
};
ReportTypesControl.prototype.Render = function () {
    this.Element.innerHTML = '';
    $(this.Element).append(TemplateEngine.Format(TemplateManager.templates['ReportSelectForm_ReportTypesRoot'], {}));

    var reportTypesList = $('#reportTypesList', this.Element);

    var num = this.ReportFolders.length - 1;
    var countReportFolder = 0;

    for (var i in this.ReportFolders) {


        if (this.ReportFolders[i].ReportFolderID == 1) continue;
        countReportFolder++;
        var itemAtt = {
            'id': this.getClientID('reportTypeId_' + this.ReportFolders[i].ReportFolderID),
            'class_report': '',
            'class_active': '',
            'class_name_report': '',
            'placeForContainerFields': '',
            'blackCountId': this.getClientID('blackCountId_' + this.ReportFolders[i].ReportFolderID),
            'whiteCountId': this.getClientID('whiteCountId_' + this.ReportFolders[i].ReportFolderID),
            'reportTypeName': this.ReportFolders[i].ReportFolderName.toUpperCase(),
            'reportTypeDescription': this.ReportFolders[i].Description
        };

        switch (countReportFolder) {
            case 1:
                itemAtt["class_report"] = 'first_report';
                itemAtt["class_active"] = 'needful_report';
                itemAtt["class_name_report"] = 'active_report';
                itemAtt["placeForContainerFields"] = 'placeForContainerFieldsId';
                break;
            case num:
                itemAtt["class_report"] = 'last_report';
                itemAtt["class_active"] = 'available_report';
                itemAtt["class_name_report"] = 'name_report';
                break;
            default:
                itemAtt["class_report"] = 'middle_remort';
                itemAtt["class_active"] = 'available_report';
                itemAtt["class_name_report"] = 'name_report';
                break;
        }

        reportTypesList.append(TemplateEngine.Format(TemplateManager.templates['ReportSelectForm_ReportTypeItem'], itemAtt));
        var parentObj = this;
        $('#' + this.getClientID('reportTypeId_' + this.ReportFolders[i].ReportFolderID)).click(function () {
            $('.active_report').each(function () {
                this.className = 'name_report';
            });
            this.className = 'active_report';

            $('.needful_report').each(function () {
                this.className = 'available_report';
            });
            var tempParent = this.parentNode.parentNode;
            tempParent.className = 'needful_report';

            $('div[hideFields="hideFields"]').each(function () {
                $(this).hide();
            });


            $('#' + parentObj.getClientID('reportContainerPlace_' + this.id.split('_')[this.id.split('_').length - 1])).show();
        });

        var fieldAtt = {
            'reportContainerPlace': this.getClientID('reportContainerPlace_' + this.ReportFolders[i].ReportFolderID)
        };

        $('#placeForContainerFieldsId').append(TemplateEngine.Format(TemplateManager.templates['ReportSelectForm_ReportsFieldsRoot'], fieldAtt));


        var reportPlaceList = $('#' + this.getClientID('reportContainerPlace_' + this.ReportFolders[i].ReportFolderID), '#placeForContainerFieldsId');

        var countReport = 0;
        for (var j in this.Reports) {

            if (this.Reports[j].ReportFolderID == this.ReportFolders[i].ReportFolderID) {
                countReport++;

                var repAtt = {
                    'itemId': this.getClientID('reportTypeId_' + this.Reports[j].ID),
                    'reportId': this.Reports[j].ID,
                    'reportName': this.Reports[j].Name,
                    'reportDescription': this.Reports[j].Description
                };
                if (countReport > 8) {
                    $('#rightReportContainerPlace', reportPlaceList).append(TemplateEngine.Format(TemplateManager.templates['ReportSelectForm_ReportNameItem'], repAtt));
                } else {
                    $('#leftReportContainerPlace', reportPlaceList).append(TemplateEngine.Format(TemplateManager.templates['ReportSelectForm_ReportNameItem'], repAtt));
                }
                $('#' + this.getClientID('reportTypeId_' + this.Reports[j].ID)).click(function () {
                    parentObj.ParentObject.editReportButtonClick($(this).attr("reportId"), -1);
                });

            }
        }
        if (countReportFolder != 1) {
            $(reportPlaceList).hide();
        }
        $('#' + this.getClientID('blackCountId_' + this.ReportFolders[i].ReportFolderID)).html(countReport).css('left',countReport > 9 ? '2px' : '5px');
        $('#' + this.getClientID('whiteCountId_' + this.ReportFolders[i].ReportFolderID)).html(countReport).css('left',countReport > 9 ? '2px' : '5px');
    }
};

ReportTypesControl.prototype.getClientID = function (name) {
    return this.Element.id + '_' + name;
};
