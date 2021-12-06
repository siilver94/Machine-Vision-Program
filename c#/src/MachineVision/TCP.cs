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


namespace ATnZ
{

    public class TCPServer_K
    {
        string IP = "";
        int port = 0;



        public TcpListener mServer;
        public TcpClient mClient;
        public NetworkStream _stream;

        public bool Connected = false;


        private delegate void dele();//delegate

        //이벤트 발생시키는 클래스에 선언
        public delegate void EveHandler(string name, object data, int length);
        public event EveHandler TalkingComm;

        public enum register
        {
            W_Register, D_Register
        }

        //이벤트 발생시키는 클래스에 선언
        public delegate void DataSendEvent(TCPServer_K.register reg, int offset, int value0, int value1);
        public event DataSendEvent DataSend;

        public byte[] D_offset;
        public byte[] W_offset;

        public TCPServer_K(string ip, int port)
        {
            this.IP = ip;
            this.port = port;

            mServer = new TcpListener(IPAddress.Parse(IP), port);
            mServer.Start();

            ListenThreadStart(0);

            D_offset = new byte[15000];
            W_offset = new byte[15000];
        }

        public void Pause()
        {
            ReceiveThreadStop();

            if (_stream != null)
            {
                _stream.Close();
            }

            if (mClient != null)
            {
                mClient.Close();
            }

            TalkingComm("DisConnected", 0, 0);
            Connected = false;
        }

        public void Disconnect()
        {
            ListenThreadStop();
            ReceiveThreadStop();
        }


        #region -----# ListenThread #-----
        private Thread ListenThread;//스레드
        bool ListenThreadFlag = false;//Bool Flag
        //스레드함수

        //tttttttttttttttttt
        private void ListenThreadMethod(object param)
        {
            int para = (int)param;


            while (true)
            {
                Thread.Sleep(100);
                if (ListenThreadFlag == false)
                    break;
                try
                {
                    mServer.BeginAcceptTcpClient(HandleAsyncConnection, mServer);
                    //여기서 무한정기다리지않고 번호표뽑고 연결할라고 대기하는놈들 몇마리인지
                    //확인 후 있으면 HandleAsyncConnection 콜백함수 호출해줌.
                    //없으면 넘어가기(비동기)
                    //AcceptTcpClient = 동기 신호로 무한정기다리면서 연결하는놈

                }
                catch (Exception)
                {

                }


            }

            try
            {
                mServer.Stop();
                mClient.Close();
                _stream.Close();
            }
            catch (Exception)
            {

            }

        }
        //스레드함수
        public void ListenThreadStart(int param)
        {
            //스레드스타트
            ListenThreadFlag = true;
            ListenThread = new Thread((new ParameterizedThreadStart(ListenThreadMethod)));
            ListenThread.Start(param);

            //스레드스타트
        }
        public void ListenThreadStop()
        {
            //스레드종료
            ListenThreadFlag = false;

        }
        #endregion


        private void HandleAsyncConnection(IAsyncResult res)
        {
            try
            {
                mClient = mServer.EndAcceptTcpClient(res);
                _stream = mClient.GetStream();
                _stream.ReadTimeout = 1000;

                ReceiveThreadStart(0);

            }
            catch
            {

            }
        }


        #region -----# ReceiveThread #-----
        //스레드변수 (스레드구성요소 3개)
        //[ FLAG ] [ METHOD ] [ THREAD ]
        private Thread ReceiveThread;//스레드
        bool ReceiveThreadFlag = false;//Bool Flag
        //스레드함수
        //ttttttttttttttttttttttttt
        private void ReceiveThreadMethod(object param)
        {
            byte[] buff = new byte[4096];
            int length = 0;

            int para = (int)param;
            TalkingComm("Connected", 0, 0);
            Connected = true;

            while (true)
            {
                //Thread.Sleep( 200 );
                if (ReceiveThreadFlag == false)
                    break;
                try
                {
                    length = _stream.Read(buff, 0, buff.Length);

                    if (length == 0)
                    {
                        Pause();
                        break;
                    }

                    if (TalkingComm != null) TalkingComm("Data", buff, length);


                    if (buff[0] == 0x50)//MC헤더
                    {
                        if (buff[12] == 0x14)//쓰기
                        {
                            if (buff[18] == 0xB4)//W레지
                            {
                                int len = buff[19];

                                int offset = buff[16] * 256 + buff[15];
                                //뒤엣놈이 높은자리의수 256자리 오히려 앞엣놈이 1의자리수

                                for (int i = 0; i < len; i++)
                                {
                                    int offset_byte = (offset * 2) + (i * 2);
                                    int order = 21 + (i * 2);

                                    W_offset[offset_byte] = buff[order];
                                    W_offset[offset_byte + 1] = buff[order + 1];

                                    if (DataSend != null)
                                        DataSend(TCPServer_K.register.W_Register, offset + i, W_offset[offset_byte], W_offset[offset_byte + 1]);
                                }

                                byte[] data = new byte[13];

                                data[0] = 0xD0;
                                data[1] = 0x00;
                                data[2] = 0x00;
                                data[3] = 0xFF;
                                data[4] = 0xFF;
                                data[5] = 0x03;
                                data[6] = 0x00;
                                data[7] = 0x04;
                                data[8] = 0x00;
                                data[9] = 0x00;
                                data[10] = 0x00;
                                data[11] = 0x00;
                                data[12] = 0x00;

                                _stream.Write(data, 0, data.Length);
                            }

                            if (buff[18] == 0xA8)//D레지
                            {
                                int len = buff[19];

                                int offset = buff[16] * 256 + buff[15];
                                //뒤엣놈이 높은자리의수 256자리 오히려 앞엣놈이 1의자리수

                                for (int i = 0; i < len; i++)
                                {
                                    int offset_byte = (offset * 2) + (i * 2);
                                    int order = 21 + (i * 2);

                                    D_offset[offset_byte] = buff[order];
                                    D_offset[offset_byte + 1] = buff[order + 1];

                                    if (DataSend != null)
                                        DataSend(TCPServer_K.register.D_Register, offset + i, D_offset[offset_byte], D_offset[offset_byte + 1]);
                                }


                                byte[] data = new byte[13];

                                data[0] = 0xD0;
                                data[1] = 0x00;
                                data[2] = 0x00;
                                data[3] = 0xFF;
                                data[4] = 0xFF;
                                data[5] = 0x03;
                                data[6] = 0x00;
                                data[7] = 0x04;
                                data[8] = 0x00;
                                data[9] = 0x00;
                                data[10] = 0x00;
                                data[11] = 0x00;
                                data[12] = 0x00;

                                _stream.Write(data, 0, data.Length);
                            }


                        }

                        if (buff[12] == 0x04)//읽기
                        {

                            if (buff[18] == 0xB4)//W레지
                            {
                                int offset = buff[16] * 256 + buff[15];

                                int len = buff[19];
                                int quantity = len * 2 + 11;
                                byte[] data = new byte[quantity];

                                data[0] = 0xD0;
                                data[1] = 0x00;
                                data[2] = 0x00;
                                data[3] = 0xFF;
                                data[4] = 0xFF;

                                data[5] = 0x03;
                                data[6] = 0x00;
                                data[7] = (byte)(2 + len * 2);
                                data[8] = 0x00;
                                data[9] = 0x00;
                                data[10] = 0x00;

                                for (int i = 0; i < len; i++)
                                {
                                    data[11 + (i * 2)] = W_offset[(offset * 2) + (i * 2)];
                                    data[11 + (i * 2) + 1] = W_offset[(offset * 2) + (i * 2) + 1];
                                }

                                _stream.Write(data, 0, data.Length);

                            }

                            if (buff[18] == 0xA8)//D레지
                            {

                                int offset = buff[16] * 256 + buff[15];

                                int len = buff[19];
                                int quantity = len * 2 + 11;
                                byte[] data = new byte[quantity];

                                data[0] = 0xD0;
                                data[1] = 0x00;
                                data[2] = 0x00;
                                data[3] = 0xFF;
                                data[4] = 0xFF;

                                data[5] = 0x03;
                                data[6] = 0x00;
                                data[7] = (byte)(2 + len * 2);
                                data[8] = 0x00;
                                data[9] = 0x00;
                                data[10] = 0x00;

                                for (int i = 0; i < len; i++)
                                {
                                    data[11 + (i * 2)] = D_offset[(offset * 2) + (i * 2)];
                                    data[11 + (i * 2) + 1] = D_offset[(offset * 2) + (i * 2) + 1];
                                }

                                _stream.Write(data, 0, data.Length);

                            }


                        }

                    }

                }
                catch (Exception)
                {

                }
            }
        }
        //스레드함수
        public void ReceiveThreadStart(int param)
        {
            //스레드스타트
            ReceiveThreadFlag = true;
            ReceiveThread = new Thread((new ParameterizedThreadStart(ReceiveThreadMethod)));
            ReceiveThread.Start(param);

        }
        public void ReceiveThreadStop()
        {
            //스레드종료
            ReceiveThreadFlag = false;
            //ReceiveThread = null;

        }
        #endregion


        public void SendString(string str)
        {
            byte[] buff = DataChange_K.StringToByteArr(str);

            try
            {
                _stream.Write(buff, 0, buff.Length);
            }
            catch (Exception)
            {
                Pause();
            }
        }

        public void Send(string str)
        {

            try
            {
                string SendData = Parsing.DeleteSpace(str);

                //int SendDataLength = textBox2.TextLength;//4

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

            }

        }
    }

    public class TCPClient_K
    {

        LingerOption lingeroption = new LingerOption(true, 0);

        string ServerIP = "";
        int ServerPort = 0;
        int ReceiveTimeOut = 0;

        string ClientIP = "";
        int ClientPort = 0;


        public delegate void EveHandler(string name, object data, int length);
        public event EveHandler TalkingComm;

        public bool Connected = false;


        public bool Server_Connected = false;
        public NetworkStream _stream = null;
        private TcpClient mClient;




        public TCPClient_K(string ServerIP, int ServerPort, int ReceiveTimeOut)
        {

            this.ServerIP = ServerIP;
            this.ServerPort = ServerPort;
            this.ReceiveTimeOut = ReceiveTimeOut;


        }

        public TCPClient_K(string ServerIP, int ServerPort, int ReceiveTimeOut, string ClientIP, int ClientPort)
        {

            this.ServerIP = ServerIP;
            this.ServerPort = ServerPort;
            this.ReceiveTimeOut = ReceiveTimeOut;
            this.ClientIP = ClientIP;
            this.ClientPort = ClientPort;

        }

        object tcplock = new object();


        public void MCWrite(int offset, int data)
        {

            lock (tcplock)
            {

                byte[] ReceiveData = new byte[1000];//데이터받음

                try
                {
                    if (_stream != null)
                    {
                        _stream.Write(Ken2.Communication.MCProtocolCmd_K.Write_W_reg(offset, data), 0, Ken2.Communication.MCProtocolCmd_K.Write_W_reg(offset, data).Length);
                    }
                }

                catch (IOException)//데이터를전송할수가없어서 plc와 연결을 끊기. 연결이 끊어지면 계속 연결시도함.
                {
                    Pause();
                    System.Windows.Forms.MessageBox.Show("연결을 확인하세요");

                }

                try
                {
                    //_stream.Read( ReceiveData , 0 , ReceiveData.Length );//리시브데이터에 집어넣음
                    //_stream.Flush( );

                }
                catch (IOException)
                {

                }



            }
        }

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
                    //_stream.Read( ReceiveData , 0 , ReceiveData.Length );//리시브데이터에 집어넣음
                    //_stream.Flush( );

                }
                catch (IOException)
                {

                }
            }

        }

        public void MCWrite_D(int offset, int data)
        {

            lock (tcplock)
            {

                byte[] ReceiveData = new byte[1000];//데이터받음

                try
                {
                    _stream.Write(Ken2.Communication.MCProtocolCmd_K.Write_Dreg(offset, data), 0, Ken2.Communication.MCProtocolCmd_K.Write_Dreg(offset, data).Length);

                }
                catch (IOException)//데이터를전송할수가없어서 plc와 연결을 끊기. 연결이 끊어지면 계속 연결시도함.
                {
                    Pause();

                }

                try
                {
                    //_stream.Read( ReceiveData , 0 , ReceiveData.Length );//리시브데이터에 집어넣음
                    //_stream.Flush( );

                }
                catch (IOException)
                {

                }



            }
        }

        public void MCWriteString_D(int offset, string str)
        {
            lock (tcplock)
            {
                byte[] ReceiveData = new byte[100];//데이터받음

                try
                {
                    _stream.Write(Ken2.Communication.MCProtocolCmd_K.Write_Dreg(offset, str), 0, Ken2.Communication.MCProtocolCmd_K.Write_Dreg(offset, str).Length);
                }
                catch (IOException)//데이터를전송할수가없어서 plc와 연결을 끊기. 연결이 끊어지면 계속 연결시도함.
                {
                    Pause();
                }

                try
                {
                    //_stream.Read( ReceiveData , 0 , ReceiveData.Length );//리시브데이터에 집어넣음
                    //_stream.Flush( );

                }
                catch (IOException)
                {

                }
            }

        }

        public void SendString(string str)
        {
            byte[] buff = DataChange_K.StringToByteArr(str);

            try
            {
                _stream.Write(buff, 0, buff.Length);
            }
            catch (Exception)
            {
                Pause();
            }
        }

        public void Send(string str)
        {

            try
            {
                string SendData = Parsing.DeleteSpace(str);

                //int SendDataLength = textBox2.TextLength;//4

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
                Thread.Sleep(1000);
                if (ConnectFlag == false)
                    break;

                try
                {

                    if (Server_Connected == false)//연결끊어졌을때만 함
                    {

                        if (ClientPort == 0)
                        {
                            mClient = new TcpClient();
                            mClient.ReceiveTimeout = ReceiveTimeOut;
                            mClient.Connect(ServerIP, ServerPort);
                            _stream = mClient.GetStream();
                            Server_Connected = true;



                            CommStart();//연결되었으니 통신스레드 시작함.
                        }
                        else
                        {

                            //System.Net.IPAddress ip = System.Net.IPAddress.Parse( ClientIP );
                            //IPEndPoint ipLocalEndPoint = new IPEndPoint( ip , ClientPort );
                            //mClient = new TcpClient( ipLocalEndPoint );

                            //mClient.Client.SetSocketOption( SocketOptionLevel.Socket , SocketOptionName.DontLinger , false );
                            //mClient.Client.SetSocketOption( SocketOptionLevel.Socket , SocketOptionName.Linger , lingeroption );
                            //mClient.Client.SetSocketOption( SocketOptionLevel.Socket , SocketOptionName.KeepAlive , 0 );

                            mClient.ReceiveTimeout = ReceiveTimeOut;
                            mClient.Connect(ServerIP, ServerPort);
                            _stream = mClient.GetStream();
                            _stream.ReadTimeout = 1000;
                            Server_Connected = true;

                            CommStart();//연결되었으니 통신스레드 시작함.

                        }


                        TalkingComm("Connected", 0, 0);
                        Connected = true;
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

            ConnectFlag = false;

        }
        #endregion



        #region -----# Comm #-----

        private Thread Comm;//스레드
        bool CommFlag = false;//Bool Flag

        //tttttttttttttttttttttttttttttttttt
        private void CommMethod()
        {
            byte[] buff = new byte[4096];
            int length = 0;

            while (true)
            {

                if (CommFlag == false)
                    break;
                try
                {

                    length = _stream.Read(buff, 0, buff.Length);

                    if (length == 0)
                    {
                        Pause();
                        break;
                    }

                    //_stream.Write( buff, 0, buff.Length );
                    //string str = Encoding.ASCII.GetString( buff , 0 , length );

                    if (TalkingComm != null) TalkingComm("Data", buff, length);
                    //TalkingComm( "2" , str );

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
            Connected = false;
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
        #endregion

    }

    public class Network_K
    {

        public static bool IsDhcp(string mac)
        {
            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();
            string result = "";
            foreach (ManagementObject objMO in objMOC)
            {
                if (objMO["MACAddress"] != null && objMO["MACAddress"].Equals(mac))
                {
                    //MessageBox.Show("a");

                    //ManagementBaseObject setIP;
                    //ManagementBaseObject newIP =
                    //objMO.GetMethodParameters("EnableStatic");
                    //result = objMO.Properties["DHCPEnabled"];
                    result = objMO["DHCPEnabled"].ToString();
                    //MessageBox.Show(a.ToString());
                    //newIP["SubnetMask"] = new string[] { subnet_mask };
                    //setIP = objMO.InvokeMethod("EnableStatic", newIP, null);


                }
            }

            if (result.Equals("True"))
            {
                return true;
            }
            else
                return false;

        }


        public static string GetIP(string mac)
        {
            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();
            string result = "";
            foreach (ManagementObject objMO in objMOC)
            {
                if (objMO["MACAddress"] != null && objMO["MACAddress"].Equals(mac) && objMO["IPAddress"] != null)
                {

                    //ManagementBaseObject setIP;
                    //ManagementBaseObject newIP =
                    //objMO.GetMethodParameters("EnableStatic");

                    result = ((string[])(objMO["IPAddress"]))[0];



                }
            }
            return result;

        }

        public static string GetGateway(string mac)
        {

            NetworkInterface[] Interfaces = NetworkInterface.GetAllNetworkInterfaces();
            string result = "";

            foreach (NetworkInterface Interface in Interfaces)
            {
                if (Interface.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;
                string add = Interface.GetPhysicalAddress().ToString();
                string _mac = add.Substring(0, 2) + ":" + add.Substring(2, 2) + ":" + add.Substring(4, 2) + ":" + add.Substring(6, 2) + ":" + add.Substring(8, 2) + ":" + add.Substring(10, 2);
                //string SubnetMask = "";
                //string Gateway = "";


                if (mac.Equals(_mac))
                //if (true)
                {

                    IPInterfaceProperties adapterProperties = Interface.GetIPProperties();

                    UnicastIPAddressInformationCollection uipis = adapterProperties.UnicastAddresses; //IP와 SubnetMask에 대한 정보를 가짐
                    GatewayIPAddressInformationCollection gates = adapterProperties.GatewayAddresses; //Gateway 정보를 가짐
                    IPAddressCollection dnsServers = adapterProperties.DnsAddresses; //DNS Server 정보를 가짐


                    if (gates.Count > 0)
                    {
                        foreach (GatewayIPAddressInformation gate in gates)
                        {

                            if (gate.Address.ToString().Substring(0, 1).Equals("0"))
                                result = "error";
                            else
                                result = gate.Address.ToString();
                        }
                    }

                }

            }

            return result;



        }



        public static string GetSubnetmask(string mac)
        {

            NetworkInterface[] Interfaces = NetworkInterface.GetAllNetworkInterfaces();
            string result = "";

            foreach (NetworkInterface Interface in Interfaces)
            {
                if (Interface.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;
                string add = Interface.GetPhysicalAddress().ToString();
                string _mac = add.Substring(0, 2) + ":" + add.Substring(2, 2) + ":" + add.Substring(4, 2) + ":" + add.Substring(6, 2) + ":" + add.Substring(8, 2) + ":" + add.Substring(10, 2);
                //string SubnetMask = "";
                //string Gateway = "";


                if (mac.Equals(_mac))
                //if (true)
                {

                    IPInterfaceProperties adapterProperties = Interface.GetIPProperties();

                    UnicastIPAddressInformationCollection uipis = adapterProperties.UnicastAddresses; //IP와 SubnetMask에 대한 정보를 가짐
                    GatewayIPAddressInformationCollection gates = adapterProperties.GatewayAddresses; //Gateway 정보를 가짐
                    IPAddressCollection dnsServers = adapterProperties.DnsAddresses; //DNS Server 정보를 가짐


                    if (uipis.Count > 0)
                    {
                        foreach (UnicastIPAddressInformation uipi in uipis)
                        {
                            if (uipi.IPv4Mask.ToString().Substring(0, 1).Equals("0"))
                                result = "error";
                            else
                                result = uipi.IPv4Mask.ToString();
                        }
                    }

                }

            }

            return result;



        }

        public static bool SetIP(string mac, string ip, string subnet_mask)
        {
            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();

            foreach (ManagementObject objMO in objMOC)
            {
                if (objMO["MACAddress"] != null && objMO["MACAddress"].Equals(mac))
                {

                    try
                    {
                        ManagementBaseObject setIP;
                        ManagementBaseObject newIP =
                            objMO.GetMethodParameters("EnableStatic");

                        newIP["IPAddress"] = null;//널로 먼저넣으면 아이피하나만됨
                        newIP["SubnetMask"] = null;

                        setIP = objMO.InvokeMethod("EnableStatic", newIP, null);

                        newIP["IPAddress"] = new string[] { ip };
                        newIP["SubnetMask"] = new string[] { subnet_mask };

                        setIP = objMO.InvokeMethod("EnableStatic", newIP, null);

                        return true;
                    }
                    catch (Exception)
                    {
                        return false;

                    }


                }
            }
            return false;
        }

        public static void SetDHCP(string mac)
        {
            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();

            foreach (var o in objMOC)
            {
                var mo = (ManagementObject)o;
                if ((o["MACAddress"] != null && o["MACAddress"].Equals(mac)))
                {
                    var ndns = mo.GetMethodParameters("SetDNSServerSearchOrder");
                    ndns["DNSServerSearchOrder"] = null;
                    var setDns = mo.InvokeMethod("SetDNSServerSearchOrder", ndns, null);
                    var enableDhcp = mo.InvokeMethod("EnableDHCP", null);
                    //mo.InvokeMethod("ReleaseDHCPLease", null);
                    //mo.InvokeMethod("RenewDHCPLease", null);
                }
            }
        }

        public static void SetDHCP2(string AdapterName)
        {
            CmdStart("netsh interface ip set address " + AdapterName + " dhcp", true);
        }

        public static void CmdStart(string command, bool UnVisible)
        {//using System.Diagnostics;

            System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + command);


            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;


            procStartInfo.CreateNoWindow = UnVisible; // Do not create the black window.

            // Now we create a process, assign its ProcessStartInfo and start it
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo = procStartInfo;
            proc.Start();
        }


        public static void SetGateway(string mac, string gateway)
        {
            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();

            foreach (ManagementObject objMO in objMOC)
            {
                if (objMO["MACAddress"] != null && objMO["MACAddress"].Equals(mac))
                {
                    ManagementBaseObject setGateway;
                    ManagementBaseObject newGateway =
                        objMO.GetMethodParameters("SetGateways");

                    newGateway["DefaultIPGateway"] = new string[] { gateway };
                    newGateway["GatewayCostMetric"] = new int[] { 1 };

                    setGateway = objMO.InvokeMethod("SetGateways", newGateway, null);

                }
            }
        }


        public static string SimplePing(string IP)
        {
            string result = "";
            Ping pingSender = new Ping();
            PingReply reply = pingSender.Send(IP);


            if (reply.Status == IPStatus.Success) //핑이 제대로 들어가고 있을 경우
            {



                result += "-- OK --" + Environment.NewLine + Environment.NewLine;
                result += "Address: " + reply.Address.ToString() + Environment.NewLine;
                result += "RoundTrip time: " + reply.RoundtripTime.ToString() + Environment.NewLine;
                result += "Time to live: " + reply.Options.Ttl.ToString() + Environment.NewLine;
                result += "RoundTrip time: " + reply.RoundtripTime.ToString() + Environment.NewLine;
                result += "Don't fragment: " + reply.Options.DontFragment.ToString() + Environment.NewLine;
                result += "Buffer size: " + reply.Buffer.Length.ToString();
            }
            else //핑이 제대로 들어가지 않고 있을 경우 
            {
                result += "-- NG --" + Environment.NewLine + Environment.NewLine;
                result += "Status :" + reply.Status.ToString();


            }

            return result;
        }


    }
}
