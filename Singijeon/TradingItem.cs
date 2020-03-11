using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace Singijeon
{
    public enum TRADING_ITEM_STATE : int
    {
        NONE,
        AUTO_TRADING_STATE_SEARCH_AND_CATCH,//종목포착
        AUTO_TRADING_STATE_BUY_BEFORE_ORDER,//매수주문접수시도중
        AUTO_TRADING_STATE_BUY_NOT_COMPLETE,//매수주문완료_체결대기
        AUTO_TRADING_STATE_BUY_NOT_COMPLETE_OUTCOUNT,//매수중_일부매수완료

        AUTO_TRADING_STATE_BUY_CANCEL_NOT_COMPLETE, //매수취소
        AUTO_TRADING_STATE_BUY_CANCEL_COMPLETE, //매수취소

        AUTO_TRADING_STATE_BUY_COMPLETE, //매수완료

        AUTO_TRADING_STATE_SELL_BEFORE_ORDER,//매도주문접수시도
        AUTO_TRADING_STATE_SELL_NOT_COMPLETE, //매도주문완료

        AUTO_TRADING_STATE_SELL_CANCEL_NOT_COMPLETE, //매도취소 접수시도
        //AUTO_TRADING_STATE_SELL_CANCEL_COMPLETE, //매도 취소는 주문완료로 대체

        AUTO_TRADING_STATE_SELL_NOT_COMPLETE_OUTCOUNT, //일부매도
        AUTO_TRADING_STATE_SELL_COMPLETE, //매도완료

    }
    public class TradingItem
    {
        public TradingStrategy ts = null;
        public TRADING_ITEM_STATE state {get{return curState;}}
        private TRADING_ITEM_STATE curState = TRADING_ITEM_STATE.NONE;
        public string buyOrderNum = string.Empty;
        public string sellOrderNum = string.Empty;
        public string buyCancelOrderNum = string.Empty;
        public string sellCancelOrderNum = string.Empty;
        public string buyOrderType = string.Empty;
        public string sellOrderType = string.Empty;
        public string itemCode = string.Empty;
        public string itemName = string.Empty;
        public long buyingPrice;
        public long sellPrice;

        public int buyingQnt;
        public int sellQnt;
        public int curQnt;
        public int trailingTickCnt;
        public int outStandingQnt;

        public long curPrice;
        protected bool isProfitSell; //매수주문 여부
        protected bool isBuy; //매수주문 여부
        protected bool isSold; //매도주문 여부
        
        protected bool isBuyCancel; //매수취소 여부
        protected bool isSellCancel; //매도취소 여부
        protected bool isCompleteBuying; //매수완료 여부
        protected bool isCompleteSold; //매수완료 여부

        public DataGridViewRow ui_rowItem;
        public string conditionUid = string.Empty;

        public string Uid { get; set; } 

        public TradingItem(TradingStrategy tsItem, string itemCode, string itemName, long buyingPrice, int buyingQnt, bool completeBuying = false, bool sold = false, string buyOrderType = "", string sellOrderType = "")
        {
            this.ts = tsItem;
            this.itemCode = itemCode;
            this.itemName = itemName;
            this.buyingPrice = buyingPrice;
            this.buyingQnt = buyingQnt;
            this.outStandingQnt = buyingQnt;
            this.isCompleteBuying = false;
            this.isBuy = false;
            this.isSold = false;
            this.isBuyCancel = false;
            this.isSellCancel = false;
            this.isCompleteSold = false;
            this.buyOrderNum = string.Empty;
            this.sellOrderNum = string.Empty;

            this.buyOrderType = buyOrderType;
            this.sellOrderType = sellOrderType;

            this.Uid = System.Guid.NewGuid().ToString();
            curState = TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_BEFORE_ORDER;
        }
        public void UpdateCurrentPrice(long _price)
        {
            this.curPrice = _price;
        }
        public void SetUiConnectRow(DataGridViewRow row)         
        {
            this.ui_rowItem = row;
        }
        public DataGridViewRow GetUiConnectRow()
        {
            return this.ui_rowItem;
        }
        public void SetConditonUid(string uid)
        {
            this.conditionUid = uid;
        }
        public void SetOutStanding(int qnt)
        {
            this.outStandingQnt = qnt;
        }
        public void SetState(TRADING_ITEM_STATE _state)
        {
            curState = _state;
        }
        public bool IsSold()
        {
            return this.isSold;
        }
        public void SetSold(bool isProfitSell = true)
        {
            this.isSold = true;
            this.isProfitSell = isProfitSell;
            curState = TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_BEFORE_ORDER;
        }
        public bool IsProfitSell()
        {
            return isProfitSell;
        }
        public bool IsSellCancel()
        {
            return this.isSellCancel;
        }
        public void SetSellCancelOrder()
        {
          curState = TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_CANCEL_NOT_COMPLETE; 
        }

        public void SetSellCancelOrderComplete()
        {
            curState = TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_COMPLETE; //매도 취소 성공 -> 매수 완료 상태
        }

       
        public bool IsCompleteSold()
        {
            return this.isCompleteSold;
        }
        public void SetCompleteSold(bool sold)
        {
            this.isCompleteSold = sold;
            if (sold)
                curState = TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_COMPLETE; 
        }
        public bool IsCompleteBuying()
        {
            return this.isCompleteBuying;
        }
        public void SetCompleteBuying(bool buying)
        {
            this.isCompleteBuying = buying;
            if (buying)
                curState = TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_COMPLETE;
        }
        public bool IsBuyCancel()
        {
            return this.isBuyCancel;
        }
        public void SetBuyCancelComplete ()
        {
             curState = TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_CANCEL_COMPLETE;
        }
        public bool IsBuy()
        {
            return this.isBuy;
        }
        public void SetBuy(bool buying)
        {
            this.isBuy = buying;
            if (buying)
                curState = TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_BEFORE_ORDER;
        }
        
    }
}