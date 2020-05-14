namespace Singijeon
{
    partial class Form3
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea3 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea4 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series4 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series5 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.candleChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chartItemCodeTextBox = new System.Windows.Forms.TextBox();
            this.ChartRequestBtn = new System.Windows.Forms.Button();
            this.StateLabel = new System.Windows.Forms.Label();
            this.ItemName = new System.Windows.Forms.Label();
            this.vpciPoint = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.envel4RadioBtn = new System.Windows.Forms.RadioButton();
            this.envel3RadioBtn = new System.Windows.Forms.RadioButton();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tick_5_minute = new System.Windows.Forms.RadioButton();
            this.tick_30_tick = new System.Windows.Forms.RadioButton();
            this.refreshCheck = new System.Windows.Forms.CheckBox();
            this.UpDownInfoText = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.candleChart)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // candleChart
            // 
            chartArea1.AxisX.IsReversed = true;
            chartArea1.AxisX.LabelStyle.Enabled = false;
            chartArea1.AxisX.ScrollBar.Enabled = false;
            chartArea1.AxisY.IsStartedFromZero = false;
            chartArea1.CursorX.IsUserEnabled = true;
            chartArea1.CursorX.IsUserSelectionEnabled = true;
            chartArea1.Name = "PriceChartArea";
            chartArea1.Position.Auto = false;
            chartArea1.Position.Height = 30F;
            chartArea1.Position.Width = 94F;
            chartArea1.Position.X = 5F;
            chartArea1.Position.Y = 3F;
            chartArea2.AlignWithChartArea = "PriceChartArea";
            chartArea2.AxisX.IsReversed = true;
            chartArea2.AxisX.LabelStyle.Enabled = false;
            chartArea2.AxisX.ScrollBar.Enabled = false;
            chartArea2.CursorX.IsUserEnabled = true;
            chartArea2.CursorX.IsUserSelectionEnabled = true;
            chartArea2.Name = "VolumeChartArea";
            chartArea2.Position.Auto = false;
            chartArea2.Position.Height = 15F;
            chartArea2.Position.Width = 94F;
            chartArea2.Position.X = 3F;
            chartArea2.Position.Y = 32F;
            chartArea3.AlignWithChartArea = "PriceChartArea";
            chartArea3.AxisX.IsReversed = true;
            chartArea3.AxisX.ScrollBar.Enabled = false;
            chartArea3.CursorX.IsUserEnabled = true;
            chartArea3.CursorX.IsUserSelectionEnabled = true;
            chartArea3.Name = "VwmaChartArea";
            chartArea3.Position.Auto = false;
            chartArea3.Position.Height = 25F;
            chartArea3.Position.Width = 94F;
            chartArea3.Position.X = 3F;
            chartArea3.Position.Y = 46F;
            chartArea4.AlignWithChartArea = "PriceChartArea";
            chartArea4.AxisX.IsReversed = true;
            chartArea4.CursorX.IsUserEnabled = true;
            chartArea4.CursorX.IsUserSelectionEnabled = true;
            chartArea4.Name = "VpciChartArea";
            chartArea4.Position.Auto = false;
            chartArea4.Position.Height = 25F;
            chartArea4.Position.Width = 94F;
            chartArea4.Position.X = 3F;
            chartArea4.Position.Y = 70F;
            this.candleChart.ChartAreas.Add(chartArea1);
            this.candleChart.ChartAreas.Add(chartArea2);
            this.candleChart.ChartAreas.Add(chartArea3);
            this.candleChart.ChartAreas.Add(chartArea4);
            this.candleChart.Location = new System.Drawing.Point(13, 39);
            this.candleChart.Name = "candleChart";
            series1.ChartArea = "PriceChartArea";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Candlestick;
            series1.Name = "StockCandle";
            series1.YValuesPerPoint = 4;
            series2.ChartArea = "VolumeChartArea";
            series2.Name = "Volume";
            series3.ChartArea = "VwmaChartArea";
            series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            series3.Name = "VWMA";
            series4.ChartArea = "VpciChartArea";
            series4.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            series4.Name = "VPCI";
            series5.BorderWidth = 3;
            series5.ChartArea = "VpciChartArea";
            series5.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series5.Name = "Middle";
            series5.ShadowColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.candleChart.Series.Add(series1);
            this.candleChart.Series.Add(series2);
            this.candleChart.Series.Add(series3);
            this.candleChart.Series.Add(series4);
            this.candleChart.Series.Add(series5);
            this.candleChart.Size = new System.Drawing.Size(776, 617);
            this.candleChart.TabIndex = 0;
            this.candleChart.Text = "chart1";
            // 
            // chartItemCodeTextBox
            // 
            this.chartItemCodeTextBox.Location = new System.Drawing.Point(13, 13);
            this.chartItemCodeTextBox.Name = "chartItemCodeTextBox";
            this.chartItemCodeTextBox.Size = new System.Drawing.Size(100, 21);
            this.chartItemCodeTextBox.TabIndex = 1;
            this.chartItemCodeTextBox.Text = "078890";
            // 
            // ChartRequestBtn
            // 
            this.ChartRequestBtn.Location = new System.Drawing.Point(120, 13);
            this.ChartRequestBtn.Name = "ChartRequestBtn";
            this.ChartRequestBtn.Size = new System.Drawing.Size(75, 23);
            this.ChartRequestBtn.TabIndex = 2;
            this.ChartRequestBtn.Text = "button1";
            this.ChartRequestBtn.UseVisualStyleBackColor = true;
            this.ChartRequestBtn.Click += new System.EventHandler(this.ChartRequestBtn_Click);
            // 
            // StateLabel
            // 
            this.StateLabel.AutoSize = true;
            this.StateLabel.Location = new System.Drawing.Point(38, 670);
            this.StateLabel.Name = "StateLabel";
            this.StateLabel.Size = new System.Drawing.Size(38, 12);
            this.StateLabel.TabIndex = 3;
            this.StateLabel.Text = "label1";
            // 
            // ItemName
            // 
            this.ItemName.AutoSize = true;
            this.ItemName.Location = new System.Drawing.Point(219, 19);
            this.ItemName.Name = "ItemName";
            this.ItemName.Size = new System.Drawing.Size(17, 12);
            this.ItemName.TabIndex = 4;
            this.ItemName.Text = "...";
            // 
            // vpciPoint
            // 
            this.vpciPoint.AutoSize = true;
            this.vpciPoint.Location = new System.Drawing.Point(118, 670);
            this.vpciPoint.Name = "vpciPoint";
            this.vpciPoint.Size = new System.Drawing.Size(38, 12);
            this.vpciPoint.TabIndex = 5;
            this.vpciPoint.Text = "label1";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.envel4RadioBtn);
            this.panel1.Controls.Add(this.envel3RadioBtn);
            this.panel1.Location = new System.Drawing.Point(256, 662);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 25);
            this.panel1.TabIndex = 6;
            // 
            // envel4RadioBtn
            // 
            this.envel4RadioBtn.AutoSize = true;
            this.envel4RadioBtn.Checked = true;
            this.envel4RadioBtn.Location = new System.Drawing.Point(117, 6);
            this.envel4RadioBtn.Name = "envel4RadioBtn";
            this.envel4RadioBtn.Size = new System.Drawing.Size(80, 16);
            this.envel4RadioBtn.TabIndex = 1;
            this.envel4RadioBtn.TabStop = true;
            this.envel4RadioBtn.Text = "envelope4";
            this.envel4RadioBtn.UseVisualStyleBackColor = true;
            this.envel4RadioBtn.CheckedChanged += new System.EventHandler(this.Envel4RadioBtn_CheckedChanged);
            // 
            // envel3RadioBtn
            // 
            this.envel3RadioBtn.AutoSize = true;
            this.envel3RadioBtn.Location = new System.Drawing.Point(3, 6);
            this.envel3RadioBtn.Name = "envel3RadioBtn";
            this.envel3RadioBtn.Size = new System.Drawing.Size(80, 16);
            this.envel3RadioBtn.TabIndex = 0;
            this.envel3RadioBtn.Text = "envelope3";
            this.envel3RadioBtn.UseVisualStyleBackColor = true;
            this.envel3RadioBtn.CheckedChanged += new System.EventHandler(this.Envel3RadioBtn_CheckedChanged);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.tick_5_minute);
            this.panel2.Controls.Add(this.tick_30_tick);
            this.panel2.Location = new System.Drawing.Point(475, 662);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(200, 25);
            this.panel2.TabIndex = 7;
            // 
            // tick_5_minute
            // 
            this.tick_5_minute.AutoSize = true;
            this.tick_5_minute.Checked = true;
            this.tick_5_minute.Location = new System.Drawing.Point(117, 6);
            this.tick_5_minute.Name = "tick_5_minute";
            this.tick_5_minute.Size = new System.Drawing.Size(71, 16);
            this.tick_5_minute.TabIndex = 1;
            this.tick_5_minute.TabStop = true;
            this.tick_5_minute.Text = "5 minute";
            this.tick_5_minute.UseVisualStyleBackColor = true;
            this.tick_5_minute.CheckedChanged += new System.EventHandler(this.Tick_5_minute_CheckedChanged);
            // 
            // tick_30_tick
            // 
            this.tick_30_tick.AutoSize = true;
            this.tick_30_tick.Location = new System.Drawing.Point(3, 6);
            this.tick_30_tick.Name = "tick_30_tick";
            this.tick_30_tick.Size = new System.Drawing.Size(58, 16);
            this.tick_30_tick.TabIndex = 0;
            this.tick_30_tick.Text = "30 tick";
            this.tick_30_tick.UseVisualStyleBackColor = true;
            this.tick_30_tick.CheckedChanged += new System.EventHandler(this.Tick_30_tick_CheckedChanged);
            // 
            // refreshCheck
            // 
            this.refreshCheck.AutoSize = true;
            this.refreshCheck.Location = new System.Drawing.Point(716, 17);
            this.refreshCheck.Name = "refreshCheck";
            this.refreshCheck.Size = new System.Drawing.Size(72, 16);
            this.refreshCheck.TabIndex = 8;
            this.refreshCheck.Text = "새로고침";
            this.refreshCheck.UseVisualStyleBackColor = true;
            this.refreshCheck.CheckedChanged += new System.EventHandler(this.refreshCheck_CheckedChanged);
            // 
            // UpDownInfoText
            // 
            this.UpDownInfoText.AutoSize = true;
            this.UpDownInfoText.Location = new System.Drawing.Point(560, 19);
            this.UpDownInfoText.Name = "UpDownInfoText";
            this.UpDownInfoText.Size = new System.Drawing.Size(11, 12);
            this.UpDownInfoText.TabIndex = 9;
            this.UpDownInfoText.Text = "0";
            // 
            // Form3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 687);
            this.Controls.Add(this.UpDownInfoText);
            this.Controls.Add(this.refreshCheck);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.vpciPoint);
            this.Controls.Add(this.ItemName);
            this.Controls.Add(this.StateLabel);
            this.Controls.Add(this.ChartRequestBtn);
            this.Controls.Add(this.chartItemCodeTextBox);
            this.Controls.Add(this.candleChart);
            this.Name = "Form3";
            this.Text = "Form3";
            ((System.ComponentModel.ISupportInitialize)(this.candleChart)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart candleChart;
        private System.Windows.Forms.TextBox chartItemCodeTextBox;
        private System.Windows.Forms.Button ChartRequestBtn;
        private System.Windows.Forms.Label StateLabel;
        private System.Windows.Forms.Label ItemName;
        private System.Windows.Forms.Label vpciPoint;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton envel4RadioBtn;
        private System.Windows.Forms.RadioButton envel3RadioBtn;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.RadioButton tick_5_minute;
        private System.Windows.Forms.RadioButton tick_30_tick;
        private System.Windows.Forms.CheckBox refreshCheck;
        private System.Windows.Forms.Label UpDownInfoText;
    }
}