<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="ClinicalBloodBank.Register" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Clinical Blood Bank - Donor Registration</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        }
        
        body {
            background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
            min-height: 100vh;
            display: flex;
            justify-content: center;
            align-items: center;
            padding: 20px;
        }
        
        .container {
            display: flex;
            max-width: 1000px;
            width: 100%;
            background: white;
            border-radius: 12px;
            box-shadow: 0 15px 30px rgba(0, 0, 0, 0.1);
            overflow: hidden;
        }
        
        .banner {
            flex: 1;
            background: linear-gradient(to bottom right, #d32f2f, #b71c1c);
            color: white;
            padding: 40px;
            display: flex;
            flex-direction: column;
            justify-content: center;
            align-items: center;
            text-align: center;
        }
        
        .banner h1 {
            font-size: 28px;
            margin-bottom: 20px;
            font-weight: 600;
        }
        
        .banner p {
            font-size: 16px;
            line-height: 1.6;
            margin-bottom: 30px;
        }
        
        .banner-img {
            width: 100%;
            max-width: 250px;
            margin-top: 20px;
        }
        
        .form-container {
            flex: 1;
            padding: 40px;
            overflow-y: auto;
            max-height: 90vh;
        }
        
        .logo {
            text-align: center;
            margin-bottom: 30px;
        }
        
        .logo h2 {
            color: #d32f2f;
            font-size: 28px;
            font-weight: 700;
        }
        
        .logo span {
            color: #2c3e50;
            font-weight: 300;
        }
        
        .form-group {
            margin-bottom: 20px;
        }
        
        .form-group label {
            display: block;
            margin-bottom: 8px;
            font-weight: 500;
            color: #2c3e50;
        }
        
        .form-control {
            width: 100%;
            padding: 12px 15px;
            border: 1px solid #ddd;
            border-radius: 6px;
            font-size: 16px;
            transition: all 0.3s;
        }
        
        .form-control:focus {
            border-color: #d32f2f;
            box-shadow: 0 0 0 3px rgba(211, 47, 47, 0.2);
            outline: none;
        }
        
        .btn {
            width: 100%;
            padding: 12px;
            background: #d32f2f;
            color: white;
            border: none;
            border-radius: 6px;
            font-size: 16px;
            font-weight: 600;
            cursor: pointer;
            transition: background 0.3s;
        }
        
        .btn:hover {
            background: #b71c1c;
        }
        
        .form-footer {
            text-align: center;
            margin-top: 20px;
            font-size: 14px;
            color: #7f8c8d;
        }
        
        .form-footer a {
            color: #d32f2f;
            text-decoration: none;
            font-weight: 500;
        }
        
        .form-footer a:hover {
            text-decoration: underline;
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
        
        .donor-info {
            background-color: #f8f9fa;
            padding: 15px;
            border-radius: 6px;
            margin-bottom: 20px;
            font-size: 14px;
            color: #495057;
        }
        
        @media (max-width: 768px) {
            .container {
                flex-direction: column;
            }
            
            .banner {
                padding: 30px 20px;
            }
            
            .form-container {
                max-height: none;
            }
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <div class="banner">
                <h1>Become a Blood Donor</h1>
                <p>Join our community of life-savers. Your donation can save up to three lives. Register today to make a difference.</p>
                <svg class="banner-img" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512">
                    <path fill="#fff" d="M64 64c0-17.7-14.3-32-32-32S0 46.3 0 64V288c0 88.4 71.6 160 160 160s160-71.6 160-160V64c0-17.7-14.3-32-32-32s-32 14.3-32 32V288c0 53-43 96-96 96s-96-43-96-96V64zm288 0c0-17.7-14.3-32-32-32s-32 14.3-32 32V288c0 88.4 71.6 160 160 160s160-71.6 160-160V64c0-17.7-14.3-32-32-32s-32 14.3-32 32V288c0 53-43 96-96 96s-96-43-96-96V64z"/>
                </svg>
            </div>
            
            <div class="form-container">
                <div class="logo">
                    <h2>DONOR <span>REGISTRATION</span></h2>
                </div>
                
                <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert">
                    <asp:Label ID="lblMessage" runat="server" Text=""></asp:Label>
                </asp:Panel>
                
                <div class="donor-info">
                    
                    <label for="txtEmail">
                    Email Address</label>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" placeholder="Enter your email" TextMode="Email"></asp:TextBox>
                </div>
                
                <div class="form-group">
                    <label for="txtPassword">Password</label>
                    <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" placeholder="Create a password" TextMode="Password"></asp:TextBox>
                </div>
                
                <div class="form-group">
                    <label for="txtConfirmPassword">Confirm Password</label>
                    <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="form-control" placeholder="Confirm your password" TextMode="Password"></asp:TextBox>
                </div>
                
                <div class="form-group">
                    <label for="txtFirstName">First Name</label>
                    <asp:TextBox ID="txtFirstName" runat="server" CssClass="form-control" placeholder="Enter your first name"></asp:TextBox>
                </div>
                
                <div class="form-group">
                    <label for="txtLastName">Last Name</label>
                    <asp:TextBox ID="txtLastName" runat="server" CssClass="form-control" placeholder="Enter your last name"></asp:TextBox>
                </div>
                
                <div class="form-group">
                    <label for="txtPhone">Phone Number</label>
                    <asp:TextBox ID="txtPhone" runat="server" CssClass="form-control" placeholder="Enter your phone number"></asp:TextBox>
                </div>
                
                <div class="form-group">
                    <label for="txtAddressLine1">Address Line 1</label>
                    <asp:TextBox ID="txtAddressLine1" runat="server" CssClass="form-control" placeholder="Enter your street address"></asp:TextBox>
                </div>
                
                <div class="form-group">
                    <label for="txtAddressLine2">Address Line 2 (optional)</label>
                    <asp:TextBox ID="txtAddressLine2" runat="server" CssClass="form-control" placeholder="Apartment, suite, etc."></asp:TextBox>
                </div>
                
                <div class="form-group">
                    <label for="txtCity">City</label>
                    <asp:TextBox ID="txtCity" runat="server" CssClass="form-control" placeholder="Enter your city"></asp:TextBox>
                </div>
                
                <div class="form-group">
                    <label for="txtState">State/Province</label>
                    <asp:TextBox ID="txtState" runat="server" CssClass="form-control" placeholder="Enter your state or province"></asp:TextBox>
                </div>
                
                <div class="form-group">
                    <label for="txtPostalCode">Postal Code</label>
                    <asp:TextBox ID="txtPostalCode" runat="server" CssClass="form-control" placeholder="Enter your postal code"></asp:TextBox>
                </div>
                
                <div class="form-group">
                    <label for="txtCountry">Country</label>
                    <asp:TextBox ID="txtCountry" runat="server" CssClass="form-control" placeholder="Enter your country" Text="SA"></asp:TextBox>
                </div>
                
                <div class="form-group">
                    <label for="txtDateOfBirth">Date of Birth</label>
                    <asp:TextBox ID="txtDateOfBirth" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                </div>
                
                <div class="form-group">
                    <label for="ddlGender">Gender</label>
                    <asp:DropDownList ID="ddlGender" runat="server" CssClass="form-control">
                        <asp:ListItem Value="">Select Gender</asp:ListItem>
                        <asp:ListItem Value="Male">Male</asp:ListItem>
                        <asp:ListItem Value="Female">Female</asp:ListItem>
                        <asp:ListItem Value="Other">Other</asp:ListItem>
                    </asp:DropDownList>
                </div>
                
                <div class="form-group">
                    <label for="ddlBloodType">Blood Type</label>
                    <asp:DropDownList ID="ddlBloodType" runat="server" CssClass="form-control">
                        <asp:ListItem Value="">Select Blood Type</asp:ListItem>
                        <asp:ListItem Value="Not Known">Not Known</asp:ListItem>
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
                
                <div class="form-group">
                    <label for="txtWeight">Weight (kg)</label>
                    <asp:TextBox ID="txtWeight" runat="server" CssClass="form-control" placeholder="Enter your weight (e.g., 75.5)"></asp:TextBox>
                </div>
                
                <div class="form-group">
                    <label for="txtHealthConditions">Health Conditions (optional)</label>
                    <asp:TextBox ID="txtHealthConditions" runat="server" CssClass="form-control" placeholder="List any health conditions or medications" TextMode="MultiLine" Rows="3"></asp:TextBox>
                </div>
                
                <asp:Button ID="btnRegister" runat="server" Text="Register as Donor" CssClass="btn" OnClick="btnRegister_Click" />
                
                <div class="form-footer">
                    <p>Already have an account? <a href="Login.aspx">Login here</a></p>
                </div>
            </div>
        </div>
    </form>
</body>
</html>