using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ClinicalBloodBank
{
    public partial class BookAppointment : System.Web.UI.Page
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
                    CheckEligibility();
                    LoadHospitals();
                    LoadUpcomingAppointments();
                    LoadNotificationCount();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Page_Load Error: " + ex.Message);
                    errorMessage.InnerText = "An error occurred while loading the appointment booking page.";
                    errorMessage.Style["display"] = "block";
                }
            }
        }

        private void CheckEligibility()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"]?.ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                eligibilityCheck.Visible = false;
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT DATE_ADD(COALESCE(MAX(donation_date), '1900-01-01'), INTERVAL 56 DAY) as next_eligible_date
                             FROM blood_inventory 
                             WHERE donor_id = @donorId";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@donorId", Session["UserId"]);
                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            DateTime nextEligibleDate = Convert.ToDateTime(result);

                            if (nextEligibleDate > DateTime.Now)
                            {
                                eligibilityCheck.Attributes["class"] = "eligibility-check fail";
                                eligibilityCheck.InnerHtml = $@"
                                    <h3>Eligibility Check</h3>
                                    <p>Based on your last donation date, you are not eligible to donate blood until {nextEligibleDate:yyyy-MM-dd}.</p>
                                    <p>Please come back after this date to book an appointment.</p>";
                                btnBookAppointment.Enabled = false;
                            }
                            else
                            {
                                eligibilityCheck.Attributes["class"] = "eligibility-check";
                                eligibilityCheck.InnerHtml = @"
                                    <h3>Eligibility Check</h3>
                                    <p>Based on your last donation date, you are eligible to donate blood.</p>";
                                btnBookAppointment.Enabled = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("CheckEligibility Error: " + ex.Message);
                    eligibilityCheck.Visible = false;
                }
            }
        }

        private void LoadHospitals()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"]?.ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT hospital_id, hospital_name FROM hospitals WHERE is_verified = 1 ORDER BY hospital_name";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            ddlHospital.Items.Clear();
                            ddlHospital.Items.Add(new ListItem("Select Hospital", ""));

                            while (reader.Read())
                            {
                                ddlHospital.Items.Add(new ListItem(
                                    reader["hospital_name"].ToString(),
                                    reader["hospital_id"].ToString()
                                ));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("LoadHospitals Error: " + ex.Message);
                }
            }
        }

        private void LoadTimeSlots(int hospitalId, DateTime selectedDate)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"]?.ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                return;
            }

            ddlTimeSlot.Items.Clear();

            // Generate time slots (9 AM to 4 PM, every hour)
            for (int hour = 9; hour <= 16; hour++)
            {
                string timeText = $"{hour}:00 - {hour + 1}:00";
                string timeValue = $"{hour}:00:00";

                ddlTimeSlot.Items.Add(new ListItem(timeText, timeValue));
            }

            // Check which time slots are already booked
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT TIME(appointment_date) as time_slot
                             FROM donation_appointments
                             WHERE hospital_id = @hospitalId 
                             AND DATE(appointment_date) = @selectedDate
                             AND status = 'scheduled'";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@hospitalId", hospitalId);
                        cmd.Parameters.AddWithValue("@selectedDate", selectedDate.ToString("yyyy-MM-dd"));

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                TimeSpan bookedTime = (TimeSpan)reader["time_slot"];
                                string bookedTimeStr = bookedTime.ToString(@"hh\:mm\:ss");

                                // Disable the booked time slot
                                ListItem item = ddlTimeSlot.Items.FindByValue(bookedTimeStr);
                                if (item != null)
                                {
                                    item.Enabled = false;
                                    item.Text += " (Booked)";
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("LoadTimeSlots Error: " + ex.Message);
                }
            }
        }

        private void LoadUpcomingAppointments()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"]?.ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                upcomingAppointments.Visible = false;
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
                             ORDER BY a.appointment_date ASC";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@donorId", Session["UserId"]);
                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            rptUpcomingAppointments.DataSource = dt;
                            rptUpcomingAppointments.DataBind();
                            lblNoUpcomingAppointments.Visible = false;
                            upcomingAppointments.Visible = true;
                        }
                        else
                        {
                            rptUpcomingAppointments.DataSource = null;
                            rptUpcomingAppointments.DataBind();
                            lblNoUpcomingAppointments.Visible = true;
                            upcomingAppointments.Visible = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("LoadUpcomingAppointments Error: " + ex.Message);
                    upcomingAppointments.Visible = false;
                }
            }
        }

        private void LoadNotificationCount()
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
                    string countQuery = "SELECT COUNT(*) FROM notifications WHERE is_read = 0 AND donor_id = @donorId";
                    using (MySqlCommand cmd = new MySqlCommand(countQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@donorId", Session["UserId"]);
                        notificationCount.InnerText = cmd.ExecuteScalar()?.ToString() ?? "0";
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("LoadNotificationCount Error: " + ex.Message);
                    notificationCount.InnerText = "0";
                }
            }
        }

        protected void ddlHospital_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ddlHospital.SelectedValue) && !string.IsNullOrEmpty(txtAppointmentDate.Text))
            {
                int hospitalId = Convert.ToInt32(ddlHospital.SelectedValue);
                DateTime selectedDate = Convert.ToDateTime(txtAppointmentDate.Text);
                LoadTimeSlots(hospitalId, selectedDate);
            }
        }

        protected void txtAppointmentDate_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ddlHospital.SelectedValue) && !string.IsNullOrEmpty(txtAppointmentDate.Text))
            {
                int hospitalId = Convert.ToInt32(ddlHospital.SelectedValue);
                DateTime selectedDate = Convert.ToDateTime(txtAppointmentDate.Text);
                LoadTimeSlots(hospitalId, selectedDate);
            }
        }

        protected void btnBookAppointment_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ddlHospital.SelectedValue) || string.IsNullOrEmpty(txtAppointmentDate.Text) || string.IsNullOrEmpty(ddlTimeSlot.SelectedValue))
            {
                errorMessage.InnerText = "Please select a hospital, date, and time slot.";
                errorMessage.Style["display"] = "block";
                return;
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

                    // Create appointment datetime from date and timeslot
                    DateTime appointmentDate = Convert.ToDateTime(txtAppointmentDate.Text);
                    TimeSpan timeSlot = TimeSpan.Parse(ddlTimeSlot.SelectedValue);
                    DateTime appointmentDateTime = appointmentDate.Add(timeSlot);

                    string query = @"INSERT INTO donation_appointments (donor_id, hospital_id, appointment_date, notes)
                             VALUES (@donorId, @hospitalId, @appointmentDate, @notes)";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@donorId", Session["UserId"]);
                        cmd.Parameters.AddWithValue("@hospitalId", Convert.ToInt32(ddlHospital.SelectedValue));
                        cmd.Parameters.AddWithValue("@appointmentDate", appointmentDateTime);
                        cmd.Parameters.AddWithValue("@notes", txtNotes.Text);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // Show success message
                            successMessage.InnerText = "Appointment booked successfully!";
                            successMessage.Style["display"] = "block";

                            // Reset form
                            ddlHospital.SelectedIndex = 0;
                            txtAppointmentDate.Text = "";
                            ddlTimeSlot.Items.Clear();
                            txtNotes.Text = "";

                            // Reload upcoming appointments
                            LoadUpcomingAppointments();
                        }
                        else
                        {
                            errorMessage.InnerText = "Failed to book appointment. Please try again.";
                            errorMessage.Style["display"] = "block";
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("btnBookAppointment_Click Error: " + ex.Message);
                    errorMessage.InnerText = "Error booking appointment. This time slot may have been booked by someone else.";
                    errorMessage.Style["display"] = "block";
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
                            LoadUpcomingAppointments();

                            // Show success message
                            successMessage.InnerText = "Appointment cancelled successfully.";
                            successMessage.Style["display"] = "block";
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
                            LoadNotificationCount();
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