using DataManagement.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace DataManagement.Models
{
    public enum ValidationType { None, JMBG, Range }

	[Serializable]
	[XmlInclude(typeof(TextField))]
	[XmlInclude(typeof(DateField))]
	[XmlInclude(typeof(TextBlock))]
	[XmlInclude(typeof(CheckBox))]
	[XmlInclude(typeof(ComboBox))]
	[XmlInclude(typeof(ConditionalTextField))]
	[XmlInclude(typeof(FixedItemsList))]
	[XmlInclude(typeof(ExternalDataStructureList))]
	[XmlInclude(typeof(ItemsList<TextField>))]
	[XmlInclude(typeof(ItemsList<TextBlock>))]
	[XmlInclude(typeof(ItemsList<CheckBox>))]
	[XmlInclude(typeof(ItemsList<ComboBox>))]
	[XmlInclude(typeof(ItemsList<ConditionalTextField>))]
	public abstract class Field
	{
		private static string TextFieldEl = "TextField";
		private static string DateFieldEl = "DateField";
		private static string TextBlockEl = "TextBlock";
		private static string ItemsListEl = "ItemsList";
		private static string FixedItemsListEl = "FixedItemsList";
		private static string ConditionalTextFieldEl = "ConditionalTextField";
		private static string CheckBoxEl = "CheckBox";
		private static string ComboBoxEl = "ComboBox";
		private static string ComboBoxItemEl = "ComboBoxItem";
		private static string ExternalDataStructureListEl = "ExternalDataStructureList";

		[XmlAttribute]
		public string Id;
		[XmlIgnore]
		public string Name
		{
			get
			{
				if (!string.IsNullOrEmpty(name)) return name;
				if (string.IsNullOrEmpty(Id)) return "";

				Field field = CorrespondingField;
				return field != null ? field.name : null;
			}
		}
		[XmlIgnore]
		public ValidationType Validation
		{
			get
			{
				if (validation != ValidationType.None) return validation;

				Field field = CorrespondingField;
				return field != null ? field.validation : ValidationType.None;
			}
		}
		[XmlIgnore]
		public string ValidationValue
		{
			get
			{
				if (!String.IsNullOrEmpty(validationValue)) return validationValue;

				Field field = CorrespondingField;
				return field != null ? field.validationValue : "";
			}
		}
		[XmlIgnore]
		public bool IsNested;

		[XmlText]
		public string ValueXml { get { return GetValue(); } set { SetValue(value); } }

		protected Field CorrespondingField
		{
			get
			{
				Field field;
				if (DataStructure.GetAllAppFields().TryGetValue(Id, out field))
				{
					return field;
				}
				return null;
			}
		}

		private string name;
		private ValidationType validation;
		private string validationValue;
		
		public Field() { }

        public Field(string id, string name, ValidationType validation, string validationValue)
        {
            Id = id;
            this.name = name;
            this.validation = validation;
			this.validationValue = validationValue;
        }

		public void SetName(string name)
		{
			this.name = name;
		}

		public void SetValidationType(ValidationType validation)
		{
			this.validation = validation;
		}

		public static Field CreateField(XmlNode node, Action<Field> callback, string idPrefix = "")
        {
			Field newField = null;
			string nodeName = node.Attributes[DataStructure.NameAtt].Value;
			string nodeId = idPrefix + node.Attributes[DataStructure.IdAtt].Value;
			ValidationType validationType = ValidationType.None;
			if (node.Attributes[DataStructure.ValidationAtt] != null)
			{
				ValidationType? parsedType = GetValidationType(node.Attributes[DataStructure.ValidationAtt].Value);
				if (parsedType == null) return null;
				validationType = parsedType.Value;
			}
			string validationValue = "";
			if (node.Attributes[DataStructure.ValidationValueAtt] != null)
				validationValue = node.Attributes[DataStructure.ValidationValueAtt].Value;

			if (node.Name == TextFieldEl)
			{
				newField = new TextField(nodeId, nodeName, validationType, validationValue);
			}
			else if (node.Name == DateFieldEl)
			{
				newField = new DateField(nodeId, nodeName, validationType, validationValue);
			}
			else if (node.Name == TextBlockEl)
			{
				newField = new TextBlock(nodeId, nodeName, validationType, validationValue);
			}
			else if (node.Name == ItemsListEl)
			{
				string elementTypeStr = node.Attributes[DataStructure.ListElementTypeAtt].Value;
				Type type = Type.GetType("DataManagement.Models." + elementTypeStr);
				var genericListType = typeof(ItemsList<>);
				var specificListType = genericListType.MakeGenericType(type);
				newField = Activator.CreateInstance(specificListType) as Field;
				newField.Id = nodeId;
				newField.SetName(nodeName);
				newField.SetValidationType(validationType);
			}
			else if (node.Name == ConditionalTextFieldEl)
			{
				string subName = node.Attributes[DataStructure.SubNameAtt].Value;
				newField = new ConditionalTextField(nodeId, nodeName, subName, validationType, validationValue);
			}
			else if (node.Name == CheckBoxEl)
			{
				newField = new CheckBox(nodeId, nodeName, validationType, validationValue);
			}
			else if (node.Name == ComboBoxEl)
			{				
				List<string> possibleValues = new List<string>();
				foreach (XmlNode childNode in node)
				{
					if (childNode.Name == ComboBoxItemEl)
					{
						possibleValues.Add(childNode.InnerText);
					}
				}
				string defaultValueIndexStr = node.Attributes[DataStructure.DefaultAtt].Value;
				int defaultValueIndex = 0;
				try
				{
					defaultValueIndex = int.Parse(defaultValueIndexStr);
				}
				catch
				{
					DataStructure.MainDataStructure.LogError("Default value in combo box " + nodeId + " has to be number.");
				}
				newField = new ComboBox(nodeId, nodeName, validationType, validationValue, possibleValues, defaultValueIndex);
			}
			else if (node.Name == FixedItemsListEl)
			{
				FixedItemsList fixedItemsListFied = new FixedItemsList(nodeId, nodeName, validationType, validationValue);
				fixedItemsListFied.Items = new List<Field>();
				foreach (XmlNode childNode in node)
				{
					fixedItemsListFied.Items.Add(CreateField(childNode, callback, nodeId + "."));
				}
				var attribute = node.Attributes[DataStructure.ConditionalAtt];

				fixedItemsListFied.SetIsConditional(attribute != null && attribute.Value.ToLower() == "true");

				newField = fixedItemsListFied;
			}
			else if (node.Name == ExternalDataStructureListEl)
			{
				XmlAttribute fileLocationAtt = node.Attributes[DataStructure.FileLocationAtt];
				XmlAttribute docOutputMainFieldIDAtt = node.Attributes[DataStructure.DocOutputMainFieldIDAtt];
				XmlAttribute docOutputFieldIDsAtt = node.Attributes[DataStructure.DocOutputFieldIDsAtt];
				string fileLocation = fileLocationAtt != null ? fileLocationAtt.Value : "";
				string docOutputMainFieldID = docOutputMainFieldIDAtt != null ? docOutputMainFieldIDAtt.Value : "";
				string docOutputFieldIDs = docOutputFieldIDsAtt != null ? docOutputFieldIDsAtt.Value : "";
				newField = new ExternalDataStructureList(nodeId, fileLocation, docOutputMainFieldID, docOutputFieldIDs);
			}

			if (!string.IsNullOrEmpty(idPrefix)) newField.IsNested = true;

			if (newField != null && callback != null) callback(newField);

			return newField;
        }

		public abstract FieldControl GenerateUIElement();
		public abstract void SetValue(string value);
		public abstract string GetValue();
		public abstract string GetDocOutput();

		private static ValidationType? GetValidationType(string attributeValue)
		{
			ValidationType validation;
			if (Enum.TryParse<ValidationType>(attributeValue, out validation))
			{
				return validation;
			}
			return null;
		}
	}
}
