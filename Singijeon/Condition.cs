using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singijeon
{
    public class Condition
    {
        public int Index { get; set; }
        public string Name { get; set; }

        public List<StockItem> interestItemList = new List<StockItem>();

        public Condition(int index, string name)
        {
            this.Index = index;
            this.Name = name;
        }
    }
}
