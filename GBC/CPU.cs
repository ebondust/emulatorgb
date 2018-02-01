﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBC
{
    public class CPU
    {
        public byte A; // general purpose registers
        public byte B;
        public byte C;
        public byte D;
        public byte E;
        public byte H;
        public byte L;
        public byte F; // Flags register.
        public ushort IP; // Instructions counter register.
        public ushort SP; // stack pointer register.
        public bool IME;// interrupts swich
        public bool Halt;// halt the cpu

        private RSV rsv;//saved register set
        //saved register set struct
        private struct RSV
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
            public ushort SP; // stack pointer register.
        }


        public void Reset()
        {
            A = 0x01;
            F = 0xb0;
            B = 0x00;
            C = 0x13;
            D = 0x00;
            E = 0xd8;
            F = 0x01;
            L = 0x4d;
            SP = 0xfffe;
            IP = 0x100;

        }

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

        // helpers
        public void Rsv()
        {
            rsv.A = A;
            rsv.B = B;
            rsv.C = C;
            rsv.D = D;
            rsv.E = E;
            rsv.F = F;
            rsv.H = H;
            rsv.L = L;
        }

        public void Rrs()
        {
            A = rsv.A;
            B = rsv.B;
            C = rsv.C;
            D = rsv.D;
            E = rsv.E;
            F = rsv.F;
            H = rsv.H;
            L = rsv.L;
        }

    }
}
