Imports ComponentSpace.SAML2
Imports System.DirectoryServices.Protocols
Imports System.Net


Partial Public Class WebSignOn
    Inherits System.Web.UI.Page
    Dim UserID As String
    Dim Password As String
    Dim ErrMsg As String = ""
    Dim WI As System.Security.Principal.WindowsIdentity
    Dim WIC As System.Security.Principal.WindowsImpersonationContext

    Private Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
        If CType(toval(GetAppSetting("dbTrusted")), Boolean) Then
            WI = CType(HttpContext.Current.User.Identity, System.Security.Principal.WindowsIdentity)
            WIC = WI.Impersonate
        End If
    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        If Not IsPostBack Then

            LnkBtnForgPass.Enabled = True
            InitializeAttributes()
            If CType(toval(GetAppSetting("dbTrusted")), Boolean) Then
                If TxtUserID.Text = "" Then TxtUserID.Text = HttpContext.Current.User.Identity.Name
                TxtUserID.Enabled = False
                TxtUserPassWd.Enabled = False
            End If


            If CType(toval(GetAppSetting("DefaultLDAPF")), Boolean) Then
                If TxtUserID.Text = "" Then TxtUserID.Text = HttpContext.Current.User.Identity.Name
                TxtUserID.Enabled = False
                TxtUserPassWd.Enabled = False
                LnkBtnForgPass.Enabled = False
                CBXLDAPF.Checked = True
                CBXLDAPF.Enabled = False
            End If

            TxtUserID.Focus()

            If toval(GetAppSetting("UseBannerF")) Then
                ProcessTableFile(txtbanner.Text, ErrMsg)
            Else
                UP_Banner.Visible = False
            End If
        End If

        'If CType(toval(GetAppSetting("dbTrusted")), Boolean) Then
        '    'TxtUserID.Text = System.Security.Principal.WindowsIdentity.GetCurrent.Name
        '    TxtUserID.Text = HttpContext.Current.User.Identity.Name
        '    TxtUserID.Enabled = False
        '    TxtUserPassWd.Enabled = False
        '    LnkBtnForgPass.Enabled = False
        'Else
        '    LnkBtnForgPass.Enabled = True
        '    LnkBtnForgPass.OnClientClick = "OpenForm('forgotpwd.aspx','');"
        'End If

        If CBool(toval(GetAppSetting("SSOOktaF"))) Or CBool(toval(GetAppSetting("SSOAzureF"))) Or CBool(toval(GetAppSetting("SSOOneLoginF"))) Then
            TxtUserID.Enabled = False
            TxtUserPassWd.Enabled = False
            LnkBtnForgPass.Enabled = False
            LnkBtnForgPass.OnClientClick = ""
            BtnSignOn.Enabled = False
            LnkBtnSSO.Enabled = True
            CBXLDAPF.Enabled = False
            LnkBtnSSO.Visible = True
            Me.MVCaptcha.ActiveViewIndex = -1

        Else
            LnkBtnSSO.Enabled = False
            LnkBtnSSO.Visible = False
            LnkBtnSSO.OnClientClick = ""
            ExampleCaptcha.Visible = True
            Me.MVCaptcha.ActiveViewIndex = 0
        End If

        If CBool(toval(GetAppSetting("LoginCaptchaF"))) Then
            Me.MVCaptcha.ActiveViewIndex = 0
        Else
            Me.MVCaptcha.ActiveViewIndex = -1
        End If

        TxtUserID.Focus()

        If toval(GetAppSetting("UseBannerF")) Then
            ProcessTableFile(txtbanner.Text, ErrMsg)
        Else
            UP_Banner.Visible = False
        End If
    End Sub
    Private Sub InitializeAttributes()
        CBXLDAPF.Attributes.Add("onclick", "doPostBack('" & CBXLDAPF.ClientID & "')")
    End Sub

    Private Function ProcessTableFile(ByVal FileName As String, ByRef ErrMsg As String) As Boolean
        Try
            Dim FileBanner As String = ValidateBanner("~/BANNER.txt")

            Dim ObjReader As New System.IO.StreamReader(FileBanner)
            Dim Banner As String = ObjReader.ReadToEnd

            txtbanner.Text = Banner

            ObjReader.Dispose()

        Catch ex As Exception
            ErrMsg = ex.Message
            Return Nothing
        End Try
    End Function

    Protected Sub BtnSignOn_Click(ByVal sender As Object, ByVal e As EventArgs) Handles BtnSignOn.Click
        Try
            Dim LoadBalancerF As Boolean = CBool(toval(GetAppSetting("LoadBalancerF")))
            Dim userHostname As String
            Dim IPAddress As String()

            If LoadBalancerF Then
                userHostname = Request.Headers("X-Forwarded-For")
                If (String.IsNullOrEmpty(userHostname)) Then userHostname = Request.UserHostAddress
                If InStr(userHostname, ":") <> 0 Then
                    IPAddress = userHostname.Split(":")
                    userHostname = IPAddress(0)
                End If
            Else
                userHostname = Request.UserHostName
            End If

            If CBool(toval(GetAppSetting("LoginCaptchaF"))) Then
                If ExampleCaptcha.Validate(CaptchaCodeTextBox.Text) Then
                    Dim ws As New localhost.DataAccess(GetAppSetting("CareWebService1"))
                    Dim ForceToChangeF As Boolean = False
                    Dim StrMsg As String = ""
                    Dim findex() As String = Nothing
                    Dim fbuffer() As String = Nothing
                    Dim SignOnAttempt As Integer
                    Dim AttemptNum As Integer
                    Dim SQL As String


                    ws.Timeout = System.Threading.Timeout.Infinite

                        If Trim(TxtUserID.Text) = "" Then
                            ErrPassword.Text = "Invalid User ID/Password !"
                            TxtUserID.Focus()
                            Exit Try
                        End If

                        'If Trim(TxtUserPassWd.Text) = "" Then
                        '    ErrPassword.Text = "Invalid User ID/Password !"
                        '    TxtUserPassWd.Focus()
                        '    Exit Try
                        'End If

                        UserID = tostr(Trim(TxtUserID.Text))
                        'Password = tostr(Trim(TxtUserPassWd.Text))
                        Password = TxtUserPassWd.Text

                        Me.MVSignOnAttempt.ActiveViewIndex = -1
                        If SignOnAttempt - AttemptNum = 1 And tostr(ViewState("WARNING-OK")) = "" Then
                            ErrPassword.Text = ""
                            Me.MVSignOnAttempt.ActiveViewIndex = 0
                            UP_SignOnAttempt.Update()
                            UPErrPassword.Update()
                            Exit Sub
                        Else

                        If ws.GetAuthorize(UserID, Password, userHostname, toval(CBXLDAPF.Checked), ErrMsg) Then
                            Dim CookiesUserID As String
                            If Me.Request.Cookies(CookiesName) IsNot Nothing Then
                                CookiesUserID = GetCookiesUserID(Me.Request.Cookies(CookiesName).Value)
                            Else
                                CookiesUserID = ""
                            End If

                            If CookiesUserID.ToUpper = UserID.ToUpper Or CookiesUserID = "" Then
                                Session("UserID") = UserID
                                Session("Password") = Password
                                'Session("DBIdentity") = ws.GetServerIdentity(UserID, Password, ErrMsg)

                                AddUserPreferencesCookies(Me, "CAREAPP_UserID", GenerateEncryptionKey(Me))
                                AddUserPreferencesCookies(Me, "CAREAPP_Password", Encrypt(Password, GenerateEncryptionKey(Me)))
                            Else
                                ' Not Yet Log-Off - Rejected
                                Session("ErrMsg") = "User " & CookiesUserID & " is not yet Logged Off !"
                                Response.Redirect("Errors.aspx", True) : Exit Sub
                            End If

                            If ErrMsg <> "" Then
                                Session("Errmsg") = ErrMsg
                            End If

                            If Not CType(toval(GetAppSetting("dbTrusted")), Boolean) And Not CType(toval(GetAppSetting("SSOOktaF")), Boolean) Then
                                If InStr(ErrMsg, "<Cmd-Change Password>") <> 0 Then
                                    Session("PWDErrMsg") = Replace(ErrMsg, "<Cmd-Change Password>", "")
                                    Session("ForceToChangeF") = True
                                    Server.Transfer("ChangePassword.aspx", False)
                                    Exit Sub
                                ElseIf InStr(ErrMsg, "Your Password Will be expired in") <> 0 AndAlso toval(Trim(Replace(Replace(ErrMsg, "Your Password Will be expired in", ""), "day(s) !", ""))) <= 0 Then
                                    Session("PWDErrMsg") = "Password is expired !"
                                    Session("ForceToChangeF") = True
                                    Server.Transfer("ChangePassword.aspx", False)
                                    Exit Sub
                                ElseIf InStr(ErrMsg, "<Cmd-Change OTP>") <> 0 Then
                                    SQL = "select ByPassOTP from get_TimeStampUser('" & UserID & "')"
                                    If ws.GetRecord(UserID, Password, findex, fbuffer, SQL, ErrMsg) Then
                                        If Not toval(GetFieldValue(fbuffer(GetFieldIndex("ByPassOTP", findex)))) Then
                                            Session("PWDErrMsg") = "Require OTP !"
                                            Server.Transfer("OTPVerification.aspx", False)
                                            Exit Sub
                                        End If
                                    End If
                                End If
                            End If

                            Server.Transfer("WebAppMain.aspx", False)
                        Else
                            ErrPassword.Text = ErrMsg

                                'For Pentest MAG dicomment
                                If ErrMsg = "Invalid User ID/Password !" Then
                                    SQL = "select SignOn_Attempt from systab"
                                    If ws.GetRecord(UserID, Password, findex, fbuffer, SQL, ErrMsg) Then
                                        SignOnAttempt = toval(GetFieldValue(fbuffer(GetFieldIndex("SignOn_Attempt", findex))))
                                    End If

                                    SQL = "select attempt_num from SysUserActivity where ID = '" & UserID & "'"
                                    If ws.GetRecord(UserID, Password, findex, fbuffer, SQL, ErrMsg) Then
                                        AttemptNum = toval(GetFieldValue(fbuffer(GetFieldIndex("attempt_num", findex))))
                                    End If

                                    If SignOnAttempt - AttemptNum = 1 Then
                                        ErrPassword.Text = "Invalid User ID/Password – Warning,  You Have Failed " & AttemptNum & " Times, If You Forgot Your Password, Please Click <i>Forgot Password</i>"
                                    ElseIf SignOnAttempt <> 0 And SignOnAttempt = AttemptNum Then
                                        ErrPassword.Text = "Invalid User ID/Password – Login Account is Locked !"
                                    End If
                                End If
                            End If
                        End If
                    Else
                        ExecuteScript(BtnSignOn, "window.alert('Invalid Captcha');")
                    CaptchaCodeTextBox.Text = ""
                    Exit Try
                End If
                UPErrPassword.Update()
            Else
                Dim ws As New localhost.DataAccess(GetAppSetting("CareWebService1"))
                Dim ForceToChangeF As Boolean = False
                Dim StrMsg As String = ""
                Dim findex() As String = Nothing
                Dim fbuffer() As String = Nothing
                Dim SignOnAttempt As Integer
                Dim AttemptNum As Integer
                Dim SQL As String

                ws.Timeout = System.Threading.Timeout.Infinite

                If Trim(TxtUserID.Text) = "" Then
                    ErrPassword.Text = "Invalid User ID/Password !"
                    TxtUserID.Focus()
                    Exit Try
                End If

                'If Trim(TxtUserPassWd.Text) = "" Then
                '    ErrPassword.Text = "Invalid User ID/Password !"
                '    TxtUserPassWd.Focus()
                '    Exit Try
                'End If

                UserID = tostr(Trim(TxtUserID.Text))
                Password = tostr(Trim(TxtUserPassWd.Text))


                Me.MVSignOnAttempt.ActiveViewIndex = -1
                If SignOnAttempt - AttemptNum = 1 And tostr(ViewState("WARNING-OK")) = "" Then
                    ErrPassword.Text = ""
                    Me.MVSignOnAttempt.ActiveViewIndex = 0
                    UP_SignOnAttempt.Update()
                    UPErrPassword.Update()
                    Exit Sub
                Else

                    If ws.GetAuthorize(UserID, Password, userHostname, toval(CBXLDAPF.Checked), ErrMsg) Then
                        Dim CookiesUserID As String
                        If Me.Request.Cookies(CookiesName) IsNot Nothing Then
                            CookiesUserID = GetCookiesUserID(Me.Request.Cookies(CookiesName).Value)
                        Else
                            CookiesUserID = ""
                        End If

                        If CookiesUserID.ToUpper = UserID.ToUpper Or CookiesUserID = "" Then
                            Session("UserID") = UserID
                            Session("Password") = Password
                            'Session("DBIdentity") = ws.GetServerIdentity(UserID, Password, ErrMsg)

                            AddUserPreferencesCookies(Me, "CAREAPP_UserID", GenerateEncryptionKey(Me))
                            AddUserPreferencesCookies(Me, "CAREAPP_Password", Encrypt(Password, GenerateEncryptionKey(Me)))
                        Else
                            ' Not Yet Log-Off - Rejected
                            Session("ErrMsg") = "User " & CookiesUserID & " is not yet Logged Off !"
                            Response.Redirect("Errors.aspx", True) : Exit Sub
                        End If

                        If ErrMsg <> "" Then
                            Session("Errmsg") = ErrMsg
                        End If

                        If Not CType(toval(GetAppSetting("dbTrusted")), Boolean) And Not CType(toval(GetAppSetting("SSOOktaF")), Boolean) And Not CType(toval(GetAppSetting("SSOAzureF")), Boolean) Then
                            If InStr(ErrMsg, "<Cmd-Change Password>") <> 0 Then
                                Session("PWDErrMsg") = Replace(ErrMsg, "<Cmd-Change Password>", "")
                                Session("ForceToChangeF") = True
                                Server.Transfer("ChangePassword.aspx", False)
                                Exit Sub
                            ElseIf InStr(ErrMsg, "Your Password Will be expired in") <> 0 AndAlso toval(Trim(Replace(Replace(ErrMsg, "Your Password Will be expired in", ""), "day(s) !", ""))) <= 0 Then
                                Session("PWDErrMsg") = "Password is expired !"
                                Session("ForceToChangeF") = True
                                Server.Transfer("ChangePassword.aspx", False)
                                Exit Sub
                            ElseIf InStr(ErrMsg, "<Cmd-Change OTP>") <> 0 Then

                                SQL = "select ByPassOTP from get_TimeStampUser('" & UserID & "')"
                                If ws.GetRecord(UserID, Password, findex, fbuffer, SQL, ErrMsg) Then
                                    If Not toval(GetFieldValue(fbuffer(GetFieldIndex("ByPassOTP", findex)))) Then
                                        Session("PWDErrMsg") = "Require OTP !"
                                        Server.Transfer("OTPVerification.aspx", False)
                                        Exit Sub
                                    End If
                                End If

                            End If
                        End If

                        Server.Transfer("WebAppMain.aspx", False)
                    Else
                        ErrPassword.Text = ErrMsg

                        'For Pentest MAG dicomment
                        If ErrMsg = "Invalid User ID/Password !" Then
                            SQL = "select SignOn_Attempt from systab"
                            If ws.GetRecord(UserID, Password, findex, fbuffer, SQL, ErrMsg) Then
                                SignOnAttempt = toval(GetFieldValue(fbuffer(GetFieldIndex("SignOn_Attempt", findex))))
                            End If

                            SQL = "select attempt_num from SysUserActivity where ID = '" & UserID & "'"
                            If ws.GetRecord(UserID, Password, findex, fbuffer, SQL, ErrMsg) Then
                                AttemptNum = toval(GetFieldValue(fbuffer(GetFieldIndex("attempt_num", findex))))
                            End If

                            If SignOnAttempt - AttemptNum = 1 Then
                                ErrPassword.Text = "Invalid User ID/Password – Warning,  You Have Failed " & AttemptNum & " Times, If You Forgot Your Password, Please Click <i>Forgot Password</i>"
                            ElseIf SignOnAttempt <> 0 And SignOnAttempt = AttemptNum Then
                                ErrPassword.Text = "Invalid User ID/Password – Login Account is Locked !"
                            End If
                        End If
                    End If
                End If
            End If
            UPErrPassword.Update()
        Catch ex As Exception
            ErrPassword.Text = ErrMsg
        Finally
            ExecuteScript(Me, "Updated()")
        End Try

    End Sub

    Private Sub BtnYesDisplay_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles BtnYesDisplay.Click
        ViewState("WARNING-OK") = "Ok"
        Me.MVSignOnAttempt.ActiveViewIndex = -1
        ErrPassword.Text = "Please click [Login] button to continue."
        BtnSignOn.Enabled = True
        LnkBtnForgPass.Enabled = True
        UP_SignOnAttempt.Update()
        UPErrPassword.Update()
    End Sub

    Private Sub BtnNoDisplay_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles BtnNoDisplay.Click
        Me.MVSignOnAttempt.ActiveViewIndex = -1
        BtnSignOn.Enabled = True
        LnkBtnForgPass.Enabled = True
        UP_SignOnAttempt.Update()
        UPErrPassword.Update()
    End Sub

    Private Sub LnkBtnForgPass_Click(sender As Object, e As EventArgs) Handles LnkBtnForgPass.Click
        Try
            OpenForm(LnkBtnForgPass, "forgotpwd.aspx")
            
            UPErrPassword.Update()
        Catch ex As Exception
            ErrPassword.Text = ErrMsg
        Finally
            ExecuteScript(Me, "Updated()")
        End Try
    End Sub

    Private Sub CBXLDAPF_CheckedChanged(sender As Object, e As EventArgs) Handles CBXLDAPF.CheckedChanged
        If CBXLDAPF.Checked Then
            TxtUserID.Text = HttpContext.Current.User.Identity.Name
            TxtUserID.Enabled = False
            TxtUserPassWd.Enabled = False
            LnkBtnForgPass.Enabled = False

        Else
            TxtUserID.Enabled = True
            TxtUserPassWd.Enabled = True
            LnkBtnForgPass.Enabled = True
        End If
    End Sub
    Private Sub LnkBtnSSO_Click(sender As Object, e As EventArgs) Handles LnkBtnSSO.Click
        Dim PartnerIdP As String = tostr(GetAppSetting("PartnerIdP"))
        Dim ReturnUrl As String = Request.QueryString("ReturnUrl")
        If CBool(toval(GetAppSetting("SSOOktaF"))) Then
            PartnerIdP = tostr(GetAppSetting("PartnerIdPOKTA"))
        ElseIf CBool(toval(GetAppSetting("SSOAzureF"))) Then
            PartnerIdP = tostr(GetAppSetting("PartnerIdPAzure"))
        ElseIf CBool(toval(GetAppSetting("SSOOneLoginF"))) Then
            PartnerIdP = tostr(GetAppSetting("PartnerIdPOneLogin"))
        End If
        SAMLServiceProvider.InitiateSSO(Page.Response, ReturnUrl, PartnerIdP)
    End Sub


    Function ValidateBanner(ByVal FileDir As String) As String
        Dim FileFullPath As String = Server.MapPath(FileDir)    ' 
        Dim FileType As String = System.IO.Path.GetExtension(FileFullPath).Replace(".", "") ' TXT
        Dim FileName As String = System.IO.Path.GetFileNameWithoutExtension(FileFullPath)   ' abc
        Dim FilePath As String = System.IO.Path.GetDirectoryName(FileFullPath)              ' c:\windows\temp


        ' 2) Check the allowable extension
        Dim AllowedExtensions() As String = {"TXT"}
        Dim AllowedExtension As String = ""

        For Each AllowedExtension In AllowedExtensions
            If FileType.ToUpper = AllowedExtension.ToUpper Then
                Exit For
            End If
        Next

        If FileType.ToUpper <> AllowedExtension.ToUpper Then
            Throw New Exception("Incorrect File Extension !")
        End If

        ' 3) Check whitelist validation for the filename
        If Not ValidateFileDir(FilePath & "\" & FileName & "." & FileType) Then
            Throw New Exception("Incorrect File Path Name !")
        End If
        ValidateBanner = FilePath & "\" & FileName & "." & FileType
    End Function
End Class