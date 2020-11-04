using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Singijeon.Item;
namespace Singijeon
{
    public class SaveLoadManager
    {
        private static SaveLoadManager instance;

        private const string DATA_FILE_NAME = @"trading_item.dat";
        private const string DATA_TRAIL_FILE_NAME = @"trailing_item.dat";
        private const string DATA_BSS_FILE_NAME = @"balance_sell_item.dat";
        private const string LOAD_DATA_FILE_NAME = @"trading_item.dat";
        private const string LOAD_DATA_TRAIL_FILE_NAME = @"trailing_item.dat";
        private const string LOAD_DATA_BSS_FILE_NAME = @"balance_sell_item.dat";
        private const string LOAD_DEFAULT_DATA_FILE_NAME = @"default_trading_item.dat";

        Form1 form;
        private AxKHOpenAPILib.AxKHOpenAPI axKHOpenAPI1;
        private SaveLoadManager()
        {

        }

        public static SaveLoadManager GetInstance()
        {
            if (instance == null)
            {
                instance = new SaveLoadManager();
            }
            return instance;
        }

        public void SetForm(Form1 _form, AxKHOpenAPILib.AxKHOpenAPI api)
        {
            form = _form;
            axKHOpenAPI1 = api;
        }

        public void SerializeTrailing(List<TrailingItem> trailingList)
        {
            List<TrailingPercentageItemForSave> trailingSaveList = new List<TrailingPercentageItemForSave>();


            foreach (var item in trailingList)
            {
                TrailingPercentageItemForSave saveItem = new TrailingPercentageItemForSave(item, item.strategy);
                trailingSaveList.Add(saveItem);
            }

            try
            {
                BinaryFormatter binFmt = new BinaryFormatter();

                using (FileStream fs = new FileStream(DateTime.Now.ToString("MM_dd") + DATA_TRAIL_FILE_NAME, FileMode.Create))
                {
                    binFmt.Serialize(fs, trailingSaveList);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(1);
                Console.WriteLine(e);
            }
        }
        public void SerializeBSS(List<BalanceStrategy> bssList)
        {
            List<BalanceStrategy> bssSaveList = new List<BalanceStrategy>();

            foreach (var item in bssList)
            {
                if(item.type == BalanceStrategy.BALANCE_STRATEGY_TYPE.SELL)
                    bssSaveList.Add(item);
            }

            try
            {
                BinaryFormatter binFmt = new BinaryFormatter();

                using (FileStream fs = new FileStream(DateTime.Now.ToString("MM_dd") + DATA_BSS_FILE_NAME, FileMode.Create))
                {
                    binFmt.Serialize(fs, bssSaveList);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(1);
                Console.WriteLine(e);
            }
        }
        public void DeserializeTrailing()
        {
            List<TrailingPercentageItemForSave> trailingSaveList = new List<TrailingPercentageItemForSave>();

            BinaryFormatter binFmt = new BinaryFormatter();
            try
            {
                using (FileStream rdr = new FileStream(DateTime.Now.ToString("MM_dd") + LOAD_DATA_TRAIL_FILE_NAME, FileMode.Open))
                {
                    trailingSaveList = (List<TrailingPercentageItemForSave>)binFmt.Deserialize(rdr);
                    foreach (var item in trailingSaveList)
                    {
                        AddTrailing(item);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(2);
                Console.WriteLine(e);
            }
        }

        public void DeserializeBSS()
        {
            List<BalanceStrategy> saveList = new List<BalanceStrategy>();

            BinaryFormatter binFmt = new BinaryFormatter();
            try
            {
                using (FileStream rdr = new FileStream(DateTime.Now.ToString("MM_dd") + LOAD_DATA_BSS_FILE_NAME, FileMode.Open))
                {
                    saveList = (List<BalanceStrategy>)binFmt.Deserialize(rdr);
                    foreach (var item in saveList)
                    {
                        AddTryingSellList(item);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(2);
                Console.WriteLine(e);
            }
        }

        public void SerializeStrategy(List<TradingStrategy> tradingStrategyList)
        {
            List<TradingStrategyForSave> list = new List<TradingStrategyForSave>();

            foreach (var item in tradingStrategyList)
            {
                TradingStrategyForSave save = new TradingStrategyForSave(item);
                list.Add(save);
            }
            try
            {
                BinaryFormatter binFmt = new BinaryFormatter();

                using (FileStream fs = new FileStream(DateTime.Now.ToString("MM_dd") + DATA_FILE_NAME, FileMode.Create))
                {
                    binFmt.Serialize(fs, list);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(3);
                Console.WriteLine(e);
            }
        }

        public void UseCustomDefaultSetting()
        {
            List<TradingStrategyForSave> list = null;
            BinaryFormatter binFmt = new BinaryFormatter();
            try
            {
                using (FileStream rdr = new FileStream(LOAD_DEFAULT_DATA_FILE_NAME, FileMode.Open))
                {
                    string account = string.Empty;
                    list = (List<TradingStrategyForSave>)binFmt.Deserialize(rdr);
                    foreach (TradingStrategyForSave ts in list)
                    {
                        AddStratgy(ts);
                        account = ts.account;
                    }
                    if(string.IsNullOrEmpty(account) == false)
                    {
                        form.SetAccountComboBox(account);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public List<TradingStrategyForSave> DeserializeStrategy()
        {
            List<TradingStrategyForSave> list = null;
            BinaryFormatter binFmt = new BinaryFormatter();
            try
            {
                using (FileStream rdr = new FileStream(DateTime.Now.ToString("MM_dd") + LOAD_DATA_FILE_NAME, FileMode.Open))
                {
                    list = (List<TradingStrategyForSave>)binFmt.Deserialize(rdr);
                    foreach (TradingStrategyForSave ts in list)
                    {
                        AddStratgy(ts);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(4);
                Console.WriteLine(e);
               
            }
            
            return list;
        }

        private void AddTrailing(TrailingPercentageItemForSave saved)
        {
            Task requestItemInfoTask = new Task(() =>
            {
                TrailingItem itemAdd = saved.ReloadTrailingItem();

                foreach(var strategyItem in form.tradingStrategyList)
                {
                    if(saved.strategySave.buyCondition.Name == strategyItem.buyCondition.Name)
                    {
                        itemAdd.strategy = strategyItem;
                    }
                }

                axKHOpenAPI1.SetInputValue("종목코드", itemAdd.itemCode);
                int result = axKHOpenAPI1.CommRqData(ConstName.RECEIVE_REAL_DATA_HOGA, "opt10004", 0, Form1.GetScreenNum().ToString());

                form.SetTrailingItem(itemAdd);

            });
            RequestTrDataManager.GetInstance().RequestTrData(requestItemInfoTask);

        }
        private void AddTryingSellList(BalanceStrategy saved)
        {
            form.BalanceSell(saved.account, saved.itemCode, saved.buyingPrice, (int)saved.curQnt, (int)saved.sellQnt, saved.profitOrderOption, saved.stoplossOrderOption, saved.takeProfitRate, saved.stoplossRate);
        }
        private void AddStratgy(TradingStrategyForSave saved)
        {

            List<TradingStrategyADDItem> tradingStrategyItemList = new List<TradingStrategyADDItem>();

            TradingStrategy ts = new TradingStrategy(
                saved.account,
                saved.buyCondition,
                saved.buyOrderOption,
                saved.totalInvestment,
                saved.buyItemCount,
                saved.sellProfitOrderOption,
                saved.sellStopLossOrderOption,
                saved.sellDivideStopLossOrderOption,
                false,
                saved.usingRestart
             );

            //추가전략 적용
            ts.remainItemCount = saved.remainItemCount;
            
            //trading item load
            foreach(var item in saved.tradingSaveItemList)
            {
                TradingItem itemAdd = item.ReloadTradingItem();
                itemAdd.ts = ts;
                ts.tradingItemList.Add(itemAdd);

                string fidList = "9001;302;10;11;25;12;13"; //9001:종목코드,302:종목명
                axKHOpenAPI1.SetRealReg("9001", itemAdd.itemCode, fidList, "1");

                //매매진행 데이터 그리드뷰 표시
                form.SetTradingItemUI(itemAdd);
            }

            if (saved.usingTimeLimit)
            {
                TradingStrategyItemBuyTimeCheck timeBuyCheck =
                     new TradingStrategyItemBuyTimeCheck(
                             StrategyItemName.BUY_TIME_LIMIT,
                             CHECK_TIMING.BUY_TIME,
                            saved.startDate,
                             saved.endDate);
                ts.AddTradingStrategyItemList(timeBuyCheck);
                ts.usingTimeLimit = true;
                ts.startDate = saved.startDate;
                ts.endDate = saved.endDate;
            }

            ts.usingTickBuy = saved.usingTickBuy;
            ts.tickBuyValue = saved.tickBuyValue;
           
            ts.usingTrailing = saved.usingTrailing;
            ts.trailTickValue = saved.trailTickValue;
            ts.trailMinusValue = saved.trailMinusValue;
           
            ts.usingPercentageBuy = saved.usingPercentageBuy;
            ts.percentageBuyValue = saved.percentageBuyValue;
   
            ts.usingVwma = saved.usingVwma;
            ts.usingEnvelope5 = saved.usingEnvelope5;
            ts.usingEnvelope7 = saved.usingEnvelope7;
            ts.usingEnvelope10 = saved.usingEnvelope10;
            ts.usingEnvelope15 = saved.usingEnvelope15;
            ts.trailTickValue = saved.trailTickValue;
   
            if (saved.usingGapTrailBuy)
            {
                ts.usingGapTrailBuy = true;
                ts.gapTrailCostPercentageValue = saved.gapTrailCostPercentageValue;
                ts.gapTrailBuyTimeValue = saved.gapTrailBuyTimeValue;

                TradingStrategyItemWithUpDownPercentValue trailGapBuy =
                    new TradingStrategyItemWithUpDownPercentValue(
                            StrategyItemName.BUY_GAP_CHECK,
                            CHECK_TIMING.BUY_TIME,
                            string.Empty,
                            ts.gapTrailCostPercentageValue);

                trailGapBuy.OnReceivedTrData += form.OnReceiveTrDataCheckGapTrailBuy;
                ts.AddTradingStrategyItemList(trailGapBuy);
            }

            if (saved.takeProfitRate > 0)
            {
                double takeProfitRate = 0;
                TradingStrategyItemWithUpDownValue takeProfitStrategy = null;
                takeProfitRate = saved.takeProfitRate;
                takeProfitStrategy =
                     new TradingStrategyItemWithUpDownValue(
                             StrategyItemName.TAKE_PROFIT_SELL,
                             CHECK_TIMING.SELL_TIME,
                             IS_TRUE_OR_FALE_TYPE.UPPER_OR_SAME,
                             takeProfitRate);
                takeProfitStrategy.OnReceivedTrData += form.OnReceiveTrDataCheckProfitSell;
                ts.AddTradingStrategyItemList(takeProfitStrategy);
                ts.takeProfitRate = takeProfitRate;
            }

            if (saved.stoplossRate < 0)
            {
                double stopLossRate = 0;
                stopLossRate = saved.stoplossRate;
                TradingStrategyItemWithUpDownValue stopLossStrategy = null;
                stopLossStrategy =
                    new TradingStrategyItemWithUpDownValue(
                            StrategyItemName.STOPLOSS_SELL,
                            CHECK_TIMING.SELL_TIME,
                            IS_TRUE_OR_FALE_TYPE.DOWN,
                            stopLossRate);

                stopLossStrategy.OnReceivedTrData += form.OnReceiveTrDataCheckStopLoss;
                ts.AddTradingStrategyItemList(stopLossStrategy);
                ts.stoplossRate = stopLossRate;
            }

            if (saved.usingBuyMore)
            {
                ts.usingBuyMore = true;
                //물타기
                TradingStrategyItemBuyingDivide buyMoreStrategy =
                    new TradingStrategyItemBuyingDivide(
                            StrategyItemName.BUY_MORE_LOSS,
                            CHECK_TIMING.SELL_TIME,
                            IS_TRUE_OR_FALE_TYPE.DOWN,
                            saved.buyMoreRateLoss,
                            saved.buyMoreMoney);

                buyMoreStrategy.OnReceivedTrData += form.OnReceiveTrDataBuyMore;
                ts.AddTradingStrategyItemList(buyMoreStrategy);
                //불타기                 
                TradingStrategyItemBuyingDivide buyMoreStrategyProfit =
                    new TradingStrategyItemBuyingDivide(
                            StrategyItemName.BUY_MORE_PROFIT,
                            CHECK_TIMING.SELL_TIME,
                            IS_TRUE_OR_FALE_TYPE.UPPER_OR_SAME,
                            saved.buyMoreRateProfit,
                            saved.buyMoreMoney);

                buyMoreStrategyProfit.OnReceivedTrData += form.OnReceiveTrDataBuyMore;
                ts.AddTradingStrategyItemList(buyMoreStrategyProfit);
            }

            if (saved.usingBuyCancelByTime)
            {
                ts.usingBuyCancelByTime = true;

                TradingStrategyItemCancelByTime buyCancelStrategy =
                new TradingStrategyItemCancelByTime(
                        StrategyItemName.BUY_CANCEL_BY_TIME,
                        CHECK_TIMING.BUY_ORDER_BEFORE_CONCLUSION,
                        DateTime.Now.Ticks
                         );

                buyCancelStrategy.OnReceivedTrData += form.OnReceiveTrDataBuyCancelByTime;
                ts.AddTradingStrategyItemList(buyCancelStrategy);

            }

            if (saved.useDivideSellLoss)
            {
                ts.useDivideSellLoss = true;
                ts.useDivideSellLossLoop = saved.useDivideSellLossLoop;
                TradingStrategyItemWithUpDownValue divideStopLossStrategy = null;
                divideStopLossStrategy =
                    new TradingStrategyItemWithUpDownValue(
                            StrategyItemName.STOPLOSS_DIVIDE_SELL,
                            CHECK_TIMING.SELL_TIME,
                            IS_TRUE_OR_FALE_TYPE.DOWN,
                            saved.divideStoplossRate);

                divideStopLossStrategy.OnReceivedTrData += form.OnReceiveTrDataCheckStopLossDivide;

                ts.AddTradingStrategyItemList(divideStopLossStrategy);
                ts.divideStoplossRate = saved.divideStoplossRate;
                ts.divideSellLossPercentage = (saved.divideSellLossPercentage);
                ts.divideSellCount = saved.divideSellCount;
            }

            if (saved.useDivideSellProfit)
            {
                ts.useDivideSellProfit = true;
                ts.useDivideSellProfitLoop = saved.useDivideSellProfitLoop;
                TradingStrategyItemWithUpDownValue divideProfitStrategy = null;
                divideProfitStrategy =
                    new TradingStrategyItemWithUpDownValue(
                            StrategyItemName.TAKE_PROFIT_DIVIDE_SELL,
                            CHECK_TIMING.SELL_TIME,
                            IS_TRUE_OR_FALE_TYPE.UPPER_OR_SAME,
                            saved.divideTakeProfitRate);

                divideProfitStrategy.OnReceivedTrData += form.OnReceiveTrDataCheckProfitDivide;
      
                ts.AddTradingStrategyItemList(divideProfitStrategy);
                ts.divideTakeProfitRate = saved.divideTakeProfitRate;
                ts.divideSellProfitPercentage = (saved.divideSellProfitPercentage);
                ts.divideSellCountProfit = saved.divideSellCountProfit;
            }

            form.tradingStrategyList.Add(ts);
            form.AddStrategyToDataGridView(ts);

            if(ts.buyCondition.Name.Contains("dummy") == false)
                form.StartMonitoring(ts.buyCondition);

            if (saved.usingDoubleCheck)
            {
                form.StartMonitoring(ts.doubleCheckCondition);
                form.doubleCheckHashTable.Add(ts.doubleCheckCondition.Name, ts);
            }

        }
    }
}
