using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ClinicalBloodBank
{
    public partial class HospitalReports : System.Web.UI.Page
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null || Session["UserType"] == null || Session["UserType"].ToString() != "hospital")
            {
                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] Page_Load: Invalid session. UserId: {Session["UserId"]}, UserType: {Session["UserType"]}");
                Response.Redirect("Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadHospitalDetails();
                LoadInventoryReport();
                LoadRequestFulfillmentReport();
            }
        }

        private void LoadHospitalDetails()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
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
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] LoadHospitalDetails Error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                    ShowMessage("Error loading hospital details.", "danger");
                }
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

        private void LoadInventoryReport()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT blood_type, SUM(quantity_ml) AS quantity_ml, status
                                    FROM blood_inventory
                                    WHERE tested_by_hospital = @hospitalId
                                    GROUP BY blood_type, status";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        gvInventory.DataSource = dt;
                        gvInventory.DataBind();
                        lblNoInventory.Visible = dt.Rows.Count == 0;

                        if (dt.Rows.Count > 0)
                        {
                            string labels = "[";
                            string data = "[";
                            foreach (DataRow row in dt.Rows)
                            {
                                labels += $"\"{row["blood_type"]?.ToString() ?? "Unknown"}\",";
                                data += $"{row["quantity_ml"]?.ToString() ?? "0"},";
                            }
                            labels = labels.TrimEnd(',') + "]";
                            data = data.TrimEnd(',') + "]";
                            ClientScript.RegisterStartupScript(this.GetType(), "inventoryChart", $"renderInventoryChart({labels}, {data});", true);
                        }
                        else
                        {
                            ClientScript.RegisterStartupScript(this.GetType(), "inventoryChart", "renderInventoryChart([], []);", true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] LoadInventoryReport Error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                    ShowMessage("Error loading inventory report.", "danger");
                }
            }
        }

        private void LoadRequestFulfillmentReport()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT request_id, blood_type, quantity_ml, status, fulfilled_at
                                    FROM blood_requests
                                    WHERE fulfilled_by_hospital = @hospitalId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        gvRequests.DataSource = dt;
                        gvRequests.DataBind();
                        lblNoRequests.Visible = dt.Rows.Count == 0;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] LoadRequestFulfillmentReport Error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                    ShowMessage("Error loading request fulfillment report.", "danger");
                }
            }
        }

        protected void btnExportInventory_Click(object sender, EventArgs e)
        {
            ExportToCsv("InventoryReport", gvInventory, "blood_type,quantity_ml,status");
        }

        protected void btnExportRequests_Click(object sender, EventArgs e)
        {
            ExportToCsv("RequestFulfillmentReport", gvRequests, "request_id,blood_type,quantity_ml,status,fulfilled_at");
        }

        private void ExportToCsv(string reportName, GridView gridView, string headers)
        {
            try
            {
                Response.Clear();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", $"attachment;filename={reportName}_{DateTime.Now:yyyyMMdd}.csv");
                Response.Charset = "";
                Response.ContentType = "application/text";

                StringBuilder sb = new StringBuilder();
                string[] headerArray = headers.Split(',');
                foreach (string header in headerArray)
                {
                    sb.Append($"\"{header}\"").Append(",");
                }
                sb.Length--; // Remove trailing comma
                sb.Append("\r\n");

                foreach (GridViewRow row in gridView.Rows)
                {
                    foreach (TableCell cell in row.Cells)
                    {
                        string cellText = cell.Text.Replace("&nbsp;", "").Replace("\"", "\"\"");
                        sb.Append($"\"{cellText}\"").Append(",");
                    }
                    sb.Length--; // Remove trailing comma
                    sb.Append("\r\n");
                }

                Response.Output.Write(sb.ToString());
                Response.Flush();
                Response.End();

                LogNotification($"Exported {reportName} to CSV");
                ShowMessage($"{reportName} exported successfully.", "success");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] ExportToCsv Error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                ShowMessage($"Error exporting {reportName}.", "danger");
            }
        }

        private void LogNotification(string message)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "INSERT INTO notifications (hospital_id, title, message, is_read, created_at) VALUES (@hospitalId, @title, @message, 0, NOW())";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@hospitalId", Session["UserId"]);
                        cmd.Parameters.AddWithValue("@title", "Report Export");
                        cmd.Parameters.AddWithValue("@message", message);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] LogNotification Error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                }
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
            Session.Clear();
            Session.Abandon();
            Response.Redirect("Login.aspx");
        }
    }
}