using System;
using System.Collections.Generic;
using System.Globalization;
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
                DateTime date;
                if (!DateTime.TryParseExact(args[0], "yyyyMMdd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out date))
                {
                    Console.WriteLine("Invalid date: " + args[0] + ". Expected YYYYMMDD.");
                    return -1;
                }

                List<string> filesToProcess;
                if (!GFKConvert.TryGetFilesToProcessForDate(Properties.Settings.Default.GFKFileDirectory, date, out filesToProcess))
                {
                    Console.WriteLine("Date " + args[0] + " was not found in the available dates.");
                    return -1;
                }

                return GFKBatchConverter.Convertfiles(filesToProcess, false);
            }
        }
    }
}
