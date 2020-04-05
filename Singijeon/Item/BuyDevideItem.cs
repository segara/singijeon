using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singijeon
{
    [Serializable]
    class BuyDevideItem
    {
        string itemCode = string.Empty;
        bool isBuyComplete = false;
        int curQnt;
        int Qnt;
        int PriceVal;
    }
}
