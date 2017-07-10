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
        //0x0C
        private void rrcC()
        {
            cpu.C = RRC(cpu.C);
            cpu.m = 1;
        }

        private ushort RRC(ushort value)
        {
           return (ushort)(value << 1 | value >> 15);
        }
        private byte RRC(byte value)
        {
            //flaga
            return (byte)(value << 1 | value >> 7);
        }

    }
}
