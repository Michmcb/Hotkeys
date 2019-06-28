using System.Windows.Forms;

namespace Hotkeys
{
	public partial class ValuePrompt : Form
	{
		public ValuePrompt()
		{
			InitializeComponent();
			uxOk.DialogResult = DialogResult.OK;
			uxCancel.DialogResult = DialogResult.Cancel;
			AcceptButton = uxOk;
			CancelButton = uxCancel;
		}
		public string Question { get => uxMessage.Text; set { uxMessage.Text = value; uxInput.Text = ""; } }
		public string Answer { get => uxInput.Text; private set => uxInput.Text = value; }
	}
}
