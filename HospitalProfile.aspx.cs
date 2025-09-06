using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Web.UI;

namespace ClinicalBloodBank
{
    public partial class HospitalProfile : System.Web.UI.Page
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null || Session["UserType"] == null || Session["UserType"].ToString() != "hospital")
            {
                Response.Redirect("Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadHospitalDetails();
            }
        }

        private void LoadHospitalDetails()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT hospital_name, contact_email, phone, address, contact_password FROM hospitals WHERE hospital_id = @hospitalId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtHospitalName.Text = reader["hospital_name"].ToString();
                                txtEmail.Text = reader["contact_email"].ToString();
                                txtPhone.Text = reader["phone"]?.ToString();
                                txtAddress.Text = reader["address"]?.ToString();
                                txtPassword.Text = reader["contact_password"].ToString();
                                litUserName.Text = reader["hospital_name"].ToString();
                                litUserInitials.Text = GetInitials(reader["hospital_name"].ToString());
                            }
                            else
                            {
                                litUserName.Text = Session["UserName"]?.ToString() ?? "Hospital";
                                litUserInitials.Text = GetInitials(Session["UserName"]?.ToString() ?? "Hospital");
                                ShowMessage("Hospital profile not found.", "danger");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] LoadHospitalDetails Error: {ex.Message}");
                    ShowMessage("Error loading hospital profile.", "danger");
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

        protected void btnUpdateProfile_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                ShowMessage("Please correct the form errors.", "danger");
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"UPDATE hospitals 
                                    SET hospital_name = @hospitalName, contact_email = @email, phone = @phone, 
                                        address = @address, contact_password = @password
                                    WHERE hospital_id = @hospitalId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@hospitalName", txtHospitalName.Text.Trim());
                        cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                        cmd.Parameters.AddWithValue("@phone", string.IsNullOrEmpty(txtPhone.Text) ? (object)DBNull.Value : txtPhone.Text.Trim());
                        cmd.Parameters.AddWithValue("@address", string.IsNullOrEmpty(txtAddress.Text) ? (object)DBNull.Value : txtAddress.Text.Trim());
                        cmd.Parameters.AddWithValue("@password", txtPassword.Text);
                        cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            LogNotification($"Updated hospital profile for ID {Session["UserId"]}");
                            Session["UserName"] = txtHospitalName.Text.Trim();
                            litUserName.Text = txtHospitalName.Text.Trim();
                            litUserInitials.Text = GetInitials(txtHospitalName.Text.Trim());
                            ShowMessage("Profile updated successfully.", "success");
                        }
                        else
                        {
                            ShowMessage("Profile not found or unauthorized.", "danger");
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] btnUpdateProfile_Click Error: {ex.Message}");
                    ShowMessage("Error updating profile.", "danger");
                }
            }
        }

        private void LogNotification(string message)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "INSERT INTO notifications (hospital_id, title, message, is_read, created_at) VALUES (@hospitalId, @title, @message, 0, NOW())";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                        cmd.Parameters.AddWithValue("@title", "Profile Update");
                        cmd.Parameters.AddWithValue("@message", message);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] LogNotification Error: {ex.Message}");
                }
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