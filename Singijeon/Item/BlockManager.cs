using AxKHOpenAPILib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Singijeon.Core;
namespace Singijeon.Item
{
    public class MicroOrder
    {
        public string itemCode;
        public int orderQnt;
    }
    public class BlockManager
    {
        private static BlockManager Instance;

        public static BlockManager GetInstance()
        {
            if (Instance == null)
            {
                Instance = new BlockManager();
            }
            return Instance;
        }

        AxKHOpenAPI axKHOpenAPI1;
        tradingStrategyGridView form1;

        

        public void Init(AxKHOpenAPI axKHOpenAPI, tradingStrategyGridView form)
        {
            this.axKHOpenAPI1 = axKHOpenAPI;
            this.form1 = form;
            this.axKHOpenAPI1.OnReceiveChejanData += API_OnReceiveChejanData;
        }

        private void API_OnReceiveChejanData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveChejanDataEvent e)
        {
            CoreEngine.GetInstance().SendLogMessage("API_OnReceiveChejanData");
            if (e.sGubun.Equals(ConstName.RECEIVE_CHEJAN_DATA_SUBMIT_OR_CONCLUSION))
            {
                CoreEngine.GetInstance().SendLogMessage("RECEIVE_CHEJAN_DATA_SUBMIT_OR_CONCLUSION");
                string orderState = axKHOpenAPI1.GetChejanData(913).Trim();
                string orderQuantity = axKHOpenAPI1.GetChejanData(900).Trim();
                string outstanding = axKHOpenAPI1.GetChejanData(902).Trim();
                string orderType = axKHOpenAPI1.GetChejanData(905).Replace("+", "").Replace("-", "").Trim();
                string ordernum = axKHOpenAPI1.GetChejanData(9203).Trim();
                string itemCode = axKHOpenAPI1.GetChejanData(9001).Replace("A", "");

                string conclusionPrice = axKHOpenAPI1.GetChejanData(910).Trim();
                string conclusionQuantity = axKHOpenAPI1.GetChejanData(911).Trim();

                if (orderState.Equals(ConstName.RECEIVE_CHEJAN_DATA_SUBMIT))
                {
                    CoreEngine.GetInstance().SendLogMessage("RECEIVE_CHEJAN_DATA_SUBMIT");
                    if (orderType.Equals(ConstName.RECEIVE_CHEJAN_DATA_BUY))
                    {
                        CoreEngine.GetInstance().SendLogMessage("RECEIVE_CHEJAN_DATA_BUY : " + ordernum);
                        CoreEngine.GetInstance().SendLogWarningMessage("conclusionQuantity : " + conclusionQuantity);
                    }
                    else if (orderType.Equals(ConstName.RECEIVE_CHEJAN_DATA_SELL))
                    {
                        CoreEngine.GetInstance().SendLogMessage("RECEIVE_CHEJAN_DATA_SELL");
                    }
                    else if (orderType.Equals(ConstName.RECEIVE_CHEJAN_CANCEL_BUY_ORDER))
                    {
                        CoreEngine.GetInstance().SendLogMessage("RECEIVE_CHEJAN_CANCEL_BUY_ORDER");
                    }
                    else if (orderType.Equals(ConstName.RECEIVE_CHEJAN_CANCEL_SELL_ORDER))
                    {
                        CoreEngine.GetInstance().SendLogMessage("RECEIVE_CHEJAN_CANCEL_SELL_ORDER");
                    }
                }
                else if (orderState.Equals(ConstName.RECEIVE_CHEJAN_DATA_CONCLUSION))
                {
                    CoreEngine.GetInstance().SendLogMessage("RECEIVE_CHEJAN_DATA_CONCLUSION");
                    if (orderType.Contains(ConstName.RECEIVE_CHEJAN_DATA_BUY))
                    {
                        CoreEngine.GetInstance().SendLogMessage("RECEIVE_CHEJAN_DATA_BUY");
                       
                        CoreEngine.GetInstance().SendLogWarningMessage("RECEIVE_CHEJAN_DATA_BUY ORDER NUM : " + ordernum);
                        CoreEngine.GetInstance().SendLogWarningMessage("conclusionQuantity : " + conclusionQuantity);

                       
                    }
                    else if (orderType.Contains(ConstName.RECEIVE_CHEJAN_DATA_SELL))
                    {
                       
                    }
                }
                else if (orderState.Equals(ConstName.RECEIVE_CHEJAN_DATA_OK))
                {
                    CoreEngine.GetInstance().SendLogMessage("RECEIVE_CHEJAN_DATA_OK");
                    if (orderType.Contains(ConstName.RECEIVE_CHEJAN_CANCEL_BUY_ORDER))
                    {
                        CoreEngine.GetInstance().SendLogMessage("RECEIVE_CHEJAN_CANCEL_BUY_ORDER");
                        if (int.Parse(outstanding) == 0)
                        {
                          
                        }
                    }
                    else if (orderType.Contains(ConstName.RECEIVE_CHEJAN_CANCEL_SELL_ORDER))
                    {
                        CoreEngine.GetInstance().SendLogMessage("RECEIVE_CHEJAN_CANCEL_SELL_ORDER");
                        if (int.Parse(outstanding) == 0)
                        {
                        }
                    }
                }
            }
            else if (e.sGubun.Equals(ConstName.RECEIVE_CHEJAN_DATA_BALANCE))
            {
            }
        }
    }
}
