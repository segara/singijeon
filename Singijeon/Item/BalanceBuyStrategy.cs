﻿using Singijeon.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace Singijeon
{
    //
    public class BalanceBuyStrategy : BalanceStrategy
    {
        public bool isBuy = false;
        public string orderOption; //현재가 or 시장가 등
        public DataGridViewRow ui_rowItem;
   
        public BalanceBuyStrategy
        (
            string _account,
            string _itemCode,
            int _buyingPrice,
            long _buyQnt,
             string _orderOption
        )
        {
            this.account = _account;
            this.itemCode = _itemCode;
            this.buyQnt = _buyQnt;
            this.buyingPrice = _buyingPrice;
            this.orderOption = _orderOption;
            this.type = BALANCE_STRATEGY_TYPE.BUY;
        }
      
        override public void CheckBalanceStrategy(object sender, string itemCode, long c_lPrice, Action func)
        {
            if (buyingPrice != 0)
            {
                if (buyingPrice >= c_lPrice)
                {
                    Form1 obj = (Form1)sender;
                    int buy_real_price = (int)buyingPrice;
                    bool isKospi = obj.IsKospi(itemCode);
                    int tick = hogaUnitCalc(isKospi, buy_real_price);
                    int minusTick = buy_real_price % tick;
                    buy_real_price = buy_real_price - minusTick;

                    int orderResult = obj.AxKHOpenAPI.SendOrder(
                              ConstName.SEND_ORDER_BUY,
                              Form1.GetScreenNum().ToString(),
                              account,
                              CONST_NUMBER.SEND_ORDER_BUY,//1:신규매수
                              itemCode,
                              (int)buyQnt,
                               (orderOption == ConstName.ORDER_JIJUNGGA) ? (int)buy_real_price : 0,
                              orderOption,//지정가
                              "" //원주문번호없음
                          );
                    if (orderResult == 0) //요청 성공시 (실거래는 안될 수 있음)
                    {
                        func();
                        state = TRADING_ITEM_STATE.AUTO_TRADING_STATE_BUYMORE_BEFORE_ORDER;

                        obj.AddTryingBuyList(this);
                        //obj.balanceStrategyList.Remove(this);
                        //DataGridView gridView = ui_rowItem.DataGridView;
                        //gridView.Rows.Remove(ui_rowItem);
                        Core.CoreEngine.GetInstance().SaveItemLogMessage(itemCode, "ui -> bbs 매수주문접수시도");

                        //UpdateAutoTradingDataGridRowSellStrategy(itemCode, ConstName.AUTO_TRADING_STATE_SELL_BEFORE_ORDER);
                    }
                    else
                    {
                        Core.CoreEngine.GetInstance().SaveItemLogMessage(itemCode, "bbs 추가주문 요청 실패");
                    }
                }
            }
        }

        public static int hogaUnitCalc(bool kospi,int price)
        {
            int hogaUnit = 1;
            if (price < 1000)
                hogaUnit = 1;
            else if (price < 5000)
                hogaUnit = 5;
            else if (price < 10000)
                hogaUnit = 10;
            else if (price < 50000)
                hogaUnit = 50;
            else if (price < 100000) {
                if(kospi)
                {
                   hogaUnit = 100;
                }
                else
                {
                    hogaUnit = 100;
                }
            }
            else if (price < 500000)
            {
                if (kospi)
                {
                    hogaUnit = 500;
                }
                else
                {
                    hogaUnit = 100;
                }
            }
            else
            {
                if (kospi)
                {
                    hogaUnit = 1000;
                }
                else
                {
                    hogaUnit = 100;
                }
            }
            return hogaUnit;
        }
          
    }
}