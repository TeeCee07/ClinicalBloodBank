using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ClinicalBloodBank
{
    public partial class ManageBloodRequests : System.Web.UI.Page
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
                    LoadBloodRequests();
                    LoadHospitalDropdown();
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

        private void LoadBloodRequests()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT r.request_id, r.blood_type, r.quantity_ml, r.urgency, r.reason, r.status, r.requested_at,
                                    CASE 
                                        WHEN r.requester_role = 'donor' THEN CONCAT(u.first_name, ' ', u.last_name)
                                        WHEN r.requester_role = 'hospital' THEN h.hospital_name
                                    END as requester_name
                                    FROM blood_requests r
                                    LEFT JOIN users u ON r.requester_id = u.user_id AND r.requester_role = 'donor'
                                    LEFT JOIN hospitals h ON r.requester_id = h.user_id AND r.requester_role = 'hospital'";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        gvBloodRequests.DataSource = dt;
                        gvBloodRequests.DataBind();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("LoadBloodRequests Error: " + ex.Message);
                    ShowErrorMessage("Error loading blood requests.");
                }
            }
        }

        private void LoadHospitalDropdown()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
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
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("LoadHospitalDropdown Error: " + ex.Message);
                    ShowErrorMessage("Error loading hospital dropdown.");
                }
            }
        }

        protected void gvBloodRequests_RowEditing(object sender, GridViewEditEventArgs e)
        {
            try
            {
                string requestId = gvBloodRequests.DataKeys[e.NewEditIndex].Value.ToString();
                LoadRequestData(requestId);
                e.Cancel = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("gvBloodRequests_RowEditing Error: " + ex.Message);
                ShowErrorMessage("Error editing blood request.");
            }
        }

        private void LoadRequestData(string requestId)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT r.request_id, r.blood_type, r.quantity_ml, r.urgency, r.reason, r.status, 
                                    r.fulfilled_by_hospital, r.patient_details as notes,
                                    CASE 
                                        WHEN r.requester_role = 'donor' THEN CONCAT(u.first_name, ' ', u.last_name)
                                        WHEN r.requester_role = 'hospital' THEN h.hospital_name
                                    END as requester_name
                                    FROM blood_requests r
                                    LEFT JOIN users u ON r.requester_id = u.user_id AND r.requester_role = 'donor'
                                    LEFT JOIN hospitals h ON r.requester_id = h.user_id AND r.requester_role = 'hospital'
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
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("LoadRequestData Error: " + ex.Message);
                    ShowErrorMessage("Error loading request data.");
                }
            }
        }

        protected void btnProcessRequest_Click(object sender, EventArgs e)
        {
            if (!ValidateRequestForm()) return;

            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"UPDATE blood_requests SET status = @status, fulfilled_by_hospital = @hospitalId, 
                                  patient_details = @notes, fulfilled_at = NOW()
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
                        UpdateInventoryForRequest();
                    }

                    LoadBloodRequests();
                    AddNotification("Request Processed", $"Blood request {hdnRequestId.Value} has been {ddlRequestStatus.SelectedValue}");
                    ShowSuccessMessage("Request processed successfully.");
                    ClearForm();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("btnProcessRequest_Click Error: " + ex.Message);
                    ShowErrorMessage("Error processing request.");
                }
            }
        }

        protected void btnClearForm_Click(object sender, EventArgs e)
        {
            ClearForm();
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
            if (string.IsNullOrEmpty(hdnRequestId.Value) || string.IsNullOrEmpty(ddlRequestStatus.SelectedValue))
            {
                ShowErrorMessage("Status is required.");
                return false;
            }
            return true;
        }

        private void UpdateInventoryForRequest()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
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
                            if (reader.Read())
                            {
                                bloodType = reader["blood_type"].ToString();
                                quantityNeeded = Convert.ToInt32(reader["quantity_ml"]);
                            }
                            else
                            {
                                return;
                            }
                        }
                    }

                    string findInventoryQuery = @"SELECT inventory_id, quantity_ml 
                                               FROM blood_inventory 
                                               WHERE blood_type = @bloodType AND status = 'available' 
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
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("UpdateInventoryForRequest Error: " + ex.Message);
                    ShowErrorMessage("Error updating inventory for request.");
                }
            }
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