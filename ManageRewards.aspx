<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ManageRewards.aspx.cs" Inherits="ClinicalBloodBank.ManageRewards" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Clinical Blood Bank - Manage Rewards</title>
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
        .btn-danger {
            background: #c62828;
            color: white;
        }
        .btn-danger:hover {
            background: #b71c1c;
        }
        .btn-success {
            background: #2e7d32;
            color: white;
        }
        .btn-success:hover {
            background: #1b5e20;
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
        .text-danger {
            color: #d32f2f;
            font-size: 12px;
            margin-top: 5px;
            display: block;
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
        .no-data {
            padding: 20px;
            text-align: center;
            color: #7f8c8d;
            font-style: italic;
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
                <a href="ManageRewards.aspx" class="menu-item active">
                    <span class="menu-icon">🎁</span> Manage Rewards
                </a>
                <asp:LinkButton ID="lnkLogout" runat="server" CssClass="menu-item" OnClick="lnkLogout_Click">
                    <span class="menu-icon">🚪</span> Logout
                </asp:LinkButton>
            </div>
        </div>

        <div class="main-content">
            <div class="header">
                <div class="welcome-text">
                    <h1>Manage Rewards</h1>
                    <p>Welcome, <asp:Literal ID="litUserName" runat="server" Text="Admin"></asp:Literal></p>
                </div>
                <div class="user-profile">
                    <div class="user-avatar">
                        <asp:Literal ID="litUserInitials" runat="server" Text="AD"></asp:Literal>
                    </div>
                </div>
            </div>

            <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert">
                <asp:Label ID="lblMessage" runat="server" Text=""></asp:Label>
            </asp:Panel>

            <div class="content-section">
                <div class="section-header">
                    <div class="section-title">Search Rewards</div>
                </div>
                <div class="form-row">
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtSearch">Search by Name or Description</label>
                            <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control" />
                        </div>
                    </div>
                    <div class="form-col">
                        <div class="form-group">
                            <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary" OnClick="btnSearch_Click" CausesValidation="false" />
                            <asp:Button ID="btnClearSearch" runat="server" Text="Clear" CssClass="btn btn-secondary" OnClick="btnClearSearch_Click" CausesValidation="false" />
                        </div>
                    </div>
                </div>
            </div>

            <div class="content-section">
                <div class="section-header">
                    <div class="section-title">Add/Edit Reward</div>
                </div>
                <asp:HiddenField ID="hdnRewardId" runat="server" />
                <div class="form-row">
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtRewardName" class="required-field">Reward Name</label>
                            <asp:TextBox ID="txtRewardName" runat="server" CssClass="form-control" />
                            <asp:RequiredFieldValidator ID="rfvRewardName" runat="server" ControlToValidate="txtRewardName"
                                ErrorMessage="Reward name is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtPointsRequired" class="required-field">Points Required</label>
                            <asp:TextBox ID="txtPointsRequired" runat="server" CssClass="form-control" TextMode="Number" />
                            <asp:RequiredFieldValidator ID="rfvPointsRequired" runat="server" ControlToValidate="txtPointsRequired"
                                ErrorMessage="Points required is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                            <asp:RangeValidator ID="rvPointsRequired" runat="server" ControlToValidate="txtPointsRequired"
                                ErrorMessage="Points must be a positive number" Display="Dynamic" CssClass="text-danger"
                                MinimumValue="1" MaximumValue="1000000" Type="Integer"></asp:RangeValidator>
                        </div>
                    </div>
                </div>
                <div class="form-row">
                    <div class="form-col">
                        <div class="form-group">
                            <label for="txtDescription">Description</label>
                            <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4" />
                        </div>
                    </div>
                </div>
                <div class="form-row">
                    <div class="form-col">
                        <div class="form-group">
                            <asp:CheckBox ID="chkIsActive" runat="server" Text="Active" />
                        </div>
                    </div>
                </div>
                <div class="form-group">
                    <asp:Button ID="btnSave" runat="server" Text="Save Reward" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-secondary" OnClick="btnCancel_Click" CausesValidation="false" />
                </div>
            </div>

            <div class="content-section">
                <div class="section-header">
                    <div class="section-title">Rewards List</div>
                </div>
                <asp:GridView ID="gvRewards" runat="server" AutoGenerateColumns="False" CssClass="table"
                    AllowPaging="True" PageSize="10" OnPageIndexChanging="gvRewards_PageIndexChanging" OnRowDataBound="gvRewards_RowDataBound">
                    <Columns>
                        <asp:BoundField DataField="reward_name" HeaderText="Reward Name" />
                        <asp:BoundField DataField="description" HeaderText="Description" />
                        <asp:BoundField DataField="points_required" HeaderText="Points Required" />
                        <asp:TemplateField HeaderText="Status">
                            <ItemTemplate>
                                <span class='<%# Eval("is_active").ToString() == "True" ? "status-active" : "status-inactive" %>'>
                                    <%# Eval("is_active").ToString() == "True" ? "Active" : "Inactive" %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Actions">
                            <ItemTemplate>
                                <asp:Button ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary" CommandArgument='<%# Eval("reward_id") %>' OnClick="btnEdit_Click" CausesValidation="false" />
                                <asp:Button ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-danger" CommandArgument='<%# Eval("reward_id") %>' OnClick="btnDelete_Click" CausesValidation="false" OnClientClick="return confirm('Are you sure you want to delete this reward?');" />
                                <asp:Button ID="btnToggle" runat="server" Text='<%# Eval("is_active").ToString() == "True" ? "Deactivate" : "Activate" %>' 
                                    CssClass='<%# Eval("is_active").ToString() == "True" ? "btn btn-secondary" : "btn btn-success" %>' 
                                    CommandArgument='<%# Eval("reward_id") %>' OnClick="btnToggle_Click" CausesValidation="false" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
                <asp:Label ID="lblNoRewards" runat="server" Text="No rewards found." CssClass="no-data" Visible="false"></asp:Label>
            </div>
        </div>
    </form>
</body>
</html>