function ReportCustomLogo(id) {
    this.Element = document.getElementById(id);
    this.Data = new Array();
    this.IsLoaded = false;
    this.Uploader = null;
    this.HandlerName = Consts.HANDLERS.SEARCH_HANDLER;
    this.Action = "UploadAttachmentsFileUpload";
};

ReportCustomLogo.prototype.Render = function () {
    this.Element.innerHTML = '';

    var attr = {
        'browseButton': this.getClientID('browseButton'),
        'uploadButton': this.getClientID('uploadButton'),
        'removeButton': this.getClientID('removeButton'),
        'browseInputId': this.getClientID('browseInputId'),
        'fileInputId': this.getClientID('fileInputId')
    };

    $(this.Element).append(TemplateEngine.Format(TemplateManager.templates['CustomLogoReport_CustomLogoRootTemplate'], attr));

    var parentObject = this;

    var uploadButton = document.getElementById(this.getClientID('uploadButton'));
    var removeLogoButton = document.getElementById(this.getClientID('removeButton'));

    this.Uploader = new plupload.Uploader({
        id: this.getClientID('browseButton') + '_upload',
        runtimes: 'html4',
        browse_button: this.getClientID('browseInputId') + '_label',
        max_file_size: '10mb',
        chunk_size: '100kb',
        unique_names: true,
        filters: [{ title: "Image files", extensions: "jpg,gif,png"}],
        url: this.HandlerName + '?params=' + JSON.stringify({ action: 'UploadAttachmentsWithUploadStatus' }),
        flash_swf_url: 'js/Common/Plupload/plupload.flash.swf',
        silverlight_xap_url: 'js/Common/Plupload/plupload.silverlight.xap',
        container: this.getClientID('browseInputId'),
        parentItem: this
    });

    this.Uploader.bind('Init', function (up, params) {
        var parentObj = up.settings.parentItem;
        if (params.runtime == 'html4') {
            $('form', '#' + parentObj.getClientID('browseInputId')).each(function (idx) {
                $(this).css("position", "static").css("height", "19px").css("width", "68px");
            });
        }
        else {
            $('div', '#' + parentObj.getClientID('browseInputId')).each(function (idx) {
                $(this).css("position", "static").css("height", "19px").css("width", "68px");
            });
            $('a', '#' + parentObj.getClientID('browseInputId')).each(function (idx) {
                $(this).css("position", "static");
            });
            if (params.runtime == 'silverlight') {
                $('div', '#' + parentObj.getClientID('browseInputId')).each(function (idx) {
                    $(this).css("opacity", "1");
                });
            }
        }
    });

    $(uploadButton).click(function (e) {
        if (parentObject.Uploader.files.length == 1) {

            parentObject.Uploader.start();
            e.preventDefault();
        } else
            alert('You must at least upload one file.');
        return false;

    });

    $(removeLogoButton).click(function (e) {
        parentObject.RemoveLogo();
    });

    this.Uploader.bind('FilesAdded', function (up, files) {
        var parentObj = up.settings.parentItem;

        var filesValue = '';

        for (var idx in files) {
            var item = files[idx];
            filesValue = item.name + ', ';
        }

        if (filesValue.length > 2) {
            document.getElementById(parentObj.getClientID('fileInputId')).value = filesValue.substring(0, filesValue.length - 2);
        }

        up.refresh();
    });


    this.Uploader.bind('UploadComplete', function (up, files) {
        var parentObj = up.settings.parentItem;
        for (var key in up.files) {
            parentObj.UploadFileToServer(parentObj, up.files[key].target_name, up.files[key].name);
        }
        
    });

    this.Uploader.bind('UploadFile', function (up, file) {
        $('<input type="hidden" name="file-' + file.id + '" value="' + file.name + '" />')
		    .appendTo('#submit-form');
    });
    
    this.Uploader.init();

    SI.Files.stylizeAll();

    this.Uploader.bind('Error', function (up, file) {
        var parentObj = up.settings.parentItem;
        alert('Only image');
        document.getElementById(parentObj.getClientID('fileInputId')).value = '';
    });

    this.ChangeRightHeightPanel();

    var params = {
        action: "GetLogoImage"
    };

    var parentObj = this;
    this.sendAjax(Consts.HANDLERS.SEARCH_HANDLER, { type: 'POST', params: JSON.stringify(params) }, function (data) {
        parentObj.SuccessUploadFile.call(parentObj, this, data);
    }, this.sendErrorCallback);
};


ReportCustomLogo.prototype.UploadFileToServer = function (sender, fileName, realFileName) {

    $.blockUI({ message: "Please wait loading...", css: { marginLeft: '35%', marginRight: '35%', width: '30%', top: '45%'} });
    var params = {
        action: this.Action,
        fileName: fileName,
        realFileName: realFileName
    };

    var parentObj = this;
    this.sendAjax(Consts.HANDLERS.SEARCH_HANDLER, { type: 'POST', params: JSON.stringify(params) }, function (data) {
        parentObj.SuccessUploadFile.call(parentObj, this, data);
    }, this.sendErrorCallback);
};


ReportCustomLogo.prototype.RemoveLogo = function() {
    $.blockUI({ message: "Please wait loading...", css: { marginLeft: '35%', marginRight: '35%', width: '30%', top: '45%'} });
    var params = {
        action: "RemoveCustomLogo"
    };

    var parentObj = this;
    this.sendAjax(Consts.HANDLERS.SEARCH_HANDLER, { type: 'POST', params: JSON.stringify(params) }, function (data) {
        parentObj.SuccessRemoveFile.call(parentObj, this, data);
    }, this.sendErrorCallback);
};

ReportCustomLogo.prototype.SuccessUploadFile = function (sender, data) {
    document.getElementById(this.getClientID('browseInputId')).value = '';

    var logoPreview = document.getElementById('logoPreviewDiv');
    logoPreview.className = '';
    var img = $('img', logoPreview)[0];
    img.style.display = '';
    img.src = './images/emptyLogoButton.png';
    if (data.FileSaveName != "") img.src = data.FileSaveName;
    this.Uploader.files.splice(0,1);
    document.getElementById(this.getClientID('fileInputId')).value = '';
    $.unblockUI();
};

ReportCustomLogo.prototype.SuccessRemoveFile = function (data) {
    var logoPreview = document.getElementById('logoPreviewDiv');
    var img = $('img', logoPreview)[0];
    img.style.display = '';
    img.src = './images/emptyLogoButton.png';

    $.unblockUI();
}; 

ReportCustomLogo.prototype.ChangeRightHeightPanel = function() {
    var height = ($(document.getElementById("leftPanelCustomLogoDiv")).height() * 1);
    var rightPanel = document.getElementById("rightPanelCustomLogoDiv");
    rightPanel.style.height = height + 'px';
};

ReportCustomLogo.prototype.getClientID = function (name) {
    return this.Element.id + '_' + name;
};

ReportCustomLogo.prototype.sendAjax = function(url, params, callbackSuccess, callbackError) {
    $.ajax({
        type: "POST",
        dataType: 'json',
        url: url,
        data: params,
        success: callbackSuccess,
        error: callbackError
    });
};

ReportCustomLogo.prototype.sendErrorCallback = function (e) {
    alert('Sorry, internal server error.');    
};