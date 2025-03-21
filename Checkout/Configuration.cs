namespace CheckoutAuthCodeGrant
{
    /// <summary>
    /// Provide application configuration.
    /// </summary>
    public static class Configuration
    {
        /// <summary>
        /// The application's Sky API callback URI.
        /// </summary>
        public static string ApplicationCallbackUri = "https://localhost:44301/auth/callback";

        /// <summary>
        /// The application's Sky API identifier.
        /// </summary>
        public static string ApplicationId = "00000000-0000-0000-0000-000000000000";

        /// <summary>
        /// The application's Sky API secret.
        /// </summary>
        public static string ApplicationSecret = "/SeCrEt=";

        /// <summary>
        /// The Sky API subscription key to use when making Payments API requests.
        /// </summary>
        public static string SubscriptionKey = "a000000000000000000000000000000c";
    }
}