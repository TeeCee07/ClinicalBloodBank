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
    public partial class ManageHospitals : System.Web.UI.Page
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
                    LoadHospitals();
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

        private void LoadHospitals()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    string query = @"SELECT hospital_id, hospital_name, address_line1, address_line2, city, province, 
                                    postal_code, country, license_number, contact_email, contact_password, is_verified 
                                    FROM hospitals 
                                    WHERE 1=1";

                    string searchText = txtSearch.Text.Trim();
                    string searchBy = ddlSearchBy.SelectedValue;

                    Debug.WriteLine($"[{DateTime.Now}] LoadHospitals - SearchBy: {searchBy}, SearchText: {searchText}");

                    MySqlCommand cmd = new MySqlCommand(query, conn);

                    if (!string.IsNullOrEmpty(searchText))
                    {
                        string[] validSearchFields = { "all", "hospital_name", "license_number", "contact_email", "city", "province", "postal_code" };
                        if (!Array.Exists(validSearchFields, field => field == searchBy))
                        {
                            ShowMessage("Invalid search field selected.", "danger");
                            Debug.WriteLine($"[{DateTime.Now}] LoadHospitals - Invalid search field: {searchBy}");
                            return;
                        }

                        if (searchBy == "all")
                        {
                            query += " AND (hospital_name LIKE @Search OR license_number LIKE @Search OR contact_email LIKE @Search OR city LIKE @Search OR province LIKE @Search OR postal_code LIKE @Search)";
                        }
                        else
                        {
                            query += $" AND {searchBy} LIKE @Search";
                        }
                        cmd.Parameters.AddWithValue("@Search", "%" + searchText + "%");
                    }

                    query += " ORDER BY hospital_name";
                    cmd.CommandText = query;

                    Debug.WriteLine($"[{DateTime.Now}] LoadHospitals - SQL Query: {query}");
                    if (cmd.Parameters.Contains("@Search"))
                    {
                        Debug.WriteLine($"[{DateTime.Now}] LoadHospitals - Search Parameter: {cmd.Parameters["@Search"].Value}");
                    }

                    conn.Open();
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvHospitals.DataSource = dt;
                    gvHospitals.DataBind();
                    gvHospitals.Visible = true;

                    if (dt.Rows.Count == 0)
                    {
                        if (string.IsNullOrEmpty(searchText))
                        {
                            ShowMessage("No hospitals available in the system.", "info");
                        }
                        else
                        {
                            ShowMessage($"No hospitals found for '{searchText}' in {searchBy}.", "warning");
                        }
                    }
                    else
                    {
                        ShowMessage($"{dt.Rows.Count} hospital(s) found.", "success");
                    }

                    txtSearch.Text = "";
                }
            }
            catch (MySqlException ex)
            {
                ShowMessage("Error searching hospitals: " + ex.Message, "danger");
                Debug.WriteLine($"[{DateTime.Now}] LoadHospitals - MySQL Error: {ex.Message}");
                gvHospitals.DataSource = null;
                gvHospitals.DataBind();
            }
            catch (Exception ex)
            {
                ShowMessage("Error searching hospitals: " + ex.Message, "danger");
                Debug.WriteLine($"[{DateTime.Now}] LoadHospitals - Error: {ex.Message}");
                gvHospitals.DataSource = null;
                gvHospitals.DataBind();
            }
        }

        protected void btnSaveHospital_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                ShowMessage("Please fix validation errors.", "danger");
                LoadHospitals();
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string hospitalId = hdnHospitalId.Value;
                    bool isNew = string.IsNullOrEmpty(hospitalId);

                    if (isNew)
                    {
                        string checkEmailQuery = "SELECT COUNT(*) FROM hospitals WHERE contact_email = @ContactEmail";
                        using (MySqlCommand checkCmd = new MySqlCommand(checkEmailQuery, conn))
                        {
                            checkCmd.Parameters.AddWithValue("@ContactEmail", txtEmail.Text.Trim());
                            long count = (long)checkCmd.ExecuteScalar();
                            if (count > 0)
                            {
                                ShowMessage("Email already exists. Please use a different email.", "danger");
                                LoadHospitals();
                                return;
                            }
                        }

                        string insertQuery = @"INSERT INTO hospitals 
                            (hospital_name, address_line1, address_line2, city, province, postal_code, country, 
                             license_number, contact_email, contact_password, is_verified) 
                            VALUES 
                            (@HospitalName, @Address1, @Address2, @City, @Province, @PostalCode, @Country, 
                             @LicenseNumber, @ContactEmail, @ContactPassword, @IsVerified)";

                        using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn))
                        {
                            AddHospitalParameters(cmd, isNew);
                            cmd.ExecuteNonQuery();

                            cmd.CommandText = "SELECT LAST_INSERT_ID()";
                            int newHospitalId = Convert.ToInt32(cmd.ExecuteScalar());

                            InsertNotification(conn, null, Convert.ToInt32(Session["AdminId"]), newHospitalId,
                                "New Hospital Added",
                                $"Hospital {txtHospitalName.Text.Trim()} was added to the system.");
                        }

                        ShowMessage("Hospital added successfully!", "success");
                    }
                    else
                    {
                        string updateQuery = @"UPDATE hospitals SET 
                            hospital_name = @HospitalName,
                            address_line1 = @Address1,
                            address_line2 = @Address2,
                            city = @City,
                            province = @Province,
                            postal_code = @PostalCode,
                            country = @Country,
                            license_number = @LicenseNumber,
                            contact_email = @ContactEmail,
                            contact_password = @ContactPassword,
                            is_verified = @IsVerified
                            WHERE hospital_id = @HospitalId";

                        using (MySqlCommand cmd = new MySqlCommand(updateQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@HospitalId", hospitalId);
                            AddHospitalParameters(cmd, isNew);
                            cmd.ExecuteNonQuery();

                            InsertNotification(conn, null, Convert.ToInt32(Session["AdminId"]), Convert.ToInt32(hospitalId),
                                "Hospital Updated",
                                $"Hospital {txtHospitalName.Text.Trim()} was updated.");
                        }

                        ShowMessage("Hospital updated successfully!", "success");
                    }

                    ClearForm();
                    LoadHospitals();
                }
            }
            catch (MySqlException ex)
            {
                ShowMessage("MySQL Error saving hospital: " + ex.Message, "danger");
                Debug.WriteLine($"[{DateTime.Now}] btnSaveHospital_Click - MySQL Error: {ex.Message}");
                LoadHospitals();
            }
            catch (Exception ex)
            {
                ShowMessage("Error saving hospital: " + ex.Message, "danger");
                Debug.WriteLine($"[{DateTime.Now}] btnSaveHospital_Click - Error: {ex.Message}");
                LoadHospitals();
            }
        }

        private void InsertNotification(MySqlConnection conn, int? donorId, int? adminId, int? hospitalId, string title, string message)
        {
            try
            {
                string query = @"INSERT INTO notifications 
                                (donor_id, admin_id, hospital_id, title, message, is_read, created_at)
                                VALUES (@DonorId, @AdminId, @HospitalId, @Title, @Message, 0, NOW())";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@DonorId", donorId.HasValue ? (object)donorId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@AdminId", adminId.HasValue ? (object)adminId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@HospitalId", hospitalId.HasValue ? (object)hospitalId.Value : DBNull.Value);
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

        private void AddHospitalParameters(MySqlCommand cmd, bool isNew)
        {
            cmd.Parameters.AddWithValue("@HospitalName", txtHospitalName.Text.Trim());
            cmd.Parameters.AddWithValue("@Address1", string.IsNullOrEmpty(txtAddressLine1.Text) ? DBNull.Value : (object)txtAddressLine1.Text.Trim());
            cmd.Parameters.AddWithValue("@Address2", string.IsNullOrEmpty(txtAddressLine2.Text) ? DBNull.Value : (object)txtAddressLine2.Text.Trim());
            cmd.Parameters.AddWithValue("@City", txtCity.Text.Trim());
            cmd.Parameters.AddWithValue("@Province", txtProvince.Text.Trim());
            cmd.Parameters.AddWithValue("@PostalCode", string.IsNullOrEmpty(txtPostalCode.Text) ? DBNull.Value : (object)txtPostalCode.Text.Trim());
            cmd.Parameters.AddWithValue("@Country", txtCountry.Text.Trim());
            cmd.Parameters.AddWithValue("@LicenseNumber", txtLicenseNumber.Text.Trim());
            cmd.Parameters.AddWithValue("@ContactEmail", txtEmail.Text.Trim());
            cmd.Parameters.AddWithValue("@ContactPassword", txtHospitalPassword.Text.Trim());
            cmd.Parameters.AddWithValue("@IsVerified", cbIsVerified.Checked);
        }

        protected void btnClearForm_Click(object sender, EventArgs e)
        {
            ClearForm();
            LoadHospitals();
        }

        private void ClearForm()
        {
            hdnHospitalId.Value = "";
            txtHospitalName.Text = "";
            txtLicenseNumber.Text = "";
            txtEmail.Text = "";
            txtHospitalPassword.Text = "";
            txtAddressLine1.Text = "";
            txtAddressLine2.Text = "";
            txtCity.Text = "";
            txtProvince.Text = "";
            txtPostalCode.Text = "";
            txtCountry.Text = "South Africa";
            cbIsVerified.Checked = true;
            passwordSection.Visible = true;
            rfvPassword.Enabled = true;
        }

        protected void gvHospitals_RowEditing(object sender, GridViewEditEventArgs e)
        {
            string hospitalId = gvHospitals.DataKeys[e.NewEditIndex].Value.ToString();
            LoadHospitalData(hospitalId);
            gvHospitals.EditIndex = -1;
            LoadHospitals();
        }

        private void LoadHospitalData(string hospitalId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    string query = @"SELECT hospital_id, hospital_name, address_line1, address_line2, city, province, 
                                    postal_code, country, license_number, contact_email, contact_password, is_verified 
                                    FROM hospitals WHERE hospital_id = @HospitalId";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@HospitalId", hospitalId);
                        conn.Open();

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hdnHospitalId.Value = reader["hospital_id"].ToString();
                                txtHospitalName.Text = reader["hospital_name"].ToString();
                                txtAddressLine1.Text = reader["address_line1"].ToString();
                                txtAddressLine2.Text = reader["address_line2"].ToString();
                                txtCity.Text = reader["city"].ToString();
                                txtProvince.Text = reader["province"].ToString();
                                txtPostalCode.Text = reader["postal_code"].ToString();
                                txtCountry.Text = reader["country"].ToString();
                                txtLicenseNumber.Text = reader["license_number"].ToString();
                                txtEmail.Text = reader["contact_email"].ToString();
                                txtHospitalPassword.Text = reader["contact_password"].ToString();
                                cbIsVerified.Checked = Convert.ToBoolean(reader["is_verified"]);
                                passwordSection.Visible = true;
                                rfvPassword.Enabled = false; // Password optional for updates
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                ShowMessage("Error loading hospital data: " + ex.Message, "danger");
                Debug.WriteLine($"[{DateTime.Now}] LoadHospitalData - MySQL Error: {ex.Message}");
            }
        }

        protected void gvHospitals_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            string hospitalId = gvHospitals.DataKeys[e.RowIndex].Value.ToString();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string hospitalNameQuery = "SELECT hospital_name FROM hospitals WHERE hospital_id = @HospitalId";
                    string hospitalName = "";
                    using (MySqlCommand nameCmd = new MySqlCommand(hospitalNameQuery, conn))
                    {
                        nameCmd.Parameters.AddWithValue("@HospitalId", hospitalId);
                        using (MySqlDataReader reader = nameCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hospitalName = reader["hospital_name"].ToString();
                            }
                        }
                    }

                    string deleteQuery = "DELETE FROM hospitals WHERE hospital_id = @HospitalId";
                    using (MySqlCommand cmd = new MySqlCommand(deleteQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@HospitalId", hospitalId);
                        cmd.ExecuteNonQuery();
                    }

                    InsertNotification(conn, null, Convert.ToInt32(Session["AdminId"]), null,
                        "Hospital Deleted",
                        $"Hospital {hospitalName} was deleted from the system.");

                    ShowMessage("Hospital deleted successfully!", "success");
                    ClearForm();
                    LoadHospitals();
                }
            }
            catch (MySqlException ex)
            {
                ShowMessage("MySQL Error deleting hospital: " + ex.Message, "danger");
                Debug.WriteLine($"[{DateTime.Now}] gvHospitals_RowDeleting - MySQL Error: {ex.Message}");
                LoadHospitals();
            }
            catch (Exception ex)
            {
                ShowMessage("Error deleting hospital: " + ex.Message, "danger");
                Debug.WriteLine($"[{DateTime.Now}] gvHospitals_RowDeleting - Error: {ex.Message}");
                LoadHospitals();
            }
        }

        protected void chkVerified_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chkVerified = (CheckBox)sender;
            GridViewRow row = (GridViewRow)chkVerified.NamingContainer;
            string hospitalId = gvHospitals.DataKeys[row.RowIndex].Value.ToString();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    string query = "UPDATE hospitals SET is_verified = @IsVerified WHERE hospital_id = @HospitalId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IsVerified", chkVerified.Checked);
                        cmd.Parameters.AddWithValue("@HospitalId", hospitalId);
                        conn.Open();
                        cmd.ExecuteNonQuery();

                        string hospitalNameQuery = "SELECT hospital_name FROM hospitals WHERE hospital_id = @HospitalId";
                        string hospitalName = "";
                        using (MySqlCommand nameCmd = new MySqlCommand(hospitalNameQuery, conn))
                        {
                            nameCmd.Parameters.AddWithValue("@HospitalId", hospitalId);
                            hospitalName = nameCmd.ExecuteScalar()?.ToString() ?? "";
                        }

                        InsertNotification(conn, null, Convert.ToInt32(Session["AdminId"]), Convert.ToInt32(hospitalId),
                            "Hospital Verification Updated",
                            $"Verification status for hospital {hospitalName} set to {(chkVerified.Checked ? "verified" : "unverified")}.");

                        ShowMessage("Hospital verification status updated successfully!", "success");
                    }
                }
                LoadHospitals();
            }
            catch (MySqlException ex)
            {
                ShowMessage("Error updating hospital verification status: " + ex.Message, "danger");
                Debug.WriteLine($"[{DateTime.Now}] chkVerified_CheckedChanged - MySQL Error: {ex.Message}");
                LoadHospitals();
            }
            catch (Exception ex)
            {
                ShowMessage("Error updating hospital verification status: " + ex.Message, "danger");
                Debug.WriteLine($"[{DateTime.Now}] chkVerified_CheckedChanged - Error: {ex.Message}");
                LoadHospitals();
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Session["SearchText"] = txtSearch.Text.Trim();
            Session["SearchBy"] = ddlSearchBy.SelectedValue;
            Debug.WriteLine($"[{DateTime.Now}] btnSearch_Click - SearchText: {Session["SearchText"]}, SearchBy: {Session["SearchBy"]}");
            LoadHospitals();
            Response.Redirect("ManageHospitals.aspx");
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (Session["SearchText"] != null && Session["SearchBy"] != null)
            {
                string searchText = Session["SearchText"].ToString();
                string searchBy = Session["SearchBy"].ToString();
                txtSearch.Text = searchText;
                try
                {
                    ddlSearchBy.SelectedValue = searchBy;
                    Debug.WriteLine($"[{DateTime.Now}] Page_PreRender - Restored SearchText: {searchText}, SearchBy: {searchBy}");
                }
                catch (ArgumentException ex)
                {
                    Debug.WriteLine($"[{DateTime.Now}] Page_PreRender - Invalid SearchBy value: {searchBy}, Error: {ex.Message}");
                }
                Session["SearchText"] = null;
                Session["SearchBy"] = null;
                LoadHospitals();
            }
        }

        protected void gvHospitals_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvHospitals.PageIndex = e.NewPageIndex;
            LoadHospitals();
        }

        protected void gvHospitals_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                Button btnDelete = (Button)e.Row.FindControl("btnDelete");
                if (btnDelete != null)
                {
                    btnDelete.OnClientClick = "return confirm('Are you sure you want to delete this hospital?');";
                    controlsToRegister.Add(btnDelete.UniqueID);
                }

                Button btnEdit = (Button)e.Row.FindControl("btnEdit");
                if (btnEdit != null)
                {
                    controlsToRegister.Add(btnEdit.UniqueID);
                }

                CheckBox chkVerified = (CheckBox)e.Row.FindControl("chkVerified");
                if (chkVerified != null)
                {
                    controlsToRegister.Add(chkVerified.UniqueID);
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

        private void ShowMessage(string message, string type)
        {
            pnlMessage.Visible = true;
            lblMessage.Text = message;
            pnlMessage.CssClass = "alert alert-" + type;
        }
    }
}