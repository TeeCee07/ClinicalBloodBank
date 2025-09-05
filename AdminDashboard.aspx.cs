using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ClinicalBloodBank
{
    public partial class AdminDashboard : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Validate session and authentication
                if (Session["UserId"] == null || Session["UserType"] == null || Session["UserType"].ToString() != "admin")
                {
                    Response.Redirect("Login.aspx");
                    return;
                }

                try
                {
                    // Load user details and dashboard data
                    LoadUserDetails();
                    LoadDashboardStats();
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
                litUserName.Text = "Administrator";
                litUserInitials.Text = "AD";
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT first_name, last_name, email FROM admins WHERE admin_id = @adminId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@adminId", Session["UserId"]);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string firstName = reader["first_name"].ToString();
                                string lastName = reader["last_name"].ToString();
                                string fullName = $"{firstName} {lastName}";
                                litUserName.Text = fullName;
                                litUserInitials.Text = GetInitials(fullName);
                            }
                            else
                            {
                                // Fallback to session values
                                litUserName.Text = Session["AdminName"]?.ToString() ?? "Administrator";
                                litUserInitials.Text = GetInitials(Session["AdminName"]?.ToString() ?? "Administrator");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("LoadUserDetails Error: " + ex.Message);
                    litUserName.Text = Session["AdminName"]?.ToString() ?? "Administrator";
                    litUserInitials.Text = GetInitials(Session["AdminName"]?.ToString() ?? "Administrator");
                }
            }
        }

        private string GetInitials(string name)
        {
            if (string.IsNullOrEmpty(name)) return "AD";
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
                litTotalDonors.Text = "0";
                litTotalHospitals.Text = "0";
                litTotalInventory.Text = "0";
                litPendingRequests.Text = "0";
                errorMessage.InnerText = "Database connection configuration is missing.";
                errorMessage.Style["display"] = "block";
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Total Donors
                    string donorsQuery = "SELECT COUNT(*) FROM donors WHERE is_active = 1";
                    using (MySqlCommand cmd = new MySqlCommand(donorsQuery, conn))
                    {
                        litTotalDonors.Text = cmd.ExecuteScalar()?.ToString() ?? "0";
                    }

                    // Total Hospitals
                    string hospitalsQuery = "SELECT COUNT(*) FROM hospitals WHERE is_verified = 1";
                    using (MySqlCommand cmd = new MySqlCommand(hospitalsQuery, conn))
                    {
                        litTotalHospitals.Text = cmd.ExecuteScalar()?.ToString() ?? "0";
                    }

                    // Total Inventory
                    string inventoryQuery = "SELECT COALESCE(SUM(quantity_ml), 0) FROM blood_inventory WHERE status = 'available'";
                    using (MySqlCommand cmd = new MySqlCommand(inventoryQuery, conn))
                    {
                        litTotalInventory.Text = cmd.ExecuteScalar()?.ToString() ?? "0";
                    }

                    // Pending Requests
                    string requestsQuery = "SELECT COUNT(*) FROM blood_requests WHERE status = 'pending'";
                    using (MySqlCommand cmd = new MySqlCommand(requestsQuery, conn))
                    {
                        litPendingRequests.Text = cmd.ExecuteScalar()?.ToString() ?? "0";
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("LoadDashboardStats Error: " + ex.Message);
                    errorMessage.InnerText = "Error loading dashboard statistics. Using default values.";
                    errorMessage.Style["display"] = "block";
                    litTotalDonors.Text = "0";
                    litTotalHospitals.Text = "0";
                    litTotalInventory.Text = "0";
                    litPendingRequests.Text = "0";
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
                    string countQuery = "SELECT COUNT(*) FROM notifications WHERE is_read = 0 AND admin_id = @adminId";
                    using (MySqlCommand cmd = new MySqlCommand(countQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@adminId", Session["UserId"]);
                        notificationCount.InnerText = cmd.ExecuteScalar()?.ToString() ?? "0";
                    }

                    // Get notifications for repeater
                    string notifQuery = @"SELECT title, message, created_at, is_read 
                                   FROM notifications 
                                   WHERE admin_id = @adminId 
                                   ORDER BY created_at DESC 
                                   LIMIT 5";
                    using (MySqlCommand cmd = new MySqlCommand(notifQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@adminId", Session["UserId"]);
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
                                   WHERE admin_id = @adminId 
                                   ORDER BY created_at DESC 
                                   LIMIT 5";
                    using (MySqlCommand cmd = new MySqlCommand(notifQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@adminId", Session["UserId"]);
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
                    string deleteQuery = "DELETE FROM notifications WHERE admin_id = @adminId";
                    using (MySqlCommand cmd = new MySqlCommand(deleteQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@adminId", Session["UserId"]);
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