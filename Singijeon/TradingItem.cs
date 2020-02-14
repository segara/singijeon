using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singijeon
{
    //
    public class TradingItem 
    {
        public TradingStrategy ts;
        public string buyOrderNum;
        public string sellOrderNum;
        public string itemCode;
        public string itemName;
        public long buyingPrice;
        public int buyingQnt;

        public bool IsSold; //매도주문 여부
        public bool IsCompleteBuying; //매수완료 여부

        public TradingItem(TradingStrategy tsItem, string itemCode, long buyingPrice, int buyingQnt, bool completeBuying = false, bool sold = false)
        {
            this.ts = tsItem;
            this.itemCode = itemCode;
            this.buyingPrice = buyingPrice;
            this.buyingQnt = buyingQnt;
            this.IsCompleteBuying = false;
            this.IsSold = false;

            this.buyOrderNum = string.Empty;
            this.sellOrderNum = string.Empty;
        }
    }
}