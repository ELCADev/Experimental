namespace CheckoutAuthCodeGrant
{
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    /// <summary>
    /// Issues requests from this application to the Payments API.
    /// </summary>
    public class PaymentsApiClient
    {
        /// <summary>
        /// Issues a request for the Payments API to finalize a Checkout transaction and returns the response.
        /// </summary>
        /// <remarks>
        /// Will retry the request with a refreshed token if authentication fails.
        /// </remarks>
        public static async Task<HttpResponseMessage> ChargeCheckoutTransaction(decimal amount, string token)
        {
            var response = await ChargeCheckoutTransactionInternal(amount, token);

            // If the response failed due to authentication, refresh the Sky API tokens and try again.
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await SkyApiAuthenticator.RefreshTokens();
                response = await GetPublicKeyInternal();
            }

            return response;
        }

        /// <summary>
        /// Issues a request for the Payments API payment configurations and returns the response.
        /// </summary>
        /// <remarks>
        /// Will retry the request with a refreshed token if authentication fails.
        /// </remarks>
        public static async Task<HttpResponseMessage> GetPaymentConfigurations()
        {
            var response = await GetPaymentConfigurationsInternal();

            // If the response failed due to authentication, refresh the Sky API tokens and try again.
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await SkyApiAuthenticator.RefreshTokens();
                response = await GetPaymentConfigurationsInternal();
            }

            return response;
        }

        /// <summary>
        /// Issues a request for the Payments API public key and returns the response.
        /// </summary>
        /// <remarks>
        /// Will retry the request with a refreshed token if authentication fails.
        /// </remarks>
        public static async Task<HttpResponseMessage> GetPublicKey()
        {
            var response = await GetPublicKeyInternal();

            // If the response failed due to authentication, refresh the Sky API tokens and try again.
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await SkyApiAuthenticator.RefreshTokens();
                response = await GetPublicKeyInternal();
            }

            return response;
        }

        /// <summary>
        /// Utility method to build the Sky API authentication header.
        /// </summary>
        private static AuthenticationHeaderValue BuildSkyApiAuthenticationHeader()
        {
            var password = Utility.EncodeBase64($"{Configuration.ApplicationId}:{Configuration.ApplicationSecret}");
            return new AuthenticationHeaderValue("Bearer", SkyApiAuthenticator.AccessToken);
        }

        /// <summary>
        /// Issues a request for the Payments API payment configurations.
        /// </summary>
        private static async Task<HttpResponseMessage> GetPaymentConfigurationsInternal()
        {
            using (var client = new HttpClient())
            {
                //Refer to https://developer.sky.blackbaud.com/docs/services/payments/operations/ListPaymentConfiguration for endpoint documentation.
                var url = "https://api.sky.blackbaud.com/payments/v1/paymentconfigurations";

                client.DefaultRequestHeaders.Authorization = BuildSkyApiAuthenticationHeader();
                client.DefaultRequestHeaders.Add("Bb-Api-Subscription-Key", Configuration.SubscriptionKey);

                return await client.GetAsync(url);
            }
        }

        /// <summary>
        /// Issues a request for the Payments API public key.
        /// </summary>
        private static async Task<HttpResponseMessage> GetPublicKeyInternal()
        {
            using (var client = new HttpClient())
            {
                //Refer to https://developer.sky.blackbaud.com/docs/services/payments/operations/GetPublicKey for endpoint documentation.
                var url = "https://api.sky.blackbaud.com/payments/v1/checkout/publickey";

                client.DefaultRequestHeaders.Authorization = BuildSkyApiAuthenticationHeader();
                client.DefaultRequestHeaders.Add("Bb-Api-Subscription-Key", Configuration.SubscriptionKey);

                return await client.GetAsync(url);
            }
        }

        /// <summary>
        /// Issues a request for Payments API to checkout a transaction.
        /// </summary>
        private static async Task<HttpResponseMessage> ChargeCheckoutTransactionInternal(decimal amount, string token)
        {
            using (var client = new HttpClient())
            {
                //Refer to https://developer.sky.blackbaud.com/docs/services/payments/operations/CreateCheckoutTransaction for endpoint documentation.
                var url = "https://api.sky.blackbaud.com/payments/v1/checkout/transaction";

                client.DefaultRequestHeaders.Authorization = BuildSkyApiAuthenticationHeader();
                client.DefaultRequestHeaders.Add("Bb-Api-Subscription-Key", Configuration.SubscriptionKey);

                var body = new Models.PaymentsApi.TransactionBody
                {
                    Amount = decimal.ToInt32(amount * 100),
                    AuthorizationToken = token
                };

                return await client.PostAsJsonAsync(url, body);
            }
        }
    }
}