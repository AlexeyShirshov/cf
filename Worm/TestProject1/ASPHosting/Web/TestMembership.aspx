<%@ Assembly Name="Worm.Orm" %>
<%@ Import Namespace="Worm.Orm" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN" "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head>
    <title>Test simple page</title>
    <script runat="server" language="VB">
        
        Public Function GetTime() As Date
            Using mgr As OrmReadOnlyDBManager = CreateDBManager()
                Return Date.Now
            End Using
        End Function
        
        Public Function CreateDBManager() As OrmReadOnlyDBManager
            Return New OrmReadOnlyDBManager(New Worm.Orm.OrmCache, New Worm.Orm.DbSchema("1"), "Data Source=vs2\sqlmain;Integrated Security=true;Initial Catalog=test;")
        End Function
    
        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs)
            If Request.QueryString.ToString = "GetAllUsers" Then
                Dim users As MembershipUserCollection = Membership.GetAllUsers
            ElseIf Request.QueryString.ToString = "FindUsersByName" Then
                Dim users As MembershipUserCollection = Membership.FindUsersByName("1%")
            ElseIf Request.QueryString.ToString = "GetNumberOfUsersOnline" Then
                Membership.GetNumberOfUsersOnline()
            ElseIf Request.QueryString.ToString = "GetUser" Then
                Dim u As MembershipUser = Membership.GetUser()
            ElseIf Request.QueryString.ToString = "GetUserByEmail" Then
                Dim s As String = Membership.GetUserNameByEmail("12ef2363-7ec6-4bce-93f7-1a8fe6a74cd8")
            ElseIf Request.QueryString.ToString = "UpdateUser" Then
                Dim u As MembershipUser = Membership.GetUser("12ef2363-7ec6-4bce-93f7-1a8fe6a74cd8")
                u.Comment = "asqwef89bh3e"
                Membership.UpdateUser(u)
            ElseIf Request.QueryString.ToString = "CreateUser" Then
                Dim u As MembershipUser = Membership.CreateUser("asdf", "sadcnskojcn", "uibsdcfbnwf@fg.ru")
                Dim s As String = u.Email
                If s <> u.UserName Then
                    Throw New ApplicationException
                End If
                
                Dim b As Boolean = Membership.ValidateUser("uibsdcfbnwf@fg.ru", "sadcnskojcn")
                If Not b Then
                    Throw New ApplicationException
                End If
                
                b = Membership.ValidateUser("uibsdcfbnwf@fg.ru", "143g90nmsdrl;gm")
                If b Then
                    Throw New ApplicationException
                End If

                Membership.DeleteUser("uibsdcfbnwf@fg.ru", False)
            ElseIf Request.QueryString.ToString = "ResetPassword" Then
                Dim u As MembershipUser = Membership.GetUser("12ef2363-7ec6-4bce-93f7-1a8fe6a74cd8")
                lblInfo.Text = u.ResetPassword()
            ElseIf Request.QueryString.ToString = "ValidatePassword" Then
                Membership.ValidateUser("a@a.ru", "iqer")
                Membership.ValidateUser("a@a.ru", "iqer")
                Membership.ValidateUser("a@a.ru", "iqer")
                Membership.ValidateUser("a@a.ru", "iqer")
                Membership.ValidateUser("a@a.ru", "iqer")
                Membership.ValidateUser("a@a.ru", "iqer")
                Dim u As MembershipUser = Membership.GetUser("a@a.ru")
                If Not u.IsLockedOut Then
                    Throw New ApplicationException
                End If
                
                u.UnlockUser()
            End If
        End Sub
</script>
</head>
<body>
    <%=GetTime() & "  test membership ok"%>
    <asp:Label runat="server" id="lblInfo" />
</body>
</html>
