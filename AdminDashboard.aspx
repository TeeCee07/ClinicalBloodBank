<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdminDashboard.aspx.cs" Inherits="ClinicalBloodBank.AdminDashboard" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Clinical Blood Bank - Admin Dashboard</title>
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
        
        .notification-footer {
            padding: 10px 15px;
            text-align: center;
            border-top: 1px solid #eee;
        }
        
        .view-all-link {
            color: #d32f2f;
            text-decoration: none;
            font-size: 14px;
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
        
        .dashboard-cards {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 20px;
            margin-bottom: 30px;
        }
        
        .card {
            background: white;
            border-radius: 10px;
            padding: 20px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }
        
        .card-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 15px;
        }
        
        .card-title {
            color: #2c3e50;
            font-size: 18px;
            font-weight: 600;
        }
        
        .card-icon {
            width: 40px;
            height: 40px;
            border-radius: 10px;
            background: rgba(211, 47, 47, 0.1);
            color: #d32f2f;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 18px;
        }
        
        .card-value {
            font-size: 24px;
            font-weight: bold;
            color: #2c3e50;
            margin-bottom: 5px;
        }
        
        .card-label {
            color: #7f8c8d;
            font-size: 14px;
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
        
        .no-notifications {
            padding: 20px;
            text-align: center;
            color: #7f8c8d;
        }
        
        .no-data {
            padding: 20px;
            text-align: center;
            color: #7f8c8d;
            font-style: italic;
        }
        
        .error-message {
            background-color: #ffebee;
            color: #c62828;
            padding: 10px;
            border-radius: 4px;
            margin: 10px 0;
            display: none;
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
                <a href="Logout.aspx" class="menu-item">
                    <span class="menu-icon">🚪</span> Logout
                </a>
            </div>
        </div>

        <div class="main-content">
            <div class="header">
                <div class="welcome-text">
                    <h1>Welcome, <asp:Literal ID="litUserName" runat="server" Text="Admin"></asp:Literal></h1>
                    <p>Admin Dashboard</p>
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
                            <div class="notification-footer">
                                <a href="ViewAllNotifications.aspx" class="view-all-link">View All Notifications</a>
                            </div>
                        </div>
                    </div>
                    <div class="user-profile">
                        <div class="user-avatar" id="userAvatar">
                            <asp:Literal ID="litUserInitials" runat="server" Text="AD"></asp:Literal>
                        </div>
                        <span><asp:Literal ID="litUserRole" runat="server" Text="Admin"></asp:Literal></span>
                        <div class="profile-dropdown" id="profileDropdown">
                            <a href="AdminProfile.aspx" class="profile-dropdown-item">Profile</a>
                            <a href="Logout.aspx" class="profile-dropdown-item">Logout</a>
                        </div>
                    </div>
                </div>
            </div>

            <div class="error-message" id="errorMessage" runat="server"></div>

            <div class="dashboard-cards">
                <div class="card">
                    <div class="card-header">
                        <div class="card-title">Total Donors</div>
                        <div class="card-icon">👤</div>
                    </div>
                    <div class="card-value">
                        <asp:Literal ID="litTotalDonors" runat="server" Text="0"></asp:Literal>
                    </div>
                    <div class="card-label">Active Donors</div>
                </div>
                <div class="card">
                    <div class="card-header">
                        <div class="card-title">Total Hospitals</div>
                        <div class="card-icon">🏥</div>
                    </div>
                    <div class="card-value">
                        <asp:Literal ID="litTotalHospitals" runat="server" Text="0"></asp:Literal>
                    </div>
                    <div class="card-label">Verified Hospitals</div>
                </div>
                <div class="card">
                    <div class="card-header">
                        <div class="card-title">Blood Inventory</div>
                        <div class="card-icon">🩺</div>
                    </div>
                    <div class="card-value">
                        <asp:Literal ID="litTotalInventory" runat="server" Text="0"></asp:Literal> ml
                    </div>
                    <div class="card-label">Available Blood</div>
                </div>
                <div class="card">
                    <div class="card-header">
                        <div class="card-title">Pending Requests</div>
                        <div class="card-icon">📋</div>
                    </div>
                    <div class="card-value">
                        <asp:Literal ID="litPendingRequests" runat="server" Text="0"></asp:Literal>
                    </div>
                    <div class="card-label">Blood Requests</div>
                </div>
            </div>

            <div class="content-section">
                <div class="section-header">
                    <div class="section-title">Recent Notifications</div>
                </div>
                <asp:Repeater ID="rptNotifications" runat="server">
                    <HeaderTemplate>
                        <table class="table">
                            <tr>
                                <th>Title</th>
                                <th>Message</th>
                                <th>Date</th>
                                <th>Status</th>
                            </tr>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td><%# Eval("title") %></td>
                            <td><%# Eval("message") %></td>
                            <td><%# Eval("created_at", "{0:yyyy-MM-dd HH:mm}") %></td>
                            <td><%# Convert.ToBoolean(Eval("is_read")) ? "Read" : "Unread" %></td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                        </table>
                    </FooterTemplate>
                </asp:Repeater>
                <asp:Label ID="lblNoNotifications" runat="server" Text="No notifications found." CssClass="no-data" Visible="false"></asp:Label>
            </div>
        </div>
        
        <script>
            // JavaScript to toggle profile dropdown
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

                // Close dropdowns when clicking outside
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