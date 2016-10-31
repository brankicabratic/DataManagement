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
	/// Interaction logic for CheckBoxControl.xaml
	/// </summary>
	public partial class CheckBoxControl : FieldControl
	{
		public CheckBoxControl()
		{
			InitializeComponent();
		}

		public void Initialize(Models.CheckBox checkBox)
		{
			FieldName.Content = checkBox.Name;

			Binding b = new Binding();
			b.Source = checkBox;
			b.Path = new PropertyPath("Value");
			b.Mode = BindingMode.TwoWay;
			BoolValue.IsChecked = checkBox.Value;
			BindingOperations.SetBinding(BoolValue, System.Windows.Controls.CheckBox.IsCheckedProperty, b);
		}

		public override void SetEditable(bool isEditable)
		{
			BoolValue.IsEnabled = isEditable;
		}
	}
}
