<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ManageAppointments.aspx.cs" Inherits="ClinicalBloodBank.ManageAppointments" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Clinical Blood Bank - Manage Appointments</title>
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
                    <h1>Manage Appointments</h1>
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
                    <div class="section-title">Update Appointment</div>
                </div>
                <asp:HiddenField ID="hdnAppointmentId" runat="server" />
                <div class="form-row">
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtAppointmentDonor">Donor</label>
                            <asp:TextBox ID="txtAppointmentDonor" runat="server" CssClass="form-control" ReadOnly="true" />
                        </div>
                    </div>
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtAppointmentHospital">Hospital</label>
                            <asp:TextBox ID="txtAppointmentHospital" runat="server" CssClass="form-control" ReadOnly="true" />
                        </div>
                    </div>
                </div>
                <div class="form-row">
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtAppointmentDateTime" class="required-field">Date & Time</label>
                            <asp:TextBox ID="txtAppointmentDateTime" runat="server" CssClass="form-control" TextMode="DateTimeLocal" />
                            <asp:RequiredFieldValidator ID="rfvAppointmentDateTime" runat="server" ControlToValidate="txtAppointmentDateTime"
                                ErrorMessage="Date and time are required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                    <div class="form-col">
                        <div class="form-group">
                            <label for="ddlAppointmentStatus" class="required-field">Status</label>
                            <asp:DropDownList ID="ddlAppointmentStatus" runat="server" CssClass="form-control">
                                <asp:ListItem Value="scheduled">Scheduled</asp:ListItem>
                                <asp:ListItem Value="completed">Completed</asp:ListItem>
                                <asp:ListItem Value="cancelled">Cancelled</asp:ListItem>
                                <asp:ListItem Value="no_show">No Show</asp:ListItem>
                            </asp:DropDownList>
                            <asp:RequiredFieldValidator ID="rfvAppointmentStatus" runat="server" ControlToValidate="ddlAppointmentStatus"
                                ErrorMessage="Status is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                </div>
                <div class="form-group">
                    <label for="txtAppointmentNotes">Notes</label>
                    <asp:TextBox ID="txtAppointmentNotes" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" />
                </div>
                <div class="form-group">
                    <asp:Button ID="btnUpdateAppointment" runat="server" Text="Update Appointment" CssClass="btn btn-primary" OnClick="btnUpdateAppointment_Click" />
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
                            <label for="ddlFilterStatus">Status</label>
                            <asp:DropDownList ID="ddlFilterStatus" runat="server" CssClass="form-control">
                                <asp:ListItem Value="">All Statuses</asp:ListItem>
                                <asp:ListItem Value="scheduled">Scheduled</asp:ListItem>
                                <asp:ListItem Value="completed">Completed</asp:ListItem>
                                <asp:ListItem Value="cancelled">Cancelled</asp:ListItem>
                                <asp:ListItem Value="no_show">No Show</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                    </div>
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtFilterDate">Date</label>
                            <asp:TextBox ID="txtFilterDate" runat="server" CssClass="form-control" TextMode="Date" />
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
                    <div class="section-title">Appointments List</div>
                </div>
                <asp:GridView ID="gvAppointments" runat="server" AutoGenerateColumns="False" CssClass="table" DataKeyNames="appointment_id"
                    OnRowEditing="gvAppointments_RowEditing" OnPageIndexChanging="gvAppointments_PageIndexChanging" OnRowDataBound="gvAppointments_RowDataBound"
                    AllowPaging="True" PageSize="10">
                    <Columns>
                        <asp:BoundField DataField="appointment_id" HeaderText="ID" ReadOnly="True" />
                        <asp:BoundField DataField="donor_name" HeaderText="Donor" />
                        <asp:BoundField DataField="hospital_name" HeaderText="Hospital" />
                        <asp:BoundField DataField="appointment_date" HeaderText="Date & Time" DataFormatString="{0:yyyy-MM-dd HH:mm}" />
                        <asp:TemplateField HeaderText="Status">
                            <ItemTemplate>
                                <span class='status-badge <%# Eval("status").ToString() == "completed" ? "status-active" : Eval("status").ToString() == "cancelled" ? "status-inactive" : "status-pending" %>'>
                                    <%# Eval("status") %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Actions">
                            <ItemTemplate>
                                <div class="action-buttons">
                                    <asp:Button ID="btnEdit" runat="server" CommandName="Edit" Text="Update" CssClass="btn btn-sm btn-edit" />
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