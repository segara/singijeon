﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace Singijeon
{
       
    public class TradingItem 
    {
        public TradingStrategy ts;
        public string buyOrderNum;
        public string sellOrderNum;
        public string orderType;
        public string itemCode;
        public string itemName;
        public long buyingPrice;
        public int buyingQnt;

        public int trailingTickCnt;

        public long curPrice;

        public bool IsSold; //매도주문 여부
        public bool IsCompleteBuying; //매수완료 여부

        public DataGridViewRow ui_rowItem;
        public string conditionUid;

        public TradingItem(TradingStrategy tsItem, string itemCode, long buyingPrice, int buyingQnt, bool completeBuying = false, bool sold = false, string orderType = "")
        {
            this.ts = tsItem;
            this.itemCode = itemCode;
            this.buyingPrice = buyingPrice;
            this.buyingQnt = buyingQnt;
            this.IsCompleteBuying = false;
            this.IsSold = false;

            this.buyOrderNum = string.Empty;
            this.sellOrderNum = string.Empty;

            this.orderType = orderType;
        }
        public void UpdateCurrentPrice(long _price)
        {
            this.curPrice = _price;
        }
        public void SetUiConnectRow(DataGridViewRow row)
        {
            this.ui_rowItem = row;
        }
        public void SetConditonUid(string uid)
        {
            this.conditionUid = uid;
        }
    }
}