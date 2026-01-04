Imports System.Data.SqlClient

Partial Class Order_Add
    Inherits Page

    Dim myConn As String = ConfigurationManager.ConnectionStrings("DefaultConnection").ConnectionString
    Dim dataMailing As Object() = Nothing
    Dim url As String = String.Empty

    Dim orderClass As New OrderClass
    Dim mailingClass As New MailingClass

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        Dim pageAccess As Boolean = PageAction("Load")
        If pageAccess = False Then
            Response.Redirect("~/order/", False)
            Exit Sub
        End If

        If Session("RoleName") = "Customer" Then
            Dim status As Boolean = orderClass.GetCustomerOnStop(Session("CustomerId").ToString())
            If status = True Then
                Response.Redirect("~/order/", False)
                Exit Sub
            End If
        End If

        If Not IsPostBack Then
            BackColor()
            BindDataCustomer()
            BindDataUser()

            txtCreatedDate.Text = Now.ToString("yyyy-MM-dd")

            BindComponentForm()
        End If
    End Sub

    Protected Sub btnSubmit_Click(sender As Object, e As EventArgs)
        BackColor()
        Try
            Dim companyAlias As String = orderClass.GetCompanyAliasByCustomer(ddlCustomer.SelectedValue)
            Dim companyDetailName As String = orderClass.GetCompanyDetailNameByCustomer(ddlCustomer.SelectedValue)

            If ddlCustomer.SelectedValue = "" Then
                MessageError(True, "CUSTOMER NAME IS REQUIRED !")
                ddlCustomer.BackColor = Drawing.Color.Red
                ddlCustomer.Focus()
                Exit Sub
            End If

            If txtOrderNumber.Text = "" Then
                MessageError(True, "ORDER NUMBER IS REQUIRED !")
                txtOrderNumber.BackColor = Drawing.Color.Red
                txtOrderNumber.Focus()
                Exit Sub
            End If

            If InStr(txtOrderNumber.Text, "\") > 0 OrElse InStr(txtOrderNumber.Text, "/") > 0 OrElse InStr(txtOrderNumber.Text, ",") > 0 OrElse InStr(txtOrderNumber.Text, "&") > 0 OrElse InStr(txtOrderNumber.Text, "#") > 0 OrElse InStr(txtOrderNumber.Text, "'") > 0 OrElse InStr(txtOrderNumber.Text, ".") > 0 Then
                MessageError(True, "PLEASE DON'T USE [ / ], [ \ ], [ & ], [ # ], [ ' ], [ . ] AND [ , ]")
                txtOrderNumber.BackColor = Drawing.Color.Red
                txtOrderNumber.Focus()
                Exit Sub
            End If

            If Trim(txtOrderNumber.Text).Length > 20 Then
                MessageError(True, "MAXIMUM 20 CHARACTERS FOR RETAILER ORDER NUMBER !")
                txtOrderNumber.BackColor = Drawing.Color.Red
                txtOrderNumber.Focus()
                Exit Sub
            End If

            If txtOrderName.Text = "" Then
                MessageError(True, "CUSTOMER NAME IS REQUIRED !")
                txtOrderName.BackColor = Drawing.Color.Red
                txtOrderName.Focus()
                Exit Sub
            End If

            If txtOrderNumber.Text = orderClass.IsOrderExist(ddlCustomer.SelectedValue, txtOrderNumber.Text.Trim()) Then
                MessageError(True, "ORDER NUMBER ALREADY EXISTS !")
                txtOrderNumber.BackColor = Drawing.Color.Red
                txtOrderNumber.Focus()
                Exit Sub
            End If

            If ddlOrderType.SelectedValue = "Builder" AndAlso Not companyDetailName = "JPMD BP" Then
                MessageError(True, "ORDER TYPE SHOULD BE REGULAR !")
                ddlOrderType.BackColor = Drawing.Color.Red
                ddlOrderType.Focus()
                Exit Sub
            End If

            If msgError.InnerText = "" Then
                Dim thisId As String = orderClass.GetNewOrderHeaderId()

                Dim success As Boolean = False
                Dim retry As Integer = 0
                Dim maxRetry As Integer = 10
                Dim orderId As String = ""

                Do While Not success
                    retry += 1
                    If retry > maxRetry Then
                        Throw New Exception("FAILED TO GENERATE UNIQUE ORDER ID")
                    End If

                    Dim randomCode As String = orderClass.GenerateRandomCode()
                    orderId = companyAlias & randomCode

                    Try
                        Using thisConn As New SqlConnection(myConn)
                            Using myCmd As New SqlCommand("INSERT INTO OrderHeaders (Id, OrderId, CustomerId, OrderNumber, OrderName, OrderNote, OrderType, Status, CreatedBy, CreatedDate, DownloadBOE, Active) VALUES (@Id, @OrderId, @CustomerId, @OrderNumber, @OrderName, @OrderNote, @OrderType, 'Unsubmitted', @CreatedBy, @CreateDate, 0, 1); INSERT INTO OrderQuotes VALUES (@Id, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0.00, 0.00, 0.00, 0.00);", thisConn)
                                myCmd.Parameters.AddWithValue("@Id", thisId)
                                myCmd.Parameters.AddWithValue("@OrderId", orderId)
                                myCmd.Parameters.AddWithValue("@CustomerId", ddlCustomer.SelectedValue)
                                myCmd.Parameters.AddWithValue("@OrderNumber", txtOrderNumber.Text.Trim())
                                myCmd.Parameters.AddWithValue("@OrderName", txtOrderName.Text.Trim())
                                myCmd.Parameters.AddWithValue("@OrderNote", txtOrderNote.Text.Trim())
                                myCmd.Parameters.AddWithValue("@OrderType", ddlOrderType.SelectedValue)
                                myCmd.Parameters.AddWithValue("@CreatedBy", ddlCreatedBy.SelectedValue)
                                myCmd.Parameters.AddWithValue("@CreateDate", txtCreatedDate.Text)

                                thisConn.Open()
                                myCmd.ExecuteNonQuery()
                            End Using
                        End Using

                        success = True

                    Catch exSql As SqlException
                        If exSql.Number = 2601 OrElse exSql.Number = 2627 Then
                            success = False
                        Else
                            Throw
                        End If
                    End Try
                Loop

                If ddlOrderType.SelectedValue = "Builder" Then
                    Using thisConn As New SqlConnection(myConn)
                        Using myCmd As New SqlCommand("INSERT INTO OrderBuilders(Id) VALUES (@Id)", thisConn)
                            myCmd.Parameters.AddWithValue("@Id", thisId)
                            thisConn.Open()
                            myCmd.ExecuteNonQuery()
                        End Using
                    End Using

                    Dim directoryOrder As String = Server.MapPath(String.Format("~/File/Builder/{0}/", orderId))
                    If Not IO.Directory.Exists(directoryOrder) Then
                        IO.Directory.CreateDirectory(directoryOrder)
                    End If
                End If

                Dim dataLog As Object() = {"OrderHeaders", thisId, Session("LoginId").ToString(), "Order Created"}
                orderClass.Logs(dataLog)

                url = String.Format("~/order/detail?orderid={0}", thisId)
                Response.Redirect(url, False)
            End If

        Catch ex As Exception
            MessageError(True, ex.ToString())
            If Not Session("RoleName") = "Developer" Then
                MessageError(True, "PLEASE CONTACT IT SUPPORT AT REZA@BIGBLINDS.CO.ID !")
                If Session("RoleName") = "Customer" Then
                    MessageError(True, "PLEASE CONTACT YOUR CUSTOMER SERVICE !")
                End If
                dataMailing = {Session("LoginId").ToString(), Session("CompanyId").ToString(), Page.Title, "btnSubmit_Click", ex.ToString()}
                mailingClass.WebError(dataMailing)
            End If
        End Try
    End Sub

    Protected Sub btnCancel_Click(sender As Object, e As EventArgs)
        Response.Redirect("~/order/", False)
    End Sub

    Protected Sub BindComponentForm()
        Try
            divCustomer.Visible = False
            divCreatedBy.Visible = False
            divCreatedDate.Visible = False
            divOrderType.Visible = False

            If Session("RoleName") = "Developer" OrElse Session("RoleName") = "IT" Then
                divCustomer.Visible = True
                divOrderType.Visible = True
                divCreatedDate.Visible = True
            End If
            If Session("RoleName") = "Developer" Then
                divCreatedBy.Visible = True
            End If
        Catch ex As Exception
            MessageError(True, ex.ToString())
            If Not Session("RoleName") = "Developer" Then
                MessageError(True, "PLEASE CONTACT IT SUPPORT AT REZA@BIGBLINDS.CO.ID !")
                If Session("RoleName") = "Customer" Then
                    MessageError(True, "PLEASE CONTACT YOUR CUSTOMER SERVICE !")
                End If
                dataMailing = {Session("LoginId").ToString(), Session("CompanyId").ToString(), Page.Title, "BindComponentForm", ex.ToString()}
                mailingClass.WebError(dataMailing)
            End If
        End Try
    End Sub

    Protected Sub BindDataCustomer()
        ddlCustomer.Items.Clear()
        Try
            Dim role As String = String.Empty
            If Session("RoleName") = "IT" Then
                role = "AND Id<>'1' AND Id<>'2'"
            End If

            Dim thisQuery As String = String.Format("SELECT * FROM Customers WHERE Active=1 {0} ORDER BY Name ASC", role)

            ddlCustomer.DataSource = orderClass.GetListData(thisQuery)
            ddlCustomer.DataTextField = "Name"
            ddlCustomer.DataValueField = "Id"
            ddlCustomer.DataBind()

            If ddlCustomer.Items.Count > 1 Then
                ddlCustomer.Items.Insert(0, New ListItem("", ""))
            End If

            If Session("RoleName") = "Customer" Then
                ddlCustomer.SelectedValue = Session("CustomerId").ToString()
            End If
        Catch ex As Exception
            ddlCustomer.Items.Clear()
            If Session("RoleName") = "Developer" Then
                MessageError(True, ex.ToString())
            End If
            dataMailing = {Session("LoginId").ToString(), Session("CompanyId").ToString(), Page.Title, "BindDataCustomer", ex.ToString()}
            mailingClass.WebError(dataMailing)
        End Try
    End Sub

    Protected Sub BindDataUser()
        ddlCreatedBy.Items.Clear()
        Try
            ddlCreatedBy.DataSource = orderClass.GetListData("SELECT * FROM CustomerLogins ORDER BY UserName ASC")
            ddlCreatedBy.DataTextField = "UserName"
            ddlCreatedBy.DataValueField = "Id"
            ddlCreatedBy.DataBind()

            If ddlCreatedBy.Items.Count > 0 Then
                ddlCreatedBy.Items.Insert(0, New ListItem("", ""))
            End If

            ddlCreatedBy.SelectedValue = Session("LoginId")
        Catch ex As Exception
            ddlCreatedBy.Items.Clear()
            If Session("RoleName") = "Developer" Then
                MessageError(True, ex.ToString())
            End If
            dataMailing = {Session("LoginId").ToString(), Session("CompanyId").ToString(), Page.Title, "BindDataLogin", ex.ToString()}
            mailingClass.WebError(dataMailing)
        End Try
    End Sub

    Protected Sub BackColor()
        MessageError(False, String.Empty)

        ddlCustomer.BackColor = Drawing.Color.Empty
        txtOrderNumber.BackColor = Drawing.Color.Empty
        txtOrderName.BackColor = Drawing.Color.Empty
        txtOrderNote.BackColor = Drawing.Color.Empty
        ddlOrderType.BackColor = Drawing.Color.Empty
    End Sub

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
