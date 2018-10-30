using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KeywordFinder
{
    public partial class frmLoading : Form
    {
        public event Action CancelWorker;
        public frmLoading()
        {
            InitializeComponent();
        }

        private void btnCancelWorker_Click(object sender, EventArgs e)
        {
            CancelWorker?.Invoke();
            this.Close();
        }
    }
}
