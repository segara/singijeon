using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Singijeon
{
    [Serializable]
    public class TrailingPercentageItemForSave
    {
        public string itemCode;

        public TradingStrategyForSave strategySave;

        public int settingTickCount = 0;
        public int curTickCount = 0;
        public bool isTrailing = true;
        public int firstPrice = 0;
        public int averagePrice = 0;
        public int sumPriceAllTick = 0; //평균가 계산을 위한 변수
        public int percentageCheckPrice = 0;
        public int gapTrailBuyCheckPrice = 0;
        public long itemInvestment = 0;
        public bool isPercentageCheckBuy = false;
        public bool isGapTrailBuy = false;   //갭상승시 매수
        public bool isVwmaCheck = false;
        public bool isCheckStockIndex = false;
        public List<double> EnvelopeValueList = new List<double>();
        public DateTime gapTrailBuyCheckDateTime = DateTime.Now;
        public long gapTrailBuyCheckTimeSecond = 0;
        public string buyOrderOption; //주문 호가 옵션
        public DateTime envelopeBuyCheckDateTime = DateTime.Now;
        public TickBongInfoMgr tickBongInfoMgr = null;

        public TrailingPercentageItemForSave()
        {

        }
        public TrailingPercentageItemForSave(TrailingItem itemTrail, TradingStrategy ts)
        {
            strategySave = new TradingStrategyForSave(ts);

            BindingFlags flags = BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
            FieldInfo[] fieldArray = itemTrail.GetType().GetFields(flags);

            BindingFlags flagsStrategySave = BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
            FieldInfo[] StrategySaveFieldArray = this.GetType().GetFields(flagsStrategySave);

            foreach (FieldInfo field in fieldArray)
            {
                foreach (FieldInfo SaveField in StrategySaveFieldArray)
                {
                    if (field.Name == SaveField.Name)
                    {
                        SaveField.SetValue(this, field.GetValue(itemTrail));
                    }
                }
            }
        }
        public TrailingItem ReloadTrailingItem()
        {
            TrailingItem returnVal = new TrailingItem();
            BindingFlags flags = BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
            FieldInfo[] fieldArray = returnVal.GetType().GetFields(flags);

            BindingFlags flagsStrategySave = BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
            FieldInfo[] ItemSaveFieldArray = this.GetType().GetFields(flagsStrategySave);
            
            foreach (FieldInfo field in fieldArray)
            {
                foreach (FieldInfo SaveField in ItemSaveFieldArray)
                {
                    if (field.Name == SaveField.Name)
                    {
                        field.SetValue(returnVal, SaveField.GetValue(this));
                    }
                }
            }
            returnVal.tickBongInfoMgr.Clear();
            returnVal.curTickCount = 0;
            return returnVal;
        }
    }
}
