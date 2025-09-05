<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ForgotPassword.aspx.cs" Inherits="ClinicalBloodBank.ForgotPassword" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Clinical Blood Bank - Forgot Password</title>
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
            cursor: button;
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
        
        .instructions {
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
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <div class="banner">
                <h1>Reset Your Password</h1>
                <p>Enter your email address and we'll send you instructions to reset your password.</p>
                <svg class="banner-img" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512">
                    <path fill="#fff" d="M64 64c0-17.7-14.3-32-32-32S0 46.3 0 64V288c0 88.4 71.6 160 160 160s160-71.6 160-160V64c0-17.7-14.3-32-32-32s-32 14.3-32 32V288c0 53-43 96-96 96s-96-43-96-96V64zm288 0c0-17.7-14.3-32-32-32s-32 14.3-32 32V288c0 88.4 71.6 160 160 160s160-71.6 160-160V64c0-17.7-14.3-32-32-32s-32 14.3-32 32V288c0 53-43 96-96 96s-96-43-96-96V64z"/>
                </svg>
            </div>
            
            <div class="form-container">
                <div class="logo">
                    <h2>PASSWORD <span>RECOVERY</span></h2>
                </div>
                
                <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert">
                    <asp:Label ID="lblMessage" runat="server" Text=""></asp:Label>
                </asp:Panel>
                
                <div class="instructions">
                    <p>Please enter the email address associated with your account. We'll email you a link to reset your password.</p>
                </div>
                
                <div class="form-group">
                    <label for="txtEmail">Email Address</label>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" placeholder="Enter your email" TextMode="Email"></asp:TextBox>
                </div>
                
                <asp:Button ID="btnResetPassword" runat="server" Text="Send Reset Link" CssClass="btn" OnClick="btnResetPassword_Click" />
                
                <div class="form-footer">
                    <p>Remember your password? <a href="Login.aspx">Back to login</a></p>
                </div>
            </div>
        </div>
    </form>
</body>
</html>