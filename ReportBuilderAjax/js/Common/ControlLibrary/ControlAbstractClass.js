var ControlAbstractClass = function(_parentObject, _id, _value, _defaultValue, _valueType, _dependentControls) {
    this.Init(_parentObject, _id, _value, _defaultValue, _valueType, _dependentControls);
};

ControlAbstractClass.prototype.Init = function(_parentObject, _id, _value, _defaultValue, _valueType, _dependentControls) {
    this.ParentObject = _parentObject;
    this.Id = _id;
    this.Value = _value;
    this.DefaultValue = _defaultValue;
    this.LastValue = null;
    this.ValueType = _valueType != null ? _valueType : ValueType.NotSet;
    this.Blocked = false;
    this.Element = null;
    this.PrintElement = null;
    this.TitleElement = null;
    this.ElementContainer = null;
    this.State = ControlState.NotSet;
    this.Type = ControlType.TextBox;
    //Info for validation
    this.ValidationInfo = {
        Required: false,
        GroupName: null,
        Name: null,
        StateFunction: null,
        DependentControls: {}
    };
    //Events function set
    this.EventSet = {
        Blur: new EventFunctionSetClass(),
        Click: new EventFunctionSetClass(),
        Focus: new EventFunctionSetClass(),
        Change: new EventFunctionSetClass(),
        MouseOver: new EventFunctionSetClass(),
        MouseOut: new EventFunctionSetClass(),
        KeyUp: new EventFunctionSetClass(),
        BeforeDataBind: new EventFunctionSetClass(),
        AfterDataBind: new EventFunctionSetClass()
    };
    //Using default event functions
    this.UseDefaultEvents = {
        Blur: true,
        Click: true,
        Focus: true,
        Change: true,
        MouseOver: true,
        MouseOut: true,
        KeyUp: true
    };
    this.DependentControls = _dependentControls != null ? _dependentControls : {};
    //Init DOM element
    this.Assign();
};

ControlAbstractClass.prototype.CreateControl = function() {
    this.Element = document.createElement("input");
    this.Element.setAttribute("type", "text");
    this.Element.setAttribute("id", this.Id);
}

ControlAbstractClass.prototype.RenderControl = function(container) {
    container.innerHTML = "";
    container.appendChild(this.Element);
}

ControlAbstractClass.prototype.TriggerEvent = function(eventType, eventName) {
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

ControlAbstractClass.prototype.GetValue = function () {
    var resultValue = "";
    if (this.Element) {
        /*if (typeof (this.Value) == 'string')
            //resultValue = this.Value.replace(/,/gi, ''); //.replace(/>/g, '&gt;').replace(/</g, '&lt;');
        else*/ resultValue = this.Value;
        switch (this.ValueType) {
            case ValueType.Money:
                var num = new Number(resultValue);
                return num.toFixed(2) * 1;
            case ValueType.Int:
                var num = new Number(resultValue);
                return num.toFixed(0) * 1;
            default:
                return resultValue;
        }
    }
    return this.DefaultValue;
};

ControlAbstractClass.prototype.GetPrintValue = function () {
    if (this.PrintElement) {
        return this.PrintElement.innerText.replace(/&/g, 'amp;'); 
    }
    return null;
};

ControlAbstractClass.prototype.PrintValueModifier = function(value) {
    return this.ValueType == ValueType.Money ? this.Element.value : value;
};

ControlAbstractClass.prototype.SetValue = function (value) {
    if (this.Value == value && this.ValueType != ValueType.Int && this.ValueType != ValueType.Money) return;
    if (this.Element) {
        this.LastValue = this.Value;
        if (typeof (value) == 'string')
            this.Value = value;//.replace(/&gt;/g, '>').replace(/&lt;/g, '<');
        else {
            this.Value = value;
        }
        /*if (this.PrintElement) {
        this.PrintElement.innerHTML = this.PrintValueModifier(this.Value);
        }*/
        this.DataBind();
    }
};

ControlAbstractClass.prototype.DataBind = function () {
    if (this.Element) {
        this.EventSet.BeforeDataBind.InvokeAll(this);
        if (this.ValueType == ValueType.Money) {
            this.Value = this.Value.toString().replace(/\(/g, "").replace(/\)/g, "").replace(/-/g,"");
        }
        this.Element.value = this.Value;
        if (this.ValueType == ValueType.Money) {
            $(this.Element).formatCurrency({
                decimalSymbol: '.',
                digitGroupSymbol: ',',
                groupDigits: true,
                symbol: '',
                negativeFormat: '%s%n'
            });
        }
        if (this.PrintElement) {
            if ($.browser.msie) {
                this.PrintElement.innerText = this.PrintValueModifier(this.Value);
            }
            else {
                this.PrintElement.textContent = this.PrintValueModifier(this.Value);
            }

        }
        this.EventSet.AfterDataBind.InvokeAll(this);
        this.PropertyChanged();
    }
};

ControlAbstractClass.prototype.Block = function() {
    //this.Element.readOnly = true;
    //this.Element.disabled = true;
    this.Blocked = true;
    this.SetDesign(DesignType.Disabled);
};

ControlAbstractClass.prototype.Unblock = function() {
    //this.Element.removeAttribute('readOnly');
    //this.Element.removeAttribute('disabled');
    this.Blocked = false;
    this.SetDesign(DesignType.Enabled);
};

ControlAbstractClass.prototype.DefaultBlurFunction = function (e) {
    if ("__/__/____" == this.Element.value) {
        this.Element.value = '';
    }


    // ‒–—―‖‗‘’‚‛“”„‟•′″‴ʹʺʻʼʽʾʿˈˊˋˌˎˏ˙ˮʹ᷾`‴″′‟‘’‚‛“”   problem symbols

    this.Element.value = this.Element.value.toString().replace(/‒/g, "-").replace(/ʽ/g, "'").replace(/`/g, "'").replace(/ˮ/g, "''").replace(/'/g, "'").replace(/ˋ/g, "'").replace(/ʹ/g, "'").replace(/ʺ/g, "''").replace(/ʻ/g, "'").replace(/ʼ/g, "'").replace(/‗/g, "_").replace(/―/g, "-").replace(/ˌ/g, ",").replace(/ˎ/g, "'").replace(/ˏ/g, "'").replace(/‖/g, "||").replace(/"/g, "''").replace(/“/g, "''").replace(/”/g, "''").replace(/„/g, "''").replace(/‟/g, "''").replace(/″/g, "''").replace(/‴/g, "''").replace(/‘/g, "'").replace(/’/g, "'").replace(/′/g, "'").replace(/ʾ/g, "'").replace(/ʿ/g, "'").replace(/ˈ/g, "'").replace(/ˊ/g, "'").replace(/˙/g, "'").replace(/ʹ᷾/g, "'").replace(/‛/g, "'").replace(//g, " ");

    if (this.Element.value) {

    }

    if (this.ValueType == ValueType.Money) {

        var re1 = /,/g;
        this.Element.value = this.Element.value.replace(re1, '');   

        if (!checkNumericHandler(this.Element.value, false)) {
            this.SetDesign(DesignType.Invalid);
            this.IsValid = false;
            return;
        }
        else {
            this.SetDesign(DesignType.Valid);
            this.IsValid = true;
        }

        /*if (this.Element.value.substring(0, 1) == '0' && this.Element.value.length > 1) {
            this.Element.value = this.Element.value.substring(1, this.Element.value.length);
        }
        var str = this.Element.value;
        var re = /[^0-9.,]/gi;
        var vale = str.replace(re, "");
        this.Element.value = vale == "" || vale == "," ? this.DefaultValue : vale;*/

        if (this.Element.value.replace(re1, "") * 1 > 1000000000) this.Element.value = 1000000000;
    }

    if (this.ValueType == ValueType.Int) {

        var re1 = /,/g;
        this.Element.value = this.Element.value.replace(re1, '');

        if (!checkNumericHandler(this.Element.value, false)) {
            this.SetDesign(DesignType.Invalid);
            this.IsValid = false;
            return;
        }
        else {
            this.SetDesign(DesignType.Valid);
            this.IsValid = true;
        }

        /*if (this.Element.value.substring(0, 1) == '0' && this.Element.value.length > 1) {
            this.Element.value = this.Element.value.substring(1, this.Element.value.length);
        }
        var str = this.Element.value;
        var re = /[^0-9]/gi;
        var vale = str.replace(re, "");
        this.Element.value = vale == "" ? this.DefaultValue : vale;*/

        if (this.Element.value.replace(re1, "") * 1 > 1000000000) this.Element.value = 1000000000;
    }

    this.SetValue(this.Element.value);
    this.SetDesign(DesignType.Inactive);
};

ControlAbstractClass.prototype.DefaultClickFunction = function(e) {};

ControlAbstractClass.prototype.DefaultFocusFunction = function (e) {
    this.SetDesign(DesignType.Active);
};

ControlAbstractClass.prototype.DefaultChangeFunction = function(e) {};

ControlAbstractClass.prototype.DefaultMouseOverFunction = function(e) {};

ControlAbstractClass.prototype.DefaultMouseOutFunction = function(e) { };

ControlAbstractClass.prototype.DefaultKeyUpFunction = function (e) {
    if (this.ValueType == ValueType.String) {
        var keyCode = this.EventSet.KeyUp.Event.keyCode;
        if (keyCode == 9) return;
    }
    
    if (this.ValueType == ValueType.Money) {
        var keyCode = this.EventSet.KeyUp.Event.keyCode;
        if (keyCode == 9 || keyCode == 16) return;
        /*if ((keyCode >= 48 && keyCode <= 57) || (keyCode >= 35 && keyCode <= 40) || keyCode == 8 || keyCode == 46 || (keyCode >= 96 && keyCode <= 105) || keyCode == 144 || keyCode == 20) {
            var st = this.Element.value;
            var wzero = st.replace(/0/gi, "");
            if (wzero != "" && st.charAt(0) == "0" && st.length != 1) {
                st = st.substr(1, st.length - 1);
            }
            if (this.Element.value.charAt(0) == "0" && this.Element.value != wzero && this.Element.value.length != 1) {
                this.Element.value = wzero == "" ? "0" : st;
            }
            return;
        }
        var str = this.Element.value;
        var re = /[^0-9.,]/gi;
        var vale = str.replace(re, "");
        this.Element.value = vale == "" || vale == "," ? this.DefaultValue : vale;*/
    }

    if (this.ValueType == ValueType.Int) {
        var keyCode = this.EventSet.KeyUp.Event.keyCode;
        if (keyCode == 9 || keyCode == 16) return;
        /*if ((keyCode >= 48 && keyCode <= 57) || (keyCode >= 35 && keyCode <= 40) || keyCode == 8 || keyCode == 46 || (keyCode >= 96 && keyCode <= 105) || keyCode == 144 || keyCode == 20) {
            var st = this.Element.value;
            var wzero = st.replace(/0/gi, "");
            if (wzero != "" && st.charAt(0) == "0" && st.length != 1) {
                st = st.substr(1, st.length - 1);
            }
            if (this.Element.value.charAt(0) == "0" && this.Element.value != wzero && this.Element.value.length != 1) {
                this.Element.value = wzero == "" ? "0" : st;
            }
            return;
        }
        var str = this.Element.value;
        var re = /[^0-9]/gi;
        var vale = str.replace(re, "");
        this.Element.value = vale == "" ? this.DefaultValue : vale;*/
    }
};

ControlAbstractClass.prototype.Assign = function(id, value) {
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
    if (this.Element) {
        this.Element.onblur = function(e) {
            thisObject.EventSet.Blur.Event = e != null ? e : window.event;
            if (thisObject.UseDefaultEvents.Blur == true) {
                thisObject.DefaultBlurFunction();
            }
            thisObject.EventSet.Blur.InvokeAll(thisObject);
        };
        this.Element.onclick = function(e) {
            thisObject.EventSet.Click.Event = e != null ? e : window.event;
            if (thisObject.UseDefaultEvents.Click == true) {
                thisObject.DefaultClickFunction();
            }
            thisObject.EventSet.Click.InvokeAll(thisObject);
        };
        this.Element.onfocus = function(e) {
            if (thisObject.Blocked) {
                thisObject.Element.blur();
                return false;
            }
            thisObject.EventSet.Focus.Event = e != null ? e : window.event;
            if (thisObject.UseDefaultEvents.Focus == true) {
                thisObject.DefaultFocusFunction();
            }
            thisObject.EventSet.Focus.InvokeAll(thisObject);
        };
        this.Element.onchange = Function.DelegateHandler(this, function(event, element) {
            if (this.Blocked) {
                this.Element.value = this.Value;
                return false;
            }
            this.EventSet.Change.Event = event;
            this.LastValue = this.Value;
            if (this.UseDefaultEvents.Change == true) {
                this.DefaultChangeFunction();
            }
            this.EventSet.Change.InvokeAll(this);
        });
        this.Element.onmouseover = function(e) {
            thisObject.EventSet.MouseOver.Event = e != null ? e : window.event;
            if (thisObject.UseDefaultEvents.MouseOver == true) {
                thisObject.DefaultMouseOverFunction();
            }
            thisObject.EventSet.MouseOver.InvokeAll(thisObject);
        };
        this.Element.onmouseout = function(e) {
            thisObject.EventSet.MouseOut.Event = e != null ? e : window.event;
            if (thisObject.UseDefaultEvents.MouseOut == true) {
                thisObject.DefaultMouseOutFunction();
            }
            thisObject.EventSet.MouseOut.InvokeAll(thisObject);
        };
        this.Element.onkeyup = function(e) {
            if (thisObject.Blocked) return false;
            thisObject.EventSet.KeyUp.Event = e != null ? e : window.event;
            if (thisObject.UseDefaultEvents.KeyUp == true) {
                thisObject.DefaultKeyUpFunction();
            }
            thisObject.EventSet.KeyUp.InvokeAll(thisObject);
        };
        this.Element.onkeydown = function(e) {
            if (thisObject.Blocked) return false;
        };
    }
    this.DataBind();
};

ControlAbstractClass.prototype.PropertyChanged = function() {
    for (controlName in this.DependentControls) {
        if (this.DependentControls[controlName].DataBind) {
            this.DependentControls[controlName].DataBind();
        } else {
            this.DependentControls[controlName].PropertyChanged();
        }
    }
};

ControlAbstractClass.prototype.Dispose = function() {
    if (this.Element) {
        for (eventSenName in this.EventSet) {
            this.EventSet[eventSenName].Clear();
        }
        this.Element = null;
    }
};

ControlAbstractClass.prototype.SetDesign = function(designType) {
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

ControlAbstractClass.prototype.IsVisible = function() {
    if (!this.Element) return false;
    return this.Element.offsetWidth > 0 || this.Element.offsetHeight > 0;
}

ControlAbstractClass.prototype.SetAsValid = function() {
    this.SetDesign(DesignType.Valid);
};

ControlAbstractClass.prototype.SetAsInvalid = function() {
    this.SetDesign(DesignType.Invalid);
};

ControlAbstractClass.prototype.SetAsChanged = function() {
    this.SetDesign(DesignType.Changed);
};


ControlAbstractClass.prototype.SetDisplayState = function(state) {
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

ControlAbstractClass.prototype.Show = function() {
    this.SetDisplayState("");
};

ControlAbstractClass.prototype.Hide = function() {
    this.SetDisplayState("none");
};

ControlAbstractClass.prototype.Validate = function() {
    for (var index in this.ValidationInfo.DependentControls) {
        var dependentControl = this.ValidationInfo.DependentControls[index];
        if (!dependentControl.Validate()) {
            return false;
        }
    }
    if (this.ValidationInfo && this.ValidationInfo.StateFunction && typeof this.ValidationInfo.StateFunction == "function") {
        return this.ValidationInfo.StateFunction.call(this);
    }
    if (this.Element) {
        return this.Element.value != null && this.Element.value != "";
    }
    return false;
}