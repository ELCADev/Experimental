namespace CheckoutAuthCodeGrant
{
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    /// <summary>
    /// Manages authentication between this application and Sky API.
    /// </summary>
    public class SkyApiAuthenticator
    {
        /// <summary>
        /// The token used to issue requests against Sky API.
        /// </summary>
        /// <remarks>
        /// Expires after 60 minutes.
        /// </remarks>
        public static string AccessToken { get; private set; }

        /// <summary>
        /// The token used to get a new Sky API access token.
        /// </summary>
        /// <remarks>
        /// Expires after 60 days but automatically refreshes if used to request a new access token.
        /// If we do not make a call to get a new access token with 60 days an organization user must revisit the login page to reauthenticate with Sky API.
        /// </remarks>
        public static string RefreshToken { get; private set; }

        /* !
            Note that tokens are stored in variables which means they are stored in memory for the lifetime of this application.
            If this application process ends, the organization user must revisit the login page to reauthenticate with Sky API.  You may chose to store these values
            in persistent storage and retreive them when the application starts.
           !
            Note that only one set of tokens are stored.
            All functionality driven through Sky API will use a single organization's tokens.  You may chose to store mutliple sets of tokens per organization if
            this application were providing functionality to multiple organizations simultaneously.
        */

        /// <summary>
        /// Clear any data pertaining to making requests against Sky API.
        /// </summary>
        public static void Logout()
        {
            AccessToken = "";
            RefreshToken = "";
        }

        /// <summary>
        /// Refreshes the OAuth tokens needed to make requests against Sky API.
        /// </summary>
        public static async Task RefreshTokens()
        {
            using (var client = new HttpClient())
            {
                // Refer to https://developer.blackbaud.com/skyapi/docs/authorization/auth-code-flow#refresh-access-token for endpoint documentation.
                var skyApiTokenUrl = $"https://oauth2.sky.blackbaud.com/token";

                var requestBody = new Dictionary<string, string>()
                {
                    { "grant_type", "refresh_token" },
                    { "refresh_token", RefreshToken }
                };

                var password = Utility.EncodeBase64($"{Configuration.ApplicationId}:{Configuration.ApplicationSecret}");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", password);

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

                var response = await client.PostAsync(skyApiTokenUrl, new FormUrlEncodedContent(requestBody));

                if (response.IsSuccessStatusCode)
                {
                    var tokens = JsonConvert.DeserializeObject<Dictionary<string, string>>(await response.Content.ReadAsStringAsync());
                    AccessToken = tokens["access_token"];
                    RefreshToken = tokens["refresh_token"];
                }
            }
        }

        /// <summary>
        /// Requests the Sky API authorization page.
        /// </summary>
        public static async Task<HttpResponseMessage> RequestSkyApiAuthorization()
        {
            using (var client = new HttpClient())
            {
                var skyApiTokenUrl = $"https://oauth2.sky.blackbaud.com/authorization?client_id={Configuration.ApplicationId}&response_type=code&redirect_uri={Configuration.ApplicationCallbackUri}";

                return await client.GetAsync(skyApiTokenUrl);
            }
        }

        /// <summary>
        /// Requests and stores the OAuth tokens needed to make requests against Sky API given an organization user's authentication code.
        /// </summary>
        public static async Task RequestTokens(string code)
        {
            using (var client = new HttpClient())
            {
                // Refer to https://developer.blackbaud.com/skyapi/docs/authorization/auth-code-flow#request-tokens for endpoint documentation.
                var skyApiTokenUrl = $"https://oauth2.sky.blackbaud.com/token";

                var requestBody = new Dictionary<string, string>()
                {
                    { "grant_type", "authorization_code" },
                    { "code", code },
                    { "redirect_uri", Configuration.ApplicationCallbackUri }
                };

                var password = Utility.EncodeBase64($"{Configuration.ApplicationId}:{Configuration.ApplicationSecret}");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", password);

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

                var response = await client.PostAsync(skyApiTokenUrl, new FormUrlEncodedContent(requestBody));

                if (response.IsSuccessStatusCode)
                {
                    //Refer to https://developer.blackbaud.com/skyapi/docs/authorization/auth-code-flow#tokens-returned for response content documentation.
                    var tokens = JsonConvert.DeserializeObject<Dictionary<string, string>>(await response.Content.ReadAsStringAsync());
                    AccessToken = tokens["access_token"];
                    RefreshToken = tokens["refresh_token"];
                }
            }
        }
    }
}