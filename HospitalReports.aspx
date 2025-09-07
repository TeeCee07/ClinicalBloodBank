<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HospitalReports.aspx.cs" Inherits="ClinicalBloodBank.HospitalReports" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Clinical Blood Bank - Hospital Reports</title>
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
                <a href="HospitalProfile.aspx" class="menu-item">
                    <span class="menu-icon">👤</span> Profile
                </a>
                <a href="HospitalReports.aspx" class="menu-item active">
                    <span class="menu-icon">📊</span> Reports
                </a>
                <a href="Logout.aspx" class="menu-item">
                    <span class="menu-icon">🚪</span> Logout
                </a>
            </div>
        </div>

        <div class="main-content">
            <div class="header">
                <div class="welcome-text">
                    <h1>Hospital Reports</h1>
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
                    <div class="section-title">Report Filters</div>
                </div>
                <div class="form-row">
                    <div class="form-col">
                        <div class="form-group">
                            <label for="ddlReportType">Report Type</label>
                            <asp:DropDownList ID="ddlReportType" runat="server" CssClass="form-control">
                                <asp:ListItem Value="blood_requests">Blood Requests</asp:ListItem>
                                <asp:ListItem Value="inventory">Blood Inventory</asp:ListItem>
                                <asp:ListItem Value="appointments">Appointments</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                    </div>
                    <div class="form-col">
                        <div class="form-group">
                            <label for="ddlBloodType">Blood Type</label>
                            <asp:DropDownList ID="ddlBloodType" runat="server" CssClass="form-control">
                                <asp:ListItem Value="">All Blood Types</asp:ListItem>
                                <asp:ListItem Value="A+">A+</asp:ListItem>
                                <asp:ListItem Value="A-">A-</asp:ListItem>
                                <asp:ListItem Value="B+">B+</asp:ListItem>
                                <asp:ListItem Value="B-">B-</asp:ListItem>
                                <asp:ListItem Value="AB+">AB+</asp:ListItem>
                                <asp:ListItem Value="AB-">AB-</asp:ListItem>
                                <asp:ListItem Value="O+">O+</asp:ListItem>
                                <asp:ListItem Value="O-">O-</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                    </div>
                </div>
                <div class="form-row">
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtStartDate">Start Date</label>
                            <asp:TextBox ID="txtStartDate" runat="server" CssClass="form-control" TextMode="Date" />
                        </div>
                    </div>
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtEndDate">End Date</label>
                            <asp:TextBox ID="txtEndDate" runat="server" CssClass="form-control" TextMode="Date" />
                        </div>
                    </div>
                </div>
                <div class="form-row">
                    <div class="form-col">
                        <div class="form-group">
                            <label for="ddlStatus">Status</label>
                            <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-control">
                                <asp:ListItem Value="">All Statuses</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                    </div>
                    <div class="form-col">
                        <div class="form-group">
                            <asp:Button ID="btnGenerateReport" runat="server" Text="Generate Report" CssClass="btn btn-primary" OnClick="btnGenerateReport_Click" />
                            <asp:Button ID="btnClearFilters" runat="server" Text="Clear Filters" CssClass="btn btn-secondary" OnClick="btnClearFilters_Click" CausesValidation="false" />
                        </div>
                    </div>
                </div>
            </div>

            <div class="content-section">
                <div class="section-header">
                    <div class="section-title">Report Results</div>
                </div>
                <asp:GridView ID="gvReport" runat="server" AutoGenerateColumns="True" CssClass="table" AllowPaging="True" PageSize="10" OnPageIndexChanging="gvReport_PageIndexChanging">
                </asp:GridView>
            </div>
        </div>

        <script>
            // Dynamically update status options based on report type
            function updateStatusOptions() {
                var reportType = document.getElementById('<%= ddlReportType.ClientID %>').value;
                var statusDropdown = document.getElementById('<%= ddlStatus.ClientID %>');
                statusDropdown.innerHTML = '<option value="">All Statuses</option>';

                if (reportType === 'blood_requests') {
                    var options = ['pending', 'approved', 'rejected', 'fulfilled'];
                    for (var i = 0; i < options.length; i++) {
                        var option = document.createElement('option');
                        option.value = options[i];
                        option.text = options[i].charAt(0).toUpperCase() + options[i].slice(1);
                        statusDropdown.appendChild(option);
                    }
                } else if (reportType === 'inventory') {
                    var options = ['available', 'reserved', 'used', 'expired'];
                    for (var i = 0; i < options.length; i++) {
                        var option = document.createElement('option');
                        option.value = options[i];
                        option.text = options[i].charAt(0).toUpperCase() + options[i].slice(1);
                        statusDropdown.appendChild(option);
                    }
                } else if (reportType === 'appointments') {
                    var options = ['scheduled', 'completed', 'cancelled', 'no_show'];
                    for (var i = 0; i < options.length; i++) {
                        var option = document.createElement('option');
                        option.value = options[i];
                        option.text = options[i].charAt(0).toUpperCase() + options[i].slice(1);
                        statusDropdown.appendChild(option);
                    }
                }
            }

            document.getElementById('<%= ddlReportType.ClientID %>').onchange = updateStatusOptions;
            window.onload = updateStatusOptions;
        </script>
    </form>
</body>
</html>