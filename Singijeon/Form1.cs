//#define TEST_CONSOLE
using Singijeon.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Singijeon
{
    public partial class tradingStrategyGridView : Form
    {
        CoreEngine coreEngine;
        string currentAccount = string.Empty;

        int screenNum = 1000;
        string server = "0";
        public static double FEE_RATE = 1;

        List<Condition> listCondition = new List<Condition>();
        List<TradingStrategy> tradingStrategyList = new List<TradingStrategy>();
        List<BalanceSellStrategy> balanceSellStrategyList = new List<BalanceSellStrategy>();
        List<TrailingItem> trailingList = new List<TrailingItem>();
        List<StockItem> stockItemList = new List<StockItem>(); //상장종목리스트

        List<TradingItem> tryingOrderList = new List<TradingItem>(); //주문접수시도
        
        //같은 종목에 대하여 주문이 여러개 들어가도 주문순서대로 응답이 오기 때문에 각각의 리스트로 들어가게됨

        List<SettlementItem> tryingSettlementItemList = new List<SettlementItem>(); //청산 접수 시도(주문번호만 따기위한 리스트)
        List<SettlementItem> settleItemList = new List<SettlementItem>(); //진행중인 청산 시도

        List<BalanceSellStrategy> tryingSellList = new List<BalanceSellStrategy>(); //잔고 매도 접수 시도(주문번호 따는 리스트)

        public tradingStrategyGridView()
        {
            InitializeComponent();

            coreEngine = CoreEngine.GetInstance();
            coreEngine.SetAxKHOpenAPI(axKHOpenAPI1);
            coreEngine.Start();

            OpenSecondWindow();

            startTimePicker.Value = DateTime.Now;
            startTimePicker.Format = DateTimePickerFormat.Custom;
            startTimePicker.CustomFormat = "HH:mm"; // Only use hours and minutes
            startTimePicker.ShowUpDown = true;

            endTimePicker.Value = DateTime.Now;
            endTimePicker.Format = DateTimePickerFormat.Custom;
            endTimePicker.CustomFormat = "HH:mm"; // Only use hours and minutes
            endTimePicker.ShowUpDown = true;

            this.FormClosing += Form_FormClosing;

            LogInToolStripMenuItem.Click += ToolStripMenuItem_Click;
            
            AddStratgyBtn.Click += AddStratgyBtn_Click;  //전략생성 버튼
            balanceSellBtn.Click += BalanceSellBtn_Click;

            accountComboBox.SelectedIndexChanged += ComboBoxIndexChanged;

            interestConditionListBox.SelectedIndexChanged += InterestConditionListBox_SelectedIndexChanged;

            accountBalanceDataGrid.CellClick += DataGridView_CellClick;
            autoTradingDataGrid.CellClick    += AutoTradingDataGridView_CellClick;
            tsDataGridView.CellClick         += TradingStrategyGridView_CellClick;

            accountBalanceDataGrid.SelectionChanged += AccountDataGridView_SelectionChanged;

            axKHOpenAPI1.OnEventConnect         += API_OnEventConnect; //로그인
            axKHOpenAPI1.OnReceiveConditionVer  += API_OnReceiveConditionVer; //검색 받기
            axKHOpenAPI1.OnReceiveRealCondition += API_OnReceiveRealCondition; //실시간 검색
            axKHOpenAPI1.OnReceiveTrCondition   += API_OnReceiveTrCondition; //검색

            axKHOpenAPI1.OnReceiveTrData     += API_OnReceiveTrData; //정보요청
            axKHOpenAPI1.OnReceiveTrData     += API_OnReceiveTrDataHoga; //정보요청(호가)
            axKHOpenAPI1.OnReceiveChejanData += API_OnReceiveChejanData; //체결잔고
            axKHOpenAPI1.OnReceiveRealData   += API_OnReceiveRealData; //실시간정보
            axKHOpenAPI1.OnReceiveRealData   += API_OnReceiveRealDataHoga; //실시간정보
            axKHOpenAPI1.OnReceiveRealData   += API_OnReceiveRealDataHoga; //실시간정보

            MartinGailManager.GetInstance().Init(axKHOpenAPI1, this);

            LoadSetting();
        }

        #region EVENT_RECEIVE_FUNCTION
        private void API_OnEventConnect(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnEventConnectEvent e)
        {
            if (e.nErrCode == 0)
            {

                server = axKHOpenAPI1.GetLoginInfo(ConstName.GET_SERVER_TYPE);
                if (server.Equals(ConstName.TEST_SERVER))
                {
                    //모의투자 
                    //FEE_RATE = 1;
                    FEE_RATE = 0.3;
                }
                else
                {
                    FEE_RATE = 0.3;
                }

                string accountList = axKHOpenAPI1.GetLoginInfo(ConstName.GET_ACCOUNT_LIST);
                string[] accountArray = accountList.Split(';');

                foreach (string accountItem in accountArray)
                {
                    if (accountItem.Length > 0)
                    {
                        accountComboBox.Items.Add(accountItem);
                    }
                }
                string codeList = axKHOpenAPI1.GetCodeListByMarket(null);
                string[] codeArray = codeList.Split(';');

                AutoCompleteStringCollection collection = new AutoCompleteStringCollection();

                foreach (string code in codeArray)
                {
                    string name = axKHOpenAPI1.GetMasterCodeName(code);
                    StockItem stockItem = new StockItem() { Code = code, Name = name };
                    stockItemList.Add(stockItem);
                    collection.Add(name);
                }

                interestTextBox.AutoCompleteCustomSource = collection;


                //사용자 조건식 불러오기
                axKHOpenAPI1.GetConditionLoad();
            }

        }
        private void API_OnReceiveConditionVer(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveConditionVerEvent e)
        {

            string conditionList = axKHOpenAPI1.GetConditionNameList();
            string[] conditionArray = conditionList.Split(';');

            listCondition.Clear();

            foreach (string conditionItem in conditionArray)
            {
                if (conditionItem.Length > 0)
                {
                    string[] conditionInfo = conditionItem.Split('^');

                    if (conditionInfo.Length == 2)
                    {
                        string index = conditionInfo[0];
                        string name = conditionInfo[1];

                        Condition condition = new Condition(int.Parse(index), name);
                        listCondition.Add(condition);
                    }
                }
            }

            foreach (Condition condition in listCondition)
            {
                BuyConditionComboBox.Items.Add(condition.Name);
                M_BuyConditionComboBox.Items.Add(condition.Name);

                interestConditionListBox.Items.Add(condition.Name);
            }
        }

        public bool CheckCanBuyItem(TradingStrategy ts, string itemCode)
        {
            //한 전략에서 매도 완료됬거나 매수취소된것
            bool returnBuy = true;
           List<TradingItem> tradeItemArray = ts.tradingItemList.FindAll(o => o.itemCode.Contains(itemCode)); 
           foreach(var item in tradeItemArray)
            {
                bool canBuy =  item.IsCompleteSold() || item.IsBuyCancel();
                if (!canBuy)
                    returnBuy = canBuy;
            }
            return returnBuy;
        }

        //검색에 편입시 호출
        private void API_OnReceiveRealCondition(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveRealConditionEvent e)
        {
            string itemCode = e.sTrCode;
            string conditionName = e.strConditionName;
            string conditionIndex = e.strConditionIndex;
       
             if (e.strType.Equals(ConstName.RECEIVE_REAL_CONDITION_INSERTED))
            {
                coreEngine.SendLogMessage("________실시간 검색 종목_________");
                coreEngine.SendLogMessage("검색명 = " + conditionName);
                //coreEngine.SendLogMessage("itemCode = " + itemCode);
                //coreEngine.SendLogMessage("검색 종류 = " + e.strType);
                coreEngine.SendLogMessage("종목명 = " + axKHOpenAPI1.GetMasterCodeName(itemCode));
                coreEngine.SendLogMessage("_________________________________");

                //종목 편입(어떤 전략(검색식)이었는지)
                TradingStrategy ts = tradingStrategyList.Find(o => o.buyCondition.Name.Equals(conditionName));

                if (ts != null)
                {
                    coreEngine.SendLogMessage("남은 가능 매수 종목수 : " + ts.remainItemCount);
                    if (ts.remainItemCount > 0)
                    {
                        StockItem stockItem = stockItemList.Find(o => o.Code.Equals(itemCode));
                        if (stockItem != null) //시장 종목 리스트 있는것
                        {
                   
                            if (ts.CheckBuyPossibleStrategyAddedItem()) //모든 구매조건을 체크
                            {
                                TradingItem tradeItem = ts.tradingItemList.Find(o => o.itemCode.Contains(itemCode)); //한 전략에서 구매하려했던 종목은 재편입하지 않음
                                TrailingItem trailingItem = trailingList.Find(o => o.itemCode.Contains(itemCode));


                                if (CheckCanBuyItem(ts, itemCode) && trailingItem == null)
                                {
                                    ts.remainItemCount--; //남을 매수할 종목수-1

                                    ts.StrategyConditionReceiveUpdate(itemCode, 0, 0, TRADING_ITEM_STATE.AUTO_TRADING_STATE_SEARCH_AND_CATCH);

                                    if (ts.usingTickBuy || ts.usingTrailing)
                                    {
                                        coreEngine.SendLogMessage("호가 체크 후 매수 ");
                                        //종목의 호가를 알아오고 틱 설정 단위로 산다
                                        Task requestItemInfoTask = new Task(() =>
                                        {
                                            coreEngine.SendLogMessage(ConstName.RECEIVE_TR_DATA_HOGA + "요청 종목코드 : " + itemCode);
                                            axKHOpenAPI1.SetInputValue("종목코드", itemCode);

                                            int result = axKHOpenAPI1.CommRqData(ConstName.RECEIVE_TR_DATA_HOGA + ":" + itemCode  +":"+ ts.buyCondition.Uid , "opt10004", 0, GetScreenNum().ToString());

                                            if (result == ErrorCode.정상처리)
                                            {
                                                coreEngine.SendLogMessage(ConstName.RECEIVE_TR_DATA_HOGA + " 성공");
                                            }
                                            else
                                            {
                                                coreEngine.SendLogMessage(ConstName.RECEIVE_TR_DATA_HOGA + " 실패");
                                            }
                                        });
                                        coreEngine.requestTrDataManager.RequestTrData(requestItemInfoTask);
                                    }
                                    else
                                    {
                                        coreEngine.SendLogMessage("즉시 매수 ");
                                        //종목의 현재가를 알아오고 그가격으로 산다
                                        Task requestItemInfoTask = new Task(() =>
                                        {
                                            coreEngine.SendLogMessage("종목코드 : " + itemCode);
                                            axKHOpenAPI1.SetInputValue("종목코드", itemCode);

                                            int result = axKHOpenAPI1.CommRqData(ConstName.RECEIVE_TR_DATA_BUY_INFO + ":" + ts.buyCondition.Uid, "opt10001", 0, GetScreenNum().ToString());

                                            if (result == ErrorCode.정상처리)
                                            {
                                                coreEngine.SendLogMessage(ConstName.RECEIVE_TR_DATA_BUY_INFO + " 성공");
                                            }
                                            else
                                            {
                                                coreEngine.SendLogMessage(ConstName.RECEIVE_TR_DATA_BUY_INFO + " 실패");
                                            }
                                        });
                                        coreEngine.requestTrDataManager.RequestTrData(requestItemInfoTask);
                                    }

                                }
                            }
                        }
                    }
                }

            }
            else if (e.strType.Equals(ConstName.RECEIVE_REAL_CONDITION_DELETE))
            {
                //종목 이탈
            }
        }
        private void API_OnReceiveTrData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            if (e.sRQName.Contains(ConstName.RECEIVE_TR_DATA_BUY_INFO))
            {
                //검색 ->검색완료 -> 매수주문 -> 현재가격을 얻어오기위해 tr요청-> 이때 "매수종목정보요청:검색넘버" 로 요청

                string[] rqNameArray = e.sRQName.Split(':');
                if (rqNameArray.Length == 2)
                {
                    string conditionUid = rqNameArray[1];
                    TradingStrategy ts = tradingStrategyList.Find(o => o.buyCondition.Uid == conditionUid);

                    if (ts != null)
                    {
                        coreEngine.SendLogMessage("요청 매수검색식 : " + ts.buyCondition.Name);

                        string itemcode = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "종목코드").Trim();
                        string price = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "현재가").Trim();

                        int i_price = 0;
                        int i_qnt = 0;

                        if (int.TryParse(price, out i_price))
                        {
                            i_price = Math.Abs(i_price);
                            i_qnt = (int)(ts.itemInvestment / i_price);

                            if (i_price > 0)
                            {
                                coreEngine.SendLogMessage("종목 매수 : " + axKHOpenAPI1.GetMasterCodeName(itemcode));

                                if(server.Equals(ConstName.TEST_SERVER ))
                                {
                                    coreEngine.SendLogMessage("모의투자 환경에서 \n 현재가 1,000원 미만인 종목, \n 총 발행 주식수 100,000주 미만 종목, \n 프리보드 종목, \n 관리종목, \n 정리매매, \n 투자주의, \n 투자경고, \n 투자위험종목, \n ELW종목 은 주문제외됩니다");
                                }

                                int orderResult =

                                axKHOpenAPI1.SendOrder(
                                    "편입종목매수",
                                    GetScreenNum().ToString(),
                                    ts.account,
                                    CONST_NUMBER.SEND_ORDER_BUY,//1:신규매수
                                    itemcode,
                                    (int)(ts.itemInvestment / i_price),
                                    i_price,
                                    ConstName.ORDER_JIJUNGGA,//지정가
                                    "" //원주문번호없음
                                );

                                if (orderResult == 0)
                                {
                                    coreEngine.SendLogMessage("매수주문 성공");

                                    TradingItem tradingItem = new TradingItem(ts, itemcode, axKHOpenAPI1.GetMasterCodeName(itemcode), i_price, i_qnt, false, false, ConstName.ORDER_JIJUNGGA);
                                    tradingItem.SetBuy(true);
                                    tradingItem.SetConditonUid(conditionUid);

                                    ts.tradingItemList.Add(tradingItem); //매수전략 내에 매매진행 종목 추가

                                    this.tryingOrderList.Add(tradingItem);

                                    string fidList = "9001;302;10;11;25;12;13"; //9001:종목코드,302:종목명
                                    axKHOpenAPI1.SetRealReg("9001", itemcode, fidList, "1");

                                    //매매진행 데이터 그리드뷰 표시

                                    int addRow = autoTradingDataGrid.Rows.Add();

                                    UpdateAutoTradingDataGridRowAll(addRow, ConstName.AUTO_TRADING_STATE_BUY_BEFORE_ORDER, itemcode, ts.buyCondition.Name, i_qnt, i_price);

                                    tradingItem.SetUiConnectRow(autoTradingDataGrid.Rows[addRow]);

                                    ts.StrategyBuyOrderUpdate(itemcode, i_price, i_qnt, TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_BEFORE_ORDER);
                                    coreEngine.SendLogMessage("자동 매수 요청 - " + "종목코드 : " + itemcode + " 주문가 : " + i_price + " 주문수량 : " + i_qnt + " 매수조건식 : " + ts.buyCondition.Name);
                                }
                            }
                        }
                        else
                        {
                            coreEngine.SendLogMessage("현재가 받기 실패");
                        }
                    }
                }
            }
            else if (e.sRQName.Contains(ConstName.RECEIVE_TR_DATA_ACCOUNT_INFO))
            {

                string codeList = string.Empty;
                int cnt = axKHOpenAPI1.GetRepeatCnt(e.sTrCode, e.sRQName); //조회내용중 멀티데이터의 갯수를 알아온다

                for (int i = 0; i < cnt; ++i)
                {
                    string itemCode = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "종목코드").Trim();
                    codeList += itemCode;
                    if (i != cnt - 1)
                    {
                        codeList += ";";
                    }
                    string itemName = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "종목명").Trim();

                    string balanceCnt = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "보유수량").Trim();
                    long lBalanceCnt = long.Parse(balanceCnt);

                    string buyingPrice = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "평균단가").Trim();
                    double dBuyingPrice = double.Parse(buyingPrice);

                    string price = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "현재가").Trim();
                    int iPrice = Math.Abs(int.Parse(price));

                    string estimatedAmount = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "평가금액").Trim();
                    long lEstimatedAmount = long.Parse(estimatedAmount);

                    string profitAmount = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "손익금액").Trim();
                    long lProfitAmount = long.Parse(profitAmount);

                    string buyingAmount = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "매입금액").Trim();
                    long lBuyingAmount = long.Parse(buyingAmount);

                    //double dProfitRate = 100 * ((iPrice - dBuyingPrice) / dBuyingPrice) - FEE_RATE;
                    double dProfitRate = GetProfitRate((double)iPrice, dBuyingPrice);
                    int rowIndex = accountBalanceDataGrid.Rows.Add();

                    Hashtable uiTable = new Hashtable() { { "계좌잔고_종목코드", itemCode }, { "계좌잔고_종목명", itemName }, { "계좌잔고_보유수량", lBalanceCnt }, { "계좌잔고_평균단가", dBuyingPrice }, { "계좌잔고_평가금액", lEstimatedAmount }, { "계좌잔고_매입금액", lBuyingAmount }, { "계좌잔고_손익금액", lProfitAmount }, { "계좌잔고_손익률", dProfitRate } };
                    Update_AccountBalanceDataGrid_UI(uiTable, rowIndex);

                }
                string fidList = "9001;302;10;11;25;12;13"; //9001:종목코드,302:종목명
                axKHOpenAPI1.SetRealReg("9002", codeList, fidList, "1");
            }
            else if (e.sRQName == "실시간미체결요청")
            {
               
                int count = axKHOpenAPI1.GetRepeatCnt(e.sTrCode, e.sRQName);
              
                for (int i = 0; i < count; i++)
                {
                    string orderCode = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "주문번호")).ToString();
                    string stockCode = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "종목코드").Trim();
                    string stockName = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "종목명").Trim();
                    int orderNumber = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "주문수량"));
                    int orderPrice = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "주문가격"));
                    int outstandingNumber = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "미체결수량"));
                    int currentPrice = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "현재가").Replace("-", ""));
                    string orderGubun = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "주문구분").Trim();
                    string orderTime = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "시간").Trim();
                    coreEngine.SendLogWarningMessage("실시간미체결요청 orderNum :" + orderCode);
                }
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

                //종목의 매매전략 얻어오기
                //모든 매매전략내 전략에 포함된 종목을 찾고, 매매전략의 손익률 셋팅과 비교
                //삭제 예정(잔고 매도로 통합)
                List<TradingItem> tradeItemListAll = GetAllTradingItemData(itemCode);

                foreach (TradingItem tradeItem in tradeItemListAll)
                {
                    tradeItem.UpdateCurrentPrice(c_lPrice);

                    if (tradeItem.IsCompleteBuying() && tradeItem.IsCompleteSold() == false && tradeItem.buyingPrice != 0) //매도 진행안된것 
                    {
                        double realProfitRate = GetProfitRate((double)c_lPrice, (double)tradeItem.buyingPrice);

                    
                        //자동 감시 주문 체크
                        if(tradeItem.state >= TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_COMPLETE
                            && tradeItem.state < TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_COMPLETE)
                            tradeItem.ts.CheckUpdateTradingStrategyAddedItem(tradeItem, realProfitRate, CHECK_TIMING.SELL_TIME);

                        if (tradeItem.ts.usingRestart && tradeItem.ts.remainItemCount == 0) //restart 처리
                        {
                            tradeItem.ts.remainItemCount = tradeItem.ts.buyItemCount;
                        }
                    }
                }

                List<BalanceSellStrategy> bssArray = balanceSellStrategyList.FindAll(o => o.itemCode.Equals(itemCode));
                foreach (BalanceSellStrategy bss in bssArray)
                {
                    if (bss != null)
                    {
                        if (!bss.isSold && bss.buyingPrice != 0)
                        {
                            double profitRate = GetProfitRate((double)c_lPrice, (double)bss.buyingPrice);
                            if (bss.takeProfitRate <= profitRate) //익절
                            {
                                int orderResult = axKHOpenAPI1.SendOrder(
                                                  "잔고익절매도",
                                                  GetScreenNum().ToString(),
                                                  bss.account,
                                                  CONST_NUMBER.SEND_ORDER_SELL,
                                                  itemCode,
                                                  (int)bss.sellQnt,
                                                  (int)c_lPrice,
                                                  "00",//지정가
                                                  "" //원주문번호없음
                                              );
                                if (orderResult == 0) //요청 성공시 (실거래는 안될 수 있음)
                                {

                                    bss.isSold = true;
                                    tryingSellList.Add(bss);
                                    coreEngine.SendLogMessage("ui -> 매도주문접수시도");
                                    UpdateAutoTradingDataGridRowSellStrategy(itemCode, ConstName.AUTO_TRADING_STATE_SELL_BEFORE_ORDER);
                                }
                                else
                                {
                                    coreEngine.SendLogMessage("잔고 익절 요청 실패");
                                }
                            }
                            else if (bss.stoplossRate > profitRate) //손절
                            {
                                int orderResult = axKHOpenAPI1.SendOrder(
                                                     "잔고손절매도",
                                                     GetScreenNum().ToString(),
                                                     bss.account,
                                                     CONST_NUMBER.SEND_ORDER_SELL,
                                                     itemCode,
                                                     (int)bss.sellQnt,
                                                     (int)c_lPrice,
                                                     "03",//시장가
                                                     "" //원주문번호없음
                                                 );
                                if (orderResult == 0) //요청 성공시 (실거래는 안될 수 있음)
                                {
                                    bss.isSold = true;
                                    tryingSellList.Add(bss);
                                    coreEngine.SendLogMessage("ui -> 매도주문접수시도");
                                    UpdateAutoTradingDataGridRowSellStrategy(itemCode, ConstName.AUTO_TRADING_STATE_SELL_BEFORE_ORDER);
                                }
                                else
                                {
                                    coreEngine.SendLogMessage("잔고 손절 요청 실패");
                                }
                            }
                        }
                    }
                }



                UpdateAccountBalanceDataGridViewRow(itemCode, c_lPrice);

                UpdateBalanceDataGridViewRow(itemCode, c_lPrice);

                UpdateAutoTradingDataGridViewRow(itemCode, c_lPrice);
            }
        }
        private List<TradingItem> GetAllTradingItemData(string itemCode)
        {
            List<TradingItem> returnList = new List<TradingItem>();

            foreach (TradingStrategy ts in tradingStrategyList)
            {
                List<TradingItem> tradeItemList = ts.tradingItemList.FindAll(o => o.itemCode.Equals(itemCode));
                if (tradeItemList != null && tradeItemList.Count > 0) //매매 진행 종목을 찾았을 경우
                {
                    foreach (TradingItem tradeItem in tradeItemList)
                    {
                        returnList.Add(tradeItem);
                    }
                }
            }
            return returnList;
        }
        private void UpdateAutoTradingDataGridRow(string itemCode, TradingItem tradeItem, long c_lPrice, string curState)
        {
            if (tradeItem != null && tradeItem.GetUiConnectRow() != null)
            {
                tradeItem.GetUiConnectRow().Cells["매매진행_진행상황"].Value = curState;
            }
           
        }

        private void UpdateAutoTradingDataGridRowSellStrategy(string itemCode, string changeState)
        {
            foreach (DataGridViewRow row in autoTradingDataGrid.Rows)
            {
                if (row.Cells["매매진행_종목코드"].Value != null)
                {
                    if (row.Cells["매매진행_종목코드"].Value.ToString().Contains(itemCode))
                    {
                        row.Cells["매매진행_진행상황"].Value = changeState;
                    }
                }
            }
        }
        //실시간 종목 조회 응답시//
        private void UpdateAccountBalanceDataGridViewRow(string itemCode, long c_lPrice)
        {
            foreach (DataGridViewRow row in accountBalanceDataGrid.Rows)
            {
                if (row.Cells["계좌잔고_종목코드"].Value != null)
                {
                    if (row.Cells["계좌잔고_종목코드"].Value.ToString().Contains(itemCode))
                    {
                        row.Cells["계좌잔고_현재가"].Value = c_lPrice;

                        double buyingPrice = double.Parse(row.Cells["계좌잔고_평균단가"].Value.ToString());
                        int balanceCount = int.Parse(row.Cells["계좌잔고_보유수량"].Value.ToString());

                        double currentAllPrice = c_lPrice * balanceCount;

                        if (buyingPrice != 0)
                        {
                            row.Cells["계좌잔고_평균단가"].Value = buyingPrice;
                            row.Cells["계좌잔고_평가금액"].Value = currentAllPrice;

                            double sellPrice = buyingPrice; // 평단가 
                            double stockFee = ((double)c_lPrice * 0.01 * FEE_RATE) * (double)balanceCount; //+ ((double)c_lPrice * 0.01 * 0.015 * (double)balanceCount); //+ ((double)buyingPrice * 0.01 * 0.015 * (double)balanceCount);
                            double allSellPrice = (sellPrice * (double)balanceCount) + stockFee;

                            row.Cells["계좌잔고_손익금액"].Value = currentAllPrice - allSellPrice;

                            double profitRate = GetProfitRate((double)c_lPrice, (double)sellPrice);
                            row.Cells["계좌잔고_손익률"].Value = profitRate;
                        }
                    }
                }
            }
        }
        private void UpdateBalanceDataGridViewRow(string itemCode, long c_lPrice)
        {
            foreach (DataGridViewRow row in balanceDataGrid.Rows)
            {
                if (row.Cells["잔고_종목코드"].Value != null)
                {
                    if (row.Cells["잔고_종목코드"].Value.ToString().Contains(itemCode))
                    {
                        row.Cells["잔고_현재가"].Value = c_lPrice;

                        double buyingPrice = double.Parse(row.Cells["잔고_매입단가"].Value.ToString());

                        if (buyingPrice != 0)
                        {
                            double profitRate = GetProfitRate((double)c_lPrice, (double)buyingPrice);

                            row.Cells["잔고_손익률"].Value = profitRate;
                        }

                    }
                }
            }
        }
        private void UpdateAutoTradingDataGridViewRow(string itemCode, long c_lPrice)
        {
            foreach (DataGridViewRow row in autoTradingDataGrid.Rows)
            {
                if (row.Cells["매매진행_종목코드"].Value != null)
                {
                    if (row.Cells["매매진행_종목코드"].Value.ToString().Contains(itemCode))
                    {
                        double buyingPrice = double.Parse(row.Cells["매매진행_매수가"].Value.ToString());
                        row.Cells["매매진행_현재가"].Value = c_lPrice;
                        if (buyingPrice != 0)
                        {
                            double profitRate = GetProfitRate((double)c_lPrice, (double)buyingPrice);
                            row.Cells["매매진행_손익률"].Value = profitRate;
                        }
                        
                    }
                }
            }
        }
        private void API_OnReceiveChejanData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveChejanDataEvent e)
        {
            coreEngine.SendLogMessage(e.sGubun);

            if (e.sGubun.Equals(ConstName.RECEIVE_CHEJAN_DATA_SUBMIT_OR_CONCLUSION))
            {
                //접수 혹은 체결
                string orderState = axKHOpenAPI1.GetChejanData(913).Trim();
                string orderType = axKHOpenAPI1.GetChejanData(905).Replace("+","").Replace("-","").Trim();

                string account = axKHOpenAPI1.GetChejanData(9201);
                string ordernum = axKHOpenAPI1.GetChejanData(9203).Trim();
                string itemCode = axKHOpenAPI1.GetChejanData(9001).Replace("A", "").Trim();
              
                string itemName = axKHOpenAPI1.GetChejanData(302).Trim();
                string orderQuantity = axKHOpenAPI1.GetChejanData(900).Trim();
                int i_orderQuantity = int.Parse(orderQuantity);
                string orderPrice = axKHOpenAPI1.GetChejanData(901).Trim();
                int i_orderPrice = int.Parse(orderPrice);
                string outstanding = axKHOpenAPI1.GetChejanData(902).Trim();
              
                string tradingType = axKHOpenAPI1.GetChejanData(906);
                string time = axKHOpenAPI1.GetChejanData(908);
                string conclusionPrice = axKHOpenAPI1.GetChejanData(910);
                string conclusionQuantity = axKHOpenAPI1.GetChejanData(911);
                string unitConclusionQuantity = axKHOpenAPI1.GetChejanData(915);
                string price = axKHOpenAPI1.GetChejanData(10).Trim();

                coreEngine.SendLogMessage("___________접수/체결_____________");
                coreEngine.SendLogMessage("종목명 : " + axKHOpenAPI1.GetMasterCodeName(itemCode));
                coreEngine.SendLogMessage("주문상태 : " + orderState);
                coreEngine.SendLogMessage("주문번호 : " + ordernum);
                coreEngine.SendLogMessage("종목코드 : " + itemCode);
                coreEngine.SendLogMessage("주문구분 : " + orderType);
                coreEngine.SendLogMessage("매매구분 : " + tradingType);
                coreEngine.SendLogMessage("주문수량 : " + orderQuantity);
                coreEngine.SendLogMessage("체결량(누적체결량) :" + conclusionQuantity);
                coreEngine.SendLogMessage("미체결 수량 :" + outstanding);
                coreEngine.SendLogMessage("단위체결량(체결당 체결량) :" + unitConclusionQuantity);
                coreEngine.SendLogMessage("________________________________");

                if (orderState.Equals(ConstName.RECEIVE_CHEJAN_DATA_SUBMIT))
                {
                    TradingItem CheckItmeExist = this.tryingOrderList.Find(o => (itemCode.Contains(o.itemCode)));
                    //주문번호 따오기 위한 부분 
                    if (CheckItmeExist != null) { 
                        if (orderType.Equals(ConstName.RECEIVE_CHEJAN_DATA_BUY))
                        {
                            coreEngine.SendLogWarningMessage(axKHOpenAPI1.GetMasterCodeName(itemCode) + "주문접수완료");
                            coreEngine.SendLogWarningMessage("수량 : "+ i_orderQuantity);
                            TradingItem item = this.tryingOrderList.Find(o => (itemCode.Contains(o.itemCode)));

                            if (item == null)
                                return;

                            coreEngine.SendLogWarningMessage("찾아낸 종목명 : " + axKHOpenAPI1.GetMasterCodeName(itemCode) + "orderNum : " + ordernum);
                            coreEngine.SendLogWarningMessage("찾아낸 종목 주문 수량 : " + item.buyingQnt);
                            item.buyingPrice = long.Parse(orderPrice);
                            item.buyOrderNum = ordernum;
                            item.buyingQnt = int.Parse(orderQuantity);
                            item.SetState(TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_NOT_COMPLETE);

                            this.tryingOrderList.Remove(item); //접수리스트에서만 지움

                            UpdateBuyAutoTradingDataGridStateOnly(ordernum, ConstName.AUTO_TRADING_STATE_BUY_NOT_COMPLETE);

                            item.ts.StrategyBuyOrderUpdate(item.itemCode, (int)item.buyingPrice, item.buyingQnt, TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_NOT_COMPLETE);
                            coreEngine.SendLogMessage("자동 매수 요청 - " + "종목코드 : " + itemCode + " 주문번호 : " + ordernum);
                        }
                        else if (orderType.Equals(ConstName.RECEIVE_CHEJAN_DATA_SELL))
                        {
                            TradingItem item = this.tryingOrderList.Find(o => (itemCode.Contains(o.itemCode)));
                            if (item == null)
                                return;

                            item.sellPrice = long.Parse(orderPrice);
                            item.sellOrderNum = ordernum;
                            item.sellQnt = int.Parse(orderQuantity);
                            item.SetState(TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_NOT_COMPLETE);

                            this.tryingOrderList.Remove(item); //접수리스트에서만 지움

                            UpdateSellAutoTradingDataGridStateOnly(ordernum, ConstName.AUTO_TRADING_STATE_SELL_NOT_COMPLETE);
                            item.ts.StrategyOnReceiveSellOrderUpdate(item.itemCode, (int)item.buyingPrice, item.buyingQnt, TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_NOT_COMPLETE);
                            coreEngine.SendLogMessage("자동 매도 요청 - " + "종목코드 : " + itemCode + " 주문번호 : " + ordernum);
                        }
                        else if (orderType.Equals(ConstName.RECEIVE_CHEJAN_CANCEL_BUY_ORDER))
                        {
                            coreEngine.SendLogMessage("!!!!!!!!!매수 취소 요청!!!!!!!!");

                           TradingItem item = this.tryingOrderList.Find(o => (itemCode.Contains(o.itemCode)));
                            if (item == null)
                                return;
                            item.buyCancelOrderNum = ordernum;
                            this.tryingOrderList.Remove(item); //접수리스트에서만 지움
                            coreEngine.SendLogMessage("매수 취소 요청 - " + "종목코드 : " + itemCode + " 주문번호 : " + ordernum);
                        }
                        else if (orderType.Equals(ConstName.RECEIVE_CHEJAN_CANCEL_SELL_ORDER))
                        {
                            TradingItem item = this.tryingOrderList.Find(o => (itemCode.Contains(o.itemCode)));
                            if (item == null)
                                return;
                            item.sellCancelOrderNum = ordernum;
                            this.tryingOrderList.Remove(item); //접수리스트에서만 지움
                            coreEngine.SendLogMessage("매도 취소 요청 - " + "종목코드 : " + itemCode + " 주문번호 : " + ordernum);
                        }
                    }
                    else //자동매매에 의한 주문이 아닐때
                    {
                        //보유 아이템 매매인지
                        List<BalanceSellStrategy> bssList = GetTryingSellList(itemCode);

                        if (bssList != null && bssList.Count > 0)
                        {
                            foreach (BalanceSellStrategy bss in bssList)
                            {
                                if (!bss.orderNum.Equals(ordernum) && bss.sellQnt == long.Parse(orderQuantity))
                                {
                                    bss.orderNum = ordernum;
                                    tryingSellList.Remove(bss);

                                    foreach (DataGridViewRow row in autoTradingDataGrid.Rows)
                                    {
                                        if (row.Cells["매매진행_종목코드"].Value.ToString().Contains(itemCode)
                                            && row.Cells["매매진행_매도량"].Value.ToString() == bss.sellQnt.ToString())
                                        {
                                            row.Cells["매매진행_주문번호"].Value = ordernum;
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        else
                        {
                            //청산인지
                            SettlementItem settleItem = this.tryingSettlementItemList.Find(o => itemCode.Contains(o.ItemCode));
                            if (settleItem != null)
                            {
                                settleItem.sellOrderNum = ordernum;
                                tryingSettlementItemList.Remove(settleItem);
                            }
                        }
                    }

                    int rowIndex = orderDataGridView.Rows.Add();
                    Hashtable uiOrderTable = new Hashtable { { "주문_주문번호",ordernum }, { "주문_계좌번호", account },{ "주문_시간", time },{ "주문_종목코드", itemCode }, { "주문_종목명", itemName }, { "주문_매매구분", orderType }, { "주문_가격구분", tradingType }, { "주문_주문량", orderQuantity }, { "주문_주문가격", orderPrice } };
                    Update_OrderDataGrid_UI(uiOrderTable, rowIndex);

                    int index = outstandingDataGrid.Rows.Add();
                    Hashtable outstandingTable = new Hashtable { { "미체결_주문번호", ordernum }, { "미체결_종목코드", itemCode },{ "미체결_종목명", itemName },{ "미체결_주문수량", orderQuantity }, { "미체결_미체결량", orderQuantity } };
                    Update_OutStandingDataGrid_UI(uiOrderTable, rowIndex);

                }
                else if (orderState.Equals(ConstName.RECEIVE_CHEJAN_DATA_CONCLUSION))
                {
                    if (int.Parse(outstanding) == 0 && string.IsNullOrEmpty(conclusionQuantity) == false)
                    {
                        if (orderType.Contains(ConstName.RECEIVE_CHEJAN_DATA_BUY))
                        {
                            coreEngine.SendLogMessage("매수 체결 체결량: " + conclusionQuantity);
                            UpdateTradingStrategyBuy(ordernum, true, int.Parse(conclusionQuantity));
                            UpdateBuyAutoTradingDataGridState(ordernum, ConstName.AUTO_TRADING_STATE_BUY_COMPLETE, true);
                        }
                        else if (orderType.Contains(ConstName.RECEIVE_CHEJAN_DATA_SELL))
                        {
                            //자동 매매매 진행중일때
                            UpdateTradingStrategySellData(ordernum, true, int.Parse(conclusionQuantity));
                            UpdateSellAutoTradingDataGridStatePrice(ordernum, ConstName.AUTO_TRADING_STATE_SELL_COMPLETE, conclusionPrice);
                            
                            //보유잔고 매도
                            BalanceSellStrategy bss = balanceSellStrategyList.Find(o => o.orderNum.Equals(ordernum));
                            if (bss != null)
                            {
                                foreach (DataGridViewRow row in accountBalanceDataGrid.Rows)
                                {
                                    if (row.Cells["계좌잔고_종목코드"].Value != null && row.Cells["계좌잔고_종목코드"].Value.ToString().Contains(bss.itemCode))
                                    {
                                        string qnt = row.Cells["계좌잔고_보유수량"].Value.ToString();
                                        int iQnt = int.Parse(qnt);
                                        iQnt = iQnt - (int)bss.sellQnt;

                                        if (iQnt > 0)
                                        {
                                            row.Cells["계좌잔고_보유수량"].Value = iQnt;
                                        }
                                        else
                                        {
                                            accountBalanceDataGrid.Rows.Remove(row);
                                            //string fidList = "9001;302;10;11;25;12;13"; //9001:종목코드,302:종목명
                                            //axKHOpenAPI1.SetRealRemove("9001", fidList);
                                        }
                                        break;
                                    }
                                }
                                foreach (DataGridViewRow row in autoTradingDataGrid.Rows)
                                {
                                    if (row.Cells["매매진행_종목코드"].Value != null
                                        && row.Cells["매매진행_종목코드"].Value.ToString().Contains(bss.itemCode)
                                        && row.Cells["매매진행_매도량"].Value != null
                                        && row.Cells["매매진행_매도량"].Value.ToString().Equals(bss.sellQnt.ToString())
                                        && row.Cells["매매진행_매수조건식"].Value.ToString().Equals("잔고자동매도")
                                        )
                                    {
                                        row.Cells["매매진행_진행상황"].Value = ConstName.AUTO_TRADING_STATE_SELL_COMPLETE;
                                        row.Cells["매매진행_매도량"].Value = bss.sellQnt;
                                        row.Cells["매매진행_매도가"].Value = conclusionPrice;
                                        row.Cells["매매진행_매도시간"].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                        break;
                                    }
                                }
                            }
                            //청산에 의한 매도
                            SettlementItem settlementItem = settleItemList.Find(o => o.sellOrderNum.Equals(ordernum));
                            if (settlementItem != null)
                            {
                                foreach (DataGridViewRow row in accountBalanceDataGrid.Rows)
                                {
                                    if (row.Cells["계좌잔고_종목코드"].Value != null)
                                    {
                                        if (row.Cells["계좌잔고_종목코드"].Value.ToString().Contains(settlementItem.ItemCode))
                                        {
                                            accountBalanceDataGrid.Rows.Remove(row);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else //일부만 매수/매도 완료
                    {
                        if (string.IsNullOrEmpty(conclusionQuantity) == false)
                        {
                            if (orderType.Contains(ConstName.RECEIVE_CHEJAN_DATA_BUY))
                            {
                                UpdateTradingStrategyBuy(ordernum, false, int.Parse(conclusionQuantity));

                                UpdateBuyTradingItemOutstand(ordernum, int.Parse(outstanding));
                                UpdateBuyAutoTradingDataGridState(ordernum, ConstName.AUTO_TRADING_STATE_BUY_NOT_COMPLETE_OUTCOUNT, false);
                            }
                            else if (orderType.Contains(ConstName.RECEIVE_CHEJAN_DATA_SELL))
                            {
                                UpdateTradingStrategySellData(ordernum, false, int.Parse(conclusionQuantity));

                                UpdateSellTradingItemOutstand(ordernum, int.Parse(outstanding));
                                UpdateSellAutoTradingDataGridStatePrice(ordernum, ConstName.AUTO_TRADING_STATE_SELL_NOT_COMPLETE_OUTCOUNT, conclusionPrice);
                            }
                        }         
                    }

                    int rowIndex = conclusionDataGrid.Rows.Add();
        
                    Hashtable uiTable = new Hashtable { { "체결_주문번호",ordernum }, { "체결_체결시간", time }, { "체결_종목코드", itemCode }, { "체결_종목명", itemName }, { "체결_주문량", orderQuantity }, { "체결_단위체결량", unitConclusionQuantity }, { "체결_누적체결량", conclusionQuantity }, { "체결_체결가", conclusionPrice }, { "체결_매매구분", orderType } };
                    Update_ConclusionDataGrid_UI(uiTable, rowIndex);

                    foreach (DataGridViewRow row in outstandingDataGrid.Rows)
                    {
                        if (row.Cells["미체결_주문번호"].Value != null && row.Cells["미체결_주문번호"].Value.ToString().Equals(ordernum))
                        {

                            row.Cells["미체결_미체결량"].Value = outstanding;
                            if (int.Parse(outstanding) == 0)
                            {
                                outstandingDataGrid.Rows.Remove(row);
                            }
                            break;
                        }
                    }
                }
                else if (orderState.Equals(ConstName.RECEIVE_CHEJAN_DATA_OK))
                {
                    if (orderType.Contains(ConstName.RECEIVE_CHEJAN_CANCEL_BUY_ORDER))
                    {
                        if (int.Parse(outstanding) == 0)
                        {
                            UpdateTradingItemRemoveByCancel(ordernum, true);
                        }
                    }
                    else if (orderType.Contains(ConstName.RECEIVE_CHEJAN_CANCEL_SELL_ORDER))
                    {
                        if (int.Parse(outstanding) == 0)
                        {
                            UpdateTradingItemRemoveByCancel(ordernum, false);
                        }
                    }
                }
            }
            else if (e.sGubun.Equals(ConstName.RECEIVE_CHEJAN_DATA_BALANCE))
            {
                //잔고 전달
                string account = axKHOpenAPI1.GetChejanData(9201);
                string itemCode = axKHOpenAPI1.GetChejanData(9001).Replace("A", "");
                string itemName = axKHOpenAPI1.GetChejanData(302).Trim();
                string balanceQnt = axKHOpenAPI1.GetChejanData(930);
                string buyingPrice = axKHOpenAPI1.GetChejanData(931);
                string totalBuyingPrice = axKHOpenAPI1.GetChejanData(932);
                string orderAvailableQnt = axKHOpenAPI1.GetChejanData(933);
                string tradingType = axKHOpenAPI1.GetChejanData(946);
                //string profitRate = axKHOpenAPI1.GetChejanData(8019);
                string price = axKHOpenAPI1.GetChejanData(10);

                coreEngine.SendLogMessage("________________잔고_____________");
                coreEngine.SendLogMessage("종목코드 : " + itemCode);
                coreEngine.SendLogMessage("종목명 : " + axKHOpenAPI1.GetMasterCodeName(itemCode));
                coreEngine.SendLogMessage("보유수량 : " + balanceQnt);
                coreEngine.SendLogMessage("주문가능수량(매도가능) : " + orderAvailableQnt);
                coreEngine.SendLogMessage("매수매도구분 :" + tradingType);
                coreEngine.SendLogMessage("매입단가 :" + buyingPrice);
                coreEngine.SendLogMessage("총매입가 :" + totalBuyingPrice);
                //coreEngine.SendLogMessage("손익률 :" + profitRate);
                coreEngine.SendLogMessage("________________________________");

                double profitRate = GetProfitRate(double.Parse(price), double.Parse(buyingPrice));
                //잔고탭 업데이트

                bool hasItem_balanceDataGrid = false;
                foreach (DataGridViewRow row in balanceDataGrid.Rows)
                {
                    if (row.Cells["잔고_종목코드"].Value != null && row.Cells["잔고_종목코드"].Value.ToString().Contains(itemCode))
                    {
                        hasItem_balanceDataGrid = true;

                        if (int.Parse(balanceQnt) > 0)
                        {
                            Hashtable uiTable = new Hashtable() { { "잔고_보유수량", balanceQnt }, { "잔고_주문가능수량", orderAvailableQnt }, { "잔고_매입단가", buyingPrice }, { "잔고_총매입가", totalBuyingPrice }, { "잔고_손익률", profitRate }, { "잔고_현재가", price } };
                            Update_BalanceDataGrid_UI(uiTable, row.Index);
                        }
                        else
                        {
                            balanceDataGrid.Rows.Remove(row);
                        }

                        break;
                    }
                }

                if (!hasItem_balanceDataGrid)
                {
                    int rowIndex = balanceDataGrid.Rows.Add();
                    Hashtable uiTable = new Hashtable(){{ "잔고_계좌번호", account }, { "잔고_종목코드", itemCode }, { "잔고_종목명", itemName }, { "잔고_보유수량", balanceQnt }, { "잔고_주문가능수량", orderAvailableQnt }, { "잔고_매입단가", buyingPrice }, { "잔고_총매입가", totalBuyingPrice }, { "잔고_손익률", profitRate }, { "잔고_매매구분", tradingType }, { "잔고_현재가", price } };
                    Update_BalanceDataGrid_UI(uiTable, rowIndex);
            
                }

                //기존잔고매수잔고탭 업데이트
                bool hasItem_accountBalanceDataGrid = false;
                foreach (DataGridViewRow row in accountBalanceDataGrid.Rows)
                {
                    if (row.Cells["계좌잔고_종목코드"].Value != null && row.Cells["계좌잔고_종목코드"].Value.ToString().Contains(itemCode))
                    {
                        hasItem_accountBalanceDataGrid = true;

                        if (int.Parse(balanceQnt) > 0)
                        {
                             Hashtable uiTable = new Hashtable { { "계좌잔고_보유수량", balanceQnt }, { "계좌잔고_평균단가", buyingPrice }, { "계좌잔고_손익률", profitRate }, { "계좌잔고_현재가", price } };
                            Update_AccountBalanceDataGrid_UI(uiTable, row.Index);
                        }
                        else
                        {
                            accountBalanceDataGrid.Rows.Remove(row);
                        }

                        break;
                    }
                }

                if (!hasItem_accountBalanceDataGrid && int.Parse(balanceQnt) > 0)
                {
                    int rowIndex = accountBalanceDataGrid.Rows.Add();
                    int evaluationAmount = int.Parse(price) * int.Parse(balanceQnt);
                    int profitAmount = (int.Parse(price) - int.Parse(buyingPrice)) * int.Parse(balanceQnt);
                    Hashtable uiTable = new Hashtable { { "계좌잔고_종목코드", itemCode } , { "계좌잔고_종목명", itemName }, { "계좌잔고_보유수량", balanceQnt }, { "계좌잔고_평균단가", buyingPrice }, { "계좌잔고_손익률", profitRate }, { "계좌잔고_현재가", price }, { "계좌잔고_매입금액", totalBuyingPrice }, { "계좌잔고_평가금액", evaluationAmount }, { "계좌잔고_손익금액", profitAmount } };
                    Update_AccountBalanceDataGrid_UI(uiTable, rowIndex);
                }
            }
        }

        private void API_OnReceiveTrCondition(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrConditionEvent e)
        {

            string strCodeName;
            string conditionName = e.strConditionName;
            int conditionIndex = e.nIndex;
            int nIdx = 0;
            string[] codeList = e.strCodeList.Split(';');
            foreach (string code in codeList)
            {
                if (code == ("")) continue;
                strCodeName = axKHOpenAPI1.GetMasterCodeName(code); // 종목명을 가져온다.
                coreEngine.SendLogMessage("conditionName = " + conditionName);
                coreEngine.SendLogMessage("itemCode = " + code);
                coreEngine.SendLogMessage("strCodeName = " + strCodeName);
            }
        }
        #endregion

        #region UI_EVENT_FUNCTION
       

        private void DataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            coreEngine.SendLogMessage("e.ColumnIndex : " + e.ColumnIndex + " e.RowIndex : " + e.RowIndex);
            if (e.RowIndex < 0)
                return;
            if (accountBalanceDataGrid.Columns["계좌잔고_청산"].Index == e.ColumnIndex)
            {
                if (e.ColumnIndex >= 0 && accountBalanceDataGrid.Columns.Count >= e.ColumnIndex)
                {
                    if (accountBalanceDataGrid["계좌잔고_청산", e.RowIndex].Value == null) //최초 생성시는 null값이 들어가 있음
                    {
                        if (accountBalanceDataGrid["계좌잔고_종목코드", e.RowIndex].Value != null)
                        {
                            string itemCode = accountBalanceDataGrid["계좌잔고_종목코드", e.RowIndex].Value.ToString().Replace("A", "");
                            int balanceCnt = int.Parse(accountBalanceDataGrid["계좌잔고_보유수량", e.RowIndex].Value.ToString());

                            int orderResult = axKHOpenAPI1.SendOrder("청산매도주문", GetScreenNum().ToString(), currentAccount, CONST_NUMBER.SEND_ORDER_SELL, itemCode.Replace("A", ""), balanceCnt, 0, "03", ""); //2:신규매도

                            if (orderResult == 0)
                            {
                                coreEngine.SendLogMessage("접수 성공");
                                accountBalanceDataGrid["계좌잔고_청산", e.RowIndex].Value = "청산주문접수";

                                SettlementItem settlementItem = new SettlementItem(currentAccount, itemCode, balanceCnt);

                                tryingSettlementItemList.Add(settlementItem);
                                settleItemList.Add(settlementItem);

                            }
                        }
                    }
                }

            }
        }
        private void AutoTradingDataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            coreEngine.SendLogMessage("e.ColumnIndex : " + e.ColumnIndex + " e.RowIndex : " + e.RowIndex);
            if (e.RowIndex < 0)
                return;
            if (autoTradingDataGrid.Columns["매매진행_취소"].Index == e.ColumnIndex)
            {
                if (e.ColumnIndex >= 0 && autoTradingDataGrid.Columns.Count >= e.ColumnIndex)
                {
                 
                    if (autoTradingDataGrid["매매진행_종목코드", e.RowIndex].Value != null
                        && autoTradingDataGrid["매매진행_진행상황", e.RowIndex].Value != null
                        && autoTradingDataGrid["매매진행_매수조건식", e.RowIndex].Value != null)
                    {
                        string itemCode = autoTradingDataGrid["매매진행_종목코드", e.RowIndex].Value.ToString().Replace("A", "");
                        string curState = autoTradingDataGrid["매매진행_진행상황", e.RowIndex].Value.ToString();
                        string curConditon = autoTradingDataGrid["매매진행_매수조건식", e.RowIndex].Value.ToString();

                        if (curState.Equals(ConstName.AUTO_TRADING_STATE_BUY_COMPLETE) || curState.Equals(ConstName.AUTO_TRADING_STATE_SELL_COMPLETE))
                        {
                            MessageBox.Show("취소할수있는 상태가 아닙니다.");
                            return;
                        }

                        if (MartinGailManager.GetInstance().CurStrategy != null && MartinGailManager.GetInstance().CurStrategy.buyCondition.Name.Equals(curConditon))
                        {
                            MessageBox.Show("마틴게일 아이템입니다.");
                            return;
                        }
                        DataGridViewRow rowItem = autoTradingDataGrid.Rows[e.RowIndex];

                        List<TradingItem> tradeItemListAll = GetAllTradingItemData(itemCode);

                        foreach (TradingItem tradeItem in tradeItemListAll)
                        {
                            if(tradeItem.ui_rowItem == rowItem)
                            {
                                if (curState.Equals(ConstName.AUTO_TRADING_STATE_BUY_NOT_COMPLETE) || curState.Equals(ConstName.AUTO_TRADING_STATE_BUY_NOT_COMPLETE_OUTCOUNT))
                                {
                                    //취소주문
                                    int orderResult = axKHOpenAPI1.SendOrder("종목주문정정", GetScreenNum().ToString(), currentAccount, CONST_NUMBER.SEND_ORDER_CANCEL_BUY, itemCode, tradeItem.outStandingQnt, (int)tradeItem.buyingPrice, tradeItem.orderType, tradeItem.buyOrderNum);

                                    if (orderResult == 0)
                                    {
                                        AddOrderList(tradeItem);
                                        coreEngine.SendLogMessage("취소 접수 성공");
                                        autoTradingDataGrid["매매진행_취소", e.RowIndex].Value = "취소접수시도";
                                    }
                                }

                                if (curState.Equals(ConstName.AUTO_TRADING_STATE_SELL_NOT_COMPLETE) || curState.Equals(ConstName.AUTO_TRADING_STATE_SELL_NOT_COMPLETE_OUTCOUNT))
                                {
                                    //취소주문
                                    int orderResult = axKHOpenAPI1.SendOrder("종목주문정정", GetScreenNum().ToString(), currentAccount, CONST_NUMBER.SEND_ORDER_CANCEL_SELL, itemCode, tradeItem.outStandingQnt, (int)tradeItem.sellPrice, tradeItem.orderType, tradeItem.sellOrderNum);

                                    if (orderResult == 0)
                                    {
                                        AddOrderList(tradeItem);
                                        coreEngine.SendLogMessage("취소 접수 성공");
                                        autoTradingDataGrid["매매진행_취소", e.RowIndex].Value = "취소접수시도";
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
   
        public void CancelBuyOrder(string itemCode , string buyOrderNum)
        {
            List<TradingItem> tradeItemListAll = GetAllTradingItemData(itemCode);

            foreach (TradingItem tradeItem in tradeItemListAll)
            {
                if(tradeItem.buyOrderNum == buyOrderNum)
                {
                     //취소주문
                    int orderResult = axKHOpenAPI1.SendOrder(
                        "종목주문정정", 
                        GetScreenNum().ToString(), 
                        currentAccount, 
                        CONST_NUMBER.SEND_ORDER_CANCEL_BUY, 
                        itemCode, 
                        tradeItem.outStandingQnt, 
                        (int)tradeItem.buyingPrice,
                        tradeItem.orderType, 
                        tradeItem.buyOrderNum
                        );

                    if (orderResult == 0)
                    {
                        AddOrderList(tradeItem);
                        coreEngine.SendLogMessage("취소 접수 성공");
                        autoTradingDataGrid["매매진행_진행상황", tradeItem.GetUiConnectRow().Index].Value = ConstName.AUTO_TRADING_STATE_CANCEL_ORDER;
                    }
                }
            }
        }
        public void TradingStrategyGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;
            if (e.ColumnIndex == tsDataGridView.Columns["매매전략_취소"].Index)
            {
                string conditionName = tsDataGridView["매매전략_매수조건식", e.RowIndex].Value.ToString();

                TradingStrategy ts = tradingStrategyList.Find(o => o.buyCondition != null && o.buyCondition.Name.Equals(conditionName));

                if (ts != null)
                {
                    DialogResult result = MessageBox.Show(conditionName + "매매조건을 삭제하시겠습니까?", "매매전략 삭제", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        if (MartinGailManager.GetInstance().IsMartinStrategy(ts))
                            MartinGailManager.GetInstance().Stop();

                        tradingStrategyList.Remove(ts);
                        tsDataGridView.Rows.RemoveAt(e.RowIndex);

                    }
                }
            }
        }
      
        private void ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            axKHOpenAPI1.CommConnect();
        }
        private void InterestConditionListBox_SelectedIndexChanged(object sender, EventArgs s)
        {
            if (sender.Equals(interestConditionListBox))
            {
                if (interestConditionListBox.SelectedItem != null)
                {
                    string conditionName = interestConditionListBox.SelectedItem.ToString();
                    Condition condition = listCondition.Find(o => o.Name.Equals(conditionName));

                    if (condition != null)
                    {
                        interestListBox.Items.Clear();
                        foreach (StockItem stockItem in condition.interestItemList)
                        {
                            interestListBox.Items.Add(stockItem.Name);
                        }
                    }
                }
            }
        }
        private void ComboBoxIndexChanged(object sender, EventArgs e)
        {

            if (sender.Equals(accountComboBox))
            {
                if (accountComboBox.SelectedItem == null)
                    return;
                string account = accountComboBox.SelectedItem.ToString();
                if (!account.Equals(currentAccount))
                {
                    axKHOpenAPI1.SetInputValue("계좌번호", account);
                    axKHOpenAPI1.SetInputValue("비밀번호", "");
                    axKHOpenAPI1.SetInputValue("상장폐지조회구분", "0");
                    axKHOpenAPI1.SetInputValue("비밀번호입력매체구분", "00");
                    axKHOpenAPI1.CommRqData(ConstName.RECEIVE_TR_DATA_ACCOUNT_INFO, "OPW00004", 0, GetScreenNum().ToString());

                    currentAccount = account;
                }

            }
        }
        private void BalanceSellBtn_Click(object sender, EventArgs e)
        {
            string itemCode = balanceItemCodeTxt.Text;
            string itemName = balanceNameTextBox.Text;
            long sellQnt = (long)balanceQntUpdown.Value;

            string accountNum = accountComboBox.Text;
            int buyingPrice = (int)double.Parse(b_averagePriceTxt.Text);


            if (accountNum.Length > 0)
            {

                if (itemCode.Length > 0)
                {
                    if (sellQnt > 0)
                    {
                        //잔고 매도 전략 추가시 기존 전략의 자동 매도는 전부 꺼준다

                        List<TradingItem> tradeItemListAll = GetAllTradingItemData(itemCode);

                        foreach (TradingItem tradeItem in tradeItemListAll)
                        {
                            tradeItem.ts.usingStoploss = false;
                            tradeItem.ts.usingTakeProfit = false;
                        }

                        //매매 전략

                        bool usingProfitCheckBox = b_ProfitSellCheckBox.Checked; //익절사용
                        double takeProfitRate = 0;
                        string sellOrderOption = "시장가";

                        if (usingProfitCheckBox)
                        {
                            takeProfitRate = (double)b_takeProfitUpdown.Value;
                        }

                        bool usingStopLoss = b_StopLossCheckBox.Checked; //손절사용

                        double stopLossRate = 0;

                        if (usingStopLoss)
                        {
                            stopLossRate = (double)b_stopLossUpdown.Value;
                        }

                        BalanceSellStrategy bs = new BalanceSellStrategy(
                            accountNum,
                            itemCode,
                            buyingPrice,
                            sellQnt,
                            sellOrderOption,
                            usingProfitCheckBox,
                            takeProfitRate,
                            usingStopLoss,
                            stopLossRate
                            );

                        balanceSellStrategyList.Add(bs);

                        int rowIndex = autoTradingDataGrid.Rows.Add();

                        autoTradingDataGrid["매매진행_진행상황", rowIndex].Value = ConstName.AUTO_TRADING_STATE_SELL_MONITORING;
                        autoTradingDataGrid["매매진행_종목코드", rowIndex].Value = itemCode;
                        autoTradingDataGrid["매매진행_종목명", rowIndex].Value = itemName;

                        DataGridViewRow rowData = GetDataFromAccountDataGrid(itemCode);
                        if (rowData != null && rowData.Cells["계좌잔고_보유수량"] != null)
                        {
                            long balanceQnt = long.Parse(rowData.Cells["계좌잔고_보유수량"].Value.ToString());
                            autoTradingDataGrid["매매진행_매수량", rowIndex].Value = balanceQnt;
                        }

                        autoTradingDataGrid["매매진행_매수가", rowIndex].Value = buyingPrice;
                        autoTradingDataGrid["매매진행_매수조건식", rowIndex].Value = "잔고자동매도"; //매수조건식이 없으므로 해당명으로 지정

                        coreEngine.SendLogMessage("전략이 입력됬습니다");
                    }
                    else
                    {
                        MessageBox.Show("매도수량은 0보다 커야 합니다.");
                    }
                }
                else
                {
                    MessageBox.Show("매도종목을 선택해주세요");
                }
            }
            else
            {
                MessageBox.Show("계좌를 선택해주세요");
            }
        }
        private void AddStratgyBtn_Click(object sender, EventArgs e)
        {
            string account = accountComboBox.Text;

            if (account.Length == 0)
            {
                MessageBox.Show("계좌를 선택해주세요");
                return;
            }

            string conditionName = BuyConditionComboBox.Text;
            Condition findCondition = null;

            if (conditionName.Length > 0)
            {
                findCondition = listCondition.Find(o => o.Name.Equals(conditionName));
                TradingStrategy inStrategyCondition = tradingStrategyList.Find(o => o.buyCondition.Name.Equals(conditionName));
                if (findCondition != null && inStrategyCondition != null)
                {
                    MessageBox.Show("현재 등록되있는 전략 검색식입니다");
                    return;
                }
            }
            else
            {
                MessageBox.Show("매수 조건식을 선택해주세요");
                return;
            }

            if (findCondition == null)
            {
                return;
            }

            string buyOrderOpt = "지정가";
            long totalInvestment = 0;
            int itemCount = 0;

            if (marketPriceRadio.Checked)
            {
                buyOrderOpt = "시장가";
            } else
            {
                buyOrderOpt = buyOrderOptionCombo.Text;
            }

            if (allCostUpDown.Value == 0)
            {
                MessageBox.Show("총 투자금액을 설정해주세요");
                return;
            }

            totalInvestment = (long)allCostUpDown.Value;
            itemCount = (int)itemCountUpdown.Value;

            List<TradingStrategyADDItem> tradingStrategyItemList = new List<TradingStrategyADDItem>();
            //매매 전략
            string sellOrderOption = "시장가";
            
            bool usingBuyRestart = loopBuyCheck.Checked;


            TradingStrategy ts = new TradingStrategy(
                account,
                findCondition,
                buyOrderOpt,
                totalInvestment,
                itemCount,
                sellOrderOption,
                false,
                usingBuyRestart
                );

            //추가전략 적용
            bool usingTimeCheck = TimeUseCheck.Checked; //시간 제한 사용

            if (usingTimeCheck)
            {
                DateTime startDate = startTimePicker.Value;
                DateTime endDate = endTimePicker.Value;
                TradingStrategyItemBuyTimeCheck timeBuyCheck =
                     new TradingStrategyItemBuyTimeCheck(
                             StrategyItemName.BUY_TIME_LIMIT,
                             CHECK_TIMING.BUY_TIME,
                            startDate,
                             endDate);
                ts.AddTradingStrategyItemList(timeBuyCheck);
                ts.usingTimeLimit = true;
                ts.startDate = startDate;
                ts.endDate = endDate;
            }

            bool usingTickbuy = usingTickBuyCheck.Checked; //틱 가격 적용

            if (usingTickbuy)
            {
                int index = buyTickComboBox.SelectedIndex;
                if (index < 0)
                {
                    MessageBox.Show("매수 틱 단위를 선택해주세요");
                    return;
                }
                index++; //0:매수 호가 배열에서 0은 최우선 매수 호가이기 때문에 인덱스에 1을 더해줌
                ts.usingTickBuy = true;
                ts.tickBuyValue = index;
            }

            bool usingTrailBuy = usingTrailingBuyCheck.Checked; //트레일링 매수 적용

            if (usingTrailBuy)
            {
                int tickValue = (int)trailingUpDown.Value;
                if (tickValue <= 0)
                {
                    MessageBox.Show("트레일링 틱 단위를 선택해주세요");
                    return;
                }

                ts.usingTrailing = true;
                ts.trailTickValue = tickValue;
            }

            bool usingProfitCheckBox = profitSellCheckBox.Checked; //익절사용

            if (usingProfitCheckBox)
            {
                double takeProfitRate = 0;
                TradingStrategyItemWithUpDownValue takeProfitStrategy = null;
                takeProfitRate = (double)profitSellUpdown.Value;
                takeProfitStrategy =
                     new TradingStrategyItemWithUpDownValue(
                             StrategyItemName.TAKE_PROFIT_SELL,
                             CHECK_TIMING.SELL_TIME,
                             "buyingPrice",
                             TradingStrategyItemWithUpDownValue.IS_TRUE_OR_FALE_TYPE.UPPER,
                             takeProfitRate);
                takeProfitStrategy.OnReceivedTrData += this.OnReceiveTrDataCheckProfitSell;
                ts.AddTradingStrategyItemList(takeProfitStrategy);
            }

            bool usingStopLoss = minusSellCheckBox.Checked; //손절사용

            if (usingStopLoss)
            {
                double stopLossRate = 0;
                stopLossRate = (double)minusSellUpdown.Value;
                TradingStrategyItemWithUpDownValue stopLossStrategy = null;
                stopLossStrategy =
                    new TradingStrategyItemWithUpDownValue(
                            StrategyItemName.STOPLOSS_SELL,
                            CHECK_TIMING.SELL_TIME,
                            "buyingPrice",
                            TradingStrategyItemWithUpDownValue.IS_TRUE_OR_FALE_TYPE.DOWN,
                            stopLossRate);

                stopLossStrategy.OnReceivedTrData += this.OnReceiveTrDataCheckStopLoss;
                ts.AddTradingStrategyItemList(stopLossStrategy);
            }

            tradingStrategyList.Add(ts);
            AddStrategyToDataGridView(ts);

            StartMonitoring(ts.buyCondition);

            coreEngine.SendLogMessage("전략이 입력됬습니다 \n 매수조건식 : " + ts.buyCondition.Name + "\n" + " 총투자금 : " + ts.totalInvestment + "\n" + " 종목수 : " + ts.buyItemCount);
        }

        private void AccountDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (accountBalanceDataGrid.SelectedRows.Count > 0)
            {
                int rowIndex = accountBalanceDataGrid.SelectedRows[0].Index;

                if (accountBalanceDataGrid["계좌잔고_종목코드", rowIndex].Value != null)
                {
                    string itemCode = accountBalanceDataGrid["계좌잔고_종목코드", rowIndex].Value.ToString();
                    string itemName = accountBalanceDataGrid["계좌잔고_종목명", rowIndex].Value.ToString();
                    long balanceQnt = long.Parse(accountBalanceDataGrid["계좌잔고_보유수량", rowIndex].Value.ToString());
                    double buyingPrice = double.Parse(accountBalanceDataGrid["계좌잔고_평균단가", rowIndex].Value.ToString());

                    balanceItemCodeTxt.Text = itemCode.Replace("A", "");
                    balanceNameTextBox.Text = itemName;
                    balanceQntUpdown.Maximum = balanceQnt;
                    balanceQntUpdown.Value = balanceQnt;
                    b_averagePriceTxt.Text = buyingPrice.ToString();
                }
                else
                {
                    accountBalanceDataGrid.ClearSelection();
                }
            }
        }

        private void addInterestBtn_Click(object sender, EventArgs e)
        {
            if (interestConditionListBox.SelectedItem != null)
            {
                string conditionNameSelect = interestConditionListBox.SelectedItem.ToString();

                Condition condition = listCondition.Find(o => o.Name.Equals(conditionNameSelect));
                if (condition != null)
                {
                    string itemName = interestTextBox.Text;
                    StockItem stockItem = stockItemList.Find(o => o.Name.Equals(itemName));

                    if (stockItem != null)
                    {
                        if (condition.interestItemList.Contains(stockItem) == false)
                        {
                            condition.interestItemList.Add(stockItem);
                            interestListBox.Items.Add(stockItem.Name);
                        }
                    }
                }

            }
            else
            {
                MessageBox.Show("조건식을 선택해주세요");
            }

        }
        private void PrintToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenSecondWindow();
        }

        private void OpenSecondWindow()
        {
            Form2 printForm = new Form2(axKHOpenAPI1);
            printForm.Show();
        }
        #endregion
        private void StartMonitoring(Condition _condition)
        {
            int result = axKHOpenAPI1.SendCondition(GetScreenNum().ToString(), _condition.Name, _condition.Index, 1);
            if (result == 1)
            {
                coreEngine.SendLogMessage("감시요청 성공");
            }
            else
            {
                coreEngine.SendLogMessage("감시요청 실패");
            }
        }

        public int GetScreenNum()
        {
            screenNum++;

            if (screenNum > 5000)
                screenNum = 1000;

            return screenNum;
        }

        public static double GetProfitRate(double curPrice, double buyPrice)
        {
            if (buyPrice <= 0)
                return 0;
            return (double)100 * ((curPrice - buyPrice) / buyPrice) - FEE_RATE;
        }

       
        DataGridViewRow GetDataFromAccountDataGrid(string itemCode)
        {
            foreach (DataGridViewRow row in accountBalanceDataGrid.Rows)
            {
                if (row.Cells["계좌잔고_종목코드"].Value != null)
                {
                    if (row.Cells["계좌잔고_종목코드"].Value.ToString().Contains(itemCode))
                    {
                        return row;
                    }
                }
            }
            return null;
        }

        private void OnReceiveTrDataCheckProfitSell(object sender, OnReceivedTrEventArgs e)
        {
            OnReceiveTrDataCheckProfitSell(e.tradingItem, e.checkNum);
        }

        public void OnReceiveTrDataCheckProfitSell(TradingItem item, double checkValue)
        {

            if (item.state == TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_NOT_COMPLETE
                || item.state == TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_NOT_COMPLETE_OUTCOUNT)
            {
                coreEngine.SendLogWarningMessage("기존 매도 요청된 아이템 익절 ? : " + item.IsProfitSell());
                if (item.IsProfitSell())
                {
                    //같은 익절 상태면 아무것도 안함
                    return;
                }
                else
                {
                    Task requestCancelTask = new Task(() =>
                    {
                        //취소주문
                        int orderResultCancel = axKHOpenAPI1.SendOrder("종목주문정정", GetScreenNum().ToString(), currentAccount, CONST_NUMBER.SEND_ORDER_CANCEL_SELL, item.itemCode, item.outStandingQnt, (int)item.sellPrice, item.orderType, item.sellOrderNum);

                        if (orderResultCancel == 0)
                        {
                            AddOrderList(item);
                            coreEngine.SendLogMessage("취소 접수 성공");
                            autoTradingDataGrid["매매진행_진행상황", item.GetUiConnectRow().Index].Value = ConstName.AUTO_TRADING_STATE_STOPLOSS_CANCEL;
                            return;
                        }
                    });
                    coreEngine.requestTrDataManager.RequestTrData(requestCancelTask);
                    return;
                }
            }
            Task requestItemInfoTask = new Task(() =>
            {
                if (item.state != TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_COMPLETE)
                    return;
                int orderResult = axKHOpenAPI1.SendOrder(
                    "종목익절매도",
                    GetScreenNum().ToString(),
                    item.ts.account,
                    CONST_NUMBER.SEND_ORDER_SELL,
                    item.itemCode,
                    item.curQnt,
                    (int)item.curPrice,
                    "00",//지정가
                    "" //원주문번호없음
                 );
                if (orderResult == 0) //요청 성공시 (실거래는 안될 수 있음)
                {
                    AddOrderList(item);
                    item.SetSold(true, true);
                    coreEngine.SendLogMessage("ui -> 매도주문접수시도");
                    UpdateAutoTradingDataGridRow(item.itemCode, item, item.curPrice, ConstName.AUTO_TRADING_STATE_SELL_BEFORE_ORDER);
                }
                else
                {
                    coreEngine.SendLogMessage("자동 익절 요청 실패");
                }
            });
            coreEngine.requestTrDataManager.RequestTrData(requestItemInfoTask);
        }

        private void API_OnReceiveTrDataHoga(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
              coreEngine.SendLogMessage(e.sRQName);

            if (e.sRQName.Contains(ConstName.RECEIVE_TR_DATA_HOGA))
            {
                coreEngine.axKHOpenAPI_OnReceiveTrData(sender, e); //호가부분 데이터 입력

                //검색 ->검색완료 -> 매수주문 -> 현재호가를 얻어오기위해 tr요청-> 이때 "RECEIVE_TR_DATA_HOGA:검색넘버:아이템코드" 로 요청

                string[] rqNameArray = e.sRQName.Split(':');
                if (rqNameArray.Length == 3)
                {
                    string conditionUid = (rqNameArray[2]);
                    TradingStrategy ts = tradingStrategyList.Find(o => o.buyCondition.Uid == conditionUid);

                    if (ts != null) {
                        string itemcode = rqNameArray[1];
                        StockWithBiddingEntity stockInfo = StockWithBiddingManager.GetInstance().GetItem(itemcode);
                       
                        if (ts.usingTrailing)
                        {

                            coreEngine.SendLogMessage(axKHOpenAPI1.GetMasterCodeName(itemcode) + " 검색 등장시 호가(트레일링) : " + stockInfo.GetBuyHoga(0));

                            int buyPrice = (int)stockInfo.GetBuyHoga(0);
                            int i_qnt = (int)(ts.itemInvestment / buyPrice);

                            int rowIndex = autoTradingDataGrid.Rows.Add(); //종목포착정보 ui추가

                            TrailingItem trailItem = new TrailingItem(itemcode, buyPrice, ts);
                            trailItem.ui_rowAutoTradingItem = autoTradingDataGrid.Rows[rowIndex];

                            UpdateAutoTradingDataGridRowAll(rowIndex, ConstName.AUTO_TRADING_STATE_SEARCH_AND_CATCH, itemcode, ts.buyCondition.Name, i_qnt, buyPrice);

                            trailingList.Add(trailItem);

                            return; //실시간 호가 받는부분에서 주문 시도
                        }
                        else//하락 트레일링 아닐때 바로매수
                        {
                          
                            int price = 0;
                            int i_qnt = 0;

                            coreEngine.SendLogMessage(axKHOpenAPI1.GetMasterCodeName(itemcode) + " 검색 등장시 호가 : " + stockInfo.GetBuyHoga(0));
                            if (stockInfo != null)
                            {
                                price = (int)stockInfo.GetBuyHoga(ts.tickBuyValue); //틱호가를 얻어옴

                                if (price > 0)
                                {
                                    i_qnt = (int)(ts.itemInvestment / price);
                                    coreEngine.SendLogMessage(axKHOpenAPI1.GetMasterCodeName(itemcode) + " 종목 매수 시도 : " + axKHOpenAPI1.GetMasterCodeName(itemcode));

                                    int orderResult =

                                    axKHOpenAPI1.SendOrder(
                                        "편입종목매수",
                                        GetScreenNum().ToString(),
                                        ts.account,
                                        CONST_NUMBER.SEND_ORDER_BUY,//1:신규매수
                                        itemcode,
                                        (int)(ts.itemInvestment / price),
                                        price,
                                         ConstName.ORDER_JIJUNGGA,//지정가
                                        "" //원주문번호없음
                                    );

                                    if (orderResult == 0)
                                    {
                                        coreEngine.SendLogMessage(axKHOpenAPI1.GetMasterCodeName(itemcode) + " 매수주문요청 성공");

                                        TradingItem tradingItem = new TradingItem(ts, itemcode, axKHOpenAPI1.GetMasterCodeName(itemcode),  price, i_qnt, false, false, ConstName.ORDER_JIJUNGGA);
                                        tradingItem.SetBuy(true);
                                        tradingItem.SetConditonUid(conditionUid);

                                        ts.tradingItemList.Add(tradingItem);
                                        AddOrderList(tradingItem);
                                        
                                        string fidList = "9001;302;10;11;25;12;13"; //9001:종목코드,302:종목명
                                        axKHOpenAPI1.SetRealReg("9001", itemcode, fidList, "1");

                                        //매매진행 데이터 그리드뷰 표시

                                        int addRow = autoTradingDataGrid.Rows.Add();
                                        tradingItem.SetUiConnectRow(autoTradingDataGrid.Rows[addRow]);
                                        UpdateAutoTradingDataGridRowAll(addRow, ConstName.AUTO_TRADING_STATE_BUY_BEFORE_ORDER, itemcode, ts.buyCondition.Name, i_qnt, price);
                                        ts.StrategyBuyOrderUpdate(itemcode, price, i_qnt, TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_BEFORE_ORDER);
                                        coreEngine.SendLogMessage("자동 매수 요청 - " + "종목코드 : " + itemcode + " 주문가 : " + price + " 주문수량 : " + i_qnt + " 매수조건식 : " + ts.buyCondition.Name);
                                    } 
                                }
                                else
                                {
                                    coreEngine.SendLogMessage(axKHOpenAPI1.GetMasterCodeName(itemcode) + " 호가 정보 받기 -> 매수 실패");
                                }
                            }
                        }
                    }
                }
            }
        }
        public void API_OnReceiveMsg(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveMsgEvent e)
        {
            coreEngine.SaveLogMessage("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            coreEngine.SaveLogMessage("ScreenNum : " + e.sScrNo + ",사용자구분명 : " + e.sRQName + ", Tr이름: " + e.sTrCode + ", MSG : " + e.sMsg);
            coreEngine.SaveLogMessage("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        }

        public void API_OnReceiveRealDataHoga(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveRealDataEvent e)
        {
            string itemCode = e.sRealKey.Trim();

            //coreEngine.SendLogMessage(e.sRealType +":"+axKHOpenAPI1.GetMasterCodeName(itemCode));

            if (e.sRealType.Contains(ConstName.RECEIVE_REAL_DATA_HOGA) || e.sRealType.Contains(ConstName.RECEIVE_REAL_DATA_USUN_HOGA))
            {
                coreEngine.axKHOpenAPI_OnReceiveRealData(sender, e); //호가부분 데이터 입력

                foreach (var trailingItem in trailingList.Reverse<TrailingItem>())
                {
                    if (trailingItem.strategy != null && itemCode.Contains(trailingItem.itemCode) && trailingItem.strategy.usingTrailing) //하락 트레일링 체크
                    {
                        StockWithBiddingEntity stockInfo = StockWithBiddingManager.GetInstance().GetItem(itemCode);
                  
                        if (stockInfo != null)
                        {
                            string itemcode = itemCode;
                            int price = (int)stockInfo.GetBuyHoga(0);
                            int i_qnt = 0;

                            //틱단위 스킵
                            //coreEngine.SendLogMessage("틱제한 " + axKHOpenAPI1.GetMasterCodeName(itemcode) + " :  현재 " + trailingItem.curTickCount.ToString() + " / 셋팅 " + trailingItem.settingTickCount.ToString());

                            if (trailingItem.curTickCount < trailingItem.settingTickCount)
                            {
                                trailingItem.curTickCount++;
                                trailingItem.sumPriceAllTick += price;
                                continue;
                            }
                            if (trailingItem.curTickCount > 0) 
                                trailingItem.lowestPrice = trailingItem.sumPriceAllTick / trailingItem.curTickCount;

                            coreEngine.SendLogMessage(axKHOpenAPI1.GetMasterCodeName(itemcode) + " 스킵체크 :  현재가 " + price.ToString() + " / 지난 최저가 " + trailingItem.lowestPrice.ToString());

                            if (price <= trailingItem.lowestPrice)
                            {
                                trailingItem.curTickCount = 0;
                                trailingItem.sumPriceAllTick = 0; 
                                trailingItem.lowestPrice = price;
                                continue;
                            }

                            i_qnt = (int)(trailingItem.strategy.itemInvestment / price);
                            price = (int)stockInfo.GetBuyHoga(trailingItem.strategy.tickBuyValue); //전략에 의한 매수틱 조정(없으면 최우선매수호가)

                            
                            coreEngine.SendLogMessage(axKHOpenAPI1.GetMasterCodeName(itemcode) + " 사용자 정의호가 (" + trailingItem.strategy.tickBuyValue + " 틱) / 값 :" + price);

                            if (price > 0 && trailingItem.isTrailing)
                            {
                                coreEngine.SendLogMessage(axKHOpenAPI1.GetMasterCodeName(itemcode) + " 종목 매수 시도  : " + axKHOpenAPI1.GetMasterCodeName(itemcode));
                                trailingItem.isTrailing = false;
                                Task requestBuyTask = new Task(() =>
                                {
                                    int orderResult =

                                    axKHOpenAPI1.SendOrder(
                                        "편입종목매수",
                                        GetScreenNum().ToString(),
                                        trailingItem.strategy.account,
                                        CONST_NUMBER.SEND_ORDER_BUY,//1:신규매수
                                        itemcode,
                                        (int)(trailingItem.strategy.itemInvestment / price),
                                        price,
                                         ConstName.ORDER_JIJUNGGA,
                                        "" //원주문번호없음
                                    );

                                    if (orderResult == 0)
                                    {
                                        coreEngine.SendLogMessage(axKHOpenAPI1.GetMasterCodeName(itemcode) + " 매수주문 성공");

                                        TradingItem tradingItem = new TradingItem(trailingItem.strategy, itemcode, axKHOpenAPI1.GetMasterCodeName(itemcode), price, i_qnt, false, false, ConstName.ORDER_JIJUNGGA);
                                        tradingItem.SetBuy(true);
                                        tradingItem.SetConditonUid(trailingItem.strategy.buyCondition.Uid);

                                        trailingItem.strategy.tradingItemList.Add(tradingItem); //매수전략 내에 매매진행 종목 추가

                                        AddOrderList(tradingItem);

                                        string fidList = "9001;302;10;11;25;12;13"; //9001:종목코드,302:종목명
                                        axKHOpenAPI1.SetRealReg("9001", itemcode, fidList, "1");

                                        //매매진행 데이터 그리드뷰 표시

                                        int addRow = 0;
                                        if (trailingItem.ui_rowAutoTradingItem != null)
                                        {
                                            addRow = trailingItem.ui_rowAutoTradingItem.Index;
                                        }
                                        else
                                        {
                                            addRow = autoTradingDataGrid.Rows.Add();
                                        }
                                        tradingItem.SetUiConnectRow(autoTradingDataGrid.Rows[addRow]);

                                        UpdateAutoTradingDataGridRowAll(addRow, ConstName.AUTO_TRADING_STATE_BUY_BEFORE_ORDER, itemcode, trailingItem.strategy.buyCondition.Name, i_qnt, price);

                                        autoTradingDataGrid["매매진행_현재가", addRow].Value = stockInfo.GetBuyHoga(0);

                                        coreEngine.SendLogMessage("자동 매수 요청 - " + "종목코드 : " + itemcode + " 주문가 : " + price + " 주문수량 : " + i_qnt + " 매수조건식 : " + trailingItem.strategy.buyCondition.Name);
                                        coreEngine.SendLogMessage("트레일링 삭제");

                                        tradingItem.ts.StrategyBuyOrderUpdate(itemcode, price, i_qnt, TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_BEFORE_ORDER);
                                        trailingList.Remove(trailingItem);
                                    }
                                    else
                                    {
                                        coreEngine.SendLogMessage("구매가 입력 실패");
                                    }

                                });
                                coreEngine.requestTrDataManager.RequestTrData(requestBuyTask);
                            }
                            else
                            {
                                coreEngine.SendLogMessage("!!!!!! 호가 정보 받기 -> 매수 실패 !!!!!!!!!!");
                            }
                        }
                    }
                }
            }
        }

        private void OnReceiveTrDataCheckStopLoss(object sender, OnReceivedTrEventArgs e)
        {
            OnReceiveTrDataCheckStopLoss(e.tradingItem, e.checkNum);
        }

        public void OnReceiveTrDataCheckStopLoss(TradingItem item, double checkValue)
        {
            if (item.state == TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_NOT_COMPLETE
                || item.state == TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_NOT_COMPLETE_OUTCOUNT)
            {
                coreEngine.SendLogWarningMessage("기존 매도 요청된 아이템 익절? : " + item.IsProfitSell());
                if(item.IsProfitSell())
                {
                    Task requestCancelTask = new Task(() =>
                    {
                        //취소주문
                        int orderResultCancel = axKHOpenAPI1.SendOrder("종목주문정정", GetScreenNum().ToString(), currentAccount, CONST_NUMBER.SEND_ORDER_CANCEL_SELL, item.itemCode, item.outStandingQnt, (int)item.sellPrice, item.orderType, item.sellOrderNum);

                        if (orderResultCancel == 0)
                        {
                            AddOrderList(item);
                            coreEngine.SendLogMessage("취소 접수 성공");
                            autoTradingDataGrid["매매진행_진행상황", item.GetUiConnectRow().Index].Value = ConstName.AUTO_TRADING_STATE_TAKE_PROFIT_CANCEL;
                            return;
                        }
                    });
                     coreEngine.requestTrDataManager.RequestTrData(requestCancelTask);
                    return;
                }
                else
                {
                    //같은 손절 주문이면 아무것도 안함
                    return;
                }
            }

            Task requestItemInfoTask = new Task(() =>
            {
                if (item.state != TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_COMPLETE)
                    return;
                int orderResult = axKHOpenAPI1.SendOrder(
                    "종목손절매도",
                    GetScreenNum().ToString(),
                    item.ts.account,
                    CONST_NUMBER.SEND_ORDER_SELL,
                    item.itemCode,
                    item.curQnt,
                    (int)item.curPrice,
                    "03",//시장가
                    "" //원주문번호없음
                );
                if (orderResult == 0) //요청 성공시 (실거래는 안될 수 있음)
                {
                    AddOrderList(item);
                    item.SetSold(true, false);
                    coreEngine.SendLogMessage("ui -> 매도주문접수시도");
                    UpdateAutoTradingDataGridRow(item.itemCode, item, item.curPrice, ConstName.AUTO_TRADING_STATE_SELL_BEFORE_ORDER);

                }
                else
                {
                    coreEngine.SendLogMessage("자동 손절 요청 실패");
                }
            });
            coreEngine.requestTrDataManager.RequestTrData(requestItemInfoTask);
        }

        private void M_AddStratgyBtn_Click(object sender, EventArgs e)
        {
            string account = accountComboBox.Text;

            if (account.Length == 0)
            {
                MessageBox.Show("계좌를 선택해주세요");
                return;
            }

            string conditionName = M_BuyConditionComboBox.Text;
            Condition findCondition = null;

            if (conditionName.Length > 0)
            {
                findCondition = listCondition.Find(o => o.Name.Equals(conditionName));

                if (findCondition != null)
                {
                    TradingStrategy findStrategy = tradingStrategyList.Find(o => o.buyCondition.Name == findCondition.Name);
                    if (findStrategy != null)
                    {
                        MessageBox.Show("현재 진행중인 조건식으로 진행 할 수 없습니다");
                        return;
                    }
                }

            }
            else
            {
                MessageBox.Show("매수 조건식을 선택해주세요");
                return;
            }

            if (findCondition == null)
            {
                return;
            }

            string buyOrderOpt = "지정가";
            long totalInvestment = 0;

            //if (marketPriceRadio.Checked)
            //{
            //    buyOrderOpt = "시장가";
            //}
            //else
            //{
            //    buyOrderOpt = buyOrderOptionCombo.Text;
            //}

            if (M_allCostUpDown.Value == 0)
            {
                MessageBox.Show("총 투자금액을 설정해주세요");
                return;
            }

            totalInvestment = (long)M_allCostUpDown.Value;

            List<TradingStrategyADDItem> tradingStrategyItemList = new List<TradingStrategyADDItem>();
            //매매 전략

            string sellOrderOption = "지정가";

            bool usingBuyRestart = false;

            TradingStrategy ts = new TradingStrategy(
                account,
                findCondition,
                buyOrderOpt,
                totalInvestment,
                1,                                       //매수할 종목 1개
                sellOrderOption,
                false,                                   //재실행 끔
                usingBuyRestart
                );

            bool usingTickbuy = M_usingTickBuyCheck.Checked; //틱 가격 적용

            if (usingTickbuy)
            {
                int index = M_buyTickComboBox.SelectedIndex;
                if (index < 0)
                {
                    MessageBox.Show("매수 틱 단위를 선택해주세요");
                    return;
                }
                index++; //0:매수 호가 배열에서 0은 최우선 매수 호가이기 때문에 인덱스에 1을 더해줌
                ts.usingTickBuy = true;
                ts.tickBuyValue = index;
            }

            bool usingUpCheckAndCancel = M_UpAndCancelCheck.Checked; //미체결 상승 이격발생
            MartinGailManager.GetInstance().using_Up_And_Cancel = true;
            if (usingUpCheckAndCancel)
            {
                double index = (double)M_UpAndCancelUpdown.Value;
              
                MartinGailManager.GetInstance().Up_And_CancelValue = index;

            }
            bool usingOutStandAndCancel = M_orderCancelcheckBox.Checked; //미체결 상승 이격발생
            MartinGailManager.GetInstance().using_Outstand_UpAndCancel = usingOutStandAndCancel;
            if (usingOutStandAndCancel)
            {
                double index = (double)M_cancelValueUpdown.Value;

                MartinGailManager.GetInstance().OutStand_And_CancelValue = index;

            }

            bool usingTimeCancelCheckBox = M_timeCancelCheckBox.Checked; //미체결 상승 이격발생
            MartinGailManager.GetInstance().using_WaitAndCancel = usingTimeCancelCheckBox;
            if (usingTimeCancelCheckBox)
            {
                int index = (int)M_waitTimeUpdown.Value;

                MartinGailManager.GetInstance().Wait_And_CancelValue = index;

            }

            bool usingTrailBuy = M_usingTrailingBuyCheck.Checked; //트레일링 매수 적용

            if (usingTrailBuy)
            {
                int tickValue = (int)M_trailingUpDown.Value;
                if (tickValue <= 0)
                {
                    MessageBox.Show("트레일링 틱 단위를 선택해주세요");
                    return;
                }

                ts.usingTrailing = true;
                ts.trailTickValue = tickValue;
            }

            double takeProfitRate = (double)M_SellUpdown.Value;

            TradingStrategyItemWithUpDownValue takeProfitStrategy =
                 new TradingStrategyItemWithUpDownValue(
                         StrategyItemName.TAKE_PROFIT_SELL,
                         CHECK_TIMING.SELL_TIME,
                         "buyingPrice",
                         TradingStrategyItemWithUpDownValue.IS_TRUE_OR_FALE_TYPE.UPPER,
                         takeProfitRate);

            takeProfitStrategy.OnReceivedTrData += this.OnReceiveTrDataCheckProfitSell;

            ts.AddTradingStrategyItemList(takeProfitStrategy);
            ts.takeProfitRate = takeProfitRate;

            double stopLossRate = (double)M_SellUpdown.Value * -1;

            TradingStrategyItemWithUpDownValue stopLossStrategy = 
                new TradingStrategyItemWithUpDownValue(
                        StrategyItemName.STOPLOSS_SELL,
                        CHECK_TIMING.SELL_TIME,
                        "buyingPrice",
                        TradingStrategyItemWithUpDownValue.IS_TRUE_OR_FALE_TYPE.DOWN,
                        stopLossRate);

            stopLossStrategy.OnReceivedTrData += this.OnReceiveTrDataCheckStopLoss;

            ts.AddTradingStrategyItemList(stopLossStrategy);
            ts.stoplossRate = stopLossRate;

            if (MartinGailManager.GetInstance().CheckMartinValid(ts) < 0)
            {
                MessageBox.Show("마틴게일 전략을 확인해주세요 errcode : " + MartinGailManager.GetInstance().CheckMartinValid(ts));
                return;
            }

            tradingStrategyList.Add(ts);
            AddStrategyToDataGridView(ts);
            StartMonitoring(ts.buyCondition);

            MartinGailManager.GetInstance().SetMartinStrategy(ts, MartinGailManager.MARTIN_MAX_STEP);

            coreEngine.SendLogMessage("마틴 게일 전략이 입력됬습니다 \n 매수조건식 : " + ts.buyCondition.Name + "\n" + " 총투자금 : " + ts.totalInvestment + "\n" + " 종목수 : " + ts.buyItemCount);
        }

       
        private void UpdateTradingStrategyBuy(string orderNum, bool buyComplete, int allQnt)
        {

            foreach (TradingStrategy ts in tradingStrategyList)
            {
                TradingItem tradeItem = ts.tradingItemList.Find(o => o.buyOrderNum.Equals(orderNum));
                if (tradeItem != null)
                {
                    tradeItem.curQnt = allQnt;
                    tradeItem.SetCompleteBuying(buyComplete);
                    if (buyComplete)
                        tradeItem.ts.StrategyOnReceiveBuyChejanUpdate(tradeItem.itemCode, (int)tradeItem.buyingPrice,  tradeItem.buyingQnt, TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_COMPLETE);
                    else
                        tradeItem.ts.StrategyOnReceiveBuyChejanUpdate(tradeItem.itemCode, (int)tradeItem.buyingPrice, tradeItem.buyingQnt, TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_NOT_COMPLETE_OUTCOUNT);

                }
            }
        }
        private void UpdateTradingStrategySellData(string orderNum, bool sellComplete, int allQnt)
        {

            foreach (TradingStrategy ts in tradingStrategyList)
            {
                TradingItem tradeItem = ts.tradingItemList.Find(o => o.sellOrderNum.Equals(orderNum));
                if (tradeItem != null)
                {
                    tradeItem.curQnt = allQnt;
                    tradeItem.SetCompleteSold(sellComplete);
                    if (sellComplete)
                    {
                        tradeItem.ts.StrategyOnReceiveSellChejanUpdate(tradeItem.itemCode, (int)tradeItem.sellPrice, tradeItem.sellQnt, TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_COMPLETE);
                        //ts.tradingItemList.Remove(tradeItem);
                    }  
                    else
                        tradeItem.ts.StrategyOnReceiveSellChejanUpdate(tradeItem.itemCode, (int)tradeItem.sellPrice, tradeItem.sellQnt, TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_NOT_COMPLETE_OUTCOUNT);

                }
            }
        }
        private void UpdateBuyTradingItemOutstand(string orderNum, int outStand)
        {

            foreach (TradingStrategy ts in tradingStrategyList)
            {
                TradingItem tradeItem = ts.tradingItemList.Find(o => o.buyOrderNum.Equals(orderNum));

                if (tradeItem != null)
                {
                    tradeItem.SetState(TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_NOT_COMPLETE_OUTCOUNT);
                    tradeItem.outStandingQnt = outStand;
                }
            }
        }
        private void UpdateSellTradingItemOutstand(string orderNum, int outStand)
        {

            foreach (TradingStrategy ts in tradingStrategyList)
            {
                TradingItem tradeItem = ts.tradingItemList.Find(o => o.sellOrderNum.Equals(orderNum));

                if (tradeItem != null)
                {
                    tradeItem.SetState(TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_NOT_COMPLETE_OUTCOUNT);
                    tradeItem.outStandingQnt = outStand;
                }
            }
        }
        private List<BalanceSellStrategy> GetTryingSellList(string itemCode)
        {
            return this.tryingSellList.FindAll(o => itemCode.Contains(o.itemCode));
        }

        private void UpdateTradingItemRemoveByCancel(string orderNum, bool buy)
        {

            foreach (TradingStrategy ts in tradingStrategyList)
            {
                if (buy)
                {
                    TradingItem tradeItem = ts.tradingItemList.Find(o => o.buyCancelOrderNum.Equals(orderNum));
                    if (tradeItem != null)
                    {
                        tradeItem.SetBuyCancel(true);
                        //ts.tradingItemList.Remove(tradeItem);
                        autoTradingDataGrid["매매진행_진행상황", tradeItem.ui_rowItem.Index].Value = ConstName.AUTO_TRADING_STATE_BUY_CANCEL_ALL;
                    }
                }
                else
                {
                    TradingItem tradeItem = ts.tradingItemList.Find(o => o.sellCancelOrderNum.Equals(orderNum));
                    if (tradeItem != null)
                    {
                        tradeItem.SetSellCancel(true);
                        //ts.tradingItemList.Remove(tradeItem);
                        autoTradingDataGrid["매매진행_진행상황", tradeItem.ui_rowItem.Index].Value = ConstName.AUTO_TRADING_STATE_SELL_CANCEL_ALL;
                    }
                }

            }
        }
        public void AddOrderList(TradingItem item)
        {
            tryingOrderList.Add(item);
        }

        private void Form_FormClosing(object sender, EventArgs e)
        {
            SaveSetting();
        }
        public void SaveSetting()
        {
            using (StreamWriter streamWriter = new StreamWriter("setting.txt", false))
            {
                //false : 덮어쓰기
                streamWriter.WriteLine("M_allCostUpDown" + ";" + M_allCostUpDown.Value);

                streamWriter.WriteLine("M_usingTickBuyCheck" + ";" + M_usingTickBuyCheck.Checked);
                streamWriter.WriteLine("M_buyTickComboBox" + ";" + M_buyTickComboBox.SelectedIndex);

                streamWriter.WriteLine("M_usingTrailingBuyCheck" + ";" + M_usingTrailingBuyCheck.Checked);
                streamWriter.WriteLine("M_trailingUpDown" + ";" + (int)M_trailingUpDown.Value);

                streamWriter.WriteLine("M_timeCancelCheckBox" + ";" + M_timeCancelCheckBox.Checked);
                streamWriter.WriteLine("M_waitTimeUpdown" + ";" + (int)M_waitTimeUpdown.Value);

                streamWriter.WriteLine("M_SellUpdown" + ";" + (double)M_SellUpdown.Value);

            }
        }
        public void LoadSetting()
        {
            try
            {
                using (StreamReader streamReader = new StreamReader("setting.txt"))
                {
                  
                    while (streamReader.EndOfStream == false)
                    {
                        string line = streamReader.ReadLine();
                        string[] strringArray = line.Split(';');

                        switch(strringArray[0])
                        {
                            case "M_allCostUpDown" :
                                M_allCostUpDown.Value = int.Parse(strringArray[1]);
                                break;
                            case "M_usingTickBuyCheck":
                                M_usingTickBuyCheck.Checked = bool.Parse(strringArray[1]);
                                break;
                            case "M_buyTickComboBox":
                                M_buyTickComboBox.SelectedIndex = int.Parse(strringArray[1]);
                                break;
                            case "M_usingTrailingBuyCheck":
                                M_usingTrailingBuyCheck.Checked = bool.Parse(strringArray[1]);
                                break;
                            case "M_trailingUpDown":
                                M_trailingUpDown.Value = int.Parse(strringArray[1]);
                                break;
                            case "M_timeCancelCheckBox":
                                M_timeCancelCheckBox.Checked = bool.Parse(strringArray[1]);
                                break;
                            case "M_waitTimeUpdown":
                                M_waitTimeUpdown.Value = int.Parse(strringArray[1]);
                                break;
                            case "M_SellUpdown":
                                M_SellUpdown.Value =  (decimal)(double.Parse(strringArray[1]));
                                break;

                        }
                        
                    }
                  


                }
            }
            catch(Exception e)
            {

            }
        }
    }
}
