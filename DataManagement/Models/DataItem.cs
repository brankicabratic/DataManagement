using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace DataManagement.Models
{
	[Serializable]
	public class DataItem
	{
		public List<Field> Fields;

		public DataItem()
		{
			Fields = new List<Field>();
		}

		public DataItem(DataStructure dataStructure) : this()
		{
			if (dataStructure != null)
			{
				Dictionary<string, Field> allFields = dataStructure.GetAllFields(false);
				int nullCount = 0;
				foreach (string key in allFields.Keys)
				{
					if (allFields[key] == null) { nullCount++; MessageBox.Show(null, "Null za: " + key, "Greška"); }
					Fields.Add(ObjectCopier.Clone<Field>(allFields[key]));
				}
				
			}
		}

		public void CopyFields(DataItem dataItem)
		{
			foreach (Field f in Fields)
			{
				Field f2 = dataItem.Fields.Find(x => x.Id == f.Id);
				if (f2 == null) return;
				f.SetValue(f2.GetValue());
			}

			foreach (Field f in dataItem.Fields)
			{
				if (Fields.Find(x => x.Id == f.Id) == null)
				{
					Fields.Add(ObjectCopier.Clone<Field>(f));
				}
			}
		}

		public Field GetField(string id)
		{
			Field field = Fields.Find(x => x.Id == id);

			if (field == null)
			{
				Field dsField;
				if (DataStructure.GetAllAppFields().TryGetValue(id, out dsField))
				{
					field = ObjectCopier.Clone<Field>(dsField);
					Fields.Add(field);
				}
			}
			return field;
		}

		public Field GetFieldWithParentReturn(string id)
		{
			Field field = null;
			while (field == null && id != "")
			{
				field = Fields.Find(x => x.Id == id);
				if (field == null)
				{
					int index = Math.Max(id.LastIndexOf("_"), id.LastIndexOf("."));
					id = index >= 0 ? id.Substring(0, index) : "";
				}
			}
			return field;
		}
	}
}
