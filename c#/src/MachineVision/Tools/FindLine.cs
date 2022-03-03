using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Cognex.VisionPro.Caliper;


namespace VisionProgram.Tools
{
    public partial class Line : Form
    {

        CogFindLineTool LineTooll;

        public Line(object FindLineTool, int lang)
        {
            InitializeComponent();
            this.LineTooll = (CogFindLineTool)FindLineTool;
            //cogBlobEditV21.Subject = (Cognex.VisionPro.c.CogBlobTool) BlobTool;



            cogFindLineEditV21.Subject = LineTooll;
           // CogFindLineEditV21.Subject = LineTooll;
            //changelang( lang );
            //this.lang = lang;
        }
        private void FindLine_FormClosing(object sender, FormClosingEventArgs e)
        {
            cogFindLineEditV21.Subject = null;
        }
    }


}
