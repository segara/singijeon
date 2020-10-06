using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singijeon
{
    class StockWithBiddingManager
    {
        private static StockWithBiddingManager instance;
        public static StockWithBiddingManager GetInstance()
        {
            if (instance == null)
            {
                instance = new StockWithBiddingManager();
            }
            return instance;
        }

        Dictionary<string, StockWithBiddingEntity> stockDictionary = new Dictionary<string, StockWithBiddingEntity>();

        private StockWithBiddingManager()
        {

        }

        public void SetItem (StockWithBiddingEntity item)
        {
            if(stockDictionary.ContainsKey(item.Code) == false)
            {
                stockDictionary.Add(item.Code, item);
            }
            else
            {
                stockDictionary[item.Code] = item;
            }
        }

        public StockWithBiddingEntity GetItem(string itemCode)
        {
            if (stockDictionary.ContainsKey(itemCode))
            {
                return stockDictionary[itemCode];
            }
            return null;
        }
    }

    public class BiddingEntityObject
    {
        public long Hoga = 0; //호가
        public long Qnt = 0;  //잔량
        public double Percentage = 0; //잔량대비
    }

    public class StockWithBiddingEntity
    {
        public string Code { get; set; }

        BiddingEntityObject[] sellBiddingEntityArray = new BiddingEntityObject[10];
        BiddingEntityObject[] buyBiddingEntityArray = new BiddingEntityObject[10];

        public StockWithBiddingEntity()
        {
            for(int i = 0; i < sellBiddingEntityArray.Length; ++i)
            {
                sellBiddingEntityArray[i] = new BiddingEntityObject();
            }
            for (int i = 0; i < buyBiddingEntityArray.Length; ++i)
            {
                buyBiddingEntityArray[i] = new BiddingEntityObject();
            }
        }

        public void SetSellHoga(int index, long hoga)
        {
            if(index < sellBiddingEntityArray.Length)
            {
                sellBiddingEntityArray[index].Hoga = hoga;
            }
        }
        public void SetSellQnt(int index, long qnt)
        {
            if (index < sellBiddingEntityArray.Length)
            {
                sellBiddingEntityArray[index].Qnt = qnt;
            }
        }
        public long GetBuyHoga(int index)
        {
            if (index < buyBiddingEntityArray.Length)
            {
                while(buyBiddingEntityArray[index].Qnt == 0 )
                {
                    index++;
                    if (index < buyBiddingEntityArray.Length)
                    {
                        if (buyBiddingEntityArray[index].Hoga > 0)
                            return Math.Abs(buyBiddingEntityArray[index].Hoga);
                    }
                }
                return Math.Abs(buyBiddingEntityArray[index].Hoga);
            }
            return 0;
        }
        public void SetBuyHoga(int index, long hoga)
        {
            if (index < buyBiddingEntityArray.Length)
            {
                buyBiddingEntityArray[index].Hoga = hoga;
            }
        }
        public void SetBuyQnt(int index, long qnt)
        {
            if (index < buyBiddingEntityArray.Length)
            {
                buyBiddingEntityArray[index].Qnt = qnt;
            }
        }
    }
}
