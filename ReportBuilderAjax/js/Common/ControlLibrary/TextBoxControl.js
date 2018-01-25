var TextBoxControl = function(_parentObject, _id, _value, _defaultValue, _valueType, _dependentControls) {
    this.Init(_parentObject, _id, _value, _defaultValue, _valueType, _dependentControls);
    
   /* if (_valueType == ValueType.Money) {
        $(this.Element).blur(function () {
            $(this).formatCurrency({
                decimalSymbol: '.',
                digitGroupSymbol: ',',
                groupDigits: true,
                symbol: ''
            });
        });
    }*/
};

//Inheritance TextBoxControl from ControlAbstractClass

ClassInheritance.Standart(TextBoxControl, ControlAbstractClass);

//Entending end overwriting class prototype members