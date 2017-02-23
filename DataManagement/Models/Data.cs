using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace DataManagement.Models
{
	public class Data
	{		
		private static object lockObj = new object();

		private static Data instance;

		public ObservableCollection<DataItem> DataItems { get { return dataItems; } set { dataItems = value; } }

		private ObservableCollection<DataItem> dataItems;

		public static void LoadData()
		{
			lock(lockObj)
			{
				XmlSerializer serializer = new XmlSerializer(typeof(Data));

				string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + Properties.Settings.Default.DataFile;
				if (!File.Exists(path))
				{
					instance = new Data();
					instance.dataItems = new ObservableCollection<DataItem>();
					return;
				}

				FileStream fs = new FileStream(path, FileMode.Open);
				XmlReader reader = XmlReader.Create(fs);

				instance = (Data)serializer.Deserialize(reader);
				fs.Close();
			}
		}

		public static void SaveData()
		{
			lock (lockObj)
			{
				XmlSerializer xsSubmit = new XmlSerializer(typeof(Data));
				StringWriter sww = new StringWriter();
				using (XmlWriter writer = XmlWriter.Create(sww, new XmlWriterSettings { OmitXmlDeclaration = true }))
				{
					xsSubmit.Serialize(writer, instance);
					var xml = sww.ToString();

					string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + Properties.Settings.Default.DataFile;

					if (File.Exists(path))
					{
						string backupPath = path + ".backup";
						if (File.Exists(backupPath))
							File.Delete(backupPath);
						File.Move(path, backupPath);
						File.Delete(path);
					}

					using (StreamWriter sw = File.AppendText(path))
					{
						sw.Write(xml);
					}
				}
			}
		}

		public static void SaveData(string path)
		{
			lock (lockObj)
			{
				XmlSerializer xsSubmit = new XmlSerializer(typeof(Data));
				StringWriter sww = new StringWriter();
				using (XmlWriter writer = XmlWriter.Create(sww, new XmlWriterSettings { OmitXmlDeclaration = true }))
				{
					xsSubmit.Serialize(writer, instance);
					var xml = sww.ToString();

					using (StreamWriter sw = File.AppendText(path))
					{
						sw.Write(xml);
					}
				}
			}
		}

		public static void AddDataItem(DataItem dataItem)
		{
			lock (lockObj)
			{
				instance.dataItems.Add(dataItem);
				SaveData();
			}
		}

		public static ObservableCollection<DataItem> GetAllData()
		{
			return instance.dataItems;
		}

		public static void RemoveItems(IList items)
		{
			lock (lockObj)
			{
				for (int i = items.Count - 1; i >= 0; i--)
				{
					DataItem dataItem = items[i] as DataItem;
					if (dataItem != null)
						instance.dataItems.Remove(dataItem);
				}
				SaveData();
			}
		}
	}
}
