using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singijeon.Core
{
    public class UserInfo
    {
        public bool LogIn { get; set; }
        public string UserName { get; set; } //사용자명
        public string UserId { get; set; } //사용자 아이디
        public string[] Accounts { get; set; } //계좌목록
        public string ConnectedServer { get; set; } //접속서버 - 모의투자 or 실서버
    }

    public class ChartData
    {
        public double HighPrice { get; set; } //고가
        public double LowPrice { get; set; } //저가
        public double OpenPrice { get; set; } //시가
        public double ClosePrice { get; set; } //종가
        public long Volume { get; set; } //거래량
        public string Time { get; set; } //시간

        public ChartData(double highPrice, double lowPrice, double openPrice, double closePrice, long volume, string time)
        {
            this.HighPrice = highPrice;
            this.LowPrice = lowPrice;
            this.OpenPrice = openPrice;
            this.ClosePrice = closePrice;
            this.Volume = volume;
            this.Time = time;
        }
    }

    public class Balance
    {
        public string ItemCode { get; set; } //종목코드
        public string ItemName { get; set; } //종목명
        public long HoldingQuantity { get; set; } //보유수량
        public double BuyingPrice { get; set; } //매입단가
        public long BuyingAmount { get; set; } //매입금액
        public long CurrentPrice { get; set; } //현재가
        public long EvaluatedAmount { get; set; } //평가금액
        public long ProfitAmount { get; set; } //손익금액
        public double ProfitRate { get; set; } //손익률
    }

    [Serializable]
    public class Group //관심종목 그룹
    {
        public static int lastID = 0;

        public int _ID { get; set; } //그룹번호
        public string Name { get; set; } //그룹명
        public List<StockItem> InterestItemList { get; set; } //관심종목 리스트

        public Group()
        {
            InterestItemList = new List<StockItem>();

            lastID++;
            _ID = lastID;
        }
    }
}
