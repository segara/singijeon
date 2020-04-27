using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singijeon.Item
{
    public class BalanceItem
    {
        public string itemCode;
        public string itemName;
        public bool bSell = false;
        public int buyingPrice;
        public int curPrice;
        public int balanceQnt;

        public BalanceItem(string _itemCode, string _itemName, int _buyingPrice, int _balanceQnt)
        {
            this.itemCode = _itemCode.Trim().Replace("A", "");
            this.itemName = _itemName;
            this.buyingPrice = _buyingPrice;
            this.balanceQnt = _balanceQnt;
            this.bSell = false;
        }
    }
}
