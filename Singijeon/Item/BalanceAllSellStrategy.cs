using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singijeon.Item
{
    public class BalanceItem
    {
        public string itemCode;
        public string itemName;
        public bool bSell = false;
        public int buyingPrice;
        public int balanceQnt;

        public BalanceItem(string _itemCode, string _itemName, int _buyingPrice, int _balanceQnt)
        {
            this.itemCode = _itemCode.Trim().Replace("A","");
            this.itemName = _itemName;
            this.buyingPrice = _buyingPrice;
            this.balanceQnt = _balanceQnt;
            this.bSell = false;
        }
    }
    public class BalanceAllSellStrategy
    {
        public bool usingStrategy = false;
        public bool usingTakeProfit = false; //익절사용여부
        public bool usingStoploss = false;   //손절사용여부
        public double takeProfitRate = 0; //익절률
        public double stoplossRate = 0; //손절률
        public string profitOrderOption; //현재가 or 시장가 등
        public string stoplossOrderOption; //현재가 or 시장가 등

        public BalanceAllSellStrategy(
             string _sellProfitOrderOption,
             string _sellStopLossOrderOption,
            bool _usingTakeProfit,
            double _takeProfitRate,
            bool _usingStoploss,
            double _stoplossRate
            )
        {
            this.usingStrategy = true;
            this.usingTakeProfit = _usingTakeProfit;
            this.takeProfitRate = _takeProfitRate;
            this.profitOrderOption = _sellProfitOrderOption;
            this.stoplossOrderOption = _sellStopLossOrderOption;
            this.usingStoploss = _usingStoploss;
            this.stoplossRate = _stoplossRate;
        }

        public void StartStrategy()
        {
            usingStrategy = true;
        }

        public void StopStrategy()
        {
            usingStrategy = false;
        }
    }
}
