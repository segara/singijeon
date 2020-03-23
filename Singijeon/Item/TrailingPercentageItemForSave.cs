using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singijeon
{
    [Serializable]
    public class TrailingPercentageItemForSave
    {
        public string itemCode;
        public string ConditionName;
        public DateTime showingTime;
        public long showingPrice;


        public int percentageCheckPrice = 0;

        public TrailingPercentageItemForSave(string _itemCode, string _ConditionName, long _showingPrice, int _percentageCheckPrice)
        {
            itemCode = _itemCode;
            ConditionName = _ConditionName;
            showingTime = DateTime.Now;
            showingPrice = _showingPrice;

            if (_percentageCheckPrice > 0)
            {
   
                percentageCheckPrice = _percentageCheckPrice;
            }
        }
    }
}
