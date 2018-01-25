function Autocomplete(elementId, container, options, data, mapper, partialFunctionality)
{
    this.Box = document.getElementById(elementId);
    this.Container = container;
    this.options = options;
    this.data = data;
    this.mapper = mapper;
    this.ListObj = null;
    this.AutocompleteObj = null;
    this.countSymbols = 1;
    this.currentArray = new Array();
    this.mapperKey = 'id';
    this.mapperValue = 'value';
    this.selectedRowColor = '#0b71a4';
    this.defaultListObj = null;
    this.tempValue = '';
    this.tempArray = new Array();
    this.cancel = false;
    this.currentRow = 0;
    this.maxRow = 0;
    this.partialFunctionality = partialFunctionality;
    
    this.initialize();
}

Autocomplete.prototype.getTableList = function () {
    var table = document.createElement("table");
    table.cellPadding = 0;
    table.cellSpacing = 0;
    table.style.width = "97%";
    var tbody = document.createElement("tbody");
    table.appendChild(tbody);
    return table;
};

Autocomplete.prototype.DrawManager = function(data) {
    this.maxRow = 0;
    if (data != null) {
        var table = this.getTableList();
        if (typeof data.Objects != 'undefined' || data.Objects != null) {
            for (var objectId in data.Objects) {
                var objectProperty = data.Objects[objectId];
                var tempStr = objectProperty[this.mapperValue].toLowerCase();
                if (tempStr.indexOf(this.Box.value == 0)) {
                    this.tempArray[this.maxRow] = objectProperty;
                    this.DrawObject(objectProperty, table.childNodes[0]);
                    this.maxRow++;
                }
            }
            this.DrawList(table, this.maxRow);
            this.currentArray = new Array();
            for (var objectId in this.tempArray) {
                var object = this.tempArray[objectId];
                this.currentArray[objectId] = object;
            }
            this.tempArray = null;
            this.tempArray = new Array();
        }
        else if (data.length > 0) {
            if (data.length == 1) {
                var objectProperty = data[0];
                this.fillObject((objectProperty[this.mapperValue] + ""), "");
            }
            else {
                for (var index in data) {
                    if (typeof index != 'undefined' && typeof parseInt(index) != 'undefined') {
                        var objectProperty = data[index];
                        var tempStr = (objectProperty[this.mapperValue] + "").toLowerCase();
                        if (tempStr.indexOf(this.Box.value) == 0 || this.partialFunctionality) {
                            this.tempArray[this.maxRow] = objectProperty;
                            this.DrawObject(objectProperty, table.childNodes[0]);
                            this.maxRow++;
                        }
                    }
                }
                if (this.maxRow > 0) {
                    this.DrawList(table, this.maxRow);
                }
                this.currentArray = new Array();
                for (var objectId in this.tempArray) {
                    var object = this.tempArray[objectId];
                    this.currentArray[objectId] = object;
                }
                this.tempArray = null;
                this.tempArray = new Array();
            }
        }
    }
};

Autocomplete.prototype.DrawObject = function(object, table) {
    var tr = document.createElement('tr');
    tr.style.height = '22px';
    var td = document.createElement('td');
    td.style.width = '100%';
    td.align = 'left';
    var aValue = document.createElement("a");
    aValue.setAttribute('entityId', (typeof object[this.mapperKey] != 'undefined') ? object[this.mapperKey] : '');
    aValue.innerHTML = typeof object[this.mapperValue] != 'undefined' ? object[this.mapperValue] : '';
    //var aEmpty = document.createElement("a");
    //var aComment = document.createElement("a");
    td.appendChild(aValue);
    //td.appendChild(aEmpty);
    //td.appendChild(aComment);
    tr.appendChild(td);
    if (this.currentRow == 0) {
        tr.style.backgroundColor = this.selectedRowColor;
        tr.style.color = '#fff';
        this.currentRow = 1;
    }
    table.appendChild(tr);
};

Autocomplete.prototype.DrawList = function(table, count) {
    this.ListObj.appendChild(table);
    if (count == 0) {
        this.ListObj.innerHTML = '';
        this.AutocompleteObj.style.display = 'none';
    }
    this.Box.parentNode.appendChild(this.AutocompleteObj);
    var loc = $.Region.getElementLocation(this.Box);
    var parentLoc = $.Region.getElementLocation($get('main_form_div'));

    /*var offsetParent = this.Box.offsetParent;
    var parentOffset = this.Box.offsetTop;
    while (offsetParent) {
    parentOffset += offsetParent.offsetLeft;
    offsetParent = offsetParent.offsetParent;
    }

    var loc = $.Region.getElementLocation(this.Box);
    loc.x = parentOffset;*/

    //to lines below was commented by ice to fix popin location problem forced by new design
    //this.AutocompleteObj.style.left = loc.left - (($.browser.msie) ? 8 : 5) + "px"; //(loc.left - (($.browser.msie) ? 2 : 0) - parentLoc.left) + 'px';
    //this.AutocompleteObj.style.top = (loc.top + this.Box.clientHeight + (($.browser.msie) ? 5 : 6)) + 'px';
        
    this.AutocompleteObj.style.display = '';
    this.ListObj.style.display = '';
    var autocomplete = this;
    $('a', this.ListObj).each(function() {
        var elem = this;
        $(this.parentNode).bind('click', function(sender) {
            if (sender.target) {
                sender.target = elem;
            }
            else {
                sender.srcElement = elem;
            }
            autocomplete.changeObject.call(autocomplete, sender);
        });
    });
    $('td', this.ListObj).each(function() { $(this).bind('mouseover', function(sender) { autocomplete.mouseOver.call(autocomplete, sender); }); });
    $('td', this.ListObj).each(function() { $(this).bind('mouseout', function(sender) { autocomplete.mouseOut.call(autocomplete, sender); }); });
};

Autocomplete.prototype.mouseOver = function(sender)
{
    var aElement = sender.target || sender.srcElement;
    var object = $('a',aElement);
    var autocomplete = this;
    var counter = 1;
    $('a',this.ListObj).each(function(){
            if (this.innerHTML == ((typeof object[0] == 'undefined') ? aElement.innerHTML : object[0].innerHTML))
            {
                this.parentNode.parentNode.style.cursor = 'default';
                this.parentNode.parentNode.style.backgroundColor = autocomplete.selectedRowColor;
                this.parentNode.parentNode.style.color = '#fff';
                ($.browser.msie) ? this.parentNode.focus() : this.focus();
                autocomplete.currentRow = counter;
            }
            else
            {
                this.parentNode.parentNode.style.backgroundColor = "#fff";
                this.parentNode.parentNode.style.color = '#000';
            }
            counter++;
        });
};

Autocomplete.prototype.mouseOut = function(sender)
{
    var aElement = sender.target || sender.srcElement;
    var object = $('a',aElement);
    var autocomplete = this;
    var overlay = false;
    $('a',this.ListObj).each(function(){
        if (this.innerHTML != ((typeof object[0] == 'undefined') ? aElement.innerHTML : object[0].innerHTML))
        {
            if (this.parentNode.parentNode.style.backgroundColor == autocomplete.selectedRowColor)
            {
                overlay = true;
            }
        } 
    });
    if (overlay)
    {
        if (typeof object[0] == 'undefined') 
        {   
            aElement.parentNode.parentNode.style.backgroundColor = "#fff"; 
            aElement.parentNode.parentNode.style.color = '#000';
        } else {
            aElement.parentNode.style.backgroundColor = "#fff"; 
            aElement.parentNode.parentNode.style.color = '#000';
        }
        this.currentRow = 0;
    }
};

Autocomplete.prototype.fillObject = function(value, id) {
    if (!this.partialFunctionality) {
        this.Box.value = value;
        $(this.Box).trigger("change");
    }
    this.currentArray = new Array();
    this.Box.blur();
    $(this.Box).attr('objectId', id);
    this.clearList();

    if (this.SelectCallback) {
        this.SelectCallback.call(this.ElementControl, [value]);
    } else {
        $(this.Box).trigger("valueSelected", [value]);
    }
};

Autocomplete.prototype.changeObject = function(sender) {
    var aElement = sender.target || sender.srcElement;
    var value = aElement.innerText || aElement.text;
    this.fillObject(value, aElement.id);
};

Autocomplete.prototype.clearList = function()
{
    this.currentRow = 0;
    this.ListObj.innerHTML = '';  
    this.AutocompleteObj.style.display = 'none';
};

Autocomplete.prototype.getDefaultListObj = function() {
    var div = document.createElement('div');
    this.setDefaultCssToObj(div);
    div.className = 'autopopin';
    return div;
};

Autocomplete.prototype.setDefaultCssToObj = function(divObj) {
    divObj.style.position = 'absolute';
    divObj.style.zIndex = '100001';
    divObj.style.overflow = 'auto';
    divObj.style.display = 'none';
    //divObj.style.color = '#333333';
    //divObj.style.backgroundColor = "#F5F0DE";
    //divObj.style.height = '145px';
    //divObj.style.border = '1px solid #333333';
};

Autocomplete.prototype.initialize = function() {
    if (!this.Box) return;
    if (this.options != null) {
        if (typeof this.options.minSymbols != 'undefined')
            if (parseInt(this.options.minSymbols))
            this.countSymbols = this.options.minSymbols;
        if (typeof this.options.selectedRowColor != 'undefined')
            if (this.options.selectedRowColor.toString())
            this.selectedRowColor = this.options.selectedRowColor;
    }

    if (this.mapper != null) {
        if (typeof this.mapper.key != 'undefined')
            this.mapperKey = this.mapper.key;

        if (typeof this.mapper.value != 'undefined')
            this.mapperValue = this.mapper.value;
    }

    if (this.data != null)
        if (this.data.params != null)
        if (typeof this.data.params[this.mapperValue] == 'undefined')
        this.data.params[this.mapperValue] = '';

    if (this.Container != null) {
        if (typeof this.Container.containerCSS != 'undefined') {
            var divObject = $('div[@class="' + this.Container.containerCSS + '"]');
            if (typeof divObject != 'undefined') {
                divObject.className = this.Container.containerCSS;
                this.ListObj = divObject;
                this.AutocompleteObj = this.ListObj;
            }
            else {
                var div = document.createElement('div');
                div.className = this.Container.containerCSS;
                this.ListObj = div;
                this.AutocompleteObj = this.ListObj;
            }
            this.setEvents();
        }
        else if (typeof this.Container.containerID != 'undefined') {
            var divObj = document.getElementById(this.Container.containerID);
            this.AutocompleteObj = divObj;
            this.setDefaultCssToObj(this.AutocompleteObj);
            var object = $('div[@class="AutocompleteContext"]', divObj);
            if (typeof object == 'undefined') this.ListObj = divObj;
            else {
                this.ListObj = object[0];
            }
            this.setEvents();
        }
        else {
            this.ListObj = this.getDefaultListObj();
            this.AutocompleteObj = this.ListObj;
            this.setEvents();
        }
    }
    else {
        this.ListObj = this.getDefaultListObj();
        this.AutocompleteObj = this.ListObj;
        this.setEvents();
    }

    var autocomplete = this;
    document.onkeydown = function(event) { autocomplete.keyBoard.call(autocomplete, event); };
};

Autocomplete.prototype.keyUpEventHanler = function(sender) {
    this.autocomplete.BindListObj(this.autocomplete, sender)
};

Autocomplete.prototype.setEvents = function() {
    this.Box.autocomplete = this;
    $(this.Box).bind('keyup', this.keyUpEventHanler);
    //$(this.Box).bind('focus',function(sender){autocomplete.BindListObj.call(autocomplete, sender)});
};

Autocomplete.prototype.unsetEvents = function() {
    $(this.Box).unbind('keyup', this.keyUpEventHanler);
};

Autocomplete.prototype.showPopin = function() {
    this.BindListObj(this, null);
};

Autocomplete.prototype.HandlerSuccess = function(data)
{
    this.DrawManager(data);
};

Autocomplete.prototype.BindData = function()
{
    if (typeof this.data.url != 'undefined')
    {
        if (this.Box.value.length >= this.countSymbols)
        {
            if (typeof this.data.params != 'undefined' )
                if (typeof this.data.params[this.mapperValue] != 'undefined')
                    {
                        this.data.params[this.mapperValue] = this.Box.value;
                        var autocomplete = this;
                        $.ajax({type:'POST',url:this.data.url,data:{params:JSON.stringify(this.data.params)},
                        dataType: 'json',success:function(data){autocomplete.HandlerSuccess.call(autocomplete, data);},error: function(data){autocomplete.Failure.call(autocomplete, data);}});
                    }
        }
    }
    else if (this.data.length > 0)
    {
        if (this.Box.value.length >= this.countSymbols)
        {
            this.DrawManager(this.data);
        }
    }    
};

Autocomplete.prototype.BindListObj = function(autoComplit, sender)
{
    if (autoComplit.cancel) {
        autoComplit.cancel = false; 
        return;
    }
    autoComplit.clearList();
    if (autoComplit.currentArray.length > 0)
    {
        if (autoComplit.Box.value.indexOf(autoComplit.tempValue) == 0)
        {
            autoComplit.DrawManager(autoComplit.currentArray);
        }
        else
        {
            autoComplit.currentArray = new Array();
            autoComplit.BindData();
        }
    }
    else
    {
        autoComplit.BindData();
    }
    autoComplit.tempValue = autoComplit.Box.value;     
}

Autocomplete.prototype.Failure = function(data)
{
    alert(data);    
}

Autocomplete.prototype.keyBoard = function(event) {
    var e = window.event;
    if (!e) e = event;
    window.status = e.keyCode;
    if (e.keyCode == 27 && this.AutocompleteObj != null && this.AutocompleteObj.style.display != 'none') {
        if (this != null) {
            if (this.AutocompleteObj != null) {
                if (this.ListObj.innerHTML != '' || this.AutocompleteObj.style.display != 'none') {
                    this.clearList();
                }
            }
            this.currentArray = new Array();
            this.cancel = true;
            this.Box.focus();
        }
    }
    if (e.keyCode == 38 || e.keyCode == 40) {
        if (e.keyCode == 40 && this.ListObj.innerHTML != '') if (this.maxRow > this.currentRow) this.currentRow++;
        if (e.keyCode == 38 && this.ListObj.innerHTML != '') if (this.currentRow > 1) this.currentRow--;
        var counter = 1;
        var autocomplete = this;
        $('a', this.ListObj).each(function() {
            if (counter == autocomplete.currentRow) {
                this.parentNode.style.cursor = 'default';
                this.parentNode.parentNode.style.backgroundColor = autocomplete.selectedRowColor;
                this.parentNode.parentNode.style.color = "#fff";
                ($.browser.msie) ? this.parentNode.focus() : this.focus();
                if (autocomplete.currentRow < 2 || autocomplete.currentRow > autocomplete.currentArray.length - 2) {
                    autocomplete.ListObj.scrollTop = 0;
                }
            }
            else {
                this.parentNode.parentNode.style.backgroundColor = "#fff";
                this.parentNode.parentNode.style.color = "#000";
            }
            counter = counter + 1;
        });
    }
    if (e.keyCode == 13) {
        var autocomplete = this;
        var counter = 1;
        var parentObj = this;
        $('a', this.ListObj).each(function() {
            if (counter == autocomplete.currentRow) {
                /*
                this.currentArray = new Array();
                autocomplete.Box.blur();
                autocomplete.Box.value = this.innerHTML;
                $(autocomplete.Box).attr('objectId', this.id);
                autocomplete.clearList();
                */

                this.cancel = true;
                parentObj.changeObject({ target: this });
            }
            counter = counter + 1;
        });
    }
};