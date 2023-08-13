using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Diagnostics;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Threading;
using System.Threading.Tasks;


namespace LaserGestures {
    public partial class MainWindow : Form {

        private InputPanel inputPanel;
        private OutputPanel outputPanel;
        private Analyser analyser;
        private ISource source;
        private Task analyserTask;
        private Recorder recorder;
        private Recogniser recogniser;
        private GestureSet gestureSet;
        

        private Size size = new Size(320, 240);
        private const string filterString = "avi files (*.avi)|*.avi|mp4 files (*.mp4)|*.mp4|All files (*.*)|*.*";


        public MainWindow() {
            InitializeComponent();

            try {
                gestureSet = GestureSet.Open("default.lg");
            } catch (FileNotFoundException) {
                gestureSet = new GestureSet();
            }
            
        }


        private void selectVideoSource() {
            OpenFileDialog fd = new OpenFileDialog();
            fd.InitialDirectory = "c:\\Documents and Settings\\Administrator\\Desktop";
            fd.Filter = filterString;

            if (fd.ShowDialog() == DialogResult.OK) {
                cleanup();
                source = new Video(fd.FileName);
                Size size = source.FrameSize;
                source.Start();

                if (source.IsRunning) {
                    inputPanel = new InputPanel(source);
                    inputPanel.Location = new Point(12, 36);
                    this.Controls.Add(inputPanel);
                    webcamToolStripMenuItem.Enabled = false;

                    outputPanel = new OutputPanel();
                    outputPanel.Location = new System.Drawing.Point(12 * 2 + size.Width, 36);
                    outputPanel.Size = size;
                    this.Controls.Add(outputPanel);

                    analyser = new Analyser(source, 240, outputPanel, gestureSet);
                    recogniser = new Recogniser(analyser, outputPanel);
                    analyserTask = new Task(() => analyser.Run());
                    analyserTask.Start();

                    Size = new Size(15 * 3 + size.Width * 2, 55 * 2 + size.Height);
                }
            }
        }

        private void selectDevice() {
            MyDeviceForm deviceForm = new MyDeviceForm();
            if (deviceForm.ShowDialog() == DialogResult.OK) {
                cleanup();
                source = new Webcam(deviceForm.VideoDeviceMoniker, size);
                source.Start();

                if (source.IsRunning) {
                    inputPanel = new InputPanel(source);
                    inputPanel.Location = new System.Drawing.Point(12, 36);
                    this.Controls.Add(inputPanel);
                    webcamToolStripMenuItem.Enabled = true;

                    outputPanel = new OutputPanel();
                    outputPanel.Location = new System.Drawing.Point(12 * 2 + size.Width, 36);
                    outputPanel.Size = size;
                    this.Controls.Add(outputPanel);

                    analyser = new Analyser(source, 240, outputPanel, gestureSet);
                    recogniser = new Recogniser(analyser, outputPanel);
                    analyserTask = new Task(() => analyser.Run());
                    analyserTask.Start();

                    Size = new Size(15 * 3 + size.Width * 2, 55 * 2 + size.Height);
                } else {
                    MessageBox.Show("Camera doesn't seem to be working. Try unplugging and reconnecting", "Hmmm....");
                }
            }
        }


        private void cleanup() {
            if (recogniser != null)
                recogniser.Dispose(true);
            if (analyser != null)
                analyser.Dispose(true);
            if (inputPanel != null)
                inputPanel.Dispose();
            if (recorder != null)
                recorder.Dispose(true);
            if (source != null)
                source.Dispose(true);
        }


        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e) {
            cleanup();
            Application.Exit();
        }


       private void startRecordingToolStripMenuItem_Click(object sender, EventArgs e) {
            saveFileDialog1.Filter = filterString;
            if (saveFileDialog1.ShowDialog(this) == DialogResult.OK) {
                string file = saveFileDialog1.FileName;
                recorder = new Recorder(source, file);
            }
        }

        private void stopRecordingToolStripMenuItem1_Click(object sender, EventArgs e) {
            if (recorder != null && recorder.IsRunning)
                recorder.Dispose(true);
        }

        private void calibrateToolStripMenuItem_Click(object sender, EventArgs e) {
            recogniser.Dispose(true);
            analyser.Dispose(true);

            CalibrationForm calibrationForm = new CalibrationForm(source);
            new Task(calibrationForm.Run).Start();

            if (calibrationForm.ShowDialog(this) == DialogResult.OK) {
                var keystoneCorners = calibrationForm.Corners;
                var threshold = calibrationForm.Threshold;
                calibrationForm.Dispose();
                analyser = new Analyser(source, threshold, outputPanel, gestureSet, keystoneCorners);
                recogniser = new Recogniser(analyser, outputPanel);
                inputPanel.Corners = keystoneCorners;
            } else {
                analyser = new Analyser(source, 240, outputPanel, gestureSet);
                recogniser = new Recogniser(analyser, outputPanel);
            }

            analyserTask = new Task(analyser.Run);
            analyserTask.Start();
        }

        private void selectDeviceToolStripMenuItem_Click(object sender, EventArgs e) {
            selectDevice();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            cleanup();
            Application.Exit();
        }

        private void openRecordingToolStripMenuItem_Click(object sender, EventArgs e) {
            selectVideoSource();
        }

        private void MainWindow_Resize(object sender, EventArgs e) {
            if (this.WindowState == FormWindowState.Minimized)
                this.ShowInTaskbar = false;
        }

        private void trayIcon_MouseDoubleClick(object sender, MouseEventArgs e) {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            this.BringToFront();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e) {
            new AboutForm().ShowDialog();
        }

        private void gestureSetToolStripMenuItem_Click(object sender, EventArgs e) {
            GestureSetForm form = new GestureSetForm(gestureSet);
            if (form.ShowDialog(this) == DialogResult.OK) {
                gestureSet = form.GetSet;
                if(analyser != null)
                    analyser.GestureSet = gestureSet;
            }
            form.Dispose();
            
        }

        private void launchScriptToolStripMenuItem_Click(object sender, EventArgs e) {
            OpenFileDialog fd = new OpenFileDialog();
            fd.InitialDirectory = Application.StartupPath + "\\ahk";
            fd.Filter = "autohotkey (*.ahk)|*.ahk";

            if (fd.ShowDialog() == DialogResult.OK) {
                Process firstProc = new Process();
                firstProc.StartInfo.FileName = fd.FileName;
                firstProc.Start();
            }
        }





    }
}