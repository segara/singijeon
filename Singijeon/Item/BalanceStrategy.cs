using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singijeon.Item
{
    public class BalanceStrategy
    {
        public enum BALANCE_STRATEGY_TYPE
        {
            NONE,
            BUY,
            SELL,
        }

        public int listIndex = 0;
        public string orderNum = string.Empty;
        public BALANCE_STRATEGY_TYPE type;
        public string account;
        public string itemCode;
        public int buyingPrice;
        public long curQnt;
        public long sellQnt;
        public long buyQnt;
        public bool isSold = false;

        virtual public void CheckBalanceStrategy(object sender, string itemCode, long c_lPrice, Action func)
        {

        }
    }
}
