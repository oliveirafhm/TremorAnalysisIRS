using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using OxyPlot;
using OxyPlot.Series;

namespace TremorAnalysis
{
    class RMSChart
    {
        private MainWindow myWindow;
        private PlotModel plotModelRMS;
        private LineSeries lineSeriesRMS;
        private PlotModel plotModelRMSCross;
        private LineSeries lineSeriesRMSCross;
        //private int plotInterval; // Milliseconds
        private int rmsChartWindow; // It means how many windows will be available to see in the chart UI

        public RMSChart(MainWindow window, int windowsToSee = 10)
        {
            this.myWindow = window;
            rmsChartWindow = windowsToSee;
            initRMSChart();
            //updateRMSChart();
        }

        private void initRMSChart()
        {
            plotModelRMS = new PlotModel
            {
                Title = "RMS",
                TitleFontSize = 12,
                Padding = new OxyThickness(0),
                TitleHorizontalAlignment = TitleHorizontalAlignment.CenteredWithinPlotArea
            };
            plotModelRMSCross = new PlotModel
            {
                Title = "RMS Crossing",
                TitleFontSize = 12,
                Padding = new OxyThickness(0),
                TitleHorizontalAlignment = TitleHorizontalAlignment.CenteredWithinPlotArea
            };

            OxyPlot.Axes.LinearAxis xRMSAxis = new OxyPlot.Axes.LinearAxis();
            xRMSAxis.Position = OxyPlot.Axes.AxisPosition.Bottom;
            xRMSAxis.Title = "windows";
            xRMSAxis.MinorStep = 1;
            xRMSAxis.MajorStep = 1;
            xRMSAxis.MinimumPadding = 0;
            xRMSAxis.MaximumPadding = 0;
            xRMSAxis.IsPanEnabled = false;
            xRMSAxis.IsZoomEnabled = false;

            OxyPlot.Axes.LinearAxis yRMSAxis = new OxyPlot.Axes.LinearAxis();
            yRMSAxis.Position = OxyPlot.Axes.AxisPosition.Left;
            yRMSAxis.Maximum = 1.0;
            yRMSAxis.MajorStep = 0.5;
            yRMSAxis.MinorStep = 0.25;
            yRMSAxis.MinimumPadding = 0;
            yRMSAxis.MaximumPadding = 0;
            yRMSAxis.IsPanEnabled = false;
            yRMSAxis.IsZoomEnabled = false;

            plotModelRMS.Axes.Add(xRMSAxis);
            plotModelRMS.Axes.Add(yRMSAxis);

            OxyPlot.Axes.LinearAxis xRMSCrossAxis = new OxyPlot.Axes.LinearAxis();
            xRMSCrossAxis.Position = OxyPlot.Axes.AxisPosition.Bottom;
            xRMSCrossAxis.Title = "windows";
            xRMSCrossAxis.MinorStep = 1;
            xRMSCrossAxis.MajorStep = 1;
            xRMSCrossAxis.MinimumPadding = 0;
            xRMSCrossAxis.MaximumPadding = 0;
            xRMSCrossAxis.IsPanEnabled = false;
            xRMSCrossAxis.IsZoomEnabled = false;

            OxyPlot.Axes.LinearAxis yRMSCrossAxis = new OxyPlot.Axes.LinearAxis();
            yRMSCrossAxis.Position = OxyPlot.Axes.AxisPosition.Left;
            yRMSCrossAxis.Maximum = 30;
            yRMSCrossAxis.MajorStep = 10;
            yRMSCrossAxis.MinorStep = 5;
            yRMSCrossAxis.MinimumPadding = 0;
            yRMSCrossAxis.MaximumPadding = 0;
            yRMSCrossAxis.IsPanEnabled = false;
            yRMSCrossAxis.IsZoomEnabled = false;

            plotModelRMSCross.Axes.Add(xRMSCrossAxis);
            plotModelRMSCross.Axes.Add(yRMSCrossAxis);

            lineSeriesRMS = new LineSeries
            {
                StrokeThickness = 2,
                CanTrackerInterpolatePoints = false,
                Title = "RMS",
                Color = OxyColor.FromRgb(0,0,153),
                Smooth = false
            };
            plotModelRMS.Series.Add(lineSeriesRMS);

            lineSeriesRMSCross = new LineSeries
            {
                StrokeThickness = 2,
                CanTrackerInterpolatePoints = false,
                Title = "RMS Crossing",
                Color = OxyColor.FromRgb(153, 0, 0),
                Smooth = false
            };
            plotModelRMSCross.Series.Add(lineSeriesRMSCross);

            myWindow.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate()
            {
                myWindow.rmsChart.Model = plotModelRMS;
                myWindow.rmsCrossChart.Model = plotModelRMSCross;
            }));
        }

        public void insertRMSData(int windowNumber, double rms)
        {
            if (lineSeriesRMS.Points.Count >= rmsChartWindow)
                lineSeriesRMS.Points.RemoveAt(0);
            lineSeriesRMS.Points.Add(new DataPoint(windowNumber, rms));
        }

        public void insertRMSCrossData(int windowNumber, int rmsCross)
        {
            if (lineSeriesRMSCross.Points.Count >= rmsChartWindow)            
                lineSeriesRMSCross.Points.RemoveAt(0);
            lineSeriesRMSCross.Points.Add(new DataPoint(windowNumber, rmsCross));
        }

        public void updateRMSChart()
        {
            myWindow.updateRMSChart();
        }

        public void clearSeries()
        {
            lineSeriesRMS.Points.Clear();
            lineSeriesRMSCross.Points.Clear();
        }

        //private void updateRMSChart()
        //{
        //    while (!myWindow.stop)
        //    {

        //    }

        //}
    }
}
