using System;
using System.Configuration;
using System.Data;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Web.UI.WebControls;

namespace ClinicalBloodBank
{
    public partial class ManageRequests : System.Web.UI.Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;

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
                if (!IsPostBack)
                {
                    LoadUserInfo();
                    LoadRequests();
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

        private void LoadRequests()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT br.request_id AS 'Request ID', h.hospital_name AS 'Requester Hospital', 
                                    br.requester_id AS 'Requester ID', br.blood_type AS 'Blood Type', 
                                    br.quantity_ml AS 'Quantity (ml)', br.urgency AS Urgency, br.status AS Status, 
                                    br.reason AS Reason, br.requested_at AS 'Requested At'
                                    FROM blood_requests br
                                    LEFT JOIN hospitals h ON br.requester_id = h.hospital_id
                                    WHERE br.status = 'pending'";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            gvRequests.DataSource = dt;
                            gvRequests.DataBind();
                            Debug.WriteLine($"[{DateTime.Now}] LoadRequests - Loaded {dt.Rows.Count} pending requests for hospital ID: {Session["UserId"]}");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadRequests - MySQL Error: {ex.Message}, ErrorCode: {ex.Number}");
                ShowMessage("Error loading requests: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadRequests - Error: {ex.Message}");
                ShowMessage("Error loading requests: " + ex.Message, "danger");
            }
        }

        protected void btnCreateRequest_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO blood_requests (requester_id, blood_type, quantity_ml, urgency, status, reason, patient_details, requested_at)
                                    VALUES (@hospitalId, @bloodType, @quantity, @urgency, 'pending', @reason, @patientDetails, CURRENT_TIMESTAMP)";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                        cmd.Parameters.AddWithValue("@bloodType", ddlBloodType.SelectedValue);
                        cmd.Parameters.AddWithValue("@quantity", Convert.ToInt32(txtQuantity.Text));
                        cmd.Parameters.AddWithValue("@urgency", ddlUrgency.SelectedValue);
                        cmd.Parameters.AddWithValue("@reason", txtReason.Text.Trim());
                        cmd.Parameters.AddWithValue("@patientDetails", string.IsNullOrEmpty(txtPatientDetails.Text) ? (object)DBNull.Value : txtPatientDetails.Text.Trim());
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            AddNotification(Convert.ToInt32(Session["UserId"]), "Blood Request Created", $"New blood request for {ddlBloodType.SelectedValue} created.");
                            ShowMessage("Blood request created successfully.", "success");
                            LoadRequests();
                            ClearForm();
                            Debug.WriteLine($"[{DateTime.Now}] btnCreateRequest_Click - Created request for hospital ID: {Session["UserId"]}, Blood Type: {ddlBloodType.SelectedValue}");
                        }
                        else
                        {
                            ShowMessage("Failed to create blood request.", "danger");
                            Debug.WriteLine($"[{DateTime.Now}] btnCreateRequest_Click - No rows affected for hospital ID: {Session["UserId"]}");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnCreateRequest_Click - MySQL Error: {ex.Message}, ErrorCode: {ex.Number}");
                ShowMessage("Error creating request: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnCreateRequest_Click - Error: {ex.Message}");
                ShowMessage("Error creating request: " + ex.Message, "danger");
            }
        }

        protected void gvRequests_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                gvRequests.PageIndex = e.NewPageIndex;
                LoadRequests();
                Debug.WriteLine($"[{DateTime.Now}] gvRequests_PageIndexChanging - Changed to page: {e.NewPageIndex}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] gvRequests_PageIndexChanging - Error: {ex.Message}");
                ShowMessage("Error changing page: " + ex.Message, "danger");
            }
        }

        protected void gvRequests_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                int requestId = Convert.ToInt32(e.CommandArgument);
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    if (e.CommandName == "ApproveRequest")
                    {
                        // Get request details
                        string selectQuery = @"SELECT requester_id, blood_type, quantity_ml 
                                              FROM blood_requests 
                                              WHERE request_id = @requestId AND status = 'pending'";
                        using (MySqlCommand selectCmd = new MySqlCommand(selectQuery, conn))
                        {
                            selectCmd.Parameters.AddWithValue("@requestId", requestId);
                            using (MySqlDataReader reader = selectCmd.ExecuteReader())
                            {
                                if (!reader.Read())
                                {
                                    ShowMessage("Request not found or already processed.", "danger");
                                    Debug.WriteLine($"[{DateTime.Now}] gvRequests_RowCommand - Request ID: {requestId} not found or not pending");
                                    return;
                                }

                                int requesterId = Convert.ToInt32(reader["requester_id"]);
                                string bloodType = reader["blood_type"].ToString();
                                int quantityMl = Convert.ToInt32(reader["quantity_ml"]);
                                reader.Close();

                                // Check if approving hospital has enough inventory
                                string inventoryQuery = @"SELECT SUM(quantity_ml) AS total_quantity 
                                                         FROM blood_inventory 
                                                         WHERE tested_by_hospital = @hospitalId 
                                                         AND blood_type = @bloodType 
                                                         AND status = 'available'";
                                using (MySqlCommand inventoryCmd = new MySqlCommand(inventoryQuery, conn))
                                {
                                    inventoryCmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                                    inventoryCmd.Parameters.AddWithValue("@bloodType", bloodType);
                                    object result = inventoryCmd.ExecuteScalar();
                                    int availableQuantity = result != DBNull.Value ? Convert.ToInt32(result) : 0;

                                    if (availableQuantity < quantityMl)
                                    {
                                        ShowMessage($"Insufficient inventory for {bloodType}. Available: {availableQuantity} ml, Required: {quantityMl} ml.", "danger");
                                        Debug.WriteLine($"[{DateTime.Now}] gvRequests_RowCommand - Insufficient inventory for request ID: {requestId}, Blood Type: {bloodType}");
                                        return;
                                    }
                                }

                                // Update approving hospital's inventory (subtract quantity)
                                string updateInventoryQuery = @"UPDATE blood_inventory 
                                                               SET quantity_ml = quantity_ml - @quantity, 
                                                                   status = CASE WHEN quantity_ml - @quantity <= 0 THEN 'used' ELSE 'available' END
                                                               WHERE tested_by_hospital = @hospitalId 
                                                               AND blood_type = @bloodType 
                                                               AND status = 'available' 
                                                               LIMIT 1";
                                using (MySqlCommand updateInventoryCmd = new MySqlCommand(updateInventoryQuery, conn))
                                {
                                    updateInventoryCmd.Parameters.AddWithValue("@quantity", quantityMl);
                                    updateInventoryCmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                                    updateInventoryCmd.Parameters.AddWithValue("@bloodType", bloodType);
                                    int rowsAffected = updateInventoryCmd.ExecuteNonQuery();

                                    if (rowsAffected == 0)
                                    {
                                        ShowMessage("Failed to update inventory.", "danger");
                                        Debug.WriteLine($"[{DateTime.Now}] gvRequests_RowCommand - Failed to update inventory for request ID: {requestId}");
                                        return;
                                    }
                                }

                                // Add to requesting hospital's inventory
                                string insertInventoryQuery = @"INSERT INTO blood_inventory (blood_type, quantity_ml, donation_date, expiration_date, tested_by_hospital, test_result, status)
                                                               VALUES (@bloodType, @quantity, CURRENT_DATE, DATE_ADD(CURRENT_DATE, INTERVAL 42 DAY), @requesterId, 'passed', 'available')";
                                using (MySqlCommand insertInventoryCmd = new MySqlCommand(insertInventoryQuery, conn))
                                {
                                    insertInventoryCmd.Parameters.AddWithValue("@bloodType", bloodType);
                                    insertInventoryCmd.Parameters.AddWithValue("@quantity", quantityMl);
                                    insertInventoryCmd.Parameters.AddWithValue("@requesterId", requesterId);
                                    insertInventoryCmd.ExecuteNonQuery();
                                }

                                // Update request status
                                string updateRequestQuery = @"UPDATE blood_requests 
                                                             SET status = 'fulfilled', fulfilled_by_hospital = @hospitalId, fulfilled_at = CURRENT_TIMESTAMP
                                                             WHERE request_id = @requestId AND status = 'pending'";
                                using (MySqlCommand updateRequestCmd = new MySqlCommand(updateRequestQuery, conn))
                                {
                                    updateRequestCmd.Parameters.AddWithValue("@requestId", requestId);
                                    updateRequestCmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                                    int rowsAffected = updateRequestCmd.ExecuteNonQuery();

                                    if (rowsAffected > 0)
                                    {
                                        AddNotification(Convert.ToInt32(Session["UserId"]), "Blood Request Approved", $"Approved blood request ID {requestId} for {bloodType}.");
                                        AddNotification(requesterId, "Blood Request Fulfilled", $"Your blood request ID {requestId} for {bloodType} has been fulfilled.");
                                        ShowMessage("Blood request approved and inventory updated.", "success");
                                        LoadRequests();
                                        Debug.WriteLine($"[{DateTime.Now}] gvRequests_RowCommand - Approved request ID: {requestId} by hospital ID: {Session["UserId"]}");
                                    }
                                    else
                                    {
                                        ShowMessage("Failed to approve request. It may already be processed.", "danger");
                                        Debug.WriteLine($"[{DateTime.Now}] gvRequests_RowCommand - No rows affected for request ID: {requestId}");
                                    }
                                }
                            }
                        }
                    }
                    else if (e.CommandName == "CancelRequest")
                    {
                        string query = "UPDATE blood_requests SET status = 'rejected' WHERE request_id = @requestId AND requester_id = @hospitalId AND status = 'pending'";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@requestId", requestId);
                            cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                AddNotification(Convert.ToInt32(Session["UserId"]), "Blood Request Cancelled", $"Blood request ID {requestId} has been cancelled.");
                                ShowMessage("Blood request cancelled successfully.", "success");
                                LoadRequests();
                                Debug.WriteLine($"[{DateTime.Now}] gvRequests_RowCommand - Cancelled request ID: {requestId} for hospital ID: {Session["UserId"]}");
                            }
                            else
                            {
                                ShowMessage("Failed to cancel request. It may already be processed.", "danger");
                                Debug.WriteLine($"[{DateTime.Now}] gvRequests_RowCommand - No rows affected for request ID: {requestId}");
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] gvRequests_RowCommand - MySQL Error: {ex.Message}, ErrorCode: {ex.Number}");
                ShowMessage("Error processing request: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] gvRequests_RowCommand - Error: {ex.Message}");
                ShowMessage("Error processing request: " + ex.Message, "danger");
            }
        }

        private void ClearForm()
        {
            ddlBloodType.SelectedIndex = 0;
            txtQuantity.Text = "";
            ddlUrgency.SelectedIndex = 0;
            txtReason.Text = "";
            txtPatientDetails.Text = "";
        }

        private void AddNotification(int hospitalId, string title, string message)
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
                        cmd.Parameters.AddWithValue("@hospitalId", hospitalId);
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
        }

        private void ShowMessage(string message, string type)
        {
            pnlMessage.Visible = true;
            lblMessage.Text = message;
            pnlMessage.CssClass = "alert alert-" + type;
            Debug.WriteLine($"[{DateTime.Now}] ShowMessage - Displayed message: {message}, Type: {type}");
        }
    }
}