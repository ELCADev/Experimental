namespace CheckoutAuthCodeGrant.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// View model for displaying errors.
    /// </summary>
    public class ErrorsViewModel
    {
        public ErrorsViewModel()
        {
            Errors = new List<ErrorViewModel>();
        }

        public IList<ErrorViewModel> Errors { get; }
    }
}