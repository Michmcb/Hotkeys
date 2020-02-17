using System;
using System.Windows.Forms;
using Hotkeys.Hk;

namespace Hotkeys
{
	public partial class ChordProcessor : Form
	{
		private Hotkey? _hotkey;
		public event ChordHitHandler ChordHit;
		public delegate void ChordHitHandler(Chord? ch);
		public ChordProcessor()
		{
			InitializeComponent();
			Activate();
		}
		public Hotkey ForHotkey
		{
			get => _hotkey ?? throw new InvalidOperationException("");
			set
			{
				_hotkey = value;
				uxChords.Text = $"{_hotkey?.Name ?? ""}...{Environment.NewLine}{string.Join(Environment.NewLine, value.Chords.Values)}";
			}
		}
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			// First, we have to figure out the modifiers. Then we have to find the correct Hotkey, based on the modifier and virtual key hit.
			// Once we know that, we can return the hotkey that was hit.
			if (keyData != Keys.Escape)
			{
				DialogResult = DialogResult.OK;
				Keys withoutMods = keyData & ~Keys.Modifiers;
				Keystroke hkm = new Keystroke(keyData);
				if (ForHotkey.Chords.TryGetValue(hkm, out Chord? ch))
				{
					ChordHit?.Invoke(ch);
					return true;
				}
				else
				{
					return base.ProcessCmdKey(ref msg, keyData);
				}
			}
			else
			{
				ChordHit?.Invoke(null);
				DialogResult = DialogResult.Cancel;
				return true;
			}
		}
		private void GrabFocus(object sender, EventArgs e)
		{
			// Try and grab focus back; that way, every keystroke the user does
			// to try and invoke a chord will get picked up by this. Otherwise, they'd have to click
			// this to bring focus back, and then press their chord.
			Activate();
		}
		private void AskForFocus(object sender, EventArgs e)
		{
			Text = "Press a Chord (Focus lost, please click me)";
		}
		private void ResetTitle(object sender, EventArgs e)
		{
			Text = "Press a Chord";
		}
	}
}
