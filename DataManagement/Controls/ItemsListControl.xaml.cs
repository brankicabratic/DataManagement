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
	/// Interaction logic for ItemsListControl.xaml
	/// </summary>
	public partial class ItemsListControl : FieldControl
	{
		public event Action OnAddClick;
		public event Action<int> OnRemoveClick;
		
		private bool isEditable = true;

		public ItemsListControl()
		{
			InitializeComponent();
		}

		public void Initialize(string name)
		{
			FieldName.Content = name;
		}

		public void AddNewItem(FieldControl item)
		{
			StackPanel sp = new StackPanel();
			sp.Orientation = Orientation.Horizontal;
			ItemsContainer.Children.Add(sp);
			sp.Children.Add(item);
			item.SetEditable(isEditable);
			Button removeButton = new Button();
			removeButton.Content = "-";
			removeButton.Width = ButtonAdd.Width;
			removeButton.Height = ButtonAdd.Height;
			removeButton.IsEnabled = isEditable;
			removeButton.VerticalAlignment = VerticalAlignment.Top;
			removeButton.Margin = new Thickness(5);
			removeButton.Click += (s, e) =>
			{
				if (ItemsContainer.Children.Contains(sp))
				{
					if (OnRemoveClick != null)
					{
						OnRemoveClick(ItemsContainer.Children.IndexOf(sp));
					}
					ItemsContainer.Children.Remove(sp);
				}
			};
			sp.Children.Add(removeButton);
		}

		public void RemoveItemAt(int index)
		{
			ItemsContainer.Children.RemoveAt(index);
		}

		public override void SetEditable(bool isEditable)
		{
			ButtonAdd.IsEnabled = isEditable;
			foreach (UIElement stackPanel in ItemsContainer.Children)
			{
				foreach (UIElement element in (stackPanel as StackPanel).Children)
				{
					if (element is FieldControl)
					{
						((FieldControl)element).SetEditable(isEditable);
					}
					else
					{
						element.IsEnabled = isEditable;
					}
				}
			}
			this.isEditable = isEditable;
		}

		private void ButtonAdd_Click(object sender, RoutedEventArgs e)
		{
			if (OnAddClick != null) OnAddClick();
		}
	}
}
