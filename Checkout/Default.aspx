<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Checkout.Default" %>

<!DOCTYPE html>
<html>
<head>
	<meta charset="utf-8" />
	<meta name="viewport" content="width=device-width, initial-scale=1.0" />
	<meta http-equiv="X-UA-Compatible" content="IE=Edge" />
	<title>Blackbaud Checkout Demo </title>
</head>
<body>
	<form action="Default.aspx" id="Submit1" method="post" runat="server">
		<h1>Blackbaud Checkout Demo</h1>
		Please select an amount:
	<select id="amount">
		<option value="25">$25</option>
		<option value="50">$50</option>
		<option value="100">$100</option>
	</select>
		<button id="donate-now">Donate now!</button>
		<asp:Button ID="SubmitBTN" Visible="false" UseSubmitBehavior="true" OnClick="Page_Load" runat="server" />
		<asp:HiddenField ID="transactionDetails" runat="server" />
		<script>
			// Utility function to handle checkout events and display messages
			function handleCheckoutEvent(eventType, details = null) {
				const messageTypes = {
					ready: 'Checkout is ready',
					loaded: 'Checkout form has been loaded',
					cancel: 'Transaction was cancelled',
					complete: 'Transaction completed successfully',
					error: 'An error occurred'
				};

				let message = messageTypes[eventType] || 'Unknown event';

				// Add details for specific events
				if (eventType === 'complete' && details) {
					message += ` - Transaction Token: ${details.transactionToken}`;
				} else if (eventType === 'error' && details) {
					message += ` - Error Code: ${details.errorCode}, Error Text: ${details.errorText}`;
				}

				console.log(`Checkout Event: ${message}`);

				// You could also update UI elements here if needed
				// For example, show a success message or error alert

				if (eventType === 'cancel' || eventType === 'complete' || eventType === 'error') {
					// Serialize the e.detail object to a JSON string
					var transactionDetails = JSON.stringify(details);

					// Set the hidden input field value
					document.getElementById('<% =transactionDetails.UniqueID %>').value = transactionDetails;

					// Submit the form
					document.getElementById('Submit1').submit();
				}
			}

			document.addEventListener('DOMContentLoaded', function () {

				// create the transaction object
				let transactionData = {
					key: '0c38c5cc-baf2-405e-a7da-60912a40fd86',
					payment_configuration_id: 'f9fd6119-95f5-4940-bba1-038b4ac23dea',
					primary_color: '#569BBE',
					'billing_address_city': 'Charleston',
					'billing_address_country': 'US',
					'billing_address_email': 'test@blackbaud.com',
					'billing_address_line': '2000 Daniel Island Drive',
					'billing_address_phone': '555-555-5555',
					'billing_address_post_code': '29492',
					'billing_address_state': 'SC',
					is_email_required: 1,
					is_name_required: 1
				};

				document.getElementById('donate-now').addEventListener('click', function (e) {
					e.preventDefault();

					// append any donor-entered information to the transaction obejct
					transactionData.Amount = document.getElementById('amount').value;

					// call the Checkout method to display the payment form
					Blackbaud_OpenCardNotPresentForm(transactionData);
				});

				document.addEventListener('checkoutReady', function () {
					// handle Ready event
					handleCheckoutEvent('ready', transactionData);
				});

				document.addEventListener('checkoutLoaded', function () {
					// handle Loaded event
					handleCheckoutEvent('loaded', transactionData);
				});

				document.addEventListener('checkoutCancel', function () {
					// handle Cancel event
					handleCheckoutEvent('cancel', transactionData);
				});

				document.addEventListener('checkoutComplete', function (e) {
					// handle Complete event
					//console.log('transaction token: ', e.detail.transactionToken);
					handleCheckoutEvent('complete', e.detail);
				});

				document.addEventListener('checkoutError', function (e) {
					// handle Error event
					//	console.log('error text: ', e.detail.errorText);
					//	console.log('error code: ', e.detail.errorCode);
					handleCheckoutEvent('error', e.detail);
				});
			});
		</script>

		<script src="https://payments.blackbaud.com/Checkout/bbCheckout.2.0.js"></script>
	</form>
</body>
</html>
