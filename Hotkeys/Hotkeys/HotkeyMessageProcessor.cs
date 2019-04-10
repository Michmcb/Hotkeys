using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Hotkeys
{
	public class HotkeyMessageProcessor : Form, IDisposable
	{
		private const int WM_HOTKEY_MSG = 0x0312;
		private NotifyIcon notifyIcon;
		private MenuItem mLoad;
		private MenuItem mUnload;
		private MenuItem mReload;
		private MenuItem mRegister;
		private MenuItem mUnregister;
		private MenuItem mStatus;
		private MenuItem mExit;

		private IDictionary<int, Hotkey> _hotkeys;
		private readonly string loadPath;
		private readonly HotkeyExecutor exec;
		private HotkeyLoader loader;
		private WaitToken<Chord> chordSecondKey;
		private ChordProcessor cp;
		private readonly object chordLock = new object();

		public HotkeyMessageProcessor(string hotkeyLoadFile, bool autoLoad)
		{
			InitializeComponent();
			loadPath = hotkeyLoadFile;
			chordSecondKey = null;
			notifyIcon = new NotifyIcon();
			mLoad = new MenuItem("Load and Register", new EventHandler(LoadAndRegister));
			mReload = new MenuItem("Reload and Register", new EventHandler(ReloadAndRegister));
			mUnload = new MenuItem("Unload and Unregister", new EventHandler(UnloadAndUnregister));
			mRegister = new MenuItem("Register", new EventHandler(Register));
			mUnregister = new MenuItem("Unregister", new EventHandler(Unregister));
			mStatus = new MenuItem("Status", new EventHandler(ShowStatus));
			mExit = new MenuItem("Exit", new EventHandler(Exit));

			exec = new HotkeyExecutor(this);
			exec.Start();
			cp = new ChordProcessor();
			cp.ChordHit += ChordInvoked;

			SetContextMenuState(false, false);

			notifyIcon.Icon = Properties.Resources.Icon;
			notifyIcon.ContextMenu = new ContextMenu(new MenuItem[] { mLoad, mReload, mUnload, mRegister, mUnregister, mStatus, mExit });
			notifyIcon.Visible = true;
			notifyIcon.Text = "Hotkeys";

			_hotkeys = null;
			loader = new HotkeyLoader(loadPath);
			if (autoLoad)
			{
				LoadAndRegister(this, null);
			}
		}
		public WaitToken<Chord> AskForChord(Hotkey hotkey)
		{
			lock (chordLock)
			{
				chordSecondKey?.Wait();
				chordSecondKey?.Dispose();
				chordSecondKey = new WaitToken<Chord>(false);
				Invoke(new Action(() => AskForChordInternal(hotkey)));
				return chordSecondKey;
			}
		}
		private void AskForChordInternal(Hotkey hk)
		{
			cp.ForHotkey = hk;
			cp.Show();
		}
		private void ChordInvoked(Chord ch)
		{
			cp.Hide();
			chordSecondKey.Result = ch;
			chordSecondKey.Set();
		}
		private void SetContextMenuState(bool areHotkeysLoaded, bool areHotkeysRegistered)
		{
			if (areHotkeysLoaded)
			{
				mLoad.Enabled = false;
				mUnload.Enabled = true;
				if (areHotkeysRegistered)
				{
					mRegister.Enabled = false;
					mUnregister.Enabled = true;
				}
				else
				{
					mRegister.Enabled = true;
					mUnregister.Enabled = false;
				}
			}
			else
			{
				mLoad.Enabled = true;
				mUnload.Enabled = mRegister.Enabled = mUnregister.Enabled = mUnload.Enabled = false;
			}
		}
		internal void LoadAndRegister(object sender, EventArgs e)
		{
			loader.Load(Handle);
			_hotkeys = loader.LoadedHotkeys;
			SetContextMenuState(true, false);
			Register(sender, e);
		}
		internal void ReloadAndRegister(object sender, EventArgs e)
		{
			UnloadAndUnregister(this, e);
			LoadAndRegister(this, e);
		}
		internal void UnloadAndUnregister(object sender, EventArgs e)
		{
			Unregister(sender, e);
			_hotkeys.Clear();
			_hotkeys = null;
			SetContextMenuState(false, false);
		}
		internal void Register(object sender, EventArgs e)
		{
			bool allGood = true;
			foreach (Hotkey chord in _hotkeys.Values)
			{
				allGood &= chord.Register();
			}
			if (!allGood)
			{
				notifyIcon.ShowBalloonTip(10000, "Status", "Not all hotkeys registered successfully; please check status!", ToolTipIcon.Error);
			}
			SetContextMenuState(true, true);
		}
		internal void Unregister(object sender, EventArgs e)
		{
			foreach (Hotkey chord in _hotkeys.Values)
			{
				chord.Unregister();
			}
			SetContextMenuState(true, false);
		}
		internal void ShowStatus(object sender, EventArgs e)
		{
			if (_hotkeys != null)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();
				foreach (Hotkey chord in _hotkeys.Values)
				{
					sb.AppendLine(chord.ToString() + ". Currently " + (chord.IsRegistered ? "Active" : "Inactive"));
				}
				MessageBox.Show(sb.ToString(), "Hotkey Status", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
			}
			else
			{
				MessageBox.Show("No hotkeys are currently loaded.", "Hotkey Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}
		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WM_HOTKEY_MSG)
			{
				if (_hotkeys.TryGetValue(m.WParam.ToInt32(), out Hotkey hotkey))
				{
					exec.InvokeHotkey(hotkey, Clipboard.GetText() ?? "");
				}
			}
			else
			{
				base.WndProc(ref m);
			}
		}
		protected override void SetVisibleCore(bool value)
		{
			// This stops the window from ever showing to the user. We only want this to be a tray application after all.
			base.SetVisibleCore(false);
		}
		private void InitializeComponent()
		{
			this.SuspendLayout();
			this.AutoScaleMode = AutoScaleMode.None;
			this.CausesValidation = false;
			this.ClientSize = new System.Drawing.Size(0, 0);
			this.ControlBox = false;
			this.Enabled = false;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = SizeGripStyle.Hide;
			this.ResumeLayout(false);
		}
		internal void Exit(object sender, EventArgs e)
		{
			// We must manually tidy up and remove the icon before we exit.
			// Otherwise it will be left behind until the user mouses over.
			// And more importantly, unregister all of our hotkeys
			foreach (Hotkey hk in _hotkeys.Values)
			{
				hk.Unregister();
			}
			notifyIcon.Visible = false;

			Application.Exit();
		}
		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (!disposedValue)
			{
				if (disposing)
				{
					mLoad.Dispose();
					mUnload.Dispose();
					mReload.Dispose();
					mRegister.Dispose();
					mUnregister.Dispose();
					mStatus.Dispose();
					mExit.Dispose();
					exec.Dispose();
					chordSecondKey?.Dispose();
				}

				UnloadAndUnregister(this, null);
				disposedValue = true;
			}
		}
		~HotkeyMessageProcessor()
		{
			Dispose(false);
		}
		void System.IDisposable.Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
