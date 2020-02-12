using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SettlementItem 
{

    public string sellOrderNum;
    public string ItemCode;
    public int orderQnt;
    public string accountNum;

    public SettlementItem(string _accoutNum, string _ItemCode, int _orderQnt)
    {
        this.accountNum = _accoutNum;
        this.ItemCode = _ItemCode;
        this.orderQnt = _orderQnt;
    }
}
