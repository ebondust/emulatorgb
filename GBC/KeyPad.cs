using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBC
{
    public class KeyPad
    {
        byte[] keys = new byte[] { 0x0F, 0x0F };
        int column;

        public enum KeyCode { left, right, up, down, a, b, start, select }

        public KeyPad()
        {
            Reset();
        }

        public void Reset()
        {
            keys = new byte[] { 0x0F, 0x0F };
            column = 0;
        }

        public byte RB(ushort addr)
        {
            switch (column)
            {
                case 0x10: return (byte) (keys[0] | 0x10);
                case 0x20: return (byte) (keys[1] | 0x20);
                default: return 0;
            }
        }

        public void WB(ushort addr, byte val)
        {
            column = val & 0x30;
        }

        
        public void KeyDown(KeyCode key)
        {
            switch (key)
            {
                case KeyCode.right: keys[1] &= 0xE; break;
                case KeyCode.left: keys[1] &= 0xD; break;
                case KeyCode.up: keys[1] &= 0xB; break;
                case KeyCode.down: keys[1] &= 0x7; break;
                case KeyCode.a: keys[0] &= 0xE; break;
                case KeyCode.b: keys[0] &= 0xD; break;
                case KeyCode.select: keys[0] &= 0xB; break;
                case KeyCode.start: keys[0] &= 0x7; break;
            }

        }

        public void KeyUp(KeyCode key)
        {
            switch (key)
            {
                case KeyCode.right: keys[1] |= 0x1; break;
                case KeyCode.left: keys[1] |= 0x2; break;
                case KeyCode.up: keys[1] |= 0x4; break;
                case KeyCode.down: keys[1] |= 0x8; break;
                case KeyCode.a: keys[0] |= 0x1; break;
                case KeyCode.b: keys[0] |= 0x2; break;
                case KeyCode.select: keys[0] |= 0x5; break;
                case KeyCode.start: keys[0] |= 0x8; break;
            }
        }


    }
}
