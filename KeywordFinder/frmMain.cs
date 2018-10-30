using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KeywordFinder
{
    public partial class frmMain : Form
    {
        private delegate void ListViewClearAllItemsDelegate();
        private delegate void ListViewAddNewItemCallback(ListViewItem lvi);
        private Process currentProcess;
        private ListViewItem currentItem;
        private ArgumentBuilder argumentBuilder;
        private BackgroundWorker searchWorker;
        private frmLoading loading;
        public frmMain()
        {
            InitializeComponent();
            currentProcess = new Process();
            argumentBuilder = new ArgumentBuilder();
            searchWorker = new BackgroundWorker();
            searchWorker.DoWork += SearchWorker_DoWork;
            searchWorker.RunWorkerCompleted += SearchWorker_RunWorkerCompleted;
            searchWorker.WorkerSupportsCancellation = true;
        }

        private void SearchWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }

        private void SearchWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            ClearAllListItems();

            argumentBuilder.Clear();
            argumentBuilder.AppendArgument("/s");
            if (chkIgnoreUpper.Checked)
            {
                argumentBuilder.AppendArgument("/i");
            }
            argumentBuilder.AppendArgument("/c:\"" + txtKeyword.Text + "\"");
            if (rbFile.Checked)
            {
                argumentBuilder.AppendArgument("/f:" + txtFile.Text);
            }
            else if (rbDirectory.Checked)
            {
                if (chkExtension.Checked)
                {
                    argumentBuilder.AppendArgument("\"" + txtTargePath.Text + "\\*." + txtExtension.Text + "\"");
                }
                else
                {
                    argumentBuilder.AppendArgument("\"" + txtTargePath.Text + "\\*.*" + "\"");
                }
            }

            currentProcess = new Process();
            currentProcess.StartInfo.FileName = "findstr";
            currentProcess.StartInfo.Arguments = argumentBuilder.ToString();
            currentProcess.StartInfo.UseShellExecute = false;
            currentProcess.StartInfo.CreateNoWindow = true;
            currentProcess.StartInfo.RedirectStandardInput = true;
            currentProcess.StartInfo.RedirectStandardOutput = true;
            currentProcess.StartInfo.RedirectStandardError = false;
            currentProcess.Start();
            currentProcess.BeginOutputReadLine();
            currentProcess.OutputDataReceived += OutputDataReceived;
            currentProcess.Exited += CurrentProcess_Exited;
        }

        private void ClearAllListItems()
        {
            if(listView1.InvokeRequired)
            {
                ListViewClearAllItemsDelegate ld = new ListViewClearAllItemsDelegate(ClearAllListItems);
                listView1.Invoke(ld);
            }
            else
            {
                listView1.Items.Clear();
            }
        }

        private void CurrentProcess_Exited(object sender, EventArgs e)
        {
            btnSearch.Enabled = true;
            loading.Close();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtTargePath.Text = dialog.SelectedPath;
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string text = null;
            string type = null;
            if(rbDirectory.Checked)
            {
                text = txtTargePath.Text;
                type = "Target Path";
            }
            else if(rbFile.Checked)
            {
                text = txtFile.Text;
                type = "Target File";
            }
            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show(string.Format("{0} can't be empty!", type), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if(string.IsNullOrEmpty(txtKeyword.Text))
            {
                MessageBox.Show(string.Format("Keyword can't be empty!"), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            loading = new frmLoading();
            loading.CancelWorker += Loading_CancelWorker;
            loading.Show();
            btnSearch.Enabled = false;
            searchWorker.RunWorkerAsync();
        }

        private void Loading_CancelWorker()
        {
            searchWorker.CancelAsync();
            if(!currentProcess.HasExited)
            {
                currentProcess.Kill();
            }
            currentProcess.Dispose();
            btnSearch.Enabled = true;
        }

        private void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            try
            {
                string data = e.Data;
                if (!string.IsNullOrEmpty(data))
                {
                    int index = GetIndex(data);
                    int sublength = data.Length - index - 1;
                    string[] dataToken = new string[2]
                    {
                    data.Substring(0, index),
                    data.Substring(index +1,sublength)
                    };
                    if (dataToken != null)
                    {
                        ListViewItem lvi = new ListViewItem();
                        lvi.Text = dataToken[0];
                        lvi.SubItems.Add(dataToken[1]);
                        ;
                        if (listView1.InvokeRequired)
                        {
                            ListViewAddNewItemCallback callback = new ListViewAddNewItemCallback(ListViewAddItem);
                            this.Invoke(callback, lvi);
                        }
                        else
                        {
                            listView1.Items.Add(lvi);
                        }
                    }
                }
            }
            catch
            {
                return;
            }
        }

        private int GetIndex(string str)
        {
            int count = 0;
            int index = 0;
            foreach(char c in str)
            {
                if(c== ':')
                {
                    if (count < 1)
                    {
                        count++;
                    }
                    else if(count ==1)
                    {
                        index++;
                        break;
                    }
                }
                
                else
                {
                    index++;
                }
            }
            return index;
        }

        private void ListViewAddItem(ListViewItem lvi)
        {
            listView1.Items.Add(lvi);
        }

        private void chkExtension_CheckedChanged(object sender, EventArgs e)
        {
            if(chkExtension.Checked)
            {
                txtExtension.Enabled = true;
            }
            else
            {
                txtExtension.Enabled = false;
            }
        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.Item != null)
            {
                currentItem = e.Item;
                btnOpen.Enabled = true;
            }
            else
            {
                btnOpen.Enabled = false;
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            Process notepadProcess = new Process();
            notepadProcess.StartInfo.FileName = "notepad.exe";
            notepadProcess.StartInfo.Arguments = currentItem.Text;
            notepadProcess.StartInfo.CreateNoWindow = false;
            notepadProcess.Start();
        }

        private void rbDirectory_CheckedChanged(object sender, EventArgs e)
        {
            txtTargePath.Enabled = true;
            btnBrowse.Enabled = true;
            txtFile.Enabled = false;
            btnChoose.Enabled = false;
            chkExtension.Enabled = true;
        }

        private void rbFile_CheckedChanged(object sender, EventArgs e)
        {
            txtTargePath.Enabled = false;
            btnBrowse.Enabled = false;
            txtFile.Enabled = true;
            btnChoose.Enabled = true;
            chkExtension.Enabled = false;
        }

        private void btnChoose_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtFile.Text = dialog.FileName;
            }
        }
    }
}
