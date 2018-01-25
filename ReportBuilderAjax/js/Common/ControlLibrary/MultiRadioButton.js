var MultiRadioButton = function(id, template) {
    this.Element = document.getElementById(id);
    this.Buttons = new Array();
    this.SelectedValue = null;

    var attr = {
        "idRadio": "radioId"
    };

    var templ = TemplateEngine.Format(template, attr);
    $(document.getElementById(id)).append(templ);
};

MultiRadioButton.prototype.Bind = function () {
    var obj = this;
    obj.Buttons = $(this.Element).find('div[isRadio=""]');
    this.Buttons.each(function () {
        $(this).bind('click', function () {
            obj.Buttons.each(function () {
                $(this).removeClass('radioActive');
                $(this).addClass('radioInactive');
            });
            $(this).addClass('radioActive');
            $(this).removeClass('radioInactive');
            obj.SelectedValue = $(this).attr("Value");
        });
    });
};

MultiRadioButton.prototype.SetValue = function (value) {
    var obj = this;
    this.Buttons.each(function () {
        if ($(this).attr("Value") == value) {
            $(this).addClass('radioActive');
            $(this).removeClass('radioInactive');
            obj.SelectedValue = value;
        }
        else {
            $(this).removeClass('radioActive');
            $(this).addClass('radioInactive');
        }
    });
};

MultiRadioButton.prototype.GetValue = function () {
    return this.SelectedValue;
};

MultiRadioButton.prototype.Show = function () {
    $(this.Element).show();
};

MultiRadioButton.prototype.Hide = function () {
    $(this.Element).hide();
};

MultiRadioButton.prototype.GetBoolValue = function () {
    return this.GetValue() == 1;
};