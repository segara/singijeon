using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace Singijeon
{
       
    public class TradingItem 
    {
        public TradingStrategy ts;
        public string buyOrderNum;
        public string sellOrderNum;
        public string buyCancelOrderNum;
        public string sellCancelOrderNum;
        public string orderType;
        public string itemCode;
        public string itemName;
        public long buyingPrice;
        public long sellPrice;
        public int buyingQnt;
        public int sellQnt;
        public int trailingTickCnt;
        public int outStandingQnt;

        public long curPrice;

        public bool IsSold; //매도주문 여부
        public bool IsBuyCancel; //매도주문 여부
        public bool IsSellCancel; //매도주문 여부
        public bool IsCompleteBuying; //매수완료 여부

        public DataGridViewRow ui_rowItem;
        public string conditionUid;
        public string Uid { get; set; } 
        public TradingItem(TradingStrategy tsItem, string itemCode, long buyingPrice, int buyingQnt, bool completeBuying = false, bool sold = false, string orderType = "")
        {
            this.ts = tsItem;
            this.itemCode = itemCode;
            this.buyingPrice = buyingPrice;
            this.buyingQnt = buyingQnt;
            this.outStandingQnt = buyingQnt;
            this.IsCompleteBuying = false;
            this.IsSold = false;
            this.IsBuyCancel = false;
            this.IsSellCancel = false;
            this.buyOrderNum = string.Empty;
            this.sellOrderNum = string.Empty;

            this.orderType = orderType;

            this.Uid = System.Guid.NewGuid().ToString();
        }
        public void UpdateCurrentPrice(long _price)
        {
            this.curPrice = _price;
        }
        public void SetUiConnectRow(DataGridViewRow row)         
        {
            this.ui_rowItem = row;
        }
        public void SetConditonUid(string uid)
        {
            this.conditionUid = uid;
        }
        public void SetOutStanding(int qnt)
        {
            this.outStandingQnt = qnt;
        }
    }
}