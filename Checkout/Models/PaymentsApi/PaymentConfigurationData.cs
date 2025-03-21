namespace CheckoutAuthCodeGrant.Models.PaymentsApi
{
    using Newtonsoft.Json;

    /// <summary>
    /// Data model for a payment configuration from Payments API.
    /// </summary>
    /// <remarks>
    /// https://developer.sky.blackbaud.com/docs/services/payments/operations/GetPaymentConfiguration
    /// </remarks>
    public class PaymentConfigurationData
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("process_mode")]
        public string ProcessMode { get; set; }
    }
}