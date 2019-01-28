using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDI.implement
{

    public abstract class UDT_Model
    {
        public readonly SAPbobsCOM.BoUTBTableType TableType;
        public readonly string TableName;
        public int Code;
        public string Name;

        public UDT_Model(SAPbobsCOM.BoUTBTableType tableType, String table)
        {
            TableType = tableType;
            TableName = table;
        }

        public List<klib.model.Select> Get()
        {
            throw new NotImplementedException();
        }
    }
}
