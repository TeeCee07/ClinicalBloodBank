using System;
using System.Configuration;
using System.Data;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics; // For debugging

namespace ClinicalBloodBank
{
    public partial class ManageDonors : System.Web.UI.Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;
        private List<string> controlsToRegister = new List<string>(); // Store control IDs for event validation

        protected void Page_Load(object sender, EventArgs e)
        {
            // Check if admin is logged in
            if (Session["AdminId"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            try
            {
                // Register PreRender event handler
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
                    LoadDonors();
                    ClearForm();
                }
            }
            catch (MySqlException sqlEx)
            {
                ShowMessage("Database error: " + sqlEx.Message, "danger");
            }
            catch (Exception ex)
            {
                ShowMessage("Error: " + ex.Message, "danger");
            }
        }

        private void LoadUserInfo()
        {
            if (Session["AdminName"] != null)
            {
                string adminName = Session["AdminName"].ToString();
                litUserName.Text = adminName;

                // Get initials for avatar
                string[] nameParts = adminName.Split(' ');
                string initials = "";
                if (nameParts.Length > 0)
                {
                    initials += nameParts[0][0].ToString();
                    if (nameParts.Length > 1)
                    {
                        initials += nameParts[1][0].ToString();
                    }
                }
                litUserInitials.Text = initials.ToUpper();
            }
        }

        private void LoadDonors()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    string query = @"SELECT donor_id, first_name, last_name, email, phone, blood_type, 
                                    weight, date_of_birth, gender, is_active 
                                    FROM donors 
                                    WHERE 1=1";

                    string searchText = txtSearch.Text.Trim();
                    string searchBy = ddlSearchBy.SelectedValue;

                    // Debug: Log search parameters
                    Debug.WriteLine($"SearchBy: {searchBy}, SearchText: {searchText}");

                    MySqlCommand cmd = new MySqlCommand(query, conn);

                    if (!string.IsNullOrEmpty(searchText))
                    {
                        // Validate searchBy
                        string[] validSearchFields = { "all", "name", "email", "phone", "blood_type", "gender", "city", "province", "postal_code" };
                        if (!Array.Exists(validSearchFields, field => field == searchBy))
                        {
                            ShowMessage("Invalid search field selected.", "danger");
                            Debug.WriteLine("Invalid search field: " + searchBy);
                            return;
                        }

                        if (searchBy == "all")
                        {
                            query += " AND (first_name LIKE @Search OR last_name LIKE @Search OR email LIKE @Search OR phone LIKE @Search OR blood_type LIKE @Search OR gender LIKE @Search OR city LIKE @Search OR province LIKE @Search OR postal_code LIKE @Search)";
                        }
                        else if (searchBy == "name")
                        {
                            query += " AND (first_name LIKE @Search OR last_name LIKE @Search)";
                        }
                        else
                        {
                            query += $" AND {searchBy} LIKE @Search";
                        }
                        cmd.Parameters.AddWithValue("@Search", "%" + searchText + "%");
                    }

                    query += " ORDER BY first_name, last_name";
                    cmd.CommandText = query;

                    // Debug: Log the query
                    Debug.WriteLine("SQL Query: " + query);
                    if (cmd.Parameters.Contains("@Search"))
                    {
                        Debug.WriteLine("Search Parameter: " + cmd.Parameters["@Search"].Value);
                    }

                    conn.Open();
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvDonors.DataSource = dt;
                    gvDonors.DataBind();

                    // Ensure GridView is visible
                    gvDonors.Visible = true;

                    // Provide feedback
                    if (dt.Rows.Count == 0)
                    {
                        if (string.IsNullOrEmpty(searchText))
                        {
                            ShowMessage("No donors available in the system.", "info");
                        }
                        else
                        {
                            ShowMessage($"No donors found for '{searchText}' in {searchBy}.", "warning");
                        }
                    }
                    else
                    {
                        ShowMessage($"{dt.Rows.Count} donor(s) found.", "success");
                    }

                    // Clear search input after filtering
                    txtSearch.Text = "";
                }
            }
            catch (MySqlException ex)
            {
                ShowMessage("Error searching donors: " + ex.Message, "danger");
                Debug.WriteLine("MySQL Error in LoadDonors: " + ex.Message);
                gvDonors.DataSource = null;
                gvDonors.DataBind();
            }
            catch (Exception ex)
            {
                ShowMessage("Error searching donors: " + ex.Message, "danger");
                Debug.WriteLine("Error in LoadDonors: " + ex.Message);
                gvDonors.DataSource = null;
                gvDonors.DataBind();
            }
        }

        protected void btnSaveDonor_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                ShowMessage("Please fix validation errors.", "danger");
                LoadDonors();
                return;
            }

            try
            {
                // Validate date and weight inputs
                if (!DateTime.TryParse(txtDateOfBirth.Text, out DateTime dateOfBirth))
                {
                    ShowMessage("Please enter a valid date of birth (yyyy-MM-dd).", "danger");
                    LoadDonors();
                    return;
                }

                DateTime? lastDonationDate = null;
                if (!string.IsNullOrEmpty(txtLastDonationDate.Text))
                {
                    if (DateTime.TryParse(txtLastDonationDate.Text, out DateTime parsedLastDonationDate))
                    {
                        lastDonationDate = parsedLastDonationDate;
                    }
                    else
                    {
                        ShowMessage("Please enter a valid last donation date (yyyy-MM-dd).", "danger");
                        LoadDonors();
                        return;
                    }
                }

                if (string.IsNullOrWhiteSpace(txtWeight.Text) || !decimal.TryParse(txtWeight.Text, out decimal weight) || weight < 0 || weight > 999.99m)
                {
                    ShowMessage("Please enter a valid weight between 0.00 and 999.99 (e.g., 75.5).", "danger");
                    LoadDonors();
                    return;
                }

                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    string donorId = hdnDonorId.Value;
                    bool isNew = string.IsNullOrEmpty(donorId);

                    if (isNew)
                    {
                        // Check if email already exists
                        string checkEmailQuery = "SELECT COUNT(*) FROM donors WHERE email = @Email";
                        using (MySqlCommand checkCmd = new MySqlCommand(checkEmailQuery, conn))
                        {
                            checkCmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                            long count = (long)checkCmd.ExecuteScalar();
                            if (count > 0)
                            {
                                ShowMessage("Email already exists. Please use a different email.", "danger");
                                LoadDonors();
                                return;
                            }
                        }

                        // Insert new donor
                        string insertQuery = @"INSERT INTO donors 
                            (first_name, last_name, email, password, phone, date_of_birth, gender, 
                             blood_type, weight, health_conditions, last_donation_date, 
                             address_line1, address_line2, city, province, postal_code, country, 
                             is_active, is_eligible, total_donations) 
                            VALUES 
                            (@FirstName, @LastName, @Email, @Password, @Phone, @DateOfBirth, @Gender, 
                             @BloodType, @Weight, @HealthConditions, @LastDonationDate, 
                             @Address1, @Address2, @City, @Province, @PostalCode, @Country, 
                             @IsActive, @IsEligible, @TotalDonations)";

                        using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn))
                        {
                            AddDonorParameters(cmd, isNew, dateOfBirth, lastDonationDate, weight);
                            cmd.ExecuteNonQuery();

                            // Get the new donor_id
                            cmd.CommandText = "SELECT LAST_INSERT_ID()";
                            int newDonorId = Convert.ToInt32(cmd.ExecuteScalar());

                            // Insert notification for admin
                            InsertNotification(conn, null, Convert.ToInt32(Session["AdminId"]), null,
                                "New Donor Added",
                                $"Donor {txtFirstName.Text.Trim()} {txtLastName.Text.Trim()} was added to the system.");
                        }

                        ShowMessage("Donor added successfully!", "success");
                    }
                    else
                    {
                        // Update existing donor
                        string updateQuery = @"UPDATE donors SET 
                            first_name = @FirstName,
                            last_name = @LastName,
                            email = @Email,
                            phone = @Phone,
                            date_of_birth = @DateOfBirth,
                            gender = @Gender,
                            blood_type = @BloodType,
                            weight = @Weight,
                            health_conditions = @HealthConditions,
                            last_donation_date = @LastDonationDate,
                            address_line1 = @Address1,
                            address_line2 = @Address2,
                            city = @City,
                            province = @Province,
                            postal_code = @PostalCode,
                            country = @Country,
                            is_active = @IsActive
                            WHERE donor_id = @DonorId";

                        using (MySqlCommand cmd = new MySqlCommand(updateQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@DonorId", donorId);
                            AddDonorParameters(cmd, isNew, dateOfBirth, lastDonationDate, weight);
                            cmd.ExecuteNonQuery();
                            ShowMessage("Donor updated successfully!", "success");
                        }
                    }

                    ClearForm();
                    LoadDonors();
                }
            }
            catch (MySqlException ex)
            {
                ShowMessage("MySQL Error saving donor: " + ex.Message, "danger");
                Debug.WriteLine("MySQL Error in btnSaveDonor_Click: " + ex.Message);
                LoadDonors();
            }
            catch (Exception ex)
            {
                ShowMessage("Error saving donor: " + ex.Message, "danger");
                Debug.WriteLine("Error in btnSaveDonor_Click: " + ex.Message);
                LoadDonors();
            }
        }

        private void InsertNotification(MySqlConnection conn, int? donorId, int? adminId, int? hospitalId, string title, string message)
        {
            try
            {
                string query = @"INSERT INTO notifications 
                                (donor_id, admin_id, hospital_id, title, message, is_read, created_at)
                                VALUES (@DonorId, @AdminId, @HospitalId, @Title, @Message, 0, @CreatedAt)";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@DonorId", donorId.HasValue ? (object)donorId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@AdminId", adminId.HasValue ? (object)adminId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@HospitalId", hospitalId.HasValue ? (object)hospitalId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Title", title);
                    cmd.Parameters.AddWithValue("@Message", message);
                    cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine("MySQL Error in InsertNotification: " + ex.Message);
                // Suppress notification errors to avoid blocking main operation
            }
        }

        private void AddDonorParameters(MySqlCommand cmd, bool isNew, DateTime dateOfBirth, DateTime? lastDonationDate, decimal weight)
        {
            cmd.Parameters.AddWithValue("@FirstName", txtFirstName.Text.Trim());
            cmd.Parameters.AddWithValue("@LastName", txtLastName.Text.Trim());
            cmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
            cmd.Parameters.AddWithValue("@Phone", txtPhone.Text.Trim());
            cmd.Parameters.AddWithValue("@DateOfBirth", dateOfBirth);
            cmd.Parameters.AddWithValue("@Gender", ddlGender.SelectedValue);
            cmd.Parameters.AddWithValue("@BloodType", ddlBloodType.SelectedValue);
            cmd.Parameters.AddWithValue("@Weight", weight);
            cmd.Parameters.AddWithValue("@HealthConditions", string.IsNullOrEmpty(txtHealthConditions.Text) ? DBNull.Value : (object)txtHealthConditions.Text.Trim());
            cmd.Parameters.AddWithValue("@LastDonationDate", lastDonationDate.HasValue ? (object)lastDonationDate.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@Address1", txtAddressLine1.Text.Trim());
            cmd.Parameters.AddWithValue("@Address2", string.IsNullOrEmpty(txtAddressLine2.Text) ? DBNull.Value : (object)txtAddressLine2.Text.Trim());
            cmd.Parameters.AddWithValue("@City", txtCity.Text.Trim());
            cmd.Parameters.AddWithValue("@Province", txtState.Text.Trim());
            cmd.Parameters.AddWithValue("@PostalCode", txtPostalCode.Text.Trim());
            cmd.Parameters.AddWithValue("@Country", txtCountry.Text.Trim());
            cmd.Parameters.AddWithValue("@IsActive", cbIsActive.Checked);

            if (isNew)
            {
                // Store password as plain text (NOT RECOMMENDED for production)
                cmd.Parameters.AddWithValue("@Password", txtDonorPassword.Text.Trim());
                cmd.Parameters.AddWithValue("@IsEligible", true); // Default for new donors
                cmd.Parameters.AddWithValue("@TotalDonations", 0); // Default for new donors
            }
        }

        protected void btnClearForm_Click(object sender, EventArgs e)
        {
            ClearForm();
            LoadDonors();
        }

        private void ClearForm()
        {
            hdnDonorId.Value = "";
            txtFirstName.Text = "";
            txtLastName.Text = "";
            txtEmail.Text = "";
            txtPhone.Text = "";
            txtAddressLine1.Text = "";
            txtAddressLine2.Text = "";
            txtCity.Text = "";
            txtState.Text = "";
            txtPostalCode.Text = "";
            txtCountry.Text = "South Africa";
            txtDateOfBirth.Text = "";
            ddlGender.SelectedIndex = 0;
            ddlBloodType.SelectedIndex = 0;
            txtWeight.Text = "";
            txtHealthConditions.Text = "";
            txtLastDonationDate.Text = "";
            txtDonorPassword.Text = "";
            cbIsActive.Checked = true;

            // Show password section for new donors
            passwordSection.Visible = true;
            rfvPassword.Enabled = true;
        }

        protected void gvDonors_RowEditing(object sender, GridViewEditEventArgs e)
        {
            // Get the donor ID from the selected row
            string donorId = gvDonors.DataKeys[e.NewEditIndex].Value.ToString();

            // Load donor data into the form
            LoadDonorData(donorId);

            // Set the edit index to -1 to cancel edit mode
            gvDonors.EditIndex = -1;
            LoadDonors();
        }

        private void LoadDonorData(string donorId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    string query = @"SELECT donor_id, first_name, last_name, email, phone, date_of_birth, 
                                    gender, blood_type, weight, health_conditions, last_donation_date, 
                                    address_line1, address_line2, city, province, postal_code, country, is_active 
                                    FROM donors WHERE donor_id = @DonorId";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DonorId", donorId);
                        conn.Open();

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hdnDonorId.Value = reader["donor_id"].ToString();
                                txtFirstName.Text = reader["first_name"].ToString();
                                txtLastName.Text = reader["last_name"].ToString();
                                txtEmail.Text = reader["email"].ToString();
                                txtPhone.Text = reader["phone"].ToString();

                                if (reader["date_of_birth"] != DBNull.Value)
                                    txtDateOfBirth.Text = Convert.ToDateTime(reader["date_of_birth"]).ToString("yyyy-MM-dd");

                                ddlGender.SelectedValue = reader["gender"].ToString();
                                ddlBloodType.SelectedValue = reader["blood_type"].ToString();
                                txtWeight.Text = reader["weight"].ToString();
                                txtHealthConditions.Text = reader["health_conditions"].ToString();

                                if (reader["last_donation_date"] != DBNull.Value)
                                    txtLastDonationDate.Text = Convert.ToDateTime(reader["last_donation_date"]).ToString("yyyy-MM-dd");
                                else
                                    txtLastDonationDate.Text = "";

                                txtAddressLine1.Text = reader["address_line1"].ToString();
                                txtAddressLine2.Text = reader["address_line2"].ToString();
                                txtCity.Text = reader["city"].ToString();
                                txtState.Text = reader["province"].ToString();
                                txtPostalCode.Text = reader["postal_code"].ToString();
                                txtCountry.Text = reader["country"].ToString();
                                cbIsActive.Checked = Convert.ToBoolean(reader["is_active"]);

                                // Hide password section for existing donors
                                passwordSection.Visible = false;
                                rfvPassword.Enabled = false;
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                ShowMessage("Error loading donor data: " + ex.Message, "danger");
                Debug.WriteLine("MySQL Error in LoadDonorData: " + ex.Message);
            }
        }

        protected void gvDonors_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            string donorId = gvDonors.DataKeys[e.RowIndex].Value.ToString();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // Get donor name for notification
                    string donorNameQuery = "SELECT first_name, last_name FROM donors WHERE donor_id = @DonorId";
                    string donorName = "";
                    using (MySqlCommand nameCmd = new MySqlCommand(donorNameQuery, conn))
                    {
                        nameCmd.Parameters.AddWithValue("@DonorId", donorId);
                        using (MySqlDataReader reader = nameCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                donorName = $"{reader["first_name"]} {reader["last_name"]}";
                            }
                        }
                    }

                    // Delete donor
                    string deleteQuery = "DELETE FROM donors WHERE donor_id = @DonorId";
                    using (MySqlCommand cmd = new MySqlCommand(deleteQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@DonorId", donorId);
                        cmd.ExecuteNonQuery();
                    }

                    // Insert notification for admin
                    InsertNotification(conn, null, Convert.ToInt32(Session["AdminId"]), null,
                        "Donor Deleted",
                        $"Donor {donorName} was deleted from the system.");

                    ShowMessage("Donor deleted successfully!", "success");
                }
                ClearForm();
                LoadDonors();
            }
            catch (MySqlException ex)
            {
                ShowMessage("MySQL Error deleting donor: " + ex.Message, "danger");
                Debug.WriteLine("MySQL Error in gvDonors_RowDeleting: " + ex.Message);
                LoadDonors();
            }
            catch (Exception ex)
            {
                ShowMessage("Error deleting donor: " + ex.Message, "danger");
                Debug.WriteLine("Error in gvDonors_RowDeleting: " + ex.Message);
                LoadDonors();
            }
        }

        protected void chkActive_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chkActive = (CheckBox)sender;
            GridViewRow row = (GridViewRow)chkActive.NamingContainer;
            string donorId = gvDonors.DataKeys[row.RowIndex].Value.ToString();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    string query = "UPDATE donors SET is_active = @IsActive WHERE donor_id = @DonorId";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IsActive", chkActive.Checked);
                        cmd.Parameters.AddWithValue("@DonorId", donorId);
                        conn.Open();
                        cmd.ExecuteNonQuery();

                        ShowMessage("Donor status updated successfully!", "success");
                    }
                }
                LoadDonors();
            }
            catch (MySqlException ex)
            {
                ShowMessage("Error updating donor status: " + ex.Message, "danger");
                Debug.WriteLine("MySQL Error in chkActive_CheckedChanged: " + ex.Message);
                LoadDonors();
            }
            catch (Exception ex)
            {
                ShowMessage("Error updating donor status: " + ex.Message, "danger");
                Debug.WriteLine("Error in chkActive_CheckedChanged: " + ex.Message);
                LoadDonors();
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            // Store search parameters in session to persist across page reload
            Session["SearchText"] = txtSearch.Text.Trim();
            Session["SearchBy"] = ddlSearchBy.SelectedValue;

            // Debug: Log search parameters before reload
            Debug.WriteLine($"[{DateTime.Now}] btnSearch_Click - SearchText: {Session["SearchText"]}, SearchBy: {Session["SearchBy"]}");

            LoadDonors();

            // Force page reload to ensure GridView updates
            Response.Redirect("ManageDonors.aspx");
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            // Restore search parameters after page reload
            if (Session["SearchText"] != null && Session["SearchBy"] != null)
            {
                string searchText = Session["SearchText"].ToString();
                string searchBy = Session["SearchBy"].ToString();
                txtSearch.Text = searchText;
                try
                {
                    ddlSearchBy.SelectedValue = searchBy;
                    // Debug: Log restored parameters
                    Debug.WriteLine($"[{DateTime.Now}] Page_PreRender - Restored SearchText: {searchText}, SearchBy: {searchBy}");
                }
                catch (ArgumentException ex)
                {
                    // Handle case where SelectedValue is invalid
                    Debug.WriteLine($"[{DateTime.Now}] Page_PreRender - Invalid SearchBy value: {searchBy}, Error: {ex.Message}");
                }
                Session["SearchText"] = null;
                Session["SearchBy"] = null;
                LoadDonors(); // Re-run search with restored parameters
                return;
            }
        }

        protected void gvDonors_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvDonors.PageIndex = e.NewPageIndex;
            LoadDonors();
        }

        protected void gvDonors_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Find the delete button and add confirmation
                Button btnDelete = (Button)e.Row.FindControl("btnDelete");
                if (btnDelete != null)
                {
                    btnDelete.OnClientClick = "return confirm('Are you sure you want to delete this donor?');";
                    controlsToRegister.Add(btnDelete.UniqueID); // Store ID for Render phase
                }

                // Find the edit button
                Button btnEdit = (Button)e.Row.FindControl("btnEdit");
                if (btnEdit != null)
                {
                    controlsToRegister.Add(btnEdit.UniqueID); // Store ID for Render phase
                }

                // Find the checkbox
                CheckBox chkActive = (CheckBox)e.Row.FindControl("chkActive");
                if (chkActive != null)
                {
                    controlsToRegister.Add(chkActive.UniqueID); // Store ID for Render phase
                }
            }
        }
        protected void lnkLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("Login.aspx");
        }


        protected override void Render(HtmlTextWriter writer)
        {
            // Register controls for event validation during Render phase
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