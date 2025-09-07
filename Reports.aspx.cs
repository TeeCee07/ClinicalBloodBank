using System;
using System.Configuration;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using System.Text;
using System.Collections.Generic;

namespace ClinicalBloodBank
{
    public partial class Reports : System.Web.UI.Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["AdminId"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadUserDetails();
                UpdateStatusDropdown();
            }
        }

        private void LoadUserDetails()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT first_name, last_name FROM admins WHERE admin_id = @adminId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@adminId", Session["AdminId"]);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string fullName = $"{reader["first_name"]} {reader["last_name"]}";
                                litUserName.Text = fullName;
                                litUserInitials.Text = GetInitials(fullName);
                            }
                            else
                            {
                                litUserName.Text = Session["AdminName"]?.ToString() ?? "Administrator";
                                litUserInitials.Text = GetInitials(Session["AdminName"]?.ToString() ?? "Administrator");
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                ShowMessage("Error loading user details: " + ex.Message, "danger");
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

        protected void ddlReportType_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateStatusDropdown();
            reportResults.Visible = false;
        }

        private void UpdateStatusDropdown()
        {
            ddlStatus.Items.Clear();
            ddlStatus.Items.Add(new ListItem("All Statuses", ""));
            switch (ddlReportType.SelectedValue)
            {
                case "donors":
                    ddlStatus.Items.Add(new ListItem("Active", "1"));
                    ddlStatus.Items.Add(new ListItem("Inactive", "0"));
                    break;
                case "inventory":
                    ddlStatus.Items.Add(new ListItem("Available", "available"));
                    ddlStatus.Items.Add(new ListItem("Expired", "expired"));
                    break;
                case "requests":
                    ddlStatus.Items.Add(new ListItem("Pending", "pending"));
                    ddlStatus.Items.Add(new ListItem("Approved", "approved"));
                    ddlStatus.Items.Add(new ListItem("Rejected", "rejected"));
                    ddlStatus.Items.Add(new ListItem("Fulfilled", "fulfilled"));
                    break;
                case "rewards":
                    ddlStatus.Items.Add(new ListItem("Active", "1"));
                    ddlStatus.Items.Add(new ListItem("Inactive", "0"));
                    break;
            }
        }

        protected void btnGenerateReport_Click(object sender, EventArgs e)
        {
            try
            {
                string reportType = ddlReportType.SelectedValue;
                if (string.IsNullOrEmpty(reportType))
                {
                    ShowMessage("Please select a report type.", "danger");
                    return;
                }

                DateTime? startDate = string.IsNullOrEmpty(txtStartDate.Text) ? null : (DateTime?)DateTime.Parse(txtStartDate.Text);
                DateTime? endDate = string.IsNullOrEmpty(txtEndDate.Text) ? null : (DateTime?)DateTime.Parse(txtEndDate.Text);
                string bloodType = ddlBloodType.SelectedValue;
                string status = ddlStatus.SelectedValue;

                DataTable reportData = GenerateReportData(reportType, startDate, endDate, bloodType, status);
                BindReportGrid(reportData, reportType);
                RenderChart(reportData, reportType);

                LogNotification($"Generated {reportType} report");
                ShowMessage("Report generated successfully.", "success");
                reportResults.Visible = true;
            }
            catch (Exception ex)
            {
                ShowMessage("Error generating report: " + ex.Message, "danger");
            }
        }

        private DataTable GenerateReportData(string reportType, DateTime? startDate, DateTime? endDate, string bloodType, string status)
        {
            DataTable dt = new DataTable();
            string query = "";
            string dateColumn = "";
            List<MySqlParameter> parameters = new List<MySqlParameter>();

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                switch (reportType)
                {
                    case "donors":
                        query = @"SELECT CONCAT(first_name, ' ', last_name) AS DonorName, email, registration_date, is_active 
                                 FROM donors WHERE 1=1";
                        dateColumn = "registration_date";
                        if (!string.IsNullOrEmpty(status))
                        {
                            query += " AND is_active = @status";
                            parameters.Add(new MySqlParameter("@status", status == "1"));
                        }
                        break;
                    case "inventory":
                        query = @"SELECT blood_type, quantity_ml, donation_date, expiration_date, status 
                                 FROM blood_inventory WHERE 1=1";
                        dateColumn = "donation_date";
                        if (!string.IsNullOrEmpty(bloodType))
                        {
                            query += " AND blood_type = @bloodType";
                            parameters.Add(new MySqlParameter("@bloodType", bloodType));
                        }
                        if (!string.IsNullOrEmpty(status))
                        {
                            query += " AND status = @status";
                            parameters.Add(new MySqlParameter("@status", status));
                        }
                        break;
                    case "requests":
                        query = @"SELECT blood_type, quantity_ml, urgency, reason, requested_at, status 
                                 FROM blood_requests WHERE 1=1";
                        dateColumn = "requested_at";
                        if (!string.IsNullOrEmpty(bloodType))
                        {
                            query += " AND blood_type = @bloodType";
                            parameters.Add(new MySqlParameter("@bloodType", bloodType));
                        }
                        if (!string.IsNullOrEmpty(status))
                        {
                            query += " AND status = @status";
                            parameters.Add(new MySqlParameter("@status", status));
                        }
                        break;
                    case "rewards":
                        query = @"SELECT reward_name, description, points_required, is_active, created_at 
                                 FROM rewards WHERE 1=1";
                        dateColumn = "created_at";
                        if (!string.IsNullOrEmpty(status))
                        {
                            query += " AND is_active = @status";
                            parameters.Add(new MySqlParameter("@status", status == "1"));
                        }
                        break;
                }

                if (startDate.HasValue)
                {
                    query += $" AND {dateColumn} >= @startDate";
                    parameters.Add(new MySqlParameter("@startDate", startDate.Value));
                }
                if (endDate.HasValue)
                {
                    query += $" AND {dateColumn} <= @endDate";
                    parameters.Add(new MySqlParameter("@endDate", endDate.Value.AddDays(1)));
                }

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddRange(parameters.ToArray());
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }
            return dt;
        }

        private void BindReportGrid(DataTable dt, string reportType)
        {
            gvReport.AutoGenerateColumns = false;
            gvReport.Columns.Clear();

            if (reportType == "donors")
            {
                gvReport.Columns.Add(new BoundField { DataField = "DonorName", HeaderText = "Donor Name" });
                gvReport.Columns.Add(new BoundField { DataField = "email", HeaderText = "Email" });
                gvReport.Columns.Add(new BoundField { DataField = "registration_date", HeaderText = "Registration Date", DataFormatString = "{0:yyyy-MM-dd}" });
                gvReport.Columns.Add(new BoundField { DataField = "is_active", HeaderText = "Active", DataFormatString = "{0:Yes;No}" });
            }
            else if (reportType == "inventory")
            {
                gvReport.Columns.Add(new BoundField { DataField = "blood_type", HeaderText = "Blood Type" });
                gvReport.Columns.Add(new BoundField { DataField = "quantity_ml", HeaderText = "Quantity (ml)" });
                gvReport.Columns.Add(new BoundField { DataField = "donation_date", HeaderText = "Donation Date", DataFormatString = "{0:yyyy-MM-dd}" });
                gvReport.Columns.Add(new BoundField { DataField = "expiration_date", HeaderText = "Expiration Date", DataFormatString = "{0:yyyy-MM-dd}" });
                gvReport.Columns.Add(new BoundField { DataField = "status", HeaderText = "Status" });
            }
            else if (reportType == "requests")
            {
                gvReport.Columns.Add(new BoundField { DataField = "blood_type", HeaderText = "Blood Type" });
                gvReport.Columns.Add(new BoundField { DataField = "quantity_ml", HeaderText = "Quantity (ml)" });
                gvReport.Columns.Add(new BoundField { DataField = "urgency", HeaderText = "Urgency" });
                gvReport.Columns.Add(new BoundField { DataField = "reason", HeaderText = "Reason" });
                gvReport.Columns.Add(new BoundField { DataField = "requested_at", HeaderText = "Requested At", DataFormatString = "{0:yyyy-MM-dd HH:mm}" });
                gvReport.Columns.Add(new BoundField { DataField = "status", HeaderText = "Status" });
            }
            

            gvReport.DataSource = dt;
            gvReport.DataBind();
            lblNoData.Visible = dt.Rows.Count == 0;
        }

        private void RenderChart(DataTable dt, string reportType)
        {
            string chartType = "bar";
            string chartTitle = "";
            List<string> labels = new List<string>();
            List<int> data = new List<int>();

            switch (reportType)
            {
                case "donors":
                    chartTitle = "Donor Distribution by Status";
                    labels.Add("Active");
                    labels.Add("Inactive");
                    int activeDonors = 0, inactiveDonors = 0;
                    foreach (DataRow row in dt.Rows)
                    {
                        if (Convert.ToBoolean(row["is_active"])) activeDonors++;
                        else inactiveDonors++;
                    }
                    data.Add(activeDonors);
                    data.Add(inactiveDonors);
                    break;
                case "inventory":
                    chartType = "pie";
                    chartTitle = "Blood Inventory by Blood Type";
                    var bloodTypes = new Dictionary<string, int>();
                    foreach (DataRow row in dt.Rows)
                    {
                        string type = row["blood_type"].ToString();
                        int quantity = Convert.ToInt32(row["quantity_ml"]);
                        if (bloodTypes.ContainsKey(type)) bloodTypes[type] += quantity;
                        else bloodTypes[type] = quantity;
                    }
                    labels.AddRange(bloodTypes.Keys);
                    data.AddRange(bloodTypes.Values);
                    break;
                case "requests":
                    chartTitle = "Blood Requests by Status";
                    var requestStatuses = new Dictionary<string, int>();
                    foreach (DataRow row in dt.Rows)
                    {
                        string status = row["status"].ToString();
                        if (requestStatuses.ContainsKey(status)) requestStatuses[status]++;
                        else requestStatuses[status] = 1;
                    }
                    labels.AddRange(requestStatuses.Keys);
                    data.AddRange(requestStatuses.Values);
                    break;
               
            }

            string script = $"renderChart({Newtonsoft.Json.JsonConvert.SerializeObject(labels)}, {Newtonsoft.Json.JsonConvert.SerializeObject(data)}, '{chartType}', '{chartTitle}');";
            ClientScript.RegisterStartupScript(this.GetType(), "renderChart", script, true);
        }

        protected void gvReport_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvReport.PageIndex = e.NewPageIndex;
            btnGenerateReport_Click(sender, e);
        }

        private void LogNotification(string message)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO notifications (admin_id, title, message, is_read, created_at) VALUES (@adminId, @title, @message, 0, NOW())";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@adminId", Session["AdminId"]);
                        cmd.Parameters.AddWithValue("@title", "Report Generated");
                        cmd.Parameters.AddWithValue("@message", message);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                ShowMessage("Error logging notification: " + ex.Message, "danger");
            }
        }

        private void ShowMessage(string message, string type)
        {
            pnlMessage.Visible = true;
            lblMessage.Text = message;
            pnlMessage.CssClass = "alert alert-" + type;
        }

        protected void lnkLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("Login.aspx");
        }
    }
}