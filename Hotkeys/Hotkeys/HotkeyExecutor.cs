using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace Hotkeys
{
	/// <summary>
	/// Invokes hotkeys and provides them with a mechanism to query for a second keystroke, to clarify which chord should be invoked.
	/// </summary>
	public class HotkeyExecutor : IDisposable
	{
		private readonly ManualResetEventSlim _interceptorProc;
		private readonly ManualResetEventSlim _hotkeyResolverProc;
		private readonly ManualResetEventSlim _processInvokerProc;
		private readonly ConcurrentQueue<Tuple<Hotkey, string>> _intercepted;
		private readonly ConcurrentQueue<Tuple<Hotkey, string>> _toResolve;
		private readonly ConcurrentQueue<Process> _toExecute;
		private bool _isWorking;
		private readonly Thread _interceptor;
		private readonly Thread _hotkeyResolver;
		private readonly Thread _processExecutor;
		private readonly WaitToken<Chord> _chordHit;
		private readonly HotkeyMessageProcessor _hmp;

		public HotkeyExecutor(HotkeyMessageProcessor hmp)
		{
			_intercepted = new ConcurrentQueue<Tuple<Hotkey, string>>();
			_toResolve = new ConcurrentQueue<Tuple<Hotkey, string>>();
			_toExecute = new ConcurrentQueue<Process>();
			_interceptorProc = new ManualResetEventSlim(false, 0);
			_hotkeyResolverProc = new ManualResetEventSlim(false, 0);
			_processInvokerProc = new ManualResetEventSlim(false, 0);
			_isWorking = false;
			_interceptor = new Thread(Intercept);
			_interceptor.Name = "Interceptor";
			_hotkeyResolver = new Thread(Resolve);
			_hotkeyResolver.Name = "Resolver";
			_processExecutor = new Thread(Execute);
			_processExecutor.Name = "Executor";
			_hmp = hmp;

			_chordHit = new WaitToken<Chord>(false, 0);

		}
		/// <summary>
		/// Prompts the user to press a chord, and blocks until the user has either pressed a valid chord or cancels
		/// </summary>
		public Chord GetChord(Hotkey hk)
		{
			ChordProcessor cp = _hmp.GetNewChordProcessor();
			cp.ChordHit += ChordInvoked;
			cp.ForHotkey = hk;
			_chordHit.Reset();
			// We have to pass this onto the HotkeyMessageProcessor, because otherwise it'll block this thread and the dialogue will get boned
			_hmp.Invoke(new Action(() =>
			{
				cp.Show();
			}));
			_chordHit.Wait();
			_hmp.DisposeChordProcessor(cp);
			//cp.Hide();
			return _chordHit.Result;
		}
		private void ChordInvoked(Chord ch)
		{
			_chordHit.Result = ch;
			_chordHit.Set();
		}
		/// <summary>
		/// Invokes the hotkey. If the hotkey has any chords, user will be asked for clarification.
		/// </summary>
		/// <param name="hotkey"></param>
		public void InvokeHotkey(Hotkey hotkey, string clipboard)
		{
			if (_isWorking)
			{
				_intercepted.Enqueue(Tuple.Create(hotkey, clipboard));
				_interceptorProc.Set();
			}
		}
		private void Intercept()
		{
			bool go = true;
			while (go)
			{
				_interceptorProc.Reset();
				if (_intercepted.TryDequeue(out Tuple<Hotkey, string> hk))
				{
					if (hk.Item1.RequiresResolution)
					{
						// If it does, we need to give it to the resolver to handle; that way the user only sees one Chord window or Prompt at a time.
						_toResolve.Enqueue(hk);
						_hotkeyResolverProc.Set();
					}
					else
					{
						// If it doesn't need any resolution (like chords or prompts), it can jump the queue and go straight to the Executor
						// This has the side effect that when a prompt or chord window is up, the user can keep invoking other hotkeys
						Result<Process, Error.Proc> p = hk.Item1.GetProc(this, hk.Item2);
						if (p.Err == Error.Proc.Ok && p.Ok != null)
						{
							_toExecute.Enqueue(p.Ok);
							_processInvokerProc.Set();
						}
					}
				}
				_interceptorProc.Wait();
				go = _isWorking;
			}
		}
		private void Resolve()
		{
			bool go = true;
			while (go)
			{
				_hotkeyResolverProc.Reset();
				if (_toResolve.TryDequeue(out Tuple<Hotkey, string> hk))
				{
					// This will block for user input, be it a prompt or a chord
					Result<Process, Error.Proc> p = hk.Item1.GetProc(this, hk.Item2);
					if (p.Err == Error.Proc.Ok && p.Ok != null)
					{
						_toExecute.Enqueue(p.Ok);
						_processInvokerProc.Set();
					}
				}
				_hotkeyResolverProc.Wait();
				go = _isWorking;
			}
		}
		private void Execute()
		{
			bool go = true;
			while (go)
			{
				_processInvokerProc.Reset();
				if (_toExecute.TryDequeue(out Process p))
				{
					using (p)
					{
						p.Start();
					}
				}
				_processInvokerProc.Wait();
				go = _isWorking;
			}
		}
		/// <summary>
		/// Starts to allow hotkey invocations
		/// </summary>
		public void Start()
		{
			_isWorking = true;
			_interceptor.Start();
			_hotkeyResolver.Start();
			_processExecutor.Start();
		}
		/// <summary>
		/// Automatically called on disposal
		/// </summary>
		public void Stop()
		{
			_isWorking = false;
			_interceptorProc.Set();
			_hotkeyResolverProc.Set();
			_processInvokerProc.Set();
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
					_interceptorProc.Dispose();
					_hotkeyResolverProc.Dispose();
					_processInvokerProc.Dispose();
					_chordHit.Dispose();
					//_cp.Dispose();
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
		#endregion
	}
}
