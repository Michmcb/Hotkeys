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
        public void SetQuestion(string question)
        {
            uxMessage.Text = question;
            uxInput.Text = "";
        }
        public string GetAnswer()
        {
            return uxInput.Text;
        }
    }
}
