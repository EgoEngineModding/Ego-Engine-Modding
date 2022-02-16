using System;
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
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1(Args));
        }
    }
}
