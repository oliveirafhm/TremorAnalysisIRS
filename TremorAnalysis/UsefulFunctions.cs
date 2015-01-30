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
    }
}
