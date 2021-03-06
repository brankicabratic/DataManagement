﻿using DataManagement.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace DataManagement.Models
{
	[Serializable]
	public class DateField : Field
	{
		[XmlIgnore]
		private static readonly CultureInfo cultureInfo = new CultureInfo("de");

		[XmlIgnore]
		public DateTime DateValue { get { return textValue == null ? DateTime.Now : DateTime.Parse(textValue, cultureInfo); } set { this.textValue = value.ToString("d", cultureInfo); } }

		private string textValue
		{
			set;
			get;
		}

		public DateField() { }

		public DateField(string id, string name, ValidationType validation, string validationValue, bool? includeInExcelExport) : base(id, name, validation, validationValue, includeInExcelExport) { }

		public override FieldControl GenerateUIElement(bool isForEditing)
		{
			DateFieldControl control = new DateFieldControl();
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
			return DateValue.ToString();
		}

		public override void SetValue(string value)
		{
			textValue = value;
		}
	}
}
