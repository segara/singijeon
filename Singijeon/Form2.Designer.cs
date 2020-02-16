namespace Singijeon
{
    partial class Form2
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
            this.LogListBox = new System.Windows.Forms.ListBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.asset_label = new System.Windows.Forms.Label();
            this.d2Asset_label = new System.Windows.Forms.Label();
            this.estimatedAsset_label = new System.Windows.Forms.Label();
            this.investment_label = new System.Windows.Forms.Label();
            this.profit_label = new System.Windows.Forms.Label();
            this.profitRate_label = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.groupBox6.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // LogListBox
            // 
            this.LogListBox.FormattingEnabled = true;
            this.LogListBox.ItemHeight = 12;
            this.LogListBox.Location = new System.Drawing.Point(12, 242);
            this.LogListBox.Name = "LogListBox";
            this.LogListBox.Size = new System.Drawing.Size(1239, 304);
            this.LogListBox.TabIndex = 0;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.tableLayoutPanel2);
            this.groupBox6.Location = new System.Drawing.Point(12, 11);
            this.groupBox6.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox6.Size = new System.Drawing.Size(356, 225);
            this.groupBox6.TabIndex = 5;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "계좌정보";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.asset_label, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.d2Asset_label, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.estimatedAsset_label, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.investment_label, 1, 3);
            this.tableLayoutPanel2.Controls.Add(this.profit_label, 1, 4);
            this.tableLayoutPanel2.Controls.Add(this.profitRate_label, 1, 5);
            this.tableLayoutPanel2.Controls.Add(this.label10, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.label11, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.label12, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.label13, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.label14, 0, 4);
            this.tableLayoutPanel2.Controls.Add(this.label15, 0, 5);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(18, 21);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 6;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(333, 187);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // asset_label
            // 
            this.asset_label.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.asset_label.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.asset_label.Location = new System.Drawing.Point(169, 0);
            this.asset_label.Name = "asset_label";
            this.asset_label.Size = new System.Drawing.Size(161, 31);
            this.asset_label.TabIndex = 6;
            this.asset_label.Text = "예수금";
            this.asset_label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // d2Asset_label
            // 
            this.d2Asset_label.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.d2Asset_label.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.d2Asset_label.Location = new System.Drawing.Point(169, 31);
            this.d2Asset_label.Name = "d2Asset_label";
            this.d2Asset_label.Size = new System.Drawing.Size(161, 31);
            this.d2Asset_label.TabIndex = 7;
            this.d2Asset_label.Text = "D+2추정예수금";
            this.d2Asset_label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // estimatedAsset_label
            // 
            this.estimatedAsset_label.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.estimatedAsset_label.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.estimatedAsset_label.Location = new System.Drawing.Point(169, 62);
            this.estimatedAsset_label.Name = "estimatedAsset_label";
            this.estimatedAsset_label.Size = new System.Drawing.Size(161, 31);
            this.estimatedAsset_label.TabIndex = 8;
            this.estimatedAsset_label.Text = "예탁자산평가액";
            this.estimatedAsset_label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // investment_label
            // 
            this.investment_label.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.investment_label.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.investment_label.Location = new System.Drawing.Point(169, 93);
            this.investment_label.Name = "investment_label";
            this.investment_label.Size = new System.Drawing.Size(161, 31);
            this.investment_label.TabIndex = 9;
            this.investment_label.Text = "당일투자원금";
            this.investment_label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // profit_label
            // 
            this.profit_label.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.profit_label.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.profit_label.Location = new System.Drawing.Point(169, 124);
            this.profit_label.Name = "profit_label";
            this.profit_label.Size = new System.Drawing.Size(161, 31);
            this.profit_label.TabIndex = 10;
            this.profit_label.Text = "당일투자손익";
            this.profit_label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // profitRate_label
            // 
            this.profitRate_label.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.profitRate_label.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.profitRate_label.Location = new System.Drawing.Point(169, 155);
            this.profitRate_label.Name = "profitRate_label";
            this.profitRate_label.Size = new System.Drawing.Size(161, 32);
            this.profitRate_label.TabIndex = 11;
            this.profitRate_label.Text = "당일손익률";
            this.profitRate_label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label10
            // 
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label10.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.label10.Location = new System.Drawing.Point(3, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(160, 31);
            this.label10.TabIndex = 0;
            this.label10.Text = "예수금";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label11.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.label11.Location = new System.Drawing.Point(3, 31);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(160, 31);
            this.label11.TabIndex = 1;
            this.label11.Text = "D+2추정예수금";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label12
            // 
            this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label12.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.label12.Location = new System.Drawing.Point(3, 62);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(160, 31);
            this.label12.TabIndex = 2;
            this.label12.Text = "예탁자산평가액";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label13
            // 
            this.label13.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label13.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.label13.Location = new System.Drawing.Point(3, 93);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(160, 31);
            this.label13.TabIndex = 3;
            this.label13.Text = "당일투자원금";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label14
            // 
            this.label14.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label14.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.label14.Location = new System.Drawing.Point(3, 124);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(160, 31);
            this.label14.TabIndex = 4;
            this.label14.Text = "당일투자손익";
            this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label15
            // 
            this.label15.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label15.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.label15.Location = new System.Drawing.Point(3, 155);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(160, 32);
            this.label15.TabIndex = 5;
            this.label15.Text = "당일손익률";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1263, 556);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.LogListBox);
            this.Name = "Form2";
            this.Text = "Form2";
            this.groupBox6.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox LogListBox;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label asset_label;
        private System.Windows.Forms.Label d2Asset_label;
        private System.Windows.Forms.Label estimatedAsset_label;
        private System.Windows.Forms.Label investment_label;
        private System.Windows.Forms.Label profit_label;
        private System.Windows.Forms.Label profitRate_label;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
    }
}