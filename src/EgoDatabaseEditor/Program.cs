using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EgoDatabaseEditor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] Args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(Args));
        }
    }
}
