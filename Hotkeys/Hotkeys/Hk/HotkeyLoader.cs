namespace Hotkeys.Hk
{
	using IniFileNet;
	using IniFileNet.IO;
	using System;
	using System.Collections.Generic;
	using System.IO;

	public static class HotkeyLoader
	{
		public static Dictionary<int, Hotkey> Load(string iniPath, IntPtr hWnd)
		{
			Dictionary<int, Hotkey> loadedHotkeys = [];
			Dictionary<string, Hotkey> chords = [];

			using IniStreamSectionReader iniReader = new(new IniStreamReader(new StreamReader(iniPath), new IniReaderOptions(allowCommentsNumberSign: true, allowKeyDelimiterColon: true, ignoreComments: true)));

			IniValueAcceptorDictionaryBuilder b = new([]);
			IniValueAcceptorOnlyLast type = b.OnlyLast("type");
			IniValueAcceptorOnlyLast mods = b.OnlyLast("mods");
			IniValueAcceptorOnlyLast key = b.OnlyLast("key");
			IniValueAcceptorOnlyLast path = b.OnlyLast("path");
			IniValueAcceptorOnlyLast args = b.OnlyLast("args");
			IniValueAcceptorOnlyLast dir = b.OnlyLast("dir");
			var acceptors = b.Acceptors;
			b = null!;
			Hotkey? lastSeenHk = null;
			while (iniReader.TryReadNext(out var sec))
			{
				Util.ResetAll(acceptors.Values);
				sec.AcceptAll(acceptors).ThrowIfError();
				if (string.IsNullOrEmpty(type.Value)) { throw new IniException(IniErrorCode.ValueMissing, "The type property is required for section " + sec.Name); }
				if (string.IsNullOrEmpty(mods.Value)) { throw new IniException(IniErrorCode.ValueMissing, "The mods property is required for section " + sec.Name); }
				if (string.IsNullOrEmpty(key.Value)) { throw new IniException(IniErrorCode.ValueMissing, "The key property is required for section " + sec.Name); }

				bool shellExec = false;
				switch (type.Value)
				{
					case "shell":
						shellExec = true;
						goto case "process";
					case "process":
						if (string.IsNullOrEmpty(path.Value))
						{
							throw new IniException(IniErrorCode.ValueMissing, "If type is shell or process, the path property is required for section: " + sec.Name);
						}
						int dot = sec.Name.IndexOf('.');
						if (dot != -1)
						{
							Hotkey? chk;
							if (dot == 0)
							{
								chk = lastSeenHk ?? throw new IniException(IniErrorCode.ValueInvalid, "Chord with dot as name, but there was no hotkey preceding it " + sec.Name);
							}
							else
							{
								string chordName = sec.Name[..dot];
								if (!chords.TryGetValue(chordName, out chk))
								{
									throw new IniException(IniErrorCode.ValueInvalid, "There is no chord with the name " + chordName);
								}
							}
							Chord c = new((dot + 1 < sec.Name.Length) ? sec.Name[(dot + 1)..] : sec.Name, KeystrokeFromKeyAndMods(key.Value, mods.Value), new InvokeTarget(path.Value, args.Value, dir.Value, shellExec));
							chk.AddChord(c);
							break;
						}

						var hk = new Hotkey(sec.Name, KeystrokeFromKeyAndMods(key.Value, mods.Value), hWnd);
						InvokeTarget t = new(path.Value, args.Value, dir.Value, shellExec);
						hk.SetInvokeTarget(t);
						loadedHotkeys[hk.Id] = hk;
						break;
					case "chord":
						lastSeenHk = hk = new Hotkey(sec.Name, KeystrokeFromKeyAndMods(key.Value, mods.Value), hWnd);
						loadedHotkeys[hk.Id] = hk;
						chords[sec.Name] = hk;
						break;
					default:
						throw new IniException(IniErrorCode.ValueInvalid, "The type property for a hotkey must be either shell, process, or chord for section: " + sec.Name);
				}
			}

			iniReader.Reader.Error.ThrowIfError();
			return loadedHotkeys;
		}
		private static Keystroke KeystrokeFromKeyAndMods(string key, string mods)
		{
			bool ctrl = false, shift = false, alt = false, win = false;
			if (mods.Contains("ctrl", StringComparison.OrdinalIgnoreCase))
			{
				ctrl = true;
			}
			if (mods.Contains("shift", StringComparison.OrdinalIgnoreCase))
			{
				shift = true;
			}
			if (mods.Contains("alt", StringComparison.OrdinalIgnoreCase))
			{
				alt = true;
			}
			if (mods.Contains("win", StringComparison.OrdinalIgnoreCase))
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
