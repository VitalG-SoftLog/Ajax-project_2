var DateTimeControl = function (_parentObject, _id, _value, _defaultValue, _valueType, _dependentControls, _minDate) {
    this.Init(_parentObject, _id, _value, _defaultValue, _valueType, _dependentControls);
    this.Calendar = null;
    this.MinDate = _minDate;
    this.InitCalendar();
};

//Inheritance CalendarControl from TextBoxControl

ClassInheritance.Standart(DateTimeControl, TextBoxControl);

//Entending end overwriting class prototype members

DateTimeControl.prototype.InitCalendar = function () {
//    if (this.Element) {
//        var parentObj = this;

//        this.Calendar = $("#" + this.Id).timepicker({ ampm: true, hourMin: 8, hourMax: 16, minDate: this.MinDate,
//            onSelect: function (dateText, inst) {
//                parentObj.SetValue.call(parentObj, dateText);
//                parentObj.DefaultBlurFunction();
//            }
//        });

//        this.EventSet.AfterDataBind.List.SetCalendarSelectedDate = function () {
//        };

    //    }

    $("#" + this.Id).timepicker({
        ampm: true,
        hourMin: 8,
        hourMax: 16
    });
};

//DateTimeControl.prototype.OnOutOfRange = function () {
//    this.Value = "";
//    if (this.Element && this.Element.value != "" && this.Element.value != "12/31/-1") {
//        this.Element.value = "";
//        if (!this.CalendarName) {
//            alert("New date of some calendar is out of range! It will be cleared!");
//        } else {
//            alert(this.CalendarName + " is out of range! It will be cleared!");
//        }
//    }
//};

//DateTimeControl.prototype.OnBeforeOpen = function () {
//    var value = this.GetValue();
//    if (value == "" || value == "12/31/-1") return;
//    this.Calendar.SetSelectedDateFromDateString(this.GetValue());
//};

//DateTimeControl.prototype.OnBeforeClose = function () {
//    var value = this.GetValue();
//    if (value == "" || value == "12/31/-1") {
//        this.Calendar.ClearSelectedDate();
//    }
//};