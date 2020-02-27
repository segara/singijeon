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
        public void Update_OutStandingDataGrid_UI(Hashtable table, int rowIndex)
        {
            if (table.ContainsKey("미체결_주문번호"))
                outstandingDataGrid["미체결_주문번호", rowIndex].Value = table["미체결_주문번호"];
            if (table.ContainsKey("미체결_종목코드"))
                outstandingDataGrid["미체결_종목코드", rowIndex].Value = table["미체결_종목코드"];
            if (table.ContainsKey("미체결_종목명"))
                outstandingDataGrid["미체결_종목명", rowIndex].Value = table["미체결_종목명"];
            if (table.ContainsKey("미체결_주문수량"))
                outstandingDataGrid["미체결_주문수량", rowIndex].Value = table["미체결_주문수량"];
            if (table.ContainsKey("미체결_미체결량"))
                outstandingDataGrid["미체결_미체결량", rowIndex].Value = table["미체결_미체결량"];
        }

        public void Update_OrderDataGrid_UI(Hashtable table, int rowIndex)
        {
            if (table.ContainsKey("주문_주문번호"))
                orderDataGridView["주문_주문번호", rowIndex].Value = table["주문_주문번호"];
            if (table.ContainsKey("주문_계좌번호"))
                orderDataGridView["주문_계좌번호", rowIndex].Value = table["주문_계좌번호"];
            if (table.ContainsKey("주문_시간"))
                orderDataGridView["주문_시간", rowIndex].Value = table["주문_시간"];
            if (table.ContainsKey("주문_종목코드"))
                orderDataGridView["주문_종목코드", rowIndex].Value = table["주문_종목코드"];
            if (table.ContainsKey("주문_종목명"))
                orderDataGridView["주문_종목명", rowIndex].Value = table["주문_종목명"];
            if (table.ContainsKey("주문_매매구분"))
                orderDataGridView["주문_매매구분", rowIndex].Value = table["주문_매매구분"];
            if (table.ContainsKey("주문_가격구분"))
                orderDataGridView["주문_가격구분", rowIndex].Value = table["주문_가격구분"];
            if (table.ContainsKey("주문_주문량"))
                orderDataGridView["주문_주문량", rowIndex].Value = table["주문_주문량"];
            if (table.ContainsKey("주문_주문가격"))
                orderDataGridView["주문_주문가격", rowIndex].Value = table["주문_주문가격"];
        }
        public void Update_ConclusionDataGrid_UI(Hashtable table, int rowIndex)
        {
            if (table.ContainsKey("체결_주문번호"))
                conclusionDataGrid["체결_주문번호", rowIndex].Value = table["체결_주문번호"];
            if (table.ContainsKey("체결_체결시간"))
                conclusionDataGrid["체결_체결시간", rowIndex].Value = table["체결_체결시간"];
            if (table.ContainsKey("체결_종목코드"))
                conclusionDataGrid["체결_종목코드", rowIndex].Value = table["체결_종목코드"];
            if (table.ContainsKey("체결_종목명"))
                conclusionDataGrid["체결_종목명", rowIndex].Value = table["체결_종목명"];
            if (table.ContainsKey("체결_주문량"))
                conclusionDataGrid["체결_주문량", rowIndex].Value = table["체결_주문량"];
            if (table.ContainsKey("체결_단위체결량"))
                conclusionDataGrid["체결_단위체결량", rowIndex].Value = table["체결_단위체결량"];
            if (table.ContainsKey("체결_누적체결량"))
                conclusionDataGrid["체결_누적체결량", rowIndex].Value = table["체결_누적체결량"];
            if (table.ContainsKey("체결_체결가"))
                conclusionDataGrid["체결_체결가", rowIndex].Value = table["체결_체결가"];
            if (table.ContainsKey("체결_매매구분"))
                conclusionDataGrid["체결_매매구분", rowIndex].Value = table["체결_매매구분"];
        }

        public void Update_AccountBalanceDataGrid_UI(Hashtable table, int rowIndex)
        {
            if (table.ContainsKey("계좌잔고_종목코드"))
                accountBalanceDataGrid["계좌잔고_종목코드", rowIndex].Value = table["계좌잔고_종목코드"];
            if (table.ContainsKey("계좌잔고_종목명"))
                accountBalanceDataGrid["계좌잔고_종목명", rowIndex].Value = table["계좌잔고_종목명"];
            if (table.ContainsKey("계좌잔고_보유수량"))
                accountBalanceDataGrid["계좌잔고_보유수량", rowIndex].Value = table["계좌잔고_보유수량"];
            if (table.ContainsKey("계좌잔고_평균단가"))
                accountBalanceDataGrid["계좌잔고_평균단가", rowIndex].Value = table["계좌잔고_평균단가"];
            if (table.ContainsKey("계좌잔고_평가금액"))
                accountBalanceDataGrid["계좌잔고_평가금액", rowIndex].Value = table["계좌잔고_평가금액"];
            if (table.ContainsKey("계좌잔고_매입금액"))
                accountBalanceDataGrid["계좌잔고_매입금액", rowIndex].Value = table["계좌잔고_매입금액"];
            if (table.ContainsKey("계좌잔고_손익금액"))
                accountBalanceDataGrid["계좌잔고_손익금액", rowIndex].Value = table["계좌잔고_손익금액"];
            if (table.ContainsKey("계좌잔고_손익률"))
                accountBalanceDataGrid["계좌잔고_손익률", rowIndex].Value = table["계좌잔고_손익률"];
            if (table.ContainsKey("계좌잔고_현재가"))
                accountBalanceDataGrid["계좌잔고_현재가", rowIndex].Value = table["계좌잔고_현재가"];
        }
        public void Update_BalanceDataGrid_UI(Hashtable table , int rowIndex)
        {
            if(table.ContainsKey("잔고_계좌번호"))
                balanceDataGrid["잔고_계좌번호", rowIndex].Value = table["잔고_계좌번호"];
            if (table.ContainsKey("잔고_종목코드"))
                balanceDataGrid["잔고_종목코드", rowIndex].Value = table["잔고_종목코드"];
            if (table.ContainsKey("잔고_종목명"))
                balanceDataGrid["잔고_종목명", rowIndex].Value = table["잔고_종목명"];
            if (table.ContainsKey("잔고_보유수량"))
                balanceDataGrid["잔고_보유수량", rowIndex].Value = table["잔고_보유수량"];
            if (table.ContainsKey("잔고_주문가능수량"))
                balanceDataGrid["잔고_주문가능수량", rowIndex].Value = table["잔고_주문가능수량"];
            if (table.ContainsKey("잔고_매입단가"))
                balanceDataGrid["잔고_매입단가", rowIndex].Value = table["잔고_매입단가"];
            if (table.ContainsKey("잔고_총매입가"))
                balanceDataGrid["잔고_총매입가", rowIndex].Value = table["잔고_총매입가"];
            if (table.ContainsKey("잔고_손익률"))
                balanceDataGrid["잔고_손익률", rowIndex].Value = table["잔고_손익률"];
            if (table.ContainsKey("잔고_매매구분"))
                balanceDataGrid["잔고_매매구분", rowIndex].Value = table["잔고_매매구분"];
            if (table.ContainsKey("잔고_현재가"))
                balanceDataGrid["잔고_현재가", rowIndex].Value = table["잔고_현재가"];
        }

        private void AddStrategyToDataGridView(TradingStrategy tradingStrategy)
        {
            if (tradingStrategy != null)
            {
                int rowIndex = tsDataGridView.Rows.Add();
                tsDataGridView["매매전략_계좌번호", rowIndex].Value = tradingStrategy.account;
                tsDataGridView["매매전략_재실행", rowIndex].Value = tradingStrategy.usingRestart;
                tsDataGridView["매매전략_매수조건식", rowIndex].Value = tradingStrategy.buyCondition.Name;
                tsDataGridView["매매전략_시간제한사용", rowIndex].Value = tradingStrategy.usingTimeLimit;

                if (tradingStrategy.usingTimeLimit)
                {
                    tsDataGridView["매매전략_시작시간", rowIndex].Value = tradingStrategy.startDate.ToString("HH:mm");
                    tsDataGridView["매매전략_종료시간", rowIndex].Value = tradingStrategy.endDate.ToString("HH:mm");
                }
                if (tradingStrategy.usingTickBuy)
                {
                    tsDataGridView["매매전략_호가적용", rowIndex].Value = (int)tradingStrategy.tickBuyValue * -1;
                }
                if (tradingStrategy.usingTickBuy)
                {
                    tsDataGridView["매매전략_호가적용", rowIndex].Value = (int)tradingStrategy.tickBuyValue * -1;
                }
                if (tradingStrategy.usingTrailing)
                {
                    tsDataGridView["매매전략_트레일링", rowIndex].Value = (int)tradingStrategy.trailTickValue;
                }

                tsDataGridView["매매전략_총투자금", rowIndex].Value = tradingStrategy.totalInvestment;
                tsDataGridView["매매전략_매수종목수", rowIndex].Value = tradingStrategy.buyItemCount;
                tsDataGridView["매매전략_종목별투자금", rowIndex].Value = tradingStrategy.itemInvestment;
                if (tradingStrategy.GetTradingStrategyItem(StrategyItemName.TAKE_PROFIT_SELL) != null)
                {
                    tsDataGridView["매매전략_익절", rowIndex].Value = tradingStrategy.GetActiveTradingStrategyItem(StrategyItemName.TAKE_PROFIT_SELL);
                    tsDataGridView["매매전략_익절률", rowIndex].Value = ((TradingStrategyItemWithUpDownValue)tradingStrategy.GetTradingStrategyItem(StrategyItemName.TAKE_PROFIT_SELL)).checkConditionValue;
                }
                if (tradingStrategy.GetTradingStrategyItem(StrategyItemName.STOPLOSS_SELL) != null)
                {
                    tsDataGridView["매매전략_손절", rowIndex].Value = tradingStrategy.GetActiveTradingStrategyItem(StrategyItemName.STOPLOSS_SELL);
                    tsDataGridView["매매전략_손절률", rowIndex].Value = ((TradingStrategyItemWithUpDownValue)tradingStrategy.GetTradingStrategyItem(StrategyItemName.STOPLOSS_SELL)).checkConditionValue;
                }

            }
        }
        private void UpdateAutoTradingDataGridRowAll(int index, string state, string itemcode, string conditionName, int i_qnt, int i_price)
        {
            autoTradingDataGrid["매매진행_진행상황", index].Value = state;
            autoTradingDataGrid["매매진행_종목코드", index].Value = itemcode;
            autoTradingDataGrid["매매진행_종목명", index].Value = axKHOpenAPI1.GetMasterCodeName(itemcode);
            autoTradingDataGrid["매매진행_매수조건식", index].Value = conditionName;
            autoTradingDataGrid["매매진행_매수금", index].Value = i_qnt * i_price;
            autoTradingDataGrid["매매진행_매수량", index].Value = i_qnt;
            autoTradingDataGrid["매매진행_매수가", index].Value = i_price;
            autoTradingDataGrid["매매진행_매수시간", index].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private void UpdateBuyAutoTradingDataGridState(string orderNum, string state, bool buyComplete = false)
        {
            foreach (TradingStrategy ts in tradingStrategyList)
            {
                TradingItem tradeItem = ts.tradingItemList.Find(o => o.buyOrderNum.Equals(orderNum));
                if (tradeItem != null)
                {
                    tradeItem.GetUiConnectRow().Cells["매매진행_진행상황"].Value = state;
                    break;
                }
            }
        }

        private void UpdateSellAutoTradingDataGridStatePrice(string orderNum, string state, string conclusionPrice)
        {
            coreEngine.SendLogWarningMessage("요청 주문 넘버 : " + orderNum);
            foreach (TradingStrategy ts in tradingStrategyList)
            {
                foreach (var item in ts.tradingItemList)
                {
                    coreEngine.SendLogWarningMessage("검색식 : " + ts.buyCondition.Name + " 종목명 : " + axKHOpenAPI1.GetMasterCodeName(item.itemCode) + "orderNum : " + item.sellOrderNum);
                }

                TradingItem tradeItem = ts.tradingItemList.Find(o => o.sellOrderNum.Equals(orderNum));
                if (tradeItem != null)
                {
                    tradeItem.GetUiConnectRow().Cells["매매진행_진행상황"].Value = state;
                    tradeItem.GetUiConnectRow().Cells["매매진행_매도가"].Value = conclusionPrice;
                    tradeItem.GetUiConnectRow().Cells["매매진행_매도시간"].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    break;
                }
            }
        }
      

        private void UpdateBuyAutoTradingDataGridStateOnly(string orderNum, string state)
        {
            foreach (TradingStrategy ts in tradingStrategyList)
            {
                TradingItem tradeItem = ts.tradingItemList.Find(o => o.buyOrderNum.Equals(orderNum));
                foreach(var item in ts.tradingItemList)
                {
                    coreEngine.SendLogWarningMessage("종목명 : " + axKHOpenAPI1.GetMasterCodeName(item.itemCode) + "orderNum : " + item.buyOrderNum);
                }
                if (tradeItem != null && tradeItem.GetUiConnectRow() != null)
                {
                    tradeItem.GetUiConnectRow().Cells["매매진행_주문번호"].Value = orderNum;
                    tradeItem.GetUiConnectRow().Cells["매매진행_진행상황"].Value = state;
                  
                    break;
                }
            }
        }

        private void UpdateSellAutoTradingDataGridStateOnly(string orderNum, string state)
        {
            foreach (TradingStrategy ts in tradingStrategyList)
            {
                TradingItem tradeItem = ts.tradingItemList.Find(o => o.sellOrderNum.Equals(orderNum));
                foreach (var item in ts.tradingItemList)
                {
                    coreEngine.SendLogWarningMessage("종목명 : " + axKHOpenAPI1.GetMasterCodeName(item.itemCode) + "orderNum : " + item.buyOrderNum);
                }
                if (tradeItem != null && tradeItem.GetUiConnectRow() != null)
                {
                    tradeItem.GetUiConnectRow().Cells["매매진행_주문번호"].Value = orderNum;
                    tradeItem.GetUiConnectRow().Cells["매매진행_진행상황"].Value = state;

                    break;
                }
            }
        }
    }
}
