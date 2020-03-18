using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singijeon
{
    [Serializable]
    public class TradingStrategyForSave
    {
        public string account;

        public string sellProfitOrderOption; //현재가 or 시장가 등
        public string sellStopLossOrderOption; //현재가 or 시장가 등

        //public bool usingTakeProfit = false; //익절사용여부
        //public bool usingStoploss = false;   //손절사용여부

        public double takeProfitRate = 0; //익절률
        public double stoplossRate = 0; //손절률
        
        public List<TradingItem> tradingItemList = new List<TradingItem>();

        public TradingStrategyForSave(TradingStrategy strategy)
        {
            sellProfitOrderOption = strategy.sellProfitOrderOption;
            sellStopLossOrderOption = strategy.sellStopLossOrderOption;

            //usingTakeProfit = strategy.usingTakeProfit;
            //usingStoploss = strategy.usingStoploss;
            account = strategy.account;
            takeProfitRate = strategy.takeProfitRate;
            stoplossRate = strategy.stoplossRate;

            tradingItemList = strategy.tradingItemList;
        }
    }
}
