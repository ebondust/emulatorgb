using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBC
{
    class CPU
    {
        public byte A; // general purporse registers
        public byte B;
        public byte C;
        public byte D;
        public byte E;
        public byte H;
        public byte L;
        public byte F; // Flags register.
        public ushort IC; // Instructions counter register.
        public byte SP; // stack pointer register.

        public ushort BC
        {
            get
            {
                ushort b = (ushort)(B << 8);
                ushort c = (ushort)(C);
                return (ushort)(b | c);
            }

            set
            {
                B = (byte)(value >> 8);
                C = (byte)(value);
            }
        }


        // z80 procesor timers.
        public int m = 0;
        public int t = 0;
    }
}
