var CalendarControl = function (_parentObject, _id, _value, _defaultValue, _valueType, _dependentControls, _minDate) {
    this.Init(_parentObject, _id, _value, _defaultValue, _valueType, _dependentControls);
    this.Calendar = null;
    this.MinDate = _minDate;
    this.InitCalendar();
};

//Inheritance CalendarControl from TextBoxControl

ClassInheritance.Standart(CalendarControl, TextBoxControl);

//Entending end overwriting class prototype members

CalendarControl.prototype.InitCalendar = function () {
    if (this.Element) {
        var parentObj = this;

        this.Calendar = $("#" + this.Id).datepicker({
            minDate: this.MinDate,
            onSelect: function (dateText, inst) {
                parentObj.SetValue.call(parentObj, dateText);
                parentObj.DefaultBlurFunction();
                parentObj.SelectValue();
            }

        });

        this.EventSet.AfterDataBind.List.SetCalendarSelectedDate = function () {
        };

    }
};

CalendarControl.prototype.SelectValue = function() {
    
};

CalendarControl.prototype.OnOutOfRange = function() {
    this.Value = "";
    if (this.Element && this.Element.value != "" && this.Element.value != "12/31/-1") {
        this.Element.value = "";
        if (!this.CalendarName) {
            alert("New date of some calendar is out of range! It will be cleared!");
        } else {
            alert(this.CalendarName + " is out of range! It will be cleared!");
        }
    }
};

CalendarControl.prototype.OnBeforeOpen = function() {
    var value = this.GetValue();
    if (value == "" || value == "12/31/-1") return;
    this.Calendar.SetSelectedDateFromDateString(this.GetValue());
};

CalendarControl.prototype.OnBeforeClose = function() {
    var value = this.GetValue();
    if (value == "" || value == "12/31/-1") {
        this.Calendar.ClearSelectedDate();
    }        
};