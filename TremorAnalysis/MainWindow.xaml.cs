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

        public PlotModel plotModelSpeedNorm;
        public LineSeries lineSerieSpeedNorm;
        public List<DataPoint> lineSerieSpeedNormBuffer = new List<DataPoint>();
        public bool plotSNNow = false;

        public int fps = 50; // Usually value
        public List<DataPoint> speedNormBuffer = new List<DataPoint>();

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

            // Start chart handle thread
            Thread chartHandleThread = new Thread(startChartHandle);
            chartHandleThread.Priority = ThreadPriority.Highest;
            chartHandleThread.Start();

            // Start signal analysis thread
            Thread signalAnalysisThread = new Thread(startSignalAnalysis);
            signalAnalysisThread.Priority = ThreadPriority.Highest;
            signalAnalysisThread.Start();
        }

        private void startSignalAnalysis()
        {
            //UpdateFrequencyLabel(" - Hz");
            // Window length (ms) that enable to get power of 2 in points
            SignalAnalysis sa = new SignalAnalysis(this, 1280);
        }

        private void startChartHandle()
        {
            ChartThread ct = new ChartThread(this);
        }

        private void startHandTracking()
        {
            MainPipeline mp = new MainPipeline(this);
            // Changed to BeginInvoke from Invoke
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate()
            {
                streamImg.Source = null;
                lineSerieSpeedNorm.Points.Clear();
                updateSpeedNormChart();                
            }));
            mp.Start();
            // Changed to BeginInvoke from Invoke
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
            {
                startButton.IsEnabled = true;
                stopButton.IsEnabled = false;
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
    }
}
