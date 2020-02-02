using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public bool usingTakeProfit = false; //익절사용여부
        public bool usingStoploss = false;   //손절사용여부
        public double takeProfitRate = 0; //익절률
        public double stoplossRate = 0; //손절률
        public string sellOrderOption; //현재가 or 시장가 등

        //매매 진행 종목 리스트
        public List<TradingItem> tradingItemList = new List<TradingItem>(); 

        public TradingStrategy(
            string _account, 
            Condition _condition,
            string _buyOrderOption, 
            long _totalInvestment, 
            int _buyItemCount,
            bool _usingTakeProfit,
            double _takeProfitRate,
            string _sellOrderOption,
            bool _usingStoploss,
            double _stoplossRate
            )
        {
            this.account = _account;
            this.buyCondition = _condition;
            this.buyOrderOption = _buyOrderOption;
            this.totalInvestment = _totalInvestment;
            this.buyItemCount = _buyItemCount;
            this.remainItemCount = _buyItemCount;

            this.usingTakeProfit = _usingTakeProfit;
            this.takeProfitRate = _takeProfitRate;
            this.sellOrderOption = _sellOrderOption;
            this.usingStoploss = _usingStoploss;
            this.stoplossRate = _stoplossRate;
            
            if(buyItemCount>0)
                this.itemInvestment = totalInvestment / buyItemCount;
        }
    }
}
