function ScheduleRecipientsFilter(parentObject, elementId) {
    this.ParentObject = parentObject;
    this.Element = document.getElementById(elementId);
    this.IsBind = false;
};

ScheduleRecipientsFilter.prototype.Bind = function() {
    if (!this.IsBind) {
        this.Element.innerHTML = TemplateEngine.Format(TemplateManager.templates['AddSchedule_scheduleFilterPopInTemplate'], {});
    }
};

