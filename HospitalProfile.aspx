<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HospitalProfile.aspx.cs" Inherits="ClinicalBloodBank.HospitalProfile" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Clinical Blood Bank - Hospital Profile</title>
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
                <a href="HospitalDashboard.aspx" class="menu-item">
                    <span class="menu-icon">🏠</span> Dashboard
                </a>
                <a href="HospitalManageInventory.aspx" class="menu-item">
                    <span class="menu-icon">🩺</span> Blood Inventory
                </a>
                <a href="ManageRequests.aspx" class="menu-item">
                    <span class="menu-icon">🩺</span> Blood Requests
                </a>
                <a href="ManageAppointments.aspx" class="menu-item">
                    <span class="menu-icon">📅</span> Appointments
                </a>
                <a href="Reports.aspx" class="menu-item">
                    <span class="menu-icon">📊</span> Reports
                </a>
                <a href="HospitalProfile.aspx" class="menu-item active">
                    <span class="menu-icon">👤</span> Profile
                </a>
                <a href="Logout.aspx" class="menu-item">
                    <span class="menu-icon">🚪</span> Logout
                </a>
            </div>
        </div>

        <div class="main-content">
            <div class="header">
                <div class="welcome-text">
                    <h1>Hospital Profile</h1>
                    <p>Hospital Panel</p>
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
                    <div class="section-title">Profile Details</div>
                </div>
                <div class="form-row">
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtHospitalId">Hospital ID</label>
                            <asp:TextBox ID="txtHospitalId" runat="server" CssClass="form-control" ReadOnly="true" />
                        </div>
                    </div>
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtHospitalName" class="required-field">Hospital Name</label>
                            <asp:TextBox ID="txtHospitalName" runat="server" CssClass="form-control" />
                            <asp:RequiredFieldValidator ID="rfvHospitalName" runat="server" ControlToValidate="txtHospitalName"
                                ErrorMessage="Hospital name is required" Display="Dynamic" CssClass="text-danger" />
                        </div>
                    </div>
                </div>
                <div class="form-row">
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtContactEmail" class="required-field">Contact Email</label>
                            <asp:TextBox ID="txtContactEmail" runat="server" CssClass="form-control" TextMode="Email" />
                            <asp:RequiredFieldValidator ID="rfvContactEmail" runat="server" ControlToValidate="txtContactEmail"
                                ErrorMessage="Contact email is required" Display="Dynamic" CssClass="text-danger" />
                            <asp:RegularExpressionValidator ID="revContactEmail" runat="server" ControlToValidate="txtContactEmail"
                                ErrorMessage="Invalid email format" Display="Dynamic" CssClass="text-danger"
                                ValidationExpression="^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$" />
                        </div>
                    </div>
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtLicenseNumber" class="required-field">License Number</label>
                            <asp:TextBox ID="txtLicenseNumber" runat="server" CssClass="form-control" />
                            <asp:RequiredFieldValidator ID="rfvLicenseNumber" runat="server" ControlToValidate="txtLicenseNumber"
                                ErrorMessage="License number is required" Display="Dynamic" CssClass="text-danger" />
                        </div>
                    </div>
                </div>
                <div class="form-row">
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtAddressLine1">Address Line 1</label>
                            <asp:TextBox ID="txtAddressLine1" runat="server" CssClass="form-control" />
                        </div>
                    </div>
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtAddressLine2">Address Line 2</label>
                            <asp:TextBox ID="txtAddressLine2" runat="server" CssClass="form-control" />
                        </div>
                    </div>
                </div>
                <div class="form-row">
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtCity" class="required-field">City</label>
                            <asp:TextBox ID="txtCity" runat="server" CssClass="form-control" />
                            <asp:RequiredFieldValidator ID="rfvCity" runat="server" ControlToValidate="txtCity"
                                ErrorMessage="City is required" Display="Dynamic" CssClass="text-danger" />
                        </div>
                    </div>
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtProvince" class="required-field">Province</label>
                            <asp:TextBox ID="txtProvince" runat="server" CssClass="form-control" />
                            <asp:RequiredFieldValidator ID="rfvProvince" runat="server" ControlToValidate="txtProvince"
                                ErrorMessage="Province is required" Display="Dynamic" CssClass="text-danger" />
                        </div>
                    </div>
                </div>
                <div class="form-row">
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtPostalCode">Postal Code</label>
                            <asp:TextBox ID="txtPostalCode" runat="server" CssClass="form-control" />
                        </div>
                    </div>
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtCountry">Country</label>
                            <asp:TextBox ID="txtCountry" runat="server" CssClass="form-control" ReadOnly="true" Text="South Africa" />
                        </div>
                    </div>
                </div>
                <div class="form-group">
                    <asp:Button ID="btnUpdateProfile" runat="server" Text="Update Profile" CssClass="btn btn-primary" OnClick="btnUpdateProfile_Click" />
                </div>
            </div>

            <div class="content-section">
                <div class="section-header">
                    <div class="section-title">Change Password</div>
                </div>
                <div class="form-row">
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtCurrentPassword" class="required-field">Current Password</label>
                            <asp:TextBox ID="txtCurrentPassword" runat="server" CssClass="form-control" TextMode="Password" />
                            <asp:RequiredFieldValidator ID="rfvCurrentPassword" runat="server" ControlToValidate="txtCurrentPassword"
                                ErrorMessage="Current password is required" Display="Dynamic" CssClass="text-danger" />
                        </div>
                    </div>
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtNewPassword" class="required-field">New Password</label>
                            <asp:TextBox ID="txtNewPassword" runat="server" CssClass="form-control" TextMode="Password" />
                            <asp:RequiredFieldValidator ID="rfvNewPassword" runat="server" ControlToValidate="txtNewPassword"
                                ErrorMessage="New password is required" Display="Dynamic" CssClass="text-danger" />
                            <asp:RegularExpressionValidator ID="revNewPassword" runat="server" ControlToValidate="txtNewPassword"
                                ErrorMessage="Password must be at least 8 characters, including 1 uppercase, 1 lowercase, and 1 number"
                                Display="Dynamic" CssClass="text-danger"
                                ValidationExpression="^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$" />
                        </div>
                    </div>
                </div>
                <div class="form-group">
                    <label for="txtConfirmPassword" class="required-field">Confirm New Password</label>
                    <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="form-control" TextMode="Password" />
                    <asp:RequiredFieldValidator ID="rfvConfirmPassword" runat="server" ControlToValidate="txtConfirmPassword"
                        ErrorMessage="Confirm password is required" Display="Dynamic" CssClass="text-danger" />
                    <asp:CompareValidator ID="cvConfirmPassword" runat="server" ControlToValidate="txtConfirmPassword"
                        ControlToCompare="txtNewPassword" ErrorMessage="Passwords do not match" Display="Dynamic" CssClass="text-danger" />
                </div>
                <div class="form-group">
                    <asp:Button ID="btnChangePassword" runat="server" Text="Change Password" CssClass="btn btn-primary" OnClick="btnChangePassword_Click" />
                </div>
            </div>
        </div>
    </form>
</body>
</html>