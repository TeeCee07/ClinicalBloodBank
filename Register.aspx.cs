using MySql.Data.MySqlClient;
using System;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;

namespace ClinicalBloodBank
{
    public partial class Register : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e) 
        { if (!IsPostBack) { if (Session["UserType"] != null) { Response.Redirect("Default.aspx"); } 
            } 
        }
        private void ShowErrorMessage(string message)
        {
            pnlMessage.Visible = true;
            lblMessage.CssClass = "alert alert-danger";
            lblMessage.Text = message;
        }

        private void ShowSuccessMessage(string message)
        {
            pnlMessage.Visible = true;
            lblMessage.CssClass = "alert alert-success";
            lblMessage.Text = message;
        }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            if (!ValidateForm()) return;

            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Check for duplicate email
                    string checkEmailQuery = "SELECT COUNT(*) FROM donors WHERE email = @email";
                    using (MySqlCommand checkCmd = new MySqlCommand(checkEmailQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@email", txtEmail.Text);
                        int emailCount = Convert.ToInt32(checkCmd.ExecuteScalar());
                        if (emailCount > 0)
                        {
                            ShowErrorMessage("This email is already registered.");
                            return;
                        }
                    }

                    // Insert into donors table
                    string donorQuery = @"INSERT INTO donors (email, password, first_name, last_name, phone, 
                                    address_line1, address_line2, city, province, postal_code, country, 
                                    date_of_birth, gender, blood_type, weight, health_conditions, 
                                    is_eligible, total_donations, is_active, created_at)
                                    VALUES (@email, @password, @firstName, @lastName, @phone, 
                                    @addressLine1, @addressLine2, @city, @province, @postalCode, @country, 
                                    @dateOfBirth, @gender, @bloodType, @weight, @healthConditions, 
                                    1, 0, 1, NOW());
                                    SELECT LAST_INSERT_ID();";
                    int donorId;
                    using (MySqlCommand cmd = new MySqlCommand(donorQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", txtEmail.Text);
                        cmd.Parameters.AddWithValue("@password", txtPassword.Text); // Plain text password
                        cmd.Parameters.AddWithValue("@firstName", txtFirstName.Text);
                        cmd.Parameters.AddWithValue("@lastName", txtLastName.Text);
                        cmd.Parameters.AddWithValue("@phone", txtPhone.Text);
                        cmd.Parameters.AddWithValue("@addressLine1", txtAddressLine1.Text);
                        cmd.Parameters.AddWithValue("@addressLine2", string.IsNullOrEmpty(txtAddressLine2.Text) ? (object)DBNull.Value : txtAddressLine2.Text);
                        cmd.Parameters.AddWithValue("@city", txtCity.Text);
                        cmd.Parameters.AddWithValue("@province", txtState.Text); // Map state to province
                        cmd.Parameters.AddWithValue("@postalCode", txtPostalCode.Text);
                        cmd.Parameters.AddWithValue("@country", txtCountry.Text);
                        cmd.Parameters.AddWithValue("@dateOfBirth", DateTime.Parse(txtDateOfBirth.Text));
                        cmd.Parameters.AddWithValue("@gender", ddlGender.SelectedValue);
                        cmd.Parameters.AddWithValue("@bloodType", ddlBloodType.SelectedValue == "Not Known" ? "Unknown" : ddlBloodType.SelectedValue);
                        cmd.Parameters.AddWithValue("@weight", decimal.Parse(txtWeight.Text));
                        cmd.Parameters.AddWithValue("@healthConditions", string.IsNullOrEmpty(txtHealthConditions.Text) ? (object)DBNull.Value : txtHealthConditions.Text);
                        donorId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    AddNotification(donorId, "New Donor Registered", $"New donor registered: {txtFirstName.Text} {txtLastName.Text}");
                    ShowSuccessMessage("Donor registered successfully. Please log in.");
                    Response.Redirect("Login.aspx");
                }
                catch (MySqlException ex)
                {
                    System.Diagnostics.Debug.WriteLine("btnRegister_Click Database Error: " + ex.Message);
                    if (ex.Number == 1062) // Duplicate entry
                    {
                        ShowErrorMessage("This email is already registered.");
                    }
                    else
                    {
                        ShowErrorMessage("Database error during registration: " + ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("btnRegister_Click General Error: " + ex.Message);
                    ShowErrorMessage("Error registering donor: " + ex.Message);
                }
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrEmpty(txtEmail.Text) || string.IsNullOrEmpty(txtPassword.Text) ||
                string.IsNullOrEmpty(txtConfirmPassword.Text) || string.IsNullOrEmpty(txtFirstName.Text) ||
                string.IsNullOrEmpty(txtLastName.Text) || string.IsNullOrEmpty(txtPhone.Text) ||
                string.IsNullOrEmpty(txtAddressLine1.Text) || string.IsNullOrEmpty(txtCity.Text) ||
                string.IsNullOrEmpty(txtState.Text) || string.IsNullOrEmpty(txtPostalCode.Text) ||
                string.IsNullOrEmpty(txtCountry.Text) || string.IsNullOrEmpty(txtDateOfBirth.Text) ||
                string.IsNullOrEmpty(ddlGender.SelectedValue) || string.IsNullOrEmpty(ddlBloodType.SelectedValue) ||
                string.IsNullOrEmpty(txtWeight.Text))
            {
                ShowErrorMessage("All required fields must be filled.");
                return false;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(txtEmail.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ShowErrorMessage("Invalid email format.");
                return false;
            }

            if (txtPassword.Text != txtConfirmPassword.Text)
            {
                ShowErrorMessage("Passwords do not match.");
                return false;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(txtPostalCode.Text, @"^\d{4,5}$"))
            {
                ShowErrorMessage("Postal code must be a 4 or 5-digit number (e.g., 1234 or 12345).");
                return false;
            }

            if (!DateTime.TryParse(txtDateOfBirth.Text, out DateTime dob) || dob > DateTime.Today)
            {
                ShowErrorMessage("Invalid date of birth.");
                return false;
            }

            int age = DateTime.Today.Year - dob.Year;
            if (dob.Date > DateTime.Today.AddYears(-age)) age--;
            if (age < 18)
            {
                ShowErrorMessage("Donors must be at least 18 years old.");
                return false;
            }

            if (!decimal.TryParse(txtWeight.Text, out decimal weight) || weight <= 0 || weight < 50)
            {
                ShowErrorMessage("Weight must be a valid number greater than 50 kg (e.g., 75.5).");
                return false;
            }

            return true;
        }

        private void AddNotification(int donorId, string title, string message)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"INSERT INTO notifications (donor_id, admin_id, hospital_id, title, message, is_read, created_at)
                                VALUES (@donorId, NULL, NULL, @title, @message, 0, NOW())";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@donorId", donorId);
                        cmd.Parameters.AddWithValue("@title", title);
                        cmd.Parameters.AddWithValue("@message", message);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (MySqlException ex)
                {
                    System.Diagnostics.Debug.WriteLine("AddNotification Database Error: " + ex.Message);
                    ShowErrorMessage("Error adding notification: " + ex.Message);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("AddNotification General Error: " + ex.Message);
                    ShowErrorMessage("Error adding notification: " + ex.Message);
                }
            }
        }
    }

}