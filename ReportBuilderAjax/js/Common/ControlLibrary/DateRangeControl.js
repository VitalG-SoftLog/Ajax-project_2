var DateRangeControl = function (id, reportId) {
    this.ReportId = reportId;
    this.Element = document.getElementById(id);
    this.Id = id;
    this.Controls = new Array();
    this.DateRanges = new Array();
    this.HoursList = new Array();
    this.MinutesList = new Array();
    this.AMPMList = new Array();
    this.Data = new Array();
};

DateRangeControl.prototype.Bind = function (template) {
    var attr = {
        "lastXDays": this.getClientID("lastXDays"),
        "startHours": "startHours",
        "startMinutes": "startMinutes",
        "startAMPM": "startAMPM",
        "endHours": "endHours",
        "endMinutes": "endMinutes",
        "endAMPM": "endAMPM"
    };

    $(this.Element).append(TemplateEngine.Format(template, attr));
    //Start Data range dropdown menu
    var DropDownAttr = {
        "idDropDown": this.getClientID("DateRange"),
        "dropDownHide": "dropDownHide",
        "attribute": "DateRange"
    };
    var DropDownTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_DropDownControl'], DropDownAttr);
    $(document.getElementById("DateRange")).append(DropDownTempl);

    this.Controls.DataRangeDropDown = new newDropDownControl(this, DropDownAttr["idDropDown"], -1, -1, ValueType.Int, null, false, 200);
    this.Controls.DataRangeDropDown.ClearOptions();

    this.Controls.DataRangeDropDown.EventSet.AfterDataBind.List.SetDisplayState = function (e) {
        this.ParentObject.DateRangeChanged(this.GetOptionValue(this.Value));
    };

    var startHours = {
        "idDropDown": this.getClientID("startHours"),
        "dropDownHide": "dropDownHide"
    };
    var startHoursTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_DropDownControl'], startHours);
    $(document.getElementById("startHours")).append(startHoursTempl);
    this.Controls.StartHours = new newDropDownControl(this, this.getClientID("startHours"), -1, -1, ValueType.Int, null, false, 65);
    this.Controls.StartHours.ClearOptions();

    var startMinutes = {
        "idDropDown": this.getClientID("startMinutes"),
        "dropDownHide": "dropDownHide"
    };
    var startMinutesTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_DropDownControl'], startMinutes);
    $(document.getElementById("startMinutes")).append(startMinutesTempl);
    this.Controls.StartMinutes = new newDropDownControl(this, this.getClientID("startMinutes"), -1, -1, ValueType.Int, null, false, 65);
    this.Controls.StartMinutes.ClearOptions();

    var startAMPM = {
        "idDropDown": this.getClientID("startAMPM"),
        "dropDownHide": "dropDownHide"
    };
    var startAMPMTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_DropDownControl'], startAMPM);
    $(document.getElementById("startAMPM")).append(startAMPMTempl);
    this.Controls.StartAMPM = new newDropDownControl(this, this.getClientID("startAMPM"), -1, -1, ValueType.Int, null, false, 65);
    this.Controls.StartAMPM.ClearOptions();

    var endHours = {
        "idDropDown": this.getClientID("endHours"),
        "dropDownHide": "dropDownHide"
    };
    var endHoursTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_DropDownControl'], endHours);
    $(document.getElementById("endHours")).append(endHoursTempl);
    this.Controls.EndHours = new newDropDownControl(this, this.getClientID("endHours"), -1, -1, ValueType.Int, null, false, 65);
    this.Controls.EndHours.ClearOptions();

    var endMinutes = {
        "idDropDown": this.getClientID("endMinutes"),
        "dropDownHide": "dropDownHide"
    };
    var endMinutesTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_DropDownControl'], endMinutes);
    $(document.getElementById("endMinutes")).append(endMinutesTempl);
    this.Controls.EndMinutes = new newDropDownControl(this, this.getClientID("endMinutes"), -1, -1, ValueType.Int, null, false, 65);
    this.Controls.EndMinutes.ClearOptions();

    var endAMPM = {
        "idDropDown": this.getClientID("endAMPM"),
        "dropDownHide": "dropDownHide"
    };
    var endAMPMTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_DropDownControl'], endAMPM);
    $(document.getElementById("endAMPM")).append(endAMPMTempl);
    this.Controls.EndAMPM = new newDropDownControl(this, this.getClientID("endAMPM"), -1, -1, ValueType.Int, null, false, 65);
    this.Controls.EndAMPM.ClearOptions();

    var parentObj = this;
    this.Controls.LastXDays = new TextBoxControl(this, this.getClientID("lastXDays"), "", "", ValueType.Int);
    $(this.Controls.LastXDays.Element).bind("keyup", function (e) {
        parentObj.validateInteger.call(this, e);
    });

    $(this.Controls.LastXDays.Element).bind("blur", function (e) {
        var nowDate = new Date();
        var newStart = new Date(nowDate.getFullYear(), nowDate.getMonth(), nowDate.getDate() - (parentObj.Controls.LastXDays.Value * 1), 0, 0, 0, 0);
        parentObj.Controls.DateFromControl.SetValue(returnShortDate(newStart));
    });

    //Start DateFrom calendar input
    var calendarDateFrom = {
        "idCalendarInput": this.getClientID("DateFrom"),
        "idCalendarContainer": "DateFromContainer"
    };
    var calendarDateFromTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_CalendarInput'], calendarDateFrom);
    $(document.getElementById("DateFrom")).append(calendarDateFromTempl);
    $(document.getElementById("DateFrom")).focus(function () {
        $(document.getElementById("DateFromContainer")).removeClass('calendarConteiner');
        $(document.getElementById("DateFromContainer")).addClass('calendarConteinerFocus');
    });
    $("#" + this.getClientID("DateFrom")).bind('blur', function () {
        $(document.getElementById("DateFromContainer")).removeClass('calendarConteinerFocus');
        $(document.getElementById("DateFromContainer")).addClass('calendarConteiner');
    });

    this.Controls.DateFromControl = new CalendarControl(this, calendarDateFrom["idCalendarInput"], "", "", ValueType.Date, {});

    //End DateFrom calendar input

    //Start DateTo calendar input
    var calendarDateTo = {
        "idCalendarInput": this.getClientID("DateTo"),
        "idCalendarContainer": "DateToContainer"
    };
    var calendarDateToTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_CalendarInput'], calendarDateTo);
    $(document.getElementById("DateTo")).append(calendarDateToTempl);
    $(document.getElementById("DateTo")).focus(function () {
        $(document.getElementById("DateToContainer")).removeClass('calendarConteiner');
        $(document.getElementById("DateToContainer")).addClass('calendarConteinerFocus');
    });
    $("#" + this.getClientID("DateTo")).bind('blur', function () {
        $(document.getElementById("DateToContainer")).removeClass('calendarConteinerFocus');
        $(document.getElementById("DateToContainer")).addClass('calendarConteiner');
    });

    this.Controls.DateToControl = new CalendarControl(this, calendarDateTo["idCalendarInput"], "", "", ValueType.Date, {});
    //End DateTo calendar input

    this.fillHours();
    this.fillMinutes();
    this.fillAMPM();
    this.fillDataRanges();

    this.Controls.StartHours.SetDropDownValue(12);
    this.Controls.StartMinutes.SetDropDownValue(0);
    this.Controls.StartAMPM.SetDropDownValue("AM");
    this.Controls.EndHours.SetDropDownValue(11);
    this.Controls.EndMinutes.SetDropDownValue(59);
    this.Controls.EndAMPM.SetDropDownValue("PM");

    this.fillData();

    // change values 

    var parentObj = this;

    this.Controls.DateFromControl.SelectValue = function () {
        var min = parentObj.Controls.DateFromControl.GetValue();
        $(parentObj.Controls.DateToControl.Calendar).datepicker('option', { minDate: min });        
    };

    this.Controls.DateToControl.SelectValue = function () {
        var max = parentObj.Controls.DateToControl.GetValue();
        $(parentObj.Controls.DateFromControl.Calendar).datepicker('option', { maxDate: max });  
    };
    //
};

DateRangeControl.prototype.getDataItemByName = function (name) {
    for (var key1 in this.Data) {
        if (this.Data[key1].FilterName.toString().toLowerCase() == name) {
            return this.Data[key1];
        }
    }
    return null;
};

DateRangeControl.prototype.fillData = function () {
    var item = this.getDataItemByName("dateperiod") || this.getDataItemByName("daterange");

    if (item != null && item.Value != null) {
        this.Controls.DataRangeDropDown.SetDropDownValue(item.Value * 1);

        if (item.Value * 1 == Consts.DATE_RANGE.CUSTOM) {
            var startDate = new Date();
            var endDate = new Date();
            var startDateItem = this.getDataItemByName("startdate");
            var endDateItem = this.getDataItemByName("enddate");
            if (startDateItem != null) {
                startDate = new Date(returnShortDate(startDateItem.Value.toString().substr(0, startDateItem.Value.toString().length - 3)));
            }

            if (endDateItem != null) {
                endDate = new Date(returnShortDate(endDateItem.Value.toString().substr(0, endDateItem.Value.toString().length - 3)));
            }

            if (startDate.getFullYear().toString() == "1900") {
                startDate = new Date();
            }
            this.Controls.DateFromControl.SetValue(returnShortDate(startDate));
            this.Controls.DateToControl.SetValue(returnShortDate(endDate));

            var min = this.Controls.DateFromControl.GetValue();
            $(this.Controls.DateToControl.Calendar).datepicker('option', { minDate: min });

            var max = this.Controls.DateToControl.GetValue();
            $(this.Controls.DateFromControl.Calendar).datepicker('option', { maxDate: max });

            this.fillTimeComboboxes(startDateItem.Value.toString(), true);
            this.fillTimeComboboxes(endDateItem.Value.toString(), false);
        }
        else if (item.Value * 1 == Consts.DATE_RANGE.LAST_X_DAYS) {
            var lastXDaysItem = this.getDataItemByName("lastxdays");
            if (lastXDaysItem != null) {
                this.Controls.LastXDays.SetValue(lastXDaysItem.Value * 1);
            }
        }
    }
};

DateRangeControl.prototype.getValueByName = function (name) {
    for (var key in this.Data) {
        var item = this.Data[key];
        if (!item || item.Value == null) continue;
        if (item.FilterName.toString().toLowerCase() == name.toString().toLowerCase()) {
            return item.DefaultValue;
        }
    }
    return null;
};

DateRangeControl.prototype.validateInteger = function (e) {
    var keyCode = e.keyCode;
    if ((keyCode >= 48 && keyCode <= 57) || (keyCode >= 35 && keyCode <= 40) || keyCode == 8 || keyCode == 46 || (keyCode >= 96 && keyCode <= 105) || keyCode == 144 || keyCode == 20) return;
    var str = this.value;
    var re = /[^0-9]/gi;
    this.value = str.replace(re, "");
};

DateRangeControl.prototype.DateRangeChanged = function (value) {
    var nowDate = new Date();
    this.setTimeVisibility(false);
    this.Controls.LastXDays.Hide();
    document.getElementById(this.getClientID("DateRange") + "_width").style.width = '175px';
    document.getElementById(this.getClientID("DateRange") + "_showDiv").style.width = '175px';
    $(document.getElementById("LastXDaysSkin")).hide();
    var newStart = nowDate;
    var newEnd = new Date(nowDate.getFullYear(), nowDate.getMonth() + 1, nowDate.getDate(), 0, 0, 0, 0);
    this.Controls.DateFromControl.Calendar.datepicker('disable');
    this.Controls.DateToControl.Calendar.datepicker('disable');

    this.Controls.StartHours.SetDropDownValue(12);
    this.Controls.StartMinutes.SetDropDownValue(0);
    this.Controls.StartAMPM.SetDropDownValue("AM");
    this.Controls.EndHours.SetDropDownValue(11);
    this.Controls.EndMinutes.SetDropDownValue(59);
    this.Controls.EndAMPM.SetDropDownValue("PM");

    switch (value * 1) {
        case Consts.DATE_RANGE.LAST_MONTH:
            var filterYear = nowDate.getFullYear();
            var filterMonth = nowDate.getMonth() + 1;
            if (filterMonth == 1) {
                filterMonth = 12;
                filterYear--;
            }
            else {
                filterMonth--;
            }
            newStart = new Date(filterYear, filterMonth - 1, 1, 0, 0, 0, 0);
            newEnd = new Date(filterYear, filterMonth - 1, daysInMonth(filterYear, filterMonth), 23, 59, 59, 0);
            break;
        case Consts.DATE_RANGE.LAST_QUARTER:
            var qyear = (nowDate.getMonth() + 1 <= 3) ? (nowDate.getFullYear() - 1) : (nowDate.getFullYear());
            newStart = new Date(qyear, (nowDate.getMonth() - ((nowDate.getMonth()) % 3) + 9) % 12, 1, 0, 0, 0, 0);
            newEnd = new Date(qyear, (newStart.getMonth() + 2), daysInMonth(qyear, (newStart.getMonth() + 3)), 23, 59, 59, 0);
            break;
        case Consts.DATE_RANGE.LAST_WEEK:
            newStart = new Date(nowDate.getFullYear(), nowDate.getMonth(), (nowDate.getDate() - nowDate.getDay() - 6), 0, 0, 0, 0);
            newEnd = new Date(newStart.getFullYear(), newStart.getMonth(), (newStart.getDate() - newStart.getDay() + 7), 23, 59, 59, 0);
            break;
        case Consts.DATE_RANGE.LAST_CALENDAR_WEEK:
            newStart = new Date(nowDate.getFullYear(), nowDate.getMonth(), (nowDate.getDate() - nowDate.getDay() - 7), 0, 0, 0, 0);
            newEnd = new Date(nowDate.getFullYear(), nowDate.getMonth(), nowDate.getDate() - nowDate.getDay() - 1, 23, 59, 59, 0);
            break;
        case Consts.DATE_RANGE.LAST_BUSINESS_WEEK:
            newStart = new Date(nowDate.getFullYear(), nowDate.getMonth(), (nowDate.getDate() - nowDate.getDay() - 6), 0, 0, 0, 0);
            newEnd = new Date(nowDate.getFullYear(), nowDate.getMonth(), nowDate.getDate() - nowDate.getDay() - 2, 23, 59, 59, 0);
            break;
        case Consts.DATE_RANGE.YESTERDAY:
            newStart = new Date(nowDate.getFullYear(), nowDate.getMonth(), nowDate.getDate() - 1, 0, 0, 0, 0);
            newEnd = new Date(nowDate.getFullYear(), nowDate.getMonth(), nowDate.getDate() - 1, 23, 59, 59, 0);
            break;
        case Consts.DATE_RANGE.PREVIOUS_BUSINESSDAY:
            var dayOfWeek = new Date(nowDate.getFullYear(), nowDate.getMonth(), nowDate.getDate(), 0, 0, 0, 0).getDay();
            if (dayOfWeek == Consts.DAY_OF_WEEK.TUESDAY || dayOfWeek == Consts.DAY_OF_WEEK.WEDNESDAY || dayOfWeek == Consts.DAY_OF_WEEK.THURSDAY
                || dayOfWeek == Consts.DAY_OF_WEEK.FRIDAY || dayOfWeek == Consts.DAY_OF_WEEK.SATURDAY) {
                newStart = new Date(nowDate.getFullYear(), nowDate.getMonth(), nowDate.getDate() - 1, 0, 0, 0, 0);
                newEnd = new Date(nowDate.getFullYear(), nowDate.getMonth(), nowDate.getDate() - 1, 23, 59, 59, 0);
            }
            else if (dayOfWeek == Consts.DAY_OF_WEEK.SUNDAY) {
                newStart = new Date(nowDate.getFullYear(), nowDate.getMonth(), nowDate.getDate() - 2, 0, 0, 0, 0);
                newEnd = new Date(nowDate.getFullYear(), nowDate.getMonth(), nowDate.getDate() - 2, 23, 59, 59, 0);
            }
            else {
                newStart = new Date(nowDate.getFullYear(), nowDate.getMonth(), nowDate.getDate() - 3, 0, 0, 0, 0);
                newEnd = new Date(nowDate.getFullYear(), nowDate.getMonth(), nowDate.getDate() - 3, 23, 59, 59, 0);
            }
            break;
        case Consts.DATE_RANGE.LAST_X_DAYS:
            var lastXDaysDateTime = nowDate;
            if (nowDate.getDate() == 1) {
                lastXDaysDateTime = new Date(nowDate.getFullYear(), nowDate.getMonth(), nowDate.getDate() - 1, 0, 0, 0, 0);
            }
            newEnd = new Date(lastXDaysDateTime.getFullYear(), lastXDaysDateTime.getMonth(), nowDate.getDate() == 1 ? lastXDaysDateTime.getDate() : lastXDaysDateTime.getDate() - 1, 23, 59, 59, 0); ;
            this.Controls.LastXDays.Show();
            $(document.getElementById("LastXDaysSkin")).show();
            document.getElementById(this.getClientID("DateRange") + "_width").style.width = '134px';
            document.getElementById(this.getClientID("DateRange") + "_showDiv").style.width = '134px';
            $(this.Controls.LastXDays).focus();
            this.Controls.LastXDays.SetValue(this.Controls.LastXDays.Value == null || this.Controls.LastXDays.Value == "" ? 7 : this.Controls.LastXDays.Value * 1);
            newStart = new Date(nowDate.getFullYear(), nowDate.getMonth(), nowDate.getDate() - this.Controls.LastXDays.Value * 1, 0, 0, 0, 0);
            break;
        case Consts.DATE_RANGE.LAST_2_WEEKS:
            newStart = new Date(nowDate.getFullYear(), nowDate.getMonth(), nowDate.getDate() - nowDate.getDay() - 14, 0, 0, 0, 0);
            newEnd = new Date(nowDate.getFullYear(), nowDate.getMonth(), nowDate.getDate() - nowDate.getDay() - 1, 23, 59, 59, 0);
            break;
        case Consts.DATE_RANGE.ALL_DATES:
            newStart = new Date(1900, 0, 1, 0, 0, 0, 0);
            newEnd = new Date(nowDate.getFullYear(), nowDate.getMonth(), nowDate.getDate(), 23, 59, 59, 0);
            break;
        case Consts.DATE_RANGE.LAST_YEAR:
            newStart = new Date(nowDate.getFullYear() - 1, 0, 1, 0, 0, 0, 0);
            newEnd = new Date(nowDate.getFullYear() - 1, 11, 31, 23, 59, 59);
            break;
        case Consts.DATE_RANGE.MONTH_TO_DATE:
            newStart = new Date(nowDate.getFullYear(), nowDate.getMonth(), 1, 0, 0, 0, 0);
            newEnd = new Date(nowDate.getFullYear(), nowDate.getMonth(), nowDate.getDate(), 23, 59, 59, 0);
            break;
        case Consts.DATE_RANGE.YEAR_TO_DATE:
            newStart = new Date(nowDate.getFullYear(), 0, 1, 0, 0, 0, 0);
            newEnd = new Date(nowDate.getFullYear(), nowDate.getMonth(), nowDate.getDate(), 23, 59, 59, 0);
            break;
        case Consts.DATE_RANGE.NONE:
            /*startDate.IsEnabled = false;
            endDate.IsEnabled = false;
            if (startDate.SelectedDate.HasValue)
            {
            newStart = startDate.SelectedDate.Value;
            }
            else
            {
            newStart = nowDate;
            }
            newEnd = endDate.SelectedDate.HasValue ? endDate.SelectedDate.Value : new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);*/
            break;
        case Consts.DATE_RANGE.CUSTOM:
            this.Controls.DateFromControl.Calendar.datepicker('enable');
            this.Controls.DateToControl.Calendar.datepicker('enable');
            this.setTimeVisibility(true);
            var startDate = this.getValueByName("startDate");
            var endDate = this.getValueByName("endDate");

            this.fillTimeComboboxes(startDate, true);
            this.fillTimeComboboxes(endDate, false);
               
            if (startDate == null && endDate == null)
            {
                newStart = new Date(nowDate.getFullYear(), nowDate.getMonth(), nowDate.getDate() - nowDate.getDay() - 7, 0, 0, 0, 0);
                newEnd = new Date(nowDate.getFullYear(), nowDate.getMonth(), nowDate.getDate() - nowDate.getDay() - 2, 23, 59, 59, 0);
            }

            break;
        default:
            break;
    }
    /*
    if(DateRangeType == (int)DateRange.Custom)
    {
    startDate.SelectedDate = StartDate;
    endDate.SelectedDate = EndDate;
    }
    else
    {
    startDate.SelectedDate = newStart.Value;
    endDate.SelectedDate = newEnd;
    StartDate = newStart.Value;
    EndDate = newEnd;
    }


    //if (startDate.SelectedDate > endDate.SelectedDate) startDate.SelectedDate = startDate.DisplayDateEnd;
    //if (endDate.SelectedDate < startDate.SelectedDate) endDate.SelectedDate = endDate.DisplayDateStart;

    //startDate.DisplayDateEnd =  endDate.SelectedDate;
    //endDate.DisplayDateStart = startDate.SelectedDate;
*/
    this.Controls.DateFromControl.SetValue(returnShortDate(newStart));
    this.Controls.DateToControl.SetValue(returnShortDate(newEnd));

    if (value * 1 == Consts.DATE_RANGE.CUSTOM) {
        var min = this.Controls.DateFromControl.GetValue();
        $(this.Controls.DateToControl.Calendar).datepicker('option', { minDate: min });

        var max = this.Controls.DateToControl.GetValue();
        $(this.Controls.DateFromControl.Calendar).datepicker('option', { maxDate: max });
    }
};

DateRangeControl.prototype.fillHours = function () {
    this.HoursList = new Array();
    for (var i = 1; i <= 12; i++) {
        this.HoursList[this.HoursList.length] = { Name: i.toString(), Value: i };
        this.Controls.StartHours.AppendOption(i, i.toString());
        this.Controls.EndHours.AppendOption(i, i.toString());
    }
};

DateRangeControl.prototype.fillMinutes = function () {
    this.MinutesList = new Array();
    for (var i = 0; i <= 59; i++) {
        this.MinutesList[this.MinutesList.length] = { Name: i <= 9 ? "0" + i.toString() : i.toString(), Value: i };
        this.Controls.StartMinutes.AppendOption(i, i <= 9 ? "0" + i.toString() : i.toString());
        this.Controls.EndMinutes.AppendOption(i, i <= 9 ? "0" + i.toString() : i.toString());
    }
};

DateRangeControl.prototype.fillAMPM = function () {
    this.AMPMList = new Array();
    this.AMPMList[this.AMPMList.length] = { Name: "AM", Value: "AM" };
    this.AMPMList[this.AMPMList.length] = { Name: "PM", Value: "PM" };
    this.Controls.StartAMPM.AppendOption("AM", "AM");
    this.Controls.StartAMPM.AppendOption("PM", "PM");
    this.Controls.EndAMPM.AppendOption("AM", "AM");
    this.Controls.EndAMPM.AppendOption("PM", "PM");
};

DateRangeControl.prototype.fillTimeComboboxes = function(timeString, isStartDate) {
    if(timeString == null || timeString.toString() == '') return;
    var s1 = timeString.split(' ');
    var s2 = s1[1].split(':');

    if (isStartDate) {
        this.Controls.StartHours.SetDropDownValue(s2[0]*1);
        this.Controls.StartMinutes.SetDropDownValue(s2[1]*1);
        this.Controls.StartAMPM.SetDropDownValue(s1[2]);
    }
    else {
        this.Controls.EndHours.SetDropDownValue(s2[0] * 1);
        this.Controls.EndMinutes.SetDropDownValue(s2[1] * 1);
        this.Controls.EndAMPM.SetDropDownValue(s1[2]);
    }
};

DateRangeControl.prototype.setTimeVisibility = function (isVisible) {
    if (isVisible) {
        $(".dateRangeTimes").css('display', "none");
    }
    else {
        $(".dateRangeTimes").css('display', "none");
    }
};

DateRangeControl.prototype.fillDataRanges = function () {
    var defaultValue = null;
    this.DateRanges = new Array();
    switch (this.ReportID)
    {
        case Consts.REPORT_TYPE.ComparisonReport:
            this.DateRanges[this.DateRanges.length] = { Name : Consts.DATE_RANGE_CAPTIONS.ITEM_LAST_MONTH, Value : Consts.DATE_RANGE.LAST_MONTH};
            this.DateRanges[this.DateRanges.length] = { Name : Consts.DATE_RANGE_CAPTIONS.ITEM_LAST_QUARTER, Value : Consts.DATE_RANGE.LAST_QUARTER };
            this.DateRanges[this.DateRanges.length] = { Name : Consts.DATE_RANGE_CAPTIONS.ITEM_LAST_YEAR, Value :Consts.DATE_RANGE.LAST_YEAR};
            defaultValue = { Name : Consts.DATE_RANGE_CAPTIONS.ITEM_YEAR_TO_DATE, Value : Consts.DATE_RANGE.YEAR_TO_DATE };
            this.DateRanges[this.DateRanges.length] = defaultValue;
            break;
        default:
            this.DateRanges[this.DateRanges.length] = { Name : Consts.DATE_RANGE_CAPTIONS.ITEM_CUSTOM , Value : Consts.DATE_RANGE.CUSTOM };
            this.DateRanges[this.DateRanges.length] = { Name : Consts.DATE_RANGE_CAPTIONS.ITEM_YESTERDAY, Value : Consts.DATE_RANGE.YESTERDAY};
            this.DateRanges[this.DateRanges.length] = { Name : Consts.DATE_RANGE_CAPTIONS.ITEM_PREVIOUS_BUSINNES_DAY, Value : Consts.DATE_RANGE.PREVIOUS_BUSINESSDAY};
            this.DateRanges[this.DateRanges.length] = { Name : Consts.DATE_RANGE_CAPTIONS.ITEM_LAST_WEEK_MONDAY_TO_SUNDAY, Value : Consts.DATE_RANGE.LAST_WEEK};
            defaultValue = { Name : Consts.DATE_RANGE_CAPTIONS.ITEM_LAST_CALENDAR_WEEK, Value : Consts.DATE_RANGE.LAST_CALENDAR_WEEK};
            this.DateRanges[this.DateRanges.length] =  defaultValue;
            this.DateRanges[this.DateRanges.length] = { Name : Consts.DATE_RANGE_CAPTIONS.ITEM_LAST_BUSINESS_WEEK, Value : Consts.DATE_RANGE.LAST_BUSINESS_WEEK};
            this.DateRanges[this.DateRanges.length] = { Name : Consts.DATE_RANGE_CAPTIONS.ITEM_LAST_X_DAYS, Value : Consts.DATE_RANGE.LAST_X_DAYS};
            this.DateRanges[this.DateRanges.length] = { Name : Consts.DATE_RANGE_CAPTIONS.ITEM_LAST_2_WEEKS, Value : Consts.DATE_RANGE.LAST_2_WEEKS};
            this.DateRanges[this.DateRanges.length] = { Name : Consts.DATE_RANGE_CAPTIONS.ITEM_LAST_MONTH, Value : Consts.DATE_RANGE.LAST_MONTH};
            this.DateRanges[this.DateRanges.length] = { Name : Consts.DATE_RANGE_CAPTIONS.ITEM_LAST_QUARTER, Value : Consts.DATE_RANGE.LAST_QUARTER};
            this.DateRanges[this.DateRanges.length] = { Name : Consts.DATE_RANGE_CAPTIONS.ITEM_LAST_YEAR, Value : Consts.DATE_RANGE.LAST_YEAR};
            this.DateRanges[this.DateRanges.length] = { Name: Consts.DATE_RANGE_CAPTIONS.ITEM_MONTH_TO_DATE, Value: Consts.DATE_RANGE.MONTH_TO_DATE };
            this.DateRanges[this.DateRanges.length] = { Name : Consts.DATE_RANGE_CAPTIONS.ITEM_YEAR_TO_DATE, Value : Consts.DATE_RANGE.YEAR_TO_DATE};
            this.DateRanges[this.DateRanges.length] = { Name : Consts.DATE_RANGE_CAPTIONS.ITEM_ALL_DATES, Value : Consts.DATE_RANGE.ALL_DATES};
        break;
    }
    
    for (var index in this.DateRanges) {
        this.Controls.DataRangeDropDown.AppendOption(this.DateRanges[index].Value, this.DateRanges[index].Name);
    }
    
    this.Controls.DataRangeDropDown.SetDropDownValue(defaultValue.Value);
};

DateRangeControl.prototype.getDataRangeValue = function () {
    return this.Controls.DataRangeDropDown.GetOptionValue(this.Controls.DataRangeDropDown.Value);
};

DateRangeControl.prototype.getStartDateValue = function () {
    var returnValue = this.Controls.DateFromControl.Value + " 00:00 AM" ;
    if (this.Controls.DataRangeDropDown.GetOptionValue(this.Controls.DataRangeDropDown.Value) * 1 == Consts.DATE_RANGE.CUSTOM) {
        returnValue = this.Controls.DateFromControl.Value + " " + 
            (this.Controls.StartHours.Value * 1 <= 9 ? "0" + this.Controls.StartHours.Value : this.Controls.StartHours.Value) + ":" +
            (this.Controls.StartMinutes.Value) + " " + this.Controls.StartAMPM.Value;
    }
    return returnValue;
};

DateRangeControl.prototype.getEndDateValue = function() {
    var returnValue = this.Controls.DateToControl.Value + " 23:59 PM";
    if (this.Controls.DataRangeDropDown.GetOptionValue(this.Controls.DataRangeDropDown.Value) * 1 == Consts.DATE_RANGE.CUSTOM) {
        returnValue = this.Controls.DateToControl.Value + " " +
            (this.Controls.EndHours.Value * 1 <= 9 ? "0" + this.Controls.EndHours.Value : this.Controls.EndHours.Value) + ":" +
            (this.Controls.EndMinutes.Value) + " " + this.Controls.EndAMPM.Value;
    }
    return returnValue;
};

DateRangeControl.prototype.getEndDateValueToValidation = function () {
    var returnValue = this.Controls.DateToControl.Value + " 11:59 PM";
    if (this.Controls.DataRangeDropDown.GetOptionValue(this.Controls.DataRangeDropDown.Value) * 1 == Consts.DATE_RANGE.CUSTOM) {
        returnValue = this.Controls.DateToControl.Value + " " +
            (this.Controls.EndHours.Value * 1 <= 9 ? "0" + this.Controls.EndHours.Value : this.Controls.EndHours.Value) + ":" +
            (this.Controls.EndMinutes.Value) + " " + this.Controls.EndAMPM.Value;
    }
    return returnValue;
};

DateRangeControl.prototype.getLastXDaysValue = function () {
    var value = this.Controls.LastXDays.Value;
    return value == null || value.toString() == "" ? 0 : value;
};

DateRangeControl.prototype.getClientID = function (name) {
    return this.Element.id + '_' + name;
};