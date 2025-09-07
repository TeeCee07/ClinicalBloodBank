<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ManageDonors.aspx.cs" Inherits="ClinicalBloodBank.ManageDonors" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Clinical Blood Bank - Manage Donors</title>
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
                    <h1>Manage Donors</h1>
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
                    <div class="section-title">Add/Edit Donor</div>
                </div>
                <asp:HiddenField ID="hdnDonorId" runat="server" />
                <asp:HiddenField ID="hdnUserId" runat="server" />
                
                <div class="form-row">
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtFirstName" class="required-field">First Name</label>
                            <asp:TextBox ID="txtFirstName" runat="server" CssClass="form-control" />
                            <asp:RequiredFieldValidator ID="rfvFirstName" runat="server" ControlToValidate="txtFirstName"
                                ErrorMessage="First name is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtLastName" class="required-field">Last Name</label>
                            <asp:TextBox ID="txtLastName" runat="server" CssClass="form-control" />
                            <asp:RequiredFieldValidator ID="rfvLastName" runat="server" ControlToValidate="txtLastName"
                                ErrorMessage="Last name is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                </div>
                
                <div class="form-row">
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtEmail" class="required-field">Email</label>
                            <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" />
                            <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail"
                                ErrorMessage="Email is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                            <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail"
                                ValidationExpression="^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"
                                ErrorMessage="Please enter a valid email address" Display="Dynamic" CssClass="text-danger"></asp:RegularExpressionValidator>
                        </div>
                    </div>
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtPhone" class="required-field">Phone</label>
                            <asp:TextBox ID="txtPhone" runat="server" CssClass="form-control" />
                            <asp:RequiredFieldValidator ID="rfvPhone" runat="server" ControlToValidate="txtPhone"
                                ErrorMessage="Phone number is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                </div>
                
                <div class="form-group">
                    <label for="txtAddressLine1" class="required-field">Address Line 1</label>
                    <asp:TextBox ID="txtAddressLine1" runat="server" CssClass="form-control" />
                    <asp:RequiredFieldValidator ID="rfvAddress1" runat="server" ControlToValidate="txtAddressLine1"
                        ErrorMessage="Address is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
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
                            <label for="txtState" class="required-field">State/Province</label>
                            <asp:TextBox ID="txtState" runat="server" CssClass="form-control" />
                            <asp:RequiredFieldValidator ID="rfvState" runat="server" ControlToValidate="txtState"
                                ErrorMessage="State is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                </div>
                
                <div class="form-row">
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtPostalCode" class="required-field">Postal Code</label>
                            <asp:TextBox ID="txtPostalCode" runat="server" CssClass="form-control" />
                            <asp:RequiredFieldValidator ID="rfvPostalCode" runat="server" ControlToValidate="txtPostalCode"
                                ErrorMessage="Postal code is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
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
                            <label for="txtDateOfBirth" class="required-field">Date of Birth</label>
                            <asp:TextBox ID="txtDateOfBirth" runat="server" CssClass="form-control" TextMode="Date" />
                            <asp:RequiredFieldValidator ID="rfvDateOfBirth" runat="server" ControlToValidate="txtDateOfBirth"
                                ErrorMessage="Date of birth is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                    <div class="form-col">
                        <div class="form-group">
                            <label for="ddlGender" class="required-field">Gender</label>
                            <asp:DropDownList ID="ddlGender" runat="server" CssClass="form-control">
                                <asp:ListItem Value="">Select Gender</asp:ListItem>
                                <asp:ListItem Value="Male">Male</asp:ListItem>
                                <asp:ListItem Value="Female">Female</asp:ListItem>
                                <asp:ListItem Value="Other">Other</asp:ListItem>
                            </asp:DropDownList>
                            <asp:RequiredFieldValidator ID="rfvGender" runat="server" ControlToValidate="ddlGender"
                                ErrorMessage="Gender is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                </div>
                
                <div class="form-row">
                    <div class="form-col">
                        <div class="form-group">
                            <label for="ddlBloodType" class="required-field">Blood Type</label>
                            <asp:DropDownList ID="ddlBloodType" runat="server" CssClass="form-control">
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
                            <asp:RequiredFieldValidator ID="rfvBloodType" runat="server" ControlToValidate="ddlBloodType"
                                ErrorMessage="Blood type is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtWeight" class="required-field">Weight (kg)</label>
                            <asp:TextBox ID="txtWeight" runat="server" CssClass="form-control" placeholder="Enter weight (e.g., 75.5)" />
                            <asp:RequiredFieldValidator ID="rfvWeight" runat="server" ControlToValidate="txtWeight"
                                ErrorMessage="Weight is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                            <asp:RegularExpressionValidator ID="revWeight" runat="server" ControlToValidate="txtWeight"
                                ValidationExpression="^\d+(\.\d{1,2})?$" ErrorMessage="Enter a valid weight (e.g., 75.5)" Display="Dynamic" CssClass="text-danger"></asp:RegularExpressionValidator>
                        </div>
                    </div>
                </div>
                
                <div class="form-group">
                    <label for="txtHealthConditions">Health Conditions</label>
                    <asp:TextBox ID="txtHealthConditions" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" />
                </div>
                
                <div class="form-group">
                    <label for="txtLastDonationDate">Last Donation Date</label>
                    <asp:TextBox ID="txtLastDonationDate" runat="server" CssClass="form-control" TextMode="Date" />
                </div>
                
                <div class="form-group" id="passwordSection" runat="server">
                    <label for="txtDonorPassword" class="required-field">Password</label>
                    <asp:TextBox ID="txtDonorPassword" runat="server" CssClass="form-control" TextMode="Password" />
                    <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtDonorPassword"
                        ErrorMessage="Password is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                </div>
                
                <div class="form-group">
                    <asp:CheckBox ID="cbIsActive" runat="server" Text=" Active Donor" Checked="true" />
                </div>
                
                <div class="form-group">
                    <asp:Button ID="btnSaveDonor" runat="server" Text="Save Donor" CssClass="btn btn-primary" OnClick="btnSaveDonor_Click" />
                    <asp:Button ID="btnClearForm" runat="server" Text="Clear Form" CssClass="btn btn-secondary" OnClick="btnClearForm_Click" CausesValidation="false" />
                </div>
            </div>
            <div class="content-section">
                <div class="section-header">
                    <div class="section-title">Donor List</div>
                   <!-- Search Controls -->
<div class="form-group">
    <label for="ddlSearchBy">Search By:</label>
    <asp:DropDownList ID="ddlSearchBy" runat="server" CssClass="form-control">
        <asp:ListItem Value="all">All Fields</asp:ListItem>
        <asp:ListItem Value="name">Name</asp:ListItem>
        <asp:ListItem Value="email">Email</asp:ListItem>
        <asp:ListItem Value="phone">Phone</asp:ListItem>
        <asp:ListItem Value="blood_type">Blood Type</asp:ListItem>
        <asp:ListItem Value="gender">Gender</asp:ListItem>
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
</div>
<!-- Message Panel -->
<asp:Panel ID="Panel1" runat="server" Visible="false">
    <asp:Label ID="Label1" runat="server"></asp:Label>
</asp:Panel>

<!-- Donors GridView -->
<asp:GridView ID="gvDonors" runat="server" AutoGenerateColumns="False" DataKeyNames="donor_id" 
    OnRowEditing="gvDonors_RowEditing" OnRowDeleting="gvDonors_RowDeleting" 
    OnPageIndexChanging="gvDonors_PageIndexChanging" OnRowDataBound="gvDonors_RowDataBound"
    CssClass="table table-striped" AllowPaging="True" PageSize="10">
    <Columns>
        <asp:BoundField DataField="first_name" HeaderText="First Name" />
        <asp:BoundField DataField="last_name" HeaderText="Last Name" />
        <asp:BoundField DataField="email" HeaderText="Email" />
        <asp:BoundField DataField="phone" HeaderText="Phone" />
        <asp:BoundField DataField="blood_type" HeaderText="Blood Type" />
        <asp:BoundField DataField="weight" HeaderText="Weight" />
        <asp:BoundField DataField="date_of_birth" HeaderText="Date of Birth" DataFormatString="{0:yyyy-MM-dd}" />
        <asp:BoundField DataField="gender" HeaderText="Gender" />
        <asp:TemplateField HeaderText="Active">
            <ItemTemplate>
                <asp:CheckBox ID="chkActive" runat="server" Checked='<%# Eval("is_active") %>' 
                    AutoPostBack="true" OnCheckedChanged="chkActive_CheckedChanged" />
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