
(function() {
    function sendAjax(url, params, callbackSuccess, callbackError) {
        $.ajax({
            type: "POST",
            dataType: 'json',
            url: url,
            data: params,
            success: callbackSuccess,
            error: callbackError
        });
    };

    function registrateTemplateSuccessCallback(data, alias) {
        for (var template in data.Templates) {
            var parts = template.split('_');
            var controlName = parts[0];
            this.templates[controlName] = "bla";
            this.templates[template] = data.Templates[template];
        }
    };

    function registrateTemplateErrorCallback(data) {
        alert("Error");
    };

    function registrateAllTemplatesSuccessCallback(data) {
        for (var template in data.Templates) {
            var parts = template.split('_');
            var controlName = parts[0];
            this.templates[controlName] = "bla";
            this.templates[template] = data.Templates[template];
        }
    };

    function registrateControlTemplateSuccessCallback(data, controlName) {
        var parentObj = this;
        this.templates[controlName] = "bla";
        for (var template in data.Templates) {
            this.templates[template] = data.Templates[template];
        }
    };

    TemplateManager = {
        templates: {},
        registrate: function(alias, fileName, scope, onSuccess) {
            if (typeof this.templates[alias] == "undefined") {
                var parentObj = this;
                sendAjax(Consts.HANDLERS.SEARCH_HANDLER, { params: JSON.stringify({ action: Consts.TEMPLATE_ACTION.GET_TEMPLATE, fileName: path }) }, function(data, onSuccess, scope) { registrateTemplateSuccessCallback.call(parentObj, data, alias); if (onSuccess) { onSuccess.call(scope) } }, registrateTemplateErrorCallback);
            }
            else {
                onSuccess.call(scope);
            }
        },

        registrateAll: function(onSuccess, scope) {
            var haveTemplates = false;
            for (var template in this.templates) {
                haveTemplates = true;
                break;
            }
            if (haveTemplates) {
                onSuccess.call(scope);
            }
            else {
                var parentObj = this;
                sendAjax(Consts.HANDLERS.SEARCH_HANDLER, { params: JSON.stringify({ action: Consts.TEMPLATE_ACTION.GET_ALL_TEMPLATES }) }, function(data) {
                        registrateAllTemplatesSuccessCallback.call(parentObj, data);
                        if (onSuccess) {
                            onSuccess()
                        }
                    },
                    registrateTemplateErrorCallback);
            }
        },

        registrateControlTemplates: function(controlName, scope, onSuccess) {
            if (typeof this.templates[controlName] == "undefined") {
                var parentObj = this;
                sendAjax(Consts.HANDLERS.SEARCH_HANDLER, { params: JSON.stringify({ action: Consts.TEMPLATE_ACTION.GET_TEMPLATE, fileName: controlName }) }, function(data) {
                    registrateControlTemplateSuccessCallback.call(parentObj, data, controlName);
                    if (onSuccess) { onSuccess.call(scope) }
                },
                 registrateTemplateErrorCallback);
            }
            else {
                onSuccess.call(scope);
            }
        }
    };
})();