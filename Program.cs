using System;
using System.Windows.Forms;

namespace BonsaiTreeGenerator
{
    /// <summary>
    /// Main entry point for the Realistic Procedural 2D Bonsai Tree Generator application
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new BonsaiTreeForm());
        }
    }
}