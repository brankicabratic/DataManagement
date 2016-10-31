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
	public class ConditionalTextField : Field
	{
		private static string FalseAnswer = "---";

		[XmlIgnore]
		public string Value { get { return textValue; } set { textValue = value; } }
		[XmlIgnore]
		public bool BoolValue { get { return boolValue; } set { boolValue = value; } }

		[XmlIgnore]
		public string SubName
		{
			get
			{
				if (!string.IsNullOrEmpty(subName)) return subName;
				if (string.IsNullOrEmpty(Id)) return "";

				Field field = CorrespondingField;
				return field != null && field is ConditionalTextField ? (field as ConditionalTextField).subName : "";
			}
		}

		private string subName;

		private string textValue;
		private bool boolValue;

		public ConditionalTextField() : this("", "", "", ValidationType.None, "") { }

		public ConditionalTextField(string id, string name, string subName, ValidationType validation, string validationValue) : base(id, name, validation, validationValue)
		{
			this.subName = subName;
		}

		public override FieldControl GenerateUIElement()
		{
			ConditionalTextFieldControl control = new ConditionalTextFieldControl();
			control.Initialize(this);
			return control;
		}

		public override string GetValue()
		{
			return boolValue ? textValue : FalseAnswer;
		}

		public override string GetDocOutput()
		{
			return boolValue ? textValue : "-";
		}

		public override void SetValue(string value)
		{
			boolValue = value != FalseAnswer;
			if (boolValue)
				textValue = value;
		}
	}
}
