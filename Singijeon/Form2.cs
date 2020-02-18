using Singijeon.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Singijeon { 
 
    public partial class Form2 : Form
    {
        CoreEngine coreEngine;
        AxKHOpenAPILib.AxKHOpenAPI axKHOpenAPI1;
        List<LogItem> logMessage = new List<LogItem>();
        int curLogIndex = 0;
        Thread taskWorker;
        delegate void CrossThreadSafetyUpdate(ListBox ctl);

        public Form2(AxKHOpenAPILib.AxKHOpenAPI _axKHOpenAPI1)
        {
            InitializeComponent();
            coreEngine = CoreEngine.GetInstance();
            coreEngine.OnReceivedLogMessage += OnReceiveLogMessage;

            axKHOpenAPI1 = _axKHOpenAPI1;
            axKHOpenAPI1.OnReceiveTrData += AxKHOpenAPI_OnReceiveTrData;

            Start();
        }
        private void Start()
        {
            taskWorker = new Thread(delegate ()
            {
                while (true)
                {
                    try
                    {
                        Update();
                        Thread.Sleep(1000); //기본 실행 주기

                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception.Message);
                    }
                }
            });
            taskWorker.Start();
        }
        void Update()
        {

          
            if (martin_curStep_txt.InvokeRequired)
            {
                martin_curStep_txt.Invoke(new MethodInvoker(delegate ()
                {
                    martin_curStep_txt.Text = MartinGailManager.GetInstance().StepInner.ToString();
                    martin_max_try_txt.Text = MartinGailManager.MARTIN_MAX_STEP.ToString();

                    martin_win_txt.Text = MartinGailManager.GetInstance().WinCnt.ToString();
                    martin_lose_txt.Text = MartinGailManager.GetInstance().LoseCnt.ToString();

                    martin_profit.Text = MartinGailManager.GetInstance().ProfitMoney.ToString();
                }));
            }
            else
            {
                martin_curStep_txt.Text = MartinGailManager.GetInstance().StepInner.ToString();
                martin_max_try_txt.Text = MartinGailManager.MARTIN_MAX_STEP.ToString();

                martin_win_txt.Text = MartinGailManager.GetInstance().WinCnt.ToString();
                martin_lose_txt.Text = MartinGailManager.GetInstance().LoseCnt.ToString();

                martin_profit.Text = MartinGailManager.GetInstance().ProfitMoney.ToString();
            }
              
        }

        private void OnReceiveLogMessage(object sender, OnReceivedLogMessageEventArgs e)
        {
            logMessage.Add(new LogItem(e.Message));
            coreEngine.SaveLogMessage(e.Message);
            if (LogListBox.InvokeRequired)
            {
                LogListBox.Invoke(new MethodInvoker(delegate ()
                {
                    while (curLogIndex < logMessage.Count)
                    {
                        LogListBox.Items.Add(logMessage[curLogIndex].logTxt);
                        //LogListBox.SelectedIndex = LogListBox.Items.Count - 1;
                        curLogIndex++;
                    }
                    //CheckLogLength();
                }));
            }
            else
            {
                while (curLogIndex < logMessage.Count)
                {
                    LogListBox.Items.Add(logMessage[curLogIndex].logTxt);
                    //LogListBox.SelectedIndex = LogListBox.Items.Count - 1;
                    curLogIndex++;
                }
                //CheckLogLength();
            }
        }

        
        private void AxKHOpenAPI_OnReceiveTrData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            if (e.sRQName.Contains(ConstName.RECEIVE_TR_DATA_ACCOUNT_INFO))
            {
                string accountName = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "계좌명");
                string bankName = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "지점명");
                string asset = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "예수금");
                string d2Asset = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "D+2추정예수금");
                string estimatedAsset = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "예탁자산평가액");
                string investment = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "당일투자원금");
                string profit = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "당일투자손익");
                string profitRate = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "당일손익율");

                long l_asset = 0;
                long l_d2asset = 0;
                long l_estimatedAsset = 0;
                long l_investment = 0;
                long l_profit = 0;
                double d_profitRate = 0;

                long.TryParse(asset, out l_asset);
                long.TryParse(d2Asset, out l_d2asset);
                long.TryParse(estimatedAsset, out l_estimatedAsset);
                long.TryParse(investment, out l_investment);
                long.TryParse(profit, out l_profit);

                double.TryParse(profitRate, out d_profitRate);

                asset_label.Text = string.Format("{0:n0}", l_asset);
                d2Asset_label.Text = string.Format("{0:n0}", l_d2asset);
                estimatedAsset_label.Text = string.Format("{0:n0}", l_estimatedAsset);
                investment_label.Text = string.Format("{0:n0}", l_investment);
                profit_label.Text = string.Format("{0:n0}", l_profit);

                profitRate_label.Text = d_profitRate.ToString();
            }
        }

        private void CheckLogLength()
        {
            if (logMessage.Count > 200)
                logMessage.RemoveRange(0, 100);
            if (LogListBox.Items.Count > 200)
            {
                for (int i = LogListBox.Items.Count - 1; i >= 100; i--)
                {
                    LogListBox.Items.RemoveAt(i);
                }
            }

            curLogIndex = logMessage.Count;

        }

        private void Button1_Click(object sender, EventArgs e)
        {
            logMessage.Clear();
            LogListBox.Items.Clear();
            curLogIndex = 0;
        }
    }
    public enum LOG_TYPE
    {
        NORMAL,
        WARNING,
        ERROR
    }
    public class LogItem
    {
        public LOG_TYPE logType;
        public string logTxt;
        public LogItem(string msg, LOG_TYPE type = LOG_TYPE.NORMAL)
        {
            logTxt = msg;
            logType = type;
        }
    }
}
