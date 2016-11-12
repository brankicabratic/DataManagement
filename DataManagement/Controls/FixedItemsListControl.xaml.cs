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
	/// Interaction logic for FixedItemsListControl.xaml
	/// </summary>
	public partial class FixedItemsListControl : FieldControl
	{
		public FixedItemsListControl()
		{
			InitializeComponent();
		}

		public void Initialize(Models.FixedItemsList fixedItemsList, bool isForEditing)
		{
			FieldName.Content = fixedItemsList.Name;

			FixedItemsList dataStructure = DataStructure.GetAllAppFields()[fixedItemsList.Id] as FixedItemsList;

			foreach (Field dsField in dataStructure.Items)
			{
				Field field = fixedItemsList.Items.Find(x => x.Id == dsField.Id);
				if (field == null)
				{
					field = ObjectCopier.Clone<Field>(dsField);
					fixedItemsList.Items.Add(field);
				}
				field.SetName(dsField.Name);
				Container.Children.Add(field.GenerateUIElement(isForEditing));
			}

			if (fixedItemsList.IsConditional)
			{
				Binding b = new Binding();
				b.Source = fixedItemsList;
				b.Path = new PropertyPath("BoolValue");
				b.Mode = BindingMode.TwoWay;
				BoolValue.IsChecked = fixedItemsList.BoolValue;
				BindingOperations.SetBinding(BoolValue, System.Windows.Controls.CheckBox.IsCheckedProperty, b);

				Container.Visibility = fixedItemsList.BoolValue ? Visibility.Visible : Visibility.Collapsed;
			}
			else
			{
				BoolValue.Visibility = Visibility.Collapsed;
			}
		}

		public override void SetEditable(bool isEditable)
		{
			BoolValue.IsEnabled = isEditable;
			foreach (UIElement element in Container.Children)
			{
				FieldControl field = element as FieldControl;
				if (field != null) field.SetEditable(isEditable);
			}
		}

		private void BoolValue_Checked(object sender, RoutedEventArgs e)
		{
			Container.Visibility = Visibility.Visible;
		}

		private void BoolValue_Unchecked(object sender, RoutedEventArgs e)
		{
			Container.Visibility = Visibility.Collapsed;
		}
	}
}
