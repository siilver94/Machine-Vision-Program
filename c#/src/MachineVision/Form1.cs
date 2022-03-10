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


namespace VisionProgram
{
    public partial class Form1 : Form
    {
        private delegate void dele();

        Ken2.UIControl.dgvManager dgvmanager;

        PylonBasler cam1, cam2 = null;

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
        int checksetting = 1;    //  검사 데이터 및 min max 배열 수량

        int[] min2 = new int[20];    //  최소값 배열
        int[] max2 = new int[20];    //  최대값 배열
        int checksetting2 = 1;    //  검사 데이터 및 min max 배열 수량

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

        String ModelNum1 = "";
        String ModelNum2 = "";

        float dbData = 0;
        //bool LiveFlag = false;

        string Mainpath = "Vision";


        private object lockObj = new object();

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

            plc1 = new MasterK200_1("192.168.50.230", 2004, 1000, "192.168.50.101", 0, this);

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

            sql = new Mysql_K("127.0.0.1", "donghae_001", "table1", "a", "qwerasdf");

            StartmainThread(0);

            SetToday();

            Directory.CreateDirectory("D:\\" + Mainpath + "\\Log");
            Directory.CreateDirectory("D:\\" + Mainpath + "\\Image");
            Log_K.WriteLog(log_lst, Mainpath, "프로그램 시작");

            if (Txt_LastModel1.Text.Length > 0)
                CurrentModelNum1 = Int32.Parse(Txt_LastModel1.Text);
            else
                CurrentModelNum1 = 1;
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

            if (name.Equals("Trigger2"))
            {
                CamPoint2 = Convert.ToInt32(data);
                this.Invoke(new dele(() =>
                {
                    Log_K.WriteLog(log_lst, Mainpath, "자동 검사2 -> " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff"));
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

            if (name.Equals("Data"))
            {

                string[] AllData = (string[])data;
                string[] indata = new string[100];
                string[] address = new string[50];

                try
                {
                    for (int i = 0; i < 50; i++)
                    {
                        indata[i] = AllData[32 + i];
                    }


                    for (int j = 0; j < 50; j++)   //  address [] 배열에 스타트 번지부터 값 350개 넣기
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

                        for (int i = 0; i < 50; i++)
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


        }




        #region Cameraaaaaaaa
        private void cam1_ImageSignal(PylonBasler.CurrentStatus Command, object Data, int ArrayNum)
        {
            if (Command == PylonBasler.CurrentStatus.OneShot)     //  검사
            {
                pictureBox_Cam1.Image = (Bitmap)Data;
                image1 = (Bitmap)Data;
                triger1();
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
                Console.WriteLine("카메라2연결");
            }
            else
            {
                //Log_K.WriteLog(log_err, Mainpath, "카메라1해제");
                Console.WriteLine("카메라2해제");
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
                            "A", "A"
                        };
                        //int rows = 2;//초기 생성 Row수
                        int rows = 1;//초기 생성 Row수

                        GridMaster.Init3(dgv, true, height, rows, ColumnsName);


                        dgv.Rows[0].Cells[0].Value = "돌출 값";
                        //dgv.Rows[0].Cells[1].Value = "";

                        dgv.Rows[0].Cells[0].Style.Font = new Font("Tahoma", 19, FontStyle.Bold);
                        dgv.Rows[0].Cells[1].Style.Font = new Font("Tahoma", 19, FontStyle.Bold);


                        GridMaster.CenterAlign(dgv);
                        dgv.ReadOnly = true;//읽기전용
                        GridMaster.DisableSortColumn(dgv);//오름차순 내림차순 정렬 막기
                        dgv.ColumnHeadersVisible = false;//컬럼헤더 가리기                        

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

                        string[] ColumnsName = new string[] {
                            "A", "A"
                        };
                        int rows = 1;//초기 생성 Row수


                        GridMaster.Init3(dgv, true, height, rows, ColumnsName);

                        dgv.Rows[0].Cells[0].Value = "돌출 값";
                        dgv.Rows[0].Cells[1].Value = "값";

                        dgv.Rows[0].Cells[0].Style.Font = new Font("Tahoma", 19, FontStyle.Bold);
                        dgv.Rows[0].Cells[1].Style.Font = new Font("Tahoma", 19, FontStyle.Bold);

                        GridMaster.CenterAlign(dgv);

                        dgv.ReadOnly = true;//읽기전용
                        GridMaster.DisableSortColumn(dgv);//오름차순 내림차순 정렬 막기
                        dgv.ColumnHeadersVisible = false;//컬럼헤더 가리기                        

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

                        string[] ColumnsName = new string[] {
                            "A"
                        };
                        int rows = 3;//초기 생성 Row수

                        GridMaster.Init3(dgv, true, height, rows, ColumnsName);

                        dgv.Rows[0].Cells[0].Value = "Auto Run";
                        dgv.Rows[1].Cells[0].Value = "PLC (192.168.0.1)";
                        dgv.Rows[2].Cells[0].Value = "CAM (192.168.100.2)";

                        dgv.Rows[0].Cells[0].Style.BackColor = Color.Crimson;
                        dgv.Rows[1].Cells[0].Style.BackColor = Color.Crimson;
                        dgv.Rows[2].Cells[0].Style.BackColor = Color.Crimson;

                        GridMaster.CenterAlign(dgv);

                        dgv.ReadOnly = true;//읽기전용
                        //dgv.Columns[ 0 ].ReadOnly = true;//읽기전용

                        GridMaster.DisableSortColumn(dgv);//오름차순 내림차순 정렬 막기

                        dgv.ColumnHeadersVisible = false;//컬럼헤더 가리기                        
                        dgv.DefaultCellStyle.Font = new Font("Tahoma", 12, FontStyle.Bold);


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

                        GridMaster.CenterAlign(dgv);
                        dgv.ReadOnly = true;//읽기전용
                        GridMaster.DisableSortColumn(dgv);//오름차순 내림차순 정렬 막기
                        dgv.ColumnHeadersVisible = false;//컬럼헤더 가리기                        
                        dgv.DefaultCellStyle.Font = new Font("Tahoma", 12, FontStyle.Bold);

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
                        GridMaster.LoadCSV_OnlyData(dgv, System.Windows.Forms.Application.StartupPath + "\\Model1.csv");//셀데이터로드
                                                                                                                        //GridMaster.LoadCSV( dgvD0 , @"C:\Users\kclip3\Desktop\CR0.csv" );//셀데이터로드

                        for (int i = 1; i < 51; i++)
                        {
                            dgv.Rows[i - 1].Cells[0].Value = i - 1;
                        }

                        dgv.Rows[0].Cells[0].Value = "Model Num";
                        dgv.Rows[0].Cells[1].Value = "Model Name";

                        //ModelNum1 = Convert.ToString(dgv.Rows[0].Cells[0].Value);
                        GridMaster.CenterAlign(dgv);
                        dgv.ReadOnly = true;//읽기전용
                                            //dgv.Columns[ 0 ].ReadOnly = true;//읽기전용

                        GridMaster.DisableSortColumn(dgv);//오름차순 내림차순 정렬 막기
                        dgv.ColumnHeadersVisible = false;//컬럼헤더 가리기                        

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
                        GridMaster.LoadCSV_OnlyData(dgv, System.Windows.Forms.Application.StartupPath + "\\Model2.csv");//셀데이터로드
                                                                                                                        //GridMaster.LoadCSV( dgvD0 , @"C:\Users\kclip3\Desktop\CR0.csv" );//셀데이터로드

                        for (int i = 1; i < 51; i++)
                        {
                            dgv.Rows[i - 1].Cells[0].Value = i - 1;
                        }

                        dgv.Rows[0].Cells[0].Value = "Model Num";
                        dgv.Rows[0].Cells[1].Value = "Model Name";

                        GridMaster.CenterAlign(dgv);
                        dgv.ReadOnly = true;//읽기전용
                        GridMaster.DisableSortColumn(dgv);//오름차순 내림차순 정렬 막기
                        dgv.ColumnHeadersVisible = false;//컬럼헤더 가리기                        

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
                        GridMaster.CenterAlign(dgv);
                        GridMaster.DisableSortColumn(dgv);//오름차순 내림차순 정렬 막기
                        dgv.ColumnHeadersVisible = false;//컬럼헤더 가리기                        

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
                        GridMaster.CenterAlign(dgv);
                        GridMaster.DisableSortColumn(dgv);//오름차순 내림차순 정렬 막기
                        dgv.ColumnHeadersVisible = false;//컬럼헤더 가리기                        

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
                        GridMaster.CenterAlign(dgv);
                        GridMaster.DisableSortColumn(dgv);//오름차순 내림차순 정렬 막기

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
                        GridMaster.CenterAlign(dgv);
                        GridMaster.DisableSortColumn(dgv);//오름차순 내림차순 정렬 막기

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

                        string[] ColumnsName = new string[] {
                            "번지" , "내용" , "Data"
                        };
                        int rows = 50;//초기 생성 Row수

                        GridMaster.Init3(dgv, true, height, rows, ColumnsName);

                        GridMaster.LoadCSV_OnlyData(dgv, System.Windows.Forms.Application.StartupPath + "\\P1.csv");//셀데이터로드


                        for (int i = 0; i < rows; i++)
                        {
                            dgv.Rows[i].Cells[0].Value = "D" + (i + 20000);
                        }

                        GridMaster.CenterAlign(dgv);


                        GridMaster.DisableSortColumn(dgv);//오름차순 내림차순 정렬 막기


                    }
                    catch (Exception)
                    {

                    }

                    break;

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
                        GridMaster.CenterAlign(dgv);
                        dgv.ReadOnly = true;//읽기전용
                        //GridMaster.DisableSortColumn( dgv );//오름차순 내림차순 정렬 막기
                        dgv.Columns[0].ReadOnly = true;//읽기전용
                        //dgv.AllowUserToResizeColumns = false;//컬럼폭 수정불가
                        dgv.Columns[1].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss"; //표시형식
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
                        GridMaster.CenterAlign(dgv);

                        dgv.ReadOnly = true;//읽기전용
                        //GridMaster.DisableSortColumn( dgv );//오름차순 내림차순 정렬 막기
                        dgv.Columns[0].ReadOnly = true;//읽기전용

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
                        GridMaster.CenterAlign(dgv);

                        dgv.ReadOnly = true;//읽기전용
                        dgv.Columns[0].ReadOnly = true;//읽기전용

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
                        GridMaster.CenterAlign(dgv);

                        dgv.ReadOnly = true;//읽기전용
                        dgv.Columns[0].ReadOnly = true;//읽기전용

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
                        GridMaster.CenterAlign(dgv);

                        dgv.ReadOnly = true;//읽기전용

                        dgv.Columns[0].ReadOnly = true;//읽기전용

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
                        GridMaster.CenterAlign(dgv);
                        dgv.ReadOnly = true;//읽기전용
                        dgv.Columns[0].ReadOnly = true;//읽기전용

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
                        GridMaster.CenterAlign(dgv);
                        dgv.ReadOnly = true;//읽기전용
                        dgv.Columns[0].ReadOnly = true; //읽기전용
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
                        GridMaster.CenterAlign(dgv);
                        dgv.ReadOnly = true;//읽기전용
                        dgv.Columns[0].ReadOnly = true;//읽기전용

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
                        GridMaster.CenterAlign(dgv);
                        dgv.ReadOnly = true;//읽기전용                
                        dgv.Columns[0].ReadOnly = true;//읽기전용

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
                cam1.OneShot();
            }
            catch (Exception)
            {
                Log_K.WriteLog(log_lst, Mainpath, "카메라1 트리거 에러");
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
            System.Diagnostics.Process.Start("explorer.exe", @"D:\" + Mainpath + @"\Image\");
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

        private void Btn_AutoStart_MouseMove(object sender, MouseEventArgs e)
        {
            Btn_AutoStart.Appearance.BackColor = Color.LightSkyBlue;
        }

        private void Btn_AutoStart_MouseLeave(object sender, EventArgs e)
        {
            Btn_AutoStart.Appearance.BackColor = Color.LightSlateGray;
        }

        private void Btn_AutoStop_MouseMove(object sender, MouseEventArgs e)
        {
            Btn_AutoStop.Appearance.BackColor = Color.LightSkyBlue;
        }

        private void Btn_AutoStop_MouseLeave(object sender, EventArgs e)
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

                //Cogtg = (CogToolGroup)CogSerializer.LoadObjectFromFile(System.Windows.Forms.Application.StartupPath + "\\" + CurrentModelNum1 + "_1.vpp");
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
            }
            catch (Exception)
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

            //plc1.MasterK_Write_W("3" + tstr.Substring(0, 1) + "3" + tstr.Substring(1, 1) + "3" + tstr.Substring(2, 1) + "3" + tstr.Substring(3, 1) + "3" + tstr.Substring(4, 1), Txt_Data.Text); 2225
            plc1.MasterK_Write_W();
        }

        private void button7_Click(object sender, EventArgs e)  //  PLC 데이터 리셋
        {
            //plc1.MCWrite(int.Parse(Txt_Address.Text), 0);
            string tstr = Txt_Address.Text;

            //plc1.MasterK_Write_W("3" + tstr.Substring(0, 1) + "3" + tstr.Substring(1, 1) + "3" + tstr.Substring(2, 1) + "3" + tstr.Substring(3, 1), "0000");
            // plc1.MasterK_Write_W("3" + tstr.Substring(0, 1) + "3" + tstr.Substring(1, 1) + "3" + tstr.Substring(2, 1) + "3" + tstr.Substring(3, 1) + "3" + tstr.Substring(4, 1), "0000");

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
            dgvStatus1.Rows[0].Cells[0].Value = "Auto Run";

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
            dgvStatus1.Rows[0].Cells[0].Value = "Auto Stop";

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
            CamPoint1 = Convert.ToInt32(textBox1.Text);   //  검사포인트 변수에 넣음

            try
            {

                Bitmap cbmp = new Bitmap(pictureBox_Cam1.Image);    //  카메라 찍어서 받은 이미지 cbm 변수에 저장
                CogImage8Grey cimage = new CogImage8Grey(cbmp);     //  비전프로에 넣을이미지로 변환
                                                                    //CogImage24PlanarColor ccimage = new CogImage24PlanarColor(cbmp); //  비전프로에 넣을이미지로 변환  //  컬러일 경우

                CogIPOneImageTool ipt = (CogIPOneImageTool)Cogtg.Tools[0];  //  IPONEImage 변수

                ipt.InputImage = cimage;    //  IPONEImage에 이미지 넣기
                ipt.Run();                  //  IPONEImage에 이미지 돌리기

                CogToolBlock result = (CogToolBlock)Cogtg.Tools["result"];  //  Cogtg 중 데이터 가져올 툴 블락 result 변수로 미리 만들어둠

                for (int k = 0; k < result.Inputs.Count; k++)    //  데이터 0으로 초기화
                {
                    result.Inputs[k].Value = 0;
                }

                CogToolBlock input = (CogToolBlock)Cogtg.Tools["Tools"];    //  툴 블락 Tools 에 어느포인트 툴 사용할지 선택하기위해 툴블락 Tools 가져옴
                input.Inputs[1].Value = CamPoint1;                          //  툴 블락 Tools에 Input 밸류를 넣어서 어느툴 사용할지 선택함


                Cogtg.Run();    //  Cogtg 실행

                double[] resultall = new double[30];    //결과 data값 넣는 배열

                resultall[CamPoint1] = Convert.ToDouble(result.Inputs[CamPoint1 - 1].Value);    // PLC에서 받은 검사 포인트 번호를 resultall 에 넣음

                double lenVal = resultall[0];

                this.Invoke(new dele(() =>
                {
                    dgvD1.Rows[0].Cells[1].Value = resultall[CamPoint1].ToString("F2");     // 메인 모니터 상에 수치 출력

                }));


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



                if (min[0] <= lenVal && lenVal <= max[0])   //OK 판정
                {
                    this.Invoke(new dele(() =>
                    {
                        dgvD1.Rows[0].Cells[1].Style.BackColor = Color.LightGreen;
                        Label_Result1.Text = "O K";
                        Label_Result1.BackColor = Color.LightGreen;

                    }));
                    try
                    {
                        if (check_OKImage1.Checked)
                            pictureBox_Cam1.Image.Save(okpath + "\\" + ModelNamelbl1.Text + "_" + time + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);

                        Log_K.WriteLog(log_lst, Mainpath, "[Cam1 결과 : OK]" + Environment.NewLine);

                        Decision1 = "OK";
                        Delay(100);

                    }
                    catch (Exception)
                    {
                        Log_K.WriteLog(log_lst, Mainpath, "검사 후 전송 OK 에러");
                        Console.WriteLine("검사 후 전송 OK");
                    }
                    Delay(100);
                }


                else   //NG 판정
                {
                    this.Invoke(new dele(() =>
                    {
                        dgvD1.Rows[0].Cells[1].Style.BackColor = Color.Crimson;

                        Label_Result1.Text = "N G";
                        Label_Result1.BackColor = Color.Crimson;
                    }));
                    try
                    {
                        if (check_NGImage1.Checked)
                            pictureBox_Cam1.Image.Save(ngpath + "\\" + ModelNamelbl1.Text + "__" + time + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);

                        Log_K.WriteLog(log_lst, Mainpath, "[Cam1 결과 : NG]" + Environment.NewLine);

                        Decision1 = "NG";
                        Delay(100);

                    }
                    catch (Exception)
                    {
                        Log_K.WriteLog(log_lst, Mainpath, "검사 후 전송 NG 에러");
                    }
                    Delay(100);
                }


                string cmd = Ken2.Database.SQLiteCMD_K.MakeInsertCmdSentence(sql.table,   // CAM1 DB 업데이트
                        "Datetime", Dtime.Now(Dtime.StringType.ForDatum),
                        "CamNum", "CAM1",
                        "ModelNum", ModelNamelbl1.Text,
                        "PointNum", Convert.ToString(CamPoint1),
                        "Data", Convert.ToString(dgvD1.Rows[0].Cells[1].Value)
                         );

                sql.ExecuteNonQuery(cmd);


                //for (int k = 0; k < result.Inputs.Count; k++)    //  데이터 0으로 초기화
                //{
                //    result.Inputs[k].Value = 0;
                //}

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

            CamPoint2 = Convert.ToInt32(textBox2.Text);   //  검사포인트 변수에 넣음

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

                CogToolBlock input = (CogToolBlock)Cogtg.Tools["Tools"];    //  툴 블락 Tools 에 어느포인트 툴 사용할지 선택하기위해 툴블락 Tools 가져옴
                input.Inputs[1].Value = CamPoint2;                          //  툴 블락 Tools에 Input 밸류를 넣어서 어느툴 사용할지 선택함

                Cogtg2.Run();

                double[] resultall2 = new double[30]; //  전체결과 앞부터 3개씩 데이터 합치기


                resultall2[CamPoint2] = Convert.ToDouble(result.Inputs[CamPoint2 - 1].Value);    // PLC에서 받은 검사 포인트 번호를 resultall 에 넣음

                double lenVal2 = resultall2[0];

                this.Invoke(new dele(() =>
                {
                    dgvD2.Rows[0].Cells[1].Value = resultall2[CamPoint2].ToString("F2");     // 메인 모니터 상에 수치 출력

                }));


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


                if (min[0] <= lenVal2 && lenVal2 <= max[0])   //OK 판정
                {
                    this.Invoke(new dele(() =>
                    {
                        dgvD2.Rows[0].Cells[1].Style.BackColor = Color.LightGreen;
                        Label_Result2.Text = "O K";
                        Label_Result2.BackColor = Color.LightGreen;

                    }));
                    try
                    {
                        if (check_OKImage2.Checked)
                            pictureBox_Cam2.Image.Save(okpath2 + "\\" + ModelNamelbl1.Text + "_" + time + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);

                        Log_K.WriteLog(log_lst, Mainpath, "[Cam1 결과 : OK]" + Environment.NewLine);

                        Decision2 = "OK";
                        Delay(100);

                    }
                    catch (Exception)
                    {
                        Log_K.WriteLog(log_lst, Mainpath, "검사 후 전송 OK 에러");
                        Console.WriteLine("검사 후 전송 OK");
                    }
                    Delay(100);
                }


                else   //NG 판정
                {
                    this.Invoke(new dele(() =>
                    {
                        dgvD2.Rows[0].Cells[1].Style.BackColor = Color.Crimson;

                        Label_Result2.Text = "N G";
                        Label_Result2.BackColor = Color.Crimson;
                    }));
                    try
                    {
                        if (check_NGImage2.Checked)
                            pictureBox_Cam2.Image.Save(ngpath2 + "\\" + ModelNamelbl1.Text + "__" + time + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);

                        Log_K.WriteLog(log_lst, Mainpath, "[Cam1 결과 : NG]" + Environment.NewLine);

                        Decision2 = "NG";
                        Delay(100);

                    }
                    catch (Exception)
                    {
                        Log_K.WriteLog(log_lst, Mainpath, "검사 후 전송 NG 에러");
                    }
                    Delay(100);
                }

                string cmd = Ken2.Database.SQLiteCMD_K.MakeInsertCmdSentence(sql.table,   // CAM2 DB 업데이트
                       "Datetime", Dtime.Now(Dtime.StringType.ForDatum),
                       "CamNum", "CAM2",
                       "ModelNum", ModelNamelbl1.Text,
                       "PointNum", Convert.ToString(CamPoint2),
                       "Data", Convert.ToString(dgvD2.Rows[0].Cells[1].Value)
                        );

                sql.ExecuteNonQuery(cmd);

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

                //Cogtg = (CogToolGroup)CogSerializer.LoadObjectFromFile(System.Windows.Forms.Application.StartupPath + "\\" + modelnum + "_1.vpp");
                cogToolGroupEditV21.Subject = Cogtg;
                //MessageBox.Show(ModelNamelbl1.Text + " 툴을 불러왔습니다.");
                Console.WriteLine(" 툴을 불러왔습니다.");
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

                string cmd = SQLiteCMD_K.Select_Equal("table1", "Data", NameSearchTB.Text,

                        "Datetime",
                        "CamNum",
                        "ModelNum",
                        "PointNum",
                        "Data"
                        );

                sql.Select(dgvH0, cmd, false);
            }
            else
            {
                string cmd = SQLiteCMD_K.Select_Datetime("table1", "Datetime", Dtime.GetDateTime_string(Date0, Time0), Dtime.GetDateTime_string(Date1, Time1), "",

                         "Datetime",
                        "CamNum",
                        "ModelNum",
                        "PointNum",
                        "Data"
                        );

                sql.Select(dgvH0, cmd, false);

            }

            dgvInit("dgvH0");
        }

        private void simpleButton2_Click(object sender, EventArgs e)    //  recheck route Button / Visible = false
        {
            System.Windows.Forms.OpenFileDialog choofdlog = new System.Windows.Forms.OpenFileDialog();
            if (textBox4.Text == "" || textBox4.Text == null)
            {
                choofdlog.InitialDirectory = @"D:\" + Mainpath + @"\Image\";
            }
            else
            {
                string folder = textBox4.Text;
                choofdlog.InitialDirectory = @"" + folder + "\\";
            }

            if (choofdlog.ShowDialog() == DialogResult.OK)
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

            if (setTool == "CogBlobTool")
            {
                try
                {
                    new Tools.Line(Cogtg.Tools[OpenTool], 0).Show();
                }
                catch (Exception)
                {

                }
            }

            else if (setTool == "CogFindCircleTool")
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
            System.Windows.Forms.OpenFileDialog choofdlog = new System.Windows.Forms.OpenFileDialog();

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
            string cmdd = "DELETE FROM table1 WHERE `Data` = '" + bcrr + "';";

            sql.ExecuteNonQuery(cmdd);
        }


        private void button2_Click(object sender, EventArgs e)
        {
            plc1.MasterK_Write_W("32303130", "0100");
            plc1.MasterK_Write_W("32303131", "0100");
         
        }

        private void button5_Click(object sender, EventArgs e)
        {
            plc1.MasterK_Write_W("32303130", "0000");
            plc1.MasterK_Write_W("32303131", "0000");
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

        private void dgvH0_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {

        }
        //ccccccccccccccccc
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

    }
}
