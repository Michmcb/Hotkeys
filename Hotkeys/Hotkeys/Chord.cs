using System;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;

namespace Hotkeys
{
	/// <summary>
	/// Represents a second keystroke that can come after a Hotkey, to invoke something
	/// </summary>
	public class Chord
	{
		private bool _hasClipboard;
		private string _args;

		public Keystroke Keystroke { get; }
		public int Id { get; private set; }
		public string Name { get; set; }
		public bool IsRegistered { get; private set; }
		public bool Shell { get; set; }
		public string Exec { get; set; }
		public Prompt[] Prompts { get; set; }
		public bool NeedsPrompt => Prompts?.Length > 0;
		public string Args
		{
			get => _args;
			set
			{
				_args = value; _hasClipboard = value?.Contains("{clipboard}") ?? false;
			}
		}
		/// <summary>
		/// Constructs a new instance of a Chord
		/// </summary>
		/// <param name="keystroke">The keystroke to invoke the chord</param>
		public Chord(Keystroke keystroke)
		{
			Id = HotkeyId.GetNextId();
			IsRegistered = false;
			Keystroke = keystroke;
		}
		/// <summary>
		/// Invokes the chord
		/// </summary>
		/// <param name="promptKeyValues">Any responses to prompts required by this chord</param>
		public virtual Error.Proc Proc(string clipboard, params PromptResponse[] promptKeyValues)
		{
			if (!System.IO.File.Exists(Exec))
			{
				return Error.Proc.FileNotFound;
			}
			StringBuilder args;
			if (_hasClipboard)
			{
				args = new StringBuilder(Args?.Replace("{clipboard}", clipboard));
			}
			else
			{
				args = new StringBuilder(Args);
			}
			if (promptKeyValues != null)
			{
				foreach (PromptResponse pr in promptKeyValues)
				{
					args.Replace(pr.Key, pr.Response);
				}
			}
			ProcessStartInfo info;
			string argsToUse = args.ToString();
			if (!string.IsNullOrEmpty(argsToUse))
			{
				info = new ProcessStartInfo(Exec, argsToUse);
			}
			else
			{
				info = new ProcessStartInfo(Exec);
			}
			info.UseShellExecute = Shell;
			Process.Start(info).Dispose();
			return Error.Proc.Ok;
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
