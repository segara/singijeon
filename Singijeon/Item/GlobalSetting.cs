using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Singijeon
{
    class GlobalSettingValue
    {
        public long trading_profit;

    }
    class GlobalSetting
    {
        private const string LOAD_DATA_FILE_NAME = @"global_setting.dat";

        public void Save(GlobalSettingValue g_value)
        {
            try
            {
                BinaryFormatter binFmt = new BinaryFormatter();

                using (System.IO.FileStream fs = new System.IO.FileStream(DateTime.Now.ToString("MM_dd") + LOAD_DATA_FILE_NAME, FileMode.Create))
                {
                    binFmt.Serialize(fs, g_value);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void Load()
        {
            BinaryFormatter binFmt = new BinaryFormatter();
            try
            {
                using (FileStream rdr = new FileStream(DateTime.Now.ToString("MM_dd") + LOAD_DATA_FILE_NAME, FileMode.Open))
                {
                    GlobalSettingValue item = (GlobalSettingValue)binFmt.Deserialize(rdr);
                    Form2.curProfit = item.trading_profit;
                }
            }
            catch (Exception e)
            {

                Console.WriteLine(e);
            }
        }
    }
}
