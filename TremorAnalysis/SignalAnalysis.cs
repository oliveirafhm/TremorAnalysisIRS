using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OxyPlot;
using System.Threading;

namespace TremorAnalysis
{
    class SignalAnalysis
    {

        private MainWindow myWindow;
        private double[] signalWindow;
        private int windowLengthP; // In points, the amostration rate usually is 50 hz
        private int windowLengthMS; // In milliseconds
        private bool sleep = false;

        // windowLengthMS in milliseconds
        public SignalAnalysis(MainWindow myWindow, int windowLengthMS)
        {
            this.myWindow = myWindow;
            this.windowLengthMS = windowLengthMS;
            initSignalAnalysis();
            startSignalAnalysis();
        }

        // Gets windowLength points from speedNormBuffer and remove then 
        private double[] getPointsFromBuffer()
        {
            List<DataPoint> temp;
            double[] yTemp = new double[windowLengthP];
            lock (myWindow.speedNormBuffer)
            {
                //temp = new List<DataPoint>(myWindow.speedNormBuffer.GetRange(0, windowLengthP));
                temp = myWindow.speedNormBuffer.GetRange(0, windowLengthP);
                myWindow.speedNormBuffer.RemoveRange(0, windowLengthP);

            }

            for (int i = 0; i < windowLengthP; i++)
            {
                yTemp[i] = temp[i].Y;
            }

            return yTemp;
        }

        private void initSignalAnalysis()
        {
            // Convert windowLength from millisencods to approximate number of points
            windowLengthP = (int)((windowLengthMS * this.myWindow.fps) / 1000);
        }

        private void startSignalAnalysis()
        {
            while (!myWindow.stop)
            {
                lock (myWindow.speedNormBuffer)
                {
                    if (myWindow.speedNormBuffer.Count < windowLengthP)
                        sleep = true;
                }
                if (sleep)
                {
                    Thread.Sleep(windowLengthMS);
                    sleep = false;
                    continue;
                }
                signalWindow = getPointsFromBuffer();
                
                double rms = UsefulFunctions.rootMeanSquare(signalWindow);
                myWindow.UpdateRMSLabel(rms.ToString("F2"));

                int rmsCross = UsefulFunctions.rmsCrossing(signalWindow, rms);
                myWindow.UpdateRMSCrossLabel(rmsCross.ToString());

                //double[] x = null;
                //double[] y = null;
                //y = Biolab.SJSM.DSP.FFT.PowerSpectrum(signalWindow, Convert.ToDouble(myWindow.fps), out x);

                //int maxEnergyIndex = 0;
                //// Frequency of most energy
                //double tremorFrequency;
                //for (int i = 0; i < y.Length; i++)
                //{
                //    if (y[i] > y[maxEnergyIndex])
                //        maxEnergyIndex = i;
                //}
                //tremorFrequency = x[maxEnergyIndex];
                //myWindow.UpdateFrequencyLabel(tremorFrequency.ToString("F1") + " Hz");
            }
        }
    }
}
