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
using System.Collections.ObjectModel;
using System.Collections;

namespace DataManagement.Models
{
	[Serializable]
	public class ExternalDataStructureList : Field
	{
		[XmlIgnore]
		public DataStructure DataStructure
		{
			get
			{
				return (DataStructure.GetAllAppFields()[Id] as ExternalDataStructureList).dataStructure;
			}
		}
		[XmlIgnore]
		public ObservableCollection<DataItem> Items { get { return items; } set { items = value; } }
		[XmlIgnore]
		private string DocOutputMainFieldID
		{
			get
			{
				return (DataStructure.GetAllAppFields()[Id] as ExternalDataStructureList).docOutputMainFieldID;
			}
		}
		[XmlIgnore]
		public List<string> DocOutputFieldIDs
		{
			get
			{
				return (DataStructure.GetAllAppFields()[Id] as ExternalDataStructureList).docOutputFieldIDs;
			}
		}

		private DataStructure dataStructure;
		private ObservableCollection<DataItem> items = new ObservableCollection<DataItem>();

		private string docOutputMainFieldID;
		private List<string> docOutputFieldIDs;

		public ExternalDataStructureList()
		{
			IsLeaf = false;
		}

		public ExternalDataStructureList(string id, string fileLocation, string docOutputMainFieldID, string docOutputFieldIDs) : base(id, "", ValidationType.None, "", null)
		{
			IsLeaf = false;
			this.docOutputMainFieldID = docOutputMainFieldID;
			this.docOutputFieldIDs = docOutputFieldIDs.Split(';').ToList();
			dataStructure = DataStructure.LoadDataStructure(fileLocation, false, id + "_");
		}

		public override FieldControl GenerateUIElement(bool isForEditing)
		{
			ExternalDataStructureListControl control = new ExternalDataStructureListControl();
			control.Initialize(this);
			return control;
		}

		public override string GetValue()
		{
			if (Items != null)
			{
				XmlSerializer xsSubmit = new XmlSerializer(typeof(List<DataItem>));
				StringWriter sww = new StringWriter();
				using (XmlWriter writer = XmlWriter.Create(sww, new XmlWriterSettings { OmitXmlDeclaration = true }))
				{
					xsSubmit.Serialize(writer, Items.ToList());				
				}
				return sww.ToString();
			}
			return "";
		}

		public override string GetDocOutput()
		{
			if (Items == null) return "-";

			StringBuilder sb = new StringBuilder();
		
			foreach (DataItem item in Items)
			{
				Field mainField = item.GetField(DocOutputMainFieldID);
				sb.Append(mainField.GetDocOutput() + "\n");
				foreach (string fieldId in DocOutputFieldIDs)
				{
					Field field = item.GetField(fieldId);
					sb.Append("\t" + field.GetDocOutput().Replace("\n", "\n\t") + "\n");
				}
				sb.Append("\n");
			}

			return sb.ToString();
		}

		public override object GetXslOutput()
		{
			return "";
		}

		public override void SetValue(string value)
		{
			if (value == null) return;
			
			XmlSerializer serializer = new XmlSerializer(typeof(List<DataItem>));
			XmlReader reader = XmlReader.Create(new StringReader(value));
			Items = new ObservableCollection<DataItem>((List<DataItem>)serializer.Deserialize(reader));
			
		}

		public void RemoveItems(IList items)
		{			
			for (int i = items.Count - 1; i >= 0; i--)
			{
				DataItem dataItem = items[i] as DataItem;
				if (dataItem != null)
					this.items.Remove(dataItem);
			}
			Data.SaveData();
		}
	}
}
