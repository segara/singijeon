using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singijeon
{
    public class MaSeries
    {
        public string time;
        public long average1;
        public long curPrice;
        public MaSeries(string _time, long _average1, long _curPrice)
        {
            time = _time;
            average1 = _average1;
            curPrice = _curPrice;
        }
    }
    public class CustomSeries
    {
        public string time;

        public long highPrice;
        public long lowPrice;
        public long openPrice;
        public long curPrice;

        long volume;

        public CustomSeries(string _time, long _highPrice, long _lowPrice, long _openPrice, long _curPrice, long _volume)
        {
            time = _time;
            highPrice = _highPrice;
            lowPrice = _lowPrice;
            openPrice = _openPrice;
            curPrice = _curPrice;

            volume = _volume;
        }
    }

    public class CheckMaUp
    {
        public enum MA_STATE
        {
            NONE,
            GOLDEN_CROSS,
            UP_STAY,
            DEAD_CROSS,
            DOWN_STAY,
        }

        AxKHOpenAPILib.AxKHOpenAPI axKHOpenAPI1;
        List<CustomSeries> allSeries = new List<CustomSeries>();
        Queue<MaSeries> maSeries = new Queue<MaSeries>();
        const int MA_PERIOD = 20;
        public const int MAX_TICK = 100;

        MA_STATE maState = MA_STATE.NONE;

        public CheckMaUp(AxKHOpenAPILib.AxKHOpenAPI _axKHOpenAPI1)
        {
            axKHOpenAPI1 = _axKHOpenAPI1;
            axKHOpenAPI1.OnReceiveTrData += AxKHOpenAPI_OnReceiveTrData;
        }

        public void ReqChartData(string itemCode)
        {
            if (axKHOpenAPI1.GetConnectState() == 1)
            {
                if (!string.IsNullOrEmpty(itemCode))
                {
                    Task requestItemInfoTask = new Task(() =>
                    {
                        axKHOpenAPI1.SetInputValue("종목코드", itemCode);
                        axKHOpenAPI1.SetInputValue("틱범위", "30");
                        axKHOpenAPI1.SetInputValue("수정주가구분", "0");
                        axKHOpenAPI1.CommRqData("틱데이터차트조회", "opt10079", 0, "1080");
                    });
                    Core.CoreEngine.GetInstance().requestTrDataManager.RequestTrData(requestItemInfoTask);
                }
            }
        }

        public void AxKHOpenAPI_OnReceiveTrData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            if (e.sRQName.Equals("틱데이터차트조회"))
            {
                allSeries.Clear();
             
                int count = axKHOpenAPI1.GetRepeatCnt(e.sTrCode, e.sRQName);
                count = Math.Min(count, MAX_TICK);
                for (int i = 0; i < count; ++i)
                {
                    long curPrice = Math.Abs(long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "현재가")));
                    long openPrice = Math.Abs(long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "시가")));
                    long highPrice = Math.Abs(long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "고가")));
                    long lowPrice = Math.Abs(long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "저가")));

                    long curVol = long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "거래량"));

                    string conclusionTime = (axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "체결시간")).Trim();

                    CustomSeries series = new CustomSeries(conclusionTime, highPrice, lowPrice, openPrice, curPrice, curVol);

                    allSeries.Add(series);
                    
                }
                MA_Calculate();
            }
        }

        public void MA_Calculate()
        {
            maSeries.Clear();
            allSeries.Reverse();

            for (int i = 0; i < allSeries.Count; ++i)
            {
                if (i + MA_PERIOD < allSeries.Count)
                {
                    long priceSum = 0;
                    long price = 0;
                    string curTime = "";
                    curTime = allSeries[i].time;
                    price = allSeries[i].curPrice;

                    for (int j = 0; j < MA_PERIOD; ++j)
                    { 
                        priceSum += allSeries[i + j].curPrice; 
                    }
                    double priceAverage = priceSum / MA_PERIOD;
                    MaSeries maSeriesItem = new MaSeries(curTime, (long)priceAverage, price);
                    maSeries.Enqueue(maSeriesItem);
                }
            }
        }

        public MaSeries GetLastItem()
        {
            if (maSeries.Count == 0)
                return null;
            return maSeries.Peek();
        }
    }
}
