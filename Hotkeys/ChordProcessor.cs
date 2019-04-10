using System.Windows.Forms;

namespace Hotkeys
{
	public partial class ChordProcessor : Form
	{
		private Hotkey _hotkey;
		public Hotkey ForHotkey { get => _hotkey; set { _hotkey = value; label1.Text = $"{_hotkey?.Name ?? ""}..."; } }
		public Chord Chord { get; private set; }

		public event ChordHitHandler ChordHit;
		public delegate void ChordHitHandler(Chord ch);

		public ChordProcessor()
		{
			InitializeComponent();
		}
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			// First, we have to figure out the modifiers. Then we have to find the correct Hotkey, based on the modifier and virtual key hit.
			// Once we know that, we can return the hotkey that was hit.
			if (keyData != Keys.Escape)
			{
				Keys withoutMods = keyData & ~Keys.Modifiers;
				Keystroke hkm = new Keystroke(keyData);
				if (_hotkey.Chords.TryGetValue(hkm, out Chord ch))
				{
					Chord = ch;
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
				return true;
			}
		}
	}
}
