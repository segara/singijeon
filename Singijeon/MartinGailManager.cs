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
        DRAW
    }
      
    public class MartinGailItem
    {
        public int step;
        public int TodayIndex;
        public string itemCode;
        public string buyOrderNum;
        public string sellOrderNum;

        public TRADING_ITEM_STATE itemState = TRADING_ITEM_STATE.NONE;
        public  MARTIN_RESULT martinState = MARTIN_RESULT.NONE;

        public long buyPrice = 0;
        public long curPrice = 0;
        public long buyQnt = 0;
        public long curQnt = 0;
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
        public TradingStrategy CurStrategy { get { return tradingStrategy; } }

        MartinGailItem item;

        AxKHOpenAPI axKHOpenAPI1;
        tradingStrategyGridView form1;
        
        DateTime startOrderTime = DateTime.Now;

        private int  step = 0;
        private int  maxStep = 0;
        private long startMoney = 0;

        public int StepInner { get { return step; } }
        public long StartMoney { get { return startMoney; } }

        private long TodayAllProfitAmount = 0;
        private int  TodayAllTry = 0;

        public int AllTryCnt { get { return TodayAllTry; } }
        public long ProfitMoney { get { return TodayAllProfitAmount; } }

        private int drawCount = 0;
        private int loseCount = 0;
        private int winCount = 0;

        public int DrawCnt { get { return drawCount; } }
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
            this.axKHOpenAPI1.OnReceiveChejanData += API_OnReceiveChejanData;
            this.axKHOpenAPI1.OnReceiveRealData += API_OnReceiveRealData; 
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
            item.curQnt = 0;
            item.buyPrice = e.BuyPrice;

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
                else if (item.itemState == TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_NOT_COMPLETE )
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
                

                if (item.itemState == TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_NOT_COMPLETE_OUTCOUNT)
                {
                    //일부 매수일때
                    startOrderTime = DateTime.Now;
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
            item = null;
            MartinGailItem itempop = martinGailStack.Pop();
            TodayAllTry++;

            if (profit > 0)
            {
                if(itempop != null)
                {
                   item.martinState = MARTIN_RESULT.WIN;
                    winCount++;
                    Restart();
                }
            }else if(profit == 0)
            {
                drawCount++;
                if (itempop != null)
                {
                    item.martinState = MARTIN_RESULT.DRAW;
                    RestartSameStep();
                }
            }
            else
            {
                loseCount++;
                if (itempop != null)
                {
                    itempop.martinState = MARTIN_RESULT.LOSE;

                    if (itempop.step >= MARTIN_MAX_STEP)
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
        private void RestartSameStep()
        {
            if (tradingStrategy != null)
            {
                tradingStrategy.remainItemCount = tradingStrategy.buyItemCount;
                if (step > 0)
                    step--;
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
        private void API_OnReceiveRealData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveRealDataEvent e)
        {
            string itemCode = e.sRealKey.Trim();


            if (e.sRealType == ConstName.RECEIVE_REAL_DATA_CONCLUSION) //주식이 체결될 때 마다 실시간 데이터를 받음
            {
                string price = axKHOpenAPI1.GetCommRealData(itemCode, 10);    //현재가
                string lowPrice = axKHOpenAPI1.GetCommRealData(itemCode, 18); //저가
                string openPrice = axKHOpenAPI1.GetCommRealData(itemCode, 16); //시가

                long c_lPrice = Math.Abs(long.Parse(price));

                if(item != null && item.itemCode == itemCode)
                {
                    item.curPrice = c_lPrice;
                }
            }
        }
        private void API_OnReceiveChejanData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveChejanDataEvent e)
        {
            if (e.sGubun.Equals(ConstName.RECEIVE_CHEJAN_DATA_SUBMIT_OR_CONCLUSION))
            {
                string orderState = axKHOpenAPI1.GetChejanData(913).Trim(); 
                string outstanding = axKHOpenAPI1.GetChejanData(902).Trim();
                string orderType = axKHOpenAPI1.GetChejanData(905).Replace("+", "").Replace("-", "").Trim();
                string ordernum = axKHOpenAPI1.GetChejanData(9203).Trim();
                string itemCode = axKHOpenAPI1.GetChejanData(9001).Replace("A", "");

                string conclusionPrice = axKHOpenAPI1.GetChejanData(910).Trim();
                string conclusionQuantity = axKHOpenAPI1.GetChejanData(911).Trim();

                if (orderState.Equals(ConstName.RECEIVE_CHEJAN_DATA_SUBMIT))
                {
                    if (orderType.Equals(ConstName.RECEIVE_CHEJAN_DATA_BUY))
                    {

                    }
                    else if (orderType.Equals(ConstName.RECEIVE_CHEJAN_DATA_SELL))
                    {

                    }
                    else if (orderType.Equals(ConstName.RECEIVE_CHEJAN_CANCEL_BUY_ORDER))
                    {

                    }
                    else if (orderType.Equals(ConstName.RECEIVE_CHEJAN_CANCEL_SELL_ORDER))
                    {
                    }
                }
                else if (orderState.Equals(ConstName.RECEIVE_CHEJAN_DATA_CONCLUSION))
                {
                  
                        if (orderType.Contains(ConstName.RECEIVE_CHEJAN_DATA_BUY))
                        {
                            if (tradingStrategy == null)
                                return;

                            TradingItem tradeItem = tradingStrategy.tradingItemList.Find(o => o.buyOrderNum.Equals(ordernum));
                            if (tradeItem != null)
                            {
                              
                                if (item != null)
                                {
                                    item.curQnt = long.Parse(conclusionQuantity); 
                                    item.buyPrice = tradeItem.buyingPrice;
                                    item.buyOrderNum = tradeItem.buyOrderNum;
                                    if(item.curQnt == item.buyQnt)
                                         PushMartinGailItem(itemCode);
                                }
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
                               
                                if (long.Parse(outstanding) == 0)
                                {
                                    TodayAllProfitAmount += (sellPrice - buyingPrice) * tradeItem.buyingQnt;
                                    PopMartinGailItem((sellPrice - buyingPrice));
                                }
                               
                            }
                        }
                    
                }
                else if (orderState.Equals(ConstName.RECEIVE_CHEJAN_DATA_OK))
                {
                    if (orderType.Contains(ConstName.RECEIVE_CHEJAN_CANCEL_BUY_ORDER))
                    {
                        if (int.Parse(outstanding) == 0)
                        {
                            if(item !=  null && item.buyOrderNum == ordernum)
                                PopMartinGailItem(0);
                        }
                    }
                    else if (orderType.Contains(ConstName.RECEIVE_CHEJAN_CANCEL_SELL_ORDER))
                    {
                        if (int.Parse(outstanding) == 0)
                        {
                            if (item != null && item.sellOrderNum == ordernum)
                                PopMartinGailItem(0);
                        }
                    }
                }
            }
            else if (e.sGubun.Equals(ConstName.RECEIVE_CHEJAN_DATA_BALANCE))
            {
            }
        }
        private void CancelBuyOrderAll()
        {
            if (item == null)
                return;

            CoreEngine.GetInstance().SendLogMessage("!!!!!!!!!!!! cancel quantity : " + (item.buyQnt - item.curQnt).ToString());
            int orderResult = axKHOpenAPI1.SendOrder("종목주문정정", form1.GetScreenNum().ToString(), tradingStrategy.account, CONST_NUMBER.SEND_ORDER_CANCEL_BUY, item.itemCode, (int)(item.buyQnt - item.curQnt), (int)item.buyPrice, ConstName.ORDER_JIJUNGGA, item.buyOrderNum);

            if (orderResult == 0)
            {
                CoreEngine.GetInstance().SendLogMessage("!!!!!!!!!!!! try cancel MartinGail Item !!!!!!!!!!!!!!");
            }
        }
                                         
        void Update()
        {
            if (item != null)
            {
                if(item.itemState == TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_NOT_COMPLETE  ||
                    item.itemState == TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_NOT_COMPLETE_OUTCOUNT)
                {
                    CoreEngine.GetInstance().SendLogMessage((startOrderTime - DateTime.Now).ToString());

                    if(item.itemState == TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_NOT_COMPLETE_OUTCOUNT)
                         CoreEngine.GetInstance().SendLogMessage(tradingStrategyGridView.GetProfitRate((double)item.curPrice, (double)item.buyPrice).ToString());

                    if ((startOrderTime - DateTime.Now).TotalSeconds > 120)
                    {
                        CoreEngine.GetInstance().SendLogMessage("!!!!!!!!!!!!Pop MartinGail Item!!!!!!!!!!!!!!");
                        CancelBuyOrderAll();
                    }
                }
            }
        }
    }
}
