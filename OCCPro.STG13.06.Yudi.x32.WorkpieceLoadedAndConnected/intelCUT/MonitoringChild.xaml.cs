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

using System.Collections.ObjectModel;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Threading; // DispacherFrame

namespace OnlineCuttingControlProcess
{
    /// <summary>
    /// Interaction logic for MonitoringChild.xaml
    /// </summary>
    public partial class MonitoringChild : UserControl
    {
        public MainWindow mainWindow = null;

        public ObservableCollection<KeyValuePair<double, double>> SimDataKeyValuePair = new ObservableCollection<KeyValuePair<double, double>>();
        public ObservableCollection<KeyValuePair<double, double>> RtmDataKeyValuePair = new ObservableCollection<KeyValuePair<double, double>>();
        public List<ObservableCollection<KeyValuePair<double, double>>> MonitoringPlot = new List<ObservableCollection<KeyValuePair<double, double>>>();


        private double prevMax = 0;
        private double prevMin = 1000;

        double currentPositionX = 0;
        double currentPositionY = 0;

        public MonitoringChild()
        {
            InitializeComponent();
 
            MonitoringPlot.Add(SimDataKeyValuePair);
            MonitoringPlot.Add(RtmDataKeyValuePair);

            LineChart.DataContext = MonitoringPlot;

            // They are the initial values for zooming
            XLinearAxis.Maximum = 500;
            XLinearAxis.Minimum = 0;
            YLinearAxis.Maximum = 500;
            YLinearAxis.Minimum = -20;

            //// Calculate the scale values depending on max&min values of both X and Y axis.
            //if ((Convert.ToInt32((double)Power.Count / 25.0) % 10) == 0) { XLinearAxis.Interval = Convert.ToInt32((double)Power.Count / 25.0); }
            //else { XLinearAxis.Interval = ((10 - (Convert.ToInt32((double)Power.Count / 25.0) % 10)) + Convert.ToInt32((double)Power.Count / 25.0)); }

            //if ((Convert.ToInt32((double)(prevMax + 10) / 10.0) % 10) == 0) { YLinearAxis.Interval = Convert.ToInt32((double)(prevMax + 10) / 10.0); }
            //else { YLinearAxis.Interval = ((10 - (Convert.ToInt32((double)(prevMax + 10) / 10.0) % 10)) + Convert.ToInt32((double)(prevMax + 10) / 10.0)); }

            // Plot line thickness setting
            var style = new Style(typeof(Polyline));
            style.Setters.Add(new Setter(Polyline.StrokeThicknessProperty, 1d));

            SimPlotLineSeries.PolylineStyle = style;
        }

        // Move ---------------------------------------------------------------------
        //
        private void MoveUpButton_Click(object sender, RoutedEventArgs e)
        {
            YLinearAxis.Maximum -= 10;
            YLinearAxis.Minimum -= 10;
        }

        private void MoveLeftButton_Click(object sender, RoutedEventArgs e)
        {
            XLinearAxis.Maximum += 10;
            XLinearAxis.Minimum += 10;
        }

        private void MoveRightButton_Click(object sender, RoutedEventArgs e)
        {
            XLinearAxis.Maximum -= 10;
            XLinearAxis.Minimum -= 10;
        }

        private void MoveDownButton_Click(object sender, RoutedEventArgs e)
        {
            YLinearAxis.Maximum += 10;
            YLinearAxis.Minimum += 10;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            // They are the initial values for zooming
            //XLinearAxis.Maximum = Power.Count;
            //XLinearAxis.Minimum = 0;
            //if ((Convert.ToInt32((double)Power.Count / 25.0) % 10) == 0) { XLinearAxis.Interval = Convert.ToInt32((double)Power.Count / 25.0); }
            //else { XLinearAxis.Interval = ((10 - (Convert.ToInt32((double)Power.Count / 25.0) % 10)) + Convert.ToInt32((double)Power.Count / 25.0)); }

            //YLinearAxis.Maximum = prevMax + 10;
            //YLinearAxis.Minimum = prevMin - 10;
            //if ((Convert.ToInt32((double)(prevMax + 10) / 10.0) % 10) == 0) { YLinearAxis.Interval = Convert.ToInt32((double)(prevMax + 10) / 10.0); }
            //else { YLinearAxis.Interval = ((10 - (Convert.ToInt32((double)(prevMax + 10) / 10.0) % 10)) + Convert.ToInt32((double)(prevMax + 10) / 10.0)); }
        }

        // Mark Checked Button -----------------------------------------------------------
        //
        private void MarkCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //PlotLineSeries.DataPointStyle = null;
            SimPlotLineSeries.Background = Brushes.Blue;
            Style style = new Style(typeof(LineDataPoint));
            style.Setters.Add(new Setter(LineDataPoint.BackgroundProperty, Brushes.Blue));
            SimPlotLineSeries.DataPointStyle = style;
        }

        private void MarkCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            //PlotLineSeries.DataPointStyle = null;
            //PlotLineSeries.Background = Brushes.Blue;
            Style style = new Style(typeof(LineDataPoint));
            style.Setters.Add(new Setter(LineDataPoint.TemplateProperty, null));
            style.Setters.Add(new Setter(LineDataPoint.BackgroundProperty, Brushes.Blue));
            SimPlotLineSeries.DataPointStyle = style;
        }

        // Axis X, Move LEFT or RIGHT ----------------------------------------------------------------
        //
        void LineChart_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                //Console.WriteLine("Zoom IN");
                XLinearAxis.Maximum += 50;
            }

            if (e.Delta < 0)
            {
                //Console.WriteLine("Zoom OUT");
                if ((XLinearAxis.Maximum - 50) >= XLinearAxis.Minimum) { XLinearAxis.Maximum -= 50; }
            }
        }

        // Graph, Move UP or DOWN, or LEFT or RIGHT --------------------------------------------------
        //
        private void LineChart_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                double deltaDirectionX = currentPositionX - e.GetPosition(this).X;
                double deltaDirectionY = currentPositionY - e.GetPosition(this).Y;

                if (deltaDirectionX > 0)
                {
                    //Console.WriteLine("Moving Left");
                    XLinearAxis.Maximum += 5;
                    XLinearAxis.Minimum += 5;
                }
                if (deltaDirectionX < 0)
                {
                    //Console.WriteLine("Moving Right");
                    XLinearAxis.Maximum -= 5;
                    XLinearAxis.Minimum -= 5;
                }

                if (deltaDirectionY > 0)
                {
                    //Console.WriteLine("Moving UP");
                    YLinearAxis.Maximum -= 1;
                    YLinearAxis.Minimum -= 1;
                }
                if (deltaDirectionY < 0)
                {
                    //Console.WriteLine("Moving DOWN");
                    YLinearAxis.Maximum += 1;
                    YLinearAxis.Minimum += 1;
                }

                currentPositionX = e.GetPosition(this).X;
                currentPositionY = e.GetPosition(this).Y;
            }
            else
            {
                currentPositionX = e.GetPosition(this).X;
                currentPositionY = e.GetPosition(this).Y;
            }
        }

        // Scale X, Move INCREASE or DECREASE -------------------------------------------------------
        //
        void XLinearAxis_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                //Console.WriteLine("Zoom OUT");
                if (XLinearAxis.Interval > 10) { XLinearAxis.Interval -= 10; }
                else { if (XLinearAxis.Interval > 0) { XLinearAxis.Interval -= 1; } }
            }

            if (e.Delta < 0)
            {
                //Console.WriteLine("Zoom IN");
                if (XLinearAxis.Interval >= 10) { XLinearAxis.Interval += 10; }
                else { if (XLinearAxis.Interval >= 0) { XLinearAxis.Interval += 1; } }
            }
            //XLinearAxis.Maximum = Power.Count;
        }

        // Scale Y, Move INCREASE or DECREASE -------------------------------------------------------
        //
        void YLinearAxis_MouseWheel(object sender, MouseWheelEventArgs e)
        {

            if (e.Delta > 0)
            {
                //Console.WriteLine("Zoom OUT");
                if (YLinearAxis.Interval > 10) { YLinearAxis.Interval -= 10; }
                else { if (YLinearAxis.Interval > 0) { YLinearAxis.Interval -= 1; } }
            }

            if (e.Delta < 0)
            {
                //Console.WriteLine("Zoom IN");
                if (YLinearAxis.Interval >= 10) { YLinearAxis.Interval += 10; }
                else { if (YLinearAxis.Interval >= 0) { YLinearAxis.Interval += 1; } }
            }
            //XLinearAxis.Maximum = Power.Count;
        }

        // This lets main window that the child is closed.
        //
        private void LineChart_Unloaded(object sender, RoutedEventArgs e)
        {
            // Unloaded event happent when he tabs are changed or child window is closed.
            // We need to detect the child is closed
            if (mainWindow.MonitoringTabItem.IsSelected) { mainWindow.isForceVsDisMonitoring = false; } // Which means child is closed

            //Console.WriteLine("isForceVsDisMonitoring= " + mainWindow.isForceVsDisMonitoring);
            //if (LineChart.Title == "Force vs. Distance") { mainWindow.isForceVsDisMonitoring = false; }
        }

    }
}
