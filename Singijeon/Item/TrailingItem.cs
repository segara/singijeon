using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Singijeon
{
    public class TrailingItem
    {
        public Stack<TickBongInfo> tickBongInfo = new Stack<TickBongInfo>();
        public TickBongInfo curTickBong = null;
        public string itemCode;
        public TradingStrategy strategy;
        public int settingTickCount = 0;
        public int curTickCount = 0;
        public bool isTrailing = true;
        public int lowestPrice = 0;
        public int averagePrice = 0;
        public int sumPriceAllTick = 0; //평균가 계산을 위한 변수
        public int percentageCheckPrice = 0;
        public int gapTrailBuyCheckPrice = 0;
        public long itemInvestment = 0;
        public bool isPercentageCheckBuy = false;   //갭상승시 매수
        public bool isGapTrailBuy = false;   //갭상승시 매수
        public bool isVwmaCheck = false;   //갭상승시 매수
        public DateTime gapTrailBuyCheckDateTime = DateTime.Now;
        public long gapTrailBuyCheckTimeSecond = 0;
        public string buyOrderOption; //주문 호가 옵션
        //public string sellOrderOption; //주문 호가 옵션
        public DataGridViewRow ui_rowAutoTradingItem;
        public CheckMaUp ma_data_info = null;

        public TrailingItem(string itemcode, int firstPrice, TradingStrategy inputStrategy)
        {
            itemCode = itemcode;
            strategy = inputStrategy;
            lowestPrice = firstPrice;
            settingTickCount = strategy.trailTickValue;
            buyOrderOption = inputStrategy.buyOrderOption;

            //sellOrderOption = inputStrategy.sellOrderOption;

            itemInvestment = inputStrategy.itemInvestment;
            if (inputStrategy.usingPercentageBuy)
            {
                percentageCheckPrice = (int)((float)firstPrice * (100.0f - inputStrategy.percentageBuyValue) * 0.01f);
                isPercentageCheckBuy = true;
            }
            if (inputStrategy.usingGapTrailBuy)
            {
                isGapTrailBuy = true;
                gapTrailBuyCheckPrice = firstPrice; //등장 시점의가격
                gapTrailBuyCheckDateTime = DateTime.Now;
                gapTrailBuyCheckTimeSecond = inputStrategy.gapTrailBuyTimeValue;
            }
            isVwmaCheck = inputStrategy.usingVwma; 
        }

        public TickBongInfo GetTickBong(int index)
        {
            int idx = 0;
            TickBongInfo bong = null;
            foreach (var bongItem in tickBongInfo)
            {
                if (idx == index && bongItem.IsComplete())
                {
                    bong = bongItem;
                    break;
                }
                idx++;
            }
            return bong;
        }
    }
}
