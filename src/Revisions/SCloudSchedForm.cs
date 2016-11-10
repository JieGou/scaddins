﻿// (C) Copyright 2013-2016 by Andrew Nicholas
//
// This file is part of SCaddins.
//
// SCaddins is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// SCaddins is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with SCaddins.  If not, see <http://www.gnu.org/licenses/>.

namespace SCaddins.SCloudSChed
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Forms;
    using Autodesk.Revit.DB;
     using Autodesk.Revit.UI;
    using SCaddins.Common;

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    public partial class Form1 : System.Windows.Forms.Form
    {
        private Document doc;

        public Form1(Document doc)
        {
            this.doc = doc;
            this.InitializeComponent();
            dataGridView1.DataSource = SCloudScheduler.GetRevisionClouds(doc);
        }

        private void SelectAll(bool all)
        {
            foreach (DataGridViewRow row in this.dataGridView1.Rows) {
                RevisionCloudItem rev = row.DataBoundItem as RevisionCloudItem;
                if (rev != null) {
                    if (all) {
                        if (!rev.Export) {
                            rev.Export = true;
                        }
                    } else {
                        if (rev.Export) {
                            rev.Export = false;
                        }
                    }
                }
            }
            dataGridView1.Refresh();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            this.SelectAll(true);
        }

        private void Button2_Click(object sender, EventArgs e)
        {
             this.SelectAll(false);
        }
        
        private void NoSelectionErrorDialog()
        {
            
        }

        private void ButtonScheduleRevisionsClick(object sender, EventArgs e)
        {         
            Dictionary<string, RevisionCloudItem> dictionary = new Dictionary<string, RevisionCloudItem>();
            foreach (DataGridViewRow row in this.dataGridView1.Rows) {
                RevisionCloudItem rev = row.DataBoundItem as RevisionCloudItem;
                if (rev != null) {
                    string s = rev.Date + rev.Description;
                    if (!dictionary.ContainsKey(s)) {
                        dictionary.Add(s, rev);
                    }
                }
            }
            var saveFileDialogResult = saveFileDialog1.ShowDialog();
            
            var saveFileName = string.Empty;
            if (saveFileDialogResult == DialogResult.OK) {
                saveFileName = saveFileDialog1.FileName;
            }
            SCloudScheduler.ExportCloudInfo(this.doc, dictionary, saveFileName);
        }
        
        private void AssignRevisionToClouds(Collection<RevisionCloudItem> revisionClouds)
        {
            #if (!REVIT2014)
            var r = new SCaddins.ExportManager.RevisionSelectionDialog(doc);
            var result = r.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK) {      
            } else {
                return;
            }
            if (r.Id != null) {
            } else {
                TaskDialog.Show("test", "id is null"); 
                return;
            }
            using (var t = new Transaction(doc, "Assign Revisions to Clouds")) {
                t.Start();
                foreach (RevisionCloudItem rc in revisionClouds) { 
                    if (rc != null) {
                        rc.SetCloudId(r.Id);
                    } else {  
                    }
                }
                t.Commit();
            }
            #endif
        }
                
        private void ButtonAssignRevisionsClick(object sender, EventArgs e)
        {
           Collection<RevisionCloudItem> cloudSelection = new Collection<RevisionCloudItem>();
           foreach (DataGridViewRow row in this.dataGridView1.Rows) {
                RevisionCloudItem rev = row.DataBoundItem as RevisionCloudItem;
                if (rev.Export) {
                    cloudSelection.Add(rev);
                } else {

                }
            }  
            AssignRevisionToClouds(cloudSelection);
            dataGridView1.DataSource = SCloudScheduler.GetRevisionClouds(doc);
            this.dataGridView1.Refresh();
        }
        void ButtonDeleteRevisionsClick(object sender, EventArgs e)
        {
          
        }
        
    }
}