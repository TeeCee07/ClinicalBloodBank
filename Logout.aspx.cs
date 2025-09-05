using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ClinicalBloodBank
{
    public partial class Logout : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString["logout"] == "success")
                {
                    // Display success message (e.g., using a Label control or JavaScript alert)
                    ClientScript.RegisterStartupScript(this.GetType(), "showSuccess", "alert('You have been logged out successfully.');", true);
                }
            }
        }
    }
}