using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;

namespace XenoUI
{
	public partial class MainWindow : Window
	{
		private ClientsWindow clientsWindow = new ClientsWindow();
		private ScriptsWindow scriptsWindow = new ScriptsWindow();

		private DispatcherTimer timer;
		private string lastcontent;

		public MainWindow()
		{
			InitializeComponent();
			this.Icon = BitmapFrame.Create(new Uri("pack://application:,,,/Resources/Images/icon.ico"));

			InitializeWebView2();

			timer = new DispatcherTimer
			{
				Interval = TimeSpan.FromSeconds(3)
			};
			timer.Tick += savechanges;
			timer.Start();
		}

		private async void savechanges(object sender, EventArgs e)
		{
			string bin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");
			string tab = Path.Combine(bin, "editor.lua");

			string content = await GetScriptContent();
			if (lastcontent != content)
			{
				File.WriteAllText(tab, content);
			}

			lastcontent = content;
		}

		private async void InitializeWebView2()
		{
			try
			{
				await script_editor.EnsureCoreWebView2Async(null);


				string bin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");
				string monaco = Path.Combine(bin, "Monaco");
				string tab = Path.Combine(bin, "editor.lua");
				string indexPath = Path.Combine(monaco, "index.html");

				if (!Directory.Exists(bin))
				{
					Directory.CreateDirectory(bin);
				}
				if (!File.Exists(tab))
				{
					string content = "print(\"Hello, World!\")";
					File.WriteAllText(tab, content);
				}

				if (File.Exists(indexPath))
				{
					script_editor.Source = new Uri(indexPath);
					await LoadWebView();
					string fileContents = await File.ReadAllTextAsync(tab);
					string escapedContents = fileContents.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
					await script_editor.CoreWebView2.ExecuteScriptAsync($"setText(\"{escapedContents}\")");
				}
				else
				{
					MessageBox.Show("Could not load the Monaco", "File Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
					this.Close();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error initializing WebView2: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				this.Close();
			}
		}

		private async Task LoadWebView()
		{
			var tcs = new TaskCompletionSource<bool>();

			script_editor.CoreWebView2.NavigationCompleted += (sender, args) =>
			{
				if (args.IsSuccess)
				{
					tcs.SetResult(true);
				}
				else
				{
					tcs.SetException(new Exception($"Navigation failed with error code: {args.WebErrorStatus}"));
				}
			};

			await tcs.Task;
		}

		public async Task<string> GetScriptContent()
		{
			string textContent = await script_editor.CoreWebView2.ExecuteScriptAsync("getText()");

			if (textContent.StartsWith("\"") && textContent.EndsWith("\""))
			{
				textContent = textContent[1..^1];
			}

			return Regex.Unescape(textContent);
		}

		private void buttonMinimize_Click(object sender, RoutedEventArgs e)
		{
			this.WindowState = WindowState.Minimized;
		}

		private void buttonMaximize_Click(object sender, RoutedEventArgs e)
		{
			if (this.WindowState == WindowState.Maximized)
			{
				this.WindowState = WindowState.Normal;
				maximizeImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/maximize.png"));
				return;
			}
			this.WindowState = WindowState.Maximized;
			maximizeImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/normalize.png"));
		}

		private async void buttonClose_Click(object sender, RoutedEventArgs e)
		{
			string bin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");
			string tab = Path.Combine(bin, "editor.lua");

			string content = await GetScriptContent();
			File.WriteAllText(tab, content);

			Application.Current.Shutdown();
		}

		private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			DragMove();
		}

		private async void buttonExecute_Click(object sender, RoutedEventArgs e)
		{
			string result = await GetScriptContent();
			string compile_status = clientsWindow.GetCompilableStatus(result);
			if (compile_status != "success")
			{
				MessageBox.Show(compile_status, "Compiler Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return;
			}
			clientsWindow.execute_script(result);
		}

		private async void buttonClear_Click(object sender, RoutedEventArgs e)
		{
			await script_editor.CoreWebView2.ExecuteScriptAsync("setText(\"\")");
		}

		private async void buttonOpenFile_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Script files (*.lua;*.luau;*.txt)|*.lua;*.luau;*.txt|All files (*.*)|*.*";
			bool? result = openFileDialog.ShowDialog();

			if (result == true)
			{
				string filePath = openFileDialog.FileName;

				try
				{
					string fileContents = await File.ReadAllTextAsync(filePath);
					string escapedContents = fileContents.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
					await script_editor.CoreWebView2.ExecuteScriptAsync($"setText(\"{escapedContents}\")");
				}
				catch (Exception ex)
				{
					MessageBox.Show($"Error loading script: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		private async void buttonSaveFile_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				string textContent = await GetScriptContent();
				SaveFileDialog saveFileDialog = new SaveFileDialog();
				saveFileDialog.Filter = "Script files (*.lua;*.luau;*.txt)|*.lua;*.luau;*.txt|All files (*.*)|*.*";

				bool? result = saveFileDialog.ShowDialog();

				if (result == true)
				{
					string filePath = saveFileDialog.FileName;
					await File.WriteAllTextAsync(filePath, textContent, Encoding.UTF8);

					MessageBox.Show("File saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private async void buttonShowMultinstance_Click(object sender, RoutedEventArgs e)
		{
			if (clientsWindow.IsVisible)
			{
				clientsWindow.Hide();
				return;
			}
			clientsWindow.Owner = this;
			clientsWindow.Show();
		}

		private async void buttonShowScripts_Click(object sender, RoutedEventArgs e)
		{
			if (scriptsWindow.IsVisible)
			{
				scriptsWindow.Hide();
				return;
			}
			scriptsWindow.Owner = this;
			scriptsWindow.Show();
		}
	}
}
