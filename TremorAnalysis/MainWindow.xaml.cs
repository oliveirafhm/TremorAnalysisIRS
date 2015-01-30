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

namespace TremorAnalysis
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public volatile bool stop = false;

        public MainWindow()
        {
            InitializeComponent();
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
            this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
            {
                fpsLabel.Content = status;
            }));
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            startButton.IsEnabled = false;
            stopButton.IsEnabled = true;

            stop = false;
            // Start the main_pipeline thread
            System.Threading.Thread thread = new System.Threading.Thread(startHandTracking);
            thread.Priority = System.Threading.ThreadPriority.Highest;
            thread.Start();
            System.Threading.Thread.Sleep(5);
        }

        private void startHandTracking()
        {
            MainPipeline mp = new MainPipeline(this);
            mp.Start();
            this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
            {
                startButton.IsEnabled = true;
                stopButton.IsEnabled = false;
                streamImg.Source = null;
            }));
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            stop = true;
            //stopButton.IsEnabled = false;
            //System.Threading.Thread.Sleep(5);
            //startButton.IsEnabled = true;
        }

        public void DisplayBitmap(Bitmap bitmap)
        {
            this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
            {
                if (bitmap != null)
                {
                    // Mirror the stream Image control
                    //streamImg.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
                    //ScaleTransform mainTransform = new ScaleTransform();
                    //mainTransform.ScaleX = -1;
                    //mainTransform.ScaleY = 1;
                    //streamImg.RenderTransform = mainTransform;

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
    }
}
