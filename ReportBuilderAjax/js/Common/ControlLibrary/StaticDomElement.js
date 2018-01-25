var StaticDomElement = function(_parent, _id) {
    this.ParentObject = _parent;
    this.Id = _id;
    this.Type = ControlType.StaticBlock;
    this.ValueType = ValueType.NoValue;
    this.Blocked = true;
    this.EventSet = {
        Click: new EventFunctionSetClass(),
        BeforeDataBind: new EventFunctionSetClass(),
        AfterDataBind: new EventFunctionSetClass()
    };
    this.Assign();
};

StaticDomElement.prototype.Assign = function(id) {
    var thisObject = this;
    this.Dispose();
    if (id) {
        this.Id = id;
    }
    if (this.Id) {
        this.Element = document.getElementById(this.Id);
        if (!this.Element) return;
        //Bind Click Event Set
        this.Element.onclick = function(e) {
            thisObject.EventSet.Click.Event = e != null ? e : window.event;
            thisObject.EventSet.Click.InvokeAll(thisObject);
        };
    }
};

StaticDomElement.prototype.Dispose = function() {
    if (this.Element) {
        this.Element = null;
    }
};

StaticDomElement.prototype.SetDesign = function(designType) {
    switch (designType) {
        case DesignType.Inactive:
            break;
        case DesignType.Active:
            break;
        case DesignType.Disabled:
            break;
        case DesignType.Invalid:
            break;
        case DesignType.Changed:
            $(this.Element).addClass("changed");
            break;
        case DesignType.Print:
            break;
    }
};

StaticDomElement.prototype.SetDisplayState = function(state) {
    var elements = new Array();
    elements.push(this.Element);
    elements.push(this.PrintElement);
    elements.push(this.ElementContainer);
    for (var i = 0; i < elements.length; i++) {
        if (elements[i]) {
            elements[i].style.display = state;
        }
    }
    this.PropertyChanged();
};

StaticDomElement.prototype.Show = function() {
    this.SetDisplayState("");
};

StaticDomElement.prototype.Hide = function() {
    this.SetDisplayState("none");
};

StaticDomElement.prototype.HideSlide = function () {
    //this.SetDisplayState("none");
    $(this.Element).hide(300);
};

StaticDomElement.prototype.ShowSlide = function () {
    //this.SetDisplayState("");
    $(this.Element).show(300);
    $(this.Element).effect("highlight", {}, 1000);
};

StaticDomElement.prototype.DataBind = function() {
    this.EventSet.BeforeDataBind.InvokeAll(this);
    this.EventSet.AfterDataBind.InvokeAll(this);
    this.PropertyChanged();
};

StaticDomElement.prototype.PropertyChanged = function() {
};

StaticDomElement.prototype.TriggerEvent = function(eventType, eventName) {
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

StaticDomElement.prototype.IsVisible = function() {
    if (!this.Element) return null;
    return this.Element.offsetWidth > 0 || this.Element.offsetHeight > 0;
};

StaticDomElement.prototype.IsPrintVisible = function() {
    if (!this.PrintElement) return null;
    return this.PrintElement.offsetWidth > 0 || this.PrintElement.offsetHeight > 0;
};

StaticDomElement.prototype.IsContainerVisible = function() {
    if (!this.ElementContainer) return null;
    return this.ElementContainer.offsetWidth > 0 || this.ElementContainer.offsetHeight > 0;
};