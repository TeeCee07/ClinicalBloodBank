using System;
using System.Configuration;
using System.Data;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Web.UI;

namespace ClinicalBloodBank
{
    public partial class HospitalProfile : System.Web.UI.Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null || Session["UserType"] == null || Session["UserType"].ToString() != "hospital")
            {
                Debug.WriteLine($"[{DateTime.Now}] Page_Load - Invalid session. UserId: {Session["UserId"]}, UserType: {Session["UserType"]}");
                Response.Redirect("Login.aspx");
                return;
            }

            try
            {
                if (!IsPostBack)
                {
                    LoadUserInfo();
                    LoadProfileDetails();
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] Page_Load - MySQL Error: {ex.Message}, ErrorCode: {ex.Number}");
                ShowMessage("Database error: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] Page_Load - Error: {ex.Message}, StackTrace: {ex.StackTrace}");
                ShowMessage("Error: " + ex.Message, "danger");
            }
        }

        private void LoadUserInfo()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT hospital_name FROM hospitals WHERE hospital_id = @hospitalId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string hospitalName = reader["hospital_name"]?.ToString() ?? "Hospital";
                                litUserName.Text = hospitalName;
                                litUserInitials.Text = GetInitials(hospitalName);
                                Debug.WriteLine($"[{DateTime.Now}] LoadUserInfo - HospitalName: {hospitalName}");
                            }
                            else
                            {
                                litUserName.Text = "Hospital";
                                litUserInitials.Text = "HO";
                                Debug.WriteLine($"[{DateTime.Now}] LoadUserInfo - Hospital not found for UserId: {Session["UserId"]}");
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadUserInfo - MySQL Error: {ex.Message}, ErrorCode: {ex.Number}");
                ShowMessage("Error loading hospital info: " + ex.Message, "danger");
                litUserName.Text = "Hospital";
                litUserInitials.Text = "HO";
            }
        }

        private string GetInitials(string name)
        {
            if (string.IsNullOrEmpty(name)) return "HO";
            string[] parts = name.Split(' ');
            string initials = "";
            foreach (string part in parts)
            {
                if (!string.IsNullOrEmpty(part))
                    initials += part[0].ToString().ToUpper();
            }
            return initials.Length > 2 ? initials.Substring(0, 2) : initials;
        }

        private void LoadProfileDetails()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT hospital_id, hospital_name, contact_email, license_number, 
                                    address_line1, address_line2, city, province, postal_code, country, is_verified 
                                    FROM hospitals WHERE hospital_id = @hospitalId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtHospitalId.Text = reader["hospital_id"].ToString();
                                txtHospitalName.Text = reader["hospital_name"].ToString();
                                txtContactEmail.Text = reader["contact_email"].ToString();
                                txtLicenseNumber.Text = reader["license_number"].ToString();
                                txtAddressLine1.Text = reader["address_line1"]?.ToString() ?? "";
                                txtAddressLine2.Text = reader["address_line2"]?.ToString() ?? "";
                                txtCity.Text = reader["city"].ToString();
                                txtProvince.Text = reader["province"].ToString();
                                txtPostalCode.Text = reader["postal_code"]?.ToString() ?? "";
                                txtCountry.Text = reader["country"].ToString();
                                Debug.WriteLine($"[{DateTime.Now}] LoadProfileDetails - Loaded profile for hospital ID: {Session["UserId"]}");
                            }
                            else
                            {
                                ShowMessage("Hospital profile not found.", "danger");
                                Debug.WriteLine($"[{DateTime.Now}] LoadProfileDetails - Hospital not found for UserId: {Session["UserId"]}");
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadProfileDetails - MySQL Error: {ex.Message}, ErrorCode: {ex.Number}");
                ShowMessage("Error loading profile details: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadProfileDetails - Error: {ex.Message}");
                ShowMessage("Error loading profile details: " + ex.Message, "danger");
            }
        }

        protected void btnUpdateProfile_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    // Check for duplicate email or license number
                    string checkQuery = @"SELECT COUNT(*) FROM hospitals 
                                         WHERE (contact_email = @contactEmail OR license_number = @licenseNumber) 
                                         AND hospital_id != @hospitalId";
                    using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@contactEmail", txtContactEmail.Text.Trim());
                        checkCmd.Parameters.AddWithValue("@licenseNumber", txtLicenseNumber.Text.Trim());
                        checkCmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                        long count = (long)checkCmd.ExecuteScalar();
                        if (count > 0)
                        {
                            ShowMessage("Email or license number already in use by another hospital.", "danger");
                            Debug.WriteLine($"[{DateTime.Now}] btnUpdateProfile_Click - Duplicate email or license number: {txtContactEmail.Text}, {txtLicenseNumber.Text}");
                            return;
                        }
                    }

                    string query = @"UPDATE hospitals 
                                    SET hospital_name = @hospitalName, contact_email = @contactEmail, 
                                        license_number = @licenseNumber, address_line1 = @addressLine1, 
                                        address_line2 = @addressLine2, city = @city, province = @province, 
                                        postal_code = @postalCode, country = @country 
                                    WHERE hospital_id = @hospitalId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@hospitalName", txtHospitalName.Text.Trim());
                        cmd.Parameters.AddWithValue("@contactEmail", txtContactEmail.Text.Trim());
                        cmd.Parameters.AddWithValue("@licenseNumber", txtLicenseNumber.Text.Trim());
                        cmd.Parameters.AddWithValue("@addressLine1", string.IsNullOrEmpty(txtAddressLine1.Text) ? (object)DBNull.Value : txtAddressLine1.Text.Trim());
                        cmd.Parameters.AddWithValue("@addressLine2", string.IsNullOrEmpty(txtAddressLine2.Text) ? (object)DBNull.Value : txtAddressLine2.Text.Trim());
                        cmd.Parameters.AddWithValue("@city", txtCity.Text.Trim());
                        cmd.Parameters.AddWithValue("@province", txtProvince.Text.Trim());
                        cmd.Parameters.AddWithValue("@postalCode", string.IsNullOrEmpty(txtPostalCode.Text) ? (object)DBNull.Value : txtPostalCode.Text.Trim());
                        cmd.Parameters.AddWithValue("@country", txtCountry.Text.Trim());
                        cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // Update session variable for hospital name
                            Session["UserName"] = txtHospitalName.Text.Trim();
                            litUserName.Text = txtHospitalName.Text.Trim();
                            litUserInitials.Text = GetInitials(txtHospitalName.Text.Trim());
                            AddNotification(Convert.ToInt32(Session["UserId"]), "Profile Updated", "Your hospital profile has been updated successfully.");
                            ShowMessage("Profile updated successfully.", "success");
                            Debug.WriteLine($"[{DateTime.Now}] btnUpdateProfile_Click - Updated profile for hospital ID: {Session["UserId"]}");
                        }
                        else
                        {
                            ShowMessage("Profile not found or no changes made.", "danger");
                            Debug.WriteLine($"[{DateTime.Now}] btnUpdateProfile_Click - No rows affected for hospital ID: {Session["UserId"]}");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnUpdateProfile_Click - MySQL Error: {ex.Message}, ErrorCode: {ex.Number}");
                ShowMessage("Error updating profile: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnUpdateProfile_Click - Error: {ex.Message}");
                ShowMessage("Error updating profile: " + ex.Message, "danger");
            }
        }

        protected void btnChangePassword_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    // Verify current password
                    string verifyQuery = "SELECT contact_password FROM hospitals WHERE hospital_id = @hospitalId";
                    string currentPassword;
                    using (MySqlCommand verifyCmd = new MySqlCommand(verifyQuery, conn))
                    {
                        verifyCmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                        currentPassword = verifyCmd.ExecuteScalar()?.ToString();
                    }

                    if (currentPassword != txtCurrentPassword.Text) // Replace with hashed password check in production
                    {
                        ShowMessage("Current password is incorrect.", "danger");
                        Debug.WriteLine($"[{DateTime.Now}] btnChangePassword_Click - Incorrect current password for hospital ID: {Session["UserId"]}");
                        return;
                    }

                    // Update password
                    string updateQuery = "UPDATE hospitals SET contact_password = @newPassword WHERE hospital_id = @hospitalId";
                    using (MySqlCommand cmd = new MySqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@newPassword", txtNewPassword.Text); // Use hashed password in production
                        cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            AddNotification(Convert.ToInt32(Session["UserId"]), "Password Changed", "Your password has been changed successfully.");
                            ShowMessage("Password changed successfully.", "success");
                            Debug.WriteLine($"[{DateTime.Now}] btnChangePassword_Click - Password changed for hospital ID: {Session["UserId"]}");
                            txtCurrentPassword.Text = "";
                            txtNewPassword.Text = "";
                            txtConfirmPassword.Text = "";
                        }
                        else
                        {
                            ShowMessage("Password change failed.", "danger");
                            Debug.WriteLine($"[{DateTime.Now}] btnChangePassword_Click - No rows affected for hospital ID: {Session["UserId"]}");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnChangePassword_Click - MySQL Error: {ex.Message}, ErrorCode: {ex.Number}");
                ShowMessage("Error changing password: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnChangePassword_Click - Error: {ex.Message}");
                ShowMessage("Error changing password: " + ex.Message, "danger");
            }
        }

        private void AddNotification(int hospitalId, string title, string message)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO notifications (hospital_id, title, message, is_read, created_at) 
                                    VALUES (@hospitalId, @title, @message, 0, CURRENT_TIMESTAMP)";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@hospitalId", hospitalId);
                        cmd.Parameters.AddWithValue("@title", title);
                        cmd.Parameters.AddWithValue("@message", message);
                        cmd.ExecuteNonQuery();
                        Debug.WriteLine($"[{DateTime.Now}] AddNotification - Added notification: {title}, {message}");
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] AddNotification - MySQL Error: {ex.Message}, ErrorCode: {ex.Number}");
                ShowMessage("Error adding notification: " + ex.Message, "danger");
            }
        }

        private void ShowMessage(string message, string type)
        {
            pnlMessage.Visible = true;
            lblMessage.Text = message;
            pnlMessage.CssClass = "alert alert-" + type;
            Debug.WriteLine($"[{DateTime.Now}] ShowMessage - Displayed message: {message}, Type: {type}");
        }
    }
}