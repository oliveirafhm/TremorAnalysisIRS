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
using System.Drawing;
using System.Threading;
using OxyPlot;
using OxyPlot.Series;

namespace TremorAnalysis
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool stop = false;

        public List<DataPoint> lineSerieSpeedNormBuffer = new List<DataPoint>();
        public bool plotSNNow = false;

        public int fps = 50; // Usually value
        public List<DataPoint> speedNormBuffer = new List<DataPoint>();
        public int windowLengthMS = 640;// 1280 = 64 points || 640 = 32 points
        
        public MainWindow()
        {
            InitializeComponent();
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            stopButton.IsEnabled = false;
        }

        public void UpdateStatus(string status)
        {
            // Changed to BeginInvoke from Invoke
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
            {
                statusLabel.Content = status;
            }));
        }

        public void UpdateFPSStatus(string status)
        {
            // Changed to BeginInvoke from Invoke
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate()
            {
                fpsLabel.Content = status;
            }));
        }

        //public void UpdateFrequencyLabel(string frequency)
        //{
        //    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate()
        //    {
        //        frequencyLabel.Content = frequency;
        //    }));
        //}

        public void UpdateRMSLabel(string rms)
        {
            // Changed to BeginInvoke from Invoke
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate()
            {
                rmsLabel.Content = rms;
            }));
        }

        public void UpdateRMSCrossLabel(string rmsCross)
        {
            // Changed to BeginInvoke from Invoke
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate()
            {
                rmsCrossLabel.Content = rmsCross;
            }));
        }
        // Take off the stop button, and put these two functions in just one button Start/Stop
        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            startButton.IsEnabled = false;
            stopButton.IsEnabled = true;

            stop = false;

            // Start main pipeline thread
            Thread handTrackingThread = new Thread(startHandTracking);
            handTrackingThread.Priority = ThreadPriority.Highest;
            handTrackingThread.Start();

            // Start palm speed chart handle thread
            Thread palmSpeedChartThread = new Thread(startPalmSpeedChart);
            palmSpeedChartThread.Priority = ThreadPriority.Highest;
            palmSpeedChartThread.Start();

            // Start signal analysis thread
            Thread signalAnalysisThread = new Thread(startSignalAnalysis);
            signalAnalysisThread.Priority = ThreadPriority.Highest;
            signalAnalysisThread.Start();

            // Start RMS chart handle thread (RMS comes from signal analysis thread)
            //Thread rmsChartThread = new Thread(startRMSChart);
            //rmsChartThread.Priority = ThreadPriority.Highest;
            //rmsChartThread.Start();
        }

        private void startSignalAnalysis()
        {
            //UpdateFrequencyLabel(" - Hz");
            // Window length (ms) that enable to get power of 2 in points
            SignalAnalysis sa = new SignalAnalysis(this, windowLengthMS);
        }

        //private void startRMSChart()
        //{

        //}

        private void startPalmSpeedChart()
        {
            PalmSpeedChart ct = new PalmSpeedChart(this);
        }

        private void startHandTracking()
        {
            MainPipeline mp = new MainPipeline(this);
            clearUI();
            mp.Start();
            // Changed to BeginInvoke from Invoke
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
            {
                startButton.IsEnabled = true;
                stopButton.IsEnabled = false;
            }));
        }

        private void clearUI()
        {
            // Changed to BeginInvoke from Invoke
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate()
            {
                streamImg.Source = null;

                updateSpeedNormChart();
                updateRMSChart();

                UpdateRMSLabel("-");
                UpdateRMSCrossLabel("-");
            }));
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            stop = true;
        }

        public void DisplayBitmap(Bitmap bitmap)
        {
            this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate()
            {
                if (bitmap != null)
                {
                    // Resizes the original bitmap to fill better the image component
                    // The image component should keep the aspect ratio
                    int unitsToResize = Convert.ToInt32(bitmap.Width - imgBorder.Width);
                    Bitmap resized = new Bitmap(bitmap,
                        new System.Drawing.Size(bitmap.Width - unitsToResize, bitmap.Height - unitsToResize));
                    // Display the stream
                    streamImg.Source = ConvertBitmap.BitmapToBitmapSource(resized);
                    // Clean up
                    bitmap.Dispose();
                    resized.Dispose();
                }
            }));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            stop = true;
        }

        public void updateSpeedNormChart()
        {
            // Changed to BeginInvoke from Invoke
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate()
            {
                palmSpeedChart.InvalidatePlot(true);
            }));
        }
        // Updates both (rms and rms crossing charts)
        public void updateRMSChart()
        {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate()
            {
                rmsChart.InvalidatePlot(true);
                rmsCrossChart.InvalidatePlot(true);
            }));
        }
    }
}
