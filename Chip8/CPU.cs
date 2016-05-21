using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8
{
    public class CPU
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

        public bool Initialized = false;

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
            Initialized = true;
            pc = 0x200;
            opcode = 0;
            I = 0;
            sp = 0;

            for (int i = 0; i < 16; i++)  // Clear stack and registers V0-VF
            {
                stack[i] = 0;
                register[i] = 0;
                keypad[i] = 0;
            }

            for (int i = 0; i < 4096; i++)// Clear memory
                memory[i] = 0;

            for (int i = 0; i < 80; i++) // Load fontset
                memory[i] = fontset[i];

            for (int i = 0; i < 64*32; i++)// Clear display
                {
                    gfx[i] = 0;
                }

            // Reset timers
            delay_timer = 0;
            sound_timer = 0;
        }



        byte[] fontset =
        {
            0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
            0x20, 0x60, 0x20, 0x20, 0x70, // 1
            0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
            0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
            0x90, 0x90, 0xF0, 0x10, 0x10, // 4
            0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
            0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
            0xF0, 0x10, 0x20, 0x40, 0x40, // 7
            0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
            0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
            0xF0, 0x90, 0xF0, 0x90, 0x90, // A
            0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
            0xF0, 0x80, 0x80, 0x80, 0xF0, // C
            0xE0, 0x90, 0x90, 0x90, 0xE0, // D
            0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
            0xF0, 0x80, 0xF0, 0x80, 0x80  // F
         };

    }
}
