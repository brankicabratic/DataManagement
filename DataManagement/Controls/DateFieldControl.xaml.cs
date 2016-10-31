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
	/// Interaction logic for DateFieldControl.xaml
	/// </summary>
	public partial class DateFieldControl : FieldControl
	{
		public DateFieldControl()
		{
			InitializeComponent();
		}

		public void Initialize(DateField dateField)
		{
			FieldName.Content = dateField.Name;
			Binding b = new Binding();
			b.Source = dateField;
			b.Path = new PropertyPath("DateValue");
			b.Mode = BindingMode.TwoWay;
			BindingOperations.SetBinding(Date, DatePicker.SelectedDateProperty, b);
		}

		public override void SetEditable(bool isEditable)
		{
			Date.IsEnabled = isEditable;
		}
	}
}
