using System;
using System.Collections.Generic;
using System.Linq;
namespace Singijeon
{
    public class TradingStrategy
    {
        public string account;
        public Condition buyCondition;   //매수 조건식
        public string buyOrderOption; //주문 호가 옵션
        public long totalInvestment;      //총 투자금액
        public int buyItemCount;           //매수종목수
        public int remainItemCount;         //
        public long itemInvestment;        //종목별 투자금

        public string sellOrderOption; //현재가 or 시장가 등

        public bool usingTakeProfit = false; //익절사용여부
        public bool usingStoploss = false;   //손절사용여부
        public double takeProfitRate = 0; //익절률
        public double stoplossRate = 0; //손절률

        public bool usingTickBuy = false;
        public int tickBuyValue = 0;

        public bool usingTrailing = false;
        public int trailTickValue = 0;

        public bool usingRestart = false;

        public bool usingTimeLimit = false;
        public DateTime startDate = DateTime.Now;
        public DateTime endDate = DateTime.Now;

        public bool onlyBuyInterest = false;

        //매매 진행 종목 리스트
        public List<TradingItem> tradingItemList = new List<TradingItem>();
        public List<TradingStrategyADDItem> tradingStrategyItemList = new List<TradingStrategyADDItem>();

        public event EventHandler<OnReceiveStrateyStateResultArgs> OnReceiveCondition; //종목 검색시
        public event EventHandler<OnReceiveStrateyStateResultArgs> OnReceiveBuyOrder; //종목 주문시
        public event EventHandler<OnReceiveStrateyStateResultArgs> OnReceiveBuyChejan; //종목 체결시
        public event EventHandler<OnReceiveStrateyStateResultArgs> OnReceiveSellOrder; //종목 주문시
        public event EventHandler<OnReceiveStrateyStateResultArgs> OnReceiveSellChejan; //종목 체결시

        public TradingStrategy(
            string _account,
            Condition _condition,
            string _buyOrderOption,
            long _totalInvestment,
            int _buyItemCount,
            string _sellOrderOption,
            bool _buyOnlyInterest,
            bool _buyRestart
            )
        {
            this.account = _account;
            this.buyCondition = _condition;
            this.buyOrderOption = _buyOrderOption;
            this.totalInvestment = _totalInvestment;
            this.buyItemCount = _buyItemCount;
            this.remainItemCount = _buyItemCount;

            //this.usingTakeProfit = _usingTakeProfit;
            //this.takeProfitRate = _takeProfitRate;
            //this.sellOrderOption = _sellOrderOption;
            //this.usingStoploss = _usingStoploss;
            //this.stoplossRate = _stoplossRate;
            this.onlyBuyInterest = _buyOnlyInterest;

            //this.usingTickBuy = _usingTickBuy;
            this.usingRestart = _buyRestart;
            

            if (buyItemCount > 0)
                this.itemInvestment = totalInvestment / buyItemCount;
        }

        public void AddTradingStrategyItemList(TradingStrategyADDItem item)
        {
            if (string.IsNullOrEmpty(item.strategyItemName) == false
                && tradingStrategyItemList.Find(o => o.strategyItemName.Equals(item.strategyItemName)) == null)
            {
                tradingStrategyItemList.Add(item);
            }
        }

        public void RemoveTradingStrategyItemList(string strategyItemName)
        {
            if (string.IsNullOrEmpty(strategyItemName) == false
                && tradingStrategyItemList.Find(o => o.strategyItemName.Equals(strategyItemName)) != null)
            {
                TradingStrategyADDItem item = tradingStrategyItemList.Find(o => o.strategyItemName.Equals(strategyItemName));
                tradingStrategyItemList.Remove(item);
            }
        }

        public void SetActiveTradingStrategyItemList(string strategyItemName, bool isActive)
        {
            if (string.IsNullOrEmpty(strategyItemName) == false
                && tradingStrategyItemList.Find(o => o.strategyItemName.Equals(strategyItemName)) != null)
            {
                TradingStrategyADDItem item = tradingStrategyItemList.Find(o => o.strategyItemName.Equals(strategyItemName));
                item.SetActive(isActive);
            }
        }

        public bool GetActiveTradingStrategyItem(string strategyItemName)
        {
            if (string.IsNullOrEmpty(strategyItemName) == false
                && tradingStrategyItemList.Find(o => o.strategyItemName.Equals(strategyItemName)) != null)
            {
                TradingStrategyADDItem item = tradingStrategyItemList.Find(o => o.strategyItemName.Equals(strategyItemName));
                return item.usingStrategy;
            }

            return false;
        }
        public T GetTradingStrategyItemWithType<T>(string strategyItemName) where T : TradingStrategyADDItem
        {
            T refItem = null;
            if (string.IsNullOrEmpty(strategyItemName) == false
                && tradingStrategyItemList.Find(o => o.strategyItemName.Equals(strategyItemName)) != null)
            {
                TradingStrategyADDItem item = tradingStrategyItemList.Find(o => o.strategyItemName.Equals(strategyItemName));
                refItem = item as T;
                return refItem;
            }

            return null;
        }

        public TradingStrategyADDItem GetTradingStrategyItem(string strategyItemName)
        {
            if (string.IsNullOrEmpty(strategyItemName) == false
                && tradingStrategyItemList.Find(o => o.strategyItemName.Equals(strategyItemName)) != null)
            {
                TradingStrategyADDItem item = tradingStrategyItemList.Find(o => o.strategyItemName.Equals(strategyItemName));
                return item;
            }

            return null;
        }
        public bool CheckBuyPossibleStrategyAddedItem()
        {
            bool returnVal = true;
            foreach (TradingStrategyADDItem item in tradingStrategyItemList)
            {
                if (item.strategyCheckTime == CHECK_TIMING.BUY_TIME)
                {
                    bool checkBool = item.CheckCondition();
                    if(checkBool == false)
                        return false;
                }
                    
            }
            return returnVal;
        }

        public void CheckUpdateTradingStrategyAddedItem(TradingItem trading_item, double inputValue, CHECK_TIMING checkTiming)
        {
            foreach(TradingStrategyADDItem item in tradingStrategyItemList)
            {
                if(item.strategyCheckTime == checkTiming)
                    item.CheckUpdate(trading_item, inputValue);
            }
        }

        public void StrategyConditionReceiveUpdate(string itemCode, int price, int qnt, TRADING_ITEM_STATE state)
        {
            if(OnReceiveCondition != null)
                OnReceiveCondition.Invoke(this, new OnReceiveStrateyStateResultArgs(itemCode, qnt, price, state));
        }
        public void StrategyBuyOrderUpdate(string itemCode, int price, int qnt, TRADING_ITEM_STATE state)
        {
            if (OnReceiveBuyOrder != null)
                OnReceiveBuyOrder.Invoke(this, new OnReceiveStrateyStateResultArgs(itemCode, qnt, price, state));
        }
        public void StrategyOnReceiveBuyChejanUpdate(string itemCode, int price, int qnt, TRADING_ITEM_STATE state)
        {
            if (OnReceiveBuyChejan != null)
                OnReceiveBuyChejan.Invoke(this, new OnReceiveStrateyStateResultArgs(itemCode, qnt, price, state));
        }
        public void StrategyOnReceiveSellOrderUpdate(string itemCode, int price, int qnt, TRADING_ITEM_STATE state)
        {
            if (OnReceiveSellOrder != null)
                OnReceiveSellOrder.Invoke(this, new OnReceiveStrateyStateResultArgs(itemCode, qnt, price, state));
        }
        public void StrategOnReceiveSellChejanUpdate(string itemCode, int price, int qnt, TRADING_ITEM_STATE state)
        {
            if (OnReceiveSellChejan != null)
                OnReceiveSellChejan.Invoke(this, new OnReceiveStrateyStateResultArgs(itemCode, qnt, price, state));
        }
    }
    public class OnReceivedTrEventArgs : EventArgs
    {
        public TradingItem tradingItem { get; set; }
        public double checkNum { get; set; }
        public OnReceivedTrEventArgs(TradingItem item , double checkValue)
        {
            this.tradingItem = item;
            this.checkNum = checkValue;
        }
    }

    public enum CHECK_TIMING
    {
        BUY_TIME,
        SELL_TIME,
    }

    public class TradingStrategyADDItem
    {
        public string valueName = string.Empty;

        public CHECK_TIMING strategyCheckTime = CHECK_TIMING.BUY_TIME;

        public bool usingStrategy = false;
        public string strategyItemName = string.Empty;

        

        public void SetActive(bool isActive)
        {
            usingStrategy = isActive;
        }

        public virtual void CheckUpdate(TradingItem item, double value)
        {

        }

        public virtual bool CheckCondition()
        {
            return false;
        }
    }
    
    public class TradingStrategyItemChangeValue : TradingStrategyADDItem
    {
        private double d_changeValue = 0;
        public double checkConditionValue { get { return d_changeValue; } set { d_changeValue = value; } }

        public TradingStrategyItemChangeValue(string _strategyItemName, CHECK_TIMING _checkTiming, string _valueName, double changeConditionValue)
        {
            usingStrategy = true;
            strategyItemName = _strategyItemName;
            strategyCheckTime = _checkTiming;
            valueName = _valueName;
            d_changeValue = changeConditionValue;
        }

        public override void CheckUpdate(TradingItem item, double value)
        {

        }
    }

    public class TradingStrategyItemBuyTimeCheck : TradingStrategyADDItem
    {
       
        private DateTime d_startTime;
        private DateTime d_endTime;

        public TradingStrategyItemBuyTimeCheck(string _strategyItemName, CHECK_TIMING _checkTiming, DateTime _startTime, DateTime _endTime)
        {
            usingStrategy = true;
            strategyItemName = _strategyItemName;
            strategyCheckTime = _checkTiming;
            d_startTime = _startTime;
            d_endTime = _endTime;
        }

        public override bool CheckCondition()
        {
            if (!usingStrategy)
                return false;

            if ((d_startTime - DateTime.Now).Ticks < 0 && (d_endTime - DateTime.Now).Ticks > 0)
                return true;
            return false;
        }

    
    }

    public class TradingStrategyItemWithUpDownValue : TradingStrategyADDItem
    {
        public enum IS_TRUE_OR_FALE_TYPE
        {
            UPPER,
            UPPER_OR_SAME,
            SAME,
            DOWN_OR_SAME,
            DOWN,
        }

       public IS_TRUE_OR_FALE_TYPE checkType = IS_TRUE_OR_FALE_TYPE.SAME;

       private double d_conditionValue = 0;
       public double checkConditionValue { get { return d_conditionValue; } set { d_conditionValue = value; } }

        public event EventHandler<OnReceivedTrEventArgs> OnReceivedTrData;

        public TradingStrategyItemWithUpDownValue(string _strategyItemName, CHECK_TIMING _checkTiming, string _valueName, IS_TRUE_OR_FALE_TYPE _checkType, double _conditionValue)
        {
            usingStrategy = true;
            strategyItemName = _strategyItemName;
            strategyCheckTime = _checkTiming;
            checkType = _checkType;
            d_conditionValue = _conditionValue;
            if (checkType == IS_TRUE_OR_FALE_TYPE.DOWN || checkType == IS_TRUE_OR_FALE_TYPE.DOWN_OR_SAME)
            {
               d_conditionValue = _conditionValue - tradingStrategyGridView.FEE_RATE;
            }
            if (checkType == IS_TRUE_OR_FALE_TYPE.UPPER || checkType == IS_TRUE_OR_FALE_TYPE.UPPER_OR_SAME)
            {
                d_conditionValue = _conditionValue + tradingStrategyGridView.FEE_RATE;
            }
        }

        public override void CheckUpdate(TradingItem item, double value)
        {
            if (!usingStrategy)
                return;
            switch(checkType)
            {
                case IS_TRUE_OR_FALE_TYPE.DOWN:
                    if(value < d_conditionValue)
                    {
                        if (OnReceivedTrData != null)
                            OnReceivedTrData.Invoke(this, new OnReceivedTrEventArgs(item, value));
                    }
                    return;
                case IS_TRUE_OR_FALE_TYPE.DOWN_OR_SAME:
                    if (value <= d_conditionValue)
                    {
                        if (OnReceivedTrData != null)
                            OnReceivedTrData.Invoke(this, new OnReceivedTrEventArgs(item, value));
                    }
                    return;
                case IS_TRUE_OR_FALE_TYPE.SAME:
                    if (value == d_conditionValue)
                    {
                        if (OnReceivedTrData != null)
                            OnReceivedTrData.Invoke(this, new OnReceivedTrEventArgs(item, value));
                    }
                    return;
                case IS_TRUE_OR_FALE_TYPE.UPPER:
                    if (value > d_conditionValue)
                    {
                        if (OnReceivedTrData != null)
                            OnReceivedTrData.Invoke(this, new OnReceivedTrEventArgs(item, value));
                    }
                    return;
                case IS_TRUE_OR_FALE_TYPE.UPPER_OR_SAME:
                    if (value >= d_conditionValue)
                    {
                        if (OnReceivedTrData != null)
                            OnReceivedTrData.Invoke(this, new OnReceivedTrEventArgs(item, value));
                    }
                    return;
            }
        }
    }
    public class OnReceiveStrateyStateResultArgs : EventArgs
    {
        public string ItemCode { get; set; }
        public int Price { get; set; }
        public int Qnt { get; set; }
        public TRADING_ITEM_STATE State { get; set; }
        public OnReceiveStrateyStateResultArgs(string itemcode, int buyQnt, int buyPrice, TRADING_ITEM_STATE state)
        {
            this.ItemCode = itemcode;
            this.Price = buyPrice;
            this.Qnt = buyQnt;
            this.State = state;
        }
    }
}
