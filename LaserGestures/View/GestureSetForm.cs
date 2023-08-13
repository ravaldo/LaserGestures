using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace LaserGestures {
    public partial class GestureSetForm : Form {

        GestureSet gestureSet = new GestureSet();
        private const string filterString = "laser gesture files (*.lg)|*.lg|All files (*.*)|*.*";

        public GestureSetForm() {
            InitializeComponent();
        }

        public GestureSetForm(GestureSet gestureSet)
            : this() {
            this.gestureSet = gestureSet;
            PopulateListView();
        }

        private void PopulateListView() {
            ListView.ListViewItemCollection list = new ListView.ListViewItemCollection(listView1);
            list.Clear();
            foreach (Gesture g in gestureSet.GetSet) {
                ListViewItem temp = new ListViewItem(g.String);
                if (g.Genre == Genre.STATIC)
                    temp.SubItems.Add("Static");
                else {
                    temp.SubItems.Add("Dynamic");
                    temp.SubItems.Add(g.Action.ToString());
                }
                list.Add(temp);
            }
        }

        private void openButton_Click(object sender, EventArgs e) {
            OpenFileDialog fd = new OpenFileDialog();
            fd.InitialDirectory = Application.StartupPath;
            fd.Filter = filterString;
            
            if (fd.ShowDialog() == DialogResult.OK) {
                gestureSet = GestureSet.Open(fd.FileName);
            }
            PopulateListView();
        }

        private void saveButton_Click(object sender, EventArgs e) {
            SaveFileDialog fd = new SaveFileDialog();
            fd.InitialDirectory = Application.StartupPath;
            fd.Filter = filterString;

            if (fd.ShowDialog() == DialogResult.OK) {
                gestureSet.Save(fd.FileName);
            }            
        }

        private void clearButton_Click(object sender, EventArgs e) {
            gestureSet.Clear();
            PopulateListView();
        }


        public GestureSet GetSet {
            get { return gestureSet; }
        }

        private void addButton_Click(object sender, EventArgs e) {
            AddGestureForm form = new AddGestureForm();
            if (form.ShowDialog() == DialogResult.OK) {
                gestureSet.Add(form.GetGesture);
            }
            PopulateListView();
        }

        private void removeButton_Click(object sender, EventArgs e) {
            foreach (ListViewItem i in listView1.Items) {
                if (i.Selected) {
                    if(i.SubItems[1].Text.Equals("Static"))
                        gestureSet.RemoveStatic(i.SubItems[0].Text);
                    else if (i.SubItems[1].Text.Equals("Dynamic"))
                        gestureSet.RemoveStatic(i.SubItems[0].Text);
                }
            }
            PopulateListView();
        }



    }
}
