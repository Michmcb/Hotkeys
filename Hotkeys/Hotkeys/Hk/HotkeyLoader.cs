using ConfigTextFile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Hotkeys.Hk
{
	public static class HotkeyLoader
	{
		public static readonly IReadOnlyDictionary<string, uint> VirtualKeys = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase)
		{
			{ "LBUTTON", 0x01 },
			{ "RBUTTON", 0x02 },
			{ "CANCEL", 0x03 },
			{ "MBUTTON", 0x04 },
			{ "XBUTTON1", 0x05 },
			{ "XBUTTON2", 0x06 },
			{ "BACK", 0x08 },
			{ "TAB", 0x09 },
			{ "CLEAR", 0x0C },
			{ "RETURN", 0x0D },
			{ "SHIFT", 0x10 },
			{ "CONTROL", 0x11 },
			{ "MENU", 0x12 },
			{ "PAUSE", 0x13 },
			{ "CAPSLOCK", 0x14 },
			{ "KANA", 0x15 },
			{ "HANGUEL", 0x15 },
			{ "HANGUL", 0x15 },
			{ "IME_ON", 0x16 },
			{ "JUNJA", 0x17 },
			{ "FINAL", 0x18 },
			{ "HANJA", 0x19 },
			{ "KANJI", 0x19 },
			{ "IME_OFF", 0x1A },
			{ "ESCAPE", 0x1B },
			{ "IME_CONVERT", 0x1C },
			{ "IME_NONCONVERT", 0x1D },
			{ "IME_ACCEPT", 0x1E },
			{ "IME_MODECHANGE", 0x1F },
			{ "SPACE", 0x20 },
			{ "PAGEUP", 0x21 },
			{ "PAGEDOWN", 0x22 },
			{ "END", 0x23 },
			{ "HOME", 0x24 },
			{ "LEFT", 0x25 },
			{ "UP", 0x26 },
			{ "RIGHT", 0x27 },
			{ "DOWN", 0x28 },
			{ "SELECT", 0x29 },
			{ "PRINT", 0x2A },
			{ "EXECUTE", 0x2B },
			{ "PRINTSCREEN", 0x2C },
			{ "INSERT", 0x2D },
			{ "DELETE", 0x2E },
			{ "HELP", 0x2F },
			{ "0", 0x30 },
			{ "1", 0x31 },
			{ "2", 0x32 },
			{ "3", 0x33 },
			{ "4", 0x34 },
			{ "5", 0x35 },
			{ "6", 0x36 },
			{ "7", 0x37 },
			{ "8", 0x38 },
			{ "9", 0x39 },
			{ "A", 0x41 },
			{ "B", 0x42 },
			{ "C", 0x43 },
			{ "D", 0x44 },
			{ "E", 0x45 },
			{ "F", 0x46 },
			{ "G", 0x47 },
			{ "H", 0x48 },
			{ "I", 0x49 },
			{ "J", 0x4A },
			{ "K", 0x4B },
			{ "L", 0x4C },
			{ "M", 0x4D },
			{ "N", 0x4E },
			{ "O", 0x4F },
			{ "P", 0x50 },
			{ "Q", 0x51 },
			{ "R", 0x52 },
			{ "S", 0x53 },
			{ "T", 0x54 },
			{ "U", 0x55 },
			{ "V", 0x56 },
			{ "W", 0x57 },
			{ "X", 0x58 },
			{ "Y", 0x59 },
			{ "Z", 0x5A },
			{ "LWIN", 0x5B },
			{ "RWIN", 0x5C },
			{ "APPS", 0x5D },
			{ "SLEEP", 0x5F },
			{ "NUMPAD0", 0x60 },
			{ "NUMPAD1", 0x61 },
			{ "NUMPAD2", 0x62 },
			{ "NUMPAD3", 0x63 },
			{ "NUMPAD4", 0x64 },
			{ "NUMPAD5", 0x65 },
			{ "NUMPAD6", 0x66 },
			{ "NUMPAD7", 0x67 },
			{ "NUMPAD8", 0x68 },
			{ "NUMPAD9", 0x69 },
			{ "MULTIPLY", 0x6A },
			{ "ADD", 0x6B },
			{ "SEPARATOR", 0x6C },
			{ "SUBTRACT", 0x6D },
			{ "DECIMAL", 0x6E },
			{ "DIVIDE", 0x6F },
			{ "F1", 0x70 },
			{ "F2", 0x71 },
			{ "F3", 0x72 },
			{ "F4", 0x73 },
			{ "F5", 0x74 },
			{ "F6", 0x75 },
			{ "F7", 0x76 },
			{ "F8", 0x77 },
			{ "F9", 0x78 },
			{ "F10", 0x79 },
			{ "F11", 0x7A },
			{ "F12", 0x7B },
			{ "F13", 0x7C },
			{ "F14", 0x7D },
			{ "F15", 0x7E },
			{ "F16", 0x7F },
			{ "F17", 0x80 },
			{ "F18", 0x81 },
			{ "F19", 0x82 },
			{ "F20", 0x83 },
			{ "F21", 0x84 },
			{ "F22", 0x85 },
			{ "F23", 0x86 },
			{ "F24", 0x87 },
			{ "NUMLOCK", 0x90 },
			{ "SCROLLLOCK", 0x91 },
			{ "LSHIFT", 0xA0 },
			{ "RSHIFT", 0xA1 },
			{ "LCONTROL", 0xA2 },
			{ "RCONTROL", 0xA3 },
			{ "LMENU", 0xA4 },
			{ "RMENU", 0xA5 },
			{ "BROWSER_BACK", 0xA6 },
			{ "BROWSER_FORWARD", 0xA7 },
			{ "BROWSER_REFRESH", 0xA8 },
			{ "BROWSER_STOP", 0xA9 },
			{ "BROWSER_SEARCH", 0xAA },
			{ "BROWSER_FAVORITES", 0xAB },
			{ "BROWSER_HOME", 0xAC },
			{ "VOLUME_MUTE", 0xAD },
			{ "VOLUME_DOWN", 0xAE },
			{ "VOLUME_UP", 0xAF },
			{ "MEDIA_NEXT_TRACK", 0xB0 },
			{ "MEDIA_PREV_TRACK", 0xB1 },
			{ "MEDIA_STOP", 0xB2 },
			{ "MEDIA_PLAY_PAUSE", 0xB3 },
			{ "LAUNCH_MAIL", 0xB4 },
			{ "LAUNCH_MEDIA_SELECT", 0xB5 },
			{ "LAUNCH_APP1", 0xB6 },
			{ "LAUNCH_APP2", 0xB7 },
			
			// For any country/region...
			{ "OEM_PLUS", 0xBB }, // +
			{ "OEM_COMMA", 0xBC }, // ,
			{ "OEM_MINUS", 0xBD }, // -
			{ "OEM_PERIOD", 0xBE }, // .

			// These are for miscellaneous chars, it can vary by keyboard. Comments indicate the US Standard Keyboard keys
			{ "OEM_1", 0xBA }, // ;:
			{ "OEM_2", 0xBF }, // /?
			{ "OEM_3", 0xC0 }, // `~
			{ "OEM_4", 0xDB }, // [{
			{ "OEM_5", 0xDC }, // \|
			{ "OEM_6", 0xDD }, // ]}
			{ "OEM_7", 0xDE }, // '"
			{ "OEM_8", 0xDF },
			{ "OEM_102", 0xE2 },

			{ "IME_PROCESS", 0xE5 },
			{ "ATTN", 0xF6 },
			{ "CRSEL", 0xF7 },
			{ "EXSEL", 0xF8 },
			{ "EREOF", 0xF9 },
			{ "PLAY", 0xFA },
			{ "ZOOM", 0xFB },
			{ "PA1", 0xFD },
			{ "OEM_CLEAR", 0xFE }
		};
		public static IDictionary<int, Hotkey> Load(string hotkeyLoadFile, IntPtr hWnd)
		{
			string ext = Path.GetExtension(hotkeyLoadFile);
			if (".cfg".Equals(ext, StringComparison.OrdinalIgnoreCase))
			{
				return LoadCfg(hotkeyLoadFile, hWnd);
			}
			else
			{
				throw new ArgumentException("File must be .cfg", nameof(hotkeyLoadFile));
			}
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
				if (VirtualKeys.TryGetValue(key, out uint vk))
				{
					return new Keystroke(vk, ctrl, alt, shift, win);
				}
				else
				{
					throw new InvalidDataException("This is not a recognized key: " + key);
				}
			}
		}
	}
}
