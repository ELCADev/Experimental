namespace CheckoutAuthCodeGrant.Models.PaymentsApi
{
    using Newtonsoft.Json;

    /// <summary>
    /// Data model for errors from Payments API.
    /// </summary>
    /// <remarks>
    /// Refer to the error response codes for various operations, for instance
    /// https://developer.sky.blackbaud.com/docs/services/payments/operations/CreateCheckoutTransaction
    /// </remarks>
    public class ErrorData
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
