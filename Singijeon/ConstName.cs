using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singijeon
{
    class ConstName
    {
        public const string GET_SERVER_TYPE = "GetServerGubun";
        public const string GET_ACCOUNT_LIST = "ACCLIST";

        public const string RECEIVE_REAL_CONDITION_INSERTED = "I";
        public const string RECEIVE_REAL_CONDITION_DELETE = "D";

        public const string RECEIVE_TR_DATA_BUY_INFO = "매수종목정보요청";
        public const string RECEIVE_TR_DATA_ACCOUNT_INFO = "계좌평가현황요청";

        public const string RECEIVE_REAL_DATA_CONCLUSION = "주식체결";

        public const string RECEIVE_CHEJAN_DATA_SUBMIT_OR_CONCLUSION = "0";
        public const string RECEIVE_CHEJAN_DATA_BALANCE              = "1";
        public const string RECEIVE_CHEJAN_DATA_SUBMIT = "접수";
        public const string RECEIVE_CHEJAN_DATA_CONCLUSION = "체결";

        public const string RECEIVE_CHEJAN_DATA_BUY = "매수";
        public const string RECEIVE_CHEJAN_DATA_SELL = "매도";

        public const string AUTO_TRADING_STATE_BUY_NOT_COMPLETE = "매수중";
        public const string AUTO_TRADING_STATE_SELL_NOT_COMPLETE = "매도중";
        public const string AUTO_TRADING_STATE_BUY_COMPLETE = "매수완료";
        public const string AUTO_TRADING_STATE_SELL_COMPLETE = "매도완료";
        public const string AUTO_TRADING_STATE_TAKE_PROFIT = "익절매도중";
        public const string AUTO_TRADING_STATE_STOPLOSS = "손절매도중";
        public const string AUTO_TRADING_STATE_CLEAR_NOT_COMPLETE = "청산중";
        public const string AUTO_TRADING_STATE_CLEAR_COMPLETE = "청산완료";
        public const string AUTO_TRADING_STATE_SELL_MONITORING = "매도감시";
        public const string AUTO_TRADING_STATE_CONCLUESION_COMPLETE = "매매완료";
    }
}
