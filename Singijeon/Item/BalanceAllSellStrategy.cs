using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singijeon.Item
{
  
    public class BalanceAllSellStrategy
    {
        public bool usingStrategy = false;
        public string profitOrderOption; //현재가 or 시장가 등
   
        public BalanceAllSellStrategy(
             string _sellProfitOrderOption
            )
        {
            this.usingStrategy = true;
            this.profitOrderOption = _sellProfitOrderOption;
        }

        public void StartStrategy()
        {
            usingStrategy = true;
        }

        public void StopStrategy()
        {
            usingStrategy = false;
        }
    }
}
