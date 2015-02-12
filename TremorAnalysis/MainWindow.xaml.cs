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

        public MainWindow()
        {
            InitializeComponent();
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            stopButton.IsEnabled = false;            
        }

        public void UpdateStatus(string status)
        {
            this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
            {
                statusLabel.Content = status;
            }));
        }

        public void UpdateFPSStatus(string status)
        {
            this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate()
            {
                fpsLabel.Content = status;
            }));
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            startButton.IsEnabled = false;
            stopButton.IsEnabled = true;

            stop = false;
            // Start main_pipeline thread
            Thread handTrackingThread = new Thread(startHandTracking);
            handTrackingThread.Priority = ThreadPriority.Highest;
            handTrackingThread.Start();

            // Start chart handle thread
            Thread chartHandleThread = new Thread(startChartHandle);
            chartHandleThread.Priority = ThreadPriority.Highest;
            chartHandleThread.Start();
            //Thread.Sleep(5);
        }

        private void startChartHandle()
        {
            ChartThread ct = new ChartThread(this);
            //this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
            //{

            //}));
        }

        private void startHandTracking()
        {
            MainPipeline mp = new MainPipeline(this);
            this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
            {
                streamImg.Source = null;
                lineSerieSpeedNorm.Points.Clear();
                updateSpeedNormChart();
            }));
            mp.Start();
            this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
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
            this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
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
            this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate()
           {
               palmSpeedChart.InvalidatePlot(true);
           }));
        }
    }
}
