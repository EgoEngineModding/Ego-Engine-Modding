using System;
using System.Windows.Forms;

namespace EgoCtfEditor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            System.Collections.Generic.List<string> a = new System.Collections.Generic.List<string>(args);
            if (args.Length > 0)
            {
                using (GameSelect gs = new GameSelect())
                {
                    if (gs.ShowDialog() == DialogResult.OK)
                    {
                        a.Add(gs.GameID.ToString());
                    }
                    else
                    {
                        a.Add((-1).ToString());
                    }
                }
            }
            Application.Run(new Form1(a.ToArray()));
        }
    }
}
