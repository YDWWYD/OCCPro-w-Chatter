using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineCuttingControlProcess
{
    internal interface XYZInterface
    {
        void SetCoordinates(CoordinateSystem coordSystem);
        void SetAxesRange(); // Used to update view
        void ChangePointPercentageVisibility(int newValue);
        void AddSimulatedDataPointTo3DPlot(double x, double y, double z, int operationIndex, int lineNo); // Draws simulated data tool path
        void AddMeasuredDataPointTo3DPlot(int timeIntervalCount, double x, double y, double z, bool tracingError); // Draws measured data tool path
        bool IsTracingToleranceExceeded(double x, double y, double z, double tracingTolerance, ref int toolPositionMatchFoundIndex, ref int ncLineNo);
        void InvalidateDisplay();
        void ClearSimulatedData();
        void ClearMeasuredData();
        void XYZGizmoVisibility(bool visible);
        void ChartOperation(ChartOperation3D move); // Used to update view
        void UpdateToolDistance(double distance);
        void ActivateRealTimeDisplayUpdate();
        void SetToolDimensions(double toolDiameter, double toolLength); // Loading tool and viewing
        void SetOperationCount(int count);
        void SetTracing(bool activateTracing);
        void LoadWorkpieceGeometry(string workpieceGeometryFile); // Loading workpiece
        void SetWorkpieceDimensions(double xMin, double yMin, double zMin, double xMax, double yMax, double zMax);
        void EnablePointSelection(bool enabled);
        int GetSelectedDataPointTimeInterval();
        void GetSelectedPointPosition(out double selectedX, out double selectedY, out double selectedZ);
        void Close();
    }
}
