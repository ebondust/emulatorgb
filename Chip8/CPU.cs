using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8
{
    class CPU
    {
        protected ushort opcode;
        protected byte[] memory = new byte[4096];
        protected byte[] register = new byte[16];
        protected ushort I;//index register
        protected ushort pc;

        protected byte[] gfx = new byte[64 * 32];

        protected byte delay_timer;
        protected byte sound_timer;
        protected ushort[] stack = new ushort[16];
        protected ushort sp = 0;
        protected byte[] keypad = new byte[16];

        protected void push(ushort value)
        {
            stack[sp] = value;
            sp++;
        }
        protected ushort pop()
        {
            sp--;
            ushort p = stack[sp];
            return p;
        }

        public void initialize()
        {

            pc = 0x200;
            opcode = 0;
            I = 0;
            sp = 0;

            for (int i = 0; i < 16; i++)  // Clear stack and registers V0-VF
            {
                stack[i] = 0;
                register[i] = 0;
            }

            for (int i = 0; i < 4096; i++)// Clear memory
                memory[i] = 0;

            for (int i = 0; i < 64; i++)// Clear display
                for (int j = 0; j < 32; j++)
                {
                    gfx[i * j] = 0;
                }

            // Reset timers
            delay_timer = 0;
            sound_timer = 0;
        }
    }
}
