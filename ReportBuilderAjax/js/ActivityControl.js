function ActivityControl(id, parentObject, data, operandTypes, totalActivities) {
    this.Data = data;
    this.Element = document.getElementById(id);
    this.ParentObject = parentObject;
    this.OperandTypes = ">,>=,<,<=,=";
    this.OperandTypes = operandTypes && operandTypes.length > 0 ? operandTypes : this.OperandTypes;
    this.TotalActivities = "Any,Total Incurred,Total Paid,Outstanding Reserves";
    this.TotalActivities = totalActivities && totalActivities.length > 0 ? totalActivities : this.TotalActivities;
    this.Controls = new Array();

    this.Index = 0;
    this.MinIndex = this.Index;
};

ActivityControl.prototype.Render = function () {
    this.Element.innerHTML = '';

    if (this.Data && this.Data.length > 0) {
        this.SetActivities();
    }
    else {
        this.CreateActivityRow(this.Index++);
    }

};


ActivityControl.prototype.CreateActivityRow = function (index) {

    var parentObject = this;

    var attributes = {
        'activitiesComboBox': this.getClientID('activitiesComboBoxContainer_' + index),
        'operandComboBox': this.getClientID('operandComboBoxContainer_' + index),
        'activityInput': this.getClientID('activityInput_' + index),
        'addActivity': this.getClientID('addActivity_' + index),
        'removeActivity': this.getClientID('removeActivity_' + index),
        'clearActivity': this.getClientID('clearActivity_' + index),
        'index': index
    };

    $(this.Element).append(TemplateEngine.Format(TemplateManager.templates['Activity_ActivityItem'], attributes));

    this.Controls[index] = {};

    this.Controls[index].ActivityTextBox = new TextBoxControl(this, attributes.activityInput, "0.00", "0.00", ValueType.String, null);

    var activityElement = document.getElementById(this.getClientID('clearActivity_' + index));

    if (index > this.MinIndex) {        
        activityElement.style.display = 'none';
    }
    else {
        activityElement.style.display = '';
    }

    var activitiesComboBoxAttr = {
        "idDropDown": this.getClientID("activitiesComboBox_" + index),
        "dropDownHide": "dropDownHide"
    };
    var activitiesComboBoxTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_DropDownControl'], activitiesComboBoxAttr);
    $(document.getElementById(this.getClientID('activitiesComboBoxContainer_' + index))).append(activitiesComboBoxTempl);
    this.Controls[index].ActivityComboBox = new newDropDownControl(this, activitiesComboBoxAttr["idDropDown"], "", "", ValueType.String, null, false, 117);

    var operandComboBoxAttr = {
        "idDropDown": this.getClientID("operandComboBox_" + index),
        "dropDownHide": "dropDownHide"
    };
    var operandComboBoxTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_DropDownControl'], operandComboBoxAttr);
    $(document.getElementById(this.getClientID('operandComboBoxContainer_' + index))).append(operandComboBoxTempl);
    this.Controls[index].OperandComboBox = new newDropDownControl(this, operandComboBoxAttr["idDropDown"], "", "", ValueType.String, null, false, 45);

    this.FillComboboxes(this.Controls[index].ActivityComboBox, this.Controls[index].OperandComboBox);


    this.Controls[index].ActivityComboBox.PropertyChanged = function (e) {
        if (this.Value == 'Any') {
            $(parentObject.Controls[index].OperandComboBox.Element).parents('div[templateId]')[0].style.visibility = 'hidden';
            $(parentObject.Controls[index].ActivityTextBox.Element).parents('div.activityInput')[0].style.visibility = 'hidden';
            $(parentObject.Controls[index].ActivityTextBox.Element).css("visibility", 'hidden');

            parentObject.Controls[index].OperandComboBox.SetDropDownValue(">");
            parentObject.Controls[index].ActivityTextBox.SetValue("0.00");
        }
        else {
            $(parentObject.Controls[index].OperandComboBox.Element).parents('div[templateId]')[0].style.visibility = 'visible';
            $(parentObject.Controls[index].ActivityTextBox.Element).parents('div.activityInput')[0].style.visibility = 'visible';
            $(parentObject.Controls[index].ActivityTextBox.Element).css("visibility", 'visible');
        }
    };

    this.Controls[index].ActivityComboBox.SetDropDownValue("Any");
    this.Controls[index].OperandComboBox.SetDropDownValue(">");

    var addActivityBtnTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_plusBtnTemplate'], {});
    $(document.getElementById(this.getClientID('addActivity_' + index))).append(addActivityBtnTempl);

    var removeActivityBtnTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_minusBtnTemplate'], {});
    $(document.getElementById(this.getClientID('removeActivity_' + index))).append(removeActivityBtnTempl);

    var clearActivityAttr = {
        "text": "CLEAR",
        "left": -20,
        "width": 45,
        "width1": 53
    };

    var clearActivityBtnTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_BtnTemplate'], clearActivityAttr);
    $(document.getElementById(this.getClientID('clearActivity_' + index))).append(clearActivityBtnTempl);

    $('#' + this.getClientID('addActivity_' + index)).click(function () {
        var idx = this.getAttribute('index') * 1;

        parentObject.CreateActivityRow(parentObject.Index++);
    });


    $('#' + this.getClientID('removeActivity_' + index)).click(function () {
        var idx = this.id.split('_')[2] * 1;

        if ($('div.activityItem', parentObject.Element).length > 1) {

            parentObject.RemoveActivity(idx);

            if (idx == parentObject.MinIndex) {
                parentObject.MinIndex = parentObject.GetMinIndex();

                var activityElement = document.getElementById(parentObject.getClientID('clearActivity_' + parentObject.MinIndex));
                activityElement.style.display = '';
            }
            
            RightTabHeight();
        }

    });

    $('#' + this.getClientID('clearActivity_' + index)).click(function () {
        var idx = this.id.split('_')[2] * 1;
        parentObject.ClearFilter();
    });

    this.Element.style.height = this.Element.style.height.length == 0 ? "58px" : this.Element.style.height.replace('px', '') * 1 + 58 + 'px';
};

ActivityControl.prototype.GetMinIndex = function () {
    var minIndex = 0;

    for (var idx in this.Controls) {
        var control = this.Controls[idx];
        if (control != null && idx > minIndex) {
            minIndex = idx;
        }
    }

    for (var i in this.Controls) {
        var ctrl = this.Controls[i];
        if (ctrl != null && i < minIndex) {
            minIndex = i;
        }
    }

    this.MinIndex = minIndex;
    return minIndex;
};

ActivityControl.prototype.RemoveActivity = function (index) {
    $('div.activityItem[index=' + index + ']').remove();
    this.Element.style.height = this.Element.style.height.replace('px', '') * 1 - 58 + 'px';
    this.Controls[index] = null;
    

};

ActivityControl.prototype.FillComboboxes = function (activityComboBox, operandTypesComboBox) {
    var activities = this.TotalActivities.split(',');
    var operandTypes = this.OperandTypes.split(',');
    
    for(var i in activities) {
        activityComboBox.AppendOption(activities[i], activities[i]);
    }
    for (var i in operandTypes) {
        operandTypesComboBox.AppendOption(operandTypes[i], operandTypes[i]);
    }
};

ActivityControl.prototype.SetActivities = function () {
    var activities = this.Data.split(',');

    for (var i = 1; i <= activities.length; i += 3) {
        if (activities[i - 1] != '') {
            this.CreateActivityRow(this.Index);
            this.Controls[this.Index].ActivityComboBox.SetDropDownValue(activities[i - 1]);
            this.Controls[this.Index].OperandComboBox.SetDropDownValue(activities[i]);
            this.Controls[this.Index].ActivityTextBox.SetValue(activities[i + 1]);
        } else {
            this.CreateActivityRow(this.Index);
        }
        
        this.Index++;
    }
};

ActivityControl.prototype.GetActivities = function () {
    var result = "";
    var parentObj = this;
    $('div.activityItem', this.Element).each(function () {
        var idx = this.getAttribute('index');
        result += parentObj.Controls[idx].ActivityComboBox.GetValue() + ',';
        result += parentObj.Controls[idx].OperandComboBox.GetValue() + ',';
        result += parentObj.Controls[idx].ActivityTextBox.GetValue() + ',';
    });

    return result.substr(0, result.length - 1);
};

ActivityControl.prototype.ClearFilter = function () {

    for (var idx in this.Controls) {
        var control = this.Controls[idx];

        if (idx * 1 > this.MinIndex && control) {
            this.RemoveActivity(idx);
            RightTabHeight();
        }
        else if (idx * 1 == this.MinIndex && control) {
            this.Controls[idx].ActivityComboBox.SetDropDownValue("Any");
            this.Controls[idx].OperandComboBox.SetDropDownValue(">");
            this.Controls[idx].ActivityTextBox.SetValue("0.00");

            this.MinIndex = idx;
        }
    }    
};


ActivityControl.prototype.getClientID = function (name) {
    return this.Element.id + '_' + name;
};