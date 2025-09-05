
using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ClinicalBloodBank
{
    public partial class ManageInventory : System.Web.UI.Page
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
                    LoadInventory();
                    LoadDonorDropdown();
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

        private void LoadInventory()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT i.inventory_id, i.blood_type, i.quantity_ml, i.donation_date, i.expiration_date, 
                                    i.test_result, i.status, 
                                    CONCAT(u.first_name, ' ', u.last_name) as donor_name,
                                    h.hospital_name
                                    FROM blood_inventory i
                                    INNER JOIN donors d ON i.donor_id = d.donor_id
                                    INNER JOIN users u ON d.user_id = u.user_id
                                    INNER JOIN hospitals h ON i.tested_by_hospital = h.hospital_id";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        gvInventory.DataSource = dt;
                        gvInventory.DataBind();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("LoadInventory Error: " + ex.Message);
                    ShowErrorMessage("Error loading inventory.");
                }
            }
        }

        private void LoadDonorDropdown()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT d.donor_id, CONCAT(u.first_name, ' ', u.last_name) as donor_name
                                    FROM donors d
                                    INNER JOIN users u ON d.user_id = u.user_id
                                    WHERE u.is_active = 1";
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
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("LoadDonorDropdown Error: " + ex.Message);
                    ShowErrorMessage("Error loading donor dropdown.");
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
                        ddlHospital.DataSource = dt;
                        ddlHospital.DataTextField = "hospital_name";
                        ddlHospital.DataValueField = "hospital_id";
                        ddlHospital.DataBind();
                        ddlHospital.Items.Insert(0, new ListItem("Select Hospital", ""));
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("LoadHospitalDropdown Error: " + ex.Message);
                    ShowErrorMessage("Error loading hospital dropdown.");
                }
            }
        }

        protected void gvInventory_RowEditing(object sender, GridViewEditEventArgs e)
        {
            try
            {
                string inventoryId = gvInventory.DataKeys[e.NewEditIndex].Value.ToString();
                LoadInventoryData(inventoryId);
                e.Cancel = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("gvInventory_RowEditing Error: " + ex.Message);
                ShowErrorMessage("Error editing inventory.");
            }
        }

        protected void gvInventory_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string inventoryId = gvInventory.DataKeys[e.RowIndex].Value.ToString();
                    string query = "DELETE FROM blood_inventory WHERE inventory_id = @inventoryId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@inventoryId", inventoryId);
                        cmd.ExecuteNonQuery();
                    }

                    LoadInventory();
                    AddNotification("Inventory Deleted", $"Blood inventory item deleted: ID {inventoryId}");
                    ShowSuccessMessage("Inventory deleted successfully.");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("gvInventory_RowDeleting Error: " + ex.Message);
                    ShowErrorMessage("Error deleting inventory.");
                }
            }
        }

        private void LoadInventoryData(string inventoryId)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
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
                                txtExpirationDate.Text = Convert.ToDateTime(reader["expiration_date"]).ToString("yyyy-MM-dd");
                                ddlDonor.SelectedValue = reader["donor_id"].ToString();
                                ddlHospital.SelectedValue = reader["tested_by_hospital"].ToString();
                                ddlTestResult.SelectedValue = reader["test_result"].ToString();
                                ddlStatus.SelectedValue = reader["status"].ToString();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("LoadInventoryData Error: " + ex.Message);
                    ShowErrorMessage("Error loading inventory data.");
                }
            }
        }

        protected void btnSaveInventory_Click(object sender, EventArgs e)
        {
            if (!ValidateInventoryForm()) return;

            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    if (string.IsNullOrEmpty(hdnInventoryId.Value)) // New inventory
                    {
                        string query = @"INSERT INTO blood_inventory (blood_type, quantity_ml, donation_date, expiration_date, 
                                      donor_id, tested_by_hospital, test_result, status, created_at)
                                      VALUES (@bloodType, @quantity, @donationDate, @expirationDate, @donorId, @hospitalId, 
                                      @testResult, @status, NOW())";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@bloodType", ddlInventoryBloodType.SelectedValue);
                            cmd.Parameters.AddWithValue("@quantity", txtQuantity.Text);
                            cmd.Parameters.AddWithValue("@donationDate", txtDonationDate.Text);
                            cmd.Parameters.AddWithValue("@expirationDate", txtExpirationDate.Text);
                            cmd.Parameters.AddWithValue("@donorId", ddlDonor.SelectedValue);
                            cmd.Parameters.AddWithValue("@hospitalId", ddlHospital.SelectedValue);
                            cmd.Parameters.AddWithValue("@testResult", ddlTestResult.SelectedValue);
                            cmd.Parameters.AddWithValue("@status", ddlStatus.SelectedValue);
                            cmd.ExecuteNonQuery();
                        }

                        AddNotification("Inventory Added", $"New blood inventory added: {txtQuantity.Text}ml of {ddlInventoryBloodType.SelectedValue}");
                        ShowSuccessMessage("Inventory added successfully.");
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
                            cmd.Parameters.AddWithValue("@donationDate", txtDonationDate.Text);
                            cmd.Parameters.AddWithValue("@expirationDate", txtExpirationDate.Text);
                            cmd.Parameters.AddWithValue("@donorId", ddlDonor.SelectedValue);
                            cmd.Parameters.AddWithValue("@hospitalId", ddlHospital.SelectedValue);
                            cmd.Parameters.AddWithValue("@testResult", ddlTestResult.SelectedValue);
                            cmd.Parameters.AddWithValue("@status", ddlStatus.SelectedValue);
                            cmd.Parameters.AddWithValue("@inventoryId", hdnInventoryId.Value);
                            cmd.ExecuteNonQuery();
                        }

                        AddNotification("Inventory Updated", $"Blood inventory updated: {txtQuantity.Text}ml of {ddlInventoryBloodType.SelectedValue}");
                        ShowSuccessMessage("Inventory updated successfully.");
                    }

                    LoadInventory();
                    ClearForm();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("btnSaveInventory_Click Error: " + ex.Message);
                    ShowErrorMessage("Error saving inventory.");
                }
            }
        }

        protected void btnClearForm_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            hdnInventoryId.Value = "";
            ddlInventoryBloodType.SelectedIndex = 0;
            txtQuantity.Text = "";
            txtDonationDate.Text = "";
            txtExpirationDate.Text = "";
            ddlDonor.SelectedIndex = 0;
            ddlHospital.SelectedIndex = 0;
            ddlTestResult.SelectedIndex = 0;
            ddlStatus.SelectedIndex = 0;
        }

        private bool ValidateInventoryForm()
        {
            if (string.IsNullOrEmpty(ddlInventoryBloodType.SelectedValue) || string.IsNullOrEmpty(txtQuantity.Text) ||
                string.IsNullOrEmpty(txtDonationDate.Text) || string.IsNullOrEmpty(txtExpirationDate.Text) ||
                string.IsNullOrEmpty(ddlDonor.SelectedValue) || string.IsNullOrEmpty(ddlHospital.SelectedValue) ||
                string.IsNullOrEmpty(ddlTestResult.SelectedValue) || string.IsNullOrEmpty(ddlStatus.SelectedValue))
            {
                ShowErrorMessage("All fields are required.");
                return false;
            }
            return true;
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
