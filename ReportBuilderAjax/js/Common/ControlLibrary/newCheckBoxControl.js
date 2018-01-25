var newCheckBoxControl = function (_parentObject, _id, _value, _checked, _valueType, _dependentControls) {
    var attr = {
        "idCheckBox": _parentObject.getClientID(_id)
    };
    var templ = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_Checkbox'], attr);
    $(document.getElementById(_id)).append(templ);
    _id = _parentObject.getClientID(_id);
    this.Init(_parentObject, _id, _value, "", _valueType, _dependentControls);
};

//Inheritance CheckBoxControl from ControlAbstractClass

ClassInheritance.Standart(newCheckBoxControl, CheckBoxControl);

//Entending end overwriting class prototype members

newCheckBoxControl.prototype.GetStringValue = function () {
    return this.GetCheckState() == 1 ? "true" : "false";
};

newCheckBoxControl.prototype.GetValue = function () {
    return this.GetCheckState();
};

newCheckBoxControl.prototype.SetValue = function (value) {
    this.SetCheckState(value);
};

newCheckBoxControl.prototype.DefaultClickFunction = function () {
    this.SetCheckState(!this.Checked);
};

newCheckBoxControl.prototype.DataBind = function () {
    if (this.Element) {
        this.EventSet.BeforeDataBind.InvokeAll(this);
        if (this.Checked == 1) {
            this.Element.checked = true;
            document.getElementById(this.Id).className = "checkboxChecked";
            if (this.PrintElement) {
                this.PrintElement.innerHTML = this.PrintValueModofier(true);
            }            
        } else {
            this.Element.checked = false;
            document.getElementById(this.Id).className = "checkboxUnchecked";
            if (this.PrintElement) {
                this.PrintElement.innerHTML = this.PrintValueModofier(false);
            }
        }
        this.EventSet.AfterDataBind.InvokeAll(this);
        this.PropertyChanged();
    }
};