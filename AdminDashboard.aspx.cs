using System;
using System.Configuration;
using System.Data;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics;

namespace ClinicalBloodBank
{
    public partial class AdminDashboard : System.Web.UI.Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;
        private List<string> controlsToRegister = new List<string>();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["AdminId"] == null)
            {
                Debug.WriteLine($"[{DateTime.Now}] Page_Load - Missing session variable: AdminId");
                Response.Redirect("Login.aspx");
                return;
            }

            try
            {
                this.PreRender += new EventHandler(Page_PreRender);
                controlsToRegister.Add(btnClearAll.UniqueID);
                controlsToRegister.Add(lnkLogout.UniqueID);
                controlsToRegister.Add(lnkProfileLogout.UniqueID);

                if (!IsPostBack)
                {
                    LoadUserDetails();
                    LoadDashboardStats();
                    LoadNotifications();
                    LoadNotificationDropdown();
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] Page_Load - MySQL Error: {ex.Message}");
                ShowMessage("Database error: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] Page_Load - Error: {ex.Message}");
                ShowMessage("Error: " + ex.Message, "danger");
            }
        }

        private void LoadUserDetails()
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                litUserName.Text = "Administrator";
                litUserInitials.Text = "AD";
                ShowMessage("Database connection configuration is missing.", "danger");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT first_name, last_name FROM admins WHERE admin_id = @adminId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@adminId", Session["AdminId"]);
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
                                litUserName.Text = Session["AdminName"]?.ToString() ?? "Administrator";
                                litUserInitials.Text = GetInitials(Session["AdminName"]?.ToString() ?? "Administrator");
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadUserDetails - MySQL Error: {ex.Message}");
                litUserName.Text = Session["AdminName"]?.ToString() ?? "Administrator";
                litUserInitials.Text = GetInitials(Session["AdminName"]?.ToString() ?? "Administrator");
                ShowMessage("Error loading user details: " + ex.Message, "danger");
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
            if (string.IsNullOrEmpty(connectionString))
            {
                litTotalDonors.Text = "0";
                litTotalHospitals.Text = "0";
                litTotalInventory.Text = "0";
                litPendingRequests.Text = "0";
                ShowMessage("Database connection configuration is missing.", "danger");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
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
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadDashboardStats - MySQL Error: {ex.Message}");
                ShowMessage("Error loading dashboard statistics: " + ex.Message, "danger");
                litTotalDonors.Text = "0";
                litTotalHospitals.Text = "0";
                litTotalInventory.Text = "0";
                litPendingRequests.Text = "0";
            }
        }

        private void LoadNotifications()
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                notificationCount.InnerText = "0";
                lblNoNotifications.Visible = true;
                ShowMessage("Database connection configuration is missing.", "danger");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // Get notification count
                    string countQuery = "SELECT COUNT(*) FROM notifications WHERE is_read = 0 AND admin_id = @adminId";
                    using (MySqlCommand cmd = new MySqlCommand(countQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@adminId", Session["AdminId"]);
                        notificationCount.InnerText = cmd.ExecuteScalar()?.ToString() ?? "0";
                    }

                    // Get all notifications for GridView
                    string notifQuery = @"SELECT title, message, created_at, is_read 
                                        FROM notifications 
                                        WHERE admin_id = @adminId 
                                        ORDER BY created_at DESC";
                    using (MySqlCommand cmd = new MySqlCommand(notifQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@adminId", Session["AdminId"]);
                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            gvNotifications.DataSource = dt;
                            gvNotifications.DataBind();
                            lblNoNotifications.Visible = false;
                        }
                        else
                        {
                            gvNotifications.DataSource = null;
                            gvNotifications.DataBind();
                            lblNoNotifications.Visible = true;
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadNotifications - MySQL Error: {ex.Message}");
                notificationCount.InnerText = "0";
                lblNoNotifications.Visible = true;
                ShowMessage("Error loading notifications: " + ex.Message, "danger");
            }
        }

        private void LoadNotificationDropdown()
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                notificationList.InnerHtml = "<div class='no-notifications'>Database connection error</div>";
                ShowMessage("Database connection configuration is missing.", "danger");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string notifQuery = @"SELECT title, message, created_at, is_read 
                                        FROM notifications 
                                        WHERE admin_id = @adminId 
                                        ORDER BY created_at DESC";
                    using (MySqlCommand cmd = new MySqlCommand(notifQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@adminId", Session["AdminId"]);
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
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadNotificationDropdown - MySQL Error: {ex.Message}");
                notificationList.InnerHtml = "<div class='no-notifications'>Error loading notifications</div>";
                ShowMessage("Error loading notifications: " + ex.Message, "danger");
            }
        }

        protected void btnClearAll_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                ShowMessage("Database connection configuration is missing.", "danger");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string deleteQuery = "DELETE FROM notifications WHERE admin_id = @adminId";
                    using (MySqlCommand cmd = new MySqlCommand(deleteQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@adminId", Session["AdminId"]);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            notificationCount.InnerText = "0";
                            notificationList.InnerHtml = "<div class='no-notifications'>No notifications</div>";
                            LoadNotifications();
                            ShowMessage("All notifications cleared.", "success");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnClearAll_Click - MySQL Error: {ex.Message}");
                ShowMessage("Error clearing notifications: " + ex.Message, "danger");
            }
        }

        protected void lnkLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("Login.aspx");
        }

        protected void gvNotifications_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvNotifications.PageIndex = e.NewPageIndex;
            LoadNotifications();
        }

        protected void gvNotifications_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Pager)
            {
                foreach (Control control in e.Row.Controls[0].Controls)
                {
                    if (control is LinkButton || control is Button)
                    {
                        controlsToRegister.Add(control.UniqueID);
                    }
                }
            }
        }

        private void ShowMessage(string message, string type)
        {
            pnlMessage.Visible = true;
            lblMessage.Text = message;
            pnlMessage.CssClass = "alert alert-" + type;
        }

        protected override void Render(HtmlTextWriter writer)
        {
            foreach (string controlId in controlsToRegister)
            {
                ClientScript.RegisterForEventValidation(controlId);
            }
            base.Render(writer);
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            // Placeholder for future use
        }
    }
}