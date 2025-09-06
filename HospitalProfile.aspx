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
                    <span class="menu-icon">📋</span> Manage Requests
                </a>
                <a href="ManageDonationAppointments.aspx" class="menu-item">
                    <span class="menu-icon">📅</span> Manage Appointments
                </a>
                <a href="HospitalProfile.aspx" class="menu-item active">
                    <span class="menu-icon">🏥</span> Profile
                </a>
                <a href="HospitalReports.aspx" class="menu-item">
                    <span class="menu-icon">📊</span> Reports
                </a>
                <a href="Notifications.aspx" class="menu-item">
                    <span class="menu-icon">🔔</span> Notifications
                </a>
                <asp:LinkButton ID="lnkLogout" runat="server" CssClass="menu-item" OnClick="lnkLogout_Click">
                    <span class="menu-icon">🚪</span> Logout
                </asp:LinkButton>
            </div>
        </div>

        <div class="main-content">
            <div class="header">
                <div class="welcome-text">
                    <h1>Welcome, <asp:Literal ID="litUserName" runat="server" Text="Hospital"></asp:Literal></h1>
                    <p>Hospital Profile</p>
                </div>
                <div class="user-profile">
                    <div class="user-avatar">
                        <asp:Literal ID="litUserInitials" runat="server" Text="HD"></asp:Literal>
                    </div>
                </div>
            </div>

            <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert">
                <asp:Label ID="lblMessage" runat="server" Text=""></asp:Label>
            </asp:Panel>

            <div class="content-section">
                <div class="section-header">
                    <div class="section-title">Update Profile</div>
                </div>
                <div class="form-group">
                    <label for="txtHospitalName" class="required-field">Hospital Name</label>
                    <asp:TextBox ID="txtHospitalName" runat="server" CssClass="form-control"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvHospitalName" runat="server" ControlToValidate="txtHospitalName"
                        ErrorMessage="Hospital name is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                </div>
                <div class="form-group">
                    <label for="txtEmail" class="required-field">Contact Email</label>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail"
                        ErrorMessage="Email is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                    <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail"
                        ErrorMessage="Invalid email format" Display="Dynamic" CssClass="text-danger"
                        ValidationExpression="^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$"></asp:RegularExpressionValidator>
                </div>
                <div class="form-group">
                    <label for="txtPhone">Phone Number</label>
                    <asp:TextBox ID="txtPhone" runat="server" CssClass="form-control" TextMode="Phone"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label for="txtAddress">Address</label>
                    <asp:TextBox ID="txtAddress" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label for="txtPassword" class="required-field">Password</label>
                    <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtPassword"
                        ErrorMessage="Password is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                </div>
                <div class="form-group">
                    <asp:Button ID="btnUpdateProfile" runat="server" Text="Update Profile" CssClass="btn btn-primary" OnClick="btnUpdateProfile_Click" />
                </div>
            </div>
        </div>
    </form>
</body>
</html>