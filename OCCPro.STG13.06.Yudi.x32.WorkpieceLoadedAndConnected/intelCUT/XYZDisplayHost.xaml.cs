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
    /// Interaction logic for XYZDisplayHost.xaml
    /// </summary>
    public partial class XYZDisplayHost : UserControl
    {
        private Color simulatedLineColor = Color.FromRgb(100, 100, 0);
        private Color[] simulatedLineColorArray = new Color[10];

        // NOT used yet, check later
        private Color measuredLineColor = Color.FromRgb(127, 0, 255);
        private Color measuredLineErrorColor = Color.FromRgb(255, 0, 0);
        private Color selectedPointColor = Color.FromRgb(127, 127, 127);

        internal XYZInterface controlHost = null;

        public XYZDisplayHost()
        {
            InitializeComponent();

            controlHost = new OpenGLUserControlHost(); ;

            Loaded += OnLoaded;
        }
        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var frameWork = (FrameworkElement)controlHost;
            //XYZGraphGrid.Children.Add(frameWork);
            XYZLayoutGrid.Children.Add(frameWork);
        }

        internal void SetBindings(Slider PointSizeSlider, Slider PointPercentageSlider, Slider ThicknessSlider)
        {
            Binding simulatedDataPointSizeBinding = new Binding();
            simulatedDataPointSizeBinding.Source = PointSizeSlider;
            simulatedDataPointSizeBinding.Path = new PropertyPath("Value");
            //SimulatedDataPointMarker.SetBinding(BasePointMarker3D.SizeProperty, simulatedDataPointSizeBinding);

            Binding simulatedDataPointPercentageDisplayBinding = new Binding();
            simulatedDataPointPercentageDisplayBinding.Source = PointPercentageSlider;
            simulatedDataPointPercentageDisplayBinding.Path = new PropertyPath("Value");
            //SimulatedDataPointMarker.SetBinding(OpacityProperty, simulatedDataPointPercentageDisplayBinding);

            Binding measuredDataSeriesLineThicknessBinding = new Binding();
            measuredDataSeriesLineThicknessBinding.Source = ThicknessSlider;
            measuredDataSeriesLineThicknessBinding.Path = new PropertyPath("Value");
            //MeasuredDataSeries3D.SetBinding(PointLineRenderableSeries3D.StrokeThicknessProperty, measuredDataSeriesLineThicknessBinding);
        }

        internal void SetCoordinates(CoordinateSystem coordSystem)
        {
            controlHost.SetCoordinates(coordSystem);
        }

        internal void ZoomToExtents()
        {
            //controlHost.ZoomToExtents();
            controlHost.ChartOperation(ChartOperation3D.ZoomToExtents);
        }

        internal void ClearMeasuredData()
        {
            controlHost.ClearMeasuredData();
        }

        internal void ClearSimulatedData()
        {
            controlHost.ClearSimulatedData();
        }

        public void SetAxesRange()
        {
            controlHost.SetAxesRange();
        }

        public void AddMeasuredDataPointTo3DPlot(double x, double y, double z)
        {
            //var pointMetaData = new PointMetadata3D(measuredLineColor);
        }

        public void AddMeasuredDataPointTo3DPlot(int timeIntervalCount, double x, double y, double z, bool tracingError)
        {
            controlHost.AddMeasuredDataPointTo3DPlot(timeIntervalCount, x, y, z, tracingError);
            /*Color lineColor;
            if (tracingError)
            {
                lineColor = measuredLineErrorColor;
            }
            else
            {
                lineColor = measuredLineColor;
            }
            var pointMetaData = new PointMetadata3D(lineColor);
            pointMetaData.Tag = timeIntervalCount;
            AddMeasuredDataPointTo3DPlot(x, y, z, pointMetaData);*/
        }

        public void AddSimulatedDataPointTo3DPlot(double x, double y, double z, int operationIndex, int lineNo)
        {
            controlHost.AddSimulatedDataPointTo3DPlot(x, y, z, operationIndex, lineNo);
        }

        internal void DeselectPoint()
        {
            //selectedPointMetaData3D = null;
            //PointSelectionTimeIntervalTextBox.Text = null;
        }

        internal bool IsTracingToleranceExceeded(double x, double y, double z, double tracingTolerance, ref int toolPositionMatchFoundIndex, ref int ncLineNo)
        {
            return controlHost.IsTracingToleranceExceeded(x, y, z, tracingTolerance, ref toolPositionMatchFoundIndex, ref ncLineNo);
        }

        internal void UpdateToolDistance(double distance)
        {
            controlHost.UpdateToolDistance(distance);
        }

        internal void Close()
        {
            controlHost.Close();
        }

        private bool IsMeasuredDataInDesiredRange(double measuredValue, double simulatedValue1, double simulatedValue2, double tolerance)
        {
            if ((measuredValue >= (simulatedValue1 - tolerance) && measuredValue <= (simulatedValue2 + tolerance)) ||
                    (measuredValue >= (simulatedValue2 - tolerance) && measuredValue <= (simulatedValue1 + tolerance)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void UpdateCameraPositionAndTarget(float positionX, float positionY, float positionZ, float targetX, float targetY, float targetZ)
        {
        }


        //Returns the greatest common denominator of two integer values
        private static int GCD(int a, int b)
        {
            int remainder;
            while (b > 0)
            {
                remainder = a % b;
                a = b;
                b = remainder;
            }
            return a;
        }

        //Sets the colors in the simulatedLineColorArray used for selecting the color of the simulated data points
        //Some elements in the array are set to a visible color, others to transparent, based on a percentage
        //This determines which simulated data points are visible
        //Takes argument (level) as int from 0 to 10 (percent visible: 0% to 100%)
        private void SetSimulatedPointVisibility(int level)
        {
            if (level >= 0 && level <= 10)
            {
                int size = simulatedLineColorArray.Length;
                int quotient = size / level;
                int gcd = GCD(size, level);
                int groupSize = size / gcd;

                //Since gcd * groupSize = size, the loops traverse the array
                //The gcd determines the number of times the pattern repeats
                for (int j = 0; j < gcd; j++)
                {
                    for (int i = 1; i <= groupSize; i++)
                    {
                        int index = (i + j * groupSize) - 1;
                        if (i % quotient == 0)
                        {
                            simulatedLineColorArray[index] = simulatedLineColor;
                        }
                        else
                        {
                            simulatedLineColorArray[index] = Color.FromArgb(0, 0, 0, 0);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Error attempting to change the percentage of simulated data points visible: Incorrect value specified.");
            }
        }

        internal void ChangePointPercentageVisibility(int newValue)
        {
            controlHost.ChangePointPercentageVisibility(newValue);
            /*SetSimulatedPointVisibility(newValue);
            UpdateSimulatedDataPoints();*/
        }

        private void UpdateSimulatedDataPoints()
        {
            /*if (xyzSimulatedDataSeries != null)
            {
                int arraySize = simulatedLineColorArray.Length;
                for (int i = 0; i < xyzSimulatedDataSeries.Count; i++)
                {
                    xyzSimulatedDataSeries.WValues[i] = new PointMetadata3D(simulatedLineColorArray[i % arraySize]);
                }
            }*/
        }

        internal void InvalidateDisplay()
        {
            controlHost.SetAxesRange();
            controlHost.InvalidateDisplay();
            /*if (!useMW)
            {
            }*/
        }

        //internal void SetToolBarTrayVertical()
        //{
        //    Grid.SetColumnSpan(xyzToolBarTray, 1);
        //    Grid.SetRowSpan(xyzToolBarTray, 2);
        //    xyzToolBarTray.Orientation = Orientation.Vertical;

        //    Grid.SetColumn(XYZGraphGrid, 1);
        //    Grid.SetRow(XYZGraphGrid, 0);
        //    Grid.SetColumnSpan(XYZGraphGrid, 1);
        //    Grid.SetRowSpan(XYZGraphGrid, 2);
        //}

        //internal void SetToolBarTrayHorizontal()
        //{
        //    Grid.SetColumnSpan(xyzToolBarTray, 2);
        //    Grid.SetRowSpan(xyzToolBarTray, 1);
        //    xyzToolBarTray.Orientation = Orientation.Horizontal;

        //    Grid.SetColumn(XYZGraphGrid, 0);
        //    Grid.SetRow(XYZGraphGrid, 1);
        //    Grid.SetColumnSpan(XYZGraphGrid, 2);
        //    Grid.SetRowSpan(XYZGraphGrid, 1);
        //}

        internal void SelectPointAtTime(int timeIntervalCount)
        {

        }

        internal void XYZGizmoVisibility(bool visible)
        {
            controlHost.XYZGizmoVisibility(visible);
        }

        internal void ActivateRealTimeDisplayUpdate()
        {
            controlHost.ActivateRealTimeDisplayUpdate();
        }

        internal void SetToolDimensions(double toolDiameter, double toolLength)
        {
            controlHost.SetToolDimensions(toolDiameter, toolLength);
        }

        internal void SetOperationCount(int count)
        {
            controlHost.SetOperationCount(count);
        }

        internal void SetTracing(bool activateTracing)
        {
            controlHost.SetTracing(activateTracing);
        }

        //internal void SetXYZGraphGridVisibility(bool isVisible)
        //{
        //    if (isVisible)
        //    {
        //        XYZGraphGrid.Visibility = System.Windows.Visibility.Visible;
        //    }
        //    else
        //    {
        //        XYZGraphGrid.Visibility = System.Windows.Visibility.Hidden;
        //    }
        //}

        internal bool LoadWorkpieceGeometry(string workpieceGeometryFile)
        {
            controlHost.LoadWorkpieceGeometry(workpieceGeometryFile);
            return true; // Means loading is completed
        }

        internal void SetWorkpieceDimensions(double xMin, double yMin, double zMin, double xMax, double yMax, double zMax)
        {
            controlHost.SetWorkpieceDimensions(xMin, yMin, zMin, xMax, yMax, zMax);
        }

        internal int GetSelectedDataPointTimeInterval()
        {
            return controlHost.GetSelectedDataPointTimeInterval();
        }

        internal void GetSelectedPointPosition(out double selectedX, out double selectedY, out double selectedZ)
        {
            controlHost.GetSelectedPointPosition(out selectedX, out selectedY, out selectedZ);
        }


        /*************************************************************************/
        /*                       Event Handlers                                  */
        /*************************************************************************/


        private void OnIncreaseX(object sender, RoutedEventArgs e)
        {

        }

        private void OnDecreaseX(object sender, RoutedEventArgs e)
        {

        }

        private void OnIncreaseY(object sender, RoutedEventArgs e)
        {

        }

        private void OnDecreaseY(object sender, RoutedEventArgs e)
        {

        }
        private void OnIncreaseZ(object sender, RoutedEventArgs e)
        {

        }

        private void OnDecreaseZ(object sender, RoutedEventArgs e)
        {

        }

        private void OnExpandGraph(object sender, RoutedEventArgs e)
        {
            if (this.Parent != null)
            {
                Grid.SetColumnSpan(this, 2);
                Grid.SetRowSpan(this, 2);
                Canvas.SetZIndex(this, 1);
                var LayoutGrid = (Grid)this.Parent;
                ((MainWindow)LayoutGrid.Parent).WindowState = WindowState.Maximized;
            }
        }

        private void OnShrinkGraph(object sender, RoutedEventArgs e)
        {
            if (this.Parent != null)
            {
                Grid.SetColumnSpan(this, 1);
                Grid.SetRowSpan(this, 1);
                Canvas.SetZIndex(this, 0);
                var LayoutGrid = (Grid)this.Parent;
            }
        }

        private void ZoomToExtentsButton_Click(object sender, RoutedEventArgs e)
        {
            ZoomToExtents();
        }

        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            controlHost.ChartOperation(ChartOperation3D.ZoomIn);
        }

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            controlHost.ChartOperation(ChartOperation3D.ZoomOut);
        }

        private void PanLeftButton_Click(object sender, RoutedEventArgs e)
        {
            controlHost.ChartOperation(ChartOperation3D.PanLeft);
        }

        private void PanRightButton_Click(object sender, RoutedEventArgs e)
        {
            controlHost.ChartOperation(ChartOperation3D.PanRight);
        }

        private void PanUpButton_Click(object sender, RoutedEventArgs e)
        {
            controlHost.ChartOperation(ChartOperation3D.PanUp);
        }

        private void PanDownButton_Click(object sender, RoutedEventArgs e)
        {
            controlHost.ChartOperation(ChartOperation3D.PanDown);
        }

        private void SelectPointButton_Checked(object sender, RoutedEventArgs e)
        {
            controlHost.EnablePointSelection(true);
        }

        private void SelectPointButton_Unchecked(object sender, RoutedEventArgs e)
        {
            controlHost.EnablePointSelection(false);
        }
    }
}
