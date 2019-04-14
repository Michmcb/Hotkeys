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
		private ConcurrentQueue<Tuple<Hotkey, string>> _hotkeys;
		private HotkeyMessageProcessor _hotkeyMsgProc;
		private bool _isWorking;

		public HotkeyExecutor(HotkeyMessageProcessor hmp)
		{
			_hotkeys = new ConcurrentQueue<Tuple<Hotkey, string>>();
			_proc = new ManualResetEventSlim(false, 0);
			_isWorking = false;
			_hotkeyMsgProc = hmp;
		}
		/// <summary>
		/// Invokes the hotkey. If the hotkey has any chords, user will be asked for clarification.
		/// </summary>
		/// <param name="hotkey"></param>
		public void InvokeHotkey(Hotkey hotkey, string clipboard)
		{
			if (_isWorking)
			{
				_hotkeys.Enqueue(Tuple.Create(hotkey, clipboard));
				_proc.Set();
			}
		}
		private void Proc()
		{
			bool go = true;
			while (go)
			{
				_proc.Reset();
				if (_hotkeys.TryDequeue(out Tuple<Hotkey, string> hk))
				{
					hk.Item1.Proc(this, hk.Item2);
				}
				_proc.Wait();
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
			if (_hotkeyMsgProc.CanAskForChord)
			{
				WaitToken<Chord> x = _hotkeyMsgProc.AskForChord(hk);
				x.Wait();
				return x.Result;
			}
			return null;
		}
		#endregion
	}
}
