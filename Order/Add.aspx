<%@ Page Language="VB" AutoEventWireup="false" CodeFile="Add.aspx.vb" Inherits="Order_Add" MasterPageFile="~/Site.master" MaintainScrollPositionOnPostback="true" Debug="true" Title="Create Order" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-heading">
        <div class="page-title">
            <div class="row">
                <div class="col-12 col-md-6 order-md-1 order-last">
                    <h3><%: Page.Title %></h3>
                    <p class="text-subtitle text-muted"></p>
                </div>
                <div class="col-12 col-md-6 order-md-2 order-first">
                    <nav aria-label="breadcrumb" class="breadcrumb-header float-start float-lg-end">
                        <ol class="breadcrumb">
                            <li class="breadcrumb-item"><a runat="server" href="~/">Home</a></li>
                            <li class="breadcrumb-item"><a runat="server" href="~/order">Order</a></li>
                            <li class="breadcrumb-item active" aria-current="page"><%: Page.Title %></li>
                        </ol>
                    </nav>
                </div>
            </div>
        </div>
    </div>
    
    <div class="page-content">
        <section class="row">
            <div class="col-12 col-sm-12 col-lg-7">
                <div class="card">
                    <div class="card-header">
                        <h4 class="card-title">Create Form</h4>
                    </div>
                    
                    <div class="card-content">
                        <div class="card-body">
                            <div class="form form-horizontal">
                                <div class="form-body">
                                    <div class="row mb-3" runat="server" id="divCustomer">
                                        <div class="col-12 col-sm-12 col-lg-3">
                                            <label>Customer Account</label>
                                        </div>
                                        <div class="col-12 col-sm-12 col-lg-9 form-group">
                                            <asp:DropDownList runat="server" ID="ddlCustomer" CssClass="form-select"></asp:DropDownList>
                                        </div>
                                    </div>
                                    
                                    <div class="row" runat="server" id="divCreatedBy">
                                        <div class="col-12 col-sm-12 col-lg-3">
                                            <label>Created By</label>
                                        </div>
                                        <div class="col-12 col-sm-12 col-lg-9 form-group">
                                            <asp:DropDownList runat="server" ID="ddlCreatedBy" CssClass="form-select"></asp:DropDownList>
                                        </div>
                                    </div>
                                    
                                    <div class="row mb-3" runat="server" id="divCreatedDate">
                                        <div class="col-12 col-sm-12 col-lg-3">
                                            <label>Created Date</label>
                                        </div>
                                        <div class="col-12 col-sm-12 col-lg-9 form-group">
                                            <asp:TextBox runat="server" ID="txtCreatedDate" TextMode="Date" CssClass="form-control"></asp:TextBox>
                                        </div>
                                    </div>
                                    
                                    <div class="row">
                                        <div class="col-12 col-sm-12 col-lg-3">
                                            <label>Order Number</label>
                                        </div>
                                        <div class="col-12 col-sm-12 col-lg-9 form-group">
                                            <asp:TextBox runat="server" ID="txtOrderNumber" CssClass="form-control" placeholder="Order Number ..." autocomplete="off"></asp:TextBox>
                                        </div>
                                    </div>
                                    
                                    <div class="row">
                                        <div class="col-12 col-sm-12 col-lg-3">
                                            <label>Customer Name</label>
                                        </div>
                                        <div class="col-12 col-sm-12 col-lg-9 form-group">
                                            <asp:TextBox runat="server" ID="txtOrderName" CssClass="form-control" placeholder="Customer Name ...." autocomplete="off"></asp:TextBox>
                                        </div>
                                    </div>
                                    
                                    <div class="row mb-3">
                                        <div class="col-12 col-sm-12 col-lg-3">
                                            <label>Order Note</label>
                                        </div>
                                        <div class="col-12 col-sm-12 col-lg-9 form-group">
                                            <asp:TextBox runat="server" TextMode="MultiLine" ID="txtOrderNote" Height="130px" CssClass="form-control" placeholder="Order Note ...." autocomplete="off" style="resize: none"></asp:TextBox>
                                        </div>
                                    </div>

                                    <div class="row mb-3" runat="server" id="divOrderType">
                                        <div class="col-12 col-sm-12 col-lg-3">
                                            <label>Order Type</label>
                                        </div>
                                        <div class="col-12 col-sm-12 col-lg-3 form-group">
                                            <asp:DropDownList runat="server" ID="ddlOrderType" CssClass="form-select">
                                                <asp:ListItem Value="Regular" Text="Regular"></asp:ListItem>
                                                <asp:ListItem Value="Builder" Text="Builder"></asp:ListItem>
                                            </asp:DropDownList>
                                        </div>
                                    </div>
                                    
                                    <div class="row" runat="server" id="divError">
                                        <div class="col-12">
                                            <div class="alert alert-danger">
                                                <span runat="server" id="msgError"></span>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    
                    <div class="card-footer text-center">
                        <asp:Button runat="server" ID="btnSubmit" CssClass="btn btn-primary" Text="Submit" OnClick="btnSubmit_Click" />
                        <asp:Button runat="server" ID="btnCancel" CssClass="btn btn-danger" Text="Cancel" OnClick="btnCancel_Click" />
                    </div>
                </div>
            </div>
        </section>
    </div>
</asp:Content>
