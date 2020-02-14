//#define TEST_CONSOLE
using System;
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

        string currentAccount = string.Empty;

        int screenNum = 1000;

        double FEE_RATE = 1;

        List<Condition> listCondition = new List<Condition>();
        List<TradingStrategy> tradingStrategyList = new List<TradingStrategy>();
        List<BalanceSellStrategy> balanceSellStrategyList = new List<BalanceSellStrategy>();

        List<StockItem> stockItemList = new List<StockItem>();//상장종목리스트

        List<TradingItem> tryingOrderList = new List<TradingItem>(); //주문접수시도

        //같은 종목에 대하여 주문이 여러개 들어가도 주문순서대로 응답이 오기 때문에 각각의 리스트로 들어가게됨

        List<SettlementItem> tryingSettlementItemList = new List<SettlementItem>(); //청산 접수 시도(주문번호만 따기위한 리스트)
        List<SettlementItem> settleItemList = new List<SettlementItem>(); //진행중인 청산 시도

        List<BalanceSellStrategy> tryingSellList = new List<BalanceSellStrategy>(); //잔고 매도 접수 시도(주문번호 따는 리스트)

        public tradingStrategyGridView()
        {
            InitializeComponent();
            axKHOpenAPI1.CommConnect();

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
            axKHOpenAPI1.OnReceiveChejanData += API_OnReceiveChejanData; //체결잔고
            axKHOpenAPI1.OnReceiveRealData += API_OnReceiveRealData; //실시간정보
        }

        #region EVENT_RECEIVE_FUNCTION
        private void API_OnEventConnect(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnEventConnectEvent e)
        {
            if (e.nErrCode == 0)
            {
                WriteLog("로그인 성공");
                string server = axKHOpenAPI1.GetLoginInfo(ConstName.GET_SERVER_TYPE);
                if (server.Equals("1"))
                {
                    //모의투자 
                    FEE_RATE = 1;
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
            else
            {
                WriteLog("로그인 실패 " + e.nErrCode.ToString());
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
                interestConditionListBox.Items.Add(condition.Name);
            }
        }
        //검색에 편입시 호출
        private void API_OnReceiveRealCondition(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveRealConditionEvent e)
        {
            string itemCode = e.sTrCode;
            string conditionName = e.strConditionName;
            string conditionIndex = e.strConditionIndex;
            Console.WriteLine("e.sTrType =" + e.strType);
            Console.WriteLine("conditionName = " + conditionName);
            Console.WriteLine("itemCode = " + itemCode);

            if (e.strType.Equals(ConstName.RECEIVE_REAL_CONDITION_INSERTED))
            {
                //종목 편입(어떤 전략(검색식)이었는지)
                TradingStrategy ts = tradingStrategyList.Find(o => o.buyCondition.Name.Equals(conditionName));

                if (ts != null)
                {
                    Console.WriteLine("남은 가능 매수 종목수 : " + ts.remainItemCount);
                    if (ts.remainItemCount > 0)
                    {
                        TradingItem tradeItem = ts.tradingItemList.Find(o => o.itemCode.Contains(itemCode)); //한 전략에서 구매하려했던 종목은 재편입하지 않음
                        if (tradeItem == null)
                        {
                            ts.remainItemCount--; //남을 매수할 종목수-1

                            axKHOpenAPI1.SetInputValue("종목코드", itemCode);
                            axKHOpenAPI1.CommRqData("매수종목정보요청:" + ts.buyCondition.Index, "opt10001", 0, GetScreenNum().ToString());
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
                    int conditionIndex = int.Parse(rqNameArray[1]);
                    TradingStrategy ts = tradingStrategyList.Find(o => o.buyCondition.Index == conditionIndex);

                    if (ts != null)
                    {
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
                                Console.WriteLine("종목 매수 : " + axKHOpenAPI1.GetMasterCodeName(itemcode));

                                int orderResult =

                                axKHOpenAPI1.SendOrder(
                                    "편입종목매수",
                                    GetScreenNum().ToString(),
                                    ts.account,
                                    1,//1:신규매수
                                    itemcode,
                                    (int)(ts.itemInvestment / i_price),
                                    i_price,
                                    "00",//지정가
                                    "" //원주문번호없음
                                );

                                if (orderResult == 0)
                                {
                                    Console.WriteLine("매수주문 성공");

                                    TradingItem tradingItem = new TradingItem(ts, itemcode, i_price, i_qnt);

                                    ts.tradingItemList.Add(tradingItem); //매수전략 내에 매매진행 종목 추가

                                    this.tryingOrderList.Add(tradingItem);

                                    string fidList = "9001;302;10;11;25;12;13"; //9001:종목코드,302:종목명
                                    axKHOpenAPI1.SetRealReg("9001", itemcode, fidList, "1");

                                    //매매진행 데이터 그리드뷰 표시

                                    int addRow = autoTradingDataGrid.Rows.Add();

                                    autoTradingDataGrid["매매진행_진행상황", addRow].Value = ConstName.AUTO_TRADING_STATE_BUY_NOT_COMPLETE;
                                    autoTradingDataGrid["매매진행_종목코드", addRow].Value = itemcode;
                                    autoTradingDataGrid["매매진행_종목명", addRow].Value = axKHOpenAPI1.GetMasterCodeName(itemcode);
                                    autoTradingDataGrid["매매진행_매수조건식", addRow].Value = ts.buyCondition.Name;
                                    autoTradingDataGrid["매매진행_매수금", addRow].Value = i_qnt * i_price;
                                    autoTradingDataGrid["매매진행_매수량", addRow].Value = i_qnt;
                                    autoTradingDataGrid["매매진행_매수가", addRow].Value = i_price;
                                    autoTradingDataGrid["매매진행_매수시간", addRow].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                    WriteLog("자동 매수 요청 - "+ "종목코드 : " + itemcode + " 주문가 : " + i_price + " 주문수량 : " + i_qnt + " 매수조건식 : " + ts.buyCondition.Name);
                                }
                            }
                        }
                        else
                        {
                            Console.Write("현재가 받기 실패");
                        }

                    }
                }
            }
            else if (e.sRQName.Contains(ConstName.RECEIVE_TR_DATA_ACCOUNT_INFO))
            {
                if (accountBalanceDataGrid.DataSource != null)
                {
                    accountBalanceDataGrid.DataSource = null;
                }
                else
                {
                    accountBalanceDataGrid.Rows.Clear();
                }

                string accountName = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "계좌명");
                string bankName = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "지점명");
                string asset = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "예수금");
                string d2Asset = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "D+2추정예수금");
                string estimatedAsset = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "예탁자산평가액");
                string investment = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "당일투자원금");
                string profit = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "당일투자손익");
                string profitRate = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "당일손익율");

                long l_asset = 0;
                long l_d2asset = 0;
                long l_estimatedAsset = 0;
                long l_investment = 0;
                long l_profit = 0;
                double d_profitRate = 0;

                long.TryParse(asset, out l_asset);
                long.TryParse(d2Asset, out l_d2asset);
                long.TryParse(estimatedAsset, out l_estimatedAsset);
                long.TryParse(investment, out l_investment);
                long.TryParse(profit, out l_profit);

                double.TryParse(profitRate, out d_profitRate);

                asset_label.Text = string.Format("{0:n0}", l_asset);
                d2Asset_label.Text = string.Format("{0:n0}", l_d2asset);
                estimatedAsset_label.Text = string.Format("{0:n0}", l_estimatedAsset);
                investment_label.Text = string.Format("{0:n0}", l_investment);
                profit_label.Text = string.Format("{0:n0}", l_profit);

                profitRate_label.Text = d_profitRate.ToString();
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

                    accountBalanceDataGrid["계좌잔고_종목코드", rowIndex].Value = itemCode;
                    accountBalanceDataGrid["계좌잔고_종목명", rowIndex].Value = itemName;
                    accountBalanceDataGrid["계좌잔고_보유수량", rowIndex].Value = lBalanceCnt;
                    accountBalanceDataGrid["계좌잔고_평균단가", rowIndex].Value = dBuyingPrice;
                    accountBalanceDataGrid["계좌잔고_평가금액", rowIndex].Value = lEstimatedAmount;
                    accountBalanceDataGrid["계좌잔고_매입금액", rowIndex].Value = lBuyingAmount;
                    accountBalanceDataGrid["계좌잔고_손익금액", rowIndex].Value = lProfitAmount;
                    accountBalanceDataGrid["계좌잔고_손익률", rowIndex].Value = dProfitRate;

                }
                string fidList = "9001;302;10;11;25;12;13"; //9001:종목코드,302:종목명
                axKHOpenAPI1.SetRealReg("9002", codeList, fidList, "1");
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
                List<TradingItem> tradeItemListAll =  GetAllTradingItemData(itemCode);

                foreach (TradingItem tradeItem in tradeItemListAll)
                {
                    if (tradeItem.IsCompleteBuying && !tradeItem.IsSold && tradeItem.buyingPrice != 0) //매수 완료 되고 매도 진행안된것 
                    {
                        double realProfitRate = GetProfitRate((double)c_lPrice, (double)tradeItem.buyingPrice);
                        if (tradeItem.ts.usingTakeProfit)
                        {
                            if (realProfitRate >= tradeItem.ts.takeProfitRate)
                            {
                                int orderResult = axKHOpenAPI1.SendOrder(
                                    "종목익절매도",
                                    GetScreenNum().ToString(),
                                    tradeItem.ts.account,
                                    2,
                                    itemCode,
                                    tradeItem.buyingQnt,
                                    (int)c_lPrice,
                                    "00",//지정가
                                    "" //원주문번호없음
                                );
                                if (orderResult == 0) //요청 성공시 (실거래는 안될 수 있음)
                                {
                                    this.tryingOrderList.Add(tradeItem);
                                    tradeItem.IsSold = true;

                                    UpdateAutoTradingDataGridRow(itemCode, tradeItem, c_lPrice, ConstName.AUTO_TRADING_STATE_TAKE_PROFIT);
                                    WriteLog("자동 익절 요청 - " + "종목코드 : " + itemCode + " 주문가 : " + c_lPrice + " 주문수량 : " + tradeItem.buyingQnt + " 매수조건식 : " + tradeItem.ts.buyCondition.Name);

                                }
                                else
                                {
                                    WriteLog("자동 익절 요청 실패");
                                }
                            }
                        }
                        if (tradeItem.ts.usingStoploss)
                        {
                            if (realProfitRate <= tradeItem.ts.stoplossRate)
                            {
                                int orderResult = axKHOpenAPI1.SendOrder(
                                        "종목손절매도",
                                        GetScreenNum().ToString(),
                                        tradeItem.ts.account,
                                        2,
                                        itemCode,
                                        tradeItem.buyingQnt,
                                        (int)c_lPrice,
                                        "03",//시장가
                                        "" //원주문번호없음
                                    );
                                if (orderResult == 0) //요청 성공시 (실거래는 안될 수 있음)
                                {
                                    this.tryingOrderList.Add(tradeItem);
                                    tradeItem.IsSold = true;
                                  
                                    UpdateAutoTradingDataGridRow(itemCode, tradeItem, c_lPrice, ConstName.AUTO_TRADING_STATE_STOPLOSS);
                                    WriteLog("자동 손절 요청 - " + "종목코드 : " + itemCode + " 주문가 : " + c_lPrice + " 주문수량 : " + tradeItem.buyingQnt + " 매수조건식 : " + tradeItem.ts.buyCondition.Name);

                                }
                                else
                                {
                                    WriteLog("자동 손절 요청 실패");
                                }
                            }
                        }
                    }
                }
               
                List<BalanceSellStrategy> bssArray = balanceSellStrategyList.FindAll(o => o.itemCode.Equals(itemCode));
                foreach(BalanceSellStrategy bss in bssArray)
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
                                                  2,
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
                                    UpdateAutoTradingDataGridRowSellStrategy(itemCode, ConstName.AUTO_TRADING_STATE_TAKE_PROFIT);
                                }
                                else
                                {
                                    WriteLog("잔고 익절 요청 실패");
                                }
                            }
                            else if (bss.stoplossRate > profitRate) //손절
                            {
                                int orderResult = axKHOpenAPI1.SendOrder(
                                                     "잔고손절매도",
                                                     GetScreenNum().ToString(),
                                                     bss.account,
                                                     2,
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
                                    UpdateAutoTradingDataGridRowSellStrategy(itemCode, ConstName.AUTO_TRADING_STATE_STOPLOSS);

                                }
                                else
                                {
                                    WriteLog("잔고 손절 요청 실패");
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
            foreach (DataGridViewRow row in autoTradingDataGrid.Rows)
            {
                if (row.Cells["매매진행_종목코드"].Value.ToString().Equals(itemCode)
                    && row.Cells["매매진행_매수조건식"].Value.ToString().Equals(tradeItem.ts.buyCondition.Name))
                {
                    row.Cells["매매진행_진행상황"].Value = curState;
                    break;
                }
            }
        }

        private void UpdateAutoTradingDataGridRowSellStrategy(string itemCode, string changeState)
        {
            foreach (DataGridViewRow row in autoTradingDataGrid.Rows)
            {
                if (row.Cells["매매진행_종목코드"].Value != null)
                {
                    if (row.Cells["매매진행_종목코드"].Value.ToString().Contains(itemCode)
                        && row.Cells["매매진행_진행상황"].Value.ToString().Equals(ConstName.AUTO_TRADING_STATE_SELL_MONITORING))
                    {
                        row.Cells["매매진행_진행상황"].Value = changeState;
                    }
                }
            }
        }
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

                        if (row.Cells["매매진행_진행상황"].Value != null 
                            && !row.Cells["매매진행_진행상황"].Value.ToString().Equals(ConstName.AUTO_TRADING_STATE_BUY_COMPLETE))
                        {
                            if (buyingPrice != 0)
                            {
                                double profitRate = GetProfitRate((double)c_lPrice, (double)buyingPrice);
                                row.Cells["매매진행_손익률"].Value = profitRate;
                            }
                        }
                    }
                }
            }
        }
        private void API_OnReceiveChejanData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveChejanDataEvent e)
        {
            Console.WriteLine(e.sGubun);

            if (e.sGubun.Equals(ConstName.RECEIVE_CHEJAN_DATA_SUBMIT_OR_CONCLUSION))
            {
                //접수 혹은 체결

                string account = axKHOpenAPI1.GetChejanData(9201);
                string ordernum = axKHOpenAPI1.GetChejanData(9203);
                string itemCode = axKHOpenAPI1.GetChejanData(9001).Replace("A","");
                string orderState = axKHOpenAPI1.GetChejanData(913);
                string itemName = axKHOpenAPI1.GetChejanData(302).Trim();
                string orderQuantity = axKHOpenAPI1.GetChejanData(900);
                string orderPrice = axKHOpenAPI1.GetChejanData(901);
                string outstanding = axKHOpenAPI1.GetChejanData(902);
                string orderType = axKHOpenAPI1.GetChejanData(905);
                string tradingType = axKHOpenAPI1.GetChejanData(906);
                string time = axKHOpenAPI1.GetChejanData(908);
                string conclusionPrice = axKHOpenAPI1.GetChejanData(910);
                string conclusionQuantity = axKHOpenAPI1.GetChejanData(911);
                string unitConclusionQuantity = axKHOpenAPI1.GetChejanData(915);
                string price = axKHOpenAPI1.GetChejanData(10);

                Console.WriteLine("___________접수/체결_____________");
                Console.WriteLine("주문상태 : " + orderState);
                Console.WriteLine("주문번호 : " + ordernum);
                Console.WriteLine("종목코드 : " + itemCode);
                Console.WriteLine("주문구분 : " + orderType);
                Console.WriteLine("매매구분 : " + tradingType);
                Console.WriteLine("주문수량 : " + orderQuantity);
                Console.WriteLine("체결량(누적체결량) :" + conclusionQuantity);
                Console.WriteLine("미체결 수량 :" + outstanding);
                Console.WriteLine("단위체결량(체결당 체결량) :" + unitConclusionQuantity);
                Console.WriteLine("________________________________");

                if (orderState.Equals(ConstName.RECEIVE_CHEJAN_DATA_SUBMIT))
                {

                    //주문번호, 계좌, 시간, 종목코드 , 종목명, 주문수량, 주문가격, 매도/매수구분, 주문구분

                    //주문번호 따오기 위한 부분
                    TradingItem item = this.tryingOrderList.Find(o => itemCode.Contains(o.itemCode));
                    if (item != null)
                    {
                        if (orderType.Contains(ConstName.RECEIVE_CHEJAN_DATA_BUY))
                        {
                            item.buyOrderNum = ordernum;
                            this.tryingOrderList.Remove(item); //접수리스트에서만 지움

                             WriteLog("자동 매수 요청 - "+ "종목코드 : " + itemCode + " 주문번호 : " + ordernum);
                        }
                        else if (orderType.Contains(ConstName.RECEIVE_CHEJAN_DATA_SELL))
                        {
                            item.sellOrderNum = ordernum;
                            this.tryingOrderList.Remove(item); //접수리스트에서만 지움
                            WriteLog("자동 매도 요청 - " + "종목코드 : " + itemCode + " 주문번호 : " + ordernum);
                        }

                    }
                    else //자동매매에 의한 주문이 아닐때
                    {
                        //보유 아이템 매매인지
                        List<BalanceSellStrategy> bssList = this.tryingSellList.FindAll(o => itemCode.Contains(o.itemCode));
           
                        if (bssList != null && bssList.Count >0)
                        {
                            foreach(BalanceSellStrategy bss in bssList)
                            {
                                if(!bss.orderNum.Equals(ordernum) && bss.sellQnt == long.Parse(orderQuantity))
                                {
                                    bss.orderNum = ordernum;
                                    tryingSellList.Remove(bss);

                                    foreach (DataGridViewRow row in autoTradingDataGrid.Rows)
                                    {
                                        if (row.Cells["매매진행_종목코드"].Value.ToString(). Contains(itemCode)
                                            && row.Cells["매매진행_매도량"].Value.ToString() == bss.sellQnt.ToString()
                                            && row.Cells["매매진행_주문번호"].Value == null)
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
                    orderDataGridView["주문_주문번호", rowIndex].Value = ordernum;
                    orderDataGridView["주문_계좌번호", rowIndex].Value = account;
                    orderDataGridView["주문_시간", rowIndex].Value = time;
                    orderDataGridView["주문_종목코드", rowIndex].Value = itemCode;
                    orderDataGridView["주문_종목명", rowIndex].Value = itemName;
                    orderDataGridView["주문_매매구분", rowIndex].Value = orderType;
                    orderDataGridView["주문_가격구분", rowIndex].Value = tradingType;
                    orderDataGridView["주문_주문량", rowIndex].Value = orderQuantity;
                    orderDataGridView["주문_주문가격", rowIndex].Value = orderPrice;

                    int index = outstandingDataGrid.Rows.Add();
                    outstandingDataGrid["미체결_주문번호", index].Value = ordernum;
                    outstandingDataGrid["미체결_종목코드", index].Value = itemCode;
                    outstandingDataGrid["미체결_종목명", index].Value = itemName;
                    outstandingDataGrid["미체결_주문수량", index].Value = orderQuantity;
                    outstandingDataGrid["미체결_미체결량", index].Value = orderQuantity;
                }
                else if (orderState.Equals(ConstName.RECEIVE_CHEJAN_DATA_CONCLUSION))
                {
                    if (int.Parse(outstanding) == 0)
                    {
                        if (orderType.Contains(ConstName.RECEIVE_CHEJAN_DATA_BUY))
                        {
                            foreach (TradingStrategy ts in tradingStrategyList)
                            {
                                TradingItem tradeItem = ts.tradingItemList.Find(o => o.buyOrderNum.Equals(ordernum));
                                if (tradeItem != null)
                                {
                                    tradeItem.IsCompleteBuying = true;

                                    foreach (DataGridViewRow row in autoTradingDataGrid.Rows)
                                    {
                                        if (row.Cells["매매진행_종목코드"].Value.ToString().Contains(tradeItem.itemCode)
                                            && row.Cells["매매진행_매수조건식"].Value.ToString().Equals(ts.buyCondition.Name))
                                        {
                                            row.Cells["매매진행_진행상황"].Value = ConstName.AUTO_TRADING_STATE_BUY_COMPLETE;
                                            break;
                                        }
                                    }

                                    break;
                                }
                            }
                        }
                        else if (orderType.Contains(ConstName.RECEIVE_CHEJAN_DATA_SELL))
                        {
                            //자동 매매매 진행중일때
                            foreach (TradingStrategy ts in tradingStrategyList)
                            {
                                TradingItem tradeItem = ts.tradingItemList.Find(o => o.sellOrderNum.Equals(ordernum));
                                if (tradeItem != null)
                                {

                                    foreach (DataGridViewRow row in autoTradingDataGrid.Rows)
                                    {
                                        if (row.Cells["매매진행_종목코드"].Value.ToString().Equals(tradeItem.itemName)
                                            && row.Cells["매매진행_매수조건식"].Value.ToString().Equals(ts.buyCondition.Name))
                                        {
                                            row.Cells["매매진행_진행상황"].Value = ConstName.AUTO_TRADING_STATE_SELL_COMPLETE;
                                            row.Cells["매매진행_매도가"].Value = conclusionPrice; 
                                           
                                           
                                            //row.Cells["매매진행_손익률"].Value =;
                                            row.Cells["매매진행_매도시간"].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                            break;
                                        }
                                    }

                                    break;
                                }
                            }
                            //보유잔고 매도
                            BalanceSellStrategy bss = balanceSellStrategyList.Find(o => o.orderNum.Equals(ordernum));
                            if (bss != null)
                            {
                                foreach(DataGridViewRow row in accountBalanceDataGrid.Rows)
                                {
                                    if(row.Cells["계좌잔고_종목코드"].Value != null && row.Cells["계좌잔고_종목코드"].Value.ToString().Contains(bss.itemCode))
                                    {
                                        string qnt = row.Cells["계좌잔고_보유수량"].Value.ToString();
                                        int iQnt = int.Parse(qnt);
                                        iQnt = iQnt - (int)bss.sellQnt;

                                        if(iQnt > 0)
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
                            SettlementItem settlementItem =  settleItemList.Find(o => o.sellOrderNum.Equals(ordernum));
                            if(settlementItem != null)
                            {
                                foreach(DataGridViewRow row in accountBalanceDataGrid.Rows)
                                {
                                    if(row.Cells["계좌잔고_종목코드"].Value != null)
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
                    int rowIndex = conclusionDataGrid.Rows.Add();
                    conclusionDataGrid["체결_주문번호", rowIndex].Value = ordernum;
                    conclusionDataGrid["체결_체결시간", rowIndex].Value = time;
                    conclusionDataGrid["체결_종목코드", rowIndex].Value = itemCode;
                    conclusionDataGrid["체결_종목명", rowIndex].Value = itemName;
                    conclusionDataGrid["체결_주문량", rowIndex].Value = orderQuantity;
                    conclusionDataGrid["체결_단위체결량", rowIndex].Value = unitConclusionQuantity;
                    conclusionDataGrid["체결_누적체결량", rowIndex].Value = conclusionQuantity;
                    conclusionDataGrid["체결_체결가", rowIndex].Value = conclusionPrice;
                    conclusionDataGrid["체결_매매구분", rowIndex].Value = orderType;


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

            }
            else if (e.sGubun.Equals(ConstName.RECEIVE_CHEJAN_DATA_BALANCE))
            {
                //잔고 전달
                string account = axKHOpenAPI1.GetChejanData(9201);
                string itemCode = axKHOpenAPI1.GetChejanData(9001).Replace("A","");
                string itemName = axKHOpenAPI1.GetChejanData(302).Trim();
                string balanceQnt = axKHOpenAPI1.GetChejanData(930);
                string buyingPrice = axKHOpenAPI1.GetChejanData(931);
                string totalBuyingPrice = axKHOpenAPI1.GetChejanData(932);
                string orderAvailableQnt = axKHOpenAPI1.GetChejanData(933);
                string tradingType = axKHOpenAPI1.GetChejanData(946);
                string profitRate = axKHOpenAPI1.GetChejanData(8019);
                string price = axKHOpenAPI1.GetChejanData(10);

                Console.WriteLine("________________잔고_____________");
                Console.WriteLine("종목코드 : " + itemCode);
                Console.WriteLine("보유수량 : " + balanceQnt);
                Console.WriteLine("주문가능수량(매도가능) : " + orderAvailableQnt);
                Console.WriteLine("매수매도구분 :" + tradingType);
                Console.WriteLine("매입단가 :" + buyingPrice);
                Console.WriteLine("총매입가 :" + totalBuyingPrice);
                Console.WriteLine("________________________________");

                //잔고탭 업데이트
                bool hasItem_balanceDataGrid = false;
                foreach (DataGridViewRow row in balanceDataGrid.Rows)
                {
                    if (row.Cells["잔고_종목코드"].Value != null && row.Cells["잔고_종목코드"].Value.ToString().Contains(itemCode))
                    {
                        hasItem_balanceDataGrid = true;

                        if (int.Parse(balanceQnt) > 0)
                        {
                            row.Cells["잔고_보유수량"].Value = balanceQnt;
                            row.Cells["잔고_주문가능수량"].Value = orderAvailableQnt;
                            row.Cells["잔고_매입단가"].Value = buyingPrice;
                            row.Cells["잔고_총매입가"].Value = totalBuyingPrice;
                            row.Cells["잔고_손익률"].Value = profitRate;
                            row.Cells["잔고_현재가"].Value = price;
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
                    balanceDataGrid["잔고_계좌번호", rowIndex].Value = account;
                    balanceDataGrid["잔고_종목코드", rowIndex].Value = itemCode;
                    balanceDataGrid["잔고_종목명", rowIndex].Value = itemName;
                    balanceDataGrid["잔고_보유수량", rowIndex].Value = balanceQnt;
                    balanceDataGrid["잔고_주문가능수량", rowIndex].Value = orderAvailableQnt;
                    balanceDataGrid["잔고_매입단가", rowIndex].Value = buyingPrice;
                    balanceDataGrid["잔고_총매입가", rowIndex].Value = totalBuyingPrice;
                    balanceDataGrid["잔고_손익률", rowIndex].Value = profitRate;
                    balanceDataGrid["잔고_매매구분", rowIndex].Value = tradingType;
                    balanceDataGrid["잔고_현재가", rowIndex].Value = price;
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
                            row.Cells["계좌잔고_보유수량"].Value = balanceQnt;
                            row.Cells["계좌잔고_평균단가"].Value = buyingPrice;
                            row.Cells["계좌잔고_손익률"].Value = profitRate;
                            row.Cells["계좌잔고_현재가"].Value = price;
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
                   
                    accountBalanceDataGrid["계좌잔고_종목코드", rowIndex].Value = itemCode;
                    accountBalanceDataGrid["계좌잔고_종목명", rowIndex].Value = itemName;
                    accountBalanceDataGrid["계좌잔고_보유수량", rowIndex].Value = balanceQnt;
                    accountBalanceDataGrid["계좌잔고_평균단가", rowIndex].Value = buyingPrice;
                    accountBalanceDataGrid["계좌잔고_손익률", rowIndex].Value = profitRate;
                    accountBalanceDataGrid["계좌잔고_현재가", rowIndex].Value = price;
                    accountBalanceDataGrid["계좌잔고_매입금액", rowIndex].Value = totalBuyingPrice;

                    accountBalanceDataGrid["계좌잔고_평가금액", rowIndex].Value = int.Parse(price) * int.Parse(balanceQnt);
                    accountBalanceDataGrid["계좌잔고_손익금액", rowIndex].Value = (int.Parse(price) - int.Parse(buyingPrice)) * int.Parse(balanceQnt);
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
                Console.WriteLine("conditionName = " + conditionName);
                Console.WriteLine("itemCode = " + code);
                Console.WriteLine("strCodeName = " + strCodeName);
            }
        }
        #endregion

        #region UI_EVENT_FUNCTION
        private void DataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            Console.WriteLine("e.ColumnIndex : " + e.ColumnIndex + " e.RowIndex : " + e.RowIndex);
            if (e.RowIndex < 0)
                return;
           if( accountBalanceDataGrid.Columns["계좌잔고_청산"].Index == e.ColumnIndex)
            {
                if (e.ColumnIndex >= 0 && accountBalanceDataGrid.Columns.Count >= e.ColumnIndex)
                {
                    if (accountBalanceDataGrid["계좌잔고_청산", e.RowIndex].Value == null) //최초 생성시는 null값이 들어가 있음
                    {
                        if (accountBalanceDataGrid["계좌잔고_종목코드", e.RowIndex].Value != null)
                        {
                            string itemCode = accountBalanceDataGrid["계좌잔고_종목코드", e.RowIndex].Value.ToString().Replace("A", "");
                            int balanceCnt = int.Parse(accountBalanceDataGrid["계좌잔고_보유수량", e.RowIndex].Value.ToString());

                            int orderResult = axKHOpenAPI1.SendOrder("청산매도주문", GetScreenNum().ToString(), currentAccount, 2, itemCode.Replace("A", ""), balanceCnt, 0, "03", ""); //2:신규매도

                            if (orderResult == 0)
                            {
                                Console.WriteLine("접수 성공");
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
        private void TradingStrategyGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;
            if(e.ColumnIndex == tsDataGridView.Columns["매매전략_취소"].Index)
            {
                string conditionName = tsDataGridView["매매전략_매수조건식", e.RowIndex].Value.ToString();

                TradingStrategy ts = tradingStrategyList.Find(o => o.buyCondition != null && o.buyCondition.Name.Equals(conditionName));
                   
                if(ts != null)
                {
                    DialogResult result = MessageBox.Show(conditionName + "매매조건을 삭제하시겠습니까?", "매매전략 삭제", MessageBoxButtons.YesNo);
                    if(result == DialogResult.Yes)
                    {
                        tradingStrategyList.Remove(ts);
                        tsDataGridView.Rows.RemoveAt(e.RowIndex);
                    }
                }
            }
        }
        private void AutoTradingDataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            Console.WriteLine("e.ColumnIndex : " + e.ColumnIndex + " e.RowIndex : " + e.RowIndex);
            if (e.RowIndex < 0)
                return;
            /*
            if (autoTradingDataGrid.Columns["매매진행_청산"].Index == e.ColumnIndex)
            {
                if (e.ColumnIndex >= 0 && autoTradingDataGrid.Columns.Count >= e.ColumnIndex)
                {
                    if (autoTradingDataGrid["매매진행_청산", e.RowIndex].Value == null) //최초 생성시는 null값이 들어가 있음
                    {
                        if (autoTradingDataGrid["매매진행_종목코드", e.RowIndex].Value != null)
                        {
                            string itemCode = autoTradingDataGrid["매매진행_종목코드", e.RowIndex].Value.ToString().Replace("A", "");
                            int balanceCnt = int.Parse(autoTradingDataGrid["매매진행_매수량", e.RowIndex].Value.ToString());
                            string conditionName = autoTradingDataGrid["매매진행_매수조건식", e.RowIndex].Value.ToString();

                            if (autoTradingDataGrid["매매진행_진행상황", e.RowIndex].Value.ToString().Equals(ConstName.AUTO_TRADING_STATE_BUY_COMPLETE) == false)
                            {
                                MessageBox.Show("매수완료 시에 청산할 수 있습니다");
                                return;
                            }
                                                   
                            TradingStrategy ts = tradingStrategyList.Find(o => o.buyCondition.Name.Equals(conditionName));
                            if (ts != null)
                            {
                                TradingItem tradingItem = ts.tradingItemList.Find(o => o.itemCode.Equals(itemCode));
                                if (tradingItem.IsCompleteBuying && tradingItem.IsSold == false)
                                {
                                    int orderResult = axKHOpenAPI1.SendOrder("청산매도주문", GetScreenNum().ToString(), ts.account, 2, itemCode, balanceCnt, 0, "03", ""); //2:신규매도

                                    if (orderResult == 0)
                                    {
                                        Console.WriteLine("접수 성공");
                                        tradingItem.IsSold = true;
                                        tryingOrderList.Add(tradingItem);
                                        autoTradingDataGrid["매매진행_진행상황", e.RowIndex].Value = ConstName.AUTO_TRADING_STATE_CLEAR_NOT_COMPLETE;
                                        autoTradingDataGrid["매매진행_청산", e.RowIndex].Value = "청산주문접수";


                                    }
                                }
                            }

                        }
                    }

                }
            }
            */
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
        private void ComboBoxIndexChanged (object sender, EventArgs e)
        {
           
          if(sender.Equals(accountComboBox))
            {
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

                        bool usingProfitCheckBox =  b_ProfitSellCheckBox.Checked; //익절사용
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
                        if(rowData != null && rowData.Cells["계좌잔고_보유수량"] != null)
                        {
                            long balanceQnt = long.Parse(rowData.Cells["계좌잔고_보유수량"].Value.ToString());
                             autoTradingDataGrid["매매진행_매수량", rowIndex].Value = balanceQnt;
                        }
                        
                        autoTradingDataGrid["매매진행_매수가", rowIndex].Value = buyingPrice;
                        autoTradingDataGrid["매매진행_매수조건식", rowIndex].Value = "잔고자동매도"; //매수조건식이 없으므로 해당명으로 지정

                        Console.WriteLine("전략이 입력됬습니다");
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

            if(account.Length == 0)
            {
                MessageBox.Show("계좌를 선택해주세요");
                return;
            }

            string conditionName = BuyConditionComboBox.Text;
            Condition findCondition = null;

            if (conditionName.Length > 0)
            {
                findCondition = listCondition.Find(o => o.Name.Equals(conditionName));
            }
            else
            {
                MessageBox.Show("매수 조건식을 선택해주세요");
                return;
            }

            if(findCondition == null)
            {
                return;
            }

            string buyOrderOpt = "지정가";
            long totalInvestment = 0;
            int itemCount = 0;

            if (marketPriceRadio.Checked)
            {
                buyOrderOpt = "시장가";
            }else
            {
                buyOrderOpt = buyOrderOptionCombo.Text;
            }

            if(allCostUpDown.Value == 0)
            {
                MessageBox.Show("총 투자금액을 설정해주세요");
                return;
            }
            
            totalInvestment = (long)allCostUpDown.Value;
            itemCount = (int)itemCountUpdown.Value;
           
            //매매 전략

            bool usingProfitCheckBox = profitSellCheckBox.Checked; //익절사용
            double takeProfitRate = 0;
            string sellOrderOption = "시장가";

            if(usingProfitCheckBox)
            {
               takeProfitRate = (double)profitSellUpdown.Value;
            }

            bool usingStopLoss = minusSellCheckBox.Checked; //손절사용

            double stopLossRate = 0;

            if (usingStopLoss)
            {
                stopLossRate = (double)minusSellUpdown.Value;
            }

            TradingStrategy ts = new TradingStrategy(
                account, 
                findCondition, 
                buyOrderOpt, 
                totalInvestment, 
                itemCount,
                usingProfitCheckBox,
                takeProfitRate,
                sellOrderOption,
                usingStopLoss,
                stopLossRate
                );

            tradingStrategyList.Add(ts);
            AddStrategyToDataGridView(ts);

            StartMonitoring(ts.buyCondition);

           WriteLog("전략이 입력됬습니다 \n 매수조건식 : " + ts.buyCondition.Name + "\n"+ " 총투자금 : " + ts.totalInvestment + "\n" + " 종목수 : " + ts.buyItemCount);
        }

        private void AccountDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if(accountBalanceDataGrid.SelectedRows.Count>0)
            {
                int rowIndex = accountBalanceDataGrid.SelectedRows[0].Index;
                
                if(accountBalanceDataGrid["계좌잔고_종목코드", rowIndex].Value != null)
                {
                    string itemCode = accountBalanceDataGrid["계좌잔고_종목코드", rowIndex].Value.ToString();
                    string itemName = accountBalanceDataGrid["계좌잔고_종목명", rowIndex].Value.ToString();
                    long balanceQnt = long.Parse(accountBalanceDataGrid["계좌잔고_보유수량", rowIndex].Value.ToString());
                    double buyingPrice = double.Parse( accountBalanceDataGrid["계좌잔고_평균단가", rowIndex].Value.ToString());

                    balanceItemCodeTxt.Text = itemCode.Replace("A","");
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
        private void Test_btn_Click(object sender, EventArgs e)
        {
            
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
            Form2 printForm = new Form2(axKHOpenAPI1);
            printForm.Show();
        }

        #endregion
        private void StartMonitoring(Condition _condition)
        {
            int result = axKHOpenAPI1.SendCondition(GetScreenNum().ToString(), _condition.Name, _condition.Index, 1);
            if(result==1)
            {
                Console.WriteLine("감시요청 성공");
            }
            else
            {
                Console.WriteLine("감시요청 실패");
            }
        }

        private int GetScreenNum()
        {
            screenNum++;

            if (screenNum > 5000)
                screenNum = 1000;
            
            return screenNum;
        }

        public double GetProfitRate(double curPrice , double buyPrice)
        {
            if (buyPrice <= 0)
                return 0;
            return (double)100 * ((curPrice - buyPrice) / buyPrice) - FEE_RATE;
        }

        private void AddStrategyToDataGridView(TradingStrategy tradingStrategy)
        {
            if(tradingStrategy != null)
            {
               int rowIndex = tsDataGridView.Rows.Add();
                tsDataGridView["매매전략_계좌번호", rowIndex].Value = tradingStrategy.account;
                tsDataGridView["매매전략_매수조건식", rowIndex].Value = tradingStrategy.buyCondition.Name;
                tsDataGridView["매매전략_매수가격", rowIndex].Value = tradingStrategy.buyOrderOption;
                tsDataGridView["매매전략_총투자금", rowIndex].Value = tradingStrategy.totalInvestment;
                tsDataGridView["매매전략_매수종목수", rowIndex].Value = tradingStrategy.buyItemCount;
                tsDataGridView["매매전략_종목별투자금", rowIndex].Value = tradingStrategy.itemInvestment;

                tsDataGridView["매매전략_익절", rowIndex].Value = tradingStrategy.usingTakeProfit;
                tsDataGridView["매매전략_익절률", rowIndex].Value = tradingStrategy.takeProfitRate;
                tsDataGridView["매매전략_손절", rowIndex].Value = tradingStrategy.usingStoploss;
                tsDataGridView["매매전략_손절률", rowIndex].Value = tradingStrategy.stoplossRate;
            }
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
        private void WriteLog(string log)
        {
            string filePath =  DateTime.Now.ToString("yyyyMMdd") + "_log.txt";
            FileInfo fi = new FileInfo(filePath);

            try
            {
                if(fi.Exists)
                {
                    using (StreamWriter sw = File.AppendText (filePath))
                    {
                        sw.WriteLine("[{0}] - {1} ", DateTime.Now.ToString("HH:mm:ss"), log);
                    }
                }
                else
                {
                    using (StreamWriter sw = new StreamWriter(filePath))
                    {
                        sw.WriteLine("[{0}] - {1} ", DateTime.Now.ToString("HH:mm:ss"), log);
                    }
                }
               
            }catch(Exception e){
                Console.WriteLine(e.Message);
            }
        }

       
    }
}
