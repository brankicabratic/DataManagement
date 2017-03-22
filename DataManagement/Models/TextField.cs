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
	public class TextField : Field
	{
		[XmlIgnore]
		public string Value { get { return textValue; } set { this.textValue = value; } }

		private string textValue
		{
			set;
			get;
		}

		public TextField() { }

		public TextField(string id, string name, ValidationType validation, string validationValue, bool? includeInExcelExport) : base(id, name, validation, validationValue, includeInExcelExport) { }

		public override FieldControl GenerateUIElement(bool isForEditing)
		{
			TextFieldControl control = new TextFieldControl();
			control.Initialize(this);
			return control;
		}

		public override string GetValue()
		{
			return textValue;
		}

		public override string GetDocOutput()
		{
			return GetValue();
		}

		public override object GetXslOutput()
		{
			switch (Validation)
			{
				case ValidationType.Range:
					float numericalValue;
					if (float.TryParse(textValue, out numericalValue))
					{
						return numericalValue;
					}
					break;
			}

			return textValue;
		}

		public override void SetValue(string value)
		{
			textValue = value;
		}
	}
}
