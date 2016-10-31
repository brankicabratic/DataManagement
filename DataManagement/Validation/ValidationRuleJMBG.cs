using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DataManagement.Validation
{
	public class ValidationRuleJMBG : ValidationRule
	{
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			long longValue;
			string strValue = value.ToString();
			if (!long.TryParse(strValue, out longValue))
			{
				return new ValidationResult(false, Properties.Resources.Validation_NotANumber);
			}
			else if (strValue.Length != 13)
			{
				return new ValidationResult(false, Properties.Resources.Validation_JMBG_InvalidLength);
			}

			return new ValidationResult(true, null);
		}
	}
}
