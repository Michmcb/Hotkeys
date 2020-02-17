using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Hotkeys.Hk
{
	/// <summary>
	/// Represents a second keystroke that can come after a Hotkey, to invoke something
	/// </summary>
	public class Chord : IEquatable<Chord>
	{
		public Keystroke Keystroke { get; }
		public int Id { get; private set; }
		public string Name { get; set; }
		public bool IsRegistered { get; private set; }
		public InvokeTarget InvokeTarget { get;  }
		/// <summary>
		/// Constructs a new instance of a Chord
		/// </summary>
		/// <param name="keystroke">The keystroke to invoke the chord</param>
		public Chord(string name, Keystroke keystroke, InvokeTarget invokeTarget)
		{
			Id = HotkeyId.GetNextId();
			IsRegistered = false;
			Name = name;
			Keystroke = keystroke;
			InvokeTarget = invokeTarget;
		}
		/// <summary>
		/// Invokes the chord
		/// </summary>
		/// <param name="promptKeyValues">Any responses to prompts required by this chord</param>
		public virtual Result<Process?, ProcErrorCode> GetProcess(string clipboard = "")
		{
			return InvokeTarget.GetProcess(clipboard);
		}
		public override string ToString()
		{
			return $"{Keystroke.ToString()}: {Name}";
		}
		public override bool Equals(object? obj)
		{
			return Equals(obj as Chord);
		}
		public bool Equals([AllowNull] Chord? other)
		{
			return other != null &&
				   Id == other.Id;
		}
		public override int GetHashCode()
		{
			return HashCode.Combine(Id);
		}
		public static bool operator ==(Chord? left, Chord? right)
		{
			return EqualityComparer<Chord>.Default.Equals(left, right);
		}
		public static bool operator !=(Chord? left, Chord? right)
		{
			return !(left == right);
		}
	}
}
