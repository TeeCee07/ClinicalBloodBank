using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ClinicalBloodBank
{
    public partial class HospitalDashboard : System.Web.UI.Page
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null || Session["UserType"] == null || Session["UserType"].ToString() != "hospital")
            {
                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] Page_Load: Invalid session. UserId: {Session["UserId"]}, UserType: {Session["UserType"]}");
                Response.Redirect("Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadHospitalDetails();
                LoadDashboardStats();
                LoadNotifications();
            }
        }

        private void LoadHospitalDetails()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
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
                                litUserName.Text = reader["hospital_name"]?.ToString() ?? "Hospital";
                                litUserInitials.Text = GetInitials(reader["hospital_name"]?.ToString() ?? "Hospital");
                            }
                            else
                            {
                                litUserName.Text = Session["UserName"]?.ToString() ?? "Hospital";
                                litUserInitials.Text = GetInitials(Session["UserName"]?.ToString() ?? "Hospital");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] LoadHospitalDetails Error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                    ShowMessage("Error loading hospital details.", "danger");
                }
            }
        }

        private string GetInitials(string name)
        {
            if (string.IsNullOrEmpty(name)) return "HD";
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
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    // Total Inventory
                    string inventoryQuery = @"SELECT COALESCE(SUM(quantity_ml), 0) AS total_ml
                                             FROM blood_inventory
                                             WHERE tested_by_hospital = @hospitalId AND status = 'available'";
                    using (MySqlCommand cmd = new MySqlCommand(inventoryQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                        litTotalInventory.Text = Convert.ToInt32(cmd.ExecuteScalar()).ToString();
                    }

                    // Pending Requests
                    string requestsQuery = @"SELECT COUNT(*) AS pending_count
                                            FROM blood_requests
                                            WHERE fulfilled_by_hospital = @hospitalId AND status = 'pending'";
                    using (MySqlCommand cmd = new MySqlCommand(requestsQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                        litPendingRequests.Text = Convert.ToInt32(cmd.ExecuteScalar()).ToString();
                    }

                    // Upcoming Appointments
                    string appointmentsQuery = @"SELECT COUNT(*) AS upcoming_count
                                                FROM donation_appointments
                                                WHERE hospital_id = @hospitalId AND status = 'Scheduled' AND appointment_date >= NOW()";
                    using (MySqlCommand cmd = new MySqlCommand(appointmentsQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                        litUpcomingAppointments.Text = Convert.ToInt32(cmd.ExecuteScalar()).ToString();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] LoadDashboardStats Error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                    ShowMessage("Error loading dashboard statistics.", "danger");
                }
            }
        }

        private void LoadNotifications()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT notification_id, title, message, created_at, is_read
                                    FROM notifications
                                    WHERE hospital_id = @hospitalId
                                    ORDER BY created_at DESC";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        // Update notification count
                        int unreadCount = 0;
                        foreach (DataRow row in dt.Rows)
                        {
                            if (!Convert.ToBoolean(row["is_read"]))
                                unreadCount++;
                        }
                        notificationCount.InnerText = unreadCount.ToString();

                        // Populate dropdown
                        string notificationHtml = "";
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                string isReadClass = Convert.ToBoolean(row["is_read"]) ? "" : "unread";
                                string createdAt = Convert.ToDateTime(row["created_at"]).ToString("yyyy-MM-dd HH:mm");
                                notificationHtml += $"<div class='notification-item {isReadClass}' onclick='markNotificationRead({row["notification_id"]})'>" +
                                                   "<div class='notification-icon'>🔔</div>" +
                                                   "<div class='notification-content'>" +
                                                   $"<div class='notification-message'>{Server.HtmlEncode(row["message"].ToString())}</div>" +
                                                   $"<div class='notification-time'>{createdAt}</div>" +
                                                   "</div></div>";
                            }
                        }
                        else
                        {
                            notificationHtml = "<div class='no-notifications'>No notifications found.</div>";
                        }
                        notificationList.InnerHtml = notificationHtml;

                        // Populate GridView
                        gvNotifications.DataSource = dt;
                        gvNotifications.DataBind();
                        lblNoNotifications.Visible = dt.Rows.Count == 0;

                        System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] LoadNotifications: Retrieved {dt.Rows.Count} notifications, {unreadCount} unread");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] LoadNotifications Error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                    ShowMessage("Error loading notifications.", "danger");
                }
            }
        }

        protected void btnClearAll_Click(object sender, EventArgs e)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"UPDATE notifications 
                                    SET is_read = 1
                                    WHERE hospital_id = @hospitalId AND is_read = 0";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            ShowMessage("All notifications marked as read.", "success");
                            LoadNotifications();
                        }
                        else
                        {
                            ShowMessage("No unread notifications to clear.", "info");
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] btnClearAll_Click Error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                    ShowMessage("Error clearing notifications.", "danger");
                }
            }
        }

        protected void gvNotifications_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvNotifications.PageIndex = e.NewPageIndex;
            LoadNotifications();
        }

        protected void gvNotifications_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                bool isRead = Convert.ToBoolean(DataBinder.Eval(e.Row.DataItem, "is_read"));
                e.Row.CssClass = isRead ? "status-read" : "status-unread";
            }
        }

        private void ShowMessage(string message, string type)
        {
            pnlMessage.Visible = true;
            lblMessage.Text = message;
            pnlMessage.CssClass = "alert alert-" + type;
            ClientScript.RegisterStartupScript(this.GetType(), "showMessage", $"alert('{message}');", true);
        }

        protected void lnkLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("Login.aspx");
        }
    }
}