using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using klib;

namespace SDI.Implement
{

    public abstract class UDT_Model : klib.Implement.BaseModel
    {
        [SAP_Nofield]
        public SAPbobsCOM.BoUTBTableType TableType { get; }
        [SAP_Nofield]
        public string TableName { get; }
        [SAP_SysField]
        public string Code { get; internal set; }
        [SAP_SysField]
        public string Name { get; internal set; }
        [SAP_SysField]
        public int DocEntry { get; internal set; }
        [SAP_SysField]
        public string Canceled { get; internal set; }
        [SAP_SysField]
        public string Object { get; internal set; }
        [SAP_SysField]
        public int LineId { get; internal set; }
        [SAP_SysField]
        public DateTime CreatedDate { get; internal set; }
        [SAP_SysField]
        public int CreateTime { get; internal set; }
        [SAP_SysField]
        public DateTime UpdateDate { get; internal set; }
        [SAP_SysField]
        public int UpdateTime{ get; internal set; }

        public UDT_Model(SAPbobsCOM.BoUTBTableType tableType, String table)
        {
            TableType = tableType;
            TableName = table;
        }


        public List<klib.model.Select> Get()
        {
            throw new NotImplementedException();
        }

        public void Load(IEnumerable<Dynamic> fields)
        {
            var p = this.GetType();

            foreach (var proper in p.GetProperties())
            {
                var attr = proper.GetCustomAttributes();
                if (attr.Where(t => t.GetType().Name == typeof(Implement.SAP_NofieldAttribute).Name).Any())
                    continue;

                var name = proper.Name.ToUpper();


                if (fields.Where(c => c.Name == name).Any())
                {
                    var value = fields.Where(c => c.Name == name).Select(c => c.Value).FirstOrDefault();
                    proper.SetValue(null, value);
                }

            }
        }
    }
}
