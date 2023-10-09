using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RoughGrep
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            var cmdline = Environment.GetCommandLineArgs();
            if (cmdline.Skip(1).FirstOrDefault() == "--install")
            {
                Logic.SetupShellIntegration();
                return;
            }

            Logic.InitApp();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var mf = new MainForm();
            Application.Run(mf);
        }
    }
}
