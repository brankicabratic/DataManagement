using DataManagement.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DataManagement.Models
{
	[Serializable]
    public class DataStructure
    {
        public static string TableFieldsEl = "TableFields";
		public static string AllFieldsEl = "AllFields";
		public static string TabEl = "Tab";

		public static string IdAtt = "id";
		public static string NameAtt = "name";
		public static string HideOnAddAtt = "hideOnAdd";
		public static string ValidationAtt = "validation";
		public static string ValidationValueAtt = "validationValue";
		public static string IncludeInExcelExportAtt = "includeInExcelExport";
		public static string DelimiterAtt = "delimiter";
		public static string ListElementTypeAtt = "listElementType";
		public static string SubNameAtt = "subName";
		public static string DefaultAtt = "default";
		public static string MandatoryAtt = "mandatory";
		public static string ConditionalAtt = "conditional";
		public static string FileLocationAtt = "fileLocation";
		public static string DocOutputMainFieldIDAtt = "docOutputMainFieldID";
		public static string DocOutputFieldIDsAtt = "docOutputFieldIDs";

		public static DataStructure MainDataStructure;

		private static Dictionary<string, Field> allAppFields = new Dictionary<string, Field>();

		public bool ErrorExists = false;

		private List<Field> tableFields;
        private Dictionary<string, Field> allFields;
        private Dictionary<string, List<Field>> tabs;
		private HashSet<string> hideOnAddTabs = new HashSet<string>();

        private StringBuilder parsingErrors = new StringBuilder();        

        public static DataStructure LoadDataStructure(string fileLocation, bool isMain = true, string idPrefix = "")
        {
			DataStructure ds = new DataStructure();

            XmlDocument xml = new XmlDocument();
            xml.Load(fileLocation);

            string[] tableFieldsIds = new string[0];

            Action<XmlNode> processNode = (node) =>
            {
                try
                {
                    if (node.Name == TableFieldsEl)
                    {
                        string separator = ";";
                        foreach (XmlAttribute attribute in node.Attributes)
                        {
                            if (attribute.Name == DelimiterAtt)
                                separator = attribute.Value;
                        }

                        tableFieldsIds = node.FirstChild.Value.Split(separator.ToCharArray());
                    }
                    else if (node.Name == AllFieldsEl)
                    {
                        ds.LoadAllFields(node, idPrefix);
                    }
                }
                catch (Exception e)
                {
                    ds.LogError(Properties.Resources.Error_ElementContentParseError.Replace("%v1%", node.Name) +
						"\n" + e.Message + "\n" + e.StackTrace);
                }
            };

            foreach (XmlNode node in xml.DocumentElement)
            {
                processNode(node);
            }

            ds.LoadTableFields(tableFieldsIds, idPrefix);

			if (isMain)
				MainDataStructure = ds;

            return ds;
        }

		public static Dictionary<string, Field> GetAllAppFields()
		{
			return allAppFields;
		}

		public static Dictionary<string, Field> GetAllExcelExportFields()
		{
			Dictionary<string, Field> fields = new Dictionary<string, Field>();
			foreach (string fieldID in allAppFields.Keys)
			{
				Field currentField = allAppFields[fieldID];
				if (currentField.IsLeaf && currentField.IncludeInExcelExport)
					fields[fieldID] = currentField;
			}
			return fields;
		}

		public Dictionary<string, Field> GetAllFields(bool includeNested)
		{
			if (!includeNested)
			{
				Dictionary<string, Field> rootFields = new Dictionary<string, Field>();
				foreach (string key in allFields.Keys)
				{
					if (!allFields[key].IsNested)
						rootFields[key] = allFields[key];
				}
				return rootFields;
			}
			return allFields;
		}

		public List<Field> GetTableFields()
		{
			return tableFields;
		}

		public Dictionary<string, List<Field>> GetTabs(bool isAdd)
		{
			if (isAdd)
			{
				Dictionary<string, List<Field>> tabs = new Dictionary<string, List<Field>>();
				foreach (string tabName in this.tabs.Keys)
				{
					if (!hideOnAddTabs.Contains(tabName))
						tabs[tabName] = this.tabs[tabName];
				}
				return tabs;
			}
			return tabs;
		}

        public string GetErrorLogs()
        {
            return parsingErrors.ToString();
        }

        private void LoadAllFields(XmlNode allFieldsNode, string idPrefix)
        {
            allFields = new Dictionary<string, Field>();
			tabs = new Dictionary<string, List<Field>>();
            foreach (XmlNode field in allFieldsNode)
            {
                if (field.Name == TabEl)
				{
					LoadTabFields(field, idPrefix);
				}
				else
				{
					LogError(Properties.Resources.Error_WrongElementPosition.Replace("%v1%", field.Name).Replace("%v2%", TabEl));
				}
            }
        }

        private void LoadTableFields(string[] fieldIds, string idPrefix)
        {
			tableFields = new List<Field>(fieldIds.Length);
            foreach (string fieldId in fieldIds)
            {
				Field field;
                if (allFields.TryGetValue(idPrefix + fieldId, out field))
                {
                    tableFields.Add(field);
				}
                else
                {
                    LogError(Properties.Resources.Error_TableKeyInvalid.Replace("%v1%", fieldId));
                }
            }
        }

		private void LoadTabFields(XmlNode tab, string idPrefix)
		{
			string tabName = tab.Attributes[NameAtt].Value;
			XmlAttribute hideOnAddAttribute = tab.Attributes[HideOnAddAtt];
			if (hideOnAddAttribute != null && hideOnAddAttribute.Value.ToLower() == "true")
				hideOnAddTabs.Add(tabName);

			List<Field> tabFields;
			if (!tabs.TryGetValue(tabName, out tabFields))
			{
				tabFields = new List<Field>();
				tabs[tabName] = tabFields;
			}

			DynamicPropertyManager<DataItem> propertyManager = new DynamicPropertyManager<DataItem>();

			foreach (XmlNode field in tab)
			{
				Action<Field> callback = (cF) => {
					allFields[cF.Id] = cF;
					allAppFields[cF.Id] = cF;
					bool isNested = string.IsNullOrEmpty(idPrefix) ? cF.IsNested : cF.Id.Replace(idPrefix, "").Contains(".");
					if (!isNested) tabFields.Add(cF);
					propertyManager.Properties.Add(
					   DynamicPropertyManager<DataItem>.CreateProperty<DataItem, string>(
						  cF.Id,
						  t => {
							  Field f = t.GetField(cF.Id);
							  return f != null ? f.GetValue() : null;
						  },
						  null
					));
				};

				Field newField = Field.CreateField(field, callback, idPrefix);
				if (newField == null)
				{
					LogError(Properties.Resources.Error_ElementParseError.Replace("%v1%", field.Name).Replace("%v2%", field.Attributes[IdAtt].Value));
				}
			}
		}

        public void LogError(string error)
        {
            ErrorExists = true;
            parsingErrors.Append(error + "\n");
        }
    }
}
