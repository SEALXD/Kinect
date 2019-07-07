namespace BasicOne
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.imageBoxColor = new Emgu.CV.UI.ImageBox();
            this.imageBoxDepth = new Emgu.CV.UI.ImageBox();
            this.cbNearMode = new System.Windows.Forms.CheckBox();
            this.imageBoxSkeleton = new Emgu.CV.UI.ImageBox();
            this.cbSeat = new System.Windows.Forms.CheckBox();
            this.cbBody = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.imageBoxColor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBoxDepth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBoxSkeleton)).BeginInit();
            this.SuspendLayout();
            // 
            // imageBoxColor
            // 
            this.imageBoxColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.imageBoxColor.Location = new System.Drawing.Point(18, 18);
            this.imageBoxColor.Margin = new System.Windows.Forms.Padding(4);
            this.imageBoxColor.Name = "imageBoxColor";
            this.imageBoxColor.Size = new System.Drawing.Size(407, 386);
            this.imageBoxColor.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.imageBoxColor.TabIndex = 2;
            this.imageBoxColor.TabStop = false;
            // 
            // imageBoxDepth
            // 
            this.imageBoxDepth.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.imageBoxDepth.Location = new System.Drawing.Point(435, 18);
            this.imageBoxDepth.Margin = new System.Windows.Forms.Padding(4);
            this.imageBoxDepth.Name = "imageBoxDepth";
            this.imageBoxDepth.Size = new System.Drawing.Size(407, 386);
            this.imageBoxDepth.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.imageBoxDepth.TabIndex = 3;
            this.imageBoxDepth.TabStop = false;
            this.imageBoxDepth.Click += new System.EventHandler(this.imageBoxDepth_Click);
            // 
            // cbNearMode
            // 
            this.cbNearMode.AutoSize = true;
            this.cbNearMode.Location = new System.Drawing.Point(18, 414);
            this.cbNearMode.Margin = new System.Windows.Forms.Padding(4);
            this.cbNearMode.Name = "cbNearMode";
            this.cbNearMode.Size = new System.Drawing.Size(88, 22);
            this.cbNearMode.TabIndex = 4;
            this.cbNearMode.Text = "近模式";
            this.cbNearMode.UseVisualStyleBackColor = true;
            this.cbNearMode.CheckedChanged += new System.EventHandler(this.cbNearMode_CheckedChanged);
            // 
            // imageBoxSkeleton
            // 
            this.imageBoxSkeleton.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.imageBoxSkeleton.Location = new System.Drawing.Point(852, 18);
            this.imageBoxSkeleton.Margin = new System.Windows.Forms.Padding(4);
            this.imageBoxSkeleton.Name = "imageBoxSkeleton";
            this.imageBoxSkeleton.Size = new System.Drawing.Size(438, 386);
            this.imageBoxSkeleton.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.imageBoxSkeleton.TabIndex = 5;
            this.imageBoxSkeleton.TabStop = false;
            // 
            // cbSeat
            // 
            this.cbSeat.AutoSize = true;
            this.cbSeat.Location = new System.Drawing.Point(182, 414);
            this.cbSeat.Margin = new System.Windows.Forms.Padding(4);
            this.cbSeat.Name = "cbSeat";
            this.cbSeat.Size = new System.Drawing.Size(70, 22);
            this.cbSeat.TabIndex = 6;
            this.cbSeat.Text = "坐姿";
            this.cbSeat.UseVisualStyleBackColor = true;
            this.cbSeat.CheckedChanged += new System.EventHandler(this.cbSeat_CheckedChanged);
            // 
            // cbBody
            // 
            this.cbBody.AutoSize = true;
            this.cbBody.Location = new System.Drawing.Point(534, 414);
            this.cbBody.Margin = new System.Windows.Forms.Padding(4);
            this.cbBody.Name = "cbBody";
            this.cbBody.Size = new System.Drawing.Size(106, 22);
            this.cbBody.TabIndex = 7;
            this.cbBody.Text = "人体区域";
            this.cbBody.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1308, 453);
            this.Controls.Add(this.cbBody);
            this.Controls.Add(this.cbSeat);
            this.Controls.Add(this.imageBoxSkeleton);
            this.Controls.Add(this.cbNearMode);
            this.Controls.Add(this.imageBoxDepth);
            this.Controls.Add(this.imageBoxColor);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.Text = "HHH";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.imageBoxColor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBoxDepth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBoxSkeleton)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Emgu.CV.UI.ImageBox imageBoxColor;
        private Emgu.CV.UI.ImageBox imageBoxDepth;
        private System.Windows.Forms.CheckBox cbNearMode;
        private Emgu.CV.UI.ImageBox imageBoxSkeleton;
        private System.Windows.Forms.CheckBox cbSeat;
        private System.Windows.Forms.CheckBox cbBody;
    }
}

