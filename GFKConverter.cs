using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GFKConverter
{
    public class StationInfo
    {
        public int stationNielsen;
        public int stationTelmar;
        public bool isExtraViewing;
        public bool isTotalViewing;
        public bool isTotalScreenUsage;
        public string stationName;

    }
    public class HHDeminfo
    {
        public int HHNum;
        public string HHDemo;
        public double HHWeight;
        public Dictionary<int, string> PersonInfo=new Dictionary<int,string>();
        public Dictionary<int, string> GuestInfo = new Dictionary<int, string>();
        public Dictionary<int, double> PersonWeights = new Dictionary<int, double>();
        public Dictionary<int, double> GuestWeights = new Dictionary<int, double>();

    }
    public class GFKConverter
    {

        public static Dictionary<int, List<StationInfo> > _stationMap;
        public static Dictionary<int, int> _householdMap;
        public static List<Demographic> _demographics;
        public static StringBuilder errors = new StringBuilder();
        private static bool errorOccurred = false;
        private string fileDate="";
        private string lastHHViewingLine = "";
        private Dictionary<int, Dictionary<int, int>> GuestMapping = new Dictionary<int, Dictionary<int, int>>();
        private List<HHDeminfo> demographicsPerHH=new List<HHDeminfo>();
        private Dictionary<string, Dictionary<int, HHDeminfo>> TSVDemographics = new Dictionary<string, Dictionary<int, HHDeminfo>>();
        //private StreamWriter _weightfile;
        //private StreamWriter WeightFile
        //{
        //    get
        //    {
        //        if (_weightfile == null)
        //        {
        //            _weightfile = new StreamWriter(Properties.Settings.Default.InterMediateFileDirectory + "\\w" + fileDate + ".dat");
        //        }
        //        return _weightfile;
        //    }
        //}
        private static void addError(string message)
        {
            errors.Append(message).Append(Environment.NewLine);
        }
        //private StreamWriter _demofile;
        //private StreamWriter DemoFile
        //{
        //    get
        //    {
        //        if (_demofile == null)
        //        {
        //            _demofile = new StreamWriter(Properties.Settings.Default.InterMediateFileDirectory + "\\d" + fileDate + ".dat");
        //        }
        //        return _demofile;
        //    }
        //}
        private Dictionary<string, StreamWriter> _viewingfiles=new Dictionary<string,StreamWriter>();
        private string lastTSVHHViewingLine = String.Empty;
        private StreamWriter Viewingfile(string viewingdate, string recordingdate)
        {
            string dateindicator = viewingdate + recordingdate;
            if (!_viewingfiles.ContainsKey(dateindicator))
            {
                _viewingfiles[dateindicator] = new StreamWriter(Properties.Settings.Default.InterMediateFileDirectory + "\\v" + dateindicator  + ".dat");
            }
            return _viewingfiles[dateindicator];
        }
        private Dictionary<string, StreamWriter> _demofiles = new Dictionary<string, StreamWriter>();
        private StreamWriter Demofile(string viewingdate, string recordingdate)
        {
            string dateindicator = viewingdate + recordingdate;
            if (!_demofiles.ContainsKey(dateindicator))
            {
                _demofiles[dateindicator] = new StreamWriter(Properties.Settings.Default.InterMediateFileDirectory + "\\d" + dateindicator + ".dat");
            }
            return _demofiles[dateindicator];
        }
        private Dictionary<string, StreamWriter> _weightfiles = new Dictionary<string, StreamWriter>();
        private StreamWriter Weightfile(string viewingdate, string recordingdate)
        {
            string dateindicator = viewingdate + recordingdate;
            if (!_weightfiles.ContainsKey(dateindicator))
            {
                _weightfiles[dateindicator] = new StreamWriter(Properties.Settings.Default.InterMediateFileDirectory + "\\w" + dateindicator + ".dat");
            }
            return _weightfiles[dateindicator];
        }


        public bool ConvertFile(string gfkFile)
        {
            HashSet<int> NielsenStations = new HashSet<int>();
#if DEBUG
            Dictionary<int, Dictionary<string, List<int>>> viewingInfo = new Dictionary<int, Dictionary<string, List<int>>>();
#endif
            bool previouserror = errorOccurred;
            errorOccurred = false;
            errors.Clear();
            StreamReader sr = new StreamReader(gfkFile);
            FileInfo fi = new FileInfo(gfkFile);
            fileDate = fi.Name.Substring(0, 8);
            demographicsPerHH.Clear();
            TSVDemographics.Clear();
            GuestMapping.Clear();
            int i,j;
            i = GetMemberID("aq");
            int HHNum = -1;
            HHDeminfo CurrentHHDem= new HHDeminfo();
            Dictionary<int, int> hhMapping = new Dictionary<int, int>();
            HashSet<string> viewingrecs = new HashSet<string>();
            //List<int> members = new List<int>();
            //List<int> guests = new List<int>();
            string line;
            double weight;
            int pnum;
            int gnum;
            int linesviewingread = 0;
            string[] totalstation = new string[0];
            string[] totalstationTSV = new string[0];
            while ((line = sr.ReadLine()) != null)
            {
                char recordType = line[0];
                string[] fields = line.Substring(1).Split('_');
                switch (recordType)
                {
                    case 'H': // household line
                        HHNum = getHouseHoldNumber(int.Parse(fields[0]));
                        if (HHNum == 242)
                            HHNum = 242;
                        viewingrecs = new HashSet<string>();
                        hhMapping.Add(int.Parse(fields[0]), HHNum);
                        CurrentHHDem = new HHDeminfo();
                        CurrentHHDem.HHDemo = fields[1];
                        CurrentHHDem.HHNum = HHNum;
                        demographicsPerHH.Add(CurrentHHDem);
#if DEBUG
                        if (!viewingInfo.ContainsKey(HHNum))
                            viewingInfo[HHNum] = new Dictionary<string, List<int>>();
#endif

                        //WriteHHDemLine(HHNum, fields[1]);
                        break;
                    case 'R': // regions
                        int numRegions = int.Parse(fields[0]);
                        // should be just 1 regions, perhaps add a warning if multiple regions are detected
                        break;
                    case 'W': //household weight
                        weight = double.Parse(fields[0]);
                        // assuming just one weight
                        CurrentHHDem.HHWeight = weight;
                        //WriteWeight (HHNum,0,weight);
                        break;
                    case 'D': //region dependent demographics
                        // region dependent demographics should be absent
                        break;
                    case 'M': // Member of family
                        pnum = GetMemberID(fields[0]);
                        if (pnum > 1) // 1 is zz and is used for general viewing; not a regular member
                        { // WritePersonDemographics(HHNum,pnum,fields[1]);
                            weight = double.Parse(fields[2].Substring(1));
                            //if (weight > 0)
                            //{
                            //    WriteWeight(HHNum, pnum, weight);
                            //}
                            CurrentHHDem.PersonInfo[pnum] = fields[1];
                            CurrentHHDem.PersonWeights[pnum] = weight;
                        }
                        break;
                    case 'G': //Guest
                        gnum = GetGuestID(fields[0]) + 100 * DayNumber(fileDate);
                        //WriteGuestDemographics(HHNum, gnum,fields[1]);
                        weight = double.Parse(fields[2].Substring(1));
                        if (weight > 0)
                        {
                            CurrentHHDem.GuestInfo[gnum] = fields[1];
                            CurrentHHDem.GuestWeights[gnum] = weight;
                            //WriteWeight(HHNum, gnum, weight);
                        }
                        break;
                    case 'T': // TVSet
                        // currently ignoring TVSet
                        break;
                    case 'V': // Viewing statement
                        linesviewingread++;
                        int Nielsenstation = int.Parse(fields[0]);
                        if (Nielsenstation == 980)
                        {

                        }
                        List<int> stationIds = getStationID(Nielsenstation);
                        foreach (int stationid in stationIds)
                        {
                            bool isExtra = getStationExtraViewing(Nielsenstation, stationid);
                            //bool isTotalViewing = getStationTotalViewing(Nielsenstation);
                            // bool isTotalScreen = (Nielsenstation==Properties.Settings.Default.TotalScreenID );// getStationTotalScreenUsage(Nielsenstation);
                            bool isTotalScreen = getStationTotalScreenUsage(Nielsenstation, stationid);
                            if (!NielsenStations.Contains(Nielsenstation))
                                NielsenStations.Add(Nielsenstation);
                            // 2-11-2016: changed the check to use -1 for exclusion
                            //if (isTotalScreen)
                            //    totalstation = fields;
                            //if (stationId > -1 && !isTotalViewing  )
                            if (stationid > -1)
                            {
                                string initflags = GetViewingFlags(fields[3]);
                                for (i = 4; i < fields.Length; i++)
                                {
#if DEBUG
                                if (!viewingInfo[HHNum].ContainsKey(fields[i]))
                                    viewingInfo[HHNum][fields[i]] = new List<int>();
                                viewingInfo[HHNum][fields[i]].Add(Nielsenstation  );
#endif
                                    if (!isTotalScreen)
                                    {
                                        if (!viewingrecs.Contains(fields[i]))
                                            viewingrecs.Add(fields[i]);
                                    }
                                    string flags = initflags;
                                    // viewing statement
                                    string starttime = fields[i].Substring(fields[i].Length - 12, 6);
                                    string endtime = fields[i].Substring(fields[i].Length - 6, 6);
                                    string viewers = fields[i].Substring(0, fields[i].Length - 12);
                                    if (isExtra)
                                        flags = String.Format("{0}1{1}", flags.Substring(0, 12), flags.Substring(13));
                                    //if (isTotalScreen && !totalstation.Contains(fields[i]) )
                                    //if (isTotalScreen && !viewingrecs.Contains(fields[i]))
                                    //        flags = String.Format("{0}1{1}", flags.Substring(0, 12), flags.Substring(13));

                                    string vflags = flags;
                                    for (j = 0; j < viewers.Length; j += 2)
                                    {
                                        string viewer = viewers.Substring(j, 2);
                                        if (viewer == "zz")
                                        {
                                            continue;
                                        }
                                        if (viewer[0] >= 'A' && viewer[0] <= 'Z') // guest
                                        {
                                            pnum = 100 * DayNumber(fileDate) + GetGuestID(viewer);
                                            // set guest flag
                                            vflags = String.Format("{0}1{1}", flags.Substring(0, 1), flags.Substring(2));
                                        }
                                        else
                                        {
                                            pnum = GetMemberID(viewer);
                                        }
                                        WriteViewingLine(HHNum, pnum, starttime, endtime, stationid, vflags);
                                        if (pnum < 100)
                                            lastHHViewingLine = "";
                                    }
                                    WriteViewingLine(HHNum, 0, starttime, endtime, stationid, flags);

                                }
                            }
                        }
                        break;
                    case 'S': // Time Shifted Viewing statement.
                        linesviewingread++;
                        List<int> SstationIds = getStationID(int.Parse(fields[0]));
                        foreach (int SstationId in SstationIds)
                        {
                            bool SisExtra = getStationExtraViewing(int.Parse(fields[0]), SstationId);
                            bool SisTotalViewing = getStationTotalViewing(int.Parse(fields[0]), SstationId);
                            bool SisTotalScreen = getStationTotalScreenUsage(int.Parse(fields[0]),SstationId);
                            if (SisTotalViewing)
                                totalstationTSV = fields;

                            if (SstationId > -1)
                            {
                                string flags = GetViewingFlags(fields[3]);
                                //if (SisExtra)
                                //    flags = String.Format("{0}1{1}", flags.Substring(0, 12), flags.Substring(13));
                                if (SisTotalScreen && !totalstationTSV.Contains(fields[4]))
                                    flags = String.Format("{0}1{1}", flags.Substring(0, 12), flags.Substring(13));

                                // base flag, to be extended with the TSV flags
                                string starttime = fields[4].Substring(fields[4].Length - 12, 6);
                                string endtime = fields[4].Substring(fields[4].Length - 6, 6);
                                string viewers = fields[4].Substring(0, fields[4].Length - 12);
                                string recordingDate = fields[5];
                                string recordingstart = fields[6];
                                string recordingend = fields[7];
                                string tsvFlags = "";
                                int dateshift = GetDateShift(recordingDate, fileDate);
                                //if (recordingDate == fileDate)
                                //    tsvFlags = String.Format("{0}1{1}", flags.Substring(0, 1), flags.Substring(2));
                                //else
                                //    tsvFlags = String.Format("{0}1{1}", flags.Substring(0, 2), flags.Substring(3));
                                // if (dateshift < 0) dateshift = 0; // for testing only!!
                                try
                                {
                                    tsvFlags = String.Format("{0}1{1}", flags.Substring(0, dateshift + 2), flags.Substring(dateshift + 3));
                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }
                                //if (HHNum == 8338)
                                //    HHNum = HHNum;
                                for (j = 0; j < viewers.Length; j += 2)
                                {
                                    string vtsvFlags = tsvFlags;
                                    string vFlags = flags;
                                    string viewer = viewers.Substring(j, 2);
                                    if (viewer == "zz")
                                    {
                                        continue;
                                    }
                                    if (viewer[0] >= 'A' && viewer[0] <= 'Z') // guest
                                    {
                                        pnum = 100 * DayNumber(fileDate) + GetGuestID(viewer);
                                        // set guest flag
                                        vFlags = String.Format("{0}1{1}", flags.Substring(0, 1), flags.Substring(2));
                                        vtsvFlags = String.Format("{0}1{1}", tsvFlags.Substring(0, 1), tsvFlags.Substring(2));

                                    }
                                    else
                                    {
                                        pnum = GetMemberID(viewer);
                                    }
                                    // 31-10-2011: do not add the record if the respondent was not in-tab on the day of recording 
                                    //if (recordingDate!=fileDate && !RespondentInTab(recordingDate, HHNum, pnum))
                                    //{
                                    //    // add day if not present
                                    //    if (!TSVDemographics.ContainsKey(recordingDate))
                                    //        TSVDemographics[recordingDate] = new Dictionary<int, HHDeminfo>();
                                    //    // add household if not present
                                    //    if (!TSVDemographics[recordingDate].ContainsKey(HHNum))
                                    //    {
                                    //        TSVDemographics[recordingDate][HHNum] = new HHDeminfo() { HHNum = HHNum, HHDemo = CurrentHHDem.HHDemo };
                                    //    }
                                    //    // add person if not present
                                    //    if (pnum < 100 && !TSVDemographics[recordingDate][HHNum].PersonInfo.ContainsKey(pnum))
                                    //    {
                                    //        TSVDemographics[recordingDate][HHNum].PersonInfo[pnum] = CurrentHHDem.PersonInfo[pnum];
                                    //        TSVDemographics[recordingDate][HHNum].PersonWeights [pnum] = 0;
                                    //    }
                                    //   // guest: check the mapping for this guest
                                    //    if (pnum > 100)
                                    //    {
                                    //        int guestNum = GetTSVGuestID(HHNum, pnum);
                                    //        if (!TSVDemographics[recordingDate][HHNum].GuestInfo.ContainsKey(guestNum))
                                    //        {
                                    //            TSVDemographics[recordingDate][HHNum].GuestInfo[guestNum] = CurrentHHDem.GuestInfo[pnum];
                                    //            TSVDemographics[recordingDate][HHNum].GuestWeights[guestNum]=CurrentHHDem.GuestWeights [pnum ];
                                    //        }
                                    //    }

                                    // }
                                    // 6-5-11: do not write the 9999 record on the day of watching
                                    // WriteViewingLine(HHNum, pnum, starttime, endtime, 9999, vFlags);
                                    //26-10-2011 Write the record with a new flag, and the actual station 
                                    //if (dateshift > 0 && pnum >= 100)
                                    //    pnum = pnum;
                                    string vOnDayTSVflags = String.Format("{0}1{1}", vFlags.Substring(0, 10), vFlags.Substring(11));
                                    WriteViewingLine(HHNum, pnum, starttime, endtime, SstationId, vOnDayTSVflags);
                                    if (recordingDate == fileDate || (RespondentInTab(recordingDate, HHNum, pnum) && pnum < 100))
                                        WriteTSVViewingline(HHNum, pnum, recordingstart, recordingend, SstationId, vtsvFlags, recordingDate);
                                    if (pnum < 100)
                                        lastHHViewingLine = "";
                                }
                                //WriteViewingLine(HHNum, 0, starttime, endtime, 9999, flags);
                                string OnDayTSVflags = String.Format("{0}1{1}", flags.Substring(0, 10), flags.Substring(11));
                                // only add if the household is in-tab; Martin 31-10
                                WriteViewingLine(HHNum, 0, starttime, endtime, SstationId, OnDayTSVflags);
                                if (recordingDate == fileDate || RespondentInTab(recordingDate, HHNum, 0))
                                    WriteTSVViewingline(HHNum, 0, recordingstart, recordingend, SstationId, tsvFlags, recordingDate);

                            }
                        }
                        break;

                }
            }
  
            WriteWeights(fileDate );
            WriteDemographics(fileDate );
            WriteStationFile(fileDate, NielsenStations );
            sr.Close();
            CloseAllFiles();
#if DEBUG
            using (StreamWriter sw = new StreamWriter(@"c:\auke\analysegfk.txt"))
            {
                foreach (int hh in viewingInfo.Keys )
                {
                    foreach (string statement in viewingInfo[hh].Keys )
                    {
                        sw.Write("{0}\t{1}",hh,statement );
                        foreach (int st in viewingInfo[hh][statement])
                            sw.Write("\t{0}", st);
                        sw.WriteLine();
                    }
                }
                sw.Close();
            }
#endif

            if (linesviewingread == 0)
            {
                errorOccurred = true;
                errors.Append("File ").Append(gfkFile).Append(" has no viewing lines");
            }
            if (errorOccurred)
                RemoveIntermediatefiles();
            if (!errorOccurred)
            {
                errorOccurred = previouserror;
                return true;
            }
            else
                return false; 
            // check household mapping
            //var mapped = (from x in hhMapping where x.Key != x.Value select x).ToList();
            //var doublemapped = (from x in hhMapping join y in hhMapping
            //                           on x.Value equals y.Value 
            //                           where x.Key !=y.Key 
            //                           select new {x,y}).ToList();
        }

        private void WriteStationFile(string fileDate, HashSet<int> NielsenStations)
        {
            StreamWriter sw = new StreamWriter(Path.Combine(Properties.Settings.Default.GFFDirectory,"STN"+fileDate+".dat"));
            Dictionary<int, string> TelmarStations = new Dictionary<int, string>();
            foreach (int st in NielsenStations)
            {
                foreach (var stinf in Stationmap[st])
                {
                    if (TelmarStations.ContainsKey(stinf.stationTelmar))
                    {
                        TelmarStations[stinf.stationTelmar] = TelmarStations[stinf.stationTelmar] +
                            " - " + stinf.stationName;
                    }
                    else
                    {
                        TelmarStations[stinf.stationTelmar] = stinf.stationName;
                    }
                }
            }

            foreach (int st in TelmarStations.Keys)
                sw.WriteLine("{0}\t{1}", st, TelmarStations[st]);
            sw.Close();
        }

 
        private int GetTSVGuestID(int HHNum, int pnum)
        {
            int retval = 0;
            int day=pnum/100;
            if (!GuestMapping.ContainsKey(HHNum))
                GuestMapping[HHNum] = new Dictionary<int, int>();
            if (!GuestMapping[HHNum].ContainsKey(pnum))
            {
                if ((from x in GuestMapping[HHNum].Values where (x/100==day) select x).Count()==0)
                    retval = day*100+99;
                else
                    retval = (from x in GuestMapping[HHNum].Values where x/100==day select x).Min() - 1;
                GuestMapping[HHNum][pnum] = retval;
            }
            else
            {
                retval = GuestMapping[HHNum][pnum];
            }
            return retval;
        }

        private void RemoveIntermediatefiles()
        {
            string file = Properties.Settings.Default.InterMediateFileDirectory + "\\w" + fileDate + ".dat";
            if (File.Exists(file))
                File.Delete(file);
            file = Properties.Settings.Default.InterMediateFileDirectory + "\\d" + fileDate + ".dat";
            if (File.Exists(file))
                File.Delete(file);
            var vfiles=Directory.GetFiles(Properties.Settings.Default.InterMediateFileDirectory,"v"+fileDate+".dat");
            foreach (string f in vfiles)
                File.Delete(f);
        }

        private static int GetDateShift(string recordingDate, string viewingDate)
        {
           // both dates are as yyyymmdd 
            DateTime end = yyyymmddToDateTime(viewingDate);
            DateTime start = yyyymmddToDateTime(recordingDate);
            return (end - start).Days;

        }

   
        private int DayNumber(string fileDate)
        {
            DateTime dt=new DateTime(int.Parse (fileDate.Substring(0,4)),
                int.Parse(fileDate.Substring(4,2)),int.Parse(fileDate.Substring(6,2)));
            switch (dt.DayOfWeek)
            {
                case DayOfWeek.Monday :
                    return 1;
                    break;
                case DayOfWeek.Tuesday:
                    return 2;
                    break;
                case DayOfWeek.Wednesday :
                    return 3;
                    break;
                case DayOfWeek.Thursday :
                    return 4;
                    break;
                case DayOfWeek.Friday :
                    return 5;
                    break;
                case DayOfWeek.Saturday :
                    return 6;
                    break;
                case DayOfWeek.Sunday :
                    return 7;
                    break;
            }
            return -1;
        }

        private void WriteWeights(string viewingdate)
        {
            // write current day
            foreach (HHDeminfo hhdem in demographicsPerHH)
            {

                Weightfile(viewingdate,viewingdate).WriteLine(fileDate + IntToStr(hhdem.HHNum , 8) + IntToStr(0, 3) + "00000" +
                        IntToStr(((int)(hhdem.HHWeight * 1000)), 8));
                foreach(int pnum in hhdem.PersonWeights.Keys)
                    if (((int)(hhdem.PersonWeights[pnum])!=0))
                        Weightfile(viewingdate, viewingdate).WriteLine(fileDate + IntToStr(hhdem.HHNum, 8) + IntToStr(pnum, 3) + "00000" +
                            IntToStr(((int)(hhdem.PersonWeights[pnum] * 1000)), 8));
                foreach(int gnum in hhdem.GuestWeights.Keys)
                   if (((int)(hhdem.GuestWeights[gnum])!=0))
                    Weightfile(viewingdate, viewingdate).WriteLine(fileDate + IntToStr(hhdem.HHNum, 8) + IntToStr(gnum, 3) + "00000" +
                        IntToStr(((int)(hhdem.GuestWeights[gnum] * 1000)), 8));

            }
            // write TSVDays
            foreach (string tsvdate in TSVDemographics.Keys)
            {

                foreach (int HHNum in TSVDemographics[tsvdate].Keys)
                {
                    HHDeminfo hhdem = TSVDemographics[tsvdate][HHNum];
                    Weightfile(viewingdate, tsvdate ).WriteLine(tsvdate  + IntToStr(hhdem.HHNum, 8) + IntToStr(0, 3) + "00000" +
                            IntToStr(((int)(hhdem.HHWeight * 1000)), 8));
                    foreach (int pnum in hhdem.PersonWeights.Keys)
                        Weightfile(viewingdate, tsvdate).WriteLine(tsvdate  + IntToStr(hhdem.HHNum, 8) + IntToStr(pnum, 3) + "00000" +
                                IntToStr(((int)(hhdem.PersonWeights[pnum] * 1000)), 8));
                    foreach (int gnum in hhdem.GuestWeights.Keys)
                        Weightfile(viewingdate, tsvdate).WriteLine(tsvdate  + IntToStr(hhdem.HHNum, 8) + IntToStr(gnum, 3) + "00000" +
                            IntToStr(((int)(hhdem.GuestWeights[gnum] * 1000)), 8));
                }

            }


        }
        // actually viewingdate seems to be the same as filedate
        private void WriteDemographics(string viewingdate)
        {
            if (errorOccurred)
                return;
            // write all demographics
            DateTime currentdate=yyyymmddToDateTime(fileDate);
            List<Demographic> HHdems = (from x in Demographics where x.EndDate>=currentdate &&
                             x.StartDate<=currentdate && x.AppliesTo.Contains("H")
                              orderby x.StartPos select x ).ToList();
            List<Demographic> Persondems = (from x in Demographics where x.EndDate>=currentdate &&
                                        x.StartDate <= currentdate && x.AppliesTo.Contains("M")
                                            orderby x.StartPos
                                            select x).ToList();
            List<Demographic> Guestdems = (from x in Demographics where x.EndDate>=currentdate &&
                                        x.StartDate <= currentdate && x.AppliesTo.Contains("G")
                                           orderby x.StartPos
                                           select x).ToList();

            foreach (HHDeminfo hhinfo in demographicsPerHH)
            { 
                //if (hhinfo.HHNum==187)
                //    currentdate = yyyymmddToDateTime(fileDate);
                // write household
                StringBuilder  lcdemline =new StringBuilder( fileDate + IntToStr(hhinfo.HHNum, 8) + IntToStr(0, 3));
                foreach (Demographic d in HHdems)
                {
                    while (lcdemline.Length < d.StartPos ) lcdemline.Append("0");
                    lcdemline.Append(IntToStr(GetDemoValue(d, hhinfo,fileDate , hhinfo.HHDemo,0,"H" ),d.Length ));
                }
                Demofile(viewingdate, viewingdate).WriteLine(lcdemline);
                //DemoFile.WriteLine(lcdemline);
                foreach (var m in hhinfo.PersonInfo)
                {
                    if (hhinfo.PersonWeights[m.Key] > 0)
                    {
                        lcdemline = new StringBuilder(fileDate + IntToStr(hhinfo.HHNum, 8) + IntToStr(m.Key, 3));
                        foreach (Demographic d in Persondems)
                        {
                            while (lcdemline.Length < d.StartPos) lcdemline.Append("0");
                            lcdemline.Append(IntToStr(GetDemoValue(d, hhinfo, fileDate, m.Value, m.Key, "M"), d.Length));
                        }
                        Demofile(viewingdate, viewingdate).WriteLine(lcdemline);
                        //DemoFile.WriteLine(lcdemline);
                    }
                }
                foreach (var g in hhinfo.GuestInfo )
                {
                    lcdemline = new StringBuilder(fileDate + IntToStr(hhinfo.HHNum, 8) + IntToStr(g.Key, 3));
                    foreach (Demographic d in Guestdems)
                    {
                        while (lcdemline.Length < d.StartPos ) lcdemline.Append("0");
                        lcdemline.Append(IntToStr(GetDemoValue(d, hhinfo, fileDate, g.Value, g.Key, "G"), d.Length));
                    }
                    Demofile(viewingdate, viewingdate).WriteLine(lcdemline);
                    //DemoFile.WriteLine(lcdemline);
 
                }
            }
            // the time shifted part; maybe should try to create one function.
            foreach (string tsvdate in TSVDemographics.Keys)
            {
                foreach (int HHNum in TSVDemographics[tsvdate].Keys)
                {
                    HHDeminfo hhinfo = TSVDemographics[tsvdate][HHNum];
                    //if (hhinfo.HHNum==187)
                    //    currentdate = yyyymmddToDateTime(fileDate);
                    // write household
                    StringBuilder lcdemline = new StringBuilder(tsvdate  + IntToStr(hhinfo.HHNum, 8) + IntToStr(0, 3));
                    foreach (Demographic d in HHdems)
                    {
                        while (lcdemline.Length < d.StartPos) lcdemline.Append("0");
                        lcdemline.Append(IntToStr(GetDemoValue(d, hhinfo, fileDate, hhinfo.HHDemo, 0, "H"), d.Length));
                    }
                    Demofile(viewingdate , tsvdate ).WriteLine(lcdemline);
                    //DemoFile.WriteLine(lcdemline);
                    foreach (var m in hhinfo.PersonInfo)
                    {
                            lcdemline = new StringBuilder(tsvdate  + IntToStr(hhinfo.HHNum, 8) + IntToStr(m.Key, 3));
                            foreach (Demographic d in Persondems)
                            {
                                while (lcdemline.Length < d.StartPos) lcdemline.Append("0");
                                lcdemline.Append(IntToStr(GetDemoValue(d, hhinfo, fileDate, m.Value, m.Key, "M"), d.Length));
                            }
                            Demofile(viewingdate, tsvdate ).WriteLine(lcdemline);
                            //DemoFile.WriteLine(lcdemline);
                    }
                    foreach (var g in hhinfo.GuestInfo)
                    {
                        lcdemline = new StringBuilder(tsvdate  + IntToStr(hhinfo.HHNum, 8) + IntToStr(g.Key, 3));
                        foreach (Demographic d in Guestdems)
                        {
                            while (lcdemline.Length < d.StartPos) lcdemline.Append("0");
                            lcdemline.Append(IntToStr(GetDemoValue(d, hhinfo, fileDate, g.Value, g.Key, "G"), d.Length));
                        }
                        Demofile(viewingdate, tsvdate ).WriteLine(lcdemline);
                        //DemoFile.WriteLine(lcdemline);

                    }
                }

            }
        }

        private int GetDemoValue(Demographic d, HHDeminfo hhinfo, string date, string gfkDemo, int MemberID, string type)
        {
            int retval = 0;
            string input = "";
            DateTime activedate = yyyymmddToDateTime(date);
            GFKConvertInfo convertinfo=new GFKConvertInfo();
            foreach (GFKConvertInfo gfkInfo in d.GFKConvertInfo)
            {
                if (gfkInfo.AppliesTo.Contains(type))
                    convertinfo = gfkInfo;
            }
            if (convertinfo.startpos > 0)
            {
                if (gfkDemo.Length < (convertinfo.startpos-1 + convertinfo.length))
                {
                    throw new Exception(String.Format("Demographic info shorter than expected for {0} position {1} length {2} is defined, input length is {3} , Household {4} Member {5} ",
                        d.getMnemonic(), convertinfo.startpos ,convertinfo.length ,gfkDemo.Length , hhinfo.HHNum ,MemberID ));
                }
                input = gfkDemo.Substring(convertinfo.startpos - 1, convertinfo.length);
            }
            switch (convertinfo.calculationtype)
            {
                case GFKCalculation.None:
                    retval = GFKStrToInt(input);
                    break;
                case GFKCalculation.CharMapped:
                    if (!convertinfo.CharvalueMapping.ContainsKey(input[0]))
                    {
                        throw new Exception(String.Format("Input value {0} for demo {1} is not mapped in democontrol. Either the input file is incorrect, or the mapping needs to be modified. The error occured with Household {2}, Member {3}",
                            input[0],d.getMnemonic(),hhinfo.HHNum,MemberID ));
                    }
                    retval = convertinfo.CharvalueMapping[input[0]];
                    break;
                case GFKCalculation.NoneWith0toA :
                    retval = GFKStrToInt(input);
                    if (retval >=10) retval +=1;
                    if (retval == 0) retval = 10;
                    break;
                case GFKCalculation.BdayToAGE:
                    DateTime Bday = yyyymmddToDateTime(input);
                    DateTime Today = yyyymmddToDateTime(fileDate);
                    retval = (Today.Year - Bday.Year);
                    if (Today.Month < Bday.Month)
                        retval -= 1;
                    if (Today.Month == Bday.Month && Today.Day < Bday.Day)
                        retval -= 1;
                    if (retval >= 100)
                        retval = 99;
                    break;
                case GFKCalculation.ComposedBinary:
                    bool valid = true;
                    foreach (var x in convertinfo.composingValues)
                    {
                        Demographic demo = GetDemo(x.Key,date);
                            int demvalue = GetDemoValue(demo, hhinfo, date, gfkDemo, MemberID, type);
                        if (!x.Value.Contains(demvalue))
                            valid = false;

                    }
                    retval = valid ? 1 : 0;
                    break;
                case GFKCalculation.MotherReference :
                    bool memberValid = true;
                    memberValid = HouseholdhasMemberWithReference(convertinfo.MotherRefStartPos, convertinfo.MotherRefLength, MemberID, hhinfo);
                    retval = memberValid ? 1 : 0;
                    break;
                case GFKCalculation.FixedValue:
                    retval = convertinfo.FixedValue;
                    break;
                case GFKCalculation.HHOtherMembers:
                    retval = HouseholdhasMemberWithCondition(convertinfo.OtherMemberValues,date, hhinfo) ? 1 : 0;
                    break;
                case GFKCalculation.MappedValues:
                    if (!convertinfo.valueMapping.ContainsKey(GFKStrToInt(input)))
                        throw new Exception(String.Format("Input value {4}, file value {0} for demo {1} is not mapped in democontrol. Either the input file is incorrect, or the mapping needs to be modified. The error occured with Household {2}, Member {3}",
                            input[0], d.getMnemonic(), hhinfo.HHNum, MemberID, GFKStrToInt(input)));
                    retval = convertinfo.valueMapping[GFKStrToInt(input)];
                    break;
 
            }
            return retval;
        }

        private Demographic GetDemo(string code, string date)
        {
            DateTime activedate = yyyymmddToDateTime(date);
            var demo = (from y in Demographics where y.StartDate <= activedate && y.EndDate >= activedate && y.Demid.Trim() == code select y).ToList();
            if (demo.Count > 1)
                throw new Exception(String.Format("Ambiguity detected in finding  demo {0}  on day{1}", code,  date));
            if (demo.Count == 0)
                throw new Exception(String.Format("Could not find composing demo {0}  on day{1}", code,  date));
            return demo[0];
        }

        private bool HouseholdhasMemberWithReference(int MrefStartPos, int MrefLength, int MemberID, HHDeminfo hhinfo)
        {
            bool retval = false;
            foreach (var x in hhinfo.PersonInfo)
            {
                int refId = GFKStrToInt (x.Value.Substring(MrefStartPos - 1, MrefLength));
                if (refId == getGFKMemberId(MemberID) && x.Key !=MemberID )
                {
                    retval = true;
                    break;
                }
            }
            return retval;
            
        }

  
        private bool HouseholdhasMemberWithCondition(Dictionary<string, List<int>> democondition, string date, HHDeminfo hhinfo)
        {
            bool retval = false;
            foreach (var x in hhinfo.PersonInfo)
            {
                bool respvalid = true;
                foreach (var y in democondition)
                {
                    Demographic demo = GetDemo(y.Key, date);
                    if (!y.Value.Contains(GetDemoValue(demo , hhinfo, date, x.Value,x.Key,"M" )))
                        respvalid = false;
                
                }
                if (respvalid)
                {
                    retval = true;
                    break;
                }
            }
            return retval;
        }

  
        private int GFKStrToInt(string p)
        {
           if (p.Length >1)
               return int.Parse(p);
           if (p[0] >= 'A')
               return (10 + (p[0] - 'A'));
           return int.Parse(p);
        }

        private void CloseAllFiles()
        {
            //if (_demofile !=null) _demofile.Close();
            //if (_weightfile != null) _weightfile.Close();
            if (_viewingfiles != null)
            {
                foreach (StreamWriter sw in _viewingfiles.Values)
                {
                    sw.Close();
                }
            }
            if (_demofiles != null)
            {
                foreach (StreamWriter sw in _demofiles.Values)
                {
                    sw.Close();
                }
            }
            if (_weightfiles != null)
            {
                foreach (StreamWriter sw in _weightfiles.Values)
                {
                    sw.Close();
                }
            }

        }

        //private  void WriteGuestDemographics(int HHNum, int gnum, string p)
        //{
        //   DemoFile.WriteLine(fileDate+IntToStr(HHNum,8)+IntToStr(gnum,3)) ;
        //}

        private string IntToStr(int num, int p)
        {
            string retval=num.ToString();
            if (retval.Length > p)
                throw new Exception("Converting to a shorter string");
            return retval.PadLeft(p, '0');
        }

        //private  void WritePersonDemographics(int HHNum, int pnum, string p)
        //{
        //    DemoFile.WriteLine(fileDate + IntToStr(HHNum, 8) + IntToStr(pnum, 3));
        //}

        //private  void WriteWeight(int HHNum, int p, double weight)
        //{
        //    if (errorOccurred)
        //        return;
        //    WeightFile.WriteLine(fileDate+IntToStr(HHNum, 8) + IntToStr(p, 3)+"00000"+IntToStr(((int) (weight*1000)),8));
        //}

        //private void WriteHHDemLine(int HHNum, string p)
        //{
        //    DemoFile.WriteLine(fileDate + IntToStr(HHNum, 8) + IntToStr(0, 3));
        //}

        private void WriteViewingLine(int HHNum, int pnum, string starttime, string endtime, int stationId, string flags)
        {
            if (errorOccurred)
                return;
            // used only for current date
            string vwline = fileDate + IntToStr(HHNum, 8) + IntToStr(pnum, 3) +
                    IntToStr(stationId, 4) + starttime + Duration(starttime, endtime) + "0000" + flags;
            if (vwline != lastHHViewingLine)
            {
                Viewingfile(fileDate, fileDate).WriteLine(vwline);
                if (pnum == 0)
                    lastHHViewingLine = vwline;
            }
        }
        private void WriteTSVViewingline(int HHNum, int pnum, string recordingstart, string recordingend, int stationId, string flags, string recordingDate)
        {
            if (errorOccurred)
                return;
            // check whether the respondent is present on the day
            // 20-6-11: check removed, respondents will be added

            //if (recordingDate!=fileDate )
            //    if (!RespondentInTab(recordingDate, HHNum, pnum))
            //        return;
            string vwline = recordingDate  + IntToStr(HHNum, 8) + IntToStr(pnum, 3) +
                    IntToStr(stationId, 4) + recordingstart + Duration(recordingstart , recordingend) 
                    + "0000" + flags;
            if (vwline != lastTSVHHViewingLine)
            {
                Viewingfile(fileDate, recordingDate ).WriteLine(vwline);
                if (pnum == 0)
                    lastTSVHHViewingLine = vwline;
            }
        }

        private bool RespondentInTab(string recordingDate, int HHNum, int pnum)
        {
#if DEBUG
           // return false;
#endif
            string recordingfile = Properties.Settings.Default.InterMediateFileDirectory + "\\W"+recordingDate+recordingDate +".Dat" ;
            if (!File.Exists(recordingfile))
            {
                recordingfile = Properties.Settings.Default.InterMediateFileDirectory + "\\W" + recordingDate + ".Dat";
                if (!File.Exists(recordingfile))
                {
                    recordingfile = Properties.Settings.Default.InterMediateFileDirectory + "\\W" + recordingDate + recordingDate + ".Dat";
                    //return true; // for testing only!!
                    throw new Exception("File " + recordingfile + " is not present. This file is needed to check the respondents for Time shifted viewing!");
                }
            }
            StreamReader sr = new StreamReader(recordingfile);
            string line;
            bool present = false;
            while ((line =sr.ReadLine())!=null)
            {
                int hh = int.Parse(line.Substring(8, 8));
                int pn = int.Parse(line.Substring(16, 3));
                if (hh == HHNum && pn == pnum)
                {
                    present = true;
                    break;
                }
            }
            return present;
        }

        private string Duration(string starttime, string endtime)
        {
           // duration in seconds;
            int start = Seconds(starttime);
            int end = Seconds(endtime);
            return (IntToStr(end-start+1,5));
        }

        private int Seconds(string time)
        {
            return 3600 * int.Parse(time.Substring(0, 2)) +
                    60 * int.Parse(time.Substring(2, 2)) +
                    int.Parse(time.Substring(4, 2));
        }

        private static int getHouseHoldNumber(int p)
        {
            // this function can lead to overlapping; need to add checks!!
            if (Householdmap.ContainsKey(p))
                return Householdmap[p];
            else
                return p ;//  % 100000;
        }

        private static string GetViewingFlags(string p)
        {
            // 1-character code: first flag is digital/analogue
            if (p.Length == 1)
                if (p[0] == '1')
                    return "1000000000000000";
                else
                    return "0000000000000000";
            else
                if (p[1] == '2')
                    return "1000000000000000";
                else
                    return "0000000000000000";



        }
        

        private static List<int> getStationID(int NielsenId)
        {
            //if (NielsenId == 65535)
            //    return 0;

            if (!Stationmap.ContainsKey(NielsenId))
            {
                errorOccurred = true;
                addError(String.Format("Invalid or new station: {0}, not contained in gfkstats.dat", NielsenId));
                Stationmap[NielsenId] = new List<StationInfo> { new StationInfo() { stationNielsen = NielsenId, stationTelmar = -1, isExtraViewing = false, stationName = "Unknown station" } }; // to avoid further errors
                //throw new Exception(String.Format("Invalid or new station: {0}, not contained in gfkstats.dat",NielsenId ));
                return new List<int>() { -1 };
            }
            else
            {
                List<int> retval = new List<int>();
                foreach (var stinf in Stationmap[NielsenId])
                    retval.Add(stinf.stationTelmar);
                return retval  ;
            }
        }
        private static bool getStationExtraViewing(int NielsenId, int stationid)
        {
            //if (NielsenId == 65535)
            //    return 0;

            if (!Stationmap.ContainsKey(NielsenId))
            {
                errorOccurred = true;
                addError(String.Format("Invalid or new station: {0}, not contained in gfkstats.dat", NielsenId));
                Stationmap[NielsenId] = new List<StationInfo> { new StationInfo() { stationNielsen = NielsenId, stationTelmar = -1, isExtraViewing = false, stationName = "Unknown station" } }; // to avoid further errors
                //throw new Exception(String.Format("Invalid or new station: {0}, not contained in gfkstats.dat",NielsenId ));
                return false;
            }
            else
            {
                foreach (var stinf in Stationmap[NielsenId])
                {
                    if (stinf.stationTelmar==stationid)
                        return stinf.isExtraViewing;
                }
                return false; // should never happen
            }
        }
   
        private static bool getStationTotalViewing(int NielsenId, int stationid)
        {
            //if (NielsenId == 65535)
            //    return 0;

            if (!Stationmap.ContainsKey(NielsenId))
            {
                errorOccurred = true;
                addError(String.Format("Invalid or new station: {0}, not contained in gfkstats.dat", NielsenId));
                Stationmap[NielsenId] = new List<StationInfo> { new StationInfo() { stationNielsen = NielsenId, stationTelmar = -1, isExtraViewing = false, stationName = "Unknown station" } }; // to avoid further errors
                //throw new Exception(String.Format("Invalid or new station: {0}, not contained in gfkstats.dat",NielsenId ));
                return false;
            }
            else
            {
                foreach (var stinf in Stationmap[NielsenId])
                {
                    if (stinf.stationTelmar == stationid)
                        return stinf.isTotalViewing;
                }
                return false; // should never happen
            }
        }
        private static bool getStationTotalScreenUsage(int NielsenId, int stationid)
        {
            //if (NielsenId == 65535)
            //    return 0;

            if (!Stationmap.ContainsKey(NielsenId))
            {
                errorOccurred = true;
                addError(String.Format("Invalid or new station: {0}, not contained in gfkstats.dat", NielsenId));
                Stationmap[NielsenId] = new List<StationInfo> { new StationInfo() { stationNielsen = NielsenId, stationTelmar = -1, isExtraViewing = false, stationName = "Unknown station" } }; // to avoid further errors
                //throw new Exception(String.Format("Invalid or new station: {0}, not contained in gfkstats.dat",NielsenId ));
                return false;
            }
            else
            {
                foreach (var stinf in Stationmap[NielsenId])
                {
                    if (stinf.stationTelmar == stationid)
                        return stinf.isTotalScreenUsage;
                }
                return false; // should never happen
            }
        }

        
        private static int GetGuestID(string p)
        {
            int num1 = (p[0] - 'A') * 26;
            int num2 = (p[1] - 'A') + 1;

            int GuestId = num1 + num2;

            return GuestId;
        }

        private static int GetMemberID(string p)
        {
            // this function is slightly odd because of historical reasons:
            // zz was always converted to 1, and other codes (aa, ab where converted
            // as base 26 integers), with the exception of aa which was 15 
            // then when numbers above 15 came along the function was adopted 
            //************************************************************************
            //************************************************************************
            //***** Note that the following sequences must be returned because *******
            //***** of changes to the number of memebers in the house hold     *******
            //***** where members aa was calculated as 15 then we received ao  *******
            //***** and ap                                                     *******
            //************************************************************************
            //************************************************************************
            //	aa -> 15
            //
            //	ab -> 2
            //
            //	.
            //
            //	.
            //
            //	an -> 14
            //
            //	ao -> 17
            //
            //	ap -> 16
            //
            //	aq -> 18 (if and when any of these are added)
            //************************************************************************
            //************************************************************************
            //************************************************************************

            int mmId;
            int Num1 = 0;
            int Num2 = 0;
            //if (p=="aq")
            //{

            //}
            switch (p[0])
            {
                case 'a':
                    {
                        switch (p[1])
                        {
                            case 'a': Num2 = 15; break;
                            case 'p': Num2 = 16; break;
                            case 'o': Num2 = 17; break;
                            default: Num2 = (p[1] - 'a') + 1; break;
                        }
                        if (p[1] >= 'q')
                            Num2 = (p[1] - 'a') + 2;
                        break;
                    }

                case 'z':
                    {
                        // House hold record
                        Num2 = 1;
                        break;
                    }

                default:
                    {
                        Num1 = (p[0] - 'a') * 26;
                        Num2 = (p[1] - 'a') + 1;
                        break;
                    }
            }

            mmId = Num1 + Num2;
            return mmId;
        }
        /// <summary>
        /// Return the internal id 
        /// see the function above for explanation
        /// 1->15
        /// 15->17
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        private int getGFKMemberId(int MemberID)
        {
            if (MemberID == 15)
                return 1;
            if (MemberID == 17)
                return 15;
            return MemberID;
        }

        public static Dictionary<int, List<StationInfo>  > Stationmap
        {
            get
            {
                if (_stationMap == null)
                    generateStationMap();
                return _stationMap;
            }
        }
        public static Dictionary<int, int> Householdmap
        {
            get
            {
                if (_householdMap == null)
                    generateHouseHoldMap();
                return _householdMap;
            }
        }

        private static void generateHouseHoldMap()
        {
            _householdMap=new Dictionary<int,int>();
            StreamReader sr = new StreamReader(Properties.Settings.Default.GFKFileDirectory + "\\HhdId.map");
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Length > 0)
                    if (line[0] != '*')
                {
                    string [] fields=line.Split('\t');
                    int NielsenNr = int.Parse(fields[0].Substring(1));
                    int TelmarNr = int.Parse(fields[1]);
                    _householdMap.Add(NielsenNr, TelmarNr);
                }
            }
        }

        private static void generateStationMap()
        {
            
            _stationMap=new Dictionary<int, List<StationInfo> >();
            StreamReader sr=new StreamReader(Properties.Settings.Default.GFKFileDirectory+"\\gfkstats.dat");
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Length > 0)
                    if (line[0] != '*')
                    {
                        string[] fields = line.Split(',');
                        
                        int NielsenNr = int.Parse(fields[0]);
                        int TelmarNr = int.Parse(fields[1]);
                        bool OtherViewing = (fields[2].Trim() == "1");
                        bool IsTotalViewing = (fields[2].Trim() == "2");
                        bool IsTotalScreenUsage = (fields[2].Trim() == "3");
                        if (!_stationMap.ContainsKey(NielsenNr))
                            _stationMap[NielsenNr] = new List<StationInfo>();

                        _stationMap[NielsenNr].Add( new StationInfo() { stationNielsen = NielsenNr, stationTelmar = TelmarNr, isExtraViewing = OtherViewing,
                            isTotalViewing=IsTotalViewing , isTotalScreenUsage=IsTotalScreenUsage ,stationName = fields.Length > 3 ? fields[3] : "Station " + TelmarNr.ToString() });
                    }
            }

        }
        private List< Demographic> Demographics
        {
            get
            {
                if (_demographics == null)
                    readDemsettings();
                return _demographics;
            }
        }
        private bool readDemsettings()
        {
            _demographics = new List<Demographic>();
            string demsfile;
            bool readOk = true;
            demsfile = Properties.Settings.Default.GFFDirectory  + "\\democontrol.dat";
            if (!File.Exists(demsfile))
            {
                throw new Exception("Democontrol.dat file not found in directory " + Properties.Settings.Default.GFFDirectory);
                
            }
            FileStream fs = File.OpenRead(demsfile);
            StreamReader sr = new StreamReader(fs);
            string strline;
            strline = sr.ReadLine();
            int linesread = 0;
            string democonversionError = "";
            while (strline != null && readOk)
            {
                if (strline.Length == 0)
                {
                    strline = sr.ReadLine();
                    continue;
                }
                linesread++;
                Demographic d;
                //11-12: subtracted 1, as people start counting with 1 not 0
                readOk &= (strline.Length >= 65);
                if (readOk)
                {
                     try
                    {
                        d = new Demographic(strline.Substring(0, 8),
                             Convert.ToInt32(strline.Substring(40, 4)) - 1, Convert.ToInt32(strline.Substring(44, 2)),
                             (Demotype)Convert.ToInt32(strline.Substring(46, 1)),yyyymmddToDateTime(strline.Substring(47, 8)),
                             yyyymmddToDateTime(strline.Substring(55, 8)), (DemoCalculation)Convert.ToInt16(strline.Substring(63, 1)),
                             (VectorType)Convert.ToInt16(strline.Substring(64, 1)), strline.Length>=67? strline.Substring(66):"");
                        _demographics.Add(d);
                    }
                    catch (Exception ex)
                    {
                        readOk = false;
                        democonversionError = ex.ToString();
                    }
                }
                strline = sr.ReadLine();
            }
            sr.Close();
            if (!readOk)
                throw new Exception ("Encountered wrong line while processing democontrol.dat, line number " + linesread.ToString()+
                    (String.IsNullOrEmpty(democonversionError)?"":"Error parsing info: ")+democonversionError );

            return readOk;

        }
        public static DateTime yyyymmddToDateTime(string s)
        {
            int day, month, year;
            if (s.Length == 8)
            {
                year = Convert.ToInt16(s.Substring(0, 4));
                month = Convert.ToInt16(s.Substring(4, 2));
                day = Convert.ToInt16(s.Substring(6, 2));
            }
            else
            {
                year = 1900;
                month = 1;
                day = 1;
            }
            return new DateTime(year, month, day);

        }
        public static String DateTimeToyyyymmdd(DateTime d)
        {
            int day, month, year;
            day = d.Day;
            month = d.Month;
            year = d.Year;
            return Convert.ToString(10000 * year + 100 * month + day);

        }

    }
}
