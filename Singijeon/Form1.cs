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
using Singijeon.Item;

namespace Singijeon
{
    public partial class Form1 : Form
    {
        CoreEngine coreEngine;
        private string currentAccount = string.Empty;
        public static string account = string.Empty;
        private static int screenNum = 1000;
        private string server = "0";
        public static double FEE_RATE = 1;

        string[] codeArray;
        string[] codeKosdaqArray;

        List<Condition> listCondition = new List<Condition>();

        public List<TradingStrategy> tradingStrategyList = new List<TradingStrategy>();

        public Hashtable doubleCheckHashTable = new Hashtable();

        public BalanceAllSellStrategy bssAll;
        List<BalanceItem> balanceItemList = new List<BalanceItem>();
        List<BalanceItem> balanceSelectedItemList = new List<BalanceItem>(); //전체잔고 매도전략
        public List<BalanceStrategy> balanceStrategyList = new List<BalanceStrategy>();

        List<TrailingItem> trailingList = new List<TrailingItem>();


        List<StockItem> stockItemList = new List<StockItem>(); //상장종목리스트

        List<TradingItem> tryingOrderList = new List<TradingItem>(); //주문접수시도

        //같은 종목에 대하여 주문이 여러개 들어가도 주문순서대로 응답이 오기 때문에 각각의 리스트로 들어가게됨

        List<SettlementItem> tryingSettlementItemList = new List<SettlementItem>(); //청산 접수 시도(주문번호만 따기위한 리스트)
        List<SettlementItem> settleItemList = new List<SettlementItem>(); //진행중인 청산 시도

        List<BalanceBuyStrategy> tryingBuyList = new List<BalanceBuyStrategy>(); //잔고 매수 접수 시도(주문번호 따는 리스트)
        List<BalanceSellStrategy> tryingSellList = new List<BalanceSellStrategy>(); //잔고 매도 접수 시도(주문번호 따는 리스트)

        List<string> rebuyStrategyList = new List<string>(); //재구매 전략 저장 리스트
        public List<string> RebuyStrategyList { get { return rebuyStrategyList; } }

        Dictionary<string, Queue<String>> rebuyStockStrategy = new Dictionary<string, Queue<String>>(); //종목명-재구매리스트 저장

        Dictionary<string, NotConclusionItem> nonConclusionList = new Dictionary<string, NotConclusionItem>();
        Form3 printForm = null;
        Form2 printForm2 = null;
        Form3 printForm_kospi = null;
        Form3 printForm_kosdaq = null;
        MA_ENVELOPE ma_5 = null;
        MA_ENVELOPE ma_7 = null;
        MA_ENVELOPE ma_10 = null;
        MA_ENVELOPE ma_15 = null;
        long buyPlusMoney = 0;
        List<MA_ENVELOPE> list_envelopeChecker = new List<MA_ENVELOPE>();
        TimerJob newTimer;
        KospiInfo info;
        //public string rebuyCondition = string.Empty;
        public AxKHOpenAPILib.AxKHOpenAPI AxKHOpenAPI { get { return axKHOpenAPI1; } }
        public Form1()
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

            accountBalanceDataGrid.CellClick += DataGridView_CellClick;
            autoTradingDataGrid.CellClick += AutoTradingDataGridView_CellClick;
            tsDataGridView.CellClick += TradingStrategyGridView_CellClick;

            accountBalanceDataGrid.SelectionChanged += AccountDataGridView_SelectionChanged;

            axKHOpenAPI1.OnEventConnect += API_OnEventConnect; //로그인
            axKHOpenAPI1.OnReceiveConditionVer += API_OnReceiveConditionVer; //검색 받기
            axKHOpenAPI1.OnReceiveRealCondition += API_OnReceiveRealCondition; //실시간 검색
            axKHOpenAPI1.OnReceiveTrCondition += API_OnReceiveTrCondition; //검색
            axKHOpenAPI1.OnReceiveMsg += API_OnReceiveMsg;

            axKHOpenAPI1.OnReceiveTrData += API_OnReceiveTrData; //정보요청
            axKHOpenAPI1.OnReceiveTrData += API_OnReceiveTrDataHoga; //정보요청(호가)
            axKHOpenAPI1.OnReceiveChejanData += API_OnReceiveChejanData; //체결잔고
            axKHOpenAPI1.OnReceiveRealData += API_OnReceiveRealData; //실시간정보
            axKHOpenAPI1.OnReceiveRealData += API_OnReceiveRealDataHoga; //실시간정보

            MartinGailManager.GetInstance().Init(axKHOpenAPI1, this);
            BlockManager.GetInstance().Init(axKHOpenAPI1, this);

            ma_5 = new MA_ENVELOPE();
            ma_5.Init(axKHOpenAPI1, 0.05);
            list_envelopeChecker.Add(ma_5);
            ma_7 = new MA_ENVELOPE();
            ma_7.Init(axKHOpenAPI1, 0.07);
            list_envelopeChecker.Add(ma_7);
            ma_10 = new MA_ENVELOPE();
            ma_10.Init(axKHOpenAPI1, 0.1);
            list_envelopeChecker.Add(ma_10);
            ma_15 = new MA_ENVELOPE();
            ma_15.Init(axKHOpenAPI1, 0.15);
            list_envelopeChecker.Add(ma_15);

            //LoadSetting();
            printForm = new Form3(axKHOpenAPI1);
            info = new KospiInfo();
            UpdateTimer();

           

        }

        void UpdateTimer()
        {
            newTimer = new TimerJob();
            newTimer.StartWork(1000 * 10, delegate ()
            {
                if (kospiInfo.InvokeRequired)
                {
                    kospiInfo.Invoke(new MethodInvoker(delegate ()
                    {
                        kospiInfo.Text = info.GetStockKospi();
                        kosdaqInfo.Text = info.GetStockKosdaq();
                    }));
                }
                else
                {
                    kospiInfo.Text = info.GetStockKospi();
                    kosdaqInfo.Text = info.GetStockKosdaq();
                }
            });
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
                string codeList = axKHOpenAPI1.GetCodeListByMarket("0");

                codeArray = codeList.Split(';');

                string codeKosdaqList = axKHOpenAPI1.GetCodeListByMarket("10");

                codeKosdaqArray = codeKosdaqList.Split(';');

                AutoCompleteStringCollection collection = new AutoCompleteStringCollection();

                foreach (string code in codeArray)
                {
                    string name = axKHOpenAPI1.GetMasterCodeName(code);
                    StockItem stockItem = new StockItem() { Code = code, Name = name };
                    stockItemList.Add(stockItem);
                    collection.Add(name);
                }
                foreach (string code in codeKosdaqArray)
                {
                    string name = axKHOpenAPI1.GetMasterCodeName(code);
                    StockItem stockItem = new StockItem() { Code = code, Name = name };
                    stockItemList.Add(stockItem);
                    collection.Add(name);
                }

                interestTextBox.AutoCompleteCustomSource = collection;
                //OpenThirdWindow();

                //사용자 조건식 불러오기
                axKHOpenAPI1.GetConditionLoad();
                SaveLoadManager.GetInstance().SetForm(this, axKHOpenAPI1);
                SaveLoadManager.GetInstance().UseCustomDefaultSetting();
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
            }
        }

        public bool CheckCanBuyItem(string itemCode)
        {
            //모든 전략에서 매도 완료됬거나 매수취소된것
            bool returnBuy = true;
            foreach (var ts in tradingStrategyList)
            {
                List<TradingItem> tradeItemArray = ts.tradingItemList.FindAll(o => o.itemCode.Contains(itemCode));
                foreach (var item in tradeItemArray)
                {
                    bool canBuy = item.IsCompleteSold() || item.IsBuyCancel();
                    if (!canBuy)
                        returnBuy = canBuy;
                }

            }
            coreEngine.SendLogMessage("returnBuy = " + axKHOpenAPI1.GetMasterCodeName(itemCode) + " : "+returnBuy);
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
                coreEngine.SendLogMessage("검색시간 = " + DateTime.Now);
                coreEngine.SendLogMessage("_________________________________");

                //종목 편입(어떤 전략(검색식)이었는지)

                TradingStrategy ts = tradingStrategyList.Find(o => o.buyCondition.Name.Equals(conditionName));

                if (doubleCheckHashTable.ContainsKey(conditionName))
                {
                    TradingStrategy ts_doubleCheck = (TradingStrategy)doubleCheckHashTable[conditionName];
                    if (ts_doubleCheck.doubleCheckItemCode.Contains(conditionName) == false)
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
                                if (ts.usingDoubleCheck)
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

                                        if (printForm.vwma_state == Form3.VWMA_CHART_STATE.UP_STAY)
                                        {
                                            //골든크로스를 목적으로 미리 up상태인것을 배재한다
                                            coreEngine.SendLogMessage(axKHOpenAPI1.GetMasterCodeName(_itemCode));
                                            return;
                                        }

                                        TrailingItem trailingItem = trailingList.Find(o => o.itemCode.Contains(_itemCode));

                                        if (CheckCanBuyItem(_itemCode) && trailingItem == null)
                                        {
                                            ts.remainItemCount--; //남을 매수할 종목수-1
                                            coreEngine.SaveItemLogMessage(_itemCode, "구매 시도 종목 추가 검색명 = " + conditionName);

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
                                        coreEngine.SaveItemLogMessage(itemCode, "구매 시도 종목 추가 검색명 = " + conditionName);
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
                                coreEngine.SaveItemLogMessage(itemcode, "종목 일반 매수 시도");

                                if (server.Equals(ConstName.TEST_SERVER))
                                {
                                    string warning = "모의투자 환경에서 \n 현재가 1,000원 미만인 종목, \n 총 발행 주식수 100,000주 미만 종목, \n 프리보드 종목, \n 관리종목, \n 정리매매, \n 투자주의, \n 투자경고, \n 투자위험종목, \n ELW종목 은 주문제외됩니다";
                                    coreEngine.SaveItemLogMessage(itemcode, warning);
                                    coreEngine.SendLogMessage(warning);
                                }

                                int orderResult =

                                axKHOpenAPI1.SendOrder(
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
                                    tradingItem.SetBuyState();


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
            else if (e.sRQName == (ConstName.RECEIVE_TR_DATA_ACCOUNT_INFO))
            {
                currentAccount = account;

                string codeList = string.Empty;
                int cnt = axKHOpenAPI1.GetRepeatCnt(e.sTrCode, e.sRQName); //조회내용중 멀티데이터의 갯수를 알아온다

                balanceItemList.Clear();

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

                    Hashtable uiTable = new Hashtable() { { "계좌잔고_종목코드", itemCode }, { "계좌잔고_종목명", itemName }, { "계좌잔고_보유수량", (int)lBalanceCnt }, { "계좌잔고_평균단가", dBuyingPrice }, { "계좌잔고_평가금액", lEstimatedAmount }, { "계좌잔고_매입금액", lBuyingAmount }, { "계좌잔고_손익금액", lProfitAmount }, { "계좌잔고_손익률", dProfitRate } };
                    Update_AccountBalanceDataGrid_UI(uiTable, rowIndex);

                    if (balanceItemList.Find(o => (o.itemCode == itemCode)) == null)
                        balanceItemList.Add(new BalanceItem(itemCode, itemName, (int)dBuyingPrice, (int)lBalanceCnt, accountBalanceDataGrid.Rows[rowIndex]));
                }
                string fidList = "9001;302;10;11;25;12;13"; //9001:종목코드,302:종목명
                axKHOpenAPI1.SetRealReg("9001", codeList, fidList, "1");
                coreEngine.SendLogWarningMessage("SetRealReg  : " + codeList);

                SaveLoadManager.GetInstance().SetForm(this, axKHOpenAPI1);
                SaveLoadManager.GetInstance().DeserializeStrategy();
                SaveLoadManager.GetInstance().DeserializeTrailing();
                SaveLoadManager.GetInstance().DeserializeBSS();
                SaveLoadManager.GetInstance().LoadAppendSetting();
            }
            else if (e.sRQName == ConstName.RECEIVE_TR_DATA_REALTIME_NOT_CONCLUSION)
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

                    coreEngine.SendLogWarningMessage(stockName + "미체결처리");
                    int index = outstandingDataGrid.Rows.Add();
                    nonConclusionList.Add(orderNum, new NotConclusionItem(orderNum, stockCode, orderGubun, stockName, orderQnt, orderPrice, outstandingNumber));
                    Hashtable outstandingTable = new Hashtable { { "미체결_주문번호", orderNum }, { "미체결_종목코드", stockCode }, { "미체결_종목명", stockName }, { "미체결_주문수량", orderQnt }, { "미체결_미체결량", outstandingNumber }, { "미체결_주문가", orderPrice } };
                    Update_OutStandingDataGrid_UI(outstandingTable, index);
                }
            }
            else if (e.sRQName.Contains(ConstName.RECEIVE_TR_DATA_FOR_BUY_MORE))
            {
                coreEngine.SendLogWarningMessage(e.sRQName + " RECEIVE_TR_DATA_FOR_BUY_MORE" + e.sTrCode);
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

                if (balanceItemList.Find(o => (o.itemCode == itemCode)) != null)
                {
                    balanceItemList.Find(o => (o.itemCode == itemCode)).curPrice = (int)c_lPrice;
                }

                //종목의 매매전략 얻어오기
                //모든 매매전략내 전략에 포함된 종목을 찾고, 매매전략의 손익률 셋팅과 비교

                List<TradingItem> tradeItemListAll = GetAllTradingItemData(itemCode);

                foreach (TradingItem tradeItem in tradeItemListAll)
                {
                    tradeItem.UpdateCurrentPrice(c_lPrice);

                    if (tradeItem.IsCompleteBuying() && tradeItem.IsCompleteSold() == false && tradeItem.buyingPrice != 0) //매도 진행안된것 
                    {
                        double realProfitRate = GetProfitRate((double)c_lPrice, (double)tradeItem.buyingPrice);
                        //coreEngine.SaveItemLogMessage(itemCode, "현재가 : " + c_lPrice + " 평단가 : " + tradeItem.buyingPrice + " 손익률 : " + realProfitRate);
                        //자동 감시 주문 체크
                        if (tradeItem.state >= TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_COMPLETE
                            && tradeItem.state < TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_COMPLETE)
                        {
                            tradeItem.ts.CheckUpdateTradingStrategyAddedItem(tradeItem, realProfitRate, CHECK_TIMING.SELL_TIME);
                        }

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
                CheckBS(itemCode, c_lPrice);
                //CheckBSS(itemCode, c_lPrice);
                CheckBSSAll(itemCode, c_lPrice);

                UpdateAccountBalanceDataGridViewRow(itemCode, c_lPrice);
                UpdateAutoTradingDataGridViewRow(itemCode, c_lPrice);
            }
        }

        private void CheckBS(string itemCode, long c_lPrice)
        {
            List<BalanceStrategy> bsList = balanceStrategyList.FindAll(o => (o.itemCode.Equals(itemCode) && o.state == TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_COMPLETE));

            foreach (var bs in bsList)
            {
                if (bs != null && bs.bUseStrategy)
                {
                    bs.CheckBalanceStrategy(this, itemCode, c_lPrice, delegate () {

                    });
                }
            }
        }

        private void CheckBS_Finish(string itemCode, bool buy, long conclusionQnt, string orderNum)
        {
            coreEngine.SaveItemLogMessage(itemCode, "CheckBS_Finish");
            List<BalanceStrategy> bsList = balanceStrategyList.FindAll(o => o.itemCode.Equals(itemCode));

            foreach (var bs in bsList)
            {
                if (buy && bs.orderNum == orderNum && bs.state == TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUYMORE_NOT_COMPLETE)
                {
                    coreEngine.SaveItemLogMessage(itemCode, "buy more finish");

                    bs.state = TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUYMORE_COMPLETE;
                    bs.bUseStrategy = false;

                    List<TradingItem> tradeItemListAll = GetAllTradingItemData(itemCode);
                    foreach (var item in tradeItemListAll)
                    {
                        if (bs.tradingItem != null && bs.tradingItem == item && item.usingStopLossAfterBuyMore)
                            item.usingStopLossAfterBuyMore = false;
                        if (bs.tradingItem != null && bs.tradingItem == item && item.usingTakeProfitAfterBuyMore)
                            item.usingTakeProfitAfterBuyMore = false;
                    }

                    BalanceBuyStrategy bbsStrategy = (BalanceBuyStrategy)bs;

                    if (bbsStrategy != null) {
                        bbsStrategy.ui_rowItem.Cells["bbs_상태"].Value = "완료";
                    }

                    tryingBuyList.Remove(bbsStrategy);

                    //balanceStrategyList.Remove(bs);
                }
            }
        }
        private int GetBssSellQnt(long c_lPrice)
        {
            if (c_lPrice < 10000)
            {
                return 10000 / (int)c_lPrice;
            }
            else
            {
                return 1;
            }
            return 1;
        }
        private void CheckBSSAll(string itemCode, long c_lPrice)
        {
            if (bssAll != null && bssAll.usingStrategy)
            {
                foreach (var item in balanceSelectedItemList)
                {
                    if (item.itemCode.Contains(itemCode.Replace("A", "")) == false)
                        continue;
                    if (!item.bSell)
                        continue;
                    double profitRate = GetProfitRate((double)c_lPrice, (double)item.buyingPrice);
                    int sellQnt = GetBssSellQnt(c_lPrice);

                    int orderResult = axKHOpenAPI1.SendOrder(
                                      ConstName.SEND_ORDER_SELL,
                                      GetScreenNum().ToString(),
                                      account,
                                      CONST_NUMBER.SEND_ORDER_SELL,
                                      itemCode,
                                      sellQnt,
                                      bssAll.profitOrderOption == ConstName.ORDER_SIJANGGA ? 0 : (int)c_lPrice,
                                      bssAll.profitOrderOption,
                                      "" //원주문번호없음
                                  );
                    if (orderResult == 0) //요청 성공시 (실거래는 안될 수 있음)
                    {
                        coreEngine.SendLogMessage(axKHOpenAPI1.GetMasterCodeName(itemCode) + " bss 익절 매도주문접수시도");
                        coreEngine.SendLogMessage("ui -> 매도주문접수시도");
                        item.bSell = false;

                        BssAllGridViewUpdate(itemCode, item.balanceQnt, ConstName.AUTO_TRADING_STATE_SELL_BEFORE_ORDER, item.ui_rowItem);
                    }
                    else
                    {
                        coreEngine.SendLogMessage(axKHOpenAPI1.GetMasterCodeName(itemCode) + " bss 잔고 익절 요청 실패");
                    }
                }
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
                string orderPrice = axKHOpenAPI1.GetChejanData(901).Trim();
                string outstanding = axKHOpenAPI1.GetChejanData(902).Trim();
                string tradingType = axKHOpenAPI1.GetChejanData(906).Trim();
                string time = axKHOpenAPI1.GetChejanData(908).Trim();
                string conclusionPrice = axKHOpenAPI1.GetChejanData(910).Trim();
                string conclusionQuantity = axKHOpenAPI1.GetChejanData(911).Trim();
                string unitConclusionQuantity = axKHOpenAPI1.GetChejanData(915).Trim();
                string error_code = axKHOpenAPI1.GetChejanData(919).Trim();
                string canOrderQuantity = axKHOpenAPI1.GetChejanData(933).Trim();

                int i_orderQuantity = int.Parse(orderQuantity);
                int i_orderPrice = int.Parse(orderPrice);

                int i_ConclusionQuantity = 0;
                int.TryParse(conclusionQuantity, out i_ConclusionQuantity);

                int i_unitConclusionQuantity = 0;
                int.TryParse(unitConclusionQuantity, out i_unitConclusionQuantity);

                string price = axKHOpenAPI1.GetChejanData(10).Trim();

                coreEngine.SaveItemLogMessage(itemCode, "___________접수/체결_____________");
                coreEngine.SaveItemLogMessage(itemCode, "종목명 : " + axKHOpenAPI1.GetMasterCodeName(itemCode));
                coreEngine.SaveItemLogMessage(itemCode, "주문상태 : " + orderState);
                coreEngine.SaveItemLogMessage(itemCode, "주문번호 : " + ordernum);
                coreEngine.SaveItemLogMessage(itemCode, "종목코드 : " + itemCode);
                coreEngine.SaveItemLogMessage(itemCode, "주문구분 : " + orderType);
                coreEngine.SaveItemLogMessage(itemCode, "주문가격 : " + orderPrice);
                coreEngine.SaveItemLogMessage(itemCode, "현재가 : " + price);
                coreEngine.SaveItemLogMessage(itemCode, "체결가 : " + conclusionPrice);
                coreEngine.SaveItemLogMessage(itemCode, "매매구분 : " + tradingType);
                coreEngine.SaveItemLogMessage(itemCode, "주문수량 : " + orderQuantity);
                coreEngine.SaveItemLogMessage(itemCode, "체결량(누적체결량) :" + conclusionQuantity);
                coreEngine.SaveItemLogMessage(itemCode, "미체결 수량 :" + outstanding);
                coreEngine.SaveItemLogMessage(itemCode, "단위체결량(체결당 체결량) :" + unitConclusionQuantity);
                coreEngine.SaveItemLogMessage(itemCode, "거부사유 :" + error_code);
                coreEngine.SaveItemLogMessage(itemCode, "________________________________");

                if (orderState.Equals(ConstName.RECEIVE_CHEJAN_DATA_SUBMIT))
                {
                    if (int.Parse(outstanding) == 0)
                    {
                        //접수인데 미체결량 0 이면 매수 또는 매도 취소신호임
                        //이때 주문번호는 기존 취소하려던 주문 번호로 들어옴
                        RemoveOrderList(ordernum);
                        return;
                    }
                    TradingItem CheckItemExist = this.tryingOrderList.Find(o => (itemCode.Contains(o.itemCode)));

                    //주문번호 따오기 위한 부분 
                    if (CheckItemExist != null)
                    {
                        if (orderType.Equals(ConstName.RECEIVE_CHEJAN_DATA_BUY))
                        {
                            coreEngine.SaveItemLogMessage(itemCode, "주문접수완료");
                            coreEngine.SaveItemLogMessage(itemCode, "수량 : " + i_orderQuantity);
                            List<TradingItem> items = this.tryingOrderList.FindAll(o => (itemCode.Contains(o.itemCode)));

                            if (items.Count > 1)
                            {
                                coreEngine.SaveItemLogMessage(itemCode, "주문리스트에 중복된 종목이 있습니다");
                            }

                            foreach (var item in items)
                            {
                                coreEngine.SaveItemLogMessage(itemCode, "찾아낸 종목명 : " + axKHOpenAPI1.GetMasterCodeName(itemCode) + " orderNum : " + ordernum);
                                coreEngine.SaveItemLogMessage(itemCode, "찾아낸 종목 주문 수량 : " + item.buyingQnt);
                                coreEngine.SaveItemLogMessage(itemCode, "찾아낸 종목 가격 : " + orderPrice);

                                if (!string.IsNullOrEmpty(orderQuantity)
                                    && int.Parse(orderQuantity) > 0)
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
                            //접수상태로 체결이 안될수도 있기 때문에 일단 주문가능한 수량에서 제외한다.
                            item.curCanOrderQnt = item.curCanOrderQnt - item.sellQnt;

                            item.SetState(TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_NOT_COMPLETE);

                            RemoveOrderList(item); //접수리스트에서만 지움

                            UpdateSellAutoTradingDataGridStateOnly(ordernum, ConstName.AUTO_TRADING_STATE_SELL_NOT_COMPLETE);
                            item.ts.StrategyOnReceiveSellOrderUpdate(item.itemCode, (int)item.buyingPrice, item.buyingQnt, TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_NOT_COMPLETE);
                            coreEngine.SaveItemLogMessage(itemCode, "매도 접수 - 주문번호 : " + ordernum);
                            coreEngine.SaveItemLogMessage(itemCode, "현재수량" + item.curQnt + " 주문수량 : " + item.sellQnt + " 매도가능수량 : " + item.curCanOrderQnt);
                        }
                        else if (orderType.Equals(ConstName.RECEIVE_CHEJAN_CANCEL_BUY_ORDER))
                        {
                            coreEngine.SaveItemLogMessage(itemCode, "!!!!!!!!!매수 취소 요청 접수!!!!!!!!");

                            TradingItem item = this.tryingOrderList.Find(o => (itemCode.Contains(o.itemCode)));
                            item.buyCancelOrderNum = ordernum;
                            //RemoveOrderList(item);  //접수리스트에서만 지움
                            coreEngine.SaveItemLogMessage(itemCode, "매수 취소 요청 - " + "종목코드 : " + itemCode + "취소시킬 주문번호 : " + item.buyOrderNum + " 취소 접수 주문번호 : " + ordernum);
                        }
                        else if (orderType.Equals(ConstName.RECEIVE_CHEJAN_CANCEL_SELL_ORDER))
                        {
                            coreEngine.SaveItemLogMessage(itemCode, "!!!!!!!!!매도 취소 요청 접수!!!!!!!!");

                            TradingItem item = this.tryingOrderList.Find(o => (itemCode.Contains(o.itemCode)));
                            item.sellCancelOrderNum = ordernum;
                            //RemoveOrderList(item); //접수리스트에서만 지움
                            coreEngine.SaveItemLogMessage(itemCode, "매도 취소 요청 - " + "종목코드 : " + itemCode + "취소시킬 주문번호 : " + item.sellOrderNum + " 취소 접수 주문번호 : " + ordernum);
                        }
                    }
                    else //자동매매에 의한 주문이 아닐때
                    {
                        coreEngine.SaveItemLogMessage(itemCode, " 원주문찾기 실패");

                        //보유 아이템 매매인지
                        RefreshBSS(itemCode, ordernum, orderQuantity);
                        RefreshBSSAll(itemCode, ordernum, orderQuantity);
                        RefreshBBS(itemCode, ordernum, orderQuantity);
                        RefreshSettlement(itemCode, ordernum, orderQuantity);

                        //외부프로그램에서  매도했을시 처리
                        if (orderType.Equals(ConstName.RECEIVE_CHEJAN_DATA_SELL))
                        {
                            foreach (TradingStrategy ts in tradingStrategyList)
                            {
                                List<TradingItem> itemArray = ts.tradingItemList.FindAll(o => o.itemCode.Equals(itemCode));
                                foreach (var item in itemArray)
                                {
                                    if (item != null)
                                    {
                                        coreEngine.SaveItemLogMessage(itemCode, "외부 매도 주문 / 현재 수량 : " + item.curQnt);

                                        if (!string.IsNullOrEmpty(orderQuantity)
                                        && int.Parse(orderQuantity) > 0
                                        && item.state >= TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_COMPLETE
                                        && item.state < TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_COMPLETE)
                                        //&& item.curQnt == int.Parse(orderQuantity)) //일부 매도는 고려하지않는다
                                        {
                                            item.sellPrice = long.Parse(orderPrice);
                                            item.sellOrderNum = ordernum;
                                            item.sellQnt = int.Parse(orderQuantity);
                                            item.SetState(TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_NOT_COMPLETE);
                                            UpdateSellAutoTradingDataGridStateOnly(ordernum, ConstName.AUTO_TRADING_STATE_SELL_NOT_COMPLETE);
                                            item.ts.StrategyOnReceiveSellOrderUpdate(item.itemCode, (int)item.buyingPrice, item.buyingQnt, TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_NOT_COMPLETE);
                                            coreEngine.SaveItemLogMessage(itemCode, "외부 매도 요청 - " + "종목코드 : " + itemCode + " 주문번호 : " + ordernum + " 수량 : " + orderQuantity);
                                        }
                                    }
                                }
                            }
                        }
                        else if (orderType.Equals(ConstName.RECEIVE_CHEJAN_DATA_BUY)) //외부 매수
                        {
                            if (string.IsNullOrEmpty(currentAccount))
                                return;

                            coreEngine.SaveLogMessage(" 물타기 스텝 : 주문완료");
                            coreEngine.SaveItemLogMessage(itemCode, " 물타기 스텝 : 주문완료"); //내가 수동으로 사든 프로그램이 사든 물타기로 취급

                            bool findItem = false;

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
                                            findItem = true;
                                        }
                                    }
                                }
                            }

                            List<BalanceStrategy> bsList = balanceStrategyList.FindAll(o => (o.itemCode.Equals(itemCode) && o.type == BalanceStrategy.BALANCE_STRATEGY_TYPE.BUY));

                            if (blockingCheckBox.Checked && bsList.Count == 0 && findItem == false)
                            {
                                coreEngine.SendLogWarningMessage("즉시 취소");
                                coreEngine.SaveItemLogMessage(itemCode, "즉시 취소");
                                int orderResult = axKHOpenAPI1.SendOrder(ConstName.RECEIVE_TR_DATA_MODIFY,
                                                                         GetScreenNum().ToString(),
                                                                         currentAccount,
                                                                         CONST_NUMBER.SEND_ORDER_CANCEL_BUY,
                                                                         itemCode,
                                                                         i_orderQuantity,
                                                                         (int)i_orderPrice,
                                                                         tradingType,
                                                                         ordernum);
                                return;
                            }
                        }
                    }

                    int rowIndex = orderDataGridView.Rows.Add();
                    Hashtable uiOrderTable = new Hashtable { { "주문_주문번호", ordernum }, { "주문_계좌번호", account }, { "주문_시간", time }, { "주문_종목코드", itemCode }, { "주문_종목명", itemName }, { "주문_매매구분", orderType }, { "주문_가격구분", tradingType }, { "주문_주문량", orderQuantity }, { "주문_주문가격", orderPrice } };
                    Update_OrderDataGrid_UI(uiOrderTable, rowIndex);

                    //미체결 처리 && 외부매수 매도

                    if (orderType != ConstName.RECEIVE_CHEJAN_CANCEL_BUY_ORDER && orderType != ConstName.RECEIVE_CHEJAN_CANCEL_SELL_ORDER)
                    {
                        coreEngine.SendLogWarningMessage(axKHOpenAPI1.GetMasterCodeName(itemCode) + " 미체결처리");

                        bool haveKey = false;
                        foreach (var key in nonConclusionList.Keys)
                        {
                            if (key.Contains(ordernum) || ordernum.Contains(key))
                            {
                                haveKey = true;
                            }
                        }

                        if (haveKey == false)
                        {
                            int index = outstandingDataGrid.Rows.Add();
                            Hashtable outstandingTable = new Hashtable { { "미체결_주문번호", ordernum }, { "미체결_종목코드", itemCode }, { "미체결_종목명", itemName }, { "미체결_주문수량", orderQuantity }, { "미체결_미체결량", orderQuantity }, { "미체결_주문가", orderPrice } };
                            Update_OutStandingDataGrid_UI(outstandingTable, index);
                            nonConclusionList.Add(ordernum, new NotConclusionItem(ordernum, itemCode, orderType, itemName, int.Parse(orderQuantity), int.Parse(orderPrice), int.Parse(orderQuantity)));
                        }
                    }

                }
                else if (orderState.Equals(ConstName.RECEIVE_CHEJAN_DATA_CONCLUSION))
                {
                    coreEngine.SaveItemLogMessage(itemCode, "체결 주문당 잔고 : " + outstanding + " / 누적 체결수량 :" + conclusionQuantity + " / 유닛체결수량 : " + i_unitConclusionQuantity);
                    //체결완료이지만 (outstanding == 0) 종목당 잔량이 남아있을수 있다(분할매도 시)
                    if (int.Parse(outstanding) == 0 && string.IsNullOrEmpty(conclusionQuantity) == false)
                    {
                        if (orderType.Contains(ConstName.RECEIVE_CHEJAN_DATA_BUY))
                        {
                            UpdateTradingStrategyBuy(ordernum, true, i_unitConclusionQuantity, i_orderPrice);
                            UpdateBuyAutoTradingDataGridState(ordernum, true);
                            CheckBS_Finish(itemCode, true, i_ConclusionQuantity, ordernum);
                        }
                        else if (orderType.Contains(ConstName.RECEIVE_CHEJAN_DATA_SELL))
                        {
                            //자동 매매 진행중일때
                            UpdateTradingStrategySellData(ordernum, i_unitConclusionQuantity, int.Parse(outstanding));
                            UpdateSellAutoTradingDataGridStatePrice(ordernum, conclusionPrice);

                            //printForm2.AddProfit((int.Parse(conclusionPrice) - i_averagePrice) * i_unitConclusionQuantity);

                            CheckBSS_Sell(ordernum, conclusionPrice); //bss 체크
                            CheckBSS_AllSell(ordernum, conclusionPrice, int.Parse(outstanding), i_ConclusionQuantity); //bss all 체크
                            CheckSettle_Sell(ordernum); //청산 체크
                            //CheckBS_Finish(itemCode, false, i_ConclusionQuantity, ordernum);

                            BalanceItem item = balanceItemList.Find(o => (o.itemCode == itemCode));
                            if (item != null)
                            {
                                long profit = (int.Parse(conclusionPrice) - item.buyingPrice) * i_unitConclusionQuantity;
                                printForm2.AddProfit(profit);
                                if (profit < 0)
                                {
                                    if(item.balanceQnt == 0)
                                    {
                                        //손절완료일때 재구매 전략 확인
                                        coreEngine.SendLogWarningMessage("재구매 실행");
                                        AddItemRebuyStrategy(item.itemCode);
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
                                UpdateBuyAutoTradingDataGridState(ordernum, false);
                            }
                            else if (orderType.Contains(ConstName.RECEIVE_CHEJAN_DATA_SELL))
                            {
                                UpdateTradingStrategySellData(ordernum, i_unitConclusionQuantity, int.Parse(outstanding));
                                UpdateSellTradingItemOutstand(ordernum, int.Parse(outstanding));
                                UpdateSellAutoTradingDataGridStatePrice(ordernum, conclusionPrice);
                                BalanceItem item = balanceItemList.Find(o => (o.itemCode == itemCode));
                                if (item != null)
                                    printForm2.AddProfit((int.Parse(conclusionPrice) - item.buyingPrice) * i_unitConclusionQuantity);
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

                string price = axKHOpenAPI1.GetChejanData(10);

                coreEngine.SaveItemLogMessage(itemCode, "________________잔고_____________");
                coreEngine.SaveItemLogMessage(itemCode, "종목코드 : " + itemCode);
                coreEngine.SaveItemLogMessage(itemCode, "종목명 : " + axKHOpenAPI1.GetMasterCodeName(itemCode));
                coreEngine.SaveItemLogMessage(itemCode, "보유수량 : " + balanceQnt);
                coreEngine.SaveItemLogMessage(itemCode, "주문가능수량(매도가능) : " + orderAvailableQnt);
                coreEngine.SaveItemLogMessage(itemCode, "매수매도구분 :" + tradingType);
                coreEngine.SaveItemLogMessage(itemCode, "매입단가 :" + buyingPrice);
                coreEngine.SaveItemLogMessage(itemCode, "총매입가 :" + totalBuyingPrice);
                coreEngine.SaveItemLogMessage(itemCode, "________________________________");

                double profitRate = GetProfitRate(double.Parse(price), double.Parse(buyingPrice));
                if (int.Parse(balanceQnt) > 0)
                {
                    foreach (TradingStrategy ts in tradingStrategyList)
                    {
                        List<TradingItem> items = ts.tradingItemList.FindAll(o => (o.itemCode == itemCode));
                        foreach (var item in items)
                        {
                            if (item != null)
                            {
                                coreEngine.SaveItemLogMessage(itemCode, "매입단가 셋팅:" + buyingPrice);

                                item.buyingPrice = int.Parse(buyingPrice);
                                item.curQnt = int.Parse(balanceQnt);
                                item.curCanOrderQnt = int.Parse(orderAvailableQnt);
                            }
                        }
                        if (items == null || items.Count == 0)
                        {
                            coreEngine.SaveItemLogMessage(itemCode, "잔고 종목을 찾을수 없습니다");
                        }
                    }

                    //UpdateTradingStrategyByBalance(itemCode, int.Parse(balanceQnt), int.Parse(buyingPrice));
                    UpdateBuyAutoTradingDataGridState(itemCode);
                }

                //잔고탭 업데이트
                //bool hasItem_balanceDataGrid = false;
                //foreach (DataGridViewRow row in balanceDataGrid.Rows)
                //{
                //    if (row.Cells["잔고_종목코드"].Value != null && row.Cells["잔고_종목코드"].Value.ToString().Contains(itemCode))
                //    {
                //        hasItem_balanceDataGrid = true;

                //        if (int.Parse(balanceQnt) > 0)
                //        {
                //            Hashtable uiTable = new Hashtable() { { "잔고_보유수량", balanceQnt }, { "잔고_현재가", price }, { "잔고_매입단가", buyingPrice }, { "잔고_주문가능수량", orderAvailableQnt }, { "잔고_총매입가", totalBuyingPrice }, { "잔고_손익률", profitRate } };
                //            Update_BalanceDataGrid_UI(uiTable, row.Index);
                //        }
                //        else
                //        {
                //            balanceDataGrid.Rows.Remove(row);
                //        }

                //        break;
                //    }
                //}

                //if (!hasItem_balanceDataGrid)
                //{
                //    int balance_rowIndex = balanceDataGrid.Rows.Add();
                //    Hashtable uiTable = new Hashtable() { { "잔고_계좌번호", account }, { "잔고_종목코드", itemCode }, { "잔고_종목명", itemName }, { "잔고_보유수량", balanceQnt }, { "잔고_주문가능수량", orderAvailableQnt }, { "잔고_매입단가", buyingPrice }, { "잔고_총매입가", totalBuyingPrice }, { "잔고_손익률", profitRate }, { "잔고_매매구분", tradingType }, { "잔고_현재가", price } };
                //    Update_BalanceDataGrid_UI(uiTable, balance_rowIndex);
                //}

                if (int.Parse(balanceQnt) > 0)
                {
                    if (balanceItemList.Find(o => (o.itemCode == itemCode)) == null)
                    {
                        //coreEngine.SendLogMessage(itemCode + " 잔고 리스트에 추가");
                        //int rowIndex = accountBalanceDataGrid.Rows.Add();
                        //balanceItemList.Add(new BalanceItem(itemCode, itemName, int.Parse(buyingPrice), int.Parse(balanceQnt), null));
                    }
                    else
                    {
                        coreEngine.SendLogMessage(itemCode + " 잔고 리스트 값 변경");
                        BalanceItem item = balanceItemList.Find(o => (o.itemCode == itemCode));
                        item.buyingPrice = int.Parse(buyingPrice);
                        item.balanceQnt = int.Parse(balanceQnt);
                    }
                }
                else
                {
                    if (balanceItemList.Find(o => (o.itemCode == itemCode)) != null)
                    {
                        balanceItemList.Remove(balanceItemList.Find(o => (o.itemCode == itemCode)));
                    }
                }


                bool hasItem_accountBalanceDataGrid = false;

                foreach (DataGridViewRow row in accountBalanceDataGrid.Rows)
                {
                    if (row.Cells["계좌잔고_종목코드"].Value != null && row.Cells["계좌잔고_종목코드"].Value.ToString().Contains(itemCode))
                    {
                        hasItem_accountBalanceDataGrid = true;

                        if (int.Parse(balanceQnt) > 0)
                        {
                            Hashtable uiTable = new Hashtable { { "계좌잔고_보유수량", int.Parse(balanceQnt) }, { "계좌잔고_현재가", price }, { "계좌잔고_평균단가", buyingPrice }, { "계좌잔고_평가금액", (int.Parse(balanceQnt) * int.Parse(buyingPrice)) }, { "계좌잔고_매입금액", totalBuyingPrice }, { "계좌잔고_손익률", profitRate } };
                            Update_AccountBalanceDataGrid_UI(uiTable, row.Index);
                        }
                        else
                        {
                            accountBalanceDataGrid.Rows.Remove(row);
                        }

                        break;
                    }
                }

                if (!string.IsNullOrEmpty(currentAccount) && !hasItem_accountBalanceDataGrid && int.Parse(balanceQnt) > 0)
                {
                    int rowIndex = accountBalanceDataGrid.Rows.Add();
                    if (balanceItemList.Find(o => (o.itemCode == itemCode)) == null)
                    {
                        //coreEngine.SendLogMessage(itemCode + " 잔고 리스트에 추가");
                        //int rowIndex = accountBalanceDataGrid.Rows.Add();
                        balanceItemList.Add(new BalanceItem(itemCode, itemName, int.Parse(buyingPrice), int.Parse(balanceQnt), accountBalanceDataGrid.Rows[rowIndex]));
                    }
                    int evaluationAmount = int.Parse(buyingPrice) * int.Parse(balanceQnt);
                    int profitAmount = (int.Parse(price) - int.Parse(buyingPrice)) * int.Parse(balanceQnt);
                    Hashtable uiTable = new Hashtable { { "계좌잔고_종목코드", itemCode }, { "계좌잔고_종목명", itemName }, { "계좌잔고_보유수량", int.Parse(balanceQnt) }, { "계좌잔고_평균단가", buyingPrice }, { "계좌잔고_손익률", profitRate }, { "계좌잔고_현재가", price }, { "계좌잔고_매입금액", totalBuyingPrice }, { "계좌잔고_평가금액", evaluationAmount }, { "계좌잔고_손익금액", profitAmount } };
                    Update_AccountBalanceDataGrid_UI(uiTable, rowIndex);

                    string fidList = "9001;302;10;11;25;12;13"; //9001:종목코드,302:종목명
                    axKHOpenAPI1.SetRealReg("9001", itemCode + ";", fidList, "1");
                }
            }
        }

        private void RefreshSettlement(string itemCode, string ordernum, string orderQuantity)
        {
            //청산인지
            SettlementItem settleItem = this.tryingSettlementItemList.Find((o => itemCode.Contains(o.ItemCode)));
            if (settleItem != null && (settleItem.orderQnt == long.Parse(orderQuantity)))
            {
                settleItem.sellOrderNum = ordernum;
                tryingSettlementItemList.Remove(settleItem);
            }
        }

        private void RefreshBSS(string itemCode, string ordernum, string orderQuantity)
        {
            List<BalanceSellStrategy> bssList = GetTryingSellList(itemCode);

            if (bssList != null && bssList.Count > 0)
            {
                foreach (BalanceSellStrategy bss in bssList)
                {
                    if (!bss.orderNum.Equals(ordernum)
                        && bss.sellQnt == long.Parse(orderQuantity)
                        && bss.state == TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_BEFORE_ORDER)
                    {
                        bss.orderNum = ordernum;
                        //tryingSellList.Remove(bss);
                        bss.ui_rowItem.Cells["bss_주문번호"].Value = ordernum;

                        break;
                    }
                }
            }
        }
        private void RefreshBSSAll(string itemCode, string ordernum, string orderQuantity)
        {
            BalanceItem item = balanceSelectedItemList.Find(o => (o.itemCode == itemCode));
            if (item != null)
            {
                item.orderNum = ordernum;
                BssAllGridViewUpdate(item.itemCode, item.balanceQnt, ConstName.AUTO_TRADING_STATE_SELL_NOT_COMPLETE, item.ui_rowItem);
            }
        }
        private void RefreshBBS(string itemCode, string ordernum, string orderQuantity)
        {
            List<BalanceBuyStrategy> bbsList = GetTryingBuyList(itemCode);

            if (bbsList != null)
            {
                foreach (BalanceBuyStrategy bbs in bbsList)
                {
                    if (!bbs.orderNum.Equals(ordernum)
                        && bbs.state == TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUYMORE_BEFORE_ORDER)
                    {
                        bbs.orderNum = ordernum;
                        //tryingSellList.Remove(bss);
                        bbs.state = TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUYMORE_NOT_COMPLETE;
                        bbs.ui_rowItem.Cells["bbs_주문번호"].Value = ordernum;

                        break;
                    }
                }
            }
        }


        private void RefreshBSS_Complete(string itemCode, string ordernum, string orderQuantity)
        {
            List<BalanceSellStrategy> bssList = GetTryingSellList(itemCode);

            if (bssList != null && bssList.Count > 0)
            {
                foreach (BalanceSellStrategy bss in bssList)
                {
                    if (bss.orderNum.Equals(ordernum) && bss.sellQnt == long.Parse(orderQuantity))
                    {
                        tryingSellList.Remove(bss);

                        foreach (DataGridViewRow row in BssDataGridView.Rows)
                        {
                            if (row.Cells["bss_종목코드"].Value.ToString().Contains(itemCode)
                                && row.Cells["bss_주문번호"].Value != null
                                && row.Cells["bss_주문번호"].Value.ToString() == bss.orderNum.ToString()
                            )
                            {
                                row.Cells["bss_상태"].Value = "완료";
                                break;
                            }
                        }
                        break;
                    }
                }
            }
        }

        private void CheckSettle_Sell(string ordernum)
        {
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

        private void CheckBSS_Sell(string ordernum, string conclusionPrice)
        {
            //보유잔고 매도
            BalanceSellStrategy bss = GetTryingSellListByOrder(ordernum);
            if (bss != null)
            {
                RefreshBSS_Complete(bss.itemCode, bss.orderNum, bss.sellQnt.ToString());
                bss.bUseStrategy = false;

                BalanceItem item = balanceItemList.Find(o => (o.itemCode == bss.itemCode));
                if (item == null)
                {
                    coreEngine.SendLogErrorMessage("wrong idx : " + bss.itemCode);
                    return;
                }

                int iQnt = item.balanceQnt;
                item.balanceQnt = iQnt - (int)bss.sellQnt;

                if (item.balanceQnt < 0)
                    coreEngine.SendLogErrorMessage("count wrong");

                foreach (DataGridViewRow row in accountBalanceDataGrid.Rows)
                {
                    if (row.Cells["계좌잔고_종목코드"].Value != null && row.Cells["계좌잔고_종목코드"].Value.ToString().Replace("A", "").Contains(bss.itemCode))
                    {
                        if (item.balanceQnt > 0)
                        {
                            row.Cells["계좌잔고_보유수량"].Value = item.balanceQnt;
                        }
                        else
                        {
                            accountBalanceDataGrid.Rows.Remove(row);
                        }
                    }
                }
            }
        }

        private void CheckBSS_AllSell(string ordernum, string conclusionPrice, int outstanding, int unitConclusionQnt)
        {
            if (outstanding > 0)
                return;

            BalanceItem item = balanceSelectedItemList.Find(o => (o.orderNum == ordernum));
            if (item == null)
            {
                return;
            }

            int iQnt = item.balanceQnt;
            item.balanceQnt = iQnt - unitConclusionQnt;

            if (item.balanceQnt < 0)
            {
                coreEngine.SendLogErrorMessage("count wrong");
            }
            item.bSell = true;
            BssAllGridViewUpdate(item.itemCode, item.balanceQnt, ConstName.AUTO_TRADING_STATE_SELL_COMPLETE, item.ui_rowItem);


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
            if (accountBalanceDataGrid.Columns["계좌잔고_종목코드"].Index == e.ColumnIndex)
            {
                string itemCode = accountBalanceDataGrid["계좌잔고_종목코드", e.RowIndex].Value.ToString().Replace("A", "");
                Form3 chartForm = new Form3(axKHOpenAPI1);

                chartForm.RequestItem(itemCode, delegate (string _itemCode)
                {
                    chartForm.Show();
                }, Form3.CHART_TYPE.MINUTE_5);
            }
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
                            DialogResult result = MessageBox.Show("종목을 청산 하시겠습니까?", "청산", MessageBoxButtons.YesNo);
                            if (result == DialogResult.Yes)
                            {
                                SellAllClear(itemCode, balanceCnt, ReceiveSellAllClear, e.RowIndex);
                            }
                          
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
                ConstName.SEND_ORDER_ALL_SELL,
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
            if (autoTradingDataGrid.Columns["매매진행_종목코드"].Index == e.ColumnIndex)
            {
                string itemCode = autoTradingDataGrid["매매진행_종목코드", e.RowIndex].Value.ToString().Replace("A", "");
                Form3 chartForm = new Form3(axKHOpenAPI1);

                chartForm.RequestItem(itemCode, delegate (string _itemCode)
                {
                    chartForm.Show();
                }, Form3.CHART_TYPE.MINUTE_5);
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
                                if (curState.Equals(ConstName.AUTO_TRADING_STATE_BUY_BEFORE_ORDER))
                                {

                                    TrailingItem item = trailingList.Find(o => (o.ui_rowAutoTradingItem == rowItem));

                                    if (item != null)
                                    {
                                        trailingList.Remove(item);
                                    }
                                    coreEngine.SendLogMessage("주문접수 시도 취소");
                                    tradeItem.SetBuyCancelOrder();
                                }
                                if (curState.Equals(ConstName.AUTO_TRADING_STATE_BUY_NOT_COMPLETE) || curState.Equals(ConstName.AUTO_TRADING_STATE_BUY_NOT_COMPLETE_OUTCOUNT))
                                {
                                    //취소주문
                                    int orderResult = axKHOpenAPI1.SendOrder(ConstName.RECEIVE_TR_DATA_MODIFY, GetScreenNum().ToString(), currentAccount, CONST_NUMBER.SEND_ORDER_CANCEL_BUY, itemCode, tradeItem.outStandingQnt, (int)tradeItem.buyingPrice, tradeItem.buyOrderType, tradeItem.buyOrderNum);

                                    if (orderResult == 0)
                                    {
                                        tradeItem.SetBuyCancelOrder();
                                        AddOrderList(tradeItem);
                                        coreEngine.SendLogMessage("취소 접수 성공");
                                        autoTradingDataGrid["매매진행_취소", e.RowIndex].Value = "취소접수시도";
                                        return;
                                    }
                                }

                                if (curState.Equals(ConstName.AUTO_TRADING_STATE_SELL_NOT_COMPLETE) || curState.Equals(ConstName.AUTO_TRADING_STATE_SELL_NOT_COMPLETE_OUTCOUNT))
                                {
                                    //취소주문
                                    int orderResult = axKHOpenAPI1.SendOrder(ConstName.RECEIVE_TR_DATA_MODIFY, GetScreenNum().ToString(), currentAccount, CONST_NUMBER.SEND_ORDER_CANCEL_SELL, itemCode, tradeItem.outStandingQnt, (int)tradeItem.sellPrice, tradeItem.sellOrderType, tradeItem.sellOrderNum);

                                    if (orderResult == 0)
                                    {
                                        tradeItem.SetSellCancelOrder();
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

                        StopMonitoring(ts.buyCondition);

                        tradingStrategyList.Remove(ts);
                        tsDataGridView.Rows.RemoveAt(e.RowIndex);
                        string removeKey = string.Empty;
                        foreach (var item in doubleCheckHashTable.Values)
                        {
                            if (((TradingStrategy)item) == ts)
                            {
                                removeKey = ts.doubleCheckCondition.Name;
                                StopMonitoring(ts.doubleCheckCondition);
                            }
                        }
                        if (!string.IsNullOrEmpty(removeKey))
                        {
                            doubleCheckHashTable.Remove(removeKey);
                        }

                    }
                }
            }
            else
            {
                string conditionName = tsDataGridView["매매전략_매수조건식", e.RowIndex].Value.ToString();
                ForceAddStrategyTextBox.Text = conditionName;
                ReBuyStrategyTextBox.Text = conditionName;
            }
        }

        private void ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            axKHOpenAPI1.CommConnect();
        }

        private void InterestConditionListBox_SelectedIndexChanged(object sender, EventArgs s)
        {

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
                    ReqAccountInfo(account);
                }
            }
        }
        public void SetAccountComboBox(string account)
        {
            if (accountComboBox.InvokeRequired)
            {
                accountComboBox.Invoke(new MethodInvoker(delegate ()
                {
                    accountComboBox.SelectedItem = account;
                }));
            }
            else
            {
                accountComboBox.SelectedItem = account;
            }
               
        }
        public void ReqAccountInfo(string account)
        {
            axKHOpenAPI1.SetInputValue("계좌번호", account);
            axKHOpenAPI1.SetInputValue("비밀번호", "");
            axKHOpenAPI1.SetInputValue("상장폐지조회구분", "0");
            axKHOpenAPI1.SetInputValue("비밀번호입력매체구분", "00");
            axKHOpenAPI1.CommRqData(ConstName.RECEIVE_TR_DATA_ACCOUNT_INFO, "OPW00004", 0, GetScreenNum().ToString());

            axKHOpenAPI1.SetInputValue("계좌번호", account);
            axKHOpenAPI1.SetInputValue("체결구분", "1");
            axKHOpenAPI1.SetInputValue("매매구분", "0"); //0:전체 1:매도 2:매수
            axKHOpenAPI1.CommRqData(ConstName.RECEIVE_TR_DATA_REALTIME_NOT_CONCLUSION, "opt10075", 0, GetScreenNum().ToString());
        }

        public void BalanceSell(string accountNum, string itemCode, int buyingPrice, int curQnt, int sellQnt, string takeProfitOrderType, string stopLossOrderType,  double takeProfitRate, double stopLossRate)
        {
             if (accountNum.Length > 0)
            {
                if (itemCode.Length > 0)
                {
                    if (sellQnt > 0)
                    {
                        //잔고 매도 전략 추가시 기존 전략의 자동 매도는 전부 꺼준다

                        //List<TradingItem> tradeItemListAll = GetAllTradingItemData(itemCode);

                        //foreach (TradingItem tradeItem in tradeItemListAll)
                        //{
                        //    tradeItem.ts.usingStoploss = false;
                        //    tradeItem.ts.usingTakeProfit = false;
                        //}

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

                        balanceStrategyList.Add(bs);

                        int rowIndex = BssDataGridView.Rows.Add();
                        bs.ui_rowItem = BssDataGridView.Rows[rowIndex];

                        Hashtable uiTable = new Hashtable() { { "bss_종목코드", itemCode }, { "bss_종목명", axKHOpenAPI1.GetMasterCodeName(itemCode) }, { "bss_매도량", sellQnt }, { "bss_설정손익률", takeProfitRate.ToString() + " / " + stopLossRate.ToString() } };
                        UpdateBssGridView(uiTable, rowIndex);
                    
                        coreEngine.SaveItemLogMessage(itemCode,"잔고 매매 전략이 입력됬습니다");
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
    
            if (!b_ProfitSellCheckBox.Checked && !b_StopLossCheckBox.Checked)
            {
                MessageBox.Show("익절 / 손절 값을 체크해주세요");
                return;
            }

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

            if (!b_ProfitSellCheckBox.Checked && !b_StopLossCheckBox.Checked)
            {
                MessageBox.Show("익절 / 손절 값을 체크해주세요");
                return;
            }
            BalanceSell(accountNum, itemCode, buyingPrice, (int)curQnt, (int)sellQnt, orderType, orderType, takeProfitRate, stopLossRate);
        }
        private void AddStratgyBtn_Click(object sender, EventArgs e)
        {
            AddStrategy();
        }
        private void AddStrategy()
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

            string divide_sellStopLossOrderOpt = "00";
            if (divide_stopLossSijangRadio.Checked)
            {
                divide_sellStopLossOrderOpt = ConstName.ORDER_SIJANGGA;
            }
            else
            {
                divide_sellStopLossOrderOpt = ConstName.ORDER_JIJUNGGA;
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
                divide_sellStopLossOrderOpt,
                false,
                usingBuyRestart
                );

            //추가전략 적용
            bool usingTimeCheck = TimeUseCheck.Checked; //시간 제한 사용

            if (usingTimeCheck)
            {
                DateTime startDate =
                    new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, startTimePicker.Value.Hour, startTimePicker.Value.Minute, 0);
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

            if (useVwmaCheckBox.Checked)
            {
                ts.usingVwma = useVwmaCheckBox.Checked;
                ts.usingTrailing = true;
                ts.trailTickValue = 30;
            }

            if (useEnvelopeCheckBox.Checked)
            {
                ts.usingEnvelope5 = useEnvelopeCheckBox.Checked;
                ts.usingTrailing = true;
                ts.trailTickValue = 30;
            }

            if (useEnvelope7CheckBox.Checked)
            {
                ts.usingEnvelope7 = useEnvelope7CheckBox.Checked;
                ts.usingTrailing = true;
                ts.trailTickValue = 30;
            }

            if (useEnvelope10CheckBox.Checked)
            {
                ts.usingEnvelope10 = useEnvelope10CheckBox.Checked;
                ts.usingTrailing = true;
                ts.trailTickValue = 30;
            }

            if (useEnvelope15CheckBox.Checked)
            {
                ts.usingEnvelope15 = useEnvelope15CheckBox.Checked;
                ts.usingTrailing = true;
                ts.trailTickValue = 30;
            }

            if (useCheckStockIndex.Checked)
            {
                ts.usingCheckIndex = useCheckStockIndex.Checked;
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

                //TradingStrategyItemWithUpDownPercentValue trailGapBuy =
                //    new TradingStrategyItemWithUpDownPercentValue(
                //            StrategyItemName.BUY_GAP_CHECK,
                //            CHECK_TIMING.BUY_TIME,
                //            string.Empty,
                //            ts.gapTrailCostPercentageValue);

                //trailGapBuy.OnReceivedTrData += this.OnReceiveTrDataCheckGapTrailBuy;
                //ts.AddTradingStrategyItemList(trailGapBuy);
            }

            bool usingProfitCheckBox = profitSellCheckBox.Checked; //익절사용
            bool usingTakeProfitAfterBuyMore = takeProfitAfterBuyMoreCheck.Checked;
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
                ts.usingTakeProfitAfterBuyMore = usingTakeProfitAfterBuyMore;
            }

            if (usingProfitCheckBox && usingTrailingStopSell)
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
                ts.usingTakeProfitAfterBuyMore = usingTakeProfitAfterBuyMore;
            }

            bool usingStopLoss = minusSellCheckBox.Checked; //손절사용
            bool usingStopLossAfterBuyMore = stopLossAfterBuyMoreCheck.Checked;

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
                ts.usingStopLossAfterBuyMore = usingStopLossAfterBuyMore;
            }

            bool usingDivideSellLoss = DivideSellLossCheckBox.Checked; //분할매도손절

            if (usingDivideSellLoss)
            {
                double stopLossRate = 0;
                stopLossRate = (double)divideRatePercentLoss.Value;
                double stopLossSellPercent = 0;
                stopLossSellPercent = (double)divideSellPercentLoss.Value;

                TradingStrategyItemWithUpDownValue divideStopLossStrategy = null;
                divideStopLossStrategy =
                    new TradingStrategyItemWithUpDownValue(
                            StrategyItemName.STOPLOSS_DIVIDE_SELL,
                            CHECK_TIMING.SELL_TIME,
                            IS_TRUE_OR_FALE_TYPE.DOWN,
                            stopLossRate);

                ts.AddTradingStrategyItemList(divideStopLossStrategy);
                divideStopLossStrategy.OnReceivedTrData += this.OnReceiveTrDataCheckStopLossDivide;
                ts.useDivideSellLoss = true;
                ts.useDivideSellLossLoop = divideLossSellLoopCheck.Checked;
                ts.divideStoplossRate = stopLossRate;
                ts.divideSellLossPercentage = (stopLossSellPercent * 0.01);
                ts.divideSellCount = (int)DivideSellCountUpDown.Value;
            }

            bool usingDivideSellProfit = DivideSellProfitCheckBox.Checked; //분할매도익절
            if (usingDivideSellProfit)
            {
                double profitRate = 0;
                profitRate = (double)divideRatePercentProfit.Value;
                double profitSellPercent = 0;
                profitSellPercent = (double)divideSellPercentProfit.Value;

                TradingStrategyItemWithUpDownValue divideProfitStrategy = null;
                divideProfitStrategy =
                    new TradingStrategyItemWithUpDownValue(
                            StrategyItemName.TAKE_PROFIT_DIVIDE_SELL,
                            CHECK_TIMING.SELL_TIME,
                            IS_TRUE_OR_FALE_TYPE.UPPER_OR_SAME,
                            profitRate);

                ts.AddTradingStrategyItemList(divideProfitStrategy);
                divideProfitStrategy.OnReceivedTrData += this.OnReceiveTrDataCheckProfitDivide;
                ts.useDivideSellProfit = true;
                ts.useDivideSellProfitLoop = divideProfitSellLoopCheck.Checked;
                ts.divideTakeProfitRate = profitRate;
                ts.divideSellProfitPercentage = (profitSellPercent * 0.01);
                ts.divideSellCountProfit = (int)DivideSellCountUpDownProfit.Value;
            }

            bool usingBuyMore = BuyMoreCheckBox.Checked; //물타기

            if (usingBuyMore)
            {
                ts.usingBuyMore = true;
                //물타기
                ts.buyMoreRateLoss = (double)BuyMorePercentUpdown.Value;
                ts.buyMoreMoney = (int)BuyMoreValueUpdown.Value;
                TradingStrategyItemBuyingDivide buyMoreStrategy =
                    new TradingStrategyItemBuyingDivide(
                            StrategyItemName.BUY_MORE_LOSS,
                            CHECK_TIMING.SELL_TIME,
                            IS_TRUE_OR_FALE_TYPE.DOWN,
                             ts.buyMoreRateLoss,
                             ts.buyMoreMoney
                             );

                buyMoreStrategy.OnReceivedTrData += this.OnReceiveTrDataBuyMore;
                ts.AddTradingStrategyItemList(buyMoreStrategy);

                //불타기
                ts.buyMoreRateProfit = (double)BuyMorePercentUpdownProfit.Value;
                ts.buyMoreMoney = (int)BuyMoreValueUpdown.Value;
                TradingStrategyItemProfitBuyingDivide buyMoreStrategyProfit =
                    new TradingStrategyItemProfitBuyingDivide(
                            StrategyItemName.BUY_MORE_PROFIT,
                            CHECK_TIMING.SELL_TIME,
                            IS_TRUE_OR_FALE_TYPE.UPPER_OR_SAME,
                             ts.buyMoreRateProfit,
                             ts.buyMoreMoney
                             );
                buyMoreStrategyProfit.OnReceivedTrData += this.OnReceiveTrDataBuyMoreProfit;
                ts.AddTradingStrategyItemList(buyMoreStrategyProfit);
            }

            bool usingBuyCancleByTime = buyCancelTimeCheckBox.Checked;

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

            if (usingDoubleCheck)
            {
                StartMonitoring(ts.doubleCheckCondition);
                doubleCheckHashTable.Add(ts.doubleCheckCondition.Name, ts);
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

                    BBSItemCodeTxt.Text = itemCode.Replace("A", "");
                    BBSItemNameTextbox.Text = itemName;
                   
                }
             
            }
        }

        private void addInterestBtn_Click(object sender, EventArgs e)
        {
            string itemName = interestTextBox.Text;
            StockItem stockItem = stockItemList.Find(o => o.Name.Equals(itemName));
            if (stockItem == null)
                return;
            string itemCode = stockItem.Code;
            string conditionName = ForceAddStrategyTextBox.Text;

            if (stockItem != null && string.IsNullOrEmpty(conditionName) == false)
            {

                TradingStrategy ts = tradingStrategyList.Find(o => o.buyCondition != null && o.buyCondition.Name.Equals(conditionName));

                if (ts != null)
                {
                    DialogResult result = MessageBox.Show(conditionName + "매매 종목을 강제 추가하겠습니까?", "매매전략 추가", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {

                        if (ts.remainItemCount == 0)
                        {
                            MessageBox.Show("매수가능 갯수 초과");
                            return;
                        }

                        TrailingItem trailingItem = trailingList.Find(o => (o.itemCode.Contains(itemCode) && o.strategy.buyCondition.Name == ts.buyCondition.Name));

                        if (CheckCanBuyItem(itemCode) && trailingItem == null)
                        {
                            ts.remainItemCount--; //남을 매수할 종목수-1
                            coreEngine.SaveItemLogMessage(itemCode, "구매 시도 종목 추가 검색명 = " + conditionName);

                            ts.StrategyConditionReceiveUpdate(itemCode, 0, 0, TRADING_ITEM_STATE.AUTO_TRADING_STATE_SEARCH_AND_CATCH);
                            TryBuyItem(ts, itemCode);
                        }

                    }
                }
            }
        }

        private void PrintToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenSecondWindow();
        }

        private void OpenSecondWindow()
        {
            printForm2 = new Form2(axKHOpenAPI1);
            printForm2.Show();
        }

        private void OpenThirdWindow()
        {
           
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

        public static int GetScreenNum()
        {
            screenNum++;

            if (screenNum > 5000)
                screenNum = 1000;

            return screenNum;
        }

        public static int GetRandom()
        {
            var rand = new Random();
            return rand.Next(100, 999);
        }

        public static double GetProfitRate(double curPrice, double buyPrice)
        {
            if (buyPrice <= 0)
                return 0;
            return (double)100 * ((curPrice - buyPrice) / buyPrice) - FEE_RATE;
        }

        public void OnReceiveTrDataCheckProfitSell(object sender, OnReceivedTrEventArgs e)
        {
            OnReceiveTrDataCheckProfitSell(e.tradingItem, e.checkNum);
        }
        public void OnReceiveTrDataCheckProfitDivide(object sender, OnReceivedTrEventArgs e)
        {
            if(e.tradingItem.usingDivideSellProfit)
            {
                OnReceiveTrDataCheckProfitSell(e.tradingItem, e.checkNum, e.tradingItem.ts.divideSellProfitPercentage);
                e.tradingItem.usingDivideSellProfit = false;

                if (e.tradingItem.usingDivideSellProfitLoop)
                {
                    e.tradingItem.usingDivideSellProfit = true;
                }
                if (e.tradingItem.divideSellCountProfit > 0)
                {
                    e.tradingItem.usingDivideSellProfit = true;
                    coreEngine.SaveItemLogMessage(e.tradingItem.itemCode, "남은 분할 익절 주문 횟수 : " + e.tradingItem.divideSellCountProfit);
                }
            }
        }
        public void OnReceiveTrDataCheckProfitSell(TradingItem item, double checkValue, double sellPercentage = 1)
        {
           
            if (item.state == TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_NOT_COMPLETE
                || item.state == TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_NOT_COMPLETE_OUTCOUNT)
            {
                if (item.IsProfitSell())
                {
                    //coreEngine.SaveItemLogMessage(item.itemCode, item.itemName + " 이전 익절 주문 실행 내용 확인");
                    //같은 익절 상태면 넘김
                    //단 익절률 이하로 떨어졌을때 걸려있는 주문 취소

                    //if( 0 < checkValue && checkValue < item.ts.divideSellProfitPercentage && sellPercentage != 1)
                    //{
                    //    coreEngine.SaveItemLogMessage(item.itemCode, "익절 확정//익절주문 취소 : " + item.itemName + " 수량 : " + item.curQnt + " 현재 손익률 : "+ checkValue);
                    //    int orderResultCancel = axKHOpenAPI1.SendOrder(ConstName.RECEIVE_TR_DATA_MODIFY, GetScreenNum().ToString(), currentAccount, CONST_NUMBER.SEND_ORDER_CANCEL_SELL, item.itemCode, item.curQnt, (int)item.sellPrice, item.sellOrderType, item.sellOrderNum);
                    //    if (orderResultCancel == 0)
                    //    {
                    //        AddOrderList(item);
                    //        item.SetSellCancelOrder();
                    //        coreEngine.SaveItemLogMessage(item.itemCode, "익절 취소 접수");
                    //        autoTradingDataGrid["매매진행_진행상황", item.GetUiConnectRow().Index].Value = ConstName.AUTO_TRADING_STATE_TAKE_PROFIT_CANCEL;
                    //        return;
                    //    }
                    //}


                    //if (checkValue > item.ts.takeProfitRate && checkValue > item.ts.divideSellProfitPercentage && sellPercentage != 1)
                    //{
                    //    //기존익절률 초과시 분할익절 취소하여 기존 익절 부분만 실행
                    //    coreEngine.SaveItemLogMessage(item.itemCode, "분할익절주문 취소 : " + item.itemName + " 수량 : " + item.curQnt + " 현재 손익률 : " + checkValue);
                    //    int orderResultCancel = axKHOpenAPI1.SendOrder(ConstName.RECEIVE_TR_DATA_MODIFY, GetScreenNum().ToString(), currentAccount, CONST_NUMBER.SEND_ORDER_CANCEL_SELL, item.itemCode, item.curQnt, (int)item.sellPrice, item.sellOrderType, item.sellOrderNum);

                    //    if (orderResultCancel == 0)
                    //    {
                    //        AddOrderList(item);
                    //        item.SetSellCancelOrder();
                    //        coreEngine.SaveItemLogMessage(item.itemCode, "익절 취소 접수");
                    //        autoTradingDataGrid["매매진행_진행상황", item.GetUiConnectRow().Index].Value = ConstName.AUTO_TRADING_STATE_TAKE_PROFIT_CANCEL;
                    //        return;
                    //    }
                    //}
                    return;
                }
                else //손절 걸려있을시
                {
                    int quantity = item.sellQnt;
                    quantity = Math.Max(1, quantity);
                    coreEngine.SaveItemLogMessage(item.itemCode, "손절주문 취소 : " + item.itemName + " 수량 " + quantity);
                    int orderResultCancel = axKHOpenAPI1.SendOrder(ConstName.RECEIVE_TR_DATA_MODIFY, GetScreenNum().ToString(), currentAccount, CONST_NUMBER.SEND_ORDER_CANCEL_SELL, item.itemCode, quantity, (int)item.sellPrice, item.sellOrderType, item.sellOrderNum);

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

            if (sellPercentage == 1)
            {
                if (item.usingTakeProfitAfterBuyMore)// 추가 불타기 후 꺼주게 설정
                    return;
            }
            else
            {
                if (item.divideSellCountProfit == 0 && item.usingTakeProfitAfterBuyMore)
                    return;

                item.divideSellCountProfit--;
                coreEngine.SaveItemLogMessage(item.itemCode, "분할 익절 주문 실행 / 카운트 :" + item.divideSellCountProfit);
            }

            item.SetSellOrderType(true);

            int orderQnt = (int)((double)item.startSellQnt * sellPercentage); //분할매도
            orderQnt = Math.Min(orderQnt, item.curCanOrderQnt);
            if (sellPercentage == 1)
            {
                orderQnt = item.curCanOrderQnt; //일반매도
            }

            int orderResult = axKHOpenAPI1.SendOrder(
                "종목익절매도",
                GetScreenNum().ToString(),
                item.ts.account,
                CONST_NUMBER.SEND_ORDER_SELL,
                item.itemCode,
                (orderQnt<=0)? 1:orderQnt,
                item.sellOrderType == ConstName.ORDER_SIJANGGA ? 0 : (int)item.curPrice,
                item.sellOrderType,//지정가
                "" //원주문번호없음
             );
            if (orderResult == 0) //요청 성공시 (실거래는 안될 수 있음)
            {
                AddOrderList(item);
                item.SetSold(true);
                coreEngine.SaveItemLogMessage(item.itemCode, "ui -> 익절매도주문접수시도");
                UpdateAutoTradingDataGridRow(item.itemCode, item, item.curPrice, ConstName.AUTO_TRADING_STATE_SELL_BEFORE_ORDER);
                UpdateAutoTradingDataGridRowWinLose(item.itemCode, item, "win");
                //printForm2.AddProfit((item.curPrice - item.buyingPrice) * item.curQnt);
            }
            else
            {
                coreEngine.SaveItemLogMessage(item.itemCode, "자동 익절 요청 실패");
            }
            coreEngine.SaveItemLogMessage(item.itemCode,
                item.itemName + "order 종목익절매도 " +
                 " 주문 수량: " + orderQnt +
                 " 주문가: " + item.curPrice +
                 " 주문구분: " + item.sellOrderType
             );
        }
        public void OnReceiveTrDataCheckStopLoss(object sender, OnReceivedTrEventArgs e)
        {
            OnReceiveTrDataCheckStopLoss(e.tradingItem, e.checkNum);
        }

        public void OnReceiveTrDataCheckStopLossDivide(object sender, OnReceivedTrEventArgs e)
        {
            //coreEngine.SaveItemLogMessage(e.tradingItem.itemCode, "분할 손절 진입");
            if (e.tradingItem.usingDivideSellLoss)
            {
                OnReceiveTrDataCheckStopLoss(e.tradingItem, e.checkNum, e.tradingItem.ts.divideSellLossPercentage, true);
                e.tradingItem.usingDivideSellLoss = false;

                if (e.tradingItem.usingDivideSellLossLoop)
                {
                    e.tradingItem.usingDivideSellLoss = true;
                }
                if (e.tradingItem.divideSellCount > 0)
                {
                    e.tradingItem.usingDivideSellLoss = true;
                    coreEngine.SaveItemLogMessage(e.tradingItem.itemCode, "남은 분할 손절 주문 횟수 : " + e.tradingItem.divideSellCount);
                }
            }
        }
        public void OnReceiveTrDataCheckStopLoss(TradingItem item, double checkValue, double sellPercentage = 1, bool StopLossDivide = false)
        {

            if (item.state == TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_CANCEL_NOT_COMPLETE)
            {
                coreEngine.SaveItemLogMessage(item.itemCode, "현재 손절할수있는 상태가 아닙니다 " + item.state);
                return;
            }

            if (item.state == TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_NOT_COMPLETE
                || item.state == TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_NOT_COMPLETE_OUTCOUNT)
            {
                if (item.IsProfitSell())
                {
                    int quantity = item.sellQnt;
                    quantity = Math.Max(1, quantity);
                    //취소주문(익절주문취소)
                    coreEngine.SaveItemLogMessage(item.itemCode,
                        "익절주문취소 : " + item.itemName
                         + " 이전 주문수량 : " + item.sellQnt
                         + " 주문 수량 : " + quantity.ToString()
                         + " 가격 : " + (int)item.sellPrice
                         + " 주문방법 : " + item.sellOrderType
                         + " 주문번호 : " + item.sellOrderNum
                        );
                    int orderResultCancel = axKHOpenAPI1.SendOrder(ConstName.RECEIVE_TR_DATA_MODIFY, GetScreenNum().ToString(), currentAccount, CONST_NUMBER.SEND_ORDER_CANCEL_SELL, item.itemCode, quantity, (int)item.sellPrice, item.sellOrderType, item.sellOrderNum);

                    if (orderResultCancel == 0)
                    {
                        AddOrderList(item);
                        item.SetSellCancelOrder();
                        coreEngine.SaveItemLogMessage(item.itemCode, "취소 접수 시도");
                        autoTradingDataGrid["매매진행_진행상황", item.GetUiConnectRow().Index].Value = ConstName.AUTO_TRADING_STATE_TAKE_PROFIT_CANCEL;

                        return;
                    }
                }
                else
                {
                    //같은 손절 주문일때
                    coreEngine.SaveItemLogMessage(item.itemCode, "기존 손절 주문 처리중 / 수량 " + item.outStandingQnt);
                    coreEngine.SaveItemLogMessage(item.itemCode, "현재 손해% : " + checkValue + " / 설정 stoplossRate : " + item.ts.stoplossRate  + " / 설정 divideStoplossRate : " + item.ts.divideStoplossRate);
                    return;
                }
            }

            if (item.state != TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_COMPLETE)
                return;
            
            if (!StopLossDivide)
            {
                if (item.usingStopLossAfterBuyMore)// 추가 물타기 후 꺼주게 설정
                    return;
            }
            else
            {
                if (item.divideSellCount == 0)
                {
                    if (item.usingStopLossAfterBuyMore)
                        return;
                }
                   
                item.divideSellCount--;
                coreEngine.SaveItemLogMessage(item.itemCode, "분할 손절 주문 실행 / 카운트 :" + item.divideSellCount);
            }
              
            item.SetSellOrderType(false);

            int orderQnt = (int)((double)item.startSellQnt * sellPercentage); //분할매도
            orderQnt = Math.Min(orderQnt, item.curCanOrderQnt);
            item.sellOrderType = item.ts.sellDivideStopLossOrderOption;

            if (sellPercentage == 1) //일반매도
            {
                orderQnt = item.curCanOrderQnt;
                item.sellOrderType = item.ts.sellStopLossOrderOption;
            }

            double minusRate = (sellPercentage == 1) ? item.ts.stoplossRate : item.ts.divideStoplossRate;
            double price = (double)item.buyingPrice * (1 + (minusRate * 0.01)); //minusRate : -값

            int tick = BalanceBuyStrategy.hogaUnitCalc(IsKospi(item.itemCode), (int)item.curPrice);
            int minusTick = (int)price % tick;
            int orderPrice = (int)price - minusTick;

            int orderResult = axKHOpenAPI1.SendOrder(
                "종목손절매도",
                GetScreenNum().ToString(),
                item.ts.account,
                CONST_NUMBER.SEND_ORDER_SELL,
                item.itemCode,
                (orderQnt <= 0) ? 1 : orderQnt,
                item.sellOrderType == ConstName.ORDER_SIJANGGA ? 0 : (int)orderPrice,
                item.sellOrderType,
                "" //원주문번호없음
            );

            if (orderResult == 0) //요청 성공시 (실거래는 안될 수 있음)
            {
                AddOrderList(item);
                item.SetSold(false);
                coreEngine.SaveItemLogMessage(item.itemCode, "ui -> 손절 매도주문접수시도");
                UpdateAutoTradingDataGridRow(item.itemCode, item, item.curPrice, ConstName.AUTO_TRADING_STATE_SELL_BEFORE_ORDER);
                UpdateAutoTradingDataGridRowWinLose(item.itemCode, item, "lose");
                //printForm2.AddProfit((item.curPrice - item.buyingPrice) * item.curQnt);
            }
            else
            {
                coreEngine.SaveItemLogMessage(item.itemCode, "자동 손절 요청 실패");
            }

            coreEngine.SaveItemLogMessage(item.itemCode,
              item.itemName + "order 종목손절매도 " +
              " 주문가능수량 : " + item.curCanOrderQnt +
              " 수량: " + orderQnt +
              " 주문가: " + orderPrice +
              " 주문구분: " + item.sellOrderType
           );
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
                            if(buyPrice <= 0)
                            {
                                coreEngine.SendLogErrorMessage("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                                coreEngine.SendLogErrorMessage(axKHOpenAPI1.GetMasterCodeName(itemcode) + " 호가찾기 에러!!!!!!!!!!!!!!!!!!!");
                                coreEngine.SendLogErrorMessage("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                                return;
                            }
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
                                    coreEngine.SendLogMessage(axKHOpenAPI1.GetMasterCodeName(itemcode) + " 종목 즉시 매수 시도 : " + axKHOpenAPI1.GetMasterCodeName(itemcode));

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
                                        coreEngine.SendLogMessage(axKHOpenAPI1.GetMasterCodeName(itemcode) + "즉시 매수주문요청 성공");

                                        TradingItem tradingItem = new TradingItem(ts, itemcode, axKHOpenAPI1.GetMasterCodeName(itemcode), price, i_qnt, false, false, ts.buyOrderOption);
                                        tradingItem.SetBuyState();
                                        

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
                else
                {
                    //그냥 호가정보만 알아오기
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
            coreEngine.SendLogErrorMessage("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            coreEngine.SendLogErrorMessage("ScreenNum : " + e.sScrNo + ",사용자구분명 : " + e.sRQName + ", Tr이름: " + e.sTrCode + ", MSG : " + e.sMsg);
            coreEngine.SendLogErrorMessage("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
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
                                    trailingItem.itemInvestment = (long)((float)trailingItem.itemInvestment);
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
                                if (price <= 0)
                                    continue;
                                if (trailingItem.curTickCount == 0)
                                {
                                    if (trailingItem.isVwmaCheck)
                                    {
                                        //주문 후에 trailingItem 은 list에서 remove 되지만
                                        //빠른속도로 tick이 증가하면 중복으로 호출될 가능성이 있음
                                        printForm.RequestItem(itemCode, delegate (string _itemCode) {

                                            if (printForm.vwma_state == Form3.VWMA_CHART_STATE.GOLDEN_CROSS || printForm.vwma_state == Form3.VWMA_CHART_STATE.UP_STAY)
                                            {
                                                TrailingItem findItem = trailingList.Find(o => (o.itemCode == _itemCode));
                                                if (findItem != null)
                                                {
                                                    StockWithBiddingEntity _stockInfo = StockWithBiddingManager.GetInstance().GetItem(_itemCode);
                                                    TrailingToBuy(findItem, _itemCode, (int)_stockInfo.GetBuyHoga(findItem.strategy.tickBuyValue), _stockInfo);
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

                            if (trailingItem.EnvelopeValueList.Count > 0
                                      && 30 < (DateTime.Now - trailingItem.envelopeBuyCheckDateTime).TotalSeconds)
                            {
                                trailingItem.envelopeBuyCheckDateTime = DateTime.Now;

                                foreach(var itemEnvelope in list_envelopeChecker)
                                {
                                    if (trailingItem.EnvelopeValueList.Contains(itemEnvelope.MA_PERCENT))
                                    {
                                        itemEnvelope.RequestItem(itemCode, delegate (string _itemCode, long curPrice, long envelopePrice) {
                                            if (curPrice < envelopePrice)
                                            {
                                                coreEngine.SaveItemLogMessage(_itemCode, "구매시도 진입 "+ curPrice + " / " + envelopePrice);
                                                TrailingItem findItem = trailingList.Find(o => (o.itemCode == _itemCode));
                                                if (findItem != null)
                                                {
                                                    StockWithBiddingEntity _stockInfo = StockWithBiddingManager.GetInstance().GetItem(_itemCode);
                                                    if((int)_stockInfo.GetBuyHoga(findItem.strategy.tickBuyValue) == 0)
                                                    {
                                                        coreEngine.SendLogErrorMessage("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                                                        coreEngine.SendLogErrorMessage(axKHOpenAPI1.GetMasterCodeName(_itemCode) + "호가 찾기 에러");
                                                        coreEngine.SendLogErrorMessage("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                                                    }
                                                    else
                                                    {
                                                        TrailingToBuy(findItem, _itemCode, (int)_stockInfo.GetBuyHoga(findItem.strategy.tickBuyValue), _stockInfo);
                                                    }
                                                   
                                                    return;
                                                }
                                            }
                                        });
                                    }
                                }
                            }

                            if (price <= 0)
                                continue;
                            if (trailingItem.isVwmaCheck)
                            {
                                continue;
                            }
                            if (trailingItem.EnvelopeValueList.Count > 0)
                            {
                                continue;
                            }
                            if(trailingItem.isCheckStockIndex)
                            {
                                if(IsKospi(trailingItem.itemCode))
                                {
                                    string kospi = info.GetStockKospi();
                                    float f_kospi = 0;
                                    float.TryParse(kospi, out f_kospi);
                                    if (f_kospi < 0)
                                    {
                                        coreEngine.SaveItemLogMessage(itemcode, "코스피지수 - 스킵");
                                        return;
                                    }
                                       
                                }
                                else
                                {
                                    string kosdaq = info.GetStockKosdaq();
                                    float f_kosdaq = 0;
                                    float.TryParse(kosdaq, out f_kosdaq);
                                    if (f_kosdaq < 0)
                                    {
                                        coreEngine.SaveItemLogMessage(itemcode, "코스닥지수 - 스킵");
                                        return;
                                    }
                                      
                                }
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
                                    tradingItem.SetBuyState();
                                   

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

            coreEngine.SendLogMessage(item.itemName + " OnReceiveTrDataBuyMore 추가매수 ");
            coreEngine.SaveItemLogMessage(item.itemCode,
                item.itemName + "OnReceiveTrDataBuyMore 추가매수 "
            );

            int buyQnt = Math.Abs((int)(tsItem.BuyMoney / item.curPrice));
            int curPrice = Math.Abs((int)item.curPrice);

            BalanceBuyStrategy bbs = BalanceBuy(account, item.itemCode, curPrice, buyQnt, item.buyOrderType, checkValue);
            bbs.SetTradingItem(item);
        }
        public void OnReceiveTrDataBuyMoreProfit(object sender, OnReceivedTrProfitBuyMoreEventArgs e)
        {
            OnReceiveTrDataBuyMoreProfit(e.strategyItem, e.tradingItem, e.checkNum);
        }
        public void OnReceiveTrDataBuyMoreProfit(TradingStrategyItemProfitBuyingDivide tsItem, TradingItem item, double checkValue)
        {
            if (item.state != TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_COMPLETE)
                return;

            coreEngine.SendLogMessage(item.itemName + " OnReceiveTrDataBuyMore 추가매수 ");
            coreEngine.SaveItemLogMessage(item.itemCode,
                item.itemName + "OnReceiveTrDataBuyMore 추가매수 "
            );

            int buyQnt = Math.Abs((int)(tsItem.BuyMoney / item.curPrice));
            int curPrice = Math.Abs((int)item.curPrice);

            BalanceBuyStrategy bbs = BalanceBuy(account, item.itemCode, curPrice, buyQnt, item.buyOrderType, checkValue);
            bbs.SetTradingItem(item);
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

      

        public void OnReceiveTrDataCheckGapTrailBuy(object sender, OnReceivedTrEventArgs e)
        {
            OnReceiveTrDataCheckGapTrailBuy(e.tradingItem, e.checkNum);
        }

        public void OnReceiveTrDataCheckGapTrailBuy(TradingItem item, double checkValue)
        {
            if (item.state == TRADING_ITEM_STATE.AUTO_TRADING_STATE_SEARCH_AND_CATCH)
            {
                //검색 -> 트레일링 에서 해당사항 체크 
                Console.WriteLine(item.itemName + "/"+ checkValue.ToString());
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

            double stopLossRate = (double)M_SellUpdownLoss.Value;

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

            int martinMaxStep = (int)MartinStepUpDown.Value;
            if (martinMaxStep <= 0)
            {
                MessageBox.Show("스텝단위를 선택해주세요");
                return;
            }
            MartinGailManager.GetInstance().MARTIN_MAX_STEP = martinMaxStep;

            MartinGailManager.GetInstance().SetMartinStrategy(ts, martinMaxStep);
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
                    tradeItem.startSellQnt = tradeItem.curQnt;
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
                    int curLastQnt = tradeItem.curQnt;
                    tradeItem.curQnt += addQnt;
                    tradeItem.startSellQnt = tradeItem.curQnt;

                    coreEngine.SaveItemLogMessage(tradeItem.itemCode, tradeItem.Uid);
                    coreEngine.SaveItemLogMessage(tradeItem.itemCode, "매수 처리");
                    coreEngine.SaveItemLogMessage(tradeItem.itemCode, "보유량 : " + tradeItem.curQnt);
                    coreEngine.SaveItemLogMessage(tradeItem.itemCode, "미체결 매도수량 : " + tradeItem.sellQnt);

                    //long PriceAverage = (long)((float)((curLastQnt * tradeItem.buyingPrice) + (priceUpdate * addQnt)) / tradeItem.curQnt);
                    //coreEngine.SaveItemLogMessage(tradeItem.itemCode, "평단가 : " + PriceAverage);
                    //tradeItem.buyingPrice = PriceAverage;
                    tradeItem.SetCompleteBuying(buyComplete);

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
        private void UpdateTradingStrategySellData(string orderNum,  int minusQnt, int outstanding)
        {

            foreach (TradingStrategy ts in tradingStrategyList)
            {
                TradingItem tradeItem = ts.tradingItemList.Find(o => o.sellOrderNum.Equals(orderNum));
                if (tradeItem != null)
                {
                    tradeItem.curQnt -= minusQnt;
                    tradeItem.sellQnt -= minusQnt;

                    coreEngine.SaveItemLogMessage(tradeItem.itemCode, "단위 매도량 : " + minusQnt);
                    coreEngine.SaveItemLogMessage(tradeItem.itemCode, "보유량 : " + tradeItem.curQnt);
                    coreEngine.SaveItemLogMessage(tradeItem.itemCode, "미체결 매도수량 : " + tradeItem.sellQnt);

                    bool sellComplete = (tradeItem.curQnt == 0) ? true : false;
                    tradeItem.SetCompleteSold(sellComplete);

                    if (sellComplete)
                    {
                        coreEngine.SaveItemLogMessage(tradeItem.itemCode, "매도 완료");
                        coreEngine.SendLogWarningMessage("SetRealRemove : " + tradeItem.itemCode);
                        axKHOpenAPI1.SetRealRemove("9001", tradeItem.itemCode); //실시간 정보받기 해제
                        tradeItem.ts.StrategyOnReceiveSellChejanUpdate(tradeItem.itemCode, (int)tradeItem.sellPrice, tradeItem.sellQnt, TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_COMPLETE);
                        tradeItem.sellQnt = 0;
                    }
                    else
                    {
                        if(outstanding > 0)
                        {
                            coreEngine.SaveItemLogMessage(tradeItem.itemCode, "매도 중");
                            tradeItem.ts.StrategyOnReceiveSellChejanUpdate(tradeItem.itemCode, (int)tradeItem.sellPrice, tradeItem.sellQnt, TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_NOT_COMPLETE_OUTCOUNT);
                        }
                        else if(outstanding == 0)
                        {
                            //미체결 0 , 잔고가 있으므로
                            
                            coreEngine.SaveItemLogMessage(tradeItem.itemCode, "분할 매도 완료");
                            tradeItem.SetCompleteBuying(true);
                            tradeItem.ts.StrategyOnReceiveSellChejanUpdate(tradeItem.itemCode, (int)tradeItem.sellPrice, tradeItem.sellQnt, TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUY_COMPLETE);
                            
                        }
                    }
                }
                else
                {

                    coreEngine.SendLogMessage("주문 찾기 실패 주문번호 : " + orderNum);
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
                    if (outStand <= 0)
                        continue;
                    tradeItem.SetState(TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_NOT_COMPLETE_OUTCOUNT);
                    tradeItem.SetOutStanding(outStand);
                }
            }
        }

        public void AddTryingSellList(BalanceSellStrategy strategy)
        {
            this.tryingSellList.Add(strategy);
        }
        public void AddTryingBuyList(BalanceBuyStrategy strategy)
        {
            this.tryingBuyList.Add(strategy);
        }
        private List<BalanceSellStrategy> GetTryingSellList(string itemCode)
        {
            return this.tryingSellList.FindAll(o => itemCode.Contains(o.itemCode));
        }

        private List<BalanceBuyStrategy> GetTryingBuyList(string itemCode)
        {
            return this.tryingBuyList.FindAll(o => itemCode.Contains(o.itemCode));
        }

        private BalanceSellStrategy GetTryingSellListByOrder(string orderNum)
        {
            return this.tryingSellList.Find(o => orderNum.Contains(o.orderNum));
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
                        coreEngine.SaveItemLogMessage(tradeItem.itemCode, "매도주문 취소완료");
                        tradeItem.SetSellCancelOrderComplete();
                        tradeItem.sellQnt = 0;
                        //ts.tradingItemList.Remove(tradeItem);
                        autoTradingDataGrid["매매진행_진행상황", tradeItem.ui_rowItem.Index].Value = ConstName.AUTO_TRADING_STATE_BUY_COMPLETE;
                        
                    }
                }

                foreach (DataGridViewRow row in outstandingDataGrid.Rows)
                {
                    if (row.Cells["미체결_주문번호"].Value != null && row.Cells["미체결_주문번호"].Value.ToString().Equals(orderNum))
                    {
                        outstandingDataGrid.Rows.Remove(row);
                        break;
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
                tradingItem.SetBuyState();
                

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
            //동일 종목 들어갈 수 있음
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

        public void RemoveOrderList(string orderNum)
        {
            TradingItem itemFindBuy = tryingOrderList.Find(o => (o.buyOrderNum == orderNum));
            if (itemFindBuy != null)
                tryingOrderList.Remove(itemFindBuy);

            TradingItem itemFindSell = tryingOrderList.Find(o => (o.sellOrderNum == orderNum));
            if (itemFindSell != null)
                tryingOrderList.Remove(itemFindSell);
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

            coreEngine.SaveAllLog();

            SaveLoadManager.GetInstance().SerializeStrategy(tradingStrategyList);
            SaveLoadManager.GetInstance().SerializeTrailing(trailingList);
            SaveLoadManager.GetInstance().SerializeBSS(balanceStrategyList);
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
                    int orderResult = axKHOpenAPI1.SendOrder(ConstName.RECEIVE_TR_DATA_MODIFY, GetScreenNum().ToString(), currentAccount, 
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
            //if (trailingSaveListBox.SelectedItem != null && TsListBox.SelectedItem != null)
            //{
            //    string selectItem = trailingSaveListBox.SelectedItem.ToString();
            //    string selectTsItem = TsListBox.SelectedItem.ToString();
            //    string[] rqNameArray = selectItem.Split(':');
            //    string condition = rqNameArray[1];
            //    string itemCode = rqNameArray[2];

            //    TradingStrategy ts = tradingStrategyList.Find(o => o.buyCondition.Name.Equals(condition));

            //    if(ts!=null && ts.remainItemCount > 0)
            //    {
            //        ts.remainItemCount--;

            //        ts.StrategyConditionReceiveUpdate(itemCode, 0, 0, TRADING_ITEM_STATE.AUTO_TRADING_STATE_SEARCH_AND_CATCH);
            //        TryBuyItem(ts, itemCode);
            //    }
            //}
        }

        private void balanceSellMonitorBtn_Click(object sender, EventArgs e)
        {
            if(bssAll != null)
            {
                {
                    bssAll.StopStrategy();
                    bssAll = null;
                    balanceSellMonitorBtn.Text = "매도시작";
                }
            }
            else
            {
                string orderType = (bssM_JijungRadio.Checked) ? ConstName.ORDER_JIJUNGGA : ConstName.ORDER_SIJANGGA;

                bssAll = new BalanceAllSellStrategy(orderType);
                balanceSellMonitorBtn.Text = "매도중";
            }
        }

        private void bssItemLoadBtn_Click(object sender, EventArgs e)
        {
            bssAllGridView.Rows.Clear();
            balanceSelectedItemList.Clear();
            foreach (var item in balanceItemList)
            {
                List<TradingItem> tradeItemListAll = GetAllTradingItemData(item.itemCode);

                if (tradeItemListAll.Count > 0)
                {
                    continue;
                }

                int rowIndex =  bssAllGridView.Rows.Add(axKHOpenAPI1.GetMasterCodeName(item.itemCode));

                DataGridViewRow new_row = bssAllGridView.Rows[rowIndex];
                new_row.Cells["bssAll_상태"].Value = "준비";
                new_row.Cells["bssAll_수량"].Value = item.balanceQnt;

                BalanceItem itemClone = (BalanceItem)item.Clone();
                itemClone.ui_rowItem = new_row;
                balanceSelectedItemList.Add(itemClone);
            }
        }

        private void BssAllGridViewUpdate(string itemCode, int qnt, string state, DataGridViewRow ui_row)
        {
         
                if(ui_row.Cells["bssAll_종목명"].Value.ToString() == axKHOpenAPI1.GetMasterCodeName(itemCode))
                {
                    ui_row.Cells["bssAll_상태"].Value = state;
                    ui_row.Cells["bssAll_수량"].Value = qnt;
                }
            
        }

        private void deleteBssList_Click(object sender, EventArgs e)
        {
            if (bssAllGridView.SelectedRows != null)
            {
                foreach(var itemRow in bssAllGridView.SelectedRows)
                {
                    
                    BalanceItem item = balanceSelectedItemList.Find(o => (o.itemName == ((DataGridViewRow)itemRow).Cells["bssAll_종목명"].Value.ToString()));
                    if (item != null)
                    {
                        balanceSelectedItemList.Remove(item);
                        bssAllGridView.Rows.Remove((DataGridViewRow)itemRow);
                    }
                }
            }
        }

        private BalanceBuyStrategy BalanceBuy(string accountNum, string itemCode, int buyingPrice, int buyQnt, string OrderType, double buyPercent)
        {
            if (accountNum.Length > 0)
            {
                if (itemCode.Length > 0)
                {
                    if (buyQnt > 0)
                    {
                        //매매 전략

                        BalanceBuyStrategy bs = new BalanceBuyStrategy(
                            accountNum,
                            itemCode,
                            buyingPrice,
                            buyQnt,
                            OrderType
                       );

                        balanceStrategyList.Add(bs);

                        int rowIndex = BBSdataGridView.Rows.Add();
                        bs.ui_rowItem = BBSdataGridView.Rows[rowIndex];
                        string codeName = axKHOpenAPI1.GetMasterCodeName(itemCode);
                        Hashtable uiTable = new Hashtable() { { "bbs_종목코드", itemCode }, { "bbs_종목명", codeName}, { "bbs_매수금", (buyingPrice * buyQnt).ToString() }, { "bbs_조건", buyPercent } };
                        UpdateBBSGridView(uiTable, rowIndex);
                        coreEngine.SaveItemLogMessage(itemCode, "잔고 매수! 전략이 입력됬습니다");
                        return bs;
                    }
                    else
                    {
                        MessageBox.Show("매수량은 0보다 커야 합니다.");
                    }
                }
                else
                {
                    MessageBox.Show("매수종목을 선택해주세요");
                }
            }
            else
            {
                MessageBox.Show("계좌를 선택해주세요");
            }
            return null;
        }

        private void AddBBSStrategyBtn_Click(object sender, EventArgs e)
        {
            string itemCode = BBSItemCodeTxt.Text;
            string itemName = BBSItemNameTextbox.Text;

            long buyMoney = (long)BBSValueUpdown.Value;
            double buyPercent = (double)BBSPercentUpdown.Value;

            BalanceItem item = null;
            if(balanceItemList.Find(o=>(o.itemCode == itemCode))!=null)
            {
                item = balanceItemList.Find(o => (o.itemCode == itemCode));
            }
            else
            {
                MessageBox.Show("잔고 데이터를 찾을 수 없습니다");
                return;
            }
            double buyingPrice = (double)(item.buyingPrice) * (1 + (buyPercent * 0.01));

            if (buyMoney <= 0)
            {
                return;
            }
               
            long buyQnt = buyMoney / (int)buyingPrice;
            string accountNum = accountComboBox.Text;

            string orderType = (bbsJijungRadio.Checked) ? ConstName.ORDER_JIJUNGGA : ConstName.ORDER_SIJANGGA;

            BalanceBuy(accountNum, itemCode, Math.Abs((int)buyingPrice), Math.Abs((int)buyQnt), orderType, buyPercent);
        }

        private void BssDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;
            if (e.ColumnIndex == BssDataGridView.Columns["bss_취소"].Index)
            {
                int listIndex = e.RowIndex;
                List<BalanceStrategy> sell_list = balanceStrategyList.FindAll(o => (o.type == BalanceStrategy.BALANCE_STRATEGY_TYPE.SELL));
              
                if (sell_list != null && sell_list.Count > listIndex)
                {
                    BalanceStrategy select_strategy = sell_list[listIndex];
                    select_strategy.bUseStrategy = false;
                    //balanceStrategyList.Remove(select_strategy);
                    //BssDataGridView.Rows.RemoveAt(e.RowIndex);
                    BssDataGridView.Rows[e.RowIndex].Cells["bss_상태"].Value = "취소";
                }
            }
        }

        private void BBSdataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;
            if (e.ColumnIndex == BBSdataGridView.Columns["bbs_취소"].Index)
            {
                int listIndex = e.RowIndex;
                List<BalanceStrategy> buy_list = balanceStrategyList.FindAll(o => (o.type == BalanceStrategy.BALANCE_STRATEGY_TYPE.BUY));
  
                if (buy_list != null && buy_list.Count > listIndex)
                {
                    BalanceStrategy select_strategy = buy_list[listIndex];
                    select_strategy.bUseStrategy = false;
                    //balanceStrategyList.Remove(select_strategy);
                    //BBSdataGridView.Rows.RemoveAt(e.RowIndex);
                    BBSdataGridView.Rows[e.RowIndex].Cells["bbs_상태"].Value = "취소";
                }
            }
        }

        private void KospiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(printForm_kospi == null)
            {
                printForm_kospi = new Form3(axKHOpenAPI1);
                printForm_kospi.FormClosed += KospiFormCloseEventArgs;
            }
             
         
            printForm_kospi.RequestKospi(delegate (string _itemCode)
            {
                printForm_kospi.btn.Click -= new System.EventHandler(printForm_kospi.ChartRequestBtn_Click);
                printForm_kospi.btn.Click += new System.EventHandler(printForm_kospi.KospiChartRequestBtn_Click);
                printForm_kospi.Show();
            }, Form3.CHART_TYPE.MINUTE_5);

        }

        private void KospiFormCloseEventArgs(object sender, FormClosedEventArgs e)
        {
            printForm_kospi = null;
        }

        private void KosdaqToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (printForm_kosdaq == null)
            {
                printForm_kosdaq = new Form3(axKHOpenAPI1);
                printForm_kosdaq.FormClosed += KosdaqFormCloseEventArgs;
            }
               
            printForm_kosdaq.RequestKosdap(delegate (string _itemCode)
            {
                printForm_kosdaq.btn.Click -= new System.EventHandler(printForm_kosdaq.ChartRequestBtn_Click);
                printForm_kosdaq.btn.Click += new System.EventHandler(printForm_kosdaq.KosdaqChartRequestBtn_Click);
                printForm_kosdaq.Show();
            }, Form3.CHART_TYPE.MINUTE_5);
        }

        private void KosdaqFormCloseEventArgs(object sender, FormClosedEventArgs e)
        {
            printForm_kosdaq = null;
        }

        public bool IsKospi(string code)
        {
            foreach(var item in codeArray)
            {
                if (item.Contains(code) || code.Contains(item))
                    return true;
            }
            return false;
        }

        private void dummyStrategyAddBtnClick(object sender, EventArgs e)
        {
            string conditionName = BuyConditionComboBox.Text;
            if (string.IsNullOrEmpty(conditionName))
                return;

            Condition add_condition = new Condition(-1, conditionName +"_dummy_"+ GetRandom().ToString());
       
            BuyConditionComboBox.Items.Add(add_condition.Name);
            M_BuyConditionComboBox.Items.Add(add_condition.Name);
            BuyConditionDoubleComboBox.Items.Add(add_condition.Name);
            listCondition.Add(add_condition);
        }

        private void openSettingFile_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                string file = openFileDialog1.SafeFileName;
                try
                { 
                    LoadSettingRead(file);
                }
                catch (IOException)
                {
                }
            }
            Console.WriteLine(result); // <-- For debugging use.
        }

        private void UseVwmaCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (this.useVwmaCheckBox.Checked == true)
            {
                usingTrailingBuyCheck.Enabled = false;
                orderPecentageCheckBox.Enabled = false;
                useEnvelopeCheckBox.Enabled = false;
                useEnvelope7CheckBox.Enabled = false;
                useEnvelope10CheckBox.Enabled = false;
                useEnvelope15CheckBox.Enabled = false;
            } 
            else
            {
                usingTrailingBuyCheck.Enabled = true;
                orderPecentageCheckBox.Enabled = true;
                useEnvelopeCheckBox.Enabled = true;
                useEnvelope7CheckBox.Enabled = true;
                useEnvelope10CheckBox.Enabled = true;
                useEnvelope15CheckBox.Enabled = true;
            } 
        }

        private void UsingTrailingBuyCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (this.usingTrailingBuyCheck.Checked == true)
            {
                useVwmaCheckBox.Enabled = false;
                useEnvelopeCheckBox.Enabled = false;
                useEnvelope7CheckBox.Enabled = false;
                useEnvelope10CheckBox.Enabled = false;
                useEnvelope15CheckBox.Enabled = false;
            }
            else
            {
                useVwmaCheckBox.Enabled = true;
                useEnvelopeCheckBox.Enabled = true;
                useEnvelope7CheckBox.Enabled = true;
                useEnvelope10CheckBox.Enabled = true;
                useEnvelope15CheckBox.Enabled = true;
            }
        }

        private void OrderPecentageCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (this.orderPecentageCheckBox.Checked == true)
            {
                useVwmaCheckBox.Enabled = false;
                useEnvelopeCheckBox.Enabled = false;
                useEnvelope7CheckBox.Enabled = false;
                useEnvelope10CheckBox.Enabled = false;
                useEnvelope15CheckBox.Enabled = false;
            }
            else
            {
                useVwmaCheckBox.Enabled = true;
                useEnvelopeCheckBox.Enabled = true;
                useEnvelope7CheckBox.Enabled = true;
                useEnvelope10CheckBox.Enabled = true;
                useEnvelope15CheckBox.Enabled = true;
            }
        }

        private void UseEnvelopeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (this.useEnvelopeCheckBox.Checked == true)
            {
                useEnvelope7CheckBox.Enabled = false;
                useEnvelope10CheckBox.Enabled = false;
                useEnvelope15CheckBox.Enabled = false;
                usingTrailingBuyCheck.Enabled = false;
                orderPecentageCheckBox.Enabled = false;
                useVwmaCheckBox.Enabled = false;
            }
            else
            {
                useEnvelope7CheckBox.Enabled = true;
                useEnvelope10CheckBox.Enabled = true;
                useEnvelope15CheckBox.Enabled = true;
                usingTrailingBuyCheck.Enabled = true;
                orderPecentageCheckBox.Enabled = true;
                useVwmaCheckBox.Enabled = true;
            }
        }

        //분할 매도는 구매 당시의 수량에서 분할을 하는 것이므로
        //익절 분할 매도, 손절 분할 매도에서 동일한 수량으로 매도하도록 강제 셋팅한다
        //이유는 익절->손절, 손절->익절등의 전환시에 현재 수량이 바뀐상태에서
        //구매당시의 분할된 값으로 수량을 매도할시 수량이 부족할 수 있기 때문이다 
        //예) 10% 씩 10번 익절분할매도가 9번된상태에서 50%씩 2번 손절매도를 처음 실행할때 수량이 부족함
        private void DivideSellPercentProfit_ValueChanged(object sender, EventArgs e)
        {
            decimal valuePecent = divideSellPercentLoss.Value = divideSellPercentProfit.Value;
            if ((float)valuePecent > 0)
            {
                float sellCount = 100.0f / (float)valuePecent;
                DivideSellCountUpDownProfit.Value = (int)sellCount;
            }
          
        }

        private void DivideSellPercentLoss_ValueChanged(object sender, EventArgs e)
        {
            decimal valuePecent = divideSellPercentProfit.Value = divideSellPercentLoss.Value;
            if ((float)valuePecent > 0)
            {
                float sellCount = 100.0f / (float)valuePecent;
                DivideSellCountUpDown.Value = (int)sellCount;
            }
        }

        private void useEnvelope7CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (this.useEnvelope7CheckBox.Checked == true)
            {
                useEnvelopeCheckBox.Enabled = false;
                useEnvelope10CheckBox.Enabled = false;
                useEnvelope15CheckBox.Enabled = false;
                usingTrailingBuyCheck.Enabled = false;
                orderPecentageCheckBox.Enabled = false;
                useVwmaCheckBox.Enabled = false;
            }
            else
            {
                useEnvelopeCheckBox.Enabled = true;
                useEnvelope10CheckBox.Enabled = true;
                useEnvelope15CheckBox.Enabled = true;
                usingTrailingBuyCheck.Enabled = true;
                orderPecentageCheckBox.Enabled = true;
                useVwmaCheckBox.Enabled = true;
            }
        }

        private void useEnvelope10CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (this.useEnvelope10CheckBox.Checked == true)
            {
                useEnvelopeCheckBox.Enabled = false;
                useEnvelope7CheckBox.Enabled = false;
                useEnvelope15CheckBox.Enabled = false;
                usingTrailingBuyCheck.Enabled = false;
                orderPecentageCheckBox.Enabled = false;
                useVwmaCheckBox.Enabled = false;
            }
            else
            {
                useEnvelopeCheckBox.Enabled = true;
                useEnvelope7CheckBox.Enabled = true;
                useEnvelope15CheckBox.Enabled = true;
                usingTrailingBuyCheck.Enabled = true;
                orderPecentageCheckBox.Enabled = true;
                useVwmaCheckBox.Enabled = true;
            }
        }

        public void AddItemRebuyStrategy(string itemCode)
        {
            coreEngine.SendLogErrorMessage("AddItemRebuyStrategy : "+ AxKHOpenAPI.GetMasterCodeName(itemCode));
            for (int i = 0; i < rebuyStrategyList.Count; ++i)
            {
                coreEngine.SendLogErrorMessage(rebuyStrategyList[i]);
            }
            string getRebuyCondition = string.Empty;
            if (rebuyStockStrategy.ContainsKey(itemCode) == false)
            {
                rebuyStockStrategy.Add(itemCode, new Queue<string>());
                Queue<string> rebuyStrategyQueue = rebuyStockStrategy[itemCode];
               
                //초기화 부분
                for (int i = 0; i < rebuyStrategyList.Count; ++i)
                {
                    rebuyStrategyQueue.Enqueue(rebuyStrategyList[i]);
                }
                getRebuyCondition = rebuyStrategyQueue.Dequeue();
            }
            else
            {
                Queue<string> rebuyStrategyQueue = rebuyStockStrategy[itemCode];
         
                if (rebuyStrategyQueue.Count>0)
                {
                    getRebuyCondition = rebuyStrategyQueue.Dequeue();
                }
            }
            coreEngine.SendLogErrorMessage("rebuy condition:"+ getRebuyCondition);
            if (string.IsNullOrEmpty(getRebuyCondition) == false)
            {
                TradingStrategy ts = tradingStrategyList.Find(o => o.buyCondition != null && o.buyCondition.Name.Equals(getRebuyCondition));
               
                if (ts != null)
                {
                    ts.itemInvestment = ts.itemInvestment + buyPlusMoney;
                    if (ts.remainItemCount == 0)
                    {
                        MessageBox.Show("재구매 매수가능 갯수 초과");
                        return;
                    }

                    TrailingItem trailingItem = trailingList.Find(o => (o.itemCode.Contains(itemCode) && o.strategy.buyCondition.Name == ts.buyCondition.Name));

                    if (trailingItem != null)
                    {
                        MessageBox.Show("진행중인 종목입니다");
                        return;
                    }
                    coreEngine.SendLogErrorMessage(AxKHOpenAPI.GetMasterCodeName(itemCode) + " 재구매 시도 체크");
                    if (CheckCanBuyItem(itemCode) && trailingItem == null)
                    {
                        ts.remainItemCount--; //남을 매수할 종목수-1
                        coreEngine.SendLogErrorMessage(AxKHOpenAPI.GetMasterCodeName(itemCode) + " 재구매 시도 종목 추가 검색명 = " + getRebuyCondition);

                        ts.StrategyConditionReceiveUpdate(itemCode, 0, 0, TRADING_ITEM_STATE.AUTO_TRADING_STATE_SEARCH_AND_CATCH);
                        TryBuyItem(ts, itemCode);
                    }
                }
            }
        }
        public void SettingRebuyCondition(string conditionName, long moneyValue)
        {
            if (string.IsNullOrEmpty(conditionName))
                return;
            if (tradingStrategyList.Find(o => o.buyCondition != null && o.buyCondition.Name.Equals(conditionName)) == null)
                return;

            if (rebuyStrategyList.Contains(conditionName) == false)
            {
                rebuyStrategyList.Add(conditionName);
            }
            else
            {
                return;
            }

            if (rebuyStrategyGridView.InvokeRequired)
            {
                rebuyStrategyGridView.Invoke(new MethodInvoker(delegate ()
                {
                    int rowIndex = rebuyStrategyGridView.Rows.Add();
                    rebuyStrategyGridView["전략명", rowIndex].Value = conditionName;
                }));
            }
            else
            {
                int rowIndex = rebuyStrategyGridView.Rows.Add();
                rebuyStrategyGridView["전략명", rowIndex].Value = conditionName;
            }

            //rebuyCondition = conditionName;
            buyPlusMoney = (long)ReBuyAddMoney.Value;

            if (AddRebuyStrategyBtn.InvokeRequired)
            {
                AddRebuyStrategyBtn.Invoke(new MethodInvoker(delegate ()
                {
                    AddRebuyStrategyBtn.Text = "실행중";
                }));
            }
            else
            {
                AddRebuyStrategyBtn.Text = "실행중";
            }
        }
        private void AddRebuyStrategyBtn_Click(object sender, EventArgs e)
        {
            string conditionName = ReBuyStrategyTextBox.Text;

            if (AddRebuyStrategyBtn.Text == "감시시작")
            {
                //CurrentRebuyText.Text = conditionName;
                //rebuyCondition = conditionName;
                buyPlusMoney = (long)ReBuyAddMoney.Value;
                AddRebuyStrategyBtn.Text = "실행중";
            }
            else
            {
                //CurrentRebuyText.Text = string.Empty;
                //rebuyCondition = string.Empty;
                AddRebuyStrategyBtn.Text = "감시시작";
            }
            SaveLoadManager.GetInstance().SaveAppendSetting();
        }

        private void useEnvelope15CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (this.useEnvelope15CheckBox.Checked == true)
            {
                useEnvelopeCheckBox.Enabled = false;
                useEnvelope7CheckBox.Enabled = false;
                useEnvelope10CheckBox.Enabled = false;
                usingTrailingBuyCheck.Enabled = false;
                orderPecentageCheckBox.Enabled = false;
                useVwmaCheckBox.Enabled = false;
            }
            else
            {
                useEnvelopeCheckBox.Enabled = true;
                useEnvelope7CheckBox.Enabled = true;
                useEnvelope10CheckBox.Enabled = true;
                usingTrailingBuyCheck.Enabled = true;
                orderPecentageCheckBox.Enabled = true;
                useVwmaCheckBox.Enabled = true;
            }
        }

        private void AddRebuyStrategyList_Click(object sender, EventArgs e)
        {
            string conditionName = ReBuyStrategyTextBox.Text;
      
            if (rebuyStrategyList.Contains(conditionName) == false)
            {
                rebuyStrategyList.Add(conditionName);
            }
            else
            {
                return;
            }
            int rowIndex = rebuyStrategyGridView.Rows.Add();
            rebuyStrategyGridView["전략명", rowIndex].Value = conditionName;
        }
    }
}
