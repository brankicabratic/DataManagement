using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataManagement.Controls;
using System.Windows.Controls;

namespace DataManagement.Models
{
	[Serializable]
	public class ItemsList<FieldType> : Field where FieldType : Field
	{
		private List<FieldType> items = new List<FieldType>();
		[NonSerialized]
		private ItemsListControl itemsListControl;

		private bool isForEditing;

		public override FieldControl GenerateUIElement(bool isForEditing)
		{
			this.isForEditing = isForEditing;

			itemsListControl = new ItemsListControl();
			itemsListControl.Initialize(Name);
			itemsListControl.OnAddClick += AddNewItem;
			itemsListControl.OnRemoveClick += RemoveItem;

			foreach (FieldType item in items)
			{
				itemsListControl.AddNewItem(item.GenerateUIElement(isForEditing));
			}

			return itemsListControl;
		}

		public override string GetValue()
		{
			if (items != null) return string.Join<string>("[ilSep]", items.Select(x => x.GetValue()));
			return "";
		}

		public override string GetDocOutput()
		{
			StringBuilder sb = new StringBuilder();

			foreach (Field field in items)
				sb.Append(field.GetDocOutput() + "\n\n");

			return sb.ToString();
		}

		public override object GetXslOutput()
		{
			return "";
		}

		public override void SetValue(string value)
		{
			if (value == null) return;
			List<string> stringValues = value.Split(new string[] { "[ilSep]" }, StringSplitOptions.None).ToList();
			for (int i = 0; i < stringValues.Count; i++)
			{
				if (i < items.Count)
					items[i].SetValue(stringValues[i]);
				else
				{
					AddNewItem(stringValues[i]);
				}
			}

			for (int i = stringValues.Count; i < items.Count; i++)
			{
				if (itemsListControl != null)
					itemsListControl.RemoveItemAt(i);
				items.RemoveAt(i);
			}
		}
		
		private void AddNewItem()
		{
			AddNewItem("");
		}

		private void AddNewItem(string value)
		{
			FieldType item = Activator.CreateInstance(typeof(FieldType)) as FieldType;
			item.SetValue(value);
			if (itemsListControl != null)
			{
				FieldControl control = item.GenerateUIElement(isForEditing);
				itemsListControl.AddNewItem(control);
			}
			items.Add(item);			
		}

		private void RemoveItem(int index)
		{
			items.RemoveAt(index);
		}
	}
}
