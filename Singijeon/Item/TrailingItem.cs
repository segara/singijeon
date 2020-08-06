using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Singijeon
{
    [Serializable]
    public class TrailingItem
    {
        public string itemCode;
        public TradingStrategy strategy;
        public int settingTickCount = 0;
        public int curTickCount = 0;
        public bool isTrailing = true;
        public int firstPrice = 0;
        public int averagePrice = 0;
        public int sumPriceAllTick = 0; //평균가 계산을 위한 변수
        public int percentageCheckPrice = 0;
        public int gapTrailBuyCheckPrice = 0;
        public long itemInvestment = 0;
        public bool isPercentageCheckBuy = false;  
        public bool isGapTrailBuy = false;   //갭상승시 매수
        public bool isVwmaCheck = false;
        public bool isEnvelopeCheck = false;
        public DateTime gapTrailBuyCheckDateTime = DateTime.Now;
        public long gapTrailBuyCheckTimeSecond = 0;
        public string buyOrderOption; //주문 호가 옵션
        public DateTime envelopeBuyCheckDateTime = DateTime.Now;
        //public string sellOrderOption; //주문 호가 옵션
        //public CheckMaUp ma_data_info = null;

        public TickBongInfoMgr tickBongInfoMgr = null;

        [NonSerialized]
        public DataGridViewRow ui_rowAutoTradingItem;

        public TrailingItem()
        {

        }
        public TrailingItem(string itemcode, int _firstPrice, TradingStrategy inputStrategy)
        {
            itemCode = itemcode;
            strategy = inputStrategy;
            firstPrice = _firstPrice;
            settingTickCount = strategy.trailTickValue;
            tickBongInfoMgr = new TickBongInfoMgr(settingTickCount);

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
            isEnvelopeCheck = inputStrategy.usingEnvelope4;
        }

       
    }
}
