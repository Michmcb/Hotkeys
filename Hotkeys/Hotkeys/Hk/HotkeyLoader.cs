namespace Hotkeys.Hk
{
	using ConfigTextFile;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	public static class HotkeyLoader
	{
		public static IDictionary<int, Hotkey> Load(string hotkeyLoadFile, IntPtr hWnd)
		{
			string ext = Path.GetExtension(hotkeyLoadFile);
			return ".cfg".Equals(ext, StringComparison.OrdinalIgnoreCase)
				? LoadCfg(hotkeyLoadFile, hWnd)
				: throw new ArgumentException("File must be .cfg", nameof(hotkeyLoadFile));
		}
		public static IDictionary<int, Hotkey> LoadCfg(string cfgPath, IntPtr hWnd)
		{
			Dictionary<int, Hotkey> loadedHotkeys = new Dictionary<int, Hotkey>();
			LoadResult result = ConfigFile.TryLoadFile(cfgPath, System.Text.Encoding.UTF8);
			if (result.ConfigTextFile == null)
			{
				throw new InvalidDataException(result.ErrMsg);
			}
			foreach (KeyValuePair<string, IConfigElement> kvp in result.ConfigTextFile.Root.Elements)
			{
				if (kvp.Value.Type == ConfigElementType.Section)
				{
					Hotkey hk = LoadCfgHotkey(kvp.Value.AsSectionElement(), hWnd);
					loadedHotkeys.Add(hk.Id, hk);
				}
			}
			return loadedHotkeys;
		}
		public static Hotkey LoadCfgHotkey(ConfigSectionElement section, IntPtr hWnd)
		{
			string type = section.TryGetElement("type").Value;
			// None of these are allowed to be null, and they all have to be strings
			if (type.Length == 0)
			{
				throw new InvalidDataException("The type property is required for section: " + section.Path);
			}
			string mods = section.TryGetElement("mods").Value;
			if (mods.Length == 0)
			{
				throw new InvalidDataException("The mods property is required for section: " + section.Path);
			}
			string key = section.TryGetElement("key").Value;
			if (key.Length == 0)
			{
				throw new InvalidDataException("The key property is required for section: " + section.Path);
			}

			bool shellExec = false;
			Hotkey hk;
			switch (type)
			{
				case "shell":
					shellExec = true;
					goto case "process";
				case "process":
					string path = section.TryGetElement("path").Value;
					if (path.Length == 0)
					{
						throw new InvalidDataException("If type is shell or process, the path property is required for section: " + section.Path);
					}
					string args = section.TryGetElement("args").Value;
					string dir = section.TryGetElement("dir").Value;

					hk = new Hotkey(section.Key, KeystrokeFromKeyAndMods(key, mods), hWnd);
					InvokeTarget t = new InvokeTarget(path, args, dir, shellExec);
					hk.SetInvokeTarget(t);
					return hk;
				case "chord":
					hk = new Hotkey(section.Key, KeystrokeFromKeyAndMods(key, mods), hWnd);
					foreach (IConfigElement chordSection in section.Elements.Values.Where(x => x.Type == ConfigElementType.Section))
					{
						Chord? chord = LoadCfgChord(chordSection.AsSectionElement());
						if (chord != null)
						{
							hk.AddChord(chord);
						}
					}
					return hk;
				default:
					throw new InvalidDataException("The type property for a hotkey must be either shell, process, or chord for section: " + section.Path);
			}
		}
		private static Chord LoadCfgChord(ConfigSectionElement section)
		{
			string type = section.TryGetElement("type").Value;
			// None of these are allowed to be null, and they all have to be strings
			if (type.Length == 0)
			{
				throw new InvalidDataException("The type property is required and must be a non-empty string for section: " + section.Path);
			}
			string mods = section.TryGetElement("mods").Value;
			if (mods.Length == 0)
			{
				throw new InvalidDataException("The mods property is required and must be a non-empty string for section: " + section.Path);
			}
			string key = section.TryGetElement("key").Value;
			if (key.Length == 0)
			{
				throw new InvalidDataException("The key property is required and must be a non-empty string for section: " + section.Path);
			}

			bool shellExec = false;
			switch (type)
			{
				case "shell":
					shellExec = true;
					goto case "process";
				case "process":
					string path = section.TryGetElement("path").Value;
					if (path.Length == 0)
					{
						throw new InvalidDataException("If type is shell or process, the path property is required for section: " + section.Path);
					}
					string args = section.TryGetElement("args").Value;
					string dir = section.TryGetElement("dir").Value;

					Chord c = new Chord(section.Key, KeystrokeFromKeyAndMods(key, mods), new InvokeTarget(path, args, dir, shellExec));
					return c;
				default:
					throw new InvalidDataException("The type property for a hotkey must be either shell, process, or chord for section: " + section.Path);
			}
		}
		private static Keystroke KeystrokeFromKeyAndMods(string key, string mods)
		{
			bool ctrl = false, shift = false, alt = false, win = false;
			if (mods.IndexOf("ctrl", StringComparison.OrdinalIgnoreCase) != -1)
			{
				ctrl = true;
			}
			if (mods.IndexOf("shift", StringComparison.OrdinalIgnoreCase) != -1)
			{
				shift = true;
			}
			if (mods.IndexOf("alt", StringComparison.OrdinalIgnoreCase) != -1)
			{
				alt = true;
			}
			if (mods.IndexOf("win", StringComparison.OrdinalIgnoreCase) != -1)
			{
				win = true;
			}
			if (key.Length >= 2 && key[0] == '0' && key[1] == 'x')
			{
				// If it starts with 0x, interpret it as a hexadecimal number
				return new Keystroke(uint.Parse(key.AsSpan(2), System.Globalization.NumberStyles.HexNumber), ctrl, alt, shift, win);
			}
			else
			{
				// Otherwise, look it up in our dictionary
				return Program.VkNameToCode.TryGetValue(key, out uint vk)
					? new Keystroke(vk, ctrl, alt, shift, win)
					: throw new InvalidDataException("This is not a recognized key: " + key);
			}
		}
	}
}
