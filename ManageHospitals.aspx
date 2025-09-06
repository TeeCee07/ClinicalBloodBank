<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ManageHospitals.aspx.cs" Inherits="ClinicalBloodBank.ManageHospitals" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Clinical Blood Bank - Manage Hospitals</title>
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
        .switch {
            position: relative;
            display: inline-block;
            width: 60px;
            height: 34px;
        }
        .switch input {
            opacity: 0;
            width: 0;
            height: 0;
        }
        .slider {
            position: absolute;
            cursor: pointer;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background-color: #ccc;
            -webkit-transition: .4s;
            transition: .4s;
        }
        .slider:before {
            position: absolute;
            content: "";
            height: 26px;
            width: 26px;
            left: 4px;
            bottom: 4px;
            background-color: white;
            -webkit-transition: .4s;
            transition: .4s;
        }
        input:checked + .slider {
            background-color: #2e7d32;
        }
        input:focus + .slider {
            box-shadow: 0 0 1px #2e7d32;
        }
        input:checked + .slider:before {
            -webkit-transform: translateX(26px);
            -ms-transform: translateX(26px);
            transform: translateX(26px);
        }
        .slider.round {
            border-radius: 34px;
        }
        .slider.round:before {
            border-radius: 50%;
        }
        .search-container {
            display: flex;
            gap: 10px;
            margin-bottom: 20px;
        }
        .search-input {
            flex: 1;
            padding: 10px;
            border: 1px solid #ddd;
            border-radius: 6px;
            font-size: 14px;
        }
        .search-button {
            padding: 10px 20px;
            background: #d32f2f;
            color: white;
            border: none;
            border-radius: 6px;
            cursor: pointer;
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
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="sidebar">
            <div class="sidebar-header">
                <h3>Clinical Blood Bank</h3>
            </div>
            <div class="sidebar-menu">
                <a href="AdminDashboard.aspx" class="menu-item">
                    <span class="menu-icon">🏠</span> Dashboard
                </a>
                <a href="ManageDonors.aspx" class="menu-item">
                    <span class="menu-icon">👤</span> Manage Donors
                </a>
                <a href="ManageHospitals.aspx" class="menu-item active">
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
                <a href="Logout.aspx" class="menu-item">
                    <span class="menu-icon">🚪</span> Logout
                </a>
            </div>
        </div>

        <div class="main-content">
            <div class="header">
                <div class="welcome-text">
                    <h1>Manage Hospitals</h1>
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
                    <div class="section-title">Add/Edit Hospital</div>
                </div>
                <asp:HiddenField ID="hdnHospitalId" runat="server" />
                
                <div class="form-row">
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtHospitalName" class="required-field">Hospital Name</label>
                            <asp:TextBox ID="txtHospitalName" runat="server" CssClass="form-control" />
                            <asp:RequiredFieldValidator ID="rfvHospitalName" runat="server" ControlToValidate="txtHospitalName"
                                ErrorMessage="Hospital name is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtLicenseNumber" class="required-field">License Number</label>
                            <asp:TextBox ID="txtLicenseNumber" runat="server" CssClass="form-control" />
                            <asp:RequiredFieldValidator ID="rfvLicenseNumber" runat="server" ControlToValidate="txtLicenseNumber"
                                ErrorMessage="License number is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                </div>
                
                <div class="form-group">
                    <label for="txtAddressLine1" class="required-field">Address Line 1</label>
                    <asp:TextBox ID="txtAddressLine1" runat="server" CssClass="form-control" />
                    <asp:RequiredFieldValidator ID="rfvAddressLine1" runat="server" ControlToValidate="txtAddressLine1"
                        ErrorMessage="Address Line 1 is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                </div>
                
                <div class="form-group">
                    <label for="txtAddressLine2">Address Line 2</label>
                    <asp:TextBox ID="txtAddressLine2" runat="server" CssClass="form-control" />
                </div>
                
                <div class="form-row">
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtCity" class="required-field">City</label>
                            <asp:TextBox ID="txtCity" runat="server" CssClass="form-control" />
                            <asp:RequiredFieldValidator ID="rfvCity" runat="server" ControlToValidate="txtCity"
                                ErrorMessage="City is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtProvince" class="required-field">Province</label>
                            <asp:TextBox ID="txtProvince" runat="server" CssClass="form-control" />
                            <asp:RequiredFieldValidator ID="rfvProvince" runat="server" ControlToValidate="txtProvince"
                                ErrorMessage="Province is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                </div>
                
                <div class="form-row">
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtPostalCode">Postal Code</label>
                            <asp:TextBox ID="txtPostalCode" runat="server" CssClass="form-control" />
                            <asp:RegularExpressionValidator ID="revPostalCode" runat="server" ControlToValidate="txtPostalCode"
                                ValidationExpression="^\d{4}$" ErrorMessage="Postal code must be 4 digits" Display="Dynamic" CssClass="text-danger"></asp:RegularExpressionValidator>
                        </div>
                    </div>
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtCountry" class="required-field">Country</label>
                            <asp:TextBox ID="txtCountry" runat="server" CssClass="form-control" Text="South Africa" />
                            <asp:RequiredFieldValidator ID="rfvCountry" runat="server" ControlToValidate="txtCountry"
                                ErrorMessage="Country is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                </div>
                
                <div class="form-row">
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtEmail" class="required-field">Contact Email</label>
                            <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" />
                            <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail"
                                ErrorMessage="Contact email is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                            <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail"
                                ValidationExpression="^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"
                                ErrorMessage="Please enter a valid email address" Display="Dynamic" CssClass="text-danger"></asp:RegularExpressionValidator>
                        </div>
                    </div>
                    <div class="form-col">
                        <div class="form-group" id="passwordSection" runat="server">
                            <label for="txtHospitalPassword" class="required-field">Password</label>
                            <asp:TextBox ID="txtHospitalPassword" runat="server" CssClass="form-control" TextMode="Password" />
                            <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtHospitalPassword"
                                ErrorMessage="Password is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                </div>
                
                <div class="form-group">
                    <asp:CheckBox ID="cbIsVerified" runat="server" Text=" Verified Hospital" Checked="true" />
                </div>
                
                <div class="form-group">
                    <asp:Button ID="btnSaveHospital" runat="server" Text="Save Hospital" CssClass="btn btn-primary" OnClick="btnSaveHospital_Click" />
                    <asp:Button ID="btnClearForm" runat="server" Text="Clear Form" CssClass="btn btn-secondary" OnClick="btnClearForm_Click" CausesValidation="false" />
                </div>
            </div>

            <div class="content-section">
                <div class="section-header">
                    <div class="section-title">Hospital List</div>
                </div>
                <div class="form-group">
                    <label for="ddlSearchBy">Search By:</label>
                    <asp:DropDownList ID="ddlSearchBy" runat="server" CssClass="form-control">
                        <asp:ListItem Value="all">All Fields</asp:ListItem>
                        <asp:ListItem Value="hospital_name">Hospital Name</asp:ListItem>
                        <asp:ListItem Value="license_number">License Number</asp:ListItem>
                        <asp:ListItem Value="contact_email">Contact Email</asp:ListItem>
                        <asp:ListItem Value="city">City</asp:ListItem>
                        <asp:ListItem Value="province">Province</asp:ListItem>
                        <asp:ListItem Value="postal_code">Postal Code</asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="form-group">
                    <label for="txtSearch">Search:</label>
                    <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control" placeholder="Enter search term"></asp:TextBox>
                </div>
                <div class="form-group">
                    <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary" OnClick="btnSearch_Click" />
                </div>

                <asp:GridView ID="gvHospitals" runat="server" AutoGenerateColumns="False" DataKeyNames="hospital_id"
                    OnRowEditing="gvHospitals_RowEditing" OnRowDeleting="gvHospitals_RowDeleting"
                    OnPageIndexChanging="gvHospitals_PageIndexChanging" OnRowDataBound="gvHospitals_RowDataBound"
                    CssClass="table table-striped" AllowPaging="True" PageSize="10">
                    <Columns>
                        <asp:BoundField DataField="hospital_name" HeaderText="Hospital Name" />
                        <asp:BoundField DataField="address_line1" HeaderText="Address Line 1" />
                        <asp:BoundField DataField="address_line2" HeaderText="Address Line 2" />
                        <asp:BoundField DataField="city" HeaderText="City" />
                        <asp:BoundField DataField="province" HeaderText="Province" />
                        <asp:BoundField DataField="postal_code" HeaderText="Postal Code" />
                        <asp:BoundField DataField="country" HeaderText="Country" />
                        <asp:BoundField DataField="license_number" HeaderText="License Number" />
                        <asp:BoundField DataField="contact_email" HeaderText="Contact Email" />
                        <asp:TemplateField HeaderText="Verified">
                            <ItemTemplate>
                                <asp:CheckBox ID="chkVerified" runat="server" Checked='<%# Eval("is_verified") %>'
                                    AutoPostBack="true" OnCheckedChanged="chkVerified_CheckedChanged" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField>
                            <ItemTemplate>
                                <asp:Button ID="btnEdit" runat="server" Text="Edit" CommandName="Edit" CssClass="btn btn-sm btn-primary" />
                                <asp:Button ID="btnDelete" runat="server" Text="Delete" CommandName="Delete" CssClass="btn btn-sm btn-danger" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </form>
</body>
</html>