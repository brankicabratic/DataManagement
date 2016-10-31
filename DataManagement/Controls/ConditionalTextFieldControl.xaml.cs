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
	/// Interaction logic for ConditionalTextFieldControl.xaml
	/// </summary>
	public partial class ConditionalTextFieldControl : FieldControl
	{
		public ConditionalTextFieldControl()
		{
			InitializeComponent();
		}

		public void Initialize(ConditionalTextField textField)
		{
			FieldName.Content = textField.Name;
			FieldSubName.Content = textField.SubName;

			Binding b = new Binding();
			b.Source = textField;
			b.Path = new PropertyPath("Value");
			b.Mode = BindingMode.TwoWay;
			BindingOperations.SetBinding(Text, TextBox.TextProperty, b);
			AppendValidationToBinding(b, textField);

			b = new Binding();
			b.Source = textField;
			b.Path = new PropertyPath("BoolValue");
			b.Mode = BindingMode.TwoWay;
			BoolValue.IsChecked = textField.BoolValue;
			BindingOperations.SetBinding(BoolValue, System.Windows.Controls.CheckBox.IsCheckedProperty, b);

			FieldSubName.Visibility = Text.Visibility = textField.BoolValue ? Visibility.Visible : Visibility.Collapsed;
		}

		public override void SetEditable(bool isEditable)
		{
			Text.IsReadOnly = !isEditable;
			BoolValue.IsEnabled = isEditable;
		}

		private void BoolValue_Checked(object sender, RoutedEventArgs e)
		{
			FieldSubName.Visibility = Text.Visibility = Visibility.Visible;
		}

		private void BoolValue_Unchecked(object sender, RoutedEventArgs e)
		{
			FieldSubName.Visibility = Text.Visibility = Visibility.Collapsed;
		}
	}
}
