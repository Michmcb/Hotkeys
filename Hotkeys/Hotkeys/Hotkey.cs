using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Hotkeys
{
	/// <summary>
	/// Represents a keystroke that can be pressed to invoke something.
	/// </summary>
	public class Hotkey
	{
		private readonly IntPtr hWnd;

		private Dictionary<Keystroke, Chord> _chords;
		private Chord _singleChord;

		public Keystroke Keystroke { get; }
		public int Id { get; private set; }
		public string Name { get; set; }
		public bool IsRegistered { get; private set; }
		public IReadOnlyDictionary<Keystroke, Chord> Chords => _chords;
		public Chord SingleChord { get; }

		/// <summary>
		/// Constructs a new instance of Hotkey
		/// </summary>
		/// <param name="keystroke">The keystroke to invoke this hotkey</param>
		/// <param name="hWnd">The handle to the window which owns this hotkey</param>
		public Hotkey(Keystroke keystroke, IntPtr hWnd)
		{
			Id = HotkeyId.GetNextId();
			IsRegistered = false;
			Keystroke = keystroke;
			this.hWnd = hWnd;
		}
		/// <summary>
		/// Sets the single chord that will be invoked when this hotkey is pressed.
		/// The chord will not require any extra keypresses; it will be invoked straight away.
		/// This method clears the Chords property
		/// </summary>
		/// <param name="ch">The chord to invoke on pressing this hotkey</param>
		public bool SetSingleChord(Chord ch)
		{
			// Totally fine for a single hotkey to have the windows key; we'll just route it straight to the single hotkey.
			if (!IsRegistered)
			{
				_chords = null;
				_singleChord = ch;
				return true;
			}
			return false;
		}
		/// <summary>
		/// Adds a chord whose keystroke may be pressed to invoke it, after invoking this
		/// hotkey.
		/// The chord may not define the Windows key as a modifier. But it may define the Windows key as the keystroke itself.
		/// </summary>
		/// <param name="ch">A chord which can be invoked with a second keystroke after pressing this hotkey</param>
		public bool AddChord(Chord ch)
		{
			if (!IsRegistered && !ch.Keystroke.HasWin)
			{
				_singleChord = null;
				// If _hotkeys is null, create it first
				(_chords ?? (_chords = new Dictionary<Keystroke, Chord>())).Add(ch.Keystroke, ch);
				return true;
			}
			return false;
		}
		/// <summary>
		/// Registers this hotkey globally with Windows. Will fail if something else has already taken the keystroke,
		/// or if this hotkey has no chords.
		/// </summary>
		public bool Register()
		{
			if (!IsRegistered && (_singleChord != null || _chords != null))
			{
				IsRegistered = NativeMethods.RegisterHotKey(hWnd, Id, Keystroke.Modifiers, Keystroke.Vk);
			}
			return IsRegistered;
		}
		/// <summary>
		/// Unregisters this hotkey with Windows.
		/// </summary>
		public bool Unregister()
		{
			if (IsRegistered)
			{
				IsRegistered = !NativeMethods.UnregisterHotKey(hWnd, Id);
			}
			return !IsRegistered;
		}
		/// <summary>
		/// Invokes this hotkey. If this Hotkey has only one chord, that chord will be invoked immediately.
		/// Otherwise, the HotkeyExecutor will be queried for an additional keystroke to match one of this Hotkey's chords.
		/// </summary>
		/// <param name="hotkeyExecutor">The HotkeyExecutor used to query for a chord. Not required if this Hotkey only has one chord.</param>
		public Error.Proc Proc(HotkeyExecutor hotkeyExecutor, string clipboard)
		{
			if (_singleChord != null)
			{
				return ProcChord(_singleChord, clipboard);
			}
			else
			{
				Chord ch = hotkeyExecutor.GetChord(this);
				return ProcChord(ch, clipboard);
			}
		}
		/// <summary>
		/// Invokes the specified chord.
		/// </summary>
		/// <param name="ch">The chord to invoke</param>
		private static Error.Proc ProcChord(Chord ch, string clipboard)
		{
			PromptResponse[] prs = null;
			if (ch.NeedsPrompt)
			{
				ValuePrompt vp = new ValuePrompt();
				prs = new PromptResponse[ch.Prompts.Length];
				for (int i = 0; i < ch.Prompts.Length; i++)
				{
					vp.Text = $"{ch.Name}: Enter Value for {ch.Prompts[i].Key}";
					vp.SetQuestion(ch.Prompts[i].Question);
					// The prompt must be the top-level window. It's fine; they pressed a hotkey which they specifically want a prompt for, so doing this isn't evil :)
					vp.TopMost = true;
					DialogResult result = vp.ShowDialog();
					string resp = (result == DialogResult.OK) ? vp.GetAnswer() : "";
					prs[i] = new PromptResponse() { Key = ch.Prompts[i].Key, Response = vp.GetAnswer() };
				}
			}
			return ch.Proc(clipboard, prs);
		}
		public override string ToString()
		{
			return $"{Keystroke.ToString()}: {Name}";
		}
		public override bool Equals(object obj)
		{
			return obj is Chord chord &&
				   Id == chord.Id;
		}
		public override int GetHashCode()
		{
			return 2108858624 + Id.GetHashCode();
		}
	}
}
