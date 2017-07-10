using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBC
{
    public class Opcode
    {
        public byte Inst;
        public int Arg1;
        public int Arg2;

        public Opcode(Byte Inst)
        {
            this.Inst = Inst;
        }

        public void Read(ref int pc, byte[] memory)
        {
            switch (Inst)
            {
                case 0x00:

                    break;
                case 0x01:

                    break;
                case 0x02:

                    break;
                case 0x03:

                    break;
                case 0x04:

                    break;
                case 0x05:

                    break;
                case 0x06:

                    break;
                case 0x07:

                    break;
                case 0x08:

                    break;
                case 0x09:

                    break;
                case 0x0A:

                    break;
                case 0x0B:

                    break;
                case 0x0C:

                    break;
                case 0x0D:

                    break;
                case 0x0E:

                    break;
                case 0x0F:

                    break;
                case 0x10:

                    break;
                case 0x11:

                    break;
                case 0x12:

                    break;
                case 0x13:

                    break;
                case 0x14:

                    break;
                case 0x15:

                    break;
                case 0x16:

                    break;
                case 0x17:

                    break;
                case 0x18:

                    break;
                case 0x19:

                    break;
                case 0x1A:

                    break;
                case 0x1B:

                    break;
                case 0x1C:

                    break;
                case 0x1D:

                    break;
                case 0x1E:

                    break;
                case 0x1F:

                    break;
            }
        }

    }
}
