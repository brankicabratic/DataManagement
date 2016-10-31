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
	/// Interaction logic for ComboBoxControl.xaml
	/// </summary>
	public partial class ComboBoxControl : FieldControl
	{
		public ComboBoxControl()
		{
			InitializeComponent();
		}

		public void Initialize(Models.ComboBox comboBox)
		{
			FieldName.Content = comboBox.Name;

			Binding b = new Binding();
			b.Source = comboBox;
			b.Path = new PropertyPath("Index");
			b.Mode = BindingMode.TwoWay;
			foreach (string value in comboBox.PossibleValues)
				Value.Items.Add(value);
			Value.SelectedIndex = comboBox.Index;
			BindingOperations.SetBinding(Value, System.Windows.Controls.ComboBox.SelectedIndexProperty, b);
		}

		public override void SetEditable(bool isEditable)
		{
			Value.IsEnabled = isEditable;
		}
	}
}
