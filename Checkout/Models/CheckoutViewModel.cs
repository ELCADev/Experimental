namespace CheckoutAuthCodeGrant.Models
{
	using CheckoutAuthCodeGrant.Models.PaymentsApi;
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations;
    using System.Linq;

	/// <summary>
	/// View model for the checkout page.
	/// </summary>
	public class CheckoutViewModel
    {
        public CheckoutViewModel()
        {
            ErrorsViewModel = new ErrorsViewModel();
			// Initialize with empty collection
			PaymentConfigurations = new SelectOptions<PaymentConfigurationData>(
				Enumerable.Empty<PaymentConfigurationData>(),
				"Id",
				"Name");
		}

		public ErrorsViewModel ErrorsViewModel { get; }

		public SelectOptions<PaymentConfigurationData> PaymentConfigurations { get; set; }

		[Display(Name = "Public key")]
        public string PublicKey { get; set; }

        [Display(Name = "Payment configuration")]
        public string SelectedPaymentConfigurationId { get; set; }
    }

	/// <summary>
	/// A replacement for MVC's SelectList functionality
	/// </summary>
	/// <typeparam name="T">The type of items in the list</typeparam>
	public class SelectOptions<T>
	{
		private readonly IEnumerable<T> _items;
		private readonly string _dataValueField;
		private readonly string _dataTextField;
		private readonly object _selectedValue;

		/// <summary>
		/// Creates a new SelectOptions instance
		/// </summary>
		/// <param name="items">The collection of items</param>
		/// <param name="dataValueField">The property to use as the value</param>
		/// <param name="dataTextField">The property to use as the display text</param>
		/// <param name="selectedValue">The currently selected value (optional)</param>
		public SelectOptions(IEnumerable<T> items, string dataValueField, string dataTextField, object selectedValue = null)
		{
			_items = items ?? throw new ArgumentNullException(nameof(items));
			_dataValueField = dataValueField ?? throw new ArgumentNullException(nameof(dataValueField));
			_dataTextField = dataTextField ?? throw new ArgumentNullException(nameof(dataTextField));
			_selectedValue = selectedValue;
		}

		/// <summary>
		/// Gets the items in the list
		/// </summary>
		public IEnumerable<T> Items => _items;

		/// <summary>
		/// Gets the property to use as the value
		/// </summary>
		public string DataValueField => _dataValueField;

		/// <summary>
		/// Gets the property to use as the display text
		/// </summary>
		public string DataTextField => _dataTextField;

		/// <summary>
		/// Gets the currently selected value
		/// </summary>
		public object SelectedValue => _selectedValue;

		/// <summary>
		/// Gets the options as a collection of SelectOption objects
		/// </summary>
		public IEnumerable<SelectOption> GetOptions()
		{
			var options = new List<SelectOption>();

			foreach (var item in _items)
			{
				var value = GetPropertyValue(item, _dataValueField)?.ToString();
				var text = GetPropertyValue(item, _dataTextField)?.ToString();
				var isSelected = _selectedValue != null && _selectedValue.ToString() == value;

				options.Add(new SelectOption
				{
					Value = value,
					Text = text,
					Selected = isSelected
				});
			}

			return options;
		}

		private static object GetPropertyValue(object obj, string propertyName)
		{
			var property = obj.GetType().GetProperty(propertyName);
			return property?.GetValue(obj);
		}
	}

	/// <summary>
	/// Represents a single option in a select list
	/// </summary>
	public class SelectOption
	{
		/// <summary>
		/// Gets or sets the option value
		/// </summary>
		public string Value { get; set; }

		/// <summary>
		/// Gets or sets the option display text
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// Gets or sets whether the option is selected
		/// </summary>
		public bool Selected { get; set; }
	}
}