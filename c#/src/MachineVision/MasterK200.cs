using Ken2.Communication;
using Ken2.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VisionProgram
{


    /// <summary>
    /// 펄스 데이터를 감지합니다.
    /// </summary>
    public class PulseDetector
    {
        int data = 0;

        /// <summary>
        /// 데이터가 바뀌기만 해도 감지합니다.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public bool Detect(int input)
        {
            if (input != data)
            {
                data = input;

                return true;
            }
            else
            {
                data = input;

                return false;
            }

        }

        /// <summary>
        /// 특정 값을 감지합니다.
        /// input데이터에 값을 넣고, DetectValue에 데이터를 집어넣으면
        /// 그값을 감지합니다.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="DetectValue"></param>
        /// <returns></returns>
        public bool Detect(int input, int DetectValue)
        {
            if (input == DetectValue && input != data)
            {
                data = input;

                return true;
            }
            else
            {
                data = input;

                return false;
            }

        }

        /// <summary>
        /// 이전 데이터 파라미터 추가됨.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="DetectValue"></param>
        /// <param name="BeforeValue"></param>
        /// <returns></returns>
        public bool Detect(int input, int DetectValue, int BeforeValue)
        {
            if (input == DetectValue && data == BeforeValue)
            {
                data = input;

                return true;
            }
            else
            {
                data = input;

                return false;
            }

        }
    }


    public class MasterK200_1
    {

        private object lockObjj = new object();

        Form1 mainform;

        private static DateTime Delay(int MS)
        {
            DateTime ThisMoment = DateTime.Now;
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, MS);
            DateTime AfterWards = ThisMoment.Add(duration);
            while (AfterWards >= ThisMoment)
            {
                System.Windows.Forms.Application.DoEvents();
                ThisMoment = DateTime.Now;
            }
            return DateTime.Now;
        }

        LingerOption lingeroption = new LingerOption(true, 0);

        string ServerIP = "";
        int ServerPort = 0;
        int ReceiveTimeOut = 0;

        string ClientIP = "";
        int ClientPort = 0;


        public delegate void EveHandler(string name, object data, int length);
        public event EveHandler TalkingComm;


        public bool Server_Connected = false;
        public NetworkStream _stream = null;
        private TcpClient mClient;


        public MasterK200_1(string ServerIP, int ServerPort, int ReceiveTimeOut, string ClientIP, int ClientPort, Form1 mainform)
        {

            this.ServerIP = ServerIP;
            this.ServerPort = ServerPort;
            this.ReceiveTimeOut = ReceiveTimeOut;
            this.ClientIP = ClientIP;
            this.ClientPort = ClientPort;
            this.mainform = mainform;

        }

        object tcplock = new object();

        public void Send(string str)
        {

            try
            {
                string SendData = Parsing.DeleteSpace(str);

                char[] CharArray = SendData.ToCharArray();// 0 0 0 0

                string[] NewSendData = new string[CharArray.Length / 2];// 2

                for (int i = 0; i < NewSendData.Length; i++)
                {
                    NewSendData[i] = CharArray[i * 2].ToString() + CharArray[i * 2 + 1].ToString();
                }

                byte[] SendBuffer = new byte[NewSendData.Length];

                for (int i = 0; i < SendBuffer.Length; i++)
                {
                    SendBuffer[i] = byte.Parse(NewSendData[i], System.Globalization.NumberStyles.HexNumber);
                }

                _stream.Write(SendBuffer, 0, SendBuffer.Length);


            }
            catch (Exception eee)
            {
                Pause();
                Console.WriteLine(eee);

            }

        }

        public void MasterK_Read_B(string address)    //  바이트로 읽기때문에 번지 address x2 해야함  16진수형식으로 EX ) 4000번지 ㅡ> (*2해서) 38 30 30 30 
        {
            //Send("4C5349532D47544F4641 0000 0033 0000 1300 0000 5400 1400 0000 0100 0700 25 44 42" + address + "2C01");   본               
            Send("4C5349532D5847540000 0000 B033 0000 1300 0152 5400 1400 0000 0100 0700 25 44 42" + address + "3C00");//DB4000 부터 30개워드 연속읽기
                                                                                                                       //44C5349532D5847540000 0000 B033 0000 1300 0152 5400 1400 0000 0100 0700 25 44 42    34303030   3C00

        }

        //public void MasterK_Write_W( string address, string value )     //  Word로 쓰기때문에 번지 그대로 써도됨 단, 16진수형식
        public void MasterK_Write_W(string address, string value)     //  Word로 쓰기때문에 번지 그대로 써도됨 단, 16진수형식
        {
            lock (lockObjj)
            {
                Delay(10);

                //Send("4C5349532D47544F4641 0000 0033 0000 1600 0000 5800 0200 0000 0100 0800 25 44 57" + address + "0200" + value);   //본래
                Send("4C5349532D5847540000 0000 B033 0000 1500 0154 5800 0200 0000 0100 0700 25 44 57" + address + "0200" + value);   //수정 후
                                                                                                                                      //4C5349532D5847540000 0000 B033 0000 1500 0154 5800 0200 0000 0100 0700 25 44 57    32303030   0200     0100

            }
        }

        #region -----# Connect #-----
        //스레드변수 (스레드구성요소 3개)
        //[ FLAG ] [ METHOD ] [ THREAD ]
        private Thread Connect;//스레드
        bool ConnectFlag = false;//Bool Flag
        //스레드함수
        private void ConnectMethod(object param)
        {
            int para = (int)param;

            while (true)
            {
                Thread.Sleep(100);
                if (ConnectFlag == false)
                    break;

                try
                {

                    if (Server_Connected == false)//연결끊어졌을때만 함
                    {


                        System.Net.IPAddress ip = System.Net.IPAddress.Parse(ClientIP);
                        IPEndPoint ipLocalEndPoint = new IPEndPoint(ip, ClientPort);
                        mClient = new TcpClient(ipLocalEndPoint);

                        mClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, false);
                        mClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingeroption);
                        mClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 0);

                        mClient.ReceiveTimeout = ReceiveTimeOut;
                        mClient.Connect(ServerIP, ServerPort);
                        _stream = mClient.GetStream();
                        _stream.ReadTimeout = 1000;
                        Server_Connected = true;

                        CommStart();//연결되었으니 통신스레드 시작함.



                        TalkingComm("Connected", 0, 0);
                    }



                }
                catch (Exception)
                {
                    //Console.WriteLine("PLC연결 실패");
                }
            }


        }
        //스레드함수
        public void ConnectStart(int param)
        {
            //스레드스타트
            ConnectFlag = true;
            Connect = new Thread((new ParameterizedThreadStart(ConnectMethod)));
            Connect.Start(param);
            //스레드스타트
        }
        public void ConnectStop()
        {

            ConnectFlag = false;

        }
        #endregion

        int Start = 20000;
        int CalcByte(int Offset)
        {
            int result = Offset - Start;
            return result * 2;
        }


        #region -----# Comm #-----


        //int flag1, flag2, flag3, flag4, flag5, flag6, flag7, flag8;
        //int flag11, flag12, flag13, flag14, flag15, flag16, flag17, flag18;
        //int flag19, flag20, flag21;

        private Thread Comm;//스레드
        bool CommFlag = false;//Bool Flag

        public interface CamPoint
        {
            void SetCamPoint1(int pointNum1);
            void SetCamPoint2(int pointNum2);
        }

        //tttttttttttttttttttttttttttttttttt
        private void CommMethod()
        {
            PulseDetector Trigger1 = new PulseDetector();
            PulseDetector Trigger2 = new PulseDetector();



            byte[] buff = new byte[4096];

            int length = 0;

            while (CommFlag)
            {
                Delay(400);
                //if ( CommFlag == false )
                //    break;
                try
                {

                    Send("4C 53 49 53 2D 58 47 54 00 00 00 00 B0 33 00 00 13 00 01 52 54 00 14 00 00 00 01 00 07 00 25 44 42 34 30 30 30 32 00");

                    //Send("4C4749532D474C4F4641 0000 0033 0000 1400 0000 5400 1400 0000 0100 0800 25 44 42 32 30 30 30 30 9001"); 1000번지일경우 400개바이트 가져오기
                    length = _stream.Read(buff, 0, buff.Length);

                    string input = BitConverter.ToString(buff, 0, length);
                    string[] result = input.Split(new string[] { "-" }, StringSplitOptions.None);


                    if (mainform.Viewdatachk.Checked)
                    {
                        if (TalkingComm != null) TalkingComm("Data", result, length);
                    }

                    int resultTriger1 = Int32.Parse(result[36]); //  36 20002번지
                    //int modelNum1 = Int32.Parse(result[32]); // 32 20000번지

                    int camPoint1 = Convert.ToInt32(result[34], 16);
                    // int camPoint1 = Int32.Parse(result[34]); // 34 20001번지   

                    if (Trigger1.Detect(resultTriger1, 1, 0))  //Cam1 트리거 신호
                    {
                        if (TalkingComm != null) TalkingComm("Trigger1", camPoint1, length);

                    }

                    int resultTriger2 = Int32.Parse(result[42]); //  36 20005번지

                    // dgvC1.Rows[i].Cells[1].Value = address[i]; //  C0 16진수
                    //
                    // int val10 = Convert.ToInt32(address[i], 16);   //  C0 10진수              
                    // dgvC1.Rows[i].Cells[2].Value = val10;
                    int camPoint2 = Convert.ToInt32(result[40], 16);
                    //int modelNum2 = Int32.Parse(result[42]); // 32 20005번지
                    //int camPoint2 = Int32.Parse(result[40]); // 34 20004번지

                    if (Trigger2.Detect(resultTriger2, 1, 0))   //Cam2 트리거 신호
                    {
                        if (TalkingComm != null) TalkingComm("Trigger2", camPoint2, length);
                    }


                    string[] mm = new string[1];
                    mm[0] = result[32];     // 20000만번지 32

                    int val10 = Convert.ToInt32(mm[0], 16);

                    int[] model = new int[1];
                    model[0] = val10;

                    int modelnum = model[0];


                    if (mainform.CurrentModelNum1 != modelnum)
                    {
                        if (TalkingComm != null) TalkingComm("ModelChange1", modelnum, length);
                    }


                    //Thread.Sleep(0);
                }
                catch (Exception)
                {

                }

            }
        }

        //스레드함수
        public void CommStart()
        {
            //스레드스타트
            CommFlag = true;
            Comm = new Thread(CommMethod);
            Comm.Start();
            //스레드스타트
        }

        public void CommStop()
        {
            CommFlag = false;
        }

        private void Pause()
        {
            try
            {
                Server_Connected = false;

                if (_stream != null)
                {
                    _stream.Close();
                }

                if (mClient != null)
                {
                    mClient.Close();
                }

                CommStop();

            }
            catch (Exception exc)
            {

            }

            TalkingComm("DisConnected", 0, 0);
        }

        public void Disconnect()
        {
            try
            {
                Pause();

                ConnectStop();
            }
            catch (Exception exc)
            {

            }
        }

        public void Dispose()
        {
            try
            {
                Pause();

                ConnectStop();
            }
            catch (Exception)
            {

            }
        }

        #endregion


    }

}
