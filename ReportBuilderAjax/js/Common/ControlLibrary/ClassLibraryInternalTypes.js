if (typeof ControlState == "undefined") {

    var ControlState = {
        NotSet: "NotSet",
        Invalid: "Valid",
        Valid: "Valid",
        Invalid: "Invalid",        
        Changed: "Changed"
    };

}

if (typeof ControlType == "undefined") {

    var ControlType = {
        NotSet: "NotSet",
        TextBox: "TextBox",
        DropDown: "DropDown",
        CheckBox: "CheckBox",
        CheckBoxSet: "CheckBoxSet",
        TextArea: "TextArea",
        Radio: "Radio",
        RadioSet: "RadioSet",
        Static: "StaticControl",
        StaticBlock: "StaticDomElement"
    };

}

if (typeof ValueType == "undefined") {

    var ValueType = {
        NotSet: "NotSet",
        String: "String",
        Int: "Int",
        Bool: "Bool",
        DateTime: "DateTime",
        Date: "Date",
        Time: "Time",
        Bit: "Bit",
        Money: "Money",
        NoValue: "NoValue"
    };

}

if (typeof DesignType == "undefined") {

    var DesignType = {
        NotSet: "NotSet",
        Inactive: "Inactive",
        Active: "Active",
        Disabled: "Disabled",
        Enabled: "Enabled",
        Invalid: "Invalid",
        Valid: "Valid",
        Changed: "Changed",
        Print: "Print"
    };

}