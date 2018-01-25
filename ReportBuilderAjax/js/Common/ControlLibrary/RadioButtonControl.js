var RadioButtonControl = function(_parentObject, _id, _value, _checked, _valueType, _dependentControls, _group) {
    this.Init(_parentObject, _id, _value, "", _valueType, _dependentControls);
    this.Type = ControlType.Radio;
    this.Group = _group != null ? _group : this.Id + "_rbg";
    this.Checked = _checked * 1 != 1 ? 0 : 1;
    this.SetGroup(_group != null);
    if (this.Element) {
        this.Element.checked = this.Checked == 1;
        //Set Default value from template    
        this.Value = this.Element.value;
    }
};

//Inheritance RadioButtonControl from CheckBoxControl

ClassInheritance.Standart(RadioButtonControl, CheckBoxControl);

//Entending end overwriting class prototype members

RadioButtonControl.prototype.CreateControl = function() {
    this.Element = document.createElement("input");
    this.Element.setAttribute("type", "radio");
    this.Element.setAttribute("id", this.Id);
    this.Element.setAttribute("name", this.Group);
};

RadioButtonControl.prototype.SetGroup = function(_group, overwrite) {
    if (this.Element) {
        if (_group) {
            this.Group = _group;
        }
        if (this.Element.name == null || this.Element.name == "") {
            this.Element.setAttribute("name", this.Group);
        } else {
            this.Group = this.Element.name;
        }
        if (overwrite == true) {
            this.Element.setAttribute("name", this.Group);
        }
    }
};

RadioButtonControl.prototype.Validate = function() {
    if (this.ValidationInfo && this.ValidationInfo.StateFunction && typeof this.ValidationInfo.StateFunction == "function") {
        return this.ValidationInfo.StateFunction.call(this);
    }
    if (this.Element) {
        return this.GetCheckState();
    }
    return false;
}