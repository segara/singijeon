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
        int curWarningIndex = 0;
        int curErrorIndex   = 0;
        int curMartinIndex = 0;

        long curProfit = 0;
        Thread taskWorker;
        delegate void CrossThreadSafetyUpdate(ListBox ctl);

        public Form2(AxKHOpenAPILib.AxKHOpenAPI _axKHOpenAPI1)
        {
            InitializeComponent();
            LogListBox.DrawItem += LogListBox_DrawItem;
            warningLogListBox.DrawItem += WarningLogListBox_DrawItem;
            coreEngine = CoreEngine.GetInstance();
            coreEngine.OnReceivedLogMessage += OnReceiveLogMessage;
            coreEngine.OnReceivedLogWarningMessage += OnReceiveLogWarningMessage;
            coreEngine.OnReceivedLogErrorMessage += OnReceiveLogErrorMessage;
            axKHOpenAPI1 = _axKHOpenAPI1;
            axKHOpenAPI1.OnReceiveTrData += AxKHOpenAPI_OnReceiveTrData;

            this.FormClosing += Form_FormClosing;

            logMessage = coreEngine.logMessage;

            Start();
        }
        private void Form_FormClosing(object sender, EventArgs e)
        {
            
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
            taskWorker.IsBackground = true;
            taskWorker.Start();
        }
        void Update()
        {
            if (martin_curStep_txt.InvokeRequired)
            {
                martin_curStep_txt.Invoke(new MethodInvoker(delegate ()
                {
                    martin_curStep_txt.Text = MartinGailManager.GetInstance().StepInner.ToString();
                    martin_max_try_txt.Text = MartinGailManager.GetInstance().MARTIN_MAX_STEP.ToString();

                    martin_win_txt.Text = MartinGailManager.GetInstance().WinCnt.ToString();
                    martin_lose_txt.Text = MartinGailManager.GetInstance().LoseCnt.ToString();

                    martin_profit.Text = MartinGailManager.GetInstance().ProfitMoney.ToString();
                    M_resultListBox.Items.Clear();
                    int index = 0;
                    while(index < MartinGailManager.GetInstance().TodayAllList().Count)
                    {
                        int todayIndex = MartinGailManager.GetInstance().TodayAllList()[index].TodayIndex;
                        int curStep = MartinGailManager.GetInstance().TodayAllList()[index].step;
                        MARTIN_RESULT result = MartinGailManager.GetInstance().TodayAllList()[index].martinState;
                        M_resultListBox.Items.Add(string.Format(" {0} | {1} | {2} ", todayIndex, curStep, result.ToString()));
                        index++;
                    }

                }));
            }
            else
            {
                martin_curStep_txt.Text = MartinGailManager.GetInstance().StepInner.ToString();
                martin_max_try_txt.Text = MartinGailManager.GetInstance().MARTIN_MAX_STEP.ToString();

                martin_win_txt.Text = MartinGailManager.GetInstance().WinCnt.ToString();
                martin_lose_txt.Text = MartinGailManager.GetInstance().LoseCnt.ToString();

                martin_profit.Text = MartinGailManager.GetInstance().ProfitMoney.ToString();
                M_resultListBox.Items.Clear();
                int index = 0;
                while (curMartinIndex < MartinGailManager.GetInstance().TodayAllList().Count)
                {
                    int todayIndex = MartinGailManager.GetInstance().TodayAllList()[index].TodayIndex;
                    int curStep = MartinGailManager.GetInstance().TodayAllList()[index].step;
                    MARTIN_RESULT result = MartinGailManager.GetInstance().TodayAllList()[index].martinState;
                    M_resultListBox.Items.Add(string.Format(" {0} | {1} | {2} ", todayIndex, curStep, result.ToString()));
                    index++;
                }

            }
              
        }

        private void OnReceiveLogMessage(object sender, OnReceivedLogMessageEventArgs e)
        {
            logMessage.Add(new LogItem(e.Message));
            //coreEngine.SaveLogMessage(e.Message);
            if (LogListBox.InvokeRequired)
            {
                LogListBox.Invoke(new MethodInvoker(delegate ()
                {
                    while (curLogIndex < logMessage.Count)
                    {
                        LogListBox.Items.Add(logMessage[curLogIndex]);
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
                    LogListBox.Items.Add(logMessage[curLogIndex]);
                    //LogListBox.SelectedIndex = LogListBox.Items.Count - 1;
                    curLogIndex++;
                }
                //CheckLogLength();
            }
        }

        private void OnReceiveLogWarningMessage(object sender, OnReceivedLogMessageEventArgs e)
        {
            logMessage.Add(new LogItem(e.Message, LOG_TYPE.WARNING));
            //coreEngine.SaveLogMessage(e.Message);

            List<LogItem> warningItem = logMessage.FindAll(o => o.logType == LOG_TYPE.WARNING);

            if (LogListBox.InvokeRequired)
            {
                LogListBox.Invoke(new MethodInvoker(delegate ()
                {
                    while (curLogIndex < logMessage.Count)
                    {

                        LogListBox.Items.Add(logMessage[curLogIndex]);
                        curLogIndex++;
                    }
                 
                }));

                warningLogListBox.Invoke(new MethodInvoker(delegate ()
                {
                    while (curWarningIndex < warningItem.Count)
                    {

                        warningLogListBox.Items.Add(warningItem[curWarningIndex].logTxt);
                        curWarningIndex++;
                    }

                }));
            }
            else
            {
                while (curLogIndex < logMessage.Count)
                {
                    LogListBox.Items.Add(logMessage[curLogIndex]);
                    curLogIndex++;
                }
             
                while (curWarningIndex < warningItem.Count)
                {

                    warningLogListBox.Items.Add(warningItem[curWarningIndex].logTxt);
                    curWarningIndex++;
                }

            }
        }

        private void OnReceiveLogErrorMessage(object sender, OnReceivedLogMessageEventArgs e)
        {
            logMessage.Add(new LogItem(e.Message, LOG_TYPE.ERROR));
            //coreEngine.SaveLogMessage(e.Message);
            List<LogItem> errorItem = logMessage.FindAll(o => o.logType == LOG_TYPE.ERROR);
            if (LogListBox.InvokeRequired)
            {
                LogListBox.Invoke(new MethodInvoker(delegate ()
                {
                    while (curLogIndex < logMessage.Count)
                    {
                       
                        LogListBox.Items.Add(logMessage[curLogIndex]);
                        //LogListBox.SelectedIndex = LogListBox.Items.Count - 1;
                        curLogIndex++;
                    }
                    //CheckLogLength();
                }));
                errorListBox.Invoke(new MethodInvoker(delegate ()
                {
                    while (curErrorIndex < errorItem.Count)
                    {
                        errorListBox.Items.Add(errorItem[curErrorIndex].logTxt);
                        curErrorIndex++;
                    }

                }));
            }
            else
            {
                while (curLogIndex < logMessage.Count)
                {
                    LogListBox.Items.Add(logMessage[curLogIndex]);
                    curLogIndex++;
                }
                while (curErrorIndex < errorItem.Count)
                {

                    errorListBox.Items.Add(errorItem[curErrorIndex].logTxt);
                    curErrorIndex++;
                }
            }
        }

        public void AddProfit(long l_profit)
        {
            curProfit += l_profit;
            coreEngine.SendLogWarningMessage("손익:"+curProfit);
            if (profit_label.InvokeRequired)
            {
                profit_label.Invoke(new MethodInvoker(delegate ()
                {
                    profit_label.Text = string.Format("{0:n0}", curProfit);
                }));
            }
            else
            {
                profit_label.Text = string.Format("{0:n0}", curProfit);
            }
         
        }

        private void AxKHOpenAPI_OnReceiveTrData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            if (e.sRQName.Contains(ConstName.RECEIVE_TR_DATA_ACCOUNT_INFO_FORM2))
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
            warningLogListBox.Items.Clear();
            curLogIndex = 0;
            curWarningIndex = 0;
        }

        private void WarningLogBtn_Click(object sender, EventArgs e)
        {
            
        }

        private void LogListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;
            var item = LogListBox.Items[e.Index] as LogItem;
            Color logColor = Color.Black;
            if (item.logType == LOG_TYPE.ERROR)
            {
                logColor = Color.Red;
            }
            else if (item.logType == LOG_TYPE.WARNING)
            {
                logColor = Color.Blue;
            }

            if (item != null)
            {
                e.Graphics.DrawString(
                    item.logTxt,
                    e.Font,
                    new SolidBrush(logColor),
                    e.Bounds);
            }
        }
        private void WarningLogListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;
            var item = warningLogListBox.Items[e.Index] as LogItem;
            Color logColor = Color.Black;
            if (item.logType == LOG_TYPE.ERROR)
            {
                logColor = Color.Red;
            }
            else if (item.logType == LOG_TYPE.WARNING)
            {
                logColor = Color.Blue;
            }

            if (item != null)
            {
                e.Graphics.DrawString(
                    item.logTxt,
                    e.Font,
                    new SolidBrush(logColor),
                    e.Bounds);
            }
        }

        private void accountRefreshBtn_Click(object sender, EventArgs e)
        {
            axKHOpenAPI1.SetInputValue("계좌번호", Form1.account);
            axKHOpenAPI1.SetInputValue("비밀번호", "");
            axKHOpenAPI1.SetInputValue("상장폐지조회구분", "0");
            axKHOpenAPI1.SetInputValue("비밀번호입력매체구분", "00");
            axKHOpenAPI1.CommRqData(ConstName.RECEIVE_TR_DATA_ACCOUNT_INFO_FORM2, "OPW00004", 0, Form1.GetScreenNum().ToString());

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
