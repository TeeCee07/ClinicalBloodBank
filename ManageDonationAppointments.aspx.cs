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
    public partial class ManageDonationAppointments : System.Web.UI.Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;
        private List<string> controlsToRegister = new List<string>();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null || Session["UserType"] == null || Session["UserType"].ToString() != "hospital")
            {
                Debug.WriteLine($"[{DateTime.Now}] Page_Load: Invalid session. UserId: {Session["UserId"]}, UserType: {Session["UserType"]}");
                Response.Redirect("Login.aspx");
                return;
            }

            try
            {
                this.PreRender += new EventHandler(Page_PreRender);
                if (!IsPostBack)
                {
                    LoadHospitalDetails();
                    LoadAppointments();
                    LoadDonorDropdown();
                    LoadNotifications();
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

        private void LoadHospitalDetails()
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
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadHospitalDetails - MySQL Error: {ex.Message}");
                ShowMessage("Error loading hospital details: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadHospitalDetails - Error: {ex.Message}");
                ShowMessage("Error loading hospital details: " + ex.Message, "danger");
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

        private void LoadAppointments()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT a.appointment_id, CONCAT(d.first_name, ' ', d.last_name) AS donor_name, a.appointment_date, a.status
                                    FROM donation_appointments a
                                    INNER JOIN donors d ON a.donor_id = d.donor_id
                                    WHERE a.hospital_id = @hospitalId";

                    string donorId = ddlFilterDonor.SelectedValue;
                    string appointmentDate = txtFilterAppointmentDate.Text;

                    if (!string.IsNullOrEmpty(donorId))
                        query += " AND a.donor_id = @DonorId";
                    if (!string.IsNullOrEmpty(appointmentDate))
                        query += " AND a.appointment_date = @AppointmentDate";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                        if (!string.IsNullOrEmpty(donorId))
                            cmd.Parameters.AddWithValue("@DonorId", donorId);
                        if (!string.IsNullOrEmpty(appointmentDate))
                            cmd.Parameters.AddWithValue("@AppointmentDate", DateTime.Parse(appointmentDate));

                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        gvAppointments.DataSource = dt;
                        gvAppointments.DataBind();

                        if (dt.Rows.Count == 0)
                            ShowMessage("No appointments found.", "info");
                        else
                            ShowMessage($"{dt.Rows.Count} appointment(s) found.", "success");
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadAppointments - MySQL Error: {ex.Message}");
                ShowMessage("Error loading appointments: " + ex.Message, "danger");
                gvAppointments.DataSource = null;
                gvAppointments.DataBind();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadAppointments - Error: {ex.Message}");
                ShowMessage("Error loading appointments: " + ex.Message, "danger");
                gvAppointments.DataSource = null;
                gvAppointments.DataBind();
            }
        }

        private void LoadDonorDropdown()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT donor_id, CONCAT(d.first_name, ' ', d.last_name) AS donor_name
                                    FROM donors d
                                    WHERE d.is_active = 1";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        ddlDonor.DataSource = dt;
                        ddlDonor.DataTextField = "donor_name";
                        ddlDonor.DataValueField = "donor_id";
                        ddlDonor.DataBind();
                        ddlDonor.Items.Insert(0, new ListItem("Select Donor", ""));
                        ddlFilterDonor.DataSource = dt;
                        ddlFilterDonor.DataTextField = "donor_name";
                        ddlFilterDonor.DataValueField = "donor_id";
                        ddlFilterDonor.DataBind();
                        ddlFilterDonor.Items.Insert(0, new ListItem("All Donors", ""));
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadDonorDropdown - MySQL Error: {ex.Message}");
                ShowMessage("Error loading donor dropdown: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadDonorDropdown - Error: {ex.Message}");
                ShowMessage("Error loading donor dropdown: " + ex.Message, "danger");
            }
        }

        protected void btnClearAll_Click(object sender, EventArgs e)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
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
                            ShowMessage("All notifications marked as read.", "success");
                        else
                            ShowMessage("No unread notifications to clear.", "info");
                    }
                    LoadNotifications();
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnClearAll_Click - MySQL Error: {ex.Message}");
                ShowMessage("Error clearing notifications: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnClearAll_Click - Error: {ex.Message}");
                ShowMessage("Error clearing notifications: " + ex.Message, "danger");
            }
        }

        private void LoadNotifications()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
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

                        int unreadCount = 0;
                        foreach (DataRow row in dt.Rows)
                        {
                            if (!Convert.ToBoolean(row["is_read"]))
                                unreadCount++;
                        }
                        notificationCount.InnerText = unreadCount.ToString();

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
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadNotifications - MySQL Error: {ex.Message}");
                ShowMessage("Error loading notifications: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadNotifications - Error: {ex.Message}");
                ShowMessage("Error loading notifications: " + ex.Message, "danger");
            }
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            try
            {
                LoadAppointments();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnFilter_Click - Error: {ex.Message}");
                ShowMessage("Error applying filter: " + ex.Message, "danger");
            }
        }

        protected void btnClearFilter_Click(object sender, EventArgs e)
        {
            try
            {
                ddlFilterDonor.SelectedIndex = 0;
                txtFilterAppointmentDate.Text = "";
                LoadAppointments();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnClearFilter_Click - Error: {ex.Message}");
                ShowMessage("Error clearing filter: " + ex.Message, "danger");
            }
        }

        protected void gvAppointments_RowEditing(object sender, GridViewEditEventArgs e)
        {
            try
            {
                string appointmentId = gvAppointments.DataKeys[e.NewEditIndex].Value.ToString();
                LoadAppointmentData(appointmentId);
                gvAppointments.EditIndex = -1;
                LoadAppointments();
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] gvAppointments_RowEditing - MySQL Error: {ex.Message}");
                ShowMessage("Error loading appointment data: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] gvAppointments_RowEditing - Error: {ex.Message}");
                ShowMessage("Error loading appointment data: " + ex.Message, "danger");
            }
        }

        protected void gvAppointments_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string appointmentId = gvAppointments.DataKeys[e.RowIndex].Value.ToString();
                    string query = "DELETE FROM donation_appointments WHERE appointment_id = @appointmentId AND hospital_id = @hospitalId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@appointmentId", appointmentId);
                        cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            AddNotification(Convert.ToInt32(Session["UserId"]), "Appointment Deleted", $"Donation appointment deleted: ID {appointmentId}");
                            ShowMessage("Appointment deleted successfully.", "success");
                        }
                        else
                        {
                            ShowMessage("Appointment not found or you lack permission to delete it.", "danger");
                        }
                    }
                    LoadAppointments();
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] gvAppointments_RowDeleting - MySQL Error: {ex.Message}");
                ShowMessage("Error deleting appointment: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] gvAppointments_RowDeleting - Error: {ex.Message}");
                ShowMessage("Error deleting appointment: " + ex.Message, "danger");
            }
        }

        protected void gvAppointments_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                gvAppointments.PageIndex = e.NewPageIndex;
                LoadAppointments();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] gvAppointments_PageIndexChanging - Error: {ex.Message}");
                ShowMessage("Error changing page: " + ex.Message, "danger");
            }
        }

        protected void gvAppointments_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                try
                {
                    Button btnDelete = (Button)e.Row.FindControl("btnDelete");
                    if (btnDelete != null)
                    {
                        btnDelete.OnClientClick = "return confirm('Are you sure you want to delete this appointment?');";
                        controlsToRegister.Add(btnDelete.UniqueID);
                    }

                    Button btnEdit = (Button)e.Row.FindControl("btnEdit");
                    if (btnEdit != null)
                    {
                        controlsToRegister.Add(btnEdit.UniqueID);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[{DateTime.Now}] gvAppointments_RowDataBound - Error: {ex.Message}");
                    ShowMessage("Error binding row: " + ex.Message, "danger");
                }
            }
        }

        private void LoadAppointmentData(string appointmentId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT appointment_id, donor_id, appointment_date, status
                                    FROM donation_appointments 
                                    WHERE appointment_id = @appointmentId AND hospital_id = @hospitalId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@appointmentId", appointmentId);
                        cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hdnAppointmentId.Value = reader["appointment_id"].ToString();
                                ddlDonor.SelectedValue = reader["donor_id"].ToString();
                                txtAppointmentDate.Text = Convert.ToDateTime(reader["appointment_date"]).ToString("yyyy-MM-ddTHH:mm");
                                ddlStatus.SelectedValue = reader["status"].ToString();
                            }
                            else
                            {
                                ShowMessage("Appointment not found or you lack permission to edit it.", "danger");
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadAppointmentData - MySQL Error: {ex.Message}");
                ShowMessage("Error loading appointment data: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadAppointmentData - Error: {ex.Message}");
                ShowMessage("Error loading appointment data: " + ex.Message, "danger");
            }
        }

        protected void btnSaveAppointment_Click(object sender, EventArgs e)
        {
            if (!ValidateAppointmentForm()) return;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    DateTime appointmentDate = DateTime.Parse(txtAppointmentDate.Text);

                    if (string.IsNullOrEmpty(hdnAppointmentId.Value)) // New appointment
                    {
                        string query = @"INSERT INTO donation_appointments (hospital_id, donor_id, appointment_date, status, created_at)
                                        VALUES (@hospitalId, @donorId, @appointmentDate, @status, CURRENT_TIMESTAMP)";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                            cmd.Parameters.AddWithValue("@donorId", ddlDonor.SelectedValue);
                            cmd.Parameters.AddWithValue("@appointmentDate", appointmentDate);
                            cmd.Parameters.AddWithValue("@status", ddlStatus.SelectedValue);
                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                AddNotification(Convert.ToInt32(Session["UserId"]), "Appointment Added",
                                    $"New appointment added for donor {ddlDonor.SelectedItem.Text} on {appointmentDate:yyyy-MM-dd HH:mm}");
                                ShowMessage("Appointment added successfully.", "success");
                            }
                            else
                            {
                                ShowMessage("Failed to add appointment.", "danger");
                            }
                        }
                    }
                    else // Update existing appointment
                    {
                        string query = @"UPDATE donation_appointments SET donor_id = @donorId, appointment_date = @appointmentDate, 
                                        status = @status
                                        WHERE appointment_id = @appointmentId AND hospital_id = @hospitalId";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@donorId", ddlDonor.SelectedValue);
                            cmd.Parameters.AddWithValue("@appointmentDate", appointmentDate);
                            cmd.Parameters.AddWithValue("@status", ddlStatus.SelectedValue);
                            cmd.Parameters.AddWithValue("@appointmentId", hdnAppointmentId.Value);
                            cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                AddNotification(Convert.ToInt32(Session["UserId"]), "Appointment Updated",
                                    $"Appointment updated for donor {ddlDonor.SelectedItem.Text} on {appointmentDate:yyyy-MM-dd HH:mm}");
                                ShowMessage("Appointment updated successfully.", "success");
                            }
                            else
                            {
                                ShowMessage("Appointment not found or you lack permission to update it.", "danger");
                            }
                        }
                    }

                    LoadAppointments();
                    ClearForm();
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnSaveAppointment_Click - MySQL Error: {ex.Message}");
                ShowMessage("Error saving appointment: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnSaveAppointment_Click - Error: {ex.Message}");
                ShowMessage("Error saving appointment: " + ex.Message, "danger");
            }
        }

        protected void btnClearForm_Click(object sender, EventArgs e)
        {
            try
            {
                ClearForm();
                LoadAppointments();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnClearForm_Click - Error: {ex.Message}");
                ShowMessage("Error clearing form: " + ex.Message, "danger");
            }
        }

        private void ClearForm()
        {
            hdnAppointmentId.Value = "";
            ddlDonor.SelectedIndex = 0;
            txtAppointmentDate.Text = "";
            ddlStatus.SelectedIndex = 0;
        }

        private bool ValidateAppointmentForm()
        {
            if (string.IsNullOrEmpty(ddlDonor.SelectedValue) ||
                string.IsNullOrEmpty(txtAppointmentDate.Text) ||
                string.IsNullOrEmpty(ddlStatus.SelectedValue))
            {
                ShowMessage("All fields are required.", "danger");
                return false;
            }

            try
            {
                DateTime appointmentDate = DateTime.Parse(txtAppointmentDate.Text);
                if (appointmentDate < DateTime.Now)
                {
                    ShowMessage("Appointment date must be in the future.", "danger");
                    return false;
                }
            }
            catch (FormatException)
            {
                ShowMessage("Invalid date format.", "danger");
                return false;
            }

            return true;
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
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] AddNotification - MySQL Error: {ex.Message}");
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
            try
            {
                Session.Clear();
                Session.Abandon();
                Response.Redirect("Login.aspx");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] lnkLogout_Click - Error: {ex.Message}");
                ShowMessage("Error logging out: " + ex.Message, "danger");
            }
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