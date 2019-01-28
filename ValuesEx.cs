using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAPbobsCOM;

namespace SDI
{
    public static class ValuesEx
    {
        public static BoYesNoEnum YesOrNo(bool val)
        {
            return val == true ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
        }

        public static bool YesOrNo(BoYesNoEnum yon)
        {
            return yon == BoYesNoEnum.tYES;
        }

        public static TimeSpan ToTime(int time)
        {
            int hour, min, sec;
            // Verify if exists seconds
            if (time.ToString().Length > 4)
            {
                hour = time / 10000;
                min = (time - (hour * 10000)) / 100;
                sec = (time - (hour * 10000) - (min * 100));

                return new TimeSpan(hour, min, sec);
            }
            else
            {
                hour = time / 100;
                min = (time - (hour * 100));
            }

            return new TimeSpan(hour, min, 0);
        }

        public static DateTime ToDateTime(DateTime date, int time)
        {
            return date + ToTime(time);
        }

        [Obsolete("Use klib")]
        public static int ToTime(DateTime date, bool addsecs = false)
        {
            if (addsecs)
                return int.Parse(date.ToString("hhmmss"));
            else
                return int.Parse(date.ToString("hhmm"));
        }
    }
}
