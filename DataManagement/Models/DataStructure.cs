using DataManagement.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DataManagement.Models
{
    public class DataStructure
    {
        public static string TableFieldsEl = "TableFields";
		public static string AllFieldsEl = "AllFields";
		public static string TabEl = "Tab";

		public static string IdAtt = "id";
		public static string NameAtt = "name";
		public static string ValidationAtt = "validation";
		public static string ValidationValueAtt = "validationValue";
		public static string DelimiterAtt = "delimiter";
		public static string ListElementTypeAtt = "listElementType";
		public static string SubNameAtt = "subName";
		public static string DefaultAtt = "default";
		public static string ConditionalAtt = "conditional";

		private static List<Field> tableFields;
        private static Dictionary<string, Field> allFields;
        private static Dictionary<string, List<Field>> tabs;

        private static StringBuilder parsingErrors = new StringBuilder();
        private static bool errorExists = false;

        public static bool LoadDataStructure()
        {
            string fileLocation = Properties.Settings.Default.DataStructureFileLocation;
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
                        LoadAllFields(node);
                    }
                }
                catch
                {
                    LogError(Properties.Resources.Error_ElementContentParseError.Replace("%v1%", node.Name));
                }
            };

            foreach (XmlNode node in xml.DocumentElement)
            {
                processNode(node);
            }

            LoadTableFields(tableFieldsIds);

            return !errorExists;
        }

		public static Dictionary<string, Field> GetAllFields(bool includeNested)
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

		public static List<Field> GetTableFields()
		{
			return tableFields;
		}

		public static Dictionary<string, List<Field>> GetTabs()
		{
			return tabs;
		}

        public static string GetErrorLogs()
        {
            return parsingErrors.ToString();
        }

        private static void LoadAllFields(XmlNode allFieldsNode)
        {
            allFields = new Dictionary<string, Field>();
			tabs = new Dictionary<string, List<Field>>();
            foreach (XmlNode field in allFieldsNode)
            {
                if (field.Name == TabEl)
				{
					LoadTabFields(field);
				}
				else
				{
					LogError(Properties.Resources.Error_WrongElementPosition.Replace("%v1%", field.Name).Replace("%v2%", TabEl));
				}
            }
        }

        private static void LoadTableFields(string[] fieldIds)
        {
			tableFields = new List<Field>(fieldIds.Length);
            foreach (string fieldId in fieldIds)
            {
				Field field;
                if (allFields.TryGetValue(fieldId, out field))
                {
                    tableFields.Add(field);
				}
                else
                {
                    LogError(Properties.Resources.Error_TableKeyInvalid.Replace("%v1%", fieldId));
                }
            }
        }

		private static void LoadTabFields(XmlNode tab)
		{
			string tabName = tab.Attributes[NameAtt].Value;
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
					if (!cF.IsNested) tabFields.Add(cF);
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

				Field newField = Field.CreateField(field, callback);
				if (newField == null)
				{
					LogError(Properties.Resources.Error_ElementParseError.Replace("%v1%", field.Name).Replace("%v2%", field.Attributes[IdAtt].Value));
				}
			}
		}

        public static void LogError(string error)
        {
            errorExists = true;
            parsingErrors.Append(error + "\n");
        }
    }
}
