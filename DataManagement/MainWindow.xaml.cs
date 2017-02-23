using ClosedXML.Excel;
using DataManagement.Models;
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
		public int ColStart;
		public int ColEnd;
		public int RowsCount
		{
			get
			{
				if (SubItems.Count == 0) return 1;
				else
				{
					Dictionary<string, List<ExcelExportDataItem>> grouped = GroupedSubItems;
					int rowsCount = 0;
					foreach (string key in grouped.Keys)
					{
						int rowsCountLocal = 0;
						foreach (ExcelExportDataItem subItem in grouped[key])
							rowsCountLocal += subItem.RowsCount;
						if (rowsCountLocal > rowsCount)
							rowsCount = rowsCountLocal;
					}
					return rowsCount;
				}
			}
		}

		public Dictionary<string, List<ExcelExportDataItem>> GroupedSubItems
		{
			get
			{
				Dictionary<string, List<ExcelExportDataItem>> groupedByColStartEnd = new Dictionary<string, List<ExcelExportDataItem>>();
				foreach (ExcelExportDataItem subItem in SubItems)
				{
					string colsStartEnd = subItem.ColStart + "-" + subItem.ColEnd;
					if (!groupedByColStartEnd.ContainsKey(colsStartEnd))
						groupedByColStartEnd[colsStartEnd] = new List<ExcelExportDataItem>();
					groupedByColStartEnd[colsStartEnd].Add(subItem);
				}
				return groupedByColStartEnd;
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
			saveFileDialog.DefaultExt = ".xlsx";
			saveFileDialog.Filter = "Document (*xlsx)|*xlsx";
			if (saveFileDialog.ShowDialog() == true)
			{
				XLWorkbook workbook = new XLWorkbook();
				IXLWorksheet worksheet = workbook.Worksheets.Add("Data");
				Dictionary<string, Field> excelFields = DataStructure.GetAllExcelExportFields();
				int headerCol = 1;
				foreach (string fieldID in excelFields.Keys)
				{
					worksheet.Cell(1, headerCol).Value = fieldID;
					worksheet.Column(headerCol).Width = Properties.Settings.Default.ExcelColumnWidth;
					headerCol++;
				}

				Func<ExcelExportDataItem, List<DataItem>, int, int, int> enterDataItems = null;
				enterDataItems = (parentExcelExportDataItem, items, row, col) =>
				{
					int firstCol = col;
					int lastCol = col;
					foreach (DataItem dataItem in items)
					{
						ExcelExportDataItem excelExportDataItem = new ExcelExportDataItem();
						excelExportDataItem.ColStart = col;
						parentExcelExportDataItem.SubItems.Add(excelExportDataItem);
						HashSet<Field> examinedComplexFields = new HashSet<Field>();
						foreach (string fieldID in excelFields.Keys)
						{
							Field field = dataItem.GetFieldWithParentReturn(fieldID);
							if (field == null)
							{
								continue;
							}
							else if (field.Id == fieldID)
							{
								if (field.IsLeaf)
								{
									IXLCell cell = worksheet.Cell(row, col++);
									object value = field.GetXslOutput();
									cell.Value = value;
									if (value is string)
										cell.DataType = XLCellValues.Text;
								}
							}
							else
							{
								if (field is FixedItemsList)
								{
									FixedItemsList fixedItemsList = field as FixedItemsList;
									Field subField = fixedItemsList.GetItem(fieldID);
									if (subField != null && subField.IsLeaf)
									{
										IXLCell cell = worksheet.Cell(row, col);
										object value = subField.GetXslOutput();
										cell.Value = value;
										if (value is string)
											cell.DataType = XLCellValues.Text;
									}
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
						excelExportDataItem.ColEnd = col;
						lastCol = col;
						col = firstCol;
						row += excelExportDataItem.RowsCount;
					}
					return lastCol;
				};

				Action<ExcelExportDataItem, int> mergeCells = null;
				mergeCells = (excelDataItem, row) =>
				{
					int startRow = row;
					Dictionary<string, List<ExcelExportDataItem>> groupedItems = excelDataItem.GroupedSubItems;
					foreach (List<ExcelExportDataItem> group in groupedItems.Values)
					{
						if (group.Count == 0) continue;
						int colStart = group[0].ColStart;
						int colEnd = group[0].ColEnd;
						foreach (ExcelExportDataItem item in group)
						{
							Dictionary<string, List<ExcelExportDataItem>> itemGroupedItems = item.GroupedSubItems;
							for (int i = colStart; i < colEnd; i++)
							{
								bool isFree = true;
								foreach (List<ExcelExportDataItem> itemGroup in itemGroupedItems.Values)
								{
									if (itemGroup.Count == 0) continue;
									isFree = !(i >= itemGroup[0].ColStart && i < itemGroup[0].ColEnd);
									if (!isFree) break;
								}
								if (isFree)
									worksheet.Range(row, i, row + item.RowsCount - 1, i).Merge();
							}
							row += item.RowsCount;
						}
						row = startRow;						
					}
				};
				
				ExcelExportDataItem mainItem = new ExcelExportDataItem();
				mainItem.ColStart = 1;
				mainItem.ColEnd = enterDataItems(mainItem, Data.GetAllData().ToList(), 2, 1);
				mergeCells(mainItem, 2);

				try
				{
					workbook.SaveAs(saveFileDialog.FileName);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
				}
			}
		}

		private void BackupButton_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.DefaultExt = ".xml";
			saveFileDialog.Filter = "XML (*xml)|*xml";
			if (saveFileDialog.ShowDialog() == true)
			{
				if (File.Exists(saveFileDialog.FileName))
					MessageBox.Show(this, Properties.Resources.Message_BackupFileAlreadyExists_Text, Properties.Resources.Message_BackupFileAlreadyExists_Caption);
				else
					Data.SaveData(saveFileDialog.FileName);
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
