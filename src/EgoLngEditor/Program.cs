using System;
using System.Windows.Forms;

namespace EgoLngEditor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1(args));
        }
    }
}
