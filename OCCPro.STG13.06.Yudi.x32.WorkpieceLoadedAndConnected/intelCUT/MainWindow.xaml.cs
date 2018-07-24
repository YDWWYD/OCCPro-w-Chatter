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
using Microsoft.Win32;

// using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;

using System.Windows.Threading;


// Test for CNC data read --->> delete it later
using System.Threading;

// Test  --->> delete it later
using System.Text.RegularExpressions;
using System.Windows.Controls.DataVisualization.Charting;
using System.Diagnostics;


namespace OnlineCuttingControlProcess
{
    /**********************************************************************************************************/
    /* VMC - Pro    Virtual Machining Control Program
    /*
    /* Description : This process connects to the specified CNC, so be able to read realtime CNC data like
    /*    X, Y, Z positions, Feed rate etc. and to send the control data to CNC like feed rate etc. to be able
    /*    take a neccessary action if a failure is detected or an unneccessary situation is occured relying on
    /*    the input data which is prepared before and loaded to the the process.   
    /*
    /* History:
    /* - Corr06 : Tracing is almost completed, that is:
    /*     - Tracks the simulated input data line by line and ready to take the needed action (to be implemented)
    /*     - Problem : - can not send data to CNC (tested using CNC simulator, will be tested with the real CNC)
    /*                 - does not work any other PC, requires CNC setup, to be done
    /*                
    /* - VMCPro.BCKUP.13.Corr07.RealCodeMerge.06.process.Trial
    /*                   
    /* - VMCPro.STG01.00 : The first stage is completed, it covers:
    /*                      - Configuration setup is completed, this datafill is mandatory to be able to run processes
    /*                      - Program communicates with CNC at Realtime, means reads data and control its function
    /*                      - All background threads and tasks are organized to prevent any error for start/stop
    /*                      - While running, there was an out of memory error, it is prevented to record only 600
    /*                      - Every 600 records are processed and deleted. (Recording has not been developed yet)
    /*                      - Tool Breakage is ready to be tested
    /*
    /* - VMCPro.STG02.00 : Tracking achitecture is changed, so it does not stop while tracking:
    /*                01 : Recording all data, no problem for memeory and time
    /*                02 : Safely disconnecting is added.
    /*                03 : Recording is seperated like MainData, Fa, et1, eta2 etc. and some enhencements
    /*
    /* - VMCPro.STG03.07 : Kalman Fildering merging is comleted
    /*
    /* - VMCPro.STG04    : Recorded files can be opened for either txt view or graph view.
    /*
    /* - VMCPro.STG05    : TEST MODE, Runs with only real-time recorded data
    /*
    /* - VMCPro.STG06    : Architecture change for Adaptive Control. It controls feed rate and spindle speed 
    /*                     from LSV2 class
    /*
    /* - VMCPro.STG06.MON: Real-time monitoring to display graphs for Power, Spindle torque, Feed Rate etc. 
    /*                   
    /* - VMCPro.STG07    : Updates 
    /*                     - Adding a Dependent selection for TEST MODE option to test processes with Simulated data       
    /*                       but without CNC real-time data, instead use recorded data for it.
    /*                     - Adding Compare selection for Data Records - Graph to compare recorded data as graph                  
    /*                     - Improvement for Recording, Adaptive control & Tool Breakage tests are done.                  
    /*                   
    /* - VMCPro.STG08    : Customization will be morged.
    /*                     - Recording files will be the same date       
    /*                            
    /* - RMCPro.STG08.21 : Beta Version, first trial
    /*                     - Deniz added his all enhencements and finilized adaptive and tool breake code       
    /*                     - All error controls for configuration setup is added.       
    /*                            
    /* - RMCPro.STG09    : Beta Version, NEW Tracking view 
    /*                     - A trial is done successfully, but most of the parts are messy, need a new version                        
    /*                     - Plotting is done in LSV2, seems cause a problem while CNC testing, need a Thread                           
    /*                           
    /* - RMCPro.STG10    : NEW Tracking view (simplified version of the code) 
    /*                     - A new Thread is created to plot real-time Fa, eta1, FtO and FeedR                        
    /*                           
    /* - RMCPro.STG11    : NEW TEST MODE view (simplified version of the process) 
    /*                     - It made unique for all modes, NORMAL, TEST MODE Standalone, TEST MODE Dependent                       
    /*                       Run that as:  
    /*                       - Load test file first     
    /*                       - Next, Setup Configuration    
    /*                       - Finally run it     
    /*
    /* - RMCPro.STG12    : NEW Recording (simplified version of the process) 
    /*                     - Before, there were so many recorded files and this may confuse users. Now, there                       
    /*                       will be only 3 files for every run:
    /*                       - <Date>_<Time>_MainD_OCC       will contain measured data from CNC     
    /*                       - <Date>_<Time>_CalcD_OCC       will contain calculated data like FeedR, Force, Force[Y], FeedR[U] etc.      
    /*                       - <Date>_<Time>_ToolB_OCC       will contain calculated data like Fa, eta1, eta2      
    /*                                                          
    /* - OCCPro.STG13    : Generate executable file (Compile in Release mode) & Workpiece Geo  
    /*                     - Executable file name intelCUT with Version 01                      
    /*                     - Then Workpiece work is being done                    
    /*                      
    /*
    /**********************************************************************************************************/

    // This is for OpenGLUserControlHost
    internal delegate void PointSelectionHandler(object sender, RoutedEventArgs e);

    /******************************************************************************************************/
    /*     Enums - Generals                                
    /******************************************************************************************************/
    //
    public enum KalmanMatrixName { X, Y, Z };
    public enum KalmanMatrixType { A, P, Q, H, R, K };
    public enum toolBreakageActionType { alarmAndStop, alarmAndContinue, alarmAndDelayStop };

    // For Visual Cutting Enums
    internal enum CoordinateSystem { LeftHanded, RightHanded };
    internal enum ChartOperation3D { ZoomToExtents, ZoomIn, ZoomOut, PanLeft, PanRight, PanUp, PanDown };

    // Check whether we need them or not, if not, DELETE
    public enum graphCtype { Fa, eta1, eta2 };
    public enum graphTtype { FeedR, Force, FeedR_U, Force_Y, Kt_Upd, F_Sim, F_Mean };

    /******************************************************************************************************/
    /*     Main Window Classe                              
    /******************************************************************************************************/
    //
    public partial class MainWindow : Window
    {
        /******************************************************************************************************/
        /*     Data Line Record Classes                              
        /******************************************************************************************************/
        //
        // Input data Line Record, to be able to trace the CNC and take the requiredd action
        public class InputDataLine
        {
            public String lineNumb { get; set; }
            public String GFileNumb { get; set; }
            public String posX { get; set; }
            public String posY { get; set; }
            public String posZ { get; set; }
            public String feedRate { get; set; }
            public String TFNC { get; set; }
            public String TFA { get; set; }
            public String TFB { get; set; }
            public String TFG { get; set; }
            public String coeffKt { get; set; }
            public String force { get; set; }
        }

        //******************************************************************************************************
        //*     Enums                                
        //******************************************************************************************************
        //
        public enum CNCType { None, Heidenhain, Fanuc, Siemens };
        private enum ConnectionStatus { NoConnection, Ready, CONNECTING, CONNECTED, WAITING, ERROR, UpdateFromControl };

        /******************************************************************************************************/
        /*     Definitions                                
        /******************************************************************************************************/
        // General
        public CNCType cncType = CNCType.None; // CNC type like Heidenhain, Fanuc etc.
        public string host = null; // CNC IP Address

        // Running of the process depens on the following items
        public bool isCNCtypeEntered = false; // Check whether it is entered, for configuration
        public bool isConnected = false; // Check whether it is entered, for configuration
        public bool isInputDataLoaded = false; // Check whether it is entered, for configuration
        public bool isToolGeoDefined = false; // Check whether it is entered, for configuration

        // Added while Heidenhain CNC connections --------------------------------------------------------------
        public bool isStoped = false; // Blinkling control for displaying 'Connecting' message
        private IntPtr hPort = IntPtr.Zero;
        private Int32 channelCount = 9; // Currently this number of channels are used, may be extended to 16

        private string ipAddress;
        private bool isLoggedIn = false;
        private HeidenhainDNC dncConnection;

        public Thread HeidenhainGetDataThread;

        // Added to get the real-time data from CNC ------------------------------------------------------------
        private bool isRunning = false; // it will be used later to control enabling a ny changes while running

        public double CNCpositionX = 0;
        public double CNCpositionY = 0;
        public double CNCpositionZ = 0;
        public double CNCfeedRate = 0;
        public double CNCspindleSpeed = 0;
        public bool stopCNCDataDisplaying = false;
        public Thread CNCdataReadingThread;

        // Timer Watch Dog
        public Thread timerWatchDogThread;
        private int timerCounter = 0;

        // Tool geometry values
        public double toolDiameter = 0;
        //public double toolLenght = 0;
        public double toolTeeth = 0;
        
        // Added to update the data at CNC -----------------------------------------------------------------------
        //private int feedRatePercentCHECK = 100; // Represent %, so means no change
        private int spindleSpdPercent = 100; // to compare with the new, if the same, no take no action
        private int updatedFeedRate = 0; // Test
        private int updatedSpeed = 0; // test
        private int updatedRapid = 0; // test

        //private bool stopCNCdataUpdating = false;
        //public Task CNCdataUpdatingTask = null;

        // Tracking the Input data with the real-time CNC data -------------------------------------------------
        public Thread trackingInputDataThread;
        public double trackTolerans = 0.1;

        // Added while workpiece development
        private bool isWorkpieceGeometryLoaded = false;
        private bool isLoadingEnded = false;
        private bool isViewingEnded = false;

        // Added for Output data folder definition
        private static string dataOutputFolderParent = AppDomain.CurrentDomain.BaseDirectory;

        // Tool Breakage
        public bool stopErrorBlinking = false;

        // This controls test mode, if true no configuration settings will ne enabled
        private bool isTestModeStandaloneOn = false; // This is for STANDALONE control, done here
        private bool isTestModeDependentOn = false; // This is for DEPENDENT control, also done in LSV2

        // Monitoring
        public bool isForceVsDisMonitoring = false;
        private bool isSpinTorqVsDisMonitoring = false;
        private bool isFeedrVsDisMonitoring = false;
        private MonitoringChild myMonitorChild; // Create the child window variable
        private List<double> ForceSimList = new List<double>();
        private List<double> DeltaSimList = new List<double>();
        private List<double> ForceRtmList = new List<double>();

        double simDelta = 0;
        double maxDelta = 480; // For X-Axis extension
        double simForce = 0;
        double rtmForce = 0;
        double prevSimForce = 0;

        // Customization
        public bool currCustomizationStatus = false;

        //private GraphicsChild myFollower; // Create the child window variable
        // New Monitoring Design
        private ToolPathFollower myFollower; // Create the child window variable
        //private GraphicsChild myFollower; // Create the child window variable
        public RealTimeGraphs myFa; // Create the child window variable
        public RealTimeGraphs myEta1; // Create the child window variable
        public RealTimeGraphs myFtO; // Create the child window variable
        public RealTimeGraphs myFeedR; // Create the child window variable

        // Others ---------------------------------------------------------------------------------------------
        //
        
        // Test for CNC data read ---> delete later
        private static CancellationTokenSource cts;

        // Test for timer, later organize it
        DispatcherTimer processTimer;
        DateTime startTime;

        // Loading progress
        private int loadingProgress = 0;

        // Workpiece Visual Cutting
        private XYZDisplayHost xyzDisplayHost = null;

        //******************************************************************************************************
        //
        // MAIN WINDOW DEVELOPMENT
        //
        //
        //******************************************************************************************************
        //
        public MainWindow()
        {
            InitializeComponent();

            // Set process priority to real-time
            Process mainProcess = Process.GetCurrentProcess();
            Console.WriteLine("Main Process: " + mainProcess.ProcessName);
            mainProcess.PriorityClass = ProcessPriorityClass.RealTime;

            // This is for real-time graph plotting. Data comes from LSV2
            LSV2Caller.mainWindow = this;            
            
            // Graph line collow definitions
            Style lineColorOrange = new Style(typeof(LineDataPoint));
            //lineColorOrange.Setters.Add(new Setter(BackgroundProperty, Brushes.DarkOrange));
            lineColorOrange.Setters.Add(new Setter(BackgroundProperty, Brushes.DarkOrange));
            lineColorOrange.Setters.Add(new Setter(TemplateProperty, null));
            Style lineColorRed = new Style(typeof(LineDataPoint));
            lineColorRed.Setters.Add(new Setter(BackgroundProperty, Brushes.Red));
            lineColorRed.Setters.Add(new Setter(TemplateProperty, null));
            Style lineColorGreen = new Style(typeof(LineDataPoint));
            lineColorGreen.Setters.Add(new Setter(BackgroundProperty, Brushes.DarkGreen));
            lineColorGreen.Setters.Add(new Setter(TemplateProperty, null));

            myFollower = new ToolPathFollower();
            FollowerFrame.Content = myFollower;

            myFa = new RealTimeGraphs();
            //myFa.LineChart.Title = "Fa Graphic";
            myFa.LineChart.MinHeight = 199;
            myFa.RtmPlotLineSeries.Title = "Fa   ";
            myFa.XLinearAxis.Title = "Sample Count (1 Tooth Period)";
            FaRealTimeFrame.Content = myFa;

            myEta1 = new RealTimeGraphs();
            //myEta1.LineChart.Title = "eta1 Graphic";
            myEta1.LineChart.MinHeight = 198;
            myEta1.RtmPlotLineSeries.Title = "eta1";
            myEta1.XLinearAxis.Title = "Sample Count";
            myEta1.RtmPlotLineSeries.DataPointStyle = lineColorGreen;
            eta1RealTimeFrame.Content = myEta1;

            myFtO = new RealTimeGraphs();
            //myFtO.LineChart.Title = "FtO Graphic";
            myFtO.LineChart.MinHeight = 199;
            myFtO.RtmPlotLineSeries.Title = "Fp  ";
            myFtO.RtmPlotLineSeries.DataPointStyle = lineColorRed;
            myFtO.YLinearAxis.Title = "Force (N)";
            FtORealTimeFrame.Content = myFtO;

            myFeedR = new RealTimeGraphs();
            //myFeedR.LineChart.Title = "Feed Rate Graphic";
            myFeedR.LineChart.MinHeight = 199;
            myFeedR.RtmPlotLineSeries.Title = "Feed";
            myFeedR.YLinearAxis.Title = "Feed Rate (mm/min.)";
            myFeedR.RtmPlotLineSeries.DataPointStyle = lineColorOrange;
            FeedrRealTimeFrame.Content = myFeedR;

            // Window resize according to the window size - This is TEMPORARY Solution.
            // Once you manage to resize all items in the window, you may change this
            if (SystemParameters.PrimaryScreenHeight <= this.Height) this.Height = SystemParameters.PrimaryScreenHeight * 0.95;
            if (SystemParameters.PrimaryScreenWidth <= this.Width) this.Width = SystemParameters.PrimaryScreenWidth * 0.98;
        }

        //// This is to prevent any exception, now it is not used, but if required, put it to the main window activation
        //private void Window_Activated(object sender, EventArgs e)
        //{
        //    tabControlMenu.SelectedIndex = 0;

        //}

        //******************************************************************************************************
        //
        // MENU DEVELOPMENT
        //
        //******************************************************************************************************
        //
        //******************************************************************************************************
        // Menu: Configuration, SubMenu: CNC type 
        //
        //******************************************************************************************************
        //
        private void heidenhainCncType(object sender, RoutedEventArgs e)
        {
            if (!isConnected)
            {
                cncType = CNCType.Heidenhain;
                isCNCtypeEntered = true;
                cncTypeButton.Content = "HEIDENHAIN";
                cncTypeButton.Background = Brushes.LightGreen;

                // This is necessay, in case connection button shows an error message
                connectionCncButton.Content = "OFF";
                connectionCncButton.Background = Brushes.LightGray;
            }
            else // already connected, keep the previous CNC type
            {
                string dummy = cncTypeButton.Content.ToString();
                cncTypeButton.Content = "ALREADY CONNECTED";
                cncTypeButton.Background = Brushes.LightPink;
                DoEvents();

                Thread.Sleep(1000);
                cncTypeButton.Content = dummy;
                cncTypeButton.Background = Brushes.LightGreen;
            }
        }

        private void fanucCncType(object sender, RoutedEventArgs e)
        {
            if (!isConnected)
            {
                cncType = CNCType.Fanuc;
                isCNCtypeEntered = true;
                cncTypeButton.Content = "FANUC";
                cncTypeButton.Background = Brushes.LightGreen;

                // This is necessay, in case connection button shows an error message
                connectionCncButton.Content = "OFF";
                connectionCncButton.Background = Brushes.LightGray;
            }
            else // already connected, keep the previous CNC type
            {
                string dummy = cncTypeButton.Content.ToString();
                cncTypeButton.Content = "ALREADY CONNECTED";
                cncTypeButton.Background = Brushes.LightPink;
                DoEvents();

                Thread.Sleep(1000);
                cncTypeButton.Content = dummy;
                cncTypeButton.Background = Brushes.LightGreen;
            }
        }

        private void siemensCncType(object sender, RoutedEventArgs e)
        {
            if (!isConnected)
            {
                cncType = CNCType.Siemens;
                isCNCtypeEntered = true;
                cncTypeButton.Content = "SIEMENS";
                cncTypeButton.Background = Brushes.LightGreen;

                // This is necessay, in case connection button shows an error message
                connectionCncButton.Content = "OFF";
                connectionCncButton.Background = Brushes.LightGray;
            }
            else // already connected, keep the previous CNC type
            {
                string dummy = cncTypeButton.Content.ToString();
                cncTypeButton.Content = "ALREADY CONNECTED";
                cncTypeButton.Background = Brushes.LightPink;
                DoEvents();

                Thread.Sleep(1000);
                cncTypeButton.Content = dummy;
                cncTypeButton.Background = Brushes.LightGreen;
            }
        }

        private void notSpecifiedCncType(object sender, RoutedEventArgs e)
        {
            notSpecifiedCncTypeUtility();
        }

        // This independent proc is required for other usage purposes
        private void notSpecifiedCncTypeUtility()
        {
            if (!isConnected)
            {
                cncType = CNCType.None;
                isCNCtypeEntered = false;
                cncTypeButton.Content = "NOT SPECIFIED";
                cncTypeButton.Background = Brushes.LightGray;

                // This is necessay, in case connection button shows an error message
                connectionCncButton.Content = "OFF";
                connectionCncButton.Background = Brushes.LightGray;
            }
            else // already connected, keep the previous CNC type
            {
                string tempStatus = cncTypeButton.Content.ToString();
                cncTypeButton.Content = "ALREADY CONNECTED";
                cncTypeButton.Background = Brushes.LightPink;
                DoEvents();

                Thread.Sleep(1000);
                cncTypeButton.Content = tempStatus;
                cncTypeButton.Background = Brushes.LightGreen;
            }
        }

        //******************************************************************************************************
        // Menu: Configuration, SubMenu: Connection
        //
        //******************************************************************************************************
        //
        private async void connectionOn(object sender, RoutedEventArgs e)
        {
            if (isCNCtypeEntered)
            {
                // Started to connection.
                isStoped = false;
                connectingDisplayAsync(); // Start blinking with 'Connecting" info
                connectionCncButton.Background = Brushes.LightCyan;

                // Connection dialog window
                var connDialogWindow = new ConnectionDialogWindow();
                connDialogWindow.mainWindow = this; // This is for IP address passing to the main window
                connDialogWindow.ShowDialog();

                // If already connected, no need to continue, LATER change this if some body
                // wants to connect other device
                if (!isConnected)
                {
                    // CNC type is known, IP address is passed from Connection dialog, now try to connect
                    // to the specified CNC
                    //
                    if (host != null)
                    {
                        switch (cncType)
                        {
                            case CNCType.Heidenhain:
                                Console.WriteLine("Heidenhain selected, IP Address: " + host);

                                isConnected = await ConfigureHeidenhainMachineSelection(true);

                                Console.WriteLine("Reading real-time CNC data from LSV2 buffer: for processing: GetHeidenhainData");
                                if (HeidenhainGetDataThread == null) { GetHeidenhainDataThread(); }
                                break;

                            case CNCType.Fanuc:
                                //Console.WriteLine("Fanuc selected, IP Address: " + host);
                                //ConfigureFanucMachineSelection();
                                break;

                            case CNCType.Siemens:
                                //Console.WriteLine("Siemens selected, IP Address: " + host);
                                //ConfigureSiemensMachineSelection();
                                break;

                            case CNCType.None:
                                Console.WriteLine("CNC is NOT selected");
                                UpdateButtonContent(connectionCncButton, "CNC NOT SELECTED");
                                break;
                        }
                    }
                    else
                    {
                        // No host, means connection dialog is closed without pinging
                        isStoped = true;
                        connectionCncButton.Content = "OFF";
                        connectionCncButton.Background = Brushes.LightGray;
                    } // host != null
                }
                else
                {
                    // Already connected, setup connection button to the porevious settings
                    isStoped = true;
                    connectionCncButton.Content = "CONNECTED";
                    connectionCncButton.Background = Brushes.LightGreen;
                } // isConnected              
            }
            else { UpdateButtonContent(connectionCncButton, "CNC NOT SELECTED"); }
        }

        private void connectionOff(object sender, RoutedEventArgs e)
        {
            connectionOffUtility();
        }

        // This independent proc is required for other usage purposes
        private void connectionOffUtility()
        {
            // If already connected, disconnect, otherwise, just set the connection buttons
            if (isConnected)
            {
                //// First. Stop all active processes
                //stopAllActiveProcesses();                
                
                // Close connection firt
                var result = LSV2Caller.CloseConnection(hPort);
                if (!result)
                {
                    Console.WriteLine("Error encountered when disconnecting from port.");
                }

                if (hPort != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(hPort);
                    hPort = IntPtr.Zero;
                }
                               
                switch (cncType)
                {
                    case CNCType.Heidenhain:
                        Console.WriteLine("Disconnect Heidenhain");

                        if (HeidenhainGetDataThread != null) { HeidenhainGetDataThread.Abort(); HeidenhainGetDataThread = null; }
                        dncConnection.CloseConnection();
                        break;

                    case CNCType.Fanuc:
                        break;

                    case CNCType.Siemens:
                        break;

                    case CNCType.None:
                        Console.WriteLine("It is connected to CNC, but CNC type is not selected, Never comes to this stage !!!!!");
                        break;
                }
            }

            isConnected = false;

            connectionCncButton.Content = "OFF";
            connectionCncButton.Background = Brushes.LightGray;
        }
        
        //******************************************************************************************************
        // Menu: Configuration, SubMenu: Simulated Input Data
        //
        //******************************************************************************************************
        //
        private void inputDataLoad(object sender, RoutedEventArgs e)
        {
            // Clear TFNC & TFG stored simulated data in case for reload
            LSV2Caller.ClearDataTFNC();
            LSV2Caller.ClearDataTFG();

            var selectDestFolder = new OpenFileDialog();
            selectDestFolder.Filter = "Text Files (*.txt)|*.txt";
            selectDestFolder.Multiselect = true;
            selectDestFolder.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (selectDestFolder.ShowDialog() == true)
            {
                inputDataFileName.Content = selectDestFolder.FileName;
//                Console.WriteLine(selectDestFolder.FileName);

                // Load the input data file to the tracing pane
                //
                if (System.IO.File.Exists(selectDestFolder.FileName))
                {
                    var simulatedDataLines = System.IO.File.ReadLines(selectDestFolder.FileName);
//                    Console.WriteLine("File exists");

                    // Since the file exists, Clear the listbox first
                    tracingListView.Items.Clear();

                    char[] delimeter = { '\t', ' ' };
                    int lineCount = 1;
                    double deltaX1 = 0, deltaX2 = 0, deltaY1 = 0, deltaY2 = 0, deltaZ1 = 0, deltaZ2 = 0;

                    double maxX = 0, minX = 0, maxY = 0, minY = 0;

                    // Loading progress control
                    int colorCount = 1;
                    int colorStep = 1;
                    loadingProgress = simulatedDataLines.Count() / 100;
                    SimDataTextBox.Text = "              Loading...";
                    myFollower.LineChart.DataContext = null;

                    foreach (string line in simulatedDataLines)
                    {
                        // Read each line and write to the tracing pane
                        //
                        //string[] words = line.Split(' ');
                        string[] words = line.Split(delimeter);

                        try
                        {
                            // Save these coordinates to able to calculate delta distance from first 2 lines
                            if (lineCount == 1) { deltaX1 = Convert.ToDouble(words[0]); deltaY1 = Convert.ToDouble(words[1]); deltaZ1 = Convert.ToDouble(words[2]); }
                            if (lineCount == 2) { deltaX2 = Convert.ToDouble(words[0]); deltaY2 = Convert.ToDouble(words[1]); deltaZ2 = Convert.ToDouble(words[2]); } 

                            tracingListView.Items.Add(new InputDataLine()
                            {
                                lineNumb = Convert.ToString(lineCount),
                                GFileNumb = words[10],
                                posX = words[0],
                                posY = words[1],
                                posZ = words[2],
                                feedRate = words[3],
                                TFNC = words[4],
                                TFA = words[5],
                                TFB = words[6],
                                TFG = words[7],
                                coeffKt = words[8],
                                force = words[9]
                            });

                            // Showing progress
                            if (colorCount == loadingProgress * colorStep) { SimDataProgressBar.Value = colorStep; DoEvents(); colorStep++; }
                            colorCount++;
                        }
                        catch (Exception eh)
                        {
                            // There is a problem with the .txt file, first clear it, then inform.
                            tracingListView.Items.Clear(); //Simulated data cleared
                            ClearToolPathGraphData();

                            MessageBox.Show("Invalid Format: " + eh.Message,
                                            "Simulated Data File",
                                            MessageBoxButton.OK, MessageBoxImage.Stop);
                            SimDataProgressBar.Value = 0;
                            SimDataTextBox.Text = "           NOT LOADED";
                            return;
                        }

                        // Send TFNC & TFG to LSV2 to store there to be processed later
                        LSV2Caller.AddDataToTFNC(Convert.ToDouble(words[4]));
                        LSV2Caller.AddDataToTFG(Convert.ToDouble(words[7]));

                        // Update Follower with the simulated data
                        if (lineCount % 10 == 0)
                        {
                            double tempX = Convert.ToDouble(words[0]);
                            double tempY = Convert.ToDouble(words[1]);

                            myFollower.PathKeyValuePair.Add(new KeyValuePair<double, double>(tempX, tempY));

                            // Find Max and Min values of graph scale
                            if (tempX > maxX) maxX = tempX; if (tempX < minX) minX = tempX;
                            if (tempY > maxY) maxY = tempY; if (tempY < minY) minY = tempY;

                        }
                        lineCount++;
                    }

                    myFollower.XLinearAxis.Maximum = maxX + 25;
                    myFollower.XLinearAxis.Minimum = minX - 25;
                    myFollower.YLinearAxis.Maximum = maxY + 25;
                    myFollower.YLinearAxis.Minimum = minY - 25;

                    myFollower.LineChart.DataContext = myFollower.MonitoringPlot;

                    // TFG/TFNC is stored for LSV2 proccess, set the count of TFG/TFNC
                    LSV2Caller.SetSimDataCount();

                    // Calculate delta distance for each step
                    double delta = Math.Sqrt(Math.Pow((Math.Abs(deltaX1) - Math.Abs(deltaX2)), 2) 
                                   + Math.Pow((Math.Abs(deltaY1) - Math.Abs(deltaY2)), 2) 
                                   + Math.Pow((Math.Abs(deltaZ1) - Math.Abs(deltaZ2)), 2));
                    LSV2Caller.SetDeltaDistance(delta);

                    // Note: Maybe in the future, we may make these to bool as only one
                    isInputDataLoaded = true;
                    LSV2Caller.SetSimInputDataLoaded(isInputDataLoaded); // Let LSV2 about Simulated data loaded or not for Test Mode: Standalone

                    inputDataButton.Content = "LOADED"; // This is for previous design, DELETE later
                    inputDataButton.Background = Brushes.LightGreen; // This is for previous design, DELETE later
                    SimDataTextBox.Text = "               LOADED";
                }
            }
        }

        private void inputDataUnload(object sender, RoutedEventArgs e)
        {
            inputDataUnloadUtility();
        }

        // This independent proc is required for other usage purposes
        private void inputDataUnloadUtility()
        {
            // Note: Maybe in the future, we may make these to bool as only one
            isInputDataLoaded = false;
            LSV2Caller.SetSimInputDataLoaded(isInputDataLoaded); // Let LSV2 about Simulated data loaded or not for Test Mode: Standalone

            // Clear the data and folder content
            tracingListView.Items.Clear();
            inputDataFileName.Content = "                                                      ";
            ClearToolPathGraphData();

            inputDataButton.Content = "UNLOADED"; // This is for previous design, DELETE later
            inputDataButton.Background = Brushes.LightGray; // This is for previous design, DELETE later
            SimDataProgressBar.Value = 0;
            SimDataTextBox.Text = "             UNLOADED";
        }

        private void ClearToolPathGraphData()
        {
            myFollower.PathKeyValuePair.Clear(); // Graph data cleared
            myFollower.ToolKeyValuePair.Clear(); // Graph data cleared
            myFollower.LineChart.DataContext = null;
            myFollower.LineChart.DataContext = myFollower.MonitoringPlot;
        }

        //******************************************************************************************************
        // Menu: Configuration, SubMenu: Tool Geometry
        //
        //******************************************************************************************************
        //
        private void toolGeoSetting(object sender, RoutedEventArgs e)
        {
            // Tool Geometry dialog window
            var toolGeoDialogWindow = new ToolGeoDialogWindow();
            toolGeoDialogWindow.mainWindow = this; // This 
            toolGeoDialogWindow.ShowDialog();

            // Check if 3 field is value bigger than zero, then allow running.
            if (Convert.ToDecimal(ToolDiameterTextBox.Text) != 0 
                && Convert.ToDecimal(ToolLenghtTextBox.Text) != 0
                && Convert.ToDecimal(ToolNumbOfTeethTextBox.Text) != 0)
            {
                isToolGeoDefined = true;
            }
            else { isToolGeoDefined = false; }

            LSV2Caller.SetToolDiameter(Convert.ToDouble(ToolDiameterTextBox.Text));
            //toolLenght = Convert.ToDouble(ToolLenghtTextBox.Text);
            LSV2Caller.SetToolLenght(Convert.ToDouble(ToolLenghtTextBox.Text));
            LSV2Caller.SetToolTeethNub(Convert.ToInt16(ToolNumbOfTeethTextBox.Text));
        }

        //******************************************************************************************************
        // Menu: Options, SubMenu: Load Workpiece geometry
        //
        // Opens the dialog for selecting the workpiece STL file
        // Once the file is selected and the dialog is closed, the workpiece geometry is loaded and displayed
        //******************************************************************************************************
        //
        private async void workPieceGeoLoad(object sender, RoutedEventArgs e)
        {
            var openWorkpieceGeometryFile = new Microsoft.Win32.OpenFileDialog();
            openWorkpieceGeometryFile.Filter = "STL files (*.stl)|*.stl";
            openWorkpieceGeometryFile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var dialogResult = openWorkpieceGeometryFile.ShowDialog();

            if (dialogResult == true)
            {
                // Before loading workpiece geo stl file, switch "Tool Path follower" to "XYZDisplayHost" view
                // Therefore, clear the existing view from the frame
                FollowerFrame.ContentTemplate = null;
                // Make Nagigation Bar hidden, we do not want to see it                
                FollowerFrame.NavigationUIVisibility = NavigationUIVisibility.Hidden; 

                // Next, set the workpiece geo view
                xyzDisplayHost = new XYZDisplayHost();

                xyzDisplayHost.VerticalAlignment = VerticalAlignment.Stretch;
                xyzDisplayHost.HorizontalAlignment = HorizontalAlignment.Stretch;
                xyzDisplayHost.SetAxesRange();
                xyzDisplayHost.ZoomToExtents();

                // Then load workpiece geo 
                //
                isLoadingEnded = false;
                LoadingDisplayAsync();
                await Task.Run(() => { isWorkpieceGeometryLoaded = xyzDisplayHost.LoadWorkpieceGeometry(openWorkpieceGeometryFile.FileName); });
                isLoadingEnded = true;
                
                if (isWorkpieceGeometryLoaded)
                {
                    // Then take the required action
                    FollowerFrame.Content = xyzDisplayHost;

                    // Viewing workpiece to the display
                    //
                    isViewingEnded = false;
                    xyzDisplayHost.SetAxesRange();
                    xyzDisplayHost.ZoomToExtents();
                    ViewingDisplayAsync();
                    await Task.Delay(5000);
                    isViewingEnded = true;
                    
                    workPieceGeoButton.Content = "LOADED";
                    workPieceGeoButton.Background = Brushes.LightGreen;
                    WorkPieceTextBox.Text = "               LOADED";
                    WorkPieceProgressBar.Foreground = Brushes.LightGreen;
                }
                else
                {
                    //  await this.ShowMessageDialog("Error", "Encountered error loading data from STL file.  Please check output log.");
                    Console.WriteLine("Encountered error loading data from STL file");
                }

                //  await CloseProgressDialog(controller);
            }
        }

        private void workPieceGeoUnload(object sender, RoutedEventArgs e)
        {
            // Switch back to "Tool Path follower"
            FollowerFrame.Content = myFollower;
            isWorkpieceGeometryLoaded = false;

            xyzDisplayHost.Close();
            xyzDisplayHost = null;
            xyzDisplayHost = new XYZDisplayHost();

            workPieceGeoButton.Content = "UNLOADED";
            workPieceGeoButton.Background = Brushes.LightGray;
            WorkPieceProgressBar.Value = 0;
            WorkPieceTextBox.Text = "             UNLOADED";
        }


        // WorkPiece mouse events
        //
        private void WorkPieceBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            WorkPieceBorder.Background = Brushes.Transparent;
        }

        private void WorkPieceBorder_MouseMove(object sender, MouseEventArgs e)
        {
            var bc = new BrushConverter();
            WorkPieceBorder.Background = (Brush)bc.ConvertFrom("#FFBEE6F9");
        }

        // Proc: Displays "Loading..." to WorkPieceTextBox
        //
        private async void LoadingDisplayAsync()
        {
            WorkPieceTextBox.Text = "              Loading...";

            for (; ; )
            {
                for (int k = 1; k < 101; k++)
                {
                    WorkPieceProgressBar.Value = k;
                    await Task.Delay(10);

                    if (isLoadingEnded) { break; }
                }
                WorkPieceProgressBar.Value = 0;
                if (isLoadingEnded) { break; }
            }
            //Console.WriteLine("LoadingDisplayAsync: ENDs");
            WorkPieceProgressBar.Value = 100;
        }

        // Proc: Displays "Viewing..." to WorkPieceTextBox
        //
        private async void ViewingDisplayAsync()
        {
            WorkPieceProgressBar.Value = 0;
            WorkPieceTextBox.Text = "              Viewing...";
            WorkPieceProgressBar.Foreground = Brushes.LightSalmon;

            for (; ; )
            {
                for (int k = 1; k < 101; k++)
                {
                    WorkPieceProgressBar.Value = k;
                    await Task.Delay(10);

                    if (isViewingEnded) { break; }
                }
                WorkPieceProgressBar.Value = 0;
                if (isViewingEnded) { break; }
            }
            //Console.WriteLine("ViewingDisplayAsync: ENDs");
            WorkPieceProgressBar.Value = 100;
        }

        //******************************************************************************************************
        // Menu: Options, SubMenu: Data Recording
        //
        //******************************************************************************************************
        //
        private void dataRecordingOn_Click(object sender, RoutedEventArgs e)
        {
            var selectDestFolder = new CommonOpenFileDialog();
            selectDestFolder.IsFolderPicker = true;
            var dialogResult = selectDestFolder.ShowDialog();

            selectDestFolder.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (dialogResult == CommonFileDialogResult.Ok)
            {
                dataOutputFolderParent = selectDestFolder.FileName;

                // Set directory folder, file name and location like: dataOutputFolderParent\VMCPro_MMddYY_HHmmSS
                LSV2Caller.SetRecordFileDirectory(dataOutputFolderParent + "\\");
                
                // Set Recording on
                LSV2Caller.SetProcessRecording(true);

                outputDataFolderButton.Content = "FOLDER SELECTED";
                outputDataFolderButton.Background = Brushes.LightGreen;
            }           
        }

        private void dataRecordingOff_Click(object sender, RoutedEventArgs e)
        {
            // Set Recording off
            LSV2Caller.SetProcessRecording(false);

            outputDataFolderButton.Content = "OFF";
            outputDataFolderButton.Background = Brushes.LightGray;
        }

        private void RecordNameButton_Click(object sender, RoutedEventArgs e)
        {
            RecordNameTextBox.Text = "OCC";
        }

        //******************************************************************************************************
        // Menu: Options, SubMenu: Kalman Filtering
        //
        //******************************************************************************************************
        //
        private void KalmanAxisX_ON_Click(object sender, RoutedEventArgs e)
        {
            // Call Kalman Filterin Dialog window to enter the matrix values
            var kalmanFilteringDialogWindow = new KalmanFilteringDialogWindow(KalmanMatrixName.X);
            kalmanFilteringDialogWindow.ShowDialog();

            // If the dialog is closed without entering data, just return without changing the status
            if (kalmanFilteringDialogWindow.dialogClosedWithoutData) { return; }

            LSV2Caller.SetKalmanAxisXActive(true);

            KalmanFilteringButton.Content = KalmanFilteringStatusText();
            KalmanFilteringButton.Background = Brushes.LightGreen;        
        }

        private void KalmanAxisX_OFF_Click(object sender, RoutedEventArgs e)
        {
            LSV2Caller.SetKalmanAxisXActive(false);

            KalmanFilteringButton.Content = KalmanFilteringStatusText();
            if (!(LSV2Caller.GetKalmanAxisXActive() || LSV2Caller.GetKalmanAxisYActive() || LSV2Caller.GetKalmanAxisZActive()))
            {
                KalmanFilteringButton.Background = Brushes.LightGray;
            }            
        }

        private void KalmanAxisY_ON_Click(object sender, RoutedEventArgs e)
        {
            // Call Kalman Filterin Dialog window to enter the matrix values
            var kalmanFilteringDialogWindow = new KalmanFilteringDialogWindow(KalmanMatrixName.Y);
            kalmanFilteringDialogWindow.ShowDialog();

            // If the dialog is closed without entering data, just return without changing the status
            if (kalmanFilteringDialogWindow.dialogClosedWithoutData) { return; }

            LSV2Caller.SetKalmanAxisYActive(true);

            KalmanFilteringButton.Content = KalmanFilteringStatusText();
            KalmanFilteringButton.Background = Brushes.LightGreen;        
        }

        private void KalmanAxisY_OFF_Click(object sender, RoutedEventArgs e)
        {
            LSV2Caller.SetKalmanAxisYActive(false);

            KalmanFilteringButton.Content = KalmanFilteringStatusText();
            if (!(LSV2Caller.GetKalmanAxisXActive() || LSV2Caller.GetKalmanAxisYActive() || LSV2Caller.GetKalmanAxisZActive()))
            {
                KalmanFilteringButton.Background = Brushes.LightGray;
            }
        }

        private void KalmanAxisZ_ON_Click(object sender, RoutedEventArgs e)
        {
            // Call Kalman Filterin Dialog window to enter the matrix values
            var kalmanFilteringDialogWindow = new KalmanFilteringDialogWindow(KalmanMatrixName.Z);
            kalmanFilteringDialogWindow.ShowDialog();

            // If the dialog is closed without entering data, just return without changing the status
            if (kalmanFilteringDialogWindow.dialogClosedWithoutData) { return; }

            LSV2Caller.SetKalmanAxisZActive(true);

            KalmanFilteringButton.Content = KalmanFilteringStatusText();
            KalmanFilteringButton.Background = Brushes.LightGreen;
        }

        private void KalmanAxisZ_OFF_Click(object sender, RoutedEventArgs e)
        {
            LSV2Caller.SetKalmanAxisZActive(false);

            KalmanFilteringButton.Content = KalmanFilteringStatusText();
            if (!(LSV2Caller.GetKalmanAxisXActive() || LSV2Caller.GetKalmanAxisYActive() || LSV2Caller.GetKalmanAxisZActive()))
            {
                KalmanFilteringButton.Background = Brushes.LightGray;
            }
        }

        private void KalmanDeactivate_Click(object sender, RoutedEventArgs e)
        {
            LSV2Caller.SetKalmanAxisXActive(false);
            LSV2Caller.SetKalmanAxisYActive(false);
            LSV2Caller.SetKalmanAxisZActive(false);

            KalmanFilteringButton.Content = "DEACTIVATED";
            KalmanFilteringButton.Background = Brushes.LightGray;
        }

        private string KalmanFilteringStatusText()
        {
            string statusText = (string)KalmanFilteringButton.Content;

            if (LSV2Caller.GetKalmanAxisXActive())
            {
                statusText = "X";

                if (LSV2Caller.GetKalmanAxisYActive()) { statusText += ", Y"; }
                if (LSV2Caller.GetKalmanAxisZActive()) { statusText += ", Z"; }
                statusText += "   ACTIVATED";
            }
            else
            {
                if (LSV2Caller.GetKalmanAxisYActive())
                {
                    statusText = "Y";

                    if (LSV2Caller.GetKalmanAxisZActive()) { statusText += ", Z"; }
                    statusText += "   ACTIVATED";
                }
                else
                {
                    if (LSV2Caller.GetKalmanAxisZActive()) { statusText = "Z   ACTIVATED"; }
                    else statusText = "DEACTIVATED";
                }
            }

            return statusText;
        }

        //******************************************************************************************************
        // Menu: Options, SubMenu: TEST MODE
        //
        //******************************************************************************************************
        //
        private void TestOnStandalone_Click(object sender, RoutedEventArgs e)
        {
            // If running, mean NORMAL mode, TEST MODE is NOT allowed to be activated
            if (isRunning) { UpdateButtonContent(tracingStatusButton, "ALREADY RUNNING"); return; }
            
            // If it is already TEST MODE ON, no need to take any action
            if (isTestModeStandaloneOn) { UpdateButtonContent(TabItemTestModeButton, "STANDALONE ALREADY ON"); return; }
            if (isTestModeDependentOn) { UpdateButtonContent(TabItemTestModeButton, "DEPENDENT ALREADY ON"); return; }

            // Load the recorded test data, this will be used as if CNC is sending data
            //
            if (TestOnLoadFile())
            //---------------
            {
                Console.WriteLine("TEST MODE Standalone is running!!!");
                //Console.WriteLine("Total count: " + LSV2Caller.RealTimeTestData.Count);

                // Disable and unset CNCType, Connection and Input Data menu items. Do unset in reverse order
                InputDataMenuItem.IsEnabled = false; // First unset Simulated Input Data
                inputDataUnloadUtility();
                ConnectionMenuItem.IsEnabled = false; // Next unset Connection
                connectionOffUtility();
                CNCTypeMenuItem.IsEnabled = false; // Finally unset CNC Type
                notSpecifiedCncTypeUtility();

                isTestModeStandaloneOn = true; 
                TabItemTestModeButton.Visibility = Visibility.Visible;

                TestOnButton.Content = "STANDALONE";          // This is for previous design, DELETE later
                TestOnButton.Background = Brushes.LightGreen; // This is for previous design, DELETE later
                TestOnTextBox.Text = "           STANDALONE";
            }
        }

        private void TestOnDependent_Click(object sender, RoutedEventArgs e)
        {
            // If running, mean NORMAL mode, TEST MODE is NOT allowed to be activated
            if (isRunning) { UpdateButtonContent(tracingStatusButton, "ALREADY RUNNING"); return; }

            // If it is already TEST MODE ON, no need to take any action
            if (isTestModeDependentOn) { UpdateButtonContent(TabItemTestModeButton, "DEPENDENT ALREADY ON"); return; }
            if (isTestModeStandaloneOn) { UpdateButtonContent(TabItemTestModeButton, "STANDALONE ALREADY ON"); return; }

            // Load the recorded test data, this will be used as if CNC is sending data
            //
            if (TestOnLoadFile())
            //---------------
            {
                Console.WriteLine("TEST MODE Dependent is running!!!");
                //LSV2Caller.SetProcessRunning(true);    //???? For TEST, later make it inline

                //LSV2Caller.SetDependentTestMode(true); //???? For TEST, later make it inline
                isTestModeDependentOn = true;
                TabItemTestModeButton.Visibility = Visibility.Visible;

                TestOnButton.Content = "DEPENDENT";           // This is for previous design, DELETE later
                TestOnButton.Background = Brushes.LightGreen; // This is for previous design, DELETE later
                TestOnTextBox.Text = "            DEPENDENT";
            }
        }

        private bool TestOnLoadFile()
        {
            // Load the recorded test data
            var selectDestFolder = new OpenFileDialog();
            selectDestFolder.Filter = "Text Files (*.txt)|*.txt";
            selectDestFolder.Multiselect = true;
            selectDestFolder.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (selectDestFolder.ShowDialog() == true)
            {
                TestOnButton.Content = "LOADING DATA";  // This is for previous design, DELETE later
                TestOnButton.Background = Brushes.Pink; // This is for previous design, DELETE later
                DoEvents();                             // This is for previous design, DELETE later

                // Load the input data file to the tracing pane
                //
                if (System.IO.File.Exists(selectDestFolder.FileName))
                {
                    var testOnLoadDataLines = System.IO.File.ReadLines(selectDestFolder.FileName);

                    ScopeData tempData;

                    // Loading progress control
                    int colorCount = 1;
                    int colorStep = 1;
                    loadingProgress = testOnLoadDataLines.Count() / 100;
                    TestOnTextBox.Text = "              Loading...";

                    try
                    {
                        foreach (string line in testOnLoadDataLines)
                        {
                            // Read each line and write to the tracing pane
                            //
                            string modLine = line.Replace("\t", " ");
                            char[] Delimeter = { '\t', ' ' };

                            string[] words = modLine.Split(Delimeter);

                            // This is NEW format - Saved from this GUI
                            //
                            tempData.TimeTNC = 0;
                            tempData.PositionX = double.Parse(words[1]);
                            tempData.PositionY = double.Parse(words[2]);
                            tempData.PositionZ = double.Parse(words[3]);
                            tempData.TorqueX = double.Parse(words[4]);
                            tempData.TorqueY = double.Parse(words[5]);
                            tempData.TorqueZ = double.Parse(words[6]);
                            tempData.TorqueS = double.Parse(words[7]);
                            tempData.Feedrate = double.Parse(words[8]);
                            tempData.SpindleSpeed = double.Parse(words[9]);
                            tempData.SpindleTic = 0;
                            tempData.MaxFt = 0;
                            tempData.MaxFtO = 0;
                            tempData.MaxS = 0;
                            tempData.Uk = 0;
                            tempData.Yk = 0;
                            tempData.Potentiometer = 0;
                            tempData.MaxS_LP = 0;
                            tempData.Kt_Updated = 0;
                            tempData.F_Sim = 0;
                            tempData.MeanFt_a = 0;
                            tempData.MeanFtO_a = 0;

                            LSV2Caller.RealTimeTestData.Add(tempData);

                            if (colorCount == loadingProgress * colorStep) { TestOnProgressBar.Value = colorStep; DoEvents(); colorStep++; }
                            colorCount++;
                        }
                    }
                    catch (Exception eh)
                    {
                        MessageBox.Show("Invalid Format: " + eh.Message,
                                        "Test Mode",
                                        MessageBoxButton.OK, MessageBoxImage.Stop);

                        TestOnButton.Content = "OFF";                // This is for previous design, DELETE later
                        TestOnButton.Background = Brushes.LightGray; // This is for previous design, DELETE later
                        TestOnProgressBar.Value = 0;
                        TestOnTextBox.Text = "                   OFF";
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        private void TestOff_Click(object sender, RoutedEventArgs e)
        {
            // Enableable CNCType, Connection and Input Data menu items.
            CNCTypeMenuItem.IsEnabled = true;
            ConnectionMenuItem.IsEnabled = true;
            InputDataMenuItem.IsEnabled = true;

            inputDataFileName.Content = "";
            isTestModeStandaloneOn = false;
            isTestModeDependentOn = false;
            TabItemTestModeButton.Visibility = Visibility.Hidden;

            LSV2Caller.SetDependentTestMode(false); 
            LSV2Caller.RealTimeTestData.Clear();

            if (isRunning) runStopUtility();

            TestOnButton.Content = "OFF";                // This is for previous design, DELETE later
            TestOnButton.Background = Brushes.LightGray; // This is for previous design, DELETE later
            TestOnProgressBar.Value = 0;                   
            TestOnTextBox.Text = "                   OFF"; 
        }

        //******************************************************************************************************
        // Menu: Options, SubMenu: Customization
        //
        //******************************************************************************************************
        //
        private void Customization_Click(object sender, RoutedEventArgs e)
        {
            // Customization dialog window
            var customizationDialogWindow = new CustomizationDialogWindow();            
            customizationDialogWindow.mainWindow = this; // This is for trackTolerans to be updated by this dialog

            // We have to do here, because it can not be done while dialog creation time since main window address
            // is not passed yet.
            customizationDialogWindow.trackingTol = trackTolerans;
            customizationDialogWindow.TrackTolTextBox.Text = trackTolerans.ToString();

            customizationDialogWindow.ShowDialog();

            if (customizationDialogWindow.isCustomizationOn == true)
            {
                CustomizationButton.Content = "ACTIVATED";
                CustomizationButton.Background = Brushes.LightGreen;
            }
            else
            {
                CustomizationButton.Content = "NOT ACTIVATED";
                CustomizationButton.Background = Brushes.LightGray;
            }
        }

        //******************************************************************************************************
        // Menu: View, SubMenu: Graphic Records
        //
        //
        //******************************************************************************************************
        //
        // Single Menu Clicks
        //
        private void SingleGraphFa_Click(object sender, RoutedEventArgs e) { DataRecordsSingleGraph("Fa", 0); }
        private void SingleGraphEta1_Click(object sender, RoutedEventArgs e) { DataRecordsSingleGraph("Eta1", 1); }
        private void SingleGraphEta2_Click(object sender, RoutedEventArgs e) { DataRecordsSingleGraph("Eta2", 2); }
        private void SingleGraphFeedR_Click(object sender, RoutedEventArgs e) { DataRecordsSingleGraph("FeedR", 0); }
        private void SingleGraphForce_Click(object sender, RoutedEventArgs e) { DataRecordsSingleGraph("Force", 1); }
        private void SingleGraphFeedR_U_Click(object sender, RoutedEventArgs e) { DataRecordsSingleGraph("FeedR[U]", 2); }
        private void SingleGraphForce_Y_Click(object sender, RoutedEventArgs e) { DataRecordsSingleGraph("Force[Y]", 3); }
        private void SingleGraphKtUpd_Click(object sender, RoutedEventArgs e) { DataRecordsSingleGraph("KtUpd", 4); }
        private void SingleGraphF_Sim_Click(object sender, RoutedEventArgs e) { DataRecordsSingleGraph("F_Sim", 5); }
        private void SingleGraphF_Mean_Click(object sender, RoutedEventArgs e) { DataRecordsSingleGraph("F_Mean", 6); }

        private void DataRecordsSingleGraph(string graphName, int dataIndex)
        {
            // First, find the file to be opened.
            var selectDestFolder = new OpenFileDialog();
            selectDestFolder.Filter = "Text Files (*.txt)|*.txt";
            selectDestFolder.Multiselect = true;
            selectDestFolder.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (selectDestFolder.ShowDialog() == true)
            {
                // Then check the file which is valid to open for this part
                string[] fileName = selectDestFolder.SafeFileName.Split('_');
                
                // Check whether the file name format is valid or not
                if (!(fileName.Count() >= 4
                      && ((fileName[2] == "ToolB" && (graphName == "Fa" || graphName == "Eta1" || graphName == "Eta2")) ||
                          (fileName[2] == "CalcD" && (graphName == "FeedR" || graphName == "Force" || graphName == "FeedR[U]" ||
                                                      graphName == "Force[Y]" || graphName == "KtUpd" || graphName == "F_Sim" || 
                                                      graphName == "F_Mean")))
                     )
                   )
                {
                    // One of the file name is not proper, report an error
                    MessageBox.Show("Invalid File Name / Format: " + selectDestFolder.SafeFileName,
                                    "Data Records",
                                    MessageBoxButton.OK, MessageBoxImage.Stop);

                    return;
                }
               
                // Create the child window variable
                GraphicsChild myChild;

                // This is important, because if it is not entered any of tabitems before creating any sub window,
                // then it causes an exception
                tabControlMenu.SelectedIndex = 2;

                // Now it is ready to open the child window
                DataRecordGraphChildContainer.Children.Add(new WPF.MDI.MdiChild()
                {
                    Name = "GraphChildAppWindow",
                    Title = selectDestFolder.SafeFileName,
                    Height = 400,
                    Width = 800,
                    Content = myChild = new GraphicsChild(selectDestFolder.FileName, null, dataIndex, null, null, 0)
                });

                // General settings
                myChild.LineChart.Title = graphName;
            }
        }

        // Comparetion Menu Clicks
        //
        private void CompareGraphFa_Click(object sender, RoutedEventArgs e) { DataRecordsCompareGraph("Fa", 0, "Fa", 0); }
        private void CompareGraphEta1_Click(object sender, RoutedEventArgs e) { DataRecordsCompareGraph("Eta1", 1, "Eta1", 1); }
        private void CompareGraphEta2_Click(object sender, RoutedEventArgs e) { DataRecordsCompareGraph("Eta2", 2, "Eta2", 2); }
        private void CompareGraphFeedR_Click(object sender, RoutedEventArgs e) { DataRecordsCompareGraph("FeedR", 0, "FeedR", 0); }
        private void CompareGraphFeedRU_Click(object sender, RoutedEventArgs e) { DataRecordsCompareGraph("FeedR", 0, "FeedR[U]", 2); }
        private void CompareGraphForce_Click(object sender, RoutedEventArgs e) { DataRecordsCompareGraph("Force", 1, "Force", 1); }
        private void CompareGraphForceY_Click(object sender, RoutedEventArgs e) { DataRecordsCompareGraph("Force", 1, "Force[Y]", 3); }
        private void CompareGraphForceS_Click(object sender, RoutedEventArgs e) { DataRecordsCompareGraph("Force", 1, "F_Sim", 5); }
        private void CompareGraphForceM_Click(object sender, RoutedEventArgs e) { DataRecordsCompareGraph("Force", 1, "F_Mean", 6); }
        private void CompareGraphUFeedR_Click(object sender, RoutedEventArgs e) { DataRecordsCompareGraph("FeedR[U]", 2, "FeedR", 0); }
        private void CompareGraphUFeedRU_Click(object sender, RoutedEventArgs e) { DataRecordsCompareGraph("FeedR[U]", 2, "FeedR[U]", 2); }
        private void CompareGraphForce_YF_Click(object sender, RoutedEventArgs e) { DataRecordsCompareGraph("Force[Y]", 3, "Force", 1); }
        private void CompareGraphForce_Y_Click(object sender, RoutedEventArgs e) { DataRecordsCompareGraph("Force[Y]", 3, "Force[Y]", 3); }
        private void CompareGraphForce_YS_Click(object sender, RoutedEventArgs e) { DataRecordsCompareGraph("Force[Y]", 3, "F_Sim", 5); }
        private void CompareGraphForce_YM_Click(object sender, RoutedEventArgs e) { DataRecordsCompareGraph("Force[Y]", 3, "F_Mean", 6); }
        private void CompareGraphKtUpd_Click(object sender, RoutedEventArgs e) { DataRecordsCompareGraph("KtUpd", 4, "KtUpd", 4); }
        private void CompareGraphF_SimF_Click(object sender, RoutedEventArgs e) { DataRecordsCompareGraph("F_Sim", 5, "Force", 1); }
        private void CompareGraphF_SimY_Click(object sender, RoutedEventArgs e) { DataRecordsCompareGraph("F_Sim", 5, "Force[Y]", 3); }
        private void CompareGraphF_Sim_Click(object sender, RoutedEventArgs e) { DataRecordsCompareGraph("F_Sim", 5, "F_Sim", 5); }
        private void CompareGraphF_SimM_Click(object sender, RoutedEventArgs e) { DataRecordsCompareGraph("F_Sim", 5, "F_Mean", 6); }
        private void CompareGraphF_MeanF_Click(object sender, RoutedEventArgs e) { DataRecordsCompareGraph("F_Mean", 6, "Force", 1); }
        private void CompareGraphF_MeanY_Click(object sender, RoutedEventArgs e) { DataRecordsCompareGraph("F_Mean", 6, "Force[Y]", 3); }
        private void CompareGraphF_MeanS_Click(object sender, RoutedEventArgs e) { DataRecordsCompareGraph("F_Mean", 6, "F_Sim", 5); }
        private void CompareGraphF_Mean_Click(object sender, RoutedEventArgs e) { DataRecordsCompareGraph("F_Mean", 6, "F_Mean", 6); }

        private void DataRecordsCompareGraph(string firstGraphName, int firstDataIndex, string secondGraphName, int secondDataIndex)
        {
            // Find the first file to be opened.
            //
            var selectDestFolderFirst = new OpenFileDialog();
            selectDestFolderFirst.Filter = "Text Files (*.txt)|*.txt";
            selectDestFolderFirst.Multiselect = true;
            selectDestFolderFirst.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (selectDestFolderFirst.ShowDialog() == true)
            {
                // Then check the file which is valid to open for this part
                string[] fileName = selectDestFolderFirst.SafeFileName.Split('_');

                // Check whether the file name format is valid or not
                if (!(fileName.Count() >= 4
                      && ((fileName[2] == "ToolB" && (firstGraphName == "Fa" || firstGraphName == "Eta1" || firstGraphName == "Eta2")) ||
                          (fileName[2] == "CalcD" && (firstGraphName == "FeedR" || firstGraphName == "Force" || firstGraphName == "FeedR[U]" ||
                                                      firstGraphName == "Force[Y]" || firstGraphName == "KtUpd" || firstGraphName == "F_Sim" ||
                                                      firstGraphName == "F_Mean")))
                     )
                   )
                {
                    // One of the file name is not proper, report an error
                    MessageBox.Show("Invalid File Name / Format: " + selectDestFolderFirst.SafeFileName,
                                    "Data Records",
                                    MessageBoxButton.OK, MessageBoxImage.Stop);

                    return;
                }
            }
               
            // Find the second file to be opened.
            //
            var selectDestFolderSecond = new OpenFileDialog();
            selectDestFolderSecond.Filter = "Text Files (*.txt)|*.txt";
            selectDestFolderSecond.Multiselect = true;
            selectDestFolderSecond.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (selectDestFolderSecond.ShowDialog() == true)
            {
                // Then check the file which is valid to open for this part
                string[] fileName = selectDestFolderSecond.SafeFileName.Split('_');

                // Check whether the file name format is valid or not
                if (!(fileName.Count() >= 4
                      && ((fileName[2] == "ToolB" && (secondGraphName == "Fa" || secondGraphName == "Eta1" || secondGraphName == "Eta2")) ||
                          (fileName[2] == "CalcD" && (secondGraphName == "FeedR" || secondGraphName == "Force" || secondGraphName == "FeedR[U]" ||
                                                      secondGraphName == "Force[Y]" || secondGraphName == "KtUpd" || secondGraphName == "F_Sim" ||
                                                      secondGraphName == "F_Mean")))
                     )
                   )
                {
                    // One of the file name is not proper, report an error
                    MessageBox.Show("Invalid File Name / Format: " + selectDestFolderSecond.SafeFileName,
                                    "Data Records",
                                    MessageBoxButton.OK, MessageBoxImage.Stop);

                    return;
                }
            }

            // Files are okay, create graph window
            //
            // Declare the child window variable
            GraphicsChild myChild;

            // This is important, because if it is not entered any of tabitems before creating any sub window,
            // then it causes an exception
            tabControlMenu.SelectedIndex = 2;

            // Now it is ready to open the child window
            DataRecordGraphChildContainer.Children.Add(new WPF.MDI.MdiChild()
            {
                Name = "GraphChildAppWindow",
                Title = "Comparision: " + selectDestFolderFirst.SafeFileName + " vs. " + selectDestFolderSecond.SafeFileName,
                Height = 400,
                Width = 800,
                Content = myChild = new GraphicsChild(selectDestFolderFirst.FileName, firstGraphName, firstDataIndex, selectDestFolderSecond.FileName, secondGraphName, secondDataIndex)
            });

            // General settings
            myChild.LineChart.Title = firstGraphName + " vs. " + secondGraphName;
        }

        //******************************************************************************************************
        // Menu: View, SubMenu: Data Records
        //
        // 
        //******************************************************************************************************
        //
        private void DataRecordsMain_Click(object sender, RoutedEventArgs e) { DataRecordsText("Main"); }
        private void DataRecordsFa_Click(object sender, RoutedEventArgs e) { DataRecordsText("Fa"); }
        private void DataRecordsEta1_Click(object sender, RoutedEventArgs e) { DataRecordsText("Eta1"); }
        private void DataRecordsEta2_Click(object sender, RoutedEventArgs e) { DataRecordsText("Eta2"); }
        private void DataRecordsFeedR_Click(object sender, RoutedEventArgs e) { DataRecordsText("FeedR"); }
        private void DataRecordsForce_Click(object sender, RoutedEventArgs e) { DataRecordsText("Force"); }
        private void DataRecordsFeedR_U_Click(object sender, RoutedEventArgs e) { DataRecordsText("FeedR[U]"); }
        private void DataRecordsForce_Y_Click(object sender, RoutedEventArgs e) { DataRecordsText("Force[Y]"); }
        private void DataRecordsKtUpd_Click(object sender, RoutedEventArgs e) { DataRecordsText("KtUpd"); }
        private void DataRecordsF_Sim_Click(object sender, RoutedEventArgs e) { DataRecordsText("F_Sim"); }
        private void DataRecordsF_Mean_Click(object sender, RoutedEventArgs e) { DataRecordsText("F_Mean"); }

        private void DataRecordsText(string fileIdx)
        {
            // First, find the file to be opened.
            var selectDestFolder = new OpenFileDialog();
            selectDestFolder.Filter = "Text Files (*.txt)|*.txt";
            selectDestFolder.Multiselect = true;
            selectDestFolder.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (selectDestFolder.ShowDialog() == true)
            {
                // Then check the file which is valid to open for this part
                string[] fileName = selectDestFolder.SafeFileName.Split('_');

                // Check whether the file name format is valid or not
                //if (!(fileName[0] == "OCC" && (fileName[1] == "MainD" || fileName[1] == "CalcD" || fileName[1] == "ToolD")))
                if (!(fileName.Count() >= 4
                        && ((fileName[2] == "MainD" && fileIdx == "Main") ||
                            (fileName[2] == "ToolB" && (fileIdx == "Fa" || fileIdx == "Eta1" || fileIdx == "Eta2")) ||
                            (fileName[2] == "CalcD" && (fileIdx == "FeedR" || fileIdx == "Force" || fileIdx == "FeedR[U]" ||
                                                        fileIdx == "Force[Y]" || fileIdx == "KtUpd" || fileIdx == "F_Sim" ||
                                                        fileIdx == "F_Mean")))
                        )
                    )
                {
                    // One of the file name and/or its index is not proper, report an error
                    MessageBox.Show("Invalid File Name and/or its index: " + selectDestFolder.SafeFileName + "     " + fileIdx,
                                    "Data Records",
                                    MessageBoxButton.OK, MessageBoxImage.Stop);

                    return;
                }
                
                // Create the child window variable
                DataRecordsChild myChild;

                // This is important, because if it is not entered any of tabitems before creating any sub window,
                // then it causes an exception
                tabControlMenu.SelectedIndex = 3;

                // File format seems okay, now check whether the file name is supported or not
                if (fileIdx == "Main")
                {
                    // Now it is ready to open the child window
                    DataRecordTextChildContainer.Children.Add(new WPF.MDI.MdiChild()
                    {
                        Name = "TextChildAppWindow",
                        Title = selectDestFolder.SafeFileName,
                        Height = 400,
                        Width = 850,
                        Content = myChild = new DataRecordsChild(fileIdx, selectDestFolder.FileName)
                    });
                }
                else
                {
                    // Now it is ready to open the child window
                    DataRecordTextChildContainer.Children.Add(new WPF.MDI.MdiChild()
                    {
                        Name = "TextChildAppWindow",
                        Title = selectDestFolder.SafeFileName,
                        Height = 600,
                        Width = 290,
                        Content = myChild = new DataRecordsChild(fileIdx, selectDestFolder.FileName)
                    });
                }
            }
        }


        //******************************************************************************************************
        // Menu: View, SubMenu: Online Graphics
        //
        //******************************************************************************************************
        //
        private void ForceVsDisMonitoring_Click(object sender, RoutedEventArgs e)
        {
            // This is important, because if it is not entered any of tabitems before creating any sub window,
            // then it causes an exception
            tabControlMenu.SelectedIndex = 1;

            // Still under TESTING, problem is I can not detect the child is closed
            //
            // If already opened, do NOT do again
            if (isForceVsDisMonitoring) return;
            isForceVsDisMonitoring = true;
            //Console.WriteLine("Setting: isForceVsDisMonitoring= " + isForceVsDisMonitoring);

            // Now it is ready to open the child window
            MonitoringChildContainer.Children.Add(new WPF.MDI.MdiChild()
            {
                Name = "MonitorChildAppWindow",
                Title = "Simulated vs. Real-Time Data Monitoring",
                Height = 400,
                Width = 850,
                Content = myMonitorChild = new MonitoringChild()
            });

            myMonitorChild.LineChart.Title = "Force vs. Distance";
            myMonitorChild.XLinearAxis.Title = "Distance (mm)";
            myMonitorChild.YLinearAxis.Title = "Force (N)";
            myMonitorChild.mainWindow = this;
        }

        private void SpindleVsDisMonitoring_Click(object sender, RoutedEventArgs e)
        {
            // This is important, because if it is not entered any of tabitems before creating any sub window,
            // then it causes an exception
            tabControlMenu.SelectedIndex = 2;

            // If already opened, do NOT do again
            //if (isForceVsDisMonitoring) return;
            //isForceVsDisMonitoring = true;

            // Now it is ready to open the child window
            MonitoringChildContainer.Children.Add(new WPF.MDI.MdiChild()
            {
                Name = "MonitorChildAppWindow",
                Title = "Simulated vs. Real-Time Data Monitoring",
                Height = 400,
                Width = 850,
                Content = myMonitorChild = new MonitoringChild()
            });

            myMonitorChild.LineChart.Title = "Spindle vs. Distance";
            myMonitorChild.XLinearAxis.Title = "Distance (mm)";
            myMonitorChild.YLinearAxis.Title = "Torque (rpm)";
            myMonitorChild.mainWindow = this;
        }

        //******************************************************************************************************
        // Menu: Run
        //
        //******************************************************************************************************
        //
        private async void RunStart_Click(object sender, RoutedEventArgs e)
        {
            // Initial checkings to be able to continue
            //
            if (isRunning) { return; } // Don't go further if running

            // First check any missing datafill for either test or normal mode
            if (isTestModeStandaloneOn)
            {
                // Test mode STANDALONE: Only tool geo datafilll is required
                if (!isToolGeoDefined)
                {
                    UpdateButtonContent(tracingStatusButton, "SETUP TOOL GEOMETRY");
                    return;
                }
            }
            else
            {
                // Normal Mode and Test Mode DEPENDENT: All config datafill is required
                if (!(isCNCtypeEntered && isConnected && isInputDataLoaded && isToolGeoDefined)) // Setup is not completed yet, don't run
                {
                    UpdateButtonContent(tracingStatusButton, "SETUP NOT COMPLETED");
                    return;
                }
            }

            // Everything is okay, ready to run the program
            //
            // Reset some parameters for rerun cases
            LSV2Caller.SetSimDataIndex(0); // simmulated data index for TFG and TFNC lists downloaded for processes
            LSV2Caller.InitializeSpindlePeriodCount(); // SpindlePeriodCount for Adaptive control 

            // Start getting CNC data
            cts = new CancellationTokenSource(); // move it into the tracking process

            // Set some initial setting to be able start properly
            //
            LSV2Caller.SetTotalCountDefault();// Set to zero, it is used to count how many records we hold while running the process
            LSV2Caller.SetFeedRatePercent(100); // Set feed rate to max as a persentage in case of re-run after tool breakage detected

            // Clear all List data records in case of any re-run
            // These are static records and has to be cleared after used.
            //
            LSV2Caller.ClearAllListRecordsOnceRun();

            // Clear all graphs for re-run case
            ClearAllGraphPlotting();

            // This is Normal Mode, Start running
            //
            processTimeButton.Content = "00:00:00.000"; // set the timer initial data

            // Initial settings in case of re-run, set to the first line of tracing list data
            if (tracingListView.Items.Count > 0)
            {
                tracingListView.SelectedIndex = 0;
                tracingListView.ScrollIntoView(tracingListView.SelectedItem);
            }

            if (!isTestModeStandaloneOn)
            {
                Console.WriteLine("Reading real-time CNC data from LSV2 buffer: for displaying: ReadCNCDataThread");
                ReadCNCDataThread();
            }

            Console.WriteLine("Displaying real-time CNC data to the screen");
            stopCNCDataDisplaying = false;
            DisplayCNCDataAsync();

            if (!isTestModeStandaloneOn)
            {
                Console.WriteLine("Tracking Input data with real-time CNC position");
                InputDataTrackingThread();

                Console.WriteLine("Updating real-time CNC feed rate and speed");
                //UpdateCNCData();
                LSV2Caller.ControlCNCTask();
            }

            tracingStatusButton.Content = "WAITING";
            tracingStatusButton.Background = Brushes.Green;
            tracingStatusButton.Foreground = Brushes.White;

            // Seems evething is okay, enable running
            isRunning = true;
            LSV2Caller.SetTrackingStarted(true); // To be able to track any data, we need to store positions, then tracking will get them.

            // ????????? Check if this is required
            // Enable Data Record open Text or Graphic window submenu item since the process is stopped.
            DataRecordTextSubMenu.IsEnabled = false;
            DataRecordGraphSubMenu.IsEnabled = false;

            // Test Mode: DEPENDENT extra settings
            //
            if (isTestModeDependentOn)
            {
                // Since this is NOT Normal mode, so tracking does not work, hence process running is not started in LSV2
                // Set it here as well as setting Dependent test mode in LSV2
                LSV2Caller.SetProcessRunning(true);
                LSV2Caller.SetDependentTestMode(true);
            }

            // Test Mode: STANDALONE extra settings
            //
            if (isTestModeStandaloneOn)
            {
                tracingStatusButton.Content = "RUNNING";
                tracingStatusButton.Background = Brushes.Firebrick;
                tracingStatusButton.Foreground = Brushes.White;

                if (LSV2Caller.GetProcessRecording())
                {
                    outputDataFolderButton.Content = "RECORDING";
                    outputDataFolderButton.Background = Brushes.Firebrick;
                    outputDataFolderButton.Foreground = Brushes.White;
                }

                LSV2Caller.InitializeAdaptiveContArrays();

                // Important to display monitor changes
                DoEvents();

                // No real connection since it it Standalone TEST mode
                LSV2Caller.SetVirtualRealConnection(false);

                // Run test data
                //
                await Task.Run(() => { LSV2Caller.TestDataActionLoop(); });
                // ------------------------

                // Set real connection back
                LSV2Caller.SetVirtualRealConnection(true);

                tracingStatusButton.Content = "TEST COMPLETED";
                tracingStatusButton.Background = Brushes.DarkBlue;
                tracingStatusButton.Foreground = Brushes.White;

                // Test ended. In case recording is started. so set to the defaults.
                if (LSV2Caller.GetProcessRecording())
                {
                    outputDataFolderButton.Content = "FOLDER SELECTED";
                    outputDataFolderButton.Background = Brushes.LightGreen;
                    outputDataFolderButton.Foreground = Brushes.Black;
                }
            }
        }

        private void ClearAllGraphPlotting()
        {
            myFollower.ToolKeyValuePair.Clear();
            myFa.RtmDataKeyValuePair.Clear();
            myEta1.RtmDataKeyValuePair.Clear();
            myFtO.RtmDataKeyValuePair.Clear();
            myFeedR.RtmDataKeyValuePair.Clear();
        }

        private void RunStop_Click(object sender, RoutedEventArgs e)
        {
            runStopUtility();
        }

        //
        private void runStopUtility()
        {
            // First set this bool to false to control stop running
            isRunning = false;

            // Enable Data Record open Text or Graphic window submenu item since the process is stopped.
            DataRecordTextSubMenu.IsEnabled = true;
            DataRecordGraphSubMenu.IsEnabled = true;

            // Reset recording to be able to create a new file if re-run the process
            //
            LSV2Caller.SetOnceRun(true);

            // This controls to generate Fa, eta1 etc. records in case the running is stopped.
            //
            LSV2Caller.SetTrackingEnded(true); // This controls to generate Fa, eta1 etc. records in case the running is stopped.

            // Stop all active processes EXCEPT HeidenhainGetDataThread which should still be running for re-run cases
            //
            stopAllActiveProcesses();

            // All threads & Tasks are stopped, just set the display info to defaults
            tracingStatusButton.Content = "STOPPED";
            tracingStatusButton.Background = Brushes.LightGray;
            tracingStatusButton.Foreground = Brushes.Black;

            // Reset the alarm button
            if (LSV2Caller.GetToolBreakageDetected())
            {
                stopErrorBlinking = false; // To stop blinking and beeping

                alarmStatus.Content = "NO ALARM";
                alarmStatus.Background = Brushes.LightGray;
                alarmStatus.Foreground = Brushes.Black;

                // Set tool breakage to false
                LSV2Caller.SetToolBreakageDetected(false);
            }

            // In case recording is already started. stop it and set to the defaults.
            if (LSV2Caller.GetProcessRecording())
            {
                outputDataFolderButton.Content = "FOLDER SELECTED";
                outputDataFolderButton.Background = Brushes.LightGreen;
                outputDataFolderButton.Foreground = Brushes.Black;
            }
        }

        // Window closing control ------------------------------------------------------------------------------
        //
        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Do you REALLY want to CLOSE it?", "Warning: Closing Window", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                // If already connected, disconnect to not block  theports for other applications.
                if (isConnected)
                {
                    // Stop all background processes
                    isStoped = true; // Stop blinkling control for displaying 'Connecting' message

                    // Stop all active processes as well as HeidenhainGetDataThread (done below)
                    //
                    stopAllActiveProcesses();

                    // Reset the alarm button to stop blinking and beeping 
                    if (LSV2Caller.GetToolBreakageDetected()) { stopErrorBlinking = false; }

                    // Wait for a while to make sure that all processes and their belongings are released
                    // to prevent an EXCEPTION:
                    // "An attempt has been made to free an RCW that is in use.  The RCW is in use on the active thread 
                    // or another thread.  Attempting to free an in-use RCW can cause corruption or data loss."
                    Thread.Sleep(200);
                   
                    // Finally Close connection
                    var result = LSV2Caller.CloseConnection(hPort);
                    if (!result)
                    {
                        Console.WriteLine("Error encountered when disconnecting from port.");
                    }

                    if (hPort != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(hPort);
                        hPort = IntPtr.Zero;
                    }

                    switch (cncType)
                    {
                        case CNCType.Heidenhain:
                            Console.WriteLine("Disconnect Heidenhain");

                            if (HeidenhainGetDataThread != null) { HeidenhainGetDataThread.Abort(); }
                            dncConnection.CloseConnection();
                            break;

                        case CNCType.Fanuc:
                            break;

                        case CNCType.Siemens:
                            break;

                        case CNCType.None:
                            Console.WriteLine("It is connected to CNC, but CNC type is not selected, Never comes to this stage !!!!!");
                            break;
                    }
                }                

                Application.Current.Shutdown();
            }
            else
            {
                e.Cancel = true;
            }
        }

        // Processes to be stopped either running is stopped or main window closed.
        private void stopAllActiveProcesses()
        {
            if (processTimer != null) { processTimer.Stop(); } // timer 
            LSV2Caller.SetProcessRunning(false); // Tell read scope data program that deactive the process
            if (CNCdataReadingThread != null) { CNCdataReadingThread.Abort(); } // Stop reading data from CNC
            stopCNCDataDisplaying = true; // Stop Displaying CNC data to the window
            if (trackingInputDataThread != null) { trackingInputDataThread.Abort(); } // Stop data tracking
            LSV2Caller.SetTrackingStarted(false); // Stop storing positions, we do not need them.
            //stopCNCdataUpdating = true; // Stop updating data in CNC
            LSV2Caller.StopCNCcontrol(); // Stop controling for feed rate and spindle speed in CNC
            if (LSV2Caller.dataPlottingThread != null) { LSV2Caller.dataPlottingThread.Abort(); }
            if (timerWatchDogThread != null) timerWatchDogThread.Abort();
        }

        //******************************************************************************************************
        // Menu: Help
        //
        //******************************************************************************************************
        //
        private void Dependencies_Click(object sender, RoutedEventArgs e)
        {
            var popup = new HelpDocumentWindow();
            popup.Title = "Dependencies Information";

            // Example-1
            //Run a0 = new Run();
            //a0.Text = "The following dll files must be at the same directory to be able to have full functionality:\n";
            ////a0.Foreground = Brushes.Red;
            ////a0.FontStyle = FontStyles.Italic;
            //a0.FontSize = 12;
            //popup.HelpDocTextBlock.Inlines.Add(a0);
            //Run a1 = new Run();
            //a1.Text = "• Interop.HeidenhainDNCLib.dll\n";
            ////a1.Foreground = Brushes.Red;
            //a1.FontStyle = FontStyles.Italic;
            ////a1.FontSize = 20;
            //popup.HelpDocTextBlock.Inlines.Add(a1);
            //Run a2 = new Run();
            //a2.Text = "• Interop.LSV2CTRL_3Lib.dll\n";
            //popup.HelpDocTextBlock.Inlines.Add(a2);
            //Run a3 = new Run();
            //a3.Text = "• OpenGLManagedCpp.dll\n";
            //popup.HelpDocTextBlock.Inlines.Add(a3); 

            // Example-2
            popup.HelpDocTextBlock.Inlines.Add(new Run("The following "));
            popup.HelpDocTextBlock.Inlines.Add(new Bold(new Run("dll")));
            popup.HelpDocTextBlock.Inlines.Add(new Run(" files must be at the same directory with intelCUT, to be able to have full functionality:\n"));
            popup.HelpDocTextBlock.Inlines.Add(new Run("  • Interop.HeidenhainDNCLib.dll\n"));
            popup.HelpDocTextBlock.Inlines.Add(new Run("  • Interop.HeidenhainDNCLib.dll\n"));
            popup.HelpDocTextBlock.Inlines.Add(new Run("  • Interop.HeidenhainDNCLib.dll\n"));
            popup.HelpDocTextBlock.Inlines.Add(new Run("  • Interop.HeidenhainDNCLib.dll\n"));
            
            popup.Background = Brushes.Beige;
            popup.Show();
        }

        private void Prerequsites_Click(object sender, RoutedEventArgs e)
        {
            var popup = new HelpDocumentWindow();
            popup.Title = "Prerequsite Information";

            //popup.HelpDocTextBlock.Text = "Tayfun";
            //popup.HelpDocTextBlock.Text += " Ozdemir\n\n";
            //popup.HelpDocTextBlock.Inlines.Add(new Run { Text = "1- High Speed Computer\n" });
            //popup.HelpDocTextBlock.Inlines.Add(new Run { Text = "2- CNC (Currently Heidenhain ITNC530 is supported)\n" });
            //popup.HelpDocTextBlock.Inlines.Add(new Run { Text = "3- Connection Setup (Heidenhain Connection exe file)\n" });

            popup.HelpDocTextBlock.Inlines.Add(new Run("The followings are required for On-line Cutting Control ("));
            popup.HelpDocTextBlock.Inlines.Add(new Bold(new Run("OCC")));
            popup.HelpDocTextBlock.Inlines.Add(new Run(") Process:\n"));
            popup.HelpDocTextBlock.Inlines.Add(new Run("  •  High Speed Computer\n"));
            popup.HelpDocTextBlock.Inlines.Add(new Run("  •  CNC ("));
            Run a0 = new Run();
            a0.Text = "Currently, Heidenhain ITNC530 supported";
            a0.Foreground = Brushes.Red;
            popup.HelpDocTextBlock.Inlines.Add(a0);
            popup.HelpDocTextBlock.Inlines.Add(new Run(")\n"));
            popup.HelpDocTextBlock.Inlines.Add(new Run("  •  Connection Setup\n"));
            popup.HelpDocTextBlock.Inlines.Add(new Run("  •  Simulated Data File (Generated bu using MachPRO)\n"));
            popup.HelpDocTextBlock.Inlines.Add(new Run("  •  Workpiece Geometry File (Generated by using CAM, "));
            Run a1 = new Run();
            a1.Text = "Optional";
            a1.Foreground = Brushes.Blue;
            popup.HelpDocTextBlock.Inlines.Add(a1);
            popup.HelpDocTextBlock.Inlines.Add(new Run(")\n"));

            popup.Background = Brushes.PowderBlue;
            popup.Show();
        }

        private void ConnectionSetUp_Click(object sender, RoutedEventArgs e)
        {
            var popup = new HelpDocumentWindow();
            popup.Title = "Connection Setup Information";

            popup.HelpDocTextBlock.Text = "Work in Progress";
            
            popup.Background = Brushes.Aquamarine;
            popup.Show();
        }

        private void UserGuide_Click(object sender, RoutedEventArgs e)
        {
            var popup = new HelpDocumentWindow();
            popup.Title = "User Guide";

            popup.HelpDocTextBlock.Text = "Work in Progress";
            
            popup.Background = Brushes.BlanchedAlmond;
            popup.Show();
        }

        private void ContactUs_Click(object sender, RoutedEventArgs e)
        {
            var popup = new HelpDocumentWindow();
            popup.Title = "Contact Us";

            popup.HelpDocTextBlock.Text = "Work in Progress";
            
            popup.Background = Brushes.LightYellow;
            popup.Show();
        }
        //
        // This is the end of MENU CONTROL procedures ----------------------------------------------------------

        //******************************************************************************************************
        // HEIDENHAIN Connection procedures
        //
        // This codes are just copied from Ramon's development and requires to be cleared up  !!!!!!!!!!!!!!
        //
        // Requires: - LSV2Caller.cs
        //           - HeidenhainDNC.cs
        //           - HeidenhainDNClib (in References)
        //
        //
        //******************************************************************************************************
        
        // Proc: HeidenhainSetUpScope **************************************************************************
        //
        // Calls the LSV2Login and GetScopeInfo methods for connecting using the LSV2 protocol
        // Also selects the channels to be used
        //
        private bool HeidenhainSetUpScope()
        {
            bool result;

            const string pw_scope = "OSZI";
            Console.WriteLine("HeidenhainSetUpScope: Calling LSV2Login (SCOPE)");

            result = LSV2Caller.LSV2Login(hPort, pw_scope, null);
            if (!result)
            {
                var errorString = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                Console.WriteLine("Error in LSV2Login (SCOPE): " + errorString);
                return false;
            }

            Console.WriteLine("HeidenhainSetUpScope: Calling GetScopeInfo");
            //           outputSW.Flush(); // Related with output, Commented out by Tayfun for compilation

            result = LSV2Caller.GetScopeInfo(hPort);
            if (!result)
            {
                Console.WriteLine("Error in GetScopeInfo");
                return false;
            }

            Int32 samplingInterval = 100;

            LSV2Caller.tChannelSel[] selectedChannels = new LSV2Caller.tChannelSel[channelCount];

            //Position, X Axis
            selectedChannels[0].ChannelId = 3;
            selectedChannels[0].Index = 0;
            selectedChannels[0].PLCAddress = -1;
            //Position, Y Axis
            selectedChannels[1].ChannelId = 3;
            selectedChannels[1].Index = 1;
            selectedChannels[1].PLCAddress = -1;
            //Position, Z Axis
            selectedChannels[2].ChannelId = 3;
            selectedChannels[2].Index = 2;
            selectedChannels[2].PLCAddress = -1;
            //Nominal Current, X Axis
            selectedChannels[3].ChannelId = 14;
            selectedChannels[3].Index = 0;
            selectedChannels[3].PLCAddress = -1;
            //Nominal Current, Y Axis
            selectedChannels[4].ChannelId = 14;
            selectedChannels[4].Index = 1;
            selectedChannels[4].PLCAddress = -1;
            //Nominal Current, Z Axis
            selectedChannels[5].ChannelId = 14;
            selectedChannels[5].Index = 2;
            selectedChannels[5].PLCAddress = -1;
            //Nominal Current, Spindle
            selectedChannels[6].ChannelId = 14;
            selectedChannels[6].Index = 22;
            selectedChannels[6].PLCAddress = -1;
            //Feed Rate
            selectedChannels[7].ChannelId = 8;
            selectedChannels[7].Index = 0;
            selectedChannels[7].PLCAddress = -1;
            //Spindle Speed
            selectedChannels[8].ChannelId = 12;
            selectedChannels[8].Index = 22;
            selectedChannels[8].PLCAddress = -1;

            LSV2Caller.tChannelPara[] channelParams = new LSV2Caller.tChannelPara[channelCount];

            result = LSV2Caller.LSV2SelectChannelsITNC(hPort, channelCount, samplingInterval, selectedChannels, channelParams);

            if (result) // Related with output, later consider with output development
            {
                //                outputSW.Flush(); // Commented out by Tayfun for compilation
                //LSV2Caller.GetScopeInfo(hPort);
            }
            else
            {
                var errorString = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                Console.WriteLine("Error in LSV2SelectChannelsITNC: " + errorString);
            }
            return result;
        }

        // Proc: HeidenhainSetUpLSV2Connection **********************************************************************
        //
        // Sets up the LSV2 protocol connection to the Heidenhain controller, based on the specified IP address
        //
        private bool HeidenhainSetUpLSV2Connection(string address)
        {
            ipAddress = address;

            bool result;

            Console.WriteLine("HeidenhainSetUpLSV2Connection: Calling LSV2Caller.OpenConnection");
            result = LSV2Caller.OpenConnection(out hPort, address);
            if (!result)
            {
                var error = Marshal.GetLastWin32Error();
                var errorString = new Win32Exception(error).Message;
                Console.WriteLine("Error in LSV2Open: (Error code " + error.ToString() + ") " + errorString);
                return false;
            }

            const string pw_inspect = "INSPECT";
            Console.WriteLine("HeidenhainSetUpLSV2Connection: Calling LSV2Login (INSPECT)");
            result = LSV2Caller.LSV2Login(hPort, pw_inspect, null);
            if (!result)
            {
                var errorString = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                Console.WriteLine("Error in LSV2Login (INSPECT): " + errorString);
                return false;
            }

            var lpPara = new LSV2Caller.LSV2PARA();
            IntPtr lpParaPtr = Marshal.AllocHGlobal(Marshal.SizeOf(lpPara));

            Marshal.StructureToPtr(lpPara, lpParaPtr, false);

            Console.WriteLine("HeidenhainSetUpLSV2Connection: Calling LSV2ReceivePara");
            result = LSV2Caller.LSV2ReceivePara(hPort, out lpParaPtr);
            Marshal.DestroyStructure(lpParaPtr, typeof(LSV2Caller.LSV2PARA));

            result = HeidenhainSetUpScope();
            isLoggedIn = result;

            return result;
        }

        // Proc: HeidenhainSetUpDNCConnection **********************************************************************
        //
        // Calls SetDNCConnection, with the specified IP address
        // Also enables the button for configuring the DNC connection
        //
        private bool HeidenhainSetUpDNCConnection(string address)
        {
            dncConnection = new HeidenhainDNC(address);
            bool result = dncConnection.RequestConnection();
            if (result)
            {
                LSV2Caller.SetDNCConnection(dncConnection);

                //                ConfigureDNCButtonItemsControl.IsEnabled = true;
            }
            return result;
        }

        // Proc: ConfigureHeidenhainMachineSelection ****************************************************************
        //
        // Sets up the connection to the Heidenhain controller, using the LSV2 protocol
        // If the dnc boolean is true, the DNC connection is also established
        //
        private async Task<bool> ConfigureHeidenhainMachineSelection(bool dnc)
        {
            ConnectionStatus status = ConnectionStatus.CONNECTING;

            bool connectionSuccess = false;

            Console.WriteLine("HeidenhainConnection");

            await Task.Run(() =>
            {
                if (host != null)
                {
                    Console.WriteLine("Heidenhain: Address is not null");

                    cncType = CNCType.Heidenhain;
                    Console.WriteLine("IP address: " + host);

                    bool lsv2Connected = HeidenhainSetUpLSV2Connection(host);

                    if (lsv2Connected)
                    {
                        connectionSuccess = dnc ? HeidenhainSetUpDNCConnection(host) : true;

                        if (connectionSuccess)
                        {
                            status = ConnectionStatus.CONNECTED;
                        }
                        else
                        {
                            Console.WriteLine("Connection error: Unable to set up DNC connection.");
                            status = ConnectionStatus.ERROR;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Connection error: Unable to set up LSV2 connection.");
                        status = ConnectionStatus.ERROR;
                    }
                }
                else
                {
                    Console.WriteLine("Connection error: Unable to connect to IP address '" + host + "'.");
                    status = ConnectionStatus.ERROR;
                }
            });

            // Stop the connection button blinking with "CONNECTING:
            isStoped = true;
                                   
            // Update the connection button with the current status and coloring
            connectionCncButton.Content = status;
            if (status == ConnectionStatus.CONNECTED)
            {
                connectionCncButton.Background = Brushes.LightGreen;
            }
            else connectionCncButton.Background = Brushes.LightPink;

            return connectionSuccess;
        }


        // Proc: GetHeidenhainData ***************************************************************************
        //
        // Calls GetDataFromScope to obtain the real-time data from the CNC
        //
        private void GetHeidenhainDataThread()
        {
            HeidenhainGetDataThread = new Thread(() =>
            {
                bool receivedData = LSV2Caller.GetDataFromScope(hPort, channelCount);
               
            });

            HeidenhainGetDataThread.Priority = ThreadPriority.Highest;
            HeidenhainGetDataThread.Name = "Heidenhain";
            HeidenhainGetDataThread.Start();
        }

        // Proc: ReadCNCData ***************************************************************************
        //
        // Before this proc, GetHeidenhainData has to be invoked, to get the real-time data from the CNC
        // Exit to stop, abort CNCdataReadingThread
        //
        private void ReadCNCDataThread()
        {
            DataForGUI data;

            CNCdataReadingThread = new Thread(() =>
                {
                    Console.WriteLine("Data receiving from CNC is started");
                    
                    while (true)
                    {
                        data = LSV2Caller.GetRealTimeData();

                        if (data.PositionX <= 1000) { CNCpositionX = data.PositionX; }
                        if (data.PositionY <= 1000) { CNCpositionY = data.PositionY; }
                        if (data.PositionZ <= 1000) { CNCpositionZ = data.PositionZ; }
                        if (data.Feedrate <= 5000) { CNCfeedRate = data.Feedrate; }
                        if (data.SpindleSpeed <= 24000) { CNCspindleSpeed = data.SpindleSpeed; }
                    }
                });

            CNCdataReadingThread.Start();
        }

        // Proc: timerWatchDog() *******************************************************************************
        //
        // This proc watches the tracking whether it is stuck or not, if stuck, then stops timer
        // to change it, just update that variable
        //
        private void timerWatchDog()
        {
            Console.WriteLine("Timer started !!!");

            timerWatchDogThread = new Thread(() =>
            {
                while (true)
                {
                    if (timerCounter > 10) 
                    {
                        // Rest it for re-run cases.
                        timerCounter = 0;
                        
                        this.Dispatcher.Invoke(() => 
                        {
                            // Note that this task does not run for STANDALONE test
                            if (!isTestModeDependentOn) tracingStatusButton.Content = "STUCK";
                            else
                            {
                                tracingStatusButton.Content = "TEST COMPLETED";

                                // DEPENDENT Test ended. In case recording is started. so set to the defaults.
                                if (LSV2Caller.GetProcessRecording())
                                {
                                    outputDataFolderButton.Content = "FOLDER SELECTED";
                                    outputDataFolderButton.Background = Brushes.LightGreen;
                                    outputDataFolderButton.Foreground = Brushes.Black;
                                }
                            }

                            tracingStatusButton.Background = Brushes.DarkBlue; 
                        });
                        DoEvents();

                        LSV2Caller.SetProcessRunning(false); // Tell LSV2: read scope data proc that deactive the process
                        LSV2Caller.SetTrackingStarted(false); // Stop storing positions, we do not need them anymore
                        LSV2Caller.SetTrackingEnded(true); // Tell LSV2 that process ended, so record Fa, delFa, delNFa, eta1 and eta2

                        processTimer.Stop(); 
                        timerWatchDogThread.Abort();
                    }

                    timerCounter++;
                    Thread.Sleep(100);
                    //Console.WriteLine(timerCounter);
                }
            });
            timerWatchDogThread.Start();
        }

        //******************************************************************************************************
        // REAL TIME PROJECT EXECUTION PROCEDURES                                                                    
        //
        // Mostly compare the simulated input file and real-time CNC data line by line and tracks the real
        // time tool tip position to be able to control tool breakage and adjustment data
        //
        //******************************************************************************************************
        //
        // Proc: RealtimeSimulationTracking ********************************************************************
        //
        // Compares realtime and simulated data (input data) and finds the mach one, and set it as selected line
        // in the input (simulated) data
        //
        private void InputDataTrackingThread()
        {
            bool isTaken = false;

            //tracingListView.SelectedItems.Clear(); // Clear the selection

            // Set the timer to measure the process elapse time -----------------------------------
            processTimer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 100), DispatcherPriority.Background, t_Tick, Dispatcher.CurrentDispatcher);
            processTimer.IsEnabled = false;
            // --------------------------------------------------------------------------------------

            //
            LSV2Caller.SetTrackingStarted(true); // Start storing positions, we will be using them to catch the positions

            trackingInputDataThread = new Thread(() =>
                {
                    // The first line in the Input data is set
                    InputDataLine data = (InputDataLine)tracingListView.Items[0];

                    // Saved CNC positions
                    DataForGUI savedData;

                    // This is for Real-time graph plotting
                    Style lineColorRed = new Style(typeof(LineDataPoint));
                    lineColorRed.Setters.Add(new Setter(BackgroundProperty, Brushes.Red));
                    lineColorRed.Setters.Add(new Setter(TemplateProperty, null));
                    Style lineColorGreen = new Style(typeof(LineDataPoint));
                    lineColorGreen.Setters.Add(new Setter(BackgroundProperty, Brushes.Green));
                    lineColorGreen.Setters.Add(new Setter(TemplateProperty, null));

                    // At this point, the tracing is already started, means we got the initial CNC position.
                    // Wait CNC to start working and its process X, Y and Z positions come to the first line of Input data file,
                    // Then start processing, so first, we have to catch the first line
                    for (; ; )
                    {
                        // Get the saved CNC positions to compare with the positions in the input data file
                        if (LSV2Caller.receivedCollection.Count > 0)
                        {
                            // Put the received CNC data to the variables, this is first one
                            isTaken = LSV2Caller.receivedCollection.TryTake(out savedData, 1500, cts.Token);

                            if (isTaken)
                            {
                                if ((((Math.Round(Convert.ToDouble(data.posX), 3) - trackTolerans) <= Math.Round(savedData.PositionX, 3))
                                                    && (Math.Round(savedData.PositionX, 3) <= (Math.Round(Convert.ToDouble(data.posX), 3) + trackTolerans)))
                                  && (((Math.Round(Convert.ToDouble(data.posY), 3) - trackTolerans) <= Math.Round(savedData.PositionY, 3))
                                                    && (Math.Round(savedData.PositionY, 3) <= (Math.Round(Convert.ToDouble(data.posY), 3) + trackTolerans)))
                                  && (((Math.Round(Convert.ToDouble(data.posZ), 3) - trackTolerans) <= Math.Round(savedData.PositionZ, 3))
                                                    && (Math.Round(savedData.PositionZ, 3) <= (Math.Round(Convert.ToDouble(data.posZ), 3) + trackTolerans)))
                                   )
                                {
                                    // Caught the first line of Input data file, then start timer, set the status as running.
                                    startTime = DateTime.Now;
                                    processTimer.Start();
                                    timerWatchDog();

                                    this.Dispatcher.Invoke(() =>
                                    {
                                        tracingStatusButton.Content = "RUNNING";
                                        tracingStatusButton.Background = Brushes.Firebrick;
                                        tracingStatusButton.Foreground = Brushes.White;

                                        if (LSV2Caller.GetProcessRecording())
                                        {
                                            outputDataFolderButton.Content = "RECORDING";
                                            outputDataFolderButton.Background = Brushes.Firebrick;
                                            outputDataFolderButton.Foreground = Brushes.White;
                                        }
                                    });

                                    // Calculate Tic period once, but later it will be done every read in LSV2 process data
                                    // Default: 600 if spindle speed is 0
                                    LSV2Caller.CalculateTicPeriod();

                                    // Tell LSV2 that activate the process
                                    LSV2Caller.SetProcessRunning(true);

                                    break;
                                }
                            }
                        }
                    }

                    // Send the simmulated data index to LSV2. This is important for Adaptive control to reference
                    // TFG data while processing. This is the first catch.
                    // NOTE: This is already set when RUN START. This is for safety.
                    LSV2Caller.SetSimDataIndex(0);

                    //Console.WriteLine("Controlling: isForceVsDisMonitoring= " + isForceVsDisMonitoring);
                    // Plot it if monitoring is allowed, first plot
                    //
                    if (isForceVsDisMonitoring)
                    {
                        // Just store the related data, then Monitoring task will read it to plot
                        simForce = Convert.ToDouble(data.force);
                        rtmForce = Convert.ToDouble(savedData.maxFtO);
                        simDelta = 0;

                        ForceSimList.Add(simForce);
                        ForceRtmList.Add(rtmForce);
                        DeltaSimList.Add(simDelta);

                        // Start plotting Task, it stops when process running stops
                        MonitoringAsync();
                    }

                    //// Since we are here, tracking process is started. Set the selected index for scrolling and update
                    //// adjusted data listview with the new feed rate and speed if necessary
                    //this.Dispatcher.Invoke(() =>
                    //    {
                    //        tracingListView.SelectedIndex = 0;
                    //        tracingListView.ScrollIntoView(tracingListView.SelectedItem);
                    //    });

                    // Continue with the second line in a loop to the end of the input data file
                    // -------------------------------------------------------------------------
                    //
                    for (int i = 1; i < tracingListView.Items.Count; i++)
                    {
                        // Get the first item
                        data = (InputDataLine)tracingListView.Items[i];

                        // Try to catch the next input data line, so wait till CNC comes to the line
                        for (; ;)
                        {
                            // Get the saved CNC positions to compare with the positions in the input data file
                            if (LSV2Caller.receivedCollection.Count > 0)
                            {
                                // Put the received CNC data to the variables, this is first one
                                isTaken = LSV2Caller.receivedCollection.TryTake(out savedData, 1500, cts.Token);

                                if (isTaken)
                                {
                                    if ((((Math.Round(Convert.ToDouble(data.posX), 3) - trackTolerans) <= Math.Round(savedData.PositionX, 3))
                                                        && (Math.Round(savedData.PositionX, 3) <= (Math.Round(Convert.ToDouble(data.posX), 3) + trackTolerans)))
                                      && (((Math.Round(Convert.ToDouble(data.posY), 3) - trackTolerans) <= Math.Round(savedData.PositionY, 3))
                                                        && (Math.Round(savedData.PositionY, 3) <= (Math.Round(Convert.ToDouble(data.posY), 3) + trackTolerans)))
                                      && (((Math.Round(Convert.ToDouble(data.posZ), 3) - trackTolerans) <= Math.Round(savedData.PositionZ, 3))
                                                        && (Math.Round(savedData.PositionZ, 3) <= (Math.Round(Convert.ToDouble(data.posZ), 3) + trackTolerans)))
                                       )
                                    {
                                        // while waiting, do nothing
                                        break;
                                    }
                                }
                                else Console.WriteLine("ERROR: Can NOT take data out !!!");
                            }
                            //else Console.WriteLine("Warning: NO data !!!");
                        }

                        //--------------------------------------------------------------------------------
                        // Next line in Input Data is cought, take the required actions
                        //--------------------------------------------------------------------------------

                        // First, let timer watch dog that tracking continues, so this variable MUST be zerod
                        timerCounter = 0;

                        // Send the simmulated data index to LSV2. This is important for Adaptive control to reference
                        // TFG data while processing. Set for every catch.
                        LSV2Caller.SetSimDataIndex(i);

                        // Tracking ListView actions
                        //
                        this.Dispatcher.Invoke(() =>
                        {
                            // This defines the selected item in the ListView
                            tracingListView.SelectedIndex = i;

                            // This defines the scrolling point, constantly after first 5 lines (value is 3)
                            if (tracingListView.Items.Count > i + 4) { tracingListView.ScrollIntoView(tracingListView.Items.GetItemAt(i + 4)); }
                        });

                        // Monitoring actions ------------------------------------------------------------------
                        //
                        if (isForceVsDisMonitoring)
                        {
                            // Just store the related data, then Monitoring task will read it to plot
                            simForce = Convert.ToDouble(data.force);
                            rtmForce = Convert.ToDouble(savedData.maxFtO);
                            simDelta += 0.4;

                            ForceSimList.Add(simForce);
                            ForceRtmList.Add(rtmForce);
                            DeltaSimList.Add(simDelta);
                        }

                        // Real-time Tool Path follower
                        this.Dispatcher.Invoke(() =>
                        {
                            if (i % 10 == 0)
                            {
                                //if (data.TFNC == 1) myFollower.CutPlotLineSeries
                                myFollower.ToolKeyValuePair.Add(new KeyValuePair<double, double>(Convert.ToDouble(data.posX), Convert.ToDouble(data.posY)));
                            }
                        });

                        //--------------------------------------------------------------------------------
                        // TAKE THE ADJUSTMENT ACTIONs
                        //--------------------------------------------------------------------------------

                        // Tool Breakage detection watch-dog
                        //
                        if (LSV2Caller.GetToolBreakageDetected())
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                // Feed rate is already set to 0 by LSV2
                                // Stop all processes like window close or stop running except update data task
                                //
                                // First reset the process is NOT running anymore, because of tool breakage
                                isRunning = false;
                                LSV2Caller.SetProcessRunning(false); // Tell read scope data program that deactive the process

                                // Stop running threads, tasks and timers
                                if (processTimer != null) { processTimer.Stop(); }

                                if (CNCdataReadingThread != null) { CNCdataReadingThread.Abort(); } // Stop reading data from CNC
                                stopCNCDataDisplaying = true; // Stop Displaying CNC data to the window
                                if (trackingInputDataThread != null) { trackingInputDataThread.Abort(); } // Stop data tracking
                                LSV2Caller.SetTrackingStarted(false); // Stop storing positions, we do not need them.

                                tracingStatusButton.Content = "FATAL ERROR";
                                tracingStatusButton.Background = Brushes.Green;
                                tracingStatusButton.Foreground = Brushes.White;
 
                                alarmStatus.Content = "TOOL BREAKAGE";
                                //alarmStatus.Background = Brushes.Red;
                                BlinkAlarm();
                            });

                            break;
                        }

                        // Adaptive Control watch-dog
                        //
                        // implement the code here, send adjusted data to CNC




                    } // End of the Input data file -----------------------------------------------------------------

                    LSV2Caller.SetProcessRunning(false); // Tell LSV2: read scope data prog that deactive the process
                    LSV2Caller.SetTrackingStarted(false); // Stop storing positions, we do not need them anymore
                    LSV2Caller.SetTrackingEnded(true); // Tell LSV2 that process ended, so record Fa, delFa, delNFa, eta1 and eta2

                    // Got the end of Input Data file, so stop tracking and process, stop timer, set
                    // status as completed.
                    processTimer.Stop();
                    timerWatchDogThread.Abort();

                    this.Dispatcher.Invoke(() =>
                    {
                        tracingStatusButton.Content = "COMPLETED";
                        tracingStatusButton.Background = Brushes.LightGreen;
                        tracingStatusButton.Foreground = Brushes.Black;
                       
                        // Tracking is stopped. In case recording is started. so set to the defaults.
                        if (LSV2Caller.GetProcessRecording())
                        {
                            outputDataFolderButton.Content = "FOLDER SELECTED";
                            outputDataFolderButton.Background = Brushes.LightGreen;
                            outputDataFolderButton.Foreground = Brushes.Black;
                        }
                    });
                }); // End of Tracking THREAD

            trackingInputDataThread.Priority = ThreadPriority.Highest;
            trackingInputDataThread.Start();
        }

        private void t_Tick(object sender, EventArgs e)
        {
            TimeSpan dTime = DateTime.Now - startTime;
            //processTimeButton.Content = Convert.ToString(DateTime.Now - startTime);
            processTimeButton.Content = String.Format("{0}:{1}:{2}.{3}",
                dTime.Hours.ToString().PadLeft(2, '0'),
                dTime.Minutes.ToString().PadLeft(2, '0'),
                dTime.Seconds.ToString().PadLeft(2, '0'), 
                dTime.Milliseconds.ToString().PadLeft(3, '0'));
        }

        // Proc: MonitoringAsync -------------------------------------------------------------------------------
        //
        // It monitors the real time data and plots to a graph for both real-time and simulated
        // data to compare them visually. To be able to run this task, process MUST be running state.
        //
        private async void MonitoringAsync()
        {
            Console.WriteLine("Monitoring is started");

            while (LSV2Caller.GetProcessRunning())
            {
                for (int i = 0; i < 10; i++)
                {
                    if (ForceSimList.Count() > 0)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            if (ForceSimList[0] > prevSimForce)
                            {
                                prevSimForce = ForceSimList[0];
                                myMonitorChild.YLinearAxis.Maximum = ForceSimList[0] + 50;
                            }

                            myMonitorChild.SimDataKeyValuePair.Add(new KeyValuePair<double, double>(DeltaSimList[0], ForceSimList[0]));
                            myMonitorChild.RtmDataKeyValuePair.Add(new KeyValuePair<double, double>(DeltaSimList[0], ForceRtmList[0]));

                            if (DeltaSimList[0] > maxDelta)
                            {
                                maxDelta += 20;
                                myMonitorChild.XLinearAxis.Maximum += 20;
                            }
                        });
                        DeltaSimList.RemoveAt(0);
                        ForceSimList.RemoveAt(0);
                        ForceRtmList.RemoveAt(0);
                    }
                    else break;
                }
                                
                await Task.Delay(15);
            }
        }

        //******************************************************************************************************
        //
        // DISPLAY UTULITY PROCs 
        //
        //******************************************************************************************************

        // Proc: connectingDisplayAsync ------------------------------------------------------------------------
        //
        // Displays "CONNECTIONG"string char by char while waiting connection.
        //        
        private async void connectingDisplayAsync()
        { 
            string[] connectingText = new string[10];

            connectingText[0] = "C         ";
            connectingText[1] = "CO        ";
            connectingText[2] = "CON       ";
            connectingText[3] = "CONN      ";
            connectingText[4] = "CONNE     ";
            connectingText[5] = "CONNEC    ";
            connectingText[6] = "CONNECT   ";
            connectingText[7] = "CONNECTI  ";
            connectingText[8] = "CONNECTIN ";
            connectingText[9] = "CONNECTING";

            for (; ; )
            {
                for (int i = 0; i <= 9; i++)
                {
                    connectionCncButton.Content = connectingText[i];
                    await Task.Delay(100);

                    if (isStoped) { break; }
                }

                if (isStoped) { break; }
            }
        }

        // Proc: DisplayCNCData --------------------------------------------------------------------------------
        //
        // Displays the real-time CNC data if CNC is running. Exit to set stopCNCdataReading to true
        //
        private async void DisplayCNCDataAsync()
        {
            for (; ; )
            {
                RealTimeXtextBox.Text = Math.Round(CNCpositionX, 4).ToString();
                RealTimeYtextBox.Text = Math.Round(CNCpositionY, 4).ToString();
                RealTimeZtextBox.Text = Math.Round(CNCpositionZ, 4).ToString();
                RealTimeFeedRateTextBox.Text = Math.Round(CNCfeedRate).ToString();
                RealTimeSpindleTextBox.Text = Math.Round(CNCspindleSpeed).ToString();

                await Task.Delay(100);

                if (stopCNCDataDisplaying) { break; }
            }
        }

        // Blinking the Alarm Button and beeping to take an immediate action
        //
        private async void BlinkAlarm()
        {
            stopErrorBlinking = true;
            while (stopErrorBlinking)
            {
                alarmStatus.Foreground = Brushes.White;
                //alarmStatus.Background = alarmStatus.Background == Brushes.Red ? Brushes.Turquoise : Brushes.Red;
                alarmStatus.Background = alarmStatus.Background == Brushes.Red ? Brushes.Gold : Brushes.Red;
                Console.Beep();
                await Task.Delay(500);

            }
        }
       
        // This is needed if you update any button info and expect the change takes an action immediatelly, invoke this.
        //
        public void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(ExitFrame), frame);
            Dispatcher.PushFrame(frame);
        }

        public object ExitFrame(object f)
        {
            ((DispatcherFrame)f).Continue = false;

            return null;
        }

        // it changes specified button content for 2 secs.
        //
        private async void UpdateButtonContent(Button myButton, string status)
        {
            string tempStatus = (string)myButton.Content;
            Brush tempColor = myButton.Background;

            myButton.Background = Brushes.LightPink;
            myButton.Content = status;
            DoEvents();
            await Task.Delay(2000);
            //myButton.Background = Brushes.LightGray;
            myButton.Background = tempColor;
            myButton.Content = tempStatus;
        }

        //******************************************************************************************************
        //
        // Event Handler PROCs 
        //
        //******************************************************************************************************
        //
        private void TestBorder_MouseMove(object sender, MouseEventArgs e)
        {
           var bc = new BrushConverter();
            TestOnBorder.Background = (Brush)bc.ConvertFrom("#FFBEE6F9");
        }

        private void TestBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            TestOnBorder.Background = Brushes.Transparent;
        }

        private void SimDataBorder_MouseMove(object sender, MouseEventArgs e)
        {
            var bc = new BrushConverter();
            SimDataBorder.Background = (Brush)bc.ConvertFrom("#FFBEE6F9");
        }

        private void SimDataBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            SimDataBorder.Background = Brushes.Transparent;
        }

        //******************************************************************************************************
        //
        // Other TEST PROCs 
        //
        //******************************************************************************************************
        //
        private void SetFeedRateButton_Click(object sender, RoutedEventArgs e)
        {
            LSV2Caller.SetFeedRatePercent(Convert.ToDouble(SetFeedRateTextBox.Text));
            Console.WriteLine("Feed rate change request: " + SetFeedRateTextBox.Text);
        }

        private void SetToolBreakage_Click(object sender, RoutedEventArgs e)
        {
            LSV2Caller.SetToolBreakageDetected(true); // Now it sets to True for test
        }

        private void SetSpindleSpeedButton_Click(object sender, RoutedEventArgs e)
        {
            LSV2Caller.SetSpindleSpdPercent(Convert.ToDouble(SetSpindleSpeedTextBox.Text));
            Console.WriteLine("Spindle Speed change request: " + SetSpindleSpeedTextBox.Text);
        }

        private void SetSpindleSpdByValButton_Click(object sender, RoutedEventArgs e)
        {
            // No development is present, most probably no capability

            //Console.WriteLine("Number: " + double.Parse(SetSpindleSpdByValTextBox.Text));
            //LSV2Caller.CalculateTicPeriod();
            //Console.WriteLine("Tic Period: " + LSV2Caller.ticPeriod);

            //var regex = new Regex(@"^[0-9]*(?:\.[0-9]*)?$");
            //Console.WriteLine(regex.IsMatch("blah"));
            //Console.WriteLine(regex.IsMatch("12.3"));
            //Console.WriteLine(regex.IsMatch("12.3.4"));

            Console.WriteLine("Customized threshold: " + LSV2Caller.GetThresholdPercentage());

            Console.WriteLine("Culculated threshold: " + LSV2Caller.TBPercentage);
        }

        private void tabControlMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TrackingTabItem != null && TrackingTabItem.IsSelected)
            {
                Console.WriteLine("Tracking tab item selected");
                //if (myMonitorChild != null) myMonitorChild.LineChart.DataContext = null;
            }

            if (MonitoringTabItem != null && MonitoringTabItem.IsSelected)
            {
                Console.WriteLine("Monitoring tab item selected");
                //if (myMonitorChild != null)
                //{
                //    //myMonitorChild.LineChart.DataContext = null;
                //    myMonitorChild.LineChart.DataContext = myMonitorChild.MonitoringPlot;
                //}
            }
        }

        private void Simulator_Click(object sender, RoutedEventArgs e)
        {
            if (myMonitorChild != null)
            {
                myMonitorChild.SimDataKeyValuePair.Add(new KeyValuePair<double, double>(Convert.ToDouble(SimX.Text), Convert.ToDouble(SimY.Text)));
            }
        }

        private void Realtime_Click(object sender, RoutedEventArgs e)
        {
            if (myMonitorChild != null)
            {
                myMonitorChild.RtmDataKeyValuePair.Add(new KeyValuePair<double, double>(Convert.ToDouble(RtmX.Text), Convert.ToDouble(RtmY.Text)));
            }
        }

        private void SetSpindleSpdByValTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = (TextBox)sender;
            string dene = "3";

            if (txt.Text == dene) Console.WriteLine("New Text: : " + txt.Text);
        }

    } // End of Main Class
}
