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
    public partial class ManageRequests : System.Web.UI.Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;
        private List<string> controlsToRegister = new List<string>();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null || Session["UserType"] == null || Session["UserType"].ToString() != "hospital")
            {
                Debug.WriteLine($"[{DateTime.Now}] Page_Load - Invalid session. UserId: {Session["UserId"]}, UserType: {Session["UserType"]}");
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
                Debug.WriteLine($"[{DateTime.Now}] Page_Load - MySQL Error: {ex.Message}, ErrorCode: {ex.Number}");
                ShowMessage("Database error: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] Page_Load - Error: {ex.Message}, StackTrace: {ex.StackTrace}");
                ShowMessage("Error: " + ex.Message, "danger");
            }
        }

        private void LoadUserInfo()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT hospital_name FROM hospitals WHERE hospital_id = @hospitalId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string hospitalName = reader["hospital_name"]?.ToString() ?? "Hospital";
                                litUserName.Text = hospitalName;
                                litUserInitials.Text = GetInitials(hospitalName);
                                Debug.WriteLine($"[{DateTime.Now}] LoadUserInfo - HospitalName: {hospitalName}");
                            }
                            else
                            {
                                litUserName.Text = "Hospital";
                                litUserInitials.Text = "HO";
                                Debug.WriteLine($"[{DateTime.Now}] LoadUserInfo - Hospital not found for UserId: {Session["UserId"]}");
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadUserInfo - MySQL Error: {ex.Message}, ErrorCode: {ex.Number}");
                ShowMessage("Error loading hospital info: " + ex.Message, "danger");
                litUserName.Text = "Hospital";
                litUserInitials.Text = "HO";
            }
        }

        private string GetInitials(string name)
        {
            if (string.IsNullOrEmpty(name)) return "HO";
            string[] parts = name.Split(' ');
            string initials = "";
            foreach (string part in parts)
            {
                if (!string.IsNullOrEmpty(part))
                    initials += part[0].ToString().ToUpper();
            }
            return initials.Length > 2 ? initials.Substring(0, 2) : initials;
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
                                        ELSE 'Unknown'
                                    END as requester_name
                                    FROM blood_requests r
                                    LEFT JOIN donors d ON r.requester_id = d.donor_id AND r.requester_role = 'donor'
                                    LEFT JOIN hospitals h ON r.requester_id = h.hospital_id AND r.requester_role = 'hospital'
                                    WHERE r.requester_id = @hospitalId AND r.requester_role = 'hospital'";

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
                        cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
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

                        Debug.WriteLine($"[{DateTime.Now}] LoadBloodRequests - Retrieved {dt.Rows.Count} requests for hospital ID {Session["UserId"]}");
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadBloodRequests - MySQL Error: {ex.Message}, ErrorCode: {ex.Number}");
                ShowMessage("Error loading blood requests: " + ex.Message, "danger");
                gvBloodRequests.DataSource = null;
                gvBloodRequests.DataBind();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadBloodRequests - Error: {ex.Message}, StackTrace: {ex.StackTrace}");
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
                                    WHERE is_verified = 1 AND hospital_id = @hospitalId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        ddlFulfillHospital.DataSource = dt;
                        ddlFulfillHospital.DataTextField = "hospital_name";
                        ddlFulfillHospital.DataValueField = "hospital_id";
                        ddlFulfillHospital.DataBind();
                        ddlFulfillHospital.Items.Insert(0, new ListItem("Select Hospital", ""));
                        Debug.WriteLine($"[{DateTime.Now}] LoadHospitalDropdown - Loaded {dt.Rows.Count} hospitals for hospital ID {Session["UserId"]}");
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadHospitalDropdown - MySQL Error: {ex.Message}, ErrorCode: {ex.Number}");
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
                Debug.WriteLine($"[{DateTime.Now}] LoadBloodTypeDropdown - Loaded blood types");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadBloodTypeDropdown - Error: {ex.Message}");
                ShowMessage("Error loading blood type dropdown: " + ex.Message, "danger");
            }
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            try
            {
                LoadBloodRequests();
                Debug.WriteLine($"[{DateTime.Now}] btnFilter_Click - Applied filters: BloodType={ddlFilterBloodType.SelectedValue}, Status={ddlFilterStatus.SelectedValue}, Urgency={ddlFilterUrgency.SelectedValue}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnFilter_Click - Error: {ex.Message}");
                ShowMessage("Error applying filter: " + ex.Message, "danger");
            }
        }

        protected void btnClearFilter_Click(object sender, EventArgs e)
        {
            try
            {
                ddlFilterBloodType.SelectedIndex = 0;
                ddlFilterStatus.SelectedIndex = 0;
                ddlFilterUrgency.SelectedIndex = 0;
                LoadBloodRequests();
                Debug.WriteLine($"[{DateTime.Now}] btnClearFilter_Click - Cleared filters");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnClearFilter_Click - Error: {ex.Message}");
                ShowMessage("Error clearing filter: " + ex.Message, "danger");
            }
        }

        protected void gvBloodRequests_RowEditing(object sender, GridViewEditEventArgs e)
        {
            try
            {
                string requestId = gvBloodRequests.DataKeys[e.NewEditIndex].Value.ToString();
                LoadRequestData(requestId);
                gvBloodRequests.EditIndex = -1;
                LoadBloodRequests();
                Debug.WriteLine($"[{DateTime.Now}] gvBloodRequests_RowEditing - Loaded request ID: {requestId}");
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] gvBloodRequests_RowEditing - MySQL Error: {ex.Message}, ErrorCode: {ex.Number}");
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
            try
            {
                gvBloodRequests.PageIndex = e.NewPageIndex;
                LoadBloodRequests();
                Debug.WriteLine($"[{DateTime.Now}] gvBloodRequests_PageIndexChanging - Changed to page: {e.NewPageIndex}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] gvBloodRequests_PageIndexChanging - Error: {ex.Message}");
                ShowMessage("Error changing page: " + ex.Message, "danger");
            }
        }

        protected void gvBloodRequests_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                try
                {
                    Button btnEdit = (Button)e.Row.FindControl("btnEdit");
                    if (btnEdit != null)
                    {
                        controlsToRegister.Add(btnEdit.UniqueID);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[{DateTime.Now}] gvBloodRequests_RowDataBound - Error: {ex.Message}");
                    ShowMessage("Error binding row: " + ex.Message, "danger");
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
                                    r.fulfilled_by_hospital, r.patient_details as notes, r.requested_at,
                                    CASE 
                                        WHEN r.requester_role = 'donor' THEN CONCAT(d.first_name, ' ', d.last_name)
                                        WHEN r.requester_role = 'hospital' THEN h.hospital_name
                                        ELSE 'Unknown'
                                    END as requester_name
                                    FROM blood_requests r
                                    LEFT JOIN donors d ON r.requester_id = d.donor_id AND r.requester_role = 'donor'
                                    LEFT JOIN hospitals h ON r.requester_id = h.hospital_id AND r.requester_role = 'hospital'
                                    WHERE r.request_id = @requestId AND r.requester_id = @hospitalId AND r.requester_role = 'hospital'";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@requestId", requestId);
                        cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
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
                                txtRequestNotes.Text = reader["notes"] != DBNull.Value ? reader["notes"].ToString() : "";
                                if (reader["fulfilled_by_hospital"] != DBNull.Value && reader["fulfilled_by_hospital"].ToString() == Session["UserId"].ToString())
                                {
                                    ddlFulfillHospital.SelectedValue = reader["fulfilled_by_hospital"].ToString();
                                }
                                else
                                {
                                    ddlFulfillHospital.SelectedIndex = 0;
                                }
                                Debug.WriteLine($"[{DateTime.Now}] LoadRequestData - Loaded request ID: {requestId}");
                            }
                            else
                            {
                                ShowMessage("Request not found or you lack permission to view it.", "danger");
                                Debug.WriteLine($"[{DateTime.Now}] LoadRequestData - Request ID {requestId} not found for hospital ID {Session["UserId"]}");
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadRequestData - MySQL Error: {ex.Message}, ErrorCode: {ex.Number}");
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
                    string query = @"UPDATE blood_requests 
                                    SET status = @status, 
                                        fulfilled_by_hospital = @hospitalId, 
                                        patient_details = @notes, 
                                        fulfilled_at = CASE WHEN @status IN ('approved', 'fulfilled') THEN CURRENT_TIMESTAMP ELSE NULL END
                                    WHERE request_id = @requestId AND requester_id = @hospitalId AND requester_role = 'hospital'";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@status", ddlRequestStatus.SelectedValue);
                        cmd.Parameters.AddWithValue("@hospitalId", string.IsNullOrEmpty(ddlFulfillHospital.SelectedValue) ? (object)DBNull.Value : ddlFulfillHospital.SelectedValue);
                        cmd.Parameters.AddWithValue("@notes", string.IsNullOrEmpty(txtRequestNotes.Text) ? (object)DBNull.Value : txtRequestNotes.Text);
                        cmd.Parameters.AddWithValue("@requestId", hdnRequestId.Value);
                        cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            ShowMessage("Request not found or you lack permission to update it.", "danger");
                            Debug.WriteLine($"[{DateTime.Now}] btnProcessRequest_Click - Request ID {hdnRequestId.Value} not found for hospital ID {Session["UserId"]}");
                            return;
                        }
                    }

                    if (ddlRequestStatus.SelectedValue == "approved" || ddlRequestStatus.SelectedValue == "fulfilled")
                    {
                        if (!UpdateInventoryForRequest())
                        {
                            string revertQuery = "UPDATE blood_requests SET status = 'pending', fulfilled_by_hospital = NULL, fulfilled_at = NULL WHERE request_id = @requestId AND requester_id = @hospitalId";
                            using (MySqlCommand revertCmd = new MySqlCommand(revertQuery, conn))
                            {
                                revertCmd.Parameters.AddWithValue("@requestId", hdnRequestId.Value);
                                revertCmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                                revertCmd.ExecuteNonQuery();
                            }
                            ShowMessage("Insufficient inventory to fulfill this request. Status reverted to pending.", "danger");
                            Debug.WriteLine($"[{DateTime.Now}] btnProcessRequest_Click - Insufficient inventory for request ID {hdnRequestId.Value}");
                            return;
                        }
                    }

                    AddNotification(Convert.ToInt32(Session["UserId"]), "Request Processed",
                        $"Blood request {hdnRequestId.Value} has been {ddlRequestStatus.SelectedValue}");
                    ShowMessage("Request processed successfully.", "success");
                    Debug.WriteLine($"[{DateTime.Now}] btnProcessRequest_Click - Processed request ID {hdnRequestId.Value}, Status: {ddlRequestStatus.SelectedValue}");
                    LoadBloodRequests();
                    ClearForm();
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnProcessRequest_Click - MySQL Error: {ex.Message}, ErrorCode: {ex.Number}");
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
            try
            {
                ClearForm();
                LoadBloodRequests();
                Debug.WriteLine($"[{DateTime.Now}] btnClearForm_Click - Form cleared");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnClearForm_Click - Error: {ex.Message}");
                ShowMessage("Error clearing form: " + ex.Message, "danger");
            }
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
            Debug.WriteLine($"[{DateTime.Now}] ClearForm - Form fields reset");
        }

        private bool ValidateRequestForm()
        {
            if (string.IsNullOrEmpty(hdnRequestId.Value))
            {
                ShowMessage("No request selected.", "danger");
                Debug.WriteLine($"[{DateTime.Now}] ValidateRequestForm - No request selected");
                return false;
            }
            if (string.IsNullOrEmpty(ddlRequestStatus.SelectedValue))
            {
                ShowMessage("Status is required.", "danger");
                Debug.WriteLine($"[{DateTime.Now}] ValidateRequestForm - Status is empty");
                return false;
            }
            if ((ddlRequestStatus.SelectedValue == "approved" || ddlRequestStatus.SelectedValue == "fulfilled") &&
                string.IsNullOrEmpty(ddlFulfillHospital.SelectedValue))
            {
                ShowMessage("Fulfilling hospital is required for approved/fulfilled requests.", "danger");
                Debug.WriteLine($"[{DateTime.Now}] ValidateRequestForm - Fulfilling hospital required for status {ddlRequestStatus.SelectedValue}");
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
                    string getRequestQuery = "SELECT blood_type, quantity_ml FROM blood_requests WHERE request_id = @requestId AND requester_id = @hospitalId";
                    string bloodType;
                    int quantityNeeded;
                    using (MySqlCommand cmd = new MySqlCommand(getRequestQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@requestId", hdnRequestId.Value);
                        cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                Debug.WriteLine($"[{DateTime.Now}] UpdateInventoryForRequest - Request ID {hdnRequestId.Value} not found for hospital ID {Session["UserId"]}");
                                return false;
                            }
                            bloodType = reader["blood_type"].ToString();
                            quantityNeeded = Convert.ToInt32(reader["quantity_ml"]);
                        }
                    }

                    string checkInventoryQuery = @"SELECT SUM(quantity_ml) as total_quantity 
                                                  FROM blood_inventory 
                                                  WHERE blood_type = @bloodType AND status = 'available' AND expiration_date > CURRENT_TIMESTAMP AND tested_by_hospital = @hospitalId";
                    using (MySqlCommand cmd = new MySqlCommand(checkInventoryQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@bloodType", bloodType);
                        cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read() && reader["total_quantity"] != DBNull.Value)
                            {
                                int totalAvailable = Convert.ToInt32(reader["total_quantity"]);
                                if (totalAvailable < quantityNeeded)
                                {
                                    Debug.WriteLine($"[{DateTime.Now}] UpdateInventoryForRequest - Insufficient inventory: {totalAvailable} ml available, {quantityNeeded} ml needed for hospital ID {Session["UserId"]}");
                                    return false;
                                }
                            }
                            else
                            {
                                Debug.WriteLine($"[{DateTime.Now}] UpdateInventoryForRequest - No available inventory for blood type {bloodType} for hospital ID {Session["UserId"]}");
                                return false;
                            }
                        }
                    }

                    string findInventoryQuery = @"SELECT inventory_id, quantity_ml 
                                                 FROM blood_inventory 
                                                 WHERE blood_type = @bloodType AND status = 'available' AND expiration_date > CURRENT_TIMESTAMP AND tested_by_hospital = @hospitalId
                                                 ORDER BY expiration_date ASC";
                    using (MySqlCommand cmd = new MySqlCommand(findInventoryQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@bloodType", bloodType);
                        cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
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
                                                              WHERE inventory_id = @inventoryId AND tested_by_hospital = @hospitalId";
                                using (MySqlCommand updateCmd = new MySqlCommand(updateInventoryQuery, conn))
                                {
                                    updateCmd.Parameters.AddWithValue("@quantity", quantityToUse);
                                    updateCmd.Parameters.AddWithValue("@inventoryId", inventoryId);
                                    updateCmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                                    updateCmd.ExecuteNonQuery();
                                    Debug.WriteLine($"[{DateTime.Now}] UpdateInventoryForRequest - Updated inventory ID {inventoryId}, used {quantityToUse} ml");
                                }

                                quantityNeeded -= quantityToUse;
                            }
                        }
                    }
                    return quantityNeeded == 0;
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] UpdateInventoryForRequest - MySQL Error: {ex.Message}, ErrorCode: {ex.Number}");
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
        protected void lnkLogout_Click(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine($"[{DateTime.Now}] lnkLogout_Click - Logging out hospital user, UserId: {Session["UserId"]}");
                Session.Abandon();
                Response.Redirect("Login.aspx");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] lnkLogout_Click - Error: {ex.Message}");
                ShowMessage("Error during logout: " + ex.Message, "danger");
            }
        }

        private void AddNotification(int? hospitalId, string title, string message)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO notifications (hospital_id, title, message, is_read, created_at) 
                                    VALUES (@hospitalId, @title, @message, 0, CURRENT_TIMESTAMP)";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@hospitalId", hospitalId.HasValue ? (object)hospitalId.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@title", title);
                        cmd.Parameters.AddWithValue("@message", message);
                        cmd.ExecuteNonQuery();
                        Debug.WriteLine($"[{DateTime.Now}] AddNotification - Added notification: {title}, {message}");
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] AddNotification - MySQL Error: {ex.Message}, ErrorCode: {ex.Number}");
                ShowMessage("Error adding notification: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] AddNotification - Error: {ex.Message}");
                ShowMessage("Error adding notification: " + ex.Message, "danger");
            }
        }

        private void ShowMessage(string message, string type)
        {
            pnlMessage.Visible = true;
            lblMessage.Text = message;
            pnlMessage.CssClass = "alert alert-" + type;
            Debug.WriteLine($"[{DateTime.Now}] ShowMessage - Displayed message: {message}, Type: {type}");
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
            // Placeholder for future use
        }
    }
}