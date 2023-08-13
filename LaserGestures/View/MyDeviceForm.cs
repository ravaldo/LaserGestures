using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;

namespace LaserGestures {
    public partial class MyDeviceForm : Form {

        FilterInfoCollection videoDevices;
        private string device;

        public MyDeviceForm() {
            InitializeComponent();

            try {
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                if (videoDevices.Count == 0)
                    throw new ApplicationException();

                foreach (FilterInfo filter in videoDevices) {
                    comboBox1.Items.Add(filter.Name);
                }
            } catch (ApplicationException) {
                comboBox1.Items.Add("No local capture devices");
                comboBox1.Enabled = false;
                okButton.Enabled = false;
            }
            comboBox1.SelectedIndex = 0;
        }

        public String VideoDeviceMoniker {
            get { return device; }
        }

        private void okButton_Click(object sender, EventArgs e) {
            device = videoDevices[comboBox1.SelectedIndex].MonikerString;
        }

    }
}