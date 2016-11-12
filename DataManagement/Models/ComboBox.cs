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
	public class ComboBox : Field
	{
		[XmlIgnore]
		public string Value { get { return textValue; } set { textValue = value; } }

		[XmlIgnore]
		public int Index
		{
			set
			{
				if (value >= 0 && value < PossibleValues.Count)
					textValue = PossibleValues[value];
			}

			get
			{
				return PossibleValues.IndexOf(textValue);
			}
		}

		[XmlIgnore]
		public int DefaultValueIndex = 0;
		[XmlIgnore]
		public List<string> PossibleValues
		{
			get
			{
				if (possibleValues != null) return possibleValues;

				return (DataStructure.GetAllAppFields()[Id] as ComboBox).PossibleValues;
			}

			set
			{
				possibleValues = value;
			}
		}
		[XmlIgnore]
		public bool IsMandatory
		{
			get
			{
				if (isMandatory == null) return (DataStructure.GetAllAppFields()[Id] as ComboBox).isMandatory.Value;
				return isMandatory.Value;
			}
		}


		private string textValue;
		private List<string> possibleValues;
		private bool? isMandatory;

		public ComboBox() { }

		public ComboBox(string id, string name, ValidationType validation, string validationValue, bool? includeInExcelExport, List<string> possibleValues, int defaultValueIndex, bool isMandatory) : base(id, name, validation, validationValue, includeInExcelExport)
		{
			PossibleValues = possibleValues;
			DefaultValueIndex = defaultValueIndex;
			this.isMandatory = isMandatory;
			if (defaultValueIndex >= 0 && defaultValueIndex < possibleValues.Count)
				textValue = possibleValues[defaultValueIndex];
		}

		public override FieldControl GenerateUIElement(bool isForEditing)
		{
			ComboBoxControl control = new ComboBoxControl();
			control.IsMandatory = !isForEditing && IsMandatory;
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
			return PossibleValues.IndexOf(textValue);
		}

		public override void SetValue(string value)
		{
			textValue = value;
		}
	}
}
