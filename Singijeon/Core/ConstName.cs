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

        public const string TEST_SERVER = "1";

        public const string RECEIVE_REAL_CONDITION_INSERTED = "I";
        public const string RECEIVE_REAL_CONDITION_DELETE = "D";

        public const string SEND_ORDER_BUY = "편입종목매수";
        public const string RECEIVE_TR_DATA_HOGA = "주식호가요청"; 
        public const string RECEIVE_TR_DATA_BUY_INFO = "매수종목정보요청";
        public const string RECEIVE_TR_DATA_ACCOUNT_INFO = "계좌평가현황요청";

        public const string ORDER_JIJUNGGA = "00";
        public const string ORDER_SIJANGGA = "03";

        public const string RECEIVE_REAL_DATA_CONCLUSION = "주식체결";
        public const string RECEIVE_REAL_DATA_HOGA = "주식호가잔량";
        public const string RECEIVE_REAL_DATA_USUN_HOGA = "주식우선호가";
        public const string RECEIVE_CHEJAN_DATA_SUBMIT_OR_CONCLUSION = "0";
        public const string RECEIVE_CHEJAN_DATA_BALANCE              = "1";
        public const string RECEIVE_CHEJAN_DATA_SUBMIT = "접수";
        public const string RECEIVE_CHEJAN_DATA_CONCLUSION = "체결";
        public const string RECEIVE_CHEJAN_DATA_OK = "확인";

        public const string RECEIVE_CHEJAN_DATA_BUY = "매수";
        public const string RECEIVE_CHEJAN_DATA_SELL = "매도";
        public const string RECEIVE_CHEJAN_CANCEL_BUY_ORDER = "매수취소";
        public const string RECEIVE_CHEJAN_CANCEL_SELL_ORDER = "매도취소";

        public const string AUTO_TRADING_STATE_SEARCH_AND_CATCH = "종목포착";

        public const string AUTO_TRADING_STATE_BUY_BEFORE_ORDER = "매수주문접수시도";
        //public const string AUTO_TRADING_STATE_BUY_ORDER_COMPLETE = "매수주문접수완료";
        public const string AUTO_TRADING_STATE_BUY_NOT_COMPLETE = "매수주문완료";
        public const string AUTO_TRADING_STATE_BUY_NOT_COMPLETE_OUTCOUNT = "일부매수";
        public const string AUTO_TRADING_STATE_BUY_COMPLETE = "매수완료";

        public const string AUTO_TRADING_STATE_BUY_CANCEL_ALL = "매수취소완료";

        public const string AUTO_TRADING_STATE_SELL_BEFORE_ORDER = "매도주문접수시도";
        //public const string AUTO_TRADING_STATE_SELL_ORDER_COMPLETE = "매도주문접수완료";
        public const string AUTO_TRADING_STATE_SELL_NOT_COMPLETE = "매도주문완료";
        public const string AUTO_TRADING_STATE_SELL_NOT_COMPLETE_OUTCOUNT = "일부매도";
        public const string AUTO_TRADING_STATE_SELL_COMPLETE = "매도완료";

        public const string AUTO_TRADING_STATE_SELL_CANCEL_ALL = "매도취소완료";

        public const string AUTO_TRADING_STATE_CANCEL_ORDER = "주문취소시도";

        public const string AUTO_TRADING_STATE_TAKE_PROFIT_CANCEL = "익절취소";
        public const string AUTO_TRADING_STATE_STOPLOSS_CANCEL = "손절취소";

        public const string AUTO_TRADING_STATE_TAKE_PROFIT = "익절매도중";
        public const string AUTO_TRADING_STATE_STOPLOSS = "손절매도중";

        public const string AUTO_TRADING_STATE_CLEAR_NOT_COMPLETE = "청산중";
        public const string AUTO_TRADING_STATE_CLEAR_COMPLETE = "청산완료";

        public const string AUTO_TRADING_STATE_SELL_MONITORING = "매도감시";
        public const string AUTO_TRADING_STATE_CONCLUESION_COMPLETE = "매매완료";
    }
    public class CONST_NUMBER
    {
        public const int SEND_ORDER_BUY = 1;
        public const int SEND_ORDER_SELL = 2;

        public const int SEND_ORDER_CANCEL_BUY = 3;
        public const int SEND_ORDER_CANCEL_SELL = 4;

        public const int SEND_ORDER_MODIFY_BUY = 5;
        public const int SEND_ORDER_MODIFY_SELL = 6;
    }
    public enum ORDER_TYPE :int
    {
        JIJUNGGA = 0,
        SIJANGGA = 3,
        JOGUNBU_GIJUNGGA = 5,
        CHOIYURI_GIJUNGGA = 6,
        CHOIUSUN_GIJUNGGA = 7,
        JIJUNGGA_IOC = 10,
        SIJANGGA_IOC = 13,
        CHOIYURI_IOC = 16,
        JIJUNGGA_FOK = 20,
        SIJANGGA_FOK = 23,
        CHOIYURI_FOK = 26,
        JANGJUNSIGAN_JONGA = 61,
        SIGAN_DANILGA = 62,
        JANGHUSIGAN_JONGA = 81
    }
    public static class Martin_ErrorCode
    {
        public const int ERR_NONE = 1;
        public const int ALREADY_STRATEGY = -2;
        public const int NOT_VALID_PROFIT = -4;
        public const int RESTART_ON = -8;
        public const int BUY_ITEM_COUNT = -16;
    
    }
        public static class ErrorCode
    {
        public const int 정상처리 = 0;
        public const int 실패 = -10;
        public const int 사용자정보교환실패 = -100;
        public const int 서버접속실패 = -101;
        public const int 버전처리실패 = -102;
        public const int 개인방화벽실패 = -103;
        public const int 메모리보호실패 = -104;
        public const int 함수입력값오류 = -105;
        public const int 통신연결종료 = -106;
        public const int 시세조회과부하 = -200;
        public const int 전문작성초기화실패 = -201;
        public const int 전문작성입력값오류 = -202;
        public const int 데이터없음 = -203;
        public const int 조회가능한종목수초과 = -204; //한번에 조회 가능한 종목개수는 최대 100종목.        
        public const int 데이터수신실패 = -205;
        public const int 조회가능한FID수초과 = -206; //.한번에 조회 가능한 FID개수는 최대 100개.      
        public const int 실시간해제오류 = -207;
        public const int 입력값오류 = -300;
        public const int 계좌비밀번호없음 = -301;
        public const int 타인계좌사용오류 = -302;
        public const int 주문가격이20억원을초과 = -303;
        public const int 주문가격이50억원을초과 = -304;
        public const int 주문수량이총발행주수의1퍼센트초과오류 = -305;
        public const int 주문수량은총발행주수의3퍼센트초과오류 = -306;
        public const int 주문전송실패 = -307;
        public const int 주문전송과부하 = -308;
        public const int 주문수량300계약초과 = -309;
        public const int 주문수량500계약초과 = -310;
        public const int 주문전송제한과부하 = -311;
        public const int 계좌정보없음 = -340;
        public const int 종목코드없음 = -500;
    }

    class StrategyItemName
    {
        public const string STOPLOSS_SELL = "매매전략_손절";
        public const string TAKE_PROFIT_SELL = "매매전략_익절";
        public const string BUY_TIME_LIMIT = "매매시간설정";
        public const string BUY_GAP_CHECK = "갭상승추격매수";
    }
 }
