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
using Singijeon.Core;

namespace Singijeon
{
    class MA_5 : SingletonBase<MA_5>
    {
        const int MAX_CANDLE = 200;
        const int MA_PERIOD = 20;
        const double MA_4_PERCENT = 0.96;
        const double MA_5_PERCENT = 0.95;

        AxKHOpenAPILib.AxKHOpenAPI axKHOpenAPI1;
      
        string screenNumber = string.Empty;
        List<long> priceList = new List<long>();

        List<long> priceMA_List = new List<long>();
        List<long> priceMA_Envelope_List = new List<long>();

        ReceiveAfter afterEventFunction = null;
        public delegate void ReceiveAfter(string itemCode, long curPrice, long envelopePrice);
        bool init = false;

        public void Init(AxKHOpenAPILib.AxKHOpenAPI _axKHOpenAPI1)
        {
            axKHOpenAPI1 = _axKHOpenAPI1;
            screenNumber = Form1.GetScreenNum().ToString();
            axKHOpenAPI1.OnReceiveTrData += AxKHOpenAPI_OnReceiveTrData;
            init = true;
        }

        public void RequestItem(string ItemCode, ReceiveAfter delFunc)
        {
            if (!init)
                return;
            afterEventFunction = delFunc;

            Task requestItemInfoTaskMinute = new Task(() =>
            {
                axKHOpenAPI1.SetInputValue("종목코드", ItemCode);
                axKHOpenAPI1.SetInputValue("틱범위", "5");
                axKHOpenAPI1.SetInputValue("수정주가구분", "0");
                int result = axKHOpenAPI1.CommRqData(ConstName.RECEIVE_TR_DATA_MINUTE_CHART + ":" + screenNumber, "opt10080", 0, "1080");
                if (result != ErrorCode.정상처리)
                {
                    Core.CoreEngine.GetInstance().SendLogErrorMessage("ERROR : " + result.ToString());
                }
            });
            Core.CoreEngine.GetInstance().requestTrDataManager.RequestTrData(requestItemInfoTaskMinute);
        }

        public void AxKHOpenAPI_OnReceiveTrData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            if (!init)
                return;

            if (e.sRQName.Contains(ConstName.RECEIVE_TR_DATA_MINUTE_CHART))
            {
                string[] strArray = e.sRQName.Split(':');
                string itemcode = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "종목코드").Replace("A", "").Trim();

                if (strArray.Length == 2)
                {
                    if (strArray[1] != screenNumber)
                        return;
                }

                priceList.Clear();
                priceMA_List.Clear();
                priceMA_Envelope_List.Clear();

                int count = axKHOpenAPI1.GetRepeatCnt(e.sTrCode, e.sRQName);
                count = Math.Min(count, MAX_CANDLE);

                for (int i = 0; i < count; ++i)
                {
                    long curPrice = Math.Abs(long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "현재가")));
                    long openPrice = Math.Abs(long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "시가")));
                    long highPrice = Math.Abs(long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "고가")));
                    long lowPrice = Math.Abs(long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "저가")));

                    long curVol = long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "거래량"));
                    string conclusionTime = (axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "체결시간")).Trim();
                    string format = "yyyyMMddHHmmss";

                    DateTime concludeTime = DateTime.ParseExact(conclusionTime, format, CultureInfo.InvariantCulture);
                 
                    priceList.Add(curPrice);
                }

                for (int i = 0; i < priceList.Count; ++i)
                {
                    if (i + MA_PERIOD < priceList.Count)
                    {
                        long priceSum = 0;
                        for (int j = 0; j < MA_PERIOD; ++j)
                        {
                            priceSum += (long)priceList[i + j]; 
                        }
                        double priceAverage = priceSum / MA_PERIOD;
                        priceMA_List.Add((long)priceAverage);
                        priceMA_Envelope_List.Add((long)(priceAverage * MA_5_PERCENT));
                    }
                }

                if (afterEventFunction != null && priceList.Count >  0 && priceMA_Envelope_List.Count > 0)
                    afterEventFunction.Invoke(itemcode, priceList[0], priceMA_Envelope_List[0]);
            }
        }
    }
}
