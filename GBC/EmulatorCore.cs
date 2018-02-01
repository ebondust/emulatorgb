using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GBC
{
    public class EmulatorCore
    {
        public CPU cpu = new CPU();
        public MMU mmu = new MMU();
        public GPU gpu;
        Dictionary<byte, Action> opcodeMap;
        Dictionary<byte, Action> extendedMap;
        private int m, t;
        private bool imeiFlag = false;

        public EmulatorCore()
        {
            gpu = new GPU(mmu);
            mmu.cpu = cpu;
            mmu.gpu = gpu;
        }

        public void LoadRom(byte[] rom)
        {
            FillOpcodeMap();
            //cpu.Reset();
            this.mmu.LoadRom(rom);
            //gpu.Reset();
            //cpu.IP = 0x250;
            //cpu.IP = 0x0100;
        }


        Queue<int> ProgramFlow = new Queue<int>();
        ushort prevIP = 0;
        int flag = -1;
        public void EmulationLoop()
        {

            while (true)
            {
                prevIP = cpu.IP;

                var opcode = mmu.RB(cpu.IP++); // Fetch instruction
                opcodeMap[opcode].Invoke(); // Dispatch
                cpu.IP &= 65535; // Mask PC to 16 bits
                m += cpu.m; // Add time to CPU clock
                t += cpu.t;

                gpu.gpuStep((ushort)cpu.m);

                if (cpu.IME && mmu.InterruptEnable != 0 && mmu.InterruptFlags != 0)
                {
                    cpu.Halt = false;
                    cpu.IME = false;
                    var ifired = mmu.InterruptEnable & mmu.InterruptFlags;
                    if ((ifired & 1) != 0) { mmu.InterruptFlags &= 0xFE; RST40(); }
                    else if ((ifired & 2) != 0) { mmu.InterruptFlags &= 0xFD; RST48(); }
                    else if ((ifired & 4) != 0) { mmu.InterruptFlags &= 0xFB; RST50(); }
                    else if ((ifired & 8) != 0) { mmu.InterruptFlags &= 0xF7; RST58(); }
                    else if ((ifired & 16) != 0) { mmu.InterruptFlags &= 0xEF; RST60(); }
                    else { cpu.IME = true; }
                }
            }
        }



        private void executeOpcode(MethodInfo info)
        {
            info.Invoke(this, null);
        }


        public void EmulateCycle()
        {
            var opcode = mmu.RB(cpu.IP++); // Fetch instruction
      
            opcodeMap[opcode].Invoke(); // Dispatch
            cpu.IP &= 65535; // Mask PC to 16 bits
            m += cpu.m; // Add time to CPU clock
            t += cpu.t;
            gpu.gpuStep((ushort)cpu.m);
        }



        //helper
        private void FillOpcodeMap()
        {
            opcodeMap = new Dictionary<byte, Action>();
            extendedMap = new Dictionary<byte, Action>();
            var methods = typeof(EmulatorCore).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Where(x => x.GetCustomAttributes(typeof(OpcodeAttribute), false).Length > 0).ToList();
            foreach (MethodInfo method in methods)
            {
                if (((OpcodeAttribute)(method.GetCustomAttributes(typeof(OpcodeAttribute), false).First())).Extended)
                    extendedMap.Add(method.GetCustomAttributes(false).OfType<OpcodeAttribute>().First().opcode, () => executeOpcode(method));
                else
                    opcodeMap.Add(method.GetCustomAttributes(false).OfType<OpcodeAttribute>().First().opcode, () => executeOpcode(method));
            }
        }


        //helper
        public void MAPcb()
        {
            var i = mmu.RB(cpu.IP);
            cpu.IP++;
            cpu.IP &= 65535;  
        }

        //opcode functions

        //0x00
        [Opcode("0x00")]
        private void nop()
        {
            cpu.m = 1;
            cpu.t = 4;
        }
        //0x01
        [Opcode("0x01")]
        private void ldBCnn()
        {
            cpu.C = mmu.RB(cpu.IP);
            cpu.B = mmu.RB((ushort)(cpu.IP+1));
            cpu.IP += 2;
            cpu.m = 3;
        }

        //0x02
        [Opcode("0x02")]
        private void ldBCmA()
        {
            mmu.WB(cpu.BC, cpu.A);
            cpu.m = 2;
        }
        //0x03
        [Opcode("0x03")]
        private void incBC()
        {      
            cpu.BC++;
            cpu.m = 1;
        }

        //0x04
        [Opcode("0x04")]
        private void incB()
        {
            cpu.B++;
            cpu.F = (cpu.B != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 1;
        }

        //0x05
        [Opcode("0x05")]
        private void decB()
        {
            cpu.B--;
            cpu.B &= 255;
            cpu.F = (cpu.B != 0) ? (byte)0 : (byte)0x80;
            cpu.F |= 0x40;
            if((((cpu.B & 0xf) + (1 & 0xf)) & 0x10) == 0x10)
                cpu.F |= 0x20;
            cpu.m = 1;
        }
        //0x06
        [Opcode("0x06")]
        private void ldBn()
        {
            cpu.B = mmu.RB(cpu.IP);
            cpu.IP++;
            cpu.m = 2;
        }

        //0x07
        [Opcode("0x07")]
        private void rrcA()
        {
            cpu.A = RRC(cpu.A);
            cpu.m = 1;
        }
        //0x08
        [Opcode("0x08")]
        private void ldMnnSp()
        {
            mmu.WW(mmu.RW(cpu.IP), cpu.SP);
            cpu.IP += 2;
            cpu.m = 3;
        }
        //0x09
        [Opcode("0x09")]
        private void addHlBc()
        {
            int temp = (cpu.HL + cpu.BC);
            cpu.HL = (ushort)temp;
            if (temp > 65535)
                cpu.F |= 0x10;
            else
                cpu.F &= 0xEF;
            cpu.m = 1;
        }
        //0x0A
        [Opcode("0x0A")]
        private void ldABcM()
        {
            cpu.A = mmu.RB(cpu.BC);
            cpu.m = 2;
        }
        //0x0B
        [Opcode("0x0B")]
        private void decBC()
        {
            cpu.BC = (ushort)(cpu.BC - 1);
            cpu.m = 1;
        }

        //0x0C
        [Opcode("0x0C")]
        private void incC()
        {
            cpu.C++;
            cpu.F = (cpu.C != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 1;
        }

        //0x0D
        [Opcode("0x0D")]
        private void decC()
        {
            cpu.C--;
            cpu.F = (cpu.C != 0) ? (byte)0 : (byte)0x80;
            cpu.F |= 0x40;
            if ((((cpu.C & 0xf) + (1 & 0xf)) & 0x10) == 0x10)
                cpu.F |= 0x20;
            cpu.m = 1;
        }

        //0x0E
        [Opcode("0x0E")]
        private void ldCn()
        {
            cpu.C = mmu.RB(cpu.IP);
            cpu.IP++;
            cpu.m = 2;
        }
        //0x0F
        [Opcode("0x0F")]
        private void rrcC()
        {
            cpu.C = RRC(cpu.C);
            cpu.m = 1;
        }
        //helper
        private ushort RRC(ushort value)
        {
            return (ushort)(value << 1 | value >> 15);
        }
        //helper
        private byte RRC(byte value)
        {
            return (byte)(value << 1 | value >> 7);
        }
        //0x10
        [Opcode("0x10")]
        private void stop()
        {
         
        }
        //0x11
        [Opcode("0x11")]
        private void ldDEnn()
        {
            cpu.E = mmu.RB(cpu.IP);
            cpu.D = mmu.RB((ushort)(cpu.IP+1));
            cpu.IP += 2;
            cpu.m = 3;
        }
        //0x12
        [Opcode("0x12")]
        private void ldDEmA()
        {
            mmu.WB(cpu.DE, cpu.A);
            cpu.m = 2;
        }
        //0x13
        [Opcode("0x13")]
        private void incDE()
        {     
            cpu.DE++;
            cpu.m = 1;
        }
        //0x14
        [Opcode("0x14")]
        private void incD()
        {
            cpu.D++;
            cpu.F = (cpu.D != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 1;
        }
        //0x15
        [Opcode("0x15")]
        private void decD()
        {
            cpu.D--;
            cpu.F = (cpu.D != 0) ? (byte)0 : (byte)0x80;
            cpu.F |= 0x40;
            if ((((cpu.D & 0xf) + (1 & 0xf)) & 0x10) == 0x10)
                cpu.F |= 0x20;
            cpu.m = 1;
        }
        //0x16
        [Opcode("0x16")]
        private void ldDn()
        {
            cpu.D = mmu.RB(cpu.IP);
            cpu.IP++;
            cpu.m = 2;
        }
        //0x17
        [Opcode("0x17")]
        private void rlA()
        {
            byte ci = ((cpu.F & 0x10)!=0) ? (byte)1 : (byte)0;
            byte co = ((cpu.A & 0x80) != 0) ? (byte)0x10 : (byte)0;
            cpu.A = (byte)((cpu.A << 1) + ci);
            cpu.A &= 255;
            cpu.F = (byte)((cpu.F & 0xEF) + co);
            cpu.m = 1;
        }
        //0x18
        [Opcode("0x18")]
        private void jrn()
        {
            int tmp = mmu.RB(cpu.IP);
            if (tmp > 127)
                tmp = -((~tmp + 1) & 255);
            cpu.IP++;
            cpu.m = 1;
            cpu.IP = (ushort)(cpu.IP + tmp);
            cpu.m++;
        }
        //0x19
        [Opcode("0x19")]
        private void addHLDE()
        {
            int temp = (cpu.HL + cpu.DE);
            cpu.HL = (ushort)temp;
            if (temp > 65535)
                cpu.F |= 0x10;
            else
                cpu.F &= 0xEF;
            cpu.m = 3;
        }
        //0x1A
        [Opcode("0x1A")]
        private void ldADEM()
        {
            cpu.A = mmu.RB(cpu.DE);
            cpu.m = 2;
        }
        //0x1B
        [Opcode("0x1B")]
        private void decDE()
        {
            cpu.DE = (ushort)(cpu.DE - 1);
            cpu.m = 1;
        }
        //0x1C
        [Opcode("0x1C")]
        private void incE()
        {
            cpu.E++;
            cpu.F = (cpu.E != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 1;
        }
        //0x1D
        [Opcode("0x1D")]
        private void decE()
        {
            cpu.E--;
            cpu.F = (cpu.E != 0) ? (byte)0 : (byte)0x80;
            cpu.F |= 0x40;
            if ((((cpu.E & 0xf) + (1 & 0xf)) & 0x10) == 0x10)
                cpu.F |= 0x20;
            cpu.m = 1;
        }
        //0x1E
        [Opcode("0x1E")]
        private void ldEn()
        {
            cpu.E = mmu.RB(cpu.IP);
            cpu.IP++;
            cpu.m = 2;
        }
        //0x1F
        [Opcode("0x1F")]
        private void rr()
        {
            int ci = (cpu.F & 0x10)!=0 ? 0x80 : 0;
            int co = (cpu.A & 1) != 0 ? 0x10 : 0;
            cpu.A = (byte)((cpu.A >> 1) + ci);
            cpu.A &= 255;
            cpu.F = (byte)((cpu.F & 0xEF) + co);
            cpu.m = 1;
        }
        //0x20
        [Opcode("0x20")]
        private void JRNzn()
        {
            int tmp = mmu.RB(cpu.IP);
            if (tmp > 127)
                tmp = (-((~tmp + 1) & 255));
            cpu.IP++;
            cpu.m = 2;
            if ((cpu.F & 0x80) == 0x00)
            {
                cpu.IP = (ushort)(cpu.IP+tmp);
                cpu.m++;
            }

        }
        //0x21
        [Opcode("0x21")]
        private void ldHLnn()
        {
            cpu.L = mmu.RB(cpu.IP);
            cpu.H = mmu.RB((ushort)(cpu.IP+1));
            cpu.IP += 2;
            cpu.m = 3;

        }
        //0x22
        [Opcode("0x22")]
        private void ldHLiA()
        {
            mmu.WB(cpu.HL, cpu.A);
            cpu.HL++;
            cpu.m = 2;
        }
        //0x23
        [Opcode("0x23")]
        private void incHL()
        {
            cpu.HL++;
            cpu.m = 1;
        }
        //0x24
        [Opcode("0x24")]
        private void incH()
        {
            cpu.H++;
            cpu.F = (cpu.H != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 1;
        }
        //0x25
        [Opcode("0x25")]
        private void decH()
        {
            cpu.H--;
            cpu.F = (cpu.H != 0) ? (byte)0 : (byte)0x80;
            cpu.F |= 0x40;
            if ((((cpu.H & 0xf) + (1 & 0xf)) & 0x10) == 0x10)
                cpu.F |= 0x20;
            cpu.m = 1;
        }
        //0x26
        [Opcode("0x26")]
        private void ldHn()
        {
            cpu.H = mmu.RB(cpu.IP);
            cpu.IP++;
            cpu.m = 2;
        }
        //0x27
        [Opcode("0x27")]
        private void DAA()
        {
            var a = cpu.A;
            if ((byte)(cpu.F & 0x20) > 0 || ((byte)(cpu.A & 15) > 9))
                cpu.A += 6;
            cpu.F &= 0xEF;
            if ((cpu.A & 0x20) > 1 || (a > 0x99))
            {
                cpu.A += 0x60;
                cpu.F |= 0x10;
            }
            cpu.m = 1;
        }
        //0x28
        [Opcode("0x28")]
        private void JRzn()
        {
            int tmp = mmu.RB(cpu.IP);
            cpu.IP++;
            cpu.m = 2;
            if ((cpu.F & 0x80) == 0x00)
                return;
            
            if (tmp > 127)
                tmp = -((~tmp + 1) & 255);
            cpu.IP = (ushort)(cpu.IP + tmp);
            cpu.m++;
        }

        //0x29
        [Opcode("0x29")]
        private void addHLHL()
        {
            cpu.HL = (ushort)(cpu.HL + cpu.HL);
            cpu.m = 1;
        }
        //0x2A
        [Opcode("0x2A")]
        private void ldiAHL()
        {
            cpu.A = mmu.RB(cpu.HL);
            cpu.HL++;
            cpu.m = 1;
        }
        //0x2B
        [Opcode("0x2B")]
        private void decHL()
        {
            cpu.HL--;
            cpu.m = 1;
        }
        //0x2C
        [Opcode("0x2C")]
        private void incL()
        {
            cpu.L++;
            cpu.F = (cpu.L != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 1;
        }
        //0x2D
        [Opcode("0x2D")]
        private void decL()
        {
            cpu.L--;  
            cpu.F = (cpu.L != 0) ? (byte)0 : (byte)0x80;
            cpu.F |= 0x40;
            if ((((cpu.L & 0xf) + (1 & 0xf)) & 0x10) == 0x10)
                cpu.F |= 0x20;
            cpu.m = 1;
        }
        //0x2E
        [Opcode("0x2E")]
        private void ldLn()
        {
            cpu.L = mmu.RB(cpu.IP);
            cpu.IP++;
            cpu.m = 2;
        }

        //0x2F
        [Opcode("0x2F")]
        private void cpl()
        {
            cpu.A ^= 255;
            cpu.F = (cpu.A !=0) ? (byte)0 : (byte)0x80;
            cpu.m = 1;
        }
        //0x30
        [Opcode("0x30")]
        private void JRNcn()
        {
            int tmp = mmu.RB(cpu.IP);
            if (tmp > 127)
                tmp = -((~tmp + 1) & 255);
            cpu.IP++;
            cpu.m = 2;
            if ((cpu.F & 0x10) == 0x00)
            {
                cpu.IP = (ushort)(cpu.IP + tmp);
                cpu.m++;
            }
        }
        //0x31
        [Opcode("0x31")]
        private void ldSPnn()
        {
            cpu.SP = mmu.RW(cpu.IP);
            cpu.IP += 2;
            cpu.m = 3;

        }
        //0x32
        [Opcode("0x32")]
        private void lddHL()
        {
            mmu.WB(cpu.HL, cpu.A);
            cpu.HL--;
            cpu.m = 2;
        }
        //0x33
        [Opcode("0x33")]
        private void incSP()
        {
            cpu.SP++;
            cpu.m = 1;
        }
        //0x34
        [Opcode("0x34")]
        private void incHLm()
        {
            byte temp = mmu.RB(cpu.HL);
            temp++;
            mmu.WB(cpu.HL, temp);
            cpu.F = (temp!=0) ? (byte) 0 : (byte)0x80;
            cpu.m = 3;
        }
        //0x35
        [Opcode("0x35")]
        private void decHLm()
        {
            byte temp = mmu.RB(cpu.HL);
            temp--;
            mmu.WB(cpu.HL, temp);
            cpu.F = (temp != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 3;
        }
        //0x36
        [Opcode("0x36")]
        private void ldHLmn()
        {
            mmu.WB(cpu.HL, mmu.RB(cpu.IP));
            cpu.IP++;
            cpu.m = 3;
        }
        //0x37
        [Opcode("0x37")]
        private void scf()
        {
            cpu.F |= 0x10;
            cpu.m = 1;
        }
        //0x38
        [Opcode("0x38")]
        private void JRCn()
        {
            int i = mmu.RB(cpu.IP);
            if (i > 127)
                i = -((~i + 1) & 255);
            cpu.IP++;
            cpu.m = 2;
            if ((cpu.F & 0x10) == 0x10)
            {
                cpu.IP = (ushort)(cpu.IP + i);
                cpu.m++;
            }
        }
        //0x39
        [Opcode("0x39")]
        private void addHLSP()
        {
            cpu.HL = (ushort)(cpu.HL + cpu.SP);
            cpu.m = 1;
        }
        //0x3A
        [Opcode("0x3A")]
        private void lddAHLm()
        {
            cpu.A = mmu.RB(cpu.HL);
            cpu.HL--;
            cpu.m = 2;
        }
        //0x3B
        [Opcode("0x3B")]
        private void decSP()
        {
            cpu.SP--;
            cpu.SP &= 65535;
            cpu.m = 1;
        }
        //0x3C
        [Opcode("0x3C")]
        private void IncA()
        {
            cpu.A++;
            cpu.F = (cpu.A != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 1;
        }
        //0x3D
        [Opcode("0x3D")]
        private void DecA()
        {
            cpu.A--;
            cpu.F = (cpu.A != 0) ? (byte)0 : (byte)0x80;
            cpu.F |= 0x40;
            if ((((cpu.A & 0xf) + (1 & 0xf)) & 0x10) == 0x10)
                cpu.F |= 0x20;
            cpu.m = 1;
        }
        //0x3E
        [Opcode("0x3E")]
        private void ldA()
        {
            cpu.A = mmu.RB(cpu.IP);
            cpu.IP++;
            cpu.m = 2;
        }
        //0x3F
        [Opcode("0x3F")]
        private void ccf()
        {
            var ci = (cpu.F & 0x10)!=0 ? 0 : 0x10;
            cpu.F = (byte)((cpu.F & 0xEF) + ci);
            cpu.m = 1;
        }
        //0x40
        [Opcode("0x40")]
        private void ldBB()
        {
            cpu.B = cpu.B;
            cpu.m = 1;
        }
        //0x50
        [Opcode("0x50")]
        private void ldDB()
        {
            cpu.D = cpu.B;
            cpu.m = 1;
        }
        //0x60
        [Opcode("0x60")]
        private void ldHB()
        {
            cpu.H = cpu.B;
            cpu.m = 1;
        }
        //0x70
        [Opcode("0x70")]
        private void ldHLmB()
        {
            mmu.WB(cpu.HL, cpu.B);
            cpu.m = 2;
        }
        //0x80
        [Opcode("0x80")]
        private void addAB()
        {
            int temp = cpu.A + cpu.B;
            if (temp > 255)
                cpu.F = 0x10;
            else
                cpu.F = 0;
            if (temp == 0)
                cpu.F |= 0x80;
            if ((((cpu.A & 0xf) + (cpu.B & 0xf)) & 0x10) == 0x10)
                cpu.F |= 0x20;

            cpu.A = (byte)temp;
            cpu.m = 1;
        }
        //0x90
        [Opcode("0x90")]
        private void subAB()
        {
            int temp = cpu.A - cpu.B;
            if (temp < 0)
                cpu.F = 0x10;
            else
                cpu.F = 0;
            cpu.A = (byte)(temp&255);
            if (temp == 0)
                cpu.F |= 0x80;
            if ((((cpu.A & 0xf) + (cpu.B & 0xf)) & 0x10) == 0x10)
                cpu.F |= 0x20;

            cpu.F |= 0x40;

           
            cpu.m = 1;
        }
        //0xA0
        [Opcode("0xA0")]
        private void andB()
        {
            cpu.A &= cpu.B;
            cpu.F = (cpu.A != 0) ?  (byte)0 : (byte)0x80;
            cpu.m = 1;
        }

        //cp

        //0xB0
        [Opcode("0xB0")]
        private void orB()
        {
            cpu.A |= cpu.B;
            cpu.A &= 255;
            cpu.F = (cpu.A != 0) ?  (byte)0 : (byte)0x80;
            cpu.m = 1;
        }

        //0xC0
        [Opcode("0xC0")]
        private void retNZ()
        {
            cpu.m = 1;
            if ((cpu.F & 0x80) == 0x00)
            {
                cpu.IP = mmu.RW(cpu.SP);
                cpu.SP += 2;
                cpu.m += 2;
            }
        }
        //0xD0
        [Opcode("0xD0")]
        private void retNC()
        {
            cpu.m = 1;
            if ((cpu.F & 0x10) == 0x00)
            {
                cpu.IP = mmu.RW(cpu.SP);
                cpu.SP += 2;
                cpu.m += 2;
            }
        }

        //0xE0
        [Opcode("0xE0")]
        private void ldHnmA()
        {
            mmu.WB((ushort)(0xFF00 + mmu.RB(cpu.IP)), cpu.A);
            cpu.IP++;
            cpu.m = 3;
        }
        //0xF0
        [Opcode("0xF0")]
        private void ldHAnm()
        {
            cpu.A = mmu.RB((ushort)(0xFF00 + mmu.RB(cpu.IP)));
            cpu.IP++;
            cpu.m = 3;
        }
        //0x41
        [Opcode("0x41")]
        private void ldBC()
        {
            cpu.B = cpu.C;
            cpu.m = 1;
        }
        //0x51
        [Opcode("0x51")]
        private void ldDC()
        {
            cpu.D = cpu.C;
            cpu.m = 1;
        }
        //0x61
        [Opcode("0x61")]
        private void ldHC()
        {
            cpu.H = cpu.C;
            cpu.m = 1;
        }
        //0x71
        [Opcode("0x71")]
        private void ldHLmC()
        {
            mmu.WB(cpu.HL, cpu.C);
            cpu.m = 2;
        }
        //0x81
        [Opcode("0x81")]
        private void addAC()
        {
            byte tempA = cpu.A;
            int result = cpu.A + cpu.C;

            cpu.F = (result > 255) ? (byte)0x10 : (byte)0;
            cpu.A = (byte)(result &= 255);
            if (cpu.A == 0)
                cpu.F |= 0x80;
            if (((cpu.A ^ cpu.C ^ tempA) & (byte)0x10) != 0)
                cpu.F |= 0x20;
            cpu.m = 1;
        }
        //0x91
        [Opcode("0x91")]
        private void subAC()
        {
            byte tempA = cpu.A;
            int result = cpu.A - cpu.C;
            cpu.F = (result < 0) ? (byte)0x50 : (byte)0x40;
            cpu.A &= 255;
            if (cpu.A == 0)
                cpu.F |= 0x80;
            if (((cpu.A ^ cpu.C ^ tempA) & (byte)0x10) != 0)
                cpu.F |= 0x20;
            cpu.m = 1;
        }
        //0xA1
        [Opcode("0xA1")]
        private void andC()
        {
            cpu.A &= cpu.C;
            cpu.A &= 255;
            cpu.F = (cpu.A != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 1;
        }
        //0xB1
        [Opcode("0xB1")]
        private void orC()
        {
            cpu.A |= cpu.C;
            cpu.A &= 255;
            cpu.F = (cpu.A != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 1;
        }
        //0xC1
        [Opcode("0xC1")]
        private void popBC()
        {
            cpu.BC = mmu.RW(cpu.SP);
            cpu.SP+=2;
            cpu.m = 3;
        }
        //0xD1
        [Opcode("0xD1")]
        private void popDE()
        {
            cpu.DE = mmu.RW(cpu.SP);
            cpu.SP += 2;
            cpu.m = 3;
        }
        //0xE1
        [Opcode("0xE1")]
        private void popHL()
        {
            cpu.HL = mmu.RW(cpu.SP);
            cpu.SP += 2;
            cpu.m = 3;
        }
        //0xF1
        [Opcode("0xF1")]
        private void popAF()
        {
            cpu.AF = mmu.RW(cpu.SP);
            cpu.SP += 2;
            cpu.m = 3;
        }
        //0x42
        [Opcode("0x42")]
        private void ldBD()
        {
            cpu.B = cpu.D;
            cpu.m = 1;
        }
        //0x52
        [Opcode("0x52")]
        private void ldDD()
        {
            cpu.D = cpu.D;
            cpu.m = 1;
        }
        //0x62
        [Opcode("0x62")]
        private void ldHD()
        {
            cpu.H = cpu.D;
            cpu.m = 1;
        }
        //0x72
        [Opcode("0x72")]
        private void ldHLmD()
        {
            mmu.WB(cpu.HL,cpu.D);
            cpu.m = 2;
        }
        //0x82
        [Opcode("0x82")]
        private void addAD()
        {
            byte tempA = cpu.A;
            int result = cpu.A + cpu.D;

            cpu.F = (result > 255) ? (byte)0x10 : (byte)0;
            cpu.A = (byte)(result &= 255);
            if (cpu.A == 0)
                cpu.F |= 0x80;
            if (((cpu.A ^ cpu.D ^ tempA) & (byte)0x10) != 0)
                cpu.F |= 0x20;
            cpu.m = 1;
        }
        //0x92
        [Opcode("0x92")]
        private void subAD()
        {
            byte tempA = cpu.A;
            int result = cpu.A - cpu.D;
            cpu.F = (result < 0) ? (byte)0x50 : (byte)0x40;
            cpu.A &= 255;
            if (cpu.A == 0)
                cpu.F |= 0x80;
            if (((cpu.A ^ cpu.D ^ tempA) & (byte)0x10) != 0)
                cpu.F |= 0x20;
            cpu.m = 1;
        }
        //0xA2
        [Opcode("0xA2")]
        private void andD()
        {
            cpu.A &= cpu.D;
            cpu.A &= 255;
            cpu.F = (cpu.A != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 1;
        }
        //0xB2
        [Opcode("0xB2")]
        private void orD()
        {
            cpu.A |= cpu.D;
            cpu.A &= 255;
            cpu.F = (cpu.A != 0) ?  (byte)0 : (byte)0x80;
            cpu.m = 1;
        }
        //0xC2
        [Opcode("0xC2")]
        private void jpNznn()
        {
            cpu.m = 3;
            if ((cpu.F & 0x80) == 0x00)
            {
                cpu.IP = mmu.RW(cpu.IP); cpu.m++;
            }
            else
                cpu.IP += 2;
        }
        //0xD2
        [Opcode("0xD2")]
        private void jpNcnn()
        {
            cpu.m = 3;
            if ((cpu.F & 0x10) == 0x00)
            {
                cpu.IP = mmu.RW(cpu.IP);
                cpu.m++;
            }
            else cpu.IP += 2;
        }
        //0xE2
        [Opcode("0xE2")]
        private void ldhCmA()
        {
            mmu.WB((ushort)(0xFF00 + cpu.C), cpu.A);
            cpu.m = 2;
        }
        //0xF2
        [Opcode("0xF2")]
        private void xx()
        {
            var opc = cpu.IP - 1;
            Debug.WriteLine("Unimplemented instruction at" + opc.ToString() + ", stopping.");
        }
        //0x43
        [Opcode("0x43")]
        private void ldBE()
        {
            cpu.B = cpu.E;
            cpu.m = 1;
        }
        //0x53
        [Opcode("0x53")]
        private void lDE()
        {
            cpu.D = cpu.E;
            cpu.m = 1;
        }
        //0x63
        [Opcode("0x63")]
        private void ldHE()
        {
            cpu.H = cpu.E;
            cpu.m = 1;
        }
        //0x73
        [Opcode("0x73")]
        private void ldHLmE()
        {
            mmu.WB(cpu.HL, cpu.E);
            cpu.m = 2;
        }
        //0x83
        [Opcode("0x83")]
        private void addAE()
        {
            byte tempA = cpu.A;
            int result = cpu.A + cpu.E;

            cpu.F = (result > 255) ? (byte)0x10 : (byte)0;
            cpu.A = (byte)(result &= 255);
            if (cpu.A == 0)
                cpu.F |= 0x80;
            if (((cpu.A ^ cpu.E ^ tempA) & (byte)0x10) != 0)
                cpu.F |= 0x20;
            cpu.m = 1;
        }
        //0x93
        [Opcode("0x93")]
        private void subAE()
        {
            byte tempA = cpu.A;
            int result = cpu.A - cpu.E;
            cpu.F = (result < 0) ? (byte)0x50 : (byte)0x40;
            cpu.A = (byte)(result&255);
            if (cpu.A == 0)
                cpu.F |= 0x80;
            if (((cpu.A ^ cpu.E ^ tempA) & (byte)0x10) != 0)
                cpu.F |= 0x20;
            cpu.m = 1;
        }
        //0xA3
        [Opcode("0xA3")]
        private void andE()
        {
            cpu.A &= cpu.E;
            cpu.A &= 255;
            cpu.F = (cpu.A != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 1;
        }
        //0xB3
        [Opcode("0xB3")]
        private void orE()
        {
            cpu.A |= cpu.E;
            cpu.A &= 255;
            cpu.F = (cpu.A != 0) ?  (byte)0 : (byte)0x80;
            cpu.m = 1;
        }
        //0xC3
        [Opcode("0xC3")]
        private void jpnn()
        {
            cpu.IP = mmu.RW(cpu.IP);
            cpu.m = 3;
        }
        //0xD3
        //0xE3
        //xx

        //0xF3
        [Opcode("0xF3")]
        private void di()
        {
            cpu.IME = false;
            cpu.m = 1;
        }
        //0x44
        [Opcode("0x44")]
        private void ldBrH()
        {
            cpu.B = cpu.H;
            cpu.m = 1;
        }
        //0x54
        [Opcode("0x54")]
        private void ldDrH()
        {
            cpu.D = cpu.H;
            cpu.m = 1;
        }
        //0x64
        [Opcode("0x64")]
        private void ldHrH()
        {
            cpu.H = cpu.H;
            cpu.m = 1;
        }
        //0x74
        [Opcode("0x74")]
        private void ldHLmH()
        {
            mmu.WB(cpu.HL, cpu.H);
            cpu.m = 2;
        }
        //0x84
        [Opcode("0x84")]
        private void addAH()
        {
            int temp = cpu.A + cpu.H;
            if (temp > 255)
                cpu.F = 0x10;
            else
                cpu.F = 0;
            cpu.A = (byte)temp;
            if (cpu.A == 0)
                cpu.F |= 0x80;
            if ((((cpu.A & 0xf) + (cpu.H & 0xf)) & 0x10) == 0x10)
                cpu.F |= 0x20;
            cpu.m = 1;
        }
        //0x94
        [Opcode("0x94")]
        private void subAH()
        {
            int temp = cpu.A - cpu.H;
            if (temp < 0)
                cpu.F = 0x10;
            else
                cpu.F = 0;
            cpu.A = (byte)temp;
            if (cpu.A == 0)
                cpu.F |= 0x80;
            if ((((cpu.A & 0xf) + (cpu.H & 0xf)) & 0x10) == 0x10)
                cpu.F |= 0x20;

            cpu.F |= 0x40;

            cpu.m = 1;
        }
        //0xA4
        [Opcode("0xA4")]
        private void andH()
        {
            cpu.A &= cpu.H;
            cpu.F = (cpu.A != 0) ?  (byte)0 : (byte)0x80;
        }
        //0xB4
        [Opcode("0xB4")]
        private void orH()
        {
            cpu.A |= cpu.H;
            cpu.A &= 255;
            cpu.F = (cpu.A != 0) ?  (byte)0 : (byte)0x80;
            cpu.m = 1;
        }
        //0xC4
        [Opcode("0xC4")]
        private void callNznn()
        {
            cpu.m = 3;
            if ((cpu.F & 0x80) == 0x00)
            {
                cpu.SP -= 2;
                mmu.WW(cpu.SP, (ushort)(cpu.IP + 2));
                cpu.IP = mmu.RW(cpu.IP);
                cpu.m += 2;
            }
            else
                cpu.IP += 2;
        }
        //0xD4
        [Opcode("0xD4")]
        private void callNcnn()
        {
            cpu.m = 3;
            if ((cpu.F & 0x10) == 0x00)
            {
                cpu.SP -= 2;
                mmu.WW(cpu.SP, (ushort)(cpu.IP + 2));
                cpu.IP = mmu.RW(cpu.IP);
                cpu.m += 2;
            }
            else
                cpu.IP += 2;
        }
        //0xE4
        //0xF4
        //xx

        //0x45
        [Opcode("0x45")]
        private void ldBrl()
        {
            cpu.B = cpu.L;
            cpu.m = 1;
        }
        //0x55
        [Opcode("0x55")]
        private void ldDrl()
        {
            cpu.D = cpu.L;
            cpu.m = 1;
        }
        //0x65
        [Opcode("0x65")]
        private void ldHrl()
        {
            cpu.H = cpu.L;
            cpu.m = 1;
        }
        //0x75
        [Opcode("0x75")]
        private void ldHLmL()
        {
            mmu.WB(cpu.HL, cpu.L);
            cpu.m = 2;
        }
        //0x85
        [Opcode("0x85")]
        private void addAL()
        {
            int temp = cpu.A + cpu.L;
            if (temp > 255)
                cpu.F = 0x10;
            else
                cpu.F = 0;
            cpu.A = (byte)temp;
            if (cpu.A == 0)
                cpu.F |= 0x80;
            if ((((cpu.A & 0xf) + (cpu.L & 0xf)) & 0x10) == 0x10)
                cpu.F |= 0x20;

            cpu.A = (byte)temp;
            cpu.m = 1;
        }
        //0x95
        [Opcode("0x95")]
        private void subAL()
        {
            int temp = cpu.A - cpu.L;
            if (temp < 0)
                cpu.F = 0x10;
            else
                cpu.F = 0;
            cpu.A = (byte)temp;
            if (cpu.A == 0)
                cpu.F |= 0x80;
            if ((((cpu.A & 0xf) + (cpu.L & 0xf)) & 0x10) == 0x10)
                cpu.F |= 0x20;

            cpu.F |= 0x40;
            cpu.m = 1;
        }
        //0xA5
        [Opcode("0xA5")]
        private void andL()
        {
            cpu.A &= cpu.L;
            cpu.F = (cpu.A != 0) ?  (byte)0 : (byte)0x80;
        }
        //0xB5
        [Opcode("0xB5")]
        private void orL()
        {
            cpu.A |= cpu.L;
            cpu.A &= 255;
            cpu.F = (cpu.A != 0) ?  (byte)0 : (byte)0x80;
            cpu.m = 1;
        }
        //0xC5
        [Opcode("0xC5")]
        private void pushBC()
        {
            cpu.SP--;
            mmu.WB(cpu.SP, cpu.B);
            cpu.SP--;
            mmu.WB(cpu.SP, cpu.C);
            cpu.m = 3;
        }
        //0xD5
        [Opcode("0xD5")]
        private void pushDE()
        {
            cpu.SP--;
            mmu.WB(cpu.SP, cpu.D);
            cpu.SP--;
            mmu.WB(cpu.SP, cpu.E);
            cpu.m = 3;
        }
        //0xE5
        [Opcode("0xE5")]
        private void pushHL()
        {
            cpu.SP--;
            mmu.WB(cpu.SP, cpu.H);
            cpu.SP--;
            mmu.WB(cpu.SP, cpu.L);
            cpu.m = 3;
        }
        //0xF5
        [Opcode("0xF5")]
        private void pushAF()
        {
            cpu.SP--;
            mmu.WB(cpu.SP, cpu.A);
            cpu.SP--;
            mmu.WB(cpu.SP, cpu.F);
            cpu.m = 3;
        }
        //0x46
        [Opcode("0x46")]
        private void ldBHLm()
        {
            cpu.B = mmu.RB(cpu.HL);
            cpu.m = 2;
        }
        //0x56
        [Opcode("0x56")]
        private void ldDHLm()
        {
            cpu.D = mmu.RB(cpu.HL);
            cpu.m = 2;
        }
        //0x66
        [Opcode("0x66")]
        private void ldHHLm()
        {
            cpu.H = mmu.RB(cpu.HL);
            cpu.m = 2;
        }
        //0x76
        [Opcode("0x76")]
        private void halt()
        {
            cpu.Halt = true;
            cpu.m = 1;
        }
        //0x86
        [Opcode("0x86")]
        private void addAHLm()
        {
            byte tm = mmu.RB(cpu.HL);
            int temp = cpu.A + tm;
            if (temp > 255)
                cpu.F = 0x10;
            else
                cpu.F = 0;
            cpu.A = (byte)temp;
            if (cpu.A == 0)
                cpu.F |= 0x80;
            if ((((cpu.A & 0xf) + (tm & 0xf)) & 0x10) == 0x10)
                cpu.F |= 0x20;
            cpu.m = 1;
        }
        //0x96
        [Opcode("0x96")]
        private void subAHLm()
        {
            byte tm = mmu.RB(cpu.HL);
            int temp = cpu.A - tm;
            if (temp < 0)
                cpu.F = 0x10;
            else
                cpu.F = 0;
            cpu.A = (byte)temp;
            if (cpu.A == 0)
                cpu.F |= 0x80;
            if ((((cpu.A & 0xf) + (tm & 0xf)) & 0x10) == 0x10)
                cpu.F |= 0x20;

            cpu.F |= 0x40;

            cpu.m = 1;
        }
        //0xA6
        [Opcode("0xA6")]
        private void andHLm()
        {
            cpu.A &= mmu.RB(cpu.HL);
            cpu.F = (cpu.A != 0) ?  (byte)0 : (byte)0x80;
            cpu.m = 2;
        }
        //0xB6
        [Opcode("0xB6")]
        private void orHLm()
        {
            cpu.A |= mmu.RB(cpu.HL);
            cpu.A &= 255;
            cpu.F = (cpu.A != 0) ?  (byte)0 : (byte)0x80;
            cpu.m = 2;
        }
        //0xC6
        [Opcode("0xC6")]
        private void addAn()
        {
            byte tm = mmu.RB(cpu.IP);
            cpu.IP++;
            int temp = cpu.A + tm;
            if (temp > 255)
                cpu.F = 0x10;
            else
                cpu.F = 0;
            cpu.A = (byte)temp;
            if (cpu.A == 0)
                cpu.F |= 0x80;
            if ((((cpu.A & 0xf) + (tm & 0xf)) & 0x10) == 0x10)
                cpu.F |= 0x20;
       
            cpu.m = 2;
        }
        //0xD6
        [Opcode("0xD6")]
        private void subAn()
        {
            byte tm = mmu.RB(cpu.IP);
            cpu.IP++;
            int temp = cpu.A - tm;
            if (temp < 0)
                cpu.F = 0x10;
            else
                cpu.F = 0;
            cpu.A = (byte)temp;
            if (cpu.A == 0)
                cpu.F |= 0x80;
            if ((((cpu.A & 0xf) + (tm & 0xf)) & 0x10) == 0x10)
                cpu.F |= 0x20;

            cpu.F |= 0x40;

            cpu.m = 2;
        }
        //0xE6
        [Opcode("0xE6")]
        private void andn()
        {
            cpu.A &= mmu.RB(cpu.IP);
            cpu.IP++;
            cpu.F = (cpu.A != 0) ?  (byte)0 : (byte)0x80;
            cpu.m = 2;
        }
        //0xF6
        [Opcode("0xF6")]
        private void orn()
        {
            cpu.A |= mmu.RB(cpu.IP);
            cpu.IP++;
            cpu.A &= 255;
            cpu.F = (cpu.A != 0) ?  (byte)0 : (byte)0x80;
            cpu.m = 2;
        }
        //0x47
        [Opcode("0x47")]
        private void ldBrA()
        {
            cpu.B = cpu.A;
            cpu.m = 1;
        }
        //0x57
        [Opcode("0x57")]
        private void ldDrA()
        {
            cpu.D = cpu.A;
            cpu.m = 1;
        }
        //0x67
        [Opcode("0x67")]
        private void ldHrA()
        {
            cpu.H = cpu.A;
            cpu.m = 1;
        }
        //0x77
        [Opcode("0x77")]
        private void ldHLmA()
        {
            mmu.WB(cpu.HL, cpu.A);
            cpu.m = 1;
        }
        //0x87
        [Opcode("0x87")]
        private void addAA()
        {
            int temp = cpu.A + cpu.A;
            if (temp > 255)
                cpu.F = 0x10;
            else
                cpu.F = 0;
            cpu.A = (byte)temp;
            if (cpu.A == 0)
                cpu.F |= 0x80;
            if ((((cpu.A & 0xf) + (cpu.A & 0xf)) & 0x10) == 0x10)
                cpu.F |= 0x20;

           
            cpu.m = 1;
        }
        //0x97
        [Opcode("0x97")]
        private void subAA()
        {
            int temp = cpu.A - cpu.A;
            if (temp < 0)
                cpu.F = 0x10;
            else
                cpu.F = 0;
            cpu.A = (byte)temp;
            if (cpu.A == 0)
                cpu.F |= 0x80;
            if ((((cpu.A & 0xf) + (cpu.A & 0xf)) & 0x10) == 0x10)
                cpu.F |= 0x20;

            cpu.F |= 0x40;
            cpu.m = 1;
        }
        //0xA7
        [Opcode("0xA7")]
        private void andA()
        {
            cpu.A &= cpu.A;
            cpu.F = (cpu.A != 0) ?  (byte)0 : (byte)0x80;
            cpu.F |= 0x20;
        }

        //0xB7
        [Opcode("0xB7")]
        private void orA()
        {
            cpu.A |= cpu.A;
            cpu.A &= 255;
            cpu.F = (cpu.A != 0) ?  (byte)0 : (byte)0x80;
            cpu.m = 1;
        }
        //0xC7
        [Opcode("0xC7")]
        private void rst0()
        {
            cpu.Rsv();
            cpu.SP -= 2;
            mmu.WW(cpu.SP, cpu.IP);
            cpu.IP = 0x00;
            cpu.m = 3;
        }
        //0xD7
        [Opcode("0xD7")]
        private void rst10()
        {
            cpu.Rsv();
            cpu.SP -= 2;
            mmu.WW(cpu.SP, cpu.IP);
            cpu.IP = 0x10;
            cpu.m = 3;
        }
        //0xE7
        [Opcode("0xE7")]
        private void rst20()
        {
            cpu.Rsv();
            cpu.SP -= 2;
            mmu.WW(cpu.SP, cpu.IP);
            cpu.IP = 0x20;
            cpu.m = 3;
        }
        //0xF7
        [Opcode("0xF7")]
        private void rst30()
        {
            cpu.Rsv();
            cpu.SP -= 2;
            mmu.WW(cpu.SP, cpu.IP);
            cpu.IP = 0x30;
            cpu.m = 3;
        }

        //0x48
        [Opcode("0x48")]
        private void ldCrB()
        {
            cpu.C = cpu.B;
            cpu.m = 1;
        }
        //0x58
        [Opcode("0x58")]
        private void ldErB()
        {
            cpu.E = cpu.B;
            cpu.m = 1;
        }
        //0x68
        [Opcode("0x68")]
        private void ldLrB()
        {
            cpu.L = cpu.B;
            cpu.m = 1;
        }
        //0x78
        [Opcode("0x78")]
        private void ldArB()
        {
            cpu.A = cpu.B;
            cpu.m = 1;
        }

        //0x88
        [Opcode("0x88")]
        private void adcAB()
        {
            byte tempA = cpu.A;
            int result = cpu.A + cpu.B;
            result += ((cpu.F & 0x10) !=0) ? (byte)1 : (byte)0;
            cpu.F = (result > 255) ? (byte)0x10 : (byte)0;
            cpu.A = (byte)(result &= 255);
            if (cpu.A == 0)
                cpu.F |= 0x80;
            if (((cpu.A ^ cpu.B ^ tempA) & (byte)0x10) != 0)
                cpu.F |= 0x20;
            cpu.m = 1;
        }
        //0x98
        [Opcode("0x98")]
        private void sbcAB()
        {
            byte tempA = cpu.A;
            int result = cpu.A - cpu.B;
            result -= ((cpu.F & 0x10) != 0) ? (byte)1 : (byte)0;
            cpu.F = (result < 0) ? (byte)0x50 : (byte)0x40;
            cpu.A = (byte)(result &= 255);
            if (cpu.A == 0)
                cpu.F |= 0x80;
            if (((cpu.A ^ cpu.B ^ tempA) & (byte)0x10) != 0)
                cpu.F |= 0x20;
            cpu.m = 1;
        }
        //0xA8
        [Opcode("0xA8")]
        private void xorB()
        {
            cpu.A ^= cpu.B;
            cpu.A &= 255;
            cpu.F = (cpu.A != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 1;
        }
        //0xB8
        [Opcode("0xB8")]
        private void CpBA()
        {
            byte tmp = cpu.A;
            tmp -= cpu.B;
            cpu.F = (tmp < 0) ? (byte)0x50 : (byte)0x40;
            tmp &= 255;
            if (tmp == 0)
                cpu.F |= 0x80;
            if (((cpu.A ^ cpu.B ^ tmp) & (byte)0x10) != 0)
                cpu.F |= 0x20;
            cpu.m = 1;
        }
        //0xC8
        [Opcode("0xC8")]
        private void retz()
        {
            cpu.m = 1;
            if ((cpu.F & 0x80) == 0x80)
            {
                cpu.IP = mmu.RW(cpu.SP); cpu.SP += 2; cpu.m += 2;
            }
        }
        //0xD8
        [Opcode("0xD8")]
        private void retc()
        {
            cpu.m = 1;
            if ((cpu.F & 0x10) == 0x10)
            {
                cpu.IP = mmu.RW(cpu.SP);
                cpu.SP += 2;
                cpu.m += 2;
            }
        }
        //0xE8
        [Opcode("0xE8")]
        private void addSPd()
        {
            var tmp = mmu.RB(cpu.IP);
            if (tmp > 127)
                tmp = (byte)-((~tmp + 1) & 255);
            cpu.IP++;
            cpu.SP += tmp;
            cpu.m = 4;

        }
        //0xF8
        [Opcode("0xF8")]
        private void ldhlSPd()
        {
            ushort tmp = mmu.RB(cpu.IP);
            if (tmp > 127)
                tmp = (byte)-((~tmp + 1) & 255);
            cpu.IP++;
            tmp += cpu.SP;
            cpu.H = (byte)((tmp >> 8) & 255);
            cpu.L = (byte) (tmp & 255);
            cpu.m = 3;
        }

        //0x49
        [Opcode("0x49")]
        private void ldCC()
        {
            cpu.C = cpu.C;
            cpu.m = 1;
        }
        //0x59
        [Opcode("0x59")]
        private void ldEC()
        {
            cpu.E = cpu.C;
            cpu.m = 1;
        }
        //0x69
        [Opcode("0x69")]
        private void ldLC()
        {
            cpu.L = cpu.C;
            cpu.m = 1;
        }
        //0x79
        [Opcode("0x79")]
        private void ldAC()
        {
            cpu.A = cpu.C;
            cpu.m = 1;
        }
        //0x89
        [Opcode("0x89")]
        private void adcAC()
        {
            byte tempA = cpu.A;
            int result = cpu.A + cpu.C;
            result += ((cpu.F & 0x10) != 0) ? (byte)1 : (byte)0;
            cpu.F = (result > 255) ? (byte)0x10 : (byte)0;
            cpu.A = (byte)(result &= 255);
            if (cpu.A == 0)
                cpu.F |= 0x80;
            if (((cpu.A ^ cpu.C ^ tempA) & (byte)0x10) != 0)
                cpu.F |= 0x20;
            cpu.m = 1;
        }
        //0x99
        [Opcode("0x99")]
        private void sbcAC()
        {
            byte tempA = cpu.A;
            int result = cpu.A - cpu.C;
            result -= ((cpu.F & 0x10) != 0) ? (byte)1 : (byte)0;
            cpu.F = (result < 0) ? (byte)0x50 : (byte)0x40;
            cpu.A = (byte)(result &= 255);
            if (cpu.A == 0)
                cpu.F |= 0x80;
            if (((cpu.A ^ cpu.C ^ tempA) & (byte)0x10) != 0)
                cpu.F |= 0x20;
            cpu.m = 1;
        }
        //0xA9
        [Opcode("0xA9")]
        private void xorC()
        {
            cpu.A ^= cpu.C;
            cpu.A &= 255;
            cpu.F = (cpu.A != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 1;
        }
        //0xB9
        [Opcode("0xB9")]
        private void CpCA()
        {
            byte tmp = cpu.A;
            tmp -= cpu.C;
            cpu.F = (tmp < 0) ? (byte)0x50 : (byte)0x40;
            tmp &= 255;
            if (tmp == 0)
                cpu.F |= 0x80;
            if (((cpu.A ^ cpu.C ^ tmp) & (byte)0x10) != 0)
                cpu.F |= 0x20;
            cpu.m = 1;
        }
        //0xC9
        [Opcode("0xC9")]
        private void ret()
        {
            cpu.IP = mmu.RW(cpu.SP);
            cpu.SP += 2;
            cpu.m = 3;
        }
        //0xD9
        [Opcode("0xD9")]
        private void retI()
        {
            cpu.IME = true;
            cpu.Rrs();
            cpu.IP = mmu.RW(cpu.SP);
            cpu.SP += 2; cpu.m = 3;
        }
        //0xE9
        [Opcode("0xE9")]
        private void jpHL()
        {
            cpu.IP = cpu.HL;
            cpu.m = 1;
        }
        //0xF9
        [Opcode("0xF9")]
        private void ldSPrHL()
        {
            ushort tmp = mmu.RB(cpu.IP);
            if (tmp > 127)
                tmp = (byte)-((~tmp + 1) & 255);
            cpu.IP++;
            tmp += cpu.SP;
            cpu.H = (byte)((tmp >> 8) & 255);
            cpu.L = (byte)(tmp & 255);
            cpu.m = 3;
        }

        //0x4A
        [Opcode("0x4A")]
        private void ldCD()
        {
            cpu.C = cpu.D;
            cpu.m = 1;
        }
        //0x5A
        [Opcode("0x5A")]
        private void ldED()
        {
            cpu.E = cpu.D;
            cpu.m = 1;
        }
        //0x6A
        [Opcode("0x6A")]
        private void ldLD()
        {
            cpu.L = cpu.D;
            cpu.m = 1;
        }
        //0x7A
        [Opcode("0x7A")]
        private void ldAD()
        {
            cpu.A = cpu.D;
            cpu.m = 1;
        }
        //0x8A
        [Opcode("0x8A")]
        private void adcAD()
        {
            byte tempA = cpu.A;
            int result = cpu.A + cpu.D;
            result += ((cpu.F & 0x10) != 0) ? (byte)1 : (byte)0;
            cpu.F = (result > 255) ? (byte)0x10 : (byte)0;
            cpu.A = (byte)(result &= 255);
            if (cpu.A == 0)
                cpu.F |= 0x80;
            if (((cpu.A ^ cpu.D ^ tempA) & (byte)0x10) != 0)
                cpu.F |= 0x20;
            cpu.m = 1;
        }
        //0x9A
        [Opcode("0x9A")]
        private void sbcAD()
        {
            byte tempA = cpu.A;
            int result = cpu.A - cpu.D;
            result -= ((cpu.F & 0x10) != 0) ? (byte)1 : (byte)0;
            cpu.F = (result < 0) ? (byte)0x50 : (byte)0x40;
            cpu.A = (byte)(result &= 255);
            if (cpu.A == 0)
                cpu.F |= 0x80;
            if (((cpu.A ^ cpu.D ^ tempA) & (byte)0x10) != 0)
                cpu.F |= 0x20;
            cpu.m = 1;
        }
        //0xAA
        [Opcode("0xAA")]
        private void xorD()
        {
            cpu.A ^= cpu.D;
            cpu.A &= 255;
            cpu.F = (cpu.A != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 1;
        }
        //0xBA
        [Opcode("0xBA")]
        private void CpAD()
        {
            byte tmp = cpu.A;
            tmp -= cpu.D;
            cpu.F = (tmp < 0) ? (byte)0x50 : (byte)0x40;
            tmp &= 255;
            if (tmp == 0)
                cpu.F |= 0x80;
            if (((cpu.A ^ cpu.D ^ tmp) & (byte)0x10) != 0)
                cpu.F |= 0x20;
            cpu.m = 1;
        }
        //0xCA
        [Opcode("0xCA")]
        private void jpznn()
        {
            cpu.m = 3;
            if ((cpu.F & 0x80) == 0x80)
            {
                cpu.IP = mmu.RW(cpu.IP);
                cpu.m++;
            }
            else
                cpu.IP += 2;
        }
        //0xDA
        [Opcode("0xDA")]
        private void jpcnn()
        {
            cpu.m = 3;
            if ((cpu.F & 0x10) == 0x00)
            {
                cpu.IP = mmu.RW(cpu.IP);
                cpu.m++;
            }
            else
                cpu.IP += 2;
        }
        //0xEA
        [Opcode("0xEA")]
        private void ldnnA()
        {
            mmu.WB(mmu.RW(cpu.IP), cpu.A);
            cpu.IP += 2;
            cpu.m = 4;
        }
        //0xFA
        [Opcode("0xFA")]
        private void ldAnn()
        {
            cpu.A = mmu.RB(mmu.RW(cpu.IP));
            cpu.IP += 2;
            cpu.m = 4;
        }

        //0x4B
        [Opcode("0x4B")]
        private void ldCE()
        {
            cpu.C = cpu.E;
            cpu.m = 1;
        }

        //0x5B
        [Opcode("0x5B")]
        private void ldEE()
        {
            cpu.E = cpu.E;
            cpu.m = 1;
        }
        //0x6B
        [Opcode("0x6B")]
        private void ldLE()
        {
            cpu.L = cpu.E;
            cpu.m = 1;
        }
        //0x7B
        [Opcode("0x7B")]
        private void ldAE()
        {
            cpu.A = cpu.E;
            cpu.m = 1;
        }
        //0x8B
        [Opcode("0x8B")]
        private void adcAE()
        {
            byte tempA = cpu.A;
            int result = cpu.A + cpu.E;
            result += ((cpu.F & 0x10) != 0) ? (byte)1 : (byte)0;
            cpu.F = (result > 255) ? (byte)0x10 : (byte)0;
            cpu.A = (byte)(result &= 255);
            if (cpu.A == 0)
                cpu.F |= 0x80;
            if (((cpu.A ^ cpu.E ^ tempA) & (byte)0x10) != 0)
                cpu.F |= 0x20;
            cpu.m = 1;
        }
        //0x9B
        [Opcode("0x9B")]
        private void sbcAE()
        {
            byte tempA = cpu.A;
            int result = cpu.A - cpu.E;
            result -= ((cpu.F & 0x10) != 0) ? (byte)1 : (byte)0;
            cpu.F = (result < 0) ? (byte)0x50 : (byte)0x40;
            cpu.A = (byte)(result &= 255);
            if (cpu.A == 0)
                cpu.F |= 0x80;
            if (((cpu.A ^ cpu.E ^ tempA) & (byte)0x10) != 0)
                cpu.F |= 0x20;
            cpu.m = 1;
        }
        //0xAB
        [Opcode("0xAB")]
        private void xorE()
        {
            cpu.A ^= cpu.E;
            cpu.A &= 255;
            cpu.F = (cpu.A != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 1;
        }
        //0xBB
        [Opcode("0xBB")]
        private void CpAE()
        {
            byte tmp = cpu.A;
            tmp -= cpu.E;
            cpu.F = (tmp < 0) ? (byte)0x50 : (byte)0x40;
            tmp &= 255;
            if (tmp == 0)
                cpu.F |= 0x80;
            if (((cpu.A ^ cpu.E ^ tmp) & (byte)0x10) != 0)
                cpu.F |= 0x20;
            cpu.m = 1;
        }
        //0xCB
        [Opcode("0xCB")]
        private void extOps()
        {
            var i = mmu.RB(cpu.IP);
            cpu.IP++;
            cpu.IP &= 65535;
            extendedMap[i].Invoke();

        }
        //0xDB
        //0xEB
        //xx
        //0xFB
        [Opcode("0xFB")]
        private void ei()
        {
            cpu.IME = true;
            imeiFlag = true;
            cpu.m = 1;
        }

        //0x4C
        [Opcode("0x4C")]
        private void ldCH()
        {
            cpu.C = cpu.H;
            cpu.m = 1;
        }
        //0x5C
        [Opcode("0x5C")]
        private void ldEH()
        {
            cpu.E = cpu.H;
            cpu.m = 1;
        }
        //0x6C
        [Opcode("0x6C")]
        private void ldLH()
        {
            cpu.L = cpu.H;
            cpu.m = 1;
        }
        //0x7C
        [Opcode("0x7C")]
        private void ldAH()
        {
            cpu.A = cpu.H;
            cpu.m = 1;
        }
        //0x8C
        [Opcode("0x8C")]
        private void adcAH()
        {
            byte tempA = cpu.A;
            int result = cpu.A + cpu.H;
            result += ((cpu.F & 0x10) != 0) ? (byte)1 : (byte)0;
            cpu.F = (result > 255) ? (byte)0x10 : (byte)0;
            cpu.A = (byte)(result &= 255);
            if (cpu.A == 0)
                cpu.F |= 0x80;
            if (((cpu.A ^ cpu.H ^ tempA) & (byte)0x10) != 0)
                cpu.F |= 0x20;
            cpu.m = 1;
        }
        //0x9C
        [Opcode("0x9C")]
        private void sbcAH()
        {
            byte tempA = cpu.A;
            int result = cpu.A - cpu.H;
            result -= ((cpu.F & 0x10) != 0) ? (byte)1 : (byte)0;
            cpu.F = (result < 0) ? (byte)0x50 : (byte)0x40;
            cpu.A = (byte)(result &= 255);
            if (cpu.A == 0)
                cpu.F |= 0x80;
            if (((cpu.A ^ cpu.H ^ tempA) & (byte)0x10) != 0)
                cpu.F |= 0x20;
            cpu.m = 1;
        }
        //0xAC
        [Opcode("0xAC")]
        private void xorH()
        {
            cpu.A ^= cpu.H;
            cpu.A &= 255;
            cpu.F = (cpu.A != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 1;
        }
        //0xBC
        [Opcode("0xBC")]
        private void CpAH()
        {
            byte tmp = cpu.A;
            tmp -= cpu.H;
            cpu.F = (tmp < 0) ? (byte)0x50 : (byte)0x40;
            tmp &= 255;
            if (tmp == 0)
                cpu.F |= 0x80;
            if (((cpu.A ^ cpu.H ^ tmp) & (byte)0x10) != 0)
                cpu.F |= 0x20;
            cpu.m = 1;
        }
        //0xCC
        [Opcode("0xCC")]
        private void callznn()
        {
            cpu.m = 3;
            if ((cpu.F & 0x80) == 0x80)
            {
                cpu.SP -= 2;
                mmu.WW(cpu.SP, (ushort)(cpu.IP + 2));
                cpu.IP = mmu.RW(cpu.IP);
                cpu.m += 2; }
            else
                cpu.IP += 2;
        }
        //0xDC
        [Opcode("0xDC")]
        private void callcnn()
        {
            cpu.m = 3;
            if ((cpu.F & 0x10) == 0x10)
            {
                cpu.IP -= 2; mmu.WW(cpu.SP, (ushort)(cpu.IP + 2));
                cpu.IP = mmu.RW(cpu.IP);
                cpu.m += 2;
            }
            else
                cpu.IP += 2;
        }
        //0xEC
        //0xFC
        //xx

        //0x4D
        [Opcode("0x4D")]
        private void ldCL()
        {
            cpu.C = cpu.L;
            cpu.m = 1;
        }
        //0x5D
        [Opcode("0x5D")]
        private void ldEL()
        {
            cpu.E = cpu.L;
            cpu.m = 1;
        }
        //0x6D
        [Opcode("0x6D")]
        private void ldLL()
        {
            cpu.L = cpu.L;
            cpu.m = 1;
        }
        //0x7D
        [Opcode("0x7D")]
        private void ldAL()
        {
            cpu.A = cpu.L;
            cpu.m = 1;
        }
        //0x8D
        [Opcode("0x8D")]
        private void adcAL()
        {
            byte tempA = cpu.A;
            int result = cpu.A + cpu.L;
            result += ((cpu.F & 0x10) != 0) ? (byte)1 : (byte)0;
            cpu.F = (result > 255) ? (byte)0x10 : (byte)0;
            cpu.A = (byte)(result &= 255);
            if (cpu.A == 0)
                cpu.F |= 0x80;
            if (((cpu.A ^ cpu.L ^ tempA) & (byte)0x10) != 0)
                cpu.F |= 0x20;
            cpu.m = 1;
        }
        //0x9D
        [Opcode("0x9D")]
        private void sbcAL()
        {
            byte tempA = cpu.A;
            int result = cpu.A - cpu.L;
            result -= ((cpu.F & 0x10) != 0) ? (byte)1 : (byte)0;
            cpu.F = (result < 0) ? (byte)0x50 : (byte)0x40;
            cpu.A = (byte)(result &= 255);
            if (cpu.A == 0)
                cpu.F |= 0x80;
            if (((cpu.A ^ cpu.L ^ tempA) & (byte)0x10) != 0)
                cpu.F |= 0x20;
            cpu.m = 1;
        }
        //0xAD
        [Opcode("0xAD")]
        private void xorL()
        {
            cpu.A ^= cpu.L;
            cpu.A &= 255;
            cpu.F = (cpu.A != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 1;
        }
        //0xBD
        [Opcode("0xBD")]
        private void CpAL()
        {
            byte tmp = cpu.A;
            tmp -= cpu.L;
            cpu.F = (tmp < 0) ? (byte)0x50 : (byte)0x40;
            tmp &= 255;
            if (tmp == 0)
                cpu.F |= 0x80;
            if (((cpu.A ^ cpu.L ^ tmp) & (byte)0x10) != 0)
                cpu.F |= 0x20;
            cpu.m = 1;
        }
        //0xCD
        [Opcode("0xCD")]
        private void callnn()
        {
            cpu.SP -= 2;
            mmu.WW(cpu.SP, (ushort)(cpu.IP + 2));
            cpu.IP = mmu.RW(cpu.IP);
            cpu.m = 5;
        }
        //0xDD
        //0xED
        //0xFD
        //xx

        //0x4E
        [Opcode("0x4E")]
        private void ldCHLm()
        {
            cpu.C = mmu.RB(cpu.HL);
            cpu.m = 2;
        }
        //0x5E
        [Opcode("0x5E")]
        private void ldEHLm()
        {
            cpu.E = mmu.RB(cpu.HL);
            cpu.m = 2;
        }
        //0x6E
        [Opcode("0x6E")]
        private void ldLHLm()
        {
            cpu.L = mmu.RB(cpu.HL);
            cpu.m = 2;
        }
        //0x7E
        [Opcode("0x7E")]
        private void ldAHLm()
        {
            cpu.A = mmu.RB(cpu.HL);
            cpu.m = 2;
        }
        //0x8E
        [Opcode("0x8E")]
        private void adcAHLm()
        {
            byte tempA = cpu.A;
            byte tmp = mmu.RB(cpu.HL);
            int result = cpu.A + tmp;
            result += ((cpu.F & 0x10) != 0) ? (byte)1 : (byte)0;
            cpu.F = (result > 255) ? (byte)0x10 : (byte)0;
            cpu.A = (byte)(result &= 255);
            if (cpu.A == 0)
                cpu.F |= 0x80;
            if (((cpu.A ^ tmp ^ tempA) & (byte)0x10) != 0)
                cpu.F |= 0x20;
            cpu.m = 2;
        }
        //0x9E
        [Opcode("0x9E")]
        private void sbcAHLm()
        {
            byte tempA = cpu.A;
            byte tmp = mmu.RB(cpu.HL);
            int result = cpu.A - tmp;
            result -= ((cpu.F & 0x10) != 0) ? (byte)1 : (byte)0;
            cpu.F = (result < 0) ? (byte)0x50 : (byte)0x40;
            cpu.A = (byte)(result &= 255);
            if (cpu.A == 0)
                cpu.F |= 0x80;
            if (((cpu.A ^ tmp ^ tempA) & (byte)0x10) != 0)
                cpu.F |= 0x20;
            cpu.m = 2;
        }
        //0xAE
        [Opcode("0xAE")]
        private void xorHLm()
        {
            cpu.A ^= mmu.RB(cpu.HL);
            cpu.A &= 255;
            cpu.F = (cpu.A != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 2;
        }
        //0xBE
        [Opcode("0xBE")]
        private void CpAHLm()
        {
            byte tmp = cpu.A;
            byte tmpHL = mmu.RB(cpu.HL);
            tmp -= tmpHL;
            cpu.F = (tmp < 0) ? (byte)0x50 : (byte)0x40;
            tmp &= 255;
            if (tmp == 0)
                cpu.F |= 0x80;
            if (((cpu.A ^ tmpHL ^ tmp) & (byte)0x10) != 0)
                cpu.F |= 0x20;
            cpu.m = 2;
        }
        //0xCE
        [Opcode("0xCE")]
        private void adcAn()
        {
            byte tempA = cpu.A;
            byte n = mmu.RB(cpu.IP);
            int result = cpu.A + n;
            cpu.IP++;
            result += ((cpu.F & 0x10) != 0) ? (byte)1 : (byte)0;
            cpu.F = (result > 255) ? (byte)0x10 : (byte)0;
            cpu.A = (byte)(result &= 255);
            if (cpu.A == 0)
                cpu.F |= 0x80;
            if (((cpu.A ^ n ^ tempA) & (byte)0x10) != 0)
                cpu.F |= 0x20;
            cpu.m = 2;
        }
        //0xDE
        [Opcode("0xDE")]
        private void sbcAn()
        {
            byte tempA = cpu.A;
            byte n = mmu.RB(cpu.IP);
            int result = cpu.A - n;
            cpu.IP++;
            result -= ((cpu.F & 0x10) != 0) ? (byte)1 : (byte)0;
            cpu.F = (result < 0) ? (byte)0x50 : (byte)0x40;
            cpu.A = (byte)(result &= 255);
            if (cpu.A == 0)
                cpu.F |= 0x80;
            if (((cpu.A ^ n ^ tempA) & (byte)0x10) != 0)
                cpu.F |= 0x20;
            cpu.m = 2;
        }
        //0xEE
        [Opcode("0xEE")]
        private void xorn()
        {
            cpu.A ^= mmu.RB(cpu.IP);
            cpu.IP++;
            cpu.A &= 255;
            cpu.F = (cpu.A != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 2;
        }
        //0xFE
        [Opcode("0xFE")]
        private void cpn()
        {
            int tmp = cpu.A;
            byte n = mmu.RB(cpu.IP);
            cpu.IP++;
            tmp -= n;
            cpu.F = (tmp < 0) ? (byte)0x50 : (byte)0x40;
            tmp &= 255;
            if (tmp == 0)
                cpu.F |= 0x80;
            if (((cpu.A ^ n ^ tmp) & (byte)0x10) != 0)
                cpu.F |= 0x20;
            cpu.m =2;
        }

        //0x4F
        [Opcode("0x4F")]
        private void ldCA()
        {
            cpu.C = cpu.A;
            cpu.m = 1;
        }
        //0x5F
        [Opcode("0x5F")]
        private void ldEA()
        {
            cpu.E = cpu.A;
            cpu.m = 1;
        }
        //0x6F
        [Opcode("0x6F")]
        private void ldLA()
        {
            cpu.L = cpu.A;
            cpu.m = 1;
        }
        //0x7F
        [Opcode("0x7F")]
        private void ldAA()
        {
            cpu.A = cpu.A;
            cpu.m = 1;
        }
        //0x8F
        [Opcode("0x8F")]
        private void adcAA()
        {
            byte tempA = cpu.A;
            int result = cpu.A + cpu.A;
            result += ((cpu.F & 0x10) != 0) ? (byte)1 : (byte)0;
            cpu.F = (result > 255) ? (byte)0x10 : (byte)0;
            cpu.A = (byte)(result &= 255);
            if (cpu.A == 0)
                cpu.F |= 0x80;
            if (((cpu.A ^ cpu.A ^ tempA) & (byte)0x10) != 0)
                cpu.F |= 0x20;
            cpu.m = 1;
        }
        //0x9F
        [Opcode("0x9F")]
        private void sbcAA()
        {
            byte tempA = cpu.A;
            int result = cpu.A - cpu.A;
            result -= ((cpu.F & 0x10) != 0) ? (byte)1 : (byte)0;
            cpu.F = (result < 0) ? (byte)0x50 : (byte)0x40;
            cpu.A = (byte)(result &= 255);
            if (cpu.A == 0)
                cpu.F |= 0x80;
            if (((cpu.A ^ cpu.A ^ tempA) & (byte)0x10) != 0)
                cpu.F |= 0x20;
            cpu.m = 1;
        }
        //0xAF
        [Opcode("0xAF")]
        private void xorA()
        {
            cpu.A ^= cpu.A;
            cpu.A &= 255;
            cpu.F = (cpu.A != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 1;
        }
        //0xBF
        [Opcode("0xBF")]
        private void CpAA()
        {
            byte tmp = cpu.A;
            tmp -= cpu.A;
            cpu.F = (tmp < 0) ? (byte)0x50 : (byte)0x40;
            tmp &= 255;
            if (tmp == 0)
                cpu.F |= 0x80;
            if (((cpu.A ^ cpu.A ^ tmp) & (byte)0x10) != 0)
                cpu.F |= 0x20;
            cpu.m = 1;
        }
        //0xCF
        [Opcode("0xCF")]
        private void rst8()
        {
            cpu.Rsv();
            cpu.SP -= 2;
            mmu.WW(cpu.SP, cpu.IP);
            cpu.IP = 0x08;
            cpu.m = 3;
        }
        //0xDF
        [Opcode("0xDF")]
        private void rst18()
        {
            cpu.Rsv();
            cpu.SP -= 2;
            mmu.WW(cpu.SP, cpu.IP);
            cpu.IP = 0x18;
            cpu.m = 3;
        }
        //0xEF
        [Opcode("0xEF")]
        private void rst28()
        {
            cpu.Rsv();
            cpu.SP -= 2;
            mmu.WW(cpu.SP, cpu.IP);
            cpu.IP = 0x28;
            cpu.m = 3;
        }
        //0xFF
        [Opcode("0xFF")]
        private void rst38()
        {
            cpu.Rsv();
            cpu.SP -= 2;
            mmu.WW(cpu.SP, cpu.IP);
            cpu.IP = 0x38;
            cpu.m = 3;
        }

        //extended
        //0x7C
        [Opcode("0x7C", Extended = true)]
        private void bit7H()
        {
           cpu.F &= 0x1F;
            cpu.F |= 0x20;
            cpu.F = ((cpu.H & 0x80)!=0) ? (byte)0 : (byte)0x80;
            cpu.m = 2;
        }
        [Opcode("0x78", Extended = true)]
        private void bit7B()
        {
            cpu.F &= 0x1F;
            cpu.F |= 0x20;
            cpu.F = ((cpu.B & 0x80) != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 2;
        }

        [Opcode("0x6E", Extended = true)]
        private void bit5HLm()
        {
            byte temp = mmu.RB(cpu.HL);
            cpu.F &= 0x1F;
            cpu.F |= 0x20;
            cpu.F = ((temp & 0x20) != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 4;
        }
        [Opcode("0x76", Extended = true)]
        private void bit6HLm()
        {
            byte temp = mmu.RB(cpu.HL);
            cpu.F &= 0x1F;
            cpu.F |= 0x20;
            cpu.F = ((temp & 0x40) != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 4;
        }

        [Opcode("0x7E", Extended = true)]
        private void bit7HLm()
        {
            byte temp = mmu.RB(cpu.HL);
            cpu.F &= 0x1F;
            cpu.F |= 0x20;
            cpu.F = ((temp & 0x80) != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 4;
        }
        [Opcode("0x7F", Extended = true)]
        private void bit7A()
        {
            cpu.F &= 0x1F;
            cpu.F |= 0x20;
            cpu.F = ((cpu.A & 0x80) != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 2;
        }
        [Opcode("0x48", Extended = true)]
        private void bit1B()
        {
            cpu.F &= 0x1F;
            cpu.F |= 0x20;
            cpu.F = ((cpu.B & 0x01) != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 2;
        }
        [Opcode("0x4F", Extended = true)]
        private void bit1A()
        {
            cpu.F &= 0x1F;
            cpu.F |= 0x20;
            cpu.F = ((cpu.A & 0x01) != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 2;
        }
        [Opcode("0x57", Extended = true)]
        private void bit2A()
        {
            cpu.F &= 0x1F;
            cpu.F |= 0x20;
            cpu.F = ((cpu.A & 0x04) != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 2;
        }
        [Opcode("0x50", Extended = true)]
        private void bit2B()
        {
            cpu.F &= 0x1F;
            cpu.F |= 0x20;
            cpu.F = ((cpu.B & 0x04) != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 2;
        }
        [Opcode("0x5F", Extended = true)]
        private void bit3A()
        {
            cpu.F &= 0x1F;
            cpu.F |= 0x20;
            cpu.F = ((cpu.A & 0x08) != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 2;
        }
        [Opcode("0x58", Extended = true)]
        private void bit3B()
        {
            cpu.F &= 0x1F;
            cpu.F |= 0x20;
            cpu.F = ((cpu.B & 0x08) != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 2;
        }
        [Opcode("0x67", Extended = true)]
        private void bit4A()
        {
            cpu.F &= 0x1F;
            cpu.F |= 0x20;
            cpu.F = ((cpu.A & 0x10) != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 2;
        }
        [Opcode("0x61", Extended = true)]
        private void bit4C()
        {
            cpu.F &= 0x1F;
            cpu.F |= 0x20;
            cpu.F = ((cpu.C & 0x10) != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 2;
        }
        [Opcode("0x60", Extended = true)]
        private void bit4B()
        {
            cpu.F &= 0x1F;
            cpu.F |= 0x20;
            cpu.F = ((cpu.B & 0x10) != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 2;
        }
        [Opcode("0x70", Extended = true)]
        private void bit6B()
        {
            cpu.F &= 0x1F;
            cpu.F |= 0x20;
            cpu.F = ((cpu.B & 0x40) != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 2;
        }
        [Opcode("0x77", Extended = true)]
        private void bit6A()
        {
            cpu.F &= 0x1F;
            cpu.F |= 0x40;
            cpu.F = ((cpu.A & 0x40) != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 2;
        }
        [Opcode("0x68", Extended = true)]
        private void bit5B()
        {
            cpu.F &= 0x1F;
            cpu.F |= 0x20;
            cpu.F = ((cpu.B & 0x20) != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 2;   
        }
        [Opcode("0x69", Extended = true)]
        private void bit5C()
        {
            cpu.F &= 0x1F;
            cpu.F |= 0x20;
            cpu.F = ((cpu.C & 0x20) != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 2;
        }
        [Opcode("0x6F", Extended = true)]
        private void bit5A()
        {
            cpu.F &= 0x1F;
            cpu.F |= 0x20;
            cpu.F = ((cpu.A & 0x20) != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 2;
        }
        [Opcode("0xDB", Extended = true)]
        private void set3E()
        {
            cpu.E |= 0x08;
            cpu.m = 2;
        }
        [Opcode("0xD8", Extended = true)]
        private void set3B()
        {
            cpu.B |= 0x08;
            cpu.m = 2;
        }
        [Opcode("0xDE", Extended = true)]
        private void set3HL()
        {
            byte tmp = mmu.RB(cpu.HL);
            tmp |= 0x08;
            mmu.WB(cpu.HL, tmp);
            cpu.m = 4;
        }
        [Opcode("0xF4", Extended = true)]
        private void set4H()
        {
            cpu.H |= 0x10;
            cpu.m = 2;
        }
        [Opcode("0xF8", Extended = true)]
        private void set7B()
        {
            cpu.B |= 0x80;
            cpu.m = 2;
        }
        [Opcode("0xFE", Extended = true)]
        private void set7HL()
        {
            byte tmp = mmu.RB(cpu.HL);
            tmp |= 0x80;
            mmu.WB(cpu.HL, tmp);
            cpu.m = 4;
        }

        //0x11
        [Opcode("0x11", Extended = true)]
        private void rlC()
        {
            byte ci = ((cpu.F & 0x10) != 0) ? (byte)1 : (byte)0;
            byte co = ((cpu.C & 0x80) != 0) ? (byte)0x10 : (byte)0;
            cpu.C = (byte)((cpu.C << 1) + ci);
            cpu.C &= 255;
            cpu.F = (cpu.C != 0) ? (byte)0 : (byte)0x80;
            cpu.F = (byte)((cpu.F & 0xEF) + co);
            cpu.m = 2;
        }
        [Opcode("0x33", Extended = true)]
        private void swapE()
        {
            byte tmp = cpu.E;
            cpu.E = (byte)(((tmp & 0xF) << 4) | ((tmp & 0xF0) >> 4));
            cpu.F = (cpu.E != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 1;
        }
        [Opcode("0x37", Extended = true)]
        private void swapA()
        {
            byte tmp = cpu.A;
            cpu.A = (byte)(((tmp & 0xF) << 4) | ((tmp & 0xF0) >> 4));
            cpu.F = (cpu.A !=0)? (byte)0 : (byte)0x80;
            cpu.m = 1;
        }
        [Opcode("0x40", Extended = true)]
        private void bit0B()
        {
            cpu.F &= 0x1F;
            cpu.F |= 0x20;
            cpu.F = ((cpu.B & 0x01) != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 2;
        }


        [Opcode("0x41", Extended = true)]
        private void bit0C()
        {
            cpu.F &= 0x1F;
            cpu.F |= 0x20;
            cpu.F = ((cpu.C & 0x01)!=0) ? (byte)0 : (byte)0x80;
            cpu.m = 2;
        }

        [Opcode("0x46", Extended = true)]
        private void bit0HLm()
        {
            byte temp = mmu.RB(cpu.HL);
            cpu.F &= 0x1F;
            cpu.F |= 0x20;
            cpu.F = ((temp & 0x01) != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 4;
        }
        [Opcode("0x47", Extended = true)]
        private void bit0A()
        {
            cpu.F &= 0x1F;
            cpu.F |= 0x20;
            cpu.F = ((cpu.A & 0x01) != 0) ? (byte)0 : (byte)0x80;
            cpu.m = 2;
        }

        [Opcode("0x87", Extended = true)]
        private void res0A()
        {
            cpu.A &= 0xFE;
            cpu.m = 2; 
        }
        [Opcode("0x9E", Extended = true)]
        private void res3HLm()
        {
            byte tmp = mmu.RB(cpu.HL);
            tmp &= 0xF7;
            mmu.WB(cpu.HL, tmp);
            cpu.m = 4;
        }

        [Opcode("0xBE", Extended = true)]
        private void res7HLm()
        {
            byte tmp = mmu.RB(cpu.HL);
            tmp &= 0x7F;
            mmu.WB(cpu.HL, tmp);
            cpu.m = 4;
        }

        [Opcode("0x27", Extended = true)]
        private void slaA()
        {
            int co = ((cpu.A & 0x80)!=0) ? 0x10 : 0;
            cpu.A = (byte)((cpu.A << 1) & 255);
            cpu.F = (cpu.A !=0) ? (byte)0 : (byte)0x80;
            cpu.F = (byte)((cpu.F & 0xEF) + co);
            cpu.m = 2;
        }
        [Opcode("0x86", Extended = true)]
        private void res0HLm()
        {
            byte temp = mmu.RB(cpu.HL);
            temp &= 0xFE;
            mmu.WB(cpu.HL, temp);
            cpu.m = 4;
        }
        [Opcode("0x3f", Extended = true)]
        private void srlA()
        {
            int co = (cpu.A & 1)!=0 ? 0x10 : 0;
            cpu.A = (byte)((cpu.A >> 1) & 255);
            cpu.F = (cpu.A) != 0 ? (byte)0 : (byte)0x80;
            cpu.F = (byte)((cpu.F & 0xEF) + co);
            cpu.m = 2;
        }
        [Opcode("0x38", Extended = true)]
        private void srlB()
        {
            int co = (cpu.B & 1) != 0 ? 0x10 : 0;
            cpu.B = (byte)((cpu.B >> 1) & 255);
            cpu.F = (cpu.B) != 0 ? (byte)0 : (byte)0x80;
            cpu.F = (byte)((cpu.F & 0xEF) + co);
            cpu.m = 2;
        }
        [Opcode("0x39", Extended = true)]
        private void srlC()
        {
            int co = (cpu.C & 1) != 0 ? 0x10 : 0;
            cpu.C = (byte)((cpu.C >> 1) & 255);
            cpu.F = (cpu.C) != 0 ? (byte)0 : (byte)0x80;
            cpu.F = (byte)((cpu.F & 0xEF) + co);
            cpu.m = 2;
        }

        [Opcode("0x19", Extended = true)]
        private void rrC()
        {
            var ci = (cpu.F & 0x10) !=0 ? 0x80 : 0;
            var co = (cpu.C & 1) != 0 ? 0x10 : 0;
            cpu.C = (byte)((cpu.C >> 1) + ci);
            cpu.C &= 255;
            cpu.F = (cpu.C != 0) ? (byte)0 : (byte)0x80;
            cpu.F = (byte)((cpu.F & 0xEF) + co);
            cpu.m = 2;
        }
        [Opcode("0x1A", Extended = true)]
        private void rrD()
        {
            var ci = (cpu.F & 0x10) != 0 ? 0x80 : 0;
            var co = (cpu.D & 1) != 0 ? 0x10 : 0;
            cpu.D = (byte)((cpu.D >> 1) + ci);
            cpu.D &= 255;
            cpu.F = (cpu.D != 0) ? (byte)0 : (byte)0x80;
            cpu.F = (byte)((cpu.F & 0xEF) + co);
            cpu.m = 2;
        }
        [Opcode("0x1B", Extended = true)]
        private void rrE()
        {
            var ci = (cpu.F & 0x10) != 0 ? 0x80 : 0;
            var co = (cpu.E & 1) != 0 ? 0x10 : 0;
            cpu.E = (byte)((cpu.E >> 1) + ci);
            cpu.E &= 255;
            cpu.F = (cpu.E != 0) ? (byte)0 : (byte)0x80;
            cpu.F = (byte)((cpu.F & 0xEF) + co);
            cpu.m = 2;
        }
        [Opcode("0x1C", Extended = true)]
        private void rrH()
        {
            var ci = (cpu.F & 0x10) != 0 ? 0x80 : 0;
            var co = (cpu.H & 1) != 0 ? 0x10 : 0;
            cpu.H = (byte)((cpu.H >> 1) + ci);
            cpu.H &= 255;
            cpu.F = (cpu.H != 0) ? (byte)0 : (byte)0x80;
            cpu.F = (byte)((cpu.F & 0xEF) + co);
            cpu.m = 2;
        }



        private void RST40()
        {
            cpu.Rsv();
            cpu.SP -= 2;
            mmu.WW(cpu.SP, cpu.IP);
            cpu.IP = 0x40;
            cpu.m = 3;
        }
        private void RST48()
        {
            cpu.Rsv();
            cpu.SP -= 2;
            mmu.WW(cpu.SP, cpu.IP);
            cpu.IP = 0x48;
            cpu.m = 3;
        }
        private void RST50()
        {
            cpu.Rsv();
            cpu.SP -= 2;
            mmu.WW(cpu.SP, cpu.IP);
            cpu.IP = 0x50;
            cpu.m = 3;
        }
        private void RST58()
        {
            cpu.Rsv();
            cpu.SP -= 2;
            mmu.WW(cpu.SP, cpu.IP);
            cpu.IP = 0x58;
            cpu.m = 3;
        }
        private void RST60()
        {
            cpu.Rsv();
            cpu.SP -= 2;
            mmu.WW(cpu.SP, cpu.IP);
            cpu.IP = 0x60;
            cpu.m = 3;
        }

    }
}









