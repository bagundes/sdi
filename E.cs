using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDI
{
    public class E
    {
        public class DataBase
        {
            /// <summary>
            /// Type of SAP data base
            /// </summary>
            public enum Types
            {
                Hana,
                MSQL,
                Others,
                None,
            }

            public enum DataTable

            {
                UDT, // User Data Table
                SDT, // System Data Table
            }
        }
    }
}
