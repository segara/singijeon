﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singijeon
{
    [Serializable]
    public class TradingItemForSave
    {
        public TradingStrategyForSave ts = null;

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

        public int curQnt;
        public int buyingQnt;
        public int sellQnt;

        public int trailingTickCnt;
        public int outStandingQnt;

        public long curPrice;
        protected bool isProfitSell; //매수주문 여부

        protected bool isBuyCancel; //매수취소 여부
        protected bool isSellCancel; //매도취소 여부
        protected bool isCompleteBuying; //매수완료 여부
        protected bool isCompleteSold; //매수완료 여부
        public string conditionUid = string.Empty;

        public TradingItemForSave(TradingItem item)
        {
            itemCode = item.itemCode;
            itemName = item.itemName;
            buyingPrice = item.buyingPrice;
            curQnt = item.curQnt;
            buyingQnt = item.buyingQnt;
            sellQnt = item.sellQnt;
            curState = item.state;
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