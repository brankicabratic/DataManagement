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
	/// Interaction logic for TextFieldControl.xaml
	/// </summary>
	public partial class TextFieldControl : FieldControl
	{
		public TextFieldControl()
		{
			InitializeComponent();
		}

		public void Initialize(TextField textField)
		{
			FieldName.Content = textField.Name;
			Binding b = new Binding();
			b.Source = textField;
			b.Path = new PropertyPath("Value");
			b.Mode = BindingMode.TwoWay;
			BindingOperations.SetBinding(Text, TextBox.TextProperty, b);
			AppendValidationToBinding(b, textField);
		}

		public override void SetEditable(bool isEditable)
		{
			Text.IsReadOnly = !isEditable;
		}
	}
}
