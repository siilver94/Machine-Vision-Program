using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Cognex.VisionPro.Caliper;

namespace Vision_Seojin.Tools
{
    public partial class Circle : Form
    {
        CogFindCircleTool CircleTolll;
        int lang = 0;
        
        public Circle(object CircleTool, int lang)
        {
            InitializeComponent();
            this.CircleTolll = (CogFindCircleTool)CircleTool;
            cogFindCircleEditV21.Subject = CircleTolll;
        }

        private void Circle_FormClosing(object sender, FormClosingEventArgs e)
        {
            cogFindCircleEditV21.Subject = null;
        }
    }
}
