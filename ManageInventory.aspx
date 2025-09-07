
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ManageInventory.aspx.cs" Inherits="ClinicalBloodBank.ManageInventory" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Clinical Blood Bank - Manage Inventory</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        }
        body {
            background-color: #f8f9fa;
            display: flex;
            min-height: 100vh;
        }
        .sidebar {
            width: 250px;
            background: linear-gradient(to bottom, #2c3e50, #1a2530);
            color: white;
            height: 100vh;
            position: fixed;
            overflow-y: auto;
        }
        .sidebar-header {
            padding: 20px;
            text-align: center;
            border-bottom: 1px solid rgba(255,255,255,0.1);
        }
        .sidebar-header h3 {
            color: white;
            font-size: 18px;
            margin: 0;
        }
        .sidebar-menu {
            padding: 20px 0;
        }
        .menu-item {
            padding: 12px 20px;
            display: flex;
            align-items: center;
            color: white;
            text-decoration: none;
            transition: background 0.3s;
            cursor: pointer;
        }
        .menu-item:hover {
            background: rgba(255,255,255,0.1);
        }
        .menu-item.active {
            background: rgba(255,255,255,0.2);
            border-left: 4px solid #d32f2f;
        }
        .menu-icon {
            margin-right: 10px;
            width: 20px;
            text-align: center;
        }
        .main-content {
            flex: 1;
            margin-left: 250px;
            padding: 20px;
        }
        .header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 20px;
            background: white;
            border-radius: 10px;
            margin-bottom: 20px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }
        .welcome-text h1 {
            color: #2c3e50;
            font-size: 24px;
            margin-bottom: 5px;
        }
        .welcome-text p {
            color: #7f8c8d;
            margin: 0;
        }
        .user-profile {
            display: flex;
            align-items: center;
            gap: 10px;
        }
        .user-avatar {
            width: 40px;
            height: 40px;
            border-radius: 50%;
            background: #d32f2f;
            color: white;
            display: flex;
            align-items: center;
            justify-content: center;
            font-weight: bold;
        }
        .content-section {
            background: white;
            border-radius: 10px;
            padding: 20px;
            margin-bottom: 20px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }
        .section-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 20px;
        }
        .section-title {
            color: #2c3e50;
            font-size: 20px;
            font-weight: 600;
        }
        .btn {
            padding: 10px 20px;
            border: none;
            border-radius: 6px;
            cursor: pointer;
            font-weight: 600;
            transition: background 0.3s;
        }
        .btn-primary {
            background: #d32f2f;
            color: white;
        }
        .btn-primary:hover {
            background: #b71c1c;
        }
        .btn-secondary {
            background: #2c3e50;
            color: white;
        }
        .btn-secondary:hover {
            background: #1a2530;
        }
        .table {
            width: 100%;
            border-collapse: collapse;
        }
        .table th,
        .table td {
            padding: 12px;
            text-align: left;
            border-bottom: 1px solid #eee;
        }
        .table th {
            background: #f8f9fa;
            color: #2c3e50;
            font-weight: 600;
        }
        .status-badge {
            padding: 5px 10px;
            border-radius: 20px;
            font-size: 12px;
            font-weight: 600;
        }
        .status-active {
            background: #e8f5e9;
            color: #2e7d32;
        }
        .status-inactive {
            background: #ffebee;
            color: #d32f2f;
        }
        .status-pending {
            background: #fff3e0;
            color: #ff9800;
        }
        .action-buttons {
            display: flex;
            gap: 5px;
        }
        .btn-sm {
            padding: 5px 10px;
            font-size: 12px;
        }
        .btn-edit {
            background: #ff9800;
            color: white;
        }
        .btn-delete {
            background: #d32f2f;
            color: white;
        }
        .form-group {
            margin-bottom: 15px;
        }
        .form-group label {
            display: block;
            margin-bottom: 5px;
            font-weight: 500;
            color: #2c3e50;
        }
        .form-control {
            width: 100%;
            padding: 10px;
            border: 1px solid #ddd;
            border-radius: 6px;
            font-size: 14px;
        }
        .form-control:focus {
            border-color: #d32f2f;
            outline: none;
            box-shadow: 0 0 0 3px rgba(211, 47, 47, 0.2);
        }
        .alert {
            padding: 12px;
            border-radius: 6px;
            margin-bottom: 20px;
        }
        .alert-danger {
            background-color: #ffebee;
            color: #c62828;
            border: 1px solid #ef9a9a;
        }
        .alert-success {
            background-color: #e8f5e9;
            color: #2e7d32;
            border: 1px solid #a5d6a7;
        }
        .alert-info {
            background-color: #e3f2fd;
            color: #0288d1;
            border: 1px solid #90caf9;
        }
        .form-row {
            display: flex;
            gap: 15px;
            margin-bottom: 15px;
        }
        .form-col {
            flex: 1;
        }
        .required-field::after {
            content: " *";
            color: #d32f2f;
        }
        .pager {
            margin-top: 20px;
            display: flex;
            justify-content: center;
            align-items: center;
            gap: 10px;
        }
        .pager-button {
            padding: 8px 12px;
            background: #f8f9fa;
            border: 1px solid #ddd;
            border-radius: 4px;
            cursor: pointer;
        }
        .pager-button.active {
            background: #d32f2f;
            color: white;
            border-color: #d32f2f;
        }
        .text-danger {
            color: #d32f2f;
            font-size: 12px;
            margin-top: 5px;
            display: block;
        }
        .form-control[readonly] {
            background-color: #f8f9fa;
            cursor: not-allowed;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="sidebar">
            <div class="sidebar-header">
                <h3>Clinical Blood Bank</h3>
            </div>
                       <div class="sidebar-menu">
    <a href="AdminDashboard.aspx" class="menu-item active">
        <span class="menu-icon">🏠</span> Dashboard
    </a>
    <a href="ManageDonors.aspx" class="menu-item">
        <span class="menu-icon">👤</span> Manage Donors
    </a>
    <a href="ManageHospitals.aspx" class="menu-item">
        <span class="menu-icon">🏥</span> Manage Hospitals
    </a>
    <a href="ManageAdmins.aspx" class="menu-item">
        <span class="menu-icon">🔑</span> Manage Admins
    </a>
    <a href="ManageInventory.aspx" class="menu-item">
        <span class="menu-icon">🩺</span> Blood Inventory
    </a>
    <a href="ManageBloodRequests.aspx" class="menu-item">
        <span class="menu-icon">🩺</span> Blood Requests
    </a>
    <a href="ManageAppointments.aspx" class="menu-item">
        <span class="menu-icon">📅</span> Appointments
    </a>
    <a href="ManageRewards.aspx" class="menu-item">
        <span class="menu-icon">🎁</span> Manage Rewards
    </a>
    <a href="Reports.aspx" class="menu-item">
        <span class="menu-icon">📊</span> Reports
    </a>
    <asp:LinkButton ID="lnkLogout" runat="server" CssClass="menu-item" OnClick="lnkLogout_Click">
        <span class="menu-icon">🚪</span> Logout
    </asp:LinkButton>
</div>
        </div>

        <div class="main-content">
            <div class="header">
                <div class="welcome-text">
                    <h1>Manage Inventory</h1>
                    <p>Admin Panel</p>
                </div>
                <div class="user-profile">
                    <div class="user-avatar">
                        <asp:Literal ID="litUserInitials" runat="server"></asp:Literal>
                    </div>
                    <span><asp:Literal ID="litUserName" runat="server"></asp:Literal></span>
                </div>
            </div>

            <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert">
                <asp:Label ID="lblMessage" runat="server" Text=""></asp:Label>
            </asp:Panel>

            <div class="content-section">
                <div class="section-header">
                    <div class="section-title">Add/Edit Inventory</div>
                </div>
                <asp:HiddenField ID="hdnInventoryId" runat="server" />
                <div class="form-row">
                    <div class="form-col">
                        <div class="form-group">
                            <label for="ddlInventoryBloodType" class="required-field">Blood Type</label>
                            <asp:DropDownList ID="ddlInventoryBloodType" runat="server" CssClass="form-control">
                                <asp:ListItem Value="">Select Blood Type</asp:ListItem>
                                <asp:ListItem Value="A+">A+</asp:ListItem>
                                <asp:ListItem Value="A-">A-</asp:ListItem>
                                <asp:ListItem Value="B+">B+</asp:ListItem>
                                <asp:ListItem Value="B-">B-</asp:ListItem>
                                <asp:ListItem Value="AB+">AB+</asp:ListItem>
                                <asp:ListItem Value="AB-">AB-</asp:ListItem>
                                <asp:ListItem Value="O+">O+</asp:ListItem>
                                <asp:ListItem Value="O-">O-</asp:ListItem>
                            </asp:DropDownList>
                            <asp:RequiredFieldValidator ID="rfvBloodType" runat="server" ControlToValidate="ddlInventoryBloodType"
                                ErrorMessage="Blood type is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtQuantity" class="required-field">Quantity (ml)</label>
                            <asp:TextBox ID="txtQuantity" runat="server" CssClass="form-control" TextMode="Number" />
                            <asp:RequiredFieldValidator ID="rfvQuantity" runat="server" ControlToValidate="txtQuantity"
                                ErrorMessage="Quantity is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                            <asp:RegularExpressionValidator ID="revQuantity" runat="server" ControlToValidate="txtQuantity"
                                ValidationExpression="^[1-9]\d*$" ErrorMessage="Quantity must be a positive integer" Display="Dynamic" CssClass="text-danger"></asp:RegularExpressionValidator>
                        </div>
                    </div>
                </div>
                <div class="form-row">
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtDonationDate" class="required-field">Donation Date</label>
                            <asp:TextBox ID="txtDonationDate" runat="server" CssClass="form-control" TextMode="Date" />
                            <asp:RequiredFieldValidator ID="rfvDonationDate" runat="server" ControlToValidate="txtDonationDate"
                                ErrorMessage="Donation date is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                    <div class="form-col">
                        <div class="form-group">
                            <label for="lblExpirationDate">Expiration Date</label>
                            <asp:Label ID="lblExpirationDate" runat="server" CssClass="form-control" ReadOnly="true" />
                        </div>
                    </div>
                </div>
                <div class="form-row">
                    <div class="form-col">
                        <div class="form-group">
                            <label for="ddlDonor" class="required-field">Donor</label>
                            <asp:DropDownList ID="ddlDonor" runat="server" CssClass="form-control" />
                            <asp:RequiredFieldValidator ID="rfvDonor" runat="server" ControlToValidate="ddlDonor"
                                ErrorMessage="Donor is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                    <div class="form-col">
                        <div class="form-group">
                            <label for="ddlHospital" class="required-field">Tested By Hospital</label>
                            <asp:DropDownList ID="ddlHospital" runat="server" CssClass="form-control" />
                            <asp:RequiredFieldValidator ID="rfvHospital" runat="server" ControlToValidate="ddlHospital"
                                ErrorMessage="Hospital is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                </div>
                <div class="form-row">
                    <div class="form-col">
                        <div class="form-group">
                            <label for="ddlTestResult" class="required-field">Test Result</label>
                            <asp:DropDownList ID="ddlTestResult" runat="server" CssClass="form-control">
                                <asp:ListItem Value="pending">Pending</asp:ListItem>
                                <asp:ListItem Value="passed">Passed</asp:ListItem>
                                <asp:ListItem Value="failed">Failed</asp:ListItem>
                            </asp:DropDownList>
                            <asp:RequiredFieldValidator ID="rfvTestResult" runat="server" ControlToValidate="ddlTestResult"
                                ErrorMessage="Test result is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                    <div class="form-col">
                        <div class="form-group">
                            <label for="ddlStatus" class="required-field">Status</label>
                            <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-control">
                                <asp:ListItem Value="available">Available</asp:ListItem>
                                <asp:ListItem Value="reserved">Reserved</asp:ListItem>
                                <asp:ListItem Value="used">Used</asp:ListItem>
                                <asp:ListItem Value="expired">Expired</asp:ListItem>
                            </asp:DropDownList>
                            <asp:RequiredFieldValidator ID="rfvStatus" runat="server" ControlToValidate="ddlStatus"
                                ErrorMessage="Status is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                </div>
                <div class="form-group">
                    <asp:Button ID="btnSaveInventory" runat="server" Text="Save Inventory" CssClass="btn btn-primary" OnClick="btnSaveInventory_Click" />
                    <asp:Button ID="btnClearForm" runat="server" Text="Clear Form" CssClass="btn btn-secondary" OnClick="btnClearForm_Click" CausesValidation="false" />
                </div>
            </div>

            <div class="content-section">
                <div class="section-header">
                    <div class="section-title">Filter Inventory</div>
                </div>
                <div class="form-row">
                    <div class="form-col">
                        <div class="form-group">
                            <label for="ddlFilterBloodType">Blood Type</label>
                            <asp:DropDownList ID="ddlFilterBloodType" runat="server" CssClass="form-control" />
                        </div>
                    </div>
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtFilterExpirationDate">Expires By</label>
                            <asp:TextBox ID="txtFilterExpirationDate" runat="server" CssClass="form-control" TextMode="Date" />
                        </div>
                    </div>
                    <div class="form-col">
                        <div class="form-group">
                            <label for="ddlFilterProvince">Province</label>
                            <asp:DropDownList ID="ddlFilterProvince" runat="server" CssClass="form-control" />
                        </div>
                    </div>
                </div>
                <div class="form-group">
                    <asp:Button ID="btnFilter" runat="server" Text="Apply Filter" CssClass="btn btn-primary" OnClick="btnFilter_Click" />
                    <asp:Button ID="btnClearFilter" runat="server" Text="Clear Filter" CssClass="btn btn-secondary" OnClick="btnClearFilter_Click" CausesValidation="false" />
                </div>
            </div>

            <div class="content-section">
                <div class="section-header">
                    <div class="section-title">Inventory List</div>
                </div>
                <asp:GridView ID="gvInventory" runat="server" AutoGenerateColumns="False" CssClass="table" DataKeyNames="inventory_id"
                    OnRowEditing="gvInventory_RowEditing" OnRowDeleting="gvInventory_RowDeleting"
                    OnPageIndexChanging="gvInventory_PageIndexChanging" OnRowDataBound="gvInventory_RowDataBound"
                    AllowPaging="True" PageSize="10">
                    <Columns>
                        <asp:BoundField DataField="inventory_id" HeaderText="ID" ReadOnly="True" />
                        <asp:BoundField DataField="blood_type" HeaderText="Blood Type" />
                        <asp:BoundField DataField="quantity_ml" HeaderText="Quantity (ml)" />
                        <asp:BoundField DataField="donation_date" HeaderText="Donation Date" DataFormatString="{0:yyyy-MM-dd}" />
                        <asp:BoundField DataField="expiration_date" HeaderText="Expiration Date" DataFormatString="{0:yyyy-MM-dd}" />
                        <asp:BoundField DataField="donor_name" HeaderText="Donor" />
                        <asp:BoundField DataField="hospital_name" HeaderText="Hospital" />
                        <asp:BoundField DataField="province" HeaderText="Province" />
                        <asp:BoundField DataField="test_result" HeaderText="Test Result" />
                        <asp:TemplateField HeaderText="Status">
                            <ItemTemplate>
                                <span class='status-badge <%# Eval("status").ToString() == "available" ? "status-active" : Eval("status").ToString() == "expired" ? "status-inactive" : "status-pending" %>'>
                                    <%# Eval("status") %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Actions">
                            <ItemTemplate>
                                <div class="action-buttons">
                                    <asp:Button ID="btnEdit" runat="server" CommandName="Edit" Text="Edit" CssClass="btn btn-sm btn-edit" />
                                    <asp:Button ID="btnDelete" runat="server" CommandName="Delete" Text="Delete" CssClass="btn btn-sm btn-delete" />
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </form>
</body>
</html>
