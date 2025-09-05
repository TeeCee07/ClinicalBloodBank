using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Web.UI;

namespace ClinicalBloodBank
{
    public partial class ForgotPassword : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnResetPassword_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();

            if (IsValidEmail(email))
            {
                // In a real application, you would:
                // 1. Generate a password reset token
                // 2. Store it in the database with an expiration time
                // 3. Send an email with a reset link

                // For this example, we'll just show a success message
                ShowAlert("Password reset instructions have been sent to your email.", "success");
            }
            else
            {
                ShowAlert("No account found with that email address.", "danger");
            }
        }

        private bool IsValidEmail(string email)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM users WHERE email = @email AND is_active = 1";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", email);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
                catch (Exception ex)
                {
                    // Log error
                    System.Diagnostics.Debug.WriteLine("Email validation error: " + ex.Message);
                    return false;
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