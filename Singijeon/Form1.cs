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
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Singijeon
{
    public partial class tradingStrategyGridView : Form
    {
        CoreEngine coreEngine;
        private string currentAccount = string.Empty;
        public static string account = string.Empty;
        private int screenNum = 1000;
        private string server = "0";
        public static double FEE_RATE = 1;

        List<Condition> listCondition = new List<Condition>();
        
        public List<TradingStrategy> tradingStrategyList = new List<TradingStrategy>();

        public Hashtable doubleCheckHashTable = new Hashtable();

        List<BalanceSellStrategy> balanceSellStrategyList = new List<BalanceSellStrategy>();
        List<TrailingItem> trailingList = new List<TrailingItem>();
  
       
        List<StockItem> stockItemList = new List<StockItem>(); //상장종목리스트

        List<TradingItem> tryingOrderList = new List<TradingItem>(); //주문접수시도

        //같은 종목에 대하여 주문이 여러개 들어가도 주문순서대로 응답이 오기 때문에 각각의 리스트로 들어가게됨

        List<SettlementItem> tryingSettlementItemList = new List<SettlementItem>(); //청산 접수 시도(주문번호만 따기위한 리스트)
        List<SettlementItem> settleItemList = new List<SettlementItem>(); //진행중인 청산 시도

        List<BalanceSellStrategy> tryingSellList = new List<BalanceSellStrategy>(); //잔고 매도 접수 시도(주문번호 따는 리스트)

        Dictionary<string, NotConclusionItem> nonConclusionList = new Dictionary<string, NotConclusionItem>();

        Form3 printForm = null;

        public tradingStrategyGridView()
        {
            InitializeComponent();

            coreEngine = CoreEngine.GetInstance();
            coreEngine.SetAxKHOpenAPI(axKHOpenAPI1);
            coreEngine.Start();

            OpenSecondWindow();

            printForm = new Form3(axKHOpenAPI1);
            OpenThirdWindow();

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
            autoTradingDataGrid.CellClick += AutoTradingDataGridView_CellClick;
            tsDataGridView.CellClick += TradingStrategyGridView_CellClick;

            accountBalanceDataGrid.SelectionChanged += AccountDataGridView_SelectionChanged;

            axKHOpenAPI1.OnEventConnect += API_OnEventConnect; //로그인
            axKHOpenAPI1.OnReceiveConditionVer += API_OnReceiveConditionVer; //검색 받기
            axKHOpenAPI1.OnReceiveRealCondition += API_OnReceiveRealCondition; //실시간 검색
            axKHOpenAPI1.OnReceiveTrCondition += API_OnReceiveTrCondition; //검색

            axKHOpenAPI1.OnReceiveTrData += API_OnReceiveTrData; //정보요청
            axKHOpenAPI1.OnReceiveTrData += API_OnReceiveTrDataHoga; //정보요청(호가)
            axKHOpenAPI1.OnReceiveChejanData += API_OnReceiveChejanData; //체결잔고
            axKHOpenAPI1.OnReceiveRealData += API_OnReceiveRealData; //실시간정보
            axKHOpenAPI1.OnReceiveRealData += API_OnReceiveRealDataHoga; //실시간정보
            
            MartinGailManager.GetInstance().Init(axKHOpenAPI1, this);

            //LoadSetting();
           
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
                BuyConditionDoubleComboBox.Items.Add(condition.Name);

                interestConditionListBox.Items.Add(condition.Name);
            }
        }

        public bool CheckCanBuyItem(string itemCode)
        {
            //모든 전략에서 매도 완료됬거나 매수취소된것
            bool returnBuy = true;
            foreach(var ts in  tradingStrategyList)
            {
                List<TradingItem> tradeItemArray = ts.tradingItemList.FindAll(o => o.itemCode.Contains(itemCode));
                foreach (var item in tradeItemArray)
                {
                    bool canBuy = item.IsCompleteSold() || item.IsBuyCancel();
                    if (!canBuy)
                        returnBuy = canBuy;
                }
               
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
                coreEngine.SendLogMessage("종목명 = " + axKHOpenAPI1.GetMasterCodeName(itemCode));
                coreEngine.SendLogMessage("_________________________________");

                //종목 편입(어떤 전략(검색식)이었는지)

                TradingStrategy ts = tradingStrategyList.Find(o => o.buyCondition.Name.Equals(conditionName));

                if (doubleCheckHashTable.ContainsKey(conditionName))
                {
                    TradingStrategy ts_doubleCheck = (TradingStrategy)doubleCheckHashTable[conditionName];
                    if(ts_doubleCheck.doubleCheckItemCode.Contains(conditionName) == false)
                    {
                        coreEngine.SendLogWarningMessage(ts_doubleCheck.buyCondition.Name + " 이중체크 리스트에 추가 : " + axKHOpenAPI1.GetMasterCodeName(itemCode));
                        ts_doubleCheck.doubleCheckItemCode.Add(itemCode);
                    }
                }

                if (ts != null)
                {
                    coreEngine.SendLogMessage(conditionName + " 남은 가능 매수 종목수 : " + ts.remainItemCount);
                    if (ts.remainItemCount > 0)
                    {
                        StockItem stockItem = stockItemList.Find(o => o.Code.Equals(itemCode));
                        if (stockItem != null) //시장 종목 리스트 있는것
                        {
                            coreEngine.SendLogMessage(conditionName + " 리스트확인 ");
                            if (ts.CheckBuyPossibleStrategyAddedItem()) //모든 구매조건을 체크
                            {
                                //TradingItem tradeItem = ts.tradingItemList.Find(o => o.itemCode.Contains(itemCode)); //한 전략에서 구매하려했던 종목은 재편입하지 않음
                                if(ts.usingDoubleCheck)
                                {
                                    if (ts.doubleCheckItemCode.Contains(itemCode) == false)
                                    {
                                        Console.WriteLine(conditionName + " 이중체크 실패 : " + axKHOpenAPI1.GetMasterCodeName(itemCode));
                                        return;
                                    }
                                      
                                    coreEngine.SendLogWarningMessage(conditionName + " 이중체크 통과 : " + axKHOpenAPI1.GetMasterCodeName(itemCode));
                                }

                                if (ts.usingVwma)
                                {
                                    printForm.RequestItem(itemCode, delegate (string _itemCode) {

                                        if (printForm.vwma_state == Form3.VWMA_CHART_STATE.DEAD_CROSS)
                                        {
                                            return;
                                        }

                                        if (ts.usingVwma && printForm.vwma_state == Form3.VWMA_CHART_STATE.UP_STAY)
                                        {
                                            //골든크로스를 목적으로 미리 up상태인것을 배재한다
                                            coreEngine.SendLogMessage(axKHOpenAPI1.GetMasterCodeName(_itemCode));
                                            return;
                                        }

                                      
                                        TrailingItem trailingItem = trailingList.Find(o => o.itemCode.Contains(_itemCode));

                                        if (CheckCanBuyItem(_itemCode) && trailingItem == null)
                                        {
                                            ts.remainItemCount--; //남을 매수할 종목수-1
                                             coreEngine.SaveItemLogMessage(_itemCode, "트레일링 추가 검색명 = " + conditionName);
                               
                                            ts.StrategyConditionReceiveUpdate(_itemCode, 0, 0, TRADING_ITEM_STATE.AUTO_TRADING_STATE_SEARCH_AND_CATCH);
                                            TryBuyItem(ts, _itemCode);
                                        }
                                        
                                    });
                                }
                                else
                                {
                                    TrailingItem trailingItem = trailingList.Find(o => o.itemCode.Contains(itemCode));

                                    if (CheckCanBuyItem(itemCode) && trailingItem == null)
                                    {
                                        ts.remainItemCount--; //남을 매수할 종목수-1
                                        coreEngine.SaveItemLogMessage(itemCode, "트레일링 추가 검색명 = " + conditionName);
                                        ts.StrategyConditionReceiveUpdate(itemCode, 0, 0, TRADING_ITEM_STATE.AUTO_TRADING_STATE_SEARCH_AND_CATCH);
                                        TryBuyItem(ts, itemCode);
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
        private void TryBuyItem(TradingStrategy ts, string itemCode)
        {
            if (ts.usingTickBuy || ts.usingTrailing || ts.usingPercentageBuy)
            {

                //종목의 호가를 알아오고 틱 설정 단위로 산다
                Task requestItemInfoTask = new Task(() =>
                {
                    coreEngine.SendLogMessage(ConstName.RECEIVE_TR_DATA_HOGA + "요청 종목코드 : " + itemCode);
                    axKHOpenAPI1.SetInputValue("종목코드", itemCode);

                    int result = axKHOpenAPI1.CommRqData(ConstName.RECEIVE_TR_DATA_HOGA + ":" + itemCode + ":" + ts.buyCondition.Uid, "opt10004", 0, GetScreenNum().ToString());

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
                coreEngine.SaveItemLogMessage(itemCode, "즉시 매수 ");
                //종목의 현재가를 알아오고 그가격으로 산다
                Task requestItemInfoTask = new Task(() =>
                {
               
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
                                coreEngine.SaveItemLogMessage(itemcode,"종목 매수 시도");

                                if (server.Equals(ConstName.TEST_SERVER))
                                {
                                    string warning = "모의투자 환경에서 \n 현재가 1,000원 미만인 종목, \n 총 발행 주식수 100,000주 미만 종목, \n 프리보드 종목, \n 관리종목, \n 정리매매, \n 투자주의, \n 투자경고, \n 투자위험종목, \n ELW종목 은 주문제외됩니다";
                                    coreEngine.SaveItemLogMessage(itemcode, warning);
                                    coreEngine.SendLogMessage(warning);
                                }

                                int orderResult =

                                axKHOpenAPI1. SendOrder(
                                    ConstName.SEND_ORDER_BUY,
                                    GetScreenNum().ToString(),
                                    ts.account,
                                    CONST_NUMBER.SEND_ORDER_BUY,//1:신규매수
                                    itemcode,
                                    (int)(ts.itemInvestment / i_price),
                                     (ts.buyOrderOption == ConstName.ORDER_JIJUNGGA) ? i_price : 0,
                                    ts.buyOrderOption,//지정가
                                    "" //원주문번호없음
                                );

                                if (orderResult == 0)
                                {
                                    coreEngine.SaveItemLogMessage(itemcode, "매수주문 성공");

                                    TradingItem tradingItem = new TradingItem(ts, itemcode, axKHOpenAPI1.GetMasterCodeName(itemcode), i_price, i_qnt, false, false, ts.buyOrderOption);
                                    tradingItem.SetBuy(true);
                                    tradingItem.SetConditonUid(conditionUid);

                                    ts.tradingItemList.Add(tradingItem); //매수전략 내에 매매진행 종목 추가
                                    AddOrderList(tradingItem);

                                    string fidList = "9001;302;10;11;25;12;13"; //9001:종목코드,302:종목명
                                    axKHOpenAPI1.SetRealReg("9001", itemcode, fidList, "1");
                                    coreEngine.SendLogMessage(axKHOpenAPI1.GetMasterCodeName(itemcode) + " realreg");

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
                currentAccount = account;

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
                    long lBalanceCnt;
                    string balanceCnt = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "보유수량").Trim();
                    long.TryParse(balanceCnt, out lBalanceCnt);

                    double dBuyingPrice;
                    string buyingPrice = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "평균단가").Trim();
                    double.TryParse(buyingPrice, out dBuyingPrice);

                    int iPrice;
                    string price = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "현재가").Trim();
                    int.TryParse(price, out iPrice);
                    iPrice = Math.Abs(iPrice);

                    long lEstimatedAmount;
                    string estimatedAmount = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "평가금액").Trim();
                    long.TryParse(estimatedAmount, out lEstimatedAmount);

                    long lProfitAmount;
                    string profitAmount = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "손익금액").Trim();
                    long.TryParse(profitAmount, out lProfitAmount);

                    long lBuyingAmount;
                    string buyingAmount = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "매입금액").Trim();
                    long.TryParse(buyingAmount, out lBuyingAmount);

                    //double dProfitRate = 100 * ((iPrice - dBuyingPrice) / dBuyingPrice) - FEE_RATE;
                    double dProfitRate = GetProfitRate((double)iPrice, dBuyingPrice);
                    int rowIndex = accountBalanceDataGrid.Rows.Add();

                    Hashtable uiTable = new Hashtable() { { "계좌잔고_종목코드", itemCode }, { "계좌잔고_종목명", itemName }, { "계좌잔고_보유수량", lBalanceCnt }, { "계좌잔고_평균단가", dBuyingPrice }, { "계좌잔고_평가금액", lEstimatedAmount }, { "계좌잔고_매입금액", lBuyingAmount }, { "계좌잔고_손익금액", lProfitAmount }, { "계좌잔고_손익률", dProfitRate } };
                    Update_AccountBalanceDataGrid_UI(uiTable, rowIndex);

                }
                string fidList = "9001;302;10;11;25;12;13"; //9001:종목코드,302:종목명
                axKHOpenAPI1.SetRealReg("9001", codeList, fidList, "1");
                coreEngine.SendLogWarningMessage("SetRealReg  : " + codeList);

                SaveLoadManager.GetInstance().SetForm(this, axKHOpenAPI1);
                SaveLoadManager.GetInstance().DeserializeStrategy();
                SaveLoadManager.GetInstance().DeserializeTrailing();
            }
            else if (e.sRQName == "실시간미체결요청")
            {

                int count = axKHOpenAPI1.GetRepeatCnt(e.sTrCode, e.sRQName);

                for (int i = 0; i < count; i++)
                {
                    string orderNum = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "주문번호")).ToString();
                    string stockCode = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "종목코드").Trim();
                    string stockName = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "종목명").Trim();
                    int orderQnt = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "주문수량"));
                    int orderPrice = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "주문가격"));
                    int outstandingNumber = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "미체결수량"));
                    int currentPrice = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "현재가").Replace("-", ""));
                    string orderGubun = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "주문구분").Trim();
                    string orderTime = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "시간").Trim();
                    
                    int index = outstandingDataGrid.Rows.Add();
                    nonConclusionList.Add(orderNum, new NotConclusionItem(orderNum, stockCode, orderGubun, stockName, orderQnt,orderPrice, outstandingNumber));
                    Hashtable outstandingTable = new Hashtable { { "미체결_주문번호", orderNum }, { "미체결_종목코드", stockCode }, { "미체결_종목명", stockName }, { "미체결_주문수량", orderQnt }, { "미체결_미체결량", outstandingNumber } };
                    Update_OutStandingDataGrid_UI(outstandingTable, index);
                }
            }
            else
            {
                coreEngine.SendLogWarningMessage(e.sRQName + " 처리 : " + e.sErrorCode + " : " + e.sMessage + " : " + e.sTrCode);
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
               
                List<TradingItem> tradeItemListAll = GetAllTradingItemData(itemCode);

                foreach (TradingItem tradeItem in tradeItemListAll)
                {
                    tradeItem.UpdateCurrentPrice(c_lPrice);

                    if (tradeItem.IsCompleteBuying() && tradeItem.IsCompleteSold() == false && tradeItem.buyingPrice != 0) //매도 진행안된것 
                    {
                        double realProfitRate = GetProfitRate((double)c_lPrice, (double)tradeItem.buyingPrice);

       
                        //자동 감시 주문 체크
                        if (tradeItem.state >= TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_COMPLETE
                            && tradeItem.state < TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_COMPLETE)
                            tradeItem.ts.CheckUpdateTradingStrategyAddedItem(tradeItem, realProfitRate, CHECK_TIMING.SELL_TIME);

                        if (tradeItem.ts.usingRestart && tradeItem.ts.remainItemCount == 0) //restart 처리
                        {
                            tradeItem.ts.remainItemCount = tradeItem.ts.buyItemCount;
                        }
                    }
                    else
                    {
                        //시간체크 주문취소
                        if (tradeItem.state == TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_NOT_COMPLETE)
                            tradeItem.ts.CheckUpdateTradingStrategyAddedItem(tradeItem, DateTime.Now.Ticks, CHECK_TIMING.BUY_ORDER_BEFORE_CONCLUSION);
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
                                                   bss.profitOrderOption == ConstName.ORDER_SIJANGGA ? 0 : (int)c_lPrice,
                                                  bss.profitOrderOption,
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
                                                      bss.stoplossOrderOption == ConstName.ORDER_SIJANGGA ? 0 : (int)c_lPrice,
                                                     bss.stoplossOrderOption,
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
        private void UpdateAutoTradingDataGridRowWinLose(string itemCode, TradingItem tradeItem, string winLose)
        {
            if (tradeItem != null && tradeItem.GetUiConnectRow() != null)
            {
                tradeItem.GetUiConnectRow().Cells["매매진행_손절익절"].Value = winLose;
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
      
        private void API_OnReceiveChejanData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveChejanDataEvent e)
        {
            coreEngine.SendLogMessage(e.sGubun);

            if (e.sGubun.Equals(ConstName.RECEIVE_CHEJAN_DATA_SUBMIT_OR_CONCLUSION))
            {
                //접수 혹은 체결
                string orderState = axKHOpenAPI1.GetChejanData(913).Trim();
                string orderType = axKHOpenAPI1.GetChejanData(905).Replace("+", "").Replace("-", "").Trim();

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

                int i_unitConclusionQuantity = 0;
                int.TryParse(unitConclusionQuantity,out i_unitConclusionQuantity);

                string price = axKHOpenAPI1.GetChejanData(10).Trim();

                int i_allQuantity, i_averagePrice = 0;
                string allQuantity = axKHOpenAPI1.GetChejanData(930).Trim();
                int.TryParse(allQuantity, out i_allQuantity);
                string averagePrice = axKHOpenAPI1.GetChejanData(931).Trim();
                int.TryParse(averagePrice, out i_averagePrice);

                coreEngine.SaveItemLogMessage(itemCode,"___________접수/체결_____________");
                coreEngine.SaveItemLogMessage(itemCode, "종목명 : " + axKHOpenAPI1.GetMasterCodeName(itemCode));
                coreEngine.SaveItemLogMessage(itemCode, "주문상태 : " + orderState);
                coreEngine.SaveItemLogMessage(itemCode, "주문번호 : " + ordernum);
                coreEngine.SaveItemLogMessage(itemCode, "종목코드 : " + itemCode);
                coreEngine.SaveItemLogMessage(itemCode, "주문구분 : " + orderType);
                coreEngine.SaveItemLogMessage(itemCode, "주문가격 : " + orderPrice);
                coreEngine.SaveItemLogMessage(itemCode, "현재가 : " + price);
                coreEngine.SaveItemLogMessage(itemCode, "매매구분 : " + tradingType);
                coreEngine.SaveItemLogMessage(itemCode, "주문수량 : " + orderQuantity);
                coreEngine.SaveItemLogMessage(itemCode, "체결량(누적체결량) :" + conclusionQuantity);
                coreEngine.SaveItemLogMessage(itemCode, "미체결 수량 :" + outstanding);
                coreEngine.SaveItemLogMessage(itemCode, "단위체결량(체결당 체결량) :" + unitConclusionQuantity);
                coreEngine.SaveItemLogMessage(itemCode, "매입단가 :" + i_averagePrice);
                coreEngine.SaveItemLogMessage(itemCode, "총보유 수량 :" + i_allQuantity);
                coreEngine.SaveItemLogMessage(itemCode, "________________________________");

                if (orderState.Equals(ConstName.RECEIVE_CHEJAN_DATA_SUBMIT))
                {
                    TradingItem CheckItemExist = this.tryingOrderList.Find(o => (itemCode.Contains(o.itemCode)));
                    if (CheckItemExist == null)
                    {
                        coreEngine.SaveItemLogMessage(itemCode, " 원주문찾기 실패");

                        //외부프로그램에서  매도했을시 처리
                        if (orderType.Equals(ConstName.RECEIVE_CHEJAN_DATA_SELL))
                        {
                            foreach (TradingStrategy ts in tradingStrategyList)
                            {
                                TradingItem item = ts.tradingItemList.Find(o => o.itemCode.Equals(itemCode));
                                if (item != null)
                                {
                                    coreEngine.SaveItemLogMessage(itemCode, "현재 수량 : " + item.curQnt);

                                    if (!string.IsNullOrEmpty(orderQuantity)
                                    && int.Parse(orderQuantity) > 0
                                    && item.curQnt == int.Parse(orderQuantity)) //일부 매도는 고려하지않는다
                                    {
                                        item.sellPrice = long.Parse(orderPrice);
                                        item.sellOrderNum = ordernum;
                                        item.sellQnt = int.Parse(orderQuantity);
                                        item.SetState(TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_NOT_COMPLETE);
                                        UpdateSellAutoTradingDataGridStateOnly(ordernum, ConstName.AUTO_TRADING_STATE_SELL_NOT_COMPLETE);
                                        item.ts.StrategyOnReceiveSellOrderUpdate(item.itemCode, (int)item.buyingPrice, item.buyingQnt, TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_NOT_COMPLETE);
                                        coreEngine.SaveItemLogMessage(itemCode, "자동 매도 요청 - " + "종목코드 : " + itemCode + " 주문번호 : " + ordernum);
                                    }
                                }
                            }
                        }
                        else if (orderType.Equals(ConstName.RECEIVE_CHEJAN_DATA_BUY))
                        {
                            coreEngine.SaveItemLogMessage(itemCode, " 물타기 스텝 : 주문완료"); //내가 수동으로 사든 프로그램이 사든 물타기로 취급
                            foreach (TradingStrategy ts in tradingStrategyList)
                            {
                                List<TradingItem> itemArray = ts.tradingItemList.FindAll(o => o.itemCode.Equals(itemCode));
                                foreach (var item in itemArray)
                                {
                                    if (item != null)
                                    {
                                        coreEngine.SaveItemLogMessage(itemCode, "물타기 전 현재 수량 : " + item.curQnt);

                                        if (!string.IsNullOrEmpty(orderQuantity)
                                        && int.Parse(orderQuantity) > 0
                                        && item.state >= TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_COMPLETE
                                        && item.state < TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_COMPLETE)
                                        {
                                            coreEngine.SaveItemLogMessage(itemCode, "uid : " + item.Uid + " 물타기 ordernum : " + ordernum);
                                            item.buyOrderNum = ordernum;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    //주문번호 따오기 위한 부분 
                    if (CheckItemExist != null)
                    {
                        if (orderType.Equals(ConstName.RECEIVE_CHEJAN_DATA_BUY))
                        {
                            coreEngine.SaveItemLogMessage(itemCode, "주문접수완료");
                            coreEngine.SaveItemLogMessage(itemCode, "수량 : " + i_orderQuantity);
                            List<TradingItem> items = this.tryingOrderList.FindAll(o => (itemCode.Contains(o.itemCode)));

                            if(items.Count > 1)
                            {
                                coreEngine.SaveItemLogMessage(itemCode, "주문리스트에 중복된 종목이 있습니다");
                            }
                            foreach (var item in items)
                            {
                                coreEngine.SaveItemLogMessage(itemCode, "찾아낸 종목명 : " + axKHOpenAPI1.GetMasterCodeName(itemCode) + "orderNum : " + ordernum);
                                coreEngine.SaveItemLogMessage(itemCode, "찾아낸 종목 주문 수량 : " + item.buyingQnt);
                                coreEngine.SaveItemLogMessage(itemCode, "찾아낸 종목 가격 : " + orderPrice);

                                if (!string.IsNullOrEmpty(orderQuantity) 
                                    && int.Parse(orderQuantity) > 0 )
                                {
                                    item.buyOrderNum = ordernum;
                                    item.buyingQnt = int.Parse(orderQuantity);
                                    item.SetState(TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_NOT_COMPLETE);

                                    //접수리스트에서만 지움
                                    RemoveOrderList(item);
                                    UpdateBuyAutoTradingDataGridStateOnly(ordernum, ConstName.AUTO_TRADING_STATE_BUY_NOT_COMPLETE);

                                    item.ts.StrategyBuyOrderUpdate(item.itemCode, (int)item.buyingPrice, item.buyingQnt, TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_NOT_COMPLETE);
                                    coreEngine.SaveItemLogMessage(itemCode, "접수 완료 - " + "종목코드 : " + itemCode + " 주문번호 : " + ordernum);
                                }
                            }

                        }
                        else if (orderType.Equals(ConstName.RECEIVE_CHEJAN_DATA_SELL))
                        {
                            TradingItem item = this.tryingOrderList.Find(o => (itemCode.Contains(o.itemCode)));
                          
                            item.sellPrice = long.Parse(orderPrice);
                            item.sellOrderNum = ordernum;
                            item.sellQnt = int.Parse(orderQuantity);
                            item.SetState(TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_NOT_COMPLETE);

                            RemoveOrderList(item); //접수리스트에서만 지움

                            UpdateSellAutoTradingDataGridStateOnly(ordernum, ConstName.AUTO_TRADING_STATE_SELL_NOT_COMPLETE);
                            item.ts.StrategyOnReceiveSellOrderUpdate(item.itemCode, (int)item.buyingPrice, item.buyingQnt, TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_NOT_COMPLETE);
                            coreEngine.SaveItemLogMessage(itemCode, "자동 매도 요청 - " + "종목코드 : " + itemCode + " 주문번호 : " + ordernum);
                        }
                        else if (orderType.Equals(ConstName.RECEIVE_CHEJAN_CANCEL_BUY_ORDER))
                        {
                            coreEngine.SaveItemLogMessage(itemCode, "!!!!!!!!!매수 취소 요청!!!!!!!!");

                            TradingItem item = this.tryingOrderList.Find(o => (itemCode.Contains(o.itemCode)));
                          
                            item.buyCancelOrderNum = ordernum;
                            RemoveOrderList(item); ; //접수리스트에서만 지움
                            coreEngine.SaveItemLogMessage(itemCode, "매수 취소 요청 - " + "종목코드 : " + itemCode + " 주문번호 : " + ordernum);
                        }
                        else if (orderType.Equals(ConstName.RECEIVE_CHEJAN_CANCEL_SELL_ORDER))
                        {
                            TradingItem item = this.tryingOrderList.Find(o => (itemCode.Contains(o.itemCode)));
                          
                            item.sellCancelOrderNum = ordernum;
                            RemoveOrderList(item); //접수리스트에서만 지움
                            coreEngine.SaveItemLogMessage(itemCode, "매도 취소 요청 - " + "종목코드 : " + itemCode + " 주문번호 : " + ordernum);
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
                                            && row.Cells["매매진행_매도량"].Value != null
                                            && row.Cells["매매진행_매도량"].Value.ToString() == bss.sellQnt.ToString()
                                        )
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
                    Hashtable uiOrderTable = new Hashtable { { "주문_주문번호", ordernum }, { "주문_계좌번호", account }, { "주문_시간", time }, { "주문_종목코드", itemCode }, { "주문_종목명", itemName }, { "주문_매매구분", orderType }, { "주문_가격구분", tradingType }, { "주문_주문량", orderQuantity }, { "주문_주문가격", orderPrice } };
                    Update_OrderDataGrid_UI(uiOrderTable, rowIndex);

                    int index = outstandingDataGrid.Rows.Add();
                    Hashtable outstandingTable = new Hashtable { { "미체결_주문번호", ordernum }, { "미체결_종목코드", itemCode }, { "미체결_종목명", itemName }, { "미체결_주문수량", orderQuantity }, { "미체결_미체결량", orderQuantity } };
                    Update_OutStandingDataGrid_UI(uiOrderTable, rowIndex);

                }
                else if (orderState.Equals(ConstName.RECEIVE_CHEJAN_DATA_CONCLUSION))
                {
                    coreEngine.SaveItemLogMessage(itemCode, "체결 프로세스 : " + outstanding + " / " + conclusionQuantity);
                    if (int.Parse(outstanding) == 0 && string.IsNullOrEmpty(conclusionQuantity) == false)
                    {
                        if (orderType.Contains(ConstName.RECEIVE_CHEJAN_DATA_BUY))
                        {
                            coreEngine.SaveItemLogMessage(itemCode, "매수 체결 단위 체결량: " + i_unitConclusionQuantity);
                            UpdateTradingStrategyBuy(ordernum, true, i_unitConclusionQuantity, i_orderPrice);
                            UpdateBuyAutoTradingDataGridState(ordernum, true);
                        }
                        else if (orderType.Contains(ConstName.RECEIVE_CHEJAN_DATA_SELL))
                        {
                            //자동 매매매 진행중일때
                            UpdateTradingStrategySellData(ordernum, true, int.Parse(conclusionQuantity));
                            UpdateSellAutoTradingDataGridStatePrice(ordernum, conclusionPrice);

                            //보유잔고 매도
                            BalanceSellStrategy bss = balanceSellStrategyList.Find(o => o.orderNum.Equals(ordernum));
                            if (bss != null)
                            {
                                foreach (DataGridViewRow row in accountBalanceDataGrid.Rows)
                                {
                                    if (row.Cells["계좌잔고_종목코드"].Value != null && row.Cells["계좌잔고_종목코드"].Value.ToString().Replace("A","").Contains(bss.itemCode))
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
                                UpdateTradingStrategyBuy(ordernum, false, i_unitConclusionQuantity, i_orderPrice);

                                UpdateBuyTradingItemOutstand(ordernum, int.Parse(outstanding));
                                UpdateBuyAutoTradingDataGridState(ordernum,  false);
                            }
                            else if (orderType.Contains(ConstName.RECEIVE_CHEJAN_DATA_SELL))
                            {
                                UpdateTradingStrategySellData(ordernum, false, int.Parse(conclusionQuantity));

                                UpdateSellTradingItemOutstand(ordernum, int.Parse(outstanding));
                                UpdateSellAutoTradingDataGridStatePrice(ordernum, conclusionPrice);
                            }
                        }
                    }

                    int rowIndex = conclusionDataGrid.Rows.Add();

                    Hashtable uiTable = new Hashtable { { "체결_주문번호", ordernum }, { "체결_체결시간", time }, { "체결_종목코드", itemCode }, { "체결_종목명", itemName }, { "체결_주문량", orderQuantity }, { "체결_단위체결량", unitConclusionQuantity }, { "체결_누적체결량", conclusionQuantity }, { "체결_체결가", conclusionPrice }, { "체결_매매구분", orderType } };
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

                coreEngine.SaveItemLogMessage(itemCode, "________________잔고_____________");
                coreEngine.SaveItemLogMessage(itemCode, "종목코드 : " + itemCode);
                coreEngine.SaveItemLogMessage(itemCode, "종목명 : " + axKHOpenAPI1.GetMasterCodeName(itemCode));
                coreEngine.SaveItemLogMessage(itemCode, "보유수량 : " + balanceQnt);
                coreEngine.SaveItemLogMessage(itemCode, "주문가능수량(매도가능) : " + orderAvailableQnt);
                coreEngine.SaveItemLogMessage(itemCode, "매수매도구분 :" + tradingType);
                coreEngine.SaveItemLogMessage(itemCode, "매입단가 :" + buyingPrice);
                coreEngine.SaveItemLogMessage(itemCode, "총매입가 :" + totalBuyingPrice);
                //coreEngine.SendLogMessage("손익률 :" + profitRate);
                coreEngine.SaveItemLogMessage(itemCode, "________________________________");

                double profitRate = GetProfitRate(double.Parse(price), double.Parse(buyingPrice));
                if(int.Parse(balanceQnt)>0)
                {
                    //UpdateTradingStrategyByBalance(itemCode, int.Parse(balanceQnt), int.Parse(buyingPrice));
                    UpdateBuyAutoTradingDataGridState(itemCode);
                }
               
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
                    Hashtable uiTable = new Hashtable() { { "잔고_계좌번호", account }, { "잔고_종목코드", itemCode }, { "잔고_종목명", itemName }, { "잔고_보유수량", balanceQnt }, { "잔고_주문가능수량", orderAvailableQnt }, { "잔고_매입단가", buyingPrice }, { "잔고_총매입가", totalBuyingPrice }, { "잔고_손익률", profitRate }, { "잔고_매매구분", tradingType }, { "잔고_현재가", price } };
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
                    Hashtable uiTable = new Hashtable { { "계좌잔고_종목코드", itemCode }, { "계좌잔고_종목명", itemName }, { "계좌잔고_보유수량", balanceQnt }, { "계좌잔고_평균단가", buyingPrice }, { "계좌잔고_손익률", profitRate }, { "계좌잔고_현재가", price }, { "계좌잔고_매입금액", totalBuyingPrice }, { "계좌잔고_평가금액", evaluationAmount }, { "계좌잔고_손익금액", profitAmount } };
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

                            SellAllClear(itemCode, balanceCnt, ReceiveSellAllClear, e.RowIndex);
                        }
                    }
                }
            }
        }
        public delegate void SendFinish(string itemCode, int balanceCnt, int rowIndex);
        public void SellAllClear(string itemCode, int balanceCnt, SendFinish delFunc, int rowIndex = -1)
        {
            //Task requestCancelTask = new Task(() =>
            //{
            int orderResult = axKHOpenAPI1.SendOrder(
                "청산매도주문",
                GetScreenNum().ToString(),
                currentAccount,
                CONST_NUMBER.SEND_ORDER_SELL,
                itemCode.Replace("A", ""),
                balanceCnt,
                0,
                ConstName.ORDER_SIJANGGA,
                ""); //2:신규매도

            if (orderResult == 0)
            {
                delFunc(itemCode, balanceCnt, rowIndex);
            }
            //});
            //coreEngine.requestTrDataManager.RequestTrData(requestCancelTask);
        }
        public void ReceiveSellAllClear(string itemCode, int balanceCnt, int rowIndex)
        {
            coreEngine.SendLogMessage("접수 성공");
            if (rowIndex > -1 && rowIndex < accountBalanceDataGrid.RowCount)
                accountBalanceDataGrid["계좌잔고_청산", rowIndex].Value = "청산주문접수";

            SettlementItem settlementItem = new SettlementItem(currentAccount, itemCode, balanceCnt);

            tryingSettlementItemList.Add(settlementItem);
            settleItemList.Add(settlementItem);
        }
        private void AutoTradingDataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            coreEngine.SendLogMessage("e.ColumnIndex : " + e.ColumnIndex + " e.RowIndex : " + e.RowIndex);
            if (e.RowIndex < 0)
                return;
            if(autoTradingDataGrid.Columns["매매진행_종목코드"].Index == e.ColumnIndex)
            {
                string itemCode = autoTradingDataGrid["매매진행_종목코드", e.RowIndex].Value.ToString().Replace("A", "");
                Form3 chartForm = new Form3(axKHOpenAPI1);
               
                chartForm.RequestItem(itemCode, delegate(string _itemCode)
                {
                    chartForm.Show();
                });
            }
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

                        if (curState.Equals(ConstName.AUTO_TRADING_STATE_SEARCH_AND_CATCH))
                        {
                            TrailingItem item = trailingList.Find(o => (o.ui_rowAutoTradingItem == rowItem));

                            if (item != null)
                            {
                                trailingList.Remove(item);
                              
                                axKHOpenAPI1.SetRealRemove("9001", item.itemCode); //실시간 정보받기 해제     
                                autoTradingDataGrid["매매진행_취소", e.RowIndex].Value = "취소접수시도";
                                return;
                            }
                        }

                        List<TradingItem> tradeItemListAll = GetAllTradingItemData(itemCode);

                        foreach (TradingItem tradeItem in tradeItemListAll)
                        {
                            if (tradeItem.ui_rowItem == rowItem)
                            {
                                if (curState.Equals(ConstName.AUTO_TRADING_STATE_BUY_NOT_COMPLETE) || curState.Equals(ConstName.AUTO_TRADING_STATE_BUY_NOT_COMPLETE_OUTCOUNT))
                                {
                                    //취소주문
                                    int orderResult = axKHOpenAPI1.SendOrder("종목주문정정", GetScreenNum().ToString(), currentAccount, CONST_NUMBER.SEND_ORDER_CANCEL_BUY, itemCode, tradeItem.outStandingQnt, (int)tradeItem.buyingPrice, tradeItem.buyOrderType, tradeItem.buyOrderNum);

                                    if (orderResult == 0)
                                    {
                                        AddOrderList(tradeItem);
                                        coreEngine.SendLogMessage("취소 접수 성공");
                                        autoTradingDataGrid["매매진행_취소", e.RowIndex].Value = "취소접수시도";
                                        return;
                                    }
                                }

                                if (curState.Equals(ConstName.AUTO_TRADING_STATE_SELL_NOT_COMPLETE) || curState.Equals(ConstName.AUTO_TRADING_STATE_SELL_NOT_COMPLETE_OUTCOUNT))
                                {
                                    //취소주문
                                    int orderResult = axKHOpenAPI1.SendOrder("종목주문정정", GetScreenNum().ToString(), currentAccount, CONST_NUMBER.SEND_ORDER_CANCEL_SELL, itemCode, tradeItem.outStandingQnt, (int)tradeItem.sellPrice, tradeItem.sellOrderType, tradeItem.sellOrderNum);

                                    if (orderResult == 0)
                                    {
                                        AddOrderList(tradeItem);
                                        coreEngine.SendLogMessage("취소 접수 성공");
                                        autoTradingDataGrid["매매진행_취소", e.RowIndex].Value = "취소접수시도";
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void CancelBuyOrder(string itemCode, string buyOrderNum)
        {
            List<TradingItem> tradeItemListAll = GetAllTradingItemData(itemCode);

            foreach (TradingItem tradeItem in tradeItemListAll)
            {
                if (tradeItem.buyOrderNum == buyOrderNum)
                {
                    //취소주문
                    //Task requestCancelTask = new Task(() =>
                    //{
                    int orderResult = axKHOpenAPI1.SendOrder(
                    "종목주문정정",
                    GetScreenNum().ToString(),
                    currentAccount,
                    CONST_NUMBER.SEND_ORDER_CANCEL_BUY,
                    itemCode,
                    tradeItem.outStandingQnt,
                    (int)tradeItem.buyingPrice,
                    tradeItem.buyOrderType,
                    tradeItem.buyOrderNum
                    );

                    if (orderResult == 0)
                    {
                        AddOrderList(tradeItem);
                        coreEngine.SendLogMessage("취소 접수 성공");
                        autoTradingDataGrid["매매진행_진행상황", tradeItem.GetUiConnectRow().Index].Value = ConstName.AUTO_TRADING_STATE_CANCEL_ORDER;
                    }
                    //});
                    //coreEngine.requestTrDataManager.RequestTrData(requestCancelTask);
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

                        foreach (var trailingItem in trailingList.Reverse<TrailingItem>())
                        {
                            if (trailingItem.strategy != null && trailingItem.strategy == ts)
                            {
                                axKHOpenAPI1.SetRealRemove("9001", trailingItem.itemCode); //실시간 정보받기 해제    
                                autoTradingDataGrid["매매진행_취소", trailingItem.ui_rowAutoTradingItem.Index].Value = "취소접수시도";
                                trailingList.Remove(trailingItem);
                            }
                        }
                        
                        tradingStrategyList.Remove(ts);
                        tsDataGridView.Rows.RemoveAt(e.RowIndex);
                        string removeKey = string.Empty;
                        foreach(var item in doubleCheckHashTable.Values)
                        {
                            if(((TradingStrategy)item) == ts)
                            {
                                removeKey = ts.doubleCheckCondition.Name;
                            }
                        }
                        if (!string.IsNullOrEmpty(removeKey))
                            doubleCheckHashTable.Remove(removeKey);
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
                account = accountComboBox.SelectedItem.ToString();
                if (!account.Equals(currentAccount))
                {
                    axKHOpenAPI1.SetInputValue("계좌번호", account);
                    axKHOpenAPI1.SetInputValue("비밀번호", "");
                    axKHOpenAPI1.SetInputValue("상장폐지조회구분", "0");
                    axKHOpenAPI1.SetInputValue("비밀번호입력매체구분", "00");
                    axKHOpenAPI1.CommRqData(ConstName.RECEIVE_TR_DATA_ACCOUNT_INFO, "OPW00004", 0, GetScreenNum().ToString());

                    axKHOpenAPI1.SetInputValue("계좌번호", account);
                    axKHOpenAPI1.SetInputValue("체결구분", "1");
                    axKHOpenAPI1.SetInputValue("매매구분", "2");
                    axKHOpenAPI1.CommRqData("실시간미체결요청", "opt10075", 0, "5700");
                }

            }
        }

        private void BalanceSell(string accountNum, string itemCode, int buyingPrice, int curQnt, int sellQnt, string takeProfitOrderType, string stopLossOrderType,  double takeProfitRate, double stopLossRate)
        {
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

                        BalanceSellStrategy bs = new BalanceSellStrategy(
                            accountNum,
                            itemCode,
                            buyingPrice,
                            curQnt,
                            sellQnt,
                            takeProfitOrderType,
                            stopLossOrderType,
                            (takeProfitRate > 0)?true:false,
                            takeProfitRate,
                            (stopLossRate < 0)?true:false,
                            stopLossRate
                        );

                        balanceSellStrategyList.Add(bs);

                        int rowIndex = autoTradingDataGrid.Rows.Add();

                        autoTradingDataGrid["매매진행_진행상황", rowIndex].Value = ConstName.AUTO_TRADING_STATE_SELL_MONITORING;
                        autoTradingDataGrid["매매진행_종목코드", rowIndex].Value = itemCode;
                        autoTradingDataGrid["매매진행_종목명", rowIndex].Value = axKHOpenAPI1.GetMasterCodeName(itemCode);
                     
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
        private void BalanceSellBtn_Click(object sender, EventArgs e)
        {
            string itemCode = balanceItemCodeTxt.Text;
            string itemName = balanceNameTextBox.Text;
            long curQnt = long.Parse(bss_curQnt.Text);
            long sellQnt = (long)balanceQntUpdown.Value;

            string accountNum = accountComboBox.Text;
            int buyingPrice = (int)double.Parse(b_averagePriceTxt.Text);

            string orderType = (bssJijungRadio.Checked) ? ConstName.ORDER_JIJUNGGA : ConstName.ORDER_SIJANGGA;
            
            bool usingProfitCheckBox = b_ProfitSellCheckBox.Checked; //익절사용
            double takeProfitRate = 0;
            
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

            BalanceSell(accountNum, itemCode, buyingPrice, (int)curQnt, (int)sellQnt, orderType, orderType, takeProfitRate, stopLossRate);
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
                if (doubleCheckHashTable.ContainsKey(conditionName))
                {
                    MessageBox.Show("현재 이중체크에 등록되있는 전략 검색식입니다 연관전략 : " + ((TradingStrategy)doubleCheckHashTable[conditionName]).buyCondition.Name);
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

            string buyOrderOpt = "00";
            long totalInvestment = 0;
            int itemCount = 0;

            if (marketPriceRadioBtn.Checked)
            {
                buyOrderOpt = ConstName.ORDER_SIJANGGA;
            }
            else
            {
                buyOrderOpt = ConstName.ORDER_JIJUNGGA;
            }
            string sellStopLossOrderOpt = "00";
            if (stopLossSijangRadio.Checked)
            {
                sellStopLossOrderOpt = ConstName.ORDER_SIJANGGA;
            }
            else
            {
                sellStopLossOrderOpt = ConstName.ORDER_JIJUNGGA;
            }

            string sellProfitOrderOpt = "00";
            if (sellProfitSijangRadio.Checked)
            {
                sellProfitOrderOpt = ConstName.ORDER_SIJANGGA;
            }
            else
            {
                sellProfitOrderOpt = ConstName.ORDER_JIJUNGGA;
            }

            if (allCostUpDown.Value == 0)
            {
                MessageBox.Show("총 투자금액을 설정해주세요");
                return;
            }

            totalInvestment = (long)allCostUpDown.Value;
            itemCount = (int)itemCountUpdown.Value;

            List<TradingStrategyADDItem> tradingStrategyItemList = new List<TradingStrategyADDItem>();
         

            bool usingBuyRestart = loopBuyCheck.Checked;


            TradingStrategy ts = new TradingStrategy(
                account,
                findCondition,
                buyOrderOpt,
                totalInvestment,
                itemCount,
                sellProfitOrderOpt,
                sellStopLossOrderOpt,
                false,
                usingBuyRestart
                );

            //추가전략 적용
            bool usingTimeCheck = TimeUseCheck.Checked; //시간 제한 사용

            if (usingTimeCheck)
            {
                DateTime startDate =
                    new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, startTimePicker.Value.Hour, startTimePicker.Value.Minute,0) ;
                DateTime endDate =
                    new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, endTimePicker.Value.Hour, endTimePicker.Value.Minute, 0);
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
                ts.trailMinusValue = (float)tickMinusValue.Value * 0.01f;
            }

            bool usingDoubleCheck = usingDoubleConditionCheck.Checked; //트레일링 매수 적용

            if (usingDoubleCheck)
            {
                string selectTsItem = BuyConditionDoubleComboBox.SelectedItem.ToString();
                if (string.IsNullOrEmpty(selectTsItem))
                {
                    MessageBox.Show("동시 사용할 전략을 선택해주세요");
                    return;
                }
                TradingStrategy inStrategyCondition = tradingStrategyList.Find(o => o.buyCondition.Name.Equals(selectTsItem));
                if (findCondition != null && inStrategyCondition != null)
                {
                    MessageBox.Show("현재 등록되있는 전략 검색식입니다");
                    return;
                }
                ts.usingDoubleCheck = true;
                Condition condition = listCondition.Find(o => o.Name.Equals(selectTsItem));
                ts.doubleCheckCondition = condition;
            }

            bool usingAutoPercentage = orderPecentageCheckBox.Checked; //틱 가격 적용

            if (usingAutoPercentage)
            {
                float tickValue = (float)orderPercentageUpdown.Value;

                ts.usingPercentageBuy = true;
                ts.usingTrailing = true; //트레일링 기능사용
                ts.percentageBuyValue = tickValue;
            }

            if(useVwmaCheckBox.Checked)
            {
                ts.usingVwma = useVwmaCheckBox.Checked;
                ts.usingTrailing = true;
                ts.trailTickValue = 30;
            }

            bool useGapTrailBuy = useGapTrailBuyCheck.Checked;
            if (useGapTrailBuy)
            {
                float costValue = (float)gapTrailCostUpdown.Value;
                //float buyPercentValue = (float)getTrailBuyUpdown.Value;
                int buyTime = (int)gapTrailTimeUpdown.Value;

                ts.usingGapTrailBuy = true; //트레일링 기능사용
                ts.gapTrailCostPercentageValue = costValue * 0.01f + 1.0f;
                //ts.gapTrailBuyPercentageValue  = buyPercentValue * 0.01f;
                ts.gapTrailBuyTimeValue = buyTime;

                TradingStrategyItemWithUpDownPercentValue trailGapBuy =
                    new TradingStrategyItemWithUpDownPercentValue(
                            StrategyItemName.BUY_GAP_CHECK,
                            CHECK_TIMING.BUY_TIME,
                            string.Empty,
                            ts.gapTrailCostPercentageValue);

                trailGapBuy.OnReceivedTrData += this.OnReceiveTrDataCheckGapTrailBuy;
                ts.AddTradingStrategyItemList(trailGapBuy);
            }

            bool usingProfitCheckBox = profitSellCheckBox.Checked; //익절사용
            bool usingTrailingStopSell = TrailingSellCheckBox.Checked;

            if (usingProfitCheckBox && !usingTrailingStopSell)
            {
                double takeProfitRate = 0;
                TradingStrategyItemWithUpDownValue takeProfitStrategy = null;
                takeProfitRate = (double)profitSellUpdown.Value;
                takeProfitStrategy =
                     new TradingStrategyItemWithUpDownValue(
                             StrategyItemName.TAKE_PROFIT_SELL,
                             CHECK_TIMING.SELL_TIME,
                             IS_TRUE_OR_FALE_TYPE.UPPER_OR_SAME,
                             takeProfitRate);
                takeProfitStrategy.OnReceivedTrData += this.OnReceiveTrDataCheckProfitSell;
                ts.AddTradingStrategyItemList(takeProfitStrategy);
                ts.takeProfitRate = takeProfitRate;
            }

            if (usingTrailingStopSell && usingProfitCheckBox)
            {
                double takeProfitRate = 0;
                TradingStrategyItemWithTrailingStopValue takeProfitStrategy = null;
                takeProfitRate = (double)profitSellUpdown.Value;
                takeProfitStrategy =
                     new TradingStrategyItemWithTrailingStopValue(
                             StrategyItemName.TAKE_PROFIT_TRAILING_SELL,
                             CHECK_TIMING.SELL_TIME,
                             IS_TRUE_OR_FALE_TYPE.UPPER_OR_SAME,
                             takeProfitRate);
                takeProfitStrategy.OnReceivedTrData += this.OnReceiveTrDataCheckProfitSell;
                ts.AddTradingStrategyItemList(takeProfitStrategy);
                ts.takeProfitRate = takeProfitRate;
                //ts.sellProfitOrderOption = ConstName.ORDER_SIJANGGA;
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
                            IS_TRUE_OR_FALE_TYPE.DOWN,
                            stopLossRate);

                stopLossStrategy.OnReceivedTrData += this.OnReceiveTrDataCheckStopLoss;
                ts.AddTradingStrategyItemList(stopLossStrategy);
                ts.stoplossRate = stopLossRate;
            }

            bool usingBuyMore = BuyMoreCheckBox1.Checked; //물타기

            if (usingBuyMore)
            {
                ts.usingBuyMore = true;
                ts.buyMoreRate = (double)BuyMorePercentUpdown.Value;
                ts.buyMoreMoney = (int)BuyMoreValueUpdown.Value;
                TradingStrategyItemBuyingDivide buyMoreStrategy =
                    new TradingStrategyItemBuyingDivide(
                            StrategyItemName.BUY_MORE,
                            CHECK_TIMING.SELL_TIME,
                            IS_TRUE_OR_FALE_TYPE.DOWN,
                             ts.buyMoreRate,
                             ts.buyMoreMoney
                             );

                buyMoreStrategy.OnReceivedTrData += this.OnReceiveTrDataBuyMore;
                ts.AddTradingStrategyItemList(buyMoreStrategy);
             
            }

            bool usingBuyCancleByTime = buyCancelTimeCheckBox.Checked; //물타기

            if (usingBuyCancleByTime)
            {
                ts.usingBuyCancelByTime = true;

                TradingStrategyItemCancelByTime buyCancelStrategy =
                    new TradingStrategyItemCancelByTime(
                            StrategyItemName.BUY_CANCEL_BY_TIME,
                            CHECK_TIMING.BUY_ORDER_BEFORE_CONCLUSION,
                            DateTime.Now.Ticks
                             );

                buyCancelStrategy.OnReceivedTrData += this.OnReceiveTrDataBuyCancelByTime;
                ts.AddTradingStrategyItemList(buyCancelStrategy);

            }

            tradingStrategyList.Add(ts);
            AddStrategyToDataGridView(ts);

            StartMonitoring(ts.buyCondition);

            if(usingDoubleCheck)
            {
                StartMonitoring(ts.doubleCheckCondition);
                doubleCheckHashTable.Add(ts.doubleCheckCondition.Name ,ts);
            }

            SaveSetting(conditionName);
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
                    bss_curQnt.Text = balanceQnt.ToString();
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
            Form2 printForm2 = new Form2(axKHOpenAPI1);
            printForm2.Show();
        }

        private void OpenThirdWindow()
        {
            printForm.Show();
        }
        #endregion
        public void StartMonitoring(Condition _condition)
        {
            Task requestItemInfoTask = new Task(() =>
            {
                _condition.ScreenNum = GetScreenNum().ToString();
                int result = axKHOpenAPI1.SendCondition(_condition.ScreenNum, _condition.Name, _condition.Index, 1);
                if (result == 1)
                {

                    coreEngine.SendLogMessage("감시요청 성공");
                }
                else
                {
                    coreEngine.SendLogMessage("감시요청 실패");
                }
            });
            Core.CoreEngine.GetInstance().requestTrDataManager.RequestTrData(requestItemInfoTask);

          
        }
        public void StopMonitoring(Condition _condition)
        {
            axKHOpenAPI1.SendConditionStop(_condition.ScreenNum, _condition.Name, _condition.Index);
           
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

        public void OnReceiveTrDataCheckProfitSell(object sender, OnReceivedTrEventArgs e)
        {
            OnReceiveTrDataCheckProfitSell(e.tradingItem, e.checkNum);
        }

        public void OnReceiveTrDataCheckProfitSell(TradingItem item, double checkValue)
        {

            if (item.state == TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_NOT_COMPLETE
                || item.state == TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_NOT_COMPLETE_OUTCOUNT)
            {
                if (item.IsProfitSell())
                {
                    coreEngine.SaveItemLogMessage(item.itemCode, item.itemName + " 이전 익절 주문 실행 내용 확인");
                    //같은 익절 상태면 아무것도 안함
                    return;
                }
                else
                {
                    coreEngine.SaveItemLogMessage(item.itemCode, "손절주문 취소 : " + item.itemName + " 수량 " + item.curQnt);
                    int orderResultCancel = axKHOpenAPI1.SendOrder("종목주문정정", GetScreenNum().ToString(), currentAccount, CONST_NUMBER.SEND_ORDER_CANCEL_SELL, item.itemCode, item.curQnt, (int)item.sellPrice, item.sellOrderType, item.sellOrderNum);

                    if (orderResultCancel == 0)
                    {
                        AddOrderList(item);
                        item.SetSellCancelOrder();
                        coreEngine.SaveItemLogMessage(item.itemCode, "취소 접수 성공");
                        autoTradingDataGrid["매매진행_진행상황", item.GetUiConnectRow().Index].Value = ConstName.AUTO_TRADING_STATE_STOPLOSS_CANCEL;
                        return;
                    }
                }
            }
            coreEngine.SaveItemLogMessage(item.itemCode, "익절 체크 " + item.itemName + " / " +  item.state);
            
            if (item.state != TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_COMPLETE)
                return;

            item.SetSellOrderType(true);

            coreEngine.SaveItemLogMessage(item.itemCode,
               item.itemName + "order 종목익절매도 " + 
                " 수량: " + item.curQnt+
                " 주문가: "+item.curPrice+
                " 주문구분: " +item.sellOrderType
            );

            int orderResult = axKHOpenAPI1.SendOrder(
                "종목익절매도",
                GetScreenNum().ToString(),
                item.ts.account,
                CONST_NUMBER.SEND_ORDER_SELL,
                item.itemCode,
                item.curQnt,
                item.sellOrderType == ConstName.ORDER_SIJANGGA ? 0 : (int)item.curPrice,
                item.sellOrderType,//지정가
                "" //원주문번호없음
             );
            if (orderResult == 0) //요청 성공시 (실거래는 안될 수 있음)
            {
                AddOrderList(item);
                item.SetSold( true);
                coreEngine.SaveItemLogMessage(item.itemCode, "ui -> 매도주문접수시도");
                UpdateAutoTradingDataGridRow(item.itemCode, item, item.curPrice, ConstName.AUTO_TRADING_STATE_SELL_BEFORE_ORDER);
                UpdateAutoTradingDataGridRowWinLose(item.itemCode, item, "win");
            }
            else
            {
                coreEngine.SaveItemLogMessage(item.itemCode, "자동 익절 요청 실패");
            }
           
        }

        private void API_OnReceiveTrDataHoga(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            //coreEngine.SendLogMessage(e.sRQName);

            if (e.sRQName.Contains(ConstName.RECEIVE_TR_DATA_HOGA))
            {
                coreEngine.axKHOpenAPI_OnReceiveTrData(sender, e); //호가부분 데이터 입력

                //검색 ->검색완료 ->  현재호가를 얻어오기위해 tr요청-> 이때 "RECEIVE_TR_DATA_HOGA:검색넘버:아이템코드" 로 요청

                string[] rqNameArray = e.sRQName.Split(':');
                if (rqNameArray.Length == 3)
                {
                    string conditionUid = (rqNameArray[2]);
                    TradingStrategy ts = tradingStrategyList.Find(o => o.buyCondition.Uid == conditionUid);

                    if (ts != null)
                    {
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

              
                            if (ts.usingPercentageBuy)
                            {
                                coreEngine.SendLogWarningMessage(axKHOpenAPI1.GetMasterCodeName(itemcode) + " 체크 호가 " + ts.percentageBuyValue + " : " + trailItem.percentageCheckPrice);
                            }

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
                                        ConstName.SEND_ORDER_BUY,
                                        GetScreenNum().ToString(),
                                        ts.account,
                                        CONST_NUMBER.SEND_ORDER_BUY,//1:신규매수
                                        itemcode,
                                        (int)(ts.itemInvestment / price),
                                        (ts.buyOrderOption == ConstName.ORDER_JIJUNGGA) ? price : 0,
                                        ts.buyOrderOption,
                                        "" //원주문번호없음
                                    );

                                    if (orderResult == 0)
                                    {
                                        coreEngine.SendLogMessage(axKHOpenAPI1.GetMasterCodeName(itemcode) + " 매수주문요청 성공");

                                        TradingItem tradingItem = new TradingItem(ts, itemcode, axKHOpenAPI1.GetMasterCodeName(itemcode), price, i_qnt, false, false, ts.buyOrderOption);
                                        tradingItem.SetBuy(true);
                                        tradingItem.SetConditonUid(conditionUid);

                                        ts.tradingItemList.Add(tradingItem);
                                        AddOrderList(tradingItem);

                                        string fidList = "9001;302;10;11;25;12;13"; //9001:종목코드,302:종목명
                                        axKHOpenAPI1.SetRealReg("9001", itemcode, fidList, "1");
                                        coreEngine.SaveItemLogMessage(itemcode, " realreg");
                                        //매매진행 데이터 그리드뷰 표시

                                        int addRow = autoTradingDataGrid.Rows.Add();
                                        tradingItem.SetUiConnectRow(autoTradingDataGrid.Rows[addRow]);
                                        UpdateAutoTradingDataGridRowAll(addRow, ConstName.AUTO_TRADING_STATE_BUY_BEFORE_ORDER, itemcode, ts.buyCondition.Name, i_qnt, price);
                                        ts.StrategyBuyOrderUpdate(itemcode, price, i_qnt, TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_BEFORE_ORDER);
                                        coreEngine.SaveItemLogMessage(itemcode, "자동 매수 요청 - " + "종목코드 : " + itemcode + " 주문가 : " + price + " 주문수량 : " + i_qnt + " 매수조건식 : " + ts.buyCondition.Name);
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

        public void SetTrailingItem(TrailingItem trailItem)
        {
            if (autoTradingDataGrid.InvokeRequired)
            {
                autoTradingDataGrid.Invoke(new MethodInvoker(delegate ()
                {
                    int rowIndex = autoTradingDataGrid.Rows.Add(); //종목포착정보 ui추가
                    trailItem.ui_rowAutoTradingItem = autoTradingDataGrid.Rows[rowIndex];

                    int i_qnt = (int)(trailItem.itemInvestment / trailItem.firstPrice);

                    UpdateAutoTradingDataGridRowAll(rowIndex, ConstName.AUTO_TRADING_STATE_SEARCH_AND_CATCH, trailItem.itemCode, trailItem.strategy.buyCondition.Name, i_qnt, trailItem.firstPrice);

                    trailingList.Add(trailItem);
                }
                ));
            }
            else
            {
                int rowIndex = autoTradingDataGrid.Rows.Add(); //종목포착정보 ui추가
                trailItem.ui_rowAutoTradingItem = autoTradingDataGrid.Rows[rowIndex];

                int i_qnt = (int)(trailItem.itemInvestment / trailItem.firstPrice);

                UpdateAutoTradingDataGridRowAll(rowIndex, ConstName.AUTO_TRADING_STATE_SEARCH_AND_CATCH, trailItem.itemCode, trailItem.strategy.buyCondition.Name, i_qnt, trailItem.firstPrice);

                trailingList.Add(trailItem);
            }
                 
        }

        public void SetTradingItemUI(TradingItem tradingItem)
        {
            int addRow = autoTradingDataGrid.Rows.Add();
            tradingItem.SetUiConnectRow(autoTradingDataGrid.Rows[addRow]);
            UpdateAutoTradingDataGridRowAll(addRow,TradingItem.StateToString(tradingItem.state), tradingItem.itemCode, tradingItem.ts.buyCondition.Name, tradingItem.buyingQnt, (int)tradingItem.buyingPrice);
          }

        public void API_OnReceiveMsg(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveMsgEvent e)
        {
            coreEngine.SendLogMessage("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            coreEngine.SendLogMessage("ScreenNum : " + e.sScrNo + ",사용자구분명 : " + e.sRQName + ", Tr이름: " + e.sTrCode + ", MSG : " + e.sMsg);
            coreEngine.SendLogMessage("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
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
                        //호가정보 아이템
                        if (stockInfo != null)
                        {
                            string itemcode = itemCode;
                            int price = (int)stockInfo.GetBuyHoga(0);
                            int i_qnt = 0;
                            bool CostCheck = true;
                            //틱단위 스킵
                            //coreEngine.SendLogMessage("틱제한 " + axKHOpenAPI1.GetMasterCodeName(itemcode) + " :  현재 " + trailingItem.curTickCount.ToString() + " / 셋팅 " + trailingItem.settingTickCount.ToString());

                            if (trailingItem.isGapTrailBuy)
                            {
                                float trailBuyCheckValue = (float)price / trailingItem.gapTrailBuyCheckPrice;

                                if (price > 0
                                    && trailBuyCheckValue > trailingItem.strategy.gapTrailCostPercentageValue
                                    && trailingItem.gapTrailBuyCheckTimeSecond > (DateTime.Now - trailingItem.gapTrailBuyCheckDateTime).TotalSeconds
                                    )
                                {
                                    coreEngine.SaveItemLogMessage(itemcode," 갭추격매수 조건 확인 /// 현재호가 : " + price + " / 체크 호가 :" + trailingItem.gapTrailBuyCheckPrice + " / 현재 퍼센티지 : " + trailBuyCheckValue + " / 체크 조건 : " + (trailingItem.strategy.gapTrailCostPercentageValue));
                                    //강제 통과 위한 설정
                                    trailingItem.settingTickCount = 0;
                                    trailingItem.itemInvestment = (long)((float)trailingItem.itemInvestment * trailingItem.strategy.gapTrailBuyPercentageValue);
                                    trailingItem.buyOrderOption = ConstName.ORDER_SIJANGGA;
                                    trailingItem.isGapTrailBuy = false;
                                    trailingItem.isPercentageCheckBuy = false;
                                    CostCheck = false;
                                }
                            }

                            if (trailingItem.isPercentageCheckBuy)
                            {
                                if (price <= 0)
                                    continue;
                                if (trailingItem.percentageCheckPrice < price)
                                {
                                    continue;
                                }
                                else
                                {
                                    coreEngine.SaveItemLogMessage(itemcode, " 호가 % 통과 현재호가" + price + " / 체크 호가 :" + trailingItem.percentageCheckPrice);
                                    trailingItem.isPercentageCheckBuy = false;
                                }
                            }

                            if (trailingItem.curTickCount < trailingItem.settingTickCount)
                            {
                                if (trailingItem.curTickCount == 0)
                                {
                                    
                                    if (trailingItem.isVwmaCheck)
                                    {

                                        printForm.RequestItem(itemCode, delegate (string _itemCode) {

                                            coreEngine.SaveItemLogMessage(_itemCode, printForm.vwma_state.ToString());

                                            if (printForm.vwma_state == Form3.VWMA_CHART_STATE.GOLDEN_CROSS || printForm.vwma_state == Form3.VWMA_CHART_STATE.UP_STAY)
                                            {
                                                TrailingItem findItem = trailingList.Find(o => (o.itemCode == _itemCode));
                                                if (findItem != null)
                                                {
                                                    StockWithBiddingEntity _stockInfo = StockWithBiddingManager.GetInstance().GetItem(itemCode);
                                                    TrailingToBuy(findItem, _itemCode, (int)stockInfo.GetBuyHoga(findItem.strategy.tickBuyValue), _stockInfo);
                                                    return;
                                                }
                                            }
                                        });
                                    }
                                   
                                }

                                trailingItem.tickBongInfoMgr.AddPrice(price);
                                trailingItem.curTickCount++;
                     
                            }
                            else
                            {
                                trailingItem.curTickCount = 0;
                            }

                            if(trailingItem.isVwmaCheck)
                            {
                                continue;
                            }


                            if ( CostCheck  && 
                                trailingItem.settingTickCount > 0 &&
                                 price <= trailingItem.firstPrice)
                            {

                                //if (trailingItem.ma_data_info.GetLastItem() != null)
                                //{
                                //    long average = trailingItem.ma_data_info.GetLastItem().average1;
                                //    coreEngine.SendLogMessage(axKHOpenAPI1.GetMasterCodeName(itemcode) + " average1 " + average + " price : " + price);
                                //    if (average > price)
                                //    {
                                //        trailingList.Remove(trailingItem);
                                //        autoTradingDataGrid["매매진행_취소", trailingItem.ui_rowAutoTradingItem.Index].Value = "취소접수시도";
                                //        return;
                                //    }
                                //}

                                //if (trailingItem.curTickCount > 0 && trailingItem.curTickCount == trailingItem.settingTickCount)
                                //{
                                //    price = trailingItem.sumPriceAllTick / trailingItem.curTickCount;
                                //}
                                //else
                                //{
                                //    continue;
                                //}

                                if (trailingItem.tickBongInfoMgr.IsCompleteBong(1) == false)
                                {
                                    coreEngine.SaveItemLogMessage(itemcode, "1봉전 미완료");
                                    continue;
                                }

                                TickBongInfo bong = trailingItem.tickBongInfoMgr.GetTickBong(1); //1봉전 틱봉 !!0봉전 아님!!
                            
                                if ((int)((float)price * (1.0f - trailingItem.strategy.trailMinusValue)) <= bong.GetAverage())
                                {
                                    coreEngine.SaveItemLogMessage(itemcode, "1봉전비교가격 : " + (int)bong.GetAverage() + "/ 현재가(보정적용):" + (int)((float)price * (1.0f - trailingItem.strategy.trailMinusValue)));
                                    continue;
                                }
                                coreEngine.SaveItemLogMessage(itemcode, " 트레일링 통과 / 1봉전비교가격 : " + (int)bong.GetAverage() + "/ 현재가:"+ price);
                            }

                            price = (int)stockInfo.GetBuyHoga(trailingItem.strategy.tickBuyValue); //전략에 의한 매수틱 조정(없으면 최우선매수호가)

                            if (price > 0 && trailingItem.isTrailing)
                            {
                                i_qnt = (int)(trailingItem.strategy.itemInvestment / price);

                                coreEngine.SendLogMessage(axKHOpenAPI1.GetMasterCodeName(itemcode) + " 종목 매수 시도  : " + axKHOpenAPI1.GetMasterCodeName(itemcode));

                                //Task requestBuyTask = new Task(() =>
                                //{
                                int orderResult =

                                axKHOpenAPI1.SendOrder(
                                   ConstName.SEND_ORDER_BUY,
                                    GetScreenNum().ToString(),
                                    trailingItem.strategy.account,
                                    CONST_NUMBER.SEND_ORDER_BUY,//1:신규매수
                                    itemcode,
                                    (int)(trailingItem.itemInvestment / price),
                                    (trailingItem.buyOrderOption == ConstName.ORDER_JIJUNGGA) ? price : 0,
                                     trailingItem.buyOrderOption,
                                    "" //원주문번호없음
                                );

                                if (orderResult == 0)
                                {
                                    trailingItem.isTrailing = false;

                                    coreEngine.SendLogMessage(axKHOpenAPI1.GetMasterCodeName(itemcode) + " 매수주문 성공");

                                    TradingItem tradingItem = new TradingItem(trailingItem.strategy, itemcode, axKHOpenAPI1.GetMasterCodeName(itemcode), price, i_qnt, false, false, trailingItem.buyOrderOption);
                                    tradingItem.SetBuy(true);
                                    tradingItem.SetConditonUid(trailingItem.strategy.buyCondition.Uid);

                                    trailingItem.strategy.tradingItemList.Add(tradingItem); //매수전략 내에 매매진행 종목 추가

                                    AddOrderList(tradingItem);

                                    string fidList = "9001;302;10;11;25;12;13"; //9001:종목코드,302:종목명
                                    axKHOpenAPI1.SetRealReg("9001", itemcode, fidList, "1");
                                    coreEngine.SendLogMessage(axKHOpenAPI1.GetMasterCodeName(itemcode) + " realreg");
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

                                    coreEngine.SaveItemLogMessage(itemcode,"매수 요청 - " + "종목코드 : " + itemcode + " 주문가 : " + price + " 주문수량 : " + i_qnt + " 매수조건식 : " + trailingItem.strategy.buyCondition.Name);

                                    tradingItem.ts.StrategyBuyOrderUpdate(itemcode, price, i_qnt, TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_BEFORE_ORDER);
                                    trailingList.Remove(trailingItem);
                             
                                }
                                else
                                {
                                    coreEngine.SaveItemLogMessage(itemcode, "구매가 입력 실패");
                                }

                                //});
                                //coreEngine.requestTrDataManager.RequestTrData(requestBuyTask);
                            }
                            else
                            {
                                coreEngine.SaveItemLogMessage(itemcode, "!!!!!! 호가 정보 받기 -> 매수 실패 price :" + price + " !!!!!!!!!!");
                            }
                        }
                    }
                }
            }
        }
        public void OnReceiveTrDataBuyMore(object sender, OnReceivedTrBuyMoreEventArgs e)
        {
            OnReceiveTrDataBuyMore(e.strategyItem, e.tradingItem, e.checkNum);
        }
        public void OnReceiveTrDataBuyMore(TradingStrategyItemBuyingDivide tsItem, TradingItem item, double checkValue)
        {
            if (item.state != TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_COMPLETE)
                return;

            coreEngine.SaveItemLogMessage(item.itemCode,
                item.itemName + "order 추가매수 "
            );

            int buyQnt = (int)(tsItem.BuyMoney / item.curPrice);
            int orderResult = axKHOpenAPI1.SendOrder(
                 ConstName.SEND_ORDER_BUY,
                GetScreenNum().ToString(),
                item.ts.account,
                CONST_NUMBER.SEND_ORDER_BUY,
                item.itemCode,
                buyQnt,
                item.buyOrderType == ConstName.ORDER_SIJANGGA ? 0 : (int)item.curPrice,
                item.buyOrderType,
                "" //원주문번호없음
            );

            if (orderResult == 0) //요청 성공시 (실거래는 안될 수 있음)
            {
                coreEngine.SaveItemLogMessage(item.itemCode, "추가물타기주문접수시도");
            }
            else
            {
                coreEngine.SaveItemLogMessage(item.itemCode, "추가물타기 매수 요청 실패");
            }
        }

        public void OnReceiveTrDataBuyCancelByTime(object sender, OnReceivedTrBuyCancel e)
        {
            if (e.tradingItem.state.Equals(ConstName.AUTO_TRADING_STATE_BUY_NOT_COMPLETE))
            {
                coreEngine.SaveItemLogMessage(e.tradingItem.itemCode, "시간 초과 매수 취소");
                //취소주문
                CancelBuyOrder(e.tradingItem.itemCode, e.tradingItem.buyOrderNum);
            }
        }

        public void OnReceiveTrDataCheckStopLoss(object sender, OnReceivedTrEventArgs e)
        {
            OnReceiveTrDataCheckStopLoss(e.tradingItem, e.checkNum);
        }

        public void OnReceiveTrDataCheckStopLoss(TradingItem item, double checkValue)
        {
            if (item.state == TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_NOT_COMPLETE
                || item.state == TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_NOT_COMPLETE_OUTCOUNT)
            {
                if (item.IsProfitSell())
                {
                    //취소주문(익절주문취소)
                    coreEngine.SaveItemLogMessage(item.itemCode, "익절주문취소 : " + item.itemName + " 수량 " + item.curQnt);
                    int orderResultCancel = axKHOpenAPI1.SendOrder("종목주문정정", GetScreenNum().ToString(), currentAccount, CONST_NUMBER.SEND_ORDER_CANCEL_SELL, item.itemCode, item.curQnt, (int)item.sellPrice, item.sellOrderType, item.sellOrderNum);

                    if (orderResultCancel == 0)
                    {
                        AddOrderList(item);
                        item.SetSellCancelOrder();
                        coreEngine.SaveItemLogMessage(item.itemCode, "취소 접수 성공");
                        autoTradingDataGrid["매매진행_진행상황", item.GetUiConnectRow().Index].Value = ConstName.AUTO_TRADING_STATE_TAKE_PROFIT_CANCEL;
                        
                        return;
                    }
                }
                else
                {
                    coreEngine.SaveItemLogMessage(item.itemCode, "기존 손절주문확인 : " + item.itemName + " 수량 " + item.outStandingQnt);
                    //같은 손절 주문이면 아무것도 안함
                    return;
                }
            }

           
            if (item.state != TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_COMPLETE)
                return;

            item.SetSellOrderType(false);

            coreEngine.SaveItemLogMessage(item.itemCode,
               item.itemName + "order 종목손절매도 " +
               " 수량: " + item.curQnt +
               " 주문가: " + item.curPrice +
               " 주문구분: " + item.sellOrderType
            );
            
            int orderResult = axKHOpenAPI1.SendOrder(
                "종목손절매도",
                GetScreenNum().ToString(),
                item.ts.account,
                CONST_NUMBER.SEND_ORDER_SELL,
                item.itemCode,
                item.curQnt,
                item.sellOrderType == ConstName.ORDER_SIJANGGA ? 0 : (int)item.curPrice,
                item.sellOrderType,
                "" //원주문번호없음
            );
            if (orderResult == 0) //요청 성공시 (실거래는 안될 수 있음)
            {
                AddOrderList(item);
                item.SetSold(false);
                coreEngine.SaveItemLogMessage(item.itemCode, "ui -> 매도주문접수시도");
                UpdateAutoTradingDataGridRow(item.itemCode, item, item.curPrice, ConstName.AUTO_TRADING_STATE_SELL_BEFORE_ORDER);
                UpdateAutoTradingDataGridRowWinLose(item.itemCode, item, "lose");
            }
            else
            {
                coreEngine.SaveItemLogMessage(item.itemCode, "자동 손절 요청 실패");
            }
        }

        public void OnReceiveTrDataCheckGapTrailBuy(object sender, OnReceivedTrEventArgs e)
        {
            OnReceiveTrDataCheckGapTrailBuy(e.tradingItem, e.checkNum);
        }

        public void OnReceiveTrDataCheckGapTrailBuy(TradingItem item, double checkValue)
        {
            if (item.state != TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_COMPLETE)
                return;

            if (item.state == TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_NOT_COMPLETE
                || item.state == TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_NOT_COMPLETE_OUTCOUNT)
            {
                //주문 정정을 한다

                //coreEngine.SendLogWarningMessage("주문 수량 정정 : " + item.itemName + " 수량 " + item.outStandingQnt);
                //int orderResultCancel = axKHOpenAPI1.SendOrder("종목주문정정", GetScreenNum().ToString(), currentAccount, CONST_NUMBER.SEND_ORDER_CANCEL_SELL, item.itemCode, item.outStandingQnt, (int)item.sellPrice, ConstName.ORDER_JIJUNGGA, item.sellOrderNum);
                //if (orderResultCancel == 0)
                //{
                //    AddOrderList(item);
                //    coreEngine.SendLogMessage("수량 정정 접수 성공");
                //    autoTradingDataGrid["매매진행_진행상황", item.GetUiConnectRow().Index].Value = ConstName.AUTO_TRADING_STATE_TAKE_PROFIT_CANCEL;
                //    return;
                //}
            }
            else if (item.state == TRADING_ITEM_STATE.AUTO_TRADING_STATE_SEARCH_AND_CATCH)
            {
              //검색 -> 트레일링 에서 해당사항 체크 
            }
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

            if (m_marketPriceRadioBtn.Checked)
            {
                buyOrderOpt = ConstName.ORDER_SIJANGGA;
            }
            else
            {
                buyOrderOpt = ConstName.ORDER_JIJUNGGA;
            }

            if (M_allCostUpDown.Value == 0)
            {
                MessageBox.Show("총 투자금액을 설정해주세요");
                return;
            }

            totalInvestment = (long)M_allCostUpDown.Value;

            List<TradingStrategyADDItem> tradingStrategyItemList = new List<TradingStrategyADDItem>();
            //매매 전략

            bool usingBuyRestart = false;

            TradingStrategy ts = new TradingStrategy(
                account,
                findCondition,
                buyOrderOpt,
                totalInvestment,
                1,                                       //매수할 종목 1개
                ConstName.ORDER_JIJUNGGA,
                ConstName.ORDER_JIJUNGGA,
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

            if (m_useVwmaCheckBox.Checked)
            {
                ts.usingVwma = m_useVwmaCheckBox.Checked;
                ts.usingTrailing = true;
                ts.trailTickValue = 30;
            }

            double takeProfitRate = (double)M_SellUpdown.Value;

            TradingStrategyItemWithUpDownValue takeProfitStrategy =
                 new TradingStrategyItemWithUpDownValue(
                         StrategyItemName.TAKE_PROFIT_SELL,
                         CHECK_TIMING.SELL_TIME,
                         IS_TRUE_OR_FALE_TYPE.UPPER_OR_SAME,
                         takeProfitRate);

            takeProfitStrategy.OnReceivedTrData += this.OnReceiveTrDataCheckProfitSell;

            ts.AddTradingStrategyItemList(takeProfitStrategy);
            ts.takeProfitRate = takeProfitRate;

            double stopLossRate = (double)M_SellUpdown.Value * -1;

            TradingStrategyItemWithUpDownValue stopLossStrategy =
                new TradingStrategyItemWithUpDownValue(
                        StrategyItemName.STOPLOSS_SELL,
                        CHECK_TIMING.SELL_TIME,
                        IS_TRUE_OR_FALE_TYPE.DOWN,
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
            SaveSetting(conditionName);
            coreEngine.SendLogMessage("마틴 게일 전략이 입력됬습니다 \n 매수조건식 : " + ts.buyCondition.Name + "\n" + " 총투자금 : " + ts.totalInvestment + "\n" + " 종목수 : " + ts.buyItemCount);
        }

        private void UpdateTradingStrategyByBalance(string itemCode, int allQnt, int priceUpdate)
        {
            foreach (TradingStrategy ts in tradingStrategyList)
            {
                coreEngine.SendLogWarningMessage("data update" + ts.buyCondition.Name);

                TradingItem tradeItem = ts.tradingItemList.Find(o => o.itemCode.Equals(itemCode));
                if (tradeItem != null 
                    && tradeItem.state >= TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_COMPLETE
                    && tradeItem.state < TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_COMPLETE)
                {
                    coreEngine.SendLogWarningMessage("trading uid : " + tradeItem.Uid);
                    tradeItem.buyingPrice = priceUpdate;
                    tradeItem.curQnt = allQnt;
                }
                else
                {
                    coreEngine.SendLogWarningMessage("종목을 찾을 수 없습니다 : " + itemCode);
                }
            }
        }
        private void UpdateTradingStrategyBuy(string orderNum, bool buyComplete, int addQnt, int priceUpdate)
        {
            foreach (TradingStrategy ts in tradingStrategyList)
            {
                TradingItem tradeItem = ts.tradingItemList.Find(o => o.buyOrderNum.Equals(orderNum));
                if (tradeItem != null)
                {
                    coreEngine.SaveItemLogMessage(tradeItem.itemCode, tradeItem.Uid);
                    coreEngine.SaveItemLogMessage(tradeItem.itemCode, "매수 처리");

                    int curLastQnt = tradeItem.curQnt;
                    tradeItem.curQnt += addQnt;

                    long PriceAverage = (long)((float)((curLastQnt * tradeItem.buyingPrice) + (priceUpdate * addQnt)) / tradeItem.curQnt);
                    
                    tradeItem.buyingPrice = PriceAverage;
                    tradeItem.SetCompleteBuying(buyComplete);

                    coreEngine.SaveItemLogMessage(tradeItem.itemCode, "평단가 : "+ PriceAverage);
                    coreEngine.SaveItemLogMessage(tradeItem.itemCode, "보유량 : " + tradeItem.curQnt);

                    if (buyComplete)
                        tradeItem.ts.StrategyOnReceiveBuyChejanUpdate(tradeItem.itemCode, (int)tradeItem.buyingPrice, tradeItem.buyingQnt, TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_COMPLETE);
                    else
                        tradeItem.ts.StrategyOnReceiveBuyChejanUpdate(tradeItem.itemCode, (int)tradeItem.buyingPrice, tradeItem.buyingQnt, TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_NOT_COMPLETE_OUTCOUNT);

                }
                else
                {
                    coreEngine.SendLogWarningMessage("주문을 찾을 수 없습니다 : " + orderNum);
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
                        coreEngine.SendLogWarningMessage("SetRealRemove : " + tradeItem.itemCode);
                        axKHOpenAPI1.SetRealRemove("9001", tradeItem.itemCode); //실시간 정보받기 해제
                        tradeItem.ts.StrategyOnReceiveSellChejanUpdate(tradeItem.itemCode, (int)tradeItem.sellPrice, tradeItem.sellQnt, TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_COMPLETE);
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
                    tradeItem.SetOutStanding(outStand);
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
                    tradeItem.SetOutStanding(outStand);
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
                        tradeItem.SetBuyCancelComplete();
                        //ts.tradingItemList.Remove(tradeItem);
                        autoTradingDataGrid["매매진행_진행상황", tradeItem.ui_rowItem.Index].Value = ConstName.AUTO_TRADING_STATE_BUY_CANCEL_ALL;
                    }
                }
                else
                {
                    TradingItem tradeItem = ts.tradingItemList.Find(o => o.sellCancelOrderNum.Equals(orderNum));
                    if (tradeItem != null)
                    {
                        tradeItem.SetSellCancelOrderComplete();
                        //ts.tradingItemList.Remove(tradeItem);
                        autoTradingDataGrid["매매진행_진행상황", tradeItem.ui_rowItem.Index].Value = ConstName.AUTO_TRADING_STATE_SELL_CANCEL_ALL;
                    }
                }
            }
        }
        void TrailingToBuy(TrailingItem findItem, string _itemCode, int price, StockWithBiddingEntity stockInfo)
        {
            int i_qnt = (int)(findItem.strategy.itemInvestment / price);
            int orderResult =

            axKHOpenAPI1.SendOrder(
                ConstName.SEND_ORDER_BUY,
                GetScreenNum().ToString(),
                findItem.strategy.account,
                CONST_NUMBER.SEND_ORDER_BUY,//1:신규매수
                _itemCode,
                (int)(findItem.itemInvestment / price),
                (findItem.buyOrderOption == ConstName.ORDER_JIJUNGGA) ? price : 0,
                    findItem.buyOrderOption,
                "" //원주문번호없음
            );

            if (orderResult == 0)
            {
                findItem.isTrailing = false;

                coreEngine.SendLogMessage(axKHOpenAPI1.GetMasterCodeName(_itemCode) + " 매수주문 성공");

                TradingItem tradingItem = new TradingItem(findItem.strategy, _itemCode, axKHOpenAPI1.GetMasterCodeName(_itemCode), price, i_qnt, false, false, findItem.buyOrderOption);
                tradingItem.SetBuy(true);
                tradingItem.SetConditonUid(findItem.strategy.buyCondition.Uid);

                findItem.strategy.tradingItemList.Add(tradingItem); //매수전략 내에 매매진행 종목 추가

                AddOrderList(tradingItem);

                string fidList = "9001;302;10;11;25;12;13"; //9001:종목코드,302:종목명
                axKHOpenAPI1.SetRealReg("9001", _itemCode, fidList, "1");
                coreEngine.SaveItemLogMessage(_itemCode, "realreg");
                //매매진행 데이터 그리드뷰 표시

                int addRow = 0;
                if (findItem.ui_rowAutoTradingItem != null)
                {
                    addRow = findItem.ui_rowAutoTradingItem.Index;
                }
                else
                {
                    addRow = autoTradingDataGrid.Rows.Add();
                }
                tradingItem.SetUiConnectRow(autoTradingDataGrid.Rows[addRow]);

                UpdateAutoTradingDataGridRowAll(addRow, ConstName.AUTO_TRADING_STATE_BUY_BEFORE_ORDER, _itemCode, findItem.strategy.buyCondition.Name, i_qnt, price);

                autoTradingDataGrid["매매진행_현재가", addRow].Value = stockInfo.GetBuyHoga(0);

                coreEngine.SaveItemLogMessage(_itemCode,"매수 요청 - " + "종목코드 : " + _itemCode + " 주문가 : " + price + " 주문수량 : " + i_qnt + " 매수조건식 : " + findItem.strategy.buyCondition.Name);

                tradingItem.ts.StrategyBuyOrderUpdate(_itemCode, price, i_qnt, TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_BEFORE_ORDER);
                trailingList.Remove(findItem);

             
            }
            else
            {
                coreEngine.SendLogMessage("구매가 입력 실패");
            }
        }
        public void AddOrderList(TradingItem item)
        {
            TradingItem itemFind = tryingOrderList.Find(o => (o.Uid == item.Uid));
            if (itemFind == null)
                tryingOrderList.Add(item);
        }
        public void RemoveOrderList(TradingItem item)
        {
            TradingItem itemFind = tryingOrderList.Find(o => (o.Uid == item.Uid));
            if (itemFind != null)
                tryingOrderList.Remove(item);
        }
        private void Form_FormClosing(object sender, EventArgs e)
        {
            axKHOpenAPI1.OnEventConnect -= API_OnEventConnect; //로그인
            axKHOpenAPI1.OnReceiveConditionVer -= API_OnReceiveConditionVer; //검색 받기
            axKHOpenAPI1.OnReceiveRealCondition -= API_OnReceiveRealCondition; //실시간 검색
            axKHOpenAPI1.OnReceiveTrCondition -= API_OnReceiveTrCondition; //검색

            axKHOpenAPI1.OnReceiveTrData -= API_OnReceiveTrData; //정보요청
            axKHOpenAPI1.OnReceiveTrData -= API_OnReceiveTrDataHoga; //정보요청(호가)
            axKHOpenAPI1.OnReceiveChejanData -= API_OnReceiveChejanData; //체결잔고
            axKHOpenAPI1.OnReceiveRealData -= API_OnReceiveRealData; //실시간정보
            axKHOpenAPI1.OnReceiveRealData -= API_OnReceiveRealDataHoga; //실시간정보

            SaveLoadManager.GetInstance().SerializeStrategy(tradingStrategyList);
            SaveLoadManager.GetInstance().SerializeTrailing(trailingList);
        
        }

       
       
        private void BuyConditionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (BuyConditionComboBox.SelectedItem != null)
            {
                string conditionName = BuyConditionComboBox.SelectedItem.ToString();
                Condition condition = listCondition.Find(o => o.Name.Equals(conditionName));
                if (condition != null)
                {
                    LoadSetting(conditionName);
                }
            }
        }

        private void outstandingDataGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;
            if (e.ColumnIndex == outstandingDataGrid.Columns["미체결_취소"].Index)
            {
                string orderNum = (string)outstandingDataGrid["미체결_주문번호", e.RowIndex].Value;
                if(string.IsNullOrEmpty(orderNum) == false && nonConclusionList.ContainsKey(orderNum))
                {
                    NotConclusionItem item = nonConclusionList[orderNum];
                    int orderResult = axKHOpenAPI1.SendOrder("종목주문정정", GetScreenNum().ToString(), currentAccount, 
                        CONST_NUMBER.SEND_ORDER_CANCEL_BUY,
                        item.itemCode, item.outstandingNumber, (int)item.orderPrice, item.orderGubun, item.orderNum);

                    if (orderResult == 0)
                    {
                        outstandingDataGrid["미체결_취소", e.RowIndex].Value = "취소접수시도";
                    }
                }
            }
        }


        private void M_BuyConditionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (M_BuyConditionComboBox.SelectedItem != null)
            {
                string conditionName = M_BuyConditionComboBox.SelectedItem.ToString();
                Condition condition = listCondition.Find(o => o.Name.Equals(conditionName));
                if (condition != null)
                {
                    LoadSetting(conditionName);
                }

            }
        }

        private void interestListBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void AddForceBtn_Click(object sender, EventArgs e)
        {
            if (trailingSaveListBox.SelectedItem != null && TsListBox.SelectedItem != null)
            {
                string selectItem = trailingSaveListBox.SelectedItem.ToString();
                string selectTsItem = TsListBox.SelectedItem.ToString();
                string[] rqNameArray = selectItem.Split(':');
                string condition = rqNameArray[1];
                string itemCode = rqNameArray[2];

                TradingStrategy ts = tradingStrategyList.Find(o => o.buyCondition.Name.Equals(condition));

                if(ts!=null && ts.remainItemCount > 0)
                {
                    ts.remainItemCount--;

                    ts.StrategyConditionReceiveUpdate(itemCode, 0, 0, TRADING_ITEM_STATE.AUTO_TRADING_STATE_SEARCH_AND_CATCH);
                    TryBuyItem(ts, itemCode);
                }
            }
        }
    }
}
