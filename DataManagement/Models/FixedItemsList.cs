using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataManagement.Controls;
using System.Windows.Controls;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

namespace DataManagement.Models
{
	[Serializable]
	public class FixedItemsList : Field
	{
		private static string FalseAnswer = "---";

		[XmlIgnore]
		public List<Field> Items = new List<Field>();
		[XmlIgnore]
		public bool IsConditional
		{
			get
			{
				return (DataStructure.GetAllFields(true)[Id] as FixedItemsList).isConditional;
			}
		}
		[XmlIgnore]
		public bool BoolValue { get; set; }

		private bool isConditional;

		public FixedItemsList() { }

		public FixedItemsList(string id, string name, ValidationType validation, string validationValue) : base(id, name, validation, validationValue) { }

		public override FieldControl GenerateUIElement()
		{
			FixedItemsListControl itemsListControl = new FixedItemsListControl();
			itemsListControl.Initialize(this);
			return itemsListControl;
		}

		public override string GetValue()
		{
			if (IsConditional && !BoolValue)
			{
				return FalseAnswer;
			}
			else if (Items != null)
			{
				XmlSerializer xsSubmit = new XmlSerializer(typeof(List<Field>));
				StringWriter sww = new StringWriter();
				using (XmlWriter writer = XmlWriter.Create(sww, new XmlWriterSettings { OmitXmlDeclaration = true }))
				{
					xsSubmit.Serialize(writer, Items);				
				}
				return sww.ToString();
			}
			return "";
		}

		public override string GetDocOutput()
		{
			if (IsConditional && !BoolValue) return "-";

			StringBuilder sb = new StringBuilder();
		
			foreach (Field field in Items)
			{
				sb.Append(field.GetDocOutput() + "\n\n");
			}

			return sb.ToString();
		}

		public override void SetValue(string value)
		{
			BoolValue = !IsConditional || !string.IsNullOrEmpty(value) && value != FalseAnswer;

			if (value == null) return;

			if (BoolValue)
			{
				XmlSerializer serializer = new XmlSerializer(typeof(List<Field>));

				XmlReader reader = XmlReader.Create(new StringReader(value));

				Items = (List<Field>)serializer.Deserialize(reader);
			}
		}

		public void SetIsConditional(bool isConditional)
		{
			this.isConditional = isConditional;
		}
	}
}
