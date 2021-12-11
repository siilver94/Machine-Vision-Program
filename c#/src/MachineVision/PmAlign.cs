using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.NetworkInformation;

using Basler.Pylon;

using Cognex.VisionPro;
using Cognex.VisionPro.ImageProcessing;
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro.ToolGroup;

using Ken2.Database;
using Ken2.Util;
using Ken2.DataManagement;
using System.Threading;
using System.Diagnostics;

namespace VisionProgram
{
    public partial class Form1 : Form
    {
        private delegate void dele();
        bool LiveFlag = false;
        PylonBasler cam1 = null;

        TcpClient tc;

        string msg = "";

        NetworkStream stream;

        Ken2.UIControl.dgvManager dgvmanager;

        Mysql_K sql;

        CogToolGroup Cogtg;
        Image image;


        //ffffffffffffff
        private void Form1_Load(object sender, EventArgs e)
        {
            this.Location = new Point(0, 0);

            Delay(300);

            cam1 = new PylonBasler("192.168.100.2");
            cam1.ImageSignal += Cam1_ImageSignal;

            


            dgvInit("dataGridView1");

            sql = new Mysql_K("127.0.0.1", "atnz", "testtable", "a", "qwerasdf");  //  ip , 데이터베이스 이름, 테이블이름, id, 비번
            Delay(1000);

            cam1.IOShot();
        }

        public Form1()
        {
            InitializeComponent();
        }

        private static DateTime Delay(int MS)    //delay 설정
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
       
        
        private void Cam1_ImageSignal(PylonBasler.CurrentStatus Command, object Data, int ArrayNum)
        {
            if (Command == PylonBasler.CurrentStatus.OneShot)     //  검사
            {
                pictureBox_Cam1.Image = (Bitmap)Data;
                pictureBox_Cam3.Image = (Bitmap)Data;
               //image = (Bitmap)Data;
                //triger1(image);

            }

            if (Command == PylonBasler.CurrentStatus.TestShot1)   //  테스트샷
            {
                pictureBox_Cam1.Image = (Bitmap)Data;
            }

            

            if (Command == PylonBasler.CurrentStatus.LiveShot)    //  라이브
            {
                pictureBox_Cam2.Image = (Bitmap)Data;
            }

            if (Command == PylonBasler.CurrentStatus.IOShot)      //  IO 트리거
            {
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                this.Invoke(new dele(() =>
                {
                    
                    //textBox5.AppendText("io 트리거 In -> " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff") + Environment.NewLine + Environment.NewLine);
                    pictureBox_Cam1.Image = (Bitmap)Data;
                    pictureBox_Cam1.Image.Save(@"D:\SaveFloder\image\ok\" + "\\" + DateTime.Now.ToString("MM/dd/yyyy hh.mm.ss.fff") + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                    sw.Stop();

                    richTextBox1.AppendText("이미지 저장 time -> " + sw.ElapsedMilliseconds.ToString() + " m/s" + Environment.NewLine + Environment.NewLine);

                    //C: \Users\Hyeon\Desktop\new
                }));

               // image = (Bitmap)Data;

                //triger1(image);
            }
        }
        private void triger1(Image image)     //트리거
        {
            Bitmap cbmp = new Bitmap(image);

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

            double[] rr = new double[100];
            double value = 0;
           
            for (int i = 0; i < result.Inputs.Count; i++)
            {
                rr[i] = Convert.ToDouble(result.Inputs[i].Value);            
            }
            value = rr[2] * (180 / Math.PI);
            this.Invoke(new dele(() =>
            {
                //Dt1.Text = rr[0].ToString();
                //Dt2.Text = rr[1].ToString();               
                //Dt3.Text = value.ToString();
                //Dt4.Text = rr[3].ToString();

                Dt1.Text = Math.Round(rr[0], 4).ToString();  //X
                Dt2.Text = Math.Round(rr[1], 4).ToString();  //Y
                Dt3.Text = Math.Round(value, 4).ToString();  //Angle
                Dt4.Text = Math.Round(rr[3]*100, 4).ToString();  //Result




                //textBox9
                if (Convert.ToDouble(textBox9.Text) <= rr[3] * 100 && Convert.ToDouble(textBox10.Text) >= rr[3] * 100)
                {
                    res.BackColor = Color.Lime;
                    Label_Result1.Text = "O K";
                    Label_Result1.BackColor = Color.LightGreen;

                    string cmd = Ken2.Database.SQLiteCMD_K.MakeInsertCmdSentence(sql.table,   //  무조건 DB에 올림 A로

                                   "Model", textBox4.Text,
                                   "Datetime", Dtime.Now(Dtime.StringType.ForDatum),
                                   "X", Dt1.Text,
                                   "Y", Dt2.Text,
                                   "Angle", Dt3.Text,
                                   "Score", Dt4.Text,
                                   "Result", "1"
                                       );
                    sql.ExecuteNonQuery(cmd);

                    if (check_OKImage1.Checked)
                    {
                        string time = DateTime.Now.ToString("HH.mm.ss");
                        pictureBox_Cam1.Image.Save(@"D:\SaveFloder\image\ok\" + "\\" + time + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                    }


                }
                else
                {
                    res.BackColor = Color.Crimson;
                    Label_Result1.Text = "N G";
                    Label_Result1.BackColor = Color.Crimson;

                    string cmd = Ken2.Database.SQLiteCMD_K.MakeInsertCmdSentence(sql.table,   //  무조건 DB에 올림 A로

                                   "Model", textBox4.Text,
                                   "Datetime", Dtime.Now(Dtime.StringType.ForDatum),
                                   "X", Dt1.Text,
                                   "Y", Dt2.Text,
                                   "Angle", Dt3.Text,
                                   "Score", Dt4.Text,
                                   "Result", "2"
                                       );
                    sql.ExecuteNonQuery(cmd);
                    if (check_NGImage1.Checked)
                    {
                        string time = DateTime.Now.ToString("HH.mm.ss");
                        pictureBox_Cam1.Image.Save(@"D:\SaveFloder\image\ng\" + "\\" + time + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                    }
                }
            }));
        }
        private void Cam1_CommSignal(bool Connected, int ArrayNum)
        {
            if (Connected)//연결 되면 밝기 적용하기.
            {
                cam1.SetExp(Convert.ToInt32(BrightValue));
               // cam1.SetExp(5000);   //  밝기 저장              
                Console.WriteLine("카메라1연결");
            }
            else
            {                
                Console.WriteLine("카메라1해제");
            }
        }
        public void dgvInit(string name)
        {
            switch (name)
            {
                case "dataGridView1":
                    try
                    {
                        //---------------↓ 기본 ↓---------------┐
                        DataGridView dgv = (DataGridView)Reflection_K.Get(this, name);          //이름가져옴
                        string DGV_name = dgv.Name;//적용
                        int height = int.Parse(DataRW.Load_Simple(DGV_name + "H", "30"));       //데이터가져옴
                        int fontheader = int.Parse(DataRW.Load_Simple(DGV_name + "FH", "12"));  //데이터가져옴
                        int fontcell = int.Parse(DataRW.Load_Simple(DGV_name + "FC", "12"));    //데이터가져옴
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
                        
                        dgv.Columns[1].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";//표시형식
                        
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("dgvH0");
                    }
                    break;
            }
        }

       
        
        private void simpleButton1_Click(object sender, EventArgs e)   //카메라 촬영
        {
            this.Invoke(new dele(() =>
            {
            }));
            try
            {
                cam1.OneShot();
                MessageBox.Show("촬영 성공");
            }
            catch (Exception)
            {               
                MessageBox.Show("촬영 실패");
            }
        }
        //ttt
        private void simpleButton2_Click(object sender, EventArgs e)   //카메라 라이브
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
        public void autoDelete()   //  폴더 자동 삭제
        {
            try
            {
                int deleteDay = Int32.Parse(Txt_DeleteDay.Text);  //  보관할 날짜
                DirectoryInfo di = new DirectoryInfo(@"D:\SaveFloder\image\");
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
            }catch (Exception)
            {
                Console.WriteLine("자동삭제");
            }    
        }
       
       

        private void simpleButton3_Click(object sender, EventArgs e)    //밝기 저장
        {
            try
            {
                cam1.SetExp(Convert.ToInt32(BrightValue.Text));
                cam1.TestShot(1);
                Console.WriteLine("밝기가 저장되었습니다.");
            }
            catch (Exception)
            {
                Console.WriteLine("카메라밝기 저장 에러");
            }
          
        }

        private void simpleButton4_Click(object sender, EventArgs e)      //저장 폴더 열기
        {
            
            Directory.CreateDirectory(@"D:\SaveFloder\image\");
            System.Diagnostics.Process.Start("explorer.exe", @"D:\SaveFloder\image\");
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        TCPClient_K client;
        
        void ClientStart()
        {
            int port = Convert.ToInt32(textBox2.Text);

            client = new TCPClient_K(textBox1.Text, Convert.ToInt32(textBox2.Text), 1000, "192.168.100.5", 0);
            client.TalkingComm += client_TalkingComm;
            client.ConnectStart(0);

        }
        //nnnnnnnn
        private void simpleButton5_Click(object sender, EventArgs e)     //접속
        {
            ClientStart();
            tc = new TcpClient(textBox1.Text, Convert.ToInt32(textBox2.Text));
            MessageBox.Show("접속되었습니다.");
            label4.BackColor = Color.Lime;
         
        }

        
        private void simpleButton6_Click(object sender, EventArgs e)    //접속 끊기
        {
            tc.Dispose();
            MessageBox.Show("접속이 종료 되었습니다.");
            label4.BackColor = Color.Crimson;            
        }
       

            private void simpleButton7_Click(object sender, EventArgs e)     // 데이터 전송
        {
            msg = textBox3.Text;

            byte[] buff = Encoding.ASCII.GetBytes(msg);

            // (2) NetworkStream을 얻어옴 
            stream = tc.GetStream();
            stream.Write(buff, 0, buff.Length);

            richTextBox1.AppendText("[송신] Time :  " + Dtime.Now(Dtime.StringType.CurrentTime) + " : " + msg + Environment.NewLine);

            richTextBox1.ScrollToCaret();
        }


        void client_TalkingComm(string name, object data, int length)   //데이터 받기
        {

            this.Invoke(new dele(() =>
            {
                
                byte[] bt = (byte[])data; 
                richTextBox1.Text += "[수신] Time : " + Dtime.Now(Dtime.StringType.CurrentTime) + " : "  + Encoding.ASCII.GetString(bt, 0, length) + Environment.NewLine + Environment.NewLine;

                if (Encoding.ASCII.GetString(bt, 0, length) == textBox11.Text)

                {
                    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                    sw.Start();
                    this.Invoke(new dele(() =>
                    {
                    }));
                    try
                    {
                        cam1.OneShot();
                        string time = DateTime.Now.ToString("HH.mm.ss");
                        pictureBox_Cam1.Image.Save(@"D:\SaveFloder\image\ok\" + "\\" + time + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                        sw.Stop();
                        richTextBox1.AppendText("이미지 저장 time -> " + sw.ElapsedMilliseconds.ToString() + " m/s" + Environment.NewLine + Environment.NewLine);
                        //textBox5.AppendText("이미지 저장 time -> " + sw.ElapsedMilliseconds.ToString() + " m/s" + Environment.NewLine + Environment.NewLine);
                      

                        //MessageBox.Show("촬영 성공");
                    }
                    catch (Exception)
                    {
                       // MessageBox.Show("촬영 실패");
                    }
                }   

            }));



        }


        //ddddddd

        private void SetToday()
        {
            Date0.Value = DateTime.Now;

            Date1.Value = DateTime.Now;

            Time0.Time = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);

            Time1.Time = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);
        }


        private void SelectHistory()   //데이터 베이스 검색
        {
            dataGridView1.Columns.Clear();

            if (NameSearchcheck.Checked)
            {

                string cmd = SQLiteCMD_K.Select_Equal("testtable", "Model", NameSearchTB.Text,
                    "Datetime",
                    "Model",
                    "X",
                    "Y",
                    "Angle",
                    "Score",
                    "Result"

                        );

                sql.Select(dataGridView1, cmd, false);
                //dgvInit("dgvH0");
            }
            else
            {
                string cmd = SQLiteCMD_K.Select_Datetime("testtable", "Datetime", Dtime.GetDateTime_string(Date0, Time0), Dtime.GetDateTime_string(Date1, Time1), "",


                    "Datetime",
                    "Model",
                    "X",
                    "Y",
                    "Angle",
                    "Score",
                    "Result"
                    );


                sql.Select(dataGridView1, cmd, false);

                dgvInit("dataGridView1");
            }
        }
        private void simpleButton11_Click(object sender, EventArgs e)    //데이터베이스 검색 실행
        {
            SelectHistory();
                    }

        private void simpleButton8_Click(object sender, EventArgs e)   //데이터베이스 추가
        {
            string cmd = Ken2.Database.SQLiteCMD_K.MakeInsertCmdSentence(sql.table,   //  무조건 DB에 올림 A로

                                    "Model", textBox4.Text,
                                    "Datetime", Dtime.Now(Dtime.StringType.ForDatum),
                                    "X",null,
                                    "Y",null,
                                    "Angle",null,
                                   "Score",null,
                                     "Result",null
                                        );
            sql.ExecuteNonQuery(cmd);
            MessageBox.Show("\"" + textBox4.Text + "\"" + " 모델이 추가 되었습니다.");
            
        }

        private void simpleButton9_Click(object sender, EventArgs e)    //데이터베이스 수정
        {
            string cmd = SQLCMD.MakeUpdateCmdSentence_where_equals(sql.table, "Model", textBox8.Text, "",

                          "Model", textBox5.Text
                          );
            sql.ExecuteNonQuery(cmd);
            
            MessageBox.Show("\"" + textBox8.Text + "\"" + " 모델이" + "\"" + textBox5.Text + "\"" + "로 변경 추가 되었습니다.");
        }

        private void simpleButton10_Click(object sender, EventArgs e)   //데이터베이스 삭제
        {
            //DELETE FROM table1 WHERE `barcode` = 'test';
            string cmd = "DELETE FROM testtable WHERE `Model` = '" + textBox7.Text + "';";                                   
            sql.ExecuteNonQuery(cmd);
            MessageBox.Show("\"" + textBox4.Text + "\"" + " 모델이 삭제 되었습니다.");
        }
        void OnInit(string name, object data)
        {
            this.Invoke(new dele(() =>
            {
                dgvInit(name);
            }));
        }

        private void dataGridView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button.ToString().Equals("Right"))
            {
                DataGridView thisdgv = (DataGridView)sender;
                dgvmanager = new Ken2.UIControl.dgvManager(thisdgv);
                dgvmanager.Init += OnInit;
                dgvmanager.Show();
            }
        }

        private void simpleButton12_Click(object sender, EventArgs e)  // 툴그룹 subject 현재 툴 넣기
        {
            cogToolGroupEditV21.Subject = Cogtg;
        }

        private void simpleButton13_Click(object sender, EventArgs e)    //툴 그룹 subject 비우기
        {
            cogToolGroupEditV21.Subject = null;
        }

        private void simpleButton14_Click(object sender, EventArgs e)     //툴 저장
        {
            if (cogToolGroupEditV21.Subject != null)
                Cogtg = cogToolGroupEditV21.Subject;
            CogSerializer.SaveObjectToFile(Cogtg, @"D:\SaveFloder\job\" +  textBox6.Text + ".vpp", typeof(System.Runtime.Serialization.Formatters.Binary.BinaryFormatter), CogSerializationOptionsConstants.Minimum);
            MessageBox.Show(textBox6.Text + "툴이 저장되었습니다.");
        }

        private void simpleButton15_Click(object sender, EventArgs e)    //툴 불러오기
        {
            
            Cogtg = (CogToolGroup)CogSerializer.LoadObjectFromFile(@"D:\SaveFloder\job\" + textBox6.Text + ".vpp");
            cogToolGroupEditV21.Subject = Cogtg;
            MessageBox.Show(textBox6.Text + " 툴을 불러왔습니다.");
        }

        private void simpleButton17_Click(object sender, EventArgs e)
        {
            this.Invoke(new dele(() =>
            {
            }));
            try
            {
                cam1.OneShot();
                MessageBox.Show("촬영 성공");
            }
            catch (Exception)
            {

                MessageBox.Show("촬영 실패");
            }
        }

        private void simpleButton16_Click(object sender, EventArgs e)    //패턴 얼라인
        {
            new PmAlign(Cogtg.Tools["CogPMAlignTool1"]).Show();
        }
    
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {     
        }

        private void simpleButton18_Click(object sender, EventArgs e)   // Today 설정
        {
            SetToday();
        }

        private void simpleButton19_Click(object sender, EventArgs e)     //csv파일로 저장
        {
            Directory.CreateDirectory(@"D:\SaveFloder\csv\");
            GridMaster.SaveCSV(dataGridView1, @"D:\SaveFloder\csv\" + Dtime.Now(Dtime.StringType.ForFile) + ".csv");

            MessageBox.Show("Save Data.\nLocation : " + @"D:\SaveFloder\csv\"+ "Message");
        }

        private void simpleButton20_Click(object sender, EventArgs e)   //CSV 폴더 열기
        {
            Directory.CreateDirectory(@"D:\SaveFloder\csv\");
            System.Diagnostics.Process.Start("explorer.exe", @"D:\SaveFloder\csv\");
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

        private void simpleButton21_Click(object sender, EventArgs e)   // 테이블 삭제
        {
            DBReset();
        }
        void DBReset()
        {
            string cmd = "DELETE FROM testtable";
            sql.ExecuteNonQuery(cmd);
        }

        private void simpleButton22_Click(object sender, EventArgs e)
        {
            string bcrr = NameSearchTB.Text;
            string cmdd = "DELETE FROM testtable WHERE `Model` = '" + bcrr + "';";

            sql.ExecuteNonQuery(cmdd);
        }

        private void simpleButton23_Click(object sender, EventArgs e)   //Job 폴더 열기
        {
            Directory.CreateDirectory(@"D:\SaveFloder\job\");
            System.Diagnostics.Process.Start("explorer.exe", @"D:\SaveFloder\job\");
        }
        // cccccccc
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (tc != null)
                    tc.Dispose();
                if (cam1 != null)
                    cam1.Dispose();

                Thread.Sleep(3000);

                try
                {
                    Process.GetCurrentProcess().Kill();
                }
                catch (Exception)
                {
                    //Log_K.WriteLog(log_err, Mainpath, "Form Closing 에러");
                    //Console.WriteLine("Form Closing");
                }
            }
            catch (Exception)
            {
                //Log_K.WriteLog(log_err, Mainpath, "Form Closing 에러2");
                //Console.WriteLine("Form Closing");
            }
        }
    }
}
