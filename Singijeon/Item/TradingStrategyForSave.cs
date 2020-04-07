using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Singijeon
{
    [Serializable]
    public class TradingStrategyForSave
    {
        public string account;
        public Condition buyCondition;   //매수 조건식
        public string buyOrderOption; //주문 호가 옵션
        public long totalInvestment;      //총 투자금액
        public int buyItemCount;           //매수종목수
        public int remainItemCount;         //
        public long itemInvestment;        //종목별 투자금

        public string sellProfitOrderOption; //현재가 or 시장가 등
        public string sellStopLossOrderOption; //현재가 or 시장가 등

        public bool usingTakeProfit = false; //익절사용여부
        public bool usingStoploss = false;   //손절사용여부

        public double takeProfitRate = 0; //익절률
        public double stoplossRate = 0; //손절률

        public bool usingTickBuy = false;
        public int tickBuyValue = 0;

        public bool usingTrailing = false;
        public int trailTickValue = 0;
        public float trailMinusValue = 0;
        public bool usingPercentageBuy = false;
        public float percentageBuyValue = 0;

        public bool usingGapTrailBuy = false;   //갭상승시 매수
        public bool usingVwma = false;   //갭상승시 매수
        public float gapTrailCostPercentageValue = 0;
        public float gapTrailBuyPercentageValue = 0.5f;
        public int gapTrailBuyTimeValue = 0;
        public bool usingRestart = false;

        public bool usingTimeLimit = false;
        public DateTime startDate = DateTime.Now;
        public DateTime endDate = DateTime.Now;

        public bool usingBuyMore = false;
        public double buyMoreRate = 0;
        public int buyMoreMoney = 0;

        public bool usingDoubleCheck = false;
        public Condition doubleCheckCondition = null;

        public List<TradingItem> tradingItemList = new List<TradingItem>();
        public List<string> doubleCheckItemCode = new List<string>();

        //매매 진행 종목 리스트
        public List<TradingItemForSave> tradingSaveItemList = new List<TradingItemForSave>();
        //public List<TradingStrategyADDItem> tradingStrategyItemList = new List<TradingStrategyADDItem>();

        public TradingStrategyForSave(BalanceSellStrategy strategy)
        {
            sellProfitOrderOption = strategy.profitOrderOption;
            sellStopLossOrderOption = strategy.stoplossOrderOption;

            account = strategy.account;
            takeProfitRate = strategy.takeProfitRate;
            stoplossRate = strategy.stoplossRate;
            tradingSaveItemList.Add(new TradingItemForSave(strategy));
        }

        public TradingStrategyForSave()
        {

        }

        public TradingStrategyForSave(TradingStrategy strategy)
        {
           

            BindingFlags flags = BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
            FieldInfo[] fieldArray = strategy.GetType().GetFields(flags);

            BindingFlags flagsStrategySave = BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
            FieldInfo[] StrategySaveFieldArray = this.GetType().GetFields(flagsStrategySave);


            foreach (FieldInfo field in fieldArray)
            {
                foreach (FieldInfo SaveField in StrategySaveFieldArray)
                {
                    if(field.Name == SaveField.Name)
                    {
                        SaveField.SetValue(this, field.GetValue(strategy));
                    }
                }
            }

            foreach (var item in strategy.tradingItemList)
            {
                tradingSaveItemList.Add(new TradingItemForSave(item, this));
            }

        }
    }
}
