namespace SoundDirectionFinderPC
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.cboxLeftDevice = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.cboxRightDevice = new System.Windows.Forms.ToolStripComboBox();
            this.btnUpdateDevices = new System.Windows.Forms.ToolStripButton();
            this.btnRecord = new System.Windows.Forms.ToolStripButton();
            this.sgraphWave = new Common.Components.ScrollingGraph();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.cboxLeftDevice,
            this.toolStripLabel2,
            this.cboxRightDevice,
            this.btnUpdateDevices,
            this.btnRecord});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(909, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(78, 22);
            this.toolStripLabel1.Text = "Левый канал";
            // 
            // cboxLeftDevice
            // 
            this.cboxLeftDevice.Name = "cboxLeftDevice";
            this.cboxLeftDevice.Size = new System.Drawing.Size(121, 25);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(86, 22);
            this.toolStripLabel2.Text = "Правый канал";
            // 
            // cboxRightDevice
            // 
            this.cboxRightDevice.Name = "cboxRightDevice";
            this.cboxRightDevice.Size = new System.Drawing.Size(121, 25);
            // 
            // btnUpdateDevices
            // 
            this.btnUpdateDevices.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnUpdateDevices.Image = global::SoundDirectionFinderPC.Properties.Resources.icon_update;
            this.btnUpdateDevices.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnUpdateDevices.Name = "btnUpdateDevices";
            this.btnUpdateDevices.Size = new System.Drawing.Size(23, 22);
            this.btnUpdateDevices.Text = "toolStripButton1";
            this.btnUpdateDevices.Click += new System.EventHandler(this.btnUpdateDevices_Click);
            // 
            // btnRecord
            // 
            this.btnRecord.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRecord.Image = global::SoundDirectionFinderPC.Properties.Resources.icon_mic_off;
            this.btnRecord.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRecord.Name = "btnRecord";
            this.btnRecord.Size = new System.Drawing.Size(23, 22);
            this.btnRecord.Text = "toolStripButton2";
            this.btnRecord.Click += new System.EventHandler(this.btnRecord_Click);
            // 
            // sgraphWave
            // 
            this.sgraphWave.IsRolling = false;
            this.sgraphWave.IsXAutoScale = false;
            this.sgraphWave.IsYAutoScale = true;
            this.sgraphWave.Location = new System.Drawing.Point(12, 28);
            this.sgraphWave.Name = "sgraphWave";
            this.sgraphWave.Size = new System.Drawing.Size(636, 438);
            this.sgraphWave.TabIndex = 1;
            this.sgraphWave.Title = "Title";
            this.sgraphWave.WindowSize = 2048;
            this.sgraphWave.XMaxValue = 2048D;
            this.sgraphWave.XMinValue = 0D;
            this.sgraphWave.XTitle = "X Axis";
            this.sgraphWave.YMaxValue = 100D;
            this.sgraphWave.YMinValue = 0D;
            this.sgraphWave.YTitle = "Y Axis";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(909, 478);
            this.Controls.Add(this.sgraphWave);
            this.Controls.Add(this.toolStrip1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripComboBox cboxLeftDevice;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripComboBox cboxRightDevice;
        private System.Windows.Forms.ToolStripButton btnUpdateDevices;
        private System.Windows.Forms.ToolStripButton btnRecord;
        private Common.Components.ScrollingGraph sgraphWave;
    }
}

