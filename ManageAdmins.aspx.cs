using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ClinicalBloodBank
{
    public partial class ManageAdmins : System.Web.UI.Page
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
                    LoadAdmins();
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

        private void LoadAdmins()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT u.user_id as admin_id, u.first_name, u.last_name, u.email, u.phone, u.is_active 
                                    FROM users u 
                                    WHERE u.role = 'admin'";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        gvAdmins.DataSource = dt;
                        gvAdmins.DataBind();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("LoadAdmins Error: " + ex.Message);
                    ShowErrorMessage("Error loading admins.");
                }
            }
        }

        protected void gvAdmins_RowEditing(object sender, GridViewEditEventArgs e)
        {
            try
            {
                string adminId = gvAdmins.DataKeys[e.NewEditIndex].Value.ToString();
                LoadAdminData(adminId);
                e.Cancel = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("gvAdmins_RowEditing Error: " + ex.Message);
                ShowErrorMessage("Error editing admin.");
            }
        }

        protected void gvAdmins_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string adminId = gvAdmins.DataKeys[e.RowIndex].Value.ToString();
                    string query = "UPDATE users SET is_active = 0 WHERE user_id = @userId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", adminId);
                        cmd.ExecuteNonQuery();
                    }

                    LoadAdmins();
                    AddNotification("Admin Deactivated", $"Administrator account has been deactivated: ID {adminId}");
                    ShowSuccessMessage("Admin deactivated successfully.");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("gvAdmins_RowDeleting Error: " + ex.Message);
                    ShowErrorMessage("Error deactivating admin.");
                }
            }
        }

        private void LoadAdminData(string adminId)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT u.user_id as admin_id, u.first_name, u.last_name, u.email, u.phone, u.is_active 
                                    FROM users u 
                                    WHERE u.user_id = @adminId AND u.role = 'admin'";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@adminId", adminId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hdnAdminId.Value = reader["admin_id"].ToString();
                                txtAdminFirstName.Text = reader["first_name"].ToString();
                                txtAdminLastName.Text = reader["last_name"].ToString();
                                txtAdminEmail.Text = reader["email"].ToString();
                                txtAdminPhone.Text = reader["phone"].ToString();
                                cbAdminActive.Checked = Convert.ToBoolean(reader["is_active"]);
                                txtAdminPassword.Visible = false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("LoadAdminData Error: " + ex.Message);
                    ShowErrorMessage("Error loading admin data.");
                }
            }
        }

        protected void btnSaveAdmin_Click(object sender, EventArgs e)
        {
            if (!ValidateAdminForm()) return;

            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    if (string.IsNullOrEmpty(hdnAdminId.Value)) // New admin
                    {
                        string userQuery = @"INSERT INTO users (email, password, role, first_name, last_name, phone, is_active, created_at) 
                                          VALUES (@email, @password, 'admin', @firstName, @lastName, @phone, 1, NOW());
                                          SELECT LAST_INSERT_ID();";
                        using (MySqlCommand cmd = new MySqlCommand(userQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@email", txtAdminEmail.Text);
                            cmd.Parameters.AddWithValue("@password", txtAdminPassword.Text);
                            cmd.Parameters.AddWithValue("@firstName", txtAdminFirstName.Text);
                            cmd.Parameters.AddWithValue("@lastName", txtAdminLastName.Text);
                            cmd.Parameters.AddWithValue("@phone", txtAdminPhone.Text);
                            cmd.ExecuteNonQuery();

                            AddNotification("New Admin Added", $"New administrator registered: {txtAdminFirstName.Text} {txtAdminLastName.Text}");
                            ShowSuccessMessage("Admin added successfully.");
                        }
                    }
                    else // Update existing admin
                    {
                        string userQuery = @"UPDATE users SET first_name = @firstName, last_name = @lastName, email = @email, 
                                          phone = @phone, is_active = @isActive WHERE user_id = @userId";
                        using (MySqlCommand cmd = new MySqlCommand(userQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@firstName", txtAdminFirstName.Text);
                            cmd.Parameters.AddWithValue("@lastName", txtAdminLastName.Text);
                            cmd.Parameters.AddWithValue("@email", txtAdminEmail.Text);
                            cmd.Parameters.AddWithValue("@phone", txtAdminPhone.Text);
                            cmd.Parameters.AddWithValue("@isActive", cbAdminActive.Checked);
                            cmd.Parameters.AddWithValue("@userId", hdnAdminId.Value);
                            cmd.ExecuteNonQuery();
                        }

                        AddNotification("Admin Updated", $"Administrator information updated: {txtAdminFirstName.Text} {txtAdminLastName.Text}");
                        ShowSuccessMessage("Admin updated successfully.");
                    }

                    LoadAdmins();
                    ClearForm();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("btnSaveAdmin_Click Error: " + ex.Message);
                    ShowErrorMessage("Error saving admin.");
                }
            }
        }

        protected void btnClearForm_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            hdnAdminId.Value = "";
            txtAdminFirstName.Text = "";
            txtAdminLastName.Text = "";
            txtAdminEmail.Text = "";
            txtAdminPassword.Text = "";
            txtAdminPhone.Text = "";
            cbAdminActive.Checked = true;
            txtAdminPassword.Visible = true;
        }

        private bool ValidateAdminForm()
        {
            if (string.IsNullOrEmpty(txtAdminFirstName.Text) || string.IsNullOrEmpty(txtAdminLastName.Text) ||
                string.IsNullOrEmpty(txtAdminEmail.Text) || string.IsNullOrEmpty(txtAdminPhone.Text))
            {
                ShowErrorMessage("All fields are required.");
                return false;
            }
            if (string.IsNullOrEmpty(hdnAdminId.Value) && string.IsNullOrEmpty(txtAdminPassword.Text))
            {
                ShowErrorMessage("Password is required for new admins.");
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