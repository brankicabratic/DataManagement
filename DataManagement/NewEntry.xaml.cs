using DataManagement.Controls;
using DataManagement.Models;
using System;
using System.Collections.Generic;
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
	/// Interaction logic for NewEntry.xaml
	/// </summary>
	public partial class NewEntry : Window
	{
		private DataItem dataItem;
		private bool isSaved = false;

		private DataStructure dataStructure;
		private Action<DataItem> onSave;

		private List<FieldControl> mandatoryFields = new List<FieldControl>();

		public NewEntry(DataStructure dataStructure, Action<DataItem> onSave)
		{
			InitializeComponent();

			this.dataStructure = dataStructure;
			this.onSave = onSave;

			dataItem = new DataItem(dataStructure);
			Dictionary<string, List<Field>> tabs = dataStructure.GetTabs(true);
			if (tabs.Keys.Count > 1)
			{
				foreach (string tabName in tabs.Keys)
				{
					StackPanel newPanel = new StackPanel();
					newPanel.Orientation = Orientation.Horizontal;
					Label dummyLabel = new Label();
					dummyLabel.Width = 50;
					newPanel.Children.Add(dummyLabel);
					StackPanel container = new StackPanel();
					newPanel.Children.Add(container);
					Expander expander = new Expander();
					expander.IsExpanded = true;
					expander.Header = tabName;
					expander.Content = newPanel;
					List<Field> fields = new List<Field>();
					foreach (Field tabField in tabs[tabName])
					{
						fields.Add(dataItem.GetField(tabField.Id));
					}
					AddFields(container, fields);
					MainContainer.Children.Add(expander);
				}
			}
			else if (tabs.Keys.Count == 1)
			{
				List<Field> fields = new List<Field>();
				foreach (Field tabField in tabs.Values.First())
				{
					fields.Add(dataItem.GetField(tabField.Id));
				}
				AddFields(MainContainer, fields);
			}
		}

		private void AddFields(Panel parent, List<Field> fields)
		{
			foreach (Field field in fields)
			{
				FieldControl control = field.GenerateUIElement(false);
				if (control.IsMandatory) mandatoryFields.Add(control);
				parent.Children.Add(control);
			}
		}

		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			bool mandatoryFieldsEdited = true;
			foreach (FieldControl control in mandatoryFields)
				if (!control.IsEdited)
				{
					mandatoryFieldsEdited = false;
					break;
				}

			if (!mandatoryFieldsEdited)
			{
				MessageBox.Show(this, Properties.Resources.Message_MandatoryFieldsNotEdited_Text, Properties.Resources.Message_MandatoryFieldsNotEdited_Caption);
				return;
			}
			else
			{
				if (onSave != null) onSave(dataItem);
				isSaved = true;
				Close();
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (!isSaved)
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
