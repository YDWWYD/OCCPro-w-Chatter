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
    /// Interaction logic for GraphicsChild.xaml
    /// </summary>
    public class MyViewModel
    {
        public PointCollection firstPointCollection { get; set; }
        public PointCollection secondPointCollection { get; set; }

        public MyViewModel()
        {
            firstPointCollection = new PointCollection();
            secondPointCollection = new PointCollection();
        }
    }
      
    public partial class GraphicsChild : UserControl
    {
        private double prevMax = 0;
        private double prevMin = 1000;

        double currentPositionX = 0;
        double currentPositionY = 0;

        // Initial X and Y Min & Max values doe resetting
        double initialXmax = 0;
        double initialXmin = 0;
        double initialXinterval = 0;
        double initialYmax = 0;
        double initialYmin = 0;
        double initialYinterval = 0;

        bool allwaysActive = false;
        bool lineChartMouseWheelUsed = true;

        public GraphicsChild(string firstFileName, string firstFileID, int firstIdx, string secondFileName, string secondFileID, int secondIdx)
        {
            InitializeComponent();

            double firstPointCollectionCount = 0;
            double secondPointCollectionCount = 0;

            MyViewModel myViewModel = new MyViewModel();
            LineChart.DataContext = myViewModel;

            // Get the first file info
            //
            if (System.IO.File.Exists(firstFileName))
            {
                var simulatedDataLines = System.IO.File.ReadLines(firstFileName);

                double lineCount = 0;
                char[] delimeter = { '\t', ' ' };

                foreach (string line in simulatedDataLines)
                {
                    string[] words = line.Split(delimeter);

                    double tempValue = Convert.ToDouble(words[firstIdx]);

                    if (tempValue > prevMax) { prevMax = tempValue; }
                    if (tempValue < prevMin) { prevMin = tempValue; }

                    //if (lineCount % 10 == 0)
                    myViewModel.firstPointCollection.Add(new Point { X = lineCount, Y = tempValue });
                    lineCount++;
                }

                firstPointCollectionCount = myViewModel.firstPointCollection.Count;
            }

            // Get the second file info if it is NOT null
            //
            if (secondFileName != null)
            {
                if (System.IO.File.Exists(secondFileName))
                {
                    var simulatedDataLines = System.IO.File.ReadLines(secondFileName);

                    double lineCount = 0;
                    char[] delimeter = { '\t', ' ' };

                    foreach (string line in simulatedDataLines)
                    {
                        string[] words = line.Split(delimeter);

                        double tempValue = Convert.ToDouble(words[secondIdx]);

                        if (tempValue > prevMax) { prevMax = tempValue; }
                        if (tempValue < prevMin) { prevMin = tempValue; }

                        myViewModel.secondPointCollection.Add(new Point { X = lineCount, Y = tempValue });
                        lineCount++;
                    }

                    //LineChart.DataContext = viewModel;
                    secondPointCollectionCount = myViewModel.secondPointCollection.Count;
                }

                // Update Line Series name since there is second lines.
                PlotLineSeries.Title = firstFileID;
                ScndPlotLineSeries.Title = secondFileID;
            }
            else LineChart.Series.RemoveAt(1);

            // ------------------------------------------------------------------------------------------------
            // X and Y Axis Boundary settings 
            // ------------------------------------------------------------------------------------------------
            // X Axis Minimum, always ZERO
            XLinearAxis.Minimum = 0;

            // X Axis Maximum, is the maximum point collection count
            if (firstPointCollectionCount > secondPointCollectionCount)
            {
                XLinearAxis.Maximum = firstPointCollectionCount;

                //// Calculate the scale values depending on max&min values of both X and Y axis.
                if ((Convert.ToInt32((double)firstPointCollectionCount / 25.0) % 10) == 0) { XLinearAxis.Interval = Convert.ToInt32((double)firstPointCollectionCount / 25.0); }
                else { XLinearAxis.Interval = ((10 - (Convert.ToInt32((double)firstPointCollectionCount / 25.0) % 10)) + Convert.ToInt32((double)firstPointCollectionCount / 25.0)); }
            }
            else
            {
                XLinearAxis.Maximum = secondPointCollectionCount;

                //// Calculate the scale values depending on max&min values of both X and Y axis.
                if ((Convert.ToInt32((double)secondPointCollectionCount / 25.0) % 10) == 0) { XLinearAxis.Interval = Convert.ToInt32((double)secondPointCollectionCount / 25.0); }
                else { XLinearAxis.Interval = ((10 - (Convert.ToInt32((double)secondPointCollectionCount / 25.0) % 10)) + Convert.ToInt32((double)secondPointCollectionCount / 25.0)); }
            }
            XLinearAxis.Maximum += 50; // Shift 50

            // Y Axis Maximum
            YLinearAxis.Maximum = prevMax + 25;

            // Y Axis Minimum
            if ((prevMax + 25) >= (prevMin - 25)) { YLinearAxis.Minimum = prevMin - 25; } else { YLinearAxis.Minimum = prevMax + 25; }

            // Scale intervals
            //
            if ((Convert.ToInt32((double)(prevMax + 10) / 10.0) % 10) == 0) { YLinearAxis.Interval = Convert.ToInt32((double)(prevMax + 10) / 10.0); }
            else { YLinearAxis.Interval = ((10 - (Convert.ToInt32((double)(prevMax + 10) / 10.0) % 10)) + Convert.ToInt32((double)(prevMax + 10) / 10.0)); }

            // Plot line thickness setting
            //
            var style = new Style(typeof(Polyline));
            style.Setters.Add(new Setter(Polyline.StrokeThicknessProperty, 1d));

            PlotLineSeries.PolylineStyle = style;

            // Save initial X and Y min & max values
            initialXmin = (double)XLinearAxis.Minimum;
            initialXmax = (double)XLinearAxis.Maximum;
            initialXinterval = (double)XLinearAxis.Interval;
            initialYmin = (double)YLinearAxis.Minimum;
            initialYmax = (double)YLinearAxis.Maximum;
            initialYinterval = (double)YLinearAxis.Interval;
        }

        // Reset button ------------------------------------------------------------------
        //
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            XLinearAxis.Minimum = initialXmin;
            XLinearAxis.Maximum = initialXmax;
            XLinearAxis.Interval = initialXinterval;
            YLinearAxis.Minimum = initialYmin;
            YLinearAxis.Maximum = initialYmax;
            YLinearAxis.Interval = initialYinterval;
        }

        // Mark Checked Button -----------------------------------------------------------
        //
        private void MarkCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //PlotLineSeries.DataPointStyle = null;
            PlotLineSeries.Background = Brushes.Blue;
            Style styleF = new Style(typeof(LineDataPoint));
            styleF.Setters.Add(new Setter(LineDataPoint.BackgroundProperty, Brushes.Blue));
            PlotLineSeries.DataPointStyle = styleF;

            if (ScndPlotLineSeries != null)
            {
                ScndPlotLineSeries.Background = Brushes.Orange;
                Style styleS = new Style(typeof(LineDataPoint));
                styleS.Setters.Add(new Setter(LineDataPoint.BackgroundProperty, Brushes.Orange));
                ScndPlotLineSeries.DataPointStyle = styleS;
            }
        }

        private void MarkCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            //PlotLineSeries.DataPointStyle = null;
            //PlotLineSeries.Background = Brushes.Blue;
            Style styleF = new Style(typeof(LineDataPoint));
            styleF.Setters.Add(new Setter(LineDataPoint.TemplateProperty, null));
            styleF.Setters.Add(new Setter(LineDataPoint.BackgroundProperty, Brushes.Blue));
            PlotLineSeries.DataPointStyle = styleF;

            if (ScndPlotLineSeries != null)
            {
                Style styleS = new Style(typeof(LineDataPoint));
                styleS.Setters.Add(new Setter(LineDataPoint.TemplateProperty, null));
                styleS.Setters.Add(new Setter(LineDataPoint.BackgroundProperty, Brushes.Blue));
                ScndPlotLineSeries.DataPointStyle = styleS;
            }
        }

        //******************************************************************************************
        // Mouse Control
        //
        //******************************************************************************************

        // Move LEFT/RIGHT (On Axis X) and UP/DOWN (On Axis Y) -------------------------------------
        // Click left button of mouse and move left/right/up/down
        //
        private void LineChart_MouseMove(object sender, MouseEventArgs e)
        {
            // This is important, because when a graph window is created, this proc is invoked
            // and cause a location problem for first time calling, so we do not want it
            if (allwaysActive)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    double deltaDirectionX = currentPositionX - e.GetPosition(this).X;
                    double deltaDirectionY = currentPositionY - e.GetPosition(this).Y;

                    if (deltaDirectionX > 0)
                    {
                        if (XLinearAxis.Maximum > (XLinearAxis.Minimum + deltaDirectionX)) XLinearAxis.Maximum += deltaDirectionX;
                        else XLinearAxis.Maximum = XLinearAxis.Minimum + deltaDirectionX;
                        XLinearAxis.Minimum += deltaDirectionX;
                    }
                    if (deltaDirectionX < 0)
                    {
                        if (XLinearAxis.Maximum > (XLinearAxis.Minimum + deltaDirectionX)) XLinearAxis.Maximum += deltaDirectionX;
                        else XLinearAxis.Maximum = XLinearAxis.Minimum + deltaDirectionX;
                        XLinearAxis.Minimum += deltaDirectionX;
                    }

                    if (deltaDirectionY > 0)
                    {
                        if (YLinearAxis.Maximum > (YLinearAxis.Minimum - deltaDirectionY)) YLinearAxis.Maximum -= deltaDirectionY;
                        else YLinearAxis.Maximum = YLinearAxis.Minimum - deltaDirectionY;
                        YLinearAxis.Minimum -= deltaDirectionY;
                    }
                    if (deltaDirectionY < 0)
                    {
                        if (YLinearAxis.Maximum > (YLinearAxis.Minimum - deltaDirectionY)) YLinearAxis.Maximum -= deltaDirectionY;
                        else YLinearAxis.Maximum = YLinearAxis.Minimum - deltaDirectionY;
                        YLinearAxis.Minimum -= deltaDirectionY;
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
            allwaysActive = true;
        }

        // Zoom IN/OUT for Axis X ---------------------------------------------------------------------
        // Move mousewheel up and down
        //
        void LineChart_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (lineChartMouseWheelUsed)
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
            lineChartMouseWheelUsed = true;
        }

        // Scale X, INCREASE or DECREASE -------------------------------------------------------
        //
        void XLinearAxis_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                if (XLinearAxis.Interval > 10) { XLinearAxis.Interval -= 10; }
                else { if (XLinearAxis.Interval > 0) { XLinearAxis.Interval -= 1; } }
            }

            if (e.Delta < 0)
            {
                if (XLinearAxis.Interval >= 10) { XLinearAxis.Interval += 10; }
                else { if (XLinearAxis.Interval >= 0) { XLinearAxis.Interval += 1; } }
            }

            lineChartMouseWheelUsed = false;
        }

        // Scale Y, INCREASE or DECREASE -------------------------------------------------------
        //
        void YLinearAxis_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            lineChartMouseWheelUsed = false;

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
        }
    }
}
