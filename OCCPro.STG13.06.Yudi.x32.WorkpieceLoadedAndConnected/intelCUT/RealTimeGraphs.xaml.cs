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
    /// Interaction logic for RealTimeGraphs.xaml
    /// </summary>
    public partial class RealTimeGraphs : UserControl
    {
        //public ObservableCollection<KeyValuePair<double, double>> SimDataKeyValuePair = new ObservableCollection<KeyValuePair<double, double>>();
        public ObservableCollection<KeyValuePair<double, double>> RtmDataKeyValuePair = new ObservableCollection<KeyValuePair<double, double>>();
        public List<ObservableCollection<KeyValuePair<double, double>>> MonitoringPlot = new List<ObservableCollection<KeyValuePair<double, double>>>();

        public RealTimeGraphs()
        {
            InitializeComponent();
            //InitializeComponent();

            //MonitoringPlot.Add(SimDataKeyValuePair);
            MonitoringPlot.Add(RtmDataKeyValuePair);

            LineChart.DataContext = MonitoringPlot;

            // They are the initial values for zooming
            XLinearAxis.Maximum = 100;
            XLinearAxis.Minimum = 0;
            YLinearAxis.Maximum = 25;
            YLinearAxis.Minimum = -20;

            // Plot line thickness setting
            var style = new Style(typeof(Polyline));
            style.Setters.Add(new Setter(Polyline.StrokeThicknessProperty, 1d));

            RtmPlotLineSeries.PolylineStyle = style;
        }
     }
}
