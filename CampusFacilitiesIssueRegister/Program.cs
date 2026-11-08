using System;
using System.IO;
using System.Windows.Forms;

namespace CampusFacilitiesIssueRegister
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Configure DataDirectory for LocalDB AttachDbFilename
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string appDataDir = Path.Combine(baseDir, "App_Data");
            if (!Directory.Exists(appDataDir))
            {
                Directory.CreateDirectory(appDataDir);
            }
            AppDomain.CurrentDomain.SetData("DataDirectory", baseDir);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
