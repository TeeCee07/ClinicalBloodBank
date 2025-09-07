using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ClinicalBloodBank
{
    public partial class DonorDashboard : System.Web.UI.Page
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
                    // Load user details and dashboard data
                    LoadUserDetails();
                    LoadDashboardStats();
                    LoadAppointments();
                    LoadDonations();
                    LoadNotifications();
                    LoadNotificationDropdown();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Page_Load Error: " + ex.Message);
                    errorMessage.InnerText = "An error occurred while loading the dashboard. Some data may not be available.";
                    errorMessage.Style["display"] = "block";
                }
            }
        }

        private void LoadUserDetails()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"]?.ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                litUserName.Text = "Donor";
                litUserInitials.Text = "DO";
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT first_name, last_name, email, blood_type FROM donors WHERE donor_id = @donorId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@donorId", Session["UserId"]);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string firstName = reader["first_name"].ToString();
                                string lastName = reader["last_name"].ToString();
                                string fullName = $"{firstName} {lastName}";
                                litUserName.Text = fullName;
                                litUserInitials.Text = GetInitials(fullName);

                                // Set blood type if available
                                string bloodType = reader["blood_type"].ToString();
                                if (!string.IsNullOrEmpty(bloodType) && bloodType != "Unknown")
                                {
                                    litBloodType.Text = bloodType;
                                }
                            }
                            else
                            {
                                // Fallback to session values
                                litUserName.Text = Session["DonorName"]?.ToString() ?? "Donor";
                                litUserInitials.Text = GetInitials(Session["DonorName"]?.ToString() ?? "Donor");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("LoadUserDetails Error: " + ex.Message);
                    litUserName.Text = Session["DonorName"]?.ToString() ?? "Donor";
                    litUserInitials.Text = GetInitials(Session["DonorName"]?.ToString() ?? "Donor");
                }
            }
        }

        private string GetInitials(string name)
        {
            if (string.IsNullOrEmpty(name)) return "DO";
            string[] parts = name.Split(' ');
            string initials = "";
            foreach (string part in parts)
            {
                if (!string.IsNullOrEmpty(part))
                    initials += part[0].ToString().ToUpper();
            }
            return initials.Length > 2 ? initials.Substring(0, 2) : initials;
        }

        private void LoadDashboardStats()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"]?.ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                litTotalDonations.Text = "0";
                errorMessage.InnerText = "Database connection configuration is missing.";
                errorMessage.Style["display"] = "block";
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Total Donations (calculate from blood_inventory for accuracy)
                    string donationsQuery = "SELECT COUNT(*) FROM blood_inventory WHERE donor_id = @donorId AND status = 'available'";
                    using (MySqlCommand cmd = new MySqlCommand(donationsQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@donorId", Session["UserId"]);
                        int donationCount = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                        litTotalDonations.Text = donationCount.ToString();

                        // Update total_donations in donors table to keep it in sync
                        string updateQuery = "UPDATE donors SET total_donations = @totalDonations WHERE donor_id = @donorId";
                        using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, conn))
                        {
                            updateCmd.Parameters.AddWithValue("@totalDonations", donationCount);
                            updateCmd.Parameters.AddWithValue("@donorId", Session["UserId"]);
                            updateCmd.ExecuteNonQuery();
                        }
                    }

                    // Next Eligibility Date
                    string eligibilityQuery = @"SELECT DATE_ADD(COALESCE(MAX(donation_date), CURDATE()), INTERVAL 56 DAY) as next_donation_date
                                        FROM blood_inventory 
                                        WHERE donor_id = @donorId AND status = 'available'";
                    using (MySqlCommand cmd = new MySqlCommand(eligibilityQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@donorId", Session["UserId"]);
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            DateTime nextDate = Convert.ToDateTime(result);
                            if (nextDate <= DateTime.Now)
                            {
                                litNextEligibility.Text = "Now";
                            }
                            else
                            {
                                litNextEligibility.Text = nextDate.ToString("yyyy-MM-dd");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("LoadDashboardStats Error: " + ex.Message);
                    errorMessage.InnerText = "Error loading dashboard statistics. Using default values.";
                    errorMessage.Style["display"] = "block";
                    litTotalDonations.Text = "0";
                    litNextEligibility.Text = "Now";
                }
            }
        }

        protected void RecordDonation(string bloodType, int quantityMl, int hospitalId)
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

                    // Insert new donation into blood_inventory
                    string insertQuery = @"INSERT INTO blood_inventory (donor_id, blood_type, quantity_ml, donation_date, tested_by_hospital, status)
                                        VALUES (@donorId, @bloodType, @quantityMl, @donationDate, @hospitalId, 'available')";
                    using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@donorId", Session["UserId"]);
                        cmd.Parameters.AddWithValue("@bloodType", bloodType);
                        cmd.Parameters.AddWithValue("@quantityMl", quantityMl);
                        cmd.Parameters.AddWithValue("@donationDate", DateTime.Now);
                        cmd.Parameters.AddWithValue("@hospitalId", hospitalId);
                        cmd.ExecuteNonQuery();
                    }

                    // Update total_donations in donors table
                    string updateQuery = "UPDATE donors SET total_donations = total_donations + 1 WHERE donor_id = @donorId";
                    using (MySqlCommand cmd = new MySqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@donorId", Session["UserId"]);
                        cmd.ExecuteNonQuery();
                    }

                    // Reload dashboard stats to reflect the new donation
                    LoadDashboardStats();
                    LoadDonations();

                    // Show success message
                    errorMessage.InnerText = "Donation recorded successfully.";
                    errorMessage.Style["color"] = "#2e7d32";
                    errorMessage.Style["background-color"] = "#e8f5e9";
                    errorMessage.Style["display"] = "block";
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("RecordDonation Error: " + ex.Message);
                    errorMessage.InnerText = "Error recording donation.";
                    errorMessage.Style["display"] = "block";
                }
            }
        }

        private void LoadAppointments()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"]?.ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                lblNoAppointments.Visible = true;
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT a.appointment_id, a.appointment_date, h.hospital_name, a.status 
                             FROM donation_appointments a 
                             INNER JOIN hospitals h ON a.hospital_id = h.hospital_id 
                             WHERE a.donor_id = @donorId AND a.appointment_date >= NOW() 
                             ORDER BY a.appointment_date ASC 
                             LIMIT 5";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@donorId", Session["UserId"]);
                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            rptAppointments.DataSource = dt;
                            rptAppointments.DataBind();
                            lblNoAppointments.Visible = false;
                        }
                        else
                        {
                            rptAppointments.DataSource = null;
                            rptAppointments.DataBind();
                            lblNoAppointments.Visible = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("LoadAppointments Error: " + ex.Message);
                    lblNoAppointments.Visible = true;
                }
            }
        }

        private void LoadDonations()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"]?.ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                lblNoDonations.Visible = true;
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT i.donation_date, i.blood_type, i.quantity_ml, h.hospital_name, i.status 
                             FROM blood_inventory i 
                             INNER JOIN hospitals h ON i.tested_by_hospital = h.hospital_id 
                             WHERE i.donor_id = @donorId 
                             ORDER BY i.donation_date DESC 
                             LIMIT 5";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@donorId", Session["UserId"]);
                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            rptDonations.DataSource = dt;
                            rptDonations.DataBind();
                            lblNoDonations.Visible = false;
                        }
                        else
                        {
                            rptDonations.DataSource = null;
                            rptDonations.DataBind();
                            lblNoDonations.Visible = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("LoadDonations Error: " + ex.Message);
                    lblNoDonations.Visible = true;
                }
            }
        }

        private void LoadNotifications()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"]?.ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                notificationCount.InnerText = "0";
                lblNoNotifications.Visible = true;
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

                    // Get notifications for repeater
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
                            rptNotifications.DataSource = dt;
                            rptNotifications.DataBind();
                            lblNoNotifications.Visible = false;
                        }
                        else
                        {
                            rptNotifications.DataSource = null;
                            rptNotifications.DataBind();
                            lblNoNotifications.Visible = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("LoadNotifications Error: " + ex.Message);
                    notificationCount.InnerText = "0";
                    lblNoNotifications.Visible = true;
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

        protected void CancelAppointment(object sender, CommandEventArgs e)
        {
            string appointmentId = e.CommandArgument.ToString();
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
                    string updateQuery = "UPDATE donation_appointments SET status = 'cancelled' WHERE appointment_id = @appointmentId AND donor_id = @donorId";
                    using (MySqlCommand cmd = new MySqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@appointmentId", appointmentId);
                        cmd.Parameters.AddWithValue("@donorId", Session["UserId"]);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // Reload appointments
                            LoadAppointments();

                            // Show success message
                            errorMessage.InnerText = "Appointment cancelled successfully.";
                            errorMessage.Style["color"] = "#2e7d32";
                            errorMessage.Style["background-color"] = "#e8f5e9";
                            errorMessage.Style["display"] = "block";
                        }
                        else
                        {
                            errorMessage.InnerText = "Failed to cancel appointment. Please try again.";
                            errorMessage.Style["display"] = "block";
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("CancelAppointment Error: " + ex.Message);
                    errorMessage.InnerText = "Error cancelling appointment.";
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