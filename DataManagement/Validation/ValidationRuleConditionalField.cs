using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DataManagement.Validation
{
	public class ValidationRuleConditionalField : ValidationRule
	{
		private CheckBox cb;

		public ValidationRuleConditionalField(CheckBox cb)
		{
			this.cb = cb;
		}

		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			if (cb.IsChecked.Value && (value == null || string.IsNullOrEmpty(value.ToString())))
				return new ValidationResult(false, Properties.Resources.Validation_FieldIsEmpty);

			return new ValidationResult(true, null);
		}
	}
}
