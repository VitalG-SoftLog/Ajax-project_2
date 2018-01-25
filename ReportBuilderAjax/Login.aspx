<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="ReportBuilderAjax.Web.Login" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    
    <title></title>
	<script type="text/javascript" src="LoginJS/jquery-1.3.2.js"></script>
    <script type="text/javascript" src="LoginJS/jquery.watermark.min.js"></script>
    <link href="LoginJS/LoginStyle.css" rel="stylesheet" type="text/css" /> 
  
    <script type="text/javascript">
        $(document).ready(function () {
            $("#" + "<%=loginTextBox.ClientID %>").watermark('                   User Name', { className: 'watermark' });
            $("#" + "<%=passTextBox.ClientID %>").watermark('                    Password', { className: 'watermark' });
        }); 
    </script>

</head>
<body style="height:100%; background-image: url(./images/mainBg.gif);" >
<form id="form1" method="post" runat="server">
    <div style="width: 476px; height:305px; margin-left:35%;margin-top: 18%; background-image: url(./images/login_background.png);">
        <table cellpadding="0" cellspacing="0">
            <tr>
                <td>
                    <div style="width: 141px; height:33px;background-image: url(./images/login_Logo.png); margin-left:170px;margin-top: 55px;"></div>
                </td>
            </tr>
            <tr>
                <td>
                    <div style="width: 243px;height: 17px;"></div>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Panel Visible="False" ID="ErrorMessage" runat="server" style="width: 243px; height:30px; border: none; margin-left:119px;
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                background-image: url(./images/login_error.png); background-repeat:no-repeat; 
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                background-color: transparent;"></asp:Panel>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:TextBox runat="server" ID="loginTextBox"  style="width: 224px; height:28px; border: none; margin-left:119px;
                         margin-top:5px; background-image: url(./images/login_input.png); background-repeat:no-repeat;background-color:
                          transparent;padding-left: 10px;padding-right: 10px;"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:TextBox runat="server" TextMode="Password" id="passTextBox" style="width: 224px; height:28px; border: none;
                         margin-left:119px; margin-top:7px;background-image: url(./images/login_input.png); background-repeat:no-repeat;
                          background-color: transparent;padding-left: 10px;padding-right: 10px;"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Button runat="server" ID="singInButton" OnClick="SingInButton_Click" style="width: 97px; height:29px; 
                         margin-left:193px; margin-top:20px; background-image: url(./images/login_button_sign.png);background-color: 
                         transparent;border: none;">
                    </asp:Button>
                </td>
            </tr>
        </table>
    </div>
</form>
</body>
</html>
