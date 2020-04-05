using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Singijeon
{
    public class SaveLoadManager
    {
        private static SaveLoadManager instance;

        private const string DATA_FILE_NAME = @"trading_item.dat";
        private const string DATA_TRAIL_FILE_NAME = @"trailing_item.dat";

        private const string LOAD_DATA_FILE_NAME = @"trading_item.dat";
        private const string LOAD_DATA_TRAIL_FILE_NAME = @"trailing_item.dat";

       
        tradingStrategyGridView form;
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
        public void SetForm(tradingStrategyGridView _form, AxKHOpenAPILib.AxKHOpenAPI api)
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

            BinaryFormatter binFmt = new BinaryFormatter();

            using (FileStream fs = new FileStream(DateTime.Now.ToString("MM_dd") + DATA_TRAIL_FILE_NAME, FileMode.Create))
            {
                binFmt.Serialize(fs, trailingSaveList);
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

            BinaryFormatter binFmt = new BinaryFormatter();
           
            using (FileStream fs = new FileStream(DateTime.Now.ToString("MM_dd")+DATA_FILE_NAME, FileMode.Create))
            {
                binFmt.Serialize(fs, list);
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
                int result = axKHOpenAPI1.CommRqData(ConstName.RECEIVE_REAL_DATA_HOGA, "opt10004", 0, form.GetScreenNum().ToString());

                form.SetTrailingItem(itemAdd);

            });
            RequestTrDataManager.GetInstance().RequestTrData(requestItemInfoTask);

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
             
                TradingStrategyItemBuyingDivide buyMoreStrategy =
                    new TradingStrategyItemBuyingDivide(
                            StrategyItemName.BUY_MORE,
                            CHECK_TIMING.SELL_TIME,
                            IS_TRUE_OR_FALE_TYPE.DOWN,
                            saved.buyMoreRate);

                buyMoreStrategy.OnReceivedTrData += form.OnReceiveTrDataBuyMore;
                ts.AddTradingStrategyItemList(buyMoreStrategy);
            }

            form.tradingStrategyList.Add(ts);
            form.AddStrategyToDataGridView(ts);

            form.StartMonitoring(ts.buyCondition);

            if (saved.usingDoubleCheck)
            {
                form.StartMonitoring(ts.doubleCheckCondition);
                form.doubleCheckHashTable.Add(ts.doubleCheckCondition.Name, ts);
            }

        }
    }
}
