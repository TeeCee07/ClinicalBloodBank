
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
    public partial class ManageAdmins : System.Web.UI.Page
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
                LoadUserInfo();

                if (Session["Message"] != null)
                {
                    pnlMessage.Visible = true;
                    lblMessage.Text = Session["Message"].ToString();
                    pnlMessage.CssClass = "alert alert-" + Session["MessageType"].ToString();
                    Session["Message"] = null;
                    Session["MessageType"] = null;
                }

                if (!IsPostBack)
                {
                    LoadAdmins();
                    ClearForm();
                }
            }
            catch (MySqlException sqlEx)
            {
                ShowMessage("Database error: " + sqlEx.Message, "danger");
                Debug.WriteLine($"[{DateTime.Now}] Page_Load - MySQL Error: {sqlEx.Message}");
            }
            catch (Exception ex)
            {
                ShowMessage("Error: " + ex.Message, "danger");
                Debug.WriteLine($"[{DateTime.Now}] Page_Load - Error: {ex.Message}");
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

        private void LoadAdmins()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    string query = @"SELECT admin_id, first_name, last_name, email, phone, is_active 
                                    FROM admins";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        conn.Open();
                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        gvAdmins.DataSource = dt;
                        gvAdmins.DataBind();

                        if (dt.Rows.Count == 0)
                        {
                            ShowMessage("No admins found in the system.", "info");
                        }
                        else
                        {
                            ShowMessage($"{dt.Rows.Count} admin(s) found.", "success");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                ShowMessage("Error loading admins: " + ex.Message, "danger");
                Debug.WriteLine($"[{DateTime.Now}] LoadAdmins - MySQL Error: {ex.Message}");
                gvAdmins.DataSource = null;
                gvAdmins.DataBind();
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading admins: " + ex.Message, "danger");
                Debug.WriteLine($"[{DateTime.Now}] LoadAdmins - Error: {ex.Message}");
                gvAdmins.DataSource = null;
                gvAdmins.DataBind();
            }
        }

        protected void btnSaveAdmin_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                ShowMessage("Please fix validation errors.", "danger");
                LoadAdmins();
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string adminId = hdnAdminId.Value;
                    bool isNew = string.IsNullOrEmpty(adminId);

                    if (isNew)
                    {
                        string checkEmailQuery = "SELECT COUNT(*) FROM admins WHERE email = @Email";
                        using (MySqlCommand checkCmd = new MySqlCommand(checkEmailQuery, conn))
                        {
                            checkCmd.Parameters.AddWithValue("@Email", txtAdminEmail.Text.Trim());
                            long count = (long)checkCmd.ExecuteScalar();
                            if (count > 0)
                            {
                                ShowMessage("Email already exists. Please use a different email.", "danger");
                                LoadAdmins();
                                return;
                            }
                        }

                        string insertQuery = @"INSERT INTO admins 
                            (email, password, first_name, last_name, phone, is_active, created_at) 
                            VALUES (@Email, @Password, @FirstName, @LastName, @Phone, @IsActive, CURRENT_TIMESTAMP);
                            SELECT LAST_INSERT_ID()";

                        using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn))
                        {
                            AddAdminParameters(cmd, isNew);
                            cmd.ExecuteNonQuery();

                            cmd.CommandText = "SELECT LAST_INSERT_ID()";
                            int newAdminId = Convert.ToInt32(cmd.ExecuteScalar());

                            InsertNotification(conn, newAdminId, "New Admin Added",
                                $"New administrator {txtAdminFirstName.Text.Trim()} {txtAdminLastName.Text.Trim()} was added.");
                        }

                        ShowMessage("Admin added successfully!", "success");
                    }
                    else
                    {
                        string updateQuery = @"UPDATE admins SET 
                            first_name = @FirstName,
                            last_name = @LastName,
                            email = @Email,
                            phone = @Phone,
                            is_active = @IsActive";
                        if (!string.IsNullOrEmpty(txtAdminPassword.Text.Trim()))
                        {
                            updateQuery += ", password = @Password";
                        }
                        updateQuery += " WHERE admin_id = @AdminId";

                        using (MySqlCommand cmd = new MySqlCommand(updateQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@AdminId", adminId);
                            AddAdminParameters(cmd, isNew);
                            cmd.ExecuteNonQuery();

                            InsertNotification(conn, Convert.ToInt32(adminId), "Admin Updated",
                                $"Administrator {txtAdminFirstName.Text.Trim()} {txtAdminLastName.Text.Trim()} was updated.");
                        }

                        ShowMessage("Admin updated successfully!", "success");
                    }

                    ClearForm();
                    LoadAdmins();
                }
            }
            catch (MySqlException ex)
            {
                ShowMessage("MySQL Error saving admin: " + ex.Message, "danger");
                Debug.WriteLine($"[{DateTime.Now}] btnSaveAdmin_Click - MySQL Error: {ex.Message}");
                LoadAdmins();
            }
            catch (Exception ex)
            {
                ShowMessage("Error saving admin: " + ex.Message, "danger");
                Debug.WriteLine($"[{DateTime.Now}] btnSaveAdmin_Click - Error: {ex.Message}");
                LoadAdmins();
            }
        }

        private void AddAdminParameters(MySqlCommand cmd, bool isNew)
        {
            cmd.Parameters.AddWithValue("@FirstName", txtAdminFirstName.Text.Trim());
            cmd.Parameters.AddWithValue("@LastName", txtAdminLastName.Text.Trim());
            cmd.Parameters.AddWithValue("@Email", txtAdminEmail.Text.Trim());
            cmd.Parameters.AddWithValue("@Phone", string.IsNullOrEmpty(txtAdminPhone.Text.Trim()) ? "" : txtAdminPhone.Text.Trim());
            cmd.Parameters.AddWithValue("@IsActive", cbAdminActive.Checked);
            if (isNew || !string.IsNullOrEmpty(txtAdminPassword.Text.Trim()))
            {
                cmd.Parameters.AddWithValue("@Password", txtAdminPassword.Text.Trim());
            }
        }

        protected void btnClearForm_Click(object sender, EventArgs e)
        {
            ClearForm();
            LoadAdmins();
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
            rfvPassword.Enabled = true;
        }

        private void LoadAdminData(string adminId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT admin_id, first_name, last_name, email, phone, is_active 
                                    FROM admins 
                                    WHERE admin_id = @AdminId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@AdminId", adminId);
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
                                txtAdminPassword.Visible = true;
                                rfvPassword.Enabled = false; // Password not required for edits
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                ShowMessage("Error loading admin data: " + ex.Message, "danger");
                Debug.WriteLine($"[{DateTime.Now}] LoadAdminData - MySQL Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading admin data: " + ex.Message, "danger");
                Debug.WriteLine($"[{DateTime.Now}] LoadAdminData - Error: {ex.Message}");
            }
        }

        protected void gvAdmins_RowEditing(object sender, GridViewEditEventArgs e)
        {
            try
            {
                string adminId = gvAdmins.DataKeys[e.NewEditIndex].Value.ToString();
                LoadAdminData(adminId);
                gvAdmins.EditIndex = -1;
                LoadAdmins();
            }
            catch (MySqlException ex)
            {
                ShowMessage("Error loading admin data: " + ex.Message, "danger");
                Debug.WriteLine($"[{DateTime.Now}] gvAdmins_RowEditing - MySQL Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading admin data: " + ex.Message, "danger");
                Debug.WriteLine($"[{DateTime.Now}] gvAdmins_RowEditing - Error: {ex.Message}");
            }
        }

        protected void gvAdmins_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string adminId = gvAdmins.DataKeys[e.RowIndex].Value.ToString();
                    string adminName = "";
                    string nameQuery = "SELECT CONCAT(first_name, ' ', last_name) FROM admins WHERE admin_id = @AdminId";
                    using (MySqlCommand nameCmd = new MySqlCommand(nameQuery, conn))
                    {
                        nameCmd.Parameters.AddWithValue("@AdminId", adminId);
                        adminName = nameCmd.ExecuteScalar()?.ToString() ?? "";
                    }

                    string query = "UPDATE admins SET is_active = 0 WHERE admin_id = @AdminId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@AdminId", adminId);
                        cmd.ExecuteNonQuery();
                    }

                    InsertNotification(conn, Convert.ToInt32(adminId), "Admin Deactivated",
                        $"Administrator {adminName} was deactivated.");
                    ShowMessage("Admin deactivated successfully!", "success");
                    ClearForm();
                    LoadAdmins();
                }
            }
            catch (MySqlException ex)
            {
                ShowMessage("MySQL Error deactivating admin: " + ex.Message, "danger");
                Debug.WriteLine($"[{DateTime.Now}] gvAdmins_RowDeleting - MySQL Error: {ex.Message}");
                LoadAdmins();
            }
            catch (Exception ex)
            {
                ShowMessage("Error deactivating admin: " + ex.Message, "danger");
                Debug.WriteLine($"[{DateTime.Now}] gvAdmins_RowDeleting - Error: {ex.Message}");
                LoadAdmins();
            }
        }

        protected void gvAdmins_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvAdmins.PageIndex = e.NewPageIndex;
            LoadAdmins();
        }

        protected void gvAdmins_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                Button btnDelete = (Button)e.Row.FindControl("btnDelete");
                if (btnDelete != null)
                {
                    btnDelete.OnClientClick = "return confirm('Are you sure you want to deactivate this admin?');";
                    controlsToRegister.Add(btnDelete.UniqueID);
                }

                Button btnEdit = (Button)e.Row.FindControl("btnEdit");
                if (btnEdit != null)
                {
                    controlsToRegister.Add(btnEdit.UniqueID);
                }
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

        private void InsertNotification(MySqlConnection conn, int? adminId, string title, string message)
        {
            try
            {
                string query = @"INSERT INTO notifications 
                                (admin_id, title, message, is_read, created_at)
                                VALUES (@AdminId, @Title, @Message, 0, CURRENT_TIMESTAMP)";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@AdminId", adminId.HasValue ? (object)adminId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Title", title);
                    cmd.Parameters.AddWithValue("@Message", message);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] InsertNotification - MySQL Error: {ex.Message}");
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

        protected void Page_PreRender(object sender, EventArgs e)
        {
            // Placeholder for future use, e.g., restoring search state
        }
    }
}