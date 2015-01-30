using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace TremorAnalysis
{
    class MainPipeline
    {
        private MainWindow myWindow;
        private PXCMSenseManager senseManager;                
        private bool disconnected = false;
        private byte[] LUT;
        private int handId = -1;
        private long startTime;

        public MainPipeline(MainWindow window)
        {
            this.myWindow = window;
            LUT = Enumerable.Repeat((byte)0, 256).ToArray();
            LUT[255] = 1;
        }

        private void ConfigureHandTracking()
        {
            PXCMHandConfiguration cfg = senseManager.QueryHand().CreateActiveConfiguration();
            if (cfg == null)
            {
                myWindow.UpdateStatus("Failed Create Hand Configuration");
                return;
            }
            cfg.EnableSegmentationImage(true);
            cfg.EnableTrackedJoints(true);
            cfg.EnableJointSpeed(PXCMHandData.JointType.JOINT_CENTER, PXCMHandData.JointSpeedType.JOINT_SPEED_ABSOLUTE, 0);
            cfg.SetTrackingMode(PXCMHandData.TrackingModeType.TRACKING_MODE_FULL_HAND);
            cfg.ApplyChanges();
            cfg.Update();
            cfg.Dispose();
        }

        /* Checking if sensor device connect or not */
        private bool DisplayDeviceConnection(bool state)
        {
            if (state)
            {
                if (!disconnected) myWindow.UpdateStatus("Device Disconnected");
                disconnected = true;
            }
            else
            {
                if (disconnected) myWindow.UpdateStatus("Device Reconnected");
                disconnected = false;
            }
            return disconnected;
        }

        private void ConfigureDevice()
        {
            PXCMCapture.DeviceInfo dInfo;
            PXCMCapture.Device device = senseManager.QueryCaptureManager().QueryDevice();
            if (device != null)
            {
                pxcmStatus result = device.QueryDeviceInfo(out dInfo);
                if (result == pxcmStatus.PXCM_STATUS_NO_ERROR && dInfo != null && 
                    dInfo.model == PXCMCapture.DeviceModel.DEVICE_MODEL_IVCAM)
                {
                    // Camera accuracy settings
                    device.SetIVCAMAccuracy(PXCMCapture.Device.IVCAMAccuracy.IVCAM_ACCURACY_FINEST);
                    device.SetIVCAMLaserPower(16);
                    // 3 - Very close range: Very low smoothing effect with high sharpness, accuracy levels, and low noise artifacts.
                    // Good for any distances of up to 350mm.
                    device.SetIVCAMFilterOption(3);
                    device.SetIVCAMMotionRangeTradeOff(0);
                    device.SetMirrorMode(PXCMCapture.Device.MirrorMode.MIRROR_MODE_HORIZONTAL);

                    device.Dispose();
                }
            }
        }
        /* Displaying Mask Images*/
        private unsafe void DisplayPicture(PXCMImage depth, PXCMHandData handData)
        {
            if (depth == null)
                return;
            PXCMImage image = depth;
            Bitmap labeledBitmap = null;
            try
            {
                labeledBitmap = new Bitmap(image.info.width, image.info.height, PixelFormat.Format32bppRgb);
            }
            catch (Exception)
            {
                image.Dispose();
                return;
            }
            // Get hand by time of appearance and hand id
            if (handData.QueryNumberOfHands() == 0)
            {
                handId = -1;
                // Clean up (test)
                depth.Dispose();
                handData.Dispose();
                image.Dispose();
                labeledBitmap.Dispose();
                return;
            }
            else if (handId == -1)            
                handData.QueryHandId(PXCMHandData.AccessOrderType.ACCESS_ORDER_BY_TIME, 0, out handId);
            
            PXCMHandData.IHand iHandData;
            handData.QueryHandDataById(handId, out iHandData);
            if (iHandData != null && 
                (iHandData.QuerySegmentationImage(out image) >= pxcmStatus.PXCM_STATUS_NO_ERROR))
            {
                // Draw image
                PXCMImage.ImageData data;
                if (image.AcquireAccess(PXCMImage.Access.ACCESS_READ, PXCMImage.PixelFormat.PIXEL_FORMAT_Y8,
                out data) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
                {
                    Rectangle rect = new System.Drawing.Rectangle(0, 0, image.info.width, image.info.height);

                    BitmapData bitmapdata = labeledBitmap.LockBits(rect, ImageLockMode.ReadWrite, labeledBitmap.PixelFormat);
                    byte* numPtr = (byte*)bitmapdata.Scan0; //dst
                    byte* numPtr2 = (byte*)data.planes[0]; //row
                    int imagesize = image.info.width * image.info.height;
                    byte num2 = (byte)iHandData.QueryBodySide();

                    byte tmp = 0;
                    for (int i = 0; i < imagesize; i++, numPtr += 4, numPtr2++)
                    {

                        tmp = (byte)(LUT[numPtr2[0]] * num2 * 100);
                        numPtr[0] = (Byte)(tmp | numPtr[0]);
                        numPtr[1] = (Byte)(tmp | numPtr[1]);
                        numPtr[2] = (Byte)(tmp | numPtr[2]);
                        numPtr[3] = 0xff;
                    }
                    bool isError = false;
                    try
                    {
                        labeledBitmap.UnlockBits(bitmapdata);
                    }
                    catch (Exception)
                    {
                        isError = true;
                    }
                    try
                    {
                        image.ReleaseAccess(data);
                    }
                    catch (Exception)
                    {
                        isError = true;
                    }

                    if (isError)
                    {
                        labeledBitmap.Dispose();
                        image.Dispose();
                        return;
                    }
                } // End draw image
            }
            if (labeledBitmap != null)
            {
                // Update the user interface
                myWindow.DisplayBitmap(labeledBitmap);
                labeledBitmap.Dispose();
            }
            // Clean up
            image.Dispose();
            depth.Dispose();
            handData.Dispose();            
        }

        private string dataToLog(PXCMHandData.IHand iHandData, int frameCounter)
        {
            PXCMPoint3DF32 massCenter = iHandData.QueryMassCenterWorld();
            PXCMHandData.JointData jointData;
            iHandData.QueryTrackedJoint(PXCMHandData.JointType.JOINT_CENTER, out jointData);
            int confidenceJoint = jointData.confidence;
            PXCMPoint3DF32 positionWorldJoint = jointData.positionWorld;            
            PXCMPoint3DF32 speedJoint = jointData.speed;

            int id = iHandData.QueryUniqueId();
            long timeStamp = iHandData.QueryTimeStamp();// The same of image.timeStamp
            bool isCalibrated = iHandData.IsCalibrated();
            Int32 openness = iHandData.QueryOpenness();// 0=close hand | 100=hand open

            if (frameCounter == 1)            
                startTime = timeStamp;
            
            string log = "HandID: " + id + " Time elapsed (s): " + UsefulFunctions.nsToS(timeStamp - startTime) +
                " Calibrated? " + isCalibrated + " Hand open? " + openness +
                "\nHand mass center -> " + UsefulFunctions.point3DToString(massCenter) +
                "\nJoint center - Confidence: " + confidenceJoint +
                    "\nSpeed -> " + UsefulFunctions.point3DToString(speedJoint) +
                "\nPosition world -> " + UsefulFunctions.point3DToString(positionWorldJoint);
            return log;
        }

        public void Start()
        {
            bool flag = true;
            /* Using PXCMSenseManager to handle data */
            senseManager = PXCMSenseManager.CreateInstance();
            if (senseManager == null)
            {
                myWindow.UpdateStatus("Failed creating SenseManager");
                return;
            }
            /* Set Module */
            pxcmStatus status = senseManager.EnableHand();
            if (status != pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                myWindow.UpdateStatus("Failed Loading Hand Module");
                return;
            }
            ConfigureHandTracking();
            FPSTimer timer = new FPSTimer(myWindow);
            if (senseManager.Init() == pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                myWindow.UpdateStatus("Init Started");
                ConfigureDevice();
                myWindow.UpdateStatus("Streaming");
                int frameCounter = 0;
                startTime = 0;
                /* Loop through the each frame */
                while (!myWindow.stop)
                {
                    if (senseManager.AcquireFrame(true) < pxcmStatus.PXCM_STATUS_NO_ERROR)
                    {
                        break;
                    }
                    frameCounter++;
                    if (!DisplayDeviceConnection(!senseManager.IsConnected()))
                    {
                        PXCMHandModule handModule = senseManager.QueryHand();
                        if (handModule == null)
                        {
                            myWindow.UpdateStatus("Failed Loading Hand Module");
                            return;
                        }
                        PXCMHandData handData = handModule.CreateOutput();
                        if (handData == null)
                        {
                            myWindow.UpdateStatus("Failed Create Output HandData");
                            return;
                        }
                        else handData.Update();
                        PXCMCapture.Sample sample = senseManager.QuerySample();
                        if (sample != null && sample.depth != null)
                        {
                            DisplayPicture(sample.depth, handData);
                        }
                        timer.Tick();
                        // Clean up
                        //sample.ReleaseImages(); //Tested and crash the app
                        handData.Dispose();
                        handModule.Dispose();
                    }
                    senseManager.ReleaseFrame();
                }
            }
            else
            {
                myWindow.UpdateStatus("Init Failed");
                flag = false;
            }
            senseManager.Close();
            senseManager.Dispose();
            if (flag)
            {
                myWindow.UpdateStatus("Stopped");
            }
        }
    }
}
