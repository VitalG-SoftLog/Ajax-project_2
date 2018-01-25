var DropDownControl = function(_parentObject, _id, _value, _defaultValue, _valueType, _dependentControls) {
    this.Init(_parentObject, _id, _value, _defaultValue, _valueType, _dependentControls);
    this.Type = ControlType.DropDown;
    this.Options = new Array();
};

//Inheritance DropDownControl from ControlAbstractClass

ClassInheritance.Standart(DropDownControl, ControlAbstractClass);

//Entending end overwriting class prototype members

DropDownControl.prototype.CreateControl = function() {
    this.Element = document.createElement("select");
    this.Element.setAttribute("id", this.Id);
}

DropDownControl.prototype.RenderControl = function(container) {
    container.innerHTML = "";
    container.appendChild(this.Element);
}

DropDownControl.prototype.DefaultChangeFunction = function() {
    if (this.Element) {
        this.OldValue = this.Element.value;
    }
    this.DefaultBlurFunction();
};

DropDownControl.prototype.FillOptions = function(data, isAppend) {
    if (this.Element) {
        if (isAppend != true) this.Options = {};
        for (key in data) {
            this.Options.push(this.NewOption(key, data[key]));
        }
        this.RenderOptions();
    }
};

DropDownControl.prototype.FindOptions = function(isAppend) {
    if (this.Element) {
        if (isAppend != true) this.Options = new Array();
        for (index in this.Element.childNodes) {
            if (index == "length") continue;
            this.Options.push(this.Element.childNodes[index]);
        }
    }
};

DropDownControl.prototype.RenderOptions = function(){
    if (this.Element) {
        this.Element.innerHTML = "";
        for (index in this.Options) {
            if (index == "length") continue;
            this.Element.appendChild(this.Options[index]);
        }
    }
};

DropDownControl.prototype.ClearOptions = function() {
    this.Options = new Array();
    this.Value = this.DefaultValue;
    if (this.PrintElement)
        this.PrintElement.innerHTML = this.PrintValueModifier(this.Value);
    this.RenderOptions();
};

DropDownControl.prototype.GetOptionsCount = function() {
    var count = 0;
    if (this.Options) {
        for (index in this.Options) {
            if (index == "length") continue;
            if (this.Options[index] != null) count++;
        }
    }
    return count;
};

DropDownControl.prototype.AppendOption = function(value, html) {
    if (this.Element) {
        var option = this.NewOption(value, html);
        this.Options.push(option);
        this.Element.appendChild(option);
        if (this.Options.length == 1 && this.PrintElement) {//Hack for set first option value as this.Value
            this.Value = option.value;
            this.PrintElement.innerHTML = this.PrintValueModifier(option.value);
        }
    }
};

DropDownControl.prototype.NewOption = function(value, html) {
    var option = document.createElement("option");
    option.value = value;
    option.innerHTML = html;
    return option;
};

DropDownControl.prototype.RemoveOptionByValue = function(value) {
    var newOptions = new Array();
    for (index in this.Options) {
        if (index == "length") continue;
        if (this.Options[index].value != value) {
            newOptions.push(this.Options[index]);
        } else {
            this.Element.removeChild(this.Options[index]);
        }
    }
    this.Options = newOptions;
};

DropDownControl.prototype.RemoveOptionByHtml = function(html) {
    var newOptions = new Array();
    for (index in this.Options) {
        if (index == "length") continue;
        if (this.Options[index].innerHTML != html) {
            newOptions.push(this.Options[index]);
        } else {
            this.Element.removeChild(this.Options[index]);
        }
    }
    this.Options = newOptions;
};

DropDownControl.prototype.HasValue = function(value) {
    for (index in this.Options) {
        if (index == "length") continue;
        if (this.Options[index].value == value) {
            return true;
        }
    }
    return false;
};

DropDownControl.prototype.GetOptionByValue = function(value) {
    for (index in this.Options) {
        if (index == "length") continue;
        if (this.Options[index].value == value) {
            return this.Options[index];
        }
    }
    return null;
};

DropDownControl.prototype.GetOptionHtmlByValue = function(value) {
    for (index in this.Options) {
        if (index == "length") continue;
        if (this.Options[index].value == value) {
            return this.Options[index].innerHTML;
        }
    }
    return "";
};

DropDownControl.prototype.GetOptionValueByIndex = function(needIndex) {
    for (index in this.Options) {
        if (index == "length") continue;
        if (needIndex == index) {
            return this.Options[index].value;
        }
    }
    return "";
};

DropDownControl.prototype.PrintValueModifier = function(value) {
    return this.GetOptionHtmlByValue(value);
};

DropDownControl.prototype.Validate = function() {
    if (this.ValidationInfo && this.ValidationInfo.StateFunction && typeof this.ValidationInfo.StateFunction == "function") {
        return this.ValidationInfo.StateFunction.call(this);
    }
    if (this.Element) {
        return this.Element.value != null && this.Element.value != " " && this.Element.value != "" && this.Element.value + "" != "-1";
    }
    return false;
}