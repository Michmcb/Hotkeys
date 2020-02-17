using System.Diagnostics;
using System.Text;

namespace Hotkeys.Hk
{
	public class InvokeTarget
	{
		private readonly bool hasClipboard;
		public InvokeTarget(string path, string? args, string? dir, bool shell)
		{
			Path = path;
			Args = args;
			Dir = dir;
			Shell = shell;
			hasClipboard = args?.Contains("{clipboard}") ?? false;
		}
		public string Path { get; }
		public string? Args { get; }
		public string? Dir { get; }
		public bool Shell { get; }
		/// <summary>
		/// Returns a process that can be started, which is synonymous with invoking the trigger
		/// </summary>
		/// <param name="promptKeyValues">Any responses to prompts required by this chord</param>
		public virtual Result<Process?, ProcErrorCode> GetProcess(string clipboard = "")
		{
			if (!System.IO.File.Exists(Path))
			{
				return new Result<Process?, ProcErrorCode>(null, ProcErrorCode.FileNotFound);
			}
			StringBuilder args;
			if (hasClipboard)
			{
				args = new StringBuilder(Args?.Replace("{clipboard}", clipboard));
			}
			else
			{
				args = new StringBuilder(Args);
			}
			ProcessStartInfo info;
			string argsToUse = args.ToString();
			if (!string.IsNullOrEmpty(argsToUse))
			{
				info = new ProcessStartInfo(Path, argsToUse);
			}
			else
			{
				info = new ProcessStartInfo(Path);
			}
			if (!(info.UseShellExecute = Shell) && Dir != null)
			{
				info.WorkingDirectory = Dir;
			}

			Process p = new Process
			{
				StartInfo = info
			};
			return new Result<Process?, ProcErrorCode>(p, ProcErrorCode.Ok);
		}
	}
}
