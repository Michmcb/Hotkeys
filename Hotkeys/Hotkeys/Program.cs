using System;
using System.Windows.Forms;

namespace Hotkeys
{
	internal static class Program
	{
		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		private static void Main(string[] args)
		{
			Application.SetHighDpiMode(HighDpiMode.SystemAware);
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			HotkeyMessageProcessor? mp = null;
			try
			{
				mp = new HotkeyMessageProcessor((args.Length == 1) ? args[0] : "hotkeys.xml", true);
				Application.Run(mp);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				if (mp?.CurrentlyRegistered == true)
				{
					mp.Unregister(null, EventArgs.Empty);
				}
				mp?.Dispose();
			}
		}
	}
}
