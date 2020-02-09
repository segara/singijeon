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

    public SettlementItem(string accoutNum, string ItemCode, int orderQnt)
    {
        this.accountNum = accountNum;
        this.ItemCode = ItemCode;
        this.orderQnt = orderQnt;
    }
}
