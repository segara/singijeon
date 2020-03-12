using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;
using System.Windows.Forms.DataVisualization.Charting;

namespace Singijeon
{
    public partial class Form3 : Form
    {
        AxKHOpenAPILib.AxKHOpenAPI axKHOpenAPI1;

        public Form3(AxKHOpenAPILib.AxKHOpenAPI _axKHOpenAPI1)
        {
            axKHOpenAPI1 = _axKHOpenAPI1;
            InitializeComponent();

            candleChart.Series["StockCandle"].CustomProperties = "PriceDownColor=Blue,PriceUpColor=Red";

            axKHOpenAPI1.OnReceiveTrData += AxKHOpenAPI_OnReceiveTrData;

            candleChart.AxisViewChanged += Chart_AxisViewChanged;
        }

        private void Chart_AxisViewChanged(object sender, ViewEventArgs e)
        {
            int startPosition = (int)e.Axis.ScaleView.ViewMinimum;
            int endPosition = (int)e.Axis.ScaleView.ViewMaximum;

            double yMinValue = double.MaxValue;
            double yMaxValue = double.MinValue;
            for(int i = startPosition; i < endPosition;i++)
            {
                Series s = candleChart.Series["StockCandle"];
                if(i < s.Points.Count)
                {
                    yMaxValue = Math.Max(yMaxValue, s.Points[i].YValues[0]);
                    yMinValue = Math.Min(yMinValue, s.Points[i].YValues[1]);
                }
             
            }
            candleChart.ChartAreas["ChartArea1"].AxisY.Maximum = yMaxValue;
            candleChart.ChartAreas["ChartArea1"].AxisY.Minimum = yMinValue;
        }

        public void AxKHOpenAPI_OnReceiveTrData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            if(e.sRQName.Equals("분봉차트조회"))
            {
                candleChart.Series["StockCandle"].Points.Clear();

                int count = axKHOpenAPI1.GetRepeatCnt(e.sTrCode, e.sRQName);
                for(int i = 0; i < count; ++i)
                {
                    long curPrice = Math.Abs(long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "현재가")));
                    long openPrice = Math.Abs(long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "시가")));
                    long highPrice = Math.Abs(long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "고가")));
                    long lowPrice = Math.Abs(long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "저가")));
 
                    //long curVol = long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "거래량"));

                    string conclusionTime = (axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "체결시간")).Trim();

                    int index = candleChart.Series["StockCandle"].Points.AddXY(conclusionTime, highPrice);
                    candleChart.Series["StockCandle"].Points[index].YValues[1] = lowPrice;
                    candleChart.Series["StockCandle"].Points[index].YValues[2] = openPrice;
                    candleChart.Series["StockCandle"].Points[index].YValues[3] = curPrice; //종가 == 현재가

                    if(openPrice < curPrice)
                    {
                        candleChart.Series["StockCandle"].Points[index].Color = Color.Red;
                        candleChart.Series["StockCandle"].Points[index].BorderColor = Color.Red;
                    }
                    else
                    {
                        candleChart.Series["StockCandle"].Points[index].Color = Color.Blue;
                        candleChart.Series["StockCandle"].Points[index].BorderColor = Color.Blue;
                    }
                }
            }
        }
        private void ChartRequestBtn_Click(object sender, EventArgs e)
        {
            if (axKHOpenAPI1.GetConnectState() == 1)
            {
                string itemCode = chartItemCodeTextBox.Text;
                if(!string.IsNullOrEmpty(itemCode))
                {
                    axKHOpenAPI1.SetInputValue("종목코드", itemCode);
                    axKHOpenAPI1.SetInputValue("틱범위", "3");
                    axKHOpenAPI1.SetInputValue("수정주가구분", "0");
                    axKHOpenAPI1.CommRqData("분봉차트조회", "opt10080", 0, "1080");

                }
            }
            else
                MessageBox.Show("로그인해주세요");
        }
    }
}
