<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="BookAppointment.aspx.cs" Inherits="ClinicalBloodBank.BookAppointment" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Clinical Blood Bank - Book Appointment</title>
    <!-- Reuse all the styles from DonorDashboard.aspx -->
    <style>
        /* All the same styles as DonorDashboard.aspx */
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
            box-shadow: 0 2px 10px rgba(0,0,0,0.15);
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

        .blood-type-badge {
            display: inline-block;
            padding: 4px 8px;
            border-radius: 4px;
            font-weight: bold;
            color: white;
        }

        .blood-type-A-plus { background-color: #d32f2f; }
        .blood-type-A-minus { background-color: #c2185b; }
        .blood-type-B-plus { background-color: #7b1fa2; }
        .blood-type-B-minus { background-color: #512da8; }
        .blood-type-AB-plus { background-color: #303f9f; }
        .blood-type-AB-minus { background-color: #1976d2; }
        .blood-type-O-plus { background-color: #0288d1; }
        .blood-type-O-minus { background-color: #0097a7; }
        .blood-type-Unknown { background-color: #7f8c8d; }

        .status-badge {
            display: inline-block;
            padding: 4px 8px;
            border-radius: 4px;
            font-weight: bold;
        }

        .status-available { background-color: #e8f5e9; color: #2e7d32; }
        .status-reserved { background-color: #fff8e1; color: #f57c00; }
        .status-used { background-color: #fbe9e7; color: #d32f2f; }
        .status-expired { background-color: #f5f5f5; color: #9e9e9e; }
        .status-pending { background-color: #e3f2fd; color: #1976d2; }
        .status-approved { background-color: #e8f5e9; color: #2e7d32; }
        .status-rejected { background-color: #ffebee; color: #d32f2f; }
        .status-fulfilled { background-color: #f3e5f5; color: #7b1fa2; }
        .status-scheduled { background-color: #e3f2fd; color: #1976d2; }
        .status-completed { background-color: #e8f5e9; color: #2e7d32; }
        .status-cancelled { background-color: #f5f5f5; color: #9e9e9e; }
        .status-no_show { background-color: #ffebee; color: #d32f2f; }

        .btn {
            padding: 8px 16px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            font-weight: 500;
            transition: background-color 0.3s;
        }

        .btn-primary {
            background-color: #d32f2f;
            color: white;
        }

        .btn-primary:hover {
            background-color: #b71c1c;
        }

        .btn-secondary {
            background-color: #f0f0f0;
            color: #2c3e50;
        }

        .btn-secondary:hover {
            background-color: #e0e0e0;
        }

        /* Appointment booking styles */
        .booking-container {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 20px;
        }

        .booking-form {
            background: white;
            border-radius: 10px;
            padding: 20px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }

        .form-group {
            margin-bottom: 20px;
        }

        .form-label {
            display: block;
            margin-bottom: 5px;
            font-weight: 500;
            color: #2c3e50;
        }

        .form-control {
            width: 100%;
            padding: 10px;
            border: 1px solid #ddd;
            border-radius: 4px;
            font-size: 14px;
        }

        .form-control:focus {
            border-color: #d32f2f;
            outline: none;
        }

        .form-select {
            width: 100%;
            padding: 10px;
            border: 1px solid #ddd;
            border-radius: 4px;
            background-color: white;
            font-size: 14px;
        }

        .calendar-container {
            background: white;
            border-radius: 10px;
            padding: 20px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }

        .calendar-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 15px;
        }

        .calendar-grid {
            display: grid;
            grid-template-columns: repeat(7, 1fr);
            gap: 5px;
        }

        .calendar-day {
            text-align: center;
            padding: 10px;
            font-weight: 600;
            color: #2c3e50;
        }

        .calendar-date {
            text-align: center;
            padding: 10px;
            border-radius: 4px;
            cursor: pointer;
            transition: background-color 0.3s;
        }

        .calendar-date:hover {
            background-color: #f0f0f0;
        }

        .calendar-date.selected {
            background-color: #d32f2f;
            color: white;
        }

        .calendar-date.disabled {
            color: #ccc;
            cursor: not-allowed;
        }

        .time-slots {
            display: grid;
            grid-template-columns: repeat(4, 1fr);
            gap: 10px;
            margin-top: 20px;
        }

        .time-slot {
            padding: 10px;
            border: 1px solid #ddd;
            border-radius: 4px;
            text-align: center;
            cursor: pointer;
            transition: background-color 0.3s;
        }

        .time-slot:hover {
            background-color: #f0f0f0;
        }

        .time-slot.selected {
            background-color: #d32f2f;
            color: white;
            border-color: #d32f2f;
        }

        .time-slot.disabled {
            color: #ccc;
            cursor: not-allowed;
        }

        .eligibility-check {
            background-color: #e8f5e9;
            padding: 15px;
            border-radius: 4px;
            margin-bottom: 20px;
        }

        .eligibility-check.fail {
            background-color: #ffebee;
        }

        @media (max-width: 768px) {
            .sidebar {
                width: 100%;
                height: auto;
                position: relative;
            }
            
            .main-content {
                margin-left: 0;
            }
            
            .dashboard-cards {
                grid-template-columns: 1fr;
            }
            
            .header {
                flex-direction: column;
                align-items: flex-start;
                gap: 15px;
            }
            
            .header-actions {
                width: 100%;
                justify-content: space-between;
            }

            .booking-container {
                grid-template-columns: 1fr;
            }

            .time-slots {
                grid-template-columns: repeat(2, 1fr);
            }
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
                <a href="DonorDashboard.aspx" class="menu-item">
                    <span class="menu-icon">🏠</span> Dashboard
                </a>
                <a href="DonorProfile.aspx" class="menu-item">
                    <span class="menu-icon">👤</span> My Profile
                </a>
                <a href="DonationHistory.aspx" class="menu-item">
                    <span class="menu-icon">📋</span> Donation History
                </a>
                <a href="BookAppointment.aspx" class="menu-item active">
                    <span class="menu-icon">📅</span> Book Appointment
                </a>
               
                                      <asp:LinkButton ID="lnkLogout" runat="server" CssClass="menu-item" OnClick="lnkLogout_Click">
    <span class="menu-icon">🚪</span> Logout
</asp:LinkButton>
                
            </div>
        </div>

        <div class="main-content">
            <div class="header">
                <div class="welcome-text">
                    <h1>Book Appointment</h1>
                    <p>Schedule your blood donation appointment</p>
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
                            <asp:Literal ID="litUserInitials" runat="server" Text="DO"></asp:Literal>
                        </div>
                        <span>Donor</span>
                        <div class="profile-dropdown" id="profileDropdown">
                            <a href="DonorProfile.aspx" class="profile-dropdown-item">Profile</a>
                            <a href="Logout.aspx" class="profile-dropdown-item">Logout</a>
                        </div>
                    </div>
                </div>
            </div>

            <div class="error-message" id="errorMessage" runat="server"></div>
            <div class="success-message" id="successMessage" runat="server" style="display: none; background-color: #e8f5e9; color: #2e7d32; padding: 10px; border-radius: 4px; margin: 10px 0;"></div>

            <div class="eligibility-check" id="eligibilityCheck" runat="server">
                <h3>Eligibility Check</h3>
                <p>Based on your last donation date, you are eligible to donate blood.</p>
            </div>

            <div class="booking-container">
                <div class="booking-form">
                    <div class="section-header">
                        <div class="section-title">Appointment Details</div>
                    </div>

                    <div class="form-group">
                        <label class="form-label">Select Hospital</label>
                        <asp:DropDownList ID="ddlHospital" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlHospital_SelectedIndexChanged">
                        </asp:DropDownList>
                    </div>

                    <div class="form-group">
                        <label class="form-label">Preferred Date</label>
                        <asp:TextBox ID="txtAppointmentDate" runat="server" CssClass="form-control" TextMode="Date" min="" OnTextChanged="txtAppointmentDate_TextChanged" AutoPostBack="true"></asp:TextBox>
                    </div>

                    <div class="form-group">
                        <label class="form-label">Preferred Time</label>
                        <asp:DropDownList ID="ddlTimeSlot" runat="server" CssClass="form-select">
                        </asp:DropDownList>
                    </div>

                    <div class="form-group">
                        <label class="form-label">Additional Notes (Optional)</label>
                        <asp:TextBox ID="txtNotes" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3"></asp:TextBox>
                    </div>

                    <asp:Button ID="btnBookAppointment" runat="server" Text="Book Appointment" CssClass="btn btn-primary" OnClick="btnBookAppointment_Click" />
                </div>

                <div class="calendar-container">
                    <div class="section-header">
                        <div class="section-title">Available Dates</div>
                    </div>
                    <div id="calendar" runat="server">
                        <!-- Calendar will be generated dynamically -->
                    </div>
                </div>
            </div>

            <div class="content-section" id="upcomingAppointments" runat="server" visible="false">
                <div class="section-header">
                    <div class="section-title">Your Upcoming Appointments</div>
                </div>
                <asp:Repeater ID="rptUpcomingAppointments" runat="server">
                    <HeaderTemplate>
                        <table class="table">
                            <tr>
                                <th>Date & Time</th>
                                <th>Hospital</th>
                                <th>Status</th>
                                <th>Actions</th>
                            </tr>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td><%# Eval("appointment_date", "{0:yyyy-MM-dd HH:mm}") %></td>
                            <td><%# Eval("hospital_name") %></td>
                            <td><span class='status-badge status-<%# Eval("status") %>'><%# Eval("status") %></span></td>
                            <td>
                                <asp:LinkButton ID="btnCancel" runat="server" CssClass="btn btn-secondary" 
                                    CommandArgument='<%# Eval("appointment_id") %>' 
                                    OnCommand="CancelAppointment" 
                                    Visible='<%# Eval("status").ToString() == "scheduled" %>'>
                                    Cancel
                                </asp:LinkButton>
                            </td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                        </table>
                    </FooterTemplate>
                </asp:Repeater>
                <asp:Label ID="lblNoUpcomingAppointments" runat="server" Text="No upcoming appointments." CssClass="no-data" Visible="false"></asp:Label>
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

                // Set minimum date for appointment date picker
                var today = new Date();
                var dd = String(today.getDate()).padStart(2, '0');
                var mm = String(today.getMonth() + 1).padStart(2, '0'); //January is 0!
                var yyyy = today.getFullYear();

                today = yyyy + '-' + mm + '-' + dd;
                document.getElementById('<%= txtAppointmentDate.ClientID %>').min = today;
            });
        </script>
    </form>
</body>
</html>