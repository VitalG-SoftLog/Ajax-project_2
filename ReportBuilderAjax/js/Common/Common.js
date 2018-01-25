var emptyString = "";

function $get(id) {
    return document.getElementById(id);
}


function returnNumber(data) {
    var num = new Number(data);
    return num.toFixed(2) * 1;
}; 

function trim(source) {
    return source.replace(/(^\s+)|(\s+$)/g, "");
}

function devThrow(msg) {
    if (jsDebug) {
        throw msg;
    }
}

function checkYear(year) {
    if (year < 1901 || year > (new Date()).getFullYear()) {
        return false;
    }
    return true;
}

function checkYearForward(year, baseYear) {
    var currentYear = (new Date()).getFullYear();
    if (year < baseYear || year > (currentYear + Consts.MISC.YEAR_OFFSET)) {
        return false;
    }
    return true;
}

function checkNumericHandler(value, showMessage) {
    if (value.length == 0) return true;
    var twoDig = true;

    if (value.indexOf('.') == 0) twoDig = false;
    else if (value.indexOf('.') > 0 && (value.substring(value.indexOf('.')).length > 3 || value.substring(value.indexOf('.')).length == 1)) twoDig = false;

    if ((isNaN(value - 0) || value * 1 < 0) || !twoDig) {
        if(showMessage)alert('Value has an invalid numerical format.\nPlease enter a dollar value with no more than two decimal places.');
        return false;
    }
    return true;
};

function openInFrame(link) {
    window.location.href = link;
};

function ReplaceQuoteCharToUnicodeString(str) {
    return str.replace(/\"/g, '&quot;');
};

function ReplaceQuoteUnicodeToChar(objToParse) {
    for (var prop in objToParse) {
        var obj = objToParse[prop];
        if (obj && obj.replace) {
            objToParse[prop] = obj.replace(/&quot;/g, '"');
        }
    }
    return objToParse;
};

function ValidateEmail(field, alerttxt) {
    with (field) {
        if (value.indexOf(",") > -1) {
            alert(alerttxt); return false;
        }
        apos = value.indexOf("@");
        dotpos = value.lastIndexOf(".");
        if (apos < 1 || dotpos - apos < 2)
        { alert(alerttxt); return false; }
        else { return true; }
    }
};

function sendAjax(url, params, callbackSuccess, callbackError) {
    $.ajax({
        type: "POST",
        dataType: 'json',
        url: url,
        data: params,
        success: callbackSuccess,
        error: callbackError
    });
};

function validateControlSet(controlSet, exceptionList) {
    var result = [];
    var message = '';
    var validationGroups = {};
    if (!exceptionList) exceptionList = {};
    for (var controlName in controlSet) {
        if (controlName in exceptionList) continue;
        var control = controlSet[controlName];
        if (!control) continue;
        var validationInfo = control.ValidationInfo;
        if (validationInfo && validationInfo.Required == true) {
            control.SetDesign(DesignType.Valid);
            for (dependentControlName in control.ValidationInfo.DependentControls) {
                var dependentControl = control.ValidationInfo.DependentControls[dependentControlName];
                dependentControl.SetDesign(DesignType.Valid);
            }
            if (!control.IsVisible()) continue;
            if (control.Blocked) continue;
            if (control.Validate()) continue;
            if (!validationInfo.Name) {
                validationInfo.Name = control.Element.Id;
            }
            if (null != validationInfo.GroupName) {
                if (validationGroups[validationInfo.GroupName]) {
                    validationGroups[validationInfo.GroupName] += '  - ' + validationInfo.Name + '<br/>';
                }
                else {
                    validationGroups[validationInfo.GroupName] = '  - ' + validationInfo.Name + '<br/>';
                }
            }
            else {
                message += '- ' + validationInfo.Name + '<br/>';
            }
            result[result.length] =
                {
                    control: control,
                    name: validationInfo.Name
                };
            control.SetDesign(DesignType.Invalid);
                for (dependentControlName in control.ValidationInfo.DependentControls) {
                    var dependentControl = control.ValidationInfo.DependentControls[dependentControlName];
                    dependentControl.SetDesign(DesignType.Invalid);
                }
        }
    }
    for (var key in validationGroups) {
        message += '<span class="validationGroupName">' + key + '</span>:<br/>' + validationGroups[key];
    }
    return { elements: result, message: message };
};

makeControlRequired = function(control, paramObject, stateFunction) {
    if (control.Element) {
        if (control.ValidationInfo) {
            var name = null;
            var groupName = null;
            var titleElementId = null;
            if (paramObject) {
                name = paramObject.name;
                groupName = paramObject.group;
                titleElementId = paramObject.titleId;
            }
            var controlTitle = null;
            if (titleElementId) {
                controlTitle = document.getElementById(titleElementId);
            } else {
                controlTitle = document.getElementById(control.Id + "_title");
            }
            if (controlTitle) {
                if (!name) {
                    name = controlTitle.innerText ? controlTitle.innerText : controlTitle.textContent;
                }
                if (!groupName) {
                    groupName = controlTitle.getAttribute("validationGroup");
                }
                if ($("img[alt='Required']", controlTitle).length == 0) {
                    var img = document.createElement("img");
                    img.src = "./images/info_required_star.png";
                    img.className = "star";
                    img.alt = "Required";
                    controlTitle.appendChild(img);
                }
                if (stateFunction && typeof stateFunction == "function") {
                    control.ValidationInfo.StateFunction = stateFunction;
                }
            } else {
                devThrow("Developer tarce info -> Common.js -> makeControlRequired -> No DOM element attached to control[id=" + control.Id + "] title!");
            }
            control.ValidationInfo.Required = true;
            control.ValidationInfo.Name = name;
            control.ValidationInfo.GroupName = groupName
        };
    } else {
        devThrow("Developer tarce info -> Common.js -> makeControlRequired -> No DOM element attached to control[id=" + control.Id + "]!");
    }
};

makeControlNotRequired = function(control, paramObject) {
    if (control.Element) {
        var titleElementId = null;
        if (paramObject) {
            titleElementId = paramObject.titleId;
        }
        if (control.ValidationInfo) {
            var controlTitle = document.getElementById(titleElementId ? titleElementId : control.Id + "_title");
            if (controlTitle) {
                $("img[alt='Required']", controlTitle).remove();
            } else {
                devThrow("Developer tarce info -> Common.js -> makeControlNotRequired -> No DOM element attached to control[id=" + control.Id + "] title!");
            }
            control.ValidationInfo.Required = false;
            control.ValidationInfo.Name = "";
            control.ValidationInfo.GroupName = "";
            control.SetDesign(DesignType.Valid);
        };
    } else {
        devThrow("Developer tarce info -> Common.js -> makeControlNotRequired -> No DOM element attached to control[id=" + control.Id + "]!");
    }
};

testControlsAlert = function (controlsSet, controlSetName, showAll) {
    if (!controlSetName) controlSetName = "No Name";
    var outStr = "<h3>" + controlSetName + "</h3>";
    var elementCount = 0;
    if (controlsSet) {
        outStr += "<table style='text-align:left;'>";
        outStr += "<tr><th><b>Control</b></th><th><b>Type</b></th><th><b>Id</b></th><th><b>Is Null</b></th></tr>";
        for (cName in controlsSet) {
            var nextLine = "<tr><td>" + cName + "&nbsp;&nbsp;</td><td>" + controlsSet[cName].Type + "&nbsp;&nbsp;</td><td>" + controlsSet[cName].Id + "&nbsp;&nbsp;</td><td>" + (controlsSet[cName].Element == null) + "</td></tr>";
            if (controlsSet[cName].Element == null) {
                outStr += nextLine;
                elementCount++;
            } else {
                if (showAll == true) {
                    outStr += nextLine;
                    elementCount++;
                }
            }
        }
        outStr += "</table><br/>";
    } else {
        outStr += "<p><b>No Controls Set In This Object Was Found!</b></p>";
    };
    if (elementCount == 0) {
        return;
    }
    var md = document.getElementById("controlTestPopUp");
    if (md == null) {
        md = document.createElement("div");
        md.style.display = "none";
        md.style.fontSize = "13px";
        md.style.backgroundColor = "Black";
        md.style.border = "solid 2px Orange";
        md.style.color = "White";
        md.style.fontFamily = "Verdana";
        md.id = "controlTestPopUp";

        var defCss = $.blockUI.defaults.css;
        $.blockUI({ message: $(md), css: { marginLeft: '5%', marginRight: '5%', width: '90%', top: '150px'} });

        var ic = document.createElement("div");
        ic.innerHTML = outStr;
        ic.style.height = "350px";
        ic.style.display = "block";
        ic.style.overflow = "auto";
        md.appendChild(ic);
        var cb = document.createElement("input");
        cb.type = "button";
        cb.value = "Close this box!";
        cb.style.margin = "10px";
        cb.onclick = function (e) {
            $.unblockUI();
        };
        md.appendChild(cb);
    } else {
        md.firstChild.innerHTML += "</hr>";
        md.firstChild.innerHTML += outStr;
    }
};

function validationAlert(titleMessage, message, callback) {
    var md = document.createElement("div");
    md.style.fontFamily = "Verdana";
    var ic = document.createElement("div");
    ic.style.height = "300px";
    ic.style.display = "block";
    ic.style.overflow = "auto";
    ic.style.margin = "10px";
    ic.style.padding = "10px";
    ic.style.border = "solid 1px Silver";
    ic.align = "left";
    var pre = document.createElement("span");
    pre.innerHTML = message;
    pre.style.fontSize = "12px";
    var title = document.createElement("div");
    title.align = "center";
    title.style.fontWeight = "bold";
    title.style.fontSize = "14px";
    title.style.color = "Gray";
    title.style.paddingTop = "10px";
    title.innerHTML = titleMessage;    
    var button = document.createElement("input");
    button.type = "button";
    button.value = "Close";
    button.style.margin = "10px";
    button.onclick = function(e) {
        $.unblockUI();
        if (typeof callback == "function") {
            callback();
        }
    };
    ic.appendChild(pre);
    md.appendChild(title);    
    md.appendChild(ic);
    md.appendChild(button);
    $.blockUI({ message: $(md), css: { marginLeft: '20%', marginRight: '20%', width: '60%', top: '30%'} });
    md.scrollIntoView();   
}


function getBoolFromControl(control) {
    var result=false;
    if (control && control.GetValue) {
        var value = control.GetValue();
        if(value){
            if (typeof value == "number") {
                result = value == 1;
            }
            if (typeof value == "string") {
                value = value.toLowerCase();
                if (value * 1 == 1 || value * 1 == 0) {
                    result = value * 1 == 1;
                } 
                if(value == "yes" || value=="no") {
                    result = value.toLowerCase() == "yes";
                }
                if (value == "true" || value == "false") {
                    result = value.toLowerCase() == "yes";
                }
            }
            if (typeof value == "boolean") {
                result = value;
            }
        }
    }    
    return result;
};

//Extend standart objects
Function.prototype.Delegate = function (context, method) {
    return function() { method.call(context); };
};

Function.prototype.DelegateWithData = function (context, method, data) {
    return function() {
        method.call(context, data);
    };
};

Function.prototype.DelegateHandler = function (context, method) {
    return function(e) {
        if (!e) e = window.event;
        method.call(context, e, this);
    };
};

Function.prototype.DelegateHandlerWithData = function (context, method, data) {
    return function(e) {
        if (!e) e = window.event;
        method.call(context, e, this, data);
    };
};

removeObjectElement = function(obj, index, reorderIndex, startIndex) {
    if (typeof obj != "object") return obj;
    if (obj[index] == null) return obj;
    var newObject = {};
    var newIndex = 1;
    if (startIndex && typeof startIndex == "number") {
        newIndex = startIndex;
    }
    for (var propertyName in obj) {
        if (index != propertyName) {
            if (reorderIndex) {
                newObject[newIndex] = obj[propertyName];
            } else {
                newObject[propertyName] = obj[propertyName];
            }
            newIndex++;
        }        
    }
    return newObject;
};

function getObjectElementsCount(object) {
    var count = 0;
    if (!object ||typeof object != "object") return count;    
    for (var name in object) {
        if(name != "remove")
            count++;
    }
    return count;
};

function getLastObjectIndex(object) {
    var lastIndex = null;
    if (!object || typeof object != "object") return lastIndex;
    for (var name in object) {
        lastIndex = name;
    }
    return lastIndex;
};

function manageZipBlock(zipElement, cityElement, stateElement, zipData, dataIndex) {
    //city and state elements must be blocked ouside this function before call this function
    var zipValue = zipElement.GetValue();
    var doUnblock = false;
    if (zipData && zipData[dataIndex] != null) {
        var doUnblock = !zipData[dataIndex];
    }
    if (doUnblock) {
        doUnblock = zipValue.length < 5 ? false : true;
    }
    if (doUnblock || zipData.length == 0) {
        cityElement.Unblock();
        stateElement.Unblock();
    }
};

function cloneObject(sourceObject) {
    return $.extend(true, {}, sourceObject);    
};

function getPhoneValue(controlSet, baseControlName) {
    var value = emptyString;
    for (var i = 1; i <= 3; i++) {
        value += controlSet[baseControlName + i].Element.value;
    }
    return value;
};

function setPhoneAsInvalid(controlSet, baseControlName) {
    for (var i = 1; i <= 3; i++) {
        controlSet[baseControlName + i].SetDesign(DesignType.Invalid);
    }
};

function setPhoneAsValid(controlSet, baseControlName) {
    for (var i = 1; i <= 3; i++) {
        controlSet[baseControlName + i].SetDesign(DesignType.Valid);
    }
};

function validateZipCode(controlSet) {
    var result = [];
    var message = emptyString;
    var re = /[^0-9]/gi;
    var value = controlSet.Element.value.toString();
    
    if (value.length < 5 || value.length != value.replace(re, "").length) {
        message = '<span class="validationGroupName">Zip Code format validation</span>:<br/> - Invalid Zip Code.<br/>';
        result[result.length] =
                {
                    control: null,
                    name: emptyString
                };

        controlSet.SetDesign(DesignType.Invalid);
    }
    else {
        controlSet.SetDesign(DesignType.Valid);
    }
    return { elements: result, message: message };
}

function validateEmail(controlSet, message) {
    var result = [];
    var resultMessage = emptyString;


    var value = controlSet.Value;
    if (value && value.split('@').length > 2) {
        resultMessage = message + ' - Only one email address allowed!<br/>';

        result[result.length] =
                {
                    control: null,
                    name: emptyString
                };

        controlSet.SetDesign(DesignType.Invalid);

        return { elements: result, message: resultMessage };
    }
    
    if (controlSet.GetValue() != "") {

        with (controlSet.Element) {
            if (value.indexOf(",") > -1) {
                resultMessage = message + ' - Invalid Email address.<br/>';

                result[result.length] =
                {
                    control: null,
                    name: emptyString
                };

                controlSet.SetDesign(DesignType.Invalid);

                return { elements: result, message: resultMessage };
            }
            
            apos = value.indexOf("@");
            dotpos = value.lastIndexOf(".");
            
            if (apos < 1 || dotpos - apos < 2) {
                resultMessage = message + ' - Invalid Email address.<br/>';

                result[result.length] =
                {
                    control: null,
                    name: emptyString
                };

                controlSet.SetDesign(DesignType.Invalid);

                return { elements: result, message: resultMessage };
            }
        }
        
    }
    
    controlSet.SetDesign(DesignType.Valid);
    
    return { elements: result, message: resultMessage };
}

function validatePhones(controlSet, phoneNumbers, baseResult) {
    var result = [];
    var message = emptyString;
    var re = /[^0-9]/gi;
    
    if (!phoneNumbers) {
        return { elements: result, message: message };
    }

    var isAnyNotValid = false;

    for (var phoneControlName in phoneNumbers) {
        phoneNumbers[phoneControlName].Value = getPhoneValue(controlSet, phoneControlName);
    }

    for (var phoneControlName in phoneNumbers) {
        var length = phoneNumbers[phoneControlName].Value.length;
        var firstControl = controlSet[phoneControlName + "1"];
        if (!firstControl.ValidationInfo.Required) {
            setPhoneAsValid(controlSet, phoneControlName);
        }
        if (length > 0 && length < 10 || length != phoneNumbers[phoneControlName].Value.replace(re,"").length) {
            isAnyNotValid = true;
            setPhoneAsInvalid(controlSet, phoneControlName);
            if (message == emptyString) {
                message = '<span class="validationGroupName">Phone numbers format validation</span>:<br/>';
                if (baseResult && baseResult.elements.length > 0) {
                    message = emptyString;
                }
            }
            message += "- " + phoneNumbers[phoneControlName].Name + "<br/>";
        }
    }

    if (isAnyNotValid) {
        if (!baseResult || baseResult.elements.length == 0) {
            result[result.length] =
                {
                    control: null,
                    name: emptyString
                };

        } else {
            baseResult.message += message;
            return baseResult;
        }
    } else {
        if (baseResult) {
            return baseResult; 
        }
    }

    return { elements: result, message: message };
};

function getMoneyFormat(number, excludeDollar) {
    var tempString = '0';
    if (!number) {
        tempString = '0';
        number = 0;
    }
    number = number.toString().replace( /,/ , "") * 1;

    number = (number * 1).toFixed(2);

    tempString = number.toString();
    tempString = tempString.replace('$');
    var mantissa = '';
    if (tempString.indexOf('.') > -1) {

        mantissa = tempString.substring(tempString.indexOf('.') + 1, tempString.indexOf('.') + 3);
        tempString = tempString.substring(0, tempString.indexOf('.'));
        if (mantissa.length == 1) {
            mantissa = mantissa + "0";
        }
    }
    else mantissa = '00';

    var temp = '';
    
    for (var i = tempString.length - 1; i > -1; i--) {
        if ((tempString.length - 1 - i) != 0 && (tempString.length - 1 - i) % 3 == 0) temp += ',' + tempString.charAt(i);
        else temp += tempString.charAt(i);
    }
    
    tempString = '';
    
    for (var j = temp.length-1; j > -1; j--) {
        tempString += temp.charAt(j);
    }

    var returnString = (excludeDollar ? '' : '$') + tempString + '.' + mantissa;

    return returnString;
};

function setDifTRStyle(tr) {
    //$(tr).css("font-style", "italic").css("font-weight", "bold");
    tr.className = tr.className + ' atention1';
    if (tr.parentNode) {
        tr.parentNode.className = tr.parentNode.className + ' stateRowDiff';
    }
};

function pdfPrintAlert(titleMessage, message, callback) {
    var md = document.createElement("div");
    md.style.fontFamily = "Verdana";
    var ic = document.createElement("div");
    ic.style.height = "100px";
    ic.style.display = "block";
    ic.style.overflow = "auto";
    ic.style.margin = "10px";
    ic.style.padding = "10px";
    ic.style.border = "solid 1px Silver";
    ic.align = "left";
    var pre = document.createElement("span");
    pre.innerHTML = message;
    pre.style.fontSize = "12px";
    var title = document.createElement("div");
    title.align = "center";
    title.style.fontWeight = "bold";
    title.style.fontSize = "14px";
    title.style.color = "Gray";
    title.style.paddingTop = "10px";
    title.innerHTML = titleMessage;
    var button = document.createElement("input");
    button.type = "button";
    button.value = "Cancel";
    button.style.margin = "10px";
    button.onclick = function (e) {
        $.unblockUI();
        if (typeof callback == "function") {
            callback(-1);
        }
    };
    var cbutton = document.createElement("input");
    cbutton.type = "button";
    cbutton.value = "Print Client";
    cbutton.style.margin = "10px";
    cbutton.onclick = function (e) {
        $.unblockUI();
        if (typeof callback == "function") {
            callback(Consts.PRINT_TYPE.CLEINT);
        }
    };
    var ibutton = document.createElement("input");
    ibutton.type = "button";
    ibutton.value = "Print Internal";
    ibutton.style.margin = "10px";
    ibutton.onclick = function (e) {
        $.unblockUI();
        if (typeof callback == "function") {
            callback(Consts.PRINT_TYPE.INTERNAL);
        }
    };
    ic.appendChild(pre);
    md.appendChild(title);
    md.appendChild(ic);
    md.appendChild(cbutton);
    md.appendChild(ibutton);
    md.appendChild(button);
    $.blockUI({ message: $(md), css: { marginLeft: '30%', marginRight: '30%', width: '40%', top: '30%'} });
    md.scrollIntoView();
}

function returnShortDate(date) {
    var shortDate = '';
    date = new Date(date);

    if (date.toString().indexOf('UTC-') > -1 || date.toString().indexOf('CST') > -1 ||
        date.toString().indexOf('PST') > -1 || date.toString().indexOf('PDT') > -1 ||
        date.toString().indexOf('EST') > -1 || date.toString().indexOf('EDT') > -1 ||
        date.toString().indexOf('CDT') > -1 || date.toString().indexOf('MDT') > -1 ||
        date.toString().indexOf('MST') > -1) {

        date = new Date(date.getFullYear(), date.getMonth(), date.getDate() + 1);
    }

    shortDate = (date.getMonth() + 1) + '/' + date.getDate() + '/' + date.getFullYear();
    return shortDate;
}

var winAuthPath = "";
function returnToLogin(url){
    document.location.href = url;
}

function returnTime(date) {
    var currentTime = new Date(date);
    var hours = currentTime.getHours();
    var minutes = currentTime.getMinutes();
    var seconds = currentTime.getSeconds();

    var suffix = "AM";
    if (hours >= 12) {
        suffix = "PM";
        hours = hours - 12;
    }
    if (hours == 0) hours = 12;
    if (minutes < 10) minutes = "0" + minutes;
    if (seconds < 10) seconds = "0" + seconds;
    return hours + ":" + minutes + ":" + seconds + " " + suffix;
};

Date.prototype.formatMMDDYYYY = function () {
    return (this.getMonth() + 1) +
    "/" + this.getDate() +
    "/" + this.getFullYear();
};

function daysInMonth(year, month) {
    var dd = new Date(year, month, 0);
    return dd.getDate();
}

function HideAllDropDown(e) {
    $('div[dropattribute]').each(function () {
        recIterationForDropDown = 0;
        var targetElement = recForDropDown(e.srcElement || e.target);
        if (!targetElement || targetElement.getAttribute && this.id != targetElement.getAttribute("isDropAttribute") + "_ImageTable") {
            if (targetElement && targetElement.className != "filter")
                this.style.display = "none";
        }
    });
};

function HideSaveCloud(e) {
    var saveElement = findParentElement(e.srcElement || e.target, 10, 'SaveAttr');
    if(!saveElement) {
        if (RBH && RBH.ReportBuilderForm) $(document.getElementById(RBH.ReportBuilderForm.getClientID("saveCloud"))).removeClass('SaveCloud');
        if (RBH && RBH.ReportBuilderForm) $(document.getElementById(RBH.ReportBuilderForm.getClientID("saveCloud"))).addClass('SaveCloudDisabled');
    }
};

function findParentElement(node, layer, attrName) {
    if(node == null) {
        return null;
    }
    layer--;
    var result = null;
    if (layer < 10) {
        if (node.tagName.toLowerCase() != "body" && node.nodeName.toLowerCase() != "body") {
            if (node.getAttribute(attrName))
                return node;
            else {
                result = findParentElement(node.parentNode, layer, attrName);
            }
        }
        else return result;
    }
    return result;
};

function HideSaveAsNewCloud(e) {
    var saveElement = findParentElement2(e.srcElement || e.target, 10, 'SaveAsNewAttr');
    if (!saveElement) {
        if (RBH && RBH.ReportBuilderForm) $(document.getElementById(RBH.ReportBuilderForm.getClientID("saveAsNewCloud"))).removeClass('SaveAsNewCloud');
        if (RBH && RBH.ReportBuilderForm) $(document.getElementById(RBH.ReportBuilderForm.getClientID("saveAsNewCloud"))).addClass('SaveAsNewCloudDisabled');
    }
};

function findParentElement2(node, layer, attrName) {
    layer--;
    var result = null;
    if (layer < 10) {
        if (node.tagName.toLowerCase() != "body" && node.nodeName.toLowerCase() != "body") {
            if (node.getAttribute(attrName))
                return node;
            else {
                result = findParentElement(node.parentNode, layer, attrName);
            }
        }
        else return result;
    }
    return result;
};

var recIterationForDropDown = 0;

function recForDropDown(node) {
    var currentNode = node;
    if (recIterationForDropDown < 10) {
        if (node && node.tagName && node.tagName.toLowerCase() != "body" && node.nodeName.toLowerCase() != "body") {
            if (node.getAttribute("isDropAttribute"))
                return node;
            else {
                recIterationForDropDown++;
                currentNode = recForDropDown(node.parentNode);
            }
        }
        else return currentNode;
    }
    return currentNode;
};


function capitalizeString(value)
{
    if (value == null || value == "") return "";
    var resultString = "";
    var values = value.toString().toLowerCase().split(" ");
    for (var s in values)
    {
        resultString += uppercaseFirst(values[s]) + " ";
    }
    return resultString;
}

function uppercaseFirst(s)
{
    if (s == null || s == "") return "";
    return s[0].toString().toUpperCase() + s.toString().substr(1, s.toString().length - 1);
}

if (typeof String.prototype.trim !== 'function') {
    String.prototype.trim = function () {
        return this.replace(/^\s+|\s+$/g, '');
    }
}

function getClientID (name) {
    return this.Element.id + '_' + name;
};



function GetUrlForResource (resource) {
    var url = document.location.toString();
    //url = "http://localhost:52878/reportbuilderuitestpage.aspx";
    //url = "http://localhost:52878/";
    //url = "http://iisdev/reportbuilder";
    //url = "http://iisdev/reportbuilder/";
    //url = "http://iisdev/reportbuilder/reportbuilderuitestpage.aspx";
    //url = "http://iisdev/reportbuildersl/";
    //url = "http://iisdev/reportbuildersl/reportbuilderuitestpage.aspx";
    //url = "https://apps.ccmsi.com/reportbuilder";
    //url = "https://apps.ccmsi.com/reportbuilder/reportbuilderuitestpage.aspx";
    //url = "http://localhost:64457/SatelliteReportTestPage.aspx#/FilterBuilder";

    if (url.indexOf("#") > -1) {
        // strip any fragment from the end
        url = url.substring(url.IndexOf("#"), url.length);
    }

    if (url.indexOf(".aspx") > -1 || url.indexOf(".htm") > -1) {
        // strip the page from the end
        url = url.substring(0, url.lastIndexOf("/") + 1);
    }

    // add a slash if necessary
    var separator = '';
    if (url.substr(-1) !== "/") {
        separator = "/";
    }

    url = url + separator + resource;
    return url;
}