using DataManagement.Models;
using ExcelLibrary.SpreadSheet;
using Microsoft.Win32;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DataManagement
{
	class ExcelExportDataItem
	{
		public int RowsCount
		{
			get
			{
				if (SubItems.Count == 0) return 1;
				else
				{
					int rowsCount = 0;
					foreach (ExcelExportDataItem subItem in SubItems)
						rowsCount += subItem.RowsCount;
					return rowsCount;
				}
			}
		}
		public List<ExcelExportDataItem> SubItems = new List<ExcelExportDataItem>();
	}

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

		private void ExportButton_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.DefaultExt = ".xls";
			saveFileDialog.Filter = "Document (*xls)|*xls";
			if (saveFileDialog.ShowDialog() == true)
			{
				Workbook workbook = new Workbook();
				Worksheet worksheet = new Worksheet("Data");
				Dictionary<string, Field> leafFields = DataStructure.GetAllExcelExportFields();
				int headerCol = 0;
				foreach (string fieldID in leafFields.Keys)
				{					
					worksheet.Cells[0, headerCol++] = new Cell(fieldID);
				}
				worksheet.Cells.ColumnWidth[0, (ushort)headerCol] = Properties.Settings.Default.ExcelColumnWidth;

				Func<ExcelExportDataItem, List<DataItem>, int, int, int> enterDataItems = null;
				enterDataItems = (parentExcelExportDataItem, items, row, col) =>
				{
					int firstCol = col;
					foreach (DataItem dataItem in items)
					{
						ExcelExportDataItem excelExportDataItem = new ExcelExportDataItem();
						parentExcelExportDataItem.SubItems.Add(new ExcelExportDataItem());
						HashSet<Field> examinedComplexFields = new HashSet<Field>();
						foreach (string fieldID in DataStructure.GetAllExcelExportFields().Keys)
						{
							Field field = dataItem.GetFieldWithParentReturn(fieldID);
							if (field == null)
							{
								continue;
							}
							else if (field.IsLeaf && (field.Id == fieldID))
							{
								worksheet.Cells[row, col++] = new Cell(field.GetXslOutput());
							}
							else
							{
								if (field is FixedItemsList)
								{
									if (field.Id == fieldID) continue;
									FixedItemsList fixedItemsList = field as FixedItemsList;
									Field subField = fixedItemsList.GetItem(fieldID);
									if (subField != null && subField.IsLeaf)
										worksheet.Cells[row, col] = new Cell(subField.GetXslOutput());
									if (subField == null || subField.IsLeaf) col++;
								}
								else if (field is ExternalDataStructureList)
								{
									if (examinedComplexFields.Contains(field)) continue;
									examinedComplexFields.Add(field);
									ExternalDataStructureList externalDS = field as ExternalDataStructureList;
									col = enterDataItems(excelExportDataItem, externalDS.Items.ToList(), row, col);
								}
							}
						}
						col = firstCol;
						row += excelExportDataItem.RowsCount;
					}
					return col;
				};

				ExcelExportDataItem mainItem = new ExcelExportDataItem();
				enterDataItems(mainItem, Data.GetAllData().ToList(), 1, 0);

				/*
				worksheet.Cells[0, 1] = new Cell((short)1);
				worksheet.Cells[2, 0] = new Cell(9999999);
				worksheet.Cells[3, 3] = new Cell((decimal)3.45);
				worksheet.Cells[2, 2] = new Cell("Text string");
				worksheet.Cells[2, 4] = new Cell("Second string");
				worksheet.Cells[4, 0] = new Cell(32764.5, "#,##0.00");
				worksheet.Cells[5, 1] = new Cell(DateTime.Now, @"YYYY-MM-DD");
				
				
				*/
				
				workbook.Worksheets.Add(worksheet);
				try
				{
					workbook.Save(saveFileDialog.FileName);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
				}
			}
		}

		private void AddButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				NewEntry newEntryWindow = new NewEntry(DataStructure.MainDataStructure, (dataItem) => { Data.AddDataItem(dataItem); });
				newEntryWindow.Owner = this;
				newEntryWindow.ShowDialog();
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message + "\n" + ex.StackTrace, "Greška");
			}
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
