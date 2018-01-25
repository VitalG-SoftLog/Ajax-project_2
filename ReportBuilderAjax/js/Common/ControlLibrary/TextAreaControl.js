var TextAreaControl = function(_parentObject, _id, _value, _defaultValue, _valueType, _dependentControls) {
    this.Init(_parentObject, _id, _value, _defaultValue, _valueType, _dependentControls);
    this.Type = ControlType.TextArea;
};

//Inheritance TextAreaControl from ControlAbstractClass

ClassInheritance.Standart(TextAreaControl, ControlAbstractClass);

//Entending end overwriting class prototype members

TextAreaControl.prototype.CreateControl = function() {
    this.Element = document.createElement("textarea");
    this.Element.setAttribute("id", this.Id);
}