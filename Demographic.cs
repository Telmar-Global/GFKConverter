using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GFKConverter
{
    public enum Demotype { HouseHold = 1, Member = 2, All = 4, Guest = 3, MemberOrGuest = 5 }
    public enum GFKCalculation
    {
        None = 0,CharMapped = 1, BdayToAGE = 2, FixedValue = 3, ComposedBinary = 4, HHOtherMembers = 5,
        NoneWith0toA = 6, MappedValues = 7, MotherReference = 8
    }
    public enum DemoCalculation { DCNone = 0, DCDateOfBrithToAge = 1 }
    public enum VectorType { VTbinary = 0, VTbyte = 1, VTshort = 2, VTLong = 3, VTNonbinary = 4 }
    public struct ValueRanges
    {
        public int from;
        public int untill;
        public int newvalue;
    }
    public struct demoValueranges
    {
        public Demographic d;
        public LinkedList<ValueRanges> valueranges;
    }
    public class GFKConvertInfo
    {
        public string AppliesTo="";
        public int startpos;
        public int length;
        public GFKCalculation calculationtype;
        public int FixedValue; // used with type 3
        public Dictionary<string, List<int>> composingValues; // used with type 4
        public Dictionary<string, List<int>> OtherMemberValues;// used with type 5/8
        public Dictionary<int, int> valueMapping; //used with type 7
        public Dictionary<char, int> CharvalueMapping; //used with type 7
        public Dictionary<string, List<int>> CurrentMemberValues;//used with type 8
        public int MotherRefStartPos;
        public int MotherRefLength;
    }
    public class Demographic : IComparable
    {
        private string demid;
        private int startPos;
        private int length;
        private Demotype demotype;
        private DateTime startDate;
        private DateTime endDate;
        private bool isspecial;
        private bool ispredef;
        private bool hasdayvalues;
        private VectorType vectortype;
        private VectorType forcetype;
        private int filepos;
        private int sizeondisk;
        private string ConvertInfo;
        private List<GFKConvertInfo> gfkconvertInfo;
        private DemoCalculation calculation;
        private LinkedList<demoValueranges> demovalList;

        public Demographic() { }
        public Demographic(string demid, int startPos, int length, Demotype demotype, DateTime startDate, DateTime endDate)
        {
            this.demid = demid;
            this.startPos = startPos;
            this.length = length;
            this.demotype = demotype;
            this.startDate = startDate;
            this.endDate = endDate;
            this.isspecial = false;
            this.ispredef = false;
            this.calculation = DemoCalculation.DCNone;

        }
        public Demographic(string demid, int startPos, int length,
                Demotype demotype, DateTime startDate, DateTime endDate,
                DemoCalculation dc, VectorType forcedType)
        {
            this.demid = demid;
            this.startPos = startPos;
            this.length = length;
            this.demotype = demotype;
            this.startDate = startDate;
            this.endDate = endDate;
            this.isspecial = false;
            this.ispredef = false;
            this.calculation = dc;
            this.forcetype = forcedType;

        }
        public Demographic(string demid, int startPos, int length,
         Demotype demotype, DateTime startDate, DateTime endDate,
         DemoCalculation dc, VectorType forcedType, string CalculationInfo)
        {
            this.demid = demid;
            this.startPos = startPos;
            this.length = length;
            this.demotype = demotype;
            this.startDate = startDate;
            this.endDate = endDate;
            this.isspecial = false;
            this.ispredef = false;
            this.calculation = dc;
            this.forcetype = forcedType;
            this.ConvertInfo = CalculationInfo;
            if (!String.IsNullOrEmpty(CalculationInfo))
                this.gfkconvertInfo = ParseConvertInfo(this.ConvertInfo);
            else
                this.gfkconvertInfo = new List<GFKConvertInfo>();
        }

        private List<GFKConvertInfo> ParseConvertInfo(string p)
        {
            List<GFKConvertInfo> retval = new List<GFKConvertInfo>();
            string[] fields = p.Split('|');
            foreach (string cvinfo in fields)
            {
                retval.Add(ParseConvertInfoByType(cvinfo));
            }
            return retval;
        }

        private GFKConvertInfo ParseConvertInfoByType(string p)
        {

            GFKConvertInfo retval = new GFKConvertInfo();
            string[] fields = p.Split('_');
            retval.AppliesTo = fields[0];
            retval.calculationtype = (GFKCalculation)int.Parse(fields[3]);
            retval.startpos = int.Parse(fields[1]);
            retval.length = int.Parse(fields[2]);
            int i;
            switch (retval.calculationtype)
            {
                case GFKCalculation.CharMapped:
                   retval.CharvalueMapping  = new Dictionary<char, int>();
                    for (i = 4; i < fields.Length; i++)
                    {
                        string[] map = fields[i].Split(' ');
                        retval.CharvalueMapping[map[0][0]] = int.Parse(map[1]);
                    }
                    break;

                case GFKCalculation.ComposedBinary  :
                    retval.composingValues = new Dictionary<string, List<int>>();
                    for (i = 4; i < fields.Length; i++)
                    {
                        string[] map = fields[i].Split(' ');
                        retval.composingValues[map[0]] = ParseValueList(map[1]);
                    }
                    
                    break;
                case GFKCalculation.MotherReference  : // mother requires another member to reference
                    retval.MotherRefStartPos = int.Parse(fields[4]);
                    retval.MotherRefLength = int.Parse(fields[5]);
                    break;
                case GFKCalculation.FixedValue :
                    retval.FixedValue = int.Parse(fields[4]);
                    break;
                case GFKCalculation.HHOtherMembers :
                    retval.OtherMemberValues = new Dictionary<string, List<int>>();
                    for (i = 4; i < fields.Length; i++)
                    {
                        string[] map = fields[i].Split(' ');
                        retval.OtherMemberValues[map[0]] = ParseValueList(map[1]);
                        
                    }
                    break;
                case GFKCalculation.MappedValues :
                    retval.valueMapping = new Dictionary<int, int>();
                    for (i = 4; i < fields.Length; i++)
                    {
                        string[] map = fields[i].Split(' ');
                        List<int> originalvalues = ParseValueList(map[0]);
                        foreach (int x in originalvalues)
                            retval.valueMapping[x] = int.Parse(map[1]);
                    }
                    break;
              }
            return retval;
        }

        private List<int> ParseValueList(string p)
        {
            List<int> retval = new List<int>();
            string[] items = p.Split(',');
            foreach (string valuerange in items)
            {
                string[] range= valuerange.Split('-');
                int from = int.Parse(range[0]);
                int to = from;
                if (range.Length == 2)
                    to = int.Parse(range[1]);
                for (int i = from; i <= to; i++)
                    retval.Add(i);
            }
            return retval;
        }

        public Demographic(string demid, Demotype demotype)
        {
            this.demid = demid;
            this.demotype = demotype;
            this.isspecial = true;
            this.ispredef = false;
            this.startDate = new DateTime(1990, 1, 1);
            this.endDate = new DateTime(3000, 1, 1);
            this.calculation = DemoCalculation.DCNone;
        }
        public Demographic(string demid, DateTime startDate, DateTime endDate)
        {
            this.demid = demid;
            this.isspecial = false;
            this.ispredef = true;
            this.startDate = startDate;
            this.endDate = endDate;
            this.demovalList = new LinkedList<demoValueranges>();
        }

        public DateTime StartDate
        {
            get { return startDate; }
        }
        public DateTime EndDate
        {
            get { return endDate; }
        }

        public bool addDemoRange(Demographic d, int from, int untill, int newvalue)
        {
            if (!ispredef)
                return false;

            ValueRanges vr = new ValueRanges();
            if (this.demovalList.Count == 0)
                this.demotype = d.GetDemoType();
            else if (this.demotype != d.GetDemoType())
                return false;

            vr.from = from;
            vr.untill = untill;
            vr.newvalue = newvalue;
            LinkedListNode<demoValueranges> ld;
            ld = null;
            if (this.demovalList.Count > 0)
                ld = this.demovalList.First;
            while (ld != null)
            {
                if (ld.Value.d == d)
                    break;
                else
                    ld = ld.Next;
            }
            if (ld == null)
            {
                demoValueranges dvr = new demoValueranges();
                dvr.d = d;
                dvr.valueranges = new LinkedList<ValueRanges>();
                dvr.valueranges.AddFirst(vr);
                ld = this.demovalList.AddLast(dvr);

            }
            else
            {
                ld.Value.valueranges.AddLast(vr);
            }
            return true;
        }

        public int getNewval(Demographic d, int value)
        {
            int retval = 0;
            LinkedListNode<demoValueranges> ld;
            ld = this.demovalList.First;
            while (ld != null)
            {
                if (ld.Value.d == d)
                    break;
                else
                    ld = ld.Next;
            }
            if (ld != null)
            {
                //               if (ld.Value.valueranges.Count>0)
                //             {
                LinkedListNode<ValueRanges> dvrnode = ld.Value.valueranges.First;
                while (dvrnode != null)
                {
                    if (dvrnode.Value.from <= value && dvrnode.Value.untill >= value)
                    {
                        retval = dvrnode.Value.newvalue;
                        break;
                    }
                    dvrnode = dvrnode.Next;
                }
                //           }
            }
            return retval;
        }
        public LinkedList<demoValueranges> getDemoValueRanges()
        {
            return this.demovalList;
        }
        public string getMnemonic()
        {

            return (demid);
        }

        public bool validon(DateTime date)
        {
            return (startDate <= date && endDate >= date);
        }
        public int StartPos
        {
            get { return startPos; }
        }
        public int Length
        {
            get { return (length); }
        }
        public Demotype GetDemoType()
        {
            return (demotype);
        }
        public bool IsSpecial()
        {
            return (isspecial);
        }
        public string Demid
        {
            get
            {
                return (demid);
            }
        }
        public bool isPredef
        {
            get { return ispredef; }
            set { ispredef = value; }
        }

        public bool hasDayValues
        {
            get { return hasdayvalues; }
            set { hasdayvalues = value; }
        }
        public VectorType vectorType
        {
            get { return vectortype; }
            set { vectortype = value; }
        }

        public VectorType forceType
        {
            get { return forcetype; }
            set { forcetype = value; }
        }
        public int filePosition
        {
            get { return filepos; }
            set { filepos = value; }
        }
        public int sizeOnDisk
        {
            get { return sizeondisk; }
            set { sizeondisk = value; }
        }
        public List<GFKConvertInfo> GFKConvertInfo
        {
            get { return gfkconvertInfo; }
        }
        public string AppliesTo
        {
            get
            {
                string retval = "";
                foreach (GFKConvertInfo gfkInfo in GFKConvertInfo)
                {
                    retval = retval + gfkInfo.AppliesTo;
                }
                return retval;
            }
        }
        public int convertvalue(int value, DateTime date)
        {
            int retval;
            switch (calculation)
            {
                case DemoCalculation.DCNone:
                    retval = value;
                    break;
                case DemoCalculation.DCDateOfBrithToAge:
                    retval = DateOfBirthToAge(value, date);
                    break;
                default:
                    retval = value;
                    break;
            }
            return retval;
        }
        private int DateOfBirthToAge(int dob, DateTime date)
        {
            // dob should be in yyyymmdd format
            DateTime birthDate = GFKConverter.yyyymmddToDateTime(Convert.ToString(dob));
            if ((birthDate.Month < date.Month) || (birthDate.Month == date.Month &&
                    birthDate.Day <= date.Day))
                return date.Year - birthDate.Year;
            else
                return date.Year - birthDate.Year - 1;
        }
        public int CompareTo(object obj)
        {
            if (this.GetType() != obj.GetType())
            {
                throw (new ArgumentException("Both objects must be of type Demographic"));
            }
            else
            {
                Demographic d2 = (Demographic)obj;
                if (this.demid == d2.demid)
                    return (this.startDate.CompareTo(d2.startDate));
                else
                    return (this.demid.CompareTo(d2.demid));
            }
        }

    }
}
