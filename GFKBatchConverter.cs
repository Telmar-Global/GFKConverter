using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net.NetworkInformation;

namespace GFKConverter
{
    public static class GFKBatchConverter
    {
        private static readonly string[] DateFormats = new string[]
        {
            "yyyy.mm.dd"
        };

        public static int Convertfiles(List<string> filestoProcess, bool useGUI)
        {
            try
            {
                List<string> gewFiles = new List<string>();
                List<string> pinasFiles = new List<string>();
                List<string> stamFiles = new List<string>();
                string gfkDirectory = Properties.Settings.Default.GFKFileDirectory;
                string gffDirectory = Properties.Settings.Default.GFFDirectory;
                foreach (string file in filestoProcess)
                {
                    string fileName = Path.GetFileName(file);
                    string fullPath = Path.Combine(gfkDirectory, fileName);
                    if (fileName.StartsWith("GEW", StringComparison.OrdinalIgnoreCase))
                        gewFiles.Add(fullPath);
                    else if (fileName.StartsWith("PINAS", StringComparison.OrdinalIgnoreCase))
                        pinasFiles.Add(fullPath);
                    else if (fileName.StartsWith("STAM", StringComparison.OrdinalIgnoreCase))
                        stamFiles.Add(fullPath);
                }

                if (gewFiles.Count == 0 && pinasFiles.Count == 0 && stamFiles.Count == 0)
                {
                    string msg = "No GEW, PINAS or STAM file found in the files to process.";
                    if (useGUI)
                        MessageBox.Show(msg);
                    else
                        Console.WriteLine(msg);
                    return -1;
                }

                if (gewFiles.Count > 0)
                    ConvertGFKWeights(gewFiles, gffDirectory);
                if (pinasFiles.Count > 0)
                    ConvertGFKViewing(pinasFiles, gffDirectory);
                if (stamFiles.Count > 0)
                    ConvertGFKDemos(stamFiles, gffDirectory);

                if (useGUI)
                    MessageBox.Show("Processing complete");
                else
                    Console.WriteLine("Processing complete");
            }
            catch (Exception ex)
            {
                if (useGUI)
                    MessageBox.Show("An unexpected error occurred: " + ex.ToString());
                else
                    Console.WriteLine("An unexpected error occurred: " + ex.ToString());
                return -2;
            }

            return 0;
        }

        private static void ConvertGFKWeights(List<string> gewFiles, string gffDirectory)
        {
            string outputPath = Path.Combine(gffDirectory, "WEIGHT.DAT");
            using (StreamWriter sw = new StreamWriter(outputPath))
            {
                foreach (string gewFile in gewFiles)
                {
                    string[] lines = File.ReadAllLines(gewFile);
                    for (int i = 1; i < lines.Length; i++)
                    {
                        string line = lines[i].Trim();
                        if (line.Length == 0)
                            continue;

                        string[] fields = line.Split(';');
                        if (fields.Length < 3)
                            continue;

                        DateTime date = ParseDate(fields[0].Trim());
                        string person = fields[1].Trim();
                        person = person.Replace("\"", "").PadLeft(8, '0');
                        string weight = fields[2].Trim();
                        weight = weight.Replace("\"", "").PadLeft(10, '0');
                        sw.WriteLine(date.ToString("yyyyMMdd") + person + "00100000" + weight);
                    }
                }
            }
        }

        private static void ConvertGFKViewing(List<string> pinasFiles, string gffDirectory)
        {
            string outputPath = Path.Combine(gffDirectory, "VIEWING.DAT");
            using (StreamWriter sw = new StreamWriter(outputPath))
            {
                foreach (string pinasFile in pinasFiles)
                {
                    string[] lines = File.ReadAllLines(pinasFile);
                    for (int i = 1; i < lines.Length; i++)
                    {
                        string line = lines[i].Trim();
                        if (line.Length == 0)
                            continue;

                        string[] fields = line.Split(';');
                        if (fields.Length < 1)
                            continue;

                        DateTime date = ParseDate(fields[1].Trim());
                        string HHID = fields[4].Trim().Replace("\"", "").PadLeft(8, '0');
                        string station = fields[10].Trim().Replace("\"", "").PadLeft(4, '0');
                        string starttime = fields[11].Trim().Replace("\"", "").PadLeft(8, '0');
                        starttime = starttime.Replace(":", "");
                        string duration = fields[12].Trim().Replace("\"", "").PadLeft(5, '0');
                        sw.WriteLine(date.ToString("yyyyMMdd" + HHID + "001" + station + starttime + duration + "0000"));
                    }
                }
            }
        }

        private class DemoControlInfo
        {
            public string Title;
            public int FlagPosition;
            public int FlagPadding;
        }

        private static void ConvertGFKDemos(List<string> stamFiles, string gffDirectory)
        {
            HashSet<DemoControlInfo> DemoControl = ReadDemoControl();
            string outputPath = Path.Combine(gffDirectory, "DEMO.DAT");
            using (StreamWriter sw = new StreamWriter(outputPath))
            {
                foreach (string stamFile in stamFiles)
                {
                    DateTime fileDate = ParseJulianFileDate(stamFile);
                    string[] lines = File.ReadAllLines(stamFile);
                    if (lines.Length == 0)
                        continue;

                    List<string> DemoHeadings = new List<string>();
                    string[] headingFields = lines[0].Split(';');
                    foreach (string heading in headingFields)
                        DemoHeadings.Add(heading.Trim().Replace("\"", ""));

                    for (int i = 1; i < lines.Length; i++)
                    {
                        string line = lines[i].Trim();
                        if (line.Length == 0)
                            continue;

                        string[] fields = line.Split(';');
                        if (fields.Length < 1)
                            continue;

                        string firstItem = fields[0].Trim().Replace("\"", "").PadLeft(8, '0');
                        StringBuilder outputLine = new StringBuilder(fileDate.ToString("yyyyMMdd") + firstItem + "001");
                        for (int f = 1; f < fields.Length; f++)
                        {
                            string heading = f < DemoHeadings.Count ? DemoHeadings[f] : "";
                            if (heading.Length > 31)
                                heading = heading.Substring(0, 31);
                            heading = heading.Trim();

                            DemoControlInfo demoInfo = FindDemoControl(DemoControl, heading);
                            if (demoInfo == null)
                                continue;

                            string item = fields[f].Trim().Replace("\"", "");
                            PlaceFlag(outputLine, demoInfo.FlagPosition, demoInfo.FlagPadding, item);
                        }

                        sw.WriteLine(outputLine.ToString());
                    }
                }
            }
        }

        private static HashSet<DemoControlInfo> ReadDemoControl()
        {
            HashSet<DemoControlInfo> demoControl = new HashSet<DemoControlInfo>();
            string path = Path.Combine(Properties.Settings.Default.MapFiles, "DEMOCONTROL.DAT");
            if (!File.Exists(path))
                throw new FileNotFoundException("DEMOCONTROL.DAT file not found in directory " + Properties.Settings.Default.MapFiles);

            string[] lines = File.ReadAllLines(path);
            foreach (string strline in lines)
            {
                if (strline.Length < 46)
                    continue;

                DemoControlInfo info = new DemoControlInfo();
                info.Title = strline.Substring(8, 31).Trim();
                info.FlagPosition = int.Parse(strline.Substring(40, 4).Trim());
                info.FlagPadding = int.Parse(strline.Substring(44, 2).Trim());
                if (info.Title.Length > 0)
                    demoControl.Add(info);
            }
            return demoControl;
        }

        private static DemoControlInfo FindDemoControl(HashSet<DemoControlInfo> demoControl, string title)
        {
            foreach (DemoControlInfo info in demoControl)
            {
                if (string.Equals(info.Title, title, StringComparison.OrdinalIgnoreCase))
                    return info;
            }
            return null;
        }

        private static void PlaceFlag(StringBuilder sb, int flagPosition, int flagPadding, string value)
        {
            int start = flagPosition - 1;
            if (start < 0)
                start = 0;
            string padded = value.PadLeft(flagPadding, '0');
            if (padded.Length > flagPadding)
                padded = padded.Substring(padded.Length - flagPadding);

            while (sb.Length < start + flagPadding)
                sb.Append('0');
            for (int i = 0; i < flagPadding; i++)
                sb[start + i] = padded[i];
        }

        private static DateTime ParseJulianFileDate(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName).TrimStart('.');
            if (nameWithoutExt.Length >= 3)
            {
                int julianDay;
                int year;
                if (int.TryParse(nameWithoutExt.Substring(nameWithoutExt.Length - 3), out julianDay)
                    && int.TryParse(extension, out year))
                {
                    if (year < 100)
                        year += (year < 50) ? 2000 : 1900;
                    DateTime fileDate = new DateTime(year, 1, 1).AddDays(julianDay - 1);
                    if (fileDate.Year == year)
                        return fileDate;
                }
            }
            throw new FormatException("Invalid Julian date in file name: " + fileName);
        }

        private static DateTime ParseDate(string value)
        {
            DateTime date;
            value = value.Trim().Trim('"').Trim();
            if (DateTime.TryParseExact(value, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                return date;
            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                return date;
            if (DateTime.TryParse(value, new CultureInfo("nl-NL"), DateTimeStyles.None, out date))
                return date;
            throw new FormatException("Invalid date: " + value);
        }
    }
}
