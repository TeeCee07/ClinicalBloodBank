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
    public partial class ManageRewards : System.Web.UI.Page
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
                controlsToRegister.Add(btnSearch.UniqueID);
                controlsToRegister.Add(btnClearSearch.UniqueID);
                controlsToRegister.Add(btnSave.UniqueID);
                controlsToRegister.Add(btnCancel.UniqueID);
                controlsToRegister.Add(lnkLogout.UniqueID);

                if (!IsPostBack)
                {
                    LoadUserInfo();
                    BindRewardGrid();
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
            else
            {
                litUserName.Text = "Administrator";
                litUserInitials.Text = "AD";
            }
        }

        private void BindRewardGrid()
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                ShowMessage("Database connection configuration is missing.", "danger");
                lblNoRewards.Visible = true;
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT reward_id, reward_name, description, points_required, is_active 
                                    FROM rewards 
                                    WHERE reward_name LIKE @search OR description LIKE @search 
                                    ORDER BY updated_at DESC";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        string searchTerm = string.IsNullOrEmpty(txtSearch.Text) ? "%" : $"%{txtSearch.Text}%";
                        cmd.Parameters.AddWithValue("@search", searchTerm);
                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            gvRewards.DataSource = dt;
                            gvRewards.DataBind();
                            lblNoRewards.Visible = false;
                        }
                        else
                        {
                            gvRewards.DataSource = null;
                            gvRewards.DataBind();
                            lblNoRewards.Visible = true;
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] BindRewardGrid - MySQL Error: {ex.Message}");
                ShowMessage("Error loading rewards: " + ex.Message, "danger");
                lblNoRewards.Visible = true;
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            BindRewardGrid();
        }

        protected void btnClearSearch_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            BindRewardGrid();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            if (string.IsNullOrEmpty(connectionString))
            {
                ShowMessage("Database connection configuration is missing.", "danger");
                return;
            }

            string action = "processed"; // Default value
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query;
                    if (string.IsNullOrEmpty(hdnRewardId.Value))
                    {
                        // Add new reward
                        query = @"INSERT INTO rewards (reward_name, description, points_required, is_active, created_at, updated_at) 
                                 VALUES (@rewardName, @description, @pointsRequired, @isActive, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)";
                        action = "added";
                    }
                    else
                    {
                        // Update existing reward
                        query = @"UPDATE rewards SET reward_name = @rewardName, description = @description, 
                                 points_required = @pointsRequired, is_active = @isActive, updated_at = CURRENT_TIMESTAMP 
                                 WHERE reward_id = @rewardId";
                        action = "updated";
                    }

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@rewardName", txtRewardName.Text);
                        cmd.Parameters.AddWithValue("@description", txtDescription.Text);
                        cmd.Parameters.AddWithValue("@pointsRequired", Convert.ToInt32(txtPointsRequired.Text));
                        cmd.Parameters.AddWithValue("@isActive", chkIsActive.Checked);
                        if (!string.IsNullOrEmpty(hdnRewardId.Value))
                            cmd.Parameters.AddWithValue("@rewardId", Convert.ToInt32(hdnRewardId.Value));
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            AddNotification(Convert.ToInt32(Session["AdminId"]), $"Reward {action}", $"Reward '{txtRewardName.Text}' has been {action}.");
                            ShowMessage($"Reward {action} successfully.", "success");
                            ClearForm();
                            BindRewardGrid();
                        }
                        else
                        {
                            ShowMessage($"No changes made to the reward.", "info");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnSave_Click - MySQL Error: {ex.Message}");
                ShowMessage($"Error {action} reward: " + ex.Message, "danger");
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ClearForm();
            ShowMessage("Form cleared.", "info");
        }

        protected void btnEdit_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int rewardId = Convert.ToInt32(btn.CommandArgument);

            if (string.IsNullOrEmpty(connectionString))
            {
                ShowMessage("Database connection configuration is missing.", "danger");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT reward_name, description, points_required, is_active 
                                    FROM rewards WHERE reward_id = @rewardId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@rewardId", rewardId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hdnRewardId.Value = rewardId.ToString();
                                txtRewardName.Text = reader["reward_name"].ToString();
                                txtDescription.Text = reader["description"].ToString();
                                txtPointsRequired.Text = reader["points_required"].ToString();
                                chkIsActive.Checked = Convert.ToBoolean(reader["is_active"]);
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnEdit_Click - MySQL Error: {ex.Message}");
                ShowMessage("Error loading reward details: " + ex.Message, "danger");
            }
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int rewardId = Convert.ToInt32(btn.CommandArgument);

            if (string.IsNullOrEmpty(connectionString))
            {
                ShowMessage("Database connection configuration is missing.", "danger");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string rewardNameQuery = "SELECT reward_name FROM rewards WHERE reward_id = @rewardId";
                    string rewardName;
                    using (MySqlCommand cmd = new MySqlCommand(rewardNameQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@rewardId", rewardId);
                        rewardName = cmd.ExecuteScalar()?.ToString() ?? "Unknown";
                    }

                    string query = "DELETE FROM rewards WHERE reward_id = @rewardId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@rewardId", rewardId);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            AddNotification(Convert.ToInt32(Session["AdminId"]), "Reward deleted", $"Reward '{rewardName}' has been deleted.");
                            ShowMessage("Reward deleted successfully.", "success");
                            BindRewardGrid();
                        }
                        else
                        {
                            ShowMessage("No reward found to delete.", "info");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnDelete_Click - MySQL Error: {ex.Message}");
                ShowMessage("Error deleting reward: " + ex.Message, "danger");
            }
        }

        protected void btnToggle_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int rewardId = Convert.ToInt32(btn.CommandArgument);

            if (string.IsNullOrEmpty(connectionString))
            {
                ShowMessage("Database connection configuration is missing.", "danger");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string rewardNameQuery = "SELECT reward_name, is_active FROM rewards WHERE reward_id = @rewardId";
                    string rewardName;
                    bool currentStatus;
                    using (MySqlCommand cmd = new MySqlCommand(rewardNameQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@rewardId", rewardId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                rewardName = reader["reward_name"].ToString();
                                currentStatus = Convert.ToBoolean(reader["is_active"]);
                            }
                            else
                            {
                                ShowMessage("Reward not found.", "danger");
                                return;
                            }
                        }
                    }

                    string query = "UPDATE rewards SET is_active = @isActive, updated_at = CURRENT_TIMESTAMP WHERE reward_id = @rewardId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@isActive", !currentStatus);
                        cmd.Parameters.AddWithValue("@rewardId", rewardId);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            string action = !currentStatus ? "activated" : "deactivated";
                            AddNotification(Convert.ToInt32(Session["AdminId"]), $"Reward {action}", $"Reward '{rewardName}' has been {action}.");
                            ShowMessage($"Reward {action} successfully.", "success");
                            BindRewardGrid();
                        }
                        else
                        {
                            ShowMessage("No changes made to reward status.", "info");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] btnToggle_Click - MySQL Error: {ex.Message}");
                ShowMessage("Error toggling reward status: " + ex.Message, "danger");
            }
        }

        protected void lnkLogout_Click(object sender, EventArgs e)
        {
            Debug.WriteLine($"[{DateTime.Now}] lnkLogout_Click - Logging out AdminId: {Session["AdminId"]}");
            Session.Clear();
            Session.Abandon();
            Response.Redirect("Login.aspx");
        }

        protected void gvRewards_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvRewards.PageIndex = e.NewPageIndex;
            BindRewardGrid();
        }

        protected void gvRewards_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Pager)
            {
                foreach (Control control in e.Row.Controls[0].Controls)
                {
                    if (control is LinkButton || control is Button)
                    {
                        controlsToRegister.Add(control.UniqueID);
                    }
                }
            }
        }

        private void ClearForm()
        {
            hdnRewardId.Value = "";
            txtRewardName.Text = "";
            txtDescription.Text = "";
            txtPointsRequired.Text = "";
            chkIsActive.Checked = true;
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
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[{DateTime.Now}] AddNotification - MySQL Error: {ex.Message}");
                ShowMessage("Error adding notification: " + ex.Message, "danger");
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
            // Placeholder for future use
        }
    }
}