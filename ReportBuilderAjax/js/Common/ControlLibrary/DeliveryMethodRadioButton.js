var DeliveryMethodRadioButton = function (id, template) {
    this.Element = document.getElementById(id);
    this.Buttons = new Array();
    this.SelectedValue = null;

    var attr = {
        "idDeliveryMethodRadio": "DeliveryMethodRadio"
    };

    var templ = TemplateEngine.Format(template, attr);
    $(document.getElementById(id)).append(templ);
};

DeliveryMethodRadioButton.prototype.Bind = function () {
    var obj = this;
    obj.Buttons = $(this.Element).find('div[DeliveryMethodRadio=""]');
    this.Buttons.each(function () {
        $(this).bind('click', function () {
            obj.Buttons.each(function () {
                $(this).removeClass('DeliveryMethodRadioActive');
                $(this).addClass('DeliveryMethodRadioInactive');
            });
            $(this).addClass('DeliveryMethodRadioActive');
            $(this).removeClass('DeliveryMethodRadioInactive');
            obj.SelectedValue = $(this).attr("Value");
        });
    });
};

DeliveryMethodRadioButton.prototype.SetValue = function (value) {
    var obj = this;
    this.Buttons.each(function () {
        if ($(this).attr("Value") == value) {
            $(this).addClass('DeliveryMethodRadioActive');
            $(this).removeClass('DeliveryMethodRadioInactive');
            obj.SelectedValue = value;
        }
        else {
            $(this).removeClass('DeliveryMethodRadioActive');
            $(this).addClass('DeliveryMethodRadioInactive');
        }
    });
};

DeliveryMethodRadioButton.prototype.GetValue = function () {
    return this.SelectedValue;
};

DeliveryMethodRadioButton.prototype.Show = function () {
    $(this.Element).show();
};

DeliveryMethodRadioButton.prototype.Hide = function () {
    $(this.Element).hide();
};