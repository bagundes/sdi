using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAPbobsCOM;

namespace SDI.models
{
    public class UDT_Tables : Implement.UDT_Model
    {

        public string U_Log { get; set; }
        public string U_PK { get; set; }
        public string U_ObjType { get; set; }
        public string U_FormDef { get; set; }
        public string U_FormPKItem { get; set; }

        public UDT_Tables() : base(BoUTBTableType.bott_NoObject, "TBLS0")
        {
        }

        public void Load(List<klib.Dynamic> values)
        {
            base.Load(values, this);
        }
    }
}
