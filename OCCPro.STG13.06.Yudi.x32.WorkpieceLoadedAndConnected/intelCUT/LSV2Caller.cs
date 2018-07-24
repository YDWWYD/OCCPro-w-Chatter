using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.IO;

namespace OnlineCuttingControlProcess
{
    internal class LSV2Caller
    {
        /**********************************************************************************************************/
        // 
        // The following definitions are defined before and required
        // 
        /**********************************************************************************************************/
        // 
        public static MainWindow mainWindow = null;

        const string lsv2dll = "LSV2D32wd.dll";
        //const string lsv2dll = "LSV2D32w.dll";
        const int MAXDIMNAME = 16;

        static int channels; // number of channels are kept, used for dada received channels from CNC

        private static CancellationTokenSource cts;

        //internal static BlockingCollection<ScopeData> receivedCollection = new BlockingCollection<ScopeData>();
        internal static BlockingCollection<DataForGUI> receivedCollection = new BlockingCollection<DataForGUI>();

        internal static HeidenhainDNC dncConnection;

        static double[,] P;

        // Motor Parameters //
        const double KtX = 1.964;
        const double KtY = 1.964;
        const double KtZ = 1.964;
        const double KtC = 1.522;
        const double KtA = 2.106;
        const double KtS = 1.571;
        const double Pitch = 12;
        const double TtoF = (2 * Math.PI) / (Pitch * 1e-3);
        const double FtoT = (Pitch * 1e-3) / (2 * Math.PI);

        static double Potentiometer = 100;
        const double ProgramFeed = 400;
        //const double ProgramFeed = 175;
        static double CurrentFeed = ProgramFeed * (Potentiometer / 100);
        static double NewFeed = CurrentFeed;
        static double def = 0;
        static int Cutting = 0;


        // Customization table (changed by Tayfun)
        //
        // GPC initialization
        static double lambda = 0.2;
        internal static double GetLambda() { return lambda; }
        internal static void SetLambda(double value) { lambda = value; }
        static double abarvalue = 1;
        internal static double GetAbarvalue() { return abarvalue; }
        internal static void SetAbarvalue(double value) { abarvalue = value; }
        static double abar = abarvalue;
        static double r = 350; // reference force [N]
        internal static double GetGPCforce() { return r; }
        internal static void SetGPCforce(double value) { r = value; }
        //
        // Constants: Mostly Related with Adaptive control
        const int Nsteps = 500000;
        const int umin = (int)(ProgramFeed * 0.2); // mm/min
        const int umax = (int)(ProgramFeed * 1.5); // mm/min
        //const int N1 = 1;
        const int N2 = 4;
        //const int Nu = 1;
        const double InitParam = 0.01;
        const int na = 2;
        const int nb = 2;
        const double c1 = 0.01;
        const double c2 = 0.001;
        //const double abarvalue = c2; // Changed by DENIZ
        const int usize = Nsteps + N2;

        static double y2 = 0;
        static double y3 = y2;
        static double yy2 = 0;
        static double yy3 = 0;
        static double[] BH = new double[3] { 0, 0, 0 };
        static double[] AH = new double[2] { 0, 0 };
        static double[] TetaRLS = new double[6] { 0, 0, 0, 0, 0, 0 };
        static double[] c = new double[3] { 0, 0, 0 };

        static double[] BHold = new double[3] { 0.01, 0.01, 0.01 };
        static double[] AHold = new double[2] { 0.01, 0.01 };
        static double[] Theta = new double[5] { InitParam, InitParam, InitParam, InitParam, InitParam };

        static double a1, a2, a3, b1, b2, b3;
        static double r1, r2, r3;
        static double g0, g1, g2, g3;
        static double[] f1 = new double[3] { 0, 0, 0 };
        static double[] f2 = new double[3] { 0, 0, 0 };
        static double[] f3 = new double[3] { 0, 0, 0 };
        static double[] f4 = new double[3] { 0, 0, 0 };
        static double[] f = new double[4] { 0, 0, 0, 0 };
        //static double[] fupdt = new double[4] { 0, 0, 0, 0 };
        //static double[] error_ff = new double[4] { 0, 0, 0, 0 };
        //static double[] Num = new double[4] { 0, 0, 0, 0 };
        //static double[] Denum = new double[3] { 0, 0, 0 };

        // These are required for data receiving and process
        static double MeanTorqueS = 0;
        static int SpindlePeriodCount = 1 + N2 - 1;
        internal static void InitializeSpindlePeriodCount() { SpindlePeriodCount = 1 + N2 - 1; }

        static double PrevPX = 0;
        static double PrevPY = 0;
        static double PrevPZ = 0;
        static double PrevFeedV = 100;
        static double PrevSpindleS = 0;
        static double PrevTorque_XV = 0;
        static double PrevTorque_YV = 0;
        static double PrevTorque_ZV = 0;
        static double PrevTorque_SV = 0;
        static double PZ;
        static double PX;
        static double PY;
        static double FeedV;
        static double SpindleSpeedRead;
        static double Torque_XV;
        static double Torque_YV;
        static double Torque_ZV;
        static double Torque_SV;
        static double Force;

        static double MaxSpindleTorque = 0;

        static double[] u = new double[usize];
        static double[] y = new double[usize];
        static double[] du = new double[usize];
        static double error;

        static double[] Phi_k = new double[5];
 
        /**********************************************************************************************************/
        // 
        // The following definitions are added by TAYFUN for onDataReceice proc. 
        // 
        /**********************************************************************************************************/
        //
        //const double numbOfSamplingPerSec = 10000; // Default is 10Khz. (0.1 msec.)
        private static double numbOfSamplingPerSec = 10000; // Default is 10Khz. (0.1 msec.)

        private static double ticPeriod = 600; // Number of tick, Default is 600
        internal static void CalculateTicPeriod() 
        {
            if (SpindleSpeedRead != 0)
            {
                // First, calculate the # of samplings from Customization datafill, it is samplingTime (msec)
                numbOfSamplingPerSec = (1 / samplingTime) * 1000;
                
                // SpindleSpeedRead     = # of spindle turns in 1 min.
                // numbOfSamplingPerSec = 10Khz x 60 secs. == 600000 samplings for 1 min.
                ticPeriod = Math.Round((numbOfSamplingPerSec * 60) / SpindleSpeedRead); 
                ticPeriod = (ticPeriod % 2 == 0) ? ticPeriod : (ticPeriod + 1);
            }
        }

        // This is for processing the data, being processed ticPeriod by ticPeriod, then deleted
        private static List<ScopeData> RealTimeProcessData = new List<ScopeData>();
        // This is for filing the data to the storage, being processed 1 by 1, then deleted
        private static List<DataForRecording> RealTimeRecordingData = new List<DataForRecording>();
        // This is for processing the data, being processed ticPeriod by ticPeriod, then deleted
        public static List<ScopeData> RealTimeTestData = new List<ScopeData>();
        //internal static void SetRealTimeTestData(ScopeData data) { RealTimeTestData.Add(data); }
        private static List<double> Force_Y = new List<double>();
        private static List<double> FeedR_U = new List<double>();
        private static List<double> Force_C = new List<double>();
        private static List<double> FeedR_C = new List<double>();
        private static List<double> Kt_Updated = new List<double>();
        private static List<double> F_Simulated = new List<double>();

        // This is to lock some works to prevent any recording and saving errors
        private static object multiThread = new object();

        private static double SumFirstTooth;
        private static double SumSecondTooth;
        private static double TheValue;
        static int ToolBreakageIteration = 1;

        private static int totalCount = 0; // This is for data record counting for every record in a tracking period
        internal static void SetTotalCountDefault() { totalCount = 0; } // Set totalCount default, zero, whenever start running        

        // Process control parameters
        private static bool isAdaptiveControlActive = true;
        internal static bool GetAdaptiveControl() { return isAdaptiveControlActive; }
        internal static void SetAdaptiveControl(bool status) { isAdaptiveControlActive = status; }
        private static bool isToolBreakageActive = true; // Being TESTED ------
        internal static bool GetToolBreakage() { return isToolBreakageActive; }
        internal static void SetToolBreakage(bool status) { isToolBreakageActive = status; }
        private static bool VirtualRealConnection = true;
        internal static bool GetVirtualRealConnection() { return VirtualRealConnection; }
        internal static void SetVirtualRealConnection(bool status) { VirtualRealConnection = status; }
        private static bool isFeedbacktoReal = true;
        internal static bool GetFeedbacktoReal() { return isFeedbacktoReal; }
        internal static void SetFeedbacktoReal(bool status) { isFeedbacktoReal = status; }

        // Customization variables
        private static double samplingTime = 0.1;
        internal static double GetSamplingTime() { return samplingTime; }
        internal static void SetSamplingTime(double value) { samplingTime = value; }
        private static double spindleExhaustion = 0.05;
        internal static double GetSpindleExhaustion() { return spindleExhaustion; }
        internal static void SetSpindleExhaustion(double value) { spindleExhaustion = value; }
        private static double maxSpindleTorqVal = 0.75;
        internal static double GetMaxSpindleTorqVal() { return maxSpindleTorqVal; }
        internal static void SetMaxSpindleTorqVal(double value) { maxSpindleTorqVal = value; }
        //internal static double GetForcePrediction() { return forcePrediction; }
        //internal static void SetForcePrediction(double value) { forcePrediction = value; }

        // Cutting Force Coefficient Related
        internal static int KtUpdateStepNumber = 0;
        internal static int Ktstep = 0;
        internal static double KtUP = 0;
        internal static double KtUPSum = 0;

        // Tool Breakage storages & references
        private static List<double> Fa = new List<double>();
        //private static List<double> Fa2 = new List<double>();
        private static List<double> delFa = new List<double>();
        private static List<double> delNFa = new List<double>();
        private static List<double> eta1 = new List<double>();
        private static List<double> eta2 = new List<double>();
        private static List<double> B1_TB = new List<double>();
        private static List<double> B2_TB = new List<double>();
        private static double eta1Reference = 1000;
        internal static double GetEta1Reference() { return eta1Reference; }
        internal static void SetEta1Reference(double refVal) { eta1Reference = refVal; }
        private static double eta2Reference = 1000;
        internal static double GetEta2Reference() { return eta2Reference; }
        internal static void SetEta2Reference(double refVal) { eta2Reference = refVal; }
        //private enum toolbreakageActionType { alarmAndStop, alarmAndContinue, alarmAndDelayStop };
        private static toolBreakageActionType toolBreakageAction = toolBreakageActionType.alarmAndStop;
        internal static toolBreakageActionType GetToolBreakageAction() { return toolBreakageAction; }
        internal static void SetToolBreakageAction(toolBreakageActionType passedVal) { toolBreakageAction = passedVal; }
        private static int thresholdPercentage = 40; // percentage
        internal static int GetThresholdPercentage() { return thresholdPercentage; }
        internal static void SetThresholdPercentage(int refVal) { thresholdPercentage = refVal; TBPercentage = thresholdPercentage / 100.0; }

        // Low pass filtering
        private static List<double> LP_TS = new List<double>();
        private static double alpha;

        // Adaptive control storage & references
        internal static int FutureCheck = 0;
        internal static double futureCheckTime = 0.5;
        internal static double GetFeatureCheckTime() { return futureCheckTime; }
        internal static void SetFeatureCheckTime(double refVal) { futureCheckTime = refVal; }
        internal static int CompensationCheck = 0;
        internal static double CompensationCheckTime = 0.15;
        internal static double GetCompCheckTime() { return CompensationCheckTime; }
        internal static void SetCompCheckTime(double refVal) { CompensationCheckTime = refVal; }
        private static int simDataIndex = 0;
        internal static void SetSimDataIndex(int simDataInt) { simDataIndex = simDataInt; }
        private static int simDataCount = 0;
        internal static void SetSimDataCount() { simDataCount = TFGMPro.Count; }
        //internal static int GetSimDataCount() { return TFGMPro.Count; }
        private static List<double> TFGMPro = new List<double>();
        internal static void AddDataToTFG(double simDataDouble) { TFGMPro.Add(simDataDouble); }
        internal static void ClearDataTFG() { TFGMPro.Clear(); }
        private static List<double> TFNCMPro = new List<double>();
        internal static void AddDataToTFNC(double simDataDouble) { TFNCMPro.Add(simDataDouble); }
        internal static void ClearDataTFNC() { TFNCMPro.Clear(); }
        private static double deltaDistance = 0;
        internal static void SetDeltaDistance(double delta) { deltaDistance = delta; }
        private static double forceOvershootTolPercent = 15;
        internal static double GetForceOvershootTolPer() { return forceOvershootTolPercent; }
        internal static void SetForceOvershootTolPer(double refVal) { forceOvershootTolPercent = refVal; forceOverShoot = (100.0 + forceOvershootTolPercent) / 100.0; }
        private static double goToCheckTime = 0.5;
        internal static double GetGoToCheckTime() { return goToCheckTime; }
        internal static void SetGoToCheckTime(double refVal) { goToCheckTime = refVal; }
        private static double delayCompansationTime = 0.15;
        internal static double GetDelayCompTime() { return delayCompansationTime; }
        internal static void SetDelayCompTime(double refVal) { delayCompansationTime = refVal; }

        // Calculation of percentage
        public static double forceOverShoot = (100.0 + forceOvershootTolPercent) / 100.0;
        public static double TBPercentage = thresholdPercentage / 100.0;

        // Tool and Process Information 
        private static double toolLength = 0.0;
        internal static void SetToolLenght(double lenght) { toolLength = lenght; }
        private static double ToolRadius;
        internal static void SetToolDiameter(double diameter) { ToolRadius = (diameter / 2) * 1e-3; } // meters
        private static int numbOfToolTeeth;
        internal static void SetToolTeethNub(int teethNumb) { numbOfToolTeeth = teethNumb; }
        
        // Process declarations
        private static bool isProcessRunning = false;
        internal static bool GetProcessRunning() { return isProcessRunning; }
        internal static void SetProcessRunning(bool running) { isProcessRunning = running; }
        private static bool isToolBreakageDetected = false;
        internal static bool GetToolBreakageDetected() { return isToolBreakageDetected; }
        internal static void SetToolBreakageDetected(bool status) { isToolBreakageDetected = status; }
        private static bool isTrackingStarted = false;
        internal static bool GetTrackingStarted() { return isTrackingStarted; }
        internal static void SetTrackingStarted(bool tracking) { isTrackingStarted = tracking; }

        // Graph Plotting
        public static Thread dataPlottingThread = null;

        // Force control
        private static bool isPeekForceUsed = true;
        internal static bool GetIsPeekForceUsed() { return isPeekForceUsed; }
        internal static void SetIsPeekForceUsed(bool forceType) { isPeekForceUsed = forceType; }

        // Recording data
        private static bool isRecordingData = false;
        internal static bool GetProcessRecording() { return isRecordingData; }
        internal static void SetProcessRecording(bool recording) { isRecordingData = recording; }
        private static bool onceRun = true; // This is the control of invoking dataRecordingAsync Task ONCE
        internal static void SetOnceRun(bool runOnce) { onceRun = runOnce; }
        private static Task dataRecordingTask = null;
        private static bool isTrackingEnded = false; // Will be used the record Fa, delFa, delNFa, eta1 and eta2 data to files.
        internal static void SetTrackingEnded(bool trackingEnded) { isTrackingEnded = trackingEnded; }

        // recordingFileDir set & get methods.
        private static string recordingFileDir = ""; // Folder name will be set here from GUI
        internal static void SetRecordFileDirectory(string fileDir) { recordingFileDir = fileDir; }
        internal static string GetRecordFileDirectory() { return recordingFileDir; }

        // Recording File names
        private static string fileNameMain = null;
        private static string fileNameCalc = null;
        private static string fileNameTool = null;

        // Testing mode
        private static bool isDependentTestOn = false;
        private static int dependentTestCount = 0;
        internal static bool GetDependentTestMode() { return isDependentTestOn; }
        internal static void SetDependentTestMode(bool status) { isDependentTestOn = status; dependentTestCount = 0; }
        private static bool isRecordingTaskRequired = true; // It is not required for Standalone test mode
        private static bool isSimInputDataLoaded = false;
        internal static bool GetSimInputDataLoaded() { return isSimInputDataLoaded; }
        internal static void SetSimInputDataLoaded(bool loaded) { isSimInputDataLoaded = loaded; }
 
        // Feed rate and spindle speed control for Adaptive control
        static double feedRatePercent = 100; // default, means don't change it
        internal static double GetFeedRatePercent() { return feedRatePercent; }
        internal static void SetFeedRatePercent(double status) { feedRatePercent = status; }
        static double spindleSpeedPercent = 100;
        internal static double GetSpindleSpdPercent() { return spindleSpeedPercent; }
        internal static void SetSpindleSpdPercent(double status) { spindleSpeedPercent = status; }

        // Deactivate ControlCNCTask
        static bool stopCNCcontrolTask = false;
        internal static bool GetCNCcontrol() { return stopCNCcontrolTask; }
        internal static void StopCNCcontrol() { stopCNCcontrolTask = true; }
        

        // Kalman Filtering Axis activation status definitions
        private static bool isKalmanAxisXActive = false;
        private static bool isKalmanAxisYActive = false;
        private static bool isKalmanAxisZActive = false;

        // Kalman Filtering Axis activation status GET & SET
        internal static bool GetKalmanAxisXActive() { return isKalmanAxisXActive; }
        internal static void SetKalmanAxisXActive(bool status) { isKalmanAxisXActive = status; }
        internal static bool GetKalmanAxisYActive() { return isKalmanAxisYActive; }
        internal static void SetKalmanAxisYActive(bool status) { isKalmanAxisYActive = status; }
        internal static bool GetKalmanAxisZActive() { return isKalmanAxisZActive; }
        internal static void SetKalmanAxisZActive(bool status) { isKalmanAxisZActive = status; }

        // KALMAN Filtering axis definitions: X axis -------------------------------
        //
        private static int maxState_n_X = 0; // Index: State (n) for X
        private static int maxMeasurement_m_X = 0; // Index: Measurement (m) for X

        internal static int GetMaxState_n_X() { return maxState_n_X; }
        internal static void SetMaxState_n_X(int count) { maxState_n_X = count; }
        internal static int GetMaxMeasurement_m_X() { return maxMeasurement_m_X; }
        internal static void SetMaxMeasurement_m_X(int count) { maxMeasurement_m_X = count; }

        static double[,] Kalman_A_X;
        static double[,] Kalman_P_X;
        static double[,] Kalman_Q_X;
        static double[,] Kalman_H_X;
        static double[,] Kalman_R_X;
        static double[,] Kalman_K_Fixed_X;
        static double[,] Kalman_K_FixedT_X;
        static double[,] m_X;
        static double[,] m2_X;
        static double IM_X = 0;
        static List<double> CompensatedX = new List<double>();

        // KALMAN Filtering axis definitions: Y axis -------------------------------
        //
        private static int maxState_n_Y = 0; // Index: State (n) for Y
        private static int maxMeasurement_m_Y = 0; // Index: Measurement (m) for Y

        internal static int GetMaxState_n_Y() { return maxState_n_Y; }
        internal static void SetMaxState_n_Y(int count) { maxState_n_Y = count; }
        internal static int GetMaxMeasurement_m_Y() { return maxMeasurement_m_Y; }
        internal static void SetMaxMeasurement_m_Y(int count) { maxMeasurement_m_Y = count; }

        static double[,] Kalman_A_Y;
        static double[,] Kalman_P_Y;
        static double[,] Kalman_Q_Y;
        static double[,] Kalman_H_Y;
        static double[,] Kalman_R_Y;
        static double[,] Kalman_K_Fixed_Y;
        static double[,] Kalman_K_FixedT_Y;
        static double[,] m_Y;
        static double[,] m2_Y;
        static double IM_Y = 0;
        static List<double> CompensatedY = new List<double>();

        // KALMAN Filtering axis definitions: Z axis ------------------------------
        //
        private static int maxState_n_Z = 0; // Index: State (n) for Z
        private static int maxMeasurement_m_Z = 0; // Index: Measurement (m) for Z

        internal static int GetMaxState_n_Z() { return maxState_n_Z; }
        internal static void SetMaxState_n_Z(int count) { maxState_n_Z = count; }
        internal static int GetMaxMeasurement_m_Z() { return maxMeasurement_m_Z; }
        internal static void SetMaxMeasurement_m_Z(int count) { maxMeasurement_m_Z = count; }

        static double[,] Kalman_A_Z;
        static double[,] Kalman_P_Z;
        static double[,] Kalman_Q_Z;
        static double[,] Kalman_H_Z;
        static double[,] Kalman_R_Z;
        static double[,] Kalman_K_Fixed_Z;
        static double[,] Kalman_K_FixedT_Z;

        // Kalman Data writing to X-Matrix Prog
        internal static void KalmanMatrixDataWritting_X(KalmanMatrixType matrixType, double[,] localArray)
        {
            switch (matrixType)
            {
                case KalmanMatrixType.A:
                    Kalman_A_X = new double[maxState_n_X, maxState_n_X];
                    for (int i = 0; i < maxState_n_X; i++) { for (int j = 0; j < maxState_n_X; j++) { Kalman_A_X[i, j] = localArray[i, j]; } }
                    break;
                case KalmanMatrixType.P:
                    Kalman_P_X = new double[maxState_n_X, maxState_n_X];
                    for (int i = 0; i < maxState_n_X; i++) { for (int j = 0; j < maxState_n_X; j++) { Kalman_P_X[i, j] = localArray[i, j]; } }
                    break;
                case KalmanMatrixType.Q:
                    Kalman_Q_X = new double[maxState_n_X, maxState_n_X];
                    for (int i = 0; i < maxState_n_X; i++) { for (int j = 0; j < maxState_n_X; j++) { Kalman_Q_X[i, j] = localArray[i, j]; } }
                    break;
                case KalmanMatrixType.H:
                    Kalman_H_X = new double[maxMeasurement_m_X, maxState_n_X];
                    for (int i = 0; i < maxMeasurement_m_X; i++) { for (int j = 0; j < maxState_n_X; j++) { Kalman_H_X[i, j] = localArray[i, j]; } }
                    break;
                case KalmanMatrixType.R:
                    Kalman_R_X = new double[maxMeasurement_m_X, maxMeasurement_m_X];
                    for (int i = 0; i < maxMeasurement_m_X; i++) { for (int j = 0; j < maxMeasurement_m_X; j++) { Kalman_R_X[i, j] = localArray[i, j]; } }
                    break;
                case KalmanMatrixType.K:
                    Kalman_K_Fixed_X = new double[maxState_n_X, maxMeasurement_m_X];
                    for (int i = 0; i < maxState_n_X; i++) { for (int j = 0; j < maxMeasurement_m_X; j++) { Kalman_K_Fixed_X[i, j] = localArray[i, j]; } }
                    break;
            }               
        }

        // Kalman Data reading from X-Matrix Prog
        internal static void KalmanMatrixDataReading_X(KalmanMatrixType matrixType, out double[,] localArray )
        {
            localArray = null;

            switch (matrixType)
            {
                case KalmanMatrixType.A:
                    localArray = new double[maxState_n_X, maxState_n_X];
                    for (int i = 0; i < maxState_n_X; i++) { for (int j = 0; j < maxState_n_X; j++) { localArray[i, j] = Kalman_A_X[i, j]; } }
                    break;
                case KalmanMatrixType.P:
                    localArray = new double[maxState_n_X, maxState_n_X];
                    for (int i = 0; i < maxState_n_X; i++) { for (int j = 0; j < maxState_n_X; j++) { localArray[i, j] = Kalman_P_X[i, j]; } }
                    break;
                case KalmanMatrixType.Q:
                    localArray = new double[maxState_n_X, maxState_n_X];
                    for (int i = 0; i < maxState_n_X; i++) { for (int j = 0; j < maxState_n_X; j++) { localArray[i, j] = Kalman_Q_X[i, j]; } }
                    break;
                case KalmanMatrixType.H:
                    localArray = new double[maxMeasurement_m_X, maxState_n_X];
                    for (int i = 0; i < maxMeasurement_m_X; i++) { for (int j = 0; j < maxState_n_X; j++) { localArray[i, j] = Kalman_H_X[i, j]; } }
                    break;
                case KalmanMatrixType.R:
                    localArray = new double[maxMeasurement_m_X, maxMeasurement_m_X];
                    for (int i = 0; i < maxMeasurement_m_X; i++) { for (int j = 0; j < maxMeasurement_m_X; j++) { localArray[i, j] = Kalman_R_X[i, j]; } }
                    break;
                case KalmanMatrixType.K:
                    localArray = new double[maxState_n_X, maxMeasurement_m_X];
                    for (int i = 0; i < maxState_n_X; i++) { for (int j = 0; j < maxMeasurement_m_X; j++) { localArray[i, j] = Kalman_K_Fixed_X[i, j]; } }
                    break;
            }
        }

        // Kalman Data writing to Y-Matrix Prog
        internal static void KalmanMatrixDataWritting_Y(KalmanMatrixType matrixType, double[,] localArray)
        {
            switch (matrixType)
            {
                case KalmanMatrixType.A:
                    Kalman_A_Y = new double[maxState_n_Y, maxState_n_Y];
                    for (int i = 0; i < maxState_n_Y; i++) { for (int j = 0; j < maxState_n_Y; j++) { Kalman_A_Y[i, j] = localArray[i, j]; } }
                    break;
                case KalmanMatrixType.P:
                    Kalman_P_Y = new double[maxState_n_Y, maxState_n_Y];
                    for (int i = 0; i < maxState_n_Y; i++) { for (int j = 0; j < maxState_n_Y; j++) { Kalman_P_Y[i, j] = localArray[i, j]; } }
                    break;
                case KalmanMatrixType.Q:
                    Kalman_Q_Y = new double[maxState_n_Y, maxState_n_Y];
                    for (int i = 0; i < maxState_n_Y; i++) { for (int j = 0; j < maxState_n_Y; j++) { Kalman_Q_Y[i, j] = localArray[i, j]; } }
                    break;
                case KalmanMatrixType.H:
                    Kalman_H_Y = new double[maxMeasurement_m_Y, maxState_n_Y];
                    for (int i = 0; i < maxMeasurement_m_Y; i++) { for (int j = 0; j < maxState_n_Y; j++) { Kalman_H_Y[i, j] = localArray[i, j]; } }
                    break;
                case KalmanMatrixType.R:
                    Kalman_R_Y = new double[maxMeasurement_m_Y, maxMeasurement_m_Y];
                    for (int i = 0; i < maxMeasurement_m_Y; i++) { for (int j = 0; j < maxMeasurement_m_Y; j++) { Kalman_R_Y[i, j] = localArray[i, j]; } }
                    break;
                case KalmanMatrixType.K:
                    Kalman_K_Fixed_Y = new double[maxState_n_Y, maxMeasurement_m_Y];
                    for (int i = 0; i < maxState_n_Y; i++) { for (int j = 0; j < maxMeasurement_m_Y; j++) { Kalman_K_Fixed_Y[i, j] = localArray[i, j]; } }
                    break;
            }
        }

        // Kalman Data reading from Y-Matrix Prog
        internal static void KalmanMatrixDataReading_Y(KalmanMatrixType matrixType, out double[,] localArray)
        {
            localArray = null;

            switch (matrixType)
            {
                case KalmanMatrixType.A:
                    localArray = new double[maxState_n_Y, maxState_n_Y];
                    for (int i = 0; i < maxState_n_Y; i++) { for (int j = 0; j < maxState_n_Y; j++) { localArray[i, j] = Kalman_A_Y[i, j]; } }
                    break;
                case KalmanMatrixType.P:
                    localArray = new double[maxState_n_Y, maxState_n_Y];
                    for (int i = 0; i < maxState_n_Y; i++) { for (int j = 0; j < maxState_n_Y; j++) { localArray[i, j] = Kalman_P_Y[i, j]; } }
                    break;
                case KalmanMatrixType.Q:
                    localArray = new double[maxState_n_Y, maxState_n_Y];
                    for (int i = 0; i < maxState_n_Y; i++) { for (int j = 0; j < maxState_n_Y; j++) { localArray[i, j] = Kalman_Q_Y[i, j]; } }
                    break;
                case KalmanMatrixType.H:
                    localArray = new double[maxMeasurement_m_Y, maxState_n_Y];
                    for (int i = 0; i < maxMeasurement_m_Y; i++) { for (int j = 0; j < maxState_n_Y; j++) { localArray[i, j] = Kalman_H_Y[i, j]; } }
                    break;
                case KalmanMatrixType.R:
                    localArray = new double[maxMeasurement_m_Y, maxMeasurement_m_Y];
                    for (int i = 0; i < maxMeasurement_m_Y; i++) { for (int j = 0; j < maxMeasurement_m_Y; j++) { localArray[i, j] = Kalman_R_Y[i, j]; } }
                    break;
                case KalmanMatrixType.K:
                    localArray = new double[maxState_n_Y, maxMeasurement_m_Y];
                    for (int i = 0; i < maxState_n_Y; i++) { for (int j = 0; j < maxMeasurement_m_Y; j++) { localArray[i, j] = Kalman_K_Fixed_Y[i, j]; } }
                    break;
            }
        }

        // Kalman Data writing to Z-Matrix Prog
        internal static void KalmanMatrixDataWritting_Z(KalmanMatrixType matrixType, double[,] localArray)
        {
            switch (matrixType)
            {
                case KalmanMatrixType.A:
                    Kalman_A_Z = new double[maxState_n_Z, maxState_n_Z];
                    for (int i = 0; i < maxState_n_Z; i++) { for (int j = 0; j < maxState_n_Z; j++) { Kalman_A_Z[i, j] = localArray[i, j]; } }
                    break;
                case KalmanMatrixType.P:
                    Kalman_P_Z = new double[maxState_n_Z, maxState_n_Z];
                    for (int i = 0; i < maxState_n_Z; i++) { for (int j = 0; j < maxState_n_Z; j++) { Kalman_P_Z[i, j] = localArray[i, j]; } }
                    break;
                case KalmanMatrixType.Q:
                    Kalman_Q_Z = new double[maxState_n_Z, maxState_n_Z];
                    for (int i = 0; i < maxState_n_Z; i++) { for (int j = 0; j < maxState_n_Z; j++) { Kalman_Q_Z[i, j] = localArray[i, j]; } }
                    break;
                case KalmanMatrixType.H:
                    Kalman_H_Z = new double[maxMeasurement_m_Z, maxState_n_Z];
                    for (int i = 0; i < maxMeasurement_m_Z; i++) { for (int j = 0; j < maxState_n_Z; j++) { Kalman_H_Z[i, j] = localArray[i, j]; } }
                    break;
                case KalmanMatrixType.R:
                    Kalman_R_Z = new double[maxMeasurement_m_Z, maxMeasurement_m_Z];
                    for (int i = 0; i < maxMeasurement_m_Z; i++) { for (int j = 0; j < maxMeasurement_m_Z; j++) { Kalman_R_Z[i, j] = localArray[i, j]; } }
                    break;
                case KalmanMatrixType.K:
                    Kalman_K_Fixed_Z = new double[maxState_n_Z, maxMeasurement_m_Z];
                    for (int i = 0; i < maxState_n_Z; i++) { for (int j = 0; j < maxMeasurement_m_Z; j++) { Kalman_K_Fixed_Z[i, j] = localArray[i, j]; } }
                    break;
            }
        }

        // Kalman Data reading from Z-Matrix Prog
        internal static void KalmanMatrixDataReading_Z(KalmanMatrixType matrixType, out double[,] localArray)
        {
            localArray = null;

            switch (matrixType)
            {
                case KalmanMatrixType.A:
                    localArray = new double[maxState_n_Z, maxState_n_Z];
                    for (int i = 0; i < maxState_n_Z; i++) { for (int j = 0; j < maxState_n_Z; j++) { localArray[i, j] = Kalman_A_Z[i, j]; } }
                    break;
                case KalmanMatrixType.P:
                    localArray = new double[maxState_n_Z, maxState_n_Z];
                    for (int i = 0; i < maxState_n_Z; i++) { for (int j = 0; j < maxState_n_Z; j++) { localArray[i, j] = Kalman_P_Z[i, j]; } }
                    break;
                case KalmanMatrixType.Q:
                    localArray = new double[maxState_n_Z, maxState_n_Z];
                    for (int i = 0; i < maxState_n_Z; i++) { for (int j = 0; j < maxState_n_Z; j++) { localArray[i, j] = Kalman_Q_Z[i, j]; } }
                    break;
                case KalmanMatrixType.H:
                    localArray = new double[maxMeasurement_m_Z, maxState_n_Z];
                    for (int i = 0; i < maxMeasurement_m_Z; i++) { for (int j = 0; j < maxState_n_Z; j++) { localArray[i, j] = Kalman_H_Z[i, j]; } }
                    break;
                case KalmanMatrixType.R:
                    localArray = new double[maxMeasurement_m_Z, maxMeasurement_m_Z];
                    for (int i = 0; i < maxMeasurement_m_Z; i++) { for (int j = 0; j < maxMeasurement_m_Z; j++) { localArray[i, j] = Kalman_R_Z[i, j]; } }
                    break;
                case KalmanMatrixType.K:
                    localArray = new double[maxState_n_Z, maxMeasurement_m_Z];
                    for (int i = 0; i < maxState_n_Z; i++) { for (int j = 0; j < maxMeasurement_m_Z; j++) { localArray[i, j] = Kalman_K_Fixed_Z[i, j]; } }
                    break;
            }
        }

        // This is to clear all List Data files -----------------------------------------------------------------------
        //
        internal static void ClearAllListRecordsOnceRun()
        {
            RealTimeProcessData.Clear();
            RealTimeRecordingData.Clear();
            Force_C.Clear(); FeedR_C.Clear(); Force_Y.Clear(); FeedR_U.Clear(); Kt_Updated.Clear(); F_Simulated.Clear();
            
            Fa.Clear(); delFa.Clear(); delNFa.Clear(); eta1.Clear(); eta2.Clear();
        }

        // Data recording proc. ----------------------------------------------------------------------------------------
        //
        private static void dataRecordingAsync()
        {
            //Console.WriteLine("Must run ONCE");
           
            int cntr = 0;

            dataRecordingTask = Task.Run(() =>
                {
                    using (var fs = new FileStream(fileNameMain, FileMode.Append))
                    {
                        byte[] bdata;

                        for (; ; )
                        {
                            if (RealTimeRecordingData.Count > 0)
                            {
                                // Recording correction for MainData, because sometimes TNC is recorded as 0,
                                // because of 2 different tasks running, one for collecting data, the other 
                                // for recording, so sometimes recording takes place after initializing the parms 
                                // as 0 and before collecting the actual data. As a result, we want to ignore 
                                // such records except if it is the first record.
                                //
                                //lock (multiThread)
                                {
                                    bdata = Encoding.Default.GetBytes(RealTimeRecordingData[cntr].TimeTNC + " "
                                            + RealTimeRecordingData[cntr].PositionX + " "
                                            + RealTimeRecordingData[cntr].PositionY + " "
                                            + RealTimeRecordingData[cntr].PositionZ + " "
                                            + RealTimeRecordingData[cntr].TorqueX + " "
                                            + RealTimeRecordingData[cntr].TorqueY + " "
                                            + RealTimeRecordingData[cntr].TorqueZ + " "
                                            + RealTimeRecordingData[cntr].TorqueS + " "
                                            + RealTimeRecordingData[cntr].Feedrate + " "
                                            + RealTimeRecordingData[cntr].SpindleSpeed + "\r\n");

                                    fs.Write(bdata, 0, bdata.Length);

                                } // End of Lock                                                                    

                                lock (multiThread) { RealTimeRecordingData.RemoveAt(cntr); } // End of Lock 
                            }

                            if (!isProcessRunning) { break; }
                        }
                    }
                });
        }

        // Real-Time Graph Plotting ----------------------------------------------------------------------------------------
        //
        private static void DataPlotThread()
        {
            int plotFaCntr = 0;
            int plotEta1Cntr = 0;
            int plotFtOCntr = 0;
            int plotFeedrCntr = 0;
            double tempData = 0;

            const int maxFaLimit = 1000;
            int maxFaShift = 1000;

            dataPlottingThread = new Thread(() =>
            {
                for (; ; )
                {
                    if (Fa.Count > plotFaCntr)
                    {
                        // Fa plotting
                        mainWindow.Dispatcher.Invoke(() =>
                        {
                            tempData = Fa[plotFaCntr];
                            mainWindow.myFa.RtmDataKeyValuePair.Add(new KeyValuePair<double, double>(plotFaCntr, tempData));

                            // Graph shifting, works good
                            //if (plotFaCntr > maxFaLimit)
                            //{
                            //    if (plotFaCntr > (maxFaShift - 20))
                            //    {
                            //        maxFaShift += 100;
                            //        mainWindow.myFa.XLinearAxis.Maximum += 100;
                            //        mainWindow.myFa.XLinearAxis.Minimum += 100;
                            //    }
                            //}
                            //else if (plotFaCntr > mainWindow.myFa.XLinearAxis.Maximum - 20) mainWindow.myFa.XLinearAxis.Maximum += 100;

                            if (plotFaCntr > mainWindow.myFa.XLinearAxis.Maximum - 20) mainWindow.myFa.XLinearAxis.Maximum += 100;
                            if (tempData > mainWindow.myFa.YLinearAxis.Maximum) mainWindow.myFa.YLinearAxis.Maximum = tempData + 50;
                            if (tempData < mainWindow.myFa.YLinearAxis.Minimum) mainWindow.myFa.YLinearAxis.Minimum = tempData - 50;
                        });

                        //plotFaCntr++;
                        plotFaCntr = plotFaCntr + 5;
                    }
                    Thread.Sleep(5);

                    // eta1 plotting
                    if (eta1.Count > plotEta1Cntr)
                    {
                        mainWindow.Dispatcher.Invoke(() =>
                        {
                            tempData = eta1[plotEta1Cntr];
                            mainWindow.myEta1.RtmDataKeyValuePair.Add(new KeyValuePair<double, double>(plotEta1Cntr, tempData));
                            if (plotEta1Cntr > mainWindow.myEta1.XLinearAxis.Maximum - 20) mainWindow.myEta1.XLinearAxis.Maximum += 100;
                            if (tempData > mainWindow.myEta1.YLinearAxis.Maximum) mainWindow.myEta1.YLinearAxis.Maximum = tempData + 50;
                            if (tempData < mainWindow.myEta1.YLinearAxis.Minimum) mainWindow.myEta1.YLinearAxis.Minimum = tempData - 50;
                        });

                        //plotEta1Cntr++;
                        plotEta1Cntr = plotEta1Cntr + 5;
                    }
                    Thread.Sleep(5);

                    // FtO plotting
                    if (Force_C.Count > plotFtOCntr)
                    {
                        mainWindow.Dispatcher.Invoke(() =>
                        {
                            tempData = Force_C[plotFtOCntr];
                            mainWindow.myFtO.RtmDataKeyValuePair.Add(new KeyValuePair<double, double>(plotFtOCntr, tempData));
                            if (plotFtOCntr > mainWindow.myFtO.XLinearAxis.Maximum - 20) mainWindow.myFtO.XLinearAxis.Maximum += 100;
                            if (tempData > mainWindow.myFtO.YLinearAxis.Maximum) mainWindow.myFtO.YLinearAxis.Maximum = tempData + 50;
                            if (tempData < mainWindow.myFtO.YLinearAxis.Minimum) mainWindow.myFtO.YLinearAxis.Minimum = tempData - 50;
                        });

                        //plotFtOCntr++;
                        plotFtOCntr = plotFtOCntr + 5;
                    }
                    Thread.Sleep(5);

                    //FeedR plotting
                    if (FeedR_C.Count > plotFeedrCntr)
                    {
                        mainWindow.Dispatcher.Invoke(() =>
                        {
                            tempData = FeedR_C[plotFeedrCntr];
                            mainWindow.myFeedR.RtmDataKeyValuePair.Add(new KeyValuePair<double, double>(plotFeedrCntr, FeedR_C[plotFeedrCntr]));
                            if (plotFeedrCntr > mainWindow.myFeedR.XLinearAxis.Maximum - 20) mainWindow.myFeedR.XLinearAxis.Maximum += 100;
                            if (tempData > mainWindow.myFeedR.YLinearAxis.Maximum) mainWindow.myFeedR.YLinearAxis.Maximum = tempData + 50;
                            if (tempData < mainWindow.myFeedR.YLinearAxis.Minimum) mainWindow.myFeedR.YLinearAxis.Minimum = tempData - 50;
                        });

                        //plotFeedrCntr++;
                        plotFeedrCntr = plotFeedrCntr + 5;
                    }
                    Thread.Sleep(5);

                    //if (!isProcessRunning) { break; }
                    if ((Fa.Count <= plotFaCntr)
                        && (eta1.Count <= plotEta1Cntr)
                        && (Force_C.Count <= plotFtOCntr)
                        && (FeedR_C.Count <= plotFeedrCntr)
                        && !isProcessRunning) { break; }
                }
            });

            dataPlottingThread.Priority = ThreadPriority.Lowest;
            dataPlottingThread.Start();
        }

        // All Calculated Data recording proc.  -------------------------------
        //
        private static void CalculatedDataRecording()
        {
            // Write data to CalcData file,
            using (var fcalc = new FileStream(fileNameCalc, FileMode.Append))
            {
                if (FeedR_C.Count > 0)
                {
                    for (int i = 0; i < FeedR_C.Count; i++)
                    {
                        byte[] bdata = Encoding.Default.GetBytes(FeedR_C[i] + " " +
                                                                 Force_C[i] + " " +
                                                                 FeedR_U[i] + " " +
                                                                 Force_Y[i] + " " +
                                                                 Kt_Updated[i] + " " +
                                                                 F_Simulated[i] + " " + 
                                                                 F_Simulated[i] + "\r\n"); // Later change this as F_Mean
                        fcalc.Write(bdata, 0, bdata.Length);
                    }
                }
            }

            // Then clear all data inside to make them ready for the re-run
            Force_C.Clear(); FeedR_C.Clear(); Force_Y.Clear(); FeedR_U.Clear(); Kt_Updated.Clear(); F_Simulated.Clear();
        }

        // All Tool Breakage Data recording proc.  -------------------------------
        //
        private static void ToolBreakageDataRecording()
        {
            // Write data to CalcData file,
            using (var ftool = new FileStream(fileNameTool, FileMode.Append))
            {
                if (Fa.Count > 0)
                {
                    // Fa count always bigger than eat counts, so in that case, save the last eta value
                    // of the bigger Fa index
                    int tempIndex = 0;
                    
                    for (int i = 0; i < Fa.Count; i++)
                    {
                        if (i < eta1.Count)
                        {


                            byte[] bdata = Encoding.Default.GetBytes(Fa[i] + " " +
                                                                     eta1[i] + " " +
                                                                     eta2[i] + "\r\n");
                            ftool.Write(bdata, 0, bdata.Length);
                            tempIndex = i;
                        }
                        else
                        {
                            // Last 3 eat values in the record will be the same, because Fa index is bigger than eta.
                            byte[] bdata = Encoding.Default.GetBytes(Fa[i] + " " +
                                                                     eta1[tempIndex] + " " +
                                                                     eta2[tempIndex] + "\r\n");
                            ftool.Write(bdata, 0, bdata.Length);
                        }
                    }
                }
            }

            // Then clear all data inside to make them ready for the re-run
            Fa.Clear(); delFa.Clear(); delNFa.Clear(); eta1.Clear(); eta2.Clear();
        }

        // Tool Breakage procs. --------------------------------------------------------------------------
        //
        internal static double rls00(double y, double Phi_k, double theta, double P, double Lamda, double na, double nb, double abar)
        {
            double c1 = 0.01; double c2 = 0.001;

            double K_k = (P * Phi_k) / (Lamda + Phi_k * P * Phi_k);
            double P_k = (1 / Lamda) * (P - abar * K_k * Phi_k * P);
            P = (c1 * P_k) / (P_k + c2 * 1);
            double theta_k = theta + abar * K_k * (y - Phi_k * theta);
            return theta_k;
        }

        // Data template for GUI reading
        //
        internal static DataForGUI GetRealTimeData()
        {
            DataForGUI collectData = new DataForGUI();

            collectData.PositionX = PX;
            collectData.PositionX = PX;
            collectData.PositionY = PY;
            collectData.PositionZ = PZ;
            collectData.TorqueX = Torque_XV;
            collectData.TorqueY = Torque_YV;
            collectData.TorqueZ = Torque_ZV;
            collectData.TorqueS = Torque_SV;
            collectData.Feedrate = FeedV;
            collectData.SpindleSpeed = SpindleSpeedRead;
            collectData.maxFtO = Force;

            return collectData;
        }
        
        // The end of Code writing by TAYFUN --------------------------------------------------------------------------
                
        [DllImport(lsv2dll, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        internal static extern string LSV2GetErrString(out IntPtr hPort, UInt32 err, UInt32 language);

        [DllImport(lsv2dll, SetLastError = true, CharSet = CharSet.Ansi)]
        internal static extern bool LSV2Open(out IntPtr hPort, [MarshalAs(UnmanagedType.LPStr)] string comPort, IntPtr baudRate, bool detectBaudRate);

        [DllImport(lsv2dll, SetLastError = true, CharSet = CharSet.Ansi)]
        internal static extern bool LSV2Login(IntPtr hPort, [MarshalAs(UnmanagedType.LPStr)] string keyWord, [MarshalAs(UnmanagedType.LPStr)] string password);

        [DllImport(lsv2dll, SetLastError = true, CharSet = CharSet.Ansi)]
        internal static extern bool LSV2Logout(IntPtr hPort, [MarshalAs(UnmanagedType.LPStr)] string keyWord);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        internal struct SP_PLCMEM_EX
        {
            UInt32 inputwordstart;
            UInt32 inputwords;

            UInt32 outputwordstart;
            UInt32 outputwords;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            internal byte[] reserved;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        internal struct SP_MONITOR
        {
            byte ScreenAnz;
            byte WinAnz;

            byte WindefAnz;
            byte PalAnz;
            byte PalColors;
            byte FontAnz;
            byte BorderColor;
            byte Dummy;
            UInt16 ScreenWidth;
            UInt16 ScreenHeight;
            UInt16 ZaehlerResX;
            UInt16 NennerResX;
            UInt16 ZaehlerResY;
            UInt16 NennerResY;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            internal byte[] reserved;
        }

        [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
        internal struct LSV2mmUnion
        {
            [FieldOffset(0)]
            internal SP_PLCMEM_EX PLCmem_ex;
            [FieldOffset(0)]
            internal SP_MONITOR Monitor;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        internal struct LSV2PARA
        {
            UInt32 markerstart;
            UInt32 markers;
            UInt32 inputstart;
            UInt32 inputs;
            UInt32 outputstart;
            UInt32 outputs;
            UInt32 counterstart;
            UInt32 counters;
            UInt32 timerstart;
            UInt32 timers;
            UInt32 wordstart;
            UInt32 words;
            UInt32 stringstart;
            UInt32 strings;
            byte stringlength;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
            internal byte[] reserved1;

            LSV2mmUnion mm;

            byte lsv2version;
            byte lsv2version_flags;

            byte MaxBlockLen;

            byte HdhBinVersion;
            byte HdhBinRevision;

            byte IsoBinVersion;
            byte IsoBinRevision;

            UInt32 HardwareVersion;
            UInt32 lsv2version_flags_ex;
            UInt16 MaxTraceLine;
            UInt16 ScopeChannels;
            UInt32 PWEncryptionKey;
        }

        [DllImport(lsv2dll, CharSet = CharSet.Ansi)]
        internal static extern bool LSV2ReceivePara(IntPtr hPort, out IntPtr lpPara);

        [DllImport(lsv2dll, CharSet = CharSet.Ansi)]
        internal static extern bool LSV2ReceiveChannelTypes(IntPtr hPort, out IntPtr infoCount, out IntPtr channelInfo);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        internal struct tChannelSel
        {
            internal Int32 ChannelId;
            internal Int32 Index;
            internal Int32 PLCAddress;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        internal struct tChannelPara
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAXDIMNAME)]
            internal byte[] NameDim;
        }

        [DllImport(lsv2dll, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool LSV2SelectChannelsITNC(IntPtr hPort, Int32 channelCount, Int32 samplingInterval, [MarshalAs(UnmanagedType.LPArray)] tChannelSel[] CS, [MarshalAs(UnmanagedType.LPArray)] tChannelPara[] CP);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        internal struct tScopeInfo
        {
            internal Int32 SamplingType;
            internal Int32 ChannelMax;
            internal Int32 SamplingIntervalMin;
            internal Int32 BufferSize;
            internal Int32 IPOInterval;
        }

        [DllImport(lsv2dll, CharSet = CharSet.Ansi)]
        internal static extern bool LSV2ReceiveScopeInfo(IntPtr hPort, out IntPtr scopeInfo);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        internal struct tTriggerData
        {
            internal Int32 Channel;
            internal Int32 TriggerMode;
            internal double Level;
            internal Int32 PreTrigger;
        }

        internal unsafe delegate bool samplingfunc_t(void* ConsumerInfo, double* Data, Int32 DataLen, bool Triggered);

        [DllImport(lsv2dll, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool LSV2ReceiveScopeData(IntPtr hPort, IntPtr ConsumerInfo, Int32 Mode, Int32 SamplingInterval, IntPtr Trigger, [MarshalAs(UnmanagedType.FunctionPtr)]samplingfunc_t func);

        [DllImport(lsv2dll, CharSet = CharSet.Ansi)]
        internal static extern bool LSV2Close(IntPtr hPort);

        [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptions]
        internal static bool OpenConnection(out IntPtr hPort, string address)
        {
            IntPtr baudRate = Marshal.AllocHGlobal(4);
            bool detectBaudRate = false;
            InitializeAdaptiveContArrays();
            bool result = false;
            try
            {
                result = LSV2Open(out hPort, address, baudRate, detectBaudRate);
            }
            catch (Exception ave)
            {
                string exceptionMessage = "LSV2Caller.OpenConnection: Caught " + ave.Message;
                Console.WriteLine(exceptionMessage);
                /*string errorFile = "C:\\Users\\Public\\ExceptionOutput.txt";
                using (System.IO.StreamWriter sw = System.IO.File.CreateText(errorFile))
                {
                    sw.WriteLine(exceptionMessage);
                    sw.WriteLine(ave.StackTrace);
                }*/
                if (ave.InnerException != null)
                {
                    string innerException = "Inner exception: " + ave.InnerException.ToString();
                    string baseException = "Base exception: " + ave.GetBaseException().ToString();
                    Console.WriteLine(innerException);
                    Console.WriteLine(baseException);
                    /*using (System.IO.StreamWriter sw = System.IO.File.AppendText(errorFile))
                    {
                        sw.WriteLine(innerException);
                        sw.WriteLine(baseException);
                    }*/
                }
                hPort = IntPtr.Zero;
                return false;
            }
            finally
            {
                Marshal.FreeHGlobal(baudRate);
            }

            if (!result)
            {
                hPort = IntPtr.Zero;
            }

            return result;
        }

        internal static bool CloseConnection(IntPtr hPort)
        {
            if (hPort != null && hPort != IntPtr.Zero)
            {
                bool result = LSV2Close(hPort);
                return result;
            }
            else
            {
                return true;
            }
        }

        [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptions]
        internal static bool GetScopeInfo(IntPtr hPort)
        {
            tScopeInfo scopeInfo = new LSV2Caller.tScopeInfo();
            int size = Marshal.SizeOf(scopeInfo);
            IntPtr scopeInfoPtr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(scopeInfo, scopeInfoPtr, false);
            bool result = false;
            Console.WriteLine("GetScopeInfo: " + scopeInfoPtr.ToString() + " size: " + size);
            try
            {
                result = LSV2ReceiveScopeInfo(hPort, out scopeInfoPtr);
                if (result)
                {
                    Console.WriteLine("Sampling Type:\t" + scopeInfo.SamplingType);
                }
                else
                {
                    Console.WriteLine("Error getting scope info!");
                }
                return result;
            }
            catch (AccessViolationException ave)
            {
                string exceptionMessage = "LSV2Caller.GetScopeInfo: Caught " + ave.Message;
                Console.WriteLine(exceptionMessage);
                /*string errorFile = "C:\\Users\\Public\\ExceptionOutput.txt";
                using (System.IO.StreamWriter sw = System.IO.File.CreateText(errorFile))
                {
                    sw.WriteLine(exceptionMessage);
                    sw.WriteLine(ave.StackTrace);
                }*/
                if (ave.InnerException != null)
                {
                    string innerException = "Inner exception: " + ave.InnerException.ToString();
                    string baseException = "Base exception: " + ave.GetBaseException().ToString();
                    Console.WriteLine(innerException);
                    Console.WriteLine(baseException);
                    /*using (System.IO.StreamWriter sw = System.IO.File.AppendText(errorFile))
                    {
                        sw.WriteLine(innerException);
                        sw.WriteLine(baseException);
                    }*/
                }
                return false;
            }
            finally
            {
                Marshal.DestroyStructure(scopeInfoPtr, typeof(tScopeInfo));
                //Marshal.FreeHGlobal(scopeInfoPtr);
            }
        }

        private static double[,] make_idty_matrix(int n)
        {
            var idty = new double[n, n]; // ( n, List<double>( n, 0 ));
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i == j) { idty[i, i] = 1; }
                    else { idty[i, j] = 0; }
                }
            }
            return idty;
        }

        //******************************************************************************************************
        // Proc: onDataReceive(void* ConsumerInfo, double* Data, Int32 DataLen, bool Triggered)
        //
        // Action: This proc is passed to LSV2ReceiveScopeData proc as its parameter and run. 
        // Description: It gets the realtime CNC data from its scope, stores it to be able to use later for 
        //       any case and processes the user requirements by relying on it.
        //
        //******************************************************************************************************
        //        
        internal unsafe static bool onDataReceive(void* ConsumerInfo, double* Data, Int32 DataLen, bool Triggered)
        {
            // The following for loop is to extract the data receiced from CNC. Data lenght is 288
            // so every reading has 9 channels x 32 = 288 data. The valid X, Y, Z info exist in
            // the first record and continues in every 30th one, but the other info like torques are
            // are valid in every record, because their smapling frequency is diffrerent in Heidenhain.
            //
            for (int i = 0; i < DataLen; i = i + channels)
            {
                //Console.WriteLine("Data received ......");

                // Save the received data from channels to the specific values 
                // to be able to understand easily
                //
                PX = Data[i];
                PY = Data[i + 1];
                PZ = Data[i + 2] - toolLength;

                Torque_XV = Data[i + 3] * KtX;
                Torque_YV = Data[i + 4] * KtY;
                Torque_ZV = Data[i + 5] * KtZ;
                Torque_SV = Data[i + 6] * KtS;
                FeedV = Math.Abs(Data[i + 7]);
                SpindleSpeedRead = Math.Abs(Data[i + 8]);

                // Since some data are not valid in every channel like X, Y, Z, this is the elimination of unvalid data
                //
                // Define all Numbers to Constants LATER
                //
                if (Math.Abs(PX) > 885) { PX = PrevPX; } else { PrevPX = PX; }
                if (Math.Abs(PY) > 800) { PY = PrevPY; } else { PrevPY = PY; }
                if (Math.Abs(PZ) > 600) { PZ = PrevPZ; } else { PrevPZ = PZ; }
                if (Math.Abs(FeedV) > 5000) { FeedV = PrevFeedV; } else { PrevFeedV = FeedV; }
                if (Math.Abs(SpindleSpeedRead) > 24000) { SpindleSpeedRead = PrevSpindleS; } else { PrevSpindleS = SpindleSpeedRead; }
                if (Math.Abs(Torque_XV) > 1000) { Torque_XV = PrevTorque_XV; } else { PrevTorque_XV = Torque_XV; }
                if (Math.Abs(Torque_YV) > 1000) { Torque_YV = PrevTorque_YV; } else { PrevTorque_YV = Torque_YV; }
                if (Math.Abs(Torque_ZV) > 1000) { Torque_ZV = PrevTorque_ZV; } else { PrevTorque_ZV = Torque_ZV; }
                if (Math.Abs(Torque_SV) > 1000) { Torque_SV = PrevTorque_SV; } else { PrevTorque_SV = Torque_SV; }

                // Check if DEPENDENT test mode is ON, if so, take data from the previously recorded test data
                if (isDependentTestOn && isProcessRunning)
                {
                    PX = RealTimeTestData[dependentTestCount].PositionX;
                    PY = RealTimeTestData[dependentTestCount].PositionY;
                    PZ = RealTimeTestData[dependentTestCount].PositionZ;

                    Torque_XV = RealTimeTestData[dependentTestCount].TorqueX;
                    Torque_YV = RealTimeTestData[dependentTestCount].TorqueY;
                    Torque_ZV = RealTimeTestData[dependentTestCount].TorqueZ;
                    Torque_SV = RealTimeTestData[dependentTestCount].TorqueS;
                    FeedV = RealTimeTestData[dependentTestCount].Feedrate;
                    SpindleSpeedRead = RealTimeTestData[dependentTestCount].SpindleSpeed;

                    dependentTestCount++;

                    // If the counter reaches the max test data count, then stop process running
                    // IMPORTANT: This may cause a problem if test data reaches to the end before
                    //            simulated data, then there may be mismatches. 
                    // SOLUTION:  Simulated data and recorded data MUST match each other
                    //            (Recorded data must be recorded while simulated data is running) 
                    if (dependentTestCount >= RealTimeTestData.Count) { isProcessRunning = false; }
                }

                // Process all received data from either CNC or previously recorded main data
                //
                ProcessReceivedData();
                //--------------------

                // IMPORTANT: This part stores the data to be used by the tracking process. If we do not do this and 
                // the tracking process reads the positions from Real-time CNC data immediatelly, it may loose some data 
                // and may not track it because of being slower than real time data.
                //
                if (isTrackingStarted)
                {
                    try
                    {
                        DataForGUI trackingData;

                        trackingData.PositionX = PX;
                        trackingData.PositionY = PY;
                        trackingData.PositionZ = PZ;
                        trackingData.TorqueX = Torque_XV;
                        trackingData.TorqueY = Torque_YV;
                        trackingData.TorqueZ = Torque_ZV;
                        trackingData.TorqueS = Torque_SV;
                        trackingData.Feedrate = FeedV;
                        trackingData.SpindleSpeed = SpindleSpeedRead;
                        trackingData.maxFtO = Force;

                        var isAdded = receivedCollection.TryAdd(trackingData, 1, cts.Token);
                        if (!isAdded)
                        {
                            Console.WriteLine("Unable to add scope data within timeout period - halting data receive.");
                            return false;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("Data acquisition cancelled");
                        cts.Dispose();
                        return false;
                    }
                } // IMPORTANT: ----------------------------------------------------------------------------------------------

            } // End of FOR LOOP ----------------------------------------------------------------------------------------------

            // Check whether tracking ended, if so, and if recoding is on, then record Fa, delFa, delNFa, eta1 and eta2 data  
            // to files ONCE, then reset to false till next tracking starts
            //
            if (isTrackingEnded)
            {
                if (isRecordingData)
                {
                    // Record Force and Feed Rate data
                    //ForceFeedrDataRecording();
                    CalculatedDataRecording();
                    
                    // If tool breakage active, record the related data
                    if (isToolBreakageActive) 
                    {
                        //toolBreakageDataRecording(); 
                        ToolBreakageDataRecording(); 
                    }

                    // Do the similar for adaptive control if required later
                }

                // Reset some parameters to get the correct starting point for re-run
                totalCount = 0;
                ToolBreakageIteration = 1;

                SetTrackingEnded(false); // reset it to not run always
            }
            
            return true;
        }

        //**************************************************************************************************
        // Proc: ProcessReceivedData
        // Desc: Process the data either received from CNC or previously recorded real-time test data
        //
        //**************************************************************************************************
        internal static void ProcessReceivedData()
        {
            double sum = 0;
            
            ScopeData tempData;
            DataForRecording recordData;

            // Put the one data combination into a temporary data record and save it at the end of the loop
            // ATTENTION: All elements MUST be initialized before saving (total 22)
            //      The first 16 items are added by Deniz
            //      The next 6 items are added by Ramon
            //
            tempData.TimeTNC = totalCount;
            tempData.PositionX = PX;
            tempData.PositionY = PY;
            tempData.PositionZ = PZ;
            tempData.TorqueX = Torque_XV;
            tempData.TorqueY = Torque_YV;
            tempData.TorqueZ = Torque_ZV;
            tempData.TorqueS = Torque_SV;
            tempData.Feedrate = FeedV;
            tempData.SpindleSpeed = SpindleSpeedRead;
            tempData.SpindleTic = 0;
            tempData.MaxFt = 0;                        // Force
            tempData.MaxFtO = 0;
            tempData.MaxS = MaxSpindleTorque;
            tempData.Uk = u[SpindlePeriodCount - 1]; // It is calculated for every ticPeriod, so writes the previous value
            tempData.Yk = y[SpindlePeriodCount - 1]; // It is calculated for every ticPeriod, so writes the previous value
            tempData.Potentiometer = Potentiometer;
            tempData.MaxS_LP = 0;
            tempData.Kt_Updated = 0;
            tempData.F_Sim = 0;
            tempData.MeanFt_a = 0;
            tempData.MeanFtO_a = 0;

            /*******************************************************************************************************************/
            //
            // ALL CONTROL PART OF PROJECT PROCESS STARTS FROM NOW ON
            //
            /*******************************************************************************************************************/

            // If the process is running, then take an action as well as recording if required, otherwise ignore recording
            //
            if (isProcessRunning)
            {
                // Data recording takes too much time and interrups the processing, so we have to do it at the background task,
                // and it must run only once till either process running is stoped or the end of tracking is reached.
                //
                if (onceRun)
                {
                    // Do not run this task again, except re-run the process
                    onceRun = false;

                    // Calculate alpha value once to be used for low pass filtering, for tool breakage.
                    const double numbOfHarmonics = 3;
                    const double dt = 1.0 / 1e4;

                    double cutOff = (SpindleSpeedRead / 60) * numbOfHarmonics * numbOfToolTeeth;
                    double RC = 1.0 / (cutOff * 2 * Math.PI);
                    alpha = dt / (RC + dt);

                    // Real-time plotting task
                    DataPlotThread();

                    if (isRecordingData)
                    {
                        // First, create the all required files once to get the same timestamps
                        //
                        mainWindow.Dispatcher.Invoke(() =>
                        {
                            fileNameMain = recordingFileDir + DateTime.Now.ToString("ddMMyy") + "_" + DateTime.Now.ToString("HHmmss") + "_MainD_" + mainWindow.RecordNameTextBox.Text + ".txt";
                            fileNameCalc = recordingFileDir + DateTime.Now.ToString("ddMMyy") + "_" + DateTime.Now.ToString("HHmmss") + "_CalcD_" + mainWindow.RecordNameTextBox.Text + ".txt";

                            //fileNameMain = recordingFileDir + "OCC_MainD_" + DateTime.Now.ToString("ddMMyy") + "_" + DateTime.Now.ToString("HHmmss") + ".txt";
                            //fileNameCalc = recordingFileDir + "OCC_CalcD_" + DateTime.Now.ToString("ddMMyy") + "_" + DateTime.Now.ToString("HHmmss") + ".txt";

                            if (isToolBreakageActive)
                            {
                                fileNameTool = recordingFileDir + DateTime.Now.ToString("ddMMyy") + "_" + DateTime.Now.ToString("HHmmss") + "_ToolB_" + mainWindow.RecordNameTextBox.Text + ".txt";
                            }
                        });

                        // Then start only recording MainData asyncroniously. The others are at the end of tracking
                        // NOTE: This is NOT required if TEST MODE STANDALONE is ON
                        if (isRecordingTaskRequired) { dataRecordingAsync(); }
                    }
                }

                // -------------------------------------------------------------------------------------------------------------
                // KALMAN FILTERING will be active if MaxSpindleTorque is bigger than maxSpindleTorqVal
                //
                if (Math.Abs(MaxSpindleTorque) > maxSpindleTorqVal)
                {
                    // KALMAN FILTERING ACTIVE ///////////////////////////////////////////////////////
                    //
                    if (isKalmanAxisXActive || isKalmanAxisXActive || isKalmanAxisXActive)
                    {
                        double FrictionX = 1.3;
                        double FrictionY = 0.36;

                        // X Direction --------------------------------------------------
                        if (isKalmanAxisXActive)
                        {
                            IM_X = 0;

                            for (int k = 0; k < 13; ++k) { m2_X[k, 0] = m_X[k, 0]; }
                            for (int k = 0; k < 13; ++k) { m_X[k, 0] = 0; }

                            for (int k = 0; k < 13; ++k)
                            {
                                for (int j = 0; j < 13; ++j)
                                {
                                    m_X[k, 0] += Kalman_A_X[k, j] * m2_X[j, 0];
                                }
                            }

                            for (int k = 0; k < 13; ++k) IM_X += Kalman_H_X[0, k] * m_X[k, 0];

                            for (int k = 0; k < 13; ++k) m_X[k, 0] = (m_X[k, 0] + Kalman_K_Fixed_X[k, 0] * ((Torque_XV - FrictionX) * KtX - IM_X));

                            CompensatedX.Add(m_X[12, 0] * TtoF);
                        }

                        // Y Direction ---------------------------------------------------
                        if (isKalmanAxisXActive)
                        {
                            IM_Y = 0;

                            for (int k = 0; k < 17; ++k) { m2_Y[k, 0] = m_Y[k, 0]; }
                            for (int k = 0; k < 17; ++k) { m_Y[k, 0] = 0; }

                            for (int k = 0; k < 17; ++k)
                            {
                                for (int j = 0; j < 17; ++j)
                                {
                                    m_Y[k, 0] += Kalman_A_Y[k, j] * m2_Y[j, 0];
                                }
                            }

                            for (int k = 0; k < 17; ++k) IM_Y += Kalman_H_Y[0, k] * m_Y[k, 0];

                            for (int k = 0; k < 17; ++k) m_Y[k, 0] = (m_Y[k, 0] + Kalman_K_Fixed_Y[k, 0] * ((Torque_YV - FrictionY) * KtY - IM_Y));

                            CompensatedY.Add(m_Y[16, 0] * TtoF);
                        }

                        // Z Direction ---------------------------------------------------
                        if (isKalmanAxisZActive)
                        {

                        }
                    } // End of Kalman Compansation Active //////////////////////////////////////
                } // (abs(MaxSpindleTorque) > maxSpindleTorqVal ) ----------------------------------------------------------------------------

                // -------------------------------------------------------------------------------------------------------------
                // Previous checking
                // Max FORCE calculation if MaxSpindleTorque is bigger than maxSpindleTorqVal
                //if (Math.Abs(MaxSpindleTorque) > maxSpindleTorqVal) // 
                //
                // New checking
                if (isSimInputDataLoaded && TFNCMPro[simDataIndex] == 1)
                {
                    // FORCE Calculation
                    if (VirtualRealConnection && isFeedbacktoReal)
                    {
                        FutureCheck = (int)(Math.Ceiling(((tempData.SpindleSpeed/60) * futureCheckTime * (tempData.Feedrate/tempData.SpindleSpeed))/0.4));
                        CompensationCheck = (int)(Math.Ceiling(((tempData.SpindleSpeed / 60) * CompensationCheckTime * (tempData.Feedrate / tempData.SpindleSpeed)) / 0.4));

                        if (simDataIndex < (simDataCount - FutureCheck) && (simDataIndex > 1))
                        {
                            if (TFGMPro[simDataIndex + FutureCheck] >= (TFGMPro[simDataIndex] * forceOverShoot))
                            {
                                tempData.MaxFt = ((MaxSpindleTorque - (spindleExhaustion * KtS)) / ToolRadius) * (TFGMPro[simDataIndex + FutureCheck] / TFGMPro[simDataIndex]);
                                tempData.MeanFt_a = ((MeanTorqueS - (spindleExhaustion * KtS)) / ToolRadius) * (TFGMPro[simDataIndex + FutureCheck] / TFGMPro[simDataIndex]);
                                //double calc = (TFGMPro[simDataIndex + 8] - (TFGMPro[simDataIndex])) * 100 / (TFGMPro[simDataIndex]);
                                //Console.WriteLine("Upcoming Force Peak by %" + (int)calc);
                            }
                            else
                            {
                                tempData.MaxFt = (((MaxSpindleTorque - (spindleExhaustion * KtS)) / ToolRadius));
                                tempData.MeanFt_a = (((MeanTorqueS - (spindleExhaustion * KtS)) / ToolRadius));
                            }

                            if ((TFGMPro[simDataIndex] > TFGMPro[simDataIndex - CompensationCheck] * forceOverShoot) && (TFGMPro[simDataIndex - CompensationCheck] != 0))
                            {
                                tempData.MaxFt = ((MaxSpindleTorque - (spindleExhaustion * KtS)) / ToolRadius) * (TFGMPro[simDataIndex] / TFGMPro[simDataIndex - CompensationCheck]);
                                tempData.MeanFt_a = ((MeanTorqueS - (spindleExhaustion * KtS)) / ToolRadius) * (TFGMPro[simDataIndex] / TFGMPro[simDataIndex - CompensationCheck]);
                            }
                        }
                        else
                        {
                            tempData.MaxFt = ((MaxSpindleTorque - (spindleExhaustion * KtS)) / ToolRadius);
                            tempData.MeanFt_a = ((MeanTorqueS - (spindleExhaustion * KtS)) / ToolRadius);
                        }
                    }
                    else
                    {
                        tempData.MaxFt = ((MaxSpindleTorque - (spindleExhaustion * KtS)) / ToolRadius);
                        tempData.MeanFt_a = ((MeanTorqueS - (spindleExhaustion * KtS)) / ToolRadius);
                        //Console.WriteLine("MaxF: " + tempData.MaxFt);
                    } // End of Feed back //////////////////////////////////////////////////////////////////

                    //tempData.MaxFt      =  ((MaxSpindleTorque-(0.1*KtS))/0.01);
                    tempData.MaxFtO = ((MaxSpindleTorque - (spindleExhaustion * KtS)) / ToolRadius);
                    tempData.MeanFtO_a = ((MeanTorqueS - (spindleExhaustion * KtS)) / ToolRadius);
                    Cutting = 1;

                    if (isSimInputDataLoaded && (TFNCMPro[simDataIndex - 10] != 0) && (TFNCMPro[simDataIndex + 10] != 0))
                    {
                        tempData.Kt_Updated = (tempData.MaxFt * 0.8) / (TFGMPro[simDataIndex] * ((tempData.Feedrate / tempData.SpindleSpeed) / numbOfToolTeeth));
                        tempData.F_Sim = (tempData.Kt_Updated * (1 / 0.8)) * TFGMPro[simDataIndex] * ((tempData.Feedrate / tempData.SpindleSpeed) / numbOfToolTeeth);
                        KtUpdateStepNumber = (int)(Math.Ceiling(((tempData.SpindleSpeed / 60) * 0.5 * (tempData.Feedrate / tempData.SpindleSpeed)) / 0.4));

                        if (Ktstep < KtUpdateStepNumber)
                        {
                            //tempData.Kt_Updated = 840;
                            KtUP = (tempData.MaxFt * 0.7) / (TFGMPro[simDataIndex] * ((tempData.Feedrate / tempData.SpindleSpeed) / numbOfToolTeeth));
                            KtUPSum = KtUPSum + KtUP;
                            Ktstep++;
                        }
                        else
                        {
                            KtUP = (tempData.MaxFt * 0.7) / (TFGMPro[simDataIndex] * ((tempData.Feedrate / tempData.SpindleSpeed) / numbOfToolTeeth));
                        }

                        tempData.Kt_Updated = KtUPSum / Ktstep;

                        tempData.F_Sim = (KtUP * (1 / 1)) * TFGMPro[simDataIndex] * ((tempData.Feedrate / tempData.SpindleSpeed) / numbOfToolTeeth);
                        //tempData.F_Sim = (1250) * TFGMPro[simDataIndex] * ((tempData.Feedrate / tempData.SpindleSpeed) / numbOfToolTeeth);

                        // GUI Kt Real-time display
                        mainWindow.Dispatcher.Invoke(() =>
                        {
                            mainWindow.KtTextBox.Text = tempData.Kt_Updated.ToString();
                        });
                    }
                }
                else
                {
                    tempData.MaxFt = ((MaxSpindleTorque - (spindleExhaustion * KtS)) / ToolRadius);
                    tempData.MaxFtO = ((MaxSpindleTorque - (spindleExhaustion * KtS)) / ToolRadius);
                    tempData.MeanFt_a = ((MeanTorqueS - (spindleExhaustion * KtS)) / ToolRadius);
                    tempData.MeanFtO_a = ((MeanTorqueS - (spindleExhaustion * KtS)) / ToolRadius);
                    Cutting = 0;
                } // End of Max Force calculation

                // Save Original Force to be able to use it for adjusted data listview and graph
                Force = tempData.MaxFtO;

                // -------------------------------------------------------------------------------------------------------------
                // The TOLL BREAKAGE and ADAPTIVE CONTROL proccess should start after 1 spin is completed, therefore data is  
                // processed in every ticPeriodth.  No process at the first hit, because no data yet, then start at the second hit,
                // but data stroring continuous for every record.
                //
                if (RealTimeProcessData.Count % ticPeriod == 0 && RealTimeProcessData.Count != 0)
                {
                    // This is important, keeps the number of the records, so it is needed to process the save data
                    // then release the unrequired buffer, otherwise it causes out of memory problem
                    //
                    //CurrentTic = tempData.TimeTNC;

                    tempData.SpindleTic = 1;
                    sum = 0;
                    SumFirstTooth = 0;
                    SumSecondTooth = 0;
                    MaxSpindleTorque = 0;

                    for (int l = 0; l < ticPeriod; l = l + 1)
                    {
                        // Low Pass filtering
                        if (l == 0) { LP_TS.Add(RealTimeProcessData[l].TorqueS); }
                        else { LP_TS.Add(LP_TS[l - 1] + (alpha * (RealTimeProcessData[l].TorqueS - LP_TS[l - 1]))); }
                                               
                        sum += (RealTimeProcessData[l].TorqueS) / ticPeriod;
                        TheValue = Math.Abs(RealTimeProcessData[l].TorqueS);

                        if (TheValue > MaxSpindleTorque) { MaxSpindleTorque = TheValue; }

                        if (l < (ticPeriod / 2)) { SumFirstTooth += (RealTimeProcessData[l].TorqueS) / (ticPeriod / numbOfToolTeeth); }
                        else { SumSecondTooth += (RealTimeProcessData[l].TorqueS) / (ticPeriod / numbOfToolTeeth); }
                    }

                    // dave filtered data and Clear LP_TS to be able to use for the next tic
                    tempData.MaxS_LP = LP_TS.Last();
                    LP_TS.Clear();

                    MeanTorqueS = sum;

                    Fa.Add(((SumFirstTooth - (spindleExhaustion * KtS)) / ToolRadius));
                    Fa.Add(((SumSecondTooth - (spindleExhaustion * KtS)) / ToolRadius));

                    // Because everytime we come here, we want to start from the first adding when calculating delFa and delNFa
                    // so we substract 2 from the total count of Fa.
                    int currentindex = Fa.Count - 2;

                    // TOOL BREAKAGE DETECTION ////////////////////////////////////////////////////////
                    //
                    if (isToolBreakageActive)
                    {
                        for (int TBLoop = 0; TBLoop < numbOfToolTeeth; TBLoop++)
                        {
                            if (tempData.TimeTNC > ticPeriod)
                            {
                                delFa.Add(Fa[currentindex] - Fa[currentindex - 1]);
                                delNFa.Add(Fa[currentindex] - Fa[currentindex - numbOfToolTeeth]);

                                /// RLS for TB //
                                if (ToolBreakageIteration > 1)
                                {
                                    int currentindexdelFa = delFa.Count - 1;

                                    double Theta1_TB = 0.1; double Theta2_TB = 0.1;
                                    double P1_TB = 1; double P2_TB = P1_TB;

                                    double Forget_TB = 0.9; double na_TB = 0; double nb_TB = 1;
                                    double c1_TB = 0.01; double c2_TB = 0.001; double abar_TB = 1;
                                    double Theta_k1_TB = 0; double Theta_k2_TB = 0;
                                    double Phi_k1_TB; double Phi_k2_TB;

                                    //Console.WriteLine("currentindexdelFa: " + currentindexdelFa);
                                    //Console.WriteLine("delFa.Count: " + delFa.Count);

                                    eta1.Add(delFa[currentindexdelFa] - Theta_k1_TB * delFa[currentindexdelFa - 1]);
                                    eta2.Add(delNFa[currentindexdelFa] - Theta_k2_TB * delNFa[currentindexdelFa - 1]);

                                    Phi_k1_TB = -delFa[currentindexdelFa];
                                    Phi_k2_TB = -delNFa[currentindexdelFa];
                                    Theta1_TB = rls00(eta1[currentindexdelFa - 1] - delFa[currentindexdelFa - 1], Phi_k1_TB, Theta1_TB, P1_TB, Forget_TB, na_TB, nb_TB, abar_TB);
                                    Theta2_TB = rls00(eta2[currentindexdelFa - 1] - delNFa[currentindexdelFa - 1], Phi_k2_TB, Theta2_TB, P2_TB, Forget_TB, na_TB, nb_TB, abar_TB);

                                    B1_TB.Add(Theta1_TB);
                                    B2_TB.Add(Theta2_TB);

                                    if (isSimInputDataLoaded && (TFNCMPro[simDataIndex] == 1) && (TFNCMPro[simDataIndex - 10] != 0) && (TFNCMPro[simDataIndex + 10] != 0))
                                    {
                                        if (currentindexdelFa > 1)
                                        {
                                            if ((Math.Abs(eta1[currentindexdelFa - 2]) > (tempData.F_Sim * TBPercentage)))
                                            {
                                                if (Math.Abs(eta1[currentindexdelFa - 1]) > (tempData.F_Sim * TBPercentage))
                                                {
                                                    //Tool Breakage detected, just take the required action, then let main process knows
                                                    //
                                                    switch (toolBreakageAction)
                                                    {
                                                        case toolBreakageActionType.alarmAndStop:
                                                            SetFeedRatePercent(0); // Stop Feed rate
                                                            break;
                                                        case toolBreakageActionType.alarmAndContinue:
                                                            // Do nothing
                                                            break;
                                                        case toolBreakageActionType.alarmAndDelayStop:
                                                            // Don't know what to do right now, develop later
                                                            //
                                                            break;
                                                    }

                                                    // In any case, report it the main process controler (GUI) to alarm
                                                    SetToolBreakageDetected(true);
                                                    return;
                                                }
                                            }
                                        }
                                    }
                                } // (ToolBreakageIteration > 1)

                                ToolBreakageIteration++;
                            } //(tempData.TimeTNC > ticPeriod)

                            currentindex++;
                        } // for loop: (int TBLoop = 0; TBLoop < NoTeeth; TBLoop++)
                    } // End of Tool Breakage detection //////////////////////////////////////////////

                    // ADAPTIVE CONTROL DETECTION ////////////////////////////////////////////////////
                    //
                    if (isAdaptiveControlActive)
                    {
                        CurrentFeed = (Potentiometer / 100) * ProgramFeed;

                        if (isPeekForceUsed == true)
                        {
                            y[SpindlePeriodCount] = tempData.MaxFt;
                        }
                        else if (isPeekForceUsed == false)
                        {
                            y[SpindlePeriodCount] = tempData.MeanFt_a;
                        }

                        u[SpindlePeriodCount] = FeedV;
                        //Console.WriteLine("MaxF: " + tempData.MaxFt);
                        
                        if ((Cutting == 1) && (FeedV * 0.95 < CurrentFeed) && (CurrentFeed < FeedV * 1.05))
                            {
                            Phi_k[0] = -y[SpindlePeriodCount - 1];
                            Phi_k[1] = -y[SpindlePeriodCount - 2];
                            Phi_k[2] = u[SpindlePeriodCount - 1];
                            Phi_k[3] = u[SpindlePeriodCount - 2];
                            Phi_k[4] = u[SpindlePeriodCount - 3];

                            def = y[SpindlePeriodCount];
                            error = r - y[SpindlePeriodCount - 1];

                            if (Math.Abs(error) > 20)
                            {
                                double[] K_k1 = { P[0, 0] * Phi_k[0], P[1, 1] * Phi_k[1], P[2, 2] * Phi_k[2], P[3, 3] * Phi_k[3], P[4, 4] * Phi_k[4] };
                                double[] K_k2 = { Phi_k[0] * P[0, 0], Phi_k[1] * P[1, 1], Phi_k[2] * P[2, 2], Phi_k[3] * P[3, 3], Phi_k[4] * P[4, 4] };
                                double K_k3 = lambda + K_k2[0] * Phi_k[0] + K_k2[1] * Phi_k[1] + K_k2[2] * Phi_k[2] + K_k2[3] * Phi_k[3] + K_k2[4] * Phi_k[4];

                                double[] K_k = { K_k1[0] / K_k3, K_k1[1] / K_k3, K_k1[2] / K_k3, K_k1[3] / K_k3, K_k1[4] / K_k3 };

                                double[] abarK_k = { abar * K_k[0], abar * K_k[1], abar * K_k[2], abar * K_k[3], abar * K_k[4] };

                                double[] P_k0 = { (1 / lambda) * (P[0, 0] - abarK_k[0] * Phi_k[0] * P[0, 0]), (1 / lambda) * (P[0, 1] - abarK_k[0] * Phi_k[1] * P[0, 0]), (1 / lambda) * (P[0, 2] - abarK_k[0] * Phi_k[2] * P[0, 0]), (1 / lambda) * (P[0, 3] - abarK_k[0] * Phi_k[3] * P[0, 0]), (1 / lambda) * (P[0, 4] - abarK_k[0] * Phi_k[4] * P[0, 0]) };
                                double[] P_k1 = { (1 / lambda) * (P[1, 0] - abarK_k[1] * Phi_k[0] * P[1, 1]), (1 / lambda) * (P[1, 1] - abarK_k[1] * Phi_k[1] * P[1, 1]), (1 / lambda) * (P[1, 2] - abarK_k[1] * Phi_k[2] * P[1, 1]), (1 / lambda) * (P[1, 3] - abarK_k[1] * Phi_k[3] * P[1, 1]), (1 / lambda) * (P[1, 4] - abarK_k[1] * Phi_k[4] * P[1, 1]) };
                                double[] P_k2 = { (1 / lambda) * (P[2, 0] - abarK_k[2] * Phi_k[0] * P[2, 2]), (1 / lambda) * (P[2, 1] - abarK_k[2] * Phi_k[1] * P[2, 2]), (1 / lambda) * (P[2, 2] - abarK_k[2] * Phi_k[2] * P[2, 2]), (1 / lambda) * (P[2, 3] - abarK_k[2] * Phi_k[3] * P[2, 2]), (1 / lambda) * (P[2, 4] - abarK_k[2] * Phi_k[4] * P[2, 2]) };
                                double[] P_k3 = { (1 / lambda) * (P[3, 0] - abarK_k[3] * Phi_k[0] * P[3, 3]), (1 / lambda) * (P[3, 1] - abarK_k[3] * Phi_k[1] * P[3, 3]), (1 / lambda) * (P[3, 2] - abarK_k[3] * Phi_k[2] * P[3, 3]), (1 / lambda) * (P[3, 3] - abarK_k[3] * Phi_k[3] * P[3, 3]), (1 / lambda) * (P[3, 4] - abarK_k[3] * Phi_k[4] * P[3, 3]) };
                                double[] P_k4 = { (1 / lambda) * (P[4, 0] - abarK_k[4] * Phi_k[0] * P[4, 4]), (1 / lambda) * (P[4, 1] - abarK_k[4] * Phi_k[1] * P[4, 4]), (1 / lambda) * (P[4, 2] - abarK_k[4] * Phi_k[2] * P[4, 4]), (1 / lambda) * (P[4, 3] - abarK_k[4] * Phi_k[3] * P[4, 4]), (1 / lambda) * (P[4, 4] - abarK_k[4] * Phi_k[4] * P[4, 4]) };

                                double trace_Pk = P_k0[0] + P_k1[1] + P_k2[2] + P_k3[3] + P_k4[4];

                                double[] c1_P_k0 = { (c1 * P_k0[0] / trace_Pk) + c2, c1 * P_k0[1] / trace_Pk, c1 * P_k0[2] / trace_Pk, c1 * P_k0[3] / trace_Pk, c1 * P_k0[4] / trace_Pk };
                                double[] c1_P_k1 = { c1 * P_k1[0] / trace_Pk, (c1 * P_k1[1] / trace_Pk) + c2, c1 * P_k1[2] / trace_Pk, c1 * P_k1[3] / trace_Pk, c1 * P_k1[4] / trace_Pk };
                                double[] c1_P_k2 = { c1 * P_k2[0] / trace_Pk, c1 * P_k2[1] / trace_Pk, (c1 * P_k2[2] / trace_Pk) + c2, c1 * P_k2[3] / trace_Pk, c1 * P_k2[4] / trace_Pk };
                                double[] c1_P_k3 = { c1 * P_k3[0] / trace_Pk, c1 * P_k3[1] / trace_Pk, c1 * P_k3[2] / trace_Pk, (c1 * P_k3[3] / trace_Pk) + c2, c1 * P_k3[4] / trace_Pk };
                                double[] c1_P_k4 = { c1 * P_k4[0] / trace_Pk, c1 * P_k4[1] / trace_Pk, c1 * P_k4[2] / trace_Pk, c1 * P_k4[3] / trace_Pk, (c1 * P_k4[4] / trace_Pk) + c2 };

                                //
                                double def_Phi_Theta = def - (Phi_k[0] * Theta[0] + Phi_k[1] * Theta[1] + Phi_k[2] * Theta[2] + Phi_k[3] * Theta[3] + Phi_k[4] * Theta[4]);
                                double[] theta_k = { Theta[0] + (abarK_k[0] * def_Phi_Theta), Theta[1] + (abarK_k[1] * def_Phi_Theta), Theta[2] + (abarK_k[2] * def_Phi_Theta), Theta[3] + (abarK_k[3] * def_Phi_Theta), Theta[4] + (abarK_k[4] * def_Phi_Theta) };

                                BH[0] = theta_k[2]; BH[1] = theta_k[3]; BH[2] = theta_k[4];
                                AH[0] = theta_k[0]; AH[1] = theta_k[1];
                            }
                            else
                            {
                                BH[0] = BHold[0]; BH[1] = BHold[1]; BH[2] = BHold[2];
                                AH[0] = AHold[0]; AH[1] = AHold[1];
                            }

                            BHold[0] = BH[0]; BHold[1] = BH[1]; BHold[2] = BH[2];
                            AHold[0] = AH[0]; AHold[1] = AH[1];
                            //TetaRLS[0] = 1; TetaRLS[1] = AHold[0]; TetaRLS[2] = AHold[1]; TetaRLS[3] = BHold[0]; TetaRLS[4] = BHold[1]; TetaRLS[5] = BHold[2];
                            TetaRLS[0] = InitParam; TetaRLS[1] = InitParam; TetaRLS[2] = InitParam; TetaRLS[3] = InitParam; TetaRLS[4] = InitParam; TetaRLS[5] = InitParam;
                            // GPC ////////////////////////////////////
                            //
                            a1 = TetaRLS[1] - 1;
                            a2 = TetaRLS[2] - TetaRLS[1];
                            a3 = -TetaRLS[2];
                            b1 = TetaRLS[3];
                            b2 = TetaRLS[4];
                            b3 = TetaRLS[5];

                            // F polynomial ///////////////////////////

                            // j=1
                            f1[0] = -a1; f1[1] = -a2; f1[2] = -a3;

                            // j=2
                            r1 = f1[0];
                            f2[0] = f1[1] - r1 * a1;
                            f2[1] = f1[2] - r1 * a2;
                            f2[2] = -r1 * a3;

                            // j=3
                            r2 = f2[0];
                            f3[0] = f2[1] - r2 * a1;
                            f3[1] = f2[2] - r2 * a2;
                            f3[2] = -r2 * a3;

                            // j=4
                            r3 = f3[0];
                            f4[0] = f3[1] - r3 * a1;
                            f4[1] = f3[2] - r3 * a2;
                            f4[2] = -r3 * a3;

                            // differential command feed rates
                            du[SpindlePeriodCount - 1] = u[SpindlePeriodCount - 1] - u[SpindlePeriodCount - 2];
                            du[SpindlePeriodCount - 2] = u[SpindlePeriodCount - 2] - u[SpindlePeriodCount - 3];
                            //

                            f[0] = (b2 * du[SpindlePeriodCount - 1] + b3 * du[SpindlePeriodCount - 2] + f1[0] * y[SpindlePeriodCount] + f1[1] * y[SpindlePeriodCount - 1] + f1[2] * y[SpindlePeriodCount - 2]);
                            f[1] = (b2 * r1 + b3) * du[SpindlePeriodCount - 1] + b3 * r1 * du[SpindlePeriodCount - 2] + f2[0] * y[SpindlePeriodCount] + f2[1] * y[SpindlePeriodCount - 1] + f2[2] * y[SpindlePeriodCount - 2];
                            f[2] = (b2 * r2 + b3 * r1) * du[SpindlePeriodCount - 1] + b3 * r2 * du[SpindlePeriodCount - 2] + f3[0] * y[SpindlePeriodCount] + f3[1] * y[SpindlePeriodCount - 1] + f3[2] * y[SpindlePeriodCount - 2];
                            f[3] = (b3 * r2 + b2 * r3) * du[SpindlePeriodCount - 1] + b3 * r3 * du[SpindlePeriodCount - 2] + f4[0] * y[SpindlePeriodCount] + f4[1] * y[SpindlePeriodCount - 1] + f4[2] * y[SpindlePeriodCount - 2];

                            g0 = b1;
                            g1 = b2 + b1 * r1;
                            g2 = b3 + b2 * r1 + b1 * r2;
                            g3 = b2 * r2 + b3 * r1 + b1 * r3;

                            u[SpindlePeriodCount] = (u[SpindlePeriodCount - 1] + ((g0 * (r - f[0]) + g1 * (r - f[1]) + g2 * (r - f[2]) + g3 * (r - f[3])) / (g0 * g0 + g1 * g1 + g2 * g2 + g3 * g3 + lambda)));//-fcupdate;;

                            // Smooth exit control
                            if (isFeedbacktoReal)
                            {
                                if (isSimInputDataLoaded && TFNCMPro.Count > (simDataIndex + 10))
                                {
                                    if (TFNCMPro[simDataIndex + 10] == 0)
                                    {
                                        u[SpindlePeriodCount] = u[SpindlePeriodCount - 1]; // For smooth exit from the workpiece
                                    }
                                }
                            }

                            if (u[SpindlePeriodCount] < umin)
                            {
                                u[SpindlePeriodCount] = umin;
                            }
                            else if (u[SpindlePeriodCount] > umax)
                            {
                                u[SpindlePeriodCount] = umax;
                            }

                            NewFeed = u[SpindlePeriodCount];

                            if (NewFeed > ProgramFeed)
                            {
                                Potentiometer = 100 + ((Math.Abs(ProgramFeed - NewFeed) * 100) / ProgramFeed);

                                if (feedRatePercent != Potentiometer) { feedRatePercent = Potentiometer; }
                            }

                            if (NewFeed < ProgramFeed)
                            {
                                Potentiometer = 100 - ((Math.Abs(ProgramFeed - NewFeed) * 100) / ProgramFeed);

                                if (feedRatePercent != Potentiometer) { feedRatePercent = Potentiometer; }
                            }
                        }
                    } // End of Adaptive Control Detection ///////////////////////////////////////////

                    // if chatter is active
                    // 
                    



                    tempData.Uk = u[SpindlePeriodCount];
                    tempData.Yk = y[SpindlePeriodCount];

                    // We need to save this data for real-time plotting
                    Force_C.Add(tempData.MaxFtO);
                    FeedR_C.Add(tempData.Feedrate);

                    // Record the following data for every tic
                    if (isRecordingData)
                    {
                        //Force_C.Add(tempData.MaxFtO);
                        //FeedR_C.Add(tempData.Feedrate);
                        Force_Y.Add(tempData.Yk);
                        FeedR_U.Add(tempData.Uk);
                        Kt_Updated.Add(tempData.Kt_Updated);
                        F_Simulated.Add(tempData.F_Sim);
                    }

                    // Increase the adaptive control period count for every entry
                    SpindlePeriodCount++;

                    // Deleting the first bunch of recorded data
                    //
                    RealTimeProcessData.RemoveRange(0, RealTimeProcessData.Count);
                }
                else
                {
                    tempData.SpindleTic = 0;
                    //MeanTorqueS = sum;
                } // (recordData.size() % ticPeriod == 0 && recordData.size() != 0) -----------------------------------------------

                // Record the data to use for processing like adaptive control, tool breakage etc.
                // Huge data, causes a memory problem, process ticPeriod by ticPeriod, then delete.
                //
                RealTimeProcessData.Add(tempData);

                // Record the data to use for saving into a file.
                // Huge data, causes a memory problem, file 1 by 1, then delete.
                //
                if (isRecordingData) 
                {
                    recordData.TimeTNC = tempData.TimeTNC;
                    recordData.PositionX = tempData.PositionX;
                    recordData.PositionY = tempData.PositionY;
                    recordData.PositionZ = tempData.PositionZ;
                    recordData.TorqueX = tempData.TorqueX;
                    recordData.TorqueY = tempData.TorqueY;
                    recordData.TorqueZ = tempData.TorqueZ;
                    recordData.TorqueS = tempData.TorqueS;
                    recordData.Feedrate = tempData.Feedrate;
                    recordData.SpindleSpeed = tempData.SpindleSpeed;

                    // Record this by locking the process time
                    lock (multiThread) { RealTimeRecordingData.Add(recordData); }                    
                }
 
                totalCount++;

            } // End of (isProcessRunning) -----------------------------------------------------------------------------------
        }

        //**************************************************************************************************
        // Proc: TestDataActionLoop for Test Mode: STANDALONE
        // Desc: Reads data from a file assumed as if it is received from CNC and process it.
        //
        //**************************************************************************************************
        //
        internal static void TestDataActionLoop()
        {
            // Set the required setting to be able to run ProcessReceivedData
            isProcessRunning = true;
            onceRun = true;
            isRecordingTaskRequired = true;

            for (int i = 0; i < RealTimeTestData.Count; i++)
            {
                // Save the received data from channels to the specific values 
                // to be able to understand easily
                //
                PX = RealTimeTestData[i].PositionX;
                PY = RealTimeTestData[i].PositionY;
                PZ = RealTimeTestData[i].PositionZ;

                Torque_XV = RealTimeTestData[i].TorqueX;
                Torque_YV = RealTimeTestData[i].TorqueY;
                Torque_ZV = RealTimeTestData[i].TorqueZ;
                Torque_SV = RealTimeTestData[i].TorqueS;
                FeedV = RealTimeTestData[i].Feedrate;
                SpindleSpeedRead = RealTimeTestData[i].SpindleSpeed;

                //Console.WriteLine("FeedV: " + FeedV);

                // Then process the received data
                ProcessReceivedData();
                //--------------------------------

                // This delay is important to process data, othervise cause a problem
                for (int j = 0; j < 10000; j++) { }

                //Console.WriteLine(i + ". record processed"); // Consumes time a lot
            }

            if (isRecordingData)
            {
                // Record Force and Feed Rate data
                //ForceFeedrDataRecording();
                CalculatedDataRecording();

                // If tool breakage active, record the related data
                if (isToolBreakageActive) 
                {
                    //toolBreakageDataRecording(); 
                    ToolBreakageDataRecording(); 
                }
            }

            // Reset some parameters to get the correct starting point for re-run
            totalCount = 0;
            ToolBreakageIteration = 1;

            isProcessRunning = false;
        }

        //**************************************************************************************************
        // Proc: ControlCNCTask
        // Desc: Sends Feed rate and Spindle Speed data to CNC continiously. Any change for that data 
        //       takes immediate action till the task is stoped.
        //
        //**************************************************************************************************
        //
        internal static void ControlCNCTask()
        {
            Task CNCcontrolTask = null;
            
            stopCNCcontrolTask = false;

            CNCcontrolTask = Task.Run(() =>
            {
                while (stopCNCcontrolTask == false)
                {
                    //Start updating data at the CNC, if a new change is detected
                    dncConnection.SetOverrideFeed((int)feedRatePercent);
                    dncConnection.SetOverrideSpeed((int)spindleSpeedPercent);
                }
            });
        }

        // Filtering Proc.
        //
        internal static void LowPassFiltering()
        {
            const double numbOfHarmonics = 3;
            const double dt = 1.0 / 1e4;

            double cutOff = (SpindleSpeedRead / 60) * numbOfHarmonics * numbOfToolTeeth;
            double RC = 1.0 / (cutOff * 2 * Math.PI);
            double alpha = dt / (RC + dt);

            LP_TS.Add(Fa[0]);

            for (int i = 0; i < Fa.Count - 1; ++i)
            {
                LP_TS.Add(LP_TS[i] + (alpha * (Fa[i + 1] - LP_TS[i])));

                Console.WriteLine("Fa[" + i + "]=" + Fa[i] + "\t" + "LP_TS[" + i + "]=" + LP_TS[i]);
            }
        }

        // Above lines are added / updated by TAYFUN ---------------------------------------------------------------

        internal static unsafe bool GetDataFromScope(IntPtr port, int channelCount)
        {
            void* hPort = (void*)port.ToPointer();
            channels = channelCount;
            string consumerInfo = "I nominal\0";
            IntPtr ConsumerInfoPtr = (IntPtr)(Marshal.StringToHGlobalAnsi(consumerInfo));
            Int32 mode = 6;
            Int32 samplingInterval = 100;

            tTriggerData* triggerData;
            tTriggerData trigger = new tTriggerData();
            triggerData = &trigger;
            bool receivedData = false;
            samplingfunc_t func = new samplingfunc_t(onDataReceive);
            IntPtr triggerPtr = Marshal.AllocHGlobal(Marshal.SizeOf(trigger));
            bool fDeleteOld = false;
            Marshal.StructureToPtr(trigger, triggerPtr, fDeleteOld);
            cts = new CancellationTokenSource();
            //totalCount = 0;
            //readAttemptCount = 0;

            /*if (ToolBreakageDetection)
            {
                dncConnection = new HeidenhainDNC();
                dncConnection.RequestConnection();
            }*/

            receivedData = LSV2ReceiveScopeData(port, ConsumerInfoPtr, mode, samplingInterval, triggerPtr, func);
            Marshal.FreeHGlobal(ConsumerInfoPtr);
            Marshal.FreeHGlobal(triggerPtr);
            return receivedData;
        }

        // Initialize Adaptive Control's arrays
        //
        internal static void InitializeAdaptiveContArrays()
        {
            int n = 5;
            P = make_idty_matrix(n);

            for (int i = 0; i < n; i++)
            {
                P[i, i] = 1000 * P[i, i];
            }

            for (int i = 0; i < usize; ++i)
            {
                u[i] = ProgramFeed;
                y[i] = 0;
                du[i] = 0;
            }
        }

        internal static void SetDNCConnection(HeidenhainDNC connection)
        {
            dncConnection = connection;
        }
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct ScopeData
    {
        public double TimeTNC;
        public double PositionX;
        public double PositionY;
        public double PositionZ;
        public double TorqueX;
        public double TorqueY;
        public double TorqueZ;
        public double TorqueS;
        public double Feedrate;
        public double SpindleSpeed;
        public double SpindleTic;
        public double MaxFt;
        public double MaxFtO;
        public double MaxS;
        public double Uk;
        public double Yk;
        public double Potentiometer;
        //public double CompensatedFx;
        //public double CompensatedFy;
        public double MaxS_LP;
        public double Kt_Updated;
        public double F_Sim;
        public double MeanFt_a;
        public double MeanFtO_a;
    }

    // To send data to GUI without saving to a buffer - TAYFUN
    //
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct DataForGUI
    {
        public double PositionX;
        public double PositionY;
        public double PositionZ;
        public double TorqueX;
        public double TorqueY;
        public double TorqueZ;
        public double TorqueS;
        public double Feedrate;
        public double SpindleSpeed;
        public double maxFtO;
    }

    // To record data
    //
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct DataForRecording
    {
        public double TimeTNC;
        public double PositionX;
        public double PositionY;
        public double PositionZ;
        public double TorqueX;
        public double TorqueY;
        public double TorqueZ;
        public double TorqueS;
        public double Feedrate;
        public double SpindleSpeed;
        //public double Uk;
        //public double Yk;
    }
}