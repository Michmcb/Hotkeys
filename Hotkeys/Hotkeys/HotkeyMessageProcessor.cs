using Hotkeys.Hk;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Hotkeys
{
	public class HotkeyMessageProcessor : Form
	{
		private const int WM_HOTKEY_MSG = 0x0312;
		private readonly NotifyIcon notifyIcon;
		private readonly ToolStripMenuItem mLoad;
		private readonly ToolStripMenuItem mUnload;
		private readonly ToolStripMenuItem mReload;
		private readonly ToolStripMenuItem mRegister;
		private readonly ToolStripMenuItem mUnregister;
		private readonly ToolStripMenuItem mStatus;
		private readonly ToolStripMenuItem mExit;
		private readonly ChordProcessor chordProcessor;
		private readonly string hotkeyLoadFile;
		private bool chordFree;
		private IDictionary<int, Hotkey> loadedHotkeys;
		public bool CurrentlyRegistered { get;private set; }
		public HotkeyMessageProcessor(string hotkeyLoadFile, bool autoLoad)
		{
			InitializeComponent();
			chordFree = true;
			CurrentlyRegistered = false;
			notifyIcon = new NotifyIcon();
			chordProcessor = new ChordProcessor();
			chordProcessor.ChordHit += ChordHit;

			mLoad = new ToolStripMenuItem("Load and Register");
			mLoad.Click += new EventHandler(LoadAndRegister);
			mReload = new ToolStripMenuItem("Reload and Register");
			mReload.Click += new EventHandler(ReloadAndRegister);
			mUnload = new ToolStripMenuItem("Unload and Unregister");
			mUnload.Click += new EventHandler(UnloadAndUnregister);
			mRegister = new ToolStripMenuItem("Register");
			mRegister.Click += new EventHandler(Register);
			mUnregister = new ToolStripMenuItem("Unregister");
			mUnregister.Click += new EventHandler(Unregister);
			mStatus = new ToolStripMenuItem("Status");
			mStatus.Click += new EventHandler(ShowStatus);
			mExit = new ToolStripMenuItem("Exit");
			mExit.Click += new EventHandler(Exit);

			SetContextMenuState(false, false);

			notifyIcon.Icon = Properties.Resources.Icon;
			notifyIcon.ContextMenuStrip = new ContextMenuStrip();
			notifyIcon.ContextMenuStrip.Items.AddRange(new ToolStripMenuItem[] { mLoad, mReload, mUnload, mRegister, mUnregister, mStatus, mExit });
			notifyIcon.Visible = true;
			notifyIcon.Text = "Hotkeys";

			loadedHotkeys = new Dictionary<int, Hotkey>();
			this.hotkeyLoadFile = hotkeyLoadFile;
			if (autoLoad)
			{
				LoadAndRegister(this, EventArgs.Empty);
			}
		}
		private void SetContextMenuState(bool areHotkeysLoaded, bool areHotkeysRegistered)
		{
			if (areHotkeysLoaded)
			{
				mLoad.Enabled = false;
				mReload.Enabled = mUnload.Enabled = true;
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
				mReload.Enabled = mUnload.Enabled = mRegister.Enabled = mUnregister.Enabled = mUnload.Enabled = false;
			}
		}
		internal void LoadAndRegister(object? sender, EventArgs e)
		{
			loadedHotkeys = HotkeyLoader.Load(hotkeyLoadFile, Handle);
			SetContextMenuState(true, false);
			Register(sender, e);
		}
		internal void ReloadAndRegister(object? sender, EventArgs e)
		{
			UnloadAndUnregister(this, e);
			LoadAndRegister(this, e);
		}
		internal void UnloadAndUnregister(object? sender, EventArgs e)
		{
			Unregister(sender, e);
			loadedHotkeys.Clear();
			SetContextMenuState(false, false);
		}
		internal void Register(object? sender, EventArgs e)
		{
			bool allGood = true;
			foreach (Hotkey chord in loadedHotkeys.Values)
			{
				allGood &= chord.Register();
			}
			CurrentlyRegistered = true;
			if (!allGood)
			{
				notifyIcon.ShowBalloonTip(10000, "Status", "Not all hotkeys registered successfully; please check status!", ToolTipIcon.Error);
			}
			SetContextMenuState(true, true);
		}
		internal void Unregister(object? sender, EventArgs e)
		{
			foreach (Hotkey chord in loadedHotkeys.Values)
			{
				chord.Unregister();
			}
			SetContextMenuState(true, false);
		}
		internal void ShowStatus(object? sender, EventArgs e)
		{
			if (loadedHotkeys != null)
			{
				StringBuilder sb = new StringBuilder();
				foreach (Hotkey chord in loadedHotkeys.Values)
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
				if (loadedHotkeys.TryGetValue(m.WParam.ToInt32(), out Hotkey? hotkey))
				{
					if (hotkey.HasChords)
					{
						lock (chordProcessor)
						{
							if (chordFree)
							{
								chordFree = false;
								chordProcessor.ForHotkey = hotkey;
								chordProcessor.Show();
								chordProcessor.Activate();
							}
						}
					}
					else
					{
						Result<Process?, ProcErrorCode> result = hotkey.GetSingleProc(Clipboard.GetText() ?? "");
						if (result.Ok != null)
						{
							using Process p = result.Ok;
							p.Start();
						}
					}
				}
			}
			else
			{
				base.WndProc(ref m);
			}
		}
		private void ChordHit(Chord? ch)
		{
			chordProcessor.Hide();
			lock (chordProcessor)
			{
				chordFree = true;
			}
			if (ch != null)
			{
				Result<Process?, ProcErrorCode> result = ch.GetProcess(Clipboard.GetText() ?? "");
				if (result.Ok != null)
				{
					using Process p = result.Ok;
					p.Start();
				}
			}
		}
		protected override void SetVisibleCore(bool value)
		{
			// This stops the window from ever showing to the user. We only want this to be a tray application after all.
			base.SetVisibleCore(false);
		}
		private void InitializeComponent()
		{
			SuspendLayout();
			AutoScaleMode = AutoScaleMode.None;
			CausesValidation = false;
			ClientSize = new Size(0, 0);
			ControlBox = false;
			Enabled = false;
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "";
			ShowIcon = false;
			ShowInTaskbar = false;
			SizeGripStyle = SizeGripStyle.Hide;
			ResumeLayout(false);
		}
		internal void Exit(object? sender, EventArgs e)
		{
			// We must manually tidy up and remove the icon before we exit.
			// Otherwise it will be left behind until the user mouses over.
			// And more importantly, unregister all of our hotkeys
			foreach (Hotkey hk in loadedHotkeys.Values)
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
					notifyIcon.Visible = false;
					chordProcessor.Dispose();
				}

				// Invoking Unregister will change our context menu state so just do it here
				foreach (Hotkey chord in loadedHotkeys.Values)
				{
					chord.Unregister();
				}
				disposedValue = true;
			}
		}
		#endregion
	}
}
