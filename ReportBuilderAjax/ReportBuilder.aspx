<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReportBuilder.aspx.cs" Inherits="ReportBuilderAjax.Web._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
   
    <title>ReportBuilderNew</title>    
    <script type="text/javascript">
        var beforeunloadCallCount = 0;  
    </script>
    
	<script type="text/javascript" src="js/Consts.js"></script>
    <script type="text/javascript" src="js/Common/json2.js"></script>
	<script type="text/javascript" src="js/Common/jquery-1.3.2.js"></script>
    <script type="text/javascript" src="js/Common/jquery.maskedinput-1.2.1.js"></script>
    <script type="text/javascript" src="js/Common/jquery.maskMoney.js"></script>
    <script type="text/javascript" src="js/Common/jquery.formatCurrency.js"></script>    
    <script type="text/javascript" src="js/Common/jquery.block.ui.js"></script>
    <script type="text/javascript" src="js/Common/jquery-ui-1.7.3.custom.min.js"></script>
    <script type="text/javascript" src="js/Common/jquery.ui.datepicker.min.js"></script>
    <script type="text/javascript" src="js/Common/jquery-ui-timepicker-addon.js"></script>
    <script type="text/javascript" src="js/Common/si.files.js"></script>
    
    <script type="text/javascript" src="js/Common/Plupload/gears_init.js"></script>
    <script type="text/javascript" src="js/Common/Plupload/plupload.full.min.js"></script>    
    <script type="text/javascript" src="js/Common/Plupload/jquery.plupload.queue.min.js"></script>
    <script type="text/javascript" src="js/Common/jquery.dataTables.js"></script> 
    <script type="text/javascript" src="js/Common/KeyTable.js"></script>  
    
    <script type="text/javascript">
        var isIeButNot8 = $.browser.msie && $.browser.version.split('.')[0] < 8;
        var isWebKit = $.browser.safari;
    </script>
    
	<script type="text/javascript" src="js/Common/RegionPlugin.js"></script>
    <script type="text/javascript" src="js/Common/Common.js"></script>	            
    <script type="text/javascript" src="js/Common/Timer.js"></script>    
    <script type="text/javascript" src="js/Common/TemplateEngine.js"></script>
    <script type="text/javascript" src="js/Common/TemplateManager.js"></script>
    <script type="text/javascript" src="js/Common/Autocomplete.js"></script>
    <script type="text/javascript" src="js/Common/jquery.extend.centerElement.js"></script>
    <script type="text/javascript" src="js/Common/setFrameSizeByContent.js"></script>        

    <script src="js/Common/ControlLibrary/ClassInheritance.js" type="text/javascript"></script>
    <script src="js/Common/ControlLibrary/ClassLibraryInternalTypes.js" type="text/javascript"></script>    
    <script src="js/Common/ControlLibrary/EventFunctionSetClass.js" type="text/javascript"></script>    
    <script src="js/Common/ControlLibrary/ControlAbstractClass.js" type="text/javascript"></script>
    <script src="js/Common/ControlLibrary/StaticDomElement.js" type="text/javascript"></script>
    <script src="js/Common/ControlLibrary/ControlAbstractClassPrototypeCustomize.js" type="text/javascript"></script>
    <script src="js/Common/ControlLibrary/TextBoxControl.js" type="text/javascript"></script>
    <script src="js/Common/ControlLibrary/DropDownControl.js" type="text/javascript"></script>
    <script src="js/Common/ControlLibrary/newDropDownControl.js" type="text/javascript"></script>
    <!--script src="js/Common/ControlLibrary/newDropDownWithCheckBoxControl.js" type="text/javascript"></script-->
    <script src="js/Common/ControlLibrary/CheckBoxControl.js" type="text/javascript"></script>
    <script src="js/Common/ControlLibrary/newCheckBoxControl.js" type="text/javascript"></script>
    <script src="js/Common/ControlLibrary/CheckBoxSetControl.js" type="text/javascript"></script>
    <script src="js/Common/ControlLibrary/RadioButtonControl.js" type="text/javascript"></script>
    <script src="js/Common/ControlLibrary/RadioButtonSetControl.js" type="text/javascript"></script>
    <script src="js/Common/ControlLibrary/TextAreaControl.js" type="text/javascript"></script>
    <script src="js/Common/ControlLibrary/StaticControl.js" type="text/javascript"></script>
    <script src="js/Common/ControlLibrary/DinamicRadioButtonSetControl.js" type="text/javascript"></script>
    <script src="js/Common/ControlLibrary/CalendarControl.js" type="text/javascript"></script>
    <script src="js/Common/ControlLibrary/MultiSelectComboBox.js" type="text/javascript"></script>
    <script src="js/Common/ControlLibrary/MultiRadioButton.js" type="text/javascript"></script>
    <script src="js/Common/ControlLibrary/DeliveryMethodRadioButton.js" type="text/javascript"></script>
    <script src="js/Common/ControlLibrary/DateTimeControl.js" type="text/javascript"></script>
    
    <script type="text/javascript" src="js/Common/CrossBrowseElement.js"></script> 
                
    <script src="js/Common/ControlLibrary/DateRangeControl.js" type="text/javascript"></script>
    <script src="js/Common/ControlLibrary/AsOfDateControl.js" type="text/javascript"></script>
    <script src="js/ClientAnalysisControl.js" type="text/javascript"></script>
    <script src="js/HierarchyControl.js" type="text/javascript"></script>
    <script src="js/FilterDataManager.js" type="text/javascript"></script>
    <script src="js/FilterBase.js" type="text/javascript"></script>
    <script src="js/Filters/LossRunSummaryFilter.js" type="text/javascript"></script>
    <script type="text/javascript" src='js/ReportBuilderHeader.js'></script> 
    <script type="text/javascript" src='js/ReleaseNotes.js'></script> 
    <script type="text/javascript" src='js/ReportBuilderForm.js'></script> 
    <script type="text/javascript" src='js/ReportSelectForm.js'></script>
    <script type="text/javascript" src='js/ReportTypes.js'></script>
    <script type="text/javascript" src='js/DeliveredReports.js'></script>
    <script type="text/javascript" src='js/CheckpointsConfiguration.js'></script>
    <script type="text/javascript" src='js/ReportCustomLogo.js'></script>
    <script type="text/javascript" src='js/ReportTabManager.js'></script> 
    <script type="text/javascript" src='js/ReportLayout.js'></script> 
    <script type="text/javascript" src='js/ViewReport.js'></script>
    <script type="text/javascript" src='js/ShedulerControl.js'></script> 
    <script type="text/javascript" src='js/ScheduleRecipientsFilter.js'></script> 
    <script type="text/javascript" src='js/ReportLayoutColumns.js'></script> 
    <script type="text/javascript" src='js/ReportLayoutSorting.js'></script> 
    <script type="text/javascript" src='js/ReportLayoutGrouping.js'></script>
    <script type="text/javascript" src='js/Common/ControlLibrary/CustomDropDown.js'></script> 

    <script src="js/TabsControlManager.js" type="text/javascript"></script>
    <script src="js/DragDropFields.js" type="text/javascript"></script>
    <script src="js/SummarizeFields.js" type="text/javascript"></script>
    <script src="js/ActivityControl.js" type="text/javascript"></script>

    <link href="Style.css" rel="stylesheet" type="text/css" />    
    <link href="css/ui-lightness/jquery-ui-1.8.16.custom.css" rel="stylesheet" type="text/css" />
    <link href="css/ui-lightness/jquery.ui.datepicker.css" rel="stylesheet" type="text/css" />
    <link href="css/ui-lightness/jquery-ui-timepicker-addon.css" rel="stylesheet" type="text/css" />

</head>
<body style="height:100%;min-height:768px; background-image: url(./images/mainBg.gif);" class="bg">
<form id="form1" runat="server">
    <div style="display:none;">
        <asp:Button ID="signOutButton" runat="server" Text="Sign Out" OnClick="SingOutButton_Click"/>
    </div>
</form>
    <div id="testtabs"></div>
    <div style="width: 100%;" align="center" >
        <table cellpadding="0" cellspacing="0">
        <tr>
            <td style="background-image: url(./images/leftBg.png); background-repeat: repeat-y;width: 15px;">
                <div></div>
            </td>
            <td valign="top">
                <div id="main_div" align="left" style="width:984px; margin-left:auto; margin-right:auto; display:block; background-color: white;"></div>
            </td>
            <td style="background-image: url(./images/rightBg.png);  background-repeat: repeat-y;width: 15px;">
                <div></div>
            </td>
        </tr>
        </table>
        <div id="blockDisplay" style="position:absolute; display:none;  width: 100%; height:100%; z-index:1000; background-color:#000000; opacity: 0.6;top: 0px;left:0px;"></div>
    </div>
</body>
</html>
<script type="text/javascript">
    var ASP_LOGOUT_BUTTON_ID = '<%=signOutButton.ClientID%>';

    $(document).bind("click", function (e) {
        HideAllDropDown(e);
        HideSaveCloud(e);
        HideSaveAsNewCloud(e);
        RightTabHeight();
    });

    $(document).ajaxComplete(function () {
        RightTabHeight();
    });
    
    if (typeof Consts != undefined) {
        if (typeof Consts.HANDLERS != undefined) {
            Consts.HANDLERS.SEARCH_HANDLER = 'HttpHandlers/ReportBuilderHandler.ashx';
        };
    };

    var jsDebug = true;
    var allowDevFrameResize = true;
    var RBH = null;
    function onTemplatesReady() {
        document.getElementById('main_div').innerHTML = "";
        RBH = new ReportBuilderHeader('main_div');
        RBH.Load();
    };
    
    $(document).ready(function(){ 
        TemplateManager.registrateAll(onTemplatesReady);
        $(document.body).keyup(bodyKeyPress);
    });

    function beforeunload(){
        
    }
    
    function bodyKeyPress(e){
        
    }

    if($.browser.msie || $.browser.mozilla || $.browser.safari){
        window.onbeforeunload = beforeunload;        
    }else{
        alert("Your browser may not work correctly with this program.  Please use a recent version of Internet Explorer for the best experience.");
        window.onunload = beforeunload;
    }
</script>