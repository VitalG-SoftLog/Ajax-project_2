var EventFunctionSetClass = function() {
    this.List = {};
    this.Event = null;
};

EventFunctionSetClass.prototype.Append = function(name, func) {
    this.List[name] = func;
};

EventFunctionSetClass.prototype.Clear = function() {
    this.List = {};
};

EventFunctionSetClass.prototype.Remove = function(name) {
    var resultList = {};
    for (funcName in this.List) {
        if (funcName != name) {
            resultList[funcName] = this.List[funcName];
        }
    }
    this.List = resultList;
};

EventFunctionSetClass.prototype.Invoke = function(name, scope) {
    if (this.List[name] && typeof this.List[name] == "function") {
        this.List[name].call(scope, this.Event);
    }
};

EventFunctionSetClass.prototype.InvokeAll = function(scope) {
    for (name in this.List) {
        this.List[name].call(scope, this.Event);
    }
};