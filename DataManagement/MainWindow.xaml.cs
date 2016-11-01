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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DataManagement
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
			InitializeComponent();

			try
            {
                DataStructure loadedDS = DataStructure.LoadDataStructure(Properties.Settings.Default.DataStructureFileLocation);
				if (loadedDS.ErrorExists)
				{
					MessageBox.Show(loadedDS.GetErrorLogs(), Properties.Resources.Message_DataStructureParseError_Caption);
				}
				else
				{
					foreach (Field field in loadedDS.GetTableFields())
					{
						DataGridTextColumn textColumn = new DataGridTextColumn();
						textColumn.Header = field.Name;
						textColumn.Binding = new Binding(field.Id);
						dataGrid.Columns.Add(textColumn);
					}

					Data.LoadData();
					dataGrid.ItemsSource = Data.GetAllData();
				}				
            }
            catch (Exception e)
            {
                MessageBox.Show(this, Properties.Resources.Message_CouldNotLoadDataStructure_Text, Properties.Resources.Message_CouldNotLoadDataStructure_Caption);
            }
        }

		private void AddButton_Click(object sender, RoutedEventArgs e)
		{
			NewEntry newEntryWindow = new NewEntry(DataStructure.MainDataStructure, (dataItem) => { Data.AddDataItem(dataItem); });
			newEntryWindow.Owner = this;
			newEntryWindow.ShowDialog();
		}

		private void PreviewButton_Click(object sender, RoutedEventArgs e)
		{
			int selectedItemsCount = dataGrid.SelectedItems.Count;
			if (selectedItemsCount != 1)
			{
				MessageBox.Show(this, Properties.Resources.Message_OnlyOnePreview_Text, Properties.Resources.Message_OnlyOnePreview_Caption);
				return;
			}
			DataItem dataItem = dataGrid.SelectedItem as DataItem;
			if (dataItem == null)
			{
				MessageBox.Show(this, Properties.Resources.Message_InternalError_Text, Properties.Resources.Message_InternalError_Caption);
				return;
			}
			ViewEditItem viewEditItemWindow = new ViewEditItem(DataStructure.MainDataStructure, dataItem, "", true);
			viewEditItemWindow.Owner = this;
			viewEditItemWindow.OnDataChanged += RefreshData;
			viewEditItemWindow.ShowDialog();			
		}

		private void DeleteButton_Click(object sender, RoutedEventArgs e)
		{
			int selectedItemsCount = dataGrid.SelectedItems.Count;
			if (selectedItemsCount == 0) return;
			MessageBoxResult result = MessageBox.Show(this, Properties.Resources.Message_DeletionConformation_Text.Replace("%v1%", selectedItemsCount.ToString()), Properties.Resources.Message_DeletionConformation_Caption, MessageBoxButton.YesNo);
			if (result == MessageBoxResult.Yes)
			{
				Data.RemoveItems(dataGrid.SelectedItems);
			}
		}

		private void RefreshData()
		{
			dataGrid.Items.Refresh();
		}
	}
}
