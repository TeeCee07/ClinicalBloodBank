using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ClinicalBloodBank
{
    public partial class DonorProfile : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Validate session and authentication
                if (Session["UserId"] == null || Session["UserType"] == null || Session["UserType"].ToString() != "donor")
                {
                    Response.Redirect("Login.aspx");
                    return;
                }

                try
                {
                    // Load user details
                    LoadUserDetails();
                    LoadNotifications();
                    LoadNotificationDropdown();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Page_Load Error: " + ex.Message);
                    errorMessage.InnerText = "An error occurred while loading the profile. Some data may not be available.";
                    errorMessage.Style["display"] = "block";
                }
            }
        }

        private void LoadUserDetails()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"]?.ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                errorMessage.InnerText = "Database connection configuration is missing.";
                errorMessage.Style["display"] = "block";
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT first_name, last_name, email, phone, date_of_birth, gender, blood_type, weight, health_conditions, address_line1, address_line2, city, province, postal_code, country FROM donors WHERE donor_id = @donorId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@donorId", Session["UserId"]);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtFirstName.Text = reader["first_name"].ToString();
                                txtLastName.Text = reader["last_name"].ToString();
                                txtEmail.Text = reader["email"].ToString();
                                txtPhone.Text = reader["phone"].ToString();

                                if (reader["date_of_birth"] != DBNull.Value)
                                {
                                    DateTime dob = Convert.ToDateTime(reader["date_of_birth"]);
                                    txtDateOfBirth.Text = dob.ToString("yyyy-MM-dd");
                                }

                                ddlGender.SelectedValue = reader["gender"].ToString();
                                ddlBloodType.SelectedValue = reader["blood_type"].ToString();

                                if (reader["weight"] != DBNull.Value)
                                {
                                    txtWeight.Text = reader["weight"].ToString();
                                }

                                txtHealthConditions.Text = reader["health_conditions"].ToString();
                                txtAddressLine1.Text = reader["address_line1"].ToString();
                                txtAddressLine2.Text = reader["address_line2"].ToString();
                                txtCity.Text = reader["city"].ToString();
                                txtProvince.Text = reader["province"].ToString();
                                txtPostalCode.Text = reader["postal_code"].ToString();

                                if (!string.IsNullOrEmpty(reader["country"].ToString()))
                                {
                                    txtCountry.Text = reader["country"].ToString();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("LoadUserDetails Error: " + ex.Message);
                    errorMessage.InnerText = "Error loading user details.";
                    errorMessage.Style["display"] = "block";
                }
            }
        }

        private void LoadNotifications()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"]?.ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                notificationCount.InnerText = "0";
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Get notification count
                    string countQuery = "SELECT COUNT(*) FROM notifications WHERE is_read = 0 AND donor_id = @donorId";
                    using (MySqlCommand cmd = new MySqlCommand(countQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@donorId", Session["UserId"]);
                        notificationCount.InnerText = cmd.ExecuteScalar()?.ToString() ?? "0";
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("LoadNotifications Error: " + ex.Message);
                    notificationCount.InnerText = "0";
                }
            }
        }

        private void LoadNotificationDropdown()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"]?.ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                notificationList.InnerHtml = "<div class='no-notifications'>Database connection error</div>";
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string notifQuery = @"SELECT title, message, created_at, is_read 
                                   FROM notifications 
                                   WHERE donor_id = @donorId 
                                   ORDER BY created_at DESC 
                                   LIMIT 5";
                    using (MySqlCommand cmd = new MySqlCommand(notifQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@donorId", Session["UserId"]);
                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            notificationList.InnerHtml = "";
                            foreach (DataRow row in dt.Rows)
                            {
                                string title = row["title"].ToString();
                                string message = row["message"].ToString();
                                DateTime createdAt = Convert.ToDateTime(row["created_at"]);
                                bool isRead = Convert.ToBoolean(row["is_read"]);

                                string notificationItem = $@"
                            <div class='notification-item {(isRead ? "" : "unread")}'>
                                <div class='notification-icon'>🔔</div>
                                <div class='notification-content'>
                                    <div class='notification-message'><strong>{title}</strong>: {message}</div>
                                    <div class='notification-time'>{createdAt:yyyy-MM-dd HH:mm}</div>
                                </div>
                            </div>";
                                notificationList.InnerHtml += notificationItem;
                            }
                        }
                        else
                        {
                            notificationList.InnerHtml = "<div class='no-notifications'>No notifications</div>";
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("LoadNotificationDropdown Error: " + ex.Message);
                    notificationList.InnerHtml = "<div class='no-notifications'>Error loading notifications</div>";
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            // Validate passwords if changed
            if (!string.IsNullOrEmpty(txtNewPassword.Text))
            {
                if (txtNewPassword.Text != txtConfirmPassword.Text)
                {
                    errorMessage.InnerText = "New passwords do not match.";
                    errorMessage.Style["display"] = "block";
                    return;
                }
            }

            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"]?.ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                errorMessage.InnerText = "Database connection configuration is missing.";
                errorMessage.Style["display"] = "block";
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // First verify current password
                    string verifyQuery = "SELECT password FROM donors WHERE donor_id = @donorId";
                    string currentPasswordHash = "";

                    using (MySqlCommand cmd = new MySqlCommand(verifyQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@donorId", Session["UserId"]);
                        currentPasswordHash = cmd.ExecuteScalar()?.ToString();
                    }

                    // In a real application, you would hash the entered password and compare with the stored hash
                    // For simplicity, we're doing a direct comparison (not secure for production)
                    if (txtCurrentPassword.Text != "current_password_validation_would_be_here")
                    {
                        errorMessage.InnerText = "Current password is incorrect.";
                        errorMessage.Style["display"] = "block";
                        return;
                    }

                    // Update donor information
                    string updateQuery = @"UPDATE donors 
                                  SET first_name = @firstName, last_name = @lastName, email = @email, 
                                      phone = @phone, date_of_birth = @dob, gender = @gender, 
                                      blood_type = @bloodType, weight = @weight, health_conditions = @healthConditions,
                                      address_line1 = @address1, address_line2 = @address2, city = @city, 
                                      province = @province, postal_code = @postalCode, country = @country
                                  WHERE donor_id = @donorId";

                    using (MySqlCommand cmd = new MySqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@firstName", txtFirstName.Text);
                        cmd.Parameters.AddWithValue("@lastName", txtLastName.Text);
                        cmd.Parameters.AddWithValue("@email", txtEmail.Text);
                        cmd.Parameters.AddWithValue("@phone", txtPhone.Text);
                        cmd.Parameters.AddWithValue("@dob", txtDateOfBirth.Text);
                        cmd.Parameters.AddWithValue("@gender", ddlGender.SelectedValue);
                        cmd.Parameters.AddWithValue("@bloodType", ddlBloodType.SelectedValue);
                        cmd.Parameters.AddWithValue("@weight", decimal.Parse(txtWeight.Text));
                        cmd.Parameters.AddWithValue("@healthConditions", txtHealthConditions.Text);
                        cmd.Parameters.AddWithValue("@address1", txtAddressLine1.Text);
                        cmd.Parameters.AddWithValue("@address2", txtAddressLine2.Text);
                        cmd.Parameters.AddWithValue("@city", txtCity.Text);
                        cmd.Parameters.AddWithValue("@province", txtProvince.Text);
                        cmd.Parameters.AddWithValue("@postalCode", txtPostalCode.Text);
                        cmd.Parameters.AddWithValue("@country", txtCountry.Text);
                        cmd.Parameters.AddWithValue("@donorId", Session["UserId"]);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            successMessage.InnerText = "Profile updated successfully.";
                            successMessage.Style["display"] = "block";
                        }
                        else
                        {
                            errorMessage.InnerText = "Failed to update profile. Please try again.";
                            errorMessage.Style["display"] = "block";
                        }
                    }

                    // Update password if changed
                    if (!string.IsNullOrEmpty(txtNewPassword.Text))
                    {
                        string passwordQuery = "UPDATE donors SET password = @password WHERE donor_id = @donorId";
                        using (MySqlCommand cmd = new MySqlCommand(passwordQuery, conn))
                        {
                            // In a real application, you would hash the password before storing it
                            cmd.Parameters.AddWithValue("@password", txtNewPassword.Text);
                            cmd.Parameters.AddWithValue("@donorId", Session["UserId"]);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("btnSave_Click Error: " + ex.Message);
                    errorMessage.InnerText = "Error updating profile. Please try again.";
                    errorMessage.Style["display"] = "block";
                }
            }
        }
        protected void lnkLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("Login.aspx");
        }

        protected void btnClearAll_Click(object sender, EventArgs e)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"]?.ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                errorMessage.InnerText = "Database connection configuration is missing.";
                errorMessage.Style["display"] = "block";
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string deleteQuery = "DELETE FROM notifications WHERE donor_id = @donorId";
                    using (MySqlCommand cmd = new MySqlCommand(deleteQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@donorId", Session["UserId"]);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            notificationCount.InnerText = "0";
                            notificationList.InnerHtml = "<div class='no-notifications'>No notifications</div>";
                            LoadNotifications();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("btnClearAll_Click Error: " + ex.Message);
                    errorMessage.InnerText = "Error clearing notifications.";
                    errorMessage.Style["display"] = "block";
                }
            }
        }
    }
}