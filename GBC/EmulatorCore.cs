using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBC
{
    public class EmulatorCore
    {
        public CPU cpu = new CPU();
        MMU mmu = new MMU();


        public void LoadRom(byte[] rom)
        {
            this.mmu.LoadRom(rom);
        }

        public void emulateCycle()
        {
            cpu.B = 0xf0;
            cpu.C = 0xA0;
            ushort bc = cpu.BC;
            cpu.BC = bc;
        }

        public void emulationLoop()
        {

        }

        //opcode functions

        //0x00
        private void nop()
        {
            cpu.m = 1;
            cpu.t = 4;
        }
        //0x01
        private void ldBCnn()
        {
            cpu.C = mmu.RB(cpu.IP++);
            cpu.B = mmu.RB(cpu.IP++);
            cpu.m = 3;
            
        }

        //0x02
        private void ldBCmA()
        {
            mmu.WB(cpu.BC, cpu.A);
            cpu.m = 2;
        }
        //0x03
        private void incBC()
        {
            cpu.C++;
            if (cpu.C == 0)
                cpu.B ++;
            cpu.m=1;
        }

        //0x04
        private void incB()
        {
            cpu.B++;
            //flag
            cpu.m = 1;
        }

        //0x05
        private void decB()
        {
            cpu.B--;
            cpu.F = (cpu.B == 0) ? (byte)0 : (byte)0x80;
            cpu.m = 1;
        }
        //0x06
        private void ldBn()
        {
            cpu.B = mmu.RB(cpu.IP++);
            cpu.m = 2;
        }

        //0x07
        private void rrcA()
        {
            cpu.A = RRC(cpu.A);
            cpu.m = 1;
        }
        //0x08
        private void ldMnnSp()
        {
            mmu.WB(mmu.RW(cpu.IP), cpu.SP);
            cpu.m = 3;
        }
        //0x09
        private void addHlBc()
        {
            cpu.HL = (ushort)(cpu.HL + cpu.BC);
            cpu.m = 1;
        }
        //0x0A
        private void ldABcM()
        {
            cpu.A = mmu.RB(cpu.BC);
            cpu.m = 1;
        }
        //0x0B
        private void decBc()
        {
            cpu.BC = (ushort)(cpu.BC-1);
            cpu.m = 1;
        }

        //0x0C
        private void incC()
        {
            cpu.C++;
            //flag
            cpu.m = 1;
        }

        //0x0D
        private void decC()
        {
            cpu.C++;
            //flag
            cpu.m = 1;
        }

        //0x0E
        private void ldCn()
        {
            cpu.C = mmu.RB(cpu.IP);
            cpu.m = 2;
        }
        //0x0F
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
            //flaga
            return (byte)(value << 1 | value >> 7);
        }
        //0x10
        private void  stop()
        {
           
        }
        //0x11
        private void ldDEnn(byte value)
        {
            cpu.E = mmu.RB(cpu.IP++);
            cpu.D = mmu.RB(cpu.IP++);
            cpu.m = 3;
        }
        //0x12
        private void ldDEmA()
        {
            mmu.WB(cpu.DE, cpu.A);
            cpu.m = 2;
        }
        //0x13
        private void incDE()
        {
            cpu.E++;
            if (cpu.E == 0)
                cpu.D++;
            cpu.m = 1;
        }
        //0x14
        private void incD()
        {
            cpu.D++;
            //flag
            cpu.m = 1;
        }
        //0x15
        private void decD()
        {
            cpu.D--;
            cpu.F = (cpu.D == 0) ? (byte)0 : (byte)0x80;
            cpu.m = 1;
        }
        //0x16
        private void ldDn()
        {
            cpu.D = mmu.RB(cpu.IP++);
            cpu.m = 2;
        }
        //0x17
        private void rlA()
        {
            cpu.A = (byte)(cpu.A<<1);
            cpu.m = 1;
        }
        //0x18
        private void jrn()
        {
            byte i = mmu.RB(cpu.IP);
            if(i>127)
                i = (byte)-((~i + 1) & 255);
            cpu.IP += mmu.RB(cpu.IP);
            cpu.m = 1;
        }
        //0x19
        private void addHLDE()
        {
            cpu.HL = (ushort)(cpu.HL + cpu.DE);
            cpu.m = 1;
        }
        //0x1A
        private void ldADEM()
        {
            cpu.A = mmu.RB(cpu.DE);
            cpu.m = 1;
        }
        //0x1B
        private void decDE()
        {
            cpu.BC = (ushort)(cpu.DE - 1);
            cpu.m = 1;
        }
        //0x1C
        private void incE()
        {
            cpu.E++;
            //flag
            cpu.m = 1;
        }
        //0x1D
        private void decE()
        {
            cpu.E--;
            //flag
            cpu.m = 1;
        }
        //0x1E
        private void ldEn()
        {
            cpu.E = mmu.RB(cpu.IP);
            cpu.m = 2;
        }
        //0x1F
        private void rr()
        {
            cpu.A = (byte)(cpu.A >>1);
            cpu.m = 1;
        }
        //0x20
        private void JRNzn()
        {
            cpu.m = 2;
            if ((cpu.F & 0x80) != 0x00)
                return;
            byte i = mmu.RB(cpu.IP);
            if (i > 127)
                i = (byte)-((~i + 1) & 255);
            cpu.IP += mmu.RB(cpu.IP);
        }
        //0x21
        private void ldHLnn()
        {
            cpu.L = mmu.RB(cpu.IP++);
            cpu.H = mmu.RB(cpu.IP++);
            cpu.m = 3;

        }
        //0x22
        private void ldi()
        {
            mmu.WB(cpu.HL, cpu.A);
            cpu.HL++;
        }
        //0x23
        private void incHL()
        {
            cpu.L++;
            if (cpu.L == 0)
                cpu.H++;
            cpu.m = 1;
        }
        //0x24
        private void incH()
        {
            cpu.H++;
            //flag
            cpu.m = 1;
        }
        //0x25
        private void decH()
        {
            cpu.H--;
            //flag
            cpu.m = 1;
        }
        //0x26
        private void ldHn()
        {
            cpu.H = mmu.RB(cpu.IP++);
            cpu.m = 2;
        }
        //0x27
        private void DAA()
        {
            var a = cpu.A;
            if ((byte)(cpu.F & 0x20)>0 || ((byte)(cpu.A & 15) > 9))
                cpu.A += 6; cpu.F &= 0xEF;
            if ((cpu.A & 0x20) >1|| (a > 0x99))
            {
                cpu.A += 0x60;
                cpu.F |= 0x10;
            }
            cpu.m = 1;
        }
        //0x28
        private void JRzn()
        {
            cpu.m = 2;
            if ((cpu.F & 0x80) == 0x00)
                return;
            byte i = mmu.RB(cpu.IP);
            if (i > 127)
                i = (byte)-((~i + 1) & 255);
            cpu.IP += mmu.RB(cpu.IP);
        }

        //0x29
        private void addHLHL()
        {
            cpu.HL = (ushort)(cpu.HL + cpu.HL);
            cpu.m = 1;
        }
        //0x2A
        private void ldiAHL()
        {
            cpu.A = mmu.RB(cpu.HL);
            cpu.HL++;
            cpu.m = 1;
        }
        //0x2B
        private void decHL()
        {
            cpu.HL--;
            cpu.m = 1;
            //flaga
        }
        //0x2C
        private void incL()
        {
            cpu.L++;
            cpu.m = 1;
            //flaga
        }
        //0x2D
        private void decL()
        {
            cpu.L--;
            cpu.m = 1;
            //flaga
        }
        //0x2E
        private void ldLn()
        {
            cpu.L = mmu.RB(cpu.IP);
            cpu.m = 2;
        }

        //0x2F
        private void cpl()
        {
            cpu.A = (byte)(~cpu.A);
            cpu.m = 1;
        }


    }
}
