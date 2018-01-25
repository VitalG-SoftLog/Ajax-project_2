var StaticControl = function(_parentObject, _id, _value, _defaultValue, _valueType, _dependentControls) {
    this.Init(_parentObject, _id, _value, _defaultValue, _valueType, _dependentControls);
    this.Type = ControlType.Static;
    this.Blocked = true;
    this.UseDefaultEvents = {
        Blur: false,
        Click: false,
        Focus: false,
        Change: false,
        MouseOver: false,
        MouseOut: false,
        KeyUp: false
    };
    this.Assign();
};

//Inheritance StaticControl from ControlAbstractClass

ClassInheritance.Standart(StaticControl, ControlAbstractClass);

//Entending end overwriting class prototype members

StaticControl.prototype.CreateControl = function() {
    this.Element = document.createElement("span");
    this.Element.setAttribute("id", this.Id);
}

StaticControl.prototype.RenderControl = function(container) {
    container.innerHTML = "";
    container.appendChild(this.Element);
}

StaticControl.prototype.DataBind = function() {
    if (this.Element) {
        this.EventSet.BeforeDataBind.InvokeAll(this);
        if (this.Value != null) this.Element.innerHTML = this.Value;
        this.EventSet.AfterDataBind.InvokeAll(this);
        this.PropertyChanged();
    }
};

StaticControl.prototype.Block = function() {};

StaticControl.prototype.Unblock = function() {};

StaticControl.prototype.DefaultBlurFunction = function(e) {};

StaticControl.prototype.DefaultFocusFunction = function(e) {};

StaticControl.prototype.Assign = function(id, value) {
    var thisObject = this;
    this.Dispose();
    if (id) {
        this.Id = id;
    }
    if (value) {
        this.Value = value;
    }
    if (this.Id) {
        this.Element = document.getElementById(this.Id);
        this.PrintElement = document.getElementById(this.Id + "_print");
        this.TitleElement = document.getElementById(this.Id + "_title");
        this.ElementContainer = document.getElementById(this.Id + "_container");
    }
    this.DataBind();
};

StaticControl.prototype.PropertyChanged = function() {
    for (controlName in this.DependentControls) {
        this.DependentControls[controlName].DataBind();
    }
};

StaticControl.prototype.Dispose = function() {
    if (this.Element) {
        for (eventSenName in this.EventSet) {
            this.EventSet[eventSenName].Clear();
        }
        this.Element = null;
    }
};

StaticControl.prototype.SetAsValid = function() {};

StaticControl.prototype.SetAsInvalid = function() {};

StaticControl.prototype.SetAsChanged = function() { };

StaticControl.prototype.SetDesign = function(designType) {
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
            break;
        case DesignType.Valid:
            break;
        case DesignType.Changed:
            break;
        case DesignType.Print:
            break;
    }
};

StaticControl.prototype.IsVisible = function() {
    if (!this.Element) return false;
    return this.Element.offsetWidth > 0 || this.Element.offsetHeight > 0;
}