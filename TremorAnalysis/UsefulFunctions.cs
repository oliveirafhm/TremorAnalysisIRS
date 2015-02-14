using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TremorAnalysis
{
    class UsefulFunctions
    {
        //Description of original timeStamp function from RealSense SDK
        //  The QueryTimeStamp function returns the time stamp when the collection of the hand data was completed.
        //Return Status
        //  The time stamp, in 100ns.
        public static double nsToS(long timeStamp)
        {
            return (timeStamp * 100) * 0.000000001;
        }

        public static string point3DToString(PXCMPoint3DF32 p3d)
        {
            return String.Format("X: {0} , Y: {1} , Z: {2}", p3d.x, p3d.y, p3d.z);
        }

        public static double calc3DPointNorm(PXCMPoint3DF32 p3d)
        {
            return Math.Sqrt(p3d.x * p3d.x + p3d.y * p3d.y + p3d.z * p3d.z);
        }

        public static double rootMeanSquare(double[] window)
        {
            double s = 0;
            int i;
            for (i = 0; i < window.Length; i++)
            {
                s += window[i] * window[i];
            }
            return Math.Sqrt(s / window.Length);
        }

        private static int indicator(double x, double y, double th)
        {
            if ((x < th && y > th) || (y < th && x > th))
                return 1;
            else
                return 0;
        }

        public static int rmsCrossing(double[] window, double rmsValue)
        {
            int sum = 0;
            for (int i = 1; i < window.Length; i++)
            {
                sum += indicator(window[i], window[i - 1], rmsValue);
            }
            return sum;
            //return (int)Math.Floor((double)sum / (window.Length - 1));
        }
    }
}
