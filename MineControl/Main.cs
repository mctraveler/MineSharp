using System;
using System.Windows.Forms;
using System.Threading;
using MineProxy;
using System.IO;

namespace MineControl
{
    public class MainClass
    {
        public static void Main()
        {
            Directory.SetCurrentDirectory(MineSharp.Settings.BaseWorldsPath);
            Application.SetCompatibleTextRenderingDefault(true);
			
            using (ControlWindow window = new ControlWindow())
            {
                Application.Run(window);
            }
            ServerCommander.Shutdown();
            Application.Exit();
        }
		
    }
}

