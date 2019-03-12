using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDI.documents
{
    public static class Copy
    {
        public static int CopyTo(int objType, int docEntry, int to)
        {
            var orig = Conn.DI.GetBusinessObject((SAPbobsCOM.BoObjectTypes)objType) as SAPbobsCOM.Documents;
            var dest = Conn.DI.GetBusinessObject((SAPbobsCOM.BoObjectTypes)to) as SAPbobsCOM.Documents;

            if (!orig.GetByKey(docEntry))
                throw new SDIException(1, $"Document {docEntry} (type {objType}) not exists");

            dest.CardCode = orig.CardCode;

            for (int i = 0; i < orig.Expenses.Count; i++)
            {
                if (orig.Expenses.ExpenseCode != 0)
                {
                    dest.Expenses.SetCurrentLine(dest.Expenses.Count - 1);
                    var foo = orig.Expenses.ExpenseCode;
                    dest.Expenses.BaseDocType = (int)orig.DocObjectCode;
                    dest.Expenses.BaseDocEntry = orig.DocEntry;
                    dest.Expenses.BaseDocLine = i;
                    dest.Expenses.Add();
                }
            }
            var res = dest.Add();

            if (res != 0)
                throw new SDIException(Conn.DI.GetLastErrorDescription());
            else
                return int.Parse(Conn.DI.GetNewObjectKey());
        }
    }
}
