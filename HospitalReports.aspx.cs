using System;
using System.Configuration;
using System.Data;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Web.UI.WebControls;

namespace ClinicalBloodBank
{
    public partial class HospitalReports : System.Web.UI.Page
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
                    // Set default date range (e.g., last 30 days)
                    txtStartDate.Text = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
                    txtEndDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                    GenerateReport();
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

        protected void btnGenerateReport_Click(object sender, EventArgs e)
        {
            try
            {
                GenerateReport();
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnGenerateReport_Click - MySQL Error: {ex.Message}, ErrorCode: {ex.Number}");
                ShowMessage("Error generating report: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnGenerateReport_Click - Error: {ex.Message}");
                ShowMessage("Error generating report: " + ex.Message, "danger");
            }
        }

        protected void btnClearFilters_Click(object sender, EventArgs e)
        {
            try
            {
                ddlReportType.SelectedValue = "blood_requests";
                ddlBloodType.SelectedValue = "";
                txtStartDate.Text = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
                txtEndDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                ddlStatus.SelectedValue = "";
                GenerateReport();
                Debug.WriteLine($"[{DateTime.Now}] btnClearFilters_Click - Filters cleared for hospital ID: {Session["UserId"]}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnClearFilters_Click - Error: {ex.Message}");
                ShowMessage("Error clearing filters: " + ex.Message, "danger");
            }
        }

        protected void gvReport_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                gvReport.PageIndex = e.NewPageIndex;
                GenerateReport();
                Debug.WriteLine($"[{DateTime.Now}] gvReport_PageIndexChanging - Changed to page: {e.NewPageIndex}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] gvReport_PageIndexChanging - Error: {ex.Message}");
                ShowMessage("Error changing page: " + ex.Message, "danger");
            }
        }

        private void GenerateReport()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "";
                    string reportType = ddlReportType.SelectedValue;

                    // Build query based on report type
                    if (reportType == "blood_requests")
                    {
                        query = @"SELECT request_id AS 'Request ID', blood_type AS 'Blood Type', quantity_ml AS 'Quantity (ml)', 
                                 urgency AS Urgency, status AS Status, reason AS Reason, requested_at AS 'Requested At', 
                                 fulfilled_at AS 'Fulfilled At' 
                                 FROM blood_requests 
                                 WHERE requester_role = 'hospital' AND requester_id = @hospitalId";
                        if (!string.IsNullOrEmpty(ddlBloodType.SelectedValue))
                            query += " AND blood_type = @bloodType";
                        if (!string.IsNullOrEmpty(ddlStatus.SelectedValue))
                            query += " AND status = @status";
                        if (!string.IsNullOrEmpty(txtStartDate.Text))
                            query += " AND requested_at >= @startDate";
                        if (!string.IsNullOrEmpty(txtEndDate.Text))
                            query += " AND requested_at <= @endDate";
                    }
                    else if (reportType == "inventory")
                    {
                        query = @"SELECT inventory_id AS 'Inventory ID', blood_type AS 'Blood Type', quantity_ml AS 'Quantity (ml)', 
                                 donation_date AS 'Donation Date', expiration_date AS 'Expiration Date', test_result AS 'Test Result', 
                                 status AS Status 
                                 FROM blood_inventory 
                                 WHERE tested_by_hospital = @hospitalId";
                        if (!string.IsNullOrEmpty(ddlBloodType.SelectedValue))
                            query += " AND blood_type = @bloodType";
                        if (!string.IsNullOrEmpty(ddlStatus.SelectedValue))
                            query += " AND status = @status";
                        if (!string.IsNullOrEmpty(txtStartDate.Text))
                            query += " AND donation_date >= @startDate";
                        if (!string.IsNullOrEmpty(txtEndDate.Text))
                            query += " AND donation_date <= @endDate";
                    }
                    else if (reportType == "appointments")
                    {
                        query = @"SELECT appointment_id AS 'Appointment ID', appointment_date AS 'Appointment Date', 
                                 status AS Status, notes AS Notes 
                                 FROM donation_appointments 
                                 WHERE hospital_id = @hospitalId";
                        if (!string.IsNullOrEmpty(ddlStatus.SelectedValue))
                            query += " AND status = @status";
                        if (!string.IsNullOrEmpty(txtStartDate.Text))
                            query += " AND appointment_date >= @startDate";
                        if (!string.IsNullOrEmpty(txtEndDate.Text))
                            query += " AND appointment_date <= @endDate";
                    }

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                        if (!string.IsNullOrEmpty(ddlBloodType.SelectedValue))
                            cmd.Parameters.AddWithValue("@bloodType", ddlBloodType.SelectedValue);
                        if (!string.IsNullOrEmpty(ddlStatus.SelectedValue))
                            cmd.Parameters.AddWithValue("@status", ddlStatus.SelectedValue);
                        if (!string.IsNullOrEmpty(txtStartDate.Text))
                            cmd.Parameters.AddWithValue("@startDate", DateTime.Parse(txtStartDate.Text));
                        if (!string.IsNullOrEmpty(txtEndDate.Text))
                            cmd.Parameters.AddWithValue("@endDate", DateTime.Parse(txtEndDate.Text).AddDays(1));

                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            gvReport.DataSource = dt;
                            gvReport.DataBind();
                            Debug.WriteLine($"[{DateTime.Now}] GenerateReport - Generated {reportType} report with {dt.Rows.Count} rows for hospital ID: {Session["UserId"]}");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] GenerateReport - MySQL Error: {ex.Message}, ErrorCode: {ex.Number}");
                ShowMessage("Error generating report: " + ex.Message, "danger");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] GenerateReport - Error: {ex.Message}");
                ShowMessage("Error generating report: " + ex.Message, "danger");
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