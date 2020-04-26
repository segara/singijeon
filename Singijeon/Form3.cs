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
using System.Globalization;

namespace Singijeon
{
    public partial class Form3 : Form
    {
        public enum CHART_TYPE
        {
           NONE,
           TICK_30,
           MINUTE_5,
        }

        public enum VWMA_CHART_STATE
        {
            NONE,
            GOLDEN_CROSS,
            UP_STAY,
            DEAD_CROSS,
            DOWN_STAY,
        }

        AxKHOpenAPILib.AxKHOpenAPI axKHOpenAPI1;

        Series maSeries;

        Series maSeries_EvelopeDown;
        Series maSeriesShort;
        Series vwmaSeriesShort;
        string itemcode;
        const int MA_PERIOD_SHORT = 5;
        const int MA_PERIOD = 20;
        const int MAX_CANDLE = 200;
        //public VWMA_CHART_STATE vwma_state_old = VWMA_CHART_STATE.NONE;
        public VWMA_CHART_STATE vwma_state = VWMA_CHART_STATE.NONE;
        public double gapPercent = 0;
        public double VPCI = 0;

        double evelopeValue = 0.96;
        const double MA_4_PERCENT = 0.96;
        const double MA_3_PERCENT = 0.97;

        CHART_TYPE curTypeChart = CHART_TYPE.MINUTE_5;

        public Button btn { get { return ChartRequestBtn; } }

        public Form3(AxKHOpenAPILib.AxKHOpenAPI _axKHOpenAPI1)
        {
            axKHOpenAPI1 = _axKHOpenAPI1;
            InitializeComponent();

            candleChart.Series["StockCandle"].CustomProperties = "PriceDownColor=Blue,PriceUpColor=Red";

            maSeries = new Series("SMA");
            maSeries.ChartType = SeriesChartType.Line;

            maSeries_EvelopeDown = new Series("SMA_DOWN");
            maSeries_EvelopeDown.ChartType = SeriesChartType.Line;
            maSeries_EvelopeDown.Color = Color.Black;

            maSeriesShort = new Series("SMA_SHORT");
            maSeriesShort.ChartType = SeriesChartType.Line;

            vwmaSeriesShort = new Series("VWMA_SHORT");
            vwmaSeriesShort.ChartType = SeriesChartType.Line;

            axKHOpenAPI1.OnReceiveTrData += AxKHOpenAPI_OnReceiveTrData;
            candleChart.AxisViewChanged += Chart_AxisViewChanged;

            this.FormClosing += Form_FormClosing;
        }

        private void Form_FormClosing(object sender, EventArgs e)
        {
            axKHOpenAPI1.OnReceiveTrData -= AxKHOpenAPI_OnReceiveTrData;
        }

        private void Chart_AxisViewChanged(object sender, ViewEventArgs e)
        {
            int startPosition = (int)e.Axis.ScaleView.ViewMinimum;
            int endPosition = (int)e.Axis.ScaleView.ViewMaximum;

            double PriceYMinValue = double.MaxValue;
            double PriceYMaxValue = double.MinValue;

            double VolumeYMinValue = double.MaxValue;
            double VolumeYMaxValue = double.MinValue;

            double VwmaYMinValue = double.MaxValue;
            double VwmaYMaxValue = double.MinValue;

            for (int i = startPosition; i < endPosition;i++)
            {
                Series sPrice = candleChart.Series["StockCandle"];
                Series sVolume = candleChart.Series["Volume"];
                Series sVwma = candleChart.Series["VWMA"];

                if (i < sPrice.Points.Count)
                {
                    PriceYMaxValue = Math.Max(PriceYMaxValue, sPrice.Points[i].YValues[0]);
                    PriceYMinValue = Math.Min(PriceYMinValue, sPrice.Points[i].YValues[1]);
                }
                if (i < sVolume.Points.Count)
                {
                    VolumeYMaxValue = Math.Max(VolumeYMaxValue, sVolume.Points[i].YValues[0]);
                    VolumeYMinValue = Math.Min(VolumeYMinValue, sVolume.Points[i].YValues[0]);
                }
                if (i < sVwma.Points.Count && sVwma.Points[i].YValues.Length > 0)
                {
                    
                    VwmaYMaxValue = Math.Max(VwmaYMaxValue, sVwma.Points[i].YValues[0]);
                    VwmaYMinValue = Math.Min(VwmaYMinValue, sVwma.Points[i].YValues[0]);
                }
            }
            candleChart.ChartAreas["PriceChartArea"].AxisY.Maximum = PriceYMaxValue;
            candleChart.ChartAreas["PriceChartArea"].AxisY.Minimum = PriceYMinValue;

            candleChart.ChartAreas["VolumeChartArea"].AxisY.Maximum = VolumeYMaxValue;

            candleChart.ChartAreas["VwmaChartArea"].AxisY.Maximum = VwmaYMaxValue;
            candleChart.ChartAreas["VwmaChartArea"].AxisY.Minimum = VwmaYMinValue;

            //candleChart.ChartAreas["VolumeChartArea"].AxisY.Minimum = VolumeYMinValue;
        }

        public void AxKHOpenAPI_OnReceiveTrData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            if(e.sRQName.Equals(ConstName.RECEIVE_TR_DATA_KOSPI_MINUTE_CHART) || e.sRQName.Equals(ConstName.RECEIVE_TR_DATA_TICK_CHART) || e.sRQName.Equals(ConstName.RECEIVE_TR_DATA_MINUTE_CHART))
            {
                if (candleChart.Series == null)
                    return;
                
                if (e.sRQName.Equals(ConstName.RECEIVE_TR_DATA_KOSPI_MINUTE_CHART) == false)
                {
                    itemcode = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "종목코드").Replace("A", "").Trim();
                    if (chartItemCodeTextBox.Text != itemcode)
                        return;
                }

                candleChart.Series["StockCandle"].Points.Clear();
                candleChart.Series["Volume"].Points.Clear();

                candleChart.Series["VWMA"].Points.Clear();
                candleChart.Series["VPCI"].Points.Clear();

                int count = axKHOpenAPI1.GetRepeatCnt(e.sTrCode, e.sRQName);
                count = Math.Min(count, MAX_CANDLE);
                for(int i = 0; i < count; ++i)
                {
                   
                    long curPrice = Math.Abs(long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "현재가")));
                    long openPrice = Math.Abs(long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "시가")));
                    long highPrice = Math.Abs(long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "고가")));
                    long lowPrice = Math.Abs(long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "저가")));
 
                    long curVol = long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "거래량"));

                    string conclusionTime = (axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "체결시간")).Trim();

                    string format = "yyyyMMddHHmmss";

                    int index = candleChart.Series["StockCandle"].Points.AddXY(DateTime.ParseExact(conclusionTime,format,CultureInfo.InvariantCulture).ToString("HH:mm:ss"), highPrice);

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

                    int volume_index = candleChart.Series["Volume"].Points.AddXY(DateTime.ParseExact(conclusionTime, format, CultureInfo.InvariantCulture).ToString("HH:mm:ss"), curVol);

                    candleChart.Series["VWMA"].Points.AddXY(DateTime.ParseExact(conclusionTime, format, CultureInfo.InvariantCulture).ToString("HH:mm:ss"), 0);
                    candleChart.Series["VPCI"].Points.AddXY(DateTime.ParseExact(conclusionTime, format, CultureInfo.InvariantCulture).ToString("HH:mm:ss"), 0);

                }
                MakeMA();
                MakeVWMA();
                MakeVPCI();
            }
        }
        public void ChartRequestBtn_Click(object sender, EventArgs e)
        {
            if (axKHOpenAPI1.GetConnectState() == 1)
            {
                string itemCode = chartItemCodeTextBox.Text;
                RequestItem(itemCode, null, curTypeChart);
            }
            else
                MessageBox.Show("로그인해주세요");
        }

        ReceiveAfter afterEventFunction = null;
        public delegate void ReceiveAfter(string itemCode);

        public void KospiChartRequestBtn_Click(object sender, EventArgs e)
        {
            RequestKospi(null, CHART_TYPE.MINUTE_5);
        }
        public void RequestKospi(ReceiveAfter delFunc, CHART_TYPE typeChart = CHART_TYPE.MINUTE_5)
        {
            afterEventFunction = delFunc;
            ItemName.Text = "코스피지수";
            Task requestItemInfoTask = new Task(() =>
            {
                axKHOpenAPI1.SetInputValue("업종코드", "001");
                axKHOpenAPI1.SetInputValue("틱범위", "5:5분");
                axKHOpenAPI1.SetInputValue("수정주가구분", "1");

                int result = axKHOpenAPI1.CommRqData(ConstName.RECEIVE_TR_DATA_KOSPI_MINUTE_CHART, "OPT20005", 0, "1080");
                if (result != ErrorCode.정상처리)
                {
                    Core.CoreEngine.GetInstance().SendLogErrorMessage("ERROR : " + result.ToString());
                }
            });
            Core.CoreEngine.GetInstance().requestTrDataLoopManager.RequestTrData(requestItemInfoTask);
         
        }
        public void RequestItem(string ItemCode, ReceiveAfter delFunc, CHART_TYPE typeChart = CHART_TYPE.TICK_30)
        {
            if (!string.IsNullOrEmpty(ItemCode))
            {
                afterEventFunction = delFunc;
                chartItemCodeTextBox.Text = ItemCode;
                ItemName.Text = axKHOpenAPI1.GetMasterCodeName(ItemCode);

                switch (typeChart)
                {
                    case  CHART_TYPE.TICK_30:
                        Task requestItemInfoTask = new Task(() =>
                        {
                            axKHOpenAPI1.SetInputValue("종목코드", ItemCode);
                            axKHOpenAPI1.SetInputValue("틱범위", "30");
                            axKHOpenAPI1.SetInputValue("수정주가구분", "0");
                            int result = axKHOpenAPI1.CommRqData(ConstName.RECEIVE_TR_DATA_TICK_CHART, "opt10079", 0, "1080");
                            if (result != ErrorCode.정상처리)
                            {
                                Core.CoreEngine.GetInstance().SendLogErrorMessage("ERROR : " + result.ToString());
                            }
                        });
                        Core.CoreEngine.GetInstance().requestTrDataManager.RequestTrData(requestItemInfoTask);
                        break;
                    case CHART_TYPE.MINUTE_5:
                        Task requestItemInfoTaskMinute = new Task(() =>
                        {
                            axKHOpenAPI1.SetInputValue("종목코드", ItemCode);
                            axKHOpenAPI1.SetInputValue("틱범위", "5");
                            axKHOpenAPI1.SetInputValue("수정주가구분", "0");
                            int result = axKHOpenAPI1.CommRqData(ConstName.RECEIVE_TR_DATA_MINUTE_CHART, "opt10080", 0, "1080");
                            if (result != ErrorCode.정상처리)
                            {
                                Core.CoreEngine.GetInstance().SendLogErrorMessage("ERROR : " + result.ToString());
                            }
                        });
                        Core.CoreEngine.GetInstance().requestTrDataManager.RequestTrData(requestItemInfoTaskMinute);
                        break;

                    default:
                        break;
                }
              
            }
        }
   
        private void MakeMA()
        {
           
            Series priceSeries = candleChart.Series["StockCandle"];
            maSeries.Points.Clear();
      
            maSeries_EvelopeDown.Points.Clear();
            maSeriesShort.Points.Clear();
            for (int i = 0; i < priceSeries.Points.Count; ++i)
            {
                if(i+MA_PERIOD < priceSeries.Points.Count)
                {
                    long priceSum = 0;
                    for (int j = 0; j < MA_PERIOD; ++j)
                    {
                        priceSum += (long)priceSeries.Points[i + j].YValues[3]; //3번째가 현재가
                    }
                    double priceAverage = priceSum / MA_PERIOD;
                    maSeries.Points.AddXY(priceSeries.Points[i].XValue, priceAverage);
                    maSeries_EvelopeDown.Points.AddXY(priceSeries.Points[i].XValue, priceAverage * evelopeValue);
                }
                if (i + MA_PERIOD_SHORT < priceSeries.Points.Count)
                {
                    long priceSum = 0;
                    for (int j = 0; j < MA_PERIOD_SHORT; ++j)
                    {
                        priceSum += (long)priceSeries.Points[i + j].YValues[3]; //3번째가 현재가
                    }
                    double priceAverage = priceSum / MA_PERIOD_SHORT;
                    maSeriesShort.Points.AddXY(priceSeries.Points[i].XValue, priceAverage);
                }
            }
            if (candleChart.Series.Contains(maSeries) == false)
                candleChart.Series.Add(maSeries);
            else
                candleChart.Series[maSeries.Name] = maSeries;

            maSeries.ChartArea = "PriceChartArea";

            if (candleChart.Series.Contains(maSeries_EvelopeDown) == false)
                candleChart.Series.Add(maSeries_EvelopeDown);
            else
                candleChart.Series[maSeries_EvelopeDown.Name] = maSeries_EvelopeDown;
            maSeries_EvelopeDown.ChartArea = "PriceChartArea";

            if (candleChart.Series.Contains(maSeriesShort) == false)
                candleChart.Series.Add(maSeriesShort);
           
            maSeriesShort.ChartArea = "PriceChartArea";

        }

        public void MakeVWMA()
        {
            Series priceSeries = candleChart.Series["StockCandle"];
            Series volumeSeries = candleChart.Series["Volume"];
            Series vwmaSeries = candleChart.Series["VWMA"];

           
            vwmaSeriesShort.Points.Clear();

            double lastValue = 0;
            double lastValueShort = 0;
            double minValue = double.MaxValue;
            double maxValue = double.MinValue;
            for (int i = 0; i < priceSeries.Points.Count; ++i)
            {
                if (i + MA_PERIOD < priceSeries.Points.Count)
                {

                    double priceSum = 0;
                    double volumeSum = 0;
                    double vwma = 0;
                    for (int j = 0; j < MA_PERIOD; ++j)
                    {
                        priceSum += (double)priceSeries.Points[i + j].YValues[3]; //3번째가 현재가
                        volumeSum += (double)volumeSeries.Points[i + j].YValues[0];
                    }
                    for (int j = 0; j < MA_PERIOD; ++j)
                    {
                        double price = (double)priceSeries.Points[i+j].YValues[3];
                        double volume = (double)volumeSeries.Points[i+j].YValues[0];
                        vwma += price * (volume / volumeSum);
                    }
              
                  
                    vwmaSeries.Points[i].YValues[0]  = vwma;
                   
                    maxValue = Math.Max(maxValue, vwma);
                    minValue = Math.Min(minValue, vwma);
                    lastValue = vwma;
                }
                else
                {
                    vwmaSeries.Points[i].YValues[0] = lastValue;
                }

                if (i + MA_PERIOD_SHORT < priceSeries.Points.Count)
                {

                    double priceSum = 0;
                    double volumeSum = 0;
                    double vwma = 0;
                    for (int j = 0; j < MA_PERIOD_SHORT; ++j)
                    {
                        priceSum += (double)priceSeries.Points[i + j].YValues[3]; //3번째가 현재가
                        volumeSum += (double)volumeSeries.Points[i + j].YValues[0];
                    }
                    for (int j = 0; j < MA_PERIOD_SHORT; ++j)
                    {
                        double price = (double)priceSeries.Points[i + j].YValues[3];
                        double volume = (double)volumeSeries.Points[i + j].YValues[0];
                        vwma += price * (volume / volumeSum);
                    }

                    vwmaSeriesShort.Points.AddXY(priceSeries.Points[i].XValue, vwma);
                    lastValueShort = vwma;

                    if (vwmaSeriesShort.Points.Count > 1)
                    {
                        if (vwmaSeriesShort.Points[0].YValues[0] > vwmaSeries.Points[0].YValues[0])
                        {
                            if(vwmaSeriesShort.Points[1].YValues[0] < vwmaSeries.Points[1].YValues[0])
                            {
                                vwma_state = VWMA_CHART_STATE.GOLDEN_CROSS;
                            }
                            else
                            {
                                vwma_state = VWMA_CHART_STATE.UP_STAY;
                            }
                          
                        }
                        else if (vwmaSeriesShort.Points[0].YValues[0] < vwmaSeries.Points[0].YValues[0])
                        {
                            if (vwmaSeriesShort.Points[1].YValues[0] > vwmaSeries.Points[1].YValues[0])
                            {
                               
                                vwma_state = VWMA_CHART_STATE.DEAD_CROSS;
                            }
                            else
                            {
                                if(vwmaSeriesShort.Points[0].YValues[0] > 0)
                                    gapPercent = ((vwmaSeries.Points[0].YValues[0] - vwmaSeriesShort.Points[0].YValues[0]) / vwmaSeriesShort.Points[0].YValues[0])*100.0;
                                vwma_state = VWMA_CHART_STATE.DOWN_STAY;
                            }

                        }
                        StateLabel.Text = vwma_state.ToString();

                    }
                   

                    maxValue = Math.Max(maxValue, vwma);
                    minValue = Math.Min(minValue, vwma);
                }
                else
                {
                    
                    //vwmaSeriesShort.Points[i].YValues[0] = lastValueShort;
                }


            }
            candleChart.ChartAreas["VwmaChartArea"].AxisY.Maximum = maxValue;
            candleChart.ChartAreas["VwmaChartArea"].AxisY.Minimum = minValue;

            if (candleChart.Series.Contains(vwmaSeriesShort) == false)
                candleChart.Series.Add(vwmaSeriesShort);
          

            vwmaSeriesShort.ChartArea = "VwmaChartArea";
        }

        public void MakeVPCI()
        {
            Series priceSeries = candleChart.Series["StockCandle"];
            Series volumeSeries = candleChart.Series["Volume"];
            Series vwmaSeries = candleChart.Series["VWMA"];
            Series vwmaShortSeries = candleChart.Series["VWMA_SHORT"];
            Series vpciSeries = candleChart.Series["VPCI"];

            Series maSeries = candleChart.Series["SMA"];
            Series maShortSeries = candleChart.Series["SMA_SHORT"];

            Series middleSeries = candleChart.Series["Middle"];
            
            middleSeries.Points.Clear();

            double lastValue = 0;
            double minValue = double.MaxValue;
            double maxValue = double.MinValue;

            for (int i = 0; i < priceSeries.Points.Count; ++i)
            {
                if (i + MA_PERIOD < priceSeries.Points.Count)
                {

                    double volumeShortSum = 0;
                    double volumeSum = 0;
                 
                    double vwma = vwmaSeries.Points[i].YValues[0];
                    double sma = maSeries.Points[i].YValues[0];

                    double vpc = vwma - sma;

                    double vwmaShort = vwmaShortSeries.Points[i].YValues[0];
                    double smaShort = maShortSeries.Points[i].YValues[0];

                    double vpr = vwmaShort / smaShort;


                    for (int j = 0; j < MA_PERIOD; ++j)
                    {
                        volumeSum += (double)volumeSeries.Points[i + j].YValues[0];
                    }
                    double volumeSumAverage = volumeSum / MA_PERIOD;

                    for (int j = 0; j < MA_PERIOD_SHORT; ++j)
                    {
                        volumeShortSum += (double)volumeSeries.Points[i + j].YValues[0];
                    }
                    double volumeShortSumAverage = volumeShortSum / MA_PERIOD_SHORT;
                    double vm = volumeShortSumAverage / volumeSumAverage;

                    double vpci = vpc * vpr * vm;
                    VPCI = vpci;
                    vpciSeries.Points[i].YValues[0] = vpci;
                    vpciPoint.Text = vpci.ToString();
                    middleSeries.Points.AddXY(vpciSeries.Points[i].XValue, 0);
                    maxValue = Math.Max(maxValue, vpci);
                    minValue = Math.Min(minValue, vpci);
                    lastValue = vpci;
                }
                else
                {
                    vpciSeries.Points[i].YValues[0] = lastValue;
                }


            }
            candleChart.ChartAreas["VpciChartArea"].AxisY.Maximum = maxValue;
            candleChart.ChartAreas["VpciChartArea"].AxisY.Minimum = minValue;

            if(afterEventFunction != null)
                afterEventFunction.Invoke(itemcode);
        }

        private void Envel4RadioBtn_CheckedChanged(object sender, EventArgs e)
        {
            evelopeValue = MA_4_PERCENT;
        }

        private void Envel3RadioBtn_CheckedChanged(object sender, EventArgs e)
        {
            evelopeValue = MA_3_PERCENT;
        }

        private void Tick_30_tick_CheckedChanged(object sender, EventArgs e)
        {
            curTypeChart = CHART_TYPE.TICK_30;
        }

        private void Tick_5_minute_CheckedChanged(object sender, EventArgs e)
        {
            curTypeChart = CHART_TYPE.MINUTE_5;
        }
    }
}
