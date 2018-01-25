function CrossBrowseElement(id, handlerName, parentId, action, isForPrint, webFilePath, getFilePath, quoteId, wasSilverlightInstalled, parentObj,  docType, attchmentId) {
    this.Element = $get(id);
    this.ParentObj = parentObj;
    this.ParentClass = null;
    this.HandlerName = handlerName;
    this.ParentId = parentId;
    this.Action = action;
    this.IsUploaded = false;
    this.UploadedFileName = '';
    this.FileName = '';
    this.DateUploaded = '';
    this.FileSize = '';
    this.FileInfo = '';
    this.IsForPrint = isForPrint;
    this.WebFilePath = webFilePath;
    this.GetFilePath = getFilePath;
    this.QuoteId = quoteId;
    this.WasSilverlightInstalled = wasSilverlightInstalled;
    this.FileControl = null;
    this.Uploader = null;
    this.CurrentRuntime = '';
    this.DocumentType = docType;
    this.AttachmentId = attchmentId;
};

CrossBrowseElement.prototype.Render = function () {
    if (!this.IsForPrint) {
        var attributes = {
            'div1': this.getClientID('div1'),
            'div2': this.getClientID('div2'),
            'div3': this.getClientID('div3'),
            'progressBarId': this.getClientID('progressBarId'),
            'uploadedFileName': this.getClientID('uploadedFileName'),
            'mainTableId': this.getClientID('mainTableId'),
            'browseInputId': this.getClientID('browseInputId'),
            'fileInputId': this.getClientID('fileInputId'),
            'multiUploadEl': this.getClientID('multiUploadEl'),
            'browseInputName': this.getClientID('browseInputName'),
            'uploadFormId': this.getClientID('uploadFormId'),
            'uploadFormName': this.getClientID('uploadFormName'),
            'parentTdId': this.getClientID('parentTdId'),
            'linkId': this.getClientID('linkId'),
            'dateUploadedId': this.getClientID('dateUploadedId'),
            'deleteBtn': this.getClientID('deleteBtn'),
            'jsid': this.Element.id
        };

        this.Element.innerHTML = TemplateEngine.Format(TemplateManager.templates['CrossBrowseElement_RootTemplate'], attributes);

        var parentObj = this;

        $('#' + this.getClientID('deleteBtn')).click(function () {
            parentObj.DeleteFileFromServer.call(parentObj, false);
        });

        var allowedFileTypes = 'doc,docx,xls,txt,bmp,jpg,jpeg,png,pdf';

        var IE = /MSIE/.test(navigator.userAgent);

        this.Uploader = new plupload.Uploader({
            id: this.getClientID('browseInputId') + '_upload' ,
            runtimes: 'html5,silverlight,html4',
            browse_button: this.getClientID('browseInputId')+ '_label',
            max_file_size: '10mb',
            chunk_size: '100kb',
            unique_names: true,
            /*filters: [
			    { title: "Allowed formats", extensions: allowedFileTypes }
		    ],*/
            url: this.HandlerName + '?params=' + JSON.stringify({ action: 'UploadAttachmentsWithUploadStatus' }),
            flash_swf_url: 'js/Common/Plupload/plupload.flash.swf',
            silverlight_xap_url: 'js/Common/Plupload/plupload.silverlight.xap',
            container: this.getClientID('browseInputId') ,
            parentItem: this,
            progressBarElement: $get(this.getClientID('progressBarId'))
        });

        this.Uploader.bind('Init', function (up, params) {
            var parentObj = up.settings.parentItem;
            parentObj.CurrentRuntime = params.runtime;
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

        this.Uploader.bind('FilesAdded', function (up, files) {
            var parentObj = up.settings.parentItem;
            //if (parentObj.ParentObj.IsBusy) return;
            parentObj.ParentObj.IsBusy = true;
            setTimeout(
            Function.Delegate(this, function () {
                this.start();
                if (parentObj.ParentObj && parentObj.ParentObj.SectionsInfo)
                    parentObj.ParentObj.SectionsInfo[8].Busy = true;
            }), 100);
        });

        this.Uploader.bind('Error', function (up, file) {
            if (file.code == plupload.FILE_SIZE_ERROR) {
                alert('The size of uploaded document exceeds allowed maximum (10M).');
            }
            else if (file.code == plupload.FILE_EXTENSION_ERROR) {
                alert("Invalid file format!");
            }
            else {
                alert(file.message);
            }
            var parentObj = up.settings.parentItem;
            parentObj.ShowUploadControl();
            parentObj.ParentObj.IsBusy = false;
        });

        this.Uploader.bind('UploadProgress', function (up, file) {
            var parentObj = up.settings.parentItem;
            parentObj.ShowProgressBar();
        });

        this.Uploader.bind('CancelUpload', function (up, file) {
            
        });

        this.Uploader.bind('ChunkUploaded', function (up, file, resp) {
            var progressBar = up.settings.progressBarElement;
            if (progressBar != null) {
                var totalPercents = up.files.length * 100;
                var currentPercents = 0;
                for (var key in up.files) {
                    currentPercents += up.files[key].percent;
                }
                var part = currentPercents == 0 ? 0 : totalPercents / currentPercents;

                $(progressBar).css('width', part == 0 ? '0px' : 170 / part + 'px');
            }
        });

        this.Uploader.bind('UploadComplete', function (up, files) {
            var parentObj = up.settings.parentItem;
            for (var key in up.files) {
                if (key * 1 == 0) {
                    parentObj.UploadFileToServer(parentObj, up.files[key].target_name, up.files[key].name);
                }
                else {
                    var browseEl = parentObj.ParentClass.AddItem();
                    browseEl.UploadFileToServer(parentObj, up.files[key].target_name, up.files[key].name);
                }
            }
            parentObj.ParentObj.IsBusy = false;
        });

        this.Uploader.bind('UploadFile', function (up, file) {
            $('<input type="hidden" name="file-' + file.id + '" value="' + file.name + '" />')
		    .appendTo('#submit-form');
        });

        this.Uploader.init();

        if (this.CurrentRuntime == 'html4') {
            $('form', '#' + this.getClientID('browseInputId')).each(function (idx) {
                $(this).css("position", "static").css("height", "19px").css("width", "68px");
            });
        }
        else {
            $('div', '#' + this.getClientID('browseInputId')).each(function (idx) {
                $(this).css("position", "static").css("height", "19px").css("width", "68px");
            });
            $('a', '#' + this.getClientID('browseInputId')).each(function (idx) {
                $(this).css("position", "static");
            });
            if (this.CurrentRuntime == 'silverlight') {
                $('div', '#' + this.getClientID('browseInputId')).each(function (idx) {
                    $(this).css("opacity", "1");
                });
            }
        }

        SI.Files.stylizeAll();
    }
    else {
        var attr = {
            'linkId': this.getClientID('linkId'),
            'mainTableId': this.getClientID('mainTableId')
        };

        this.Element.innerHTML = TemplateEngine.Format(TemplateManager.templates['CrossBrowseElement_PrintRootTemplate'], attr);
    }
};

CrossBrowseElement.prototype.EnableControls = function () {
    var parentObj = this;

    var browseBtn = $get(this.getClientID('browseInputId'));

    $(browseBtn).mouseup(function () {
        $(browseBtn).removeClass("cabinet_push");
    }).mousedown(function () {
        $(browseBtn).addClass("cabinet_push");
    }).mouseleave(function () {
        $(browseBtn).removeClass("cabinet_push");
    });

    $($get(this.getClientID('deleteBtn'))).unbind('click');

    this.FileControl = $get(this.getClientID('fileInputId'));

    var parentObj = this;

    var deleteBtn = $get(this.getClientID('deleteBtn'));

    
    $(deleteBtn).click(function () {
        parentObj.DeleteFileFromServer.call(parentObj, false);
    });
    


    $(deleteBtn).mouseup(function () {
        $(this).removeClass("deleteDocument_push");
    }).mousedown(function () {
        $(this).addClass("deleteDocument_push");
    }).mouseleave(function () {
        $(this).removeClass("deleteDocument_push");
    });
};

CrossBrowseElement.prototype.DisableControls = function() {
};

CrossBrowseElement.prototype.UploadFileToServer = function (sender, fileName, realFileName) {

    $.blockUI({ message: "Please wait loading...", css: { marginLeft: '35%', marginRight: '35%', width: '30%', top: '45%'} });
    var params = {
        action: this.Action,
        fileName: fileName,
        realFileName: realFileName,
        quoteId: this.ParentObj.QuoteId,
        attachment: { 'AttachmentId': 0, 'DocumentTypeId': this.DocumentType, 'FileName': fileName, 'ActualFileName': realFileName, 'QuoteId': this.ParentObj.QuoteId }
    };

    var parentObj = this;
    this.sendAjax(Consts.HANDLERS.SEARCH_HANDLER, { type: 'POST', params: JSON.stringify(params) }, function (data) {
        parentObj.SuccessUploadFile.call(parentObj, this, data);
    }, this.sendErrorCallback);
};

CrossBrowseElement.prototype.SuccessUploadFile = function(sender, data) {
    if ($get(this.getClientID('div1'))) {
        $get(this.getClientID('div1')).style.display = '';
    }
    if ($get(this.getClientID('div2'))) {
        $get(this.getClientID('div2')).style.display = 'none';
    }
    $get(this.getClientID('progressBarId') + '_container').style.display = 'none';
    $get(this.getClientID('fileInputId')).style.display = '';    

    var h6Element = document.createElement('h3');

    var h4Element = document.createElement('h4');
    h4Element.innerHTML = data.FileName;
    
    var aElement = document.createElement('a');
    aElement.innerHTML = data.FileName;
   aElement.href = this.GetFilePath + '?savedFileName=' + data.FileSaveName + '&originalFileName=' + data.FileName + '&quoteId=' + this.ParentObj.QuoteId + '&attachmentId=' + this.AttachmentId;

    h6Element.appendChild(aElement);

    $get(this.getClientID('uploadedFileName')).appendChild(h6Element);
    $get(this.getClientID('uploadedFileName')).appendChild(h4Element);  

    $get(this.getClientID('fileInputId')).value = '';
    this.IsUploaded = true;
    this.UploadedFileName = data.FileSaveName;
    this.FileName = data.FileName;
    this.DateUploaded = data.DateUploaded;
    this.FileSize = data.FileSize;
    this.FileInfo = data.FileInfo;
    this.ParentClass.AddAttachment(data.AttachmentId ? data.AttachmentId : 0, this.DocumentType, data.FileSaveName, data.FileName);
    if (this.ParentObj.SectionsInfo)
        this.ParentObj.SectionsInfo[8].Busy = false;

    $.unblockUI();
};

CrossBrowseElement.prototype.ShowProgressBar = function () {
    if ($get(this.getClientID('div1'))) {
        $get(this.getClientID('div1')).style.display = 'none';
    }
    if ($get(this.getClientID('div2'))) {
        $get(this.getClientID('div2')).style.display = '';
    }
    $get(this.getClientID('progressBarId') + '_container').style.display = '';
    $get(this.getClientID('fileInputId')).style.display = 'none';    
};

CrossBrowseElement.prototype.ShowUploadControl = function () {
    if ($get(this.getClientID('div1'))) {
        $get(this.getClientID('div1')).style.display = 'none';
    }
    if ($get(this.getClientID('div2'))) {
        $get(this.getClientID('div2')).style.display = '';
    }
    $get(this.getClientID('progressBarId') + '_container').style.display = 'none';
    $get(this.getClientID('fileInputId')).style.display = '';    

    if (this.Uploader.runtime == 'html4') {
        this.Uploader.destroy();
        this.Uploader = null;
        this.Render();  
    }
};

CrossBrowseElement.prototype.ErrorUploadingFile = function(sender, data) {
    alert('The size of uploaded document exceeds allowed maximum (10M).');
    if ($get(this.getClientID('div1'))) {
        $get(this.getClientID('div1')).style.display = 'none';
    }
    if ($get(this.getClientID('div2'))) {
        $get(this.getClientID('div2')).style.display = '';
    }
    $get(this.getClientID('progressBarId') + '_container').style.display = 'none';
    $get(this.getClientID('fileInputId')).style.display = '';    
    this.Render();
    this.IsUploaded = false;
    this.UploadedFileName = '';
    this.FileName = '';
};

CrossBrowseElement.prototype.sendErrorCallback = function (e) {
    if (e) {
        if (e.responseText.indexOf('MAPS Login Page') > -1) {
            returnToLogin(winAuthPath + '?ReturnURL=MAPSPage.aspx%3f' + 'QuoteId=' + this.ParentObj.QuoteId + '%26ActiveSectionNumber=' + this.ParentObj.ActiveSectionNumber);
            return;
        }
    }    
    else {
        alert('Sorry, internal server error.');
    }
};

CrossBrowseElement.prototype.DeleteFileFromServer = function (isSectionDelete) {
    var params = {
        action: Consts.MAPS_HANDLER_ACTIONS.DELETE_FILE,
        fileName: this.UploadedFileName,
        quoteId: this.ParentObj.QuoteId,
        attachmentId: this.AttachmentId
    };

    var parentObj = this;
    this.sendAjax(Consts.HANDLERS.SEARCH_HANDLER, { type: 'POST', params: JSON.stringify(params) }, function (data) {
        parentObj.successFileDelete.call(parentObj, data, isSectionDelete);
    }, function (e) { parentObj.sendErrorCallback.call(parentObj, e); });
};

CrossBrowseElement.prototype.successFileDelete = function (result, isSectionDelete) {
    if (result.IsError == true) {
        alert('Error removing file!');
    }
    else {
        if ($get(this.getClientID('div1'))) {
            $get(this.getClientID('div1')).style.display = 'none';
        }
        if ($get(this.getClientID('div2'))) {
            $get(this.getClientID('div2')).style.display = '';
        }
        $get(this.getClientID('progressBarId') + '_container').style.display = 'none';
        $get(this.getClientID('fileInputId')).style.display = '';

        this.ParentClass.DeleteAttachment(this.AttachmentId, this.UploadedFileName, this.Element);
        
        this.Uploader.destroy();
        this.Uploader = null;
    }
};

CrossBrowseElement.prototype.getInputElement = function() {
    return this.getClientID('fileInputId');
};

CrossBrowseElement.prototype.UrlChanged = function(sender) {
    var links = sender.value.split('\\');
    $get(this.getClientID('fileInputId')).value = '...\\' + links[links.length - 1];
    this.UploadFileToServer();
};

CrossBrowseElement.prototype.GetValue = function() {
    return $get(this.getClientID('browseInputId')).value;
};

CrossBrowseElement.prototype.sendAjax = function(url, params, callbackSuccess, callbackError) {
    $.ajax({
        type: "POST",
        dataType: 'json',
        url: url,
        data: params,
        success: callbackSuccess,
        error: callbackError
    });
};

CrossBrowseElement.prototype.getClientID = function(name){
    return this.Element.id + '_' + name;
};



