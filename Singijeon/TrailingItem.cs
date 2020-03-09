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

        public bool isPercentageCheckBuy = false;   //갭상승시 매수
        public bool isGapTrailBuy = false;   //갭상승시 매수
        public DateTime gapTrailBuyCheckDateTime = DateTime.Now;
        public long gapTrailBuyCheckTimeSecond = 0;
        public string buyOrderOption; //주문 호가 옵션

        public DataGridViewRow ui_rowAutoTradingItem;

        public TrailingItem(string itemcode, int firstPrice, TradingStrategy inputStrategy)
        {
            itemCode = itemcode;
            strategy = inputStrategy;  
            lowestPrice = firstPrice;
            settingTickCount = strategy.trailTickValue;
            buyOrderOption = inputStrategy.buyOrderOption;

            if (inputStrategy.usingPercentageBuy)
            {
                percentageCheckPrice = (int)((float)firstPrice * (100.0f + inputStrategy.percentageBuyValue) * 0.01f);
                isPercentageCheckBuy = true;
            }
            if (inputStrategy.usingGapTrailBuy)
            {
                isGapTrailBuy = true;
                gapTrailBuyCheckPrice = firstPrice; //등장 시점의가격
                gapTrailBuyCheckDateTime = DateTime.Now;
                gapTrailBuyCheckTimeSecond = inputStrategy.gapTrailBuyTimeValue;
            }
           
        }
    }
}
