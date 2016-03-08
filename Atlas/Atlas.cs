//Project:      Atlas
//File:         Atlas.cs
//Description:  This file connects the forms application with the Atlas Classes.
//Programmers:  Jordan Poirier, Thom Taylor, Matthew Thiessen, Tylor McLaughlin
//Date:         5/1/2016

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AtlasClasses;



namespace Atlas
{
    public partial class Atlas : Form
    {
        Detection d;
        public Atlas()
        {
            InitializeComponent();
            d = new Detection();
        }

        private void Atlas_FormClosing(object sender, FormClosingEventArgs e)
        {
            d.stopKinect(d._sensor);
        }

        private void Atlas_Load(object sender, EventArgs e)
        {
            d.setUpSensors();
            this.Refresh();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            //d.sensorAngle(trackBar1.Value);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            d.sensorAngle(trackBar1.Value);
        }

        private void Atlas_Paint(object sender, PaintEventArgs e)
        {
            depthView.Image = d.Difference;
            this.Refresh();
        }
    }
}
