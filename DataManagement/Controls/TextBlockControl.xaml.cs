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
	/// Interaction logic for TextBlockControl.xaml
	/// </summary>
	public partial class TextBlockControl : FieldControl
	{
		public TextBlockControl()
		{
			InitializeComponent();
		}

		public void Initialize(Models.TextBlock textBlock)
		{
			FieldName.Content = textBlock.Name;
			Binding b = new Binding();
			b.Source = textBlock;
			b.Path = new PropertyPath("Value");
			b.Mode = BindingMode.TwoWay;
			BindingOperations.SetBinding(Text, TextBox.TextProperty, b);
		}

		public override void SetEditable(bool isEditable)
		{
			Text.IsReadOnly = !isEditable;
		}
	}
}
