namespace CheckoutAuthCodeGrant.Models.PaymentsApi
{
    using Newtonsoft.Json;

    /// <summary>
    /// Request body for checking out a transaction from Payments API.
    /// </summary>
    /// <remarks>
    /// https://developer.sky.blackbaud.com/docs/services/payments/operations/CreateCheckoutTransaction
    /// </remarks>
    public class TransactionBody
    {
        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("authorization_token")]
        public string AuthorizationToken { get; set; }
    }
}