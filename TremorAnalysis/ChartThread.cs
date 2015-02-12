using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TremorAnalysis
{
    class ChartThread
    {
        private MainWindow myWindow;
        private int plotInterval = 50; // Milliseconds
        private int fps = 50; // Usually value
        private int speedNormChartWindow = 10*50; // seconds * fps

        public ChartThread(MainWindow window)
        {
            this.myWindow = window;
            initSpeedNormChart();
            updateSpeedNormChart();
        }

        private void initSpeedNormChart()
        {
            myWindow.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate()
            {
                myWindow.plotModelSpeedNorm = new PlotModel
                {
                    Title = "Palm Speed (norm)",
                    TitleFontSize = 12,
                    //PlotAreaBackground = OxyColor.FromRgb(229, 229, 229),
                    TitlePadding = 1,
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

                OxyPlot.Axes.LinearAxis yAxis = new OxyPlot.Axes.LinearAxis();
                yAxis.Position = OxyPlot.Axes.AxisPosition.Left;
                yAxis.Title = "||speed||";
                yAxis.Maximum = 1.5;

                myWindow.plotModelSpeedNorm.Axes.Add(xAxis);
                myWindow.plotModelSpeedNorm.Axes.Add(yAxis);

                myWindow.lineSerieSpeedNorm = new LineSeries
                {
                    StrokeThickness = 2,
                    CanTrackerInterpolatePoints = false,
                    Title = "||Speed (m/s)||",
                    Smooth = false
                };
                myWindow.plotModelSpeedNorm.Series.Add(myWindow.lineSerieSpeedNorm);

                myWindow.palmSpeedChart.Model = myWindow.plotModelSpeedNorm;
            }));
        }

        private void updateSpeedNormChart()
        {
            while (!myWindow.stop)
            {
                if (myWindow.plotSNNow)
                {
                    myWindow.lineSerieSpeedNorm.Points.Clear();
                    lock (myWindow.lineSerieSpeedNormBuffer)
                    {
                        myWindow.lineSerieSpeedNorm.Points.AddRange(myWindow.lineSerieSpeedNormBuffer);
                        if (myWindow.lineSerieSpeedNormBuffer.Count() >= speedNormChartWindow)
                        {
                            //myWindow.lineSerieSpeedNormBuffer.RemoveAt(0);
                            myWindow.lineSerieSpeedNormBuffer.RemoveRange(0, 3);
                        }
                        myWindow.plotSNNow = false;
                    }
                    myWindow.updateSpeedNormChart();
                }
                Thread.Sleep(plotInterval);
            }
            myWindow.lineSerieSpeedNormBuffer.Clear();// Clear buffer
        }
    }
}
