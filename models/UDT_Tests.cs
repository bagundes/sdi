using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAPbobsCOM;

namespace SDI.models
{
    public class UDT_Tests : Implement.UDT_Model
    {
        public UDT_Tests() : base(BoUTBTableType.bott_Document, "AUDT0")
        {
        }
    }
}
