using AxKHOpenAPILib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singijeon.Core
{
    class CoreEngine
    {
        AxKHOpenAPI axKHOpenAPI;
        public UserInfo userInfo;

        private static CoreEngine coreInstance;
        public RequestTrDataManager requestTrDataManager;

        //Events
        public event EventHandler<OnReceivedLogMessageEventArgs> OnReceivedLogMessage; //로그 수신 시
        public event EventHandler<OnReceivedUserInfoEventArgs> OnReceivedUserInfo; //사용자 정보 수신 시
       

        private CoreEngine()
        {
            requestTrDataManager = RequestTrDataManager.GetInstance();
            requestTrDataManager.Run();

        }
        public static CoreEngine GetInstance() //싱글톤으로 jost 객체관리
        {
            if (coreInstance == null)
                coreInstance = new CoreEngine();

            return coreInstance;
        }

        public void SetAxKHOpenAPI(AxKHOpenAPI axKHOpenAPI)
        {
            this.axKHOpenAPI = axKHOpenAPI;

            axKHOpenAPI.OnEventConnect += AxKHOpenAPI_OnEventConnect;
            axKHOpenAPI.OnReceiveTrData += axKHOpenAPI_OnReceiveTrData;
            //axKHOpenAPI.OnReceiveRealData += axKHOpenAPI_OnReceiveRealData;

        }

        public AxKHOpenAPI GetAxKHOpenAPI()
        {
            return axKHOpenAPI;

        }

        public void Start()
        {
            if (axKHOpenAPI != null)
                axKHOpenAPI.CommConnect();
            else
            {
                string message = " axKHOpenAPI를 등록해주세요.";
                SendLogMessage(message);
            }
        }


        public void SendLogMessage(string logMessage) //Event를 이용해 로그 메세지 전달
        {

            StackFrame callStack = new StackFrame(1, true);
            logMessage = DateTime.Now.ToString("[HH:mm:ss] ") + logMessage + " ("+ Path.GetFileName(callStack.GetFileName())+ ") line : "+ callStack.GetFileLineNumber();
            OnReceivedLogMessage?.Invoke(this, new OnReceivedLogMessageEventArgs(logMessage));

            //함수들의 인자(object sender, OnReceivedLogMessageEventArgs e)
            //SendLogMessage메세지가 실행되면 OnReceivedLogMessage에 등록되있던 함수들이 같이 실행됨
        }

        private void AxKHOpenAPI_OnEventConnect(object sender, _DKHOpenAPIEvents_OnEventConnectEvent e)
        {
            if (e.nErrCode == ErrorCode.정상처리)
            {
                GetUserInfo();
                SendLogMessage("로그인 성공");
            }
            else if (e.nErrCode == ErrorCode.사용자정보교환실패)
                SendLogMessage("로그인 실패 : 사용자 정보교환 실패");
            else if (e.nErrCode == ErrorCode.서버접속실패)
                SendLogMessage("로그인 실패 : 서버접속 실패");
            else if (e.nErrCode == ErrorCode.버전처리실패)
                SendLogMessage("로그인 실패 : 버전처리 실패");
        }

        public void GetUserInfo()
        {
            string userName = axKHOpenAPI.GetLoginInfo("USER_NAME");
            string userId = axKHOpenAPI.GetLoginInfo("USER_ID");
            string accountList = axKHOpenAPI.GetLoginInfo("ACCLIST");
            string server = string.Empty;

            if (axKHOpenAPI.GetLoginInfo("GetServerGubun").Equals("1"))
                server = "모의서버";
            else
                server = "실서버";

            string[] accounts = accountList.Split(';');

            this.userInfo = new UserInfo()
            {
                LogIn = true,
                UserName = userName,
                UserId = userId,
                Accounts = accounts,
                ConnectedServer = server
            };

            OnReceivedUserInfo?.Invoke(this, new OnReceivedUserInfoEventArgs(userInfo));
        }


        public  void axKHOpenAPI_OnReceiveTrData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            if (e.sRQName.Contains(ConstName.RECEIVE_TR_DATA_HOGA))
            {
                StockWithBiddingEntity newObj = new StockWithBiddingEntity();

                string[] rqNameArray = e.sRQName.Split(':');
                if (rqNameArray.Length == 3)
                {
                    string itemCode = rqNameArray[2];
                    newObj.Code = itemCode;
                }
                for (int i = 0; i < 10; i++)
                {
                    if (i == 9)
                    {
                        Console.WriteLine("매도 최우선호가 / 잔량 ");
                        string firstSellHoga = axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, 0, "매도최우선호가").Trim().Replace("+", "");
                        string firstSellQnt  = axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, 0, "매도최우선잔량").Trim();
                        if (!string.IsNullOrEmpty(firstSellHoga))
                            newObj.SetSellHoga(i, long.Parse(firstSellHoga));
                        if (!string.IsNullOrEmpty(firstSellQnt))
                            newObj.SetSellQnt(i, long.Parse(firstSellQnt));
                    }
                    else
                    {
                        Console.WriteLine("매도" + (10 - i) + "차선호가 /잔량");
                        string hoga = (axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, 0, "매도" + (10 - i) + "차선호가").Trim().Replace("+",""));
                        string qnt = (axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, 0, "매도" + (10 - i) + "차선잔량").Trim());
                        
                        if(!string.IsNullOrEmpty(hoga))
                            newObj.SetSellHoga(i, long.Parse(hoga));
                        if (!string.IsNullOrEmpty(qnt))
                            newObj.SetSellQnt(i, long.Parse(qnt));
                    }
                }
                for (int i = 0; i < 10; i++)
                {
                    if (i == 0)
                    {
                        Console.WriteLine("매수 최우선호가 / 잔량 ");
                        string firstBuyHoga = (axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, 0, "매수최우선호가").Trim().Replace("+", ""));
                        string firstBuyQnt = (axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, 0, "매수최우선잔량").Trim());
                        if (!string.IsNullOrEmpty(firstBuyHoga))
                            newObj.SetBuyHoga(i, long.Parse(firstBuyHoga));
                        if (!string.IsNullOrEmpty(firstBuyQnt))
                            newObj.SetBuyQnt(i, long.Parse(firstBuyQnt));
                    }
                    else
                    {
                        Console.WriteLine("매수" + (i + 1) + "차선호가 / 잔량");
                        string hoga =  (axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, 0, "매수" + (i + 1) + "차선호가").Trim().Replace("+", ""));
                        string qnt = (axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, 0, "매수" + (i + 1) + "차선잔량").Trim());
                        if (!string.IsNullOrEmpty(hoga))
                            newObj.SetBuyHoga(i, long.Parse(hoga));
                        if (!string.IsNullOrEmpty(qnt))
                            newObj.SetBuyQnt(i, long.Parse(qnt));
                    }
                }
                StockWithBiddingManager.GetInstance().SetItem(newObj);
            }
        }

        public void axKHOpenAPI_OnReceiveRealData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveRealDataEvent e)
        {
            string itemCode = e.sRealKey.Trim();

            if (e.sRealType.Contains(ConstName.RECEIVE_REAL_DATA_HOGA))
            {
                StockWithBiddingEntity newObj = new StockWithBiddingEntity();
                newObj.Code = itemCode;

                int maxAmount = 0;
                for (int i = 0; i < 10; i++)
                {
                    string sellhoga = axKHOpenAPI.GetCommRealData(e.sRealType, 50 - i).Trim().Replace("+", "");
                    string sellqnt = axKHOpenAPI.GetCommRealData(e.sRealType, 70 - i).Trim();
                    //잔량대비 = axKHOpenAPI1.GetCommRealData(e.sRealType, 90 - i).Trim(),
                    if (!string.IsNullOrEmpty(sellhoga))
                        newObj.SetSellHoga(i, long.Parse(sellhoga));
                    if (!string.IsNullOrEmpty(sellqnt))
                        newObj.SetSellQnt(i, long.Parse(sellqnt));


                    string buyhoga = axKHOpenAPI.GetCommRealData(e.sRealType, 51 + i).Trim().Replace("+", "");
                    string buyqnt = axKHOpenAPI.GetCommRealData(e.sRealType, 71 + i).Trim();
                    if (!string.IsNullOrEmpty(buyhoga))
                        newObj.SetBuyHoga(i, long.Parse(buyhoga));
                    if (!string.IsNullOrEmpty(buyqnt))
                        newObj.SetBuyQnt(i, long.Parse(buyqnt));
                    //    잔량대비 = axKHOpenAPI1.GetCommRealData(e.sRealType, 91 + i).Trim(),

                    //int sellAmount = int.Parse(axKHOpenAPI.GetCommRealData(e.sRealType, 70 - i).Trim());
                    //int buyAmount = int.Parse(axKHOpenAPI.GetCommRealData(e.sRealType, 71 + i).Trim());

                    //if (maxAmount < sellAmount)
                    //    maxAmount = sellAmount;
                    //if (maxAmount < buyAmount)
                    //    maxAmount = buyAmount;

                }

                StockWithBiddingManager.GetInstance().SetItem(newObj);

            }
        }
    }



}
