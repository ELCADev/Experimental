namespace CheckoutAuthCodeGrant.Models.PaymentsApi
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    /// <summary>
    /// Data model for payment configurations from Payments API.
    /// </summary>
    /// <remarks>
    /// https://developer.sky.blackbaud.com/docs/services/payments/operations/ListPaymentConfiguration
    /// </remarks>
    public class PaymentConfigurationsData
    {
        [JsonProperty("value")]
        public List<PaymentConfigurationData> Value { get; set; }
    }
}