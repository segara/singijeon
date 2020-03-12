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
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.candleChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chartItemCodeTextBox = new System.Windows.Forms.TextBox();
            this.ChartRequestBtn = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.candleChart)).BeginInit();
            this.SuspendLayout();
            // 
            // candleChart
            // 
            chartArea1.AxisX.IsReversed = true;
            chartArea1.AxisY.IsStartedFromZero = false;
            chartArea1.CursorX.IsUserEnabled = true;
            chartArea1.CursorX.IsUserSelectionEnabled = true;
            chartArea1.Name = "ChartArea1";
            this.candleChart.ChartAreas.Add(chartArea1);
            this.candleChart.Location = new System.Drawing.Point(12, 39);
            this.candleChart.Name = "candleChart";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Candlestick;
            series1.Name = "StockCandle";
            series1.YValuesPerPoint = 4;
            this.candleChart.Series.Add(series1);
            this.candleChart.Size = new System.Drawing.Size(776, 399);
            this.candleChart.TabIndex = 0;
            this.candleChart.Text = "chart1";
            // 
            // chartItemCodeTextBox
            // 
            this.chartItemCodeTextBox.Location = new System.Drawing.Point(13, 13);
            this.chartItemCodeTextBox.Name = "chartItemCodeTextBox";
            this.chartItemCodeTextBox.Size = new System.Drawing.Size(100, 21);
            this.chartItemCodeTextBox.TabIndex = 1;
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
            // Form3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.ChartRequestBtn);
            this.Controls.Add(this.chartItemCodeTextBox);
            this.Controls.Add(this.candleChart);
            this.Name = "Form3";
            this.Text = "Form3";
            ((System.ComponentModel.ISupportInitialize)(this.candleChart)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart candleChart;
        private System.Windows.Forms.TextBox chartItemCodeTextBox;
        private System.Windows.Forms.Button ChartRequestBtn;
    }
}