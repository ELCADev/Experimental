using System;
using System.Web.UI;
using Newtonsoft.Json;

namespace Checkout
{
	public partial class Default : System.Web.UI.Page
	{
		protected async void Page_Load(object sender, EventArgs e)
		{
			string transactionDetailsJson = transactionDetails.Value;
			if (!string.IsNullOrEmpty(transactionDetailsJson))
			{
				try
				{
					// Deserialize the JSON string to a strongly-typed object
					var transactionDetails = JsonConvert.DeserializeObject<TransactionDetails>(transactionDetailsJson);

					// Process the transaction details object
					var transactionToken = transactionDetails.Token;

					if (Request["amount"] != null)
					{
						var amount = Request["amount"].ToString();
						var checkout = new CheckoutAuthCodeGrant.Controls.Checkout();
						await checkout.SubmitCheckout(amount, transactionToken);
					}
					else
					{
						// Handle the case where amount is not provided
						// Log or display an error message
					}
				}
				catch (JsonException ex)
				{
					// Handle JSON deserialization errors
					// Log the exception and display an error message
				}
				catch (Exception ex)
				{
					// Handle other potential exceptions
					// Log the exception and display an error message
				}
			}
		}
	}

	// Define a strongly-typed model for transaction details
	public class TransactionDetails
	{
		public string Token { get; set; }
		// Add other properties as needed
	}
}