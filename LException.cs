using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDI
{
    public class SDIException : klib.Implement.InternalException
    {
        public override string Message => MsgDecode(R.Project.LocationResx);
        public SDIException(string message) : base(message)
        {
        }

        public SDIException(Exception ex) : base(ex)
        {
        }

        public SDIException(int code, params object[] values) : base(code, values)
        {
        }
    }
}
