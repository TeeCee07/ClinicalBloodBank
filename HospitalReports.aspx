%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HospitalReports.aspx.cs" Inherits="ClinicalBloodBank.HospitalReports" %>

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
        .no-data {
            padding: 20px;
            text-align: center;
            color: #7f8c8d;
            font-style: italic;
        }
        .chart-container {
            max-width: 600px;
            margin: 20px auto;
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
                <a href="HospitalBloodInventory.aspx" class="menu-item">
                    <span class="menu-icon">🩺</span> Blood Inventory
                </a>
                <a href="ManageRequests.aspx" class="menu-item">
                    <span class="menu-icon">📋</span>  Blood Requests
                </a>
                <a href="ManageDonationAppointments.aspx" class="menu-item">
                    <span class="menu-icon">📅</span> Manage Appointments
                </a>
                <a href="HospitalProfile.aspx" class="menu-item">
                    <span class="menu-icon">🏥</span> Profile
                </a>
                <a href="HospitalReports.aspx" class="menu-item active">
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
                    <p>Hospital Reports</p>
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
                    <div class="section-title">Inventory Status</div>
                    <asp:Button ID="btnExportInventory" runat="server" Text="Export to CSV" CssClass="btn btn-primary" OnClick="btnExportInventory_Click" />
                </div>
                <asp:GridView ID="gvInventory" runat="server" AutoGenerateColumns="False" CssClass="table" AllowPaging="True" PageSize="10">
                    <Columns>
                        <asp:BoundField DataField="blood_type" HeaderText="Blood Type" NullDisplayText="Unknown" />
                        <asp:BoundField DataField="quantity_ml" HeaderText="Total Quantity (ml)" />
                        <asp:BoundField DataField="status" HeaderText="Status" />
                    </Columns>
                </asp:GridView>
                <asp:Label ID="lblNoInventory" runat="server" Text="No inventory data found." CssClass="no-data" Visible="false"></asp:Label>
                <div class="chart-container">
                    <canvas id="inventoryChart"></canvas>
                </div>
            </div>

            <div class="content-section">
                <div class="section-header">
                    <div class="section-title">Request Fulfillment</div>
                    <asp:Button ID="btnExportRequests" runat="server" Text="Export to CSV" CssClass="btn btn-primary" OnClick="btnExportRequests_Click" />
                </div>
                <asp:GridView ID="gvRequests" runat="server" AutoGenerateColumns="False" CssClass="table" AllowPaging="True" PageSize="10">
                    <Columns>
                        <asp:BoundField DataField="request_id" HeaderText="ID" />
                        <asp:BoundField DataField="blood_type" HeaderText="Blood Type" />
                        <asp:BoundField DataField="quantity_ml" HeaderText="Quantity (ml)" />
                        <asp:BoundField DataField="status" HeaderText="Status" />
                        <asp:BoundField DataField="fulfilled_at" HeaderText="Fulfilled At" DataFormatString="{0:yyyy-MM-dd HH:mm}" NullDisplayText="N/A" />
                    </Columns>
                </asp:GridView>
                <asp:Label ID="lblNoRequests" runat="server" Text="No requests found." CssClass="no-data" Visible="false"></asp:Label>
            </div>
        </div>

        <script src="https://cdn.jsdelivr.net/npm/chart.js@3.9.1/dist/chart.min.js"></script>
        <script>
            function renderInventoryChart(labels, data) {
                if (!labels || !data || labels.length === 0 || data.length === 0) {
                    document.getElementById('inventoryChart').style.display = 'none';
                    return;
                }
                var ctx = document.getElementById('inventoryChart').getContext('2d');
                new Chart(ctx, {
                    type: 'pie',
                    data: {
                        labels: labels,
                        datasets: [{
                            label: 'Blood Inventory by Type (ml)',
                            data: data,
                            backgroundColor: ['#d32f2f', '#2c3e50', '#388e3c', '#0288d1', '#ffca28', '#26a69a', '#7b1fa2', '#ff9800'],
                            borderColor: ['#b71c1c', '#1a252f', '#2e7d32', '#01579b', '#ffb300', '#00897b', '#4a148c', '#f57c00'],
                            borderWidth: 1
                        }]
                    },
                    options: {
                        responsive: true,
                        plugins: { legend: { position: 'top' }, title: { display: true, text: 'Blood Inventory by Type' } }
                    }
                });
            }
        </script>
    </form>
</body>
</html>