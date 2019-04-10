using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Hotkeys
{
	[XmlRoot("root")]
	public class XmlHotkeys
	{
		[XmlArray("Hotkeys")]
		[XmlArrayItem("Hotkey")]
		public XmlHotkey[] Hotkeys { get; set; }
		[XmlArray("Chords")]
		[XmlArrayItem("Chord")]
		public XmlChord[] Chords { get; set; }
	}
	public class XmlHotkey
	{
		[XmlAttribute("type")]
		public string Type { get; set; }
		[XmlAttribute("name")]
		public string Name { get; set; }
		[XmlElement("Key")]
		public XmlKey XmlKey { get; set; }
		[XmlElement]
		public string Path { get; set; }
		[XmlElement]
		public string Args { get; set; }
		[XmlArray("Prompts")]
		[XmlArrayItem("P")]
		public XmlPrompt[] Prompts { get; set; }
		[XmlArray("Chords")]
		[XmlArrayItem("Chord")]
		public XmlChord[] Chords { get; set; }
	}
	public class XmlChord
	{
		[XmlAttribute("type")]
		public string Type { get; set; }
		[XmlAttribute("name")]
		public string Name { get; set; }
		[XmlElement("Key")]
		public XmlKey XmlKey { get; set; }
		[XmlElement]
		public string Path { get; set; }
		[XmlElement]
		public string Args { get; set; }
		[XmlArray("Prompts")]
		[XmlArrayItem("P")]
		public XmlPrompt[] Prompts { get; set; }
	}
	public class XmlKey
	{
		[XmlAttribute("mod")]
		public string RawMods { get; set; }
		[XmlText]
		public string Key { get; set; }
	}
	public class XmlPrompt
	{
		[XmlAttribute("key")]
		public string Key { get; set; }
		[XmlText]
		public string Prompt { get; set; }
	}
	public class HotkeyLoader
	{
		private readonly string loadPath;
		public IDictionary<int, Hotkey> LoadedChords { get; private set; }

		public HotkeyLoader(string loadPath)
		{
			this.loadPath = loadPath;
		}
		public void Load(IntPtr hWnd)
		{
			LoadedChords = new Dictionary<int, Hotkey>();
			XmlSerializer xs = new XmlSerializer(typeof(XmlHotkeys));
			XmlHotkeys xh;
			using (System.IO.FileStream fs = new System.IO.FileStream(loadPath, System.IO.FileMode.Open))
			{
				xh = (XmlHotkeys)xs.Deserialize(fs);
			}
			foreach (XmlHotkey rawHk in xh.Hotkeys)
			{
				// If a hotkey's a network path, it might not actually exist just yet, especially if we're loading on system boot
				// so don't check if the exe path exists....yet.
				Hotkey chord = LoadChord(rawHk, hWnd);
				if (chord != null)
				{
					LoadedChords.Add(chord.Id, chord);
				}
			}
		}
		private Hotkey LoadChord(XmlHotkey x, IntPtr hWnd)
		{
			uint vk = 0;
			if (Enum.TryParse(x.XmlKey.Key, out System.Windows.Forms.Keys k))
			{
				vk = (uint)k;
			}
			else
			{
				return null;
			}
			Keystroke keystroke = KeystrokeFromString(vk, x.XmlKey.RawMods);

			bool shellExec = false;
			Hotkey chord = null;
			switch (x.Type)
			{
				case "shell":
					shellExec = true;
					goto case "process";
				case "process":
					{
						// If we have a hotkey, then treat it as a chord anyways, with a single hotkey.
						chord = new Hotkey(keystroke, hWnd)
						{
							Name = x.Name
						};
						Prompt[] ps = null;
						if (x.Prompts?.Length > 0)
						{
							ps = new Prompt[x.Prompts.Length];
							for (int i = 0; i < x.Prompts.Length; i++)
							{
								XmlPrompt xp = x.Prompts[i];
								ps[i] = new Prompt() { Key = xp.Key, Question = xp.Prompt };
							}
						}
						chord.SetSingleChord(new Chord(keystroke)
						{
							Exec = x.Path,
							Args = x.Args,
							Name = x.Name,
							Prompts = ps,
							Shell = shellExec
						});
					}
					break;
				case "chord":
					chord = new Hotkey(keystroke, hWnd)
					{
						Name = x.Name
					};
					foreach (XmlChord xc in x.Chords)
					{
						if (Enum.TryParse(xc.XmlKey.Key, out System.Windows.Forms.Keys kc))
						{
							vk = (uint)kc;
						}
						else
						{
							return null;
						}
						// We set shellExec in each of this, because it might get changed
						switch (xc.Type)
						{
							case "process":
								shellExec = false;
								break;
							case "shell":
								shellExec = true;
								break;
							// Can't have nested chords, and everything else is wrong
							case "chord":
							default:
								return null;
						}
						Prompt[] ps = null;
						if (xc.Prompts?.Length > 0)
						{
							ps = new Prompt[xc.Prompts.Length];
							for (int i = 0; i < xc.Prompts.Length; i++)
							{
								XmlPrompt xp = x.Prompts[i];
								ps[i] = new Prompt() { Key = xp.Key, Question = xp.Prompt };
							}
						}
						Keystroke keystrokec = KeystrokeFromString(vk, xc.XmlKey.RawMods);
						chord.AddChord(new Chord(keystrokec)
						{
							Exec = xc.Path,
							Args = xc.Args,
							Name = xc.Name,
							Prompts = ps,
							Shell = shellExec
						});
					}
					break;
			}

			return chord;
		}
		private Keystroke KeystrokeFromString(uint vk, string s)
		{
			Keystroke mod = new Keystroke(vk);
			if (!string.IsNullOrEmpty(s))
			{
				if (s.IndexOf("ctrl", StringComparison.CurrentCultureIgnoreCase) != -1)
				{
					mod.Ctrl(true);
				}
				if (s.IndexOf("shift", StringComparison.CurrentCultureIgnoreCase) != -1)
				{
					mod.Shift(true);
				}
				if (s.IndexOf("alt", StringComparison.CurrentCultureIgnoreCase) != -1)
				{
					mod.Alt(true);
				}
				if (s.IndexOf("win", StringComparison.CurrentCultureIgnoreCase) != -1)
				{
					mod.Win(true);
				}
			}
			mod.NoRepeat(true);
			return mod;
		}
	}
}
