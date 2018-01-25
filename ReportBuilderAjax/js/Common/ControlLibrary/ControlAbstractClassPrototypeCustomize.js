ControlAbstractClass.prototype.SetDesign = function(designType) {
    switch (designType) {
        case DesignType.Inactive:
            //if (isIeButNot8) $(this.Element).removeClass("focus");
            break;
        case DesignType.Active:
            //if(isIeButNot8) $(this.Element).addClass("focus");
            break;
        case DesignType.Disabled:
            $(this.Element).addClass("disabled");
            break;
        case DesignType.Enabled:
            $(this.Element).removeClass("disabled");
            break;
        case DesignType.Invalid:
            $(this.Element).addClass("req");
            break;
        case DesignType.Valid:
            $(this.Element).removeClass("req");
            break;
        case DesignType.Changed:
            $(this.Element).addClass("changed");
            break;
        case DesignType.Print:
            break;
    }
};