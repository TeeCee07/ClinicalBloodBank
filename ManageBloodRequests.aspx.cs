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
    public partial class ManageBloodRequests : System.Web.UI.Page
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
                    LoadBloodRequests();
                    LoadHospitalDropdown();
                    LoadBloodTypeDropdown();
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

        private void LoadBloodRequests()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT r.request_id, r.blood_type, r.quantity_ml, r.urgency, r.reason, r.status, r.requested_at,
                                    CASE 
                                        WHEN r.requester_role = 'donor' THEN CONCAT(d.first_name, ' ', d.last_name)
                                        WHEN r.requester_role = 'hospital' THEN h.hospital_name
                                    END as requester_name
                                    FROM blood_requests r
                                    LEFT JOIN donors d ON r.requester_id = d.donor_id AND r.requester_role = 'donor'
                                    LEFT JOIN hospitals h ON r.requester_id = h.hospital_id AND r.requester_role = 'hospital'
                                    WHERE 1=1";

                    // Apply filters
                    string bloodType = ddlFilterBloodType.SelectedValue;
                    string status = ddlFilterStatus.SelectedValue;
                    string urgency = ddlFilterUrgency.SelectedValue;

                    if (!string.IsNullOrEmpty(bloodType))
                        query += " AND r.blood_type = @BloodType";
                    if (!string.IsNullOrEmpty(status))
                        query += " AND r.status = @Status";
                    if (!string.IsNullOrEmpty(urgency))
                        query += " AND r.urgency = @Urgency";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        if (!string.IsNullOrEmpty(bloodType))
                            cmd.Parameters.AddWithValue("@BloodType", bloodType);
                        if (!string.IsNullOrEmpty(status))
                            cmd.Parameters.AddWithValue("@Status", status);
                        if (!string.IsNullOrEmpty(urgency))
                            cmd.Parameters.AddWithValue("@Urgency", urgency);

                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        gvBloodRequests.DataSource = dt;
                        gvBloodRequests.DataBind();

                        if (dt.Rows.Count == 0)
                            ShowMessage("No blood requests found.", "info");
                        else
                            ShowMessage($"{dt.Rows.Count} blood request(s) found.", "success");
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadBloodRequests - MySQL Error: {ex.Message}");
                ShowMessage("Error loading blood requests: " + ex.Message, "danger");
                gvBloodRequests.DataSource = null;
                gvBloodRequests.DataBind();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadBloodRequests - Error: {ex.Message}");
                ShowMessage("Error loading blood requests: " + ex.Message, "danger");
                gvBloodRequests.DataSource = null;
                gvBloodRequests.DataBind();
            }
        }

        private void LoadHospitalDropdown()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT hospital_id, hospital_name 
                                    FROM hospitals 
                                    WHERE is_verified = 1";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        ddlFulfillHospital.DataSource = dt;
                        ddlFulfillHospital.DataTextField = "hospital_name";
                        ddlFulfillHospital.DataValueField = "hospital_id";
                        ddlFulfillHospital.DataBind();
                        ddlFulfillHospital.Items.Insert(0, new ListItem("Select Hospital", ""));
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadHospitalDropdown - MySQL Error: {ex.Message}");
                ShowMessage("Error loading hospital dropdown: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadHospitalDropdown - Error: {ex.Message}");
                ShowMessage("Error loading hospital dropdown: " + ex.Message, "danger");
            }
        }

        private void LoadBloodTypeDropdown()
        {
            try
            {
                ddlFilterBloodType.Items.Clear();
                ddlFilterBloodType.Items.Add(new ListItem("All Blood Types", ""));
                ddlFilterBloodType.Items.Add(new ListItem("A+", "A+"));
                ddlFilterBloodType.Items.Add(new ListItem("A-", "A-"));
                ddlFilterBloodType.Items.Add(new ListItem("B+", "B+"));
                ddlFilterBloodType.Items.Add(new ListItem("B-", "B-"));
                ddlFilterBloodType.Items.Add(new ListItem("AB+", "AB+"));
                ddlFilterBloodType.Items.Add(new ListItem("AB-", "AB-"));
                ddlFilterBloodType.Items.Add(new ListItem("O+", "O+"));
                ddlFilterBloodType.Items.Add(new ListItem("O-", "O-"));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadBloodTypeDropdown - Error: {ex.Message}");
                ShowMessage("Error loading blood type dropdown: " + ex.Message, "danger");
            }
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            LoadBloodRequests();
        }

        protected void btnClearFilter_Click(object sender, EventArgs e)
        {
            ddlFilterBloodType.SelectedIndex = 0;
            ddlFilterStatus.SelectedIndex = 0;
            ddlFilterUrgency.SelectedIndex = 0;
            LoadBloodRequests();
        }

        protected void gvBloodRequests_RowEditing(object sender, GridViewEditEventArgs e)
        {
            try
            {
                string requestId = gvBloodRequests.DataKeys[e.NewEditIndex].Value.ToString();
                LoadRequestData(requestId);
                gvBloodRequests.EditIndex = -1;
                LoadBloodRequests();
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] gvBloodRequests_RowEditing - MySQL Error: {ex.Message}");
                ShowMessage("Error loading request data: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] gvBloodRequests_RowEditing - Error: {ex.Message}");
                ShowMessage("Error loading request data: " + ex.Message, "danger");
            }
        }

        protected void gvBloodRequests_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvBloodRequests.PageIndex = e.NewPageIndex;
            LoadBloodRequests();
        }

        protected void gvBloodRequests_RowDataBound(object sender, GridViewRowEventArgs e)
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

        private void LoadRequestData(string requestId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT r.request_id, r.blood_type, r.quantity_ml, r.urgency, r.reason, r.status, 
                                    r.fulfilled_by_hospital, r.patient_details as notes,
                                    CASE 
                                        WHEN r.requester_role = 'donor' THEN CONCAT(d.first_name, ' ', d.last_name)
                                        WHEN r.requester_role = 'hospital' THEN h.hospital_name
                                    END as requester_name
                                    FROM blood_requests r
                                    LEFT JOIN donors d ON r.requester_id = d.donor_id AND r.requester_role = 'donor'
                                    LEFT JOIN hospitals h ON r.requester_id = h.hospital_id AND r.requester_role = 'hospital'
                                    WHERE r.request_id = @requestId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@requestId", requestId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hdnRequestId.Value = reader["request_id"].ToString();
                                txtRequester.Text = reader["requester_name"].ToString();
                                txtRequestBloodType.Text = reader["blood_type"].ToString();
                                txtRequestQuantity.Text = reader["quantity_ml"].ToString();
                                txtUrgency.Text = reader["urgency"].ToString();
                                txtReason.Text = reader["reason"].ToString();
                                ddlRequestStatus.SelectedValue = reader["status"].ToString();
                                txtRequestNotes.Text = reader["notes"].ToString();
                                if (reader["fulfilled_by_hospital"] != DBNull.Value)
                                {
                                    ddlFulfillHospital.SelectedValue = reader["fulfilled_by_hospital"].ToString();
                                }
                                else
                                {
                                    ddlFulfillHospital.SelectedIndex = 0;
                                }
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadRequestData - MySQL Error: {ex.Message}");
                ShowMessage("Error loading request data: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadRequestData - Error: {ex.Message}");
                ShowMessage("Error loading request data: " + ex.Message, "danger");
            }
        }

        protected void btnProcessRequest_Click(object sender, EventArgs e)
        {
            if (!ValidateRequestForm()) return;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"UPDATE blood_requests SET status = @status, fulfilled_by_hospital = @hospitalId, 
                                  patient_details = @notes, fulfilled_at = CASE WHEN @status IN ('approved', 'fulfilled') THEN CURRENT_TIMESTAMP ELSE NULL END
                                  WHERE request_id = @requestId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@status", ddlRequestStatus.SelectedValue);
                        cmd.Parameters.AddWithValue("@hospitalId", string.IsNullOrEmpty(ddlFulfillHospital.SelectedValue) ? (object)DBNull.Value : ddlFulfillHospital.SelectedValue);
                        cmd.Parameters.AddWithValue("@notes", txtRequestNotes.Text);
                        cmd.Parameters.AddWithValue("@requestId", hdnRequestId.Value);
                        cmd.ExecuteNonQuery();
                    }

                    if (ddlRequestStatus.SelectedValue == "approved")
                    {
                        if (!UpdateInventoryForRequest())
                        {
                            ShowMessage("Insufficient inventory to fulfill this request.", "danger");
                            return;
                        }
                    }

                    AddNotification(Convert.ToInt32(Session["AdminId"]), "Request Processed", $"Blood request {hdnRequestId.Value} has been {ddlRequestStatus.SelectedValue}");
                    ShowMessage("Request processed successfully.", "success");
                    LoadBloodRequests();
                    ClearForm();
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnProcessRequest_Click - MySQL Error: {ex.Message}");
                ShowMessage("Error processing request: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnProcessRequest_Click - Error: {ex.Message}");
                ShowMessage("Error processing request: " + ex.Message, "danger");
            }
        }

        protected void btnClearForm_Click(object sender, EventArgs e)
        {
            ClearForm();
            LoadBloodRequests();
        }

        private void ClearForm()
        {
            hdnRequestId.Value = "";
            txtRequester.Text = "";
            txtRequestBloodType.Text = "";
            txtRequestQuantity.Text = "";
            txtUrgency.Text = "";
            txtReason.Text = "";
            ddlRequestStatus.SelectedIndex = 0;
            ddlFulfillHospital.SelectedIndex = 0;
            txtRequestNotes.Text = "";
        }

        private bool ValidateRequestForm()
        {
            if (string.IsNullOrEmpty(hdnRequestId.Value))
            {
                ShowMessage("No request selected.", "danger");
                return false;
            }
            if (string.IsNullOrEmpty(ddlRequestStatus.SelectedValue))
            {
                ShowMessage("Status is required.", "danger");
                return false;
            }
            if (string.IsNullOrEmpty(ddlFulfillHospital.SelectedValue))
            {
                ShowMessage("Fulfilling hospital is required.", "danger");
                return false;
            }
            return true;
        }

        private bool UpdateInventoryForRequest()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string getRequestQuery = "SELECT blood_type, quantity_ml FROM blood_requests WHERE request_id = @requestId";
                    string bloodType;
                    int quantityNeeded;
                    using (MySqlCommand cmd = new MySqlCommand(getRequestQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@requestId", hdnRequestId.Value);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                return false;
                            }
                            bloodType = reader["blood_type"].ToString();
                            quantityNeeded = Convert.ToInt32(reader["quantity_ml"]);
                        }
                    }

                    // Check available inventory
                    string checkInventoryQuery = @"SELECT SUM(quantity_ml) as total_quantity 
                                                FROM blood_inventory 
                                                WHERE blood_type = @bloodType AND status = 'available' AND expiration_date > CURRENT_TIMESTAMP";
                    using (MySqlCommand cmd = new MySqlCommand(checkInventoryQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@bloodType", bloodType);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read() && reader["total_quantity"] != DBNull.Value)
                            {
                                int totalAvailable = Convert.ToInt32(reader["total_quantity"]);
                                if (totalAvailable < quantityNeeded)
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }

                    // Update inventory
                    string findInventoryQuery = @"SELECT inventory_id, quantity_ml 
                                               FROM blood_inventory 
                                               WHERE blood_type = @bloodType AND status = 'available' AND expiration_date > CURRENT_TIMESTAMP
                                               ORDER BY expiration_date ASC";
                    using (MySqlCommand cmd = new MySqlCommand(findInventoryQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@bloodType", bloodType);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read() && quantityNeeded > 0)
                            {
                                int inventoryId = Convert.ToInt32(reader["inventory_id"]);
                                int availableQuantity = Convert.ToInt32(reader["quantity_ml"]);
                                int quantityToUse = Math.Min(availableQuantity, quantityNeeded);

                                string updateInventoryQuery = @"UPDATE blood_inventory 
                                                             SET quantity_ml = quantity_ml - @quantity,
                                                             status = CASE WHEN quantity_ml - @quantity <= 0 THEN 'used' ELSE 'available' END
                                                             WHERE inventory_id = @inventoryId";
                                using (MySqlCommand updateCmd = new MySqlCommand(updateInventoryQuery, conn))
                                {
                                    updateCmd.Parameters.AddWithValue("@quantity", quantityToUse);
                                    updateCmd.Parameters.AddWithValue("@inventoryId", inventoryId);
                                    updateCmd.ExecuteNonQuery();
                                }

                                quantityNeeded -= quantityToUse;
                            }
                        }
                    }
                    return true;
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] UpdateInventoryForRequest - MySQL Error: {ex.Message}");
                ShowMessage("Error updating inventory: " + ex.Message, "danger");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] UpdateInventoryForRequest - Error: {ex.Message}");
                ShowMessage("Error updating inventory: " + ex.Message, "danger");
                return false;
            }
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