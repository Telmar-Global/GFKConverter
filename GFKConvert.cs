using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace GFKConverter
{
    public partial class GFKConvert : Form
    {
        public GFKConvert()
        {
            InitializeComponent();
            textGfkDir.Text  = Properties.Settings.Default.GFKFileDirectory;
            txtGFKProcessed.Text=Properties.Settings.Default.GFKProcessedDirectory;
            textIntermediateDir.Text = Properties.Settings.Default.InterMediateFileDirectory;
            textGFFDir.Text = Properties.Settings.Default.GFFDirectory;
            textWTDDir.Text = Properties.Settings.Default.WTDDirectory;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Setings cannot be changed here, please edit config file manually");
            //string newdir=ChangeDirectory(Properties.Settings.Default.GFKFileDirectory, textGfkDir,"Select directory for GFK input files");
            //if (!String.IsNullOrEmpty(newdir))
            //{
            //    Properties.Settings.Default.GFKFileDirectory = newdir;
            //    Properties.Settings.Default.Save();
            //}
        }

        private string ChangeDirectory(string p, TextBox textDir, string dialogText)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.Description = dialogText ;
            dlg.SelectedPath = p;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                textDir.Text = dlg.SelectedPath;
                return dlg.SelectedPath;
            }
            return "";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Seetings cannot be changed here, please edit config file manually");

            //string newdir = ChangeDirectory(Properties.Settings.Default.InterMediateFileDirectory , textIntermediateDir , "Select directory for intermediate GFF files");
            //if (!String.IsNullOrEmpty(newdir))
            //{
            //    Properties.Settings.Default.InterMediateFileDirectory  = newdir;
            //    Properties.Settings.Default.Save();
                
            //}

        }

        private void button3_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Seetings cannot be changed here, please edit config file manually");

            //string newdir = ChangeDirectory(Properties.Settings.Default.GFFDirectory , textGFFDir , "Select directory for GFF files");
            //if (!String.IsNullOrEmpty(newdir))
            //{
            //    Properties.Settings.Default.GFFDirectory  = newdir;
            //    Properties.Settings.Default.Save();

            //}

        }

        private void button4_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Seetings cannot be changed here, please edit config file manually");

            //string newdir = ChangeDirectory(Properties.Settings.Default.WTDDirectory ,textWTDDir , "Select directory for WTD files");
            //if (!String.IsNullOrEmpty(newdir))
            //{
            //    Properties.Settings.Default.WTDDirectory  = newdir;
            //    Properties.Settings.Default.Save();

            //}

        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void textGfkDir_TextChanged(object sender, EventArgs e)
        {
            showAvailableFiles();
        }

        private static bool TryGetFileDate(string fileName, out DateTime fileDate)
        {
            fileDate = DateTime.MinValue;
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
                    try
                    {
                        fileDate = new DateTime(year, 1, 1).AddDays(julianDay - 1);
                        if (fileDate.Year == year)
                            return true;
                    }
                    catch { }
                }
            }
            return false;
        }

        private static void CollectAvailableFiles(string directory, out List<string> fileNames, out List<DateTime> dates)
        {
            fileNames = new List<string>();
            HashSet<DateTime> dateSet = new HashSet<DateTime>();
            foreach (string path in Directory.GetFiles(directory, "*.*"))
            {
                string fileName = Path.GetFileName(path);
                fileNames.Add(fileName);
                DateTime fileDate;
                if (TryGetFileDate(fileName, out fileDate))
                    dateSet.Add(fileDate.Date);
            }
            dates = new List<DateTime>(dateSet);
            dates.Sort();
        }

        public static bool TryGetFilesToProcessForDate(string gfkDirectory, DateTime date, out List<string> filesToProcess)
        {
            filesToProcess = new List<string>();
            List<string> fileNames;
            List<DateTime> dates;
            CollectAvailableFiles(gfkDirectory, out fileNames, out dates);

            string dateText = date.ToString("dd-MM-yyyy");
            bool dateFound = false;
            foreach (DateTime availableDate in dates)
            {
                if (availableDate.ToString("dd-MM-yyyy") == dateText)
                {
                    dateFound = true;
                    break;
                }
            }
            if (!dateFound)
                return false;

            foreach (string fileName in fileNames)
            {
                DateTime fileDate;
                if (TryGetFileDate(fileName, out fileDate) && fileDate.Date == date.Date)
                    filesToProcess.Add(Path.Combine(gfkDirectory, fileName));
            }
            return true;
        }

        private void showAvailableFiles()
        {
            listFiles.Items.Clear();
            lbDates.Items.Clear();
            try
            {
                List<string> fileNames;
                List<DateTime> dates;
                CollectAvailableFiles(textGfkDir.Text, out fileNames, out dates);
                foreach (string fileName in fileNames)
                    listFiles.Items.Add(fileName);
                foreach (DateTime date in dates)
                    lbDates.Items.Add(date.ToString("dd-MM-yyyy"));
            }
            catch { }
        }

        private void SelectFilesForDate(DateTime date)
        {
            listFiles.ClearSelected();
            for (int i = 0; i < listFiles.Items.Count; i++)
            {
                string listFileName = listFiles.Items[i].ToString();
                DateTime fileDate;
                if (TryGetFileDate(listFileName, out fileDate) && fileDate.Date == date.Date)
                    listFiles.SetSelected(i, true);
            }
        }

        private List<string> GetSelectedFilesToProcess()
        {
            List<string> filesToProcess = new List<string>();
            foreach (string fileName in listFiles.SelectedItems)
            {
                filesToProcess.Add(Path.Combine(textGfkDir.Text, fileName));
            }
            return filesToProcess;
        }

        private void cmdProcessToIntermediate_Click(object sender, EventArgs e)
        {
            List<string> filesToProcess = GetSelectedFilesToProcess();
            GFKBatchConverter.Convertfiles(filesToProcess, true);
            showAvailableFiles();
            return;
        }

        private void btnGFKProcessed_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Setings cannot be changed here, please edit config file manually");

            //string newdir = ChangeDirectory(Properties.Settings.Default.GFKProcessedDirectory ,txtGFKProcessed , "Select directory for GFK processed files");
            //if (!String.IsNullOrEmpty(newdir))
            //{
            //    Properties.Settings.Default.GFKProcessedDirectory  = newdir;
            //    Properties.Settings.Default.Save();
            //}

        }

        private void button1_Click_1(object sender, EventArgs e)
        {

        }

        private void btnGFKProcessed_Click_1(object sender, EventArgs e)
        {

        }

        private void listFiles_Click(object sender, EventArgs e)
        {
            txtInfo.Text = "Hallo";
        }

        private void lbDates_Click(object sender, EventArgs e)
        {
            if (lbDates.SelectedItem == null)
                return;

            DateTime date;
            if (!DateTime.TryParseExact(lbDates.SelectedItem.ToString(), "dd-MM-yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out date))
                return;

            string julianDate = date.ToString("yy") + date.DayOfYear.ToString("000");
            txtInfo.Text = julianDate;

            SelectFilesForDate(date);

            string fileExtension = "0" + julianDate.Substring(0, 2);
            string fileName = "STAMDEF" + julianDate.Substring(julianDate.Length - 3) + "." + fileExtension;
            string fullPath = Path.Combine(Properties.Settings.Default.GFKFileDirectory, fileName);
            if (File.Exists(fullPath))
            {
                txtInfo.Text = fullPath;
                PopulateDemoTree(fullPath);
            }
            else
            {
                txtInfo.Text = "No file found";
                tvDemo.Nodes.Clear();
            }
        }

        private static string Unquote(string value)
        {
            if (value == null)
                return "";
            return value.Trim().Trim('"').Trim();
        }

        private void PopulateDemoTree(string stamdefPath)
        {
            tvDemo.BeginUpdate();
            try
            {
                tvDemo.Nodes.Clear();
                string[] lines = File.ReadAllLines(stamdefPath);
                for (int i = 3; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (line.Length == 0)
                        continue;

                    string[] fields = line.Split(';');
                    if (fields.Length < 7)
                        continue;

                    string level1 = Unquote(fields[1]);
                    string level2 = Unquote(fields[2]);
                    string level3 = Unquote(fields[6]);

                    if (level1.Length == 0)
                        continue;
                    if (level2.Length == 0)
                        continue;

                    TreeNode node1 = tvDemo.Nodes[level1];
                    if (node1 == null)
                        node1 = tvDemo.Nodes.Add(level1, level1);

                    TreeNode node2 = node1.Nodes[level2];
                    if (node2 == null)
                        node2 = node1.Nodes.Add(level2, level2);

                    if (level3.Length > 0 && node2.Nodes[level3] == null)
                        node2.Nodes.Add(level3, level3);
                }
            }
            finally
            {
                tvDemo.EndUpdate();
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }
    }
}
