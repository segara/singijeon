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
    public enum TRADING_ITEM_STATE
    {
        NONE,
        AUTO_TRADING_STATE_SEARCH_AND_CATCH,//종목포착
        AUTO_TRADING_STATE_BUY_BEFORE_ORDER,//매수주문접수시도중
        AUTO_TRADING_STATE_BUY_NOT_COMPLETE,//매수주문완료_체결대기
        AUTO_TRADING_STATE_BUY_NOT_COMPLETE_OUTCOUNT,//매수중_일부매수완료
        AUTO_TRADING_STATE_BUY_COMPLETE, //매수완료

        AUTO_TRADING_STATE_SELL_BEFORE_ORDER,//매도주문접수시도
        AUTO_TRADING_STATE_SELL_NOT_COMPLETE, //매도주문완료
        AUTO_TRADING_STATE_SELL_NOT_COMPLETE_OUTCOUNT, //일부매도
        AUTO_TRADING_STATE_SELL_COMPLETE, //매도완료
    }

    public enum MARTIN_RESULT
    {
        NONE,
        HAVE_ITEM,
        WIN,
        LOSE,
    }
      
    public class MartinGailItem
    {
        public int step;
        public int TodayIndex;
        public string itemCode;
        public TRADING_ITEM_STATE itemState = TRADING_ITEM_STATE.NONE;
        public  MARTIN_RESULT martinState = MARTIN_RESULT.NONE;
        public long buyQnt = 0;   
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

        MartinGailItem item;

        AxKHOpenAPI axKHOpenAPI1;
        tradingStrategyGridView form1;

        
        DateTime startOrderTime = DateTime.Now;

        TimeSpan timeSpanBuy;

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
                        Thread.Sleep(1000); //기본 실행 주기

                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception.Message);
                    }
                }
            });
            taskWorker.IsBackground = true;
            taskWorker.Start();
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
            strategy.OnReceiveCondition += OnReceiveConditionResult;
            strategy.OnReceiveBuyOrder += OnReceiveOrderTryResult;
            strategy.OnReceiveBuyChejan += OnReceiveChejanResult;
        }

        private void OnReceiveConditionResult(object sender, OnReceiveStrateyStateResultArgs e)
        {
            item = new MartinGailItem();
            item.itemState = e.State;
            item.itemCode = e.ItemCode;
            item.buyQnt = e.BuyQnt;
            CoreEngine.GetInstance().SendLogMessage("!!!!! 마틴게일 아이템 :" + e.State.ToString());
        }
        private void OnReceiveOrderTryResult(object sender, OnReceiveStrateyStateResultArgs e)
        {
            if(item != null)
            {
                CoreEngine.GetInstance().SendLogMessage("!!!!! 마틴게일 아이템 :" + e.State.ToString());
                item.itemState = e.State;
                if (item.itemState == TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_BEFORE_ORDER)
                {
                    //주문접수 시도 완료
                    startOrderTime = DateTime.Now;
                }
                else if (item.itemState == TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_NOT_COMPLETE)
                {
                    //주문접수 완료
                    startOrderTime = DateTime.Now;
                }
            }
           
        }
        public void OnReceiveChejanResult(object sender, OnReceiveStrateyStateResultArgs e)
        {
            if (item != null)
            {
                CoreEngine.GetInstance().SendLogMessage("!!!!! 마틴게일 아이템 :" + e.State.ToString());
                item.itemState = e.State;
                if(item.itemState == TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_NOT_COMPLETE_OUTCOUNT)
                {
                    //일부 매수일때
                }
                else if(item.itemState == TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_COMPLETE)
                {
                    //매수완료
                    startOrderTime = DateTime.Now;
                }
            }
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

        public List<MartinGailItem> TodayAllList()
        {
            return todayAllItems;
        }
        public void PushMartinGailItem(string itemCode)
        {
            CoreEngine.GetInstance().SendLogMessage("Push Martin GailItem");
            step++;

            if (item != null && item.itemCode == itemCode)
            {
                item.martinState = MARTIN_RESULT.HAVE_ITEM;
                item.step = step;
                item.TodayIndex = todayAllItems.Count;

                todayAllCode.Add(itemCode);
                martinGailStack.Push(item);
                todayAllItems.Add(item);
            }

        }

        public void PopMartinGailItem(long profit)
        {
            CoreEngine.GetInstance().SendLogMessage("Pop MartinGail Item");
            MartinGailItem item = martinGailStack.Pop();
            TodayAllTry++;

            if (profit > 0)
            {
                if(item != null)
                {
                   item.martinState = MARTIN_RESULT.WIN;
                    winCount++;
                    Restart();
                }
            }
            else
            {
                loseCount++;
                if (item != null)
                {
                    item.martinState = MARTIN_RESULT.LOSE;

                    if (item.step >= MARTIN_MAX_STEP)
                    {
                        Restart();
                    }
                    else
                    {
                        GoNext();
                    }
                }
            }
        }
        //마틴게일에서 매수한 종목인지 체크하기 위해
        public bool HaveMartinGailStrategyItemCode(string itemcode)
        {
            if (todayAllCode.Contains(itemcode))
                return true;

            return false;
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
            item = null;
            step = 0;
            maxStep = 0;
            startMoney = 0;
         }

        private void Restart()
        {
            if (tradingStrategy != null)
            {
                tradingStrategy.itemInvestment = startMoney;
                tradingStrategy.remainItemCount = tradingStrategy.buyItemCount;
                step = 0; //only innerStep 0
            }
         
        }
        private void GoNext()
        {
            if (tradingStrategy != null)
            {
                long buyAmount = tradingStrategy.itemInvestment * 2;
                tradingStrategy.itemInvestment = buyAmount;
                tradingStrategy.remainItemCount = tradingStrategy.buyItemCount;
            }
        }
        private void API_OnReceiveChejanData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveChejanDataEvent e)
        {
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
                                martinGailStack.Peek().buyQnt = tradeItem.buyingQnt;
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

                                //MartinGailItem item = martinGailStack.Pop();
                                //item.profitAmount = (sellPrice - buyingPrice) * tradeItem.buyingQnt;
                                TodayAllProfitAmount += (sellPrice - buyingPrice) * tradeItem.buyingQnt;

                                if ((sellPrice - buyingPrice) > 0)
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
                    else //미체결 상태
                    {

                    }
                }
            }
            else if (e.sGubun.Equals(ConstName.RECEIVE_CHEJAN_DATA_BALANCE))
            {
            }
        }
        void Update()
        {
            if (item != null)
            {
                if(item.itemState == TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_NOT_COMPLETE  ||
                    item.itemState == TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_NOT_COMPLETE_OUTCOUNT)
                    Console.WriteLine((startOrderTime - DateTime.Now));
            }
          
        }
    }
}
