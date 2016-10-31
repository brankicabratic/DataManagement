using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DataManagement.Validation
{
	public class ValidationRuleRange : ValidationRule
	{
		private double min, max;

		public ValidationRuleRange(string validationValue)
		{
			min = int.MinValue;
			max = int.MaxValue;
			string[] splitted = validationValue.Split('-');
			if (splitted.Length > 0)
				double.TryParse(splitted[0], out min);
			if (splitted.Length > 1)
				double.TryParse(splitted[1], out max);
		}

		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			double doubleValue;
			string strValue = value.ToString();
			if (!double.TryParse(strValue, out doubleValue))
			{
				return new ValidationResult(false, Properties.Resources.Validation_NotANumber);
			}
			else if (doubleValue < min || doubleValue > max)
			{
				return new ValidationResult(false, Properties.Resources.Validation_Range_NumberOutOfBounds.Replace("%v1%", min.ToString()).Replace("%v2%", max.ToString()));
			}

			return new ValidationResult(true, null);
		}
	}
}
