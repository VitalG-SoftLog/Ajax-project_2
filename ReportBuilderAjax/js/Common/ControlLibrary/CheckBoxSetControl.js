var CheckBoxSetControl = function(_parent, _containerId, _multipleSelect, _dependentControls) {
    this.ParentObject = _parent;
    this.Id = _containerId;
    this.Element = document.getElementById(this.Id);
    this.PrintElement = document.getElementById(this.Id + "_print");
    this.ElementContainer = null;
    this.Buttons = new Array();
    this.Type = ControlType.CheckBoxSet;
    this.DependentControls = _dependentControls != null ? _dependentControls : {};
    this.Selected = [];
    this.MultipleSelect = _multipleSelect != null ? _multipleSelect : true;
    this.EventSet = {
        PropertyChanged: new EventFunctionSetClass()
    }
    //Info for validation
    this.ValidationInfo = {
        Required: false,
        GroupName: null,
        Name: null,
        StateFunction: null,
        DependentControls: {}
    };
};

CheckBoxSetControl.prototype.CreateControl = function() {
    this.Element = document.createElement("div");
    this.Element.setAttribute("id", this.Id);
    for (var index = 0; index < this.Buttons.length; index++) {
        if (this.Buttons[index] == null) continue;
        this.Element.appendChild(this.Buttons[index].Element);
    }
};

CheckBoxSetControl.prototype.RenderControl = function(container) {
    container.innerHTML = "";
    container.appendChild(this.Element);
};

CheckBoxSetControl.prototype.TriggerEvent = function(eventType, eventName) {
    if (eventType in this.EventSet) {
        var typeEventSet = this.EventSet[eventType];
        if (eventName == null) {
            typeEventSet.InvokeAll(this);
            return;
        }
        if (eventName in typeEventSet.List) {
            typeEventSet.Invoke(eventName, this);
        }
    }
};

CheckBoxSetControl.prototype.FindButtons = function() {
    if (this.Element) {
        var elements = this.Element.getElementsByTagName("input");
        for (var index = 0; index < elements.length; index++) {
            if (elements[index] == null) continue;
            var element = elements[index];
            if (element.id == null || element.id == "") {
                element.id = this.Id + "_cb" + index;
            }
            if (element.type.toLowerCase() == "checkbox") {
                this.AppendButton(new CheckBoxControl(this, element.id, element.value, element.checked == true ? 1 : 0));
            }
        }
    }
};

CheckBoxSetControl.prototype.AppendButton = function(checkBoxControl) {
    if (checkBoxControl.Checked == 1) {
        this.Selected.push(checkBoxControl);
    }
    checkBoxControl.EventSet.Click.List.GroupProperyChangedCallback = function() {
        if (this.Checked == 1) {
            this.ParentObject.Selected.push(this);
        } else {
            this.ParentObject.Selected.pop(this);
        };
        if (this.ParentObject.MultipleSelect != true) {
            this.ParentObject.UncheckAllExceptThis(this);
        }
        this.ParentObject.PropertyChanged();
    }
    this.Buttons.push(checkBoxControl);
};

CheckBoxSetControl.prototype.UncheckAllExceptThis = function(sender) {
    for (var index = 0; index < this.Buttons.length; index++) {
        if (this.Buttons[index] == null) continue;
        if (sender != this.Buttons[index]) {
            this.Buttons[index].SetCheckState(0);
        }
    }
};

CheckBoxSetControl.prototype.GetSelected = function() {
    var result = [];
    for (var i = 0; i < this.Buttons.length; i++) {
        if (this.Buttons[i].Checked == 1) result.push(this.Buttons[i]);
    }
    return result;
};

CheckBoxSetControl.prototype.GetValue = function() {
    var result = [];
    for (var i = 0; i < this.Buttons.length; i++) {
        if (this.Buttons[i].Checked == 1) result.push(this.Buttons[i].GetValue());
    }
    return result;
};

CheckBoxSetControl.prototype.HasValue = function(value) {
    var values = this.GetValue();
    var result = false;
    if (values && values.length) {
        for (var i = 0; i < values.length; i++) {
            var buttonValue = values[i];
            if (buttonValue && buttonValue == value) result = true;
        }
    }
    return result;
};

CheckBoxSetControl.prototype.SetValue = function(value) {
    for (key in this.Buttons) {
        if (key == "length") continue;
        if (this.Buttons[key] == null) continue;
        if (this.Buttons[key].GetValue() == value) {
            this.Buttons[key].SetCheckState(1);
            if (this.PrintElement) {
                this.PrintElement.innerHTML = this.PrintValueModifier(value);
            } 
        } else {
            if (this.MultipleSelect == false) {
                this.Buttons[key].SetCheckState(0);
            }
        }
    }
    this.PropertyChanged();
};

CheckBoxSetControl.prototype.Assign = function() {
    this.Element = document.getElementById(this.Id);
    this.TitleElement = document.getElementById(this.Id + "_title");
    this.PrintElement = document.getElementById(this.Id + "_print");
    this.ElementContainer = document.getElementById(this.Id + "_container");
};

CheckBoxSetControl.prototype.PropertyChanged = function() {
    if (this.PrintElement && this.Selected) {
        this.PrintElement.innerHTML = this.PrintValueModifier(this.Selected);
    }
    this.EventSet.PropertyChanged.InvokeAll(this);
    for (controlName in this.DependentControls) {
        if (this.DependentControls[controlName].DataBind) {
            this.DependentControls[controlName].DataBind();
        } else {
            this.DependentControls[controlName].PropertyChanged()
        }
    }    
};

CheckBoxSetControl.prototype.PrintValueModifier = function(values) {
    var values = values;
    if (typeof values != "array") {
        return "Wrong input data type!";
    }
    var resultString = "";
    for (var i = 0; i < values.length; i++) {
        resultString += values[i];
        if (i < values.length - 1) {
            resultString += ", ";
        }
    }
    if (values.length > 0) {
        resultString += ".";
    }
    return resultString;
};

CheckBoxSetControl.prototype.SetDisplayState = function(state) {
    var elements = new Array();
    elements.push(this.Element);
    elements.push(this.PrintElement);
    elements.push(this.TitleElement);
    elements.push(this.ElementContainer);
    for (var i = 0; i < elements.length; i++) {
        if (elements[i]) {
            elements[i].style.display = state;
        }
    }
};

CheckBoxSetControl.prototype.Show = function() {
    this.SetDisplayState("");
};

CheckBoxSetControl.prototype.Hide = function() {
    this.SetDisplayState("none");
};

CheckBoxSetControl.prototype.SetAsChanged = function() {
    this.SetDesign(DesignType.Changed);
};

CheckBoxSetControl.prototype.SetDesign = function(designType) {
    switch (designType) {
        case DesignType.Inactive:
            break;
        case DesignType.Active:
            break;
        case DesignType.Disabled:
            break;
        case DesignType.Enabled:
            break;
        case DesignType.Invalid:
            $(this.Element).addClass("req");
            break;
        case DesignType.Valid:
            $(this.Element).removeClass("req");
            break;
        case DesignType.Changed:
            $(this.Element).addClass("changed");
            break;
        case DesignType.Print:
            break;
    }
};

CheckBoxSetControl.prototype.IsVisible = function() {
    if (!this.Element) return false;
    return this.Element.offsetWidth > 0 || this.Element.offsetHeight > 0;
};

CheckBoxSetControl.prototype.Validate = function() {
    if (this.ValidationInfo && this.ValidationInfo.StateFunction && typeof this.ValidationInfo.StateFunction == "function") {
        return this.ValidationInfo.StateFunction.call(this);
    }
    return this.GetValue().length > 0;
};