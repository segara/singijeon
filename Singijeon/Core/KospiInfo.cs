using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Singijeon.Core
{
    public class KospiInfo
    {
        public String[] GetStock()
        {
            String strURL = "http://vip.mk.co.kr/newst/include/incMainIncludeChart2009.php?hdate1=20100422115311";

            String tempStr = GetHtmlString(strURL);
            
            String[] STock = new String[3];
            if (string.IsNullOrEmpty(tempStr))
                return STock;
            String[] SplitStr = tempStr.Split(':');
            int count = 0;

            foreach (String s in SplitStr)
            {
                switch (s)
                {
                    case "코스피":
                        STock[0] = SplitStr[count + 1].ToString() + " ";
                        STock[0] += SplitStr[count + 2].ToString() + " ";
                        STock[0] += SplitStr[count + 3].ToString() + " ";
                        STock[0] += SplitStr[count + 4].ToString() + " ";
                        break;

                    case "코스닥":
                        STock[1] = SplitStr[count + 1].ToString() + " ";
                        STock[1] += SplitStr[count + 2].ToString() + " ";
                        STock[1] += SplitStr[count + 3].ToString() + " ";
                        STock[1] += SplitStr[count + 4].ToString() + " ";
                        break;

                    case "원달러환율":
                        STock[2] = SplitStr[count + 1].ToString() + " ";
                        STock[2] += SplitStr[count + 2].ToString() + " ";
                        STock[2] += SplitStr[count + 3].ToString() + " ";
                        STock[2] += SplitStr[count + 4].ToString() + " ";
                        break;
                }
                count++;
            }
            return STock;
        }
        public String GetStockKospi()
        {
            string[] STock = GetStock();
            return STock[0];
        }
        public String GetStockKosdaq()
        {
            string[] STock = GetStock();
            return STock[1];
        }
        private String GetHtmlString(String url)
        {
            try {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.Default);
                String strHtml = reader.ReadToEnd();

                //strHtml = Regex.Replace(strHtml, @"<(.|\n)*?>", String.Empty);
                strHtml = Regex.Replace(strHtml, @"<(.|\n)*?>", String.Empty);
                strHtml = strHtml.Replace(" ", "").Replace("\t", "").Replace("//-->", "");
                String[] str = strHtml.Split(new Char[] { '\n' });
                strHtml = null;
                foreach (String s in str)
                {
                    if (s.Trim() != "")
                        strHtml += s + ":";
                }

                reader.Close();
                response.Close();

                return strHtml;
            }
            catch (Exception exception)
            {
                CoreEngine.GetInstance().SendLogErrorMessage(exception.Message);
                return string.Empty;
            }
               

          
        }
    }
}
