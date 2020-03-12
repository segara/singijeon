using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Singijeon.Core;
namespace Singijeon
{
    public class TickBongInfo
    {
        int saveCount = 0; //봉당 저장갯수 예)5틱봉 => 5개 30틱봉 =>30개
        int curSaveIndex = 0; //현재 저장횟수
        int sumPrice = 0;
        int average = 0;
        public TickBongInfo(int _saveCount)
        {
            saveCount = _saveCount;
        }

        public void AddPrice(int _price)
        {
            if(curSaveIndex >= saveCount)
            {
                CoreEngine.GetInstance().SendLogErrorMessage("저장갯수초과");
                return;
            }
            sumPrice += _price;
            curSaveIndex++;

            if (curSaveIndex == saveCount)
                average = CalAverage();
        }

        public int GetAverage()
        {
            return average;
        }

        public bool IsComplete()
        {
            if (curSaveIndex == saveCount)
                return true;

            return false;
        }

        private int CalAverage()
        {
            float returnValue = 0;
            if (curSaveIndex > 0)
            {
                returnValue = (float)sumPrice / (float)curSaveIndex;
            }

            CoreEngine.GetInstance().SendLogWarningMessage(sumPrice + "/" + curSaveIndex);

            if (returnValue == 0)
                CoreEngine.GetInstance().SendLogErrorMessage("평균값 0");

            if (curSaveIndex < saveCount)
                CoreEngine.GetInstance().SendLogErrorMessage("저장갯수 " + curSaveIndex);

            if(sumPrice == 0)
                CoreEngine.GetInstance().SendLogErrorMessage( "저장가격 " + sumPrice);

            
            return (int)returnValue;
        }
    }
}
