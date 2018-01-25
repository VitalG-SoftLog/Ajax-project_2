var RadioButtonSetControl = function(_parent, _containerId, _dependentControls, template) {
    this.ParentObject = _parent;
    this.Buttons = new Array();
    this.Type = ControlType.RadioSet;
    this.DependentControls = _dependentControls != null ? _dependentControls : {};
    this.Selected = null;
    this.LastValue = null;
    this.Value = null;
    this.MulipleSelect = false;
    this.IdPostfix = "_rb";
    this.Blocked = false;
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
    
    var attr = {
        "idButton": _parent.getClientID(_containerId)
    };
    var templ = TemplateEngine.Format(template, attr);
    $(document.getElementById(_containerId)).append(templ);
    this.Id = _parent.getClientID(_containerId);
    this.Assign();
};

RadioButtonSetControl.prototype.CreateControl = function() {
    this.Element = document.createElement("div");
    this.Element.setAttribute("id", this.Id);
    for (var index = 0; index < this.Buttons.length; index++) {
        if (this.Buttons[index] == null) continue;
        this.Element.appendChild(this.Buttons[index].Element);
    }
};

RadioButtonSetControl.prototype.RenderControl = function(container) {
    container.innerHTML = "";
    container.appendChild(this.Element);
};

RadioButtonSetControl.prototype.TriggerEvent = function(eventType, eventName) {
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

RadioButtonSetControl.prototype.FindButtons = function() {
    if (this.Element) {
        var elements = this.Element.getElementsByTagName("input");
        for (var index = 0; index < elements.length; index++) {
            if (elements[index] == null) continue;
            var element = elements[index];
            if (element.id == null || element.id == "") {
                element.id = this.Id + "_rb" + index;
            }
            if (element.type == "radio") {
                this.AppendButton(new RadioButtonControl(this, element.id, element.value, element.checked == true ? 1 : 0));
            }
        }
    }
};

RadioButtonSetControl.prototype.AppendButton = function(radioButtonControl) {
    radioButtonControl.EventSet.Click.List.GroupProperyChangedCallback = function() {
        if (this.ParentObject.Blocked) {
            return false;
        }
        this.ParentObject.SetValue(this.Value);
        document.needSave = true;
    }
    var thisObject = this;
    //<CCMSI DESIGN HACK>
    $("[clickfor='" + radioButtonControl.Id + "']", this.Element).click(function(e) {
        var clickId = this.getAttribute("clickfor");
        $("#" + clickId).trigger('click');
    });
    if (radioButtonControl.Checked == 1) {
        this.Selected = radioButtonControl;
    }
    //</CCMSI DESIGN HACK>
    this.Buttons.push(radioButtonControl);
};

RadioButtonSetControl.prototype.GetSelected = function() {
    return this.Selected;
};

RadioButtonSetControl.prototype.GetValue = function() {
    return this.Selected ? this.Selected.GetValue() : null;
};

RadioButtonSetControl.prototype.GetBoolValue = function () {
    return this.GetValue() == "yes";
};

RadioButtonSetControl.prototype.SetValue = function(value) {
    value = value + '';
    var changed = false;
    for (key in this.Buttons) {
        if (key == "length") continue;
        if (this.Buttons[key] == null) continue;
        if (this.Buttons[key].GetValue().toLowerCase() == value.toLowerCase()) {
            this.Buttons[key].SetCheckState(1);
            if (this.Selected && this.Selected.Value != this.Buttons[key].Value) {
                this.LastValue = this.Selected ? this.Selected.Value : null;
                this.Value = this.Buttons[key].Value;
                changed = true;
            }
            if (!this.Selected && this.Buttons[key]) {//for first start or none state
                changed = true;
            }
            this.Selected = this.Buttons[key];
        }
    }
    if (changed) {
        this.PropertyChanged();
    }
};

RadioButtonSetControl.prototype.Assign = function() {
    this.Element = document.getElementById(this.Id);
    this.PrintElement = document.getElementById(this.Id + "_print");
    this.TitleElement = document.getElementById(this.Id + "_title");
    this.ElementContainer = document.getElementById(this.Id + "_container");
};

RadioButtonSetControl.prototype.PropertyChanged = function() {
    this.SetAttachedSkinedControl();
    if (this.PrintElement && this.Selected) {
        this.PrintElement.innerHTML = this.PrintValueModifier(this.Selected.GetValue());
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

RadioButtonSetControl.prototype.PrintValueModifier = function(value) {
    return value.toUpperCase();
};

RadioButtonSetControl.prototype.SetAttachedSkinedControl = function() {
    if (this.Element) {
        var attachedClass = null;
        if (this.Selected && this.Selected.Element) {
            var attachedClass = this.Selected.Element.getAttribute("attachedClass");
        }
        if (attachedClass != null) {
            this.Element.className = attachedClass;
        }
    }
};

RadioButtonSetControl.prototype.SetAsChanged = function() {
    this.SetDesign(DesignType.Changed);
};

RadioButtonSetControl.prototype.Block = function() {
    this.Blocked = true;
    this.SetDesign(DesignType.Disabled);
};

RadioButtonSetControl.prototype.Unblock = function() {
    this.Blocked = false;
    this.SetDesign(DesignType.Enabled);
};

RadioButtonSetControl.prototype.SetDesign = function(designType) {
    if (!this.Element) return;
    switch (designType) {
        case DesignType.Inactive:
            break;
        case DesignType.Active:
            break;
        case DesignType.Disabled:
            this.Element.tBodies[0].className = "notenabled";
            break;
        case DesignType.Enabled:
            this.Element.tBodies[0].className = "";
            break;
        case DesignType.Invalid:
            $(this.Element).addClass("rq");
            break;
        case DesignType.Valid:
            $(this.Element).removeClass("rq");
            break;
        case DesignType.Changed:
            $(this.Element.firstChild).addClass("changed");
            break;
        case DesignType.Print:
            break;
    }
};

RadioButtonSetControl.prototype.IsVisible = function() {
    if (!this.Element) return false;
    //testElement = this.Element; //this is normal case
    var testElement = this.Element.offsetParent;
    if (!isWebKit) testElement = this.Element;
    if (!testElement) return false;
    return testElement.offsetWidth > 0 || testElement.offsetHeight > 0;
};

RadioButtonSetControl.prototype.Validate = function() {
    if (this.ValidationInfo && this.ValidationInfo.StateFunction && typeof this.ValidationInfo.StateFunction == "function") {
        return this.ValidationInfo.StateFunction.call(this);
    }
    return this.Selected != null;
};

RadioButtonSetControl.prototype.SetDisplayState = function(state) {
    var elements = new Array();
    elements.push(this.Element);
    elements.push(this.TitleElement);
    elements.push(this.PrintElement);
    elements.push(this.ElementContainer);
    for (var i = 0; i < elements.length; i++) {
        if (elements[i]) {
            elements[i].style.display = state;
        }
    }
};

RadioButtonSetControl.prototype.Show = function() {
    this.SetDisplayState("");
};

RadioButtonSetControl.prototype.Hide = function() {
    this.SetDisplayState("none");
};