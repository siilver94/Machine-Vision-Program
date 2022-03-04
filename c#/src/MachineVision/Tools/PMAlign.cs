using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Cognex.VisionPro;
using Cognex.VisionPro.Display;
using Cognex.VisionPro.PMAlign;

namespace VisionProgram.Tools
{
    public partial class PMAlign : Form
    {
        //public PMAlign()
        //{
        //    InitializeComponent();
        //}
        CogPMAlignTool PM;
        int lang = 0;

        public PMAlign(object PM, int lang)
        {
            InitializeComponent();
            this.PM = (CogPMAlignTool)PM;
           
            changelang(lang);
            this.lang = lang;
        }

        void changelang(int lang)
        {
            if (lang == 0)
            {

            }
            else
            {
                //등록된 이미지 登陆过的页面

                GRcon0.Text = "最近页面";//최근 이미지
                groupControl1.Text = "登陆过的页面";
                groupControl2.Text = "结果页面";
                groupControl3.Text = "运行/点数";

                groupControl5.Text = "情报信息";
                groupControl4.Text = "设置";

                simpleButton2.Text = "最近页面复制";
                simpleButton3.Text = "解除";
                simpleButton4.Text = "登陆";

                button1.Text = "保存";
                button2.Text = "页面复制";

                simpleButton1.Text = "运行";

                //xtraTabPage1.Text = "主页画面";
                //xtraTabPage2.Text = "覆盖设置";
            }
        }

        private void PMAlign_Load(object sender, EventArgs e)
        {
            UpdateScreen();

            cogRecordDisplay0.Fit(true);
            cogRecordDisplay1.Fit(true);
            cogRecordDisplay2.Fit(true);
            this.Location = new Point(0, 0);
        }

        void TrainedInfo()
        {
            labelControl1.Appearance.BackColor = Color.Lime;

            if (lang == 0)
            {
                labelControl1.Text = "Trained";
            }
            else
            {
                labelControl1.Text = "登陆";
            }

        }
        void UnTrainedInfo()
        {
            labelControl1.Appearance.BackColor = Color.Red;

            if (lang == 0)
            {
                labelControl1.Text = "Not Trained";
            }
            else
            {
                labelControl1.Text = "解除";

            }

        }

        //#ff4 keyy
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            CogDisplay ds = (CogDisplay)Reflection_K.Get(cogImageMaskEditV21, "cogDisplay1");

            switch (keyData)
            {
                case Keys.F1:
                    cogRecordDisplay0.MouseMode = Cognex.VisionPro.Display.CogDisplayMouseModeConstants.Pointer;
                    cogRecordDisplay1.MouseMode = Cognex.VisionPro.Display.CogDisplayMouseModeConstants.Pointer;
                    cogRecordDisplay2.MouseMode = Cognex.VisionPro.Display.CogDisplayMouseModeConstants.Pointer;

                    ds.MouseMode = Cognex.VisionPro.Display.CogDisplayMouseModeConstants.Pointer;

                    break;

                case Keys.F2:
                    cogRecordDisplay0.MouseMode = Cognex.VisionPro.Display.CogDisplayMouseModeConstants.Pan;
                    cogRecordDisplay1.MouseMode = Cognex.VisionPro.Display.CogDisplayMouseModeConstants.Pan;
                    cogRecordDisplay2.MouseMode = Cognex.VisionPro.Display.CogDisplayMouseModeConstants.Pan;

                    ds.MouseMode = Cognex.VisionPro.Display.CogDisplayMouseModeConstants.Pan;

                    break;

                case Keys.F3:
                    cogRecordDisplay0.Fit(true);
                    cogRecordDisplay1.Fit(true);
                    cogRecordDisplay2.Fit(true);

                    ds.Fit(true);

                    break;

                case Keys.F4:
                    Run();

                    break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        void UpdateScreen()
        {

            cogRecordDisplay0.Record = PM.CreateCurrentRecord().SubRecords[0];
            cogRecordDisplay1.Record = PM.CreateCurrentRecord().SubRecords[1];

            if (PM.Pattern.Trained)
            {
                TrainedInfo();
                cogRecordDisplay2.Record = PM.CreateLastRunRecord().SubRecords[0];

            }
            else
            {
                UnTrainedInfo();
                cogRecordDisplay2.Record = null;

            }
        }

        void Run()
        {
            try
            {
                PM.Run();
                kenLabel1.Text =
              ((int)(Math.Round(PM.Results[0].Score, 2) * 100)).ToString();

            }
            catch (Exception ee)
            {
                kenLabel1.Text = "0";
                //Console.WriteLine( ee);
                //MessageBox.Show( "트레인 되어 있지 않습니다." );
            }

            UpdateScreen();
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            Run();
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            PM.Pattern.TrainImage = PM.InputImage.CopyBase(CogImageCopyModeConstants.CopyPixels);

            UpdateScreen();
        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            PM.Pattern.Untrain();

            UpdateScreen();
        }

        private void simpleButton4_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                CogRectangle rollypolly = (CogRectangle)PM.Pattern.TrainRegion;
                PM.Pattern.Origin.TranslationX = rollypolly.CenterX;
                PM.Pattern.Origin.TranslationY = rollypolly.CenterY;
            }
            else if (radioButton2.Checked)
            {
                CogRectangleAffine roll1 = (CogRectangleAffine)PM.Pattern.TrainRegion;

                PM.Pattern.Origin.TranslationX = roll1.CenterX;
                PM.Pattern.Origin.TranslationY = roll1.CenterY;
            }
            else if (radioButton3.Checked)
            {
                CogCircle roll2 = (CogCircle)PM.Pattern.TrainRegion;

                PM.Pattern.Origin.TranslationX = roll2.CenterX;
                PM.Pattern.Origin.TranslationY = roll2.CenterY;
            }
            else if (radioButton4.Checked)
            {
                CogCircularAnnulusSection roll3 = (CogCircularAnnulusSection)PM.Pattern.TrainRegion;

                PM.Pattern.Origin.TranslationX = roll3.CenterX;
                PM.Pattern.Origin.TranslationY = roll3.CenterY;
            }
            try
            {
                PM.Pattern.Train();
            }
            catch (Exception)
            {

            }

            Run();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                cogImageMaskEditV21.Image = PM.Pattern.TrainImage.CopyBase(CogImageCopyModeConstants.CopyPixels);
            }
            catch (Exception)
            {

            }

            try
            {
                cogImageMaskEditV21.MaskImage = PM.Pattern.TrainImageMask.Copy();
            }
            catch (Exception)
            {

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PM.Pattern.TrainImage = (CogImage8Grey)cogImageMaskEditV21.Image.CopyBase(CogImageCopyModeConstants.CopyPixels);
            PM.Pattern.TrainImageMask = (CogImage8Grey)cogImageMaskEditV21.MaskImage.CopyBase(CogImageCopyModeConstants.CopyPixels);

            cogImageMaskEditV21.Image = new CogImage8Grey();
            cogImageMaskEditV21.MaskImage = new CogImage8Grey();

            UpdateScreen();
        }
    }

    public class Reflection_K
    {
        public static object Get(object Destination, string FieldName)
        {
            System.Reflection.FieldInfo FI = Destination.GetType().GetField(FieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            return FI.GetValue(Destination);
        }
    }
}
