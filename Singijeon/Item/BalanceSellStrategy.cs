using Singijeon.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Singijeon
{
    //
    public class BalanceSellStrategy : BalanceStrategy
    {
        public bool usingTakeProfit = false; //익절사용여부
        public bool usingStoploss = false;   //손절사용여부
        public double takeProfitRate = 0; //익절률
        public double stoplossRate = 0; //손절률
        public string profitOrderOption; //현재가 or 시장가 등
        public string stoplossOrderOption; //현재가 or 시장가 등
        [NonSerialized]
        public DataGridViewRow ui_rowItem;
        //매매 진행 종목 리스트

        public BalanceSellStrategy(
            string _account,
            string _itemCode,
            int _buyingPrice,
            long _curQnt,
            long _sellQnt,
             string _sellProfitOrderOption,
             string _sellStopLossOrderOption,
            bool _usingTakeProfit,
            double _takeProfitRate,
            bool _usingStoploss,
            double _stoplossRate
            )
        {
            this.account = _account;
            this.itemCode = _itemCode;
            this.curQnt = _curQnt;
            this.sellQnt = _sellQnt;
            this.buyingPrice = _buyingPrice;
            this.usingTakeProfit = _usingTakeProfit;
            this.takeProfitRate = _takeProfitRate;
            this.profitOrderOption = _sellProfitOrderOption;
            this.stoplossOrderOption = _sellStopLossOrderOption;
            this.usingStoploss = _usingStoploss;
            this.stoplossRate = _stoplossRate;
            this.type = BALANCE_STRATEGY_TYPE.SELL;
        }

        override public void CheckBalanceStrategy(object sender, string itemCode, long c_lPrice, Action func)
        {
            Form1 obj = (Form1)sender;

            if (!isSold && buyingPrice != 0)
            {
                double profitRate = Form1.GetProfitRate((double)c_lPrice, (double)buyingPrice);

                if (usingTakeProfit && takeProfitRate <= profitRate) //익절
                {
                    int orderResult = obj.AxKHOpenAPI.SendOrder(
                                        "잔고익절매도",
                                        Form1.GetScreenNum().ToString(),
                                        account,
                                        CONST_NUMBER.SEND_ORDER_SELL,
                                        itemCode,
                                        (int)sellQnt,
                                        profitOrderOption == ConstName.ORDER_SIJANGGA ? 0 : (int)c_lPrice,
                                        profitOrderOption,
                                        "" //원주문번호없음
                                    );
                    if (orderResult == 0) //요청 성공시 (실거래는 안될 수 있음)
                    {
                        isSold = true;
                        state = TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_BEFORE_ORDER;
                        obj.AddTryingSellList(this);
                        //obj.balanceStrategyList.Remove(this);
                        Core.CoreEngine.GetInstance().SendLogMessage("ui -> bss 매도주문접수시도");
                        //UpdateAutoTradingDataGridRowSellStrategy(itemCode, ConstName.AUTO_TRADING_STATE_SELL_BEFORE_ORDER);
                    }
                    else
                    {
                        Core.CoreEngine.GetInstance().SendLogMessage("bss 잔고 익절 요청 실패");
                    }
                }
                if (usingStoploss && stoplossRate > profitRate) //손절
                {
                    int orderResult = obj.AxKHOpenAPI.SendOrder(
                                            "잔고손절매도",
                                            Form1.GetScreenNum().ToString(),
                                            account,
                                            CONST_NUMBER.SEND_ORDER_SELL,
                                            itemCode,
                                            (int)sellQnt,
                                            stoplossOrderOption == ConstName.ORDER_SIJANGGA ? 0 : (int)c_lPrice,
                                            stoplossOrderOption,
                                            "" //원주문번호없음
                                        );
                    if (orderResult == 0) //요청 성공시 (실거래는 안될 수 있음)
                    {
                        isSold = true;
                        state = TRADING_ITEM_STATE.AUTO_TRADING_STATE_SELL_BEFORE_ORDER;
                        obj.AddTryingSellList(this);
                        //obj.balanceStrategyList.Remove(this);
                        Core.CoreEngine.GetInstance().SendLogMessage("ui ->bss 매도주문접수시도");
                        //UpdateAutoTradingDataGridRowSellStrategy(itemCode, ConstName.AUTO_TRADING_STATE_SELL_BEFORE_ORDER);
                    }
                    else
                    {
                        Core.CoreEngine.GetInstance().SendLogMessage("bss 잔고 손절 요청 실패");
                    }
                }
            }
             
        }
    }

}