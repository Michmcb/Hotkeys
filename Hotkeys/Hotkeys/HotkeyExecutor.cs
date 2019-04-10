using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Hotkeys
{
	/// <summary>
	/// Invokes hotkeys and provides them with a mechanism to query for a second keystroke, to clarify which chord should be invoked.
	/// </summary>
	public class HotkeyExecutor : IDisposable
	{
		private ManualResetEventSlim _proc;
		private ConcurrentQueue<Hotkey> _hotkeys;
		private HotkeyMessageProcessor _hotkeyMsgProc;
		private bool _isWorking;

		public HotkeyExecutor(HotkeyMessageProcessor hmp)
		{
			_hotkeys = new ConcurrentQueue<Hotkey>();
			_proc = new ManualResetEventSlim(false, 0);
			_isWorking = false;
			_hotkeyMsgProc = hmp;
		}
		/// <summary>
		/// Invokes the hotkey. If the hotkey has any chords, user will be asked for clarification.
		/// </summary>
		/// <param name="hotkey"></param>
		public void InvokeHotkey(Hotkey hotkey)
		{
			if (_isWorking)
			{
				_hotkeys.Enqueue(hotkey);
				_proc.Set();
			}
		}
		private void Proc()
		{
			bool go = true;
			while (go)
			{
				_proc.Wait();
				_proc.Reset();
				if (_hotkeys.TryDequeue(out Hotkey hk))
				{
					hk.Proc(this);
				}
				go = _isWorking;
			}
		}
		/// <summary>
		/// Starts worker thread to allow hotkey invocations
		/// </summary>
		public void Start()
		{
			_isWorking = true;
			new Thread(Proc).Start();
		}
		/// <summary>
		/// Stops worker thread; automatically called on disposal
		/// </summary>
		public void Stop()
		{
			_isWorking = false;
			_proc.Set();
		}
		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls
		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					Stop();
					_proc.Dispose();
				}
				disposedValue = true;
			}
		}
		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
		}
		public Chord GetChord(Hotkey hk)
		{
			WaitToken<Chord> x = _hotkeyMsgProc.AskForChord(hk);
			x.Wait();
			return x.Result;
		}
		#endregion
	}
}
