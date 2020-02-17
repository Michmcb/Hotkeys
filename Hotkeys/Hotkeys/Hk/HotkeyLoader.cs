using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Hotkeys.Hk
{
	public static class HotkeyLoader
	{
		private static readonly XName xType = "type";
		private static readonly XName xName = "name";
		private static readonly XName xMods = "mods";
		private static readonly XName xHotkey = "Hotkey";
		private static readonly XName xChords = "Chords";
		private static readonly XName xKey = "Key";
		private static readonly XName xPath = "Path";
		private static readonly XName xArgs = "Args";
		private static readonly XName xDir = "Dir";
		public static IDictionary<int, Hotkey> Load(string hotkeyLoadFile, IntPtr hWnd)
		{
			string ext = Path.GetExtension(hotkeyLoadFile);
			if (".xml".Equals(ext, StringComparison.OrdinalIgnoreCase))
			{
				return LoadXml(hotkeyLoadFile, hWnd);
			}
			else if (".json".Equals(ext, StringComparison.OrdinalIgnoreCase))
			{
				return LoadJson(hotkeyLoadFile, hWnd);
			}
			else
			{
				throw new ArgumentException("File must be either .xml or .json", nameof(hotkeyLoadFile));
			}
		}
		public static IDictionary<int, Hotkey> LoadXml(string xmlPath, IntPtr hWnd)
		{
			Dictionary<int, Hotkey> loadedHotkeys = new Dictionary<int, Hotkey>();
			using StreamReader xin = new StreamReader(xmlPath, System.Text.Encoding.UTF8);
			XDocument doc = XDocument.Load(xin);

			XElement array = doc.Root;

			foreach (XElement xhotkey in array.Elements())
			{
				if (xhotkey.Name == xHotkey)
				{
					Hotkey hk = LoadXmlHotkey(xhotkey, hWnd);
					loadedHotkeys.Add(hk.Id, hk);
				}
			}
			return loadedHotkeys;
		}
		public static IDictionary<int, Hotkey> LoadJson(string jsonPath, IntPtr hWnd)
		{
			Dictionary<int, Hotkey> loadedHotkeys = new Dictionary<int, Hotkey>();
			using JsonTextReader jin = new JsonTextReader(new StreamReader(jsonPath, System.Text.Encoding.UTF8))
			{
				CloseInput = true
			};
			JObject root = JObject.Load(jin);
			JToken? array = root["hotkeys"];
			if (array != null && array.Type == JTokenType.Array)
			{
				foreach (JObject jhotkey in array.Children<JObject>())
				{
					Hotkey hk = LoadJsonHotkey(jhotkey, hWnd);
					loadedHotkeys.Add(hk.Id, hk);
				}
			}
			return loadedHotkeys;
		}
		private static Hotkey LoadJsonHotkey(JObject jhk, IntPtr hWnd)
		{
			JToken? jtype = jhk["type"];
			JToken? jname = jhk["name"];
			JToken? jmods = jhk["mods"];
			JToken? jvk = jhk["vk"];
			// None of these are allowed to be null
			if (jtype == null || jname == null || jmods == null || jvk == null
				|| jtype.Type == JTokenType.Null || jname.Type == JTokenType.Null || jmods.Type == JTokenType.Null || jvk.Type == JTokenType.Null)
			{
				throw new InvalidDataException($"The type, name, mods, and vk properties are required at: {jhk.Path}");
			}

			string name = jname.Value<string>();
			string mods = jmods.Value<string>();
			uint vk = jvk.Value<uint>();
			string type = jtype.Value<string>();

			bool shellExec = false;
			Hotkey hk;
			switch (type)
			{
				case "shell":
					shellExec = true;
					goto case "process";
				case "process":
					JToken? jpath = jhk["path"];
					JToken? jargs = jhk["args"];
					JToken? jdir = jhk["dir"];
					if (jpath == null || jpath.Type != JTokenType.String)
					{
						throw new InvalidDataException($"If type is shell or process, the path property is required at: {jhk.Path}");
					}
					string path = jpath.Value<string>();
					string? args = (jargs != null && jargs.Type == JTokenType.String) ? jargs.Value<string>() : null;
					string? dir = !shellExec ? (jdir != null && jdir.Type == JTokenType.String) ? jdir.Value<string>() : null : null;
					hk = new Hotkey(name, KeystrokeFromVkAndMods(vk, mods), hWnd);
					InvokeTarget t = new InvokeTarget(path, args, dir, shellExec);
					hk.SetInvokeTarget(t);
					return hk;
				case "chord":
					hk = new Hotkey(name, KeystrokeFromVkAndMods(vk, mods), hWnd);
					JToken? jchords = jhk["chords"];
					if (jchords != null && jchords.Type == JTokenType.Array)
					{
						foreach (JObject jchord in jchords.Children<JObject>())
						{
							Chord? chord = LoadJsonChord(jchord);
							if (chord != null)
							{
								hk.AddChord(chord);
							}
						}
					}
					return hk;
				default:
				throw new InvalidDataException($"The type property for a hotkey must be either shell, process, or chord at: {jhk.Path}");
			}
		}
		private static Chord LoadJsonChord(JObject jchord)
		{
			JToken? jtype = jchord["type"];
			JToken? jname = jchord["name"];
			JToken? jmods = jchord["mods"];
			JToken? jvk = jchord["vk"];

			// None of these are allowed to be null
			if (jtype == null || jname == null || jmods == null || jvk == null
				|| jtype.Type == JTokenType.Null || jname.Type == JTokenType.Null || jmods.Type == JTokenType.Null || jvk.Type == JTokenType.Null)
			{
				throw new InvalidDataException($"The type, name, mods, and vk properties are required at: {jchord.Path}");
			}

			string name = jname.Value<string>();
			string mods = jmods.Value<string>();
			uint vk = jvk.Value<uint>();
			string type = jtype.Value<string>();

			bool shellExec = false;
			switch (type)
			{
				case "shell":
					shellExec = true;
					goto case "process";
				case "process":
					JToken? jpath = jchord["path"];
					JToken? jargs = jchord["args"];
					JToken? jdir = jchord["dir"];
					if (jpath == null || jpath.Type != JTokenType.String)
					{
						throw new InvalidDataException($"If type is shell or process, the path property is required at: {jchord.Path}");
					}
					string path = jpath.Value<string>();
					string? args = (jargs != null && jargs.Type == JTokenType.String) ? jargs.Value<string>() : null;
					string? dir = !shellExec ? (jdir != null && jdir.Type == JTokenType.String) ? jdir.Value<string>() : null : null;
					InvokeTarget t = new InvokeTarget(path, args, dir, shellExec);
					Chord c = new Chord(name, KeystrokeFromVkAndMods(vk, mods), t);
					return c;
				default:
				throw new InvalidDataException($"The type property for a chord must be either shell or process at: {jchord.Path}");
			}
		}
		private static Hotkey LoadXmlHotkey(XElement xhk, IntPtr hWnd)
		{
			XAttribute? xtype = xhk.Attribute(xType);
			XAttribute? xname = xhk.Attribute(xName);
			XElement? xkey = xhk.Element(xKey);
			XAttribute? xmods = xkey?.Attribute(xMods);

			// None of these are allowed to be null
			if (xtype == null || xname == null || xmods == null || xkey == null)
			{
				throw new InvalidDataException($"The type, name, and mods attributes and the Key element are required at: {GetAbsoluteXPath(xhk)}");
			}
			string name = xname.Value;
			string mods = xmods.Value;
			uint vk = uint.Parse(xkey.Value);
			string type = xtype.Value;

			bool shellExec = false;
			Hotkey hk;
			switch (type)
			{
				case "shell":
					shellExec = true;
					goto case "process";
				case "process":
					XElement? xpath = xhk.Element(xPath);
					XElement? xargs = xhk.Element(xArgs);
					XElement? xdir = xhk.Element(xDir);
					if (xpath == null)
					{
						throw new InvalidDataException($"If type is shell or process, the path property is required at: {GetAbsoluteXPath(xhk)}");
					}
					string path = xpath.Value;
					string? args = xargs?.Value;
					string? dir = shellExec ? null : xdir?.Value;
					hk = new Hotkey(name, KeystrokeFromVkAndMods(vk, mods), hWnd);
					InvokeTarget t = new InvokeTarget(path, args, dir, shellExec);
					hk.SetInvokeTarget(t);
					return hk;
				case "chord":
					hk = new Hotkey(name, KeystrokeFromVkAndMods(vk, mods), hWnd);
					XElement? xchords = xhk.Element(xChords);
					if (xchords != null)
					{
						foreach (XElement xchord in xchords.Elements())
						{
							Chord? chord = LoadXmlChord(xchord);
							if (chord != null)
							{
								hk.AddChord(chord);
							}
						}
					}
					return hk;
				default:
				throw new InvalidDataException($"The type attribute for a hotkey must be either shell, process, or chord at: {GetAbsoluteXPath(xhk)}");
			}
		}
		private static Chord LoadXmlChord(XElement xchord)
		{
			XAttribute? xtype = xchord.Attribute(xType);
			XAttribute? xname = xchord.Attribute(xName);
			XElement? xkey = xchord.Element(xKey);
			XAttribute? xmods = xkey?.Attribute(xMods);

			// None of these are allowed to be null
			if (xtype == null || xname == null || xmods == null || xkey == null)
			{
				throw new InvalidDataException($"The type, name, and mods attributes and the Key element are required at: {GetAbsoluteXPath(xchord)}");
			}
			string name = xname.Value;
			string mods = xmods.Value;
			uint vk = uint.Parse(xkey.Value);
			string type = xtype.Value;

			bool shellExec = false;
			switch (type)
			{
				case "shell":
					shellExec = true;
					goto case "process";
				case "process":
					XElement? xpath = xchord.Element(xPath);
					XElement? xargs = xchord.Element(xArgs);
					XElement? xdir = xchord.Element(xDir);
					if (xpath == null)
					{
						throw new InvalidDataException($"If type is shell or process, the path property is required at: {GetAbsoluteXPath(xchord)}");
					}
					string path = xpath.Value;
					string? args = xargs?.Value;
					string? dir = shellExec ? null : xdir?.Value;
					InvokeTarget t = new InvokeTarget(path, args, dir, shellExec);
					Chord c = new Chord(name, KeystrokeFromVkAndMods(vk, mods), t);
					return c;
				default:
				throw new InvalidDataException($"The type attribute for a chord must be either shell or process at: {GetAbsoluteXPath(xchord)}");
			}
		}
		private static Keystroke KeystrokeFromVkAndMods(uint vk, string s)
		{
			bool ctrl = false, shift = false, alt = false, win = false;
			if (!string.IsNullOrEmpty(s))
			{
				if (s.IndexOf("ctrl", StringComparison.CurrentCultureIgnoreCase) != -1)
				{
					ctrl = true;
				}
				if (s.IndexOf("shift", StringComparison.CurrentCultureIgnoreCase) != -1)
				{
					shift = true;
				}
				if (s.IndexOf("alt", StringComparison.CurrentCultureIgnoreCase) != -1)
				{
					alt = true;
				}
				if (s.IndexOf("win", StringComparison.CurrentCultureIgnoreCase) != -1)
				{
					win = true;
				}
			}
			Keystroke mod = new Keystroke(vk, ctrl, alt, shift, win);
			return mod;
		}
		/// <summary>
		/// Get the absolute XPath to a given XElement
		/// (e.g. "/people/person[6]/name[1]/last[1]").
		/// </summary>
		private static string GetAbsoluteXPath(XElement element)
		{
			if (element == null)
			{
				throw new ArgumentNullException(nameof(element));
			}

			static string relativeXPath(XElement e)
			{
				int? index = IndexPosition(e);
				string name = e.Name.LocalName;

				// If the element is the root, no index is required

				return (index == -1) ? "/" + name : $"/{name}[{index?.ToString() ?? "?"}]";
			}

			IEnumerable<string> ancestors = element.Ancestors().Select(x => relativeXPath(x));
			return string.Concat(ancestors.Reverse().ToArray()) +
				   relativeXPath(element);
		}
		/// <summary>
		/// Get the index of the given XElement relative to its
		/// siblings with identical names. If the given element is
		/// the root, -1 is returned.
		/// </summary>
		/// <param name="element">
		/// The element to get the index of.
		/// </param>
		private static int? IndexPosition(XElement? element)
		{
			if (element == null)
			{
				throw new ArgumentNullException(nameof(element));
			}
			if (element.Parent == null)
			{
				return -1;
			}

			int i = 1; // Indexes for nodes start at 1, not 0

			foreach (XElement sibling in element.Parent.Elements(element.Name))
			{
				if (sibling == element)
				{
					return i;
				}

				i++;
			}
			return null;
		}
	}
}
