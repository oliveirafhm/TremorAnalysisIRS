using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OxyPlot;

namespace TremorAnalysis
{
    class HandIRSData
    {
        // Hand data
        private int id;
        private long timeStamp;
        private bool isCalibrated;
        private Int32 openness;
        private PXCMPoint3DF32 massCenter;
        // Joint hand center data
        private PXCMHandData.JointData jointData;
        private int confidenceJoint;
        private PXCMPoint3DF32 positionWorldJoint;
        private PXCMPoint3DF32 speedJoint;

        private double timeElapsed;
        private double speedNorm;
        //private long startTime;
        public long startTime { get; set; }

        public HandIRSData(PXCMHandData.IHand iHandData, long start_time = -1)
        {
            id = iHandData.QueryUniqueId();
            timeStamp = iHandData.QueryTimeStamp();// The same of image.timeStamp
            isCalibrated = iHandData.IsCalibrated();
            openness = iHandData.QueryOpenness();// 0=close hand | 100=hand open
            massCenter = iHandData.QueryMassCenterWorld();

            iHandData.QueryTrackedJoint(PXCMHandData.JointType.JOINT_CENTER, out jointData);
            confidenceJoint = jointData.confidence;
            positionWorldJoint = jointData.positionWorld;// Meters
            speedJoint = jointData.speed;

            if (start_time == -1)
                startTime = timeStamp;
            else startTime = start_time;

            timeElapsed = UsefulFunctions.nsToS(timeStamp - startTime);
            speedNorm = UsefulFunctions.calc3DPointNorm(speedJoint);
        }

        public string PrintData()
        {
            string log = "HandID: " + id +
                        "\nTime elapsed (s): " + timeElapsed +
                        "\nCalibrated? " + isCalibrated + " Hand open? " + openness +
                        "\nHand mass center -> " + UsefulFunctions.point3DToString(massCenter) +
                        "\nJoint center - Confidence: " + confidenceJoint +
                            "\nSpeed (norm) -> " + speedNorm +
                        "\nPosition world -> " + UsefulFunctions.point3DToString(positionWorldJoint);
            return log;
        }

        /* Returns the hand speed norm along the time */
        public DataPoint getPlotData()
        {
            return new DataPoint(timeElapsed, speedNorm);
        }
    }
}
