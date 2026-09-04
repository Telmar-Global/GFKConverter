using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

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
                    else if (fileName.StartsWith("STAM", StringComparison.OrdinalIgnoreCase)
                        && !fileName.StartsWith("STAMDEF", StringComparison.OrdinalIgnoreCase))
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

                List<string> domesticids = new List<string>();
                if (stamFiles.Count > 0)
                    domesticids = ConvertGFKDemos(stamFiles, gffDirectory);
                if (pinasFiles.Count > 0)
                    ConvertGFKViewing(pinasFiles, gffDirectory, domesticids);

                string processedDirectory = Properties.Settings.Default.GFKProcessedDirectory;
                if (!Directory.Exists(processedDirectory))
                    Directory.CreateDirectory(processedDirectory);

                string movePrefix = DateTime.Now.ToString("yyyyMMdd_HHmmss_");
                foreach (string file in filestoProcess)
                {
                    string fileName = Path.GetFileName(file);
                    string sourcePath = File.Exists(file) ? file : Path.Combine(gfkDirectory, fileName);
                    string destPath = Path.Combine(processedDirectory, movePrefix + fileName);
                    File.Move(sourcePath, destPath);
                }

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
            foreach (string gewFile in gewFiles)
            {
                DateTime fileDate = ParseJulianFileDate(gewFile);
                string outputPath = Path.Combine(gffDirectory, "W" + fileDate.ToString("yyyyMMdd") + ".DAT");
                using (StreamWriter sw = new StreamWriter(outputPath))
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

        private static void ConvertGFKViewing(List<string> pinasFiles, string gffDirectory, List<string> domesticids)
        {
            Dictionary<int, string> stationMap = ReadStationMap();
            foreach (string pinasFile in pinasFiles)
            {
                DateTime fileDate = ParseJulianFileDate(pinasFile);
                string outputPath = Path.Combine(gffDirectory, "V" + fileDate.ToString("yyyyMMdd") + ".DAT");
                using (StreamWriter sw = new StreamWriter(outputPath))
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

                        DateTime progdate = ParseDate(fields[1].Trim());
                        DateTime usagedate = ParseDate(fields[2].Trim());
                        bool hasplaybackdate = (fields[14].Trim() != "");

                        string HHID = fields[4].Trim().Replace("\"", "").PadLeft(8, '0');
                        string stationRaw = fields[10].Trim().Replace("\"", "");
                        string station = stationRaw.PadLeft(4, '0');
                        if (station.Length > 4)
                            station = station.Substring(station.Length - 4);
                        int stationKey;
                        string mappedStation;
                        if (int.TryParse(stationRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out stationKey)
                            && stationMap.TryGetValue(stationKey, out mappedStation))
                            station = mappedStation;
                        string starttime = fields[11].Trim().Replace("\"", "").PadLeft(8, '0');
                        starttime = starttime.Replace(":", "");
                        string duration = fields[12].Trim().Replace("\"", "").PadLeft(5, '0');
                        string outofhome = fields[19].Trim().Replace("\"", "");
                        //Now some logic about the flags
                        char[] viewflags = "0000000000000000".ToCharArray();
                        // Flag 1 - Normal viewing on Broadcast station
                        viewflags[0] = '1';
                        // Flag 2 - Set if Guest
                        if (outofhome != "0")
                        {
                            viewflags[1] = '1';
                        }
                        // Flag 3 - Set if Domestic worker
                        if (domesticids.Contains(HHID))
                        {
                            viewflags[2] = '1';
                        }
                        // Time shidted viewing is when the record has a playback date.
                        // Flag 4 - Set for normal viewing playbackdate = date
                        if (hasplaybackdate)
                        {
                            // Viewed on same day as aired
                            if (fields[1] == fields[2]) 
                            {
                                viewflags[3] = '1';
                            }
                            else
                            {
                                TimeSpan difference = usagedate - progdate;
                                double totalDays = difference.TotalDays;
                                // Flag 5 - Normal viewing + 1 = (( Playbackdate - Date ) <= 7 )
                                if (totalDays <= 7)
                                {
                                    viewflags[4] = '1';
                                }

                                // Flag 6 - Normal viewing + 8 = (( Playbackdate - Date ) <= 27 )
                                if ((totalDays > 7) && (totalDays <= 27))
                                {
                                    viewflags[5] = '1';
                                }
                            }
                        }
                            // Flag 13 - Set if non broadcast station
                            string flags = new string(viewflags);


                        sw.WriteLine(progdate.ToString("yyyyMMdd" + HHID + "001" + station + starttime + duration + "0000" + flags));
                        // And one more for total viewing Station 0
                        sw.WriteLine(progdate.ToString("yyyyMMdd" + HHID + "001" + "0000" + starttime + duration + "0000" + flags));
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

        private static List<string> ConvertGFKDemos(List<string> stamFiles, string gffDirectory)
        {
            List<string> domesticids = new List<string>();
            HashSet<DemoControlInfo> DemoControl = ReadDemoControl();
            foreach (string stamFile in stamFiles)
            {
                int domesticRef = ReadDomesticWorkerRef(stamFile);
                DateTime fileDate = ParseJulianFileDate(stamFile);
                string[] lines = File.ReadAllLines(stamFile);
                if (lines.Length == 0)
                    continue;

                string outputPath = Path.Combine(gffDirectory, "D" + fileDate.ToString("yyyyMMdd") + ".DAT");
                using (StreamWriter sw = new StreamWriter(outputPath))
                {

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
                        if (fields.Length >= 23)
                        {
                            string column23 = fields[22].Trim().Replace("\"", "").Trim();
                            int column23Value;
                            if (int.TryParse(column23, NumberStyles.Integer, CultureInfo.InvariantCulture, out column23Value)
                                && column23Value == domesticRef)
                            {
                                domesticids.Add(firstItem);
                            }
                        }

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
            return domesticids;
        }

        private static Dictionary<int, string> ReadStationMap()
        {
            Dictionary<int, string> stationMap = new Dictionary<int, string>();
            string path = Path.Combine(Properties.Settings.Default.MapFiles, "gfkstats.txt");
            if (!File.Exists(path))
                throw new FileNotFoundException("gfkstats.txt file not found in directory " + Properties.Settings.Default.MapFiles);

            string[] lines = File.ReadAllLines(path);
            foreach (string strline in lines)
            {
                string line = strline.Trim();
                if (line.Length == 0)
                    continue;

                string[] fields = line.Split(';');
                if (fields.Length < 2)
                    continue;

                int gfkStation;
                if (!int.TryParse(fields[0].Trim().Replace("\"", ""), NumberStyles.Integer, CultureInfo.InvariantCulture, out gfkStation))
                    continue;

                string telmarStation = fields[1].Trim().Replace("\"", "").PadLeft(4, '0');
                if (telmarStation.Length > 4)
                    telmarStation = telmarStation.Substring(telmarStation.Length - 4);
                stationMap[gfkStation] = telmarStation;
            }
            return stationMap;
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

        private static int ReadDomesticWorkerRef(string stamFile)
        {
            string directory = Path.GetDirectoryName(stamFile);
            string fileName = Path.GetFileName(stamFile);
            string stamdefFileName;
            if (fileName.StartsWith("STAMDEF", StringComparison.OrdinalIgnoreCase))
            {
                stamdefFileName = fileName;
            }
            else
            {
                int stamIndex = fileName.IndexOf("STAM", StringComparison.OrdinalIgnoreCase);
                if (stamIndex < 0)
                    throw new FormatException("STAM file name does not contain STAM: " + fileName);
                stamdefFileName = fileName.Substring(0, stamIndex) + "STAMDEF" + fileName.Substring(stamIndex + 4);
            }
            string stamdefPath = Path.Combine(directory, stamdefFileName);
            if (!File.Exists(stamdefPath))
                throw new FileNotFoundException("STAMDEF file not found: " + stamdefPath);

            string[] lines = File.ReadAllLines(stamdefPath);
            foreach (string strline in lines)
            {
                string line = strline.Trim();
                if (line.Length == 0)
                    continue;

                string[] fields = line.Split(';');
                if (fields.Length < 7)
                    continue;

                string column7 = fields[6].Trim().Replace("\"", "").Trim();
                if (column7.IndexOf("Domestic worker", StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                string column6 = fields[5].Trim().Replace("\"", "").Trim();
                int domesticRef;
                if (!int.TryParse(column6, NumberStyles.Integer, CultureInfo.InvariantCulture, out domesticRef))
                    throw new FormatException("Invalid Domestic worker value in STAMDEF file: " + column6);
                return domesticRef;
            }

            throw new InvalidDataException("Domestic worker not found in STAMDEF file: " + stamdefPath);
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
