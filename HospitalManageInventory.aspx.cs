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
    public partial class HospitalManageInventory : System.Web.UI.Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;
        private List<string> controlsToRegister = new List<string>();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null || Session["UserType"] == null || Session["UserType"].ToString() != "hospital")
            {
                Debug.WriteLine($"[{DateTime.Now}] Page_Load: Invalid session. UserId: {Session["UserId"]}, UserType: {Session["UserType"]}");
                Response.Redirect("Login.aspx");
                return;
            }

            try
            {
                this.PreRender += new EventHandler(Page_PreRender);
                if (!IsPostBack)
                {
                    LoadHospitalDetails();
                    LoadInventory();
                    LoadDonorDropdown();
                    LoadBloodTypeDropdown();
                    LoadNotifications();
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

        private void LoadHospitalDetails()
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
                                litUserName.Text = reader["hospital_name"]?.ToString() ?? "Hospital";
                                litUserInitials.Text = GetInitials(reader["hospital_name"]?.ToString() ?? "Hospital");
                            }
                            else
                            {
                                litUserName.Text = Session["UserName"]?.ToString() ?? "Hospital";
                                litUserInitials.Text = GetInitials(Session["UserName"]?.ToString() ?? "Hospital");
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadHospitalDetails - MySQL Error: {ex.Message}");
                ShowMessage("Error loading hospital details: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadHospitalDetails - Error: {ex.Message}");
                ShowMessage("Error loading hospital details: " + ex.Message, "danger");
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

        private void LoadInventory()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT i.inventory_id, i.blood_type, i.quantity_ml, i.donation_date, i.expiration_date, 
                                    i.test_result, i.status, 
                                    CONCAT(d.first_name, ' ', d.last_name) as donor_name
                                    FROM blood_inventory i
                                    INNER JOIN donors d ON i.donor_id = d.donor_id
                                    WHERE i.tested_by_hospital = @hospitalId";

                    string bloodType = ddlFilterBloodType.SelectedValue;
                    string expirationDate = txtFilterExpirationDate.Text;

                    if (!string.IsNullOrEmpty(bloodType))
                        query += " AND i.blood_type = @BloodType";
                    if (!string.IsNullOrEmpty(expirationDate))
                        query += " AND i.expiration_date <= @ExpirationDate";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                        if (!string.IsNullOrEmpty(bloodType))
                            cmd.Parameters.AddWithValue("@BloodType", bloodType);
                        if (!string.IsNullOrEmpty(expirationDate))
                            cmd.Parameters.AddWithValue("@ExpirationDate", DateTime.Parse(expirationDate));

                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        gvInventory.DataSource = dt;
                        gvInventory.DataBind();

                        if (dt.Rows.Count == 0)
                            ShowMessage("No inventory items found.", "info");
                        else
                            ShowMessage($"{dt.Rows.Count} inventory item(s) found.", "success");
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadInventory - MySQL Error: {ex.Message}");
                ShowMessage("Error loading inventory: " + ex.Message, "danger");
                gvInventory.DataSource = null;
                gvInventory.DataBind();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadInventory - Error: {ex.Message}");
                ShowMessage("Error loading inventory: " + ex.Message, "danger");
                gvInventory.DataSource = null;
                gvInventory.DataBind();
            }
        }

        private void LoadDonorDropdown()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT donor_id, CONCAT(COALESCE(first_name, ''), ' ', COALESCE(last_name, '')) as donor_name
                                    FROM donors
                                    WHERE is_active = 1";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        ddlDonor.DataSource = dt;
                        ddlDonor.DataTextField = "donor_name";
                        ddlDonor.DataValueField = "donor_id";
                        ddlDonor.DataBind();
                        ddlDonor.Items.Insert(0, new ListItem("Select Donor", ""));
                        if (dt.Rows.Count == 0)
                            ShowMessage("No active donors found.", "info");
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadDonorDropdown - MySQL Error: {ex.Message}");
                ShowMessage("Error loading donor dropdown: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadDonorDropdown - Error: {ex.Message}");
                ShowMessage("Error loading donor dropdown: " + ex.Message, "danger");
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

        protected void btnClearAll_Click(object sender, EventArgs e)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"UPDATE notifications 
                                    SET is_read = 1
                                    WHERE hospital_id = @hospitalId AND is_read = 0";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            ShowMessage("All notifications marked as read.", "success");
                            LoadNotifications();
                        }
                        else
                        {
                            ShowMessage("No unread notifications to clear.", "info");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnClearAll_Click - MySQL Error: {ex.Message}");
                ShowMessage("Error clearing notifications: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnClearAll_Click - Error: {ex.Message}");
                ShowMessage("Error clearing notifications: " + ex.Message, "danger");
            }
        }

        private void LoadNotifications()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT notification_id, title, message, created_at, is_read
                                    FROM notifications
                                    WHERE hospital_id = @hospitalId
                                    ORDER BY created_at DESC";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        int unreadCount = 0;
                        foreach (DataRow row in dt.Rows)
                        {
                            if (!Convert.ToBoolean(row["is_read"]))
                                unreadCount++;
                        }
                        notificationCount.InnerText = unreadCount.ToString();

                        string notificationHtml = "";
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                string isReadClass = Convert.ToBoolean(row["is_read"]) ? "" : "unread";
                                string createdAt = Convert.ToDateTime(row["created_at"]).ToString("yyyy-MM-dd HH:mm");
                                notificationHtml += $"<div class='notification-item {isReadClass}' onclick='markNotificationRead({row["notification_id"]})'>" +
                                                   "<div class='notification-icon'>🔔</div>" +
                                                   "<div class='notification-content'>" +
                                                   $"<div class='notification-message'>{Server.HtmlEncode(row["message"].ToString())}</div>" +
                                                   $"<div class='notification-time'>{createdAt}</div>" +
                                                   "</div></div>";
                            }
                        }
                        else
                        {
                            notificationHtml = "<div class='no-notifications'>No notifications found.</div>";
                        }
                        notificationList.InnerHtml = notificationHtml;

                        Debug.WriteLine($"[{DateTime.Now}] LoadNotifications: Retrieved {dt.Rows.Count} notifications, {unreadCount} unread");
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadNotifications - MySQL Error: {ex.Message}");
                ShowMessage("Error loading notifications: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadNotifications - Error: {ex.Message}");
                ShowMessage("Error loading notifications: " + ex.Message, "danger");
            }
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            try
            {
                LoadInventory();
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
                txtFilterExpirationDate.Text = "";
                LoadInventory();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnClearFilter_Click - Error: {ex.Message}");
                ShowMessage("Error clearing filter: " + ex.Message, "danger");
            }
        }

        protected void gvInventory_RowEditing(object sender, GridViewEditEventArgs e)
        {
            try
            {
                string inventoryId = gvInventory.DataKeys[e.NewEditIndex].Value.ToString();
                LoadInventoryData(inventoryId);
                gvInventory.EditIndex = -1;
                LoadInventory();
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] gvInventory_RowEditing - MySQL Error: {ex.Message}");
                ShowMessage("Error loading inventory data: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] gvInventory_RowEditing - Error: {ex.Message}");
                ShowMessage("Error loading inventory data: " + ex.Message, "danger");
            }
        }

        protected void gvInventory_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string inventoryId = gvInventory.DataKeys[e.RowIndex].Value.ToString();
                    string query = "DELETE FROM blood_inventory WHERE inventory_id = @inventoryId AND tested_by_hospital = @hospitalId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@inventoryId", inventoryId);
                        cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            AddNotification(Convert.ToInt32(Session["UserId"]), "Inventory Deleted", $"Blood inventory item deleted: ID {inventoryId}");
                            ShowMessage("Inventory deleted successfully.", "success");
                        }
                        else
                        {
                            ShowMessage("Inventory item not found or you lack permission to delete it.", "danger");
                        }
                    }
                    LoadInventory();
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] gvInventory_RowDeleting - MySQL Error: {ex.Message}");
                ShowMessage("Error deleting inventory: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] gvInventory_RowDeleting - Error: {ex.Message}");
                ShowMessage("Error deleting inventory: " + ex.Message, "danger");
            }
        }

        protected void gvInventory_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                gvInventory.PageIndex = e.NewPageIndex;
                LoadInventory();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] gvInventory_PageIndexChanging - Error: {ex.Message}");
                ShowMessage("Error changing page: " + ex.Message, "danger");
            }
        }

        protected void gvInventory_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                try
                {
                    Button btnDelete = (Button)e.Row.FindControl("btnDelete");
                    if (btnDelete != null)
                    {
                        btnDelete.OnClientClick = "return confirm('Are you sure you want to delete this inventory item?');";
                        controlsToRegister.Add(btnDelete.UniqueID);
                    }

                    Button btnEdit = (Button)e.Row.FindControl("btnEdit");
                    if (btnEdit != null)
                    {
                        controlsToRegister.Add(btnEdit.UniqueID);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[{DateTime.Now}] gvInventory_RowDataBound - Error: {ex.Message}");
                    ShowMessage("Error binding row: " + ex.Message, "danger");
                }
            }
        }

        private void LoadInventoryData(string inventoryId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT inventory_id, blood_type, quantity_ml, donation_date, expiration_date, 
                                    donor_id, test_result, status
                                    FROM blood_inventory 
                                    WHERE inventory_id = @inventoryId AND tested_by_hospital = @hospitalId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@inventoryId", inventoryId);
                        cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hdnInventoryId.Value = reader["inventory_id"].ToString();
                                ddlInventoryBloodType.SelectedValue = reader["blood_type"].ToString();
                                txtQuantity.Text = reader["quantity_ml"].ToString();
                                txtDonationDate.Text = Convert.ToDateTime(reader["donation_date"]).ToString("yyyy-MM-dd");
                                lblExpirationDate.Text = Convert.ToDateTime(reader["expiration_date"]).ToString("yyyy-MM-dd");
                                ddlDonor.SelectedValue = reader["donor_id"].ToString();
                                ddlTestResult.SelectedValue = reader["test_result"].ToString();
                                ddlStatus.SelectedValue = reader["status"].ToString();
                            }
                            else
                            {
                                ShowMessage("Inventory item not found or you lack permission to edit it.", "danger");
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadInventoryData - MySQL Error: {ex.Message}");
                ShowMessage("Error loading inventory data: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadInventoryData - Error: {ex.Message}");
                ShowMessage("Error loading inventory data: " + ex.Message, "danger");
            }
        }

        protected void btnSaveInventory_Click(object sender, EventArgs e)
        {
            if (!ValidateInventoryForm()) return;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    DateTime donationDate = DateTime.Parse(txtDonationDate.Text);
                    DateTime expirationDate = donationDate.AddDays(42);

                    string bloodType = ddlInventoryBloodType.SelectedValue; // Use selected blood type, not "unknown"

                    if (string.IsNullOrEmpty(hdnInventoryId.Value)) // New inventory
                    {
                        string query = @"INSERT INTO blood_inventory (blood_type, quantity_ml, donation_date, expiration_date, 
                                      donor_id, tested_by_hospital, test_result, status, created_at)
                                      VALUES (@bloodType, @quantity, @donationDate, @expirationDate, @donorId, @hospitalId, 
                                      @testResult, @status, CURRENT_TIMESTAMP)";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@bloodType", bloodType);
                            cmd.Parameters.AddWithValue("@quantity", int.Parse(txtQuantity.Text));
                            cmd.Parameters.AddWithValue("@donationDate", donationDate);
                            cmd.Parameters.AddWithValue("@expirationDate", expirationDate);
                            cmd.Parameters.AddWithValue("@donorId", int.Parse(ddlDonor.SelectedValue));
                            cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                            cmd.Parameters.AddWithValue("@testResult", ddlTestResult.SelectedValue);
                            cmd.Parameters.AddWithValue("@status", ddlStatus.SelectedValue);
                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                AddNotification(Convert.ToInt32(Session["UserId"]), "Inventory Added",
                                    $"New blood inventory added: {txtQuantity.Text}ml of {bloodType}");
                                ShowMessage("Inventory added successfully.", "success");
                            }
                            else
                            {
                                ShowMessage("Failed to add inventory.", "danger");
                            }
                        }
                    }
                    else // Update existing inventory
                    {
                        string query = @"UPDATE blood_inventory SET blood_type = @bloodType, quantity_ml = @quantity, 
                                      donation_date = @donationDate, expiration_date = @expirationDate, 
                                      donor_id = @donorId, test_result = @testResult, status = @status
                                      WHERE inventory_id = @inventoryId AND tested_by_hospital = @hospitalId";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@bloodType", bloodType);
                            cmd.Parameters.AddWithValue("@quantity", int.Parse(txtQuantity.Text));
                            cmd.Parameters.AddWithValue("@donationDate", donationDate);
                            cmd.Parameters.AddWithValue("@expirationDate", expirationDate);
                            cmd.Parameters.AddWithValue("@donorId", int.Parse(ddlDonor.SelectedValue));
                            cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                            cmd.Parameters.AddWithValue("@testResult", ddlTestResult.SelectedValue);
                            cmd.Parameters.AddWithValue("@status", ddlStatus.SelectedValue);
                            cmd.Parameters.AddWithValue("@inventoryId", int.Parse(hdnInventoryId.Value));
                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                AddNotification(Convert.ToInt32(Session["UserId"]), "Inventory Updated",
                                    $"Blood inventory updated: {txtQuantity.Text}ml of {bloodType}");
                                ShowMessage("Inventory updated successfully.", "success");
                            }
                            else
                            {
                                ShowMessage("Inventory item not found or you lack permission to update it.", "danger");
                            }
                        }
                    }

                    LoadInventory();
                    ClearForm();
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnSaveInventory_Click - MySQL Error: {ex.Message}");
                ShowMessage("Error saving inventory: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnSaveInventory_Click - Error: {ex.Message}");
                ShowMessage("Error saving inventory: " + ex.Message, "danger");
            }
        }

        protected void btnClearForm_Click(object sender, EventArgs e)
        {
            try
            {
                ClearForm();
                LoadInventory();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnClearForm_Click - Error: {ex.Message}");
                ShowMessage("Error clearing form: " + ex.Message, "danger");
            }
        }

        private void ClearForm()
        {
            hdnInventoryId.Value = "";
            ddlInventoryBloodType.SelectedIndex = 0;
            txtQuantity.Text = "";
            txtDonationDate.Text = "";
            lblExpirationDate.Text = "";
            ddlDonor.SelectedIndex = 0;
            ddlTestResult.SelectedIndex = 0;
            ddlStatus.SelectedIndex = 0;
        }

        private bool ValidateInventoryForm()
        {
            if (string.IsNullOrEmpty(ddlInventoryBloodType.SelectedValue) ||
                string.IsNullOrEmpty(txtQuantity.Text) ||
                string.IsNullOrEmpty(txtDonationDate.Text) ||
                string.IsNullOrEmpty(ddlDonor.SelectedValue) ||
                string.IsNullOrEmpty(ddlTestResult.SelectedValue) ||
                string.IsNullOrEmpty(ddlStatus.SelectedValue))
            {
                ShowMessage("All fields except expiration date are required.", "danger");
                return false;
            }

            try
            {
                int quantity = int.Parse(txtQuantity.Text);
                if (quantity <= 0)
                {
                    ShowMessage("Quantity must be greater than 0.", "danger");
                    return false;
                }

                DateTime donationDate = DateTime.Parse(txtDonationDate.Text);
                if (donationDate > DateTime.Now)
                {
                    ShowMessage("Donation date cannot be in the future.", "danger");
                    return false;
                }
            }
            catch (FormatException)
            {
                ShowMessage("Invalid quantity or date format.", "danger");
                return false;
            }

            return true;
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
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] AddNotification - MySQL Error: {ex.Message}");
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
            try
            {
                Session.Clear();
                Session.Abandon();
                Response.Redirect("Login.aspx");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] lnkLogout_Click - Error: {ex.Message}");
                ShowMessage("Error logging out: " + ex.Message, "danger");
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

        protected void Page_PreRender(object sender, EventArgs e)
        {
            // Placeholder for future use
        }
    }
}