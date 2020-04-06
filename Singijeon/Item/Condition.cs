using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singijeon
{
    [Serializable]
    public class Condition
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string Uid { get; set; } //같은 검색식 구분을 위해
        public string ScreenNum { get; set; } //같은 검색식 구분을 위해
        public List<StockItem> interestItemList = new List<StockItem>();

        public Condition(int index, string name)
        {
            this.Index = index;
            this.Name = name;
            this.Uid = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }
    }
}
