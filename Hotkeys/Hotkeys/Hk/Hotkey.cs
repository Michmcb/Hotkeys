using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Hotkeys.Hk
{
	/// <summary>
	/// Represents a keystroke that can be pressed to invoke something.
	/// </summary>
	public class Hotkey : IEquatable<Hotkey>
	{
		private readonly IntPtr hWnd;
		private readonly Dictionary<Keystroke, Chord> chords;
		public Keystroke Keystroke { get; }
		public int Id { get; private set; }
		public string Name { get; set; }
		public bool IsRegistered { get; private set; }
		public IReadOnlyDictionary<Keystroke, Chord> Chords => chords;
		public InvokeTarget? InvokeTarget { get; private set; }
		/// <summary>
		/// If the hotkey has multiple chords, true. Otherwise, false.
		/// </summary>
		public bool HasChords => InvokeTarget == null;
		/// <summary>
		/// Constructs a new instance of Hotkey
		/// </summary>
		/// <param name="keystroke">The keystroke to invoke this hotkey</param>
		/// <param name="hWnd">The handle to the window which owns this hotkey</param>
		public Hotkey(string name, Keystroke keystroke, IntPtr hWnd)
		{
			Id = HotkeyId.GetNextId();
			IsRegistered = false;
			chords = new Dictionary<Keystroke, Chord>();
			Name = name;
			Keystroke = keystroke;
			this.hWnd = hWnd;
		}
		/// <summary>
		/// Sets the single target that will be invoked when this hotkey is pressed.
		/// The chord will not require any extra keypresses; it will be invoked straight away.
		/// This method clears the Chords property
		/// </summary>
		/// <param name="t">The trigger to invoke on pressing this hotkey</param>
		public bool SetInvokeTarget(InvokeTarget t)
		{
			// Totally fine for a single hotkey to have the windows key; we'll just route it straight to the single hotkey.
			if (!IsRegistered)
			{
				chords.Clear();
				InvokeTarget = t;
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
				InvokeTarget = null;
				// If _hotkeys is null, create it first
				chords.Add(ch.Keystroke, ch);
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
			if (!IsRegistered && (InvokeTarget != null || chords != null))
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
		/// Gets the Process that should be invoked for this Hotkey, using the provided <paramref name="chordProcessor"/> to
		/// get the specific Chord the user wants to invoke
		/// </summary>
		/// <param name="chordKeystroke">The Chord's keystroke</param>
		public Result<Process?, ProcErrorCode> GetProc(Keystroke chordKeystroke, string clipboard)
		{
			if (InvokeTarget != null)
			{
				throw new InvalidOperationException("Cannot call GetProc if property SingleChord is not null");
			}
			// If the chord is null, that means the user doesn't want to invoke anything after all
			if (chords.TryGetValue(chordKeystroke, out Chord? chord))
			{
				return chord.GetProcess(clipboard);
			}
			return new Result<Process?, ProcErrorCode>(null, ProcErrorCode.UserCancelled);
		}
		/// <summary>
		/// Gets the Process that should be invoked for this Hotkey, assuming the user doesn't have to press
		/// any extra Chords.
		/// </summary>
		/// <param name="clipboard">The contents of the clipboard</param>
		public Result<Process?, ProcErrorCode> GetSingleProc(string clipboard)
		{
			if (InvokeTarget == null)
			{
				throw new InvalidOperationException("Cannot call GetSingleProc if property SingleChord is null");
			}
			return InvokeTarget.GetProcess(clipboard);
		}
		public override string ToString()
		{
			return $"{Keystroke.ToString()}: {Name}";
		}
		public override bool Equals(object? obj)
		{
			return Equals(obj as Hotkey);
		}
		public bool Equals([AllowNull] Hotkey? other)
		{
			return other != null &&
				   Id == other.Id;
		}
		public override int GetHashCode()
		{
			return HashCode.Combine(Id);
		}
		public static bool operator ==(Hotkey? left, Hotkey? right)
		{
			return EqualityComparer<Hotkey>.Default.Equals(left, right);
		}
		public static bool operator !=(Hotkey? left, Hotkey? right)
		{
			return !(left == right);
		}
	}
}
