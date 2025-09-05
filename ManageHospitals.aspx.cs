using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ClinicalBloodBank
{
    public partial class ManageHospitals : System.Web.UI.Page
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
                    LoadHospitals();
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

        private void LoadHospitals()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT h.hospital_id, h.hospital_name, h.license_number, h.address, 
                                    u.first_name as contact_person, u.email, u.phone, h.is_verified 
                                    FROM hospitals h 
                                    INNER JOIN users u ON h.user_id = u.user_id";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        gvHospitals.DataSource = dt;
                        gvHospitals.DataBind();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("LoadHospitals Error: " + ex.Message);
                    ShowErrorMessage("Error loading hospitals.");
                }
            }
        }

        protected void gvHospitals_RowEditing(object sender, GridViewEditEventArgs e)
        {
            try
            {
                string hospitalId = gvHospitals.DataKeys[e.NewEditIndex].Value.ToString();
                LoadHospitalData(hospitalId);
                e.Cancel = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("gvHospitals_RowEditing Error: " + ex.Message);
                ShowErrorMessage("Error editing hospital.");
            }
        }

        protected void gvHospitals_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string hospitalId = gvHospitals.DataKeys[e.RowIndex].Value.ToString();
                    string getUserIdQuery = "SELECT user_id FROM hospitals WHERE hospital_id = @hospitalId";
                    string userId;
                    using (MySqlCommand cmd = new MySqlCommand(getUserIdQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@hospitalId", hospitalId);
                        userId = cmd.ExecuteScalar()?.ToString();
                    }

                    if (userId != null)
                    {
                        string userQuery = "UPDATE users SET is_active = 0 WHERE user_id = @userId";
                        using (MySqlCommand cmd = new MySqlCommand(userQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@userId", userId);
                            cmd.ExecuteNonQuery();
                        }

                        string hospitalQuery = "UPDATE hospitals SET is_verified = 0 WHERE hospital_id = @hospitalId";
                        using (MySqlCommand cmd = new MySqlCommand(hospitalQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@hospitalId", hospitalId);
                            cmd.ExecuteNonQuery();
                        }

                        LoadHospitals();
                        AddNotification("Hospital Deactivated", $"Hospital has been deactivated: ID {hospitalId}");
                        ShowSuccessMessage("Hospital deactivated successfully.");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("gvHospitals_RowDeleting Error: " + ex.Message);
                    ShowErrorMessage("Error deactivating hospital.");
                }
            }
        }

        private void LoadHospitalData(string hospitalId)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT h.hospital_id, h.hospital_name, h.license_number, h.address, h.is_verified,
                                    u.first_name as contact_person, u.email, u.phone
                                    FROM hospitals h 
                                    INNER JOIN users u ON h.user_id = u.user_id
                                    WHERE h.hospital_id = @hospitalId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@hospitalId", hospitalId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hdnHospitalId.Value = reader["hospital_id"].ToString();
                                txtHospitalName.Text = reader["hospital_name"].ToString();
                                txtLicenseNumber.Text = reader["license_number"].ToString();
                                txtHospitalAddress.Text = reader["address"].ToString();
                                txtContactPerson.Text = reader["contact_person"].ToString();
                                txtHospitalEmail.Text = reader["email"].ToString();
                                txtHospitalPhone.Text = reader["phone"].ToString();
                                cbIsVerified.Checked = Convert.ToBoolean(reader["is_verified"]);
                                txtHospitalPassword.Visible = false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("LoadHospitalData Error: " + ex.Message);
                    ShowErrorMessage("Error loading hospital data.");
                }
            }
        }

        protected void btnSaveHospital_Click(object sender, EventArgs e)
        {
            if (!ValidateHospitalForm()) return;

            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    if (string.IsNullOrEmpty(hdnHospitalId.Value)) // New hospital
                    {
                        string userQuery = @"INSERT INTO users (email, password, role, first_name, last_name, phone, address, is_active, created_at) 
                                          VALUES (@email, @password, 'hospital', @firstName, @lastName, @phone, @address, 1, NOW());
                                          SELECT LAST_INSERT_ID();";
                        using (MySqlCommand cmd = new MySqlCommand(userQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@email", txtHospitalEmail.Text);
                            cmd.Parameters.AddWithValue("@password", txtHospitalPassword.Text);
                            cmd.Parameters.AddWithValue("@firstName", txtContactPerson.Text.Split(' ')[0]);
                            cmd.Parameters.AddWithValue("@lastName", txtContactPerson.Text.Split(' ').Length > 1 ? txtContactPerson.Text.Split(' ')[1] : "");
                            cmd.Parameters.AddWithValue("@phone", txtHospitalPhone.Text);
                            cmd.Parameters.AddWithValue("@address", txtHospitalAddress.Text);
                            int userId = Convert.ToInt32(cmd.ExecuteScalar());

                            string hospitalQuery = @"INSERT INTO hospitals (user_id, hospital_name, license_number, address, city, is_verified) 
                                                  VALUES (@userId, @hospitalName, @licenseNumber, @address, 'Unknown', @isVerified)";
                            using (MySqlCommand hospitalCmd = new MySqlCommand(hospitalQuery, conn))
                            {
                                hospitalCmd.Parameters.AddWithValue("@userId", userId);
                                hospitalCmd.Parameters.AddWithValue("@hospitalName", txtHospitalName.Text);
                                hospitalCmd.Parameters.AddWithValue("@licenseNumber", txtLicenseNumber.Text);
                                hospitalCmd.Parameters.AddWithValue("@address", txtHospitalAddress.Text);
                                hospitalCmd.Parameters.AddWithValue("@isVerified", cbIsVerified.Checked);
                                hospitalCmd.ExecuteNonQuery();
                            }

                            AddNotification("New Hospital Added", $"New hospital registered: {txtHospitalName.Text}");
                            ShowSuccessMessage("Hospital added successfully.");
                        }
                    }
                    else // Update existing hospital
                    {
                        string getUserIdQuery = "SELECT user_id FROM hospitals WHERE hospital_id = @hospitalId";
                        string userId;
                        using (MySqlCommand cmd = new MySqlCommand(getUserIdQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@hospitalId", hdnHospitalId.Value);
                            userId = cmd.ExecuteScalar()?.ToString();
                        }

                        if (userId != null)
                        {
                            string userQuery = @"UPDATE users SET first_name = @firstName, last_name = @lastName, email = @email, 
                                              phone = @phone, address = @address WHERE user_id = @userId";
                            using (MySqlCommand cmd = new MySqlCommand(userQuery, conn))
                            {
                                cmd.Parameters.AddWithValue("@firstName", txtContactPerson.Text.Split(' ')[0]);
                                cmd.Parameters.AddWithValue("@lastName", txtContactPerson.Text.Split(' ').Length > 1 ? txtContactPerson.Text.Split(' ')[1] : "");
                                cmd.Parameters.AddWithValue("@email", txtHospitalEmail.Text);
                                cmd.Parameters.AddWithValue("@phone", txtHospitalPhone.Text);
                                cmd.Parameters.AddWithValue("@address", txtHospitalAddress.Text);
                                cmd.Parameters.AddWithValue("@userId", userId);
                                cmd.ExecuteNonQuery();
                            }

                            string hospitalQuery = @"UPDATE hospitals SET hospital_name = @hospitalName, license_number = @licenseNumber, 
                                                  address = @address, is_verified = @isVerified WHERE hospital_id = @hospitalId";
                            using (MySqlCommand cmd = new MySqlCommand(hospitalQuery, conn))
                            {
                                cmd.Parameters.AddWithValue("@hospitalName", txtHospitalName.Text);
                                cmd.Parameters.AddWithValue("@licenseNumber", txtLicenseNumber.Text);
                                cmd.Parameters.AddWithValue("@address", txtHospitalAddress.Text);
                                cmd.Parameters.AddWithValue("@isVerified", cbIsVerified.Checked);
                                cmd.Parameters.AddWithValue("@hospitalId", hdnHospitalId.Value);
                                cmd.ExecuteNonQuery();
                            }

                            AddNotification("Hospital Updated", $"Hospital information updated: {txtHospitalName.Text}");
                            ShowSuccessMessage("Hospital updated successfully.");
                        }
                    }

                    LoadHospitals();
                    ClearForm();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("btnSaveHospital_Click Error: " + ex.Message);
                    ShowErrorMessage("Error saving hospital.");
                }
            }
        }

        protected void btnClearForm_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            hdnHospitalId.Value = "";
            txtHospitalName.Text = "";
            txtLicenseNumber.Text = "";
            txtHospitalAddress.Text = "";
            txtContactPerson.Text = "";
            txtHospitalEmail.Text = "";
            txtHospitalPhone.Text = "";
            txtHospitalPassword.Text = "";
            cbIsVerified.Checked = false;
            txtHospitalPassword.Visible = true;
        }

        private bool ValidateHospitalForm()
        {
            if (string.IsNullOrEmpty(txtHospitalName.Text) || string.IsNullOrEmpty(txtLicenseNumber.Text) ||
                string.IsNullOrEmpty(txtHospitalAddress.Text) || string.IsNullOrEmpty(txtContactPerson.Text) ||
                string.IsNullOrEmpty(txtHospitalEmail.Text) || string.IsNullOrEmpty(txtHospitalPhone.Text))
            {
                ShowErrorMessage("All fields are required.");
                return false;
            }
            if (string.IsNullOrEmpty(hdnHospitalId.Value) && string.IsNullOrEmpty(txtHospitalPassword.Text))
            {
                ShowErrorMessage("Password is required for new hospitals.");
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