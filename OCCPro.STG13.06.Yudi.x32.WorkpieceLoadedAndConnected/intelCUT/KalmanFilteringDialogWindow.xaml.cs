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
using System.Windows.Shapes;

using Microsoft.Win32;
using System.Data;
using System.IO;
using System.Windows.Threading; // DispacherFrame
using System.Threading;

namespace OnlineCuttingControlProcess
{
    /// <summary>
    /// Interaction logic for KalmanFilterinDialogWindow.xaml
    /// </summary>
    public partial class KalmanFilteringDialogWindow : Window
    {
        private KalmanMatrixName kalmanMatrixName;
        public bool dialogClosedWithoutData = true;
        private bool isNotReOpened = true;

        private KalmanFilterMatrixData A_Data, P_Data, Q_Data, H_Data, R_Data, K_Data;
        private static string kalmanFileName = null;
        private static string kalmanDirectory = null;

        private int nMax = 0;
        private int mMax = 0;

        private string errorString; // It is for data missing error string to the status text area

        /******************************************************************************************************/
        // Main Window 
        //
        /******************************************************************************************************/
        //
        public KalmanFilteringDialogWindow(KalmanMatrixName matrixName)
        {
            InitializeComponent();

            // Initializing Data Entry ComboBox
            DataEntryComboBox.Items.Add("Not Selected");    // index=0
            DataEntryComboBox.Items.Add("Manually");        // index=1
            DataEntryComboBox.Items.Add("Load from File");  // index=2

            // Initializing state numbers
            for (int i = 1; i <= 50; i++) { StateComboBox.Items.Add(i); }

            // Initializing measurement numbers
            for (int i = 1; i <= 10; i++) { MeasureComboBox.Items.Add(i); }

            // Initializing Fill up Type
            MatrixTypeComboBox.Items.Add("Not Selected");   // index=0
            MatrixTypeComboBox.Items.Add("A Matrix");       // index=1
            MatrixTypeComboBox.Items.Add("P Matrix");       // index=2
            MatrixTypeComboBox.Items.Add("Q Matrix");       // index=3
            MatrixTypeComboBox.Items.Add("H Matrix");       // index=4
            MatrixTypeComboBox.Items.Add("R Matrix");       // index=5
            MatrixTypeComboBox.Items.Add("K Matrix");       // index=6
            MatrixTypeComboBox.Items.Add("All");            // index=7
            //MatrixTypeComboBox.Items.Add("Empty Fields");   // index=8

            // Initializing Fill up Type
            FillUpTypeComboBox.Items.Add("Not Selected");   // index=0
            FillUpTypeComboBox.Items.Add("Whole Body");          // index=1
            FillUpTypeComboBox.Items.Add("Empty Fields");   // index=2

            kalmanMatrixName = matrixName;

            switch (kalmanMatrixName)
            {
                case KalmanMatrixName.X:
                    KalmanWindowButton.Content = "X";

                    if (LSV2Caller.GetKalmanAxisXActive())
                    {
                        nMax = LSV2Caller.GetMaxState_n_X();
                        mMax = LSV2Caller.GetMaxMeasurement_m_X();
                    }
                    else { nMax = 0; mMax = 0; }
                    break;
                case KalmanMatrixName.Y:
                    KalmanWindowButton.Content = "Y";

                    if (LSV2Caller.GetKalmanAxisYActive())
                    {
                        nMax = LSV2Caller.GetMaxState_n_Y();
                        mMax = LSV2Caller.GetMaxMeasurement_m_Y();
                    }
                    else { nMax = 0; mMax = 0; }
                    break;
                case KalmanMatrixName.Z:
                    KalmanWindowButton.Content = "Z";

                    if (LSV2Caller.GetKalmanAxisZActive())
                    {
                        nMax = LSV2Caller.GetMaxState_n_Z();
                        mMax = LSV2Caller.GetMaxMeasurement_m_Z();
                    }
                    else { nMax = 0; mMax = 0; }
                    break;
            }

            // Check maxState_n & maxMeasurement_m, if one of them is ZERO, means no previously entered data, so just return
            if ((nMax == 0) || (mMax == 0)) { return; }

            //--------------------------------------------------------------------------------------------------------------
            // If we are here, means there is an entered data before, just update the dialog window with that
            //
            double[,] kalmanTempData = new double[nMax, nMax];

            // Initialize the indices and create the matrices
            //
            StatusLabel.Content = "Re-Opened";
            isNotReOpened = false;

            // Update indices
            StateComboBox.SelectedItem = nMax;
            MeasureComboBox.SelectedItem = mMax;

            // Enable 'Matrix' buttons
            MatrixTypeComboBox.IsEnabled = true;

            //A_Data Re-opened
            A_Data = new KalmanFilterMatrixData(nMax, nMax);
            A_TabItem.DataContext = A_Data.DTable.DefaultView;

            //P_Data Re-opened
            P_Data = new KalmanFilterMatrixData(nMax, nMax);
            P_TabItem.DataContext = P_Data.DTable.DefaultView;

            //Q_Data Re-opened
            Q_Data = new KalmanFilterMatrixData(nMax, nMax);
            Q_TabItem.DataContext = Q_Data.DTable.DefaultView;

            //H_Data Re-opened
            H_Data = new KalmanFilterMatrixData(mMax, nMax);
            H_TabItem.DataContext = H_Data.DTable.DefaultView;

            //R_Data Re-opened
            R_Data = new KalmanFilterMatrixData(mMax, mMax);
            R_TabItem.DataContext = R_Data.DTable.DefaultView;

            //K_Data Re-opened
            K_Data = new KalmanFilterMatrixData(nMax, mMax);
            K_TabItem.DataContext = K_Data.DTable.DefaultView;
          
            switch (kalmanMatrixName)
            {
                case KalmanMatrixName.X:
                    LSV2Caller.KalmanMatrixDataReading_X(KalmanMatrixType.A, out kalmanTempData);
                    for (int i = 0; i < nMax; i++) { for (int j = 0; j < nMax; j++) { A_Data.DTable.Rows[i][j] = kalmanTempData[i, j]; } }
                    LSV2Caller.KalmanMatrixDataReading_X(KalmanMatrixType.P, out kalmanTempData);
                    for (int i = 0; i < nMax; i++) { for (int j = 0; j < nMax; j++) { P_Data.DTable.Rows[i][j] = kalmanTempData[i, j]; } }
                    LSV2Caller.KalmanMatrixDataReading_X(KalmanMatrixType.Q, out kalmanTempData);
                    for (int i = 0; i < nMax; i++) { for (int j = 0; j < nMax; j++) { Q_Data.DTable.Rows[i][j] = kalmanTempData[i, j]; } }
                    LSV2Caller.KalmanMatrixDataReading_X(KalmanMatrixType.H, out kalmanTempData);
                    for (int i = 0; i < mMax; i++) { for (int j = 0; j < nMax; j++) { H_Data.DTable.Rows[i][j] = kalmanTempData[i, j]; } }
                    LSV2Caller.KalmanMatrixDataReading_X(KalmanMatrixType.R, out kalmanTempData);
                    for (int i = 0; i < mMax; i++) { for (int j = 0; j < mMax; j++) { R_Data.DTable.Rows[i][j] = kalmanTempData[i, j]; } }
                    LSV2Caller.KalmanMatrixDataReading_X(KalmanMatrixType.K, out kalmanTempData);
                    for (int i = 0; i < nMax; i++) { for (int j = 0; j < mMax; j++) { K_Data.DTable.Rows[i][j] = kalmanTempData[i, j]; } }
                    break;
                case KalmanMatrixName.Y:
                    LSV2Caller.KalmanMatrixDataReading_Y(KalmanMatrixType.A, out kalmanTempData);
                    for (int i = 0; i < nMax; i++) { for (int j = 0; j < nMax; j++) { A_Data.DTable.Rows[i][j] = kalmanTempData[i, j]; } }
                    LSV2Caller.KalmanMatrixDataReading_Y(KalmanMatrixType.P, out kalmanTempData);
                    for (int i = 0; i < nMax; i++) { for (int j = 0; j < nMax; j++) { P_Data.DTable.Rows[i][j] = kalmanTempData[i, j]; } }
                    LSV2Caller.KalmanMatrixDataReading_Y(KalmanMatrixType.Q, out kalmanTempData);
                    for (int i = 0; i < nMax; i++) { for (int j = 0; j < nMax; j++) { Q_Data.DTable.Rows[i][j] = kalmanTempData[i, j]; } }
                    LSV2Caller.KalmanMatrixDataReading_Y(KalmanMatrixType.H, out kalmanTempData);
                    for (int i = 0; i < mMax; i++) { for (int j = 0; j < nMax; j++) { H_Data.DTable.Rows[i][j] = kalmanTempData[i, j]; } }
                    LSV2Caller.KalmanMatrixDataReading_Y(KalmanMatrixType.R, out kalmanTempData);
                    for (int i = 0; i < mMax; i++) { for (int j = 0; j < mMax; j++) { R_Data.DTable.Rows[i][j] = kalmanTempData[i, j]; } }
                    LSV2Caller.KalmanMatrixDataReading_Y(KalmanMatrixType.K, out kalmanTempData);
                    for (int i = 0; i < nMax; i++) { for (int j = 0; j < mMax; j++) { K_Data.DTable.Rows[i][j] = kalmanTempData[i, j]; } }
                    break;
                case KalmanMatrixName.Z:
                    LSV2Caller.KalmanMatrixDataReading_Z(KalmanMatrixType.A, out kalmanTempData);
                    for (int i = 0; i < nMax; i++) { for (int j = 0; j < nMax; j++) { A_Data.DTable.Rows[i][j] = kalmanTempData[i, j]; } }
                    LSV2Caller.KalmanMatrixDataReading_Z(KalmanMatrixType.P, out kalmanTempData);
                    for (int i = 0; i < nMax; i++) { for (int j = 0; j < nMax; j++) { P_Data.DTable.Rows[i][j] = kalmanTempData[i, j]; } }
                    LSV2Caller.KalmanMatrixDataReading_Z(KalmanMatrixType.Q, out kalmanTempData);
                    for (int i = 0; i < nMax; i++) { for (int j = 0; j < nMax; j++) { Q_Data.DTable.Rows[i][j] = kalmanTempData[i, j]; } }
                    LSV2Caller.KalmanMatrixDataReading_Z(KalmanMatrixType.H, out kalmanTempData);
                    for (int i = 0; i < mMax; i++) { for (int j = 0; j < nMax; j++) { H_Data.DTable.Rows[i][j] = kalmanTempData[i, j]; } }
                    LSV2Caller.KalmanMatrixDataReading_Z(KalmanMatrixType.R, out kalmanTempData);
                    for (int i = 0; i < mMax; i++) { for (int j = 0; j < mMax; j++) { R_Data.DTable.Rows[i][j] = kalmanTempData[i, j]; } }
                    LSV2Caller.KalmanMatrixDataReading_Z(KalmanMatrixType.K, out kalmanTempData);
                    for (int i = 0; i < nMax; i++) { for (int j = 0; j < mMax; j++) { K_Data.DTable.Rows[i][j] = kalmanTempData[i, j]; } }
                    break;
            }
        }

        //******************************************************************************************************
        // Class: KalmanFilterMatrixData
        //
        // Description: It writes the matrix data to the window
        //
        //******************************************************************************************************
        //
        class KalmanFilterMatrixData
        {
            private DataTable dTable;

            public DataTable DTable
            {
                get { return dTable; }
                set { dTable = value; }
            }

            internal KalmanFilterMatrixData(int matrixRows, int matrixColumns)
            {
                DTable = new DataTable();

                DataColumn column;
                DataRow row;

                for (int i = 1; i <= matrixColumns; i++)
                {
                    //string name = "A" + i;
                    string name = "Colm " + i;
                    column = new DataColumn();
                    column.DataType = typeof(double);
                    column.ColumnName = name;
                    DTable.Columns.Add(column);
                }

                for (int i = 0; i < matrixRows; i++)
                {
                    row = DTable.NewRow();
                    DTable.Rows.Add(row);
                }
            }

            internal KalmanFilterMatrixData(double[,] matrix)
                : this(matrix.GetLength(0), matrix.GetLength(1))
            {
                int rows = DTable.Rows.Count;
                int columns = DTable.Columns.Count;
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < columns; j++) { DTable.Rows[i][j] = matrix[i, j]; }
                }
            }
        } // End of class KalmanFilterMatrixData

        //******************************************************************************************************
        // Events and Utulity Procedures
        //
        //******************************************************************************************************
        //
        private void MatrixGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
        }

        private void MatrixGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = "  Row " + (e.Row.GetIndex() + 1).ToString() + "     ";
            //e.Row.Header = (e.Row.GetIndex() + 1).ToString();
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

        public static bool IsNumeric(string s) { double n; return Double.TryParse(s, out n); }

        private void UpdateErrorStatus(string status)
        {
            string tempStatus = (string)StatusLabel.Content;
            StatusLabel.Foreground = Brushes.Red;
            StatusLabel.Content = status;
            DoEvents();
            Thread.Sleep(2000);
            StatusLabel.Foreground = Brushes.Black;
            StatusLabel.Content = tempStatus;
        }

        private bool CheckAnyMissingDataInMatrix()
        {
            bool isDataMissing = false;
            
            // Only A_Data checking is enough to decide that matrices are created
            if (A_Data != null)
            {
                // Now check if any data entered, or there is any missing data entry
                nMax = StateComboBox.SelectedIndex + 1;
                mMax = MeasureComboBox.SelectedIndex + 1;

                bool errorCoughtForA = false;
                bool errorCoughtForP = false;
                bool errorCoughtForQ = false;
                bool errorCoughtForH = false;
                bool errorCoughtForR = false;
                bool errorCoughtForK = false;

                errorString = "Missing data entry for : ";

                // Check A, P and Q at the same time since they are all nxn
                for (int i = 0; i < nMax; i++)
                {
                    for (int j = 0; j < nMax; j++)
                    {
                        errorCoughtForA = !IsNumeric(A_Data.DTable.Rows[i][j].ToString());
                        errorCoughtForP = !IsNumeric(P_Data.DTable.Rows[i][j].ToString());
                        errorCoughtForQ = !IsNumeric(Q_Data.DTable.Rows[i][j].ToString());
                    }
                }

                if (errorCoughtForA) { errorString += "A "; }
                if (errorCoughtForP) { errorString += "P "; }
                if (errorCoughtForQ) { errorString += "Q "; }

                // Check H
                for (int i = 0; i < mMax; i++)
                {
                    for (int j = 0; j < nMax; j++) { errorCoughtForH = !IsNumeric(H_Data.DTable.Rows[i][j].ToString()); }
                }
                if (errorCoughtForH) { errorString += "H "; }

                // Check R
                for (int i = 0; i < mMax; i++)
                {
                    for (int j = 0; j < mMax; j++) { errorCoughtForR = !IsNumeric(R_Data.DTable.Rows[i][j].ToString()); }
                }
                if (errorCoughtForR) { errorString += "R "; }

                // Check K
                for (int i = 0; i < nMax; i++)
                {
                    for (int j = 0; j < mMax; j++) { errorCoughtForK = !IsNumeric(K_Data.DTable.Rows[i][j].ToString()); }
                }
                if (errorCoughtForK) { errorString += "K "; }

                if (errorCoughtForA || errorCoughtForP || errorCoughtForQ || errorCoughtForH || errorCoughtForR || errorCoughtForK)
                {
                    // There is a missing entry, don't save it, update error status and just return
                    UpdateErrorStatus(errorString);
                    isDataMissing = true;
                }
            }
            else
            {
                // Data is NOT entered, don't save it, update error status and just return
                UpdateErrorStatus("No data Entered, or Loaded from a file !!!");
                isDataMissing = true;
            }

            return isDataMissing;
        }

        //******************************************************************************************************
        // Click Procedures
        //
        //******************************************************************************************************
        //
        private void DataEntryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (DataEntryComboBox.SelectedIndex)
            {
                case 0: // Not Selected
                    StateComboBox.IsEnabled = false;
                    StateComboBox.SelectedIndex = 0;
                    MeasureComboBox.IsEnabled = false;
                    MeasureComboBox.SelectedIndex = 0;
                    EnterIndexButton.IsEnabled = false;
                    MatrixTypeComboBox.IsEnabled = false;
                    MatrixTypeComboBox.SelectedIndex = 0;
                    FillUpTypeComboBox.IsEnabled = false;
                    FillUpTypeComboBox.SelectedIndex = 0;
                    FillUpDataTextBox.IsEnabled = false;
                    EnterFillButton.IsEnabled = false;
                    StatusLabel.Content = "Empty";

                    // Reset n & m values
                    nMax = 0;
                    mMax = 0;

                    // Clear all tables
                    A_TabItem.DataContext = null;
                    P_TabItem.DataContext = null;
                    Q_TabItem.DataContext = null;
                    H_TabItem.DataContext = null;
                    R_TabItem.DataContext = null;
                    K_TabItem.DataContext = null;
                    break;

                case 1: // Manually
                    // Clear all tables
                    A_TabItem.DataContext = null;
                    P_TabItem.DataContext = null;
                    Q_TabItem.DataContext = null;
                    H_TabItem.DataContext = null;
                    R_TabItem.DataContext = null;
                    K_TabItem.DataContext = null;

                    StateComboBox.IsEnabled = true;
                    MeasureComboBox.IsEnabled = true;

                    // Reset state (n) and Measure (m) in any case to come here again
                    StateComboBox.SelectedItem = 1;
                    MeasureComboBox.SelectedItem = 1;

                    EnterIndexButton.IsEnabled = true;
                    StatusLabel.Content = "Being Filled Up...";
                    break;

                case 2: // Load from File
                    // Clear all tables
                    A_TabItem.DataContext = null;
                    P_TabItem.DataContext = null;
                    Q_TabItem.DataContext = null;
                    H_TabItem.DataContext = null;
                    R_TabItem.DataContext = null;
                    K_TabItem.DataContext = null;

                    var selectDestFolder = new OpenFileDialog();
                    selectDestFolder.Filter = "Text Files (*.txt)|*.txt";
                    selectDestFolder.Multiselect = true;
                    selectDestFolder.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                    if (selectDestFolder.ShowDialog() == true)
                    {
                        // Load the input data file to the tracing pane
                        //
                        if (System.IO.File.Exists(selectDestFolder.FileName))
                        {
                            var simulatedDataLines = System.IO.File.ReadLines(selectDestFolder.FileName);

                            int fileIdx = 0;

                            // Calculate the counter index <n>
                            string[] columnWords = simulatedDataLines.ElementAt(0).Split(' ');
                            nMax = columnWords.Count();
                            double[,] dummyMatrix = new double[nMax, nMax];

                            // Writing to matrix A, size nxn
                            //
                            for (int i = 0; i < nMax; i++)
                            {
                                string recordLine = simulatedDataLines.ElementAt(fileIdx);

                                string[] words = recordLine.Split(' ');

                                for (int j = 0; j < nMax; j++)
                                {
                                    dummyMatrix[i, j] = Convert.ToDouble(words[j]);
                                }

                                fileIdx++;
                            }
                            A_Data = new KalmanFilterMatrixData(dummyMatrix);
                            A_TabItem.DataContext = A_Data.DTable.DefaultView;

                            // Writing to matrix P, size nxn
                            //
                            for (int i = 0; i < nMax; i++)
                            {
                                string recordLine = simulatedDataLines.ElementAt(fileIdx);

                                string[] words = recordLine.Split(' ');

                                for (int j = 0; j < nMax; j++)
                                {
                                    dummyMatrix[i, j] = Convert.ToDouble(words[j]);
                                }

                                fileIdx++;
                            }
                            P_Data = new KalmanFilterMatrixData(dummyMatrix);
                            P_TabItem.DataContext = P_Data.DTable.DefaultView;

                            // Writing to matrix Q, size nxn
                            //
                            for (int i = 0; i < nMax; i++)
                            {
                                string recordLine = simulatedDataLines.ElementAt(fileIdx);

                                string[] words = recordLine.Split(' ');

                                for (int j = 0; j < nMax; j++)
                                {
                                    dummyMatrix[i, j] = Convert.ToDouble(words[j]);
                                }

                                fileIdx++;
                            }
                            Q_Data = new KalmanFilterMatrixData(dummyMatrix);
                            Q_TabItem.DataContext = Q_Data.DTable.DefaultView;

                            // Calculate the counter index <m>
                            int tempCnt = fileIdx;
                            mMax = 0;
                            string[] wordsTemp;
                            int wordsTempCnt = nMax;

                            while (nMax == wordsTempCnt)
                            {
                                string recordLineTemp = simulatedDataLines.ElementAt(tempCnt);
                                wordsTemp = recordLineTemp.Split(' ');
                                wordsTempCnt = wordsTemp.Count();
                                mMax++;
                                tempCnt++;
                            }
                            mMax--;

                            // Writing to matrix H, size mxn
                            //
                            double[,] dummyMatrixH = new double[mMax, nMax];
                            for (int i = 0; i < mMax; i++)
                            {
                                string recordLine = simulatedDataLines.ElementAt(fileIdx);

                                string[] words = recordLine.Split(' ');

                                for (int j = 0; j < nMax; j++)
                                {
                                    dummyMatrixH[i, j] = Convert.ToDouble(words[j]);
                                }

                                fileIdx++;
                            }
                            H_Data = new KalmanFilterMatrixData(dummyMatrixH);
                            H_TabItem.DataContext = H_Data.DTable.DefaultView;

                            // Writing to matrix R, size mxm
                            //
                            double[,] dummyMatrixR = new double[mMax, mMax];
                            for (int i = 0; i < mMax; i++)
                            {
                                string recordLine = simulatedDataLines.ElementAt(fileIdx);

                                string[] words = recordLine.Split(' ');

                                for (int j = 0; j < mMax; j++)
                                {
                                    dummyMatrixR[i, j] = Convert.ToDouble(words[j]);
                                }

                                fileIdx++;
                            }
                            R_Data = new KalmanFilterMatrixData(dummyMatrixR);
                            R_TabItem.DataContext = R_Data.DTable.DefaultView;

                            // Writing to matrix K, size nxm
                            //
                            double[,] dummyMatrixK = new double[nMax, mMax];
                            for (int i = 0; i < nMax; i++)
                            {
                                string recordLine = simulatedDataLines.ElementAt(fileIdx);

                                string[] words = recordLine.Split(' ');

                                for (int j = 0; j < mMax; j++)
                                {
                                    dummyMatrixK[i, j] = Convert.ToDouble(words[j]);
                                }

                                fileIdx++;
                            }
                            K_Data = new KalmanFilterMatrixData(dummyMatrixK);
                            K_TabItem.DataContext = K_Data.DTable.DefaultView;

                            // Update Matrix Index Buttons to show up index values
                            StateComboBox.SelectedIndex = nMax - 1;
                            MeasureComboBox.SelectedIndex = mMax - 1;

                            // Status label setting
                            StatusLabel.Content = selectDestFolder.FileName;
                        }
                    }

                    // Save the filename and initial directory to use them while 'save' and 'save as' actions
                    kalmanFileName = selectDestFolder.SafeFileName;
                    kalmanDirectory = System.IO.Path.GetDirectoryName(selectDestFolder.FileName);
                    break;
            }
        }

        private void EnterIndexButton_Click(object sender, RoutedEventArgs e)
        {
            // Initialize the 'n' and 'm' values from the entry buttons
            nMax = Convert.ToInt16(StateComboBox.SelectedItem);
            mMax = Convert.ToInt16(MeasureComboBox.SelectedItem);

            //A_Data Manuel entry
            A_Data = new KalmanFilterMatrixData(nMax, nMax);
            A_TabItem.DataContext = A_Data.DTable.DefaultView;

            //P_Data Manuel entry
            P_Data = new KalmanFilterMatrixData(nMax, nMax);
            P_TabItem.DataContext = P_Data.DTable.DefaultView;

            //Q_Data Manuel entry
            Q_Data = new KalmanFilterMatrixData(nMax, nMax);
            Q_TabItem.DataContext = Q_Data.DTable.DefaultView;

            //H_Data Manuel entry
            H_Data = new KalmanFilterMatrixData(mMax, nMax);
            H_TabItem.DataContext = H_Data.DTable.DefaultView;

            //R_Data Manuel entry
            R_Data = new KalmanFilterMatrixData(mMax, mMax);
            R_TabItem.DataContext = R_Data.DTable.DefaultView;

            //K_Data Manuel entry
            K_Data = new KalmanFilterMatrixData(nMax, mMax);
            K_TabItem.DataContext = K_Data.DTable.DefaultView;

            // Enable 'Matrix' buttons
            MatrixTypeComboBox.IsEnabled = true;

            //// Enable 'Fill Up' buttons
            //FillUpTypeComboBox.IsEnabled = true;
        }

        private void MatrixTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MatrixTypeComboBox.SelectedIndex == 0)
            {
                FillUpTypeComboBox.IsEnabled = false;
                FillUpDataTextBox.IsEnabled = false;
                EnterFillButton.IsEnabled = false;
            }
            else
            {
                FillUpTypeComboBox.IsEnabled = true;
                //FillUpDataTextBox.IsEnabled = true;
                //EnterFillButton.IsEnabled = true;
            }
        }

        private void FillUpTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FillUpTypeComboBox.SelectedIndex == 0)
            {
                FillUpDataTextBox.IsEnabled = false;
                EnterFillButton.IsEnabled = false;
            }
            else
            {
                FillUpDataTextBox.IsEnabled = true;
                EnterFillButton.IsEnabled = true;
            }        
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // First check whether Data entry type is not entered, or entered but there is missing data entry
            if (DataEntryComboBox.SelectedIndex == 0)
            {
                // Data is NOT entered, don't save it, update error status and just return
                UpdateErrorStatus("Data Entry Method is NOT seletcted yet !!!");
                return;
            }
            else
            {
                // Check whether there is any missing data, if there is don't save, just return
                if (CheckAnyMissingDataInMatrix())
                {
                    UpdateErrorStatus(errorString);
                    return;
                }
            }

            // Data is entered properly, so continue to save the data
            //
            Microsoft.Win32.SaveFileDialog savedlg = new Microsoft.Win32.SaveFileDialog();

            if (kalmanFileName == null)
            {
                savedlg.FileName = "KalmanFilter_" + KalmanWindowButton.Content + DateTime.Now.ToString("_ddMMyy") + "_"
                            + DateTime.Now.ToString("HHmmss"); // Default file name

            }
            else { savedlg.FileName = kalmanFileName; }

            savedlg.DefaultExt = ".txt"; // Default file extension
            savedlg.Filter = "Text files (.txt)|*.txt"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = savedlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string fileName = savedlg.FileName;

                using (var fs = new FileStream(fileName, FileMode.Append))
                {
                    // Write A
                    for (int i = 0; i < nMax; i++)
                    {
                        string recordLine = "";
                        for (int j = 0; j < nMax - 1; j++) { recordLine += A_Data.DTable.Rows[i][j].ToString() + " "; }
                        recordLine += A_Data.DTable.Rows[i][nMax - 1].ToString() + "\r\n";

                        byte[] bdata = Encoding.Default.GetBytes(recordLine);
                        fs.Write(bdata, 0, bdata.Length);
                    }

                    // Write P
                    for (int i = 0; i < nMax; i++)
                    {
                        string recordLine = "";
                        for (int j = 0; j < nMax - 1; j++) { recordLine += P_Data.DTable.Rows[i][j].ToString() + " "; }
                        recordLine += P_Data.DTable.Rows[i][nMax - 1].ToString() + "\r\n";

                        byte[] bdata = Encoding.Default.GetBytes(recordLine);
                        fs.Write(bdata, 0, bdata.Length);
                    }

                    // Write Q
                    for (int i = 0; i < nMax; i++)
                    {
                        string recordLine = "";
                        for (int j = 0; j < nMax - 1; j++) { recordLine += Q_Data.DTable.Rows[i][j].ToString() + " "; }
                        recordLine += Q_Data.DTable.Rows[i][nMax - 1].ToString() + "\r\n";

                        byte[] bdata = Encoding.Default.GetBytes(recordLine);
                        fs.Write(bdata, 0, bdata.Length);
                    }

                    // Check H
                    for (int i = 0; i < mMax; i++)
                    {
                        string recordLine = "";
                        for (int j = 0; j < nMax - 1; j++) { recordLine += H_Data.DTable.Rows[i][j].ToString() + " "; }
                        recordLine += H_Data.DTable.Rows[i][nMax - 1].ToString() + "\r\n";

                        byte[] bdata = Encoding.Default.GetBytes(recordLine);
                        fs.Write(bdata, 0, bdata.Length);
                    }

                    // Check R
                    for (int i = 0; i < mMax; i++)
                    {
                        string recordLine = "";
                        for (int j = 0; j < mMax - 1; j++) { recordLine += R_Data.DTable.Rows[i][j].ToString() + " "; }
                        recordLine += R_Data.DTable.Rows[i][mMax - 1].ToString() + "\r\n";

                        byte[] bdata = Encoding.Default.GetBytes(recordLine);
                        fs.Write(bdata, 0, bdata.Length);
                    }

                    // Check K
                    for (int i = 0; i < nMax; i++)
                    {
                        string recordLine = "";
                        for (int j = 0; j < mMax - 1; j++) { recordLine += K_Data.DTable.Rows[i][j].ToString() + " "; }
                        recordLine += K_Data.DTable.Rows[i][mMax - 1].ToString() + "\r\n";

                        byte[] bdata = Encoding.Default.GetBytes(recordLine);
                        fs.Write(bdata, 0, bdata.Length);
                    }

                    // Status label settings
                    StatusLabel.Content = savedlg.FileName;
                }
            } // Saving to a file
        }

        private void EnterFillButton_Click(object sender, RoutedEventArgs e)
        {
            switch (MatrixTypeComboBox.SelectedIndex)
            {
                case 0: // Not Selected

                    break;

                case 1: // Matrix A
                    switch (FillUpTypeComboBox.SelectedIndex)
                    {
                        case 0:
                            // Do nothing
                            break;
                        case 1:
                            // Whole
                            for (int i = 0; i < nMax; i++)
                            {
                                for (int j = 0; j < nMax; j++) { A_Data.DTable.Rows[i][j] = Convert.ToDouble(FillUpDataTextBox.Text); }
                            }
                            break;
                        case 2:
                            // Empty fields
                            EmptyFieldsAPQ("A");                            
                            break;
                    }
                    MatrixTab.SelectedIndex = 0;
                    break;

                case 2: // Matrix P
                    switch (FillUpTypeComboBox.SelectedIndex)
                    {
                        case 0:
                            // Do nothing
                            break;
                        case 1:
                            // Whole
                            for (int i = 0; i < nMax; i++)
                            {
                                for (int j = 0; j < nMax; j++) { P_Data.DTable.Rows[i][j] = Convert.ToDouble(FillUpDataTextBox.Text); }
                            }
                            break;
                        case 2:
                            // Empty fields
                            EmptyFieldsAPQ("P");                            
                            break;
                    }
                    MatrixTab.SelectedIndex = 1;
                    break;

                case 3: // Matrix Q
                    switch (FillUpTypeComboBox.SelectedIndex)
                    {
                        case 0:
                            // Do nothing
                            break;
                        case 1:
                            // Whole
                            for (int i = 0; i < nMax; i++)
                            {
                                for (int j = 0; j < nMax; j++) { Q_Data.DTable.Rows[i][j] = Convert.ToDouble(FillUpDataTextBox.Text); }
                            }
                            break;
                        case 2:
                            // Empty fields
                            EmptyFieldsAPQ("Q");                            
                            break;
                    }
                    MatrixTab.SelectedIndex = 2;
                    break;

                case 4: // Matrix H
                    switch (FillUpTypeComboBox.SelectedIndex)
                    {
                        case 0:
                            // Do nothing
                            break;
                        case 1:
                            // Whole
                            for (int i = 0; i < mMax; i++)
                            {
                                for (int j = 0; j < nMax; j++) { H_Data.DTable.Rows[i][j] = Convert.ToDouble(FillUpDataTextBox.Text); }
                            }
                            break;
                        case 2:
                            // Empty fields
                            EmptyFieldsH();
                            break;
                    }
                    MatrixTab.SelectedIndex = 3;
                    break;

                case 5: // Matrix R
                    switch (FillUpTypeComboBox.SelectedIndex)
                    {
                        case 0:
                            // Do nothing
                            break;
                        case 1:
                            // Whole
                            for (int i = 0; i < mMax; i++)
                            {
                                for (int j = 0; j < mMax; j++) { R_Data.DTable.Rows[i][j] = Convert.ToDouble(FillUpDataTextBox.Text); }
                            }
                            break;
                        case 2:
                            // Empty fields
                            EmptyFieldsR();
                            break;
                    }
                    MatrixTab.SelectedIndex = 4;
                    break;

                case 6: // Matrix K
                    switch (FillUpTypeComboBox.SelectedIndex)
                    {
                        case 0:
                            // Do nothing
                            break;
                        case 1:
                            // Whole
                            for (int i = 0; i < nMax; i++)
                            {
                                for (int j = 0; j < mMax; j++) { K_Data.DTable.Rows[i][j] = Convert.ToDouble(FillUpDataTextBox.Text); }
                            }
                            break;
                        case 2:
                            // Empty fields
                            EmptyFieldsK();
                            break;
                    }
                    MatrixTab.SelectedIndex = 5;
                    break;

                case 7: // All
                    switch (FillUpTypeComboBox.SelectedIndex)
                    {
                        case 0:
                            // Do nothing
                            break;
                        case 1:
                            // Whole

                            // For Matrix A, P and Q
                            for (int i = 0; i < nMax; i++)
                            {
                                for (int j = 0; j < nMax; j++)
                                {
                                    A_Data.DTable.Rows[i][j] = Convert.ToDouble(FillUpDataTextBox.Text);
                                    P_Data.DTable.Rows[i][j] = Convert.ToDouble(FillUpDataTextBox.Text);
                                    Q_Data.DTable.Rows[i][j] = Convert.ToDouble(FillUpDataTextBox.Text);
                                }
                            }

                            // For Matrix H
                            for (int i = 0; i < mMax; i++)
                            {
                                for (int j = 0; j < nMax; j++)
                                {
                                    H_Data.DTable.Rows[i][j] = Convert.ToDouble(FillUpDataTextBox.Text);
                                }
                            }

                            // For Matrix R
                            for (int i = 0; i < mMax; i++)
                            {
                                for (int j = 0; j < mMax; j++)
                                {
                                    R_Data.DTable.Rows[i][j] = Convert.ToDouble(FillUpDataTextBox.Text);
                                }
                            }

                            // For Matrix K                   
                            for (int i = 0; i < nMax; i++)
                            {
                                for (int j = 0; j < mMax; j++)
                                {
                                    K_Data.DTable.Rows[i][j] = Convert.ToDouble(FillUpDataTextBox.Text);
                                }
                            }
                            break;
                        case 2:
                            // Empty fields
                            EmptyFieldsAPQ("ALL");
                            EmptyFieldsH();
                            EmptyFieldsR();
                            EmptyFieldsK();
                            break;
                    }
                    MatrixTab.SelectedIndex = 0;
                    break;
            }
        }

        private void EmptyFieldsAPQ(string matrixType) // Valid range: "A", "P", "Q", "ALL"
        {
            // For Matrix A, P and Q
            for (int i = 0; i < nMax; i++)
            {
                for (int j = 0; j < nMax; j++)
                {
                    switch (matrixType)
                    {
                        case "A":
                            if (!IsNumeric(A_Data.DTable.Rows[i][j].ToString()))
                            {
                                A_Data.DTable.Rows[i][j] = Convert.ToDouble(FillUpDataTextBox.Text);
                            }
                            break;
                        case "P":
                            if (!IsNumeric(P_Data.DTable.Rows[i][j].ToString()))
                            {
                                P_Data.DTable.Rows[i][j] = Convert.ToDouble(FillUpDataTextBox.Text);
                            }
                            break;
                        case "Q":
                            if (!IsNumeric(Q_Data.DTable.Rows[i][j].ToString()))
                            {
                                Q_Data.DTable.Rows[i][j] = Convert.ToDouble(FillUpDataTextBox.Text);
                            }
                            break;
                        case "ALL":
                            if (!IsNumeric(A_Data.DTable.Rows[i][j].ToString()))
                            {
                                A_Data.DTable.Rows[i][j] = Convert.ToDouble(FillUpDataTextBox.Text);
                            }
                            if (!IsNumeric(P_Data.DTable.Rows[i][j].ToString()))
                            {
                                P_Data.DTable.Rows[i][j] = Convert.ToDouble(FillUpDataTextBox.Text);
                            }
                            if (!IsNumeric(Q_Data.DTable.Rows[i][j].ToString()))
                            {
                                Q_Data.DTable.Rows[i][j] = Convert.ToDouble(FillUpDataTextBox.Text);
                            }
                            break;
                    }
                }
            }
        }

        private void EmptyFieldsH()
        {
            // For Matrix H
            for (int i = 0; i < mMax; i++)
            {
                for (int j = 0; j < nMax; j++)
                {
                    if (!IsNumeric(H_Data.DTable.Rows[i][j].ToString()))
                    {
                        H_Data.DTable.Rows[i][j] = Convert.ToDouble(FillUpDataTextBox.Text);
                    }
                }
            }
        }

        private void EmptyFieldsR()
        {
            // For Matrix R
            for (int i = 0; i < mMax; i++)
            {
                for (int j = 0; j < mMax; j++)
                {
                    if (!IsNumeric(R_Data.DTable.Rows[i][j].ToString()))
                    {
                        R_Data.DTable.Rows[i][j] = Convert.ToDouble(FillUpDataTextBox.Text);
                    }
                }
            }
        }

        private void EmptyFieldsK()
        {
            // For Matrix K                   
            for (int i = 0; i < nMax; i++)
            {
                for (int j = 0; j < mMax; j++)
                {
                    if (!IsNumeric(K_Data.DTable.Rows[i][j].ToString()))
                    {
                        K_Data.DTable.Rows[i][j] = Convert.ToDouble(FillUpDataTextBox.Text);
                    }
                }
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            // First check whether Data entry type is not entered, or entered but there is missing data entry
            if ((DataEntryComboBox.SelectedIndex == 0) && isNotReOpened)
            {
                // Data is NOT entered, no need to clear
                UpdateErrorStatus("Data Entry Method is NOT seletcted yet !!!");
                return;
            }

            // Only A_Data checking is enough to decide that data is entered
            if ((A_Data == null) && isNotReOpened)
            {
                // Data is NOT entered, no need to clear
                UpdateErrorStatus("Data is NOT entered yet !!!");
                return;
            }

            // Since we are here, data exists to clear, then clear it         
            //
            //A_Data Manuel entry
            A_Data = new KalmanFilterMatrixData(nMax, nMax);
            A_TabItem.DataContext = A_Data.DTable.DefaultView;

            //P_Data Manuel entry
            P_Data = new KalmanFilterMatrixData(nMax, nMax);
            P_TabItem.DataContext = P_Data.DTable.DefaultView;

            //Q_Data Manuel entry
            Q_Data = new KalmanFilterMatrixData(nMax, nMax);
            Q_TabItem.DataContext = Q_Data.DTable.DefaultView;

            //H_Data Manuel entry
            H_Data = new KalmanFilterMatrixData(mMax, nMax);
            H_TabItem.DataContext = H_Data.DTable.DefaultView;

            //R_Data Manuel entry
            R_Data = new KalmanFilterMatrixData(mMax, mMax);
            R_TabItem.DataContext = R_Data.DTable.DefaultView;

            //K_Data Manuel entry
            K_Data = new KalmanFilterMatrixData(nMax, mMax);
            K_TabItem.DataContext = K_Data.DTable.DefaultView;
        }

        private void OkayButton_Click(object sender, RoutedEventArgs e)
        {
            if (CheckAnyMissingDataInMatrix()) { return; }

            // This is not GOOD
            //// No need to save any data if 'n' or 'm' is zero, means no data entered, just close dialog and return
            //if ((nMax == 0) || (mMax == 0)) { dialogClosedWithoutData = true; this.Close(); return; }
            //if (CheckAnyMissingDataInMatrix()) { dialogClosedWithoutData = true; this.Close(); return; }
                       
            // Since we are here, there is a valid data, so save n & m values
            switch (kalmanMatrixName)
            {
                case KalmanMatrixName.X:
                    LSV2Caller.SetMaxState_n_X(nMax);
                    LSV2Caller.SetMaxMeasurement_m_X(mMax);
                    break;
                case KalmanMatrixName.Y:
                    LSV2Caller.SetMaxState_n_Y(nMax);
                    LSV2Caller.SetMaxMeasurement_m_Y(mMax);
                    break;
                case KalmanMatrixName.Z:
                    LSV2Caller.SetMaxState_n_Z(nMax);
                    LSV2Caller.SetMaxMeasurement_m_Z(mMax);
                    break;
            }

            double[,] tempData = new double[nMax, nMax];
            double[,] tempDataH = new double[mMax, nMax];
            double[,] tempDataR = new double[mMax, mMax];
            double[,] tempDataK = new double[nMax, mMax];
            
            // Store the data
            switch (kalmanMatrixName)
            {
                case KalmanMatrixName.X:
                    for (int i = 0; i < nMax; i++) { for (int j = 0; j < nMax; j++) { tempData[i, j] = Convert.ToDouble(A_Data.DTable.Rows[i][j]); } }
                    LSV2Caller.KalmanMatrixDataWritting_X(KalmanMatrixType.A, tempData);
                    for (int i = 0; i < nMax; i++) { for (int j = 0; j < nMax; j++) { tempData[i, j] = Convert.ToDouble(P_Data.DTable.Rows[i][j]); } }
                    LSV2Caller.KalmanMatrixDataWritting_X(KalmanMatrixType.P, tempData);
                    for (int i = 0; i < nMax; i++) { for (int j = 0; j < nMax; j++) { tempData[i, j] = Convert.ToDouble(Q_Data.DTable.Rows[i][j]); } }
                    LSV2Caller.KalmanMatrixDataWritting_X(KalmanMatrixType.Q, tempData);
                    for (int i = 0; i < mMax; i++) { for (int j = 0; j < nMax; j++) { tempDataH[i, j] = Convert.ToDouble(H_Data.DTable.Rows[i][j]); } }
                    LSV2Caller.KalmanMatrixDataWritting_X(KalmanMatrixType.H, tempDataH);
                    for (int i = 0; i < mMax; i++) { for (int j = 0; j < mMax; j++) { tempDataR[i, j] = Convert.ToDouble(R_Data.DTable.Rows[i][j]); } }
                    LSV2Caller.KalmanMatrixDataWritting_X(KalmanMatrixType.R, tempDataR);
                    for (int i = 0; i < nMax; i++) { for (int j = 0; j < mMax; j++) { tempDataK[i, j] = Convert.ToDouble(K_Data.DTable.Rows[i][j]); } }
                    LSV2Caller.KalmanMatrixDataWritting_X(KalmanMatrixType.K, tempDataK);
                    break;
                case KalmanMatrixName.Y:
                    for (int i = 0; i < nMax; i++) { for (int j = 0; j < nMax; j++) { tempData[i, j] = Convert.ToDouble(A_Data.DTable.Rows[i][j]); } }
                    LSV2Caller.KalmanMatrixDataWritting_Y(KalmanMatrixType.A, tempData);
                    for (int i = 0; i < nMax; i++) { for (int j = 0; j < nMax; j++) { tempData[i, j] = Convert.ToDouble(P_Data.DTable.Rows[i][j]); } }
                    LSV2Caller.KalmanMatrixDataWritting_Y(KalmanMatrixType.P, tempData);
                    for (int i = 0; i < nMax; i++) { for (int j = 0; j < nMax; j++) { tempData[i, j] = Convert.ToDouble(Q_Data.DTable.Rows[i][j]); } }
                    LSV2Caller.KalmanMatrixDataWritting_Y(KalmanMatrixType.Q, tempData);
                    for (int i = 0; i < mMax; i++) { for (int j = 0; j < nMax; j++) { tempDataH[i, j] = Convert.ToDouble(H_Data.DTable.Rows[i][j]); } }
                    LSV2Caller.KalmanMatrixDataWritting_Y(KalmanMatrixType.H, tempDataH);
                    for (int i = 0; i < mMax; i++) { for (int j = 0; j < mMax; j++) { tempDataR[i, j] = Convert.ToDouble(R_Data.DTable.Rows[i][j]); } }
                    LSV2Caller.KalmanMatrixDataWritting_Y(KalmanMatrixType.R, tempDataR);
                    for (int i = 0; i < nMax; i++) { for (int j = 0; j < mMax; j++) { tempDataK[i, j] = Convert.ToDouble(K_Data.DTable.Rows[i][j]); } }
                    LSV2Caller.KalmanMatrixDataWritting_Y(KalmanMatrixType.K, tempDataK);
                    break;
                case KalmanMatrixName.Z:
                    for (int i = 0; i < nMax; i++) { for (int j = 0; j < nMax; j++) { tempData[i, j] = Convert.ToDouble(A_Data.DTable.Rows[i][j]); } }
                    LSV2Caller.KalmanMatrixDataWritting_Z(KalmanMatrixType.A, tempData);
                    for (int i = 0; i < nMax; i++) { for (int j = 0; j < nMax; j++) { tempData[i, j] = Convert.ToDouble(P_Data.DTable.Rows[i][j]); } }
                    LSV2Caller.KalmanMatrixDataWritting_Z(KalmanMatrixType.P, tempData);
                    for (int i = 0; i < nMax; i++) { for (int j = 0; j < nMax; j++) { tempData[i, j] = Convert.ToDouble(Q_Data.DTable.Rows[i][j]); } }
                    LSV2Caller.KalmanMatrixDataWritting_Z(KalmanMatrixType.Q, tempData);
                    for (int i = 0; i < mMax; i++) { for (int j = 0; j < nMax; j++) { tempDataH[i, j] = Convert.ToDouble(H_Data.DTable.Rows[i][j]); } }
                    LSV2Caller.KalmanMatrixDataWritting_Z(KalmanMatrixType.H, tempDataH);
                    for (int i = 0; i < mMax; i++) { for (int j = 0; j < mMax; j++) { tempDataR[i, j] = Convert.ToDouble(R_Data.DTable.Rows[i][j]); } }
                    LSV2Caller.KalmanMatrixDataWritting_Z(KalmanMatrixType.R, tempDataR);
                    for (int i = 0; i < nMax; i++) { for (int j = 0; j < mMax; j++) { tempDataK[i, j] = Convert.ToDouble(K_Data.DTable.Rows[i][j]); } }
                    LSV2Caller.KalmanMatrixDataWritting_Z(KalmanMatrixType.K, tempDataK);
                    break;
            }

            dialogClosedWithoutData = false;
            this.Close();
        }
    }
}
