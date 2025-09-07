
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ManageDonationAppointments.aspx.cs" Inherits="ClinicalBloodBank.ManageDonationAppointments" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Clinical Blood Bank - Manage Donation Appointments</title>
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
        .header-actions {
            display: flex;
            align-items: center;
            gap: 15px;
        }
        .notification-container {
            position: relative;
        }
        .notification-bell {
            position: relative;
            cursor: pointer;
            font-size: 20px;
            color: #2c3e50;
            padding: 8px;
            border-radius: 50%;
            transition: background-color 0.3s;
        }
        .notification-bell:hover {
            background-color: #f0f0f0;
        }
        .notification-badge {
            position: absolute;
            top: 0;
            right: 0;
            background: #d32f2f;
            color: white;
            border-radius: 50%;
            width: 18px;
            height: 18px;
            font-size: 12px;
            display: flex;
            align-items: center;
            justify-content: center;
        }
        .notification-dropdown {
            position: absolute;
            top: 45px;
            right: 0;
            width: 350px;
            background: white;
            border-radius: 8px;
            box-shadow: 0 4px 20px rgba(0,0,0,0.15);
            z-index: 1000;
            display: none;
        }
        .notification-dropdown.show {
            display: block;
        }
        .notification-header {
            padding: 15px;
            border-bottom: 1px solid #eee;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }
        .notification-title {
            font-weight: 600;
            color: #2c3e50;
        }
        .clear-all-btn {
            background: none;
            border: none;
            color: #d32f2f;
            cursor: pointer;
            font-size: 14px;
        }
        .clear-all-btn:hover {
            text-decoration: underline;
        }
        .notification-list {
            max-height: 300px;
            overflow-y: auto;
        }
        .notification-item {
            padding: 12px 15px;
            border-bottom: 1px solid #f0f0f0;
            display: flex;
            align-items: flex-start;
        }
        .notification-item:last-child {
            border-bottom: none;
        }
        .notification-item.unread {
            background-color: #f8f9fa;
        }
        .notification-icon {
            margin-right: 10px;
            color: #d32f2f;
            font-size: 16px;
        }
        .notification-content {
            flex: 1;
        }
        .notification-message {
            font-size: 14px;
            margin-bottom: 5px;
            color: #2c3e50;
        }
        .notification-time {
            font-size: 12px;
            color: #7f8c8d;
        }
        .user-profile {
            display: flex;
            align-items: center;
            gap: 10px;
            position: relative;
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
            cursor: pointer;
        }
        .profile-dropdown {
            position: absolute;
            top: 50px;
            right: 0;
            background: white;
            border-radius: 5px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
            width: 150px;
            z-index: 100;
            display: none;
        }
        .profile-dropdown.show {
            display: block;
        }
        .profile-dropdown-item {
            padding: 10px 15px;
            display: block;
            color: #2c3e50;
            text-decoration: none;
            transition: background 0.3s;
        }
        .profile-dropdown-item:hover {
            background: #f8f9fa;
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
        .status-scheduled {
            background: #e8f5e9;
            color: #2e7d32;
        }
        .status-no-show {
            background: #ffebee;
            color: #d32f2f;
        }
        .status-successful {
            background: #e3f2fd;
            color: #0288d1;
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
                <a href="ManageDonationAppointments.aspx" class="menu-item active">
                    <span class="menu-icon">📅</span> Manage Appointments
                </a>
                <a href="HospitalProfile.aspx" class="menu-item">
                    <span class="menu-icon">🏥</span> Profile
                </a>
                <a href="HospitalReports.aspx" class="menu-item">S
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
                    <h1>Welcome, <asp:Literal ID="litUserName" runat="server" Text="Hospital"></asp:Literal></h1>
                    <p>Manage Donation Appointments</p>
                </div>
                <div class="header-actions">
                    <div class="notification-container">
                        <div class="notification-bell" id="notificationBell">
                            🔔
                            <span class="notification-badge" id="notificationCount" runat="server">0</span>
                        </div>
                        <div class="notification-dropdown" id="notificationDropdown">
                            <div class="notification-header">
                                <div class="notification-title">Notifications</div>
                                <asp:LinkButton ID="btnClearAll" runat="server" CssClass="clear-all-btn" OnClick="btnClearAll_Click">Clear All</asp:LinkButton>
                            </div>
                            <div class="notification-list" id="notificationList" runat="server">
                                <div class="no-notifications">Loading notifications...</div>
                            </div>
                        </div>
                    </div>
                    <div class="user-profile">
                        <div class="user-avatar" id="userAvatar">
                            <asp:Literal ID="litUserInitials" runat="server" Text="HD"></asp:Literal>
                        </div>
                        <div class="profile-dropdown" id="profileDropdown">
                            <a href="HospitalProfile.aspx" class="profile-dropdown-item">Profile</a>
                            <asp:LinkButton ID="lnkProfileLogout" runat="server" CssClass="profile-dropdown-item" OnClick="lnkLogout_Click">Logout</asp:LinkButton>
                        </div>
                    </div>
                </div>
            </div>

            <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert">
                <asp:Label ID="lblMessage" runat="server" Text=""></asp:Label>
            </asp:Panel>

            <div class="content-section">
                <div class="section-header">
                    <div class="section-title">Add/Edit Appointment</div>
                </div>
                <asp:HiddenField ID="hdnAppointmentId" runat="server" />
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
                            <label for="txtAppointmentDate" class="required-field">Appointment Date</label>
                            <asp:TextBox ID="txtAppointmentDate" runat="server" CssClass="form-control" TextMode="DateTimeLocal" />
                            <asp:RequiredFieldValidator ID="rfvAppointmentDate" runat="server" ControlToValidate="txtAppointmentDate"
                                ErrorMessage="Appointment date is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                </div>
                <div class="form-row">
                    <div class="form-col">
                        <div class="form-group">
                            <label for="ddlStatus" class="required-field">Status</label>
                            <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-control">
                                <asp:ListItem Value="scheduled">Scheduled</asp:ListItem>
                                <asp:ListItem Value="no-show">No Show</asp:ListItem>
                                <asp:ListItem Value="successful">Successful</asp:ListItem>
                            </asp:DropDownList>
                            <asp:RequiredFieldValidator ID="rfvStatus" runat="server" ControlToValidate="ddlStatus"
                                ErrorMessage="Status is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                </div>
                <div class="form-group">
                    <asp:Button ID="btnSaveAppointment" runat="server" Text="Save Appointment" CssClass="btn btn-primary" OnClick="btnSaveAppointment_Click" />
                    <asp:Button ID="btnClearForm" runat="server" Text="Clear Form" CssClass="btn btn-secondary" OnClick="btnClearForm_Click" CausesValidation="false" />
                </div>
            </div>

            <div class="content-section">
                <div class="section-header">
                    <div class="section-title">Filter Appointments</div>
                </div>
                <div class="form-row">
                    <div class="form-col">
                        <div class="form-group">
                            <label for="ddlFilterDonor">Donor</label>
                            <asp:DropDownList ID="ddlFilterDonor" runat="server" CssClass="form-control" />
                        </div>
                    </div>
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtFilterAppointmentDate">Appointment Date</label>
                            <asp:TextBox ID="txtFilterAppointmentDate" runat="server" CssClass="form-control" TextMode="Date" />
                        </div>
                    </div>
                    <div class="form-col">
                        <div class="form-group">
                            <label for="ddlFilterStatus">Status</label>
                            <asp:DropDownList ID="ddlFilterStatus" runat="server" CssClass="form-control">
                                <asp:ListItem Value="">All Statuses</asp:ListItem>
                                <asp:ListItem Value="scheduled">Scheduled</asp:ListItem>
                                <asp:ListItem Value="no-show">No Show</asp:ListItem>
                                <asp:ListItem Value="successful">Successful</asp:ListItem>
                            </asp:DropDownList>
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
                    <div class="section-title">Appointment List</div>
                </div>
                <asp:GridView ID="gvAppointments" runat="server" AutoGenerateColumns="False" CssClass="table" DataKeyNames="appointment_id"
                    OnRowEditing="gvAppointments_RowEditing" OnRowDeleting="gvAppointments_RowDeleting"
                    OnPageIndexChanging="gvAppointments_PageIndexChanging" OnRowDataBound="gvAppointments_RowDataBound"
                    AllowPaging="True" PageSize="10">
                    <Columns>
                        <asp:BoundField DataField="appointment_id" HeaderText="ID" ReadOnly="True" />
                        <asp:BoundField DataField="donor_name" HeaderText="Donor" />
                        <asp:BoundField DataField="blood_type" HeaderText="Blood Type" />
                        <asp:BoundField DataField="appointment_date" HeaderText="Appointment Date" DataFormatString="{0:yyyy-MM-dd HH:mm}" />
                        <asp:TemplateField HeaderText="Status">
                            <ItemTemplate>
                                <span class='status-badge <%# Eval("status").ToString() == "scheduled" ? "status-scheduled" : Eval("status").ToString() == "no-show" ? "status-no-show" : "status-successful" %>'>
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

        <script>
            document.addEventListener('DOMContentLoaded', function () {
                var userAvatar = document.getElementById('userAvatar');
                var profileDropdown = document.getElementById('profileDropdown');
                var notificationBell = document.getElementById('notificationBell');
                var notificationDropdown = document.getElementById('notificationDropdown');

                if (userAvatar && profileDropdown) {
                    userAvatar.addEventListener('click', function (e) {
                        e.stopPropagation();
                        profileDropdown.classList.toggle('show');
                        if (notificationDropdown) notificationDropdown.classList.remove('show');
                    });
                }

                if (notificationBell && notificationDropdown) {
                    notificationBell.addEventListener('click', function (e) {
                        e.stopPropagation();
                        notificationDropdown.classList.toggle('show');
                        if (profileDropdown) profileDropdown.classList.remove('show');
                    });
                }

                document.addEventListener('click', function (event) {
                    if (profileDropdown && userAvatar && !userAvatar.contains(event.target) && !profileDropdown.contains(event.target)) {
                        profileDropdown.classList.remove('show');
                    }
                    if (notificationDropdown && notificationBell && !notificationBell.contains(event.target) && !notificationDropdown.contains(event.target)) {
                        notificationDropdown.classList.remove('show');
                    }
                });
            });
        </script>
    </form>
</body>
</html>
