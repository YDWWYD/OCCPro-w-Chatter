using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OnlineCuttingControlProcess
{
    /// <summary>
    /// Interaction logic for DataRecordsChild.xaml
    /// </summary>
    public partial class DataRecordsChild : UserControl
    {
        // Main data record listview
        public class MainDataRecordListView
        {
            public String lineNumb { get; set; }
            public String posX { get; set; }
            public String posY { get; set; }
            public String posZ { get; set; }
            public String torqX { get; set; }
            public String torqY { get; set; }
            public String torqZ { get; set; }
            public String torqS { get; set; }
            public String feedRate { get; set; }
            public String spindleS { get; set; }
            //public String spindleTic { get; set; }
            //public String maxFt { get; set; }
            //public String maxFtO { get; set; }
            //public String maxSpindleTorq { get; set; }
            //public String uk { get; set; }
            //public String yk { get; set; }
            //public String Potentiometer { get; set; }
        }

        // Fa record listview
        public class FaRecordListView
        {
            public String lineNumb { get; set; }
            public String Fa { get; set; }
        }

        // eta1 record listview
        public class eta1RecordListView
        {
            public String lineNumb { get; set; }
            public String eta1 { get; set; }
        }

        // eta2 record listview
        public class eta2RecordListView
        {
            public String lineNumb { get; set; }
            public String eta2 { get; set; }
        }

        // Force record listview
        public class forceCRecordListView
        {
            public String lineNumb { get; set; }
            public String forceC { get; set; }
        }

        // Feedr record listview
        public class feedrCRecordListView
        {
            public String lineNumb { get; set; }
            public String feedrC { get; set; }
        }

        // ForceY record listview
        public class forceYRecordListView
        {
            public String lineNumb { get; set; }
            public String forceY { get; set; }
        }

        // FeedrU record listview
        public class feedrURecordListView
        {
            public String lineNumb { get; set; }
            public String feedrU { get; set; }
        }

        // Constructor
        //
        public DataRecordsChild(string fileIdx, string fileName)
        {
            InitializeComponent();

            // Then set the required info according to the target file name
            switch (fileIdx)
            {
                case "Main":
                    //Console.WriteLine(fileNameTarget);
                    SetMaindataListView();

                    // Load the main data file
                    //
                    if (System.IO.File.Exists(fileName))
                    {
                        var simulatedDataLines = System.IO.File.ReadLines(fileName);

                        foreach (string line in simulatedDataLines)
                        {
                            // Read each line and write to the tracing pane
                            //
                            string[] words = line.Split(' ');

                            DataRecordListView.Items.Add(new MainDataRecordListView()
                            {
                                lineNumb = words[0],
                                posX = words[1],
                                posY = words[2],
                                posZ = words[3],
                                torqX = words[4],
                                torqY = words[5],
                                torqZ = words[6],
                                torqS = words[7],
                                feedRate = words[8],
                                spindleS = words[9]
                                //spindleTic = words[10],
                                //maxFt = words[11],
                                //maxFtO = words[12],
                                //maxSpindleTorq = words[13],
                                //uk = words[14],
                                //yk = words[15],
                                //Potentiometer = words[16]
                            });
                        }
                    }

                    break;
                case "Fa":
                    //Console.WriteLine(fileNameTarget);
                    SetFaListView();

                    // Load Fa data file
                    //
                    if (System.IO.File.Exists(fileName))
                    {
                        var simulatedDataLines = System.IO.File.ReadLines(fileName);

                        int lineCount = 1;
                        char[] delimeter = { '\t', ' ' };

                        foreach (string line in simulatedDataLines)
                        {
                            string[] words = line.Split(delimeter);

                            // Read each line and write to the tracing pane
                            DataRecordListView.Items.Add(new FaRecordListView()
                            {
                                lineNumb = Convert.ToString(lineCount),
                                Fa = words[0]
                            });
                            lineCount++;
                        }
                    }                   
                    break;
                case "Eta1":
                    //Console.WriteLine(fileNameTarget);
                    SetEta1ListView();

                    // Load eta1 data file
                    //
                    if (System.IO.File.Exists(fileName))
                    {
                        var simulatedDataLines = System.IO.File.ReadLines(fileName);

                        int lineCount = 1;
                        char[] delimeter = { '\t', ' ' };

                        foreach (string line in simulatedDataLines)
                        {
                            string[] words = line.Split(delimeter);

                            // Read each line and write to the tracing pane
                            DataRecordListView.Items.Add(new eta1RecordListView()
                            {
                                lineNumb = Convert.ToString(lineCount),
                                eta1 = words[1]
                            });
                            lineCount++;
                        }
                    }                   
                    break;
                case "Eta2":
                    //Console.WriteLine(fileNameTarget);
                    SetEta2ListView();

                    // Load eta2 data file
                    //
                    if (System.IO.File.Exists(fileName))
                    {
                        var simulatedDataLines = System.IO.File.ReadLines(fileName);

                        int lineCount = 1;
                        char[] delimeter = { '\t', ' ' };

                        foreach (string line in simulatedDataLines)
                        {
                            string[] words = line.Split(delimeter);

                            // Read each line and write to the tracing pane
                            DataRecordListView.Items.Add(new eta2RecordListView()
                            {
                                lineNumb = Convert.ToString(lineCount),
                                eta2 = words[2]
                            });
                            lineCount++;
                        }
                    }                   
                    break;
                case "Force":
                    //Console.WriteLine(fileNameTarget);
                    SetForceCListView();

                    // Load data file
                    //
                    if (System.IO.File.Exists(fileName))
                    {
                        var forceCLines = System.IO.File.ReadLines(fileName);

                        int lineCount = 1;
                        char[] delimeter = { '\t', ' ' };

                        foreach (string line in forceCLines)
                        {
                            string[] words = line.Split(delimeter);

                            // Read each line and write to the tracing pane
                            DataRecordListView.Items.Add(new forceCRecordListView()
                            {
                                lineNumb = Convert.ToString(lineCount),
                                forceC = words[1]
                            });
                            lineCount++;
                        }
                    }
                    break;
                case "FeedR":
                    //Console.WriteLine(fileNameTarget);
                    SetFeedrCListView();

                    // Load data file
                    //
                    if (System.IO.File.Exists(fileName))
                    {
                        var feedrCLines = System.IO.File.ReadLines(fileName);

                        int lineCount = 1;
                        char[] delimeter = { '\t', ' ' };

                        foreach (string line in feedrCLines)
                        {
                            string[] words = line.Split(delimeter);

                            // Read each line and write to the tracing pane
                            DataRecordListView.Items.Add(new feedrCRecordListView()
                            {
                                lineNumb = Convert.ToString(lineCount),
                                feedrC = words[0]
                            });
                            lineCount++;
                        }
                    }
                    break;
                case "Force[Y]":
                    //Console.WriteLine(fileNameTarget);
                    SetForceYListView();

                    // Load data file
                    //
                    if (System.IO.File.Exists(fileName))
                    {
                        var forceYLines = System.IO.File.ReadLines(fileName);

                        int lineCount = 1;
                        char[] delimeter = { '\t', ' ' };

                        foreach (string line in forceYLines)
                        {
                            string[] words = line.Split(delimeter);

                            // Read each line and write to the tracing pane
                            DataRecordListView.Items.Add(new forceYRecordListView()
                            {
                                lineNumb = Convert.ToString(lineCount),
                                forceY = words[3]
                            });
                            lineCount++;
                        }
                    }
                    break;
                case "FeedR[U]":
                    //Console.WriteLine(fileNameTarget);
                    SetFeedrUListView();

                    // Load data file
                    //
                    if (System.IO.File.Exists(fileName))
                    {
                        var feedrULines = System.IO.File.ReadLines(fileName);

                        int lineCount = 1;
                        char[] delimeter = { '\t', ' ' };

                        foreach (string line in feedrULines)
                        {
                            string[] words = line.Split(delimeter);

                            // Read each line and write to the tracing pane
                            DataRecordListView.Items.Add(new feedrURecordListView()
                            {
                                lineNumb = Convert.ToString(lineCount),
                                feedrU = words[2]
                            });
                            lineCount++;
                        }
                    }
                    break;

                default:
                    MessageBox.Show("Not developed yet. Index: " + fileIdx,
                                    "Data Records",
                                    MessageBoxButton.OK, MessageBoxImage.Stop);

                    break;
            }
        }

        // Set the main data list view format
        private void SetMaindataListView()
        {
            GridView mainDataGridView = new GridView();
            mainDataGridView.AllowsColumnReorder = true;

            GridViewColumn gvc1 = new GridViewColumn();
            gvc1.DisplayMemberBinding = new Binding("lineNumb");
            gvc1.Header = "Line#";
            gvc1.Width = 60;
            mainDataGridView.Columns.Add(gvc1);

            GridViewColumn gvc2 = new GridViewColumn();
            gvc2.DisplayMemberBinding = new Binding("posX");
            gvc2.Header = "PosX(mm)";
            gvc2.Width = 70;
            mainDataGridView.Columns.Add(gvc2);

            GridViewColumn gvc3 = new GridViewColumn();
            gvc3.DisplayMemberBinding = new Binding("posY");
            gvc3.Header = "PosY(mm)";
            gvc3.Width = 70;
            mainDataGridView.Columns.Add(gvc3);

            GridViewColumn gvc4 = new GridViewColumn();
            gvc4.DisplayMemberBinding = new Binding("posZ");
            gvc4.Header = "PosZ(mm)";
            gvc4.Width = 70;
            mainDataGridView.Columns.Add(gvc4);

            GridViewColumn gvc5 = new GridViewColumn();
            gvc5.DisplayMemberBinding = new Binding("torqX");
            gvc5.Header = "TorqX(Nm)";
            gvc5.Width =80;
            mainDataGridView.Columns.Add(gvc5);

            GridViewColumn gvc6 = new GridViewColumn();
            gvc6.DisplayMemberBinding = new Binding("torqY");
            gvc6.Header = "TorqY(Nm)";
            gvc6.Width = 80;
            mainDataGridView.Columns.Add(gvc6);

            GridViewColumn gvc7 = new GridViewColumn();
            gvc7.DisplayMemberBinding = new Binding("torqZ");
            gvc7.Header = "TorqZ(Nm)";
            gvc7.Width = 80;
            mainDataGridView.Columns.Add(gvc7);

            GridViewColumn gvc8 = new GridViewColumn();
            gvc8.DisplayMemberBinding = new Binding("torqS");
            gvc8.Header = "TorqS(Nm)";
            gvc8.Width = 80;
            mainDataGridView.Columns.Add(gvc8);

            GridViewColumn gvc9 = new GridViewColumn();
            gvc9.DisplayMemberBinding = new Binding("feedRate");
            gvc9.Header = "Feed(mm/min)";
            gvc9.Width = 90;
            mainDataGridView.Columns.Add(gvc9);

            GridViewColumn gvc10 = new GridViewColumn();
            gvc10.DisplayMemberBinding = new Binding("spindleS");
            gvc10.Header = "SpindleS(m/sec)";
            gvc10.Width = 120;
            mainDataGridView.Columns.Add(gvc10);

            //GridViewColumn gvc11 = new GridViewColumn();
            //gvc11.DisplayMemberBinding = new Binding("spindleTic");
            //gvc11.Header = "Spindle Tic(#)";
            //gvc11.Width = 70;
            //mainDataGridView.Columns.Add(gvc11);

            //GridViewColumn gvc12 = new GridViewColumn();
            //gvc12.DisplayMemberBinding = new Binding("maxFt");
            //gvc12.Header = "Max Ft(N)";
            //gvc12.Width = 70;
            //mainDataGridView.Columns.Add(gvc12);

            //GridViewColumn gvc13 = new GridViewColumn();
            //gvc13.DisplayMemberBinding = new Binding("maxFtO");
            //gvc13.Header = "Max FtO(N)";
            //gvc13.Width = 70;
            //mainDataGridView.Columns.Add(gvc13);

            //GridViewColumn gvc14 = new GridViewColumn();
            //gvc14.DisplayMemberBinding = new Binding("maxSpindleTorq");
            //gvc14.Header = "Max SpindleT(Nm)";
            //gvc14.Width = 70;
            //mainDataGridView.Columns.Add(gvc14);

            //GridViewColumn gvc15 = new GridViewColumn();
            //gvc15.DisplayMemberBinding = new Binding("uk");
            //gvc15.Header = "U (FeedR)";
            //gvc15.Width = 60;
            //mainDataGridView.Columns.Add(gvc15);

            //GridViewColumn gvc16 = new GridViewColumn();
            //gvc16.DisplayMemberBinding = new Binding("yk");
            //gvc16.Header = "Y (Force)";
            //gvc16.Width = 60;
            //mainDataGridView.Columns.Add(gvc16);

            //GridViewColumn gvc17 = new GridViewColumn();
            //gvc17.DisplayMemberBinding = new Binding("Potentiometer");
            //gvc17.Header = "Potentiometer";
            //gvc17.Width = 60;
            //mainDataGridView.Columns.Add(gvc17);

            DataRecordListView.View = mainDataGridView;
        }

        // Set the Fa list view format
        private void SetFaListView()
        {
            GridView FaGridView = new GridView();
            FaGridView.AllowsColumnReorder = true;

            GridViewColumn gvc1 = new GridViewColumn();
            gvc1.DisplayMemberBinding = new Binding("lineNumb");
            gvc1.Header = "Line#";
            gvc1.Width = 75;
            FaGridView.Columns.Add(gvc1);

            GridViewColumn gvc2 = new GridViewColumn();
            gvc2.DisplayMemberBinding = new Binding("Fa");
            gvc2.Header = "Fa";
            gvc2.Width = 150;
            FaGridView.Columns.Add(gvc2);

            DataRecordListView.View = FaGridView;
        }

        // Set the Eta1 list view format
        private void SetEta1ListView()
        {
            GridView eta1GridView = new GridView();
            eta1GridView.AllowsColumnReorder = true;

            GridViewColumn gvc1 = new GridViewColumn();
            gvc1.DisplayMemberBinding = new Binding("lineNumb");
            gvc1.Header = "Line#";
            gvc1.Width = 75;
            eta1GridView.Columns.Add(gvc1);

            GridViewColumn gvc2 = new GridViewColumn();
            gvc2.DisplayMemberBinding = new Binding("eta1");
            gvc2.Header = "eta1";
            gvc2.Width = 150;
            eta1GridView.Columns.Add(gvc2);

            DataRecordListView.View = eta1GridView;
        }

        // Set the Eta2 list view format
        private void SetEta2ListView()
        {
            GridView eta2GridView = new GridView();
            eta2GridView.AllowsColumnReorder = true;

            GridViewColumn gvc1 = new GridViewColumn();
            gvc1.DisplayMemberBinding = new Binding("lineNumb");
            gvc1.Header = "Line#";
            gvc1.Width = 75;
            eta2GridView.Columns.Add(gvc1);

            GridViewColumn gvc2 = new GridViewColumn();
            gvc2.DisplayMemberBinding = new Binding("eta2");
            gvc2.Header = "eta2";
            gvc2.Width = 150;
            eta2GridView.Columns.Add(gvc2);

            DataRecordListView.View = eta2GridView;
        }

        // Set the Force (Calculated) list view format
        private void SetForceCListView()
        {
            GridView adjGridView = new GridView();
            adjGridView.AllowsColumnReorder = true;

            GridViewColumn gvc1 = new GridViewColumn();
            gvc1.DisplayMemberBinding = new Binding("lineNumb");
            gvc1.Header = "Line#";
            gvc1.Width = 75;
            adjGridView.Columns.Add(gvc1);

            GridViewColumn gvc2 = new GridViewColumn();
            gvc2.DisplayMemberBinding = new Binding("forceC");
            gvc2.Header = "Force";
            gvc2.Width = 150;
            adjGridView.Columns.Add(gvc2);

            DataRecordListView.View = adjGridView;
        }

        // Set the FeedR list view format
        private void SetFeedrCListView()
        {
            GridView adjGridView = new GridView();
            adjGridView.AllowsColumnReorder = true;

            GridViewColumn gvc1 = new GridViewColumn();
            gvc1.DisplayMemberBinding = new Binding("lineNumb");
            gvc1.Header = "Line#";
            gvc1.Width = 75;
            adjGridView.Columns.Add(gvc1);

            GridViewColumn gvc2 = new GridViewColumn();
            gvc2.DisplayMemberBinding = new Binding("feedrC");
            gvc2.Header = "FeedR";
            gvc2.Width = 150;
            adjGridView.Columns.Add(gvc2);

            DataRecordListView.View = adjGridView;
        }

        // Set the Force[Y] list view format
        private void SetForceYListView()
        {
            GridView adjGridView = new GridView();
            adjGridView.AllowsColumnReorder = true;

            GridViewColumn gvc1 = new GridViewColumn();
            gvc1.DisplayMemberBinding = new Binding("lineNumb");
            gvc1.Header = "Line#";
            gvc1.Width = 75;
            adjGridView.Columns.Add(gvc1);

            GridViewColumn gvc2 = new GridViewColumn();
            gvc2.DisplayMemberBinding = new Binding("forceY");
            gvc2.Header = "Force[Y]";
            gvc2.Width = 150;
            adjGridView.Columns.Add(gvc2);

            DataRecordListView.View = adjGridView;
        }

        // Set the FeedR[U] list view format
        private void SetFeedrUListView()
        {
            GridView adjGridView = new GridView();
            adjGridView.AllowsColumnReorder = true;

            GridViewColumn gvc1 = new GridViewColumn();
            gvc1.DisplayMemberBinding = new Binding("lineNumb");
            gvc1.Header = "Line#";
            gvc1.Width = 75;
            adjGridView.Columns.Add(gvc1);

            GridViewColumn gvc2 = new GridViewColumn();
            gvc2.DisplayMemberBinding = new Binding("feedrU");
            gvc2.Header = "FeedR[U]";
            gvc2.Width = 150;
            adjGridView.Columns.Add(gvc2);

            DataRecordListView.View = adjGridView;
        }
    }
}
