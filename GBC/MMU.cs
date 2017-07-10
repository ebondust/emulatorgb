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
        byte[] rom;// rom banks
        byte[] wram; // working ram
        byte[] eram; // external ram
        byte[] zram; // zero-page ram, high speed memory

        public void LoadRom(byte[] rom)
        {
            this.rom = rom;
        }

        public Byte RB(ushort addr) // read byte from memory.
        {

            switch (addr & 0xF000)
            {
                case 0x0000:
                    if (inBios)
                    {
                        if (IP < 0x0100)
                            return bios[addr];
                        else if (IP == 0x0100)
                            inBios = false;
                    }
                    break;
                case 0x1000:
                case 0x2000:
                case 0x3000:
                    return rom[addr];
                    break;

                case 0x4000:
                case 0x5000:
                case 0x6000:
                case 0x7000:
                    return rom[addr];
                    break;

                case 0x8000:
                case 0x9000:
                    return 0;
                    break;

                case 0xA000:
                case 0xB000:
                    return eram[addr];
                    break;
                case 0xC000:
                case 0xD000:
                    return wram[addr];
                    break;
                case 0xE000:
                    return wram[addr];
                    break;
                case 0xF000:
                    if (IP <= 0xFDFF)
                        return wram[addr];
                    if (IP <= 0xFE9F)
                        return 0;
                    if (IP <= 0xFF7F)
                        return 0;
                    if (IP <= 0xFFFF)
                        return zram[addr];
                    break;

            }

            return rom[IP];
        }

        public ushort RW(ushort addr) // read word(double byte) from memory.
        {
            ushort W = RB(addr);
            W += ((ushort)(RB(addr) << 8));
            return W;
        }

        public void WB(ushort addr, byte value) // write byte to memory.
        {

        }

        public void WW(ushort addr, ushort value) // write word to memory.
        {

        }

    }
}
