using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        [NonSerialized]
        public DataGridViewRow ui_rowItem;
        public BalanceItem(string _itemCode, string _itemName, int _buyingPrice, int _balanceQnt, DataGridViewRow ui_item)
        {
            this.itemCode = _itemCode.Trim().Replace("A", "");
            this.itemName = _itemName;
            this.buyingPrice = _buyingPrice;
            this.balanceQnt = _balanceQnt;
            this.bSell = false;
            this.ui_rowItem = ui_item;
        }
    }
}
