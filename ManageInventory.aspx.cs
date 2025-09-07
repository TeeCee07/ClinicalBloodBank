
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
    public partial class ManageInventory : System.Web.UI.Page
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

                if (!IsPostBack)
                {
                    LoadInventory();
                    LoadDonorDropdown();
                    LoadHospitalDropdown();
                    LoadBloodTypeDropdown();
                    LoadProvinceDropdown();
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

        private void LoadInventory()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT i.inventory_id, i.blood_type, i.quantity_ml, i.donation_date, i.expiration_date, 
                                    i.test_result, i.status, 
                                    CONCAT(d.first_name, ' ', d.last_name) as donor_name,
                                    h.hospital_name, h.province
                                    FROM blood_inventory i
                                    INNER JOIN donors d ON i.donor_id = d.donor_id
                                    INNER JOIN hospitals h ON i.tested_by_hospital = h.hospital_id
                                    WHERE 1=1";

                    // Apply filters
                    string bloodType = ddlFilterBloodType.SelectedValue;
                    string expirationDate = txtFilterExpirationDate.Text;
                    string province = ddlFilterProvince.SelectedValue;

                    if (!string.IsNullOrEmpty(bloodType))
                        query += " AND i.blood_type = @BloodType";
                    if (!string.IsNullOrEmpty(expirationDate))
                        query += " AND i.expiration_date <= @ExpirationDate";
                    if (!string.IsNullOrEmpty(province))
                        query += " AND h.province = @Province";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        if (!string.IsNullOrEmpty(bloodType))
                            cmd.Parameters.AddWithValue("@BloodType", bloodType);
                        if (!string.IsNullOrEmpty(expirationDate))
                            cmd.Parameters.AddWithValue("@ExpirationDate", DateTime.Parse(expirationDate));
                        if (!string.IsNullOrEmpty(province))
                            cmd.Parameters.AddWithValue("@Province", province);

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
                    string query = @"SELECT donor_id, CONCAT(first_name, ' ', last_name) as donor_name
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
                        ddlHospital.DataSource = dt;
                        ddlHospital.DataTextField = "hospital_name";
                        ddlHospital.DataValueField = "hospital_id";
                        ddlHospital.DataBind();
                        ddlHospital.Items.Insert(0, new ListItem("Select Hospital", ""));
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
                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        ddlFilterProvince.DataSource = dt;
                        ddlFilterProvince.DataTextField = "province";
                        ddlFilterProvince.DataValueField = "province";
                        ddlFilterProvince.DataBind();
                        ddlFilterProvince.Items.Insert(0, new ListItem("All Provinces", ""));
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadProvinceDropdown - MySQL Error: {ex.Message}");
                ShowMessage("Error loading province dropdown: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] LoadProvinceDropdown - Error: {ex.Message}");
                ShowMessage("Error loading province dropdown: " + ex.Message, "danger");
            }
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
                    string query = "DELETE FROM blood_inventory WHERE inventory_id = @inventoryId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@inventoryId", inventoryId);
                        cmd.ExecuteNonQuery();
                    }

                    AddNotification(Convert.ToInt32(Session["AdminId"]), "Inventory Deleted", $"Blood inventory item deleted: ID {inventoryId}");
                    ShowMessage("Inventory deleted successfully.", "success");
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
            gvInventory.PageIndex = e.NewPageIndex;
            LoadInventory();
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
                    string query = @"SELECT inventory_id, blood_type, quantity_ml, donation_date, expiration_date, 
                                    donor_id, tested_by_hospital, test_result, status
                                    FROM blood_inventory 
                                    WHERE inventory_id = @inventoryId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@inventoryId", inventoryId);
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
                                ddlHospital.SelectedValue = reader["tested_by_hospital"].ToString();
                                ddlTestResult.SelectedValue = reader["test_result"].ToString();
                                ddlStatus.SelectedValue = reader["status"].ToString();
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
                    DateTime expirationDate = donationDate.AddDays(42); // Blood expires after 42 days

                    if (string.IsNullOrEmpty(hdnInventoryId.Value)) // New inventory
                    {
                        string query = @"INSERT INTO blood_inventory (blood_type, quantity_ml, donation_date, expiration_date, 
                                      donor_id, tested_by_hospital, test_result, status, created_at)
                                      VALUES (@bloodType, @quantity, @donationDate, @expirationDate, @donorId, @hospitalId, 
                                      @testResult, @status, CURRENT_TIMESTAMP)";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@bloodType", ddlInventoryBloodType.SelectedValue);
                            cmd.Parameters.AddWithValue("@quantity", txtQuantity.Text);
                            cmd.Parameters.AddWithValue("@donationDate", donationDate);
                            cmd.Parameters.AddWithValue("@expirationDate", expirationDate);
                            cmd.Parameters.AddWithValue("@donorId", ddlDonor.SelectedValue);
                            cmd.Parameters.AddWithValue("@hospitalId", ddlHospital.SelectedValue);
                            cmd.Parameters.AddWithValue("@testResult", ddlTestResult.SelectedValue);
                            cmd.Parameters.AddWithValue("@status", ddlStatus.SelectedValue);
                            cmd.ExecuteNonQuery();
                        }

                        AddNotification(Convert.ToInt32(Session["AdminId"]), "Inventory Added",
                            $"New blood inventory added: {txtQuantity.Text}ml of {ddlInventoryBloodType.SelectedValue}");
                        ShowMessage("Inventory added successfully.", "success");
                    }
                    else // Update existing inventory
                    {
                        string query = @"UPDATE blood_inventory SET blood_type = @bloodType, quantity_ml = @quantity, 
                                      donation_date = @donationDate, expiration_date = @expirationDate, 
                                      donor_id = @donorId, tested_by_hospital = @hospitalId, 
                                      test_result = @testResult, status = @status
                                      WHERE inventory_id = @inventoryId";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@bloodType", ddlInventoryBloodType.SelectedValue);
                            cmd.Parameters.AddWithValue("@quantity", txtQuantity.Text);
                            cmd.Parameters.AddWithValue("@donationDate", donationDate);
                            cmd.Parameters.AddWithValue("@expirationDate", expirationDate);
                            cmd.Parameters.AddWithValue("@donorId", ddlDonor.SelectedValue);
                            cmd.Parameters.AddWithValue("@hospitalId", ddlHospital.SelectedValue);
                            cmd.Parameters.AddWithValue("@testResult", ddlTestResult.SelectedValue);
                            cmd.Parameters.AddWithValue("@status", ddlStatus.SelectedValue);
                            cmd.Parameters.AddWithValue("@inventoryId", hdnInventoryId.Value);
                            cmd.ExecuteNonQuery();
                        }

                        AddNotification(Convert.ToInt32(Session["AdminId"]), "Inventory Updated",
                            $"Blood inventory updated: {txtQuantity.Text}ml of {ddlInventoryBloodType.SelectedValue}");
                        ShowMessage("Inventory updated successfully.", "success");
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
            ClearForm();
            LoadInventory();
        }

        private void ClearForm()
        {
            hdnInventoryId.Value = "";
            ddlInventoryBloodType.SelectedIndex = 0;
            txtQuantity.Text = "";
            txtDonationDate.Text = "";
            lblExpirationDate.Text = "";
            ddlDonor.SelectedIndex = 0;
            ddlHospital.SelectedIndex = 0;
            ddlTestResult.SelectedIndex = 0;
            ddlStatus.SelectedIndex = 0;
        }

        private bool ValidateInventoryForm()
        {
            if (string.IsNullOrEmpty(ddlInventoryBloodType.SelectedValue) || string.IsNullOrEmpty(txtQuantity.Text) ||
                string.IsNullOrEmpty(txtDonationDate.Text) || string.IsNullOrEmpty(ddlDonor.SelectedValue) ||
                string.IsNullOrEmpty(ddlHospital.SelectedValue) || string.IsNullOrEmpty(ddlTestResult.SelectedValue) ||
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
        protected void lnkLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("Login.aspx");
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