using Singijeon.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Singijeon
{
    public partial class Form2 : Form
    {
        CoreEngine coreEngine;
        AxKHOpenAPILib.AxKHOpenAPI axKHOpenAPI1;
        public Form2(AxKHOpenAPILib.AxKHOpenAPI _axKHOpenAPI1)
        {
            InitializeComponent();
            coreEngine = CoreEngine.GetInstance();
            coreEngine.OnReceivedLogMessage += OnReceiveLogMessage;

            axKHOpenAPI1 = _axKHOpenAPI1;
            axKHOpenAPI1.OnReceiveTrData += AxKHOpenAPI_OnReceiveTrData;
        }

        private void OnReceiveLogMessage(object sender, OnReceivedLogMessageEventArgs e)
        {
            LogListBox.Items.Add(e.Message);
            LogListBox.SelectedIndex = LogListBox.Items.Count - 1;
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
    }
}
