using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Singijeon
{
    [Serializable]
    public class TradingItemForSave
    {
        public TradingStrategyForSave tsSave = null;

        public TRADING_ITEM_STATE state { get { return curState; } }
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
        public int startSellQnt;
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

        public bool usingStopLossAfterBuyMore = false;
        public int divideSellCount = 100;
        public TradingItemForSave()
        {

        }
        public  TradingItemForSave(TradingItem item, TradingStrategyForSave ts)
        {
            this.tsSave = (ts);
            BindingFlags flags = BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
            FieldInfo[] fieldArray = item.GetType().GetFields(flags);

            BindingFlags flagsStrategySave = BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
            FieldInfo[] ItemSaveFieldArray = this.GetType().GetFields(flagsStrategySave);


            foreach (FieldInfo field in fieldArray)
            {
                foreach (FieldInfo SaveField in ItemSaveFieldArray)
                {
                    if (field.Name == SaveField.Name)
                    {
                        SaveField.SetValue(this, field.GetValue(item));
                    }
                }
            }
            
        }
        public TradingItem ReloadTradingItem()
        {
            TradingItem returnVal = new TradingItem();

            BindingFlags flags = BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
            FieldInfo[] returnFieldArray = returnVal.GetType().GetFields(flags);

            BindingFlags flagsStrategySave = BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
            FieldInfo[] ItemSaveFieldArray = this.GetType().GetFields(flagsStrategySave);


            foreach (FieldInfo field in returnFieldArray)
            {
                foreach (FieldInfo SaveField in ItemSaveFieldArray)
                {
                    if (field.Name == SaveField.Name)
                    {
                        field.SetValue(returnVal, SaveField.GetValue(this));
                    }
                }
            }
            returnVal.tickBongInfoMgr.Clear();
            return returnVal;
        }

        public TradingItemForSave(BalanceSellStrategy item)
        {
            itemCode = item.itemCode;
          
            buyingPrice = item.buyingPrice;
            curQnt = (int)item.curQnt;
            buyingQnt = (int)item.curQnt;
            sellQnt = (int)item.sellQnt;
            
            curState = TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_COMPLETE;
        }
    }
}
