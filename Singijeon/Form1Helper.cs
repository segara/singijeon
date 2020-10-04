//#define TEST_CONSOLE
using Singijeon.Core;
using Singijeon.Item;
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
    public partial class Form1
    {
        //실시간 종목 조회 응답시//
        private void UpdateAccountBalanceDataGridViewRow(string itemCode, long c_lPrice)
        {
            if (balanceItemList.Find(o => (o.itemCode == itemCode)) != null)
            {
                BalanceItem item = balanceItemList.Find(o => (o.itemCode == itemCode));
                DataGridViewRow row = item.ui_rowItem;
                if(row.Index == -1)
                {
                    coreEngine.SendLogErrorMessage(itemCode + " 계좌잔고 ui 업데이트에러");
                    return;
                }
                if (row != null  && row.Cells["계좌잔고_종목코드"].Value != null)
                {
                    if (row.Cells["계좌잔고_종목코드"].Value.ToString().Contains(itemCode))
                    {
                        row.Cells["계좌잔고_현재가"].Value = c_lPrice;

                        double buyingPrice = double.Parse(row.Cells["계좌잔고_평균단가"].Value.ToString());
                        int balanceCount = int.Parse(row.Cells["계좌잔고_보유수량"].Value.ToString());
                        double currentAllPrice = c_lPrice * balanceCount;

                        if (buyingPrice != 0)
                        {
                            //row.Cells["계좌잔고_평균단가"].Value = buyingPrice;
                            row.Cells["계좌잔고_평가금액"].Value = currentAllPrice;

                            double sellPrice = buyingPrice; // 평단가 
                            double stockFee = ((double)c_lPrice * 0.01 * FEE_RATE) * (double)balanceCount; //+ ((double)c_lPrice * 0.01 * 0.015 * (double)balanceCount); //+ ((double)buyingPrice * 0.01 * 0.015 * (double)balanceCount);
                            double allSellPrice = (sellPrice * (double)balanceCount) + stockFee;

                            row.Cells["계좌잔고_손익금액"].Value = (currentAllPrice - allSellPrice);

                            double profitRate = GetProfitRate((double)c_lPrice, (double)sellPrice);
                            row.Cells["계좌잔고_손익률"].Value = profitRate.ToString("F2");
                        }
                    }
                }
            }
        }
        //private void UpdateBalanceDataGridViewRow(string itemCode, long c_lPrice)
        //{
        //    foreach (DataGridViewRow row in balanceDataGrid.Rows)
        //    {
        //        if (row.Cells["잔고_종목코드"].Value != null)
        //        {
        //            if (row.Cells["잔고_종목코드"].Value.ToString().Contains(itemCode))
        //            {
        //                row.Cells["잔고_현재가"].Value = c_lPrice;

        //                double buyingPrice = double.Parse(row.Cells["잔고_매입단가"].Value.ToString());

        //                if (buyingPrice != 0)
        //                {
        //                    double profitRate = GetProfitRate((double)c_lPrice, (double)buyingPrice);

        //                    row.Cells["잔고_손익률"].Value = profitRate;
        //                }

        //            }
        //        }
        //    }
        //}
        private void UpdateAutoTradingDataGridViewRow(string itemCode, long c_lPrice)
        {
            foreach (DataGridViewRow row in autoTradingDataGrid.Rows)
            {
                if (row.Cells["매매진행_종목코드"].Value != null)
                {
                    if (row.Cells["매매진행_종목코드"].Value.ToString().Contains(itemCode))
                    {
                        row.Cells["매매진행_현재가"].Value = c_lPrice;

                        if (row.Cells["매매진행_매수가"].Value != null)
                        {
                            double buyingPrice = double.Parse(row.Cells["매매진행_매수가"].Value.ToString());

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
            if (table.ContainsKey("미체결_주문가"))
                outstandingDataGrid["미체결_주문가", rowIndex].Value = table["미체결_주문가"];
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
        //public void Update_BalanceDataGrid_UI(Hashtable table , int rowIndex)
        //{
        //    if(table.ContainsKey("잔고_계좌번호"))
        //        balanceDataGrid["잔고_계좌번호", rowIndex].Value = table["잔고_계좌번호"];
        //    if (table.ContainsKey("잔고_종목코드"))
        //        balanceDataGrid["잔고_종목코드", rowIndex].Value = table["잔고_종목코드"];
        //    if (table.ContainsKey("잔고_종목명"))
        //        balanceDataGrid["잔고_종목명", rowIndex].Value = table["잔고_종목명"];
        //    if (table.ContainsKey("잔고_보유수량"))
        //        balanceDataGrid["잔고_보유수량", rowIndex].Value = table["잔고_보유수량"];
        //    if (table.ContainsKey("잔고_주문가능수량"))
        //        balanceDataGrid["잔고_주문가능수량", rowIndex].Value = table["잔고_주문가능수량"];
        //    if (table.ContainsKey("잔고_매입단가"))
        //        balanceDataGrid["잔고_매입단가", rowIndex].Value = table["잔고_매입단가"];
        //    if (table.ContainsKey("잔고_총매입가"))
        //        balanceDataGrid["잔고_총매입가", rowIndex].Value = table["잔고_총매입가"];
        //    if (table.ContainsKey("잔고_손익률"))
        //        balanceDataGrid["잔고_손익률", rowIndex].Value = table["잔고_손익률"];
        //    if (table.ContainsKey("잔고_매매구분"))
        //        balanceDataGrid["잔고_매매구분", rowIndex].Value = table["잔고_매매구분"];
        //    if (table.ContainsKey("잔고_현재가"))
        //        balanceDataGrid["잔고_현재가", rowIndex].Value = table["잔고_현재가"];
        //}

        public void UpdateBssGridView(Hashtable table, int rowIndex)
        {
            if (table.ContainsKey("bss_진행상황"))
                BssDataGridView["bss_진행상황", rowIndex].Value = table["bss_진행상황"];
            if (table.ContainsKey("bss_종목코드"))
                BssDataGridView["bss_종목코드", rowIndex].Value = table["bss_종목코드"];
            if (table.ContainsKey("bss_종목명"))
                BssDataGridView["bss_종목명", rowIndex].Value = table["bss_종목명"];
            if (table.ContainsKey("bss_매도량"))
                BssDataGridView["bss_매도량", rowIndex].Value = table["bss_매도량"];
            if (table.ContainsKey("bss_설정손익률"))
                BssDataGridView["bss_설정손익률", rowIndex].Value = table["bss_설정손익률"];
        }

        private void UpdateBBSGridView(Hashtable table, int rowIndex)
        {
            if (table.ContainsKey("bbs_종목코드"))
                BBSdataGridView["bbs_종목코드", rowIndex].Value = table["bbs_종목코드"];
            if (table.ContainsKey("bbs_종목명"))
                BBSdataGridView["bbs_종목명", rowIndex].Value = table["bbs_종목명"];
            if (table.ContainsKey("bbs_조건"))
                BBSdataGridView["bbs_조건", rowIndex].Value = table["bbs_조건"];
            if (table.ContainsKey("bbs_매수금"))
                BBSdataGridView["bbs_매수금", rowIndex].Value = table["bbs_매수금"];
        }

        public void AddStrategyToDataGridView(TradingStrategy tradingStrategy)
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

            coreEngine.SaveItemLogMessage(itemcode, state);
        }

        private void UpdateBuyAutoTradingDataGridState(string orderNum, bool buyComplete = false)
        {
            foreach (TradingStrategy ts in tradingStrategyList)
            {
                coreEngine.SendLogWarningMessage(ts.buyCondition.Name);
                TradingItem tradeItem = ts.tradingItemList.Find(o => o.buyOrderNum.Equals(orderNum));
                foreach (var item in ts.tradingItemList)
                {
                    coreEngine.SendLogWarningMessage(item.itemName + " 요청 주문 넘버 : " + orderNum);
                }
                if (tradeItem != null)
                {
                    tradeItem.GetUiConnectRow().Cells["매매진행_진행상황"].Value = TradingItem.StateToString(tradeItem.state);
                    tradeItem.GetUiConnectRow().Cells["매매진행_매수가"].Value = tradeItem.buyingPrice;
                    tradeItem.GetUiConnectRow().Cells["매매진행_매수량"].Value = tradeItem.curQnt;
                    tradeItem.GetUiConnectRow().Cells["매매진행_매수금"].Value = tradeItem.curQnt * tradeItem.buyingPrice;
                    break;
                }
            }
        }

        private void UpdateBuyAutoTradingDataGridState(string itemCode)
        {
            foreach (TradingStrategy ts in tradingStrategyList)
            {
                coreEngine.SendLogWarningMessage("ui update" + ts.buyCondition.Name);
                List<TradingItem> tradeItemArray = ts.tradingItemList.FindAll(o => o.itemCode.Equals(itemCode));
                foreach (TradingItem tradeItem in tradeItemArray)
                {
                    if (tradeItem != null)
                    {
                        coreEngine.SendLogWarningMessage("tradeItem uid" + tradeItem.Uid);
                        tradeItem.GetUiConnectRow().Cells["매매진행_진행상황"].Value = TradingItem.StateToString(tradeItem.state);
                        tradeItem.GetUiConnectRow().Cells["매매진행_매수가"].Value = tradeItem.buyingPrice;
                        tradeItem.GetUiConnectRow().Cells["매매진행_매수량"].Value = tradeItem.curQnt;
                        tradeItem.GetUiConnectRow().Cells["매매진행_매수금"].Value = tradeItem.curQnt * tradeItem.buyingPrice;
                        break;
                    }
                }
            }
        }

        private void UpdateSellAutoTradingDataGridStatePrice(string orderNum, string conclusionPrice)
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
                    tradeItem.GetUiConnectRow().Cells["매매진행_진행상황"].Value = TradingItem.StateToString(tradeItem.state);
                    tradeItem.GetUiConnectRow().Cells["매매진행_매도가"].Value = conclusionPrice;
                    tradeItem.GetUiConnectRow().Cells["매매진행_매도시간"].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
        }

        private void UpdateBuyAutoTradingDataGridStateOnly(string orderNum, string state)
        {
            foreach (TradingStrategy ts in tradingStrategyList)
            {
                foreach (var item in ts.tradingItemList)
                {
                    coreEngine.SendLogWarningMessage("종목명 : " + axKHOpenAPI1.GetMasterCodeName(item.itemCode) + "orderNum : " + item.buyOrderNum);
                }

                TradingItem tradeItem = ts.tradingItemList.Find(o => o.buyOrderNum.Equals(orderNum));

                if (tradeItem != null && tradeItem.GetUiConnectRow() != null)
                {
                    tradeItem.GetUiConnectRow().Cells["매매진행_주문번호"].Value = orderNum;
                    tradeItem.GetUiConnectRow().Cells["매매진행_진행상황"].Value = state;

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
                    coreEngine.SendLogWarningMessage("종목명 : " + axKHOpenAPI1.GetMasterCodeName(item.itemCode) + "orderNum : " + item.sellOrderNum);
                }
                if (tradeItem != null && tradeItem.GetUiConnectRow() != null)
                {
                    coreEngine.SendLogWarningMessage("종목찾기 성공 : " + state);

                    tradeItem.GetUiConnectRow().Cells["매매진행_주문번호"].Value = orderNum;
                    tradeItem.GetUiConnectRow().Cells["매매진행_진행상황"].Value = state;

                    break;
                }
            }
        }
        public void SaveSetting(string name)
        {
            using (StreamWriter streamWriter = new StreamWriter(name + ".txt", false))
            {
                streamWriter.WriteLine("allCostUpDown" + ";" + allCostUpDown.Value);

                streamWriter.WriteLine("usingTickBuyCheck" + ";" + usingTickBuyCheck.Checked);
                streamWriter.WriteLine("buyTickComboBox" + ";" + buyTickComboBox.SelectedIndex);

                streamWriter.WriteLine("usingTrailingBuyCheck" + ";" + usingTrailingBuyCheck.Checked);
                streamWriter.WriteLine("trailingUpDown" + ";" + (int)trailingUpDown.Value);

                streamWriter.WriteLine("itemCountUpdown" + ";" + (int)itemCountUpdown.Value);

                streamWriter.WriteLine("orderPecentageCheckBox" + ";" + orderPecentageCheckBox.Checked);
                streamWriter.WriteLine("orderPercentageUpdown" + ";" + (double)orderPercentageUpdown.Value);

                streamWriter.WriteLine("profitSellCheckBox" + ";" + profitSellCheckBox.Checked);
                streamWriter.WriteLine("profitSellUpdown" + ";" + (double)profitSellUpdown.Value);

                streamWriter.WriteLine("minusSellCheckBox" + ";" + minusSellCheckBox.Checked);
                streamWriter.WriteLine("minusSellUpdown" + ";" + (double)minusSellUpdown.Value);

                streamWriter.WriteLine("useGapTrailBuyCheck" + ";" + useGapTrailBuyCheck.Checked);
                streamWriter.WriteLine("gapTrailTimeUpdown" + ";" + (int)gapTrailTimeUpdown.Value);
                streamWriter.WriteLine("gapTrailCostUpdown" + ";" + (double)gapTrailCostUpdown.Value);

                streamWriter.WriteLine("TimeUseCheck" + ";" + TimeUseCheck.Checked);
                streamWriter.WriteLine("startTimePicker" + ";" + startTimePicker.Value);
                streamWriter.WriteLine("endTimePicker" + ";" + endTimePicker.Value);

                streamWriter.WriteLine("useVwmaCheckBox" + ";" + useVwmaCheckBox.Checked);
                streamWriter.WriteLine("useEnvelopeCheckBox" + ";" + useEnvelopeCheckBox.Checked);
                streamWriter.WriteLine("useEnvelope7CheckBox" + ";" + useEnvelope7CheckBox.Checked);
                streamWriter.WriteLine("useEnvelope10CheckBox" + ";" + useEnvelope10CheckBox.Checked);
                streamWriter.WriteLine("tickMinusValue" + ";" + (double)tickMinusValue.Value);

                streamWriter.WriteLine("sellProfitSijangRadio" + ";" + sellProfitSijangRadio.Checked);
                streamWriter.WriteLine("sellProfitJijungRadio" + ";" + sellProfitJijungRadio.Checked);

                streamWriter.WriteLine("stopLossSijangRadio" + ";" + stopLossSijangRadio.Checked);
                streamWriter.WriteLine("stopLossJijungRadio" + ";" + stopLossJijungRadio.Checked);
                //false : 덮어쓰기
                streamWriter.WriteLine("M_allCostUpDown" + ";" + M_allCostUpDown.Value);

                streamWriter.WriteLine("M_usingTickBuyCheck" + ";" + M_usingTickBuyCheck.Checked);
                streamWriter.WriteLine("M_buyTickComboBox" + ";" + M_buyTickComboBox.SelectedIndex);

                streamWriter.WriteLine("M_usingTrailingBuyCheck" + ";" + M_usingTrailingBuyCheck.Checked);
                streamWriter.WriteLine("M_trailingUpDown" + ";" + (int)M_trailingUpDown.Value);

                streamWriter.WriteLine("M_timeCancelCheckBox" + ";" + M_timeCancelCheckBox.Checked);
                streamWriter.WriteLine("M_waitTimeUpdown" + ";" + (int)M_waitTimeUpdown.Value);

                streamWriter.WriteLine("M_SellUpdown" + ";" + (double)M_SellUpdown.Value);
                streamWriter.WriteLine("M_SellUpdownLoss" + ";" + (double)M_SellUpdownLoss.Value);

                streamWriter.WriteLine("m_useVwmaCheckBox" + ";" + m_useVwmaCheckBox.Checked);
                streamWriter.WriteLine("marketPriceRadioBtn" + ";" + marketPriceRadioBtn.Checked);
                streamWriter.WriteLine("curPriceRadio" + ";" + curPriceRadio.Checked);

                streamWriter.WriteLine("usingDoubleConditionCheck" + ";" + usingDoubleConditionCheck.Checked);
                streamWriter.WriteLine("BuyConditionDoubleComboBox" + ";" + BuyConditionDoubleComboBox.SelectedItem);
                streamWriter.WriteLine("TrailingSellCheckBox" + ";" + TrailingSellCheckBox.Checked);

                streamWriter.WriteLine("BuyMoreCheckBox1" + ";" + BuyMoreCheckBox.Checked);
                streamWriter.WriteLine("BuyMorePercentUpdown" + ";" + (double)BuyMorePercentUpdown.Value);
                streamWriter.WriteLine("BuyMorePercentUpdownProfit" + ";" + (double)BuyMorePercentUpdownProfit.Value);
                streamWriter.WriteLine("BuyMoreValueUpdown" + ";" + (int)BuyMoreValueUpdown.Value);

                streamWriter.WriteLine("buyCancelTimeCheckBox" + ";" + buyCancelTimeCheckBox.Checked);

                streamWriter.WriteLine("DivideSellLossCheckBox" + ";" + DivideSellLossCheckBox.Checked);
                streamWriter.WriteLine("divideRatePercentLoss" + ";" + (double)divideRatePercentLoss.Value);
                streamWriter.WriteLine("divideSellPercentLoss" + ";" + (double)divideSellPercentLoss.Value);

                streamWriter.WriteLine("DivideSellProfitCheckBox" + ";" + DivideSellProfitCheckBox.Checked);
                streamWriter.WriteLine("divideRatePercentProfit" + ";" + (double)divideRatePercentProfit.Value);
                streamWriter.WriteLine("divideSellPercentProfit" + ";" + (double)divideSellPercentProfit.Value);

                streamWriter.WriteLine("divideLossSellLoopCheck" + ";" + divideLossSellLoopCheck.Checked);
                streamWriter.WriteLine("divideProfitSellLoopCheck" + ";" + divideProfitSellLoopCheck.Checked);
                streamWriter.WriteLine("takeProfitAfterBuyMoreCheck" + ";" + takeProfitAfterBuyMoreCheck.Checked);
                streamWriter.WriteLine("stopLossAfterBuyMoreCheck" + ";" + stopLossAfterBuyMoreCheck.Checked);
                streamWriter.WriteLine("DivideSellCountUpDownProfit" + ";" + (int)DivideSellCountUpDownProfit.Value);
                streamWriter.WriteLine("DivideSellCountUpDown" + ";" + (int)DivideSellCountUpDown.Value);
            }
        }
        public void ClearSetting()
        {
            allCostUpDown.Value = 0;

            usingTickBuyCheck.Checked = false;
            buyTickComboBox.SelectedIndex = 0;

            usingTrailingBuyCheck.Checked = false;
            trailingUpDown.Value = trailingUpDown.Minimum;

            itemCountUpdown.Value = itemCountUpdown.Minimum;

            orderPecentageCheckBox.Checked = false;
            orderPercentageUpdown.Value = orderPercentageUpdown.Minimum;

            profitSellCheckBox.Checked = false;
            profitSellUpdown.Value = profitSellUpdown.Minimum;

            minusSellCheckBox.Checked = false;
            minusSellUpdown.Value = minusSellUpdown.Maximum;

            useGapTrailBuyCheck.Checked = false;
            gapTrailTimeUpdown.Value = gapTrailTimeUpdown.Minimum;
            gapTrailCostUpdown.Value = gapTrailCostUpdown.Minimum;

            BuyMorePercentUpdown.Value = BuyMorePercentUpdown.Maximum;
            BuyMorePercentUpdownProfit.Value = BuyMorePercentUpdownProfit.Minimum;

            TimeUseCheck.Checked = false;
            startTimePicker.Value = DateTime.Now;
            endTimePicker.Value = DateTime.Now;

            sellProfitJijungRadio.Checked = true;
            stopLossSijangRadio.Checked = true;

            marketPriceRadioBtn.Checked = false;
            curPriceRadio.Checked = true;

            useVwmaCheckBox.Checked = false;
            useEnvelopeCheckBox.Checked = false;
            useEnvelope7CheckBox.Checked = false;
            useEnvelope10CheckBox.Checked = false;
            tickMinusValue.Value = tickMinusValue.Minimum;

            usingDoubleConditionCheck.Checked = false;
            BuyConditionDoubleComboBox.SelectedText = string.Empty;
            BuyConditionDoubleComboBox.SelectedItem = string.Empty;

            TrailingSellCheckBox.Checked = false;

            BuyMoreCheckBox.Checked = false;
            buyCancelTimeCheckBox.Checked = false;

            DivideSellLossCheckBox.Checked = false;
            divideRatePercentLoss.Value = divideRatePercentLoss.Maximum;
            divideSellPercentLoss.Value = divideSellPercentLoss.Minimum;

            DivideSellProfitCheckBox.Checked = false;
            divideRatePercentProfit.Value = divideRatePercentProfit.Minimum;
            divideSellPercentProfit.Value = divideSellPercentProfit.Minimum;

            divideProfitSellLoopCheck.Checked = false;
            divideLossSellLoopCheck.Checked = false;

            takeProfitAfterBuyMoreCheck.Checked = false;
            stopLossAfterBuyMoreCheck.Checked = false;

            DivideSellCountUpDown.Value = 100;
            DivideSellCountUpDownProfit.Value = 100;
        }
        public void LoadSettingRead(string settingCondition)
        {
            ClearSetting();
            try
            {
                using (StreamReader streamReader = new StreamReader(settingCondition))
                {

                    while (streamReader.EndOfStream == false)
                    {
                        string line = streamReader.ReadLine();
                        string[] strringArray = line.Split(';');

                        switch (strringArray[0])
                        {
                            case "M_allCostUpDown":
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
                                M_SellUpdown.Value = (decimal)(double.Parse(strringArray[1]));
                                break;
                            case "M_SellUpdownLoss":
                                M_SellUpdownLoss.Value = (decimal)(double.Parse(strringArray[1]));
                                break;
                            case "m_useVwmaCheckBox":
                                m_useVwmaCheckBox.Checked = bool.Parse(strringArray[1]);
                                break;
                            case "allCostUpDown":
                                allCostUpDown.Value = int.Parse(strringArray[1]);
                                break;
                            case "usingTickBuyCheck":
                                usingTickBuyCheck.Checked = bool.Parse(strringArray[1]);
                                break;
                            case "buyTickComboBox":
                                buyTickComboBox.SelectedIndex = int.Parse(strringArray[1]);
                                break;
                            case "usingTrailingBuyCheck":
                                usingTrailingBuyCheck.Checked = bool.Parse(strringArray[1]);
                                break;
                            case "trailingUpDown":
                                trailingUpDown.Value = int.Parse(strringArray[1]);
                                break;
                            case "orderPecentageCheckBox":
                                orderPecentageCheckBox.Checked = bool.Parse(strringArray[1]);
                                break;
                            case "orderPercentageUpdown":
                                orderPercentageUpdown.Value = (decimal)(double.Parse(strringArray[1]));
                                break;
                            case "itemCountUpdown":
                                itemCountUpdown.Value = int.Parse(strringArray[1]);
                                break;
                            case "profitSellCheckBox":
                                profitSellCheckBox.Checked = bool.Parse(strringArray[1]);
                                break;
                            case "profitSellUpdown":
                                profitSellUpdown.Value = (decimal)(double.Parse(strringArray[1]));
                                break;
                            case "minusSellCheckBox":
                                minusSellCheckBox.Checked = bool.Parse(strringArray[1]);
                                break;
                            case "minusSellUpdown":
                                minusSellUpdown.Value = (decimal)(double.Parse(strringArray[1]));
                                break;
                            case "useGapTrailBuyCheck":
                                useGapTrailBuyCheck.Checked = bool.Parse(strringArray[1]);
                                break;
                            case "gapTrailTimeUpdown":
                                gapTrailTimeUpdown.Value = int.Parse(strringArray[1]);
                                break;
                            case "gapTrailCostUpdown":
                                gapTrailCostUpdown.Value = (decimal)(double.Parse(strringArray[1]));
                                break;
                            case "TimeUseCheck":
                                TimeUseCheck.Checked = bool.Parse(strringArray[1]);
                                break;
                            case "startTimePicker":
                                startTimePicker.Value = DateTime.Parse(strringArray[1]);
                                break;
                            case "endTimePicker":
                                endTimePicker.Value = DateTime.Parse(strringArray[1]);
                                break;
                            case "tickMinusValue":
                                tickMinusValue.Value = (decimal)(double.Parse(strringArray[1]));
                                break;
                            case "sellProfitSijangRadio":
                                sellProfitSijangRadio.Checked = bool.Parse(strringArray[1]);
                                break;
                            case "sellProfitJijungRadio":
                                sellProfitJijungRadio.Checked = bool.Parse(strringArray[1]);
                                break;
                            case "stopLossSijangRadio":
                                stopLossSijangRadio.Checked = bool.Parse(strringArray[1]);
                                break;
                            case "stopLossJijungRadio":
                                stopLossJijungRadio.Checked = bool.Parse(strringArray[1]);
                                break;
                            case "useVwmaCheckBox":
                                useVwmaCheckBox.Checked = bool.Parse(strringArray[1]);
                                break;
                            case "useEnvelopeCheckBox":
                                useEnvelopeCheckBox.Checked = bool.Parse(strringArray[1]);
                                break;
                            case "useEnvelope7CheckBox":
                                useEnvelope7CheckBox.Checked = bool.Parse(strringArray[1]);
                                break;
                            case "useEnvelope10CheckBox":
                                useEnvelope10CheckBox.Checked = bool.Parse(strringArray[1]);
                                break;
                            case "marketPriceRadioBtn":
                                marketPriceRadioBtn.Checked = bool.Parse(strringArray[1]);
                                break;
                            case "curPriceRadio":
                                curPriceRadio.Checked = bool.Parse(strringArray[1]);
                                break;
                            case "usingDoubleConditionCheck":
                                usingDoubleConditionCheck.Checked = bool.Parse(strringArray[1]);
                                break;
                            case "BuyConditionDoubleComboBox":
                                BuyConditionDoubleComboBox.SelectedItem = (strringArray[1]);
                                break;
                            case "TrailingSellCheckBox":
                                TrailingSellCheckBox.Checked = bool.Parse(strringArray[1]);
                                break;
                            case "BuyMoreCheckBox1":
                                BuyMoreCheckBox.Checked = bool.Parse(strringArray[1]);
                                break;
                            case "BuyMorePercentUpdown":
                                BuyMorePercentUpdown.Value = (decimal)(double.Parse(strringArray[1]));
                                break;
                            case "BuyMorePercentUpdownProfit":
                                BuyMorePercentUpdownProfit.Value = (decimal)(double.Parse(strringArray[1]));
                                break;
                            case "BuyMoreValueUpdown":
                                BuyMoreValueUpdown.Value = (decimal)(double.Parse(strringArray[1]));
                                break;
                            case "buyCancelTimeCheckBox":
                                buyCancelTimeCheckBox.Checked = bool.Parse(strringArray[1]);
                                break;
                            case "DivideSellLossCheckBox":
                                DivideSellLossCheckBox.Checked = bool.Parse(strringArray[1]);
                                break;
                            case "divideRatePercentLoss":
                                divideRatePercentLoss.Value = (decimal)(double.Parse(strringArray[1]));
                                break;
                            case "divideSellPercentLoss":
                                divideSellPercentLoss.Value = (decimal)(double.Parse(strringArray[1]));
                                break;
                            case "DivideSellProfitCheckBox":
                                DivideSellProfitCheckBox.Checked = bool.Parse(strringArray[1]);
                                break;
                            case "divideRatePercentProfit":
                                divideRatePercentProfit.Value = (decimal)(double.Parse(strringArray[1]));
                                break;
                            case "divideSellPercentProfit":
                                divideSellPercentProfit.Value = (decimal)(double.Parse(strringArray[1]));
                                break;
                            case "divideLossSellLoopCheck":
                                divideLossSellLoopCheck.Checked = bool.Parse(strringArray[1]);
                                break;
                            case "divideProfitSellLoopCheck":
                                divideProfitSellLoopCheck.Checked = bool.Parse(strringArray[1]);
                                break;
                            case "stopLossAfterBuyMoreCheck":
                                stopLossAfterBuyMoreCheck.Checked = bool.Parse(strringArray[1]);
                                break;
                            case "takeProfitAfterBuyMoreCheck":
                                takeProfitAfterBuyMoreCheck.Checked = bool.Parse(strringArray[1]);
                                break;
                            case "DivideSellCountUpDown":
                                DivideSellCountUpDown.Value = int.Parse(strringArray[1]);
                                break;
                            case "DivideSellCountUpDownProfit":
                                DivideSellCountUpDownProfit.Value = int.Parse(strringArray[1]);
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public void LoadSetting(string settingCondition)
        {
            LoadSettingRead(settingCondition + ".txt");
        }
    }
}
