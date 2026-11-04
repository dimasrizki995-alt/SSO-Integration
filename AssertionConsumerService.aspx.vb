Imports System
Imports System.Collections.Generic
Imports System.Net
Imports System.Web.Security
Imports ComponentSpace.SAML2

Namespace CareWebApplication.SAML

    Partial Public Class AssertionConsumerService
        Inherits System.Web.UI.Page
        Public Const AttributesSessionKey As String = "saml-attributes"
        Dim userName As String = Nothing
        Dim Attribute() As ComponentSpace.SAML2.Assertions.SAMLAttribute = Nothing
        Dim targetUrl As String = Nothing
        Dim partnerIdP As String = Nothing
        Dim isInResponseToF As Boolean = False
        Dim authnContext As String = Nothing

        Dim UserID As String
        Dim Password As String
        Dim ErrMsg As String = ""

        Protected Sub Page_Load(sender As Object, e As System.EventArgs)
            Dim ws As New localhost.DataAccess(GetAppSetting("CareWebService1"))

            'Receive and process the SAML assertion contained in the SAML response.
            'The SAML response is received either as part of IdP-initiated or SP-initiated SSO.
            SAMLServiceProvider.ReceiveSSO(Page.Request, isInResponseToF, partnerIdP, authnContext, userName, Attribute, targetUrl)

            'Session(AttributesSessionKey) = Attribute
            If targetUrl Is Nothing Then
                If CBool(toval(GetAppSetting("SSOOktaF"))) Then
                    targetUrl = tostr(GetAppSetting("targetUrlOKTA"))
                ElseIf CBool(toval(GetAppSetting("SSOAzureF"))) Then
                    targetUrl = tostr(GetAppSetting("targetUrlAzure"))
                ElseIf CBool(toval(GetAppSetting("SSOOneLoginF"))) Then
                    targetUrl = tostr(GetAppSetting("targetUrlOneLogin"))
                End If
            End If

            If userName = "" Then
                Session("Errmsg") = "Please Check Configuration Single Signon (IdP)  !"
                Response.Redirect("Errors.aspx", True) : Exit Sub
            Else
                If CBool(toval(GetAppSetting("SSOOktaF"))) Or CBool(toval(GetAppSetting("SSOAzureF"))) Or CBool(toval(GetAppSetting("SSOOneLoginF"))) Then
                    Dim sql As String
                    Dim dataset As DataSet = Nothing
                    UserID = userName
                    sql = "select ID from SysUser where Email = '" & tostr(userName, , False) & "'"
                    If ws.GetDataSet(UserID, Password, dataset, sql, ErrMsg) Then
                        If dataset.Tables(0).Rows.Count <> 0 Then
                            Dim ID As String = tostr(dataset.Tables(0).Rows(0)("ID"), , False)
                            UserID = ID
                        End If
                    End If
                End If


                If ws.GetAuthorize(UserID, Password, Request.UserHostName, False, ErrMsg) Then
                    Dim CookiesUserID As String
                    If Me.Request.Cookies(CookiesName) IsNot Nothing Then
                        CookiesUserID = GetCookiesUserID(Me.Request.Cookies(CookiesName).Value)
                    Else
                        CookiesUserID = ""
                    End If

                    If CookiesUserID.ToUpper = UserID.ToUpper Or CookiesUserID = "" Then
                        Session("UserID") = UserID
                        Session("Password") = Password

                        AddUserPreferencesCookies(Me, "CAREAPP_UserID", GenerateEncryptionKey(Me))
                        AddUserPreferencesCookies(Me, "CAREAPP_Password", Encrypt(Password, GenerateEncryptionKey(Me)))
                    Else
                        ' Not Yet Log-Off - Rejected
                        Session("Errmsg") = "User " & CookiesUserID & " is not yet Logged Off !"
                        Response.Redirect("Errors.aspx", True) : Exit Sub
                    End If


                    Response.Redirect(targetUrl, False)
                End If
            End If





            If ErrMsg <> "" Then
                Session("Errmsg") = ErrMsg
                Response.Redirect("Errors.aspx", True) : Exit Sub
            End If

        End Sub
    End Class


End Namespace


