var FilterBase = function () {

}

FilterBase.prototype.sendAjax = function (url, params, callbackSuccess, callbackError) {
    $.ajax({
        type: "POST",
        dataType: 'json',
        url: url,
        data: params,
        success: callbackSuccess,
        error: callbackError
    });
};

FilterBase.prototype.getClientID = function (name) {
    return this.Element.id + '_' + name;
};

