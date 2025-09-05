
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ManageHospitals.aspx.cs" Inherits="ClinicalBloodBank.ManageHospitals" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Clinical Blood Bank - Manage Hospitals</title>
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
                <a href="ManageHospitals.aspx" class="menu-item active">
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
                    <h1>Manage Hospitals</h1>
                    <p>Admin Panel</p>
                </div>
                <div class="user-profile">
                    <div class="user-avatar">
                        <asp:Literal ID="litUserInitials" runat="server"></asp:Literal>
                    </div>
                    <span><asp:Literal ID="litUserName" runat="server"></asp:Literal></span>
                </div>
            </div>

            <div class="content-section">
                <div class="section-header">
                    <div class="section-title">Add/Edit Hospital</div>
                </div>
                <asp:HiddenField ID="hdnHospitalId" runat="server" />
                <div class="form-group">
                    <label for="txtHospitalName">Hospital Name</label>
                    <asp:TextBox ID="txtHospitalName" runat="server" CssClass="form-control" />
                </div>
                <div class="form-group">
                    <label for="txtLicenseNumber">License Number</label>
                    <asp:TextBox ID="txtLicenseNumber" runat="server" CssClass="form-control" />
                </div>
                <div class="form-group">
                    <label for="txtHospitalAddress">Address</label>
                    <asp:TextBox ID="txtHospitalAddress" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" />
                </div>
                <div class="form-group">
                    <label for="txtContactPerson">Contact Person</label>
                    <asp:TextBox ID="txtContactPerson" runat="server" CssClass="form-control" />
                </div>
                <div class="form-group">
                    <label for="txtHospitalEmail">Email</label>
                    <asp:TextBox ID="txtHospitalEmail" runat="server" CssClass="form-control" TextMode="Email" />
                </div>
                <div class="form-group">
                    <label for="txtHospitalPhone">Phone</label>
                    <asp:TextBox ID="txtHospitalPhone" runat="server" CssClass="form-control" TextMode="Phone" />
                </div>
                <div class="form-group">
                    <label for="txtHospitalPassword">Password (for new hospitals)</label>
                    <asp:TextBox ID="txtHospitalPassword" runat="server" CssClass="form-control" TextMode="Password" />
                </div>
                <div class="form-group">
                    <asp:CheckBox ID="cbIsVerified" runat="server" Text=" Verified Hospital" />
                </div>
                <asp:Button ID="btnSaveHospital" runat="server" Text="Save Hospital" CssClass="btn btn-primary" OnClick="btnSaveHospital_Click" />
                <asp:Button ID="btnClearForm" runat="server" Text="Clear Form" CssClass="btn btn-secondary" OnClick="btnClearForm_Click" />
            </div>

            <div class="content-section">
                <div class="section-header">
                    <div class="section-title">Hospital List</div>
                </div>
                <asp:GridView ID="gvHospitals" runat="server" AutoGenerateColumns="False" CssClass="table" DataKeyNames="hospital_id"
                    OnRowEditing="gvHospitals_RowEditing" OnRowDeleting="gvHospitals_RowDeleting">
                    <Columns>
                        <asp:BoundField DataField="hospital_id" HeaderText="ID" ReadOnly="True" />
                        <asp:BoundField DataField="hospital_name" HeaderText="Name" />
                        <asp:BoundField DataField="license_number" HeaderText="License" />
                        <asp:BoundField DataField="address" HeaderText="Address" />
                        <asp:BoundField DataField="contact_person" HeaderText="Contact" />
                        <asp:BoundField DataField="email" HeaderText="Email" />
                        <asp:BoundField DataField="phone" HeaderText="Phone" />
                        <asp:TemplateField HeaderText="Status">
                            <ItemTemplate>
                                <span class='status-badge <%# Convert.ToBoolean(Eval("is_verified")) ? "status-active" : "status-inactive" %>'>
                                    <%# Convert.ToBoolean(Eval("is_verified")) ? "Verified" : "Pending" %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Actions">
                            <ItemTemplate>
                                <div class="action-buttons">
                                    <asp:Button ID="btnEdit" runat="server" CommandName="Edit" Text="Edit" CssClass="btn btn-sm btn-edit" />
                                    <asp:Button ID="btnDelete" runat="server" CommandName="Delete" Text="Deactivate" CssClass="btn btn-sm btn-delete" OnClientClick="return confirm('Are you sure you want to deactivate this hospital?');" />
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
