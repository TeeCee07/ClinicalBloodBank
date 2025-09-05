using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ClinicalBloodBank
{
    public partial class ManageAppointments : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["UserType"] == null || Session["UserType"].ToString() != "admin")
                {
                    Response.Redirect("Login.aspx");
                    return;
                }

                try
                {
                    LoadAppointments();
                    litUserName.Text = Session["FirstName"] + " " + Session["LastName"];
                    litUserInitials.Text = Session["FirstName"].ToString().Substring(0, 1) + Session["LastName"].ToString().Substring(0, 1);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Page_Load Error: " + ex.Message);
                    ShowErrorMessage("An error occurred while loading the page. Please try again.");
                }
            }
        }

        private void ShowErrorMessage(string message)
        {
            ClientScript.RegisterStartupScript(this.GetType(), "showError", $"alert('{message}');", true);
        }

        private void ShowSuccessMessage(string message)
        {
            ClientScript.RegisterStartupScript(this.GetType(), "showSuccess", $"alert('{message}');", true);
        }

        private void LoadAppointments()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT a.appointment_id, a.appointment_date, a.status, a.notes,
                                    CONCAT(u.first_name, ' ', u.last_name) as donor_name,
                                    h.hospital_name
                                    FROM donation_appointments a
                                    INNER JOIN donors d ON a.donor_id = d.donor_id
                                    INNER JOIN users u ON d.user_id = u.user_id
                                    INNER JOIN hospitals h ON a.hospital_id = h.hospital_id";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        gvAppointments.DataSource = dt;
                        gvAppointments.DataBind();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("LoadAppointments Error: " + ex.Message);
                    ShowErrorMessage("Error loading appointments.");
                }
            }
        }

        protected void gvAppointments_RowEditing(object sender, GridViewEditEventArgs e)
        {
            try
            {
                string appointmentId = gvAppointments.DataKeys[e.NewEditIndex].Value.ToString();
                LoadAppointmentData(appointmentId);
                e.Cancel = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("gvAppointments_RowEditing Error: " + ex.Message);
                ShowErrorMessage("Error editing appointment.");
            }
        }

        private void LoadAppointmentData(string appointmentId)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT a.appointment_id, a.appointment_date, a.status, a.notes,
                                    CONCAT(u.first_name, ' ', u.last_name) as donor_name,
                                    h.hospital_name
                                    FROM donation_appointments a
                                    INNER JOIN donors d ON a.donor_id = d.donor_id
                                    INNER JOIN users u ON d.user_id = u.user_id
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
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("LoadAppointmentData Error: " + ex.Message);
                    ShowErrorMessage("Error loading appointment data.");
                }
            }
        }

        protected void btnUpdateAppointment_Click(object sender, EventArgs e)
        {
            if (!ValidateAppointmentForm()) return;

            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
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

                    LoadAppointments();
                    AddNotification("Appointment Updated", $"Appointment {hdnAppointmentId.Value} has been updated to {ddlAppointmentStatus.SelectedValue}");
                    ShowSuccessMessage("Appointment updated successfully.");
                    ClearForm();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("btnUpdateAppointment_Click Error: " + ex.Message);
                    ShowErrorMessage("Error updating appointment.");
                }
            }
        }

        protected void btnClearForm_Click(object sender, EventArgs e)
        {
            ClearForm();
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
            if (string.IsNullOrEmpty(hdnAppointmentId.Value) || string.IsNullOrEmpty(txtAppointmentDateTime.Text) || string.IsNullOrEmpty(ddlAppointmentStatus.SelectedValue))
            {
                ShowErrorMessage("Date and status are required.");
                return false;
            }
            return true;
        }

        private void AddNotification(string title, string message)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"INSERT INTO notifications (user_id, title, message, is_read, created_at) 
                                  VALUES (@userId, @title, @message, 0, NOW())";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", Session["UserId"]);
                        cmd.Parameters.AddWithValue("@title", title);
                        cmd.Parameters.AddWithValue("@message", message);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("AddNotification Error: " + ex.Message);
                    ShowErrorMessage("Error adding notification.");
                }
            }
        }
    }
}