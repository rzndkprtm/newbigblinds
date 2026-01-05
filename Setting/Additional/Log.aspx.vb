Imports System.Data.SqlClient

Partial Class Setting_Additional_Log
    Inherits Page

    Dim settingClass As New SettingClass

    Dim myConn As String = ConfigurationManager.ConnectionStrings("DefaultConnection").ConnectionString

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        Dim pageAccess As Boolean = PageAction("Load")
        If pageAccess = False Then
            Response.Redirect("~/additional", False)
            Exit Sub
        End If

        If Not IsPostBack Then
            MessageError(False, String.Empty)
            BindData(txtSearch.Text)
        End If
    End Sub

    Protected Sub btnRefresh_Click(sender As Object, e As EventArgs)
        MessageError(False, String.Empty)
        BindData(txtSearch.Text)
    End Sub

    Protected Sub btnSearch_Click(sender As Object, e As EventArgs)
        MessageError(False, String.Empty)
        BindData(txtSearch.Text)
    End Sub

    Protected Sub BindData(searchText As String)
        Try
            Dim searchString As String = String.Empty

            If Not String.IsNullOrEmpty(searchText) Then
                searchString = "WHERE Logs.Type LIKE '%" & searchText.Trim() & "%'"
            End If

            Dim thisString As String = String.Format("SELECT Logs.*, CustomerLogins.UserName AS ActionName FROM Logs LEFT JOIN CustomerLogins ON Logs.ActionBy=CustomerLogins.Id {0}", searchString)

            gvList.DataSource = settingClass.GetListData(thisString)
            gvList.DataBind()
        Catch ex As Exception
            MessageError(True, ex.ToString())
            If Not Session("RoleName") = "Developer" Then
                MessageError(True, "PLEASE CONTACT IT SUPPORT AT REZA@BIGBLINDS.CO.ID !")
            End If
        End Try
    End Sub

    Protected Sub btnDelete_Click(sender As Object, e As EventArgs)
        MessageError(False, String.Empty)
        Try
            Dim thisId As String = txtIdDelete.Text

            Using thisConn As New SqlConnection(myConn)
                Using myCmd As SqlCommand = New SqlCommand("DELETE FROM Logs WHERE Id=@Id", thisConn)
                    myCmd.Parameters.AddWithValue("@Id", thisId)

                    thisConn.Open()
                    myCmd.ExecuteNonQuery()
                End Using
            End Using

            Response.Redirect("~/setting/additional/log", False)
        Catch ex As Exception
            MessageError(True, ex.ToString())
            If Not Session("RoleName") = "Developer" Then
                MessageError(True, "PLEASE CONTACT IT SUPPORT AT REZA@BIGBLINDS.CO.ID !")
            End If
        End Try
    End Sub

    Protected Function GetDataName(type As String, dataId As String, desc As String) As String
        If Not String.IsNullOrEmpty(type) OrElse Not String.IsNullOrEmpty(dataId) Then
            Dim thisQuery As String = String.Format("SELECT Name FROM {0} WHERE Id={1}", type, dataId)
            If type = "CustomerLogins" Then
                thisQuery = String.Format("SELECT UserName FROM {0} WHERE Id={1}", type, dataId)
            End If
            If type = "OrderHeaders" Then
                thisQuery = String.Format("SELECT OrderId FROM {0} WHERE Id={1}", type, dataId)
            End If
            If type = "OrderDetails" Then
                thisQuery = String.Format("SELECT Id FROM {0} WHERE Id={1}", type, dataId)
            End If

            Dim dataName As String = settingClass.GetItemData(thisQuery)

            Dim thisDes As String = String.Format("{0} -> {1}", dataName, desc)
            Return thisDes
        End If
        Return String.Empty
    End Function

    Protected Sub MessageError(visible As Boolean, message As String)
        divError.Visible = visible : msgError.InnerText = message
    End Sub

    Protected Function PageAction(action As String) As Boolean
        Try
            Dim roleId As String = Session("RoleId").ToString()
            Dim levelId As String = Session("LevelId").ToString()
            Dim actionClass As New ActionClass

            Return actionClass.GetActionAccess(roleId, levelId, Page.Title, action)
        Catch ex As Exception
            Response.Redirect("~/account/login", False)
            HttpContext.Current.ApplicationInstance.CompleteRequest()
            Return False
        End Try
    End Function
End Class
