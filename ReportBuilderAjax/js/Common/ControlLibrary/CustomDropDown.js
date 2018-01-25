function CustomDropDown(parentElement, parentObject, data, width, isFilter, filterValue, usePrefix) {
    this.ParentElement = parentElement;
    this.ParentObject = parentObject;
    this.Data = data;
    this.Width = width;
    this.UsePrefix = (usePrefix == undefined) ? false : usePrefix;

    this.Element = null;
    this.OptionsElement = null;
    this.ControlButton = null;
    this.Input = null;
    this.FilterValue = filterValue;

    this.Value = new Array();
    this.ExcludeMode = false;
    this.Options = new Array();
    this.CurrentControl = null;
    this.IsClicked = false; //for firefox
    this.IsFilter = isFilter;
    this.SaveValues = new Array();
    this.ExcludeValues = new Array();
    this.Bind();
};

CustomDropDown.prototype.Bind = function () {
    var optionsHTML = '';

    for (var idx in this.Data) {
        var item = this.Data[idx];

        if (item) {
            var optionAttributes = {
                'elementId': item.Value,
                'elementValue': item.Name
            };

            optionsHTML += TemplateEngine.Format(TemplateManager.templates['DropDownTemplates_option'], optionAttributes);
        }
    }

    var attributes = {
        'options': optionsHTML
    };

    this.ParentElement.innerHTML = TemplateEngine.Format(TemplateManager.templates['DropDownTemplates_main'], attributes);

    this.Input = $('input[controlBox="1"]', this.ParentElement)[0];

    this.Input.style.width = this.Width - 27 + 'px';
    this.ParentElement.style.width = this.Width + 'px';
    this.Input.parentNode.style.width = this.Width - 27 + 'px';
    this.OptionsElement = $('div[class="options"]', this.ParentElement)[0];
    this.OptionsElement.style.display = 'none';
    this.OptionsElement.style.minWidth = this.Width + 'px';
    this.checkWidth();
    //this.OptionsElement.style.width = this.ParentElement.style.width.replace('px', '') * 1 - 10 + 'px';

    this.ControlButton = $('div[class="controlButton"]', this.ParentElement)[0];

    var parentObj = this;

    $(this.ControlButton).bind('click', function (sender) {
        parentObj.checkLists();
        parentObj.DrawOptions();
    });

    $(this.Input).bind('click', function (sender) {
        parentObj.checkLists();
        parentObj.DrawOptions();
    });

    this.FillOptions();

    this.Set_Value(null);

    if (!this.IsFilter) {
        var filterControl = $('tr[filterBox="1"]', this.ParentElement)[0];
        filterControl.style.display = 'none';

        //var althernative = $('tr[althernativeBox="1"]', this.ParentElement)[0];
        //althernative.style.display = '';
    }
    else {
        $('div[class="filterButton"]', this.ParentElement).bind('click', function () {

            var filterText = $('input[class="filterInput"]', this.ParentElement)[0];

            parentObj.FilterValue = filterText.value;
            parentObj.PreOnClick();
            parentObj.OnClick();

        });
    }

    var filterInput = $('input[class="filterInput"]', this.ParentElement)[0];

    filterInput.onkeydown = function (event) {
        parentObj.keyBoard.call(parentObj, event);
    };

    this.CheckMode();
};

CustomDropDown.prototype.FillOptions = function () {
    var parentObj = this;

    this.Options = null;
    this.Options = new Array();

    $('div[class="option"]', this.OptionsElement).each(function () {

        parentObj.Options[parentObj.Options.length] = this;

        $(this).bind('mouseover', function () {
            var checkbox = $('[checkbox="1"]', this)[0];
            if (checkbox.className == "checkboxUnchecked") {
                var text = $('td[text="1"]', this)[0];
                $(this).addClass('changeOption');
                $(text).addClass('changeName');
            }
        });

        $(this).bind('mouseout', function () {
            var checkbox = $('[checkbox="1"]', this)[0];
            if (checkbox.className == "checkboxUnchecked") {
                var text = $('td[text="1"]', this)[0];
                $(this).removeClass('changeOption');
                $(text).removeClass('changeName');
            }
        });

        $(this).bind('click', function () {
            if (!parentObj.IsClicked) {

                var checkbox = $('[checkbox="1"]', this)[0];
                if (checkbox.className == "checkboxUnchecked") {
                    checkbox.className = "checkboxChecked";
                } else {
                    checkbox.className = "checkboxUnchecked";
                }

                parentObj.checkValue(checkbox);

                parentObj.OnChange();
            }
            else {
                parentObj.IsClicked = false;
            }
        });
    });

    $('div[checkbox="1"]', this.OptionsElement).each(function () {
        $(this).bind('click', function () {
            if (!parentObj.IsClicked) {
                parentObj.IsClicked = true;

                if (this.className == "checkboxUnchecked") {
                    this.className = "checkboxChecked";
                } else {
                    this.className = "checkboxUnchecked";
                }

                parentObj.checkValue(this);

                parentObj.OnChange();
            }
            else {
                parentObj.IsClicked = false;
            }
        });
    });
};

CustomDropDown.prototype.OnChange = function() {

};

CustomDropDown.prototype.checkLists = function () {
    var parentObj = this;

    $('div[class="options"]').each(function () {
        if (parentObj.OptionsElement != this) {
            this.style.display = 'none';
        }
    });

    var docHeight = $(document).height();
    var currentHeight = $(this.ParentElement).offset().top;

    if (this.IsFilter) {
        if (docHeight < currentHeight + 320) {
            if (this.Data.length >= 9) {
                this.OptionsElement.style.marginTop = '-307px';
            }
            else {
                this.OptionsElement.style.marginTop = -(77 + 27 * this.Data.length) + 'px'; 
            }
        }
    }
    else {
        if (docHeight < currentHeight + 220) {
            if (this.Data.length >= 9) {
                this.OptionsElement.style.marginTop = '-247px';
            }
            else {
                this.OptionsElement.style.marginTop = -(18 + 27 * this.Data.length) + 'px';
            }            
        }
    }
};

CustomDropDown.prototype.checkWidth = function () {
    var minWidth = this.Width;

    if (this.IsFilter) {
        minWidth = 290;
    }

    var maxlength = 0;

    for (var idx in this.Data) {
        var item = this.Data[idx];

        if (item.Name.length > maxlength)
            maxlength = item.Name.length;

        if (!this.IsFilter) {
            if (item && maxlength > 30) {
                minWidth = maxlength * 8;
            }
        }
        else {
            if (item && maxlength > 40) {
                minWidth = maxlength * 8;
            }
        }
    }

    this.OptionsElement.style.minWidth = minWidth + 'px';
};

CustomDropDown.prototype.CheckMode = function () {
    var options = this.Data.length;

    var checkValues = 0;

    $('div[checkbox="1"]', this.OptionsElement).each(function () {
        if (this.className == 'checkboxChecked') {
            checkValues++;
        }
    });

    if (checkValues > options / 2 && this.UsePrefix) {
        this.ExcludeMode = true;       
    }
    else {
        this.ExcludeMode = false;
    }
};

CustomDropDown.prototype.OnClick = function() {
};

CustomDropDown.prototype.PreOnClick = function () {
    var existValue = false;
    var parentObj = this;
    $('div[checkbox="1"]', this.OptionsElement).each(function () {
        var id = this.getAttribute('elementId');
        if (this.className == "checkboxUnchecked" && id != '') {
            for (var tInd in parentObj.SaveValues) {
                var tItem = parentObj.SaveValues[tInd];
                if (tItem != null) {
                    if (tItem.Value == id) {
                        parentObj.SaveValues[tInd] = null;
                    }
                }
            }
        }

        if (parentObj.ExcludeMode) {
            parentObj.ExcludeValues[parentObj.ExcludeValues.length] = {
                Value: id,
                Name: ''
            };
        }
    });

    if (parentObj.ExcludeMode) {
        $('div[checkbox="1"]', this.OptionsElement).each(function () {
            var id = this.getAttribute('elementId');
            if (this.className == "checkboxChecked" && id != '') {

                for (var exInd in parentObj.ExcludeValues) {
                    var exItem = parentObj.ExcludeValues[exInd];
                    if (exItem != null) {
                        if (exItem.Value == id) {
                            parentObj.ExcludeValues[exInd] = null;
                        }
                    }
                }
            }
        });
    }
    
    for (var idx in this.Value) {
        var item = this.Value[idx];
        if (item) {
            for (var ind in this.SaveValues) {
                var saveItem = this.SaveValues[ind];

                if (saveItem != null) {
                    if (saveItem.Value == item.Value) {
                        existValue = true;
                    }
                }
            }

            if (!existValue) {
                this.SaveValues[this.SaveValues.length] = item;
            }

            existValue = false;
        }
    }

    this.Value = null;
    this.Value = new Array();
    this.Value = this.SaveValues;
};

CustomDropDown.prototype.keyBoard = function (event) {
    var e = window.event;
    if (!e) e = event;
    window.status = e.keyCode;
    if (e.keyCode == 13) {

        var parentObj = this;

        $('div[class="options"]', document).each(function () {
            if (parentObj != null && this.style.display == '') {
                
                
                var filterText = $('input[class="filterInput"]', this)[0];
                parentObj.FilterValue = filterText.value;
                parentObj.PreOnClick();
                parentObj.OnClick();
            }
        });
    }
};

CustomDropDown.prototype.checkValue = function (control) {

    var elementId = control.getAttribute('elementId');

    if (elementId == '' && control.className == "checkboxUnchecked") {
        $('div[checkbox="1"]', this.OptionsElement).each(function () {
            this.className = "checkboxUnchecked";
        });

        if (this.FilterValue == '') {
            this.ExcludeMode = false;
            this.SaveValues = null;
            this.ExcludeValues = null;
            this.SaveValues = new Array();
            this.ExcludeValues = new Array();
        }
    }
    else if (elementId == '' && control.className == "checkboxChecked") {
        $('div[checkbox="1"]', this.OptionsElement).each(function () {
            this.className = "checkboxChecked";
        });

        if (this.FilterValue == '') {
            if (this.UsePrefix) this.ExcludeMode = true;
            this.SaveValues = null;
            this.ExcludeValues = null;
            this.SaveValues = new Array();
            this.ExcludeValues = new Array();
        }
    }
    else if (control.className == "checkboxUnchecked") {
        $('div[checkbox="1"]', this.OptionsElement).each(function () {
            var attr = this.getAttribute('elementId');

            if (attr == '') {
                this.className = "checkboxUnchecked";
            }
        });
    }
    else if (control.className == "checkboxChecked") {
        var allvalues = true;
        var allControl = null;

        $('div[checkbox="1"]', this.OptionsElement).each(function () {
            var attr = this.getAttribute('elementId');

            if (attr != '') {
                if (this.className == "checkboxUnchecked") allvalues = false;
            }
            else {
                allControl = this;
            }
        });

        if (allvalues) allControl.className = "checkboxChecked"; ;
    }

    this.CurrentControl = control;
    this.PushValue();
};

CustomDropDown.prototype.PushValue = function () {
    var parentObj = this;

    var allChecked = false;

    parentObj.Value = null;
    parentObj.Value = new Array();

    $('div[checkbox="1"]', this.OptionsElement).each(function () {
        var value = this.getAttribute('elementId');
        var name = this.getAttribute('elementValue');

        if (value == '') {
            if (this.className == "checkboxChecked") {
                allChecked = true;

                if (parentObj.FilterValue == '') {
                    parentObj.Value[parentObj.Value.length] = {
                        Value: value,
                        Name: name
                    };
                }
                else {
                    allChecked = false;
                }
            }
        }

        if (!allChecked && this.className == "checkboxChecked") {
            if (value != '') {
                parentObj.Value[parentObj.Value.length] = {
                    Value: value,
                    Name: name
                };
            }
        }

    });

    this.Set_Value(parentObj.Value);
};

CustomDropDown.prototype.DrawOptions = function () {

    if (this.OptionsElement.style.display == '') {
        this.OptionsElement.style.display = 'none';
    }
    else {
        this.OptionsElement.style.display = '';
    }


    var elements = $('div[class="elements"]', this.OptionsElement)[0];

    if (this.Data.length > 10) {
        elements.style.height = '200px';
    }

};

CustomDropDown.prototype.Get_Value = function () {
    var value = '';
    if (this.ExcludeMode) {

        if (this.UsePrefix) {
            value += 'e;';
        }

        if (this.ExcludeValues && this.ExcludeValues.length > 0) {
            for (var idx in this.ExcludeValues) {
                var item = this.ExcludeValues[idx];
                if (item && item != null) {
                    if (item.Value != '') {
                        value += item.Value + ',';
                    }
                }
            }
        }
        else {
            $('div[class="checkboxUnchecked"]', this.OptionsElement).each(function () {
                var elementId = this.getAttribute('elementId');

                if (elementId != '') {
                    value += elementId + ',';
                }
            });
        }

    }
    else {
        if (this.UsePrefix) {
            value += 'i;';
        }

        if (this.SaveValues && this.SaveValues.length > 0) {
            for (var idx in this.SaveValues) {
                var item = this.SaveValues[idx];
                if (item && item != null) {
                    if (item.Value != '') {
                        value += item.Value + ',';
                    }
                }
            }
        }
        else {
            $('div[class="checkboxChecked"]', this.OptionsElement).each(function () {
                var elementId = this.getAttribute('elementId');

                if (elementId != '') {
                    value += elementId + ',';
                }
            });
        }
    }

    if (value.length > 2) {
        value = value.substring(0, value.length - 1);
    }

    if (!this.UsePrefix) {
        var isAll = true;
        $('div[class="checkboxUnchecked"]', this.OptionsElement).each(function () {
            var elementId = this.getAttribute('elementId');

            if (elementId != '') {
                isAll = false;
            }
        });

        if (isAll) value = '';
    }
    return value;
};

CustomDropDown.prototype.ReBind = function (data) {
    var options = $('div[class="elements"]', this.ParentElement)[0];

    options.innerHTML = 0;

    var optionsHTML = '';
    this.Data = data;

    for (var idx in this.Data) {
        var item = this.Data[idx];

        if (item) {
            var optionAttributes = {
                'elementId': item.Value,
                'elementValue': item.Name
            };

            optionsHTML += TemplateEngine.Format(TemplateManager.templates['DropDownTemplates_option'], optionAttributes);
        }
    }

    options.innerHTML = optionsHTML;

    this.checkWidth();
    this.FillOptions();
    this.Set_Value(this.Value);
};

CustomDropDown.prototype.Set_ValueMode = function (values) {
    this.Value = null;
    this.Value = new Array();

    if (values != '') {

        var splitter = values.split(';');

        if (splitter.length == 1 && values.indexOf('e') < 0) {
            this.ExcludeMode = false;

            var itemSplitter = values.split(',');

            for (var idx in itemSplitter) {
                var item = itemSplitter[idx];

                if (item) {
                    this.Value[this.Value.length] = {
                        Value: item,
                        Name: ''
                    };
                }
            }
        }
        else if (splitter.length == 1 && values.indexOf('e') > -1) {
            this.Value[this.Value.length] = {
                Value: '',
                Name: ''
            };
        }
        else if (splitter.length > 1) {
            var mode = splitter[splitter.length - 2];

            if (mode.indexOf('e') > -1) {
                this.ExcludeMode = true;

                var itemsSplitter = splitter[splitter.length - 1].split(',');

                var isExist = false;

                for (var i in this.Data) {
                    var value = this.Data[i];
                    if (value) {
                        for (var it in itemsSplitter) {
                            var item = itemsSplitter[it];

                            if (item) {
                                if (item == value.Value || value.Value == '') {
                                    isExist = true;
                                }
                            }
                        }

                        if (!isExist) {
                            this.Value[this.Value.length] = {
                                Value: value.Value,
                                Name: ''
                            };
                        }
                    }

                    isExist = false;
                }
            }

            if (mode.indexOf('i') > -1) {
                this.ExcludeMode = false;

                var itemsSplitter = splitter[splitter.length - 1].split(',');

                for (var it in itemsSplitter) {
                    var item = itemsSplitter[it];

                    if (item) {
                        this.Value[this.Value.length] = {
                            Value: item,
                            Name: ''
                        };
                    }
                }
            }
        }
    }

    this.Set_Value(this.Value);
};

CustomDropDown.prototype.Set_Value = function (values) {
    
    if (values == null) {
        this.Input.value = 'ALL';
    }
    else if (values.length == 0) {
        this.Input.value = '';
    }
    else if (values.length == 1) {
        for (var idx in values) {
            var value = values[idx];
            if (value && value.Value == '') {
                this.Input.value = 'ALL';
            }
            else {
                this.Input.value = value.Value;
            }
        }
    }
    else {
        counter = 0;
        for (var v in values) {
            var item = values[v];
            if (item != null) {
                counter++;
            }
        }
        if (counter >= this.Data.length - 1) {
            this.Input.value = 'ALL';
        }
        else {
            for (var idx in values) {
                var value = values[idx];
                if (value && value.Value == '') {
                    this.Input.value = 'ALL';
                } else {
                    this.Input.value = 'Multiple Items Selected';
                }
            }
        }
    }

    this.Value = null;
    this.Value = new Array();

    if (values == null) {

        for (var idx in this.Data) {
            var item = this.Data[idx];

            if (item.Value != '') {
                this.Value[this.Value.length] = item;
            }
        }

    }
    else if (values.length == 1) {
        if (values[0].Value == '') {
            for (var idx in this.Data) {
                var item = this.Data[idx];

                if (item.Value != '') {
                    this.Value[this.Value.length] = item;
                }
            }
        }
        else {
            this.Value = values;
        }
    }
    else {
        this.Value = values;
    }

    this.checkedValues(this.Value);
    this.OverlayOptions();
};

CustomDropDown.prototype.OverlayOptions = function () {
    var parentObj = this;

    for (var idx in this.Options) {
        var option = this.Options[idx];

        if (option) {
            $('div[checkbox="1"]', option).each(function () {
                var text = $('td[text="1"]', option)[0];
                
                if (this.className == "checkboxChecked") {
                    $(option).addClass('changeOption');
                    $(text).addClass('changeName');
                }
                else {
                    if (parentObj.CurrentControl) {
                        var currentCheckValue = parentObj.CurrentControl.getAttribute('elementId');
                        var itemValue = this.getAttribute('elementId');
                        if (currentCheckValue != itemValue) {
                            $(option).removeClass('changeOption');
                            $(text).removeClass('changeName');
                        }
                    }
                    else {
                        $(option).removeClass('changeOption');
                        $(text).removeClass('changeName');
                    }
                }
            });
        }
    }
};

CustomDropDown.prototype.checkedValues = function (values) {
    $('div[checkbox="1"]', this.OptionsElement).each(function () {
        this.className = "checkboxUnchecked";
    });
    
    if (this.Input.value == 'ALL') {
        $('div[checkbox="1"]', this.OptionsElement).each(function () {
            this.className = "checkboxChecked";
        });
    }
    else {
        $('div[checkbox="1"]', this.OptionsElement).each(function () {

            var itemValue = this.getAttribute('elementId');

            for (var idx in values) {
                var value = values[idx];
                if (value && value.Value == itemValue) {
                    this.className = "checkboxChecked";
                }
            }
        });
    }
};


$(document).click(function (sender) {
    var element = sender.srcElement || sender.target;
    if (element.className != 'input' && element.className != 'controlButton' && element.className != 'option' && element.className != 'checkboxChecked' && element.className != 'checkboxUnchecked') {
        
        var clicker = element.getAttribute('clicker');
        
        if (clicker != '1' || clicker != 1) {
            $('div[combobox="1"]').each(function () {
                var optionsElement = $('div[class="options"]', this)[0];
                optionsElement.style.display = 'none';
            });
        }                
    }
});
