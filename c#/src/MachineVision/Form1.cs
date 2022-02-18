using Cognex.VisionPro;
using Cognex.VisionPro.ImageProcessing;
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro.ToolGroup;
using Ken2.Database;
using Ken2.DataManagement;
using Ken2.Util;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;


namespace MainProgram
{
    public partial class Form1 : Form
    {
        private delegate void dele();

        Ken2.UIControl.dgvManager dgvmanager;

        PylonBasler cam1,cam2 = null;
  
        public MasterK200_1 plc1;
        
        Mysql_K sql;

        public int CurrentModelNum1 = 1;
        public int CurrentModelNum2 = 1;

        int CamPoint1 = 0;
        int CamPoint2 = 0;

        CogToolGroup Cogtg;                     //  툴그룹 가져오는 변수
        CogToolGroup Cogtg2;                     //  툴그룹 가져오는 변수

        int[] resultdata = new int[100]; //  비전 결과 데이터 가져오기

        int[] min = new int[20];    //  최소값 배열
        int[] max = new int[20];    //  최대값 배열
        int checksetting = 17;    //  검사 데이터 및 min max 배열 수량

        int[] min2 = new int[20];    //  최소값 배열
        int[] max2 = new int[20];    //  최대값 배열
        int checksetting2 = 17;    //  검사 데이터 및 min max 배열 수량

        int totalcnt = 0;
        int okcnt = 0;
        int ngcnt = 0;
        
        int okcnt2 = 0;
        int ngcnt2 = 0;
        int totalcnt2 = 0;

        string Decision1 = "";
        string Decision2 = "";

        //Bitmap image1 = null;
        Bitmap image1 = null;
        Bitmap image2 = null;
        bool LiveFlag = false;
        bool LiveFlag2 = false;

        //bool LiveFlag = false;

        string Mainpath = "Vision";


        private object lockObj = new object();

        private static DateTime Delay( int MS )
        {
            DateTime ThisMoment = DateTime.Now;
            TimeSpan duration = new TimeSpan( 0, 0, 0, 0, MS );
            DateTime AfterWards = ThisMoment.Add( duration );
            while ( AfterWards >= ThisMoment )
            {
                System.Windows.Forms.Application.DoEvents( );
                ThisMoment = DateTime.Now;
            }
            return DateTime.Now;
        }

        public Form1()
        {
            InitializeComponent();
        }

        public void SaveTxt()
        {
            ControlData.Save(Txt_DeleteDay); 
            ControlData.Save(Txt_LastModel1);
            ControlData.Save(textBox_ok);
            ControlData.Save(textBox_ng);
            ControlData.Save(textBox_total);
            ControlData.Save(checkBox_ModelChangeManual1);
            ControlData.Save(check_OKImage1);
            ControlData.Save(check_NGImage1);

            ControlData.Save(Txt_LastModel2);
            ControlData.Save(textBox_ok2);
            ControlData.Save(textBox_ng2);
            ControlData.Save(textBox_total2);
            ControlData.Save(checkBox_ModelChangeManual2);
            ControlData.Save(check_OKImage2);
            ControlData.Save(check_NGImage2);

        }

        public void LoadTxt()
        {
            ControlData.Load(Txt_DeleteDay);
            ControlData.Load(Txt_LastModel1);
            ControlData.Load(textBox_ok);
            ControlData.Load(textBox_ng);
            ControlData.Load(textBox_total);
            ControlData.Load(checkBox_ModelChangeManual1);
            ControlData.Load(check_OKImage1);
            ControlData.Load(check_NGImage1);

            ControlData.Load(Txt_LastModel2);
            ControlData.Load(textBox_ok2);
            ControlData.Load(textBox_ng2);
            ControlData.Load(textBox_total2);
            ControlData.Load(checkBox_ModelChangeManual2);
            ControlData.Load(check_OKImage2);
            ControlData.Load(check_NGImage2);

        }

 

        //ffffffffffffffffff
        private void Form1_Load(object sender, EventArgs e)
        {

            //Delay(2000);

            this.Location = new Point(0, 0);
            xtraTabControl_Model.ShowTabHeader = DevExpress.Utils.DefaultBoolean.False;
            //탭닫기
            title_kenlb.Controls.Add(title_lbc);
            title_kenlb.Controls.Add(title_piced);
            //제목 투명효과
           int port = Int32.Parse(Txt_Port.Text);

            plc1 = new MasterK200_1("192.168.0.1", 2004, 1000, "192.168.0.20", 0, this);

            plc1.TalkingComm += Plc1_TalkingComm;

            Delay(300);

            plc1.ConnectStart(100);

            LoadTxt();

            okcnt = int.Parse(textBox_ok.Text);
            ngcnt = int.Parse(textBox_ng.Text);
            totalcnt = int.Parse(textBox_total.Text);

            okcnt2 = int.Parse(textBox_ok2.Text);
            ngcnt2 = int.Parse(textBox_ng2.Text);
            totalcnt2 = int.Parse(textBox_total2.Text);

            dgvInit("dgvD1");       //  검사 결과
            dgvInit("dgvD2");       //  검사 결과
            dgvInit("dgvStatus1");  //  연결 상태
            dgvInit("dgvStatus2");  //  연결 상태
            dgvInit("dgvM1");       //  모델
            dgvInit("dgvM2");       //  모델
            dgvInit("dgvCam1");     //  카메라1 
            dgvInit("dgvCam2");     //  카메라1 밝기
            dgvInit("dgvS1");       //  세팅(설정값)
            dgvInit("dgvS2");       //  세팅(설정값)
            dgvInit("dgvC1");       //  PLC통신
            dgvInit("dgvH0");       //  DB 이력

            dgvInit("dgvH1");       //  판정 그리드뷰
            dgvInit("dgvH2");       //  판정 그리드뷰
            dgvInit("dgvH3");       //  판정 그리드뷰
            dgvInit("dgvH4");       //  판정 그리드뷰
            dgvInit("dgvH5");       //  판정 그리드뷰
            dgvInit("dgvH6");       //  판정 그리드뷰
            dgvInit("dgvH7");       //  판정 그리드뷰
            dgvInit("dgvH8");       //  판정 그리드뷰


            cam1 = new PylonBasler("192.168.100.2");
            cam1.ImageSignal += cam1_ImageSignal;
            cam1.CommSignal += Cam1_CommSignal1;

            cam2 = new PylonBasler("192.168.101.2");
            cam2.ImageSignal += Cam2_ImageSignal;
            cam2.CommSignal += Cam2_CommSignal;

            sql = new Mysql_K("127.0.0.1", "Seojin_001", "table1", "a", "qwerasdf");

            StartmainThread(0);

            SetToday();

            Directory.CreateDirectory("D:\\" + Mainpath + "\\Log");
            Directory.CreateDirectory("D:\\" + Mainpath + "\\Image");
            Log_K.WriteLog(log_lst, Mainpath, "프로그램 시작");

            modelOpen1(CurrentModelNum1);
            //modelOpen2(CurrentModelNum2);

            autoRun();
        }

        public byte[] HexStringToByteHex(string strHex)
        {
            if (strHex.Length % 2 != 0)
                MessageBox.Show("HexString는 홀수일 수 없습니다. - " + strHex);

            byte[] bytes = new byte[strHex.Length / 2];

            for (int count = 0; count < strHex.Length; count += 2)
            {
                bytes[count / 2] = System.Convert.ToByte(strHex.Substring(count, 2), 16);
            }
            return bytes;
        }

        public string ConvertHexToString(string HexValue)
        {
            string StrValue = "";
            while (HexValue.Length > 0)
            {
                StrValue += System.Convert.ToChar(System.Convert.ToUInt32(HexValue.Substring(0, 2), 16)).ToString();
                HexValue = HexValue.Substring(2, HexValue.Length - 2);
            }

            return StrValue;
        }

        int start = 1;
        int end = 0;
        int start2 = 1;
        int end2 = 0;

        private void Plc1_TalkingComm(string name, object data, int length)
        {
            if (name.Equals("Connected"))
            {
                this.Invoke(new dele(() =>
                {
                    kenLabel4.GradientBottom = Color.Lime;
                    dgvStatus1.Rows[1].Cells[0].Style.BackColor = Color.Lime;
                    dgvStatus2.Rows[1].Cells[0].Style.BackColor = Color.Lime;
                    Log_K.WriteLog(log_lst, "Log", "plc1 연결 성공");
                    
                }));
            }

            if (name.Equals("DisConnected"))
            {
                this.Invoke(new dele(() =>
                {
                    kenLabel4.GradientBottom = Color.Red;
                    dgvStatus1.Rows[1].Cells[0].Style.BackColor = Color.Crimson;
                    dgvStatus2.Rows[1].Cells[0].Style.BackColor = Color.Crimson;
                    Log_K.WriteLog(log_lst, "Log", "plc1 연결 해제");
                }));
            }

            if (name.Equals("Trigger1"))
            {
                CamPoint1 = Convert.ToInt32(data);
                this.Invoke(new dele(() =>
                {
                    Log_K.WriteLog(log_lst, Mainpath, "자동 검사1 -> " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff"));
                }));
                try
                {
                    cam1.OneShot();
                }
                catch (Exception)
                {
                    Log_K.WriteLog(log_lst, Mainpath, "카메라1 트리거 에러");
                }
            }

            if (name.Equals("Data"))
            {
               
                    string[] AllData = (string[])data;
                    string[] indata = new string[600];
                    string[] address = new string[300];

                    try
                    {
                        
                        for (int i = 0; i < 600; i++)
                        {
                            indata[i] = AllData[32 + i];
                        }

                        
                        for (int j = 0; j < 300; j++)   //  address [] 배열에 스타트 번지부터 값 300개 넣기
                        {
                            address[j] = indata[start] + indata[end];
                            start += 2;
                            end += 2;
                        }
                    }
                    catch (Exception)
                    {
                        this.Invoke(new dele(() =>
                        {
                            Log_K.WriteLog(log_lst, Mainpath, "plc 전체 데이터가져오기 에러");
                        }));
                    }

                    start = 1;
                    end = 0;

                    try
                    {
                        this.Invoke(new dele(() =>
                        {
                            
                            for (int i = 0; i < 300; i++)
                            {
                                dgvC1.Rows[i].Cells[1].Value = address[i]; //  C0 16진수

                                int val10 = Convert.ToInt32(address[i], 16);   //  C0 10진수
                                dgvC1.Rows[i].Cells[2].Value = val10;
                            }
                        }));

                    }
                    catch (Exception)
                    {
                        this.Invoke(new dele(() =>
                        {
                            Log_K.WriteLog(log_lst, Mainpath, "PLC_C1 데이터 넣기 에러");
                        }));
                    }

                
            }

            if (name.Equals("Trigger1"))   //  트리거 On
            {
                Delay(100);

                this.Invoke(new dele(() =>
                {
                    Log_K.WriteLog(log_lst, Mainpath, "[Cam1] Triger1" + Environment.NewLine);
                   
                }));

                cam1.OneShot();

                
                Delay(1000);
              
                plc1.MasterK_Write_W("3230303132", "0100"); //  검사 완료
                
                

            }
            
            if (name.Equals("Trigger2"))   //  트리거 On
            {
               

                this.Invoke(new dele(() =>
                {
                    Log_K.WriteLog(log_lst, Mainpath, "[Cam2] Triger2" + Environment.NewLine);
                    
                }));

                cam2.OneShot();

                

                start = 0;
                end = 1;

                string[] AllData = (string[])data;          
                string[] indata = new string[600];
                string[] address = new string[300];

                try
                {
                    
                    for (int i = 0; i < 600; i++)
                    {
                        indata[i] = AllData[32 + i];
                    }

                    
                    for (int j = 0; j < 300; j++)   //  address [] 배열에 스타트 번지부터 값 300개 넣기
                    {
                        address[j] = indata[start] + indata[end];
                        start += 2;
                        end += 2;
                    }
                }
                catch (Exception)
                {
                    Log_K.WriteLog(log_lst, Mainpath, "Trigger2 에러");
                }

                string bcrr = "";

                for (int i = 16; i < 21; i++)    //  바코드 길이 문자가져오기
                {
                    Console.WriteLine(ConvertHexToString(address[i]));
                    bcrr += ConvertHexToString(address[i]);
                }

                start2 = 1;
                end2 = 0;

              
                string[] address2 = new string[300];

                try
                {
                    
                    for (int i = 0; i < 600; i++)
                    {
                        indata[i] = AllData[32 + i];
                    }


                    for (int j = 0; j < 300; j++)   //  address [] 배열에 스타트 번지부터 값 200개 넣기
                    {
                        address2[j] = indata[start2] + indata[end2];
                        start2 += 2;
                        end2 += 2;
                    }
                }
                catch (Exception)
                {
                    Log_K.WriteLog(log_lst, Mainpath, "Trigger2 data input 에러");
                }

               
                int[] IntData = new int[300];   //  10진수 가져오기 IntData 안에 10진수 데이터 있음
               
                for (int i = 0; i < 300; i++)
                {
                    IntData[i] = Convert.ToInt32(address2[i], 16);
                }

                
                string[] plcdata = new string[300];
               
                for (int i = 0; i < 300; i++)
                {
                    if (IntData[i] <= 32768)
                    {
                        plcdata[i] = IntData[i].ToString();
                    }
                    else
                    {
                        int temp = IntData[i] - 65536;
                        plcdata[i] = temp.ToString();
                    }
                }

               

                try
                {
                    int rows = sql.ExecuteQuery_Select_Count("SELECT COUNT(*) FROM table1 WHERE `Barcode`='" + bcrr + "' ;"); //  db 에서 바코드 찾음
                    
                    string model = ModelNamelbl1.Text;

                        string cmd = SQLCMD.MakeUpdateCmdSentence_where_equals(sql.table, "Barcode", bcrr, "",

                            "Model", model,
                            "Datetime", Dtime.Now(Dtime.StringType.ForDatum),
                            "Barcode", bcrr,
                            "CamNum", "2"
                           
                            );

                        sql.ExecuteNonQuery(cmd);

                        Log_K.WriteLog(log_lst, Mainpath, "Triger2 DB에 바코드 있음 > 업데이트 / " + bcrr);
              
                }
                catch (Exception)
                {
                    Log_K.WriteLog(log_lst, Mainpath, "Triger2 DB 저장 Error");
                    Console.WriteLine("Triger2 DB 저장 Error ");
                }

       
                Delay(1000);
              
                plc1.MasterK_Write_W("3230303135", "0100"); //  카메라2 완료 
                Log_K.WriteLog(log_lst, Mainpath, "Triger2 DB에 업데이트 완료");


                if (check_DBDelete.Checked)
                {
                    string cmdd = "DELETE FROM table1 WHERE `Barcode` = '" + bcrr + "';";
                    sql.ExecuteNonQuery(cmdd);
                }
            }

            if (name.Equals("ModelChange1"))//데이터 보기
            {
                if (!checkBox_ModelChangeManual1.Checked)
                {
                    int[] AllData = (int[])data;
                    int modelnum = AllData[0];

                        modelOpen1(modelnum);
                        CurrentModelNum1 = modelnum;

                }            

            }

            if (name.Equals("Save1"))   //Save1 (20030) _ 캡 조립검사 1번 장비 데이터 저장 + 세척유무
            {
                Delay(100);

                Log_K.WriteLog(log_lst, Mainpath, "save1 신호");

                start = 0;
                end = 1;

                string[] AllData = (string[])data;
                string[] indata = new string[600];
                string[] address = new string[300];

                try
                {
                    
                    for (int i = 0; i < 600; i++)
                    {
                        indata[i] = AllData[32 + i];
                    }

                   
                    for (int j = 0; j < 300; j++)   //  address [] 배열에 스타트 번지부터 값 200개 넣기
                    {
                        address[j] = indata[start] + indata[end];
                        start += 2;
                        end += 2;
                    }
                }
                catch (Exception)
                {
                    Log_K.WriteLog(log_lst, Mainpath, "plc Save1 에러");
                }

                string bcrr = "";

                for (int i = 32; i < 38; i++)    //  바코드 길이 문자가져오기
                {
                    Console.WriteLine(ConvertHexToString(address[i]));
                    bcrr += ConvertHexToString(address[i]);
                }

                start2 = 1;
                end2 = 0;

                
                string[] address2 = new string[300];

                try
                {
                    
                    for (int i = 0; i < 600; i++)
                    {
                        indata[i] = AllData[32 + i];
                    }

                    for (int j = 0; j < 300; j++)   //  address [] 배열에 스타트 번지부터 값 200개 넣기
                    {
                        address2[j] = indata[start2] + indata[end2];
                        start2 += 2;
                        end2 += 2;
                    }
                }
                catch (Exception)
                {
                    Log_K.WriteLog(log_lst, Mainpath, "plc Save1 에러");
                }

                
                int[] IntData = new int[300];   //  10진수 가져오기 IntData 안에 10진수 데이터 있음
                
                for (int i = 0; i < 300; i++)
                {
                    IntData[i] = Convert.ToInt32(address2[i], 16);
                }

                
                string[] plcdata = new string[300];
               
                for (int i = 0; i < 300; i++)
                {
                    if (IntData[i] <= 32768)
                    {
                        plcdata[i] = (IntData[i] * 0.1).ToString();
                    }
                    else
                    {
                        int temp = IntData[i] - 65536;
                        plcdata[i] = (temp * 0.1).ToString();
                    }
                }

                try
                {
                    int Decision = IntData[38]; //  캡 결과

                    string dbResult = "";

                    if (Decision == 1)
                        dbResult = "OK";
                    else
                        dbResult = "NG";

                    int Decision2 = IntData[42];    //  세척 결과

                    string dbResult2 = "OK";

                    Delay(100);

                    int rows = sql.ExecuteQuery_Select_Count("SELECT COUNT(*) FROM table1 WHERE `Barcode`='" + bcrr + "' ;"); //  db 에서 바코드 찾음

                    string model = ModelNamelbl1.Text;

                        string cmd = SQLCMD.MakeUpdateCmdSentence_where_equals(sql.table, "Barcode", bcrr, "",

                        "Model", model,
                        "Datetime", Dtime.Now(Dtime.StringType.ForDatum),
                        "Barcode", bcrr,
                        "Wash", "OK",
                        "CapNum", "1",
                        "CapResult", dbResult,
                        "Cap1", plcdata[39],
                        "Cap2", plcdata[40],
                        "Cap3", plcdata[41]
                         );

                        sql.ExecuteNonQuery(cmd);

                        
                        Log_K.WriteLog(log_lst, Mainpath, "save1 DB에 #1 Cap조립 데이터 > 업데이트 / " + bcrr + " / " + dbResult2 + " / " + dbResult + " / " + plcdata[39] + " / " + plcdata[40] + " / " + plcdata[41]);

                    
                    Delay(100);
                    
                        plc1.MasterK_Write_W("3230303331", "0100"); //  Save1 _ 저장 완료 (20031) 
                    Log_K.WriteLog(log_lst, Mainpath, "save1 저장완료");
                    
                }

                catch (Exception)
                {
                    Log_K.WriteLog(log_lst, Mainpath, "save1 DB 저장 #1 Cap조립 Error1");
                    Console.WriteLine("DB 저장 Error ");
                }
            }

            if (name.Equals("Save2"))   //  Save2 (20050) _ 캡 조립검사 2번 장비 데이터 저장 + 세척유무
            {
                Delay(100);

                Log_K.WriteLog(log_lst, Mainpath, "save2 신호");

                start = 0;
                end = 1;

                string[] AllData = (string[])data;
                string[] indata = new string[600];
                string[] address = new string[300];

                try
                {
                    
                    for (int i = 0; i < 600; i++)
                    {
                        indata[i] = AllData[32 + i];
                    }
                 
                    for (int j = 0; j < 300; j++)   //  address [] 배열에 스타트 번지부터 값 200개 넣기
                    {
                        address[j] = indata[start] + indata[end];
                        start += 2;
                        end += 2;
                    }
                }
                catch (Exception)
                {
                    Log_K.WriteLog(log_lst, Mainpath, "plc Save2 에러");
                }

                string bcrr = "";

                for (int i = 52; i < 58; i++)    //  바코드 길이 문자가져오기
                {
                    Console.WriteLine(ConvertHexToString(address[i]));
                    bcrr += ConvertHexToString(address[i]);
                }

                start2 = 1;
                end2 = 0;
             
                string[] address2 = new string[300];

                try
                {
                    
                    for (int i = 0; i < 600; i++)
                    {
                        indata[i] = AllData[32 + i];
                    }

                    for (int j = 0; j < 300; j++)   //  address [] 배열에 스타트 번지부터 값 200개 넣기
                    {
                        address2[j] = indata[start2] + indata[end2];
                        start2 += 2;
                        end2 += 2;
                    }
                }
                catch (Exception)
                {
                    Log_K.WriteLog(log_lst, Mainpath, "plc Save2 에러");
                }

               
                int[] IntData = new int[300];   //  10진수 가져오기 IntData 안에 10진수 데이터 있음
                
                for (int i = 0; i < 300; i++)
                {
                    IntData[i] = Convert.ToInt32(address2[i], 16);
                }

                string[] plcdata = new string[300];

                for (int i = 0; i < 300; i++)
                {
                    if (IntData[i] <= 32768)
                    {
                        plcdata[i] = (IntData[i] * 0.1).ToString();
                    }
                    else
                    {
                        int temp = IntData[i] - 65536;

                        plcdata[i] = (temp * 0.1).ToString();
                    }
                }

                try
                {
                    int Decision = IntData[58];

                    string dbResult = "";

                    if (Decision == 1)
                        dbResult = "OK";
                    else
                        dbResult = "NG";
                    
                    int Decision2 = IntData[62];

                    string dbResult2 = "";

                    if (Decision2 == 1)
                        dbResult2 = "OK";
                    else
                        dbResult2 = "NG";


                    Delay(100);

                    int rows = sql.ExecuteQuery_Select_Count("SELECT COUNT(*) FROM table1 WHERE `Barcode`='" + bcrr + "' ;"); //  db 에서 바코드 찾음

                    string model = ModelNamelbl1.Text;

                        string cmd = SQLCMD.MakeUpdateCmdSentence_where_equals(sql.table, "Barcode", bcrr, "",

                        "Model", model,
                        "Datetime", Dtime.Now(Dtime.StringType.ForDatum),
                        "Barcode", bcrr,
                        "Wash", "OK",
                        "CapNum", "2",
                        "CapResult", dbResult,
                        "Cap1", plcdata[59],
                        "Cap2", plcdata[60],
                        "Cap3", plcdata[61]

                            );

                        sql.ExecuteNonQuery(cmd);

                        Log_K.WriteLog(log_lst, Mainpath, "save2 DB에 #2 Cap조립 데이터 > 업데이트 / " + bcrr + " / " + dbResult2 + " / " + dbResult + " / " + plcdata[59] + " / " + plcdata[60] + " / " + plcdata[61]);

                         Delay(100);

                         plc1.MasterK_Write_W("3230303531", "0100"); //  Save2 _ 저장 완료 (20051) 

                }

                catch (Exception)
                {
                    Log_K.WriteLog(log_lst, Mainpath, "save2 DB 저장 #2 Cap조립 Error2");
                    Console.WriteLine("DB 저장 Error2 ");
                }
            }

            if (name.Equals("Save3"))   //  Save3 (20070) _ LVDT 검사 1번 장비 데이터 저장
            {
                Delay(100);

                Log_K.WriteLog(log_lst, Mainpath, "save3 신호");

                start = 0;
                end = 1;

                string[] AllData = (string[])data;
                string[] indata = new string[600];
                string[] address = new string[300];

                try
                {
                    
                    for (int i = 0; i < 600; i++)
                    {
                        indata[i] = AllData[32 + i];
                    }

                    for (int j = 0; j < 300; j++)   //  address [] 배열에 스타트 번지부터 값 200개 넣기
                    {
                        address[j] = indata[start] + indata[end];
                        start += 2;
                        end += 2;
                    }
                }
                catch (Exception)
                {
                    Log_K.WriteLog(log_lst, Mainpath, "plc Save3 에러");
                }

                string bcrr = "";

                for (int i = 72; i < 78; i++)    //  바코드 길이 문자가져오기
                {
                    Console.WriteLine(ConvertHexToString(address[i]));
                    bcrr += ConvertHexToString(address[i]);
                }

                start2 = 1;
                end2 = 0;

                string[] address2 = new string[300];

                try
                {
                    for (int i = 0; i < 600; i++)
                    {
                        indata[i] = AllData[32 + i];
                    }

                    for (int j = 0; j < 300; j++)   //  address [] 배열에 스타트 번지부터 값 200개 넣기
                    {
                        address2[j] = indata[start2] + indata[end2];
                        start2 += 2;
                        end2 += 2;
                    }
                }
                catch (Exception)
                {
                    Log_K.WriteLog(log_lst, Mainpath, "plc Save3 에러");
                }

                int[] IntData = new int[300];   //  10진수 가져오기 IntData 안에 10진수 데이터 있음

                for (int i = 0; i < 300; i++)
                {
                    IntData[i] = Convert.ToInt32(address2[i], 16);
                }

                string[] plcdata = new string[300];

                for (int i = 0; i < 300; i++)
                {
                    if (IntData[i] <= 32768)
                    {
                        plcdata[i] = (IntData[i] * 0.01).ToString();
                    }
                    else
                    {
                        int temp = IntData[i] - 65536;

                        plcdata[i] = (temp * 0.01).ToString();
                    }
                }

                try
                {
                    int Decision = IntData[78];

                    string dbResult = "";

                    if (Decision == 1)
                        dbResult = "OK";
                    else
                        dbResult = "NG";

                    Delay(100);

                    int rows = sql.ExecuteQuery_Select_Count("SELECT COUNT(*) FROM table1 WHERE `Barcode`='" + bcrr + "' ;"); //  db 에서 바코드 찾음

                    string model = ModelNamelbl1.Text;

                        string cmd = SQLCMD.MakeUpdateCmdSentence_where_equals(sql.table, "Barcode", bcrr, "",

                        "Model", model,
                        "Datetime", Dtime.Now(Dtime.StringType.ForDatum),
                        "Barcode", bcrr,
                        "LVDTResult", dbResult,
                        "LVDT1", plcdata[79],
                        "LVDT2", plcdata[80],
                        "LVDT3", plcdata[81],
                        "LVDT4", plcdata[82],
                        "LVDT5", plcdata[83],
                        "LVDT6", plcdata[84],
                        "LVDT7", plcdata[85],
                        "LVDT8", plcdata[86],
                        "LVDT9", plcdata[87]

                            );

                        sql.ExecuteNonQuery(cmd);

                        Log_K.WriteLog(log_lst, Mainpath, "save3 DB에 LVDT 데이터 > 업데이트" + bcrr + " / " + dbResult + " / " + plcdata[79] + " / " + plcdata[80] + " / " + plcdata[81] + " / " + plcdata[82] + " / " + plcdata[83] + " / " + plcdata[84] + " / " + plcdata[85] + " / " + plcdata[86] + " / " + plcdata[87]);

                        Delay(100);

                        plc1.MasterK_Write_W("3230303731", "0100"); //  Save3 _ 저장 완료 (20071) 

                }

                catch (Exception)
                {
                    Log_K.WriteLog(log_lst, Mainpath, "save3 DB 저장 Error3");
                    Console.WriteLine("DB 저장 Error3 ");
                }
            }

            if (name.Equals("Save4"))   //  Save4 (20100) _ 가스리크검사 1번 장비 데이터 저장
            {
                Delay(100);

                Log_K.WriteLog(log_lst, Mainpath, "save4 신호");

                start = 0;
                end = 1;

                string[] AllData = (string[])data;
                string[] indata = new string[600];
                string[] address = new string[300];

                try
                {
                    for (int i = 0; i < 600; i++)
                    {
                        indata[i] = AllData[32 + i];
                    }

                    for (int j = 0; j < 300; j++)   //  address [] 배열에 스타트 번지부터 값 200개 넣기
                    {
                        address[j] = indata[start] + indata[end];
                        start += 2;
                        end += 2;
                    }
                }
                catch (Exception)
                {
                    Log_K.WriteLog(log_lst, Mainpath, "plc Save4 에러");
                }

                string bcrr = "";

                for (int i = 102; i < 108; i++)    //  바코드 길이 문자가져오기
                {
                    Console.WriteLine(ConvertHexToString(address[i]));
                    bcrr += ConvertHexToString(address[i]);
                }

                start2 = 1;
                end2 = 0;

                string[] address2 = new string[300];

                try
                {
                    for (int i = 0; i < 600; i++)
                    {
                        indata[i] = AllData[32 + i];
                    }

                    for (int j = 0; j < 300; j++)   //  address [] 배열에 스타트 번지부터 값 200개 넣기
                    {
                        address2[j] = indata[start2] + indata[end2];
                        start2 += 2;
                        end2 += 2;
                    }
                }
                catch (Exception)
                {
                    Log_K.WriteLog(log_lst, Mainpath, "plc Save4 에러");
                }

                int[] IntData = new int[300];   //  10진수 가져오기 IntData 안에 10진수 데이터 있음
                for (int i = 0; i < 300; i++)
                {
                    IntData[i] = Convert.ToInt32(address2[i], 16);
                }

                string[] plcdata = new string[300];

                for (int i = 0; i < 300; i++)
                {
                    if (IntData[i] <= 32768)
                    {
                        plcdata[i] = (IntData[i] * 0.01).ToString();
                    }
                    else
                    {
                        int temp = IntData[i] - 65536;

                        plcdata[i] = (temp * 0.01).ToString();
                    }
                }

                try
                {
                    int Decision = IntData[108];

                    string dbResult = "";

                    if (Decision == 1)
                        dbResult = "OK";
                    else
                        dbResult = "NG";

                    Delay(100);

                    int rows = sql.ExecuteQuery_Select_Count("SELECT COUNT(*) FROM table1 WHERE `Barcode`='" + bcrr + "' ;"); //  db 에서 바코드 찾음

                    string model = ModelNamelbl1.Text;
                        string cmd = SQLCMD.MakeUpdateCmdSentence_where_equals(sql.table, "Barcode", bcrr, "",

                        "Model", model,
                        "Datetime", Dtime.Now(Dtime.StringType.ForDatum),
                        "Barcode", bcrr,
                        "GasLeakNum", "1",
                        "GasLeakResult", dbResult,
                        "GasLeak1", plcdata[109]

                            );

                        sql.ExecuteNonQuery(cmd);

                        Log_K.WriteLog(log_lst, Mainpath, "save4 DB에 Gasleak 1 데이터 > 업데이트" + bcrr + " / " + dbResult + " / " + plcdata[109]);

                        Delay(100);

                        plc1.MasterK_Write_W("3230313031", "0100"); //  Save4 _ 저장 완료 (20101) 

                }

                catch (Exception)
                {
                    Log_K.WriteLog(log_lst, Mainpath, "save4 DB 저장 Error4");
                    Console.WriteLine("DB 저장 Error4 ");
                }
            }

            if (name.Equals("Save5"))   //  Save5 (20120) _ 워터리크검사 1번 장비 데이터 저장
            {
                Delay(100);

                Log_K.WriteLog(log_lst, Mainpath, "save5 신호");

                start = 0;
                end = 1;

                string[] AllData = (string[])data;
                string[] indata = new string[600];
                string[] address = new string[300];

                try
                {
                    for (int i = 0; i < 600; i++)
                    {
                        indata[i] = AllData[32 + i];
                    }

                    for (int j = 0; j < 300; j++)   //  address [] 배열에 스타트 번지부터 값 200개 넣기
                    {
                        address[j] = indata[start] + indata[end];
                        start += 2;
                        end += 2;
                    }
                }
                catch (Exception)
                {
                    Log_K.WriteLog(log_lst, Mainpath, "plc Save5 에러");
                }

                string bcrr = "";

                for (int i = 122; i < 128; i++)    //  바코드 길이 문자가져오기
                {
                    Console.WriteLine(ConvertHexToString(address[i]));
                    bcrr += ConvertHexToString(address[i]);
                }

                start2 = 1;
                end2 = 0;

                string[] address2 = new string[300];

                try
                {
                    for (int i = 0; i < 600; i++)
                    {
                        indata[i] = AllData[32 + i];
                    }

                    for (int j = 0; j < 300; j++)   //  address [] 배열에 스타트 번지부터 값 200개 넣기
                    {
                        address2[j] = indata[start2] + indata[end2];
                        start2 += 2;
                        end2 += 2;
                    }
                }
                catch (Exception)
                {
                    Log_K.WriteLog(log_lst, Mainpath, "plc Save5 에러");
                }

                int[] IntData = new int[300];   //  10진수 가져오기 IntData 안에 10진수 데이터 있음
                for (int i = 0; i < 300; i++)
                {
                    IntData[i] = Convert.ToInt32(address2[i], 16);
                }

                string[] plcdata = new string[300];

                for (int i = 0; i < 300; i++)
                {
                    if (IntData[i] <= 32768)
                    {
                        plcdata[i] = (IntData[i] * 0.01).ToString();
                    }
                    else
                    {
                        int temp = IntData[i] - 65536;

                        plcdata[i] = (temp * 0.01).ToString();
                    }
                }

                try
                {
                    int Decision = IntData[128];

                    string dbResult = "";

                    if (Decision == 1)
                        dbResult = "OK";
                    else
                        dbResult = "NG";

                    Delay(100);

                    int rows = sql.ExecuteQuery_Select_Count("SELECT COUNT(*) FROM table1 WHERE `Barcode`='" + bcrr + "' ;"); //  db 에서 바코드 찾음

                    string model = ModelNamelbl1.Text;
                        string cmd = SQLCMD.MakeUpdateCmdSentence_where_equals(sql.table, "Barcode", bcrr, "",

                        "Model", model,
                        "Datetime", Dtime.Now(Dtime.StringType.ForDatum),
                        "Barcode", bcrr,
                        "WaterLeakNum", "1",
                        "WaterLeakResult", dbResult,
                        "WaterLeak1", plcdata[129]

                            );

                        sql.ExecuteNonQuery(cmd);

                        Log_K.WriteLog(log_lst, Mainpath, "save5 DB에 Waterleak 1 데이터 > 업데이트" + bcrr + " / " + dbResult + " / " + plcdata[129]);

                    Delay(100);

                        plc1.MasterK_Write_W("3230313231", "0100"); //  Save5 _ 저장 완료 (20121) 

                }

                catch (Exception)
                {
                    Log_K.WriteLog(log_lst, Mainpath, "save5 DB 저장 Error5");
                    Console.WriteLine("DB 저장 Error5 ");
                }
            }

            if (name.Equals("Save6"))   //  Save6 (20140) _ 가스리크검사 2번 장비 데이터 저장
            {
                Delay(100);

                Log_K.WriteLog(log_lst, Mainpath, "save6 신호");

                start = 0;
                end = 1;

                string[] AllData = (string[])data;
                string[] indata = new string[600];
                string[] address = new string[300];

                try
                {
                    for (int i = 0; i < 600; i++)
                    {
                        indata[i] = AllData[32 + i];
                    }

                    for (int j = 0; j < 300; j++)   //  address [] 배열에 스타트 번지부터 값 200개 넣기
                    {
                        address[j] = indata[start] + indata[end];
                        start += 2;
                        end += 2;
                    }
                }
                catch (Exception)
                {
                    Log_K.WriteLog(log_lst, Mainpath, "plc Save6 에러");
                }

                string bcrr = "";

                for (int i = 142; i < 148; i++)    //  바코드 길이 문자가져오기
                {
                    Console.WriteLine(ConvertHexToString(address[i]));
                    bcrr += ConvertHexToString(address[i]);
                }

                start2 = 1;
                end2 = 0;

                string[] address2 = new string[300];

                try
                {
                    for (int i = 0; i < 600; i++)
                    {
                        indata[i] = AllData[32 + i];
                    }

                    for (int j = 0; j < 300; j++)   //  address [] 배열에 스타트 번지부터 값 200개 넣기
                    {
                        address2[j] = indata[start2] + indata[end2];
                        start2 += 2;
                        end2 += 2;
                    }
                }
                catch (Exception)
                {
                    Log_K.WriteLog(log_lst, Mainpath, "plc Save6 에러");
                }

                int[] IntData = new int[300];   //  10진수 가져오기 IntData 안에 10진수 데이터 있음
                for (int i = 0; i < 300; i++)
                {
                    IntData[i] = Convert.ToInt32(address2[i], 16);
                }

                string[] plcdata = new string[300];

                for (int i = 0; i < 300; i++)
                {
                    if (IntData[i] <= 32768)
                    {
                        plcdata[i] = (IntData[i] * 0.01).ToString();
                    }
                    else
                    {
                        int temp = IntData[i] - 65536;

                        plcdata[i] = (temp * 0.01).ToString();
                    }
                }

                try
                {

                    int Decision = IntData[148];

                    string dbResult = "";

                    if (Decision == 1)
                        dbResult = "OK";
                    else
                        dbResult = "NG";

                    Delay(100);

                    int rows = sql.ExecuteQuery_Select_Count("SELECT COUNT(*) FROM table1 WHERE `Barcode`='" + bcrr + "' ;"); //  db 에서 바코드 찾음

                    string model = ModelNamelbl1.Text;

                        string cmd = SQLCMD.MakeUpdateCmdSentence_where_equals(sql.table, "Barcode", bcrr, "",

                        "Model", model,
                        "Datetime", Dtime.Now(Dtime.StringType.ForDatum),
                        "Barcode", bcrr,
                        "GasLeakNum", "2",
                        "GasLeakResult", dbResult,
                        "GasLeak1", plcdata[149]

                            );

                        sql.ExecuteNonQuery(cmd);

                        Log_K.WriteLog(log_lst, Mainpath, "save6 DB에 Gasleak 2 데이터 > 업데이트" + bcrr + " / " + dbResult + " / " + plcdata[149]);

                    Delay(100);

                        plc1.MasterK_Write_W("3230313431", "0100"); //  Save6 _ 저장 완료 (20141) 
                }

                catch (Exception)
                {
                    Log_K.WriteLog(log_lst, Mainpath, "save6 DB 저장 Error6");
                    Console.WriteLine("DB 저장 Error6 ");
                }
            }

            if (name.Equals("Save7"))   //  Save7 (20160) _ 워터리크검사 2번 장비 데이터 저장
            {
                Delay(100);

                Log_K.WriteLog(log_lst, Mainpath, "save7 신호");

                start = 0;
                end = 1;

                string[] AllData = (string[])data;
                string[] indata = new string[600];
                string[] address = new string[300];

                try
                {
                    for (int i = 0; i < 600; i++)
                    {
                        indata[i] = AllData[32 + i];
                    }

                    for (int j = 0; j < 300; j++)   //  address [] 배열에 스타트 번지부터 값 200개 넣기
                    {
                        address[j] = indata[start] + indata[end];
                        start += 2;
                        end += 2;
                    }
                }
                catch (Exception)
                {
                    Log_K.WriteLog(log_lst, Mainpath, "plc Save7 에러");
                }

                string bcrr = "";

                for (int i = 162; i < 168; i++)    //  바코드 길이 문자가져오기
                {
                    Console.WriteLine(ConvertHexToString(address[i]));
                    bcrr += ConvertHexToString(address[i]);
                }

                start2 = 1;
                end2 = 0;

                string[] address2 = new string[300];

                try
                {
                    for (int i = 0; i < 600; i++)
                    {
                        indata[i] = AllData[32 + i];
                    }

                    for (int j = 0; j < 300; j++)   //  address [] 배열에 스타트 번지부터 값 200개 넣기
                    {
                        address2[j] = indata[start2] + indata[end2];
                        start2 += 2;
                        end2 += 2;
                    }
                }
                catch (Exception)
                {
                    Log_K.WriteLog(log_lst, Mainpath, "plc Save7 에러");
                }

                int[] IntData = new int[300];   //  10진수 가져오기 IntData 안에 10진수 데이터 있음
                for (int i = 0; i < 300; i++)
                {
                    IntData[i] = Convert.ToInt32(address2[i], 16);
                }

                string[] plcdata = new string[300];

                for (int i = 0; i < 300; i++)
                {
                    if (IntData[i] <= 32768)
                    {
                        plcdata[i] = (IntData[i] * 0.01 ).ToString();
                    }
                    else
                    {
                        int temp = IntData[i] - 65536;
                        plcdata[i] = (temp * 0.01).ToString();
                    }
                }

                try
                {
                    int Decision = IntData[168];

                    string dbResult = "";

                    if (Decision == 1)
                        dbResult = "OK";
                    else
                        dbResult = "NG";

                    Delay(100);

                    int rows = sql.ExecuteQuery_Select_Count("SELECT COUNT(*) FROM table1 WHERE `Barcode`='" + bcrr + "' ;"); //  db 에서 바코드 찾음

                    string model = ModelNamelbl1.Text;
                        string cmd = SQLCMD.MakeUpdateCmdSentence_where_equals(sql.table, "Barcode", bcrr, "",

                        "Model", model,
                        "Datetime", Dtime.Now(Dtime.StringType.ForDatum),
                        "Barcode", bcrr,
                        "WaterLeakNum", "2",
                        "WaterLeakResult", dbResult,
                        "WaterLeak1", plcdata[169]

                            );

                        sql.ExecuteNonQuery(cmd);

                        Log_K.WriteLog(log_lst, Mainpath, "save7 DB에 Waterleak 2 데이터 > 업데이트" + bcrr + " / " + dbResult + " / " + plcdata[169]);

                        Delay(100);

                        plc1.MasterK_Write_W("3230313631", "0100"); //  Save7 _ 저장 완료 (20161) 

                }

                catch (Exception)
                {
                    Log_K.WriteLog(log_lst, Mainpath, "save7 DB 저장 Error7");
                    Console.WriteLine("DB 저장 Error7 ");
                }
            }


            if (name.Equals("Save8"))   //  Save8 (20160) _ 워터리크검사 2번 장비 데이터 저장
            {
                Delay(100);

                Log_K.WriteLog(log_lst, Mainpath, "save8 신호");

                start = 0;
                end = 1;

                string[] AllData = (string[])data;
                string[] indata = new string[600];
                string[] address = new string[300];

                try
                {
                    for (int i = 0; i < 600; i++)
                    {
                        indata[i] = AllData[32 + i];
                    }

                    for (int j = 0; j < 300; j++)   //  address [] 배열에 스타트 번지부터 값 200개 넣기
                    {
                        address[j] = indata[start] + indata[end];
                        start += 2;
                        end += 2;
                    }
                }
                catch (Exception)
                {
                    Log_K.WriteLog(log_lst, Mainpath, "plc Save8 에러");
                }

                string bcrr = "";

                for (int i = 182; i < 188; i++)    //  바코드 길이 문자가져오기
                {
                    Console.WriteLine(ConvertHexToString(address[i]));
                    bcrr += ConvertHexToString(address[i]);
                }

                start2 = 1;
                end2 = 0;

                string[] address2 = new string[300];

                try
                {
                    for (int i = 0; i < 600; i++)
                    {
                        indata[i] = AllData[32 + i];
                    }

                    for (int j = 0; j < 300; j++)   //  address [] 배열에 스타트 번지부터 값 200개 넣기
                    {
                        address2[j] = indata[start2] + indata[end2];
                        start2 += 2;
                        end2 += 2;
                    }
                }
                catch (Exception)
                {
                    Log_K.WriteLog(log_lst, Mainpath, "plc Save8 에러");
                }

                int[] IntData = new int[300];   //  10진수 가져오기 IntData 안에 10진수 데이터 있음
                for (int i = 0; i < 300; i++)
                {
                    IntData[i] = Convert.ToInt32(address2[i], 16);
                }

                string[] plcdata = new string[300];

                for (int i = 0; i < 300; i++)
                {
                    if (IntData[i] <= 32768)
                    {
                        plcdata[i] = (IntData[i] * 0.01).ToString();
                    }
                    else
                    {
                        int temp = IntData[i] - 65536;

                        plcdata[i] = (temp * 0.01).ToString();
                    }
                }

                try
                {
                    Delay(100);

                    int rows = sql.ExecuteQuery_Select_Count("SELECT COUNT(*) FROM table1 WHERE `Barcode`='" + bcrr + "' ;"); //  db 에서 바코드 찾음

                    string model = ModelNamelbl1.Text;

                    if (rows == 0)//없을때
                    //if (rows == 0 || bcrr != "\0\0\0\0\0\0\0\0\0\0")//없을때
                    {
                        string cmd = Ken2.Database.SQLiteCMD_K.MakeInsertCmdSentence(sql.table,   //  무조건 DB에 올림 A로

                        "Model", model,
                        "Datetime", Dtime.Now(Dtime.StringType.ForDatum),
                        "Barcode", bcrr
                         );

                        sql.ExecuteNonQuery(cmd);

                        Log_K.WriteLog(log_lst, Mainpath, "Save8 DB에 데이터 없음8 > 인서트" + " / " + bcrr);
                    }

                    else//데이터 있다.
                    {
                        string cmd = SQLCMD.MakeUpdateCmdSentence_where_equals(sql.table, "Barcode", bcrr, "",

                        "Model", model,
                        "Datetime", Dtime.Now(Dtime.StringType.ForDatum),
                        "Barcode", bcrr
                            );

                        sql.ExecuteNonQuery(cmd);

                        Log_K.WriteLog(log_lst, Mainpath, "Save8 DB에 데이터 있음8 > 업데이트 / "+ bcrr);

                    }
                    Delay(100);
                    plc1.MasterK_Write_W("3230313831", "0100"); //  Save8 _ 저장 완료 (20181) 

                }

                catch (Exception)
                {
                    Log_K.WriteLog(log_lst, Mainpath, "Save8 DB 저장 Error8");
                    Console.WriteLine("DB 저장 Error8 ");
                }
            }
            
            if (name.Equals("Check1"))   //  check1 (20200) _ 캡 조립검사 1번 제품 판정 확인 요청
            {
                Delay(100);
                this.Invoke(new dele(() =>
                {
                    Log_K.WriteLog(log_lst, Mainpath, "check1 신호");
                }));

                    start = 0;
                    end = 1;

                    string[] AllData = (string[])data;                   
                    string[] indata = new string[600];
                    string[] address = new string[300];

                    try
                    {
                        for (int i = 0; i < 600; i++)
                        {
                            indata[i] = AllData[32 + i];
                        }

                        for (int j = 0; j < 300; j++)   //  address [] 배열에 스타트 번지부터 값 200개 넣기
                        {
                            address[j] = indata[start] + indata[end];
                            start += 2;
                            end += 2;
                        }
                    }

                    catch (Exception)
                    {
                        this.Invoke(new dele(() =>
                        {
                            Log_K.WriteLog(log_lst, Mainpath, "plc Check1 에러");
                        }));
                    }

                    string bcrr = "";

                    for (int i = 203; i < 209; i++)    //  바코드 길이 문자가져오기
                    {
                        Console.WriteLine(ConvertHexToString(address[i]));
                        bcrr += ConvertHexToString(address[i]);
                    }

                    //dgvH1.Columns.Clear();

                    Delay(100);

                    try
                    {
                        int rows = sql.ExecuteQuery_Select_Count("SELECT COUNT(*) FROM table1 WHERE `Barcode`='" + bcrr + "' ;"); //  db 에서 바코드 찾음
                        
                        if (rows == 0)//없을때
                        {
                            this.Invoke(new dele(() =>
                            {
                                Log_K.WriteLog(log_lst, Mainpath, "#1 캡 조립 바코드 없음 / " + bcrr);
                            }));
                            Delay(50);
                            plc1.MasterK_Write_W("3230323031", "0300"); //  Check1 _ 판정 NG = 2 (20201)  

                            dgvInit("dgvH1");

                        }

                        else//데이터 있다.
                        {
                            string cmd = SQLiteCMD_K.Select_Equal("table1", "Barcode", bcrr,
                                
                            "CapResult"

                            );

                            //sql.Select(dgvH1, cmd, false);

                            this.Invoke(new dele(() =>
                            {
                                Log_K.WriteLog(log_lst, Mainpath, "#1 캡 조립 바코드 있음 / " + bcrr);
                            }));

                            dgvInit("dgvH1");

                            Delay(500);

                            //string result1 = dgvH1.Rows[0].Cells[0].Value.ToString();
                            //if (result1 == "OK")
                            //{
                            //    Delay(50);

                            //        plc1.MasterK_Write_W("3230323031", "0100"); //  Check1 _ 판정 OK = 1 (20201)  

                            //}
                            //else if(result1 == "NG")
                            //{
                            //    Delay(50);
                            //    plc1.MasterK_Write_W("3230323031", "0200"); //  Check1 _ 판정 NG = 2 (20201)  

                            //}
                            //else
                            //{
                            //    result1 = "판정없음";
                            //}
                            //this.Invoke(new dele(() =>
                            //{
                            //    Log_K.WriteLog(log_lst, Mainpath, "#1 캡 조립 바코드 판정 : " + bcrr + " / " + result1);
                            //}));

                        }

                        Delay(500);

                            plc1.MasterK_Write_W("3230323032", "0100"); //  Check1 _ 완료 (20202) 

                        this.Invoke(new dele(() =>
                        {
                            Log_K.WriteLog(log_lst, Mainpath, "#1 캡 조립 바코드 check :  완료 / " + bcrr);
                        }));

                    }
                    catch (Exception)
                    {
                        this.Invoke(new dele(() =>
                        {
                            Log_K.WriteLog(log_lst, Mainpath, "#1 캡 조립 판정데이터 Error");
                        }));
                        Console.WriteLine("DB 저장 Error ");
                    }

            }


            if (name.Equals("Check2"))   //  check2 (20210) _ 캡 조립검사 2번 제품 판정 확인 요청
            {
                Delay(100);
                this.Invoke(new dele(() =>
                {
                    Log_K.WriteLog(log_lst, Mainpath, "check2 신호");
                }));

                    start = 0;
                    end = 1;

                    string[] AllData = (string[])data;

                    string[] indata = new string[600];

                    string[] address = new string[300];

                    try
                    {
                        for (int i = 0; i < 600; i++)
                        {
                            indata[i] = AllData[32 + i];
                        }

                        for (int j = 0; j < 300; j++)   //  address [] 배열에 스타트 번지부터 값 200개 넣기
                        {
                            address[j] = indata[start] + indata[end];
                            start += 2;
                            end += 2;
                        }
                    }

                    catch (Exception)
                    {
                        this.Invoke(new dele(() =>
                        {
                            Log_K.WriteLog(log_lst, Mainpath, "plc Check2 에러");
                        }));
                    }

                    string bcrr = "";

                    for (int i = 213; i < 219; i++)    //  바코드 길이 문자가져오기
                    {
                        Console.WriteLine(ConvertHexToString(address[i]));
                        bcrr += ConvertHexToString(address[i]);
                    }

                    //dgvH2.Columns.Clear();

                    Delay(100);

                    try
                    {
                        int rows = sql.ExecuteQuery_Select_Count("SELECT COUNT(*) FROM table1 WHERE `Barcode`='" + bcrr + "' ;"); //  db 에서 바코드 찾음

                        if (rows == 0)//없을때
                        {
                            this.Invoke(new dele(() =>
                            {
                                Log_K.WriteLog(log_lst, Mainpath, "#2 캡 조립 바코드 없음 / " + bcrr);
                            }));
                            Delay(50);

                                plc1.MasterK_Write_W("3230323131", "0300"); //  Check2 _ 판정 NG = 2 (20211)  

                            dgvInit("dgvH2");

                        }

                        else//데이터 있다.
                        {
                            string cmd = SQLiteCMD_K.Select_Equal("table1", "Barcode", bcrr,

                            "CapResult"

                            );

                            //sql.Select(dgvH2, cmd, false);

                            this.Invoke(new dele(() =>
                            {
                                Log_K.WriteLog(log_lst, Mainpath, "#2 캡 조립 바코드 있음 / " + bcrr);
                            }));

                            dgvInit("dgvH2");

                            Delay(500);

                            //string result1 = dgvH2.Rows[0].Cells[0].Value.ToString();
                            //if (result1 == "OK")
                            //{
                            //    Delay(50);

                            //        plc1.MasterK_Write_W("3230323131", "0100"); //  Check2 _ 판정 OK = 1 (20211)  

                            //}
                            //else if (result1 == "NG")
                            //{
                            //    Delay(50);

                            //        plc1.MasterK_Write_W("3230323131", "0200"); //  Check2 _ 판정 NG = 2 (20211)  

                            //}
                            //else
                            //{
                            //    Delay(50);

                            //    result1 = "판정없음";

                            //}
                            //this.Invoke(new dele(() =>
                            //{
                            //    Log_K.WriteLog(log_lst, Mainpath, "#2 캡 조립 바코드 판정 : " + bcrr + " / " + result1);
                            //}));

                        }

                        Delay(500);

                            plc1.MasterK_Write_W("3230323132", "0100"); //  Check2 _ 완료 (20212) 
                        this.Invoke(new dele(() =>
                        {
                            Log_K.WriteLog(log_lst, Mainpath, "#2 캡 조립 바코드 check :  완료 / " + bcrr);
                        }));

                    }
                    catch (Exception)
                    {
                        this.Invoke(new dele(() =>
                        {
                            Log_K.WriteLog(log_lst, Mainpath, "#2 캡 조립 판정데이터 Error");
                        }));
                        Console.WriteLine("DB 저장 Error ");
                    }
            }

            if (name.Equals("Check3"))   //  check3 (20220) _ LVDT 검사 제품 판정 확인 요청
            {
                Delay(100);
                this.Invoke(new dele(() =>
                {
                    Log_K.WriteLog(log_lst, Mainpath, "check3 신호");
                }));

                    start = 0;
                    end = 1;

                    string[] AllData = (string[])data;

                    string[] indata = new string[600];

                    string[] address = new string[300];

                    try
                    {

                        for (int i = 0; i < 600; i++)
                        {
                            indata[i] = AllData[32 + i];
                        }

                        for (int j = 0; j < 300; j++)   //  address [] 배열에 스타트 번지부터 값 200개 넣기
                        {
                            address[j] = indata[start] + indata[end];
                            start += 2;
                            end += 2;
                        }
                    }

                    catch (Exception)
                    {
                        this.Invoke(new dele(() =>
                        {
                            Log_K.WriteLog(log_lst, Mainpath, "plc Check3 에러");
                        }));
                    }

                    string bcrr = "";

                    for (int i = 223; i < 229; i++)    //  바코드 길이 문자가져오기
                    {
                        Console.WriteLine(ConvertHexToString(address[i]));
                        bcrr += ConvertHexToString(address[i]);
                    }

                    //dgvH3.Columns.Clear();

                    Delay(100);

                    try
                    {
                        int rows = sql.ExecuteQuery_Select_Count("SELECT COUNT(*) FROM table1 WHERE `Barcode`='" + bcrr + "' ;"); //  db 에서 바코드 찾음

                        if (rows == 0)//없을때
                        {
                            this.Invoke(new dele(() =>
                            {
                                Log_K.WriteLog(log_lst, Mainpath, "# LVDT 바코드 없음 / " + bcrr);
                            }));
                            Delay(50);

                                plc1.MasterK_Write_W("3230323231", "0300"); //  Check3 _ 판정 (20221)  
                            dgvInit("dgvH3");
                        }

                        else//데이터 있다.
                        {
                            string cmd = SQLiteCMD_K.Select_Equal("table1", "Barcode", bcrr,

                            "LVDTResult"

                            );

                            //sql.Select(dgvH3, cmd, false);

                            this.Invoke(new dele(() =>
                            {
                                Log_K.WriteLog(log_lst, Mainpath, "# LVDT 바코드 있음 / " + bcrr);
                            }));

                            dgvInit("dgvH3");

                            Delay(500);

                            //string result1 = dgvH3.Rows[0].Cells[0].Value.ToString();
                            //if (result1 == "OK")
                            //{
                            //    Delay(50);

                            //        plc1.MasterK_Write_W("3230323231", "0100"); //  Check3 _ 판정 OK = 1 (20221)  

                            //}
                                
                            //else if(result1 == "NG")
                            //{
                            //    Delay(50);

                            //        plc1.MasterK_Write_W("3230323231", "0200"); //  Check3 _ 판정 NG = 2 (20221)  
                                
                            //}
                            //else
                            //{
                            //    result1 = "판정없음";
                            //}
                            //this.Invoke(new dele(() =>
                            //{
                            //    Log_K.WriteLog(log_lst, Mainpath, "# LVDT 조립 바코드 check : " + bcrr + " / " + result1);
                            //}));
                        }
                        Delay(500);

                            plc1.MasterK_Write_W("3230323232", "0100"); //  Check3 _ 완료 (20222) 
                        this.Invoke(new dele(() =>
                        {
                            Log_K.WriteLog(log_lst, Mainpath, "# LVDT 조립 바코드 check :  완료 / " + bcrr);
                        }));

                    }
                    catch (Exception)
                    {
                        this.Invoke(new dele(() =>
                        {
                            Log_K.WriteLog(log_lst, Mainpath, "# LVDT 판정데이터 Error");
                        }));
                        Console.WriteLine("DB 저장 Error ");
                    }
            }


            if (name.Equals("Check4"))   //  check4 (20230) _ #1 GasLeak 검사 제품 판정 확인 요청
            {
                Delay(100);
                this.Invoke(new dele(() =>
                {
                    Log_K.WriteLog(log_lst, Mainpath, "check4 신호");
                }));

                    start = 0;
                    end = 1;

                    string[] AllData = (string[])data;
                    string[] indata = new string[600];
                    string[] address = new string[300];

                    try
                    {
                        for (int i = 0; i < 600; i++)
                        {
                            indata[i] = AllData[32 + i];
                        }

                        for (int j = 0; j < 300; j++)   //  address [] 배열에 스타트 번지부터 값 200개 넣기
                        {
                            address[j] = indata[start] + indata[end];
                            start += 2;
                            end += 2;
                        }
                    }

                    catch (Exception)
                    {
                        this.Invoke(new dele(() =>
                        {
                            Log_K.WriteLog(log_lst, Mainpath, "plc Check4 에러");
                        }));
                    }

                    string bcrr = "";

                    for (int i = 233; i < 239; i++)    //  바코드 길이 문자가져오기
                    {
                        Console.WriteLine(ConvertHexToString(address[i]));
                        bcrr += ConvertHexToString(address[i]);
                    }

                    //dgvH4.Columns.Clear();

                    Delay(100);

                    try
                    {
                        int rows = sql.ExecuteQuery_Select_Count("SELECT COUNT(*) FROM table1 WHERE `Barcode`='" + bcrr + "' ;"); //  db 에서 바코드 찾음

                        if (rows == 0)//없을때
                        {
                            this.Invoke(new dele(() =>
                            {
                                Log_K.WriteLog(log_lst, Mainpath, "#1 GasLeak 바코드 없음 / " + bcrr);
                            }));
                            Delay(50);

                                plc1.MasterK_Write_W("3230323331", "0300"); //  Check4 _ 판정 (20231)  
                            dgvInit("dgvH4");
                        }

                        else//데이터 있다.
                        {
                            string cmd = SQLiteCMD_K.Select_Equal("table1", "Barcode", bcrr,

                            "GasLeakResult"

                            );

                           // sql.Select(dgvH4, cmd, false);

                            this.Invoke(new dele(() =>
                            {
                                Log_K.WriteLog(log_lst, Mainpath, "#1 GasLeak 바코드 있음 / " + bcrr);
                            }));

                            dgvInit("dgvH4");

                            Delay(500);

                            //string result1 = dgvH4.Rows[0].Cells[0].Value.ToString();
                            //if (result1 == "OK")
                            //{
                            //    Delay(50);

                            //        plc1.MasterK_Write_W("3230323331", "0100"); //  Check4 _ 판정 OK = 1 (20231)  

                            //}
                                
                            //else if(result1 == "NG")
                            //{
                            //    Delay(50);

                            //        plc1.MasterK_Write_W("3230323331", "0200"); //  Check4 _ 판정 NG = 2 (20231)  

                            //}
                            //else
                            //{
                            //    result1 = "판정없음";
                            //}
                            //this.Invoke(new dele(() =>
                            //{
                            //    Log_K.WriteLog(log_lst, Mainpath, "#1 GasLeak 바코드 check : " + bcrr + " / " + result1);
                            //}));
                        }
                        Delay(500);

                            plc1.MasterK_Write_W("3230323332", "0100"); //  Check4 _ 완료 (20232) 
                        this.Invoke(new dele(() =>
                        {
                            Log_K.WriteLog(log_lst, Mainpath, "#1 GasLeak 바코드 check :  완료 / " + bcrr);
                        }));
                    }
                    catch (Exception)
                    {
                        this.Invoke(new dele(() =>
                        {
                            Log_K.WriteLog(log_lst, Mainpath, "#1 GasLeak 판정데이터 Error");
                        }));
                        Console.WriteLine("DB 저장 Error ");
                    }
            }


            if (name.Equals("Check5"))   //  check5 (20240) _ #1 WaterLeak 검사 제품 판정 확인 요청
            {
                Delay(100);
                this.Invoke(new dele(() =>
                {
                    Log_K.WriteLog(log_lst, Mainpath, "check5 신호");
                }));

                    start = 0;
                    end = 1;

                    string[] AllData = (string[])data;
                    string[] indata = new string[600];
                    string[] address = new string[300];

                    try
                    {
                        for (int i = 0; i < 600; i++)
                        {
                            indata[i] = AllData[32 + i];
                        }

                        for (int j = 0; j < 300; j++)   //  address [] 배열에 스타트 번지부터 값 200개 넣기
                        {
                            address[j] = indata[start] + indata[end];
                            start += 2;
                            end += 2;
                        }
                    }

                    catch (Exception)
                    {
                        this.Invoke(new dele(() =>
                        {
                            Log_K.WriteLog(log_lst, Mainpath, "plc Check5 에러");
                        }));
                    }

                    string bcrr = "";

                    for (int i = 243; i < 249; i++)    //  바코드 길이 문자가져오기
                    {
                        Console.WriteLine(ConvertHexToString(address[i]));
                        bcrr += ConvertHexToString(address[i]);
                    }

                    //dgvH5.Columns.Clear();

                    Delay(100);

                    try
                    {
                        int rows = sql.ExecuteQuery_Select_Count("SELECT COUNT(*) FROM table1 WHERE `Barcode`='" + bcrr + "' ;"); //  db 에서 바코드 찾음

                        if (rows == 0)//없을때
                        {
                            this.Invoke(new dele(() =>
                            {
                                Log_K.WriteLog(log_lst, Mainpath, "#1 WaterLeak 바코드 없음 / " + bcrr);
                            }));
                            Delay(50);

                                plc1.MasterK_Write_W("3230323431", "0300"); //  Check5 _ 판정 (20241)  

                            dgvInit("dgvH5");
                        }

                        else//데이터 있다.
                        {
                            string cmd = SQLiteCMD_K.Select_Equal("table1", "Barcode", bcrr,

                            "WaterLeakResult"

                            );

                            //sql.Select(dgvH5, cmd, false);

                            this.Invoke(new dele(() =>
                            {
                                Log_K.WriteLog(log_lst, Mainpath, "#1 WaterLeak 바코드 있음 / " + bcrr);
                            }));

                            dgvInit("dgvH5");

                            Delay(500);

                            //string result1 = dgvH5.Rows[0].Cells[0].Value.ToString();
                            //if (result1 == "OK")
                            //{
                            //    Delay(50);

                            //        plc1.MasterK_Write_W("3230323431", "0100"); //  Check5 _ 판정 OK = 1 (20241)  
                                
                            //}
                            //else if(result1 == "NG")
                            //{
                            //    Delay(50);
                            //    plc1.MasterK_Write_W("3230323431", "0200"); //  Check5 _ 판정 NG = 2 (20241)  

                            //}
                            //else
                            //{
                            //    result1 = "판정없음";
                            //}
                            //this.Invoke(new dele(() =>
                            //{
                            //    Log_K.WriteLog(log_lst, Mainpath, "#1 WaterLeak 바코드 check : " + bcrr + " / " + result1);
                            //}));
                        }
                        Delay(500);

                            plc1.MasterK_Write_W("3230323432", "0100"); //  Check5 _ 완료 (20242) 
                        this.Invoke(new dele(() =>
                        {
                            Log_K.WriteLog(log_lst, Mainpath, "#1 WaterLeak 바코드 check :  완료 / " + bcrr);
                        }));
                    }
                    catch (Exception)
                    {
                        this.Invoke(new dele(() =>
                        {
                            Log_K.WriteLog(log_lst, Mainpath, "#1 WaterLeak 판정데이터 Error");
                        }));
                        Console.WriteLine("DB 저장 Error ");
                    }

            }


            if (name.Equals("Check6"))   //  check6 (20250) _ #2 GasLeak 검사 제품 판정 확인 요청
            {
                Delay(100);
                this.Invoke(new dele(() =>
                {
                    Log_K.WriteLog(log_lst, Mainpath, "check6 신호");
                }));

                    start = 0;
                    end = 1;

                    string[] AllData = (string[])data;

                    string[] indata = new string[600];

                    string[] address = new string[300];

                    try
                    {

                        for (int i = 0; i < 600; i++)
                        {
                            indata[i] = AllData[32 + i];
                        }

                        for (int j = 0; j < 300; j++)   //  address [] 배열에 스타트 번지부터 값 200개 넣기
                        {
                            address[j] = indata[start] + indata[end];
                            start += 2;
                            end += 2;
                        }
                    }

                    catch (Exception)
                    {
                        this.Invoke(new dele(() =>
                        {
                            Log_K.WriteLog(log_lst, Mainpath, "plc Check6 에러");
                        }));
                    }

                    string bcrr = "";

                    for (int i = 253; i < 259; i++)    //  바코드 길이 문자가져오기
                    {
                        Console.WriteLine(ConvertHexToString(address[i]));
                        bcrr += ConvertHexToString(address[i]);
                    }

                    //dgvH6.Columns.Clear();

                    Delay(500); //  100>500   1021  change

                    try
                    {
                        int rows = sql.ExecuteQuery_Select_Count("SELECT COUNT(*) FROM table1 WHERE `Barcode`='" + bcrr + "' ;"); //  db 에서 바코드 찾음

                        if (rows == 0)//없을때
                        {
                            this.Invoke(new dele(() =>
                            {
                                Log_K.WriteLog(log_lst, Mainpath, "#2 GasLeak 바코드 없음 / " + bcrr);
                            }));
                            Delay(50);
                            plc1.MasterK_Write_W("3230323531", "0300"); //  Check6 _ 판정 (20251)  
                            dgvInit("dgvH6");
                        }

                        else//데이터 있다.
                        {
                            string cmd = SQLiteCMD_K.Select_Equal("table1", "Barcode", bcrr,

                            "GasLeakResult"

                            );

                            //sql.Select(dgvH6, cmd, false);

                            this.Invoke(new dele(() =>
                            {
                                Log_K.WriteLog(log_lst, Mainpath, "#2 GasLeak 바코드 있음 / " + bcrr);
                            }));

                            dgvInit("dgvH6");

                            Delay(500);

                           // string result1 = dgvH6.Rows[0].Cells[0].Value.ToString();
                            //if (result1 == "OK")
                            //{
                            //    Delay(50);
                            //    plc1.MasterK_Write_W("3230323531", "0100"); //  Check6 _ 판정 OK = 1 (20251)      
                            //}
                            //else if (result1 == "NG")
                            //{
                            //    Delay(50);
                            //    plc1.MasterK_Write_W("3230323531", "0200"); //  Check6 _ 판정 NG = 2 (20251)  

                            //}
                            //else
                            //{
                            //    result1 = "판정없음";
                            //}
                            //this.Invoke(new dele(() =>
                            //{
                            //    Log_K.WriteLog(log_lst, Mainpath, "#2 GasLeak 바코드 check : " + bcrr + " / " + result1);
                            //}));
                        }
                        Delay(500);
                        plc1.MasterK_Write_W("3230323532", "0100"); //  Check6 _ 완료 (20252) 
                        this.Invoke(new dele(() =>
                        {
                            Log_K.WriteLog(log_lst, Mainpath, "#2 GasLeak 바코드 check :  완료 / " + bcrr);
                        }));
                    }
                    catch (Exception)
                    {
                        this.Invoke(new dele(() =>
                        {
                            Log_K.WriteLog(log_lst, Mainpath, "#2 GasLeak 판정데이터 Error");
                        }));
                        Console.WriteLine("DB 저장 Error ");
                    }
            }


            if (name.Equals("Check7"))   //  check7 (20260) _ #2 WaterLeak 검사 제품 판정 확인 요청
            {
                Delay(100);
                this.Invoke(new dele(() =>
                {
                    Log_K.WriteLog(log_lst, Mainpath, "check7 신호");
                }));

                    start = 0;
                    end = 1;

                    string[] AllData = (string[])data;
                    string[] indata = new string[600];
                    string[] address = new string[300];

                    try
                    {
                        for (int i = 0; i < 600; i++)
                        {
                            indata[i] = AllData[32 + i];
                        }

                        for (int j = 0; j < 300; j++)   //  address [] 배열에 스타트 번지부터 값 200개 넣기
                        {
                            address[j] = indata[start] + indata[end];
                            start += 2;
                            end += 2;
                        }
                    }

                    catch (Exception)
                    {
                        this.Invoke(new dele(() =>
                        {
                            Log_K.WriteLog(log_lst, Mainpath, "plc Check7 에러");
                        }));
                    }

                    string bcrr = "";

                    for (int i = 263; i < 269; i++)    //  바코드 길이 문자가져오기
                    {
                        Console.WriteLine(ConvertHexToString(address[i]));
                        bcrr += ConvertHexToString(address[i]);
                    }

                   // dgvH7.Columns.Clear();

                    Delay(100);

                    try
                    {
                        int rows = sql.ExecuteQuery_Select_Count("SELECT COUNT(*) FROM table1 WHERE `Barcode`='" + bcrr + "' ;"); //  db 에서 바코드 찾음

                        if (rows == 0)//없을때
                        {
                            this.Invoke(new dele(() =>
                            {
                                Log_K.WriteLog(log_lst, Mainpath, "#2 WaterLeak 바코드 없음 / " + bcrr);
                            }));
                            Delay(50);
                            plc1.MasterK_Write_W("3230323631", "0300"); //  Check7 _ 판정 (20261)  
                            dgvInit("dgvH7");
                        }

                        else//데이터 있다.
                        {
                            string cmd = SQLiteCMD_K.Select_Equal("table1", "Barcode", bcrr,

                            "WaterLeakResult"

                            );

                            //sql.Select(dgvH7, cmd, false);

                            this.Invoke(new dele(() =>
                            {
                                Log_K.WriteLog(log_lst, Mainpath, "#2 WaterLeak 바코드 있음 / " + bcrr);
                            }));

                            dgvInit("dgvH7");

                            Delay(500);

                            //string result1 = dgvH7.Rows[0].Cells[0].Value.ToString();
                            //if (result1 == "OK")
                            //{
                            //    Delay(50);
                            //    plc1.MasterK_Write_W("3230323631", "0100"); //  Check7 _ 판정 OK = 1 (20261)   

                            //}
                            //else if (result1 == "NG")
                            //{
                            //    Delay(50);
                            //    plc1.MasterK_Write_W("3230323631", "0200"); //  Check7 _ 판정 NG = 2 (20261)  

                            //}
                            //else
                            //{
                            //    result1 = "판정없음";
                            //}
                            //this.Invoke(new dele(() =>
                            //{
                            //    Log_K.WriteLog(log_lst, Mainpath, "#2 WaterLeak 바코드 check : " + bcrr + " / " + result1);
                            //}));
                        }
                        Delay(500);

                            plc1.MasterK_Write_W("3230323632", "0100"); //  Check7 _ 완료 (20262) 

                        this.Invoke(new dele(() =>
                        {
                            Log_K.WriteLog(log_lst, Mainpath, "#2 WaterLeak 바코드 check :  완료 / " + bcrr);
                        }));

                    }
                    catch (Exception)
                    {
                        this.Invoke(new dele(() =>
                        {
                            Log_K.WriteLog(log_lst, Mainpath, "#2 WaterLeak 판정데이터 Error");
                        }));
                        Console.WriteLine("DB 저장 Error ");
                    }

            }


            if (name.Equals("Check8"))   //  check8 (20270) _ #2 Vision 검사 제품 판정 확인 요청
            {
                Delay(100);
                this.Invoke(new dele(() =>
                {
                    Log_K.WriteLog(log_lst, Mainpath, "check8 신호");
                }));

                this.Invoke(new dele(() =>
                {

                    start = 0;
                    end = 1;

                    string[] AllData = (string[])data;
                    string[] indata = new string[600];
                    string[] address = new string[300];

                    try
                    {
                        for (int i = 0; i < 600; i++)
                        {
                            indata[i] = AllData[32 + i];
                        }

                        for (int j = 0; j < 300; j++)   //  address [] 배열에 스타트 번지부터 값 200개 넣기
                        {
                            address[j] = indata[start] + indata[end];
                            start += 2;
                            end += 2;
                        }
                    }

                    catch (Exception)
                    {
                        this.Invoke(new dele(() =>
                        {
                            Log_K.WriteLog(log_lst, Mainpath, "plc Check8 에러");
                        }));
                    }

                    string bcrr = "";

                    for (int i = 273; i < 279; i++)    //  바코드 길이 문자가져오기
                    {
                        Console.WriteLine(ConvertHexToString(address[i]));
                        bcrr += ConvertHexToString(address[i]);
                    }

                    //dgvH8.Columns.Clear();
                    Delay(100);

                    try
                    {
                        int rows = sql.ExecuteQuery_Select_Count("SELECT COUNT(*) FROM table1 WHERE `Barcode`='" + bcrr + "' ;"); //  db 에서 바코드 찾음

                        if (rows == 0)//없을때
                        {
                            this.Invoke(new dele(() =>
                            {
                                Log_K.WriteLog(log_lst, Mainpath, "#2 Vision 바코드 없음 / " + bcrr);
                            }));

                                plc1.MasterK_Write_W("3230323731", "0300"); //  Check8 _ 판정 (20271)  

                            dgvInit("dgvH8");
                        }

                        else//데이터 있다.
                        {
                            string cmd = SQLiteCMD_K.Select_Equal("table1", "Barcode", bcrr,

                            "CamResult2"

                            );

                            //sql.Select(dgvH8, cmd, false);

                            this.Invoke(new dele(() =>
                            {
                                Log_K.WriteLog(log_lst, Mainpath, "#2 Vision 바코드 있음 / " + bcrr);
                            }));

                            dgvInit("dgvH8");

                            //Delay(200);

                           // string result1 = dgvH8.Rows[0].Cells[0].Value.ToString();
                            //if (result1 == "OK")
                            //{
                            //        plc1.MasterK_Write_W("3230323731", "0100"); //  Check8 _ 판정 OK = 1 (20271) 
                                
                            //}
                            //else if (result1 == "NG")
                            //{

                            //       plc1.MasterK_Write_W("3230323731", "0200"); //  Check8 _ 판정 NG = 2 (20271)  

                            //}
                            //else
                            //{
                            //    result1 = "판정없음";
                            //}
                            //this.Invoke(new dele(() =>
                            //{
                            //    Log_K.WriteLog(log_lst, Mainpath, "#2 Vision 바코드 check : " + bcrr + " / " + result1);
                            //}));
                        }
                        Delay(500);

                        plc1.MasterK_Write_W("3230323732", "0100"); //  Check8 _ 완료 (20272) 

                        this.Invoke(new dele(() =>
                        {
                            Log_K.WriteLog(log_lst, Mainpath, "#2 Vision 바코드 check : 완료 / " + bcrr);
                        }));
                    }
                    catch (Exception)
                    {
                        this.Invoke(new dele(() =>
                        {
                            Log_K.WriteLog(log_lst, Mainpath, "#2 Vision 판정데이터 Error");
                        }));
                        Console.WriteLine("DB 저장 Error ");
                    }
                }));
            }

            if (name.Equals("DBReset"))   //  DB Reset
            {
                //DBReset();
                this.Invoke(new dele(() =>
                {
                    Log_K.WriteLog(log_lst, Mainpath, "DB Data Reset -> " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff"));
                }));
            }
        }

        //ccccccccccccccccccccc
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveTxt();

            StopmainThread(0);

            try
            {
                if (cam1 != null)
                    cam1.Dispose();
                if (plc1 != null)
                {
                    plc1.CommStop();
                    plc1.Disconnect();
                    plc1.Dispose();
                }

                Thread.Sleep(1000);

                try
                {
                    Process.GetCurrentProcess().Kill();
                }
                catch (Exception)
                {
                    //Log_K.WriteLog(log_err, Mainpath, "Form Closing 에러");
                }
            }
            catch (Exception)
            {
                //Log_K.WriteLog(log_err, Mainpath, "Form Closing 에러2");
            }
        }

        #region Cameraaaaaaaa
        private void cam1_ImageSignal(PylonBasler.CurrentStatus Command, object Data, int ArrayNum)
        {
            if (Command == PylonBasler.CurrentStatus.OneShot)     //  검사
            {
                pictureBox_Cam1.Image = (Bitmap)Data;
                image1 = (Bitmap)Data;
                triger1( );
                //point = Convert.ToInt32(textBox2.Text);
                //triger1(image1);
            }

            if (Command == PylonBasler.CurrentStatus.TestShot1)   //  테스트샷
            {
                pictureBox_Cam1.Image = (Bitmap)Data;
                image1 = (Bitmap)Data;
            }

            if (Command == PylonBasler.CurrentStatus.IOShot)      //  IO 트리거
            {
                this.Invoke(new dele(() =>
                {
                    Log_K.WriteLog(log_lst, Mainpath, "io 트리거 In -> " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff"));
                }));

                pictureBox_Cam1.Image = (Bitmap)Data;
                image1 = (Bitmap)Data;
                //triger1(image);
            }

            if (Command == PylonBasler.CurrentStatus.LiveShot)    //  라이브
            {
                pictureBox_Cam1.Image = (Bitmap)Data;
            }

            if (Command == PylonBasler.CurrentStatus.ContinuousShot)   //  연속 촬영
            {
                pictureBox_Cam1.Image = (Bitmap)Data;
                image1 = (Bitmap)Data;
                //triger1(image);
            }

            if (Command == PylonBasler.CurrentStatus.LiveShot)   //  연속 촬영
            {
                pictureBox_Cam1.Image = (Bitmap)Data;
            }

            else if (Command == PylonBasler.CurrentStatus.Stop)
            {
                pictureBox_Cam1.Image = (Bitmap)Data;
            }
        }

        private void Cam1_CommSignal1(bool Connected, int ArrayNum)
        {
            if (Connected)//연결 되면 밝기 적용하기.
            {
                cam1.SetExp(Convert.ToInt32(dgvCam1.Rows[0].Cells[0].Value));
                //cam1.SetExp(Convert.ToInt32(Txt_Address.Text));   //  밝기 저장
                //Log_K.WriteLog(log_err, Mainpath, "카메라1연결");
                Console.WriteLine("카메라1연결");
            }
            else
            {
                //Log_K.WriteLog(log_err, Mainpath, "카메라1해제");
                Console.WriteLine("카메라1해제");
            }

        }

        private void Cam2_ImageSignal(PylonBasler.CurrentStatus Command, object Data, int ArrayNum)
        {
            if (Command == PylonBasler.CurrentStatus.OneShot)     //  검사
            {
                pictureBox_Cam2.Image = (Bitmap)Data;
                image2 = (Bitmap)Data;
                triger2();
                //point = Convert.ToInt32(textBox2.Text);
                //triger1(image1);
            }

            if (Command == PylonBasler.CurrentStatus.TestShot1)   //  테스트샷
            {
                pictureBox_Cam2.Image = (Bitmap)Data;
                image2 = (Bitmap)Data;
            }

            if (Command == PylonBasler.CurrentStatus.IOShot)      //  IO 트리거
            {
                this.Invoke(new dele(() =>
                {
                    Log_K.WriteLog(log_lst, Mainpath, "io 트리거 In -> " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff"));
                }));

                pictureBox_Cam2.Image = (Bitmap)Data;
                image2 = (Bitmap)Data;
                //triger1(image);
            }

            if (Command == PylonBasler.CurrentStatus.LiveShot)    //  라이브
            {
                pictureBox_Cam2.Image = (Bitmap)Data;
            }

            if (Command == PylonBasler.CurrentStatus.ContinuousShot)   //  연속 촬영
            {
                pictureBox_Cam2.Image = (Bitmap)Data;
                image2 = (Bitmap)Data;
                //triger1(image);
            }

            if (Command == PylonBasler.CurrentStatus.LiveShot)   //  연속 촬영
            {
                pictureBox_Cam2.Image = (Bitmap)Data;
            }

            else if (Command == PylonBasler.CurrentStatus.Stop)
            {
                pictureBox_Cam2.Image = (Bitmap)Data;
            }
        }

        private void Cam2_CommSignal(bool Connected, int ArrayNum)
        {
            if (Connected)//연결 되면 밝기 적용하기.
            {
                cam2.SetExp(Convert.ToInt32(dgvCam2.Rows[0].Cells[0].Value));
                //cam1.SetExp(Convert.ToInt32(Txt_Address.Text));   //  밝기 저장
                //Log_K.WriteLog(log_err, Mainpath, "카메라1연결");
                Console.WriteLine("카메라1연결");
            }
            else
            {
                //Log_K.WriteLog(log_err, Mainpath, "카메라1해제");
                Console.WriteLine("카메라1해제");
            }
        }


        #endregion
        
        #region 관리자모드 단축키 Ctrl+Q / W
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Keys key = keyData & ~(Keys.Shift | Keys.Control);
            switch (key)
            {
                case Keys.Q://
                    if ((keyData & Keys.Control) != 0)
                    {
                        xtraTabControl_Model.ShowTabHeader = DevExpress.Utils.DefaultBoolean.True;
                        simpleButton4.Visible = true;
                        simpleButton5.Visible = true;
                    }
                    break;
                case Keys.W://
                    if ((keyData & Keys.Control) != 0)
                    {
                        xtraTabControl_Model.ShowTabHeader = DevExpress.Utils.DefaultBoolean.False;
                        simpleButton4.Visible = false;
                        simpleButton5.Visible = false;
                    }
                    break;
            }
            return base.ProcessCmdKey(ref msg, keyData);

        }
        #endregion
        
        #region ////////////////// mainThread //////////////////    //  시간
        private Thread mainThread;
        bool mainThreadFlag = false;
        CultureInfo culture = new CultureInfo("en-US");

        private void mainThreadMethod(object param)
        {
            int para = (int)param;

            while (mainThreadFlag)
            {
                this.Invoke(new dele(() =>
                {
                    //TimeLabel.Text = Dtime.Now(Dtime.StringType.CurrentTime);
                    //TimeLabel.Text = DateTime.Now.ToString("yyyy-MM-dd ddd tt hh:mm:ss", culture);

                    try
                    {
                        if (cam1.Connected)
                            dgvStatus1.Rows[2].Cells[0].Style.BackColor = Color.Lime;
                        else
                            dgvStatus1.Rows[2].Cells[0].Style.BackColor = Color.Crimson;
                        if (cam2.Connected)
                            dgvStatus2.Rows[2].Cells[0].Style.BackColor = Color.Lime;
                        else
                            dgvStatus2.Rows[2].Cells[0].Style.BackColor = Color.Crimson;
                        //if (plc1.Server_Connected)
                        //{
                        //    dgvStatus1.Rows[1].Cells[0].Style.BackColor = Color.Lime;
                        //    dgvStatus2.Rows[1].Cells[0].Style.BackColor = Color.Lime;
                        //}
                        //else
                        //{
                        //    dgvStatus1.Rows[1].Cells[0].Style.BackColor = Color.Crimson;
                        //    dgvStatus2.Rows[1].Cells[0].Style.BackColor = Color.Crimson;
                        //}

                            //if (cam1.Connected)
                            //    dgvStatus1.Rows[2].Cells[0].Style.BackColor = Color.Lime;
                            //else
                            //    dgvStatus1.Rows[2].Cells[0].Style.BackColor = Color.Crimson;

                            //if (cam2.Connected)
                            //    dgvStatus2.Rows[2].Cells[0].Style.BackColor = Color.Lime;
                            //else
                            //    dgvStatus2.Rows[2].Cells[0].Style.BackColor = Color.Crimson;

                            //if (plc1.Connected)
                            //    dgvStatus1.Rows[1].Cells[0].Style.BackColor = Color.Lime;
                            //else
                            //    dgvStatus1.Rows[1].Cells[0].Style.BackColor = Color.Crimson;

                            //if (cam1.Connected)
                            //    dgvStatus1.Rows[2].Cells[0].Style.BackColor = Color.Lime;
                            //else
                            //    dgvStatus1.Rows[2].Cells[0].Style.BackColor = Color.Crimson;

                    }
                    catch (Exception)
                    {
                        Log_K.WriteLog(log_lst, Mainpath, "메인쓰레드에러");
                    }


                }));

            }

            Thread.Sleep(1000);
        }
        public void StartmainThread(int param)
        {
            mainThreadFlag = true;
            mainThread = new Thread((new ParameterizedThreadStart(mainThreadMethod)));
            mainThread.Start(param);
        }
        public void StopmainThread(int None)
        {
            mainThreadFlag = false;
        }
        public void KillmainThread(int None)
        {
            mainThread.Abort();
        }
        #endregion ////////////////// mainThread //////////////////
        
        #region DGVvvvvvvv
        public void dgvInit(string name)
        {
            switch (name)
            {
                case "dgvD1":

                    try
                    {
                        //---------------↓ 기본 ↓---------------┐
                        DataGridView dgv = (DataGridView)Reflection_K.Get(this, name);//이름가져옴
                        string DGV_name = dgv.Name;//적용
                        int height = int.Parse(DataRW.Load_Simple(DGV_name + "H", "30"));//데이터가져옴
                        int fontheader = int.Parse(DataRW.Load_Simple(DGV_name + "FH", "12"));//데이터가져옴
                        int fontcell = int.Parse(DataRW.Load_Simple(DGV_name + "FC", "12"));//데이터가져옴
                        GridMaster.FontSize2(dgv, fontheader, fontcell);//적용
                        //GridMaster.FontSize2( dgv , "New Gulim" , fontheader , fontcell );//한자나 글자 깨질 때 이걸로 사용하세요.
                        //---------------↑ 기본 ↑---------------┘

                        //---------------↓ 생성 ↓---------------┐
                        string[] ColumnsName = new string[] {
                            //"Inspection", "Value", "Check"
                            //"A", "A", "A", "A","A", "A", "A", "A","A", "A", "A", "A", "A", "A", "A", "A", "A", "A"
                            "A", "A"
                        };
                        //int rows = 2;//초기 생성 Row수
                        int rows = 18;//초기 생성 Row수

                        GridMaster.Init3(dgv, true, height, rows, ColumnsName);
                        //---------------↑ 생성 ↑---------------┘

                        //---------------↓ 사용자 데이터 추가 부분 ↓---------------┐
                        //GridMaster.LoadCSV_OnlyData( dgv , System.Windows.Forms.Application.StartupPath + "\\AAAA.csv" );//셀데이터로드
                        //GridMaster.LoadCSV( dgvD0 , @"C:\Users\kclip3\Desktop\CR0.csv" );//셀데이터로드

                        //dgv.Rows[0].Cells[0].Value = "Inspection";
                        //dgv.Rows[1].Cells[0].Value = "Value";

                        //dgv.Rows[0].Cells[1].Value = "Area 1";
                        //dgv.Rows[0].Cells[2].Value = "Area 2";
                        //dgv.Rows[0].Cells[3].Value = "Area 3";
                        //dgv.Rows[0].Cells[4].Value = "Area 4";
                        //dgv.Rows[0].Cells[5].Value = "Area 5";
                        //dgv.Rows[0].Cells[6].Value = "Area 6";
                        //dgv.Rows[0].Cells[7].Value = "Area 7";
                        //dgv.Rows[0].Cells[8].Value = "Area 8";
                        //dgv.Rows[0].Cells[9].Value = "Area 9";
                        //dgv.Rows[0].Cells[10].Value = "Area 10";
                        //dgv.Rows[0].Cells[11].Value = "Area 11";
                        //dgv.Rows[0].Cells[12].Value = "Area 12";
                        //dgv.Rows[0].Cells[13].Value = "Area 13";
                        //dgv.Rows[0].Cells[14].Value = "Area 14";
                        //dgv.Rows[0].Cells[15].Value = "Area 15";
                        //dgv.Rows[0].Cells[16].Value = "Area 16";
                        //dgv.Rows[0].Cells[17].Value = "Pattern";

                        dgv.Rows[0].Cells[0].Value = "돌출 값";
                        dgv.Rows[0].Cells[1].Value = "값";

                        dgv.Rows[0].Cells[0].Style.Font = new Font("Tahoma", 19, FontStyle.Bold);
                        dgv.Rows[0].Cells[1].Style.Font = new Font("Tahoma", 19, FontStyle.Bold);



                        dgv.Rows[1].Cells[0].Value = "Area 1";
                        dgv.Rows[2].Cells[0].Value = "Area 2";
                        dgv.Rows[3].Cells[0].Value = "Area 3";
                        dgv.Rows[4].Cells[0].Value = "Area 4";
                        dgv.Rows[5].Cells[0].Value = "Area 5";
                        dgv.Rows[6].Cells[0].Value = "Area 6";
                        dgv.Rows[7].Cells[0].Value = "Area 7";
                        dgv.Rows[8].Cells[0].Value = "Area 8";
                        dgv.Rows[9].Cells[0].Value = "Area 9";
                        dgv.Rows[10].Cells[0].Value = "Area 10";
                        dgv.Rows[11].Cells[0].Value = "Area 11";
                        dgv.Rows[12].Cells[0].Value = "Area 12";
                        dgv.Rows[13].Cells[0].Value = "Area 13";
                        dgv.Rows[14].Cells[0].Value = "Area 14";
                        dgv.Rows[15].Cells[0].Value = "Area 15";
                        dgv.Rows[16].Cells[0].Value = "Area 16";
                        dgv.Rows[17].Cells[0].Value = "Pattern";

                        //---------------↑ 사용자 데이터 추가 부분 ↑---------------┘

                        //---------------↓ 정렬 ↓---------------┐
                        GridMaster.CenterAlign(dgv);
                        //GridMaster.LeftAlign( dgv );
                        //GridMaster.Align( dgv , 0 , DataGridViewContentAlignment.MiddleLeft );//단일 Column 정렬
                        //---------------↑ 정렬 ↑---------------┘

                        //---------------↓ 설정 ↓---------------┐
                        dgv.ReadOnly = true;//읽기전용
                        //dgv.Columns[0].ReadOnly = true;//읽기전용
                        //dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Tahoma", 25, FontStyle.Bold);
                        
                        GridMaster.DisableSortColumn(dgv);//오름차순 내림차순 정렬 막기

                        //dgv.AllowUserToResizeColumns = false;//컬럼폭 수정불가
                        dgv.ColumnHeadersVisible = false;//컬럼헤더 가리기                        
                        //dgv.Columns[ 1 ].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";//표시형식

                        //dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;//스페이스 시 줄바꿈
                        //dgv.DefaultCellStyle.BackColor = Color.Black;//색반전
                        //dgv.DefaultCellStyle.ForeColor = Color.White;//색반전
                        //dgv.DefaultCellStyle.Font = new Font("Tahoma", 18, FontStyle.Bold);
                        //dgv.DefaultCellStyle.SelectionBackColor = Color.Transparent;
                        //dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
                        //dgv.BackgroundColor = Color.Black;

                        //---------------↑ 설정 ↑---------------┘
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("dgvd1");
                    }

                    break;
                case "dgvD2":

                    try
                    {
                        //---------------↓ 기본 ↓---------------┐
                        DataGridView dgv = (DataGridView)Reflection_K.Get(this, name);//이름가져옴
                        string DGV_name = dgv.Name;//적용
                        int height = int.Parse(DataRW.Load_Simple(DGV_name + "H", "30"));//데이터가져옴
                        int fontheader = int.Parse(DataRW.Load_Simple(DGV_name + "FH", "12"));//데이터가져옴
                        int fontcell = int.Parse(DataRW.Load_Simple(DGV_name + "FC", "12"));//데이터가져옴
                        GridMaster.FontSize2(dgv, fontheader, fontcell);//적용
                        //GridMaster.FontSize2( dgv , "New Gulim" , fontheader , fontcell );//한자나 글자 깨질 때 이걸로 사용하세요.
                        //---------------↑ 기본 ↑---------------┘

                        //---------------↓ 생성 ↓---------------┐
                        string[] ColumnsName = new string[] {
                            //"Inspection", "Value", "Check"
                            //"A", "A", "A", "A","A", "A", "A", "A","A", "A", "A", "A", "A", "A", "A", "A", "A", "A"
                            "A", "A"
                        };
                        int rows = 1;//초기 생성 Row수


                        GridMaster.Init3(dgv, true, height, rows, ColumnsName);
                        //---------------↑ 생성 ↑---------------┘

                        //---------------↓ 사용자 데이터 추가 부분 ↓---------------┐
                        //GridMaster.LoadCSV_OnlyData( dgv , System.Windows.Forms.Application.StartupPath + "\\AAAA.csv" );//셀데이터로드
                        //GridMaster.LoadCSV( dgvD0 , @"C:\Users\kclip3\Desktop\CR0.csv" );//셀데이터로드
                        //dgv.Rows[0].Cells[0].Value = "검사내용";
                        //dgv.Rows[0].Cells[1].Value = "값";

                        //dgv.Rows[0].Cells[1].Value = "Area 1";
                        //dgv.Rows[0].Cells[2].Value = "Area 2";
                        //dgv.Rows[0].Cells[3].Value = "Area 3";
                        //dgv.Rows[0].Cells[4].Value = "Area 4";
                        //dgv.Rows[0].Cells[5].Value = "Area 5";
                        //dgv.Rows[0].Cells[6].Value = "Area 6";
                        //dgv.Rows[0].Cells[7].Value = "Area 7";
                        //dgv.Rows[0].Cells[8].Value = "Area 8";
                        //dgv.Rows[0].Cells[9].Value = "Area 9";
                        //dgv.Rows[0].Cells[10].Value = "Area 10";
                        //dgv.Rows[0].Cells[11].Value = "Area 11";
                        //dgv.Rows[0].Cells[12].Value = "Area 12";
                        //dgv.Rows[0].Cells[13].Value = "Area 13";
                        //dgv.Rows[0].Cells[14].Value = "Area 14";
                        //dgv.Rows[0].Cells[15].Value = "Area 15";
                        //dgv.Rows[0].Cells[16].Value = "Area 16";
                        //dgv.Rows[0].Cells[17].Value = "Pattern";

                        dgv.Rows[0].Cells[0].Value = "돌출 값";
                        //dgv.Rows[0].Cells[1].Value = "값";

                        dgv.Rows[0].Cells[0].Style.Font = new Font("Tahoma", 19, FontStyle.Bold);
                        dgv.Rows[0].Cells[1].Style.Font = new Font("Tahoma", 19, FontStyle.Bold);

                        //dgv.Rows[1].Cells[0].Value = "Area 1";
                        //dgv.Rows[2].Cells[0].Value = "Area 2";
                        //dgv.Rows[3].Cells[0].Value = "Area 3";
                        //dgv.Rows[4].Cells[0].Value = "Area 4";
                        //dgv.Rows[5].Cells[0].Value = "Area 5";
                        //dgv.Rows[6].Cells[0].Value = "Area 6";
                        //dgv.Rows[7].Cells[0].Value = "Area 7";
                        //dgv.Rows[8].Cells[0].Value = "Area 8";
                        //dgv.Rows[9].Cells[0].Value = "Area 9";
                        //dgv.Rows[10].Cells[0].Value = "Area 10";
                        //dgv.Rows[11].Cells[0].Value = "Area 11";
                        //dgv.Rows[12].Cells[0].Value = "Area 12";
                        //dgv.Rows[13].Cells[0].Value = "Area 13";
                        //dgv.Rows[14].Cells[0].Value = "Area 14";
                        //dgv.Rows[15].Cells[0].Value = "Area 15";
                        //dgv.Rows[16].Cells[0].Value = "Area 16";
                        //dgv.Rows[17].Cells[0].Value = "Pattern";

                        //---------------↑ 사용자 데이터 추가 부분 ↑---------------┘

                        //---------------↓ 정렬 ↓---------------┐
                        GridMaster.CenterAlign(dgv);
                        //GridMaster.LeftAlign( dgv );
                        //GridMaster.Align( dgv , 0 , DataGridViewContentAlignment.MiddleLeft );//단일 Column 정렬
                        //---------------↑ 정렬 ↑---------------┘

                        //---------------↓ 설정 ↓---------------┐
                        dgv.ReadOnly = true;//읽기전용
                        //dgv.Columns[0].ReadOnly = true;//읽기전용

                        GridMaster.DisableSortColumn(dgv);//오름차순 내림차순 정렬 막기

                        //dgv.AllowUserToResizeColumns = false;//컬럼폭 수정불가
                        dgv.ColumnHeadersVisible = false;//컬럼헤더 가리기                        
                        //dgv.Columns[ 1 ].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";//표시형식

                        //dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;//스페이스 시 줄바꿈
                        //dgv.DefaultCellStyle.BackColor = Color.Black;//색반전
                        //dgv.DefaultCellStyle.ForeColor = Color.White;//색반전
                        //dgv.DefaultCellStyle.Font = new Font("Tahoma", 18, FontStyle.Bold);
                        //dgv.DefaultCellStyle.SelectionBackColor = Color.Transparent;
                        //dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
                        //dgv.BackgroundColor = Color.Black;

                        //---------------↑ 설정 ↑---------------┘
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("dgvd2");
                    }

                    break;

                case "dgvStatus1":

                    try
                    {
                        //---------------↓ 기본 ↓---------------┐
                        DataGridView dgv = (DataGridView)Reflection_K.Get(this, name);//이름가져옴
                        string DGV_name = dgv.Name;//적용
                        int height = int.Parse(DataRW.Load_Simple(DGV_name + "H", "30"));//데이터가져옴
                        int fontheader = int.Parse(DataRW.Load_Simple(DGV_name + "FH", "12"));//데이터가져옴
                        int fontcell = int.Parse(DataRW.Load_Simple(DGV_name + "FC", "12"));//데이터가져옴
                        GridMaster.FontSize2(dgv, fontheader, fontcell);//적용
                        //GridMaster.FontSize2( dgv , "New Gulim" , fontheader , fontcell );//한자나 글자 깨질 때 이걸로 사용하세요.
                        //---------------↑ 기본 ↑---------------┘

                        //---------------↓ 생성 ↓---------------┐
                        string[] ColumnsName = new string[] {
                            "A"
                        };
                        int rows = 3;//초기 생성 Row수

                        GridMaster.Init3(dgv, true, height, rows, ColumnsName);
                        //---------------↑ 생성 ↑---------------┘

                        //---------------↓ 사용자 데이터 추가 부분 ↓---------------┐
                        //GridMaster.LoadCSV_OnlyData( dgv , System.Windows.Forms.Application.StartupPath + "\\AAAA.csv" );//셀데이터로드
                        //GridMaster.LoadCSV( dgvD0 , @"C:\Users\kclip3\Desktop\CR0.csv" );//셀데이터로드

                        dgv.Rows[0].Cells[0].Value = "Auto Run";
                        dgv.Rows[1].Cells[0].Value = "PLC (192.168.0.1)";
                        dgv.Rows[2].Cells[0].Value = "CAM (192.168.100.2)";

                        dgv.Rows[0].Cells[0].Style.BackColor = Color.Crimson;
                        dgv.Rows[1].Cells[0].Style.BackColor = Color.Crimson;
                        dgv.Rows[2].Cells[0].Style.BackColor = Color.Crimson;


                        //---------------↑ 사용자 데이터 추가 부분 ↑---------------┘

                        //---------------↓ 정렬 ↓---------------┐
                        GridMaster.CenterAlign(dgv);
                        //GridMaster.LeftAlign( dgv );
                        //GridMaster.Align( dgv , 0 , DataGridViewContentAlignment.MiddleLeft );//단일 Column 정렬
                        //---------------↑ 정렬 ↑---------------┘

                        //---------------↓ 설정 ↓---------------┐
                        dgv.ReadOnly = true;//읽기전용
                        //dgv.Columns[ 0 ].ReadOnly = true;//읽기전용

                        GridMaster.DisableSortColumn(dgv);//오름차순 내림차순 정렬 막기

                        //dgv.AllowUserToResizeColumns = false;//컬럼폭 수정불가
                        dgv.ColumnHeadersVisible = false;//컬럼헤더 가리기                        
                        //dgv.Columns[ 1 ].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";//표시형식

                        //dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;//스페이스 시 줄바꿈
                        //dgv.DefaultCellStyle.BackColor = Color.Black;//색반전
                        //dgv.DefaultCellStyle.ForeColor = Color.White;//색반전
                        dgv.DefaultCellStyle.Font = new Font("Tahoma", 12, FontStyle.Bold);
                        //dgv.DefaultCellStyle.SelectionBackColor = Color.Transparent;
                        //dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
                        //dgv.BackgroundColor = Color.Black;

                        //---------------↑ 설정 ↑---------------┘
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("dgvstatus1");
                    }

                    break;
                case "dgvStatus2":

                    try
                    {
                        //---------------↓ 기본 ↓---------------┐
                        DataGridView dgv = (DataGridView)Reflection_K.Get(this, name);//이름가져옴
                        string DGV_name = dgv.Name;//적용
                        int height = int.Parse(DataRW.Load_Simple(DGV_name + "H", "30"));//데이터가져옴
                        int fontheader = int.Parse(DataRW.Load_Simple(DGV_name + "FH", "12"));//데이터가져옴
                        int fontcell = int.Parse(DataRW.Load_Simple(DGV_name + "FC", "12"));//데이터가져옴
                        GridMaster.FontSize2(dgv, fontheader, fontcell);//적용
                        //GridMaster.FontSize2( dgv , "New Gulim" , fontheader , fontcell );//한자나 글자 깨질 때 이걸로 사용하세요.
                        //---------------↑ 기본 ↑---------------┘

                        //---------------↓ 생성 ↓---------------┐
                        string[] ColumnsName = new string[] {
                            "A"
                        };
                        int rows = 3;//초기 생성 Row수

                        GridMaster.Init3(dgv, true, height, rows, ColumnsName);
                        //---------------↑ 생성 ↑---------------┘

                        //---------------↓ 사용자 데이터 추가 부분 ↓---------------┐
                        //GridMaster.LoadCSV_OnlyData( dgv , System.Windows.Forms.Application.StartupPath + "\\AAAA.csv" );//셀데이터로드
                        //GridMaster.LoadCSV( dgvD0 , @"C:\Users\kclip3\Desktop\CR0.csv" );//셀데이터로드

                        dgv.Rows[0].Cells[0].Value = "Auto Run";
                        dgv.Rows[1].Cells[0].Value = "PLC (192.168.0.1)";
                        dgv.Rows[2].Cells[0].Value = "CAM (192.168.101.2)";

                        dgv.Rows[0].Cells[0].Style.BackColor = Color.Crimson;
                        dgv.Rows[1].Cells[0].Style.BackColor = Color.Crimson;
                        dgv.Rows[2].Cells[0].Style.BackColor = Color.Crimson;


                        //---------------↑ 사용자 데이터 추가 부분 ↑---------------┘

                        //---------------↓ 정렬 ↓---------------┐
                        GridMaster.CenterAlign(dgv);
                        //GridMaster.LeftAlign( dgv );
                        //GridMaster.Align( dgv , 0 , DataGridViewContentAlignment.MiddleLeft );//단일 Column 정렬
                        //---------------↑ 정렬 ↑---------------┘

                        //---------------↓ 설정 ↓---------------┐
                        dgv.ReadOnly = true;//읽기전용
                        //dgv.Columns[ 0 ].ReadOnly = true;//읽기전용

                        GridMaster.DisableSortColumn(dgv);//오름차순 내림차순 정렬 막기

                        //dgv.AllowUserToResizeColumns = false;//컬럼폭 수정불가
                        dgv.ColumnHeadersVisible = false;//컬럼헤더 가리기                        
                        //dgv.Columns[ 1 ].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";//표시형식

                        //dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;//스페이스 시 줄바꿈
                        //dgv.DefaultCellStyle.BackColor = Color.Black;//색반전
                        //dgv.DefaultCellStyle.ForeColor = Color.White;//색반전
                        dgv.DefaultCellStyle.Font = new Font("Tahoma", 12, FontStyle.Bold);
                        //dgv.DefaultCellStyle.SelectionBackColor = Color.Transparent;
                        //dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
                        //dgv.BackgroundColor = Color.Black;

                        //---------------↑ 설정 ↑---------------┘
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("dgvstatus2");
                    }

                    break;

                case "dgvM1":

                    try
                    {
                        //---------------↓ 기본 ↓---------------┐
                        DataGridView dgv = (DataGridView)Reflection_K.Get(this, name);//이름가져옴
                        string DGV_name = dgv.Name;//적용
                        int height = int.Parse(DataRW.Load_Simple(DGV_name + "H", "30"));//데이터가져옴
                        int fontheader = int.Parse(DataRW.Load_Simple(DGV_name + "FH", "12"));//데이터가져옴
                        int fontcell = int.Parse(DataRW.Load_Simple(DGV_name + "FC", "12"));//데이터가져옴
                        GridMaster.FontSize2(dgv, fontheader, fontcell);//적용
                                                                        //GridMaster.FontSize2( dgv , "New Gulim" , fontheader , fontcell );//한자나 글자 깨질 때 이걸로 사용하세요.
                                                                        //---------------↑ 기본 ↑---------------┘

                        //---------------↓ 생성 ↓---------------┐
                        string[] ColumnsName = new string[] {
                                "A","A"
                            };
                        int rows = 50;//초기 생성 Row수

                        GridMaster.Init3(dgv, true, height, rows, ColumnsName);
                        //---------------↑ 생성 ↑---------------┘

                        //---------------↓ 사용자 데이터 추가 부분 ↓---------------┐
                        //GridMaster.LoadCSV_OnlyData( dgv , System.Windows.Forms.Application.StartupPath + "\\AAAA.csv" );//셀데이터로드
                        GridMaster.LoadCSV_OnlyData(dgv, System.Windows.Forms.Application.StartupPath + "\\Model1.csv");//셀데이터로드
                                                                                                                        //GridMaster.LoadCSV( dgvD0 , @"C:\Users\kclip3\Desktop\CR0.csv" );//셀데이터로드

                        for (int i = 1; i < 51; i++)
                        {
                            dgv.Rows[i - 1].Cells[0].Value = i - 1;
                        }

                        dgv.Rows[0].Cells[0].Value = "Model Num";
                        dgv.Rows[0].Cells[1].Value = "Model Name";


                        //---------------↑ 사용자 데이터 추가 부분 ↑---------------┘

                        //---------------↓ 정렬 ↓---------------┐
                        GridMaster.CenterAlign(dgv);
                        //GridMaster.LeftAlign( dgv );
                        //GridMaster.Align( dgv , 0 , DataGridViewContentAlignment.MiddleLeft );//단일 Column 정렬
                        //---------------↑ 정렬 ↑---------------┘

                        //---------------↓ 설정 ↓---------------┐
                        dgv.ReadOnly = true;//읽기전용
                                            //dgv.Columns[ 0 ].ReadOnly = true;//읽기전용

                        GridMaster.DisableSortColumn(dgv);//오름차순 내림차순 정렬 막기

                        //dgv.AllowUserToResizeColumns = false;//컬럼폭 수정불가
                        dgv.ColumnHeadersVisible = false;//컬럼헤더 가리기                        
                                                         //dgv.Columns[ 1 ].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";//표시형식

                        //dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;//스페이스 시 줄바꿈
                        //dgv.DefaultCellStyle.BackColor = Color.Black;//색반전
                        //dgv.DefaultCellStyle.ForeColor = Color.White;//색반전
                        //dgv.DefaultCellStyle.Font = new Font("Tahoma", 40, FontStyle.Bold);
                        //dgv.DefaultCellStyle.SelectionBackColor = Color.Transparent;
                        //dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
                        //dgv.BackgroundColor = Color.Black;

                        //---------------↑ 설정 ↑---------------┘
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("dgvm1");
                    }

                    break;

                case "dgvM2":

                    try
                    {
                        //---------------↓ 기본 ↓---------------┐
                        DataGridView dgv = (DataGridView)Reflection_K.Get(this, name);//이름가져옴
                        string DGV_name = dgv.Name;//적용
                        int height = int.Parse(DataRW.Load_Simple(DGV_name + "H", "30"));//데이터가져옴
                        int fontheader = int.Parse(DataRW.Load_Simple(DGV_name + "FH", "12"));//데이터가져옴
                        int fontcell = int.Parse(DataRW.Load_Simple(DGV_name + "FC", "12"));//데이터가져옴
                        GridMaster.FontSize2(dgv, fontheader, fontcell);//적용
                                                                        //GridMaster.FontSize2( dgv , "New Gulim" , fontheader , fontcell );//한자나 글자 깨질 때 이걸로 사용하세요.
                                                                        //---------------↑ 기본 ↑---------------┘

                        //---------------↓ 생성 ↓---------------┐
                        string[] ColumnsName = new string[] {
                                "A","A"
                            };
                        int rows = 50;//초기 생성 Row수

                        GridMaster.Init3(dgv, true, height, rows, ColumnsName);
                        //---------------↑ 생성 ↑---------------┘

                        //---------------↓ 사용자 데이터 추가 부분 ↓---------------┐
                        //GridMaster.LoadCSV_OnlyData( dgv , System.Windows.Forms.Application.StartupPath + "\\AAAA.csv" );//셀데이터로드
                        GridMaster.LoadCSV_OnlyData(dgv, System.Windows.Forms.Application.StartupPath + "\\Model2.csv");//셀데이터로드
                                                                                                                        //GridMaster.LoadCSV( dgvD0 , @"C:\Users\kclip3\Desktop\CR0.csv" );//셀데이터로드

                        for (int i = 1; i < 51; i++)
                        {
                            dgv.Rows[i - 1].Cells[0].Value = i - 1;
                        }

                        dgv.Rows[0].Cells[0].Value = "Model Num";
                        dgv.Rows[0].Cells[1].Value = "Model Name";


                        //---------------↑ 사용자 데이터 추가 부분 ↑---------------┘

                        //---------------↓ 정렬 ↓---------------┐
                        GridMaster.CenterAlign(dgv);
                        //GridMaster.LeftAlign( dgv );
                        //GridMaster.Align( dgv , 0 , DataGridViewContentAlignment.MiddleLeft );//단일 Column 정렬
                        //---------------↑ 정렬 ↑---------------┘

                        //---------------↓ 설정 ↓---------------┐
                        dgv.ReadOnly = true;//읽기전용
                                            //dgv.Columns[ 0 ].ReadOnly = true;//읽기전용

                        GridMaster.DisableSortColumn(dgv);//오름차순 내림차순 정렬 막기

                        //dgv.AllowUserToResizeColumns = false;//컬럼폭 수정불가
                        dgv.ColumnHeadersVisible = false;//컬럼헤더 가리기                        
                                                         //dgv.Columns[ 1 ].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";//표시형식

                        //dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;//스페이스 시 줄바꿈
                        //dgv.DefaultCellStyle.BackColor = Color.Black;//색반전
                        //dgv.DefaultCellStyle.ForeColor = Color.White;//색반전
                        //dgv.DefaultCellStyle.Font = new Font("Tahoma", 40, FontStyle.Bold);
                        //dgv.DefaultCellStyle.SelectionBackColor = Color.Transparent;
                        //dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
                        //dgv.BackgroundColor = Color.Black;

                        //---------------↑ 설정 ↑---------------┘
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("dgvm2");
                    }

                    break;
                case "dgvCam1":

                    try
                    {
                        //---------------↓ 기본 ↓---------------┐
                        DataGridView dgv = (DataGridView)Reflection_K.Get(this, name);//이름가져옴
                        string DGV_name = dgv.Name;//적용
                        int height = int.Parse(DataRW.Load_Simple(DGV_name + "H", "30"));//데이터가져옴
                        int fontheader = int.Parse(DataRW.Load_Simple(DGV_name + "FH", "12"));//데이터가져옴
                        int fontcell = int.Parse(DataRW.Load_Simple(DGV_name + "FC", "12"));//데이터가져옴
                        GridMaster.FontSize2(dgv, fontheader, fontcell);//적용
                                                                        //---------------↑ 기본 ↑---------------┘

                        //---------------↓ 생성 ↓---------------┐
                        string[] ColumnsName = new string[] {
                                "A"
                            };
                        int rows = 1;//초기 생성 Row수


                        GridMaster.Init3(dgv, true, height, rows, ColumnsName);
                        //---------------↑ 생성 ↑---------------┘

                        //---------------↓ 사용자 데이터 추가 부분 ↓---------------┐
                        //GridMaster.LoadCSV_OnlyData(dgv, System.Windows.Forms.Application.StartupPath + "\\M.csv");//셀데이터로드
                        //dgv.Rows[ 0 ].Cells[ 0 ].Value = "CORE HEIGHT 1";


                        //for ( int i = 1 ; i < 2 ; i++ )
                        //{
                        //    dgv.Rows [ i - 1 ].Cells [ 0 ].Value = i;
                        //}

                        //---------------↑ 사용자 데이터 추가 부분 ↑---------------┘

                        //---------------↓ 정렬 ↓---------------┐
                        GridMaster.CenterAlign(dgv);
                        //GridMaster.LeftAlign( dgv );
                        //GridMaster.Align( dgv , 0 , DataGridViewContentAlignment.MiddleLeft );//단일 Column 정렬
                        //---------------↑ 정렬 ↑---------------┘

                        //---------------↓ 설정 ↓---------------┐
                        //dgv.ReadOnly = true;//읽기전용
                        //dgv.Rows[0].Cells[0].Selected = true; // 셀0,0 선택
                        //dgv.Columns[ 0 ].ReadOnly = true;//읽기전용

                        GridMaster.DisableSortColumn(dgv);//오름차순 내림차순 정렬 막기

                        //dgv.AllowUserToResizeColumns = false;//컬럼폭 수정불가
                        dgv.ColumnHeadersVisible = false;//컬럼헤더 가리기                        
                                                         //dgv.Columns[ 1 ].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";//표시형식

                        //dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;//스페이스 시 줄바꿈
                        //dgv.DefaultCellStyle.BackColor = Color.Black;//색반전
                        //dgv.DefaultCellStyle.ForeColor = Color.White;//색반전
                        //dgv.DefaultCellStyle.SelectionBackColor = Color.Transparent;
                        //dgv.DefaultCellStyle.SelectionForeColor = Color.Black;

                        //---------------↑ 설정 ↑---------------┘
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("dgv초기화cam1");
                    }

                    break;

                case "dgvCam2":

                    try
                    {
                        //---------------↓ 기본 ↓---------------┐
                        DataGridView dgv = (DataGridView)Reflection_K.Get(this, name);//이름가져옴
                        string DGV_name = dgv.Name;//적용
                        int height = int.Parse(DataRW.Load_Simple(DGV_name + "H", "30"));//데이터가져옴
                        int fontheader = int.Parse(DataRW.Load_Simple(DGV_name + "FH", "12"));//데이터가져옴
                        int fontcell = int.Parse(DataRW.Load_Simple(DGV_name + "FC", "12"));//데이터가져옴
                        GridMaster.FontSize2(dgv, fontheader, fontcell);//적용
                                                                        //---------------↑ 기본 ↑---------------┘

                        //---------------↓ 생성 ↓---------------┐
                        string[] ColumnsName = new string[] {
                                "A"
                            };
                        int rows = 1;//초기 생성 Row수


                        GridMaster.Init3(dgv, true, height, rows, ColumnsName);
                        //---------------↑ 생성 ↑---------------┘

                        //---------------↓ 사용자 데이터 추가 부분 ↓---------------┐
                        //GridMaster.LoadCSV_OnlyData(dgv, System.Windows.Forms.Application.StartupPath + "\\M.csv");//셀데이터로드
                        //dgv.Rows[ 0 ].Cells[ 0 ].Value = "CORE HEIGHT 1";


                        //for ( int i = 1 ; i < 2 ; i++ )
                        //{
                        //    dgv.Rows [ i - 1 ].Cells [ 0 ].Value = i;
                        //}

                        //---------------↑ 사용자 데이터 추가 부분 ↑---------------┘

                        //---------------↓ 정렬 ↓---------------┐
                        GridMaster.CenterAlign(dgv);
                        //GridMaster.LeftAlign( dgv );
                        //GridMaster.Align( dgv , 0 , DataGridViewContentAlignment.MiddleLeft );//단일 Column 정렬
                        //---------------↑ 정렬 ↑---------------┘

                        //---------------↓ 설정 ↓---------------┐
                        //dgv.ReadOnly = true;//읽기전용
                        //dgv.Rows[0].Cells[0].Selected = true; // 셀0,0 선택
                        //dgv.Columns[ 0 ].ReadOnly = true;//읽기전용

                        GridMaster.DisableSortColumn(dgv);//오름차순 내림차순 정렬 막기

                        //dgv.AllowUserToResizeColumns = false;//컬럼폭 수정불가
                        dgv.ColumnHeadersVisible = false;//컬럼헤더 가리기                        
                                                         //dgv.Columns[ 1 ].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";//표시형식

                        //dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;//스페이스 시 줄바꿈
                        //dgv.DefaultCellStyle.BackColor = Color.Black;//색반전
                        //dgv.DefaultCellStyle.ForeColor = Color.White;//색반전
                        //dgv.DefaultCellStyle.SelectionBackColor = Color.Transparent;
                        //dgv.DefaultCellStyle.SelectionForeColor = Color.Black;

                        //---------------↑ 설정 ↑---------------┘
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("dgv초기화cam2");
                    }

                    break;

                case "dgvS1":   //  설정값
                    try
                    {
                        //---------------↓ 기본 ↓---------------┐
                        DataGridView dgv = (DataGridView)Reflection_K.Get(this, name);//이름가져옴
                        string DGV_name = dgv.Name;//적용
                        int height = int.Parse(DataRW.Load_Simple(DGV_name + "H", "30"));//데이터가져옴
                        int fontheader = int.Parse(DataRW.Load_Simple(DGV_name + "FH", "12"));//데이터가져옴
                        int fontcell = int.Parse(DataRW.Load_Simple(DGV_name + "FC", "12"));//데이터가져옴
                        GridMaster.FontSize2(dgv, fontheader, fontcell);//적용
                        //---------------↑ 기본 ↑---------------┘

                        //---------------↓ 생성 ↓---------------┐
                        string[] ColumnsName = new string[] {
                           "Inspection","Min","Max"
                        };
                        int rows = checksetting;//초기 생성 Row수

                        GridMaster.Init3(dgv, true, height, rows, ColumnsName);
                        //---------------↑ 생성 ↑---------------┘

                        //---------------↓ 사용자 데이터 추가 부분 ↓---------------┐
                        //GridMaster.LoadCSV_OnlyData(dgv, System.Windows.Forms.Application.StartupPath + "\\S0.csv");//셀데이터로드
                        //dgv.Rows[ 0 ].Cells[ 0 ].Value = "CORE HEIGHT 1";


                        //for ( int i = 0 ; i < rows ; i++ )
                        //{
                        //    dgv.Rows [ i ].Cells [ 0 ].Value = ( i + 1 );
                        //}

                        //---------------↑ 사용자 데이터 추가 부분 ↑---------------┘

                        //---------------↓ 정렬 ↓---------------┐
                        GridMaster.CenterAlign(dgv);
                        //GridMaster.LeftAlign( dgv );
                        //GridMaster.Align( dgv , 0 , DataGridViewContentAlignment.MiddleLeft );//단일 Column 정렬
                        //---------------↑ 정렬 ↑---------------┘

                        //---------------↓ 설정 ↓---------------┐
                        //dgv.ReadOnly = true;//읽기전용
                        //dgv.Rows[0].Cells[0].Selected = true; // 셀0,0 선택
                        //dgv.Columns[ 0 ].ReadOnly = true;//읽기전용

                        GridMaster.DisableSortColumn(dgv);//오름차순 내림차순 정렬 막기

                        //dgv.AllowUserToResizeColumns = false;//컬럼폭 수정불가
                        //dgv.ColumnHeadersVisible = false;//컬럼헤더 가리기                        
                        //dgv.Columns[ 1 ].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";//표시형식

                        //dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;//스페이스 시 줄바꿈
                        //dgv.DefaultCellStyle.BackColor = Color.Black;//색반전
                        //dgv.DefaultCellStyle.ForeColor = Color.White;//색반전
                        //dgv.DefaultCellStyle.SelectionBackColor = Color.Transparent;
                        //dgv.DefaultCellStyle.SelectionForeColor = Color.Black;

                        //---------------↑ 설정 ↑---------------┘
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("dgv 초기화s1");
                    }
                    break;

                case "dgvS2":   //  설정값
                    try
                    {
                        //---------------↓ 기본 ↓---------------┐
                        DataGridView dgv = (DataGridView)Reflection_K.Get(this, name);//이름가져옴
                        string DGV_name = dgv.Name;//적용
                        int height = int.Parse(DataRW.Load_Simple(DGV_name + "H", "30"));//데이터가져옴
                        int fontheader = int.Parse(DataRW.Load_Simple(DGV_name + "FH", "12"));//데이터가져옴
                        int fontcell = int.Parse(DataRW.Load_Simple(DGV_name + "FC", "12"));//데이터가져옴
                        GridMaster.FontSize2(dgv, fontheader, fontcell);//적용
                        //---------------↑ 기본 ↑---------------┘

                        //---------------↓ 생성 ↓---------------┐
                        string[] ColumnsName = new string[] {
                           "Inspection","Min","Max"
                        };
                        int rows = checksetting2;//초기 생성 Row수

                        GridMaster.Init3(dgv, true, height, rows, ColumnsName);
                        //---------------↑ 생성 ↑---------------┘

                        //---------------↓ 사용자 데이터 추가 부분 ↓---------------┐
                        //GridMaster.LoadCSV_OnlyData(dgv, System.Windows.Forms.Application.StartupPath + "\\S0.csv");//셀데이터로드
                        //dgv.Rows[ 0 ].Cells[ 0 ].Value = "CORE HEIGHT 1";


                        //for ( int i = 0 ; i < rows ; i++ )
                        //{
                        //    dgv.Rows [ i ].Cells [ 0 ].Value = ( i + 1 );
                        //}

                        //---------------↑ 사용자 데이터 추가 부분 ↑---------------┘

                        //---------------↓ 정렬 ↓---------------┐
                        GridMaster.CenterAlign(dgv);
                        //GridMaster.LeftAlign( dgv );
                        //GridMaster.Align( dgv , 0 , DataGridViewContentAlignment.MiddleLeft );//단일 Column 정렬
                        //---------------↑ 정렬 ↑---------------┘

                        //---------------↓ 설정 ↓---------------┐
                        //dgv.ReadOnly = true;//읽기전용
                        //dgv.Rows[0].Cells[0].Selected = true; // 셀0,0 선택
                        //dgv.Columns[ 0 ].ReadOnly = true;//읽기전용

                        GridMaster.DisableSortColumn(dgv);//오름차순 내림차순 정렬 막기

                        //dgv.AllowUserToResizeColumns = false;//컬럼폭 수정불가
                        //dgv.ColumnHeadersVisible = false;//컬럼헤더 가리기                        
                        //dgv.Columns[ 1 ].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";//표시형식

                        //dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;//스페이스 시 줄바꿈
                        //dgv.DefaultCellStyle.BackColor = Color.Black;//색반전
                        //dgv.DefaultCellStyle.ForeColor = Color.White;//색반전
                        //dgv.DefaultCellStyle.SelectionBackColor = Color.Transparent;
                        //dgv.DefaultCellStyle.SelectionForeColor = Color.Black;

                        //---------------↑ 설정 ↑---------------┘
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("dgv 초기화s2");
                    }
                    break;

                case "dgvC1":

                    try
                    {
                        //---------------↓ 기본 ↓---------------┐
                        DataGridView dgv = (DataGridView)Reflection_K.Get(this, name);//이름가져옴
                        string DGV_name = dgv.Name;//적용
                        int height = int.Parse(DataRW.Load_Simple(DGV_name + "H", "30"));//데이터가져옴
                        int fontheader = int.Parse(DataRW.Load_Simple(DGV_name + "FH", "12"));//데이터가져옴
                        int fontcell = int.Parse(DataRW.Load_Simple(DGV_name + "FC", "12"));//데이터가져옴
                        GridMaster.FontSize2(dgv, fontheader, fontcell);//적용
                        //---------------↑ 기본 ↑---------------┘

                        //---------------↓ 생성 ↓---------------┐
                        string[] ColumnsName = new string[] {
                            "번지" , "내용" , "Data"
                        };
                        int rows = 500;//초기 생성 Row수

                        GridMaster.Init3(dgv, true, height, rows, ColumnsName);
                        //---------------↑ 생성 ↑---------------┘

                        //---------------↓ 사용자 데이터 추가 부분 ↓---------------┐
                        //GridMaster.LoadCSV_OnlyData( dgv , System.Windows.Forms.Application.StartupPath + "\\AAAA.csv" );//셀데이터로드
                        GridMaster.LoadCSV_OnlyData( dgv, System.Windows.Forms.Application.StartupPath + "\\P1.csv" );//셀데이터로드
                        //dgv.Rows[ 0 ].Cells[ 0 ].Value = "CORE HEIGHT 1";

                        for ( int i = 0; i < rows; i++)
                        {
                            dgv.Rows[i].Cells[0].Value = "D" + (i + 20000);
                        }

                        //---------------↑ 사용자 데이터 추가 부분 ↑---------------┘

                        //---------------↓ 정렬 ↓---------------┐
                        GridMaster.CenterAlign(dgv);
                        //GridMaster.LeftAlign( dgv );
                        //GridMaster.Align( dgv , 0 , DataGridViewContentAlignment.MiddleLeft );//단일 Column 정렬
                        //---------------↑ 정렬 ↑---------------┘

                        //---------------↓ 설정 ↓---------------┐
                        //dgv.ReadOnly = true;//읽기전용
                        GridMaster.DisableSortColumn(dgv);//오름차순 내림차순 정렬 막기
                                                          //dgv.Columns[ 0 ].ReadOnly = true;//읽기전용
                                                          //dgv.AllowUserToResizeColumns = false;//컬럼폭 수정불가

                        //dgv.Columns[ 1 ].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";//표시형식
                        //dgv.ColumnHeadersVisible = false;//컬럼헤더 가리기                        
                        //dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;//스페이스 시 줄바꿈
                        //dgv.DefaultCellStyle.BackColor = Color.Black;//색반전
                        //dgv.DefaultCellStyle.ForeColor = Color.White;//색반전

                        //---------------↑ 설정 ↑---------------┘

                    }
                    catch (Exception)
                    {

                    }

                    break;


                //case "dgvH0":

                //    try
                //    {
                //        //---------------↓ 기본 ↓---------------┐
                //        DataGridView dgv = (DataGridView)Reflection_K.Get(this, name);//이름가져옴
                //        string DGV_name = dgv.Name;//적용
                //        int height = int.Parse(DataRW.Load_Simple(DGV_name + "H", "30"));//데이터가져옴
                //        int fontheader = int.Parse(DataRW.Load_Simple(DGV_name + "FH", "12"));//데이터가져옴
                //        int fontcell = int.Parse(DataRW.Load_Simple(DGV_name + "FC", "12"));//데이터가져옴
                //        GridMaster.FontSize2(dgv, fontheader, fontcell);//적용
                //        //---------------↑ 기본 ↑---------------┘

                //        //---------------↓ 생성 ↓---------------┐
                //        string[] ColumnsName = new string[] {
                //            //"A","A","A","A","A","A","A","A"
                //            };
                //        int rows = 0;//초기 생성 Row수

                //        GridMaster.Init3(dgv, false, height, rows, ColumnsName);
                //        //---------------↑ 생성 ↑---------------┘

                //        //---------------↓ 사용자 데이터 추가 부분 ↓---------------┐
                //        //GridMaster.LoadCSV_OnlyData( dgv , System.Windows.Forms.Application.StartupPath + "\\AAAA.csv" );//셀데이터로드
                //        //dgv.Rows[ 0 ].Cells[ 0 ].Value = "CORE HEIGHT 1";

                //        //dgv.Columns[0].HeaderText = "Model";
                //        //dgv.Columns[1].HeaderText = "DateTime";
                //        //dgv.Columns[2].HeaderText = "Result";
                //        //dgv.Columns[3].HeaderText = "Area 1";
                //        //dgv.Columns[4].HeaderText = "Area 2";
                //        //dgv.Columns[5].HeaderText = "Area 3";
                //        //dgv.Columns[6].HeaderText = "Area 4";
                //        //dgv.Columns[7].HeaderText = "Area 5";

                //        //dgv.Columns[0].HeaderText = "Model";
                //        //dgv.Columns[1].HeaderText = "Datetime";
                //        //dgv.Columns[2].HeaderText = "Barcode";
                //        //dgv.Columns[3].HeaderText = "CamNum";
                //        //dgv.Columns[4].HeaderText = "CamResult1";
                //        //dgv.Columns[5].HeaderText = "CamResult2";
                //        //dgv.Columns[6].HeaderText = "CapNum";
                //        //dgv.Columns[7].HeaderText = "CapResult";
                //        //dgv.Columns[8].HeaderText = "Wash";
                //        //dgv.Columns[9].HeaderText = "Cap1";
                //        //dgv.Columns[10].HeaderText = "Cap2";
                //        //dgv.Columns[11].HeaderText = "Cap3";
                //        //dgv.Columns[12].HeaderText = "LVDTResult";
                //        //dgv.Columns[13].HeaderText = "LVDT1";
                //        //dgv.Columns[14].HeaderText = "LVDT2";
                //        //dgv.Columns[15].HeaderText = "LVDT3";
                //        //dgv.Columns[16].HeaderText = "LVDT4";
                //        //dgv.Columns[17].HeaderText = "LVDT5";
                //        //dgv.Columns[18].HeaderText = "LVDT6";
                //        //dgv.Columns[19].HeaderText = "LVDT7";
                //        //dgv.Columns[20].HeaderText = "LVDT8";
                //        //dgv.Columns[21].HeaderText = "LVDT9";
                //        //dgv.Columns[22].HeaderText = "GasLeakNum";
                //        //dgv.Columns[23].HeaderText = "GasLeakResult";
                //        //dgv.Columns[24].HeaderText = "GasLeak1";
                //        //dgv.Columns[25].HeaderText = "WaterLeakNum";
                //        //dgv.Columns[26].HeaderText = "WaterLeakResult";
                //        //dgv.Columns[27].HeaderText = "WaterLeak1";

                //        //dgv.Columns[1].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";

                //        //---------------↓ OKNG 색칠 ↓---------------┐

                //        //GridMaster.Color_Painting(dgv, 5);
                //        //GridMaster.Color_Painting(dgv, 12);

                //        //GridMaster.Color_Painting(dgv, 17);
                //        //GridMaster.Color_Painting(dgv, 19);
                //        //GridMaster.Color_Painting(dgv, 21);
                //        //GridMaster.Color_Painting(dgv, 23);
                //        //GridMaster.Color_Painting(dgv, 27);
                //        //GridMaster.Color_Painting(dgv, 38);



                //        //---------------↑ OKNG 색칠 ↑---------------┘



                //        //---------------↑ 사용자 데이터 추가 부분 ↑---------------┘

                //        //---------------↓ 정렬 ↓---------------┐
                //        GridMaster.CenterAlign(dgv);
                //        //GridMaster.LeftAlign( dgv );
                //        //GridMaster.Align( dgv , 0 , DataGridViewContentAlignment.MiddleLeft );//단일 Column 정렬
                //        //---------------↑ 정렬 ↑---------------┘

                //        //---------------↓ 설정 ↓---------------┐
                //        dgv.ReadOnly = true;//읽기전용
                //        //GridMaster.DisableSortColumn(dgv);//오름차순 내림차순 정렬 막기
                //        //dgv.Columns[0].ReadOnly = true;//읽기전용
                //        //dgv.AllowUserToResizeColumns = false;//컬럼폭 수정불가
                //        dgv.Columns[1].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";//표시형식
                //        //dgv.ColumnHeadersVisible = false;//컬럼헤더 가리기                        
                //        //dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;//스페이스 시 줄바꿈
                //        //dgv.DefaultCellStyle.BackColor = Color.Black;//색반전
                //        //dgv.DefaultCellStyle.ForeColor = Color.White;//색반전

                //        //---------------↑ 설정 ↑---------------┘

                //        //for ( int i = 0 ; i < dgvH0.RowCount ; i++ )
                //        //{
                //        //    for ( int j = 3 ; j < 8 ; j++ )
                //        //    {
                //        //        if ( dgvH0.Rows [ i ].Cells [ j ].Value.ToString( ) != "1" )
                //        //        {
                //        //            dgvH0.Rows [ i ].Cells [ j ].Style.BackColor = Color.Crimson;
                //        //        }
                //        //    }
                //        //}
                //    }
                //    catch (Exception)
                //    {
                //        Console.WriteLine("dgvH0");
                //    }

                //    break;


                case "dgvH0":

                    try
                    {
                        //---------------↓ 기본 ↓---------------┐
                        DataGridView dgv = (DataGridView)Reflection_K.Get(this, name);//이름가져옴
                        string DGV_name = dgv.Name;//적용
                        int height = int.Parse(DataRW.Load_Simple(DGV_name + "H", "30"));//데이터가져옴
                        int fontheader = int.Parse(DataRW.Load_Simple(DGV_name + "FH", "12"));//데이터가져옴
                        int fontcell = int.Parse(DataRW.Load_Simple(DGV_name + "FC", "12"));//데이터가져옴
                        GridMaster.FontSize2(dgv, fontheader, fontcell);//적용
                        //---------------↑ 기본 ↑---------------┘

                        //---------------↓ 생성 ↓---------------┐
                        string[] ColumnsName = new string[] {
                            //"A","A","A","A","A","A","A","A"
                            };
                        int rows = 0;//초기 생성 Row수

                        GridMaster.Init3(dgv, false, height, rows, ColumnsName);
                        //---------------↑ 생성 ↑---------------┘

                        //---------------↓ 사용자 데이터 추가 부분 ↓---------------┐
                        //GridMaster.LoadCSV_OnlyData( dgv , System.Windows.Forms.Application.StartupPath + "\\AAAA.csv" );//셀데이터로드
                        //dgv.Rows[ 0 ].Cells[ 0 ].Value = "CORE HEIGHT 1";

                        //dgv.Columns[0].HeaderText = "Model";
                        //dgv.Columns[1].HeaderText = "DateTime";
                        //dgv.Columns[2].HeaderText = "Result";
                        //dgv.Columns[3].HeaderText = "Area 1";
                        //dgv.Columns[4].HeaderText = "Area 2";
                        //dgv.Columns[5].HeaderText = "Area 3";
                        //dgv.Columns[6].HeaderText = "Area 4";
                        //dgv.Columns[7].HeaderText = "Area 5";

                        //dgv.Columns[1].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";

                        //---------------↓ OKNG 색칠 ↓---------------┐

                        //GridMaster.Color_Painting(dgv, 5);
                        //GridMaster.Color_Painting(dgv, 12);

                        //GridMaster.Color_Painting(dgv, 17);
                        //GridMaster.Color_Painting(dgv, 19);
                        //GridMaster.Color_Painting(dgv, 21);
                        //GridMaster.Color_Painting(dgv, 23);
                        //GridMaster.Color_Painting(dgv, 27);
                        //GridMaster.Color_Painting(dgv, 38);



                        //---------------↑ OKNG 색칠 ↑---------------┘



                        //---------------↑ 사용자 데이터 추가 부분 ↑---------------┘

                        //---------------↓ 정렬 ↓---------------┐
                        GridMaster.CenterAlign(dgv);
                        //GridMaster.LeftAlign( dgv );
                        //GridMaster.Align( dgv , 0 , DataGridViewContentAlignment.MiddleLeft );//단일 Column 정렬
                        //---------------↑ 정렬 ↑---------------┘

                        //---------------↓ 설정 ↓---------------┐
                        dgv.ReadOnly = true;//읽기전용
                        //GridMaster.DisableSortColumn( dgv );//오름차순 내림차순 정렬 막기
                        dgv.Columns[0].ReadOnly = true;//읽기전용
                        //dgv.AllowUserToResizeColumns = false;//컬럼폭 수정불가
                        dgv.Columns[1].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";//표시형식
                        //dgv.ColumnHeadersVisible = false;//컬럼헤더 가리기                        
                        //dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;//스페이스 시 줄바꿈
                        //dgv.DefaultCellStyle.BackColor = Color.Black;//색반전
                        //dgv.DefaultCellStyle.ForeColor = Color.White;//색반전
                        //dgv.Columns[1].SortMode = DataGridViewColumnSortMode.Programmatic;
                        //dgv.Columns[0].HeaderCell.SortGlyphDirection = SortOrder.Ascending;
                        //---------------↑ 설정 ↑---------------┘

                        //for ( int i = 0 ; i < dgvH0.RowCount ; i++ )
                        //{
                        //    for ( int j = 3 ; j < 8 ; j++ )
                        //    {
                        //        if ( dgvH0.Rows [ i ].Cells [ j ].Value.ToString( ) != "1" )
                        //        {
                        //            dgvH0.Rows [ i ].Cells [ j ].Style.BackColor = Color.Crimson;
                        //        }
                        //    }
                        //}
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("dgvH0");
                    }

                    break;

                case "dgvH1":

                    try
                    {
                        //---------------↓ 기본 ↓---------------┐
                        DataGridView dgv = (DataGridView)Reflection_K.Get(this, name);//이름가져옴
                        string DGV_name = dgv.Name;//적용
                        int height = int.Parse(DataRW.Load_Simple(DGV_name + "H", "30"));//데이터가져옴
                        int fontheader = int.Parse(DataRW.Load_Simple(DGV_name + "FH", "12"));//데이터가져옴
                        int fontcell = int.Parse(DataRW.Load_Simple(DGV_name + "FC", "12"));//데이터가져옴
                        GridMaster.FontSize2(dgv, fontheader, fontcell);//적용
                        //---------------↑ 기본 ↑---------------┘

                        //---------------↓ 생성 ↓---------------┐
                        string[] ColumnsName = new string[] {
                            //"A","A","A","A","A","A","A","A"
                            };
                        int rows = 0;//초기 생성 Row수

                        GridMaster.Init3(dgv, false, height, rows, ColumnsName);
                        //---------------↑ 생성 ↑---------------┘

                        //---------------↓ 사용자 데이터 추가 부분 ↓---------------┐
                        //GridMaster.LoadCSV_OnlyData( dgv , System.Windows.Forms.Application.StartupPath + "\\AAAA.csv" );//셀데이터로드
                        //dgv.Rows[ 0 ].Cells[ 0 ].Value = "CORE HEIGHT 1";

                        //dgv.Columns[0].HeaderText = "Model";
                        //dgv.Columns[1].HeaderText = "DateTime";
                        //dgv.Columns[2].HeaderText = "Result";
                        //dgv.Columns[3].HeaderText = "Area 1";
                        //dgv.Columns[4].HeaderText = "Area 2";
                        //dgv.Columns[5].HeaderText = "Area 3";
                        //dgv.Columns[6].HeaderText = "Area 4";
                        //dgv.Columns[7].HeaderText = "Area 5";

                        //dgv.Columns[1].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";

                        //---------------↓ OKNG 색칠 ↓---------------┐

                        //GridMaster.Color_Painting(dgv, 5);
                        //GridMaster.Color_Painting(dgv, 12);

                        //GridMaster.Color_Painting(dgv, 17);
                        //GridMaster.Color_Painting(dgv, 19);
                        //GridMaster.Color_Painting(dgv, 21);
                        //GridMaster.Color_Painting(dgv, 23);
                        //GridMaster.Color_Painting(dgv, 27);
                        //GridMaster.Color_Painting(dgv, 38);



                        //---------------↑ OKNG 색칠 ↑---------------┘



                        //---------------↑ 사용자 데이터 추가 부분 ↑---------------┘

                        //---------------↓ 정렬 ↓---------------┐
                        GridMaster.CenterAlign(dgv);
                        //GridMaster.LeftAlign( dgv );
                        //GridMaster.Align( dgv , 0 , DataGridViewContentAlignment.MiddleLeft );//단일 Column 정렬
                        //---------------↑ 정렬 ↑---------------┘

                        //---------------↓ 설정 ↓---------------┐
                        dgv.ReadOnly = true;//읽기전용
                        //GridMaster.DisableSortColumn( dgv );//오름차순 내림차순 정렬 막기
                        dgv.Columns[0].ReadOnly = true;//읽기전용
                        //dgv.AllowUserToResizeColumns = false;//컬럼폭 수정불가
                        //dgv.Columns[1].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";//표시형식
                        //dgv.ColumnHeadersVisible = false;//컬럼헤더 가리기                        
                        //dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;//스페이스 시 줄바꿈
                        //dgv.DefaultCellStyle.BackColor = Color.Black;//색반전
                        //dgv.DefaultCellStyle.ForeColor = Color.White;//색반전

                        //---------------↑ 설정 ↑---------------┘

                        //for ( int i = 0 ; i < dgvH0.RowCount ; i++ )
                        //{
                        //    for ( int j = 3 ; j < 8 ; j++ )
                        //    {
                        //        if ( dgvH0.Rows [ i ].Cells [ j ].Value.ToString( ) != "1" )
                        //        {
                        //            dgvH0.Rows [ i ].Cells [ j ].Style.BackColor = Color.Crimson;
                        //        }
                        //    }
                        //}
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("dgvH1");
                    }

                    break;

                case "dgvH2":

                    try
                    {
                        //---------------↓ 기본 ↓---------------┐
                        DataGridView dgv = (DataGridView)Reflection_K.Get(this, name);//이름가져옴
                        string DGV_name = dgv.Name;//적용
                        int height = int.Parse(DataRW.Load_Simple(DGV_name + "H", "30"));//데이터가져옴
                        int fontheader = int.Parse(DataRW.Load_Simple(DGV_name + "FH", "12"));//데이터가져옴
                        int fontcell = int.Parse(DataRW.Load_Simple(DGV_name + "FC", "12"));//데이터가져옴
                        GridMaster.FontSize2(dgv, fontheader, fontcell);//적용
                        //---------------↑ 기본 ↑---------------┘

                        //---------------↓ 생성 ↓---------------┐
                        string[] ColumnsName = new string[] {
                            //"A","A","A","A","A","A","A","A"
                            };
                        int rows = 0;//초기 생성 Row수

                        GridMaster.Init3(dgv, false, height, rows, ColumnsName);
                        //---------------↑ 생성 ↑---------------┘

                        //---------------↓ 사용자 데이터 추가 부분 ↓---------------┐
                        //GridMaster.LoadCSV_OnlyData( dgv , System.Windows.Forms.Application.StartupPath + "\\AAAA.csv" );//셀데이터로드
                        //dgv.Rows[ 0 ].Cells[ 0 ].Value = "CORE HEIGHT 1";

                        //dgv.Columns[0].HeaderText = "Model";
                        //dgv.Columns[1].HeaderText = "DateTime";
                        //dgv.Columns[2].HeaderText = "Result";
                        //dgv.Columns[3].HeaderText = "Area 1";
                        //dgv.Columns[4].HeaderText = "Area 2";
                        //dgv.Columns[5].HeaderText = "Area 3";
                        //dgv.Columns[6].HeaderText = "Area 4";
                        //dgv.Columns[7].HeaderText = "Area 5";

                        //dgv.Columns[1].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";

                        //---------------↓ OKNG 색칠 ↓---------------┐

                        //GridMaster.Color_Painting(dgv, 5);
                        //GridMaster.Color_Painting(dgv, 12);

                        //GridMaster.Color_Painting(dgv, 17);
                        //GridMaster.Color_Painting(dgv, 19);
                        //GridMaster.Color_Painting(dgv, 21);
                        //GridMaster.Color_Painting(dgv, 23);
                        //GridMaster.Color_Painting(dgv, 27);
                        //GridMaster.Color_Painting(dgv, 38);



                        //---------------↑ OKNG 색칠 ↑---------------┘



                        //---------------↑ 사용자 데이터 추가 부분 ↑---------------┘

                        //---------------↓ 정렬 ↓---------------┐
                        GridMaster.CenterAlign(dgv);
                        //GridMaster.LeftAlign( dgv );
                        //GridMaster.Align( dgv , 0 , DataGridViewContentAlignment.MiddleLeft );//단일 Column 정렬
                        //---------------↑ 정렬 ↑---------------┘

                        //---------------↓ 설정 ↓---------------┐
                        dgv.ReadOnly = true;//읽기전용
                        //GridMaster.DisableSortColumn( dgv );//오름차순 내림차순 정렬 막기
                        dgv.Columns[0].ReadOnly = true;//읽기전용
                        //dgv.AllowUserToResizeColumns = false;//컬럼폭 수정불가
                        //dgv.Columns[1].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";//표시형식
                        //dgv.ColumnHeadersVisible = false;//컬럼헤더 가리기                        
                        //dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;//스페이스 시 줄바꿈
                        //dgv.DefaultCellStyle.BackColor = Color.Black;//색반전
                        //dgv.DefaultCellStyle.ForeColor = Color.White;//색반전

                        //---------------↑ 설정 ↑---------------┘

                        //for ( int i = 0 ; i < dgvH0.RowCount ; i++ )
                        //{
                        //    for ( int j = 3 ; j < 8 ; j++ )
                        //    {
                        //        if ( dgvH0.Rows [ i ].Cells [ j ].Value.ToString( ) != "1" )
                        //        {
                        //            dgvH0.Rows [ i ].Cells [ j ].Style.BackColor = Color.Crimson;
                        //        }
                        //    }
                        //}
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("dgvH2");
                    }

                    break;

                case "dgvH3":

                    try
                    {
                        //---------------↓ 기본 ↓---------------┐
                        DataGridView dgv = (DataGridView)Reflection_K.Get(this, name);//이름가져옴
                        string DGV_name = dgv.Name;//적용
                        int height = int.Parse(DataRW.Load_Simple(DGV_name + "H", "30"));//데이터가져옴
                        int fontheader = int.Parse(DataRW.Load_Simple(DGV_name + "FH", "12"));//데이터가져옴
                        int fontcell = int.Parse(DataRW.Load_Simple(DGV_name + "FC", "12"));//데이터가져옴
                        GridMaster.FontSize2(dgv, fontheader, fontcell);//적용
                        //---------------↑ 기본 ↑---------------┘

                        //---------------↓ 생성 ↓---------------┐
                        string[] ColumnsName = new string[] {
                            //"A","A","A","A","A","A","A","A"
                            };
                        int rows = 0;//초기 생성 Row수

                        GridMaster.Init3(dgv, false, height, rows, ColumnsName);
                        //---------------↑ 생성 ↑---------------┘

                        //---------------↓ 사용자 데이터 추가 부분 ↓---------------┐
                        //GridMaster.LoadCSV_OnlyData( dgv , System.Windows.Forms.Application.StartupPath + "\\AAAA.csv" );//셀데이터로드
                        //dgv.Rows[ 0 ].Cells[ 0 ].Value = "CORE HEIGHT 1";

                        //dgv.Columns[0].HeaderText = "Model";
                        //dgv.Columns[1].HeaderText = "DateTime";
                        //dgv.Columns[2].HeaderText = "Result";
                        //dgv.Columns[3].HeaderText = "Area 1";
                        //dgv.Columns[4].HeaderText = "Area 2";
                        //dgv.Columns[5].HeaderText = "Area 3";
                        //dgv.Columns[6].HeaderText = "Area 4";
                        //dgv.Columns[7].HeaderText = "Area 5";

                        //dgv.Columns[1].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";

                        //---------------↓ OKNG 색칠 ↓---------------┐

                        //GridMaster.Color_Painting(dgv, 5);
                        //GridMaster.Color_Painting(dgv, 12);

                        //GridMaster.Color_Painting(dgv, 17);
                        //GridMaster.Color_Painting(dgv, 19);
                        //GridMaster.Color_Painting(dgv, 21);
                        //GridMaster.Color_Painting(dgv, 23);
                        //GridMaster.Color_Painting(dgv, 27);
                        //GridMaster.Color_Painting(dgv, 38);



                        //---------------↑ OKNG 색칠 ↑---------------┘



                        //---------------↑ 사용자 데이터 추가 부분 ↑---------------┘

                        //---------------↓ 정렬 ↓---------------┐
                        GridMaster.CenterAlign(dgv);
                        //GridMaster.LeftAlign( dgv );
                        //GridMaster.Align( dgv , 0 , DataGridViewContentAlignment.MiddleLeft );//단일 Column 정렬
                        //---------------↑ 정렬 ↑---------------┘

                        //---------------↓ 설정 ↓---------------┐
                        dgv.ReadOnly = true;//읽기전용
                        //GridMaster.DisableSortColumn( dgv );//오름차순 내림차순 정렬 막기
                        dgv.Columns[0].ReadOnly = true;//읽기전용
                        //dgv.AllowUserToResizeColumns = false;//컬럼폭 수정불가
                        //dgv.Columns[1].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";//표시형식
                        //dgv.ColumnHeadersVisible = false;//컬럼헤더 가리기                        
                        //dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;//스페이스 시 줄바꿈
                        //dgv.DefaultCellStyle.BackColor = Color.Black;//색반전
                        //dgv.DefaultCellStyle.ForeColor = Color.White;//색반전

                        //---------------↑ 설정 ↑---------------┘

                        //for ( int i = 0 ; i < dgvH0.RowCount ; i++ )
                        //{
                        //    for ( int j = 3 ; j < 8 ; j++ )
                        //    {
                        //        if ( dgvH0.Rows [ i ].Cells [ j ].Value.ToString( ) != "1" )
                        //        {
                        //            dgvH0.Rows [ i ].Cells [ j ].Style.BackColor = Color.Crimson;
                        //        }
                        //    }
                        //}
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("dgvH3");
                    }

                    break;

                case "dgvH4":

                    try
                    {
                        //---------------↓ 기본 ↓---------------┐
                        DataGridView dgv = (DataGridView)Reflection_K.Get(this, name);//이름가져옴
                        string DGV_name = dgv.Name;//적용
                        int height = int.Parse(DataRW.Load_Simple(DGV_name + "H", "30"));//데이터가져옴
                        int fontheader = int.Parse(DataRW.Load_Simple(DGV_name + "FH", "12"));//데이터가져옴
                        int fontcell = int.Parse(DataRW.Load_Simple(DGV_name + "FC", "12"));//데이터가져옴
                        GridMaster.FontSize2(dgv, fontheader, fontcell);//적용
                        //---------------↑ 기본 ↑---------------┘

                        //---------------↓ 생성 ↓---------------┐
                        string[] ColumnsName = new string[] {
                            //"A","A","A","A","A","A","A","A"
                            };
                        int rows = 0;//초기 생성 Row수

                        GridMaster.Init3(dgv, false, height, rows, ColumnsName);
                        //---------------↑ 생성 ↑---------------┘

                        //---------------↓ 사용자 데이터 추가 부분 ↓---------------┐
                        //GridMaster.LoadCSV_OnlyData( dgv , System.Windows.Forms.Application.StartupPath + "\\AAAA.csv" );//셀데이터로드
                        //dgv.Rows[ 0 ].Cells[ 0 ].Value = "CORE HEIGHT 1";

                        //dgv.Columns[0].HeaderText = "Model";
                        //dgv.Columns[1].HeaderText = "DateTime";
                        //dgv.Columns[2].HeaderText = "Result";
                        //dgv.Columns[3].HeaderText = "Area 1";
                        //dgv.Columns[4].HeaderText = "Area 2";
                        //dgv.Columns[5].HeaderText = "Area 3";
                        //dgv.Columns[6].HeaderText = "Area 4";
                        //dgv.Columns[7].HeaderText = "Area 5";

                        //dgv.Columns[1].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";

                        //---------------↓ OKNG 색칠 ↓---------------┐

                        //GridMaster.Color_Painting(dgv, 5);
                        //GridMaster.Color_Painting(dgv, 12);

                        //GridMaster.Color_Painting(dgv, 17);
                        //GridMaster.Color_Painting(dgv, 19);
                        //GridMaster.Color_Painting(dgv, 21);
                        //GridMaster.Color_Painting(dgv, 23);
                        //GridMaster.Color_Painting(dgv, 27);
                        //GridMaster.Color_Painting(dgv, 38);



                        //---------------↑ OKNG 색칠 ↑---------------┘



                        //---------------↑ 사용자 데이터 추가 부분 ↑---------------┘

                        //---------------↓ 정렬 ↓---------------┐
                        GridMaster.CenterAlign(dgv);
                        //GridMaster.LeftAlign( dgv );
                        //GridMaster.Align( dgv , 0 , DataGridViewContentAlignment.MiddleLeft );//단일 Column 정렬
                        //---------------↑ 정렬 ↑---------------┘

                        //---------------↓ 설정 ↓---------------┐
                        dgv.ReadOnly = true;//읽기전용
                        //GridMaster.DisableSortColumn( dgv );//오름차순 내림차순 정렬 막기
                        dgv.Columns[0].ReadOnly = true;//읽기전용
                        //dgv.AllowUserToResizeColumns = false;//컬럼폭 수정불가
                        //dgv.Columns[1].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";//표시형식
                        //dgv.ColumnHeadersVisible = false;//컬럼헤더 가리기                        
                        //dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;//스페이스 시 줄바꿈
                        //dgv.DefaultCellStyle.BackColor = Color.Black;//색반전
                        //dgv.DefaultCellStyle.ForeColor = Color.White;//색반전

                        //---------------↑ 설정 ↑---------------┘

                        //for ( int i = 0 ; i < dgvH0.RowCount ; i++ )
                        //{
                        //    for ( int j = 3 ; j < 8 ; j++ )
                        //    {
                        //        if ( dgvH0.Rows [ i ].Cells [ j ].Value.ToString( ) != "1" )
                        //        {
                        //            dgvH0.Rows [ i ].Cells [ j ].Style.BackColor = Color.Crimson;
                        //        }
                        //    }
                        //}
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("dgvH4");
                    }

                    break;

                case "dgvH5":

                    try
                    {
                        //---------------↓ 기본 ↓---------------┐
                        DataGridView dgv = (DataGridView)Reflection_K.Get(this, name);//이름가져옴
                        string DGV_name = dgv.Name;//적용
                        int height = int.Parse(DataRW.Load_Simple(DGV_name + "H", "30"));//데이터가져옴
                        int fontheader = int.Parse(DataRW.Load_Simple(DGV_name + "FH", "12"));//데이터가져옴
                        int fontcell = int.Parse(DataRW.Load_Simple(DGV_name + "FC", "12"));//데이터가져옴
                        GridMaster.FontSize2(dgv, fontheader, fontcell);//적용
                        //---------------↑ 기본 ↑---------------┘

                        //---------------↓ 생성 ↓---------------┐
                        string[] ColumnsName = new string[] {
                            //"A","A","A","A","A","A","A","A"
                            };
                        int rows = 0;//초기 생성 Row수

                        GridMaster.Init3(dgv, false, height, rows, ColumnsName);
                        //---------------↑ 생성 ↑---------------┘

                        //---------------↓ 사용자 데이터 추가 부분 ↓---------------┐
                        //GridMaster.LoadCSV_OnlyData( dgv , System.Windows.Forms.Application.StartupPath + "\\AAAA.csv" );//셀데이터로드
                        //dgv.Rows[ 0 ].Cells[ 0 ].Value = "CORE HEIGHT 1";

                        //dgv.Columns[0].HeaderText = "Model";
                        //dgv.Columns[1].HeaderText = "DateTime";
                        //dgv.Columns[2].HeaderText = "Result";
                        //dgv.Columns[3].HeaderText = "Area 1";
                        //dgv.Columns[4].HeaderText = "Area 2";
                        //dgv.Columns[5].HeaderText = "Area 3";
                        //dgv.Columns[6].HeaderText = "Area 4";
                        //dgv.Columns[7].HeaderText = "Area 5";

                        //dgv.Columns[1].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";

                        //---------------↓ OKNG 색칠 ↓---------------┐

                        //GridMaster.Color_Painting(dgv, 5);
                        //GridMaster.Color_Painting(dgv, 12);

                        //GridMaster.Color_Painting(dgv, 17);
                        //GridMaster.Color_Painting(dgv, 19);
                        //GridMaster.Color_Painting(dgv, 21);
                        //GridMaster.Color_Painting(dgv, 23);
                        //GridMaster.Color_Painting(dgv, 27);
                        //GridMaster.Color_Painting(dgv, 38);



                        //---------------↑ OKNG 색칠 ↑---------------┘



                        //---------------↑ 사용자 데이터 추가 부분 ↑---------------┘

                        //---------------↓ 정렬 ↓---------------┐
                        GridMaster.CenterAlign(dgv);
                        //GridMaster.LeftAlign( dgv );
                        //GridMaster.Align( dgv , 0 , DataGridViewContentAlignment.MiddleLeft );//단일 Column 정렬
                        //---------------↑ 정렬 ↑---------------┘

                        //---------------↓ 설정 ↓---------------┐
                        dgv.ReadOnly = true;//읽기전용
                        //GridMaster.DisableSortColumn( dgv );//오름차순 내림차순 정렬 막기
                        dgv.Columns[0].ReadOnly = true;//읽기전용
                        //dgv.AllowUserToResizeColumns = false;//컬럼폭 수정불가
                        //dgv.Columns[1].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";//표시형식
                        //dgv.ColumnHeadersVisible = false;//컬럼헤더 가리기                        
                        //dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;//스페이스 시 줄바꿈
                        //dgv.DefaultCellStyle.BackColor = Color.Black;//색반전
                        //dgv.DefaultCellStyle.ForeColor = Color.White;//색반전

                        //---------------↑ 설정 ↑---------------┘

                        //for ( int i = 0 ; i < dgvH0.RowCount ; i++ )
                        //{
                        //    for ( int j = 3 ; j < 8 ; j++ )
                        //    {
                        //        if ( dgvH0.Rows [ i ].Cells [ j ].Value.ToString( ) != "1" )
                        //        {
                        //            dgvH0.Rows [ i ].Cells [ j ].Style.BackColor = Color.Crimson;
                        //        }
                        //    }
                        //}
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("dgvH5");
                    }

                    break;

                case "dgvH6":

                    try
                    {
                        //---------------↓ 기본 ↓---------------┐
                        DataGridView dgv = (DataGridView)Reflection_K.Get(this, name);//이름가져옴
                        string DGV_name = dgv.Name;//적용
                        int height = int.Parse(DataRW.Load_Simple(DGV_name + "H", "30"));//데이터가져옴
                        int fontheader = int.Parse(DataRW.Load_Simple(DGV_name + "FH", "12"));//데이터가져옴
                        int fontcell = int.Parse(DataRW.Load_Simple(DGV_name + "FC", "12"));//데이터가져옴
                        GridMaster.FontSize2(dgv, fontheader, fontcell);//적용
                        //---------------↑ 기본 ↑---------------┘

                        //---------------↓ 생성 ↓---------------┐
                        string[] ColumnsName = new string[] {
                            //"A","A","A","A","A","A","A","A"
                            };
                        int rows = 0;//초기 생성 Row수

                        GridMaster.Init3(dgv, false, height, rows, ColumnsName);
                        //---------------↑ 생성 ↑---------------┘

                        //---------------↓ 사용자 데이터 추가 부분 ↓---------------┐
                        //GridMaster.LoadCSV_OnlyData( dgv , System.Windows.Forms.Application.StartupPath + "\\AAAA.csv" );//셀데이터로드
                        //dgv.Rows[ 0 ].Cells[ 0 ].Value = "CORE HEIGHT 1";

                        //dgv.Columns[0].HeaderText = "Model";
                        //dgv.Columns[1].HeaderText = "DateTime";
                        //dgv.Columns[2].HeaderText = "Result";
                        //dgv.Columns[3].HeaderText = "Area 1";
                        //dgv.Columns[4].HeaderText = "Area 2";
                        //dgv.Columns[5].HeaderText = "Area 3";
                        //dgv.Columns[6].HeaderText = "Area 4";
                        //dgv.Columns[7].HeaderText = "Area 5";

                        //dgv.Columns[1].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";

                        //---------------↓ OKNG 색칠 ↓---------------┐

                        //GridMaster.Color_Painting(dgv, 5);
                        //GridMaster.Color_Painting(dgv, 12);

                        //GridMaster.Color_Painting(dgv, 17);
                        //GridMaster.Color_Painting(dgv, 19);
                        //GridMaster.Color_Painting(dgv, 21);
                        //GridMaster.Color_Painting(dgv, 23);
                        //GridMaster.Color_Painting(dgv, 27);
                        //GridMaster.Color_Painting(dgv, 38);



                        //---------------↑ OKNG 색칠 ↑---------------┘



                        //---------------↑ 사용자 데이터 추가 부분 ↑---------------┘

                        //---------------↓ 정렬 ↓---------------┐
                        GridMaster.CenterAlign(dgv);
                        //GridMaster.LeftAlign( dgv );
                        //GridMaster.Align( dgv , 0 , DataGridViewContentAlignment.MiddleLeft );//단일 Column 정렬
                        //---------------↑ 정렬 ↑---------------┘

                        //---------------↓ 설정 ↓---------------┐
                        dgv.ReadOnly = true;//읽기전용
                        //GridMaster.DisableSortColumn( dgv );//오름차순 내림차순 정렬 막기
                        dgv.Columns[0].ReadOnly = true;//읽기전용
                        //dgv.AllowUserToResizeColumns = false;//컬럼폭 수정불가
                        //dgv.Columns[1].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";//표시형식
                        //dgv.ColumnHeadersVisible = false;//컬럼헤더 가리기                        
                        //dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;//스페이스 시 줄바꿈
                        //dgv.DefaultCellStyle.BackColor = Color.Black;//색반전
                        //dgv.DefaultCellStyle.ForeColor = Color.White;//색반전

                        //---------------↑ 설정 ↑---------------┘

                        //for ( int i = 0 ; i < dgvH0.RowCount ; i++ )
                        //{
                        //    for ( int j = 3 ; j < 8 ; j++ )
                        //    {
                        //        if ( dgvH0.Rows [ i ].Cells [ j ].Value.ToString( ) != "1" )
                        //        {
                        //            dgvH0.Rows [ i ].Cells [ j ].Style.BackColor = Color.Crimson;
                        //        }
                        //    }
                        //}
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("dgvH6");
                    }

                    break;

                case "dgvH7":

                    try
                    {
                        //---------------↓ 기본 ↓---------------┐
                        DataGridView dgv = (DataGridView)Reflection_K.Get(this, name);//이름가져옴
                        string DGV_name = dgv.Name;//적용
                        int height = int.Parse(DataRW.Load_Simple(DGV_name + "H", "30"));//데이터가져옴
                        int fontheader = int.Parse(DataRW.Load_Simple(DGV_name + "FH", "12"));//데이터가져옴
                        int fontcell = int.Parse(DataRW.Load_Simple(DGV_name + "FC", "12"));//데이터가져옴
                        GridMaster.FontSize2(dgv, fontheader, fontcell);//적용
                        //---------------↑ 기본 ↑---------------┘

                        //---------------↓ 생성 ↓---------------┐
                        string[] ColumnsName = new string[] {
                            //"A","A","A","A","A","A","A","A"
                            };
                        int rows = 0;//초기 생성 Row수

                        GridMaster.Init3(dgv, false, height, rows, ColumnsName);
                        //---------------↑ 생성 ↑---------------┘

                        //---------------↓ 사용자 데이터 추가 부분 ↓---------------┐
                        //GridMaster.LoadCSV_OnlyData( dgv , System.Windows.Forms.Application.StartupPath + "\\AAAA.csv" );//셀데이터로드
                        //dgv.Rows[ 0 ].Cells[ 0 ].Value = "CORE HEIGHT 1";

                        //dgv.Columns[0].HeaderText = "Model";
                        //dgv.Columns[1].HeaderText = "DateTime";
                        //dgv.Columns[2].HeaderText = "Result";
                        //dgv.Columns[3].HeaderText = "Area 1";
                        //dgv.Columns[4].HeaderText = "Area 2";
                        //dgv.Columns[5].HeaderText = "Area 3";
                        //dgv.Columns[6].HeaderText = "Area 4";
                        //dgv.Columns[7].HeaderText = "Area 5";

                        //dgv.Columns[1].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";

                        //---------------↓ OKNG 색칠 ↓---------------┐

                        //GridMaster.Color_Painting(dgv, 5);
                        //GridMaster.Color_Painting(dgv, 12);

                        //GridMaster.Color_Painting(dgv, 17);
                        //GridMaster.Color_Painting(dgv, 19);
                        //GridMaster.Color_Painting(dgv, 21);
                        //GridMaster.Color_Painting(dgv, 23);
                        //GridMaster.Color_Painting(dgv, 27);
                        //GridMaster.Color_Painting(dgv, 38);



                        //---------------↑ OKNG 색칠 ↑---------------┘



                        //---------------↑ 사용자 데이터 추가 부분 ↑---------------┘

                        //---------------↓ 정렬 ↓---------------┐
                        GridMaster.CenterAlign(dgv);
                        //GridMaster.LeftAlign( dgv );
                        //GridMaster.Align( dgv , 0 , DataGridViewContentAlignment.MiddleLeft );//단일 Column 정렬
                        //---------------↑ 정렬 ↑---------------┘

                        //---------------↓ 설정 ↓---------------┐
                        dgv.ReadOnly = true;//읽기전용
                        //GridMaster.DisableSortColumn( dgv );//오름차순 내림차순 정렬 막기
                        dgv.Columns[0].ReadOnly = true;//읽기전용
                        //dgv.AllowUserToResizeColumns = false;//컬럼폭 수정불가
                        //dgv.Columns[1].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";//표시형식
                        //dgv.ColumnHeadersVisible = false;//컬럼헤더 가리기                        
                        //dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;//스페이스 시 줄바꿈
                        //dgv.DefaultCellStyle.BackColor = Color.Black;//색반전
                        //dgv.DefaultCellStyle.ForeColor = Color.White;//색반전

                        //---------------↑ 설정 ↑---------------┘

                        //for ( int i = 0 ; i < dgvH0.RowCount ; i++ )
                        //{
                        //    for ( int j = 3 ; j < 8 ; j++ )
                        //    {
                        //        if ( dgvH0.Rows [ i ].Cells [ j ].Value.ToString( ) != "1" )
                        //        {
                        //            dgvH0.Rows [ i ].Cells [ j ].Style.BackColor = Color.Crimson;
                        //        }
                        //    }
                        //}
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("dgvH7");
                    }

                    break;


                case "dgvH8":

                    try
                    {
                        //---------------↓ 기본 ↓---------------┐
                        DataGridView dgv = (DataGridView)Reflection_K.Get(this, name);//이름가져옴
                        string DGV_name = dgv.Name;//적용
                        int height = int.Parse(DataRW.Load_Simple(DGV_name + "H", "30"));//데이터가져옴
                        int fontheader = int.Parse(DataRW.Load_Simple(DGV_name + "FH", "12"));//데이터가져옴
                        int fontcell = int.Parse(DataRW.Load_Simple(DGV_name + "FC", "12"));//데이터가져옴
                        GridMaster.FontSize2(dgv, fontheader, fontcell);//적용
                        //---------------↑ 기본 ↑---------------┘

                        //---------------↓ 생성 ↓---------------┐
                        string[] ColumnsName = new string[] {
                            //"A","A","A","A","A","A","A","A"
                            };
                        int rows = 0;//초기 생성 Row수

                        GridMaster.Init3(dgv, false, height, rows, ColumnsName);
                        //---------------↑ 생성 ↑---------------┘

                        //---------------↓ 사용자 데이터 추가 부분 ↓---------------┐
                        //GridMaster.LoadCSV_OnlyData( dgv , System.Windows.Forms.Application.StartupPath + "\\AAAA.csv" );//셀데이터로드
                        //dgv.Rows[ 0 ].Cells[ 0 ].Value = "CORE HEIGHT 1";

                        //dgv.Columns[0].HeaderText = "Model";
                        //dgv.Columns[1].HeaderText = "DateTime";
                        //dgv.Columns[2].HeaderText = "Result";
                        //dgv.Columns[3].HeaderText = "Area 1";
                        //dgv.Columns[4].HeaderText = "Area 2";
                        //dgv.Columns[5].HeaderText = "Area 3";
                        //dgv.Columns[6].HeaderText = "Area 4";
                        //dgv.Columns[7].HeaderText = "Area 5";

                        //dgv.Columns[1].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";

                        //---------------↓ OKNG 색칠 ↓---------------┐

                        //GridMaster.Color_Painting(dgv, 5);
                        //GridMaster.Color_Painting(dgv, 12);

                        //GridMaster.Color_Painting(dgv, 17);
                        //GridMaster.Color_Painting(dgv, 19);
                        //GridMaster.Color_Painting(dgv, 21);
                        //GridMaster.Color_Painting(dgv, 23);
                        //GridMaster.Color_Painting(dgv, 27);
                        //GridMaster.Color_Painting(dgv, 38);



                        //---------------↑ OKNG 색칠 ↑---------------┘



                        //---------------↑ 사용자 데이터 추가 부분 ↑---------------┘

                        //---------------↓ 정렬 ↓---------------┐
                        GridMaster.CenterAlign(dgv);
                        //GridMaster.LeftAlign( dgv );
                        //GridMaster.Align( dgv , 0 , DataGridViewContentAlignment.MiddleLeft );//단일 Column 정렬
                        //---------------↑ 정렬 ↑---------------┘

                        //---------------↓ 설정 ↓---------------┐
                        dgv.ReadOnly = true;//읽기전용
                        //GridMaster.DisableSortColumn( dgv );//오름차순 내림차순 정렬 막기
                        dgv.Columns[0].ReadOnly = true;//읽기전용
                        //dgv.AllowUserToResizeColumns = false;//컬럼폭 수정불가
                        //dgv.Columns[1].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";//표시형식
                        //dgv.ColumnHeadersVisible = false;//컬럼헤더 가리기                        
                        //dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;//스페이스 시 줄바꿈
                        //dgv.DefaultCellStyle.BackColor = Color.Black;//색반전
                        //dgv.DefaultCellStyle.ForeColor = Color.White;//색반전

                        //---------------↑ 설정 ↑---------------┘

                        //for ( int i = 0 ; i < dgvH0.RowCount ; i++ )
                        //{
                        //    for ( int j = 3 ; j < 8 ; j++ )
                        //    {
                        //        if ( dgvH0.Rows [ i ].Cells [ j ].Value.ToString( ) != "1" )
                        //        {
                        //            dgvH0.Rows [ i ].Cells [ j ].Style.BackColor = Color.Crimson;
                        //        }
                        //    }
                        //}
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("dgvH8");
                    }

                    break;
            }
        }

        void OnInit(string name, object data)
        {
            this.Invoke(new dele(() =>
            {
                dgvInit(name);
            }));
        }

        #endregion
        
        #region DGV 자동맞춤
        private void dgvD1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button.ToString().Equals("Middle"))
            {
                DataGridView thisdgv = (DataGridView)sender;
                dgvmanager = new Ken2.UIControl.dgvManager(thisdgv);
                dgvmanager.Init += OnInit;
                dgvmanager.Show();
            }
        }

        private void dgvStatus1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button.ToString().Equals("Middle"))
            {
                DataGridView thisdgv = (DataGridView)sender;
                dgvmanager = new Ken2.UIControl.dgvManager(thisdgv);
                dgvmanager.Init += OnInit;
                dgvmanager.Show();
            }
        }

        private void dgvM1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button.ToString().Equals("Middle"))
            {
                DataGridView thisdgv = (DataGridView)sender;
                dgvmanager = new Ken2.UIControl.dgvManager(thisdgv);
                dgvmanager.Init += OnInit;
                dgvmanager.Show();
            }
        }

        private void dgvCam1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button.ToString().Equals("Middle"))
            {
                DataGridView thisdgv = (DataGridView)sender;
                dgvmanager = new Ken2.UIControl.dgvManager(thisdgv);
                dgvmanager.Init += OnInit;
                dgvmanager.Show();
            }
        }

        private void dgvS1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button.ToString().Equals("Middle"))
            {
                DataGridView thisdgv = (DataGridView)sender;
                dgvmanager = new Ken2.UIControl.dgvManager(thisdgv);
                dgvmanager.Init += OnInit;
                dgvmanager.Show();
            }
        }

        private void dgvC1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button.ToString().Equals("Middle"))
            {
                DataGridView thisdgv = (DataGridView)sender;
                dgvmanager = new Ken2.UIControl.dgvManager(thisdgv);
                dgvmanager.Init += OnInit;
                dgvmanager.Show();
            }
        }

        private void dgvH0_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button.ToString().Equals("Middle"))
            {
                DataGridView thisdgv = (DataGridView)sender;
                dgvmanager = new Ken2.UIControl.dgvManager(thisdgv);
                dgvmanager.Init += OnInit;
                dgvmanager.Show();
            }
        }

        private void dgvD2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button.ToString().Equals("Middle"))
            {
                DataGridView thisdgv = (DataGridView)sender;
                dgvmanager = new Ken2.UIControl.dgvManager(thisdgv);
                dgvmanager.Init += OnInit;
                dgvmanager.Show();
            }
        }

        private void dgvStatus2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button.ToString().Equals("Middle"))
            {
                DataGridView thisdgv = (DataGridView)sender;
                dgvmanager = new Ken2.UIControl.dgvManager(thisdgv);
                dgvmanager.Init += OnInit;
                dgvmanager.Show();
            }
        }

        private void dgvM2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button.ToString().Equals("Middle"))
            {
                DataGridView thisdgv = (DataGridView)sender;
                dgvmanager = new Ken2.UIControl.dgvManager(thisdgv);
                dgvmanager.Init += OnInit;
                dgvmanager.Show();
            }
        }

        private void dgvCam2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button.ToString().Equals("Middle"))
            {
                DataGridView thisdgv = (DataGridView)sender;
                dgvmanager = new Ken2.UIControl.dgvManager(thisdgv);
                dgvmanager.Init += OnInit;
                dgvmanager.Show();
            }
        }

        private void dgvS2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button.ToString().Equals("Middle"))
            {
                DataGridView thisdgv = (DataGridView)sender;
                dgvmanager = new Ken2.UIControl.dgvManager(thisdgv);
                dgvmanager.Init += OnInit;
                dgvmanager.Show();
            }
        }

        private void dgvH1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button.ToString().Equals("Middle"))
            {
                DataGridView thisdgv = (DataGridView)sender;
                dgvmanager = new Ken2.UIControl.dgvManager(thisdgv);
                dgvmanager.Init += OnInit;
                dgvmanager.Show();
            }
        }

        private void dgvH2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button.ToString().Equals("Middle"))
            {
                DataGridView thisdgv = (DataGridView)sender;
                dgvmanager = new Ken2.UIControl.dgvManager(thisdgv);
                dgvmanager.Init += OnInit;
                dgvmanager.Show();
            }
        }

        private void dgvH3_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button.ToString().Equals("Middle"))
            {
                DataGridView thisdgv = (DataGridView)sender;
                dgvmanager = new Ken2.UIControl.dgvManager(thisdgv);
                dgvmanager.Init += OnInit;
                dgvmanager.Show();
            }
        }

        private void dgvH4_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button.ToString().Equals("Middle"))
            {
                DataGridView thisdgv = (DataGridView)sender;
                dgvmanager = new Ken2.UIControl.dgvManager(thisdgv);
                dgvmanager.Init += OnInit;
                dgvmanager.Show();
            }
        }

        private void dgvH5_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button.ToString().Equals("Middle"))
            {
                DataGridView thisdgv = (DataGridView)sender;
                dgvmanager = new Ken2.UIControl.dgvManager(thisdgv);
                dgvmanager.Init += OnInit;
                dgvmanager.Show();
            }
        }

        private void dgvH6_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button.ToString().Equals("Middle"))
            {
                DataGridView thisdgv = (DataGridView)sender;
                dgvmanager = new Ken2.UIControl.dgvManager(thisdgv);
                dgvmanager.Init += OnInit;
                dgvmanager.Show();
            }
        }

        private void dgvH7_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button.ToString().Equals("Middle"))
            {
                DataGridView thisdgv = (DataGridView)sender;
                dgvmanager = new Ken2.UIControl.dgvManager(thisdgv);
                dgvmanager.Init += OnInit;
                dgvmanager.Show();
            }
        }


        private void dgvH8_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button.ToString().Equals("Middle"))
            {
                DataGridView thisdgv = (DataGridView)sender;
                dgvmanager = new Ken2.UIControl.dgvManager(thisdgv);
                dgvmanager.Init += OnInit;
                dgvmanager.Show();
            }
        }

        #endregion

        #region 상단 버튼
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        private void title_kenlb_MouseDown(object sender, MouseEventArgs e) //  드래그
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void mini_kenb_Click(object sender, EventArgs e)    //  최소화
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void exit_kenb_Click(object sender, EventArgs e)    //  닫기
        {
            Application.Exit();
        }

        #endregion

        #region 버튼
        private void Btn_Main_Click(object sender, EventArgs e)     //  메인화면
        {
            xtraTabControl_Model.SelectedTabPage = Tab_Main;
        }

        private void Btn_AutoStart_Click(object sender, EventArgs e)    //  자동 시작
        {
            autoRun();
        }

        private void Btn_AutoStop_Click(object sender, EventArgs e)     //  자동 정지
        {
            autoStop();
        }

        private void Btn_Manual_Click(object sender, EventArgs e)   //  수동 검사1
        {
            this.Invoke(new dele(() =>
            {
                Log_K.WriteLog(log_lst, Mainpath, "수동 검사 -> " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff"));
            }));
            try
            {
                cam1.OneShot( );
            }
            catch ( Exception )
            {
                Log_K.WriteLog( log_lst, Mainpath, "카메라1 트리거 에러" );
            }
        }

        private void Btn_Manual2_Click(object sender, EventArgs e)  //  수동 검사2
        {
            this.Invoke(new dele(() =>
            {
                Log_K.WriteLog(log_lst, Mainpath, "수동 검사2 -> " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff"));
            }));
            try
            {
                cam2.OneShot();
            }
            catch (Exception)
            {
                Log_K.WriteLog(log_lst, Mainpath, "카메라2 트리거 에러");
            }
        }

        private void Btn_Model_Click(object sender, EventArgs e)    //  모델
        {
            xtraTabControl_Model.SelectedTabPage = Tab_Model;
        }

        private void Btn_Cam_Click(object sender, EventArgs e)      //  카메라
        {
            xtraTabControl_Model.SelectedTabPage = Tab_Cam;
        }

        private void Btn_Vision_Click(object sender, EventArgs e)   //  비전
        {
            //xtraTabControl1.SelectedTabPage = xtraTabPage6;
            xtraTabControl_Model.SelectedTabPage = Tab_Vision;
        }

        private void Btn_Setting_Click(object sender, EventArgs e)  //  설정
        {
            xtraTabControl_Model.SelectedTabPage = Tab_Setting;
        }

        private void Btn_Image_Click(object sender, EventArgs e)    //  이미지 폴더
        {
            //Process.Start("explorer.exe", "D:\\" + "\\Vision" + "\\Image");
            //Directory.CreateDirectory( @"D:\" + Mainpath + @"\Image\" );
            System.Diagnostics.Process.Start( "explorer.exe", @"D:\" + Mainpath + @"\Image\" );
        }

        private void Btn_History_Click(object sender, EventArgs e)  //  DB 이력
        {
            xtraTabControl_Model.SelectedTabPage = Tab_Db;
        }


        private void Btn_Main_MouseMove(object sender, MouseEventArgs e)
        {
            Btn_Main.Appearance.BackColor = Color.LightSkyBlue;
        }

        private void Btn_Main_MouseLeave(object sender, EventArgs e)
        {
            Btn_Main.Appearance.BackColor = Color.LightSlateGray;
        }

        private void Btn_AutoStart_MouseMove( object sender, MouseEventArgs e )
        {
            Btn_AutoStart.Appearance.BackColor = Color.LightSkyBlue;
        }

        private void Btn_AutoStart_MouseLeave( object sender, EventArgs e )
        {
            Btn_AutoStart.Appearance.BackColor = Color.LightSlateGray;
        }

        private void Btn_AutoStop_MouseMove( object sender, MouseEventArgs e )
        {
            Btn_AutoStop.Appearance.BackColor = Color.LightSkyBlue;
        }

        private void Btn_AutoStop_MouseLeave( object sender, EventArgs e )
        {
            Btn_AutoStop.Appearance.BackColor = Color.LightSlateGray;
        }
        private void Btn_Manual_MouseMove(object sender, MouseEventArgs e)
        {
            Btn_Manual.Appearance.BackColor = Color.LightSkyBlue;
        }

        private void Btn_Manual_MouseLeave(object sender, EventArgs e)
        {
            Btn_Manual.Appearance.BackColor = Color.LightSlateGray;
        }

        private void Btn_Manual2_MouseMove(object sender, MouseEventArgs e)
        {
            Btn_Manual2.Appearance.BackColor = Color.LightSkyBlue;
        }

        private void Btn_Manual2_MouseLeave(object sender, EventArgs e)
        {
            Btn_Manual2.Appearance.BackColor = Color.LightSlateGray;
        }

        private void Btn_Model_MouseMove(object sender, MouseEventArgs e)
        {
            Btn_Model.Appearance.BackColor = Color.LightSkyBlue;
        }

        private void Btn_Model_MouseLeave(object sender, EventArgs e)
        {
            Btn_Model.Appearance.BackColor = Color.LightSlateGray;
        }

        private void Btn_Cam_MouseMove(object sender, MouseEventArgs e)
        {
            Btn_Cam.Appearance.BackColor = Color.LightSkyBlue;
        }

        private void Btn_Cam_MouseLeave(object sender, EventArgs e)
        {
            Btn_Cam.Appearance.BackColor = Color.LightSlateGray;
        }

        private void Btn_Vision_MouseMove(object sender, MouseEventArgs e)
        {
            Btn_Vision.Appearance.BackColor = Color.LightSkyBlue;
        }

        private void Btn_Vision_MouseLeave(object sender, EventArgs e)
        {
            Btn_Vision.Appearance.BackColor = Color.LightSlateGray;
        }

        private void Btn_Setting_MouseMove(object sender, MouseEventArgs e)
        {
            Btn_Setting.Appearance.BackColor = Color.LightSkyBlue;
        }

        private void Btn_Setting_MouseLeave(object sender, EventArgs e)
        {
            Btn_Setting.Appearance.BackColor = Color.LightSlateGray;
        }

        private void Btn_Image_MouseMove(object sender, MouseEventArgs e)
        {
            Btn_Image.Appearance.BackColor = Color.LightSkyBlue;
        }

        private void Btn_Image_MouseLeave(object sender, EventArgs e)
        {
            Btn_Image.Appearance.BackColor = Color.LightSlateGray;
        }

        private void Btn_History_MouseMove(object sender, MouseEventArgs e)
        {
            Btn_History.Appearance.BackColor = Color.LightSkyBlue;
        }

        private void Btn_History_MouseLeave(object sender, EventArgs e)
        {
            Btn_History.Appearance.BackColor = Color.LightSlateGray;
        }

        private void Btn_Reset1_MouseMove(object sender, MouseEventArgs e)
        {
            Btn_Reset1.Appearance.BackColor = Color.LightSkyBlue;
        }

        private void Btn_Reset1_MouseLeave(object sender, EventArgs e)
        {
            Btn_Reset1.Appearance.BackColor = Color.LightSlateGray;
        }

        #endregion

        #region 모델 화면 버튼

        //모델 관리

        private void modelOpen1(int modelnum)   //  모델 열기 함수1
        {
            if (CurrentModelNum1 == 0)
                CurrentModelNum1 = 1;
            else
                CurrentModelNum1 = modelnum;

            try
            {
                ModelNamelbl1.Text = dgvM1.Rows[modelnum].Cells[1].Value.ToString();

                Cogtg = (CogToolGroup)CogSerializer.LoadObjectFromFile(System.Windows.Forms.Application.StartupPath + "\\" + CurrentModelNum1 + "_1.vpp");
                CogToolBlock result = (CogToolBlock)Cogtg.Tools["result"];
                for (int i = 0; i < result.Inputs.Count; i++)
                {
                    result.Inputs[i].Value = 0;
                }

                GridMaster.LoadCSV_OnlyData(dgvS1, System.Windows.Forms.Application.StartupPath + "\\" + CurrentModelNum1 + "_S1.csv");//셀데이터로드
                GridMaster.LoadCSV_OnlyData(dgvCam1, System.Windows.Forms.Application.StartupPath + "\\" + CurrentModelNum1 + "_C1.csv");//셀데이터로드

                Cogtg2 = (CogToolGroup)CogSerializer.LoadObjectFromFile(System.Windows.Forms.Application.StartupPath + "\\" + CurrentModelNum1 + "_2.vpp");
                CogToolBlock result2 = (CogToolBlock)Cogtg2.Tools["result"];
                for (int i = 0; i < result2.Inputs.Count; i++)
                {
                    result2.Inputs[i].Value = 0;
                }

                GridMaster.LoadCSV_OnlyData(dgvS2, System.Windows.Forms.Application.StartupPath + "\\" + CurrentModelNum1 + "_S2.csv");//셀데이터로드
                GridMaster.LoadCSV_OnlyData(dgvCam2, System.Windows.Forms.Application.StartupPath + "\\" + CurrentModelNum1 + "_C2.csv");//셀데이터로드

            }
            catch (Exception)
            {
                Log_K.WriteLog(log_lst, Mainpath, "ModelOpen1 에러");
            }

            try
            {
                if (cam1 != null)
                {
                    cam1.SetExp(Convert.ToInt32(dgvCam1.Rows[0].Cells[0].Value));
                    cam2.SetExp(Convert.ToInt32(dgvCam2.Rows[0].Cells[0].Value));
                }

            }
            catch (Exception)
            {
                Console.WriteLine("camexposure 에러");
            }

            cogToolGroupEditV21.Subject = null;
            cogToolGroupEditV22.Subject = null;

            Txt_LastModel1.Text = modelnum.ToString();
            SettingMinMax();
            SettingMinMax2();
        }

        private void Btn_M1save_Click(object sender, EventArgs e)   //  모델 저장1
        {
            if (MessageBox.Show("저장 하시겠습니까?", "안내", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {

                if (cogToolGroupEditV21.Subject != null)
                    Cogtg = cogToolGroupEditV21.Subject;

                    try
                    {
                        if (dgvM1.Rows[dgvM1.CurrentCell.RowIndex].Cells[1].Value.ToString() != "")
                        {

                            CogSerializer.SaveObjectToFile(Cogtg, System.Windows.Forms.Application.StartupPath + "\\" + dgvM1.CurrentCell.RowIndex + "_1.vpp", typeof(System.Runtime.Serialization.Formatters.Binary.BinaryFormatter), CogSerializationOptionsConstants.Minimum);

                            GridMaster.SaveCSV_OnlyData(dgvS1, System.Windows.Forms.Application.StartupPath + "\\" + dgvM1.CurrentCell.RowIndex + "_S1.csv");//셀데이터 세이브
                            GridMaster.SaveCSV_OnlyData(dgvCam1, System.Windows.Forms.Application.StartupPath + "\\" + dgvM1.CurrentCell.RowIndex + "_C1.csv");//셀데이터 세이브

                            SettingMinMax();

                            CogSerializer.SaveObjectToFile(Cogtg2, System.Windows.Forms.Application.StartupPath + "\\" + dgvM1.CurrentCell.RowIndex + "_2.vpp", typeof(System.Runtime.Serialization.Formatters.Binary.BinaryFormatter), CogSerializationOptionsConstants.Minimum);

                            GridMaster.SaveCSV_OnlyData(dgvS2, System.Windows.Forms.Application.StartupPath + "\\" + dgvM1.CurrentCell.RowIndex + "_S2.csv");//셀데이터 세이브
                            GridMaster.SaveCSV_OnlyData(dgvCam2, System.Windows.Forms.Application.StartupPath + "\\" + dgvM1.CurrentCell.RowIndex + "_C2.csv");//셀데이터 세이브

                            SettingMinMax2();

                            MessageBox.Show("저장이 완료되었습니다.");
                        }
                        else if (dgvM1.Rows[dgvM1.CurrentCell.RowIndex].Cells[1].Value.ToString() == "")
                        {
                            dgvM1.Rows[dgvM1.CurrentCell.RowIndex].Cells[1].Value = ModelNamelbl1.Text;
                            CogSerializer.SaveObjectToFile(Cogtg, System.Windows.Forms.Application.StartupPath + "\\" + dgvM1.CurrentCell.RowIndex + "_1.vpp", typeof(System.Runtime.Serialization.Formatters.Binary.BinaryFormatter), CogSerializationOptionsConstants.Minimum);

                            GridMaster.SaveCSV_OnlyData(dgvS1, System.Windows.Forms.Application.StartupPath + "\\" + dgvM1.CurrentCell.RowIndex + "_S1.csv");//셀데이터 세이브
                            GridMaster.SaveCSV_OnlyData(dgvCam1, System.Windows.Forms.Application.StartupPath + "\\" + dgvM1.CurrentCell.RowIndex + "_C1.csv");//셀데이터 세이브

                            SettingMinMax();

                            CogSerializer.SaveObjectToFile(Cogtg2, System.Windows.Forms.Application.StartupPath + "\\" + dgvM1.CurrentCell.RowIndex + "_2.vpp", typeof(System.Runtime.Serialization.Formatters.Binary.BinaryFormatter), CogSerializationOptionsConstants.Minimum);

                            GridMaster.SaveCSV_OnlyData(dgvS2, System.Windows.Forms.Application.StartupPath + "\\" + dgvM1.CurrentCell.RowIndex + "_S2.csv");//셀데이터 세이브
                            GridMaster.SaveCSV_OnlyData(dgvCam2, System.Windows.Forms.Application.StartupPath + "\\" + dgvM1.CurrentCell.RowIndex + "_C2.csv");//셀데이터 세이브

                            SettingMinMax2();

                            MessageBox.Show("저장이 완료되었습니다.");
                        }

                        GridMaster.SaveCSV_OnlyData(dgvC1, System.Windows.Forms.Application.StartupPath + "\\P1.csv");//셀데이터 세이브
                    }

                    catch (Exception)
                    {
                        //Log_K.WriteLog(log_err, Mainpath, "모델 저장버튼 에러");
                        //Console.WriteLine("모델 저장버튼");
                    }
            }
            else
            {
                try
                {
                    MessageBox.Show("취소되었습니다.");
                }
                catch (Exception)
                {
                    //Log_K.WriteLog(log_err, Mainpath, "모델 저장버튼 에러2");
                    Console.WriteLine("모델 저장버튼");
                }
            }
        }


        private void Btn_M1open_Click(object sender, EventArgs e)   //  모델 열기1
        {
            if (dgvM1.CurrentCell == null)
            {
                return;
            }
            if (POPUP.YesOrNo("안내", "모델을 불러오시겠습니까?"))
            {
                int modelnum = dgvM1.CurrentCell.RowIndex;

                modelOpen1(modelnum);
                MessageBox.Show("모델을 성공적으로 불러왔습니다.", "Message");

            }
        }
        private void Btn_M1change_Click(object sender, EventArgs e) //  모델명 변경1
        {
            try
            {
                if (dgvM1.CurrentCell == null)
                {
                    return;
                }
                if (txt_Modelname1.Text.Equals(""))
                {
                    MessageBox.Show("모델명을 기입해주세요.", "Error");
                    return;
                }
                if (POPUP.YesOrNo("INFO", "모델 이름을 변경하시겠습니까?"))
                {
                    dgvM1.Rows[dgvM1.CurrentCell.RowIndex].Cells[1].Value = txt_Modelname1.Text;
                    GridMaster.SaveCSV_OnlyData(dgvM1, System.Windows.Forms.Application.StartupPath + "\\Model1.csv");//셀데이터 세이브
                    MessageBox.Show("성공적으로 변경되었습니다.", "Messagebox");
                }
            }
            catch (Exception)
            {

            }
        }


        private void simpleButton6_Click(object sender, EventArgs e)  //모델 삭제1
        {
            try
            {
                if (POPUP.YesOrNo("WARNING", "모델을 삭제하시겠습니까?"))
                {
                    int modelnum = dgvM1.CurrentCell.RowIndex;

                    ModelNamelbl1.Text = "";
                    dgvM1.Rows[dgvM1.CurrentCell.RowIndex].Cells[1].Value = null;

                    File.Delete(System.Windows.Forms.Application.StartupPath + "\\" + modelnum + "_1.vpp");   //파일 삭제
                    File.Delete(System.Windows.Forms.Application.StartupPath + "\\" + modelnum + "_S1.csv");
                    File.Delete(System.Windows.Forms.Application.StartupPath + "\\" + modelnum + "_C1.csv");
                    File.Delete(System.Windows.Forms.Application.StartupPath + "\\" + modelnum + "_2.vpp");
                    File.Delete(System.Windows.Forms.Application.StartupPath + "\\" + modelnum + "_S2.csv");
                    File.Delete(System.Windows.Forms.Application.StartupPath + "\\" + modelnum + "_C2.csv");
                    GridMaster.SaveCSV_OnlyData(dgvM1, System.Windows.Forms.Application.StartupPath + "\\Model1.csv");//셀데이터 세이브

                    MessageBox.Show("모델을 성공적으로 삭제하였습니다.", "Message");

                }
            }catch (Exception)
            {
                //Log_K.WriteLog(log_err, Mainpath, "모델 삭제버튼 에러1");
            }

        }

        private void modelOpen2(int modelnum)   //  모델 열기 함수2
        {
            if (CurrentModelNum1 == 0)
                CurrentModelNum1 = 1;
            else
                CurrentModelNum1 = modelnum;

            try
            {
                ModelNamelbl1.Text = dgvM2.Rows[modelnum].Cells[1].Value.ToString();

                Cogtg = (CogToolGroup)CogSerializer.LoadObjectFromFile(System.Windows.Forms.Application.StartupPath + "\\" + CurrentModelNum1 + "_1.vpp");
                CogToolBlock result = (CogToolBlock)Cogtg.Tools["result"];
                for (int i = 0; i < result.Inputs.Count; i++)
                {
                    result.Inputs[i].Value = 0;
                }

                GridMaster.LoadCSV_OnlyData(dgvS1, System.Windows.Forms.Application.StartupPath + "\\" + CurrentModelNum1 + "_S1.csv");//셀데이터로드
                GridMaster.LoadCSV_OnlyData(dgvCam1, System.Windows.Forms.Application.StartupPath + "\\" + CurrentModelNum1 + "_C1.csv");//셀데이터로드


                Cogtg2 = (CogToolGroup)CogSerializer.LoadObjectFromFile(System.Windows.Forms.Application.StartupPath + "\\" + CurrentModelNum1 + "_2.vpp");
                CogToolBlock result2 = (CogToolBlock)Cogtg2.Tools["result"];
                for (int i = 0; i < result2.Inputs.Count; i++)
                {
                    result2.Inputs[i].Value = 0;
                }

                GridMaster.LoadCSV_OnlyData(dgvS2, System.Windows.Forms.Application.StartupPath + "\\" + CurrentModelNum1 + "_S2.csv");//셀데이터로드
                GridMaster.LoadCSV_OnlyData(dgvCam2, System.Windows.Forms.Application.StartupPath + "\\" + CurrentModelNum1 + "_C2.csv");//셀데이터로드

            }
            catch (Exception)
            {
                Log_K.WriteLog(log_lst, Mainpath, "ModelOpen2 에러");
            }

            try
            {
                if (cam1 != null)
                {
                    cam1.SetExp(Convert.ToInt32(dgvCam1.Rows[0].Cells[0].Value));
                    cam2.SetExp(Convert.ToInt32(dgvCam2.Rows[0].Cells[0].Value));
                }

            }
            catch (Exception)
            {
                Console.WriteLine("camexposure 에러");
            }

            cogToolGroupEditV21.Subject = null;
            cogToolGroupEditV22.Subject = null;

            Txt_LastModel2.Text = modelnum.ToString();
            SettingMinMax();
            SettingMinMax2();
        }


        private void Btn_M2save_Click(object sender, EventArgs e)   //  모델저장 2
        {
            if (MessageBox.Show("모델을 저장하시겠습니까?", "안내", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {

                if (cogToolGroupEditV22.Subject != null)
                    Cogtg2 = cogToolGroupEditV22.Subject;

                try
                {
                    if (dgvM2.Rows[dgvM2.CurrentCell.RowIndex].Cells[1].Value.ToString() != "")
                    {
                        CogSerializer.SaveObjectToFile(Cogtg2, System.Windows.Forms.Application.StartupPath + "\\" + dgvM2.CurrentCell.RowIndex + "_2.vpp", typeof(System.Runtime.Serialization.Formatters.Binary.BinaryFormatter), CogSerializationOptionsConstants.Minimum);
                        
                        GridMaster.SaveCSV_OnlyData(dgvS2, System.Windows.Forms.Application.StartupPath + "\\" + dgvM2.CurrentCell.RowIndex + "_S2.csv");//셀데이터 세이브
                        GridMaster.SaveCSV_OnlyData(dgvCam2, System.Windows.Forms.Application.StartupPath + "\\" + dgvM2.CurrentCell.RowIndex + "_C2.csv");//셀데이터 세이브

                        SettingMinMax();

                        MessageBox.Show("저장이 완료되었습니다.");
                    }
                    else if (dgvM2.Rows[dgvM2.CurrentCell.RowIndex].Cells[1].Value.ToString() == "")
                    {
                        dgvM2.Rows[dgvM2.CurrentCell.RowIndex].Cells[1].Value = ModelNamelbl1.Text;

                        CogSerializer.SaveObjectToFile(Cogtg2, System.Windows.Forms.Application.StartupPath + "\\" + dgvM2.CurrentCell.RowIndex + "_2.vpp", typeof(System.Runtime.Serialization.Formatters.Binary.BinaryFormatter), CogSerializationOptionsConstants.Minimum);

                        GridMaster.SaveCSV_OnlyData(dgvS2, System.Windows.Forms.Application.StartupPath + "\\" + dgvM2.CurrentCell.RowIndex + "_S2.csv");//셀데이터 세이브
                        GridMaster.SaveCSV_OnlyData(dgvCam2, System.Windows.Forms.Application.StartupPath + "\\" + dgvM2.CurrentCell.RowIndex + "_C2.csv");//셀데이터 세이브

                        SettingMinMax();

                        MessageBox.Show("저장이 완료되었습니다.");
                    }

                    GridMaster.SaveCSV_OnlyData(dgvC1, System.Windows.Forms.Application.StartupPath + "\\P1.csv");//셀데이터 세이브
                }

                catch (Exception)
                {
                    //Log_K.WriteLog(log_err, Mainpath, "모델 저장버튼 에러");
                    Console.WriteLine("모델 저장버튼");
                }
            }
            else
            {
                try
                {
                    MessageBox.Show("취소.");
                }
                catch (Exception)
                {
                    //Log_K.WriteLog(log_err, Mainpath, "모델 저장버튼 에러2");
                    Console.WriteLine("모델 저장버튼");
                }
            }
        }
        private void Btn_M2change_Click(object sender, EventArgs e) //  모델명 변경2
        {
            try
            {
                if (dgvM2.CurrentCell == null)
                {
                    return;
                }
                if (txt_Modelname2.Text.Equals(""))
                {
                    MessageBox.Show("모델명을 기입해주세요", "Error");
                    return;
                }
                if (POPUP.YesOrNo("INFO", "모델 이름을 바꾸시겠습니까?"))
                {
                    dgvM2.Rows[dgvM2.CurrentCell.RowIndex].Cells[1].Value = txt_Modelname2.Text;
                    GridMaster.SaveCSV_OnlyData(dgvM2, System.Windows.Forms.Application.StartupPath + "\\Model2.csv");//셀데이터 세이브
                    MessageBox.Show("성공적으로 변경되었습니다.", "Messagebox");
                }
            }
            catch (Exception)
            {
                //Log_K.WriteLog(log_err, Mainpath, "Model1 이름바꾸기 에러");
                //Console.WriteLine("모델1 이름바꾸기");
            }
        }

        private void Btn_M2open_Click(object sender, EventArgs e)   //  모델열기 2
        {
            if (dgvM2.CurrentCell == null)
            {
                return;
            }
            if (POPUP.YesOrNo("안내", "모델을 불러오시겠습니까 ?"))
            {
                int modelnum = dgvM2.CurrentCell.RowIndex;

                modelOpen2(modelnum);
                MessageBox.Show("모델을 성공적으로 불러왔습니다.", "Message");
            }
        }

        private void Btn_M2Delete_Click(object sender, EventArgs e) //모델2 삭제
        {
            try
            {
                if (POPUP.YesOrNo("WARNING", "모델을 삭제하시겠습니까?"))
                {
                    int modelnum = dgvM2.CurrentCell.RowIndex;

                    ModelNamelbl1.Text = "";
                    dgvM1.Rows[dgvM1.CurrentCell.RowIndex].Cells[1].Value = null;

                    File.Delete(System.Windows.Forms.Application.StartupPath + "\\" + modelnum + "_1.vpp");
                    File.Delete(System.Windows.Forms.Application.StartupPath + "\\" + modelnum + "_S1.csv");
                    File.Delete(System.Windows.Forms.Application.StartupPath + "\\" + modelnum + "_C1.csv");

                    File.Delete(System.Windows.Forms.Application.StartupPath + "\\" + modelnum + "_2.vpp");
                    File.Delete(System.Windows.Forms.Application.StartupPath + "\\" + modelnum + "_S2.csv");
                    File.Delete(System.Windows.Forms.Application.StartupPath + "\\" + modelnum + "_C2.csv");

                    MessageBox.Show("모델을 성공적으로 삭제하였습니다.", "Message");

                }
            }
            catch (Exception)
            {
                //Log_K.WriteLog(log_err, Mainpath, "모델 삭제버튼 에러1");
            }
        }


        #endregion

        #region PLC 통신 버튼
        private void button9_Click(object sender, EventArgs e)  //  PLC 데이터 쓰기
        {
            //plc1.MCWrite(int.Parse(Txt_Address.Text), int.Parse(Txt_Data.Text));
            string tstr = Txt_Address.Text;

            //plc1.MasterK_Write_W("3" + tstr.Substring(0, 1) + "3" + tstr.Substring(1, 1) + "3" + tstr.Substring(2, 1) + "3" + tstr.Substring(3, 1) , Txt_Data.Text);
            plc1.MasterK_Write_W("3" + tstr.Substring(0, 1) + "3" + tstr.Substring(1, 1) + "3" + tstr.Substring(2, 1) + "3" + tstr.Substring(3, 1) + "3" + tstr.Substring(4, 1), Txt_Data.Text);
        }

        private void button7_Click(object sender, EventArgs e)  //  PLC 데이터 리셋
        {
            //plc1.MCWrite(int.Parse(Txt_Address.Text), 0);
            string tstr = Txt_Address.Text;
            
            //plc1.MasterK_Write_W("3" + tstr.Substring(0, 1) + "3" + tstr.Substring(1, 1) + "3" + tstr.Substring(2, 1) + "3" + tstr.Substring(3, 1), "0000");
            plc1.MasterK_Write_W("3" + tstr.Substring(0, 1) + "3" + tstr.Substring(1, 1) + "3" + tstr.Substring(2, 1) + "3" + tstr.Substring(3, 1) + "3" + tstr.Substring(4, 1), "0000");

        }

        #endregion

        #region 메인화면 버튼
        private void Btn_Reset1_Click(object sender, EventArgs e)
        {
            //this.Invoke(new dele(() =>
            //{
            //    okcnt = 0;
            //    ngcnt = 0;
            //    totalcnt = 0;
            //    textBox_ok.Text = "0";
            //    textBox_ng.Text = "0";
            //    textBox_total.Text = "0";
            //    label_ok1.Text = "0";
            //    label_ng1.Text = "0";
            //    label_total1.Text = "0";
            //}));
        }

        private void Btn_Reset2_Click(object sender, EventArgs e)
        {
            //this.Invoke(new dele(() =>
            //{
            //    okcnt2 = 0;
            //    ngcnt2 = 0;
            //    totalcnt2 = 0;
            //    textBox_ok2.Text = "0";
            //    textBox_ng2.Text = "0";
            //    textBox_total2.Text = "0";
            //    label_ok2.Text = "0";
            //    label_ng2.Text = "0";
            //    label_total2.Text = "0";
            //}));
        }
        #endregion

        #region 카메라 버튼
        private void Btn_CamSave_Click(object sender, EventArgs e)  //  카메라 세이브
        {
            GridMaster.SaveCSV_OnlyData(dgvCam1, System.Windows.Forms.Application.StartupPath + "\\" + CurrentModelNum1 + "_C1.csv");//셀데이터 세이브
            GridMaster.SaveCSV_OnlyData(dgvCam2, System.Windows.Forms.Application.StartupPath + "\\" + CurrentModelNum2 + "_C2.csv");//셀데이터 세이브

            try
            {
                cam1.SetExp(Convert.ToInt32(dgvCam1.Rows[0].Cells[0].Value));
                cam1.TestShot(1);

            }
            catch (Exception)
            {
                Log_K.WriteLog(log_lst, Mainpath, "카메라밝기 저장 에러");
                Console.WriteLine("카메라밝기 저장 에러");
            }

            try
            {
                //cam1.SetExp(Convert.ToInt32(dgvCam1.Rows[0].Cells[0].Value));
                //cam1.TestShot(1);
                cam2.SetExp(Convert.ToInt32(dgvCam2.Rows[0].Cells[0].Value));
                cam2.TestShot(1);
            }
            catch (Exception)
            {
                Log_K.WriteLog(log_lst, Mainpath, "카메라밝기 저장 에러");
                Console.WriteLine("카메라밝기 저장 에러");
            }
            //cam1.SetExp(Convert.ToInt32(dgvCam1.Rows[0].Cells[0].Value));
            //cam1.TestShot(1);

        }

        private void Btn_Cam1live_Click(object sender, EventArgs e)     //  카메라 라이브
        {
            try
            {
                if (!LiveFlag)
                {
                    cam1.LiveShot(100);
                    LiveFlag = true;
                    label_LiveStatus.BackColor = Color.Lime;
                }
                else if (LiveFlag)
                {
                    cam1.Stop();
                    LiveFlag = false;
                    label_LiveStatus.BackColor = Color.Crimson;
                }
            }
            catch (Exception)
            {
                //Log_K.WriteLog(log_err, Mainpath, "카메라1 라이브 에러");
                Console.WriteLine("카메라1 라이브 에러");
            }
        }

        private void Btn_Cam2live_Click(object sender, EventArgs e)
        {
            try
            {
                if (!LiveFlag2)
                {
                    cam2.LiveShot(100);
                    LiveFlag2 = true;
                    label_LiveStatus2.BackColor = Color.Lime;
                }
                else if (LiveFlag2)
                {
                    cam2.Stop();
                    LiveFlag2 = false;
                    label_LiveStatus2.BackColor = Color.Crimson;
                }
            }
            catch (Exception)
            {
                //Log_K.WriteLog(log_err, Mainpath, "카메라1 라이브 에러");
                Console.WriteLine("카메라2 라이브 에러");
            }
        }

        private void Btn_Cam1trigger_Click(object sender, EventArgs e)      //  카메라 트리거
        {
            try
            {
                cam1.OneShot();
            }
            catch (Exception)
            {
                //Log_K.WriteLog(log_err, Mainpath, "카메라1 트리거 에러");
                Console.WriteLine("카메라1 트리거 에러");
            }
        }

        private void Btn_Cam1Continuous_Click(object sender, EventArgs e)       //  카메라 컨티뉴어스
        {
            Log_K.WriteLog(log_lst, Mainpath, "컨티뉴어스 트리거 In -> " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff"));

            //cam1.ContinuousShot();

            Log_K.WriteLog(log_lst, Mainpath, "컨티뉴어스 완료 -> " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff"));
        }

        private void Btn_Cam1ContinuousStop_Click(object sender, EventArgs e)   //  카메라 Stop
        {
            cam1.Stop();
        }
        #endregion

        #region 설정 화면 버튼
        private void simpleButton28_Click(object sender, EventArgs e)   //  세팅 세이브
        {
            GridMaster.SaveCSV_OnlyData(dgvS1, System.Windows.Forms.Application.StartupPath + "\\" + CurrentModelNum1 + "_S1.csv");//셀데이터 세이브
            SettingMinMax();
            GridMaster.SaveCSV_OnlyData(dgvS2, System.Windows.Forms.Application.StartupPath + "\\" + CurrentModelNum2 + "_S2.csv");//셀데이터 세이브
            SettingMinMax2();
        }

        private void simpleButton18_Click(object sender, EventArgs e)   //  프로그램 자동실행 On
        {
            try
            {
                // 시작프로그램 등록하는 레지스트리
                string runKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
                RegistryKey strUpKey = Registry.LocalMachine.OpenSubKey(runKey);
                if (strUpKey.GetValue("VisionStartup") == null)
                {
                    strUpKey.Close();
                    strUpKey = Registry.LocalMachine.OpenSubKey(runKey, true);
                    // 시작프로그램 등록명과 exe경로를 레지스트리에 등록
                    strUpKey.SetValue("VisionStartup", Application.ExecutablePath);
                }
                MessageBox.Show("성공적으로 적용되었습니다.");
            }
            catch
            {
                MessageBox.Show("적용 실패했습니다.");
            }
        }

        private void simpleButton19_Click(object sender, EventArgs e)   //  프로그램 자동실행 Off
        {
            try
            {
                string runKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
                RegistryKey strUpKey = Registry.LocalMachine.OpenSubKey(runKey, true);
                // 레지스트리값 제거
                strUpKey.DeleteValue("VisionStartup");
                MessageBox.Show("해제 처리되었습니다.");
            }
            catch
            {
                MessageBox.Show("적용 실패했습니다.");
            }
        }
        #endregion

        #region DB 화면 버튼
        private void simpleButton14_Click(object sender, EventArgs e)   //  검색
        {
            SelectHistory();
        }

        private void simpleButton15_Click(object sender, EventArgs e)   //  오늘날짜 설정
        {
            SetToday();
        }

        private void simpleButton3_Click(object sender, EventArgs e)    //  csv 파일로 저장
        {
            Directory.CreateDirectory(@"D:\" + Mainpath + @"\Data\");
            GridMaster.SaveCSV(dgvH0, @"D:\" + Mainpath + @"\Data\" + Dtime.Now(Dtime.StringType.ForFile) + ".csv");

            MessageBox.Show("Save Data.\nLocation : " + @"D:\" + Mainpath + @"\Data\", "Message");
        }

        private void simpleButton34_Click(object sender, EventArgs e)   //  csv 폴더 열기
        {
            Directory.CreateDirectory(@"D:\" + Mainpath + @"\Data\");
            System.Diagnostics.Process.Start("explorer.exe", @"D:\" + Mainpath + @"\Data\");
        }
        #endregion

        void autoRun()
        {
            plc1.CommStart();

            dgvStatus1.Rows[0].Cells[0].Style.BackColor = Color.Lime;
            dgvStatus1.Rows [ 0 ].Cells [ 0 ].Value = "Auto Run";

            dgvStatus2.Rows[0].Cells[0].Style.BackColor = Color.Lime;
            dgvStatus2.Rows[0].Cells[0].Value = "Auto Run";

            Btn_AutoStop.Enabled = true;

            Btn_AutoStart.Enabled = false;
            Btn_Manual.Enabled = false;
            Btn_Manual2.Enabled = false;
            Btn_Model.Enabled = false;
            Btn_Cam.Enabled = false;
            Btn_Vision.Enabled = false;
            //Btn_History.Enabled = false;

            xtraTabControl_Model.SelectedTabPage = Tab_Main;

            cogToolGroupEditV21.Subject = null;
            cogToolGroupEditV22.Subject = null;
        }

        void autoStop()
        {
            plc1.CommStop();

            dgvStatus1.Rows[0].Cells[0].Style.BackColor = Color.Crimson;
            dgvStatus1.Rows [ 0 ].Cells [ 0 ].Value = "Auto Stop";

            dgvStatus2.Rows[0].Cells[0].Style.BackColor = Color.Crimson;
            dgvStatus2.Rows[0].Cells[0].Value = "Auto Stop";

            Btn_AutoStart.Enabled = true;

            Btn_AutoStop.Enabled = false;
            Btn_Manual.Enabled = true;
            Btn_Manual2.Enabled = true;
            Btn_Model.Enabled = true;
            Btn_Cam.Enabled = true;
            Btn_Vision.Enabled = true;
            //Btn_History.Enabled = true;
        }


        #region 트리거 triger
        
        //t1t1t1t1t1
        private void triger1()  //  vision2
        {
            
            autoDelete();

            string time = DateTime.Now.ToString("HH.mm.ss");
            totalcnt += 1;

            try
            {

                Bitmap cbmp = new Bitmap(pictureBox_Cam1.Image);

                CogImage8Grey cimage = new CogImage8Grey(cbmp);

                CogIPOneImageTool ipt = (CogIPOneImageTool)Cogtg.Tools[0];

                ipt.InputImage = cimage;
                ipt.Run();

                CogToolBlock result = (CogToolBlock)Cogtg.Tools["result"];

                for (int k = 0; k < result.Inputs.Count; k++)    //  데이터 0으로 초기화
                {
                    result.Inputs[k].Value = 0;
                }

                Cogtg.Run();

                double[] resultall = new double[100]; //  전체결과 앞부터 3개씩 데이터 합치기

                for (int i = 0; i < result.Inputs.Count / 3; i++)
                {
                    resultall[i] = Convert.ToDouble(result.Inputs[i * 3].Value) + Convert.ToDouble(result.Inputs[i * 3 + 1].Value) + Convert.ToDouble(result.Inputs[i * 3 + 2].Value);
                }

                for (int j = 0; j < checksetting; j++) // 1,2부터 시작 - 데이터 넣기
                {
                    this.Invoke(new dele(() =>
                    {
                        if (j == 16)
                            dgvD1.Rows[j + 1].Cells[1].Value = Convert.ToInt32(resultall[j] * 100);
                        else
                            dgvD1.Rows[j + 1].Cells[1].Value = Convert.ToInt32(resultall[j]);
                    }));
                }

                string imgsavepath = @"D:\Vision\Image";
                string year = imgsavepath + "\\" + DateTime.Now.ToString("yyyy");
                string month = year + DateTime.Now.ToString("MM");
                string day = month + DateTime.Now.ToString("dd");
                string cam1path = day + "\\Cam1";
                string cam2path = day + "\\Cam2";
                string okpath = cam1path + "\\OK";
                string ngpath = cam1path + "\\NG";


                if (!System.IO.Directory.Exists(day))
                    System.IO.Directory.CreateDirectory(day);
                if (!System.IO.Directory.Exists(okpath))
                    System.IO.Directory.CreateDirectory(okpath);
                if (!System.IO.Directory.Exists(ngpath))
                    System.IO.Directory.CreateDirectory(ngpath);

                int pattern = Convert.ToInt32(resultall[16] * 100);

                this.Invoke(new dele(() =>
                {
                    int cnt = 0;
                    for (int i = 0; i < checksetting; i++)
                    {
                        if (i == 16)
                        {
                            if (min[i] <= pattern && pattern <= max[i])
                            {
                                dgvD1.Rows[i + 1].Cells[1].Style.BackColor = Color.LightGreen;
                                cnt += 1;
                            }
                            else
                            {
                                dgvD1.Rows[i + 1].Cells[1].Style.BackColor = Color.Crimson;
                            }
                        }
                        else
                        {
                            if (min[i] <= resultall[i] && resultall[i] <= max[i])
                            {
                                dgvD1.Rows[i + 1].Cells[1].Style.BackColor = Color.LightGreen;
                                cnt += 1;
                            }
                            else
                            {
                                dgvD1.Rows[i + 1].Cells[1].Style.BackColor = Color.Crimson;
                            }
                        }
                    }

                    if (cnt == checksetting)    //  ok판정
                    {
                        okcnt += 1;
                        Label_Result1.Text = "O K";
                        Label_Result1.BackColor = Color.LightGreen;

                        try
                        {
                            if(check_OKImage1.Checked)
                                pictureBox_Cam1.Image.Save(okpath + "\\" + ModelNamelbl1.Text + "_" + time + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);

                            Log_K.WriteLog(log_lst, Mainpath, "[Cam1 결과 : OK]" + Environment.NewLine);

                            Decision1 = "OK";
                            Delay(100);

                                plc1.MasterK_Write_W("3230303131", "0100"); //  최종판정ok 

                            try
                            {
                               
                                this.Invoke(new dele(() =>
                                {
                                    Log_K.WriteLog(log_lst, Mainpath, "Cam1 검사 최종판정ok 보냄");
                                }));
                            }
                            catch (Exception)
                            {
                                this.Invoke(new dele(() =>
                                {
                                    Log_K.WriteLog(log_lst, Mainpath, "Cam1 검사 최종판정ok 에러");
                                }));
                            }

                            //Delay(500);
                            try
                            {

                                this.Invoke(new dele(() =>
                                {
                                    Log_K.WriteLog(log_lst, Mainpath, "Cam1 검사 완료신호 보냄");
                                }));
                            }
                            catch (Exception)
                            {
                                this.Invoke(new dele(() =>
                                {
                                    Log_K.WriteLog(log_lst, Mainpath, "Cam1 검사 완료신호 에러");
                                }));
                            }

                        }
                        catch (Exception)
                        {
                            Log_K.WriteLog(log_lst, Mainpath, "검사 후 전송 OK 에러");
                            Console.WriteLine("검사 후 전송 OK");
                        }
                        Delay(100);
                    
                            plc1.MasterK_Write_W("3230303131", "0100"); //  최종판정ok 
                      
                    }
                    else                // ng 판정
                    {
                        ngcnt += 1;
                        Label_Result1.Text = "N G";
                        Label_Result1.BackColor = Color.Crimson;

                        try
                        {
                            if (check_NGImage1.Checked)
                                pictureBox_Cam1.Image.Save(ngpath + "\\" + ModelNamelbl1.Text + "__" + time + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);

                            Log_K.WriteLog(log_lst, Mainpath, "[Cam1 결과 : NG]" + Environment.NewLine);

                            Decision1 = "NG";
                            Delay(100);

                                plc1.MasterK_Write_W("3230303131", "0200"); //  최종판정ng 

                            try
                            {
                                //plc1.MasterK_Write_W("3230303131", "0200"); //  최종판정ng
                                this.Invoke(new dele(() =>
                                {
                                    Log_K.WriteLog(log_lst, Mainpath, "Cam1 검사 최종판정ng 보냄");
                                }));
                            }
                            catch (Exception)
                            {
                                this.Invoke(new dele(() =>
                                {
                                    Log_K.WriteLog(log_lst, Mainpath, "Cam1 검사 최종판정ng 에러");
                                }));
                            }

                        }
                        catch (Exception)
                        {
                            Log_K.WriteLog(log_lst, Mainpath, "검사 후 전송 NG 에러");
                        }
                        Delay(100);

                            plc1.MasterK_Write_W("3230303131", "0200"); //  최종판정ng 
                    }


                    string cmd = Ken2.Database.SQLiteCMD_K.MakeInsertCmdSentence(sql.table,   //  무조건 DB에 올림 A로

                            "Model", ModelNamelbl1.Text,
                        "Datetime", Dtime.Now(Dtime.StringType.ForDatum),
                        "CamNum", "1",
                        "CamResult1", Decision1
                        
                            );

                    sql.ExecuteNonQuery(cmd);

                    Log_K.WriteLog(log_lst, Mainpath, "Cam1 DB에 데이터 > 인서트");
                   

                }));
                

                for (int k = 0; k < result.Inputs.Count; k++)    //  데이터 0으로 초기화
                {
                    result.Inputs[k].Value = 0;
                }

                this.Invoke(new dele(() =>
                {
                    cogRecordDisplay1.Record = Cogtg.CreateLastRunRecord().SubRecords[0];  //  메인화면 이미지 띄우기
                    cogRecordDisplay1.AutoFit = true;
                }));


            }
            catch (Exception ex)
            {
                Log_K.WriteLog(log_lst, Mainpath, "triger1 함수NG 에러");
                Console.WriteLine("triger1 함수 NG");
            }

        }


        //t2t2t2t2
        private void triger2()
        {
            autoDelete();

            string time = DateTime.Now.ToString("HH.mm.ss");
            totalcnt2 += 1;

            try
            {

                Bitmap cbmp = new Bitmap(pictureBox_Cam2.Image);
                CogImage8Grey cimage = new CogImage8Grey(cbmp);
                CogIPOneImageTool ipt = (CogIPOneImageTool)Cogtg2.Tools[0];

                ipt.InputImage = cimage;
                ipt.Run();

                CogToolBlock result = (CogToolBlock)Cogtg2.Tools["result"];

                for (int k = 0; k < result.Inputs.Count; k++)    //  데이터 0으로 초기화
                {
                    result.Inputs[k].Value = 0;
                }

                Cogtg2.Run();

                double[] resultall = new double[100]; //  전체결과 앞부터 3개씩 데이터 합치기

                for (int i = 0; i < result.Inputs.Count / 3; i++)
                {
                    resultall[i] = Convert.ToDouble(result.Inputs[i * 3].Value) + Convert.ToDouble(result.Inputs[i * 3 + 1].Value) + Convert.ToDouble(result.Inputs[i * 3 + 2].Value);
                }

                for (int j = 0; j < checksetting2; j++) // 1,2부터 시작 - 데이터 넣기
                {
                    this.Invoke(new dele(() =>
                    {
                        if (j == 16)
                            dgvD2.Rows[j + 1].Cells[1].Value = Convert.ToInt32(resultall[j] * 100);
                        else
                            dgvD2.Rows[j + 1].Cells[1].Value = Convert.ToInt32(resultall[j]);
                    }));
                }

                string imgsavepath = @"D:\Vision\Image";
                string year = imgsavepath + "\\" + DateTime.Now.ToString("yyyy");
                string month = year + DateTime.Now.ToString("MM");
                string day = month + DateTime.Now.ToString("dd");
                string cam1path = day + "\\Cam1";
                string cam2path = day + "\\Cam2";
                string okpath2 = cam2path + "\\OK";
                string ngpath2 = cam2path + "\\NG";

                if (!System.IO.Directory.Exists(day))
                    System.IO.Directory.CreateDirectory(day);
                if (!System.IO.Directory.Exists(okpath2))
                    System.IO.Directory.CreateDirectory(okpath2);
                if (!System.IO.Directory.Exists(ngpath2))
                    System.IO.Directory.CreateDirectory(ngpath2);


                int pattern = Convert.ToInt32(resultall[16] * 100);

                this.Invoke(new dele(() =>
                {
                    int cnt = 0;
                    for (int i = 0; i < checksetting2; i++)
                    {
                        if (i == 16)
                        {
                            if (min2[i] <= pattern && pattern <= max2[i])
                            {
                                dgvD2.Rows[i + 1].Cells[1].Style.BackColor = Color.LightGreen;
                                cnt += 1;
                            }
                            else
                            {
                                dgvD2.Rows[i + 1].Cells[1].Style.BackColor = Color.Crimson;
                            }
                        }
                        else
                        {
                            if (min2[i] <= resultall[i] && resultall[i] <= max2[i])
                            {
                                dgvD2.Rows[i + 1].Cells[1].Style.BackColor = Color.LightGreen;
                                cnt += 1;
                            }
                            else
                            {
                                dgvD2.Rows[i + 1].Cells[1].Style.BackColor = Color.Crimson;
                            }
                        }
                    }

                    if (cnt == checksetting2)    //  ok판정
                    {
                        okcnt2 += 1;
                        Label_Result2.Text = "O K";
                        Label_Result2.BackColor = Color.LightGreen;

                        try
                        {
                            if (check_OKImage2.Checked)
                                pictureBox_Cam2.Image.Save(okpath2 + "\\" + ModelNamelbl1.Text + "_" + time + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                            
                            Log_K.WriteLog(log_lst, Mainpath, "[Cam2 결과 : OK]" + Environment.NewLine);
                            
                            //txt_Cam2Result.Text = "OK";

                            plc1.MasterK_Write_W("3230303134", "0100"); //  카메라 2 최종판정ok 

                            try
                            {
                                this.Invoke(new dele(() =>
                                {
                                    Log_K.WriteLog(log_lst, Mainpath, "Cam2 검사 최종판정ok 보냄");
                                }));
                            }
                            catch (Exception)
                            {
                                this.Invoke(new dele(() =>
                                {
                                    Log_K.WriteLog(log_lst, Mainpath, "Cam2 검사 최종판정ok 에러");
                                }));
                            }
                        }
                        catch (Exception)
                        {
                            Log_K.WriteLog(log_lst, Mainpath, "검사 후 전송 OK 에러");
                            Console.WriteLine("검사 후 전송 OK");
                        }

                            plc1.MasterK_Write_W("3230303134", "0100"); //  카메라 2 최종판정ok 
                    }
                    else                // ng 판정
                    {
                        ngcnt2 += 1;
                        Label_Result2.Text = "N G";
                        Label_Result2.BackColor = Color.Crimson;

                        try
                        {
                            if (check_NGImage2.Checked)
                                pictureBox_Cam2.Image.Save(ngpath2 + "\\" + ModelNamelbl1.Text + "__" + time + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                           
                            Log_K.WriteLog(log_lst, Mainpath, "[Cam2 결과 : NG]" + Environment.NewLine);

                            //txt_Cam2Result.Text = "NG";
                            
                                plc1.MasterK_Write_W("3230303134", "0200"); //  카메라2 최종판정ok 
                           
                        }
                        catch (Exception)
                        {
                            Log_K.WriteLog(log_lst, Mainpath, "검사 후 전송 NG 에러");
                            Console.WriteLine("Cam2 검사 후 전송 NG ");
                        }

                            plc1.MasterK_Write_W("3230303134", "0200"); //  카메라2 최종판정ok 
                    }

                }));

                for (int k = 0; k < result.Inputs.Count; k++)    //  데이터 0으로 초기화
                {
                    result.Inputs[k].Value = 0;
                }

                this.Invoke(new dele(() =>
                {
                    cogRecordDisplay2.Record = Cogtg2.CreateLastRunRecord().SubRecords[0];  //  메인화면 이미지 띄우기
                    cogRecordDisplay2.AutoFit = true;
                }));

            }
            catch (Exception ex)
            {
                Log_K.WriteLog(log_lst, Mainpath, "triger2 함수NG 에러");
                Console.WriteLine("triger2 함수 NG");
            }

        }

        #endregion

       

        private void SettingMinMax()
        {
            try
            {
                for (int i = 0; i < checksetting; i++)
                {
                    min[i] = Convert.ToInt32(dgvS1.Rows[i].Cells[1].Value);
                }

                for (int j = 0; j < checksetting; j++)
                {
                    max[j] = Convert.ToInt32(dgvS1.Rows[j].Cells[2].Value);
                }

            }
            catch (Exception)
            {
                Log_K.WriteLog(log_lst, Mainpath, "settingMinmax 에러" + Environment.NewLine);
            }
        }


        private void SettingMinMax2()
        {
            try
            {
                for (int i = 0; i < checksetting2; i++)
                {
                    min2[i] = Convert.ToInt32(dgvS2.Rows[i].Cells[1].Value);
                }

                for (int j = 0; j < checksetting2; j++)
                {
                    max2[j] = Convert.ToInt32(dgvS2.Rows[j].Cells[2].Value);
                }

            }
            catch (Exception)
            {
                Log_K.WriteLog(log_lst, Mainpath, "settingMinmax2 에러" + Environment.NewLine);
            }
        }

        private void simpleButton71_Click(object sender, EventArgs e)   //  JobIn
        {
            if (xtraTabControlVision.SelectedTabPage == Tab_VisionTool1)
            {
                int modelnum = Convert.ToInt32(Txt_LastModel1.Text);
                Cogtg = (CogToolGroup)CogSerializer.LoadObjectFromFile(System.Windows.Forms.Application.StartupPath + "\\" + modelnum + "_1.vpp");
                cogToolGroupEditV21.Subject = Cogtg;
                MessageBox.Show(ModelNamelbl1.Text + " 툴을 불러왔습니다.");
            }
            if (xtraTabControlVision.SelectedTabPage == Tab_VisionTool2)
            {
                int modelnum = Convert.ToInt32(Txt_LastModel1.Text);
                Cogtg = (CogToolGroup)CogSerializer.LoadObjectFromFile(System.Windows.Forms.Application.StartupPath + "\\" + modelnum + "_2.vpp");          
                cogToolGroupEditV22.Subject = Cogtg2;
                MessageBox.Show(ModelNamelbl1.Text + " 툴을 불러왔습니다.");
            }
        }


        private void simpleButton8_Click(object sender, EventArgs e)    //  JobOut
        {
            if (xtraTabControlVision.SelectedTabPage == Tab_VisionTool1)
            {
                Cogtg = cogToolGroupEditV21.Subject;
                cogToolGroupEditV21.Subject = null;
            }
            if (xtraTabControlVision.SelectedTabPage == Tab_VisionTool2)
            {
                Cogtg2 = cogToolGroupEditV22.Subject;
                cogToolGroupEditV22.Subject = null;
            }
        }
               
        public void autoDelete()   //  폴더 자동 삭제
        {
            try
            {
                int deleteDay = Int32.Parse(Txt_DeleteDay.Text);  //  보관할 날짜
                DirectoryInfo di = new DirectoryInfo(@"D:\Vision\Image");
                if (di.Exists)
                {
                    DirectoryInfo[] dirInfo = di.GetDirectories();
                    string IDate = DateTime.Today.AddDays(-deleteDay).ToString("yyyyMMdd"); //  최근 수정된날짜 해당 날짜동안 보관 경우 삭제( 오늘 3월17일 , 삭제날짜 1일일 경우 16일까지 보관)

                    foreach (DirectoryInfo dir in dirInfo)
                    {
                        if (IDate.CompareTo(dir.LastWriteTime.ToString("yyyyMMdd")) > 0)
                        {
                            dir.Attributes = FileAttributes.Normal;
                            dir.Delete(true);
                        }
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("자동삭제");
            }
        }
        
        private void SetToday()
        {
            Date0.Value = DateTime.Now;
            Date1.Value = DateTime.Now;

            Time0.Time = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
            Time1.Time = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);
        }

        private void SelectHistory()
        {
            dgvH0.Columns.Clear();

            if (NameSearchcheck.Checked)
            {

                string cmd = SQLiteCMD_K.Select_Equal("table1", "Barcode", NameSearchTB.Text,

                        "Model", 
                        "Datetime",
                        "Barcode",
                        "CamResult1",
                        "CamResult2",
                        "CapNum",
                        "CapResult",
                        "Wash",
                        "Cap1",
                        "Cap2",
                        "Cap3",
                        "LVDTResult",
                        "LVDT1",
                        "LVDT2",
                        "LVDT3",
                        "LVDT4",
                        "LVDT5",
                        "LVDT6",
                        "LVDT7",
                        "LVDT8",
                        "LVDT9",
                        "GasLeakNum",
                        "GasLeakResult",
                        "GasLeak1",
                        "WaterLeakNum",
                        "WaterLeakResult",
                        "WaterLeak1"
                        );

                sql.Select(dgvH0, cmd, false);
            }
            else
            {
                string cmd = SQLiteCMD_K.Select_Datetime("table1", "Datetime", Dtime.GetDateTime_string(Date0, Time0), Dtime.GetDateTime_string(Date1, Time1), "",

                        "Model",
                        "Datetime",
                        "Barcode",
                        "CamResult1",
                        "CamResult2",
                        "CapNum",
                        "CapResult",
                        "Wash",
                        "Cap1",
                        "Cap2",
                        "Cap3",
                        "LVDTResult",
                        "LVDT1",
                        "LVDT2",
                        "LVDT3",
                        "LVDT4",
                        "LVDT5",
                        "LVDT6",
                        "LVDT7",
                        "LVDT8",
                        "LVDT9",
                        "GasLeakNum",
                        "GasLeakResult",
                        "GasLeak1",
                        "WaterLeakNum",
                        "WaterLeakResult",
                        "WaterLeak1"

                        );

                sql.Select(dgvH0, cmd, false);
         
            }
            
            dgvInit("dgvH0");
        }

        private void simpleButton2_Click(object sender, EventArgs e)    //  recheck route Button / Visible = false
        {
            System.Windows.Forms.OpenFileDialog choofdlog = new System.Windows.Forms.OpenFileDialog();
            if(textBox4.Text == "" || textBox4.Text == null)
            {
                choofdlog.InitialDirectory = @"D:\" + Mainpath + @"\Image\";
            }
            else
            {
                string folder = textBox4.Text;
                choofdlog.InitialDirectory = @"" + folder + "\\";
            }

            if(choofdlog.ShowDialog() == DialogResult.OK)
            {
                Bitmap bmp = (Bitmap)Image.FromFile(choofdlog.FileName);
                pictureBox_Cam1.Image = bmp;
                retriger1();
            }
        }

      
        private void retriger1()  
        {
            this.Invoke(new dele(() =>
            {
                Log_K.WriteLog(log_lst, Mainpath, "[Cam1] ReTriger1" + Environment.NewLine);
            }));

            autoDelete();

            string time = DateTime.Now.ToString("HH.mm.ss");

            try
            {

                Bitmap cbmp = new Bitmap(pictureBox_Cam1.Image);
                CogImage8Grey cimage = new CogImage8Grey(cbmp);
                CogIPOneImageTool ipt = (CogIPOneImageTool)Cogtg.Tools[0];

                ipt.InputImage = cimage;
                ipt.Run();

                CogToolBlock result = (CogToolBlock)Cogtg.Tools["result"];

                for (int k = 0; k < result.Inputs.Count; k++)    //  데이터 0으로 초기화
                {
                    result.Inputs[k].Value = 0;
                }

                Cogtg.Run();

                double[] resultall = new double[100]; //  전체결과 앞부터 3개씩 데이터 합치기

                for (int i = 0; i < result.Inputs.Count / 3; i++)
                {
                    resultall[i] = Convert.ToDouble(result.Inputs[i * 3].Value) + Convert.ToDouble(result.Inputs[i * 3 + 1].Value) + Convert.ToDouble(result.Inputs[i * 3 + 2].Value);
                }

                for (int j = 0; j < checksetting; j++) // 1,2부터 시작 - 데이터 넣기
                {
                    this.Invoke(new dele(() =>
                    {
                        if (j == 16)
                            dgvD1.Rows[j + 1].Cells[1].Value = Convert.ToInt32(resultall[j] * 100);
                        else
                            dgvD1.Rows[j + 1].Cells[1].Value = Convert.ToInt32(resultall[j]);
                    }));
                }

                int pattern = Convert.ToInt32( resultall[16] * 100);

                this.Invoke(new dele(() =>
                {
                    int cnt = 0;
                    for (int i = 0; i < checksetting; i++)
                    {
                        if (i == 16)
                        {
                            if (min[i] <= pattern && pattern <= max[i])
                            {
                                dgvD1.Rows[i + 1].Cells[1].Style.BackColor = Color.LightGreen;
                                cnt += 1;
                            }
                            else
                            {
                                dgvD1.Rows[i + 1].Cells[1].Style.BackColor = Color.Crimson;
                            }
                        }
                        else
                        {
                            if (min[i] <= resultall[i] && resultall[i] <= max[i])
                            {
                                dgvD1.Rows[i + 1].Cells[1].Style.BackColor = Color.LightGreen;
                                cnt += 1;
                            }
                            else
                            {
                                dgvD1.Rows[i + 1].Cells[1].Style.BackColor = Color.Crimson;
                            }
                        }
                    }


                    if (cnt == checksetting)    //  ok판정
                    {
                        Label_Result1.Text = "O K";
                        Label_Result1.BackColor = Color.LightGreen;

                        try
                        {

                            Log_K.WriteLog(log_lst, Mainpath, "[Cam1 Re 결과 : OK]" + Environment.NewLine);

                            Decision1 = "OK";

                        }
                        catch (Exception)
                        {
                            Log_K.WriteLog(log_lst, Mainpath, "rt1 검사 후 전송 OK 에러");
                        }
                    }
                    else                // ng 판정
                    {
                        Label_Result1.Text = "N G";
                        Label_Result1.BackColor = Color.Crimson;
                        try
                        {

                            Log_K.WriteLog(log_lst, Mainpath, "[Cam1 Re 결과 : NG]" + Environment.NewLine);

                            Decision1 = "NG";

                        }
                        catch (Exception)
                        {
                            Log_K.WriteLog(log_lst, Mainpath, "rt1 검사 후 전송 NG 에러");
                        }
                    }
                }));


                for (int k = 0; k < result.Inputs.Count; k++)    //  데이터 0으로 초기화
                {
                    result.Inputs[k].Value = 0;
                }

                this.Invoke(new dele(() =>
                {
                    cogRecordDisplay1.Record = Cogtg.CreateLastRunRecord().SubRecords[0];  //  메인화면 이미지 띄우기
                    cogRecordDisplay1.AutoFit = true;
                }));


            }
            catch (Exception ex)
            {
                Log_K.WriteLog(log_lst, Mainpath, "retriger1 함수NG 에러");
                Console.WriteLine("retriger1 함수 NG");
            }

        }

        private void retriger2()
        {
            this.Invoke(new dele(() =>
            {
                Log_K.WriteLog(log_lst, Mainpath, "[Cam2] ReTriger2" + Environment.NewLine);
            }));

            autoDelete();

            string time = DateTime.Now.ToString("HH.mm.ss");

            try
            {

                Bitmap cbmp = new Bitmap(pictureBox_Cam2.Image);
                CogImage8Grey cimage = new CogImage8Grey(cbmp);
                CogIPOneImageTool ipt = (CogIPOneImageTool)Cogtg2.Tools[0];

                ipt.InputImage = cimage;
                ipt.Run();

                CogToolBlock result = (CogToolBlock)Cogtg2.Tools["result"];

                for (int k = 0; k < result.Inputs.Count; k++)    //  데이터 0으로 초기화
                {
                    result.Inputs[k].Value = 0;
                }

                Cogtg2.Run();

                double[] resultall = new double[100]; //  전체결과 앞부터 3개씩 데이터 합치기

                for (int i = 0; i < result.Inputs.Count / 3; i++)
                {
                    resultall[i] = Convert.ToDouble(result.Inputs[i * 3].Value) + Convert.ToDouble(result.Inputs[i * 3 + 1].Value) + Convert.ToDouble(result.Inputs[i * 3 + 2].Value);
                }

                for (int j = 0; j < checksetting2; j++) // 1,2부터 시작 - 데이터 넣기
                {
                    this.Invoke(new dele(() =>
                    {
                        if (j == 16)
                            dgvD2.Rows[j + 1].Cells[1].Value = Convert.ToInt32(resultall[j] * 100);
                        else
                            dgvD2.Rows[j + 1].Cells[1].Value = Convert.ToInt32(resultall[j]);
                    }));
                }

                int pattern = Convert.ToInt32( resultall[16] * 100);

                this.Invoke(new dele(() =>
                {
                    int cnt = 0;
                    for (int i = 0; i < checksetting2; i++)
                    {
                        if (i == 16)
                        {
                            if (min2[i] <= pattern && pattern <= max2[i])
                            {
                                dgvD2.Rows[i + 1].Cells[1].Style.BackColor = Color.LightGreen;
                                cnt += 1;
                            }
                            else
                            {
                                dgvD2.Rows[i + 1].Cells[1].Style.BackColor = Color.Crimson;
                            }
                        }
                        else
                        {
                            if (min2[i] <= resultall[i] && resultall[i] <= max2[i])
                            {
                                dgvD2.Rows[i + 1].Cells[1].Style.BackColor = Color.LightGreen;
                                cnt += 1;
                            }
                            else
                            {
                                dgvD2.Rows[i + 1].Cells[1].Style.BackColor = Color.Crimson;
                            }
                        }
                    }

                    if (cnt == checksetting2)    //  ok판정
                    {
                        Label_Result2.Text = "O K";
                        Label_Result2.BackColor = Color.LightGreen;
  
                        try
                        {

                            Log_K.WriteLog(log_lst, Mainpath, "[Cam2 Re 결과 : OK]" + Environment.NewLine);

                            Decision2 = "OK";

                        }
                        catch (Exception)
                        {
                            Log_K.WriteLog(log_lst, Mainpath, "rt2 검사 후 전송 OK 에러");
                        }
                    }
                    else                // ng 판정
                    {
                        Label_Result2.Text = "N G";
                        Label_Result2.BackColor = Color.Crimson;

                        try
                        {

                            Log_K.WriteLog(log_lst, Mainpath, "[Cam2 Re 결과 : NG]" + Environment.NewLine);

                            Decision2 = "NG";

                        }
                        catch (Exception)
                        {
                            Log_K.WriteLog(log_lst, Mainpath, "rt2 검사 후 전송 NG 에러");
                        }

                    }

                    }));

                for (int k = 0; k < result.Inputs.Count; k++)    //  데이터 0으로 초기화
                {
                    result.Inputs[k].Value = 0;
                }

                this.Invoke(new dele(() =>
                {
                    cogRecordDisplay2.Record = Cogtg2.CreateLastRunRecord().SubRecords[0];  //  메인화면 이미지 띄우기
                    cogRecordDisplay2.AutoFit = true;
                }));

            }
            catch (Exception ex)
            {
                Log_K.WriteLog(log_lst, Mainpath, "retriger2 함수NG 에러");
                Console.WriteLine("retriger2 함수 NG");
            }

        }

        string setTool = "";
        string setToolNum = "";
        string OpenTool = "";

        private void kenButton3_Click(object sender, EventArgs e)   //  세팅버튼1
        {
            setTool = comboBoxEdit1.Text;
            setToolNum = numericUpDown1.Value.ToString();
            OpenTool = setTool + setToolNum;
            
            if(setTool == "CogBlobTool")
            {
                try
                {
                    new Tools.Line(Cogtg.Tools[OpenTool], 0).Show();
                }
                catch (Exception)
                {

                }
            }

            else if(setTool == "CogFindCircleTool")
            {
                try
                {
                    new Tools.Circle(Cogtg.Tools[OpenTool], 0).Show();
                }
                catch (Exception)
                {

                }
            }
        }

        private void kenButton4_Click(object sender, EventArgs e)   //  패턴 설정1
        {
           new Tools.Line(Cogtg.Tools["CogFindLineTool1"], 0).ShowDialog();
        }

        private void kenButton10_Click(object sender, EventArgs e)  //  LoadImage1
        {
            System.Windows.Forms.OpenFileDialog choofdlog = new System.Windows.Forms.OpenFileDialog( );

            choofdlog.InitialDirectory = @"D:\" + Mainpath + @"\Image\";

            if (choofdlog.ShowDialog() == DialogResult.OK)
            {
                Bitmap bmp = (Bitmap)Image.FromFile(choofdlog.FileName);
                pictureBox_Cam1.Image = bmp;
                retriger1();
            }
            xtraTabControl_Model.SelectedTabPageIndex = 0;
        }

        string setTool2 = "";
        string setToolNum2 = "";
        string OpenTool2 = "";

        private void kenButton2_Click(object sender, EventArgs e)   //  세팅버튼2
        {
            setTool2 = comboBoxEdit2.Text;
            setToolNum2 = numericUpDown2.Value.ToString();
            OpenTool2 = setTool2 + setToolNum2;
            
            if (setTool2 == "CogBlobTool")
            {
                try
                {
                    new Tools.Blob(Cogtg2.Tools[OpenTool2], 0).Show();
                }
                catch (Exception)
                {

                }
            }

            else if (setTool2 == "CogFindCircleTool")
            {
                try
                {
                    new Tools.Circle(Cogtg2.Tools[OpenTool2], 0).Show();
                }
                catch (Exception)
                {

                }
            }

        }

        private void kenButton1_Click(object sender, EventArgs e)   //  패턴 설정2
        {
            new Tools.PMAlign(Cogtg2.Tools["CogPMAlignTool1"], 0).ShowDialog();
        }


        private void kenButton5_Click(object sender, EventArgs e)    //  LoadImage2
        {
            System.Windows.Forms.OpenFileDialog choofdlog = new System.Windows.Forms.OpenFileDialog();

            choofdlog.InitialDirectory = @"D:\" + Mainpath + @"\Image\";

            if (choofdlog.ShowDialog() == DialogResult.OK)
            {
                Bitmap bmp = (Bitmap)Image.FromFile(choofdlog.FileName);
                pictureBox_Cam2.Image = bmp;
                retriger2();
            }
            xtraTabControl_Model.SelectedTabPageIndex = 0;
        }


        private void NameSearchcheck_CheckedChanged(object sender, EventArgs e)
        {
            NameSearchTB.Text = "";
            if (NameSearchcheck.Checked)
            {
                NameSearchTB.Enabled = true;
            }
            else
            {
                NameSearchTB.Enabled = false;
            }
        }

        private void simpleButton4_Click(object sender, EventArgs e)
        {
            DBReset();
        }
        
        void DBReset()
        {
            string cmd = "DELETE FROM table1";
            sql.ExecuteNonQuery(cmd);
        }

        private void simpleButton5_Click(object sender, EventArgs e)
        {
            string bcrr = "a";
            string cmdd = "DELETE FROM table1 WHERE `Barcode` = '" + bcrr + "';";

            sql.ExecuteNonQuery(cmdd);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            plc1.MasterK_Write_W("3230303939", "0100");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            plc1.MasterK_Write_W("3230303939", "0000");
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            plc1.MasterK_Write_W("3230303939", "0100");
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            plc1.MasterK_Write_W("3230303939", "0000");
        }

        private void button2_Click(object sender, EventArgs e)
        {
                plc1.MasterK_Write_W("3230303034", "0100");
                plc1.MasterK_Write_W("3230303035", "0100");
            plc1.MasterK_Write_W("3230303036", "0100");
            plc1.MasterK_Write_W("3230303037", "0100"); 

        }

        private void button5_Click(object sender, EventArgs e)
        {
            plc1.MasterK_Write_W("3230303034", "0000");
            plc1.MasterK_Write_W("3230303035", "0000");
            plc1.MasterK_Write_W("3230303036", "0000");
            plc1.MasterK_Write_W("3230303037", "0000");
        }

        private void kenButton6_Click(object sender, EventArgs e)
        {
            new Tools.Line(Cogtg.Tools["CogFindLineTool2"], 0).ShowDialog();
        }

        private void kenButton8_Click(object sender, EventArgs e)
        {
            new Tools.Line(Cogtg2.Tools["CogFindLineTool1"], 0).ShowDialog();
        }

        private void kenButton7_Click(object sender, EventArgs e)
        {
            new Tools.Line(Cogtg2.Tools["CogFindLineTool2"], 0).ShowDialog();
        }

        private void dgvH0_CellFormatting( object sender, DataGridViewCellFormattingEventArgs e )
        {

        }
        
    }
}
