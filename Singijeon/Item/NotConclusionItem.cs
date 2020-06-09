using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singijeon
{
    
    public enum ConclusionType
    {
        BUY,
        SELL
    }
    
public class NotConclusionItem
    {
       public string orderNum = string.Empty;
        public string itemCode = string.Empty;
        public string itemName = string.Empty;
        public int orderQnt;
        public int orderPrice;
        public int outstandingNumber;
        public int currentPrice;
        public string orderGubun = string.Empty;
        public ConclusionType conclusionType = ConclusionType.BUY;

        public NotConclusionItem(string _orderNum, string _itemCode, string _orderGubun, string _itemName, int _orderQnt, int _orderPrice, int _outStandingNum)
        {
            orderNum = _orderNum;
            itemCode = _itemCode;
            itemName = _itemName;
            orderQnt = _orderQnt;
            orderPrice = _orderPrice;
            outstandingNumber = _outStandingNum;
            orderGubun = _orderGubun;
            if (_orderGubun == "매수")
            {
                conclusionType = ConclusionType.BUY;
            }
            else if(_orderGubun == "매도")
            {
                conclusionType = ConclusionType.SELL;
            }
        
        }
    }
}
