using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBC
{
    [AttributeUsage(AttributeTargets.Method)]
    public class OpcodeAttribute : System.Attribute
    {

        public bool Extended { get; set; } = false;

        public readonly byte opcode;

        public OpcodeAttribute(string opcode)
        {
            this.opcode = Convert.ToByte(opcode, 16);
        }
    }
}
