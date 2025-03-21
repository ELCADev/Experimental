namespace CheckoutAuthCodeGrant
{
    using System;
    using System.Text;

    /// <summary>
    /// Provide shared utility methods.
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Convert a string to its base-64 representation.
        /// </summary>
        public static string EncodeBase64(string s)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(s));
        }
    }
}