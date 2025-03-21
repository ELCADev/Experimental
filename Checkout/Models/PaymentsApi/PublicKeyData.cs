namespace CheckoutAuthCodeGrant.Models.PaymentsApi
{
    using Newtonsoft.Json;

    /// <summary>
    /// Data model for a public key from Payments API.
    /// </summary>
    /// <remarks>
    /// https://developer.sky.blackbaud.com/docs/services/payments/operations/GetPublicKey
    /// </remarks>
    public class PublicKeyData
    {
        [JsonProperty("public_key")]
        public string Value { get; set; }
    }
}
