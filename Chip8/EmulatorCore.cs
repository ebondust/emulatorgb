using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8
{
    class EmulatorCore:CPU
    {
        public List<string> errorList = new List<string>();
        byte[] programData;



        public EmulatorCore()
        {

        }

        public void LoadProgram(string path)
        {
            FileStream fStream = File.OpenRead(path);
            programData = new byte[fStream.Length];

            fStream.Read(programData, 0, programData.Length);
            fStream.Close();
            //Begins the process of writing the byte array back to a file

            using (Stream file = File.OpenWrite(path))
            {
                file.Write(programData, 0, programData.Length);
            }

            for (int i = 0; i < programData.Length; ++i)
                memory[i + 512] = programData[i];
        }

        public bool emulateCycle()
        {
            if (programData.Length + 512 < pc + 1)
                return false;
            opcode = (ushort)(memory[pc] << 8 | memory[pc + 1]);

            return true;
        }

        public void Input(int key)
        {
            keypad[key] = 1;
        }
        public void endInput(int key)
        {
            keypad[key] = 0;
        }        

        public void drawGfx()
        {

        }
    }
}