using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FishPlanner.Forms
{
    public partial class cmbTags: Form
    {
        public cmbTags()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // cmbTags
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "cmbTags";
            this.ResumeLayout(false);

        }

        private ContextMenuStrip contextMenuStrip1;
        private IContainer components;
    }
}
