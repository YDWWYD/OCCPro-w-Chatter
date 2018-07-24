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
    /// Interaction logic for ToolPathFollower.xaml
    /// </summary>
    public partial class ToolPathFollower : UserControl
    {
        public ObservableCollection<KeyValuePair<double, double>> PathKeyValuePair = new ObservableCollection<KeyValuePair<double, double>>();
        public ObservableCollection<KeyValuePair<double, double>> ToolKeyValuePair = new ObservableCollection<KeyValuePair<double, double>>();
        public List<ObservableCollection<KeyValuePair<double, double>>> MonitoringPlot = new List<ObservableCollection<KeyValuePair<double, double>>>();

        public ToolPathFollower()
        {
            InitializeComponent();

            MonitoringPlot.Add(PathKeyValuePair);
            MonitoringPlot.Add(ToolKeyValuePair);

            LineChart.DataContext = MonitoringPlot;

            // They are the initial values for zooming
            XLinearAxis.Maximum = 500;
            XLinearAxis.Minimum = 0;
            YLinearAxis.Maximum = 500;
            YLinearAxis.Minimum = -20;

            // Plot line thickness setting for Path
            var styleSim = new Style(typeof(Polyline));
            styleSim.Setters.Add(new Setter(Polyline.StrokeThicknessProperty, 2d));
            SimPlotLineSeries.PolylineStyle = styleSim;

            // Plot line thickness setting for Tool
            var styleRtm = new Style(typeof(Polyline));
            styleRtm.Setters.Add(new Setter(Polyline.StrokeThicknessProperty, 4d));
            RtmPlotLineSeries.PolylineStyle = styleRtm;
        }
    }
}
