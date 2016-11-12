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

namespace DataManagement.Controls
{
	/// <summary>
	/// Interaction logic for ExternalDataStructureListControl.xaml
	/// </summary>
	public partial class ExternalDataStructureListControl : FieldControl
	{
		private ExternalDataStructureList externalDataStructureList;

		public ExternalDataStructureListControl()
		{
			InitializeComponent();
		}

		public void Initialize(ExternalDataStructureList externalDSListField)
		{
			this.externalDataStructureList = externalDSListField;

			foreach (Field field in externalDSListField.DataStructure.GetTableFields())
			{
				DataGridTextColumn textColumn = new DataGridTextColumn();
				textColumn.Header = field.Name;
				textColumn.Binding = new Binding(field.Id);
				dataGrid.Columns.Add(textColumn);
			}

			dataGrid.ItemsSource = externalDSListField.Items;
		}

		public override void SetEditable(bool isEditable) { }

		private void AddButton_Click(object sender, RoutedEventArgs e)
		{
			NewEntry newEntryWindow = new NewEntry(externalDataStructureList.DataStructure, (dataItem) => 
			{
				externalDataStructureList.Items.Add(dataItem); Data.SaveData();
			});
			newEntryWindow.Owner = Window.GetWindow(this);
			newEntryWindow.ShowDialog();
		}

		private void PreviewButton_Click(object sender, RoutedEventArgs e)
		{
			int selectedItemsCount = dataGrid.SelectedItems.Count;
			if (selectedItemsCount != 1)
			{
				MessageBox.Show(Window.GetWindow(this), Properties.Resources.Message_OnlyOnePreview_Text, Properties.Resources.Message_OnlyOnePreview_Caption);
				return;
			}
			DataItem dataItem = dataGrid.SelectedItem as DataItem;
			if (dataItem == null)
			{
				MessageBox.Show(Window.GetWindow(this), Properties.Resources.Message_InternalError_Text, Properties.Resources.Message_InternalError_Caption);
				return;
			}
			ViewEditItem viewEditItemWindow = new ViewEditItem(externalDataStructureList.DataStructure, dataItem, Properties.Resources.ExternalDataViewItem_Title, false);
			viewEditItemWindow.Owner = Window.GetWindow(this);
			viewEditItemWindow.OnDataChanged += RefreshData;
			viewEditItemWindow.ShowDialog();
		}

		private void DeleteButton_Click(object sender, RoutedEventArgs e)
		{
			int selectedItemsCount = dataGrid.SelectedItems.Count;
			if (selectedItemsCount == 0) return;
			MessageBoxResult result = MessageBox.Show(Window.GetWindow(this), Properties.Resources.Message_DeletionConformation_Text.Replace("%v1%", selectedItemsCount.ToString()), Properties.Resources.Message_DeletionConformation_Caption, MessageBoxButton.YesNo);
			if (result == MessageBoxResult.Yes)
			{
				externalDataStructureList.RemoveItems(dataGrid.SelectedItems);
			}
		}

		private void RefreshData()
		{
			dataGrid.Items.Refresh();
		}
	}
}
