namespace CheckoutAuthCodeGrant.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// View model for the login page.
    /// </summary>
    public class LoginViewModel
    {
        public LoginViewModel()
        {
            ErrorsViewModel = new ErrorsViewModel();
        }

        [Display(Name = "Sky API access token")]
        public string AccessToken { get; set; }

        public ErrorsViewModel ErrorsViewModel { get; }

        public bool IsLoggedIn { get { return !string.IsNullOrWhiteSpace(AccessToken); } }

        [Display(Name = "Sky API refresh token")]
        public string RefreshToken { get; set; }
    }
}