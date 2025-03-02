using System;
using System.Windows.Forms;

namespace WaveformDisplay
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Ensure the application is run with the proper UI thread model
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Start the main form
            Application.Run(new MainForm());
        }
    }
}
