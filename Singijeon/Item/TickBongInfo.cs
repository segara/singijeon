using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Singijeon.Core;
namespace Singijeon
{
    [Serializable]
    public class TickBongInfoMgr
    {
        public Stack<TickBongInfo> tickBongInfoList = new Stack<TickBongInfo>();
        public TickBongInfo curTickBong = null;
        public int settingMaxCount;
        public TickBongInfoMgr(int _settingMaxCount)
        {
            settingMaxCount = _settingMaxCount;
            tickBongInfoList.Push(new TickBongInfo(_settingMaxCount));
            curTickBong = tickBongInfoList.Peek();
        }
        public void Clear()
        {
            tickBongInfoList.Clear();
            tickBongInfoList.Push(new TickBongInfo(settingMaxCount));
            curTickBong = tickBongInfoList.Peek();
        }
        public bool IsCompleteBong (int index)
        {
            int idx = 0;
        
            foreach (var bongItem in tickBongInfoList)
            {
                if (idx == index)
                {
                    return bongItem.IsComplete();
                }
                idx++;
            }
            return false;
        }
        public void AddPrice(double _price)
        {
             if(curTickBong.IsComplete())
             {
                tickBongInfoList.Push(new TickBongInfo(settingMaxCount));
                curTickBong = tickBongInfoList.Peek();
            }
             curTickBong.AddPrice(_price);
        }
        public TickBongInfo GetTickBong(int index)
        {
            int idx = 0;
            TickBongInfo bong = null;
            foreach (var bongItem in tickBongInfoList)
            {
                if (idx == index && bongItem.IsComplete())
                {
                    bong = bongItem;
                    break;
                }
                idx++;
            }
            return bong;
        }
    }
    [Serializable]
    public class TickBongInfo
    {
        int saveCount = 0; //봉당 저장갯수 예)5틱봉 => 5개 30틱봉 =>30개
        int curSaveIndex = 0; //현재 저장횟수
        public int CurSaveIndex { get { return curSaveIndex; } }
        double sumPrice = 0;
        double average = 0;

        public TickBongInfo(int _saveCount)
        {
            saveCount = _saveCount;
        }

        public void AddPrice(double _price)
        {
            if(curSaveIndex >= saveCount)
            {
                CoreEngine.GetInstance().SendLogErrorMessage("저장갯수초과");
                return;
            }
            sumPrice += _price;
            curSaveIndex++;

            average = CalAverage();
        }

        public double GetAverage()
        {
            return average;
        }

        public bool IsComplete()
        {
            if (curSaveIndex == saveCount)
                return true;

            return false;
        }

        private double CalAverage()
        {
            double returnValue = 0;
            if (curSaveIndex > 0)
            {
                returnValue = (double)sumPrice / (double)curSaveIndex;
            }

            if (returnValue == 0)
                CoreEngine.GetInstance().SendLogErrorMessage("평균값 0");
            
            if(sumPrice == 0)
                CoreEngine.GetInstance().SendLogErrorMessage( "저장가격 " + sumPrice);

            
            return returnValue;
        }
    }
}
