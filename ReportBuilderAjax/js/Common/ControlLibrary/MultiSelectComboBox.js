function MultiSelectComboBox (_parentObject, _id, _options, callbackfunction) {
    this.ParentObject = _parentObject;
    this.Box = document.getElementById(_id);
    this.BoxId = _id;
    this.Options = _options;
    this.ListObj = null;
	this.ListTable = null;
    this.Image = null;
    this.isVisibleList = false;
    this.ListDiv = null;
    this.initialize();
    this.ListActive = false;
    this.CallBackFunc = callbackfunction;
};

MultiSelectComboBox.prototype.getDefaultListObj = function () {
    var div = document.createElement('div');
    div.style.position = 'absolute';
    div.style.zIndex = '1000001';
    div.style.overflow = 'auto';
    div.style.display = 'none';
    div.style.backgroundColor = "#fff";
    div.style.border = '1px solid #bdbdbd';
    div.style.textAlign = 'left';
    this.Box.parentNode.appendChild(div);
    this.ListDiv = div;

    var table = document.createElement('table');
    table.id = 'listTable';
    table.width = '100%';
    table.cellSpacing = '0';

    div.appendChild(table);
    this.ListTable = table;
    return div;
};

MultiSelectComboBox.prototype.DrawObject = function (object, table) {
    var tr = document.createElement('tr');
    var td1 = document.createElement('td');
    var td2 = document.createElement('td');
    td1.style.width = '10%';
    td2.style.width = '90%';
    td2.style.verticalAlign = "middle";

    var checkBox = document.createElement("input");
    checkBox.type = "checkbox";
    checkBox.style.border = "0";
    checkBox.setAttribute('value', object.Id);

    var span = document.createElement('span');
    span.innerHTML = object.Name;
    span.style.paddingLeft = "5px";
    span.style.cursor = "default";

    var parentObj = this;
    $(checkBox).click(function (event) { parentObj.ClickItem.call(parentObj, event, checkBox); });
    $(tr).bind('mouseover', function (sender) { parentObj.mouseOver.call(parentObj, sender); });
    $(tr).bind('mouseout', function (sender) { parentObj.mouseOut.call(parentObj, sender); });
    $(tr).click(function (event) { parentObj.ClickSpanItem.call(parentObj, event, tr); });

    td1.appendChild(checkBox);
    td2.appendChild(span);
    tr.appendChild(td1);
    tr.appendChild(td2);
    $(table).append(tr);
};

MultiSelectComboBox.prototype.initialize = function () {
    if (!this.Box) return;

    this.Image = document.createElement("img");
    this.Image.src = "./images/cmbbutton.gif";
    this.Box.parentNode.appendChild(this.Image);

    this.Box.value = 'Choose a State ...';
    this.ListObj = this.getDefaultListObj();

    if (this.Image) {
        this.Image.style.display = '';
        this.Image.style.position = 'absolute';
        this.Image.style.margin = '4px 0px 0px -20px';
    }

    this.ListObj.style.width = this.Box.style.width.replace('px', '') * 1 + 3 + 'px';

    this.BindListObj();
    this.setEvents();
};

MultiSelectComboBox.prototype.ListMouseOver = function () {
    this.ListActive = true;
};

MultiSelectComboBox.prototype.ListMouseOut = function () {
    this.ListActive = false;
};

MultiSelectComboBox.prototype.setEvents = function () {
    var parentObj = this;
    $(this.Box).click(function (sender) { parentObj.ClickPreBind.call(parentObj, sender) });
    if (this.Image) $(this.Image).click(function (sender) { parentObj.ClickPreBind.call(parentObj, sender) });
    $(document.body).bind("click", function (e) {
        parentObj.BodyClick.call(parentObj, this, e);
    });
    $(this.ListDiv).bind('mouseover', function (sender) { parentObj.ListMouseOver.call(parentObj, sender); });
    $(this.ListDiv).bind('mouseout', function (sender) { parentObj.ListMouseOut.call(parentObj, sender); });
};

MultiSelectComboBox.prototype.BodyClick = function(e, el) {
    if (!this.isVisibleList || el.target == this.Box || this.ListActive || el.target == this.Image) return;
    this.isVisibleList = !this.isVisibleList;

    if (this.isVisibleList) this.ListObj.style.display = '';
    else this.ListObj.style.display = 'none';
};

MultiSelectComboBox.prototype.ClickPreBind = function (sender) {
    this.isVisibleList = !this.isVisibleList;
    
	if(this.isVisibleList) this.ListObj.style.display = '';
	else this.ListObj.style.display = 'none';
};

MultiSelectComboBox.prototype.ClickItem = function (event, sender) {
    if (sender.value == 'ALL' && sender.checked) {
        $('input', this.ListObj).each(function () {
            this.checked = true;
            this.parentNode.parentNode.className = "checked";
        });
    } else if (sender.value == 'ALL' && !sender.checked) {
        $('input', this.ListObj).each(function () {
            this.checked = false;
            this.parentNode.parentNode.className = "";
        });
    }

    this.Box.value = this.GetSelectedItemsString();
    this.CallBackFunc.call(this.ParentObject, this);
};

MultiSelectComboBox.prototype.mouseOver = function (sender) {
    var obj = sender.target || sender.srcElement;
    var parents = $(obj).parents('tr');
    if (parents && parents.length > 0) {
        parents[0].className = "checked";
    }
};

MultiSelectComboBox.prototype.mouseOut = function (sender) {
    var obj = sender.target || sender.srcElement;
    var parents = $(obj).parents('tr');
    if (parents && parents.length > 0) {
        var checked = $("input", parents[0])[0].checked;
        if (!checked) {
            parents[0].className = "";
        }
    }
};


MultiSelectComboBox.prototype.ClickSpanItem = function (event, sender) {
    if (!sender) return;
    var obj = event.target || event.srcElement;
    if (obj.nodeName == 'INPUT') return;
    var inp = $('input', sender);
    if (inp && inp.length > 0) {
        inp[0].checked = !inp[0].checked;

        if (inp[0].value == 'ALL') {
            if (inp[0].checked) {
                $('input', this.ListObj).each(function () {
                    this.checked = true;
                    this.parentNode.parentNode.className = "checked";
                });
            } else if (!inp[0].checked) {
                $('input', this.ListObj).each(function () {
                    this.checked = false;
                    this.parentNode.parentNode.className = "";
                });
            }
        }
    }
    this.Box.value = this.GetSelectedItemsString();
    this.CallBackFunc.call(this.ParentObject);
};

MultiSelectComboBox.prototype.ClearOptions = function () {
    if (this.ListTable) {
        this.ListDiv.removeChild(this.ListTable);
    }
    this.ListTable = document.createElement('table');
    this.ListTable.id = 'listTable';
    this.ListTable.width = '100%';
    this.ListTable.cellSpacing = '0';
    if (this.ListDiv) {
        this.ListDiv.appendChild(this.ListTable);
        this.Box.value = 'Choose a State ...';
    }
    
};

MultiSelectComboBox.prototype.ClearOptionsOffice = function () {
    if (this.ListTable) {
        this.ListDiv.removeChild(this.ListTable);
    }
    this.ListTable = document.createElement('table');
    this.ListTable.id = 'listTable';
    this.ListTable.width = '100%';
    this.ListTable.cellSpacing = '0';
    if (this.ListDiv) {
        this.ListDiv.appendChild(this.ListTable);
        this.Box.value = 'Choose a office ...';
    }
    
};

MultiSelectComboBox.prototype.BindListObj = function (sender) {
    if (this.Options == null || this.Options.length == 0) return;

    for (var index in this.Options) {
        this.DrawObject(this.Options[index], this.ListTable);
    }
    if (this.Options.length >= 11) {
        this.ListDiv.style.height = '250px';
    }
};

MultiSelectComboBox.prototype.GetSelectedItems = function () {
    var result = new Array();
    var allCheckBox = null;

    $('input', this.ListObj).each(function () {
        if (this.value == 'ALL') allCheckBox = this;
        if (this.checked && this.value != 'ALL') result.push(this.value);
    });

    if (result.length == this.Options.length - 1) {
        allCheckBox.checked = true;
        allCheckBox.parentNode.parentNode.className = "checked";
    }
    else {
        allCheckBox.checked = false;
        allCheckBox.parentNode.parentNode.className = "";
    }

    return result;
};

MultiSelectComboBox.prototype.GetSelectedItemsString = function () {
    var result = '';
	var resultArray = this.GetSelectedItems();
	if (resultArray.length == 0) return 'Choose a State ...';

	if (resultArray.length > 1) return 'Multiple Items Selected';

	if (resultArray.length == 1 && resultArray[0] == 'ALL') return 'Multiple Items Selected';
	
	if(resultArray.length == 1){
		for(var i = 0; i < this.Options.length; i++){
			if(this.Options[i].Id == resultArray[0]) return  this.Options[i].Name;
		}
	}
		
	return result;
};

MultiSelectComboBox.prototype.GetOptionsCount = function () {
    var count = 0;
    if (this.Options) {
        count = this.Options.length + 1;
    }
    return count;
};