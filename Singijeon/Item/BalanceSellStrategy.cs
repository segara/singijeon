﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singijeon
{
    //
    public class BalanceSellStrategy
    {
        public string orderNum = string.Empty;

        public string account;
        public string itemCode;
        public int buyingPrice;
        public long curQnt;
        public long sellQnt;
        public bool isSold = false;

        public bool usingTakeProfit = false; //익절사용여부
        public bool usingStoploss = false;   //손절사용여부
        public double takeProfitRate = 0; //익절률
        public double stoplossRate = 0; //손절률
        public string profitOrderOption; //현재가 or 시장가 등
        public string stoplossOrderOption; //현재가 or 시장가 등
        //매매 진행 종목 리스트
        public List<TradingItem> tradingItemList = new List<TradingItem>();

        public BalanceSellStrategy(
            string _account,
            string _itemCode,
            int _buyingPrice,
            long _curQnt,
            long _sellQnt,
             string _sellProfitOrderOption,
             string _sellStopLossOrderOption,
            bool _usingTakeProfit,
            double _takeProfitRate,
            bool _usingStoploss,
            double _stoplossRate
            )
        {
            this.account = _account;
            this.itemCode = _itemCode;
            this.curQnt = _curQnt;
            this.sellQnt = _sellQnt;
            this.buyingPrice = _buyingPrice;
            this.usingTakeProfit = _usingTakeProfit;
            this.takeProfitRate = _takeProfitRate;
            this.profitOrderOption = _sellProfitOrderOption;
            this.stoplossOrderOption = _sellStopLossOrderOption;
            this.usingStoploss = _usingStoploss;
            this.stoplossRate = _stoplossRate;
        }
    }

}