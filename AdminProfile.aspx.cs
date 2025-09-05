using System;
using System.Data;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;

namespace ClinicalBloodBank
{
    public partial class AdminProfile : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Validate session and authentication
                if (Session["UserId"] == null || Session["UserType"] == null || Session["UserType"].ToString() != "admin")
                {
                    Response.Redirect("Login.aspx");
                    return;
                }

                LoadUserProfile();
            }
        }

        private void LoadUserProfile()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"]?.ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                ShowErrorMessage("Database connection configuration is missing.");
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Get user details from database
                    string userQuery = "SELECT first_name, last_name, email, phone, address_line1, address_line2, " +
                                      "city, state, postal_code, country, profile_picture " +
                                      "FROM users WHERE user_id = @userId";
                    using (MySqlCommand cmd = new MySqlCommand(userQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", Session["UserId"]);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Populate form fields
                                txtFirstName.Text = reader["first_name"].ToString();
                                txtLastName.Text = reader["last_name"].ToString();
                                txtEmail.Text = reader["email"].ToString();
                                txtPhone.Text = reader["phone"].ToString();
                                txtAddress1.Text = reader["address_line1"].ToString();
                                txtAddress2.Text = reader["address_line2"].ToString();
                                txtCity.Text = reader["city"].ToString();
                                txtState.Text = reader["state"].ToString();
                                txtPostalCode.Text = reader["postal_code"].ToString();
                                txtCountry.Text = reader["country"].ToString();

                                // Set header user name
                                litHeaderUserName.Text = reader["first_name"].ToString() + " " + reader["last_name"].ToString();

                                // Handle profile picture
                                string profilePicture = reader["profile_picture"].ToString();
                                if (!string.IsNullOrEmpty(profilePicture))
                                {
                                    imgProfile.ImageUrl = profilePicture;
                                    imgProfile.Visible = true;
                                    profilePlaceholder.Visible = false;

                                    // Set header avatar
                                    headerAvatar.Style["background-image"] = $"url('{profilePicture}')";
                                    headerAvatar.InnerText = "";
                                }
                                else
                                {
                                    imgProfile.Visible = false;
                                    profilePlaceholder.Visible = true;
                                    profilePlaceholder.InnerText = reader["first_name"].ToString().Substring(0, 1) +
                                                                   reader["last_name"].ToString().Substring(0, 1);

                                    // Set header avatar with initials
                                    headerAvatar.Style["background-image"] = "none";
                                    headerAvatar.InnerText = reader["first_name"].ToString().Substring(0, 1) +
                                                            reader["last_name"].ToString().Substring(0, 1);
                                }
                            }
                            else
                            {
                                ShowErrorMessage("User profile not found.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("LoadUserProfile Error: " + ex.Message);
                    ShowErrorMessage("Error loading user profile.");
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
                return;

            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"]?.ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                ShowErrorMessage("Database connection configuration is missing.");
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Handle profile picture upload
                    string profilePicturePath = null;
                    if (fileProfile.HasFile)
                    {
                        profilePicturePath = SaveUploadedFile(fileProfile.PostedFile);
                    }

                    // Update user details
                    string updateQuery = "UPDATE users SET first_name = @firstName, last_name = @lastName, " +
                                        "email = @email, phone = @phone, address_line1 = @address1, " +
                                        "address_line2 = @address2, city = @city, state = @state, " +
                                        "postal_code = @postalCode, country = @country " +
                                        (profilePicturePath != null ? ", profile_picture = @profilePicture " : "") +
                                        "WHERE user_id = @userId";

                    using (MySqlCommand cmd = new MySqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@firstName", txtFirstName.Text);
                        cmd.Parameters.AddWithValue("@lastName", txtLastName.Text);
                        cmd.Parameters.AddWithValue("@email", txtEmail.Text);
                        cmd.Parameters.AddWithValue("@phone", txtPhone.Text);
                        cmd.Parameters.AddWithValue("@address1", txtAddress1.Text);
                        cmd.Parameters.AddWithValue("@address2", txtAddress2.Text);
                        cmd.Parameters.AddWithValue("@city", txtCity.Text);
                        cmd.Parameters.AddWithValue("@state", txtState.Text);
                        cmd.Parameters.AddWithValue("@postalCode", txtPostalCode.Text);
                        cmd.Parameters.AddWithValue("@country", txtCountry.Text);
                        cmd.Parameters.AddWithValue("@userId", Session["UserId"]);

                        if (profilePicturePath != null)
                        {
                            cmd.Parameters.AddWithValue("@profilePicture", profilePicturePath);
                        }

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // Update session variables
                            Session["FirstName"] = txtFirstName.Text;
                            Session["LastName"] = txtLastName.Text;
                            Session["Email"] = txtEmail.Text;

                            ShowSuccessMessage("Profile updated successfully!");

                            // Reload profile to reflect changes
                            LoadUserProfile();
                        }
                        else
                        {
                            ShowErrorMessage("Failed to update profile.");
                        }
                    }
                }
                catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
                {
                    ShowErrorMessage("This email address is already registered.");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("btnSave_Click Error: " + ex.Message);
                    ShowErrorMessage("Error updating profile.");
                }
            }
        }

        protected void btnRemovePicture_Click(object sender, EventArgs e)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicalBloodBankDB"]?.ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                ShowErrorMessage("Database connection configuration is missing.");
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Remove profile picture
                    string updateQuery = "UPDATE users SET profile_picture = NULL WHERE user_id = @userId";
                    using (MySqlCommand cmd = new MySqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", Session["UserId"]);
                        cmd.ExecuteNonQuery();
                    }

                    ShowSuccessMessage("Profile picture removed successfully!");

                    // Reload profile to reflect changes
                    LoadUserProfile();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("btnRemovePicture_Click Error: " + ex.Message);
                    ShowErrorMessage("Error removing profile picture.");
                }
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("AdminDashboard.aspx");
        }

        protected void cvProfilePicture_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (!fileProfile.HasFile)
            {
                args.IsValid = true; // No file is acceptable
                return;
            }

            // Check file type
            string fileExtension = Path.GetExtension(fileProfile.FileName).ToLower();
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

            args.IsValid = Array.IndexOf(allowedExtensions, fileExtension) >= 0;
        }

        private string SaveUploadedFile(HttpPostedFile file)
        {
            try
            {
                string uploadFolder = Server.MapPath("~/Uploads/ProfilePictures/");

                // Create directory if it doesn't exist
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                // Generate unique filename
                string fileExtension = Path.GetExtension(file.FileName);
                string fileName = $"admin_{Session["UserId"]}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
                string filePath = Path.Combine(uploadFolder, fileName);

                // Save file
                file.SaveAs(filePath);

                // Return relative path for database storage
                return $"/Uploads/ProfilePictures/{fileName}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SaveUploadedFile Error: " + ex.Message);
                return null;
            }
        }

        private void ShowSuccessMessage(string message)
        {
            pnlSuccess.Visible = true;
            pnlError.Visible = false;
            litSuccessMessage.Text = message;
        }

        private void ShowErrorMessage(string message)
        {
            pnlError.Visible = true;
            pnlSuccess.Visible = false;
            litErrorMessage.Text = message;
        }
    }
}