namespace Hotkeys;

using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using IniFileNet.IO;

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
		using (IniStreamReader keyCodeReader = new(new StreamReader(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "keycode.ini"), Encoding.UTF8), new IniReaderOptions(ignoreComments: true, allowCommentsNumberSign: true, allowKeyDelimiterColon: true)))
		{
			while (true)
			{
				var rr = keyCodeReader.Read();
				if (rr.Token == IniToken.Section && rr.Content == "Keycodes")
				{
					break;
				}
				else
				{
					MessageBox.Show("Failed to load keycode.ini. Could not find section Keycodes.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
			}
			bool go = true;
			string key = "";
			while (go)
			{
				var rr = keyCodeReader.Read();
				switch (rr.Token)
				{
					case IniToken.Section:
					case IniToken.End:
						go = false;
						break;
					case IniToken.Key:
						key = rr.Content.Trim();
						break;
					case IniToken.Value:
						uint vk;
						ReadOnlySpan<char> raw = rr.Content.Trim();
						NumberStyles ns;
						if (raw.StartsWith("0x"))
						{
							raw = raw[2..];
							ns = NumberStyles.HexNumber;
						}
						else
						{
							ns = NumberStyles.None;
						}
						if (uint.TryParse(raw, ns, null, out vk))
						{
							VkNameToCode[key] = vk;
							VkCodeToName[vk] = key;
						}
						else
						{
							MessageBox.Show(string.Concat("Failed to load keycode.ini. Could not parse key \"", key, "\" value \"", rr.Content, "\""), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
						}

						break;
					case IniToken.Error:
						MessageBox.Show("Failed to load keycode.ini. Could not find section Keycodes.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
						return;
				}
			}
		}

		Application.SetHighDpiMode(HighDpiMode.SystemAware);
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);

		HotkeyMessageProcessor? mp = null;
		try
		{
			mp = new HotkeyMessageProcessor((args.Length == 1) ? args[0] : "hotkeys.ini", true);
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
