using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singijeon.Core
{
    public class OnReceivedLogMessageEventArgs : EventArgs
    {
        public String Message { get; set; }

        public OnReceivedLogMessageEventArgs(String Message)
        {
            this.Message = Message;
        }
    }

    public class OnReceivedUserInfoEventArgs : EventArgs
    {
        public UserInfo UserInfo { get; set; }
        public string Message { get; set; }

        public OnReceivedUserInfoEventArgs(UserInfo userInfo)
        {
            this.UserInfo = userInfo;
            this.Message = "사용자 정보를 수신하였습니다.";
        }
    }

    
}
