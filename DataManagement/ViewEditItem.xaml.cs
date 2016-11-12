using DataManagement.Controls;
using DataManagement.Models;
using Microsoft.Win32;
using Novacode;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DataManagement
{
	/// <summary>
	/// Interaction logic for ViewEditItem.xaml
	/// </summary>
	public partial class ViewEditItem : Window
	{
		public event Action OnDataChanged;

		private DataItem originalDataItem;
		private DataItem copiedDataItem;

		private List<FieldControl> allUserControls = new List<FieldControl>();
		private bool isInEditMode = false;

		public ViewEditItem(DataStructure dataStructure, DataItem dataItem, string label, bool enableExport)
		{
			originalDataItem = dataItem;
			copiedDataItem = ObjectCopier.Clone<DataItem>(dataItem);

			InitializeComponent();

			ItemName.Content = label;
			ExportButton.Visibility = enableExport ? Visibility.Visible : Visibility.Collapsed;

			Dictionary<string, List<Field>> tabsDict = dataStructure.GetTabs(false);

			foreach (string tabName in tabsDict.Keys)
			{
				TabItem tabItem = new TabItem();
				tabItem.Header = tabName;
				tabs.Items.Add(tabItem);

				ScrollViewer scroll = new ScrollViewer();
				StackPanel sp = new StackPanel();
				scroll.Content = sp;
				sp.Orientation = System.Windows.Controls.Orientation.Vertical;
				foreach (Field tabField in tabsDict[tabName])
				{
					FieldControl userControl = tabField is ExternalDataStructureList ? originalDataItem.GetField(tabField.Id).GenerateUIElement(true) : copiedDataItem.GetField(tabField.Id).GenerateUIElement(true);
					userControl.SetEditable(false);
					allUserControls.Add(userControl);
					sp.Children.Add(userControl);
				}

				tabItem.Content = scroll;
			}
		}

		private void ExportButton_Click(object sender, RoutedEventArgs e)
		{
			string templateLocation = Properties.Settings.Default.TemplateFileLocation;
			string templateExtension = System.IO.Path.GetExtension(templateLocation);

			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.DefaultExt = templateExtension;
			saveFileDialog.Filter = "Document (*" + templateExtension + ")|*" + templateExtension;
			saveFileDialog.FileName = originalDataItem.GetField("ImePrezime").GetValue();
			if (saveFileDialog.ShowDialog() == true)
			{
				if (File.Exists(templateLocation))
				{
					DocX doc = DocX.Load(templateLocation);
					Dictionary<string, Field> allFields = DataStructure.MainDataStructure.GetAllFields(true);
					foreach (string id in allFields.Keys)
					{
						string value = originalDataItem.GetField(id).GetDocOutput();
						if (value == null) value = "N/A";
						doc.ReplaceText("[" + id + "]", value);
					}
					try
					{
						doc.SaveAs(saveFileDialog.FileName);
					}
					catch
					{
						MessageBox.Show(Properties.Resources.Message_FileSaveFailed_Text, Properties.Resources.Message_FileSaveFailed_Caption);
					}
				}
				else
				{
					Console.WriteLine("djslafjldaskfj;ladsdf");
				}
			}
		}

		private void EditSaveButton_Click(object sender, RoutedEventArgs e)
		{
			if (!isInEditMode)
			{
				allUserControls.ForEach(x => x.SetEditable(true));
				EditSaveButton.Content = Properties.Resources.Button_Save;
			}
			else
			{
				allUserControls.ForEach(x => x.SetEditable(false));
				EditSaveButton.Content = Properties.Resources.Button_EditingOn;
				originalDataItem.CopyFields(copiedDataItem);
				Data.SaveData();
				if (OnDataChanged != null) OnDataChanged();
			}

			isInEditMode = !isInEditMode;			
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (isInEditMode)
			{
				MessageBoxResult result = MessageBox.Show(Properties.Resources.Message_ChangeLossWarning_Text, Properties.Resources.Message_ChangeLossWarning_Caption, MessageBoxButton.YesNo);
				if (result == MessageBoxResult.No)
				{
					e.Cancel = true;
				}
			}
		}
	}
}
