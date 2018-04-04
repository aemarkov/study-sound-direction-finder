namespace SoundDirectionViiewer
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
            this.cboxComs = new System.Windows.Forms.ToolStripComboBox();
            this.btnRefreshComs = new System.Windows.Forms.ToolStripButton();
            this.btnOpenCom = new System.Windows.Forms.ToolStripButton();
            this.sgraphAdc = new Common.Components.ScrollingGraph();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.AllowMerge = false;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.cboxComs,
            this.btnRefreshComs,
            this.btnOpenCom});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(528, 28);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(38, 25);
            this.toolStripLabel1.Text = "COM:";
            // 
            // cboxComs
            // 
            this.cboxComs.Name = "cboxComs";
            this.cboxComs.Size = new System.Drawing.Size(121, 28);
            // 
            // btnRefreshComs
            // 
            this.btnRefreshComs.AutoSize = false;
            this.btnRefreshComs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRefreshComs.Image = global::SoundDirectionViiewer.Properties.Resources.icon_update;
            this.btnRefreshComs.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRefreshComs.Name = "btnRefreshComs";
            this.btnRefreshComs.Size = new System.Drawing.Size(25, 25);
            this.btnRefreshComs.Text = "Обновить";
            this.btnRefreshComs.Click += new System.EventHandler(this.btnRefreshComs_Click);
            // 
            // btnOpenCom
            // 
            this.btnOpenCom.AutoSize = false;
            this.btnOpenCom.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnOpenCom.Image = global::SoundDirectionViiewer.Properties.Resources.icon_disconnected;
            this.btnOpenCom.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnOpenCom.Name = "btnOpenCom";
            this.btnOpenCom.Size = new System.Drawing.Size(25, 25);
            this.btnOpenCom.Text = "Подключиться";
            this.btnOpenCom.Click += new System.EventHandler(this.btnOpenCom_Click);
            // 
            // sgraphAdc
            // 
            this.sgraphAdc.IsRolling = false;
            this.sgraphAdc.IsXAutoScale = false;
            this.sgraphAdc.IsYAutoScale = false;
            this.sgraphAdc.Location = new System.Drawing.Point(12, 31);
            this.sgraphAdc.Name = "sgraphAdc";
            this.sgraphAdc.Size = new System.Drawing.Size(500, 400);
            this.sgraphAdc.TabIndex = 6;
            this.sgraphAdc.Title = "";
            this.sgraphAdc.WindowSize = 512;
            this.sgraphAdc.XMaxValue = 0D;
            this.sgraphAdc.XMinValue = 0D;
            this.sgraphAdc.XTitle = "Отсчеты";
            this.sgraphAdc.YMaxValue = 4D;
            this.sgraphAdc.YMinValue = 0D;
            this.sgraphAdc.YTitle = "Амплитуда";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(528, 448);
            this.Controls.Add(this.sgraphAdc);
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
        private System.Windows.Forms.ToolStripComboBox cboxComs;
        private System.Windows.Forms.ToolStripButton btnRefreshComs;
        private System.Windows.Forms.ToolStripButton btnOpenCom;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private Common.Components.ScrollingGraph sgraphAdc;
    }
}

