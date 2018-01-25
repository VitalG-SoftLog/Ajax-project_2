var CheckBoxControl = function(_parentObject, _id, _value, _checked, _valueType, _dependentControls) {
    this.Init(_parentObject, _id, _value, "", _valueType, _dependentControls);
    this.Type = ControlType.CheckBox;
    this.Checked = _checked * 1 != 1 ? 0 : 1;
    if (this.Element) {
        this.Element.checked = this.Checked == 1;
        //Set Default value from template    
        this.Value = this.Element.value;
    }
};

//Inheritance CheckBoxControl from ControlAbstractClass

ClassInheritance.Standart(CheckBoxControl, ControlAbstractClass);

//Entending end overwriting class prototype members

CheckBoxControl.prototype.CreateControl = function() {
    this.Element = document.createElement("input");
    this.Element.setAttribute("type", "checkbox");
    this.Element.setAttribute("id", this.Id);
}

CheckBoxControl.prototype.IsChecked = function() {
    return this.Checked == 1;
};

CheckBoxControl.prototype.GetValue = function() {
    if (this.Element) {
        return this.Element.value;
    }
    return null;
};

CheckBoxControl.prototype.SetValue = function(value) {
    if (this.Element) {
        this.Element.value = value;
    }
};

CheckBoxControl.prototype.GetCheckState = function() {
    return this.Checked;
};

CheckBoxControl.prototype.SetCheckState = function(state) {
    state = state * 1;
    if (this.Checked == state) return;
    if (state != 1) state = 0;
    if (this.Element) {
        this.Checked = state;
        this.DataBind();
    }
};

CheckBoxControl.prototype.DefaultBlurFunction = function() {
    this.SetDesign(DesignType.Inactive);
};

CheckBoxControl.prototype.DefaultClickFunction = function() {
    var checked = this.Element.checked == true ? 1 : 0;
    this.SetCheckState(checked);
};

CheckBoxControl.prototype.DataBind = function() {
    if (this.Element) {
        this.EventSet.BeforeDataBind.InvokeAll(this);
        if (this.Checked == 1) {
            this.Element.checked = true;
            if (this.PrintElement) {
                this.PrintElement.innerHTML = this.PrintValueModofier(true);
            }            
        } else {
            this.Element.checked = false;
            if (this.PrintElement) {
                this.PrintElement.innerHTML = this.PrintValueModofier(false);
            }
        }
        this.EventSet.AfterDataBind.InvokeAll(this);
        this.PropertyChanged();
    }
};

CheckBoxControl.prototype.PrintValueModofier = function(value) {
    switch (value) {
        case true: return "Yes";
        case false: return "No";
    }
    return "Not set";
};

CheckBoxControl.prototype.Validate = function() {
    if (this.ValidationInfo && this.ValidationInfo.StateFunction && typeof this.ValidationInfo.StateFunction == "function") {
        return this.ValidationInfo.StateFunction.call(this);
    }
    if (this.Element) {
        return this.GetCheckState();
    }
    return false;
};