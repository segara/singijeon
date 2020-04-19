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
    [Serializable]
    public enum MARTIN_RESULT
    {
        NONE,
        HAVE_ITEM,
        WIN,
        LOSE,
        DRAW
    }
    [Serializable]
    public class MartinGailItem
    {
        public int step;
        public int TodayIndex;
        public string itemCode;
        public string buyOrderNum;
        public string sellOrderNum;

        public TRADING_ITEM_STATE itemState = TRADING_ITEM_STATE.NONE;
        public MARTIN_RESULT martinState = MARTIN_RESULT.NONE;

        public long buyPrice = 0;
        public long curPrice = 0;
        public long buyQnt = 0;
        public long curQnt = 0;
        public long sellQnt = 0;
        public long profitAmount = 0;      //손익금액
        public double profitPercentage = 0;      //손익률

    }
    public class MartinGailManager
    {
        private static MartinGailManager martinInstance;

        public int MARTIN_MAX_STEP = 4;

        List<string> todayAllCode = new List<string>();
        List<MartinGailItem> todayAllItems = new List<MartinGailItem>();

        Stack<MartinGailItem> martinGailStack = new Stack<MartinGailItem>();

        TradingStrategy tradingStrategy;   //적용된 전략
        public TradingStrategy CurStrategy { get { return tradingStrategy; } }

        MartinGailItem martin_item;
        MartinGailItem Item {
            get {
                if(martin_item != null)
                {
                    return martin_item;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                martin_item = value;
            }

        }

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

        public bool using_Up_And_Cancel = false;
        public double Up_And_CancelValue = 0;
        public bool using_Outstand_UpAndCancel = false;
        public double OutStand_And_CancelValue = 0;
        public bool using_WaitAndCancel = false;
        public int Wait_And_CancelValue = 0;

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

            strategy.OnReceiveBuyOrder += OnReceiveBuyOrderTryResult;
            strategy.OnReceiveBuyChejan += OnReceiveChejanResult;

            strategy.OnReceiveSellOrder += OnReceiveSellOrderTryResult;
            strategy.OnReceiveSellChejan += OnReceiveSellChejanResult;
        }

        private void OnReceiveConditionResult(object sender, OnReceiveStrateyStateResultArgs e)
        {
            Item = new MartinGailItem();
           
            Item.itemState = e.State;
            Item.itemCode = e.ItemCode;
            startOrderTime = DateTime.Now;
            CoreEngine.GetInstance().SendLogMessage("!!!!! 마틴게일 아이템 :" + e.State.ToString());
        }
        private void OnReceiveBuyOrderTryResult(object sender, OnReceiveStrateyStateResultArgs e)
        {
            if(Item != null)
            {
                CoreEngine.GetInstance().SendLogMessage("!!!!! 마틴게일 아이템 :" + e.State.ToString());

                Item.itemState = e.State;
                Item.buyQnt = e.Qnt;
                if (Item.itemState == TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_BEFORE_ORDER 
                    || Item.itemState == TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_NOT_COMPLETE)
                {
                    //주문접수 시도 완료
                    startOrderTime = DateTime.Now;
                }
                
            }
        }

        private void OnReceiveSellOrderTryResult(object sender, OnReceiveStrateyStateResultArgs e)
        {
            if (Item != null)
            {
                CoreEngine.GetInstance().SendLogMessage("!!!!! 마틴게일 아이템 :" + e.State.ToString());

                Item.itemState = e.State;
                Item.sellQnt = e.Qnt;
                if (Item.itemState == TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_BEFORE_ORDER ||
                    Item.itemState == TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_NOT_COMPLETE)
                {
                    //주문접수 시도 완료
                    startOrderTime = DateTime.Now;
                }
               
            }
        }
        public void OnReceiveChejanResult(object sender, OnReceiveStrateyStateResultArgs e)
        {
            if (Item != null)
            {
                CoreEngine.GetInstance().SendLogMessage("!!!!! 마틴게일 아이템 :" + e.State.ToString());

                Item.itemState = e.State;
               

                if (Item.itemState == TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_NOT_COMPLETE_OUTCOUNT ||
                    Item.itemState == TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_COMPLETE)
                {
                    //매수완료 ,일부 매수일때
                    startOrderTime = DateTime.Now;
                }
              
            }
        }

        public void OnReceiveSellChejanResult(object sender, OnReceiveStrateyStateResultArgs e)
        {
            if (Item != null)
            {
                CoreEngine.GetInstance().SendLogMessage("!!!!! 마틴게일 아이템 :" + e.State.ToString());

                Item.itemState = e.State;


                if (Item.itemState == TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_NOT_COMPLETE_OUTCOUNT
                    || Item.itemState == TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_COMPLETE)
                {
                    //일부 매수, 매수완료 일때
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
            CoreEngine.GetInstance().SendLogMessage("Push Martin GailItem Try");
            step++;

            if (Item != null && Item.itemCode == itemCode)
            {
                CoreEngine.GetInstance().SendLogWarningMessage("Push Martin GailItem");
                Item.martinState = MARTIN_RESULT.HAVE_ITEM;
                Item.step = step;
                Item.TodayIndex = todayAllItems.Count;

                todayAllCode.Add(itemCode);
                martinGailStack.Push(Item);
                todayAllItems.Add(Item);
            }

        }

        public void PopMartinGailItem(long profit)
        {
            CoreEngine.GetInstance().SendLogMessage("PopMartinGailItem");
            Item = null;
            if(martinGailStack.Count == 0)
            {
                CoreEngine.GetInstance().SendLogWarningMessage("!!!!!!!!!!empty stack!!!!!!");
                RestartSameStep();
                return;
            }
            MartinGailItem itempop = martinGailStack.Pop();

            TodayAllTry++;

            if (profit > 0)
            {
                if(itempop != null)
                {
                    itempop.martinState = MARTIN_RESULT.WIN;
                    winCount++;
                    Restart();
                }
            }else if(profit == 0)
            {
                drawCount++;
                if (itempop != null)
                {
                    itempop.martinState = MARTIN_RESULT.DRAW;
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
            Item = null;
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

                if(Item != null && Item.itemCode == itemCode)
                {
                    Item.curPrice = c_lPrice;
                }
            }
        }
        private void API_OnReceiveChejanData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveChejanDataEvent e)
        {
            CoreEngine.GetInstance().SendLogMessage("API_OnReceiveChejanData");
            if (e.sGubun.Equals(ConstName.RECEIVE_CHEJAN_DATA_SUBMIT_OR_CONCLUSION))
            {
                CoreEngine.GetInstance().SendLogMessage("RECEIVE_CHEJAN_DATA_SUBMIT_OR_CONCLUSION");
                string orderState = axKHOpenAPI1.GetChejanData(913).Trim(); 
                string outstanding = axKHOpenAPI1.GetChejanData(902).Trim();
                string orderType = axKHOpenAPI1.GetChejanData(905).Replace("+", "").Replace("-", "").Trim();
                string ordernum = axKHOpenAPI1.GetChejanData(9203).Trim();
                string itemCode = axKHOpenAPI1.GetChejanData(9001).Replace("A", "");

                string conclusionPrice = axKHOpenAPI1.GetChejanData(910).Trim();
                string conclusionQuantity = axKHOpenAPI1.GetChejanData(911).Trim();

                if (orderState.Equals(ConstName.RECEIVE_CHEJAN_DATA_SUBMIT))
                {
                    CoreEngine.GetInstance().SendLogMessage("RECEIVE_CHEJAN_DATA_SUBMIT");
                    if (orderType.Equals(ConstName.RECEIVE_CHEJAN_DATA_BUY))
                    {
                        CoreEngine.GetInstance().SendLogMessage("RECEIVE_CHEJAN_DATA_BUY : "+ ordernum);
                        CoreEngine.GetInstance().SendLogWarningMessage("conclusionQuantity : " + conclusionQuantity);
                        if (tradingStrategy == null)
                            return;

                        List<TradingItem> tradeItemArray = tradingStrategy.tradingItemList.FindAll(o => o.itemCode.Equals(itemCode));
                        if (tradeItemArray.Count > 0)
                        {
                            foreach(var item in tradeItemArray)
                            {
                                if (Item != null && Item.itemCode == itemCode && string.IsNullOrEmpty(item.buyOrderNum) == false)
                                {
                                    Item.buyOrderNum = item.buyOrderNum;
                                }
                            }
                        }
                    }
                    else if (orderType.Equals(ConstName.RECEIVE_CHEJAN_DATA_SELL))
                    {
                        CoreEngine.GetInstance().SendLogMessage("RECEIVE_CHEJAN_DATA_SELL");
                    }
                    else if (orderType.Equals(ConstName.RECEIVE_CHEJAN_CANCEL_BUY_ORDER))
                    {
                        CoreEngine.GetInstance().SendLogMessage("RECEIVE_CHEJAN_CANCEL_BUY_ORDER");
                    }
                    else if (orderType.Equals(ConstName.RECEIVE_CHEJAN_CANCEL_SELL_ORDER))
                    {
                        CoreEngine.GetInstance().SendLogMessage("RECEIVE_CHEJAN_CANCEL_SELL_ORDER");
                    }
                }
                else if (orderState.Equals(ConstName.RECEIVE_CHEJAN_DATA_CONCLUSION))
                {
                    CoreEngine.GetInstance().SendLogMessage("RECEIVE_CHEJAN_DATA_CONCLUSION");
                    if (orderType.Contains(ConstName.RECEIVE_CHEJAN_DATA_BUY))
                    {
                        CoreEngine.GetInstance().SendLogMessage("RECEIVE_CHEJAN_DATA_BUY");
                        if (tradingStrategy == null)
                                return;
                        CoreEngine.GetInstance().SendLogWarningMessage("RECEIVE_CHEJAN_DATA_BUY ORDER NUM : " + ordernum);
                        CoreEngine.GetInstance().SendLogWarningMessage("conclusionQuantity : " + conclusionQuantity);

                        TradingItem tradeItem = tradingStrategy.tradingItemList.Find(o => o.buyOrderNum.Equals(ordernum));

                        if (tradeItem != null && string.IsNullOrEmpty(conclusionQuantity) == false)
                        {
                            if (Item != null)
                            {
                                CoreEngine.GetInstance().SendLogMessage(Item.curQnt + "/" + Item.buyQnt);

                                Item.curQnt = long.Parse(conclusionQuantity); 
                                Item.buyPrice = tradeItem.buyingPrice;
                                Item.buyOrderNum = tradeItem.buyOrderNum;
                                if(Item.curQnt == Item.buyQnt)
                                  PushMartinGailItem(itemCode);
                            }
                        }
                        else
                        {
                            CoreEngine.GetInstance().SendLogWarningMessage("tradeItem is null ");
                        }
                    }
                    else if (orderType.Contains(ConstName.RECEIVE_CHEJAN_DATA_SELL))
                    {
                        if (tradingStrategy == null)
                            return;

                        TradingItem tradeItem = tradingStrategy.tradingItemList.Find(o => o.sellOrderNum.Equals(ordernum));
                        if (tradeItem != null  && string.IsNullOrEmpty(outstanding) == false && string.IsNullOrEmpty(conclusionPrice) == false)
                        {
                            long buyingPrice = tradeItem.buyingPrice;
                            long sellPrice = long.Parse(conclusionPrice.Replace("+", ""));

                            if (long.Parse(outstanding) == 0)
                            {
                                CoreEngine.GetInstance().SendLogMessage("Outstanding 0 : Profit : " + (sellPrice - buyingPrice) * tradeItem.buyingQnt);
                                TodayAllProfitAmount += (sellPrice - buyingPrice) * tradeItem.buyingQnt;
                                PopMartinGailItem((sellPrice - buyingPrice));
                            }
                            else
                            {
                                if (Item != null)
                                {
                                    Item.curQnt = long.Parse(outstanding);
                                    Item.sellOrderNum = tradeItem.sellOrderNum;
                                }
                            }
                        }
                    }
                }
                else if (orderState.Equals(ConstName.RECEIVE_CHEJAN_DATA_OK))
                {
                    CoreEngine.GetInstance().SendLogMessage("RECEIVE_CHEJAN_DATA_OK");
                    if (orderType.Contains(ConstName.RECEIVE_CHEJAN_CANCEL_BUY_ORDER))
                    {
                        CoreEngine.GetInstance().SendLogMessage("RECEIVE_CHEJAN_CANCEL_BUY_ORDER");
                        if (int.Parse(outstanding) == 0)
                        {
                            if(Item !=  null)
                                PopMartinGailItem(0);
                        }
                    }
                    else if (orderType.Contains(ConstName.RECEIVE_CHEJAN_CANCEL_SELL_ORDER))
                    {
                        CoreEngine.GetInstance().SendLogMessage("RECEIVE_CHEJAN_CANCEL_SELL_ORDER");
                        if (int.Parse(outstanding) == 0)
                        {
                            if (Item != null)
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
            if (Item == null)
                return;
            CoreEngine.GetInstance().SendLogMessage("!!!!!!!!!!!! cancel code : " + Item.itemCode);
            CoreEngine.GetInstance().SendLogMessage("!!!!!!!!!!!! cancel orderNum : " + Item.buyOrderNum);
            CoreEngine.GetInstance().SendLogMessage("!!!!!!!!!!!! cancel quantity : " + (Item.buyQnt - Item.curQnt).ToString());
            form1.CancelBuyOrder(Item.itemCode, Item.buyOrderNum);
        }
        private void SellAllClear()
        {
            if (Item == null)
                return;
            CoreEngine.GetInstance().SendLogMessage("!!!!!!!!!!!! SellAllClear code : " + Item.itemCode);
            CoreEngine.GetInstance().SendLogMessage("!!!!!!!!!!!! SellAllClear orderNum : " + Item.buyOrderNum);
            CoreEngine.GetInstance().SendLogMessage("!!!!!!!!!!!! SellAllClear quantity : " + Item.curQnt.ToString());
            form1.SellAllClear(Item.itemCode, (int)Item.curQnt, form1.ReceiveSellAllClear);
        }
        private void CancelBeforeBuyOrder()
        {
            if (Item == null)
                return;
            CoreEngine.GetInstance().SendLogMessage("!!!!!!!!!!!! CancelBeforeBuyOrder cancel code : " + Item.itemCode);
            CoreEngine.GetInstance().SendLogMessage("!!!!!!!!!!!! CancelBeforeBuyOrder cancel orderNum : " + Item.buyOrderNum);
            CoreEngine.GetInstance().SendLogMessage("!!!!!!!!!!!! CancelBeforeBuyOrder cancel quantity : " + (Item.buyQnt - Item.curQnt).ToString());
            //form1.CancelBuyOrder(Item.itemCode, Item.buyOrderNum);
            if (tradingStrategy != null)
            {
                List<TradingItem> tradeItemListAll = tradingStrategy.tradingItemList.FindAll(o=>(o.itemCode == Item.itemCode));

                foreach (TradingItem tradeItem in tradeItemListAll)
                {
                    if(tradeItem.state == TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_BEFORE_ORDER)
                    {
                        tradeItem.SetBuyCancelComplete();
                        PopMartinGailItem(0);
                        return;
                    }    
                }
            }
        }
        private void CancelTrailing()
        {
            if (Item == null)
                return;
            CoreEngine.GetInstance().SendLogMessage("!!!!!!!!!!!! CancelBeforeBuyOrder cancel code : " + Item.itemCode);
            CoreEngine.GetInstance().SendLogMessage("!!!!!!!!!!!! CancelBeforeBuyOrder cancel orderNum : " + Item.buyOrderNum);
            CoreEngine.GetInstance().SendLogMessage("!!!!!!!!!!!! CancelBeforeBuyOrder cancel quantity : " + (Item.buyQnt - Item.curQnt).ToString());
            //form1.CancelBuyOrder(Item.itemCode, Item.buyOrderNum);
            if (tradingStrategy != null)
            {
                List<TradingItem> tradeItemListAll = tradingStrategy.tradingItemList.FindAll(o => (o.itemCode == Item.itemCode));

                foreach (TradingItem tradeItem in tradeItemListAll)
                {
                    if (tradeItem.state == TRADING_ITEM_STATE.AUTO_TRADING_STATE_SEARCH_AND_CATCH)
                    {
                        tradeItem.SetBuyCancelComplete();
                        PopMartinGailItem(0);
                        return;
                    }
                }
            }
        }
        void Update()
        {
            if (Item != null)
            {
                if(Item.itemState == TRADING_ITEM_STATE.AUTO_TRADING_STATE_SEARCH_AND_CATCH)
                {
                    if ((DateTime.Now - startOrderTime).TotalSeconds > Wait_And_CancelValue)
                    {
                        CoreEngine.GetInstance().SendLogMessage("트레일링 취소");
                        CancelTrailing();
                    }
                }
                if (Item.itemState == TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_NOT_COMPLETE ||
                    Item.itemState == TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_NOT_COMPLETE_OUTCOUNT)
                {
                    //Console.WriteLine((DateTime.Now - startOrderTime).ToString());
                    //Console.WriteLine(tradingStrategyGridView.GetProfitRate((double)Item.curPrice, (double)Item.buyPrice).ToString());

                    if (Item.itemState == TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_NOT_COMPLETE_OUTCOUNT)
                    {
                        if (using_Outstand_UpAndCancel && tradingStrategyGridView.GetProfitRate((double)Item.curPrice, (double)Item.buyPrice) > OutStand_And_CancelValue)
                        {
                            CoreEngine.GetInstance().SendLogWarningMessage("!!!!!!!!!!!!using_Outstand_UpAndCancel!!!!!!!!!!!!!!");
                            CancelBuyOrderAll();
                            SellAllClear(); //청산
                        }
                    }
                    else
                    {
                        if (using_Up_And_Cancel && tradingStrategyGridView.GetProfitRate((double)Item.curPrice, (double)Item.buyPrice) > Up_And_CancelValue)
                        {
                            CoreEngine.GetInstance().SendLogMessage("!!!!!!!!!!!!using_Up_And_Cancel Pop MartinGail Item!!!!!!!!!!!!!!");
                            CancelBuyOrderAll();
                        }
                    }

                    if (using_WaitAndCancel && (DateTime.Now - startOrderTime).TotalSeconds > Wait_And_CancelValue)
                    {
                        CoreEngine.GetInstance().SendLogMessage("!!!!!!!!!!!!using_WaitAndCancel Pop MartinGail Item!!!!!!!!!!!!!!");
                        CancelBuyOrderAll();
                    }
                }

                if (Item.itemState == TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_BEFORE_ORDER )
                {
                    //Console.WriteLine("주문접수성공 대기 : " + (DateTime.Now - startOrderTime).ToString());
                    if ((DateTime.Now - startOrderTime).TotalSeconds > Wait_And_CancelValue)
                    {
                        CoreEngine.GetInstance().SendLogMessage("주문접수성공 실패");
                        CancelBeforeBuyOrder();
                    }
                }
            }
        }
    }
}
