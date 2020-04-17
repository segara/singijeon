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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea5 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea6 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea7 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea8 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series6 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series7 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series8 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series9 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series10 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.candleChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chartItemCodeTextBox = new System.Windows.Forms.TextBox();
            this.ChartRequestBtn = new System.Windows.Forms.Button();
            this.StateLabel = new System.Windows.Forms.Label();
            this.ItemName = new System.Windows.Forms.Label();
            this.vpciPoint = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.envel3RadioBtn = new System.Windows.Forms.RadioButton();
            this.envel4RadioBtn = new System.Windows.Forms.RadioButton();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tick_5_minute = new System.Windows.Forms.RadioButton();
            this.tick_30_tick = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.candleChart)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // candleChart
            // 
            chartArea5.AxisX.IsReversed = true;
            chartArea5.AxisX.LabelStyle.Enabled = false;
            chartArea5.AxisX.ScrollBar.Enabled = false;
            chartArea5.AxisY.IsStartedFromZero = false;
            chartArea5.CursorX.IsUserEnabled = true;
            chartArea5.CursorX.IsUserSelectionEnabled = true;
            chartArea5.Name = "PriceChartArea";
            chartArea5.Position.Auto = false;
            chartArea5.Position.Height = 30F;
            chartArea5.Position.Width = 94F;
            chartArea5.Position.X = 5F;
            chartArea5.Position.Y = 3F;
            chartArea6.AlignWithChartArea = "PriceChartArea";
            chartArea6.AxisX.IsReversed = true;
            chartArea6.AxisX.LabelStyle.Enabled = false;
            chartArea6.AxisX.ScrollBar.Enabled = false;
            chartArea6.CursorX.IsUserEnabled = true;
            chartArea6.CursorX.IsUserSelectionEnabled = true;
            chartArea6.Name = "VolumeChartArea";
            chartArea6.Position.Auto = false;
            chartArea6.Position.Height = 15F;
            chartArea6.Position.Width = 94F;
            chartArea6.Position.X = 3F;
            chartArea6.Position.Y = 32F;
            chartArea7.AlignWithChartArea = "PriceChartArea";
            chartArea7.AxisX.IsReversed = true;
            chartArea7.AxisX.ScrollBar.Enabled = false;
            chartArea7.CursorX.IsUserEnabled = true;
            chartArea7.CursorX.IsUserSelectionEnabled = true;
            chartArea7.Name = "VwmaChartArea";
            chartArea7.Position.Auto = false;
            chartArea7.Position.Height = 25F;
            chartArea7.Position.Width = 94F;
            chartArea7.Position.X = 3F;
            chartArea7.Position.Y = 46F;
            chartArea8.AlignWithChartArea = "PriceChartArea";
            chartArea8.AxisX.IsReversed = true;
            chartArea8.CursorX.IsUserEnabled = true;
            chartArea8.CursorX.IsUserSelectionEnabled = true;
            chartArea8.Name = "VpciChartArea";
            chartArea8.Position.Auto = false;
            chartArea8.Position.Height = 25F;
            chartArea8.Position.Width = 94F;
            chartArea8.Position.X = 3F;
            chartArea8.Position.Y = 70F;
            this.candleChart.ChartAreas.Add(chartArea5);
            this.candleChart.ChartAreas.Add(chartArea6);
            this.candleChart.ChartAreas.Add(chartArea7);
            this.candleChart.ChartAreas.Add(chartArea8);
            this.candleChart.Location = new System.Drawing.Point(13, 39);
            this.candleChart.Name = "candleChart";
            series6.ChartArea = "PriceChartArea";
            series6.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Candlestick;
            series6.Name = "StockCandle";
            series6.YValuesPerPoint = 4;
            series7.ChartArea = "VolumeChartArea";
            series7.Name = "Volume";
            series8.ChartArea = "VwmaChartArea";
            series8.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            series8.Name = "VWMA";
            series9.ChartArea = "VpciChartArea";
            series9.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            series9.Name = "VPCI";
            series10.BorderWidth = 3;
            series10.ChartArea = "VpciChartArea";
            series10.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series10.Name = "Middle";
            series10.ShadowColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.candleChart.Series.Add(series6);
            this.candleChart.Series.Add(series7);
            this.candleChart.Series.Add(series8);
            this.candleChart.Series.Add(series9);
            this.candleChart.Series.Add(series10);
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
            this.ItemName.Size = new System.Drawing.Size(0, 12);
            this.ItemName.TabIndex = 4;
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
            // Form3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 687);
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
    }
}