using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Web.UI;

namespace ClinicalBloodBank
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            { 
               Session.Clear(); }
            }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowAlert("Email and password are required.", "danger");
                return;
            }

            if (AuthenticateUser(email, password))
            {
                string userType = Session["UserType"] as string;
                switch (userType)
                {
                    case "donor":
                        Response.Redirect("DonorDashboard.aspx");
                        break;
                    case "hospital":
                        Response.Redirect("HospitalDashboard.aspx");
                        break;
                    case "admin":
                        Response.Redirect("AdminDashboard.aspx");
                        break;
                    default:
                        ShowAlert("Unknown user type. Please contact support.", "danger");
                        break;
                }
            }
            else
            {
                ShowAlert("Invalid login credentials. Please try again.", "danger");
            }
        }

        private bool AuthenticateUser(string email, string password)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Check donors table
                    string donorQuery = "SELECT donor_id, first_name, last_name FROM donors WHERE email = @email AND password = @password AND is_active = 1";
                    using (MySqlCommand cmd = new MySqlCommand(donorQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@password", password); // Plain text comparison
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string donorId = reader["donor_id"].ToString();
                                string fullName = $"{reader["first_name"]} {reader["last_name"]}";
                                reader.Close();

                                // Update last_login
                                string updateQuery = "UPDATE donors SET last_login = NOW() WHERE donor_id = @donorId";
                                using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, conn))
                                {
                                    updateCmd.Parameters.AddWithValue("@donorId", donorId);
                                    updateCmd.ExecuteNonQuery();
                                }

                                // Add notification
                                AddNotification(Convert.ToInt32(donorId), null, $"Donor {fullName} logged in");
                                Session["UserId"] = donorId;
                                Session["UserType"] = "donor";
                                Session["UserName"] = fullName;
                                return true;
                            }
                        }
                    }

                    // Check admins table
                    string adminQuery = "SELECT admin_id, first_name, last_name FROM admins WHERE email = @email AND password = @password AND is_active = 1";
                    using (MySqlCommand cmd = new MySqlCommand(adminQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@password", password); // Plain text comparison
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string adminId = reader["admin_id"].ToString();
                                string fullName = $"{reader["first_name"]} {reader["last_name"]}";
                                reader.Close();

                                // Check if admin is associated with a hospital
                                bool isHospitalAdmin = false;
                                string hospitalQuery = "SELECT hospital_id FROM hospitals WHERE admin_id = @adminId";
                                using (MySqlCommand hospitalCmd = new MySqlCommand(hospitalQuery, conn))
                                {
                                    hospitalCmd.Parameters.AddWithValue("@adminId", adminId);
                                    using (MySqlDataReader hospitalReader = hospitalCmd.ExecuteReader())
                                    {
                                        isHospitalAdmin = hospitalReader.HasRows;
                                    }
                                }

                                // Update last_login
                                string updateQuery = "UPDATE admins SET last_login = NOW() WHERE admin_id = @adminId";
                                using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, conn))
                                {
                                    updateCmd.Parameters.AddWithValue("@adminId", adminId);
                                    updateCmd.ExecuteNonQuery();
                                }

                                // Add notification
                                AddNotification(null, Convert.ToInt32(adminId), $"{(isHospitalAdmin ? "Hospital Admin" : "Admin")} {fullName} logged in");
                                Session["UserId"] = adminId;
                                Session["UserType"] = isHospitalAdmin ? "hospital" : "admin";
                                Session["UserName"] = fullName;
                                Session["AdminId"] = adminId; // For ManageDonors.aspx
                                Session["AdminName"] = fullName; // For ManageDonors.aspx
                                return true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Authentication error: " + ex.Message);
                    ShowAlert("Error during login: " + ex.Message, "danger");
                }
            }

            return false;
        }

        private void AddNotification(int? donorId, int? adminId, string message)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "INSERT INTO notifications (donor_id, admin_id, hospital_id, title, message, is_read, created_at) " +
                                   "VALUES (@donorId, @adminId, NULL, 'Login', @message, 0, NOW())";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@donorId", donorId ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@adminId", adminId ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@message", message);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("AddNotification error: " + ex.Message);
                }
            }
        }

        private void ShowAlert(string message, string type)
        {
            pnlMessage.Visible = true;
            pnlMessage.CssClass = type == "danger" ? "alert alert-danger" : "alert alert-success";
            lblMessage.Text = message;
        }
    }

}