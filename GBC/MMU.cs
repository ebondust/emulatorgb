using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBC
{
    class MMU
    {
        ///<summary> memory managment unit </summary>
        public Stack<byte> stack = new Stack<byte>();

        public ushort IP; // Instruction pointer.
        bool inBios = true;

        // memory regions
        byte[] bios; // bios
        byte[] rom; // rom banks
        byte[] wram; // working ram
        byte[] eram; // external ram
        byte[] zram; // zero-page ram, high speed memory
    }
}
