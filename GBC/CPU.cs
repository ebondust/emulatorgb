using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBC
{
    public class CPU
    {
        public byte A; // general purporse registers
        public byte B;
        public byte C;
        public byte D;
        public byte E;
        public byte H;
        public byte L;
        public byte F; // Flags register.
        public ushort IP; // Instructions counter register.
        public byte SP; // stack pointer register.

        public ushort AF
        {
            get
            {
                ushort a = (ushort)(A << 8);
                ushort f = (ushort)(F);
                return (ushort)(a | f);
            }

            set
            {
                A = (byte)(value >> 8);
                F = (byte)(value);
            }
        }

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

        public ushort DE
        {
            get
            {
                ushort d = (ushort)(D << 8);
                ushort e = (ushort)(E);
                return (ushort)(d | e);
            }

            set
            {
                D = (byte)(value >> 8);
                E = (byte)(value);
            }
        }

        public ushort HL
        {
            get
            {
                ushort h = (ushort)(H << 8);
                ushort l = (ushort)(L);
                return (ushort)(h | l);
            }

            set
            {
                H = (byte)(value >> 8);
                L = (byte)(value);
            }
        }


        // procesor timers.
        public int m = 0;
        public int t = 0;
    }
}
