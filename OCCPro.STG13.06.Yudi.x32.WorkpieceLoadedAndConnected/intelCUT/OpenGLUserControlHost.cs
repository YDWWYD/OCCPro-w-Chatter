using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WPFOpenGLLib;
using System.Windows.Forms.Integration;
using System.Windows;
using System.Reflection;

namespace OnlineCuttingControlProcess
{
    class OpenGLUserControlHost : WindowsFormsHost, XYZInterface
    {
        private OpenGLUserControl openGLControl;
        internal PointSelectionHandler pointSelector;

        // Constructor
        internal OpenGLUserControlHost()
        {
            openGLControl = new OpenGLUserControl();
            this.Child = openGLControl;
            openGLControl.pointSelectionEventHandler += OnXYZControlPointSelected;
        }

        void OnXYZControlPointSelected(object sender, RoutedEventArgs e) { pointSelector(sender, e); }

        // Procs.
        void XYZInterface.SetAxesRange() { /* not implemented */ }
        void XYZInterface.ChangePointPercentageVisibility(int newValue) { /* not implemented */ }
        void XYZInterface.InvalidateDisplay() { openGLControl.InvalidateDisplay(); }
        void XYZInterface.ClearSimulatedData() { openGLControl.ClearSimulatedData(); }
        void XYZInterface.ClearMeasuredData() { openGLControl.ClearMeasuredData(); }
        void XYZInterface.SetCoordinates(CoordinateSystem coordSystem) { /* not implemented */ }
        void XYZInterface.XYZGizmoVisibility(bool visible) { /* not implemented */ }
        void XYZInterface.UpdateToolDistance(double distance) { openGLControl.UpdateToolDistance(distance); }
        void XYZInterface.ActivateRealTimeDisplayUpdate() { openGLControl.ActivateRealTimeDisplayUpdate(); }
        void XYZInterface.SetToolDimensions(double toolDiameter, double toolLength) { openGLControl.SetToolDimensions(toolDiameter, toolLength); }
        void XYZInterface.SetOperationCount(int count) { openGLControl.SetOperationCount(count); }
        void XYZInterface.SetTracing(bool activateTracing) { openGLControl.SetTracing(activateTracing); }
        void XYZInterface.LoadWorkpieceGeometry(string workpieceGeometryFile) { openGLControl.LoadWorkpieceGeometry(workpieceGeometryFile); }
        void XYZInterface.EnablePointSelection(bool enabled) { openGLControl.EnablePointSelection(enabled); }
        int XYZInterface.GetSelectedDataPointTimeInterval() { return openGLControl.GetSelectedDataPointTimeInterval(); }
        void XYZInterface.Close() { openGLControl.Close(); }

        bool XYZInterface.IsTracingToleranceExceeded(double x, double y, double z, double tracingTolerance, ref int toolPositionMatchFoundIndex, ref int ncLineNo)
        {
            return openGLControl.IsTracingToleranceExceeded(x, y, z, tracingTolerance, ref toolPositionMatchFoundIndex, ref ncLineNo);
        }

        void XYZInterface.AddSimulatedDataPointTo3DPlot(double x, double y, double z, int operationIndex, int lineNo)
        {
            openGLControl.AddSimulatedDataPoint(x, y, z, operationIndex, lineNo);
        }

        void XYZInterface.AddMeasuredDataPointTo3DPlot(int timeIntervalCount, double x, double y, double z, bool tracingError)
        {
            openGLControl.AddMeasuredDataPoint(timeIntervalCount, x, y, z, tracingError);
        }

        void XYZInterface.ChartOperation(ChartOperation3D move)
        {
            Type chartType = typeof(OpenGLUserControl);
            chartType.InvokeMember(
                System.Enum.GetName(typeof(ChartOperation3D), move),
                BindingFlags.InvokeMethod | BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, openGLControl, null);
        }

        void XYZInterface.SetWorkpieceDimensions(double xMin, double yMin, double zMin, double xMax, double yMax, double zMax)
        {
            openGLControl.SetWorkpieceDimensions(xMin, yMin, zMin, xMax, yMax, zMax);
        }

        void XYZInterface.GetSelectedPointPosition(out double selectedX, out double selectedY, out double selectedZ)
        {
            double x = 0, y = 0, z = 0;
            openGLControl.GetSelectedPointPosition(ref x, ref y, ref z);
            selectedX = x;
            selectedY = y;
            selectedZ = z;
        }
    }
}
