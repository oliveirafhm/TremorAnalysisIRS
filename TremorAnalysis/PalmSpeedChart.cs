using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TremorAnalysis
{
    class PalmSpeedChart
    {
        private MainWindow myWindow;
        private PlotModel plotModelSpeedNorm;
        private LineSeries lineSeriesSpeedNorm;
        private int plotInterval = 50; // Milliseconds       
        private int speedNormChartWindow; // In points (seconds * fps) it means the visible part of chart

        public PalmSpeedChart(MainWindow window, int secondsToSee = 10)
        {
            this.myWindow = window;
            speedNormChartWindow = secondsToSee * this.myWindow.fps;
            initSpeedNormChart();
            updateSpeedNormChart();
        }

        private void initSpeedNormChart()
        {
            plotModelSpeedNorm = new PlotModel
            {
                Title = "Palm Speed (norm)",
                TitleFontSize = 12,
                //PlotAreaBackground = OxyColor.FromRgb(229, 229, 229),
                //TitlePadding = 1,
                Padding = new OxyThickness(0),                  
                //Background = OxyColor.FromRgb(229, 229, 229),
                TitleHorizontalAlignment = TitleHorizontalAlignment.CenteredWithinPlotArea
                //LegendTitle = "Palm speed (norm)",
                //LegendOrientation = LegendOrientation.Horizontal,
                //LegendPlacement = LegendPlacement.Inside,
                //LegendPosition = LegendPosition.TopRight                    
            };
            OxyPlot.Axes.LinearAxis xAxis = new OxyPlot.Axes.LinearAxis();
            xAxis.Position = OxyPlot.Axes.AxisPosition.Bottom;
            xAxis.Title = "seconds";
            xAxis.MinorStep = 0.5;
            xAxis.MajorStep = 1;
            xAxis.MinimumPadding = 0;
            xAxis.MaximumPadding = 0;
            xAxis.IsPanEnabled = false;
            xAxis.IsZoomEnabled = false;
            
            OxyPlot.Axes.LinearAxis yAxis = new OxyPlot.Axes.LinearAxis();
            yAxis.Position = OxyPlot.Axes.AxisPosition.Left;
            yAxis.Title = "||speed||";
            yAxis.Maximum = 1.0;
            yAxis.MinimumPadding = 0;
            yAxis.MaximumPadding = 0;
            yAxis.IsPanEnabled = false;
            yAxis.IsZoomEnabled = false;

            plotModelSpeedNorm.Axes.Add(xAxis);
            plotModelSpeedNorm.Axes.Add(yAxis);

            lineSeriesSpeedNorm = new LineSeries
            {
                StrokeThickness = 2,
                CanTrackerInterpolatePoints = false,
                Title = "||Speed (m/s)||",                                
                Smooth = false
            };
            plotModelSpeedNorm.Series.Add(lineSeriesSpeedNorm);

            // Changed to BeginInvoke from Invoke
            myWindow.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate()
            {
                myWindow.palmSpeedChart.Model = plotModelSpeedNorm;
            }));
        }

        private void updateSpeedNormChart()
        {
            while (!myWindow.stop)
            {
                if (myWindow.plotSNNow)
                {
                    lineSeriesSpeedNorm.Points.Clear(); // Clears the current points from chart
                    // Mutex
                    lock (myWindow.lineSerieSpeedNormBuffer)
                    {
                        // Adds new points that will be plotted after
                        lineSeriesSpeedNorm.Points.AddRange(myWindow.lineSerieSpeedNormBuffer);
                        if (myWindow.lineSerieSpeedNormBuffer.Count() >= speedNormChartWindow)
                            // Deletes some initial points to keep buffer size correct as specified by secondsToSee
                            myWindow.lineSerieSpeedNormBuffer.RemoveRange(0, 3);

                        myWindow.plotSNNow = false;
                    }
                    myWindow.updateSpeedNormChart();// Updates the chart in the UI
                }
                Thread.Sleep(plotInterval);
            }
            // Clear buffer and palm speed chart data
            myWindow.lineSerieSpeedNormBuffer.Clear();
            lineSeriesSpeedNorm.Points.Clear();
        }
    }
}
