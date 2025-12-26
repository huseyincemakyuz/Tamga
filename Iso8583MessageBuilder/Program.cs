using System;
using System.Windows.Forms;
using Iso8583MessageBuilder.Forms;

namespace Iso8583MessageBuilder
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}