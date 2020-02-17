using AxKHOpenAPILib;
using Singijeon.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Singijeon
{
    public enum MARTIN_RESULT
    {
        NONE,
        HAVE_ITEM,
        PROFIT_TAKE,
        STOP_LOSS,
    }
      
    public class MartinGailItem
    {
        public int step;
        public int TodayIndex;
        public string itemCode;

        public  MARTIN_RESULT martinState = MARTIN_RESULT.NONE;

        public long profitAmount       = 0;      //손익금액
        public double profitPercentage = 0;      //손익률
    }

    public class MartinGailManager
    {
        private static MartinGailManager martinInstance;

       public const int MARTIN_MAX_STEP = 3;

        List<string> todayAllCode = new List<string>();
        List<MartinGailItem> todayAllItems = new List<MartinGailItem>();

        Stack<MartinGailItem> martinGailStack = new Stack<MartinGailItem>();


       TradingStrategy tradingStrategy;   //적용된 전략


        AxKHOpenAPI axKHOpenAPI1;
        tradingStrategyGridView form1;

        private int  step = 0;
        private int  maxStep = 0;
        private long startMoney = 0;

        public int StepInner { get { return step; } }
        public long StartMoney { get { return startMoney; } }

        private long TodayAllProfitAmount = 0;
        private int  TodayAllTry = 0;

        public int AllTryCnt { get { return TodayAllTry; } }
        public long ProfitMoney { get { return TodayAllProfitAmount; } }

        private int loseCount = 0;
        private int winCount = 0;
        public int LoseCnt { get { return loseCount; } }
        public int WinCnt { get { return winCount; } }

        Thread taskWorker;

        private MartinGailManager()
        {
            taskWorker = new Thread(delegate ()
            {
                while (true)
                {
                    try
                    {
                        Update();
                        Thread.Sleep(100); //기본 실행 주기

                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception.Message);
                    }
                }
            });
        }

        public static MartinGailManager GetInstance()
        {
            if (martinInstance == null)
            {
                martinInstance = new MartinGailManager();
            }
            return martinInstance;
        }

        public void Init(AxKHOpenAPI axKHOpenAPI, tradingStrategyGridView form)
        {
            this.axKHOpenAPI1 = axKHOpenAPI;
            this.form1 = form;
            this.axKHOpenAPI1.OnReceiveChejanData += API_OnReceiveChejanData; //체결잔고
        }

        public void SetMartinStrategy(TradingStrategy strategy, int _maxStep)
        {
            step = 0;
            maxStep = _maxStep;
            startMoney = strategy.itemInvestment;
            tradingStrategy = strategy;
        }

        public int CheckMartinValid(TradingStrategy strategy)
        {
            if (tradingStrategy != null)
            {
                return Martin_ErrorCode.ALREADY_STRATEGY;
            }
            if(strategy.takeProfitRate == 0 || strategy.takeProfitRate != strategy.stoplossRate * -1)
            {
                return Martin_ErrorCode.NOT_VALID_PROFIT;
            }
            if (strategy.usingRestart != false)
            {
                return Martin_ErrorCode.RESTART_ON;
            }
            if (strategy.buyItemCount != 1)
            {
                return Martin_ErrorCode.BUY_ITEM_COUNT;
            }
   
            return Martin_ErrorCode.ERR_NONE;
        }

        public void PushMartinGailItem(string itemCode)
        {
            CoreEngine.GetInstance().SendLogMessage("Push Martin GailItem");
            MartinGailItem item = new MartinGailItem();
            item.itemCode = itemCode;
            item.martinState = MARTIN_RESULT.HAVE_ITEM;

            todayAllCode.Add(itemCode);
            martinGailStack.Push(item);
            todayAllItems.Add(item);
            step = martinGailStack.Count;
            item.step = martinGailStack.Count;
            item.TodayIndex = todayAllItems.Count;
        }

        public void PopMartinGailItem(long profit)
        {
            CoreEngine.GetInstance().SendLogMessage("Pop MartinGail Item");
            MartinGailItem item = martinGailStack.Pop();
           
            item.profitAmount = profit;
            TodayAllProfitAmount += profit;

            if (profit > 0)
            {
                item.martinState = MARTIN_RESULT.PROFIT_TAKE;
                winCount++;
                tradingStrategy.itemInvestment = startMoney;
                tradingStrategy.remainItemCount = tradingStrategy.buyItemCount;
            }
            else if (item.martinState == MARTIN_RESULT.STOP_LOSS)
            {
                item.martinState = MARTIN_RESULT.STOP_LOSS;
                loseCount++;
                if(item.step >= MARTIN_MAX_STEP)
                {
                    tradingStrategy.itemInvestment = startMoney;
                    tradingStrategy.remainItemCount = tradingStrategy.buyItemCount;

                    step = 0; //only innerStep 0
                }
                else
                {
                    long buyAmount = tradingStrategy.itemInvestment * 2;
                    tradingStrategy.itemInvestment = buyAmount;
                    tradingStrategy.remainItemCount = tradingStrategy.buyItemCount;
                    //tradingStrategy.usingRestart = true;
                }
            }
            TodayAllTry++;
        }
        //마틴게일에서 매수한 종목인지 체크하기 위해
        public bool HaveMartinGailStrategyItemCode(string itemcode)
        {
            if (todayAllCode.Contains(itemcode))
                return false;

            return true;
        }

        public bool IsMartinStrategy(TradingStrategy strategy)
        {
            if (tradingStrategy == null)
                return false;

            if (tradingStrategy == strategy)
                return true;
            return false;
        }

        public bool IsMartinCondition(Condition condition)
        {
            if (tradingStrategy == null)
                return false;

            if (tradingStrategy.buyCondition == condition)
                return true;
            return false;
        }

        public void Stop()
        {
            martinGailStack.Clear();

            tradingStrategy = null;

            step = 0;
            maxStep = 0;
            startMoney = 0;
         }

        private void API_OnReceiveChejanData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveChejanDataEvent e)
        {
            CoreEngine.GetInstance().SendLogMessage("API_OnReceiveChejanData");
            if (e.sGubun.Equals(ConstName.RECEIVE_CHEJAN_DATA_SUBMIT_OR_CONCLUSION))
            {
                string orderState = axKHOpenAPI1.GetChejanData(913);
                string outstanding = axKHOpenAPI1.GetChejanData(902);
                string orderType = axKHOpenAPI1.GetChejanData(905);
                string ordernum = axKHOpenAPI1.GetChejanData(9203);
                string itemCode = axKHOpenAPI1.GetChejanData(9001).Replace("A", "");

                string conclusionPrice = axKHOpenAPI1.GetChejanData(910);
                string conclusionQuantity = axKHOpenAPI1.GetChejanData(911);

                if (orderState.Equals(ConstName.RECEIVE_CHEJAN_DATA_SUBMIT))
                {
                    if (orderType.Contains(ConstName.RECEIVE_CHEJAN_DATA_BUY))
                    {
                     
                    }
                    else if (orderType.Contains(ConstName.RECEIVE_CHEJAN_DATA_SELL))
                    {
                     
                    }
                }
                else if (orderState.Equals(ConstName.RECEIVE_CHEJAN_DATA_CONCLUSION))
                {
                    if (int.Parse(outstanding) == 0)
                    {
                        if (orderType.Contains(ConstName.RECEIVE_CHEJAN_DATA_BUY))
                        {
                            if (tradingStrategy == null)
                                return;
                            TradingItem tradeItem = tradingStrategy.tradingItemList.Find(o => o.buyOrderNum.Equals(ordernum));
                            if (tradeItem != null)
                            {
                                PushMartinGailItem(itemCode);
                            }
                        }
                        else if (orderType.Contains(ConstName.RECEIVE_CHEJAN_DATA_SELL))
                        {
                            if (tradingStrategy == null)
                                return;
                            TradingItem tradeItem = tradingStrategy.tradingItemList.Find(o => o.sellOrderNum.Equals(ordernum));
                            if (tradeItem != null)
                            {
                                long buyingPrice = tradeItem.buyingPrice;
                                long sellPrice = long.Parse(conclusionPrice.Replace("+", ""));
                                if((sellPrice - buyingPrice) > 0)
                                {
                                    PopMartinGailItem((sellPrice - buyingPrice));
                                }
                                else
                                {
                                    PopMartinGailItem((sellPrice - buyingPrice));
                                }
                                
                            }
                        }
                    }
                }
            }
             
        }
        void Update()
        {
          
        }
    }
}
