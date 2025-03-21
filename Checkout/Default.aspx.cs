using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Checkout
{
	public partial class Default : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			string transactionDetailsJson = transactionDetails.Value;
			if (!string.IsNullOrEmpty(transactionDetailsJson))
			{
				// Deserialize the JSON string to an object
				var transactionDetails = Newtonsoft.Json.JsonConvert.DeserializeObject(transactionDetailsJson);

				// Process the transaction details object
				var transactionToken = transactionDetails.ToString();

				var amount = Request["amount"].ToString();

				CheckoutAuthCodeGrant.Controls.Checkout.SubmitButton_Click(amount, transactionToken);
			}
		}
	}
}