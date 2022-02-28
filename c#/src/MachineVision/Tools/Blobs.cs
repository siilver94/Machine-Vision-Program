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
using Cognex.VisionPro.Blob;
using System.Diagnostics;

namespace VisionProgram.Tools
{
    public partial class Blob : Form
    {
        CogBlobTool BlobTooll;
        int lang = 0;

        public Blob(object BlobTool, int lang)
        {
            InitializeComponent();
            this.BlobTooll = (CogBlobTool)BlobTool;
            //cogBlobEditV21.Subject = (Cognex.VisionPro.c.CogBlobTool) BlobTool;

            cogBlobEditV21.Subject = BlobTooll;
            //changelang( lang );
            //this.lang = lang;
        }

        private void Blob_FormClosing(object sender, FormClosingEventArgs e)
        {
            cogBlobEditV21.Subject = null;
            //Application.OpenForms["Blob"].Close();
        }

        //private void Blob_FormClosing(object sender, FormClosingEventArgs e)
        //{
        //    cogBlobEditV21.Subject = null;
        //    System.Environment.Exit(1);
        //    //Application.Exit();
        //}

        //private void Blob_FormClosing_1(object sender, FormClosingEventArgs e)
        //{

        //}
    }
}
