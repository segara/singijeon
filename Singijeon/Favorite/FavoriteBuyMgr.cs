using AxKHOpenAPILib;
using Singijeon;
using Singijeon.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class FavoriteBuyMgr
{
    AxKHOpenAPI axKHOpenAPI1;
    Form1 form1;

    public FavoriteBuyMgr ()
    {
       

    }
    
    public void Init(AxKHOpenAPI axKHOpenAPI, Form1 form)
    {
        this.axKHOpenAPI1 = axKHOpenAPI;
        this.form1 = form;
        //this.axKHOpenAPI1.OnReceiveChejanData += API_OnReceiveChejanData;
        this.axKHOpenAPI1.OnReceiveRealData += API_OnReceiveRealData;

    }

 

    private void API_OnReceiveRealData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveRealDataEvent e)
    {
        string itemCode = e.sRealKey.Trim();


        if (e.sRealType == ConstName.RECEIVE_REAL_DATA_CONCLUSION) //주식이 체결될 때 마다 실시간 데이터를 받음
        {
            string price = axKHOpenAPI1.GetCommRealData(itemCode, 10);    //현재가
            string lowPrice = axKHOpenAPI1.GetCommRealData(itemCode, 18); //저가
            string openPrice = axKHOpenAPI1.GetCommRealData(itemCode, 16); //시가

            long c_lPrice = Math.Abs(long.Parse(price));

           
        }
    }
}

