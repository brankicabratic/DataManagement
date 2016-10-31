using DataManagement.Models;
using DataManagement.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace DataManagement.Controls
{
	public abstract class FieldControl: UserControl
	{
		public abstract void SetEditable(bool isEditable);

		protected void AppendValidationToBinding(Binding binding, Field field)
		{
			switch (field.Validation)
			{
				case ValidationType.JMBG:
					binding.ValidationRules.Add(new ValidationRuleJMBG());
					break;
				case ValidationType.Range:
					binding.ValidationRules.Add(new ValidationRuleRange(field.ValidationValue));
					break;
			}
		}
	}
}
