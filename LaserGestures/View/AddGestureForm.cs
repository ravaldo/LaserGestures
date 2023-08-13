using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace LaserGestures {
    public partial class AddGestureForm : Form {
        Gesture gesture;

        public AddGestureForm() {
            InitializeComponent();

            okButton.Enabled = false;

            comboBox1.Items.Add(DynamicAction.SEEK.ToString());
            comboBox1.Items.Add(DynamicAction.TURN.ToString());
            comboBox1.SelectedIndex = 0;
            setComboBox();
        }

        private void textBox1_TextChanged(object sender, EventArgs e) {
            Match match = Regex.Match(textBox1.Text, "^[UDRL1379]+(-[UDRL1379]+)*$");
            okButton.Enabled = match.Success ? true : false;
        }

        private void setComboBox() {
            if (staticRadioButton.Checked)
                comboBox1.Enabled = false;
            else if (dynamicRadioButton.Checked)
                comboBox1.Enabled = true;
        }

        private void staticRadioButton_CheckedChanged(object sender, EventArgs e) {
            setComboBox();
        }

        private void okButton_Click(object sender, EventArgs e) {
            if (staticRadioButton.Checked)
                gesture = new Gesture(textBox1.Text);
            else if (dynamicRadioButton.Checked) {
                if (comboBox1.SelectedIndex == 0)
                    gesture = new Gesture(textBox1.Text, DynamicAction.SEEK);
                if (comboBox1.SelectedIndex == 1)
                    gesture = new Gesture(textBox1.Text, DynamicAction.TURN);
            }
        }

        public Gesture GetGesture {
            get { return gesture; }
        }

    }

}
