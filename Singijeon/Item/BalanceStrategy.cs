using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singijeon.Item
{
    [Serializable]
    public class BalanceStrategy
    {
        public enum BALANCE_STRATEGY_TYPE
        {
            NONE,
            BUY,
            SELL,
        }
        public bool bUseStrategy = true;
        public int listIndex = 0;
        public string orderNum = string.Empty;
        public BALANCE_STRATEGY_TYPE type;
        public TRADING_ITEM_STATE state = TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_COMPLETE;
        public string account;
        public string itemCode;
        public int buyingPrice;
        public long curQnt;
        public long sellQnt;
        public long buyQnt;
        public bool isSold = false;
        public TradingItem tradingItem;

        public bool usingTakeProfit = false; //익절사용여부
        public bool usingStoploss = false;   //손절사용여부
        public double takeProfitRate = 0; //익절률
        public double stoplossRate = 0; //손절률
        public string profitOrderOption; //현재가 or 시장가 등
        public string stoplossOrderOption; //현재가 or 시장가 등
        public string divideStoplossOrderOption; //현재가 or 시장가 등

        virtual public void CheckBalanceStrategy(object sender, string itemCode, long c_lPrice, Action func)
        {

        }

        public void SetTradingItem(TradingItem _tradingItem)
        {
            tradingItem = _tradingItem;
        }

    }
}
