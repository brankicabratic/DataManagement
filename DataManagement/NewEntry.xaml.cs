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

		public NewEntry()
		{
			InitializeComponent();

			dataItem = new DataItem(true);
			Dictionary<string, List<Field>> tabs = DataStructure.GetTabs();
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
				parent.Children.Add(field.GenerateUIElement());
			}
		}

		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			Data.AddDataItem(dataItem);
			isSaved = true;
			Close();
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
