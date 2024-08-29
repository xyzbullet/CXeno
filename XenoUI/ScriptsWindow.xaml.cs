using System.Windows;
using System.Windows.Input;

namespace XenoUI
{
    public partial class ScriptsWindow : Window
    {
        public ScriptsWindow()
        {
            InitializeComponent();
			MouseLeftButtonDown += Window_MouseLeftButtonDown;
		}
		private void buttonClose_Click(object sender, RoutedEventArgs e)
		{
			Hide();
		}
		private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			DragMove();
		}
	}
}
