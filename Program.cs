using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace GFKConverter
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new GFKConvert());
                return 0;
            }
            else
            {
                // should be a single file
                // will support multiple files
                List<string > filesToProcess=new List<string>();
                foreach(string fname in args)
                {
                    string fullname = fname;
                    if (fname.Length < 13)
                        fullname = System.IO.Path.Combine(Properties.Settings.Default.GFKFileDirectory, fname);
                    filesToProcess.Add(fullname);

                }
                return GFKBatchConverter.Convertfiles(filesToProcess,false );

            }
        }
    }
}
