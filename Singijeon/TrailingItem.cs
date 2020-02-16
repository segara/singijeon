using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singijeon
{
    public class TrailingItem
    {
        public string itemCode;
        public TradingStrategy strategy;
        public int settingTickCount = 0;
        public int curTickCount = 0;

        public int lowestPrice = 0;

        public TrailingItem(string itemcode, int firstPrice, TradingStrategy inputStrategy)
        {
            itemCode = itemcode;
            strategy = inputStrategy;

            lowestPrice = firstPrice;

            settingTickCount = strategy.trailTickValue;
        }
    }
}
