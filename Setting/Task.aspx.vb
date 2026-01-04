Imports System.Data
Imports System.Data.SqlClient

Partial Class Setting_Task
    Inherits Page

    Dim myConn As String = ConfigurationManager.ConnectionStrings("DefaultConnection").ConnectionString

    Dim settingClass As New SettingClass

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If String.IsNullOrEmpty(Request.QueryString("action")) Then
            Response.Redirect("~/", False)
            Exit Sub
        End If

        Dim thisAction As String = Request.QueryString("action").ToString()
        If thisAction = "salescosting" Then
            CreateSalesCosting()
        End If
        If thisAction = "resetcashsaleorder" Then
            ResetDataCashSaleOrder()
        End If
        If thisAction = "unshipment" Then
            UnshipmentOrder()
        End If
        If thisAction = "orderactioncontext" Then
            DeleteContext()
        End If
        If thisAction = "login" Then
            If String.IsNullOrEmpty(Request.QueryString("user")) Then
                Response.Redirect("~", False)
                Exit Sub
            End If
            GetTemporary(Request.QueryString("user").ToString())
        End If
        If thisAction = "resetpassword" Then
            ResetPassword()
        End If
    End Sub

    Protected Sub CreateSalesCosting()
        Try
            Dim salesClass As New SalesClass
            Dim checkData As Integer = salesClass.GetItemData_Integer("SELECT COUNT(*) FROM Sales WHERE SummaryDate=GETDATE()")
            If checkData = 0 Then
                Dim thisId As String = salesClass.CreateId("SELECT TOP 1 Id FROM Sales ORDER BY Id DESC")

                Using thisConn As New SqlConnection(myConn)
                    Using myCmd As SqlCommand = New SqlCommand("INSERT INTO Sales(Id, SummaryDate, TotalCostPrice, TotalSellingPrice, TotalPaidAmount) VALUES(@Id, GETDATE(), 0, 0, 0)", thisConn)
                        myCmd.Parameters.AddWithValue("@Id", thisId)

                        thisConn.Open()
                        myCmd.ExecuteNonQuery()
                    End Using
                End Using
            End If
        Catch ex As Exception
        End Try
    End Sub

    Protected Sub ResetDataCashSaleOrder()
        Try
            Dim orderClass As New OrderClass
            Dim thisData As DataSet = orderClass.GetListData("SELECT OrderHeaders.* FROM OrderHeaders LEFT JOIN OrderInvoices ON OrderHeaders.Id=OrderInvoices.Id WHERE OrderHeaders.Status='Proforma Sent' AND OrderInvoices.DueDate=CAST(GETDATE() AS DATE)")

            If thisData.Tables(0).Rows.Count > 0 Then
                For i As Integer = 0 To thisData.Tables(0).Rows.Count - 1
                    Dim thisId As String = thisData.Tables(0).Rows(i).Item("Id").ToString()

                    Using thisConn As New SqlConnection(myConn)
                        thisConn.Open()

                        Using myCmd As SqlCommand = New SqlCommand("UPDATE OrderHeaders SET Status='Unsubmitted', SubmittedDate=NULL WHERE Id=@Id; DELETE FROM OrderInvoices WHERE Id=@Id;", thisConn)
                            myCmd.Parameters.AddWithValue("@Id", thisId)

                            myCmd.ExecuteNonQuery()
                        End Using

                        Dim serviceData As DataSet = orderClass.GetListData("SELECT OrderDetails.* FROM OrderDetails LEFT JOIN Products ON OrderDetails.ProductId=Products.Id WHERE OrderDetails.HeaderId='" & thisId & "' AND Products.DesignId='16'")
                        If serviceData.Tables(0).Rows.Count > 0 Then
                            For iDetail As Integer = 0 To serviceData.Tables(0).Rows.Count - 1
                                Dim serviceId As String = serviceData.Tables(0).Rows(iDetail).Item("Id").ToString()

                                Using myCmd As SqlCommand = New SqlCommand("UPDATE OrderDetails SET Active=0 WHERE Id=@ItemId; DELETE FROM OrderCostings WHERE HeaderId=@HeaderId AND ItemId=@ItemId", thisConn)
                                    myCmd.Parameters.AddWithValue("@ItemId", serviceId)
                                    myCmd.Parameters.AddWithValue("@HeaderId", thisId)

                                    myCmd.ExecuteNonQuery()
                                End Using
                            Next
                        End If

                        thisConn.Close()
                    End Using
                Next
            End If
        Catch ex As Exception
        End Try
    End Sub

    Protected Sub UnshipmentOrder()
        Try
            If Now.DayOfWeek >= DayOfWeek.Monday AndAlso Now.DayOfWeek <= DayOfWeek.Friday Then
                Dim mailClass As New MailingClass
                Dim unshipmentClass As New UnshipmentClass

                Dim fileName As String = Trim("Unshipment - In Production Order " & Now.ToString("dd MMm yyyy") & ".pdf")

                Dim pdfFilePath As String = Server.MapPath("~/File/Order/" & fileName)
                unshipmentClass.BindContent(pdfFilePath)

                mailClass.MailUnshipment(pdfFilePath)
            End If
        Catch ex As Exception
        End Try
    End Sub

    Protected Sub DeleteContext()
        Try
            Using thisConn As New SqlConnection(myConn)
                Using myCmd As SqlCommand = New SqlCommand("DELETE FROM OrderActionContext", thisConn)

                    thisConn.Open()
                    myCmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
        End Try
    End Sub

    Protected Sub GetTemporary(user As String)
        Try
            Dim thisId As String = String.Empty

            Using thisConn As New SqlConnection(myConn)
                Using myCmd As SqlCommand = New SqlCommand("INSERT INTO Temporarys OUTPUT INSERTED.Id VALUES (NEWID(), @UserName)", thisConn)
                    myCmd.Parameters.AddWithValue("@UserName", user)

                    thisConn.Open()
                    thisId = myCmd.ExecuteScalar().ToString()
                End Using
            End Using
            Dim url As String = String.Format("~/information?uid={0}", thisId)
            Response.Redirect(url, False)
        Catch ex As Exception
        End Try
    End Sub

    Protected Sub ResetPassword()
        Dim loginData As DataSet = settingClass.GetListData("SELECT * FROM CustomerLogins")
        If loginData.Tables(0).Rows.Count > 0 Then
            For iLogin As Integer = 0 To loginData.Tables(0).Rows.Count - 1
                Dim loginId As String = loginData.Tables(0).Rows(iLogin).Item("Id").ToString()
                Dim userName As String = loginData.Tables(0).Rows(iLogin).Item("UserName").ToString()

                Dim newPassword As String = settingClass.Encrypt(userName)

                Using thisConn As New SqlConnection(myConn)
                    Using myCmd As SqlCommand = New SqlCommand("UPDATE CustomerLogins SET Password=@Password WHERE Id=@Id", thisConn)
                        myCmd.Parameters.AddWithValue("@Id", loginId)
                        myCmd.Parameters.AddWithValue("@Password", newPassword)

                        thisConn.Open()
                        myCmd.ExecuteNonQuery()
                    End Using
                End Using
            Next
        End If
    End Sub
End Class
