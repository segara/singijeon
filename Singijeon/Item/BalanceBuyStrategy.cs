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
    public class BalanceBuyStrategy : BalanceStrategy
    {
        public bool isBuy = false;
        public string orderOption; //현재가 or 시장가 등
        public DataGridViewRow ui_rowItem;

        public  BalanceBuyStrategy
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
        }

        override public void CheckBalanceStrategy(object sender, string itemCode, long c_lPrice, Action func)
        {
            if (buyingPrice != 0)
            {
                if (buyingPrice >= c_lPrice)
                {
                    tradingStrategyGridView obj = (tradingStrategyGridView)sender;
                    int orderResult = obj.AxKHOpenAPI.SendOrder(
                              ConstName.SEND_ORDER_BUY,
                              tradingStrategyGridView.GetScreenNum().ToString(),
                              account,
                              CONST_NUMBER.SEND_ORDER_BUY,//1:신규매수
                              itemCode,
                              (int)buyQnt,
                               (orderOption == ConstName.ORDER_JIJUNGGA) ? buyingPrice : 0,
                              orderOption,//지정가
                              "" //원주문번호없음
                          );
                    if (orderResult == 0) //요청 성공시 (실거래는 안될 수 있음)
                    {
                        func();
                        obj.balanceStrategyList.Remove(this);
                        DataGridView gridView = ui_rowItem.DataGridView;
                        gridView.Rows.Remove(ui_rowItem);
                        Core.CoreEngine.GetInstance().SendLogMessage("ui -> bbs 매수주문접수시도");
                        //UpdateAutoTradingDataGridRowSellStrategy(itemCode, ConstName.AUTO_TRADING_STATE_SELL_BEFORE_ORDER);
                    }
                    else
                    {
                        Core.CoreEngine.GetInstance().SendLogMessage("bbs 추가주문 요청 실패");
                    }
                }
            }
        }
    }
}