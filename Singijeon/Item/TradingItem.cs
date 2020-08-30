using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace Singijeon
{
    [Serializable]
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

        AUTO_TRADING_STATE_BUYMORE_BEFORE_ORDER,//추가매수주문접수시도중
        AUTO_TRADING_STATE_BUYMORE_NOT_COMPLETE,//추가매수주문완료_체결대기
        AUTO_TRADING_STATE_BUYMORE_COMPLETE, //추가매수완료

        AUTO_TRADING_STATE_SELL_BEFORE_ORDER,//매도주문접수시도
        AUTO_TRADING_STATE_SELL_NOT_COMPLETE, //매도주문완료

        AUTO_TRADING_STATE_SELL_CANCEL_NOT_COMPLETE, //매도취소 접수시도
        //AUTO_TRADING_STATE_SELL_CANCEL_COMPLETE, //매도 취소는 주문완료로 대체

        AUTO_TRADING_STATE_SELL_NOT_COMPLETE_OUTCOUNT, //일부매도
        AUTO_TRADING_STATE_SELL_COMPLETE, //매도완료

    }

    [Serializable]
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
        public int curCanOrderQnt;
        public int startSellQnt; //매도 시작시 균등 매도를 위한 보유수량 저장
        public int trailingTickCnt;
        public int outStandingQnt;

        public long curPrice;
        protected bool isProfitSell; //매수주문 여부
       
        protected bool isBuyCancel; //매수취소 여부
        protected bool isSellCancel; //매도취소 여부
        protected bool isCompleteBuying; //매수완료 여부
        protected bool isCompleteSold; //매수완료 여부
        public string conditionUid = string.Empty;
        
        public string Uid { get; set; }

        public TickBongInfoMgr tickBongInfoMgr = new TickBongInfoMgr(30);
        public bool startTrailingSell = false;
        public bool useBuyMore = true;
    
        public bool usingBuyCancelByTime = true;

        public bool usingDivideSellProfit = false;
        public bool usingDivideSellProfitLoop = false;
        public bool usingDivideSellLoss = false;
        public bool usingDivideSellLossLoop = false;
        public bool usingTakeProfitAfterBuyMore = false;
        public bool usingStopLossAfterBuyMore = false;
        public int divideSellCount = 100;
        public int divideSellCountProfit = 100;
        [NonSerialized]
        public DataGridViewRow ui_rowItem;
        public TradingItem()
        {
        }

        public TradingItem(TradingStrategy tsItem, string itemCode, string itemName, long buyingPrice, int buyingQnt, bool completeBuying = false, bool sold = false, string buyOrderType = "", string sellOrderType = "")
        {
            this.ts = tsItem;
            this.itemCode = itemCode;
            this.itemName = itemName;
            this.buyingPrice = buyingPrice;
            this.buyingQnt = buyingQnt;
            this.outStandingQnt = buyingQnt;
            this.isCompleteBuying = false;
            this.conditionUid = ts.buyCondition.Uid;
            this.isBuyCancel = false;   
            this.isCompleteSold = false;

            this.buyOrderNum = string.Empty;
            this.sellOrderNum = string.Empty;

            this.buyOrderType = buyOrderType;
            //this.sellOrderType = sellOrderType;
            this.usingDivideSellLoss = ts.useDivideSellLoss;
            this.usingDivideSellProfit = ts.useDivideSellProfit;
            this.usingDivideSellLossLoop = ts.useDivideSellLossLoop;
            this.usingDivideSellProfitLoop = ts.useDivideSellProfitLoop;
            this.usingStopLossAfterBuyMore = ts.usingStopLossAfterBuyMore;
            this.usingTakeProfitAfterBuyMore = ts.usingTakeProfitAfterBuyMore;
            this.divideSellCount = ts.divideSellCount;
            this.divideSellCountProfit = ts.divideSellCountProfit;
            this.Uid = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
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
        public void SetSellOrderType(bool profitSell)
        {
            if(profitSell)
                sellOrderType = ts.sellProfitOrderOption;
            else
                sellOrderType = ts.sellStopLossOrderOption;
        }
       
        public void SetSold(bool isProfitSell = true)
        {
            this.isProfitSell = isProfitSell;
            curState = TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_BEFORE_ORDER;
        }
        public bool IsProfitSell()
        {
            return isProfitSell;
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
        public void SetBuyCancelOrder()
        {
            curState = TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_CANCEL_NOT_COMPLETE;
        }
        public void SetBuyCancelComplete ()
        {
             curState = TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_CANCEL_COMPLETE;
        }
       
        public void SetBuyState()
        {
             curState = TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_BEFORE_ORDER;
        }
        public static string StateToString(TRADING_ITEM_STATE state)
        {
            switch (state)
            {
                case TRADING_ITEM_STATE.NONE:
                    return ConstName.AUTO_TRADING_STATE_NONE;
                case TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_NOT_COMPLETE_OUTCOUNT:
                    return ConstName.AUTO_TRADING_STATE_SELL_NOT_COMPLETE_OUTCOUNT;
                case TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_NOT_COMPLETE:
                    return ConstName.AUTO_TRADING_STATE_SELL_NOT_COMPLETE;
                case TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_COMPLETE:
                    return ConstName.AUTO_TRADING_STATE_SELL_COMPLETE;
                case TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_CANCEL_NOT_COMPLETE:
                    return ConstName.AUTO_TRADING_STATE_SELL_CANCEL_NOT_COMPLETE;
                case TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_BEFORE_ORDER:
                    return ConstName.AUTO_TRADING_STATE_SELL_BEFORE_ORDER;
                case TRADING_ITEM_STATE.AUTO_TRADING_STATE_SEARCH_AND_CATCH:
                    return ConstName.AUTO_TRADING_STATE_SEARCH_AND_CATCH;
                case TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_NOT_COMPLETE_OUTCOUNT:
                    return ConstName.AUTO_TRADING_STATE_BUY_NOT_COMPLETE_OUTCOUNT;
                case TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_NOT_COMPLETE:
                    return ConstName.AUTO_TRADING_STATE_BUY_NOT_COMPLETE;
                case TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_COMPLETE:
                    return ConstName.AUTO_TRADING_STATE_BUY_COMPLETE;
                case TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_CANCEL_NOT_COMPLETE:
                    return ConstName.AUTO_TRADING_STATE_BUY_CANCEL_NOT_COMPLETE;
                case TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_CANCEL_COMPLETE:
                    return ConstName.AUTO_TRADING_STATE_BUY_CANCEL_COMPLETE;
                case TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_BEFORE_ORDER:
                    return ConstName.AUTO_TRADING_STATE_BUY_BEFORE_ORDER;
                default:
                    return "ERROR";
            }
        }

        
    }
}