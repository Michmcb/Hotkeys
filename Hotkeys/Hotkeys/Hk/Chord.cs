namespace Hotkeys.Hk
{
	using System;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;

	/// <summary>
	/// Represents a second keystroke that can come after a Hotkey, to invoke something
	/// </summary>
	public sealed class Chord : IEquatable<Chord>
	{
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
		public Keystroke Keystroke { get; }
		public int Id { get; }
		public string Name { get; }
		public bool IsRegistered { get; private set; }
		public InvokeTarget InvokeTarget { get; }
		/// <summary>
		/// Invokes the chord
		/// </summary>
		public Result<Process?, ProcErrorCode> GetProcess(string clipboard = "")
		{
			return InvokeTarget.GetProcess(clipboard);
		}
		public override string ToString()
		{
			return string.Concat(Keystroke.ToString(), ": ", Name);
		}
		public override bool Equals(object? obj)
		{
			return Equals(obj as Chord);
		}
		public bool Equals([AllowNull] Chord? other)
		{
			return other != null && Id == other.Id;
		}
		public override int GetHashCode()
		{
			return HashCode.Combine(Id);
		}
		public static bool operator ==(Chord? left, Chord? right) => left?.Id == right?.Id;
		public static bool operator !=(Chord? left, Chord? right) => left?.Id != right?.Id;
	}
}
