<%@ Page  Language="vb" AutoEventWireup="false" CodeBehind="WebSignOn.aspx.vb" Inherits="WebApplication.WebSignOn" meta:resourcekey="PageResource1" %>
<%@ Register Assembly="BotDetect" Namespace="BotDetect.Web.UI" TagPrefix="BotDetect" %>
<!DOCTYPE html>
<head id="Head1" runat="server">
    <title>Care 7.0 Login Page</title>
    <script type="text/javascript" src="WebScript2.js?20230330"></script>  
	 <link rel="icon" type="image/x-icon" href="favicon.ico"/>  
    <style type="text/css">
                
        * { margin: 0; padding: 0; }

        body {
	        border-top-width: 100px;
	        border-top-style: solid;
	        border-top-color: #fff;
	        font: 8pt "Lucida Grande", Verdana;
	        color: #000000;
	        background: #fff;
	        
        }
        
        .TextBox
        {
            font: 8pt "Lucida Grande", Verdana;
            border-top: 1px solid #aeaeae;
            border-left: 1px solid #bbbbbb;
            border-right: 1px solid #bbbbbb;
            border-bottom: 1px solid #bbbbbb;
            width: 100px;
            vertical-align: middle;
            height:18px;
            padding-top:3px;
        }
        .MasterTextMessage
        {
            font: 8pt "Lucida Grande", Verdana;
            color: Red;
        }
        .auto-style1 {
            width: 40%;
            height: 45px;
        }
        .auto-style2 {
            width: 20%;
            height: 45px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
          <asp:ScriptManager ID="ScriptManager2" runat="server">
                     <Services>
                        <asp:ServiceReference Path="~/CallwebServiceMethods.asmx"  />
                    </Services>
                    <Scripts>
                        <asp:ScriptReference Path="~/CallwebServiceMethods2.js?20230330" />
                        <asp:ScriptReference Path="~/Scripts/AjaxControlToolkit/Bundle" />
                    </Scripts> 
                </asp:ScriptManager>
        <div align="center">
            <img src="Img/LOGO CARE LOGIN.jpg" alt="CARE TECHNOLOGIES" width = "500px"/>
            <asp:Panel ID="PnlLogin" runat="server" Width="500px" 
                meta:resourcekey="PnlLoginResource1">
                <table style="width:100%; border-top: solid 1px #EAEAEA; border-bottom: solid 1px #EAEAEA;">
                    <tr>
                        <td style="text-align:left; " class="auto-style1">
                            <div><asp:Label ID="LblUserID" runat="server" Text="User ID" Height="18px" meta:resourcekey="LblUserIDResource1" /></div>
                            <div><asp:TextBox ID="TxtUserID" runat="server" CssClass="TextBox" Width="95%" meta:resourcekey="TxtUserIDResource1" AutoCompleteType="None" MaxLength="50" /></div>
                                <%--<ajaxToolkit:FilteredTextBoxExtender ID="FilteredTextBoxExtender2" runat="server" FilterType="Custom, Numbers, UppercaseLetters, LowercaseLetters" TargetControlID="TxtUserID" ValidChars ="-_()\ .@" Enabled="True" />--%>
                        </td>
                        <td style="text-align:left; " class="auto-style1">
                            <div><asp:Label ID="LblUserPassWd" runat="server" Text="Password" Height="18px" meta:resourcekey="LblUserPassWdResource1" /></div>
                            <div><asp:TextBox AutoComplete="off" ID="TxtUserPassWd" runat="server" CssClass="TextBox" TextMode="Password" Width="95%" meta:resourcekey="TxtUserPassWdResource1" AutoCompleteType="None" MaxLength="510"/></div>
                                <%--<ajaxToolkit:FilteredTextBoxExtender ID="FilteredTextBoxExtender3" runat="server" FilterType="Custom, Numbers, UppercaseLetters, LowercaseLetters" TargetControlID="TxtUserPassWd" ValidChars ="-_()\ .@" Enabled="True" />--%>
                        </td>
                        <td style="text-align:center; padding-top:18px; " class="auto-style2">
                            <asp:Button ID="BtnSignOn" runat="server" Text="Login" Font-Names="Calibri" Font-Size="10pt" Height="25px" Width="95%" meta:resourcekey="BtnSignOnResource1" onclientclick="Updating()" />          
                        </td>
                    </tr>
                     <tr align="center">
                        <td colspan="3"> <asp:CheckBox ID="CBXLDAPF" runat="server" Text ="  Integrated Security" AutoPostBack="true" /></td>
                  
                    <tr  align="center">
                        <td colspan="3"><asp:LinkButton ID="LnkBtnSSO" runat="server" Text="Login With SSO" ToolTip="Click this button to login Application with SSO account"  />&nbsp;</td>
                    </tr>
                    <tr align="center">
                        <td colspan="3">
                            <asp:LinkButton ID="LnkBtnForgPass" runat="server" Text="Forgot Password?" ToolTip="Click this button to send your password to your email account" meta:resourcekey="LnkBtnForgPassResource1" />
                        </td>
                    </tr>
               </table>

  <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" >
                    <ContentTemplate>
                        <asp:MultiView ID="MVCaptcha" runat="server">
                             <asp:View ID="ViewCaptcha" runat="server">
                        <table border="0" cellpadding="3" cellspacing="0">
                           <tr align="center">
                                <td>
                                    <BotDetect:WebFormsCaptcha ID="ExampleCaptcha" runat="server" />
                                    
                                    
                                </td>
                            </tr>
                                <tr align="center">
                                    <td>
                                    &nbsp;
                                    </td>
                                </tr>
                            </tr>
                                <tr align="center">
                                    <td>
                                    <asp:TextBox ID="CaptchaCodeTextBox" runat="server" CssClass="TextBox"/>
                                    </td>
                                </tr>
                            
                        </table>
                                 </asp:View>
                             </asp:MultiView>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </asp:Panel>
        </div>
        
        <div align="center">
        <asp:UpdatePanel ID="UP_SignOnAttempt" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:MultiView ID="MVSignOnAttempt" runat="server">
                    <asp:View ID="ViewSignOnAttempt" runat="server">
                        <table style="width:100%;">
                            <tr>
                                <td>&nbsp;</td>
                            </tr>
                            <tr>
                                <td class="MasterTextMessage" style="background-color:#FFFFFF;">
                                    WARNING! You have failed multiple times to log in,Your Account will be locked if You fail once more. Proceed Anyway?
                                    &nbsp;
                                    <asp:LinkButton ID="BtnYesDisplay" runat="server" Text="Yes" OnClientClick="Updating()" meta:resourcekey="BtnYesDisplayResource1" />
                                    &nbsp;
                                    <asp:LinkButton ID="BtnNoDisplay" runat="server" Text="No" 
                                        OnClientClick="Updating()" meta:resourcekey="BtnNoDisplayResource1" />
                                </td>
                            </tr>
                        </table>
                    </asp:View>
                </asp:MultiView>
            </ContentTemplate> 
        </asp:UpdatePanel>
    </div>
        
        <table style="width:100%;">
            <tr>
                <td style="text-align:center;">
                    <asp:UpdatePanel ID="UPErrPassword" runat="server" UpdateMode="Conditional" >
                        <ContentTemplate>
                            <asp:Label ID="ErrPassword" runat="server" ForeColor="Red" Font-Size="8pt" 
                                meta:resourcekey="ErrPasswordResource1"></asp:Label>
                        </ContentTemplate>
                    </asp:UpdatePanel>      
                </td>
            </tr>
        </table>
        
        <asp:UpdatePanel ID="UP_Banner" runat="server" UpdateMode="Conditional" >
            <ContentTemplate>
                <table style="width:100%;">
                    <tr>
                        <td style="text-align:center;">
                            <asp:TextBox ID="txtbanner" runat="server" CssClass="TextBox" Width="500px" Height="100px" TextMode="MultiLine" enabled="false" />
                        </td>
                    </tr>
                </table>
            </ContentTemplate>
        </asp:UpdatePanel> 
                        
        <table style="width:100%;">
            <tr>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td style="text-align:center; font-family: Lucida Grande, Verdana; font-size: 8pt;">
                    <asp:Label ID="LblCopyRight" runat="server" 
                        Text="Copyright © PT. CARE Technologies 2025. All Rights Reserved" 
                        Height="18px" meta:resourcekey="LblCopyRightResource1"  />
                </td>
            </tr>                
        </table>
    </form>
</body>
