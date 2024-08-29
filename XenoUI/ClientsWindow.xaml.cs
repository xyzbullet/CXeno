using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace XenoUI
{
	public partial class ClientsWindow : Window
	{
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public struct ClientInfo
		{
			[MarshalAs(UnmanagedType.LPStr)]
			public string name;
			public int id;
		}

		[DllImport("Xeno.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void Initialize();

		[DllImport("Xeno.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr GetClients();

		[DllImport("Xeno.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern void Execute(byte[] scriptSource, string[] clientUsers, int numUsers);
		[DllImport("Xeno.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern IntPtr Compilable(byte[] scriptSource);

		private DispatcherTimer timer;
		public List<ClientInfo> ActiveClients { get; private set; } = new List<ClientInfo>();

		public void execute_script(string script)
		{
			string[] clientUsers = new string[ActiveClients.Count];
			for (int i = 0; i < ActiveClients.Count; i++)
			{
				clientUsers[i] = ActiveClients[i].name;
			}
			byte[] scriptBytes = Encoding.UTF8.GetBytes(script);
			Execute(scriptBytes, clientUsers, clientUsers.Length);
		}
		public string GetCompilableStatus(string scriptSource)
		{
			byte[] scriptSourceBytes = System.Text.Encoding.ASCII.GetBytes(scriptSource);
			IntPtr resultPtr = Compilable(scriptSourceBytes);
			string result = Marshal.PtrToStringAnsi(resultPtr);
			Marshal.FreeCoTaskMem(resultPtr);

			return result;
		}

		public ClientsWindow()
		{
			InitializeComponent();
			Initialize();
			MouseLeftButtonDown += Window_MouseLeftButtonDown;
			timer = new DispatcherTimer
			{
				Interval = TimeSpan.FromMilliseconds(100)
			};
			timer.Tick += Timer_Tick;
			timer.Start();
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			var newClients = GetClientInfoFromDll();
			var newClientIds = new HashSet<int>(newClients.Select(c => c.id));

			// Remove invalid clients
			var checkBoxesToRemove = new List<CheckBox>();
			foreach (var child in checkBoxContainer.Children)
			{
				if (child is CheckBox checkBox)
				{
					string[] parts = checkBox.Content.ToString().Split(new[] { ", PID: " }, StringSplitOptions.None);
					if (parts.Length == 2 && int.TryParse(parts[1].Trim(), out int id))
					{
						if (!newClientIds.Contains(id))
						{
							checkBoxesToRemove.Add(checkBox);
						}
					}
				}
			}

			foreach (var checkBox in checkBoxesToRemove)
			{
				checkBoxContainer.Children.Remove(checkBox);
			}

			// Add new clients
			foreach (var client in newClients)
			{
				bool exists = false;
				foreach (var child in checkBoxContainer.Children)
				{
					if (child is CheckBox checkBox)
					{
						if (checkBox.Content.ToString() == $"{client.name}, PID: {client.id}")
						{
							exists = true;
							break;
						}
					}
				}
				if (!exists && client.name != "" && client.name != " ")
				{
					AddCheckBox($"{client.name}, PID: {client.id}");
				}
			}

			// Update ActiveClients
			ActiveClients.Clear();
			foreach (var child in checkBoxContainer.Children)
			{
				if (child is CheckBox checkBox && checkBox.IsChecked == true)
				{
					string[] parts = checkBox.Content.ToString().Split(new[] { ", PID: " }, StringSplitOptions.None);
					if (parts.Length == 2)
					{
						string name = parts[0].Trim();
						if (int.TryParse(parts[1].Trim(), out int id))
						{
							ActiveClients.Add(new ClientInfo { name = name, id = id });
						}
					}
				}
			}
		}

		private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			DragMove();
		}

		private void AddCheckBox(string content)
		{
			CheckBox newCheckBox = new CheckBox
			{
				Content = content,
				Foreground = Brushes.White,
				FontFamily = new FontFamily("Franklin Gothic Medium"),
				IsChecked = true,
				Background = Brushes.Black
			};

			checkBoxContainer.Children.Add(newCheckBox);
		}

		private void buttonClose_Click(object sender, RoutedEventArgs e)
		{
			Hide();
		}

		private List<ClientInfo> GetClientInfoFromDll()
		{
			IntPtr ptr = GetClients();
			var clients = new List<ClientInfo>();
			IntPtr currentPtr = ptr;
			ClientInfo client;

			while (true)
			{
				client = Marshal.PtrToStructure<ClientInfo>(currentPtr);
				if (client.name == null)
				{
					break;
				}

				clients.Add(client);
				currentPtr += Marshal.SizeOf<ClientInfo>();
			}

			return clients;
		}
	}
}
