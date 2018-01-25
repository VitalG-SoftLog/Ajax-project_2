var DinamicRadioButtonSetControl = function(_parent, _containerId, qustionText, _dependentControls) {
    this.Container = null;
    this.QustionText = qustionText ? qustionText : "No question text";
    this.ClassNames = {
        0: "none",
        1: "first",
        2: "second",
        3: "multi1",
        4: "multi2",
        5: "multi3",
        6: "multi4",
        7: "multi5"
    };
    this.ButtonsClassNames = {
        1: "one",
        2: "two",
        3: "three",
        4: "four",
        5: "five",
        6: "six",
        7: "seven"
    };
    this.MaxButtonsCount = 7;
    //From radio buttons set control
    this.ParentObject = _parent;
    this.Id = _containerId;
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
    this.Prepare();
};

//Inheritance DinamicRadioButtonSetControl from RadioButtonSetControl

ClassInheritance.Standart(DinamicRadioButtonSetControl, RadioButtonSetControl);

//Entending end overwriting class prototype members


DinamicRadioButtonSetControl.prototype.Prepare = function() {
    this.Container = document.getElementById(this.Id + "_container");
    this.CreateControl();
    this.Assign();
    this.HiddenRadioContainer = document.getElementById(this.Id + "_hiddenRadioContainer");
    this.ButtonsContainer = document.getElementById(this.Id + "_buttonsContainer");
};

DinamicRadioButtonSetControl.prototype.CreateControl = function() {
    var attributes = {
        id: this.Id,
        defaultClassName: this.ClassNames[0],
        defaultPrintValue: "",
        questionText: this.QustionText
    };
    this.Container.innerHTML = TemplateEngine.Format(TemplateManager.templates['DinamicRadioButtonSet_RootTemplate'], attributes);
};

DinamicRadioButtonSetControl.prototype.Fill = function(data, doNotOverwrite) {
    var hiddenTemplate = TemplateManager.templates['DinamicRadioButtonSet_HiddenRadioTemplate'];
    var buttonTemplate = TemplateManager.templates['DinamicRadioButtonSet_ButtonTemplate'];
    var buttonsCount = this.Buttons.length;
    if (!doNotOverwrite) {
        buttonsCount = 0;
        $(this.HiddenRadioContainer).empty();
        $(this.ButtonsContainer).empty();
        this.Buttons = new Array();
        this.Selected = null;
        this.Value = null;
        this.LastValue = null;
    }
    for (var index in data) {
        var item = data[index];
        buttonsCount++;
        if (buttonsCount > this.MaxButtonsCount) {
            alert("Buttons limit for id=[" + this.Id + "]! Next element rendering canceled!");
            break;
        };
        var hAttributes = {
            id: this.Id,
            value: item.Id,
            attachedClassName: this.ClassNames[buttonsCount]
        };
        var bAttributes = {
            id: this.Id,
            value: item.Id,
            title: item.Name,
            className: this.ButtonsClassNames[buttonsCount],
            width: item.Name.length * 11 + "px"
        };
        var hiddenHTML = TemplateEngine.Format(hiddenTemplate, hAttributes);
        var buttonHTML = TemplateEngine.Format(buttonTemplate, bAttributes);
        var hiddenElement = $(hiddenHTML)[0];
        var buttonElement = $(buttonHTML)[0];
        this.HiddenRadioContainer.appendChild(hiddenElement);
        this.ButtonsContainer.appendChild(buttonElement);
    }
    this.FindButtons();

};

DinamicRadioButtonSetControl.prototype.FindButtons = function() {
    if (this.Element) {
        var elements = this.HiddenRadioContainer.getElementsByTagName("input");
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
