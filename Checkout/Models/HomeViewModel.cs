namespace CheckoutAuthCodeGrant.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// View model for the home page.
    /// </summary>
    public class HomeViewModel
    {
        public HomeViewModel()
        {
            ErrorsViewModel = new ErrorsViewModel();
        }

        [Display(Name = "Application callback URI")]
        public string ApplicationCallbackUri { get { return Configuration.ApplicationCallbackUri; } }

        [Display(Name = "Application ID")]
        public string ApplicationId { get { return Configuration.ApplicationId; } }

        [Display(Name = "Application secret is set")]
        public bool ApplicationSecretSet { get { return Configuration.ApplicationSecret.Length > 0; } }

        public ErrorsViewModel ErrorsViewModel { get; }
    }
}