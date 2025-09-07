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
    public partial class ManageAppointments : System.Web.UI.Page
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
                if (!IsPostBack)
                {
                    LoadUserInfo();
                    LoadAppointments();
                    LoadStatusDropdown();
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

        private void LoadUserInfo()
        {
            if (Session["AdminName"] != null)
            {
                string adminName = Session["AdminName"].ToString();
                litUserName.Text = adminName;
                string[] nameParts = adminName.Split(' ');
                string initials = nameParts[0][0].ToString() + (nameParts.Length > 1 ? nameParts[1][0].ToString() : "");
                litUserInitials.Text = initials.ToUpper();
                Debug.WriteLine($"[{DateTime.Now}] LoadUserInfo - AdminName: {adminName}, Initials: {initials}");
            }
        }

        private void LoadAppointments()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT a.appointment_id, a.appointment_date, a.status, a.notes,
                                    CONCAT(d.first_name, ' ', d.last_name) as donor_name,
                                    h.hospital_name
                                    FROM donation_appointments a
                                    INNER JOIN donors d ON a.donor_id = d.donor_id
                                    INNER JOIN hospitals h ON a.hospital_id = h.hospital_id
                                    WHERE 1=1";

                    // Apply filters
                    string status = ddlFilterStatus.SelectedValue;
                    string filterDate = txtFilterDate.Text;

                    if (!string.IsNullOrEmpty(status))
                        query += " AND a.status = @Status";
                    if (!string.IsNullOrEmpty(filterDate))
                        query += " AND DATE(a.appointment_date) = @FilterDate";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        if (!string.IsNullOrEmpty(status))
                            cmd.Parameters.AddWithValue("@Status", status);
                        if (!string.IsNullOrEmpty(filterDate))
                            cmd.Parameters.AddWithValue("@FilterDate", filterDate);

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

        private void LoadStatusDropdown()
        {
            try
            {
                ddlFilterStatus.Items.Clear();
                ddlFilterStatus.Items.Add(new ListItem("All Statuses", ""));
                ddlFilterStatus.Items.Add(new ListItem("Scheduled", "scheduled"));
                ddlFilterStatus.Items.Add(new ListItem("Completed", "completed"));
                ddlFilterStatus.Items.Add(new ListItem("Cancelled", "cancelled"));
                ddlFilterStatus.Items.Add(new ListItem("No Show", "no_show"));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadStatusDropdown - Error: {ex.Message}");
                ShowMessage("Error loading status dropdown: " + ex.Message, "danger");
            }
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            LoadAppointments();
        }

        protected void btnClearFilter_Click(object sender, EventArgs e)
        {
            ddlFilterStatus.SelectedIndex = 0;
            txtFilterDate.Text = "";
            LoadAppointments();
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

        protected void gvAppointments_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvAppointments.PageIndex = e.NewPageIndex;
            LoadAppointments();
        }

        protected void gvAppointments_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                Button btnEdit = (Button)e.Row.FindControl("btnEdit");
                if (btnEdit != null)
                {
                    controlsToRegister.Add(btnEdit.UniqueID);
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
                    string query = @"SELECT a.appointment_id, a.appointment_date, a.status, a.notes,
                                    CONCAT(d.first_name, ' ', d.last_name) as donor_name,
                                    h.hospital_name
                                    FROM donation_appointments a
                                    INNER JOIN donors d ON a.donor_id = d.donor_id
                                    INNER JOIN hospitals h ON a.hospital_id = h.hospital_id
                                    WHERE a.appointment_id = @appointmentId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@appointmentId", appointmentId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hdnAppointmentId.Value = reader["appointment_id"].ToString();
                                txtAppointmentDonor.Text = reader["donor_name"].ToString();
                                txtAppointmentHospital.Text = reader["hospital_name"].ToString();
                                txtAppointmentDateTime.Text = Convert.ToDateTime(reader["appointment_date"]).ToString("yyyy-MM-ddTHH:mm");
                                ddlAppointmentStatus.SelectedValue = reader["status"].ToString();
                                txtAppointmentNotes.Text = reader["notes"].ToString();
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

        protected void btnUpdateAppointment_Click(object sender, EventArgs e)
        {
            if (!ValidateAppointmentForm()) return;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"UPDATE donation_appointments SET appointment_date = @appointmentDate, 
                                  status = @status, notes = @notes
                                  WHERE appointment_id = @appointmentId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@appointmentDate", txtAppointmentDateTime.Text);
                        cmd.Parameters.AddWithValue("@status", ddlAppointmentStatus.SelectedValue);
                        cmd.Parameters.AddWithValue("@notes", txtAppointmentNotes.Text);
                        cmd.Parameters.AddWithValue("@appointmentId", hdnAppointmentId.Value);
                        cmd.ExecuteNonQuery();
                    }

                    AddNotification(Convert.ToInt32(Session["AdminId"]), "Appointment Updated", $"Appointment {hdnAppointmentId.Value} has been updated to {ddlAppointmentStatus.SelectedValue}");
                    ShowMessage("Appointment updated successfully.", "success");
                    LoadAppointments();
                    ClearForm();
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnUpdateAppointment_Click - MySQL Error: {ex.Message}");
                ShowMessage("Error updating appointment: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnUpdateAppointment_Click - Error: {ex.Message}");
                ShowMessage("Error updating appointment: " + ex.Message, "danger");
            }
        }

        protected void btnClearForm_Click(object sender, EventArgs e)
        {
            ClearForm();
            LoadAppointments();
        }

        private void ClearForm()
        {
            hdnAppointmentId.Value = "";
            txtAppointmentDonor.Text = "";
            txtAppointmentHospital.Text = "";
            txtAppointmentDateTime.Text = "";
            ddlAppointmentStatus.SelectedIndex = 0;
            txtAppointmentNotes.Text = "";
        }

        private bool ValidateAppointmentForm()
        {
            if (string.IsNullOrEmpty(hdnAppointmentId.Value))
            {
                ShowMessage("No appointment selected.", "danger");
                return false;
            }
            if (string.IsNullOrEmpty(txtAppointmentDateTime.Text))
            {
                ShowMessage("Date and time are required.", "danger");
                return false;
            }
            if (string.IsNullOrEmpty(ddlAppointmentStatus.SelectedValue))
            {
                ShowMessage("Status is required.", "danger");
                return false;
            }

            // Validate appointment date and time
            try
            {
                DateTime appointmentDate = DateTime.Parse(txtAppointmentDateTime.Text);
                DayOfWeek day = appointmentDate.DayOfWeek;
                TimeSpan time = appointmentDate.TimeOfDay;

                if (day == DayOfWeek.Sunday)
                {
                    ShowMessage("Appointments cannot be scheduled on Sundays.", "danger");
                    return false;
                }
                else if (day == DayOfWeek.Saturday)
                {
                    if (time < new TimeSpan(8, 0, 0) || time > new TimeSpan(14, 0, 0))
                    {
                        ShowMessage("Appointments on Saturdays must be between 8:00 AM and 2:00 PM.", "danger");
                        return false;
                    }
                }
                else // Monday to Friday
                {
                    if (time < new TimeSpan(8, 0, 0) || time > new TimeSpan(17, 0, 0))
                    {
                        ShowMessage("Appointments from Monday to Friday must be between 8:00 AM and 5:00 PM.", "danger");
                        return false;
                    }
                }

                if (appointmentDate < DateTime.Now)
                {
                    ShowMessage("Appointments cannot be scheduled in the past.", "danger");
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

        private void AddNotification(int? adminId, string title, string message)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO notifications (admin_id, title, message, is_read, created_at) 
                                    VALUES (@adminId, @title, @message, 0, CURRENT_TIMESTAMP)";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@adminId", adminId.HasValue ? (object)adminId.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@title", title);
                        cmd.Parameters.AddWithValue("@message", message);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] AddNotification - MySQL Error: {ex.Message}");
                ShowMessage("Error adding notification: " + ex.Message, "danger");
            }
        }
        protected void lnkLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("Login.aspx");
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
            // Placeholder for future use, e.g., restoring filter state
        }
    }
}