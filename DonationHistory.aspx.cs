using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ClinicalBloodBank
{
    public partial class DonationHistory : System.Web.UI.Page
    {
        private int currentPage = 1;
        private const int PageSize = 10;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Validate session and authentication
                if (Session["UserId"] == null || Session["UserType"] == null || Session["UserType"].ToString() != "donor")
                {
                    Response.Redirect("Login.aspx");
                    return;
                }

                try
                {
                    LoadDashboardStats();
                    LoadYearFilter();
                    LoadDonations();
                    LoadNotificationCount();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Page_Load Error: " + ex.Message);
                    errorMessage.InnerText = "An error occurred while loading donation history.";
                    errorMessage.Style["display"] = "block";
                }
            }
        }

        private void LoadDashboardStats()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"]?.ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                errorMessage.InnerText = "Database connection configuration is missing.";
                errorMessage.Style["display"] = "block";
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Total Donations
                    string donationsQuery = "SELECT total_donations FROM donors WHERE donor_id = @donorId";
                    using (MySqlCommand cmd = new MySqlCommand(donationsQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@donorId", Session["UserId"]);
                        litTotalDonations.Text = cmd.ExecuteScalar()?.ToString() ?? "0";
                    }

                    // Last Donation Date
                    string lastDonationQuery = @"SELECT MAX(donation_date) 
                                         FROM blood_inventory 
                                         WHERE donor_id = @donorId";
                    using (MySqlCommand cmd = new MySqlCommand(lastDonationQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@donorId", Session["UserId"]);
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            DateTime lastDonation = Convert.ToDateTime(result);
                            litLastDonation.Text = lastDonation.ToString("yyyy-MM-dd");
                        }
                        else
                        {
                            litLastDonation.Text = "Never";
                        }
                    }

                    // Total Blood Donated
                    string totalBloodQuery = @"SELECT COALESCE(SUM(quantity_ml), 0) 
                                       FROM blood_inventory 
                                       WHERE donor_id = @donorId";
                    using (MySqlCommand cmd = new MySqlCommand(totalBloodQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@donorId", Session["UserId"]);
                        litTotalBlood.Text = cmd.ExecuteScalar()?.ToString() ?? "0";
                    }

                    // Next Eligibility Date
                    string eligibilityQuery = @"SELECT DATE_ADD(COALESCE(MAX(donation_date), CURDATE()), INTERVAL 56 DAY) as next_donation_date
                                        FROM blood_inventory 
                                        WHERE donor_id = @donorId AND status = 'available'";
                    using (MySqlCommand cmd = new MySqlCommand(eligibilityQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@donorId", Session["UserId"]);
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            DateTime nextDate = Convert.ToDateTime(result);
                            if (nextDate <= DateTime.Now)
                            {
                                litNextEligibility.Text = "Now";
                            }
                            else
                            {
                                litNextEligibility.Text = nextDate.ToString("yyyy-MM-dd");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("LoadDashboardStats Error: " + ex.Message);
                    errorMessage.InnerText = "Error loading dashboard statistics.";
                    errorMessage.Style["display"] = "block";
                }
            }
        }

        private void LoadYearFilter()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"]?.ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT DISTINCT YEAR(donation_date) as year 
                             FROM blood_inventory 
                             WHERE donor_id = @donorId 
                             ORDER BY year DESC";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@donorId", Session["UserId"]);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            ddlYearFilter.Items.Clear();
                            ddlYearFilter.Items.Add(new ListItem("All Years", ""));

                            while (reader.Read())
                            {
                                int year = Convert.ToInt32(reader["year"]);
                                ddlYearFilter.Items.Add(new ListItem(year.ToString(), year.ToString()));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("LoadYearFilter Error: " + ex.Message);
                }
            }
        }

        private void LoadDonations()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"]?.ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                lblNoDonations.Visible = true;
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Build the base query
                    string query = @"SELECT SQL_CALC_FOUND_ROWS i.donation_date, i.blood_type, i.quantity_ml, 
                            h.hospital_name, i.status, i.test_result
                     FROM blood_inventory i 
                     INNER JOIN hospitals h ON i.tested_by_hospital = h.hospital_id 
                     WHERE i.donor_id = @donorId";

                    // Add filters
                    if (!string.IsNullOrEmpty(ddlStatusFilter.SelectedValue))
                    {
                        query += " AND i.status = @status";
                    }

                    if (!string.IsNullOrEmpty(ddlYearFilter.SelectedValue))
                    {
                        query += " AND YEAR(i.donation_date) = @year";
                    }

                    // Add sorting and pagination
                    query += $" ORDER BY {ddlSortBy.SelectedValue} LIMIT @offset, @pageSize";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@donorId", Session["UserId"]);

                        if (!string.IsNullOrEmpty(ddlStatusFilter.SelectedValue))
                        {
                            cmd.Parameters.AddWithValue("@status", ddlStatusFilter.SelectedValue);
                        }

                        if (!string.IsNullOrEmpty(ddlYearFilter.SelectedValue))
                        {
                            cmd.Parameters.AddWithValue("@year", Convert.ToInt32(ddlYearFilter.SelectedValue));
                        }

                        cmd.Parameters.AddWithValue("@offset", (currentPage - 1) * PageSize);
                        cmd.Parameters.AddWithValue("@pageSize", PageSize);

                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        // Get total count
                        int totalRecords = 0;
                        using (MySqlCommand countCmd = new MySqlCommand("SELECT FOUND_ROWS()", conn))
                        {
                            totalRecords = Convert.ToInt32(countCmd.ExecuteScalar());
                        }

                        if (dt.Rows.Count > 0)
                        {
                            rptDonations.DataSource = dt;
                            rptDonations.DataBind();
                            lblNoDonations.Visible = false;

                            // Setup pagination
                            if (totalRecords > PageSize)
                            {
                                paginationContainer.Visible = true;
                                int totalPages = (int)Math.Ceiling((double)totalRecords / PageSize);
                                lblPageInfo.Text = $"Page {currentPage} of {totalPages}";

                                btnPrev.Enabled = (currentPage > 1);
                                btnNext.Enabled = (currentPage < totalPages);
                            }
                            else
                            {
                                paginationContainer.Visible = false;
                            }
                        }
                        else
                        {
                            rptDonations.DataSource = null;
                            rptDonations.DataBind();
                            lblNoDonations.Visible = true;
                            paginationContainer.Visible = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("LoadDonations Error: " + ex.Message);
                    lblNoDonations.Visible = true;
                    paginationContainer.Visible = false;
                }
            }
        }

        private void LoadNotificationCount()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"]?.ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                notificationCount.InnerText = "0";
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string countQuery = "SELECT COUNT(*) FROM notifications WHERE is_read = 0 AND donor_id = @donorId";
                    using (MySqlCommand cmd = new MySqlCommand(countQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@donorId", Session["UserId"]);
                        notificationCount.InnerText = cmd.ExecuteScalar()?.ToString() ?? "0";
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("LoadNotificationCount Error: " + ex.Message);
                    notificationCount.InnerText = "0";
                }
            }
        }

        protected void FilterChanged(object sender, EventArgs e)
        {
            currentPage = 1;
            LoadDonations();
        }

        protected void btnPrev_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                LoadDonations();
            }
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            currentPage++;
            LoadDonations();
        }
        protected void lnkLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("Login.aspx");
        }
        protected void btnClearAll_Click(object sender, EventArgs e)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"]?.ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                errorMessage.InnerText = "Database connection configuration is missing.";
                errorMessage.Style["display"] = "block";
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string deleteQuery = "DELETE FROM notifications WHERE donor_id = @donorId";
                    using (MySqlCommand cmd = new MySqlCommand(deleteQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@donorId", Session["UserId"]);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            notificationCount.InnerText = "0";
                            LoadNotificationCount();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("btnClearAll_Click Error: " + ex.Message);
                    errorMessage.InnerText = "Error clearing notifications.";
                    errorMessage.Style["display"] = "block";
                }
            }
        }
    }
}