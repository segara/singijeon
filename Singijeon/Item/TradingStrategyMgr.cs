using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singijeon
{
    class TradingStrategyMgr
    {
        private static TradingStrategyMgr instance;

        public List<TradingStrategy> tradingStrategyList = new List<TradingStrategy>();

        private TradingStrategyMgr()
        {

        }

        public static TradingStrategyMgr GetInstance()
        {
            if (instance == null)
            {
                instance = new TradingStrategyMgr();
            }
            return instance;
        }
    }
}
