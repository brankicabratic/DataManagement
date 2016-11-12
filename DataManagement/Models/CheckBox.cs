using DataManagement.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace DataManagement.Models
{
	[Serializable]
	public class CheckBox : Field
	{
		[XmlIgnore]
		public bool Value { get { return boolValue; } set { boolValue = value; } }

		private bool boolValue;
		
		public CheckBox() { }

		public CheckBox(string id, string name, ValidationType validation, string validationValue, bool? includeInExcelExport) : base(id, name, validation, validationValue, includeInExcelExport) { }

		public override FieldControl GenerateUIElement(bool isForEditing)
		{
			CheckBoxControl control = new CheckBoxControl();
			control.Initialize(this);
			return control;
		}

		public override string GetValue()
		{
			return boolValue ? "1" : "0";
		}

		public override string GetDocOutput()
		{
			return boolValue ? Properties.Resources.Yes : Properties.Resources.No;
		}

		public override object GetXslOutput()
		{
			return boolValue ? 1 : 0;
		}

		public override void SetValue(string value)
		{
			boolValue = value != "0";
		}

	}
}
