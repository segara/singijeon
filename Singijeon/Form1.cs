#define TEST_CONSOLE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
#if TEST_CONSOLE
        double FEE_RATE = 1;
#else
         double FEE_RATE = 0.33;
#endif
        List<Condition> listCondition = new List<Condition>();
        List<TradingStrategy> tradingStrategyList = new List<TradingStrategy>();

        List<TradingItem> tryingOrderList = new List<TradingItem>(); //주문접수시도


        public tradingStrategyGridView()
        {
            InitializeComponent();

            LogInToolStripMenuItem.Click += ToolStripMenuItem_Click;

            accountComboBox.SelectedIndexChanged += ComboBoxIndexChanged;

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
            if(e.nErrCode == 0) 
            {
                Console.WriteLine("로그인 성공");
                string server = axKHOpenAPI1.GetLoginInfo("GetServerGubun");
                if(server.Equals("1"))
                {
                    //모의투자 
                    FEE_RATE = 1;
                }
                else
                {
                    FEE_RATE = 0.33;
                }

                string accountList = axKHOpenAPI1.GetLoginInfo("ACCLIST");
                string[] accountArray = accountList.Split(';');

                foreach(string accountItem in accountArray)
                {
                    if(accountItem.Length > 0)
                    {
                        accountComboBox.Items.Add(accountItem);
                    }
                }
                //사용자 조건식 불러오기
                axKHOpenAPI1.GetConditionLoad();
            }
            else
            {
                MessageBox.Show("로그인 실패 " + e.nErrCode.ToString());
            }
        }
        private void API_OnReceiveConditionVer(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveConditionVerEvent e)
        {

            string conditionList = axKHOpenAPI1.GetConditionNameList();
            string[] conditionArray = conditionList.Split(';');

            listCondition.Clear();

            foreach (string conditionItem in conditionArray)
            {
                if(conditionItem.Length>0)
                {
                    string[] conditionInfo = conditionItem.Split('^');

                    if(conditionInfo.Length == 2)
                    {
                        string index = conditionInfo[0];
                        string name = conditionInfo[1];

                        Condition condition = new Condition(int.Parse(index), name);
                        listCondition.Add(condition);
                    }
                }
            }

            foreach(Condition condition in listCondition)
            {
                BuyConditionComboBox.Items.Add(condition.Name);
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

            if (e.strType.Equals("I"))
            {
                //종목 편입(어떤 전략(검색식)이었는지)
                TradingStrategy ts = tradingStrategyList.Find(o => o.buyCondition.Name.Equals(conditionName));

                if (ts != null)
                {
                    Console.WriteLine("남은 가능 매수 종목수 : " + ts.remainItemCount);
                    if (ts.remainItemCount > 0)
                    {
                        TradingItem tradeItem = ts.tradingItemList.Find(o => o.itemCode.Contains(itemCode)); //한 전략에서 구매하려했던 종목은 재편입하지 않음
                        if(tradeItem == null)
                        {
                            ts.remainItemCount--; //남을 매수할 종목수-1

                            axKHOpenAPI1.SetInputValue("종목코드", itemCode);
                            axKHOpenAPI1.CommRqData("매수종목정보요청:" + ts.buyCondition.Index, "opt10001", 0, GetScreenNum().ToString());
                        }
                    }
                }

            }
            else if (e.strType.Equals("D"))
            {
                //종목 이탈
            }
        }
        private void API_OnReceiveTrData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            if (e.sRQName.Contains("매수종목정보요청"))
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

                                    TradingItem tradingItem = new TradingItem(itemcode, i_price, i_qnt);

                                    ts.tradingItemList.Add(tradingItem); //매수전략 내에 매매진행 종목 추가

                                    this.tryingOrderList.Add(tradingItem);

                                    string fidList = "9001;302;10;11;25;12;13"; //9001:종목코드,302:종목명
                                    axKHOpenAPI1.SetRealReg("9001", itemcode, fidList, "1");

                                    //매매진행 데이터 그리드뷰 표시

                                    int addRow = autoTradingDataGrid.Rows.Add();

                                    autoTradingDataGrid["매매진행_진행상황", addRow].Value = "매수중";
                                    autoTradingDataGrid["매매진행_종목코드", addRow].Value = itemcode;
                                    autoTradingDataGrid["매매진행_종목명", addRow].Value = axKHOpenAPI1.GetMasterCodeName(itemcode);
                                    autoTradingDataGrid["매매진행_매수조건식", addRow].Value = ts.buyCondition.Name;
                                    autoTradingDataGrid["매매진행_매수금", addRow].Value = i_qnt * i_price;
                                    autoTradingDataGrid["매매진행_매수량", addRow].Value = i_qnt;
                                    autoTradingDataGrid["매매진행_매수가", addRow].Value = i_price;
                                    autoTradingDataGrid["매매진행_매수시간", addRow].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
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
            else if (e.sRQName.Contains("계좌평가현황요청"))
            {
               string accountName = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "계좌명");
               string bankName = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "지점명");
               string asset = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "예수금");
               string d2Asset =  axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "D+2추정예수금");
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

                asset_label.Text = string.Format("{0:n0}",l_asset);
                d2Asset_label.Text = string.Format("{0:n0}", l_d2asset);
                estimatedAsset_label.Text = string.Format("{0:n0}", l_estimatedAsset);
                investment_label.Text = string.Format("{0:n0}", l_investment);
                profit_label.Text = string.Format("{0:n0}", l_profit);

                profitRate_label.Text = d_profitRate.ToString();
                string codeList = string.Empty;
                int cnt = axKHOpenAPI1.GetRepeatCnt(e.sTrCode, e.sRQName); //조회내용중 멀티데이터의 갯수를 알아온다
                for(int i = 0; i < cnt; ++i)
                {
                    string itemCode = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "종목코드").Trim();
                    codeList += itemCode;
                    if(i != cnt-1)
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

                    int rowIndex = balanceDataGrid.Rows.Add();

                    double dProfitRate = 100*((iPrice - dBuyingPrice) / dBuyingPrice) - FEE_RATE;
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

            if (e.sRealType == "주식체결") //주식이 체결될 때 마다 실시간 데이터를 받음
            {
                string price = axKHOpenAPI1.GetCommRealData(itemCode, 10);    //현재가
                string lowPrice = axKHOpenAPI1.GetCommRealData(itemCode, 18); //저가
                string openPrice = axKHOpenAPI1.GetCommRealData(itemCode, 16); //시가

                long c_lPrice = Math.Abs(long.Parse(price));

                //종목의 매매전략 얻어오기
                //모든 매매전략내 전략에 포함된 종목을 찾고, 매매전략의 손익률 셋팅과 비교
                
                foreach(TradingStrategy ts in tradingStrategyList)
                {
                    List<TradingItem> tradeItemList = ts.tradingItemList.FindAll(o => o.itemCode.Equals(itemCode));
                     
                    if(tradeItemList != null && tradeItemList.Count>0) //매매 진행 종목을 찾았을 경우
                    {
                        foreach(TradingItem tradeItem in tradeItemList)
                        {
                            if (tradeItem.IsCompleteBuying && !tradeItem.IsSold) //매수 완료 되고 매도 진행안된것 
                            {
                                if(tradeItem.buyingPrice != 0)
                                {
                                    double realProfitRate = (100 * ((c_lPrice - tradeItem.buyingPrice) / (double)tradeItem.buyingPrice)) - FEE_RATE;
                                    if (ts.usingTakeProfit)
                                    {
                                        if (realProfitRate >= ts.takeProfitRate)
                                        {
                                            int orderResult = axKHOpenAPI1.SendOrder(
                                                "종목익절매도",
                                                GetScreenNum().ToString(),
                                                ts.account,
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

                                                foreach(DataGridViewRow row in autoTradingDataGrid.Rows)
                                                {
                                                    if(row.Cells["매매진행_종목코드"].Value.ToString().Equals(itemCode) 
                                                        && row.Cells["매매진행_매수조건식"].Value.ToString().Equals(ts.buyCondition.Name))
                                                    {
                                                        row.Cells["매매진행_진행상황"].Value = "매도중";
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    if (ts.usingStoploss)
                                    {
                                        if (realProfitRate <= ts.takeProfitRate)
                                        {
                                            int orderResult = axKHOpenAPI1.SendOrder(
                                                 "종목손절매도",
                                                 GetScreenNum().ToString(),
                                                 ts.account,
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
                                                foreach (DataGridViewRow row in autoTradingDataGrid.Rows)
                                                {
                                                    if (row.Cells["매매진행_종목코드"].Value.ToString().Equals(itemCode)
                                                        && row.Cells["매매진행_매수조건식"].Value.ToString().Equals(ts.buyCondition.Name))
                                                    {
                                                        row.Cells["매매진행_진행상황"].Value = "매도중";
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                       
                            }
                        }
                     
                    }
                }

                foreach (DataGridViewRow row in accountBalanceDataGrid.Rows)
                {
                    if (row.Cells["계좌잔고_종목코드"].Value != null)
                    {
                        if (row.Cells["계좌잔고_종목코드"].Value.ToString().Contains(itemCode))
                        {
                            row.Cells["계좌잔고_현재가"].Value = c_lPrice;

                            double buyingPrice = double.Parse(row.Cells["계좌잔고_평균단가"].Value.ToString());

                            if (buyingPrice != 0)
                            {
                                double profitRate = 100 * (c_lPrice - buyingPrice) / buyingPrice - FEE_RATE;
                                row.Cells["계좌잔고_손익률"].Value = profitRate;
                             
                            }

                        }
                    }
                }

                foreach (DataGridViewRow row in balanceDataGrid.Rows)
                {
                    if(row.Cells["잔고_종목코드"].Value != null)
                    {
                        if(row.Cells["잔고_종목코드"].Value.ToString().Contains(itemCode))
                        {
                            row.Cells["잔고_현재가"].Value = c_lPrice;
                           
                            double buyingPrice = double.Parse(row.Cells["잔고_매입단가"].Value.ToString());

                            if(buyingPrice != 0)
                            {
                                double profitRate = 100 * (c_lPrice - buyingPrice) / buyingPrice - FEE_RATE;
                                row.Cells["잔고_손익률"].Value = profitRate;
                            }
                      
                        }
                    }
                }

                foreach (DataGridViewRow row in autoTradingDataGrid.Rows)
                {
                    if (row.Cells["매매진행_종목코드"].Value != null)
                    {
                        if (row.Cells["매매진행_종목코드"].Value.ToString().Contains(itemCode))
                        {
                            double buyingPrice = double.Parse(row.Cells["매매진행_매수가"].Value.ToString());
                            row.Cells["매매진행_현재가"].Value = c_lPrice;
                            if (row.Cells["매매진행_진행상황"].Value != null && !row.Cells["매매진행_진행상황"].Value.ToString().Equals("매매완료"))
                            {
                                if (buyingPrice != 0)
                                {
                                    double profitRate = 100 * (c_lPrice - buyingPrice) / buyingPrice - FEE_RATE;
                                    row.Cells["매매진행_손익률"].Value = profitRate;
                                }
                            }
                        }
                    }
                }

            }
        }
        private void API_OnReceiveChejanData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveChejanDataEvent e)
        {
            Console.WriteLine(e.sGubun);

            if(e.sGubun.Equals("0"))
            {
                //접수 혹은 체결

                string account = axKHOpenAPI1.GetChejanData(9201);
                string ordernum = axKHOpenAPI1.GetChejanData(9203);
                string itemCode = axKHOpenAPI1.GetChejanData(9001);
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

                if(orderState.Equals("접수"))
                {

                    //주문번호, 계좌, 시간, 종목코드 , 종목명, 주문수량, 주문가격, 매도/매수구분, 주문구분

                    //주문번호 따오기 위한 부분
                    TradingItem item = this.tryingOrderList.Find(o => itemCode.Contains(o.itemCode));
                    if(item != null)
                    {
                        if (orderType.Contains("매수"))
                        {
                            item.buyOrderNum = ordernum;
                            this.tryingOrderList.Remove(item); //접수리스트에서만 지움
                        }
                        else if (orderType.Contains("매도"))
                        {
                            item.sellOrderNum = ordernum;
                            this.tryingOrderList.Remove(item); //접수리스트에서만 지움
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
                    balanceDataGrid["미체결_주문번호", index].Value = ordernum;
                    balanceDataGrid["미체결_종목코드", index].Value = itemCode;
                    balanceDataGrid["미체결_종목명", index].Value = itemName;
                    balanceDataGrid["미체결_주문수량", index].Value = orderQuantity;
                    balanceDataGrid["미체결_미체결량", index].Value = orderQuantity;
                }
                else if(orderState.Equals("체결"))
                {
                    if(int.Parse(outstanding)==0)
                    {
                        if (orderType.Contains("매수"))
                        {
                            foreach (TradingStrategy ts in tradingStrategyList)
                            {
                                TradingItem tradeItem = ts.tradingItemList.Find(o => o.buyOrderNum.Equals(ordernum));
                                if (tradeItem != null)
                                {
                                    tradeItem.IsCompleteBuying = true;
                                    
                                    foreach (DataGridViewRow row in autoTradingDataGrid.Rows)
                                    {
                                        if (row.Cells["매매진행_종목코드"].Value.ToString().Equals(tradeItem.itemName)
                                            && row.Cells["매매진행_매수조건식"].Value.ToString().Equals(ts.buyCondition.Name))
                                        {
                                            row.Cells["매매진행_진행상황"].Value = "매수완료";
                                            break;
                                        }
                                    }

                                    break;
                                }
                            }
                        }
                        else if(orderType.Contains("매도"))
                        {
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
                                            row.Cells["매매진행_진행상황"].Value = "매매완료";
                                            row.Cells["매매진행_매도가"].Value = conclusionPrice;
                                            //row.Cells["매매진행_손익률"].Value =;
                                            row.Cells["매매진행_매도시간"].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                            break;
                                        }
                                    }

                                    break;
                                }
                            }
                        }
                    }
                    int rowIndex = conclusionDataGrid.Rows.Add();
                    orderDataGridView["체결_주문번호", rowIndex].Value = ordernum;
                    orderDataGridView["체결_체결시간", rowIndex].Value = time;
                    orderDataGridView["체결_종목코드", rowIndex].Value = itemCode;
                    orderDataGridView["체결_종목명", rowIndex].Value = itemName;
                    orderDataGridView["체결_주문량", rowIndex].Value = orderQuantity;
                    orderDataGridView["체결_단위체결량", rowIndex].Value = unitConclusionQuantity;
                    orderDataGridView["체결_누적체결량", rowIndex].Value = conclusionQuantity;
                    orderDataGridView["체결_체결가", rowIndex].Value = conclusionPrice;
                    orderDataGridView["체결_매매구분", rowIndex].Value = orderType;

                   
                    foreach (DataGridViewRow row in outstandingDataGrid.Rows)
                    {
                        if (row.Cells["미체결_주문번호"].Value != null && row.Cells["미체결_주문번호"].Value.ToString().Equals(itemCode))
                        {
                            
                            row.Cells["미체결_미체결량"].Value = outstanding;
                            if(int.Parse(outstanding)==0)
                            {
                                outstandingDataGrid.Rows.Remove(row);
                            }
                            break;
                        }
                    }
                
                }

            }
            else if(e.sGubun.Equals("1"))
            {
                //잔고 전달
                string account = axKHOpenAPI1.GetChejanData(9201);
                string itemCode = axKHOpenAPI1.GetChejanData(9001);
                string itemName = axKHOpenAPI1.GetChejanData(302).Trim();
                string balanceQnt = axKHOpenAPI1.GetChejanData(930);
                string buyingPrice = axKHOpenAPI1.GetChejanData(931);
                string totalBuyingPrice = axKHOpenAPI1.GetChejanData(932);
                string orderAvailableQnt = axKHOpenAPI1.GetChejanData(933);
                string tradingType = axKHOpenAPI1.GetChejanData(946);
                string profitRate = axKHOpenAPI1.GetChejanData(819);
                string price = axKHOpenAPI1.GetChejanData(10);

                Console.WriteLine("________________잔고_____________");
                Console.WriteLine("종목코드 : " + itemCode);
                Console.WriteLine("보유수량 : " + balanceQnt);
                Console.WriteLine("주문가능수량(매도가능) : " + orderAvailableQnt);
                Console.WriteLine("매수매도구분 :" + tradingType);
                Console.WriteLine("매입단가 :" + buyingPrice);
                Console.WriteLine("총매입가 :" + totalBuyingPrice);
                Console.WriteLine("________________________________");

                bool hasItem = false;
                foreach(DataGridViewRow row in balanceDataGrid.Rows)
                {
                    if(row.Cells["잔고_종목코드"].Value != null && row.Cells["잔고_종목코드"].Value.ToString().Equals(itemCode))
                    {
                        hasItem = true;

                        if(int.Parse(balanceQnt)>0)
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

                if(!hasItem)
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
                    balanceDataGrid["잔고_현재가",rowIndex].Value = price;
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
           foreach(string code in codeList)
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
        private void ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            axKHOpenAPI1.CommConnect();
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
                    axKHOpenAPI1.CommRqData("계좌평가현황요청", "OPW00004", 0, GetScreenNum().ToString());

                    currentAccount = account;
                }
            
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
            //매도 전략

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

            //매수전략

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

            //매수 조건식 감시 시작
            StartMonitoring(ts.buyCondition);

            Console.WriteLine("전략이 입력됬습니다");
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

        private void balanceDataGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
