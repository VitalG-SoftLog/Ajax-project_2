﻿var newDropDownControl = function (_parentObject, _id, _value, _defaultValue, _valueType, _dependentControls, _filter, width) {

    this.Init(_parentObject, _id, _value, _defaultValue, _valueType, _dependentControls);
    document.getElementById(this.getClientID("width")).style.width = (width + 'px');
    document.getElementById(this.getClientID("showDiv")).style.width = (width - 30 + 'px');
    var obj = this;
    if (_filter) {
        if (width < 350) width = 350;
        document.getElementById(this.getClientID("ImageTable")).style.width = (width + 10 + 'px');
        document.getElementById(this.getClientID("filterInput")).parentElement.parentElement.parentElement.parentElement.style.paddingLeft = ((width - 218) / 2 + 'px');
        this.Element.parentElement.style.minWidth = (width - 15 + 'px');

        $('div[class="dropDown_filterButton"]', this.parentElement).unbind();
        $('div[class="dropDown_filterButton"]', this.parentElement).bind('click', function () {

            var filterText = $('input[class="dropDown_filterCenter_Input"]', this.ParentElement);
            for (var idx=0; idx < filterText.length; idx++) {
                if (filterText[idx].offsetWidth != 0 &&  filterText[idx].offsetHeight != 0) {
                    filterText = filterText[idx];
                }
            };
            obj.FilterValue = filterText.value;
            obj.OnClick();

        });
    }
    else {
        document.getElementById(this.getClientID("filter")).style.display = "none";
        this.Element.parentElement.style.minWidth = (width + 'px');
        document.getElementById(this.getClientID("ImageTable")).style.width = (width + 115 + 'px');
    }
    $(document.getElementById(this.getClientID("width"))).bind("click", function () { obj.ShowHideDropDown(); });
    this.Options = new Array();
    $(".filter").css('display', 'none');
    //$('tr[filterBox=1]', $(this.Element).parents('div[templateId]')[0]).css("display", filterVisible ? '' : 'none');
};

ClassInheritance.Standart(newDropDownControl, DropDownControl);

newDropDownControl.prototype.SetDropDownValue = function (value) {
    if (this.Value == value && this.ValueType != ValueType.Int && this.ValueType != ValueType.Money) return;
    if (this.Element) {
        this.Value = document.getElementById(this.getClientID(value)).innerHTML;
        this.LastValue = this.Value;
        if (typeof (value) == 'string')
            this.Value = this.LastValue.replace(/&gt;/g, '>').replace(/&lt;/g, '<');
        this.DataBind();
        document.getElementById(this.getClientID("showDiv")).value = this.Value;
        document.getElementById(this.getClientID("showDiv")).style.fontFamily = "Arial";
        document.getElementById(this.getClientID("showDiv")).style.fontSize = "11px";
    }
};

newDropDownControl.prototype.ShowHideDropDown = function (value) {
    var dropDown = "";
    if (this.id)
        dropDown = document.getElementById(value + "ImageTable").parentElement;
    else
        dropDown = document.getElementById(this.getClientID("ImageTable"));
    if (dropDown.style.display == "none") {
        dropDown.style.display = "";
    }
    else dropDown.style.display = "none";
};

newDropDownControl.prototype.getClientID = function (name) {
    return this.Element.id + '_' + name;
};

newDropDownControl.prototype.RenderOptions = function () {
    if (this.Element) {
        for (index in this.Options) {
            if (index == "length") continue;
            this.Element.appendChild(this.Options[index]);
        }
    }
};

newDropDownControl.prototype.AppendOption = function (value, html) {
    var obj = this;
    if (this.Element) {
        var option = this.NewOption(value, html);
        var option1 = document.createElement("div");
        this.Options.push(option);
        option.style.backgroundColor = "white";
        option.style.fontFamily = "Arial";
        option.style.paddingTop = "6px";
        option.style.paddingLeft = "7px";
        option.style.backgroundColor = "transparent";
        option1.style.border = "1px solid white";        
        option1.style.fontSize = "11px";
        option1.style.cursor = "pointer";
        option1.style.height = "25px";
        $(option1).append(option);
        $(this.Element).append(option1);
        if (this.Element.childNodes.length > 11) {
            document.getElementById(this.Element.id).style.height = "300px";
            document.getElementById(this.getClientID("ImageTable")).style.height = "310px";
        };
    }
};

newDropDownControl.prototype.NewOption = function (value, html) {
    var obj = this;
    var option = document.createElement("div");
    option.id = this.Element.id + "_" + value;
    option.innerHTML = html;
    option.setAttribute('value', value);
    option.style.whiteSpace = 'nowrap';
    option.style.paddingRight = '18px';
    $(option).bind("click", function () { obj.SetSelect(value); });
    $(option).bind("mouseover", function () { obj.MouseOver(option.parentNode); });
    $(option).bind("mouseout", function () { obj.MouseOut(option.parentNode); });
    return option;
};

newDropDownControl.prototype.SetSelect = function (sender) {
    this.SetDropDownValue(sender);
    this.ShowHideDropDown(this.Element.id);
};

newDropDownControl.prototype.MouseOver = function (sender) {
    sender.style.backgroundImage = "url('./images/background_select_DropDown.png')";
    sender.style.border = "1px solid #168baa";
    sender.style.color = "white";
};

newDropDownControl.prototype.MouseOut = function (sender) {
    sender.style.backgroundImage = "";
    sender.style.backgroundColor = "white";
    sender.style.border = "1px solid white";
    sender.style.color = "black";
};

newDropDownControl.prototype.GetOptionValue = function (text) {
    for (index in this.Options) {
        if (index == "length") continue;
        if (this.Options[index].innerHTML == text) {
            return this.Options[index].getAttribute('value');
        }
    }
    return "";
};

newDropDownControl.prototype.ClearOptions = function (text) {
    for (var i in this.Options) {
        var el = document.getElementById(this.Options[i].id);
        $(el.parentNode).remove();
     //   $('#' + this.Options[i].id).remove();
    }
    this.Options = new Array();
};

newDropDownControl.prototype.ShowFilter = function () {
    $('tr[filterBox=1]', $(this.Element).parents('div[templateId]')[0]).css("display", '');
};

newDropDownControl.prototype.HideFilter = function () {
    $('tr[filterBox=1]', $(this.Element).parents('div[templateId]')[0]).css("display", 'none');
};

newDropDownControl.prototype.OnClick = function () {
    var asd = "";
};