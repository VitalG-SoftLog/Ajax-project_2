function ShedulerControl(elementId, parentObject) {
    this.Element = document.getElementById(elementId);
    this.ReportId = -1;
    this.ParentObject = parentObject;
    this.UserReportId = -1;
    this.Controls = new Array();
    this.Data = null;
    this.SheduleTabs = new Array();
    this.CurrentTab = 1;
    this.SheduleId = -1;
    this.DropDownData = ['First Week', 'Second Week', 'Third Week', 'Fourth Week', 'Last Week'];
    this.RecipientsPopIn = null;
    this.TabsView = new Array();
};

ShedulerControl.prototype.Bind = function (reportId, userReportId, sheduleId) {
    this.Element.innerHTML = '';

    var params = {
        action: Consts.HANDLER_ACTIONS.GET_SCHEDULES,
        userReportID: userReportId,
        reportId: reportId,
        sheduleId: sheduleId
    };

    this.ReportId = reportId;
    this.UserReportId = userReportId;
    this.SheduleId = sheduleId;

    var parentObj = this;
    $.blockUI({ message: "Please wait loading...", css: { marginLeft: '35%', marginRight: '35%', width: '30%', top: '45%'} });
    this.ParentObject.sendAjax(Consts.HANDLERS.SEARCH_HANDLER, { type: 'POST', params: JSON.stringify(params) }, function (data) {
        parentObj.successfullGetSchedules.call(parentObj, data);
    }, function (e) { parentObj.ParentObject.sendErrorCallback.call(parentObj, e); });
    

};

ShedulerControl.prototype.successfullGetSchedules = function (data) {
    $.unblockUI();
    var scheduleAttr = {
        'AddSchedulePopUp': this.getClientID('AddSchedulePopUp'),
        'AddSchedulePopUpContainer': this.getClientID('AddSchedulePopUpContainer'),
        'idDeliveryMethodRadio': this.getClientID('idDeliveryMethodRadio')
    };

    var schedulePopUp = TemplateEngine.Format(TemplateManager.templates['AddSchedule_AddSchedulePopUp'], scheduleAttr);
    $.blockUI({ message: schedulePopUp, css: { marginLeft: '22%', marginRight: '20%', width: '924px',  top: '10%'} });
    this.Element = document.getElementById(this.getClientID('AddSchedulePopUpContainer'));
    this.Controls.DeliveryMethodRadio = new DeliveryMethodRadioButton(this.getClientID('idDeliveryMethodRadio'), TemplateManager.templates['AddSchedule_DeliveryMethodRadio']);
    this.Controls.DeliveryMethodRadio.Bind();
    this.Controls.DeliveryMethodRadio.SetValue(1);

    this.Controls.ScheduleStopCheckBoxControl = new newCheckBoxControl(this, "ScheduleStopCheckBox", "", 1);
    this.Controls.ScheduleStopCheckBoxControl.DataBind();

    this.Controls.WithoutDataCheckBoxControl = new newCheckBoxControl(this, "WithoutDataCheckBox", "", 1);
    this.Controls.WithoutDataCheckBoxControl.DataBind();

    var calendarScheduleStart = {
        "idScheduleCalendarInput": this.getClientID("ScheduleStartCalendar"),
        "idScheduleCalendarContainer": "ScheduleStartContainer"
    };
    var calendarScheduleStartTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_ScheduleCalendarInput'], calendarScheduleStart);
    $(document.getElementById("ScheduleStartCalendar")).append(calendarScheduleStartTempl);

    this.Controls.calendarScheduleStartControl = new CalendarControl(this, calendarScheduleStart["idScheduleCalendarInput"], "", "", ValueType.Date, {});

    var calendarScheduleStop = {
        "idScheduleCalendarInput": this.getClientID("ScheduleStopCalendar"),
        "idScheduleCalendarContainer": "ScheduleStopCalendarContainer"
    };
    var calendarScheduleStopTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_ScheduleCalendarInput'], calendarScheduleStop);
    $(document.getElementById("ScheduleStopCalendar")).append(calendarScheduleStopTempl);

    this.Controls.calendarScheduleStopControl = new CalendarControl(this, calendarScheduleStop["idScheduleCalendarInput"], "", "", ValueType.Date, {});

    var calendarScheduleTime = {
        "idScheduleDateTimeInput": this.getClientID("ScheduleTimeCalendar"),
        "idScheduleDateTimeContainer": "ScheduleDateTimeContainer"
    };
    var calendarScheduleTimeTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_ScheduleDateTimeInput'], calendarScheduleTime);
    $(document.getElementById("ScheduleTimeCalendar")).append(calendarScheduleTimeTempl);

    this.Controls.calendarScheduleTimeControl = new DateTimeControl(this, calendarScheduleTime["idScheduleDateTimeInput"], "", "", ValueType.Date, {});

    var parentObj = this;

    $('div[reportSheduleTabs="1"]', this.Element).each(function () {
        if (parentObj.SheduleTabs.length == 0) {
            parentObj.SheduleTabs[parentObj.SheduleTabs.length] = this;
        }

        $(this).bind('click', function () {
            var tabId = this.getAttribute('sheduleTab');

            parentObj.BindTab(tabId);
        });
    });

    this.BindTab(this.CurrentTab);
    
};

ShedulerControl.prototype.BindTab = function (tabId) {
    this.CurrentTab = tabId;

    this.InactiveTabs();

    var tabElement = $('div[sheduleTab="' + tabId + '"]', this.Element)[0];

    if (tabElement) {
        $('td[corner="1"]', tabElement)[0].className = 'leftTabActive';
        $('td[corner="2"]', tabElement)[0].className = 'centerTabActive';
        $('td[corner="3"]', tabElement)[0].className = 'rightTabActive';

    }

    var tabTemplate = '';

    var onceTabSchedule = document.getElementById('onceTabShedule');
    var dailyTabShedule = document.getElementById('dailyTabShedule');
    var weeklyTabSchedule = document.getElementById('weeklyTabSchedule');
    var monthlyTabSchedule = document.getElementById('monthlyTabSchedule');
    var quarterlyTabSchedule = document.getElementById('quarterlyTabSchedule');
    var yearlyTabSchedule = document.getElementById('yearlyTabSchedule');

    onceTabSchedule.style.display = 'none';
    dailyTabShedule.style.display = 'none';
    weeklyTabSchedule.style.display = 'none';
    monthlyTabSchedule.style.display = 'none';
    quarterlyTabSchedule.style.display = 'none';
    yearlyTabSchedule.style.display = 'none';

    switch (tabId * 1) {
        case Consts.SHEDULE_TAB.ONCE:


            if (this.TabsView[tabId] == null) {
                tabTemplate += this.get_OnceTemplate();
                onceTabSchedule.innerHTML = tabTemplate;
                this.AfterBind(tabId, false);
            }

            onceTabSchedule.style.display = '';

            break;
        case Consts.SHEDULE_TAB.DAILY:
            if (this.TabsView[tabId] == null) {
                tabTemplate += this.get_DailyTemplate();
                dailyTabShedule.innerHTML = tabTemplate;
                this.AfterBind(tabId, false);
            }

            dailyTabShedule.style.display = '';
            break;
        case Consts.SHEDULE_TAB.WEEKLY:
            if (this.TabsView[tabId] == null) {
                tabTemplate += this.get_WeeklyTemplate();
                weeklyTabSchedule.innerHTML = tabTemplate;
                this.AfterBind(tabId, false);
            }

            weeklyTabSchedule.style.display = '';
            break;
        case Consts.SHEDULE_TAB.MONTHLY:
            if (this.TabsView[tabId] == null) {
                tabTemplate += this.get_MonthlyTemplate();
                monthlyTabSchedule.innerHTML = tabTemplate;
                this.AfterBind(tabId, false);
            }

            monthlyTabSchedule.style.display = '';
            break;
        case Consts.SHEDULE_TAB.QUARTERLY:
            if (this.TabsView[tabId] == null) {
                tabTemplate += this.get_QuarterlyTemplate();
                quarterlyTabSchedule.innerHTML = tabTemplate;
                this.AfterBind(tabId, false);

                this.Controls.MultiRadioQuarterly = new MultiRadioButton("quarterlyOneDayOfWeekControl", TemplateManager.templates['AddSchedule_multiRadioQuarterly']);
                this.Controls.MultiRadioQuarterly.Bind();
                //this.Controls.MultiRadioQuarterly.SetValue(1);
            }

            quarterlyTabSchedule.style.display = '';
            break;
        case Consts.SHEDULE_TAB.YEARLY:
            if (this.TabsView[tabId] == null) {
                tabTemplate += this.get_YearlyTemplate();
                yearlyTabSchedule.innerHTML = tabTemplate;
                this.AfterBind(tabId, false);

                this.Controls.MultiRadioYearly = new MultiRadioButton("yearlyMonthsRadioControl", TemplateManager.templates['AddSchedule_yearlyMultiRadioTemplate']);
                this.Controls.MultiRadioYearly.Bind();
                this.Controls.MultiRadioYearly.SetValue(1);
            }

            yearlyTabSchedule.style.display = '';
            break;
    }

    if (this.TabsView[tabId] == null) {
        this.TabsView[tabId] = tabTemplate;
    }

    var parentObj = this;

    var cancel = document.getElementById('SheduleCancelBttn');
    $(cancel).bind('click', function () {
        parentObj.Cancel();
    });

    var save = document.getElementById('SaveScheduleBttn');
    $(save).bind('click', function () {
        parentObj.SaveSchedule();
    });

    var addRecipientsSchedule = document.getElementById('addRecipientsSchedule');
    $(addRecipientsSchedule).bind('click', function () {
        parentObj.AddRecipients();
    });
};

ShedulerControl.prototype.AddRecipients = function () {
    this.RecipientsPopIn = new ScheduleRecipientsFilter(this, 'recipientsSchedulePopIn');
    this.RecipientsPopIn.Bind();    
};

ShedulerControl.prototype.SaveSchedule = function () {
    alert('implementing');
};

ShedulerControl.prototype.Cancel = function () {
    $.unblockUI();
    this.Element.innerHTML = '';
    this.TabsView = null;
    this.TabsView = new Array();
    this.CurrentTab = 1;
    this.SheduleId = -1;
};

ShedulerControl.prototype.AfterBind = function (tabId, isExist) {
    var parentObj = this;

    switch (tabId * 1) {
        case Consts.SHEDULE_TAB.ONCE:
            break;
        case Consts.SHEDULE_TAB.DAILY:
            this.Controls.DailyOneDayOfWeekRadio = document.getElementById('dailyOneDayOfWeekRadio');
            this.Controls.DailyEveryWeekRadio = document.getElementById('dailyEveryWeekRadio');
            this.Controls.DailyRepeaterAfterRadio = document.getElementById('dailyRepeaterAfterRadio');

            var dailyRepeaterAfterControl = document.getElementById('dailyRepeaterAfterControl');
            var repeatAfterDailyTextBox = document.getElementById('repeatAfterDailyTextBox');
            var dailyOneDayOfWeekRadioControl = document.getElementById('dailyOneDayOfWeekRadioControl');

            $(this.Controls.DailyOneDayOfWeekRadio).bind('click', function () {
                if (this.className == 'radioInactive') {
                    this.className = 'radioActive';
                    parentObj.Controls.DailyEveryWeekRadio.className = 'radioInactive';
                    parentObj.Controls.DailyRepeaterAfterRadio.className = 'radioInactive';
                    dailyRepeaterAfterControl.style.opacity = '0.7';
                    dailyOneDayOfWeekRadioControl.style.opacity = '1';
                    repeatAfterDailyTextBox.disabled = true;

                    $('div[checkbox="1"]', dailyOneDayOfWeekRadioControl).each(function () {
                        this.disabled = false;
                    });
                }
            });

            $(this.Controls.DailyEveryWeekRadio).bind('click', function () {
                if (this.className == 'radioInactive') {
                    this.className = 'radioActive';
                    parentObj.Controls.DailyOneDayOfWeekRadio.className = 'radioInactive';
                    parentObj.Controls.DailyRepeaterAfterRadio.className = 'radioInactive';
                    dailyRepeaterAfterControl.style.opacity = '0.7';
                    dailyOneDayOfWeekRadioControl.style.opacity = '0.7';
                    repeatAfterDailyTextBox.disabled = true;

                    $('div[checkbox="1"]', dailyOneDayOfWeekRadioControl).each(function () {
                        this.disabled = true;
                        this.className = 'checkboxUnchecked';
                    });
                }
            });

            $(this.Controls.DailyRepeaterAfterRadio).bind('click', function () {
                if (this.className == 'radioInactive') {
                    this.className = 'radioActive';
                    parentObj.Controls.DailyOneDayOfWeekRadio.className = 'radioInactive';
                    parentObj.Controls.DailyEveryWeekRadio.className = 'radioInactive';
                    dailyOneDayOfWeekRadioControl.style.opacity = '0.7';
                    dailyRepeaterAfterControl.style.opacity = '1';
                    repeatAfterDailyTextBox.disabled = false;

                    $('div[checkbox="1"]', dailyOneDayOfWeekRadioControl).each(function () {
                        this.disabled = true;
                        this.className = 'checkboxUnchecked';
                    });
                }
            });

            $('div[checkbox="1"]', dailyOneDayOfWeekRadioControl).each(function () {
                $(this).bind('click', function () {
                    if (this.className == 'checkboxUnchecked') {
                        this.className = 'checkboxChecked';
                    }
                    else {
                        this.className = 'checkboxUnchecked';
                    }
                });
            });

            if (!isExist) {
                this.Controls.DailyOneDayOfWeekRadio.className = 'radioActive';
                dailyRepeaterAfterControl.style.opacity = '0.7';
                repeatAfterDailyTextBox.disabled = true;
            }
            break;
        case Consts.SHEDULE_TAB.WEEKLY:
            var weeklyOneDayOfWeekRadioControl = document.getElementById('weeklyOneDayOfWeekRadioControl');

            $('div[checkbox="1"]', weeklyOneDayOfWeekRadioControl).each(function () {
                $(this).bind('click', function () {
                    if (this.className == 'checkboxUnchecked') {
                        this.className = 'checkboxChecked';
                    }
                    else {
                        this.className = 'checkboxUnchecked';
                    }
                });
            });

            break;
        case Consts.SHEDULE_TAB.MONTHLY:
            if (!isExist) {
                var monthlyComboBoxAttr = {
                    "idDropDown": this.getClientID("monthlyOneWeekDropDown"),
                    "dropDownHide": "dropDownHide"
                };

                var monthlyOneWeekComboBoxTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_DropDownControl'], monthlyComboBoxAttr);

                $(document.getElementById('monthlyOneWeekDropDownControl')).append(monthlyOneWeekComboBoxTempl);
                this.Controls.MonthlyOneWeekDropDown = new newDropDownControl(this, monthlyComboBoxAttr["idDropDown"], "", "", ValueType.String, null, false, 100);
                this.FillCombobox(this.Controls.MonthlyOneWeekDropDown);
            }

            this.Controls.MonthlyOneDayRadio = document.getElementById('monthlyOneDayRadio');
            this.Controls.MonthlyOneWeekRadio = document.getElementById('monthlyOneWeekRadio');

            var monthlyOneCalendarDayTextBox = document.getElementById('monthlyOneCalendarDayTextBox');
            var monthlyOneDayOfWeekControl = document.getElementById('monthlyOneDayOfWeekControl');

            $(this.Controls.MonthlyOneDayRadio).bind('click', function () {
                if (this.className == 'radioInactive') {
                    this.className = 'radioActive';
                }
                parentObj.Controls.MonthlyOneWeekRadio.className = 'radioInactive';
                monthlyOneCalendarDayTextBox.style.opacity = '1';
                monthlyOneCalendarDayTextBox.disabled = false;
                parentObj.Controls.MonthlyOneWeekDropDown.Element.style.opacity = '0.7';
                parentObj.Controls.MonthlyOneWeekDropDown.Element.disabled = true;
                monthlyOneDayOfWeekControl.style.opacity = '0.7';

                $('div[checkbox="1"]', monthlyOneDayOfWeekControl).each(function () {
                    this.disabled = true;
                    this.className = 'checkboxUnchecked';
                });

            });

            $(this.Controls.MonthlyOneWeekRadio).bind('click', function () {
                if (this.className == 'radioInactive') {
                    this.className = 'radioActive';
                }
                parentObj.Controls.MonthlyOneDayRadio.className = 'radioInactive';
                monthlyOneCalendarDayTextBox.style.opacity = '0.7';
                monthlyOneCalendarDayTextBox.disabled = true;
                parentObj.Controls.MonthlyOneWeekDropDown.Element.style.opacity = '1';
                parentObj.Controls.MonthlyOneWeekDropDown.Element.disabled = false;
                monthlyOneDayOfWeekControl.style.opacity = '1';

                $('div[checkbox="1"]', monthlyOneDayOfWeekControl).each(function () {
                    this.disabled = false;
                });

            });

            $('div[checkbox="1"]', monthlyOneDayOfWeekControl).each(function () {
                $(this).bind('click', function () {
                    if (this.className == 'checkboxUnchecked') {
                        this.className = 'checkboxChecked';
                    }
                    else {
                        this.className = 'checkboxUnchecked';
                    }
                });
            });

            if (!isExist) {
                this.Controls.MonthlyOneDayRadio.className = 'radioActive';
                monthlyOneDayOfWeekControl.style.opacity = '0.7';
                this.Controls.MonthlyOneWeekDropDown.Element.style.opacity = '0.7';
                this.Controls.MonthlyOneWeekDropDown.Element.disabled = true;
            }

            $('div[checkbox="1"]', monthlyOneDayOfWeekControl).each(function () {
                this.disabled = true;
                this.className = 'checkboxUnchecked';
            });

            var monthlyMontsControl = document.getElementById('montlyMonthsControl');

            $('div[checkbox="1"]', monthlyMontsControl).each(function () {
                $(this).bind('click', function () {
                    if (this.className == 'checkboxUnchecked') {
                        this.className = 'checkboxChecked';
                    }
                    else {
                        this.className = 'checkboxUnchecked';
                    }
                });
            });

            break;
        case Consts.SHEDULE_TAB.QUARTERLY:
            if (!isExist) {
                var quarterlyComboBoxAttr = {
                    "idDropDown": this.getClientID("quarterlyOneWeekDropDown"),
                    "dropDownHide": "dropDownHide"
                };

                var quarterlyOneWeekComboBoxTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_DropDownControl'], quarterlyComboBoxAttr);

                $(document.getElementById('quarterlyOneWeekDropDownControl')).append(quarterlyOneWeekComboBoxTempl);
                this.Controls.QuarterlyOneWeekDropDown = new newDropDownControl(this, quarterlyComboBoxAttr["idDropDown"], "", "", ValueType.String, null, false, 100);

                this.FillCombobox(this.Controls.QuarterlyOneWeekDropDown);

                this.Controls.QuarterlyOneDayRadio = document.getElementById('quarterlyOneDayRadio');
                this.Controls.QuarterlyOneWeekRadio = document.getElementById('quarterlyOneWeekRadio');

                var quarterlyOneDayOfWeekControl = document.getElementById('quarterlyOneDayOfWeekControl');
                var quarterlyOneDaysTextBox = document.getElementById('quarterlyOneDaysTextBox');

                $(this.Controls.QuarterlyOneDayRadio).bind('click', function () {
                    if (this.className == 'radioInactive') {
                        this.className = 'radioActive';

                        parentObj.Controls.QuarterlyOneWeekRadio.className = 'radioInactive';
                        quarterlyOneDaysTextBox.style.opacity = '1';
                        quarterlyOneDaysTextBox.disabled = false;
                        parentObj.Controls.QuarterlyOneWeekDropDown.Element.disabled = true;
                        parentObj.Controls.QuarterlyOneWeekDropDown.Element.style.opacity = '0.7';
                        quarterlyOneDayOfWeekControl.style.opacity = '0.7';
                    }

                    $('div[isRadio=""]', quarterlyOneDayOfWeekControl).each(function () {
                        this.disabled = true;
                        this.className = 'radioInactive';
                    });

                });

                $(this.Controls.QuarterlyOneWeekRadio).bind('click', function () {
                    if (this.className == 'radioInactive') {
                        this.className = 'radioActive';

                        parentObj.Controls.QuarterlyOneDayRadio.className = 'radioInactive';
                        quarterlyOneDaysTextBox.style.opacity = '0.7';
                        quarterlyOneDaysTextBox.disabled = true;
                        parentObj.Controls.QuarterlyOneWeekDropDown.Element.disabled = false;
                        parentObj.Controls.QuarterlyOneWeekDropDown.Element.style.opacity = '1';
                        quarterlyOneDayOfWeekControl.style.opacity = '1';
                    }

                    $('div[isRadio=""]', quarterlyOneDayOfWeekControl).each(function () {
                        this.disabled = false;
                    });

                    parentObj.Controls.MultiRadioQuarterly.SetValue(1);
                });

                this.Controls.QuarterlyOneDayRadio.className = 'radioActive';
                this.Controls.QuarterlyOneWeekDropDown.Element.disabled = true;
                this.Controls.QuarterlyOneWeekDropDown.Element.style.opacity = '0.7';
                quarterlyOneDayOfWeekControl.style.opacity = '0.7';
            }
            break;
        case Consts.SHEDULE_TAB.YEARLY:
            if (!isExist) {
                var yearlyComboBoxAttr = {
                    "idDropDown": this.getClientID("yearlyOneWeekDropDown"),
                    "dropDownHide": "dropDownHide"
                };

                var yearlyOneWeekComboBoxTempl = TemplateEngine.Format(TemplateManager.templates['ReportBuilderForm_DropDownControl'], yearlyComboBoxAttr);

                $(document.getElementById('yearlyOneWeekDropDownControl')).append(yearlyOneWeekComboBoxTempl);
                this.Controls.YearlyOneWeekDropDown = new newDropDownControl(this, yearlyComboBoxAttr["idDropDown"], "", "", ValueType.String, null, false, 100);
                this.FillCombobox(this.Controls.YearlyOneWeekDropDown);

                this.Controls.YearlyOneDayRadio = document.getElementById('yearlyOneDayRadio');
                this.Controls.YearlyOneWeekRadio = document.getElementById('yearlyOneWeekRadio');

                var yearlyMonthsRadioControl = document.getElementById('yearlyMonthsRadioControl');
                var yearlyOneDaysTextBox = document.getElementById('yearlyOneDayTextBox');

                $(this.Controls.YearlyOneDayRadio).bind('click', function () {
                    if (this.className == 'radioInactive') {
                        this.className = 'radioActive';

                        parentObj.Controls.YearlyOneWeekRadio.className = 'radioInactive';
                        yearlyOneDaysTextBox.style.opacity = '1';
                        yearlyOneDaysTextBox.disabled = false;
                        parentObj.Controls.YearlyOneWeekDropDown.Element.disabled = true;
                        parentObj.Controls.YearlyOneWeekDropDown.Element.style.opacity = '0.7';
                        //yearlyMonthsRadioControl.style.opacity = '0.7';
                    }

                });

                $(this.Controls.YearlyOneWeekRadio).bind('click', function () {
                    if (this.className == 'radioInactive') {
                        this.className = 'radioActive';

                        parentObj.Controls.YearlyOneDayRadio.className = 'radioInactive';
                        yearlyOneDaysTextBox.style.opacity = '0.7';
                        yearlyOneDaysTextBox.disabled = true;
                        parentObj.Controls.YearlyOneWeekDropDown.Element.disabled = false;
                        parentObj.Controls.YearlyOneWeekDropDown.Element.style.opacity = '1';
                        //yearlyMonthsRadioControl.style.opacity = '1';
                    }

                    $('div[isRadio=""]', yearlyMonthsRadioControl).each(function () {
                        this.disabled = false;
                    });

                    
                });

                this.Controls.YearlyOneDayRadio.className = 'radioActive';
                this.Controls.YearlyOneWeekDropDown.Element.disabled = true;
                this.Controls.YearlyOneWeekDropDown.Element.style.opacity = '0.7';
                //yearlyMonthsRadioControl.style.opacity = '0.7';
            }
            break;
    }
};

ShedulerControl.prototype.FillCombobox = function (combobox) {
    for (var i in this.DropDownData) {
        combobox.AppendOption(this.DropDownData[i], this.DropDownData[i]);
    }

    combobox.SetDropDownValue('First Week');
};

ShedulerControl.prototype.get_OnceTemplate = function () {
    var template = '';

    template += TemplateEngine.Format(TemplateManager.templates['AddSchedule_OnceSheduleTemplate'], {});

    return template;
};

ShedulerControl.prototype.get_DailyTemplate = function () {
    var template = '';

    template += TemplateEngine.Format(TemplateManager.templates['AddSchedule_DailyScheduleTemplate'], {});

    return template;
};

ShedulerControl.prototype.get_WeeklyTemplate = function () {
    var template = '';

    template += TemplateEngine.Format(TemplateManager.templates['AddSchedule_WeeklyScheduleTemplate'], {});

    return template;
};

ShedulerControl.prototype.get_MonthlyTemplate = function () {
    var template = '';

    template += TemplateEngine.Format(TemplateManager.templates['AddSchedule_MonthlyScheduleTemplate'], {});

    return template;
};

ShedulerControl.prototype.get_QuarterlyTemplate = function () {
    var template = '';

    template += TemplateEngine.Format(TemplateManager.templates['AddSchedule_QuarterlyScheduleTemplate'], {});

    return template;
};

ShedulerControl.prototype.get_YearlyTemplate = function () {
    var template = '';

    template += TemplateEngine.Format(TemplateManager.templates['AddSchedule_YearlyScheduleTemplate'], {});

    return template;
};

ShedulerControl.prototype.InactiveTabs = function(name) {
    $('div[reportSheduleTabs="1"]', this.Element).each(function () {
        $('td[corner="1"]', this)[0].className = 'leftTabInActive';
        $('td[corner="2"]', this)[0].className = 'centerTabInActive';
        $('td[corner="3"]', this)[0].className = 'rightTabInActive';

    });
};

ShedulerControl.prototype.getClientID = function (name) {
    return this.Element.id + '_' + name;
};