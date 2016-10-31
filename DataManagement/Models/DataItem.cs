using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

		public DataItem(bool initializeFields) : this()
		{
			if (initializeFields)
			{
				Dictionary<string, Field> allFields = DataStructure.GetAllFields(false);
				foreach (string key in allFields.Keys)
				{
					Fields.Add(ObjectCopier.Clone<Field>(allFields[key]));
				}
			}
		}

		public void CoppyFields(DataItem dataItem)
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
				if (DataStructure.GetAllFields(true).TryGetValue(id, out dsField))
				{
					field = ObjectCopier.Clone<Field>(dsField);
					Fields.Add(field);
				}
			}
			return field;
		}
	}
}
