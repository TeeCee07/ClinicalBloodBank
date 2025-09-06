using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Web.UI;

namespace ClinicalBloodBank
{
    public partial class Login : System.Web.UI.Page
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Session.Clear();
                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] Page_Load: Session cleared");
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;

            System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] btnLogin_Click: Attempting login with email={email}");

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowAlert("Email and password are required.", "danger");
                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] btnLogin_Click: Empty email or password");
                return;
            }

            if (AuthenticateUser(email, password))
            {
                string userType = Session["UserType"] as string;
                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] btnLogin_Click: Authenticated user, type={userType}, UserId={Session["UserId"]}, UserName={Session["UserName"]}");
                switch (userType)
                {
                    case "donor":
                        Response.Redirect("DonorDashboard.aspx");
                        break;
                    case "admin":
                        Response.Redirect("AdminDashboard.aspx");
                        break;
                    case "hospital":
                        Response.Redirect("HospitalDashboard.aspx");
                        break;
                    default:
                        ShowAlert("Unknown user type. Please contact support.", "danger");
                        System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] btnLogin_Click: Unknown user type={userType}");
                        break;
                }
            }
            else
            {
                ShowAlert("Invalid login credentials. Please try again.", "danger");
                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] btnLogin_Click: Authentication failed for email={email}");
            }
        }

        private bool AuthenticateUser(string email, string password)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] AuthenticateUser: Database connection opened");

                    // Check donors table
                    string donorQuery = "SELECT donor_id, first_name, last_name FROM donors WHERE email = @email AND password = @password AND is_active = 1";
                    using (MySqlCommand cmd = new MySqlCommand(donorQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@password", password);
                        System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] AuthenticateUser: Executing donor query for email={email}");
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string donorId = reader["donor_id"].ToString();
                                string fullName = $"{reader["first_name"]} {reader["last_name"]}";
                                reader.Close();

                                string updateQuery = "UPDATE donors SET last_login = NOW() WHERE donor_id = @donorId";
                                using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, conn))
                                {
                                    updateCmd.Parameters.AddWithValue("@donorId", donorId);
                                    updateCmd.ExecuteNonQuery();
                                    System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] AuthenticateUser: Updated last_login for donor_id={donorId}");
                                }

                                AddNotification(Convert.ToInt32(donorId), null, null, $"Donor {fullName} logged in");
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
                        cmd.Parameters.AddWithValue("@password", password);
                        System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] AuthenticateUser: Executing admin query for email={email}");
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string adminId = reader["admin_id"].ToString();
                                string fullName = $"{reader["first_name"]} {reader["last_name"]}";
                                reader.Close();

                                string updateQuery = "UPDATE admins SET last_login = NOW() WHERE admin_id = @adminId";
                                using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, conn))
                                {
                                    updateCmd.Parameters.AddWithValue("@adminId", adminId);
                                    updateCmd.ExecuteNonQuery();
                                    System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] AuthenticateUser: Updated last_login for admin_id={adminId}");
                                }

                                AddNotification(null, Convert.ToInt32(adminId), null, $"Admin {fullName} logged in");
                                Session["UserId"] = adminId;
                                Session["UserType"] = "admin";
                                Session["UserName"] = fullName;
                                Session["AdminId"] = adminId;
                                Session["AdminName"] = fullName;
                                return true;
                            }
                        }
                    }

                    // Check hospitals table
                    string hospitalQuery = "SELECT hospital_id, hospital_name FROM hospitals WHERE contact_email = @email AND contact_password = @password AND is_verified = 1";
                    using (MySqlCommand cmd = new MySqlCommand(hospitalQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@password", password);
                        System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] AuthenticateUser: Executing hospital query for email={email}");
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string hospitalId = reader["hospital_id"].ToString();
                                string hospitalName = reader["hospital_name"].ToString();
                                reader.Close();

                                string updateQuery = "UPDATE hospitals SET last_login = NOW() WHERE hospital_id = @hospitalId";
                                using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, conn))
                                {
                                    updateCmd.Parameters.AddWithValue("@hospitalId", hospitalId);
                                    updateCmd.ExecuteNonQuery();
                                    System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] AuthenticateUser: Updated last_login for hospital_id={hospitalId}");
                                }

                                AddNotification(null, null, Convert.ToInt32(hospitalId), $"Hospital {hospitalName} logged in");
                                Session["UserId"] = hospitalId;
                                Session["UserType"] = "hospital";
                                Session["UserName"] = hospitalName;
                                return true;
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] AuthenticateUser: No hospital found for email={email}, is_verified=1");
                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] AuthenticateUser: MySQL error - Code: {ex.Number}, Message: {ex.Message}");
                    ShowAlert($"Database error during login: {ex.Message}", "danger");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] AuthenticateUser: General error - {ex.Message}");
                    ShowAlert($"Error during login: {ex.Message}", "danger");
                }
            }

            return false;
        }

        private void AddNotification(int? donorId, int? adminId, int? hospitalId, string message)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "INSERT INTO notifications (donor_id, admin_id, hospital_id, title, message, is_read, created_at) " +
                                   "VALUES (@donorId, @adminId, @hospitalId, 'Login', @message, 0, NOW())";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@donorId", donorId ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@adminId", adminId ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@hospitalId", hospitalId ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@message", message);
                        cmd.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] AddNotification: Notification added - {message}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] AddNotification: Error - {ex.Message}");
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