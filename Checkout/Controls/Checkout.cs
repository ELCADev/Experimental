using CheckoutAuthCodeGrant.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;

namespace CheckoutAuthCodeGrant.Controls
{
	public class Checkout
	{
		//protected void Page_Load(object sender, EventArgs e)
		//{
		//	if (!IsPostBack)
		//	{
		//		InitializeControl();
		//	}
		//}

		//public async void InitializeControl()
		//{
		//	await SetPublicKey();
		//	await SetPaymentConfigurations();
		//}

		public async Task LoadCheckoutPage()
		{
			var model = new CheckoutViewModel();
			await SetPublicKey(model);
			await SetPaymentConfigurations(model);
		}

		public async Task SetPublicKey(CheckoutViewModel model)
		{
			var response = await PaymentsApiClient.GetPublicKey();
			var content = await response.Content.ReadAsStringAsync();

			if (!response.IsSuccessStatusCode)
			{
				ShowError(TryGetErrorMessage(content) ??
					"The attempt to retrieve the public key from Payments API did not succeed.");
				return;
			}

			var publicKeyData = JsonConvert.DeserializeObject<Models.PaymentsApi.PublicKeyData>(content);
			model.PublicKey = publicKeyData.Value;
			//PublicKeyField.Value = publicKeyData.Value;
		}

		public async Task SetPaymentConfigurations(CheckoutViewModel model)
		{
			var response = await PaymentsApiClient.GetPaymentConfigurations();
			var content = await response.Content.ReadAsStringAsync();

			if (!response.IsSuccessStatusCode)
			{
				ShowError(TryGetErrorMessage(content) ??
					"The attempt to retrieve payment configurations did not succeed.");
				return;
			}

			var paymentConfigurationsData =
				JsonConvert.DeserializeObject<Models.PaymentsApi.PaymentConfigurationsData>(content);

			if (!paymentConfigurationsData.Value.Any())
			{
				ShowError("There were no payment configurations returned from Payments API.");
				return;
			}

			var validPaymentConfigurations = paymentConfigurationsData.Value
				.Where(pc => pc.ProcessMode != "Live");

			if (!validPaymentConfigurations.Any())
			{
				ShowError("All payment configurations are currently marked with a 'Live' process mode.");
				return;
			}

			if (string.IsNullOrEmpty(model.SelectedPaymentConfigurationId))
			{
				model.SelectedPaymentConfigurationId = validPaymentConfigurations.First(pc => pc.ProcessMode != "Live").Id;
			}
		}

		public async Task SubmitCheckout(String Amount, String TokenField)
		{
			try
			{
				decimal amount;
				if (!decimal.TryParse(Amount, out amount))
				{
					ShowError("Invalid amount specified.");
					return;
				}

				var token = TokenField;
				if (string.IsNullOrEmpty(token))
				{
					ShowError("Payment token not generated.");
					return;
				}

				var response = await PaymentsApiClient.ChargeCheckoutTransaction(amount, token);
				var content = await response.Content.ReadAsStringAsync();

				if (!response.IsSuccessStatusCode)
				{
					ShowError(TryGetErrorMessage(content) ??
						"The payment transaction failed to process.");
					return;
				}

				// Handle successful payment
				// You might want to redirect to a success page or show a success message
			}
			catch (Exception ex)
			{
				ShowError("An error occurred while processing the payment: " + ex.Message);
			}
		}

		public void ShowError(string message)
		{
			//ErrorMessage.Text = WebUtility.HtmlEncode(message);
			//ErrorPanel.Visible = true;
		}

		public string TryGetErrorMessage(string content)
		{
			try
			{
				var error = JsonConvert.DeserializeObject<Models.PaymentsApi.ErrorData>(content);
				return error?.Message;
			}
			catch
			{
				return null;
			}
		}
	}
}