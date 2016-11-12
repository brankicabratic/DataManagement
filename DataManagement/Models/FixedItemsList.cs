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
				Field field;
				if (DataStructure.GetAllAppFields().TryGetValue(Id, out field))
					return (field as FixedItemsList).isConditional;
				else
					return isConditional;
			}
		}
		[XmlIgnore]
		public bool BoolValue { get; set; }

		private bool isConditional;

		public FixedItemsList() { }

		public FixedItemsList(string id, string name, ValidationType validation, string validationValue, bool? includeInExcelExport) : base(id, name, validation, validationValue, includeInExcelExport)
		{
			IsLeaf = IsConditional;
		}

		public Field GetItem(string id)
		{			
			Field field = null;
			string startId = id;
			while (field == null && id != "")
			{
				field = Items.Find(x => x.Id == id);
				if (field == null)
				{
					int index = Math.Max(id.LastIndexOf("_"), id.LastIndexOf("."));
					id = index >= 0 ? id.Substring(0, index) : "";
				}
			}

			if (field == null) return null;
			
			if (field.Id != startId && field is FixedItemsList)
			{
				return (field as FixedItemsList).GetItem(startId);
			}
			else if (field.Id == id)
			{
				return field;
			}
			else
			{
				return null;
			}
		}

		public override FieldControl GenerateUIElement(bool isForEditing)
		{
			FixedItemsListControl itemsListControl = new FixedItemsListControl();
			itemsListControl.Initialize(this, isForEditing);
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

			bool first = true;
			foreach (Field field in Items)
			{
				if (first)
					first = false;
				else
					sb.Append("\n\n");
				sb.Append(field.GetDocOutput());
			}

			return sb.ToString();
		}

		public override object GetXslOutput()
		{
			return BoolValue ? 1 : 0;
		}

		public override void SetValue(string value)
		{
			IsLeaf = IsConditional;

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
			IsLeaf = isConditional;
		}
	}
}
