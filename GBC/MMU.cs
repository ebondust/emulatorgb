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

        public Byte RB() // read byte from memory.
        {

            switch (IP & 0xF000)
            {
                case 0x0000:
                    if (inBios)
                    {
                        if (IP < 0x0100)
                            return bios[IP];
                        else if (IP == 0x0100)
                            inBios = false;
                    }
                    break;
                case 0x1000:
                case 0x2000:
                case 0x3000:
                    return rom[IP];
                    break;

                case 0x4000:
                case 0x5000:
                case 0x6000:
                case 0x7000:
                    return rom[IP];
                    break;

                case 0x8000:
                case 0x9000:
                    return 0;
                    break;

                case 0xA000:
                case 0xB000:
                    return eram[IP];
                    break;
                case 0xC000:
                case 0xD000:
                    return wram[IP];
                    break;
                case 0xE000:
                    return wram[IP];
                    break;
                case 0xF000:
                    if (IP <= 0xFDFF)
                        return wram[IP];
                    if (IP <= 0xFE9F)
                        return 0;
                    if (IP <= 0xFF7F)
                        return 0;
                    if (IP <= 0xFFFF)
                        return zram[IP];
                    break;

            }

            return rom[IP];
        }

        public ushort RW() // read word(double byte) from memory.
        {
            ushort W = RB();
            IP++;
            W += ((ushort)(RB() << 8));
            return W;
        }

        public void WB(byte value) // write byte to memory.
        {

        }

        public void WW(ushort value) // write word to memory.
        {

        }

    }
}
