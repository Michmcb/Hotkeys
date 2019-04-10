using System.Windows.Forms;

namespace Hotkeys
{
    internal class Program
    {
        [System.STAThread]
        private static int Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (HotkeyMessageProcessor mp = new HotkeyMessageProcessor((args.Length == 1) ? args[0] : "hotkeys.xml", true))
            {
                Application.Run(mp);
            }

            return 0;
        }
    }
}
