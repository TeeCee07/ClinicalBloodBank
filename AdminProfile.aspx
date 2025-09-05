<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdminProfile.aspx.cs" Inherits="ClinicalBloodBank.AdminProfile" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Clinical Blood Bank - Admin Profile</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        }
        
        body {
            background-color: #f8f9fa;
            color: #333;
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
            background-size: cover;
            background-position: center;
        }
        
        .profile-container {
            background: white;
            border-radius: 10px;
            padding: 30px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
            max-width: 800px;
            margin: 0 auto;
        }
        
        .profile-header {
            text-align: center;
            margin-bottom: 30px;
        }
        
        .profile-title {
            color: #2c3e50;
            font-size: 24px;
            margin-bottom: 10px;
        }
        
        .profile-subtitle {
            color: #7f8c8d;
            font-size: 16px;
        }
        
        .profile-picture-section {
            text-align: center;
            margin-bottom: 30px;
        }
        
        .profile-picture {
            width: 120px;
            height: 120px;
            border-radius: 50%;
            object-fit: cover;
            border: 4px solid #e9ecef;
            margin-bottom: 15px;
        }
        
        .profile-picture-placeholder {
            width: 120px;
            height: 120px;
            border-radius: 50%;
            background: #d32f2f;
            color: white;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 40px;
            font-weight: bold;
            margin: 0 auto 15px auto;
            border: 4px solid #e9ecef;
        }
        
        .file-upload {
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: 10px;
        }
        
        .file-upload-label {
            background: #f8f9fa;
            border: 1px dashed #dee2e6;
            padding: 10px 20px;
            border-radius: 5px;
            cursor: pointer;
            transition: background 0.3s;
        }
        
        .file-upload-label:hover {
            background: #e9ecef;
        }
        
        .form-group {
            margin-bottom: 20px;
        }
        
        .form-row {
            display: flex;
            gap: 20px;
            margin-bottom: 20px;
        }
        
        .form-half {
            flex: 1;
        }
        
        .form-label {
            display: block;
            margin-bottom: 8px;
            font-weight: 600;
            color: #2c3e50;
        }
        
        .form-control {
            width: 100%;
            padding: 12px 15px;
            border: 1px solid #ced4da;
            border-radius: 5px;
            font-size: 16px;
            transition: border-color 0.3s;
        }
        
        .form-control:focus {
            border-color: #d32f2f;
            outline: none;
        }
        
        .btn-primary {
            background: #d32f2f;
            color: white;
            border: none;
            padding: 12px 25px;
            border-radius: 5px;
            font-size: 16px;
            font-weight: 600;
            cursor: pointer;
            transition: background 0.3s;
        }
        
        .btn-primary:hover {
            background: #b71c1c;
        }
        
        .alert {
            padding: 15px;
            border-radius: 5px;
            margin-bottom: 20px;
        }
        
        .alert-success {
            background: #d4edda;
            color: #155724;
            border: 1px solid #c3e6cb;
        }
        
        .alert-danger {
            background: #f8d7da;
            color: #721c24;
            border: 1px solid #f5c6cb;
        }
        
        .text-danger {
            color: #dc3545;
            font-size: 14px;
            margin-top: 5px;
        }
        
        .action-buttons {
            display: flex;
            justify-content: flex-end;
            gap: 15px;
            margin-top: 30px;
        }
        
        .btn-secondary {
            background: #6c757d;
            color: white;
            border: none;
            padding: 12px 25px;
            border-radius: 5px;
            font-size: 16px;
            font-weight: 600;
            cursor: pointer;
            transition: background 0.3s;
            text-decoration: none;
            display: inline-block;
        }
        
        .btn-secondary:hover {
            background: #5a6268;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server" enctype="multipart/form-data">
        <div class="sidebar">
            <div class="sidebar-header">
                <h3>Clinical Blood Bank</h3>
            </div>
            <div class="sidebar-menu">
                <a href="AdminDashboard.aspx" class="menu-item">
                    <span class="menu-icon">🏠</span> Dashboard
                </a>
                <a href="AdminProfile.aspx" class="menu-item active">
                    <span class="menu-icon">👤</span> My Profile
                </a>
                <a href="ManageDonors.aspx" class="menu-item">
                    <span class="menu-icon">👥</span> Manage Donors
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
                    <h1>Admin Profile</h1>
                    <p>Update your personal information</p>
                </div>
                <div class="header-actions">
                    <div class="user-profile">
                        <div class="user-avatar" id="headerAvatar" runat="server"></div>
                        <span><asp:Literal ID="litHeaderUserName" runat="server"></asp:Literal></span>
                    </div>
                </div>
            </div>

            <asp:Panel ID="pnlSuccess" runat="server" CssClass="alert alert-success" Visible="false">
                <asp:Literal ID="litSuccessMessage" runat="server"></asp:Literal>
            </asp:Panel>

            <asp:Panel ID="pnlError" runat="server" CssClass="alert alert-danger" Visible="false">
                <asp:Literal ID="litErrorMessage" runat="server"></asp:Literal>
            </asp:Panel>

            <div class="profile-container">
                <div class="profile-header">
                    <h2 class="profile-title">Admin Profile</h2>
                    <p class="profile-subtitle">Update your personal information and profile picture</p>
                </div>

                <div class="profile-picture-section">
                    <asp:Image ID="imgProfile" runat="server" CssClass="profile-picture" Visible="false" />
                    <div id="profilePlaceholder" runat="server" class="profile-picture-placeholder"></div>
                    
                    <div class="file-upload">
                        <label for="fileProfile" class="file-upload-label">
                            <i class="fas fa-camera"></i> Change Profile Picture
                        </label>
                        <asp:FileUpload ID="fileProfile" runat="server" Style="display: none;" onchange="previewImage(this)" />
                        <asp:Button ID="btnRemovePicture" runat="server" Text="Remove Picture" CssClass="btn-secondary" OnClick="btnRemovePicture_Click" CausesValidation="false" />
                        <asp:CustomValidator ID="cvProfilePicture" runat="server" ErrorMessage="Please select a valid image file (JPG, PNG, GIF)" 
                            Display="Dynamic" CssClass="text-danger" OnServerValidate="cvProfilePicture_ServerValidate"></asp:CustomValidator>
                    </div>
                </div>

                <div class="form-row">
                    <div class="form-half">
                        <div class="form-group">
                            <label class="form-label">First Name</label>
                            <asp:TextBox ID="txtFirstName" runat="server" CssClass="form-control" MaxLength="50" required></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvFirstName" runat="server" ControlToValidate="txtFirstName"
                                ErrorMessage="First name is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                    <div class="form-half">
                        <div class="form-group">
                            <label class="form-label">Last Name</label>
                            <asp:TextBox ID="txtLastName" runat="server" CssClass="form-control" MaxLength="50" required></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvLastName" runat="server" ControlToValidate="txtLastName"
                                ErrorMessage="Last name is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                </div>

                <div class="form-group">
                    <label class="form-label">Email Address</label>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" MaxLength="100" required></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail"
                        ErrorMessage="Email is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                    <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail"
                        ValidationExpression="^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"
                        ErrorMessage="Please enter a valid email address" Display="Dynamic" CssClass="text-danger"></asp:RegularExpressionValidator>
                </div>

                <div class="form-group">
                    <label class="form-label">Phone Number</label>
                    <asp:TextBox ID="txtPhone" runat="server" CssClass="form-control" MaxLength="20" required></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvPhone" runat="server" ControlToValidate="txtPhone"
                        ErrorMessage="Phone number is required" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
                </div>

                <div class="form-group">
                    <label class="form-label">Address Line 1</label>
                    <asp:TextBox ID="txtAddress1" runat="server" CssClass="form-control" MaxLength="255"></asp:TextBox>
                </div>

                <div class="form-group">
                    <label class="form-label">Address Line 2</label>
                    <asp:TextBox ID="txtAddress2" runat="server" CssClass="form-control" MaxLength="255"></asp:TextBox>
                </div>

                <div class="form-row">
                    <div class="form-half">
                        <div class="form-group">
                            <label class="form-label">City</label>
                            <asp:TextBox ID="txtCity" runat="server" CssClass="form-control" MaxLength="100"></asp:TextBox>
                        </div>
                    </div>
                    <div class="form-half">
                        <div class="form-group">
                            <label class="form-label">State</label>
                            <asp:TextBox ID="txtState" runat="server" CssClass="form-control" MaxLength="100"></asp:TextBox>
                        </div>
                    </div>
                </div>

                <div class="form-row">
                    <div class="form-half">
                        <div class="form-group">
                            <label class="form-label">Postal Code</label>
                            <asp:TextBox ID="txtPostalCode" runat="server" CssClass="form-control" MaxLength="20"></asp:TextBox>
                        </div>
                    </div>
                    <div class="form-half">
                        <div class="form-group">
                            <label class="form-label">Country</label>
                            <asp:TextBox ID="txtCountry" runat="server" CssClass="form-control" MaxLength="100" Text="USA"></asp:TextBox>
                        </div>
                    </div>
                </div>

                <div class="action-buttons">
                    <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn-secondary" OnClick="btnCancel_Click" CausesValidation="false" />
                    <asp:Button ID="btnSave" runat="server" Text="Save Changes" CssClass="btn-primary" OnClick="btnSave_Click" />
                </div>
            </div>
        </div>

        <script>
            function previewImage(input) {
                if (input.files && input.files[0]) {
                    var reader = new FileReader();
                    reader.onload = function (e) {
                        // Show preview if we have an image element
                        var imgPreview = document.getElementById('<%= imgProfile.ClientID %>');
                        if (imgPreview) {
                            imgPreview.src = e.target.result;
                            imgPreview.style.display = 'block';
                        }
                        
                        // Hide placeholder
                        var placeholder = document.getElementById('<%= profilePlaceholder.ClientID %>');
                        if (placeholder) {
                            placeholder.style.display = 'none';
                        }
                    }
                    reader.readAsDataURL(input.files[0]);
                }
            }

            // Trigger file upload when label is clicked
            document.addEventListener('DOMContentLoaded', function () {
                var fileUploadLabel = document.querySelector('.file-upload-label');
                var fileUpload = document.getElementById('<%= fileProfile.ClientID %>');
                
                if (fileUploadLabel && fileUpload) {
                    fileUploadLabel.addEventListener('click', function () {
                        fileUpload.click();
                    });
                }
            });
        </script>
    </form>
</body>
</html>