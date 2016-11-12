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
		private Models.ComboBox comboBox;

		public ComboBoxControl()
		{
			InitializeComponent();
		}

		public void Initialize(Models.ComboBox comboBox)
		{
			this.comboBox = comboBox;
			FieldName.Content = comboBox.Name;

			foreach (string value in comboBox.PossibleValues)
				Value.Items.Add(value);
			if (IsMandatory)
				Value.Items.Add("");

			if (!IsMandatory)
			{
				Binding b = new Binding();
				b.Source = comboBox;
				b.Path = new PropertyPath("Index");
				b.Mode = BindingMode.TwoWay;
				Value.SelectedIndex = comboBox.Index;
				BindingOperations.SetBinding(Value, System.Windows.Controls.ComboBox.SelectedIndexProperty, b);
			}
			else
			{
				Value.SelectedIndex = comboBox.PossibleValues.Count;
			}
		}

		public override void SetEditable(bool isEditable)
		{
			Value.IsEnabled = isEditable;
		}

		private void Value_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			IsEdited = !String.IsNullOrEmpty(Value.SelectedItem.ToString());
			if (IsMandatory && IsEdited)
			{
				Binding b = new Binding();
				b.Source = comboBox;
				b.Path = new PropertyPath("Index");
				b.Mode = BindingMode.TwoWay;
				Value.Items.Remove("");
				comboBox.Index = Value.SelectedIndex;
				BindingOperations.SetBinding(Value, System.Windows.Controls.ComboBox.SelectedIndexProperty, b);
			}
		}
	}
}
