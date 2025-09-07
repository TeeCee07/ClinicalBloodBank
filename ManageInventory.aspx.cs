using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ClinicalBloodBank
{
    public partial class ManageInventory : System.Web.UI.Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;
        private List<string> controlsToRegister = new List<string>();
        private readonly List<string> validBloodTypes = new List<string> { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["AdminId"] == null)
            {
                Debug.WriteLine($"[{DateTime.Now}] Page_Load - Invalid session. AdminId: {Session["AdminId"]}");
                Response.Redirect("Login.aspx");
                return;
            }

            try
            {
                this.PreRender += new EventHandler(Page_PreRender);
                if (!IsPostBack)
                {
                    LoadUserInfo();
                    LoadInventory();
                    LoadDonorDropdown();
                    LoadHospitalDropdown();
                    LoadBloodTypeDropdown();
                    LoadProvinceDropdown();
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
                litUserName.Text = Session["AdminName"]?.ToString() ?? "Admin";
                litUserInitials.Text = GetInitials(litUserName.Text);
                Debug.WriteLine($"[{DateTime.Now}] LoadUserInfo - AdminName: {litUserName.Text}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadUserInfo - Error: {ex.Message}, StackTrace: {ex.StackTrace}");
                ShowMessage("Error loading admin info: " + ex.Message, "danger");
                litUserName.Text = "Admin";
                litUserInitials.Text = "AD";
            }
        }

        private string GetInitials(string name)
        {
            if (string.IsNullOrEmpty(name)) return "AD";
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
                                    COALESCE(CONCAT(d.first_name, ' ', d.last_name), 'Transfer') as donor_name,
                                    h.hospital_name, h.province
                                    FROM blood_inventory i
                                    LEFT JOIN donors d ON i.donor_id = d.donor_id
                                    INNER JOIN hospitals h ON i.tested_by_hospital = h.hospital_id";

                    // Apply filters
                    string bloodType = ddlFilterBloodType.SelectedValue;
                    string expirationDate = txtFilterExpirationDate.Text;
                    string province = ddlFilterProvince.SelectedValue;

                    if (!string.IsNullOrEmpty(bloodType))
                        query += " WHERE i.blood_type = @BloodType";
                    if (!string.IsNullOrEmpty(expirationDate))
                        query += (query.Contains("WHERE") ? " AND" : " WHERE") + " i.expiration_date <= @ExpirationDate";
                    if (!string.IsNullOrEmpty(province))
                        query += (query.Contains("WHERE") ? " AND" : " WHERE") + " h.province = @Province";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        if (!string.IsNullOrEmpty(bloodType))
                            cmd.Parameters.AddWithValue("@BloodType", bloodType);
                        if (!string.IsNullOrEmpty(expirationDate))
                            cmd.Parameters.AddWithValue("@ExpirationDate", DateTime.Parse(expirationDate));
                        if (!string.IsNullOrEmpty(province))
                            cmd.Parameters.AddWithValue("@Province", province);

                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            gvInventory.DataSource = dt;
                            gvInventory.DataBind();

                            if (dt.Rows.Count == 0)
                                ShowMessage("No inventory items found.", "info");
                            else
                                ShowMessage($"{dt.Rows.Count} inventory item(s) found.", "success");
                            Debug.WriteLine($"[{DateTime.Now}] LoadInventory - Loaded {dt.Rows.Count} inventory items");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadInventory - MySQL Error: {ex.Message}, ErrorCode: {ex.Number}");
                ShowMessage("Error loading inventory: " + ex.Message, "danger");
                gvInventory.DataSource = null;
                gvInventory.DataBind();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadInventory - Error: {ex.Message}, StackTrace: {ex.StackTrace}");
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
                    string query = @"SELECT d.donor_id, CONCAT(d.first_name, ' ', d.last_name) as donor_name
                                    FROM donors d
                                    WHERE d.is_active = 1";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            ddlDonor.DataSource = dt;
                            ddlDonor.DataTextField = "donor_name";
                            ddlDonor.DataValueField = "donor_id";
                            ddlDonor.DataBind();
                            ddlDonor.Items.Insert(0, new ListItem("Select Donor (or Transfer)", ""));
                            Debug.WriteLine($"[{DateTime.Now}] LoadDonorDropdown - Loaded {dt.Rows.Count} active donors");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadDonorDropdown - MySQL Error: {ex.Message}, ErrorCode: {ex.Number}");
                ShowMessage("Error loading donor dropdown: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadDonorDropdown - Error: {ex.Message}, StackTrace: {ex.StackTrace}");
                ShowMessage("Error loading donor dropdown: " + ex.Message, "danger");
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
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            ddlHospital.DataSource = dt;
                            ddlHospital.DataTextField = "hospital_name";
                            ddlHospital.DataValueField = "hospital_id";
                            ddlHospital.DataBind();
                            ddlHospital.Items.Insert(0, new ListItem("Select Hospital", ""));
                            Debug.WriteLine($"[{DateTime.Now}] LoadHospitalDropdown - Loaded {dt.Rows.Count} hospitals");
                        }
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
                Debug.WriteLine($"[{DateTime.Now}] LoadHospitalDropdown - Error: {ex.Message}, StackTrace: {ex.StackTrace}");
                ShowMessage("Error loading hospital dropdown: " + ex.Message, "danger");
            }
        }

        private void LoadBloodTypeDropdown()
        {
            try
            {
                ddlFilterBloodType.Items.Clear();
                ddlFilterBloodType.Items.Add(new ListItem("All Blood Types", ""));
                foreach (var bloodType in validBloodTypes)
                {
                    ddlFilterBloodType.Items.Add(new ListItem(bloodType, bloodType));
                }
                Debug.WriteLine($"[{DateTime.Now}] LoadBloodTypeDropdown - Loaded blood types");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadBloodTypeDropdown - Error: {ex.Message}, StackTrace: {ex.StackTrace}");
                ShowMessage("Error loading blood type dropdown: " + ex.Message, "danger");
            }
        }

        private void LoadProvinceDropdown()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT DISTINCT province FROM hospitals WHERE is_verified = 1 ORDER BY province";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            ddlFilterProvince.DataSource = dt;
                            ddlFilterProvince.DataTextField = "province";
                            ddlFilterProvince.DataValueField = "province";
                            ddlFilterProvince.DataBind();
                            ddlFilterProvince.Items.Insert(0, new ListItem("All Provinces", ""));
                            Debug.WriteLine($"[{DateTime.Now}] LoadProvinceDropdown - Loaded {dt.Rows.Count} provinces");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadProvinceDropdown - MySQL Error: {ex.Message}, ErrorCode: {ex.Number}");
                ShowMessage("Error loading province dropdown: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadProvinceDropdown - Error: {ex.Message}, StackTrace: {ex.StackTrace}");
                ShowMessage("Error loading province dropdown: " + ex.Message, "danger");
            }
        }

        [WebMethod]
        public static string GetDonorBloodType(string donorId)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT blood_type FROM donors WHERE donor_id = @donorId AND is_active = 1";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@donorId", donorId);
                        object result = cmd.ExecuteScalar();
                        string bloodType = result != null ? result.ToString() : "";
                        Debug.WriteLine($"[{DateTime.Now}] GetDonorBloodType - Donor ID: {donorId}, Blood Type: {bloodType}");
                        return bloodType;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] GetDonorBloodType - Error: {ex.Message}, StackTrace: {ex.StackTrace}");
                return "";
            }
        }

        protected void ddlDonor_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string donorId = ddlDonor.SelectedValue;
                if (string.IsNullOrEmpty(donorId))
                {
                    ddlInventoryBloodType.SelectedIndex = 0;
                    ddlInventoryBloodType.Enabled = true;
                    ddlInventoryBloodType.CssClass = "form-control";
                    Debug.WriteLine($"[{DateTime.Now}] ddlDonor_SelectedIndexChanged - No donor selected, blood type dropdown enabled");
                    return;
                }

                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT blood_type FROM donors WHERE donor_id = @donorId AND is_active = 1";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@donorId", donorId);
                        object result = cmd.ExecuteScalar();
                        string bloodType = result?.ToString() ?? "";

                        if (!string.IsNullOrEmpty(bloodType) && bloodType.ToLower() != "unknown" && IsValidBloodType(bloodType))
                        {
                            ddlInventoryBloodType.SelectedValue = bloodType;
                            ddlInventoryBloodType.Enabled = false;
                            ddlInventoryBloodType.CssClass = "form-control";
                            Debug.WriteLine($"[{DateTime.Now}] ddlDonor_SelectedIndexChanged - Donor ID: {donorId}, Blood Type: {bloodType}, Dropdown disabled");
                        }
                        else
                        {
                            ddlInventoryBloodType.SelectedIndex = 0;
                            ddlInventoryBloodType.Enabled = true;
                            ddlInventoryBloodType.CssClass = "form-control";
                            if (!string.IsNullOrEmpty(bloodType) && !IsValidBloodType(bloodType))
                            {
                                ShowMessage($"Invalid donor blood type '{bloodType}'. Please select a valid blood type.", "danger");
                                Debug.WriteLine($"[{DateTime.Now}] ddlDonor_SelectedIndexChanged - Invalid blood type: {bloodType} for Donor ID: {donorId}");
                            }
                            else
                            {
                                Debug.WriteLine($"[{DateTime.Now}] ddlDonor_SelectedIndexChanged - Donor ID: {donorId}, Blood Type: {bloodType}, Dropdown enabled");
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] ddlDonor_SelectedIndexChanged - MySQL Error: {ex.Message}, ErrorCode: {ex.Number}");
                ShowMessage("Error loading donor blood type: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] ddlDonor_SelectedIndexChanged - Error: {ex.Message}, StackTrace: {ex.StackTrace}");
                ShowMessage("Error loading donor blood type: " + ex.Message, "danger");
            }
        }

        private bool IsValidBloodType(string bloodType)
        {
            if (string.IsNullOrEmpty(bloodType)) return false;
            return validBloodTypes.Contains(bloodType, StringComparer.OrdinalIgnoreCase);
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            LoadInventory();
        }

        protected void btnClearFilter_Click(object sender, EventArgs e)
        {
            ddlFilterBloodType.SelectedIndex = 0;
            txtFilterExpirationDate.Text = "";
            ddlFilterProvince.SelectedIndex = 0;
            LoadInventory();
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
                Debug.WriteLine($"[{DateTime.Now}] gvInventory_RowEditing - MySQL Error: {ex.Message}, ErrorCode: {ex.Number}");
                ShowMessage("Error loading inventory data: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] gvInventory_RowEditing - Error: {ex.Message}, StackTrace: {ex.StackTrace}");
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
                    string query = "DELETE FROM blood_inventory WHERE inventory_id = @inventoryId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@inventoryId", inventoryId);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            AddNotification(Convert.ToInt32(Session["AdminId"]), "Inventory Deleted", $"Blood inventory item deleted: ID {inventoryId}");
                            ShowMessage("Inventory deleted successfully.", "success");
                            Debug.WriteLine($"[{DateTime.Now}] gvInventory_RowDeleting - Deleted inventory ID: {inventoryId} for admin ID: {Session["AdminId"]}");
                        }
                        else
                        {
                            ShowMessage("Inventory item not found.", "danger");
                            Debug.WriteLine($"[{DateTime.Now}] gvInventory_RowDeleting - No rows affected for inventory ID: {inventoryId}");
                        }
                    }
                }
                LoadInventory();
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] gvInventory_RowDeleting - MySQL Error: {ex.Message}, ErrorCode: {ex.Number}");
                ShowMessage("Error deleting inventory: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] gvInventory_RowDeleting - Error: {ex.Message}, StackTrace: {ex.StackTrace}");
                ShowMessage("Error deleting inventory: " + ex.Message, "danger");
            }
        }

        protected void gvInventory_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                gvInventory.PageIndex = e.NewPageIndex;
                LoadInventory();
                Debug.WriteLine($"[{DateTime.Now}] gvInventory_PageIndexChanging - Changed to page: {e.NewPageIndex}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] gvInventory_PageIndexChanging - Error: {ex.Message}, StackTrace: {ex.StackTrace}");
                ShowMessage("Error changing page: " + ex.Message, "danger");
            }
        }

        protected void gvInventory_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
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
        }

        private void LoadInventoryData(string inventoryId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT i.inventory_id, i.blood_type, i.quantity_ml, i.donation_date, i.expiration_date, 
                                    i.donor_id, i.tested_by_hospital, i.test_result, i.status,
                                    COALESCE(d.blood_type, '') as donor_blood_type
                                    FROM blood_inventory i
                                    LEFT JOIN donors d ON i.donor_id = d.donor_id
                                    WHERE i.inventory_id = @inventoryId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@inventoryId", inventoryId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hdnInventoryId.Value = reader["inventory_id"].ToString();
                                string donorBloodType = reader["donor_blood_type"].ToString();
                                string inventoryBloodType = reader["blood_type"].ToString();

                                // Set blood type to inventory's blood type
                                if (IsValidBloodType(inventoryBloodType))
                                {
                                    ddlInventoryBloodType.SelectedValue = inventoryBloodType;
                                }
                                else
                                {
                                    ddlInventoryBloodType.SelectedIndex = 0;
                                    ShowMessage($"Invalid blood type '{inventoryBloodType}' in inventory. Please select a valid blood type.", "danger");
                                }

                                // Check donor's blood type
                                if (!string.IsNullOrEmpty(donorBloodType) && donorBloodType.ToLower() != "unknown" && IsValidBloodType(donorBloodType))
                                {
                                    ddlInventoryBloodType.SelectedValue = donorBloodType;
                                    ddlInventoryBloodType.Enabled = false;
                                    ddlInventoryBloodType.CssClass = "form-control";
                                }
                                else
                                {
                                    ddlInventoryBloodType.Enabled = true;
                                    ddlInventoryBloodType.CssClass = "form-control";
                                }

                                txtQuantity.Text = reader["quantity_ml"].ToString();
                                txtDonationDate.Text = Convert.ToDateTime(reader["donation_date"]).ToString("yyyy-MM-dd");
                                lblExpirationDate.Text = Convert.ToDateTime(reader["expiration_date"]).ToString("yyyy-MM-dd");
                                ddlDonor.SelectedValue = reader["donor_id"] != DBNull.Value ? reader["donor_id"].ToString() : "";
                                ddlHospital.SelectedValue = reader["tested_by_hospital"].ToString();
                                ddlTestResult.SelectedValue = reader["test_result"].ToString();
                                ddlStatus.SelectedValue = reader["status"].ToString();
                                Debug.WriteLine($"[{DateTime.Now}] LoadInventoryData - Loaded inventory ID: {inventoryId}, Donor Blood Type: {donorBloodType}, BloodType Dropdown Enabled: {ddlInventoryBloodType.Enabled}");
                            }
                            else
                            {
                                ShowMessage("Inventory item not found.", "danger");
                                Debug.WriteLine($"[{DateTime.Now}] LoadInventoryData - Inventory ID: {inventoryId} not found");
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadInventoryData - MySQL Error: {ex.Message}, ErrorCode: {ex.Number}");
                ShowMessage("Error loading inventory data: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadInventoryData - Error: {ex.Message}, StackTrace: {ex.StackTrace}");
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
                    using (MySqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            DateTime donationDate = DateTime.Parse(txtDonationDate.Text);
                            DateTime expirationDate = donationDate.AddDays(42);
                            string bloodType = ddlInventoryBloodType.SelectedValue;

                            // Validate blood type matches donor's known blood type
                            if (!string.IsNullOrEmpty(ddlDonor.SelectedValue))
                            {
                                string donorBloodType = GetDonorBloodType(ddlDonor.SelectedValue);
                                if (donorBloodType != "unknown" && !string.IsNullOrEmpty(donorBloodType) && donorBloodType != bloodType)
                                {
                                    ShowMessage($"Selected blood type ({bloodType}) does not match donor's known blood type ({donorBloodType}).", "danger");
                                    Debug.WriteLine($"[{DateTime.Now}] btnSaveInventory_Click - Blood type mismatch: Selected {bloodType}, Donor {donorBloodType}");
                                    return;
                                }
                            }

                            if (string.IsNullOrEmpty(hdnInventoryId.Value)) // New inventory
                            {
                                string query = @"INSERT INTO blood_inventory (blood_type, quantity_ml, donation_date, expiration_date, 
                                                donor_id, tested_by_hospital, test_result, status, created_at)
                                                VALUES (@bloodType, @quantity, @donationDate, @expirationDate, @donorId, @hospitalId, 
                                                @testResult, @status, CURRENT_TIMESTAMP)";
                                using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@bloodType", bloodType);
                                    cmd.Parameters.AddWithValue("@quantity", txtQuantity.Text);
                                    cmd.Parameters.AddWithValue("@donationDate", donationDate);
                                    cmd.Parameters.AddWithValue("@expirationDate", expirationDate);
                                    cmd.Parameters.AddWithValue("@donorId", string.IsNullOrEmpty(ddlDonor.SelectedValue) ? (object)DBNull.Value : ddlDonor.SelectedValue);
                                    cmd.Parameters.AddWithValue("@hospitalId", ddlHospital.SelectedValue);
                                    cmd.Parameters.AddWithValue("@testResult", ddlTestResult.SelectedValue);
                                    cmd.Parameters.AddWithValue("@status", ddlStatus.SelectedValue);
                                    cmd.ExecuteNonQuery();
                                }

                                // Update donor's blood type if it was unknown
                                if (!string.IsNullOrEmpty(ddlDonor.SelectedValue) && GetDonorBloodType(ddlDonor.SelectedValue) == "unknown")
                                {
                                    string updateDonorQuery = @"UPDATE donors SET blood_type = @bloodType WHERE donor_id = @donorId";
                                    using (MySqlCommand cmd = new MySqlCommand(updateDonorQuery, conn, transaction))
                                    {
                                        cmd.Parameters.AddWithValue("@bloodType", bloodType);
                                        cmd.Parameters.AddWithValue("@donorId", ddlDonor.SelectedValue);
                                        cmd.ExecuteNonQuery();
                                    }
                                    AddNotification(Convert.ToInt32(Session["AdminId"]), "Donor Blood Type Updated",
                                        $"Donor ID {ddlDonor.SelectedValue} blood type updated to {bloodType}", conn, transaction);
                                }

                                AddNotification(Convert.ToInt32(Session["AdminId"]), "Inventory Added",
                                    $"New blood inventory added: {txtQuantity.Text}ml of {bloodType}", conn, transaction);
                                transaction.Commit();
                                ShowMessage("Inventory added successfully.", "success");
                                Debug.WriteLine($"[{DateTime.Now}] btnSaveInventory_Click - Added inventory: {txtQuantity.Text}ml of {bloodType} for admin ID: {Session["AdminId"]}");
                            }
                            else // Update existing inventory
                            {
                                string query = @"UPDATE blood_inventory SET blood_type = @bloodType, quantity_ml = @quantity, 
                                                donation_date = @donationDate, expiration_date = @expirationDate, 
                                                donor_id = @donorId, tested_by_hospital = @hospitalId, 
                                                test_result = @testResult, status = @status
                                                WHERE inventory_id = @inventoryId";
                                using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@bloodType", bloodType);
                                    cmd.Parameters.AddWithValue("@quantity", txtQuantity.Text);
                                    cmd.Parameters.AddWithValue("@donationDate", donationDate);
                                    cmd.Parameters.AddWithValue("@expirationDate", expirationDate);
                                    cmd.Parameters.AddWithValue("@donorId", string.IsNullOrEmpty(ddlDonor.SelectedValue) ? (object)DBNull.Value : ddlDonor.SelectedValue);
                                    cmd.Parameters.AddWithValue("@hospitalId", ddlHospital.SelectedValue);
                                    cmd.Parameters.AddWithValue("@testResult", ddlTestResult.SelectedValue);
                                    cmd.Parameters.AddWithValue("@status", ddlStatus.SelectedValue);
                                    cmd.Parameters.AddWithValue("@inventoryId", hdnInventoryId.Value);
                                    int rowsAffected = cmd.ExecuteNonQuery();
                                    if (rowsAffected == 0)
                                    {
                                        ShowMessage("Inventory item not found.", "danger");
                                        Debug.WriteLine($"[{DateTime.Now}] btnSaveInventory_Click - No rows affected for inventory ID: {hdnInventoryId.Value}");
                                        transaction.Rollback();
                                        return;
                                    }
                                }

                                // Update donor's blood type if it was unknown
                                if (!string.IsNullOrEmpty(ddlDonor.SelectedValue) && GetDonorBloodType(ddlDonor.SelectedValue) == "unknown")
                                {
                                    string updateDonorQuery = @"UPDATE donors SET blood_type = @bloodType WHERE donor_id = @donorId";
                                    using (MySqlCommand cmd = new MySqlCommand(updateDonorQuery, conn, transaction))
                                    {
                                        cmd.Parameters.AddWithValue("@bloodType", bloodType);
                                        cmd.Parameters.AddWithValue("@donorId", ddlDonor.SelectedValue);
                                        cmd.ExecuteNonQuery();
                                    }
                                    AddNotification(Convert.ToInt32(Session["AdminId"]), "Donor Blood Type Updated",
                                        $"Donor ID {ddlDonor.SelectedValue} blood type updated to {bloodType}", conn, transaction);
                                }

                                AddNotification(Convert.ToInt32(Session["AdminId"]), "Inventory Updated",
                                    $"Blood inventory updated: {txtQuantity.Text}ml of {bloodType}", conn, transaction);
                                transaction.Commit();
                                ShowMessage("Inventory updated successfully.", "success");
                                Debug.WriteLine($"[{DateTime.Now}] btnSaveInventory_Click - Updated inventory ID: {hdnInventoryId.Value} for admin ID: {Session["AdminId"]}");
                            }

                            LoadInventory();
                            ClearForm();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnSaveInventory_Click - MySQL Error: {ex.Message}, ErrorCode: {ex.Number}");
                ShowMessage("Error saving inventory: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnSaveInventory_Click - Error: {ex.Message}, StackTrace: {ex.StackTrace}");
                ShowMessage("Error saving inventory: " + ex.Message, "danger");
            }
        }

        protected void btnClearForm_Click(object sender, EventArgs e)
        {
            ClearForm();
            LoadInventory();
        }

        private void ClearForm()
        {
            hdnInventoryId.Value = "";
            ddlInventoryBloodType.SelectedIndex = 0;
            ddlInventoryBloodType.Enabled = true;
            ddlInventoryBloodType.CssClass = "form-control";
            txtQuantity.Text = "";
            txtDonationDate.Text = "";
            lblExpirationDate.Text = "";
            ddlDonor.SelectedIndex = 0;
            ddlHospital.SelectedIndex = 0;
            ddlTestResult.SelectedIndex = 0;
            ddlStatus.SelectedIndex = 0;
            Debug.WriteLine($"[{DateTime.Now}] ClearForm - Form cleared");
        }

        private bool ValidateInventoryForm()
        {
            if (string.IsNullOrEmpty(ddlInventoryBloodType.SelectedValue) || string.IsNullOrEmpty(txtQuantity.Text) ||
                string.IsNullOrEmpty(txtDonationDate.Text) || string.IsNullOrEmpty(ddlHospital.SelectedValue) ||
                string.IsNullOrEmpty(ddlTestResult.SelectedValue) || string.IsNullOrEmpty(ddlStatus.SelectedValue))
            {
                ShowMessage("All required fields must be filled.", "danger");
                Debug.WriteLine($"[{DateTime.Now}] ValidateInventoryForm - Validation failed: Missing required fields");
                return false;
            }

            try
            {
                int quantity = int.Parse(txtQuantity.Text);
                if (quantity <= 0)
                {
                    ShowMessage("Quantity must be greater than 0.", "danger");
                    Debug.WriteLine($"[{DateTime.Now}] ValidateInventoryForm - Validation failed: Invalid quantity {txtQuantity.Text}");
                    return false;
                }

                DateTime donationDate = DateTime.Parse(txtDonationDate.Text);
                if (donationDate > DateTime.Now)
                {
                    ShowMessage("Donation date cannot be in the future.", "danger");
                    Debug.WriteLine($"[{DateTime.Now}] ValidateInventoryForm - Validation failed: Future donation date {txtDonationDate.Text}");
                    return false;
                }
            }
            catch (FormatException)
            {
                ShowMessage("Invalid quantity or date format.", "danger");
                Debug.WriteLine($"[{DateTime.Now}] ValidateInventoryForm - Validation failed: Invalid format for quantity or date");
                return false;
            }

            return true;
        }

        private void AddNotification(int adminId, string title, string message)
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
                        cmd.Parameters.AddWithValue("@adminId", adminId);
                        cmd.Parameters.AddWithValue("@title", title);
                        cmd.Parameters.AddWithValue("@message", message);
                        cmd.ExecuteNonQuery();
                        Debug.WriteLine($"[{DateTime.Now}] AddNotification - Added notification: {title}, {message} for admin ID: {adminId}");
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] AddNotification - MySQL Error: {ex.Message}, ErrorCode: {ex.Number}");
                ShowMessage("Error adding notification: " + ex.Message, "danger");
            }
        }

        private void AddNotification(int adminId, string title, string message, MySqlConnection conn, MySqlTransaction transaction)
        {
            try
            {
                string query = @"INSERT INTO notifications (admin_id, title, message, is_read, created_at) 
                                VALUES (@adminId, @title, @message, 0, CURRENT_TIMESTAMP)";
                using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@adminId", adminId);
                    cmd.Parameters.AddWithValue("@title", title);
                    cmd.Parameters.AddWithValue("@message", message);
                    cmd.ExecuteNonQuery();
                    Debug.WriteLine($"[{DateTime.Now}] AddNotification - Added notification: {title}, {message} for admin ID: {adminId}");
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] AddNotification - MySQL Error: {ex.Message}, ErrorCode: {ex.Number}");
                ShowMessage("Error adding notification: " + ex.Message, "danger");
                throw;
            }
        }
        protected void lnkLogout_Click(object sender, EventArgs e)
        {
            try
            {
                Session.Clear();
                Session.Abandon();
                Response.Redirect("Login.aspx");
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] lnkLogout_Click - MySQL Error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                ShowMessage("Error logging out: " + ex.Message, "danger");
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