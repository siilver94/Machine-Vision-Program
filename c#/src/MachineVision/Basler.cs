using Basler.Pylon;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VisionProgram
{
        /// <summary>
        /// ImageSignal 이벤트로 사진 받기
        /// </summary>
        public sealed class PylonBasler
        {
            //트리거모드 ON하고 컨티뉴어스 스타트하면 그때부터 접점신호받을때마다 촬영함.

            public enum CurrentStatus
            {
                OneShot, IOShot, ContinuousShot, TestShot1, TestShot2, LiveShot, Stop
            }

            public string ip = "";
            public Camera camera;
            private PixelDataConverter m_converter = new PixelDataConverter();
            public RotateFlipType Rotate;
            bool USE_Rotate = false;

            public bool Connected = false;

            public delegate void EveHandler(CurrentStatus Command, object Data, int ArrayNum);
            public event EveHandler ImageSignal;

            public delegate void EveHandler2(bool Connected, int ArrayNum);
            public event EveHandler2 CommSignal;
            public int ArrayNum = -1;

            CurrentStatus Mode = CurrentStatus.Stop;

            public PylonBasler(string ip)
            {
                this.ip = ip;
                StartconnectThread(0);
            }

            public PylonBasler(string ip, int ArrayNum)
            {
                this.ip = ip;
                this.ArrayNum = ArrayNum;
                StartconnectThread(0);
            }

            public PylonBasler(string ip, RotateFlipType Rotate, int ArrayNum)
            {
                this.ip = ip;
                this.Rotate = Rotate;
                this.ArrayNum = ArrayNum;
                USE_Rotate = true;
                StartconnectThread(0);
            }

            #region ////////////////// connectThread //////////////////
            private Thread connectThread;
            bool connectThreadFlag = false;
            private void connectThreadMethod(object param)
            {
                while (connectThreadFlag)
                {
                    try
                    {
                        if (camera == null || !camera.IsConnected)
                        {
                            if (Connected)
                            {
                                CommSignal(false, ArrayNum);
                                Connected = false;
                            }

                            Connect();
                        }
                    }
                    catch (Exception)
                    {

                    }
                    Thread.Sleep(1000);
                }
            }
            void StartconnectThread(int param)
            {
                connectThreadFlag = true;
                connectThread = new Thread((new ParameterizedThreadStart(connectThreadMethod)));
                connectThread.Start(param);
            }
            void StopconnectThread(int None)
            {
                connectThreadFlag = false;
            }
            void KillconnectThread(int None)
            {
                connectThread.Abort();
            }
            #endregion ////////////////// connectThread //////////////////

            #region ////////////////// LiveThread //////////////////
            private Thread LiveThread;
            bool LiveThreadFlag = false;
            private void LiveThreadMethod(object param)
            {
                int para = (int)param;
                while (LiveThreadFlag)
                {
                    try
                    {
                        try
                        {
                            // Starts the grabbing of one image.
                            camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.SingleFrame);
                            camera.StreamGrabber.Start(1, GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
                        }
                        catch (Exception)
                        {
                            //ShowException( exception );
                        }
                    }
                    catch (Exception)
                    {

                    }
                    Thread.Sleep(para);
                }
            }
            public void StartLiveThread(int param)
            {
                LiveThreadFlag = true;
                LiveThread = new Thread((new ParameterizedThreadStart(LiveThreadMethod)));
                LiveThread.Start(param);
            }
            public void StopLiveThread(int None)
            {
                LiveThreadFlag = false;
            }
            public void KillLiveThread(int None)
            {
                LiveThread.Abort();
            }
            #endregion ////////////////// LiveThread //////////////////

            public void Connect()
            {// 카메라 열기
                Environment.SetEnvironmentVariable("PYLON_GIGE_HEARTBEAT", "500" /*ms*/);

                try
                {
                    List<ICameraInfo> allCameras = CameraFinder.Enumerate();
                    foreach (ICameraInfo cameraInfo in allCameras)
                    {
                        ListViewItem item = new ListViewItem(cameraInfo[CameraInfoKey.FriendlyName]);

                        string toolTipText = "";
                        foreach (KeyValuePair<string, string> kvp in cameraInfo)
                        {
                            toolTipText += kvp.Key + ": " + kvp.Value + "\n";
                        }
                        item.ToolTipText = toolTipText;
                        item.Tag = cameraInfo;

                        if (toolTipText.IndexOf(ip) > 0)
                        {
                            camera = new Camera(cameraInfo);
                            camera.CameraOpened += Configuration.AcquireContinuous;
                            camera.StreamGrabber.ImageGrabbed += OnImageCallBack;

                            camera.Open();

                            UseGamma(false);
                            SetBlackLevel(0);

                            Connected = true;
                            CommSignal(true, ArrayNum);
                        }
                    }

                }
                catch (Exception)
                {

                }
            }

            public void Dispose()
            {// 카메라 FREE

                StopconnectThread(0);

                try
                {
                    if (camera != null)
                    {
                        Stop();
                        camera.StreamGrabber.ImageGrabbed -= OnImageCallBack;
                        camera.Close();
                        camera.Dispose();
                    }
                }
                catch (Exception)
                {

                }


            }

            Bitmap GetBitmap(IGrabResult grabResult)
            {
                if (grabResult.IsValid)
                {
                    Bitmap bmp = new Bitmap(grabResult.Width, grabResult.Height, PixelFormat.Format32bppRgb);
                    BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
                    IntPtr ptrBmp = bmpData.Scan0;

                    m_converter.OutputPixelFormat = PixelType.BGRA8packed;
                    m_converter.Convert(ptrBmp, bmpData.Stride * bmp.Height, grabResult);

                    bmp.UnlockBits(bmpData);

                    return bmp;
                }
                return null;
            }

            void OnImageCallBack(Object sender, ImageGrabbedEventArgs e)
            {
                try
                {
                    if (e.GrabResult.IsValid)
                    {
                        Bitmap bmp = new Bitmap(e.GrabResult.Width, e.GrabResult.Height, PixelFormat.Format32bppRgb);

                        BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
                        IntPtr ptrBmp = bmpData.Scan0;

                        m_converter.OutputPixelFormat = PixelType.BGRA8packed;
                        m_converter.Convert(ptrBmp, bmpData.Stride * bmp.Height, e.GrabResult);

                        bmp.UnlockBits(bmpData);

                        if (USE_Rotate)
                            bmp.RotateFlip(this.Rotate);

                        ImageSignal(Mode, bmp, ArrayNum);
                    }

                }
                catch (Exception)
                {

                }
            }

            object ModeChangeLock = new object();

            //---------------↓ 촬영모드관련 ↓---------------┐

            bool BeforeMode()
            {
                bool result = false;

                if (Mode == CurrentStatus.Stop || Mode == CurrentStatus.IOShot || Mode == CurrentStatus.OneShot || Mode == CurrentStatus.TestShot1 || Mode == CurrentStatus.TestShot2)
                    result = true;

                return result;
            }

            /// <summary>
            /// 나중에 Stop()으로 꺼야합니다.
            /// </summary>
            public void IOShot()
            {
                lock (ModeChangeLock)
                {
                    if (BeforeMode())
                    {
                        Mode = CurrentStatus.IOShot;

                        try
                        {
                            camera.Parameters[PLCamera.TriggerSource].SetValue(PLCamera.TriggerSource.Line1);
                            camera.Parameters[PLCamera.TriggerMode].SetValue(PLCamera.TriggerMode.On);
                            camera.Parameters[PLCamera.TriggerActivation].SetValue(PLCamera.TriggerActivation.RisingEdge);

                            camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.Continuous);
                            camera.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                }
            }


            /// <summary>
            /// 나중에 Stop()으로 꺼야합니다.
            /// </summary>
            public void IOShot_FallingEdge()
            {
                lock (ModeChangeLock)
                {
                    if (BeforeMode())
                    {
                        Mode = CurrentStatus.IOShot;

                        try
                        {
                            camera.Parameters[PLCamera.TriggerSource].SetValue(PLCamera.TriggerSource.Line1);
                            camera.Parameters[PLCamera.TriggerMode].SetValue(PLCamera.TriggerMode.On);
                            camera.Parameters[PLCamera.TriggerActivation].SetValue(PLCamera.TriggerActivation.FallingEdge);

                            camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.Continuous);
                            camera.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);

                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }

            /// <summary>
            /// 한번만 촬영함.
            /// </summary>
            public void OneShot()
            {
                lock (ModeChangeLock)
                {
                    if (BeforeMode())
                    {
                        Mode = CurrentStatus.OneShot;

                        try
                        {
                         //if ( camera.StreamGrabber.IsGrabbing )
                         //   {
                         //       camera.StreamGrabber.Stop( );
                         //   }
                            // Starts the grabbing of one image.
                            camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.SingleFrame);
                            camera.StreamGrabber.Start(1, GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            //ShowException( e );
                            //MessageBox.Show( e.Message );
                            camera.Parameters [ PLCamera.AcquisitionMode ].SetValue( PLCamera.AcquisitionMode.SingleFrame );
                            camera.StreamGrabber.Start( 1, GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber );
                            //ShowException( exception e );
                        }
                    }
                }
            }

        private void ShowException( Exception exception )
        {
            MessageBox.Show( "Exception caught:\n" + exception.Message,"Error",MessageBoxButtons.OK, MessageBoxIcon.Error );
        }

        /// <summary>
        /// 한번만 촬영함.
        /// OneShot이랑 내용은 같고
        /// 이벤트 파라미터만 TestShot1 또는 2로 날립니다.
        /// Test1 Test2 2개 사용가능
        /// </summary>
        public void TestShot(int num)
            {
                lock (ModeChangeLock)
                {
                    if (BeforeMode())
                    {
                        if (num == 1)
                            Mode = CurrentStatus.TestShot1;
                        else
                            Mode = CurrentStatus.TestShot2;

                        try
                        {
                            // Starts the grabbing of one image.
                            camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.SingleFrame);
                            camera.StreamGrabber.Start(1, GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
                        }
                        catch (Exception)
                        {
                            //ShowException( exception );
                        }
                    }
                }
            }

            /// <summary>
            /// Stop()으로꺼야합니다.
            /// Interval 100~1000
            /// </summary>
            public void LiveShot(int Interval)
            {
                if (Interval < 100 || Interval > 1000)
                    Interval = 500;

                lock (ModeChangeLock)
                {
                    if (BeforeMode())
                    {
                        Mode = CurrentStatus.LiveShot;
                        StartLiveThread(Interval);
                    }
                }
            }


            /// <summary>
            /// LiveShot 멈추기.
            /// </summary>
            public void Stop()
            {
                lock (ModeChangeLock)
                {
                    // Stop the grabbing.
                    try
                    {
                        StopLiveThread(0);
                        //camera.StreamGrabber.Stop( );
                        //camera.Parameters[ PLCamera.TriggerMode ].SetValue( PLCamera.TriggerMode.Off );
                    }
                    catch (Exception)
                    {

                    }

                    Mode = CurrentStatus.Stop;
                }
            }

            //---------------↑ 촬영모드관련 ↑---------------┘



            //---------------↓ 설정값 변경 ↓---------------┐

            public void SetBlackLevel(int iValue)
            {
                try
                {
                    camera.Parameters[PLCamera.BlackLevelRaw].SetValue(iValue);
                }
                catch (Exception)
                {

                }
            }

            public void SetGain(int iValue)
            {
                try
                {
                    camera.Parameters[PLCamera.GainAuto].SetValue(PLCamera.GainAuto.Off);

                    camera.Parameters[PLCamera.GainSelector].SetValue(PLCamera.GainSelector.DigitalAll);

                    camera.Parameters[PLCamera.GainRaw].SetValue(iValue);
                }
                catch (Exception)
                {

                }
            }

            public void SetExp(int iValue)
            {
                try
                {

                    camera.Parameters[PLCamera.ExposureTimeAbs].SetValue(iValue);

                }
                catch (Exception)
                {

                }
            }

            public int GetExp()
            {
                int result = 0;
                try
                {
                    result = (int)camera.Parameters[PLCamera.ExposureTimeAbs].GetValue();

                }
                catch (Exception)
                {

                }
                return result;
            }

            public void UseGamma(bool use)
            {
                camera.Parameters[PLCamera.GammaEnable].SetValue(use);

            }

            public void SetGamma(int iValue)
            {
                try
                {
                    camera.Parameters[PLCamera.GammaEnable].SetValue(false);
                    camera.Parameters[PLCamera.GammaSelector].SetValue(PLCamera.GammaSelector.User);
                    camera.Parameters[PLCamera.Gamma].SetValue(iValue);
                }
                catch (Exception)
                {

                }
            }

            public void DigitalShift(int iValue)
            {
                try
                {
                    camera.Parameters[PLCamera.DigitalShift].SetValue(iValue);
                }
                catch (Exception)
                {

                }
            }

            public void Save()
            {
                camera.Parameters[PLCamera.UserSetSelector].SetValue(PLCamera.UserSetSelector.UserSet3);
                camera.Parameters[PLCamera.UserSetSave].Execute();


            }

            public void Load()
            {
                camera.Parameters[PLCamera.UserSetSelector].SetValue(PLCamera.UserSetSelector.UserSet3);
                camera.Parameters[PLCamera.UserSetLoad].Execute();

            }

            public void AutoWhiteBalanceOnce()
            {
                try
                {
                    camera.Parameters[PLCamera.BalanceWhiteAuto].SetValue(PLCamera.BalanceWhiteAuto.Once);
                }
                catch (Exception)
                {


                }


            }

            public void AutoWhiteBalanceOff()
            {
                try
                {
                    camera.Parameters[PLCamera.BalanceWhiteAuto].SetValue(PLCamera.BalanceWhiteAuto.Off);
                }
                catch (Exception)
                {


                }


            }

            public int GetDelay()
            {
                int result = 0;
                try
                {
                    result = (int)camera.Parameters[PLCamera.TriggerDelayAbs].GetValue();

                }
                catch (Exception)
                {

                }

                return result;
            }

            public void SetDelay(int iValue)
            {
                try
                {

                    camera.Parameters[PLCamera.TriggerDelayAbs].SetValue(iValue);

                }
                catch (Exception ex)
                {

                }
            }

            public int GetDebouncer()
            {
                int result = 0;
                try
                {
                    result = (int)camera.Parameters[PLCamera.LineDebouncerTimeAbs].GetValue();

                }
                catch (Exception ex)
                {

                }

                return result;
            }

            public void SetDebouncer(double iValue)
            {
                try
                {
                    camera.Parameters[PLCamera.LineDebouncerTimeAbs].SetValue(iValue);
                }
                catch (Exception ex)
                {

                }
            }

            //---------------↑ 설정값 변경 ↑---------------┘

        }
    }
