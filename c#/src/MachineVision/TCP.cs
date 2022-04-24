using Ken2.Communication;
using Ken2.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Vision_DH
{

    public class TCPClient_Monitor
    {
        LingerOption lingeroption = new LingerOption(true, 0);

        //---------------↓ 통신관련 ↓---------------┐
        string ServerIP = "";
        int ServerPort = 0;
        int ReceiveTimeOut = 0;
        string ClientIP = "";
        int ClientPort = 0;
        //---------------↑ 통신관련 ↑---------------┘

        public delegate void EveHandler(string name, object data);
        public event EveHandler TalkingComm;

        public bool Connected = false;

        Form1 mainform;

        public NetworkStream _stream = null;
        private TcpClient mClient;



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

        public void SendString(string str)
        {
            try
            {
                byte[] buff = Ken2.Communication.DataChange_K.StringToByteArr(str);
                _stream.Write(buff, 0, buff.Length);
            }
            catch (Exception)
            {

            }
        }

        public TCPClient_Monitor(string ServerIP, int ServerPort, int ReceiveTimeOut, Form1 mainform)
        {

            this.ServerIP = ServerIP;
            this.ServerPort = ServerPort;
            this.ReceiveTimeOut = ReceiveTimeOut;
            this.mainform = mainform;
            ConnectStart(0);

        }

        public TCPClient_Monitor(string ServerIP, int ServerPort, int ReceiveTimeOut, string ClientIP, int ClientPort)
        {

            this.ServerIP = ServerIP;
            this.ServerPort = ServerPort;
            this.ReceiveTimeOut = ReceiveTimeOut;
            this.ClientIP = ClientIP;
            this.ClientPort = ClientPort;

            ConnectStart(0);

        }

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

        #region -----# Connect #-----
        private Thread Connect;//스레드
        bool ConnectFlag = false;//Bool Flag
        //스레드함수
        private void ConnectMethod(object param)
        {
            int para = (int)param;

            while (true)
            {
                Thread.Sleep(1000);
                if (ConnectFlag == false)
                    break;

                try
                {

                    if (Connected == false)//연결끊어졌을때만 함
                    {

                        if (ClientPort == 0)
                        {
                            mClient = new TcpClient();
                            mClient.ReceiveTimeout = ReceiveTimeOut;
                            mClient.Connect(ServerIP, ServerPort);
                            _stream = mClient.GetStream();
                            Connected = true;

                            CommStart();//연결되었으니 통신스레드 시작함.
                        }
                        else
                        {
                            System.Net.IPAddress ip = System.Net.IPAddress.Parse(ClientIP);
                            IPEndPoint ipLocalEndPoint = new IPEndPoint(ip, 0);
                            mClient = new TcpClient(ipLocalEndPoint);

                            mClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, false);
                            mClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingeroption);
                            mClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 0);

                            mClient.ReceiveTimeout = ReceiveTimeOut;
                            mClient.Connect(ServerIP, ServerPort);
                            _stream = mClient.GetStream();
                            _stream.ReadTimeout = 1000;
                            Connected = true;

                            CommStart();//연결되었으니 통신스레드 시작함.

                        }


                        TalkingComm("Connected", Connected);
                    }



                }
                catch (Exception)
                {

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
            Connect.Abort();

            ConnectFlag = false;

        }
        #endregion

        /// <summary>
        /// 받은 데이터에서 상태에 해당하는 데이터만 추출해 옴
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public int ViewPrintStatus(string Data)
        {
            try
            {

                string[] split = Data.Split('\n');
                string[] buff0 = split[0].Split(',');
                string[] buff1 = split[1].Split(',');

                if (buff0[2] == "1")
                    return 2;
                if (buff1[7] == "1")
                    return 1;

            }
            catch (Exception)
            {


            }
            return 0;
        }

        #region -----# Comm #-----

        private Thread Comm;//스레드
        bool CommFlag = false;//Bool Flag

        //private void CommMethod()
        //{
        //    byte[] buff = new byte[1024];
        //    int length = 0;


        //    while (CommFlag)
        //    {
        //        try
        //        {

        //            SendString(
        //                mainform.ModelNamelbl.Text + "~" + mainform.ModelNamelbl.Text + "~" + mainform.ModelNamelbl1.Text + "~" + mainform.ModelNamelbl2.Text + "~" +
        //                mainform.QuantityData[0] + "~" + mainform.QuantityData[1] + "~" + mainform.QuantityData[2] + "~" + mainform.QuantityData[3] + "~" +
        //                mainform.QuantityData[4] + "~" + mainform.QuantityData[5] + "~" + mainform.QuantityData[6] + "~" + mainform.QuantityData[7] + "~" +
        //                mainform.dgvDE0.Rows[1].Cells[1].Value.ToString() + "~" + mainform.dgvDE0.Rows[1].Cells[2].Value.ToString() + "~" + mainform.dgvDE0.Rows[1].Cells[3].Value.ToString() + "~" + mainform.dgvDE0.Rows[1].Cells[4].Value.ToString()
        //                + "@0"
        //                );

        //            length = _stream.Read(buff, 0, buff.Length);

        //        }
        //        catch (System.IO.IOException)
        //        {
        //            Pause();
        //        }
        //        catch (Exception exc)
        //        {

        //        }

        //        Thread.Sleep(2000);//2초마다 한번씩
        //    }
        //}

        //스레드함수
        public void CommStart()
        {
            //스레드스타트
            CommFlag = true;
            //Comm = new Thread(CommMethod);
            Comm.Start();
            //스레드스타트
        }

        public void CommStop()
        {
            CommFlag = false;
        }

        /// <summary>
        /// 연결 상태유지 및 재 연결 시도.
        /// 통신은 중단.
        /// </summary>
        private void Pause()
        {
            try
            {
                Connected = false;
                CommStop();

                if (_stream != null)
                {
                    _stream.Close();
                }

                if (mClient != null)
                {
                    mClient.Close();
                }

                TalkingComm("DisConnected", Connected);

            }
            catch (Exception exc)
            {

            }

        }

        #endregion

    }


    public class TCPClient_LabelPrinter
    {
        LingerOption lingeroption = new LingerOption(true, 0);

        //---------------↓ 통신관련 ↓---------------┐
        string ServerIP = "";
        int ServerPort = 0;
        int ReceiveTimeOut = 0;
        string ClientIP = "";
        int ClientPort = 0;
        //---------------↑ 통신관련 ↑---------------┘

        public delegate void EveHandler(string name, object data);
        public event EveHandler TalkingComm;

        public bool Connected = false;
        public int PrinterStatus = 0;
        //프린터의 상태 0 = 출력된 라벨 없음
        //1 = 출력된 라벨 있음
        //2 = 에러

        public NetworkStream _stream = null;
        private TcpClient mClient;

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

        public void SendString(string str)
        {
            try
            {
                byte[] buff = Ken2.Communication.DataChange_K.StringToByteArr(str);
                _stream.Write(buff, 0, buff.Length);
            }
            catch (Exception)
            {

            }
        }

        public TCPClient_LabelPrinter(string ServerIP, int ServerPort, int ReceiveTimeOut)
        {

            this.ServerIP = ServerIP;
            this.ServerPort = ServerPort;
            this.ReceiveTimeOut = ReceiveTimeOut;

            ConnectStart(0);

        }

        public TCPClient_LabelPrinter(string ServerIP, int ServerPort, int ReceiveTimeOut, string ClientIP, int ClientPort)
        {

            this.ServerIP = ServerIP;
            this.ServerPort = ServerPort;
            this.ReceiveTimeOut = ReceiveTimeOut;
            this.ClientIP = ClientIP;
            this.ClientPort = ClientPort;

            ConnectStart(0);

        }

        #region -----# Connect #-----
        private Thread Connect;//스레드
        bool ConnectFlag = false;//Bool Flag
        //스레드함수
        private void ConnectMethod(object param)
        {
            int para = (int)param;

            while (true)
            {
                Thread.Sleep(1000);
                if (ConnectFlag == false)
                    break;

                try
                {

                    if (Connected == false)//연결끊어졌을때만 함
                    {

                        if (ClientPort == 0)
                        {
                            mClient = new TcpClient();
                            mClient.ReceiveTimeout = ReceiveTimeOut;
                            mClient.Connect(ServerIP, ServerPort);
                            _stream = mClient.GetStream();
                            Connected = true;

                            CommStart();//연결되었으니 통신스레드 시작함.
                        }
                        else
                        {
                            System.Net.IPAddress ip = System.Net.IPAddress.Parse(ClientIP);
                            IPEndPoint ipLocalEndPoint = new IPEndPoint(ip, 0);
                            mClient = new TcpClient(ipLocalEndPoint);

                            mClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, false);
                            mClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingeroption);
                            mClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 0);

                            mClient.ReceiveTimeout = ReceiveTimeOut;
                            mClient.Connect(ServerIP, ServerPort);
                            _stream = mClient.GetStream();
                            _stream.ReadTimeout = 1000;
                            Connected = true;

                            CommStart();//연결되었으니 통신스레드 시작함.

                        }


                        TalkingComm("Connected", Connected);
                    }



                }
                catch (Exception)
                {

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
            Connect.Abort();

            ConnectFlag = false;

        }
        #endregion

        /// <summary>
        /// 받은 데이터에서 상태에 해당하는 데이터만 추출해 옴
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public int ViewPrintStatus(string Data)
        {
            try
            {

                string[] split = Data.Split('\n');
                string[] buff0 = split[0].Split(',');
                string[] buff1 = split[1].Split(',');

                if (buff0[2] == "1")
                    return 2;
                if (buff1[7] == "1")
                    return 1;

            }
            catch (Exception)
            {


            }
            return 0;
        }

        #region -----# Comm #-----

        private Thread Comm;//스레드
        bool CommFlag = false;//Bool Flag

        private void CommMethod()
        {
            byte[] buff = new byte[1024];
            int length = 0;

            string status_cmd = "^XA^MMP~HS^XZ";//상태물어보기 명령어

            while (CommFlag)
            {
                try
                {
                    SendString(status_cmd);

                    while (_stream.DataAvailable)
                    {
                        length = _stream.Read(buff, 0, buff.Length);
                        string print_data = Encoding.ASCII.GetString(buff, 0, length);

                        int print_sta = ViewPrintStatus(print_data);

                        switch (print_sta)
                        {
                            case 0:
                                //없다.

                                PrinterStatus = 0;

                                break;
                            case 1:
                                //남아있다.

                                PrinterStatus = 1;

                                break;
                            case 2:
                                //에러났다.

                                PrinterStatus = 2;

                                break;
                        }

                    }

                }
                catch (System.IO.IOException)
                {
                    Pause();
                }
                catch (Exception exc)
                {

                }

                Thread.Sleep(2000);//2초마다 한번씩
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

        /// <summary>
        /// 연결 상태유지 및 재 연결 시도.
        /// 통신은 중단.
        /// </summary>
        private void Pause()
        {
            try
            {
                Connected = false;
                CommStop();

                if (_stream != null)
                {
                    _stream.Close();
                }

                if (mClient != null)
                {
                    mClient.Close();
                }


            }
            catch (Exception exc)
            {

            }

            TalkingComm("DisConnected", Connected);
        }

        #endregion

    }

    public class TCPClient_PLC1
    {
        string ServerIP = "";
        int ServerPort = 0;
        int ReceiveTimeOut = 0;
        LingerOption lingeroption = new LingerOption(true, 0);

        public delegate void EveHandler(string name, object data, int length);
        public event EveHandler TalkingComm;

        public bool Connected = false;
        NetworkStream _stream = null;
        private TcpClient mClient;
        Form1 mainform;

        string ClientIP = "";
        int ClientPort = 0;


        public TCPClient_PLC1(string ServerIP, int ServerPort, int ReceiveTimeOut, Form1 mainform)
        {

            this.ServerIP = ServerIP;
            this.ServerPort = ServerPort;
            this.ReceiveTimeOut = ReceiveTimeOut;
            this.mainform = mainform;
            ConnectStart();

        }

        public TCPClient_PLC1(string ServerIP, int ServerPort, int ReceiveTimeOut, string ClientIP, int ClientPort, Form1 mainform)
        {
            this.ServerIP = ServerIP;
            this.ServerPort = ServerPort;
            this.ReceiveTimeOut = ReceiveTimeOut;
            this.mainform = mainform;
            this.ClientIP = ClientIP;
            this.ClientPort = ClientPort;

            ConnectStart();
        }


        object tcplock = new object();



        public void MCWrite_Clear(int offset, int length)
        {

            lock (tcplock)
            {

                byte[] ReceiveData = new byte[1000];//데이터받음

                try
                {
                    _stream.Write(Ken2.Communication.MCProtocolCmd_K.Write_W_Clear(offset, length), 0, Ken2.Communication.MCProtocolCmd_K.Write_W_Clear(offset, length).Length);

                }
                catch (IOException)//데이터를전송할수가없어서 plc와 연결을 끊기. 연결이 끊어지면 계속 연결시도함.
                {
                    Pause();

                }

                try
                {
                    _stream.Read(ReceiveData, 0, ReceiveData.Length);//리시브데이터에 집어넣음
                    _stream.Flush();

                }
                catch (IOException)
                {

                }



            }
        }

        public void MCWriteString(int offset, string str)
        {
            lock (tcplock)
            {
                byte[] ReceiveData = new byte[100];//데이터받음

                try
                {
                    _stream.Write(Ken2.Communication.MCProtocolCmd_K.Write_W_reg(offset, str), 0, Ken2.Communication.MCProtocolCmd_K.Write_W_reg(offset, str).Length);
                }
                catch (IOException)//데이터를전송할수가없어서 plc와 연결을 끊기. 연결이 끊어지면 계속 연결시도함.
                {
                    Pause();
                }

                try
                {
                    _stream.Read(ReceiveData, 0, ReceiveData.Length);//리시브데이터에 집어넣음
                    _stream.Flush();

                }
                catch (IOException)
                {

                }
            }

        }

        object ReadLock = new object();

        public int[] MCRead_By_Offsets(int offset, int num)
        {
            lock (tcplock)
            {
                byte[] ReceiveData = new byte[2000];//데이터받음
                byte[] Command_Byte = Ken2.Communication.MCProtocolCmd_K.Read_Dreg(offset, num);
                try
                {
                    _stream.Write(Command_Byte, 0, Command_Byte.Length);
                }
                catch (IOException)//데이터를전송할수가없어서 plc와 연결을 끊기. 연결이 끊어지면 계속 연결시도함.
                {
                    Pause();
                }

                try
                {
                    _stream.Read(ReceiveData, 0, ReceiveData.Length);//리시브데이터에 집어넣음
                    _stream.Flush();
                }
                catch (IOException)
                {

                }

                return Ken2.Communication.MCProtocolCmd_K.View_MCData(ReceiveData);
            }
        }

        public byte[] MCRead(int offset, int num)
        {
            lock (tcplock)
            {
                byte[] ReceiveData = new byte[2000];//데이터받음
                byte[] Command_Byte = Ken2.Communication.MCProtocolCmd_K.Read_Dreg(offset, num);
                try
                {
                    _stream.Write(Command_Byte, 0, Command_Byte.Length);
                }
                catch (IOException)//데이터를전송할수가없어서 plc와 연결을 끊기. 연결이 끊어지면 계속 연결시도함.
                {
                    Pause();
                }

                try
                {
                    _stream.Read(ReceiveData, 0, ReceiveData.Length);//리시브데이터에 집어넣음
                    _stream.Flush();
                }
                catch (IOException)
                {

                }

                return Ken2.Communication.MCProtocolCmd_K.View_MCData_Byte(ReceiveData);
            }
        }

        public void MCWrite(int offset, int data)
        {

            lock (tcplock)
            {
                byte[] ReceiveData = new byte[2000];//데이터받음
                byte[] Command_Byte = Ken2.Communication.MCProtocolCmd_K.Write_Dreg(offset, data);


                try
                {
                    _stream.Write(Command_Byte, 0, Command_Byte.Length);

                }
                catch (IOException)//데이터를전송할수가없어서 plc와 연결을 끊기. 연결이 끊어지면 계속 연결시도함.
                {
                    Pause();

                }

                try
                {
                    _stream.Read(ReceiveData, 0, ReceiveData.Length);//리시브데이터에 집어넣음
                    _stream.Flush();

                }
                catch (IOException)
                {

                }



            }
        }

        int Start = 5000;

        int CalcByte(int Offset)
        {
            int result = Offset - Start;
            return result * 2;
        }

        string DecimalToBinary(int dec)
        {
            string s = Convert.ToString(dec, 2).PadLeft(16, '0');
            return s;
        }

        #region -----# Connect #-----
        private Thread Connect;
        bool ConnectFlag = false;//Bool Flag

        private void ConnectMethod()
        {
            while (ConnectFlag)
            {

                try
                {

                    if (Connected == false)//연결끊어졌을때만 함
                    {
                        if (ClientIP.Equals(""))
                        {
                            mClient = new TcpClient();
                            mClient.ReceiveTimeout = ReceiveTimeOut;
                            mClient.Connect(ServerIP, ServerPort);
                            _stream = mClient.GetStream();
                            Connected = true;

                            CommStart();//연결되었으니 통신스레드 시작함.
                        }
                        else
                        {
                            System.Net.IPAddress ip = System.Net.IPAddress.Parse(ClientIP);
                            IPEndPoint ipLocalEndPoint = new IPEndPoint(ip, 0);
                            mClient = new TcpClient(ipLocalEndPoint);


                            mClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, false);
                            mClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingeroption);
                            mClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 0);


                            mClient.ReceiveTimeout = ReceiveTimeOut;
                            mClient.Connect(ServerIP, ServerPort);
                            _stream = mClient.GetStream();
                            _stream.ReadTimeout = 1000;
                            Connected = true;

                            CommStart();//연결되었으니 통신스레드 시작함.

                        }

                        //TalkingComm( "ServerConnected" , Connected );
                    }

                }
                catch (Exception)
                {

                }

                Thread.Sleep(1000);

            }

        }
        //스레드함수
        public void ConnectStart()
        {
            //스레드스타트
            ConnectFlag = true;
            Connect = new Thread(ConnectMethod);
            Connect.Start();
            //스레드스타트
        }
        public void ConnectStop()
        {
            Connect.Abort();
            //스레드종료
            ConnectFlag = false;

            //스레드종료
        }
        #endregion

        #region -----# Comm #-----

        private Thread Comm;//스레드
        bool CommFlag = false;//Bool Flag

        double RoundUp(string d_value, int n_point)
        {
            double bf = double.Parse(d_value);
            double res = Math.Round(bf, n_point);

            return res;
        }

        string ByteToDecision(byte bt)
        {
            if (bt == 1)
                return "OK";
            else if (bt == 2)
                return "NG";
            else
                return "";
        }

        public static string PLCValue(string data, int word_num)
        {
            try
            {
                long buff = long.Parse(data);


                if (word_num == 1)
                {

                    if (buff > 32767)
                        buff = buff - 65536;


                    return buff.ToString();
                }
                else if (word_num == 2)
                {
                    long diff = 4294967296;

                    if (buff > 2147483647)
                        buff = buff - diff‬;

                    return buff.ToString();

                }

            }
            catch (Exception)
            {
                try
                {
                    if (data.Equals("OK") || data.Equals("NG"))
                        return data;
                }
                catch (Exception)
                {

                }
            }
            return "0";
        }

        public string DecimalPoint(string str, int point)
        {
            if (point < 0)
                return "0";

            int div = 10;

            for (int i = 0; i < point - 1; i++)
            {
                div *= 10;

            }

            string str_ = (double.Parse(str) / div).ToString("N" + point.ToString());

            return str_;
        }

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
        #region CommMethod

        //tttttttttttttttttttttttttttttttttttt
        private void CommMethod()
        {
            //PulseDetector Save1 = new PulseDetector();
            //PulseDetector Save2 = new PulseDetector();
            //PulseDetector Save3 = new PulseDetector();
            //PulseDetector LabelPrint = new PulseDetector();

            //PulseDetector BarcodeCheck = new PulseDetector();
            //PulseDetector BarcodeCheck2 = new PulseDetector();

            //PulseDetector Balance = new PulseDetector();
            //PulseDetector Balance2 = new PulseDetector();
            //PulseDetector Balance3 = new PulseDetector();


            //PulseDetector ManualPrint = new PulseDetector();

            //CountPlay flip = new CountPlay();

            //CountPlay quantity = new CountPlay();

            //byte[] buff = MCRead(2000, 200);//200개 바이트
            byte[] buff = new byte[4096]; ;//200개 바이트
            int length = 0;

            while (CommFlag)
            {
                Delay(300);
                try
                {
                    int[] commdata = MCRead_By_Offsets(2000, 500);//2000번지 300워드
                    //string input = BitConverter.ToString(buff, 0, length);
                    length = commdata.Length;
                    string[] result = new string[length];

                    for (int i = 0; i <length; i++)
                       result[i] = Convert.ToString(commdata[i]);

                    
                    if (mainform.Viewdatachk.Checked)
                    {
                        if (TalkingComm != null) TalkingComm("Data", result, length);
                    }


                }
                catch (Exception)
                {

                }

                //Thread.Sleep(200);

            }
        }

        #endregion

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
            //스레드종료
            CommFlag = false;

            //스레드종료
        }

        private void Pause()
        {
            try
            {
                Connected = false;

                if (_stream != null)
                {
                    _stream.Close();
                    _stream = null;
                }

                if (mClient != null)
                {
                    mClient.Close();
                    mClient = null;
                }

                CommStop();

            }
            catch (Exception)
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
        public void Disconnection()
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
