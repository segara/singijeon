using System;
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
        public long sellQnt;
        public bool isSold = false;

        public bool usingTakeProfit = false; //익절사용여부
        public bool usingStoploss = false;   //손절사용여부
        public double takeProfitRate = 0; //익절률
        public double stoplossRate = 0; //손절률
        public string sellOrderOption; //현재가 or 시장가 등

        //매매 진행 종목 리스트
        public List<TradingItem> tradingItemList = new List<TradingItem>();

        public BalanceSellStrategy(
            string _account,
            string _itemCode,
            int _buyingPrice,
            long _sellQnt,
             string _sellOrderOption,
            bool _usingTakeProfit,
            double _takeProfitRate,
            bool _usingStoploss,
            double _stoplossRate
            )
        {
            this.account = _account;
            this.itemCode = _itemCode;
            this.sellQnt = _sellQnt;
            this.buyingPrice = _buyingPrice;
            this.usingTakeProfit = _usingTakeProfit;
            this.takeProfitRate = _takeProfitRate;
            this.sellOrderOption = _sellOrderOption;
            this.usingStoploss = _usingStoploss;
            this.stoplossRate = _stoplossRate;

         
        }
    }

}