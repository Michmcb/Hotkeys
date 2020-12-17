namespace Hotkeys
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using System.Windows.Forms;
	using ConfigTextFile;

	internal static class Program
	{
		public static IDictionary<string, uint> VkNameToCode = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase);
		public static IDictionary<uint, string> VkCodeToName = new Dictionary<uint, string>();
		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		private static void Main(string[] args)
		{
			LoadResult lr = ConfigFile.TryLoadFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "keycode.cfg"), Encoding.UTF8, LoadCommentsPreference.Ignore);
			if (lr.ConfigTextFile == null)
			{
				MessageBox.Show("Failed to load keycode.cfg. " + lr.ErrMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			List<string> errors = new();
			foreach (IConfigElement elem in lr.ConfigTextFile.Root.Elements.Values.Where(x => x.Type == ConfigElementType.String))
			{
				if (elem.Value.StartsWith("0x"))
				{
					if (uint.TryParse(elem.Value.AsSpan(2), NumberStyles.HexNumber, null, out uint keycode))
					{
						VkNameToCode[elem.Key] = keycode;
						VkCodeToName[keycode] = elem.Key;
					}
					else
					{
						errors.Add(string.Concat("Could not parse as a hexadecimal number: ", elem.Key, " = ", elem.Value));
					}
				}
				else
				{
					if (uint.TryParse(elem.Value, NumberStyles.None, null, out uint keycode))
					{
						VkNameToCode[elem.Key] = keycode;
						VkCodeToName[keycode] = elem.Key;
					}
					else
					{
						errors.Add(string.Concat("Could not parse as a decimal number: ", elem.Key, " = ", elem.Value));
					}
				}
			}

			Application.SetHighDpiMode(HighDpiMode.SystemAware);
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			HotkeyMessageProcessor? mp = null;
			try
			{
				mp = new HotkeyMessageProcessor((args.Length == 1) ? args[0] : "hotkeys.cfg", true);
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
