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

            // Decode opcode
            switch (opcode & 0xF000)
            {
                // Some opcodes //

                case 0x0000:
                    switch (opcode & 0x000F)
                    {
                        case 0x0000: // 0x00E0: Clears the screen    
                            for (int i = 0; i < 2048; ++i)
                                gfx[i] = 0x0;
                            pc += 2;
                            break;

                        case 0x000E: // 0x00EE: Returns from subroutine          
                            pc = pop();	// Put the stored return address from the stack back into the program counter					
                            pc += 2;

                            break;

                        default:
                            pc += 2;
                            errorList.Add("Unknown opcode [0x0000]:" + (opcode).ToString("X") + " \n");
                            break;
                    }
                    break;

                case 0x1000:
                    {
                        pc = (ushort)(opcode & 0x0FFF);
                        break;
                    }
                case 0x2000:
                    {

                        push(pc);
                        pc = (ushort)(opcode & 0x0FFF);
                        break;
                    }
                case 0x3000:
                    {
                        ushort lOp = (ushort)(opcode & 0x00FF);
                        int v = (ushort)(opcode & 0x0F00) >> 8;
                        if (register[v] == lOp)
                            pc += 2;
                        pc += 2;
                        break;

                    }
                case 0x4000:
                    {
                        ushort lOp = (ushort)(opcode & 0x00FF);
                        int v = (ushort)(opcode & 0x0F00) >> 8;
                        if (register[v] != lOp)
                            pc += 2;
                        pc += 2;
                        break;
                    }
                case 0x5000:
                    {
                        int v = (ushort)(opcode & 0x0F00) >> 8;
                        int v2 = (ushort)(opcode & 0x00F0) >> 4;
                        if (register[v] == register[v2])
                            pc += 2;
                        pc += 2;
                        break;
                    }

                case 0x6000:
                    {

                        ushort lOp = (ushort)((opcode & 0x00FF));
                        int v = (ushort)(opcode & 0x0F00) >> 8;
                        register[v] = (byte)lOp;
                        pc += 2;
                        break;
                    }
                case 0x7000:
                    {

                        ushort lOp = (ushort)(opcode & 0x00FF);
                        int v = (ushort)(opcode & 0x0F00) >> 8;
                        register[v] += (byte)lOp;
                        pc += 2;
                        break;
                    }
                case 0x8000:
                    {
                        int v;
                        int v2;
                        switch (opcode & 0x000F)
                        {
                            case 0x0000:
                                v = (ushort)(opcode & 0x0F00) >> 8;
                                v2 = (ushort)(opcode & 0x00F0) >> 4;
                                register[v] = register[v2];
                                pc += 2;
                                break;
                            case 0x0001:
                                v = (ushort)(opcode & 0x0F00) >> 8;
                                v2 = (ushort)(opcode & 0x00F0) >> 4;
                                register[v] = (byte)(register[v] | register[v2]);
                                pc += 2;
                                break;
                            case 0x0002:
                                v = (ushort)(opcode & 0x0F00) >> 8;
                                v2 = (ushort)(opcode & 0x00F0) >> 4;
                                register[v] = (byte)(register[v] & register[v2]);
                                pc += 2;
                                break;
                            case 0x0003:
                                v = (ushort)(opcode & 0x0F00) >> 8;
                                v2 = (ushort)(opcode & 0x00F0) >> 4;
                                register[v] ^= (register[v2]);
                                pc += 2;
                                break;
                            case 0x0004:
                                if (register[(opcode & 0x0F00) >> 8] + register[(opcode & 0x00F0) >> 4] > 255)
                                    register[15] = 1;
                                else
                                    register[15] = 0;
                                register[(opcode & 0x0F00) >> 8] += register[(opcode & 0x00F0) >> 4];
                                pc += 2;
                                break;
                            case 0x0005:
                                if (register[(opcode & 0x0F00) >> 8] > register[(opcode & 0x00F0) >> 4])
                                    register[15] = 1;
                                else
                                    register[15] = 0;
                                register[(opcode & 0x0F00) >> 8] -= register[(opcode & 0x00F0) >> 4];
                                pc += 2;
                                break;
                            case 0x0006:
                                v = (ushort)(opcode & 0x0F00) >> 8;
                                v2 = (ushort)(opcode & 0x00F0) >> 4;
                                register[v] = (byte)(register[v] >> 1);
                                register[15] = (byte)v2;
                                pc += 2;
                                break;
                            case 0x0007:
                                if (register[(opcode & 0x0F00) >> 8] > register[(opcode & 0x00F0) >> 4])
                                    register[15] = 0;
                                else
                                    register[15] = 1;
                                register[(opcode & 0x00F0) >> 4] -= register[(opcode & 0x0F00) >> 8];
                                pc += 2;
                                break;
                            case 0x0008:
                                v = (ushort)(opcode & 0x0F00) >> 8;
                                v2 = (ushort)(opcode & 0x00F0) >> 4;
                                register[v] = (byte)(register[v] << 1);
                                register[15] = (byte)v2;
                                pc += 2;
                                break;
                            default:
                                pc += 2;
                                errorList.Add("Unknown opcode [0x8000]: " + (opcode).ToString("X") + " \n");
                                break;
                        }
                        break;

                    }
                case 0x9000:
                    {
                        int v = (ushort)(opcode & 0x0F00) >> 8;
                        int v2 = (ushort)(opcode & 0x00F0) >> 4;
                        if (register[v] != register[v2])
                            pc += 2;
                        pc += 2;
                        break;
                    }
                case 0xA000: // ANNN: Sets I to the address NNN
                   
                    I = (ushort)(opcode & 0x0FFF);
                    errorList.Add("" + (opcode).ToString("X"));
                    pc += 2;
                    break;

                case 0xB000:

                    pc = (ushort)((opcode & 0x0FFF) + register[0]);
                    break;

                case 0xC000:
                    {
                        int v = (ushort)(opcode & 0x0F00) >> 8;
                        register[v] = (byte)(new Random().Next(255) & (opcode & 0x00FF));
                        pc += 2;
                        break;
                    }
                case 0xD000:
                    {

                        break;
                    }
                case 0xE000:
                    switch (opcode & 0x00FF)
                    {
                        // if the key stored in VX is pressed
                        case 0x009E:
                            if (keypad[register[(opcode & 0x0F00) >> 8]] != 0)
                                pc += 4;
                            else
                                pc += 2;
                            break;

                        case 0x00A1: // EXA1: Skips the next instruction if the key stored in VX isn't pressed
                            if (keypad[register[(opcode & 0x0F00) >> 8]] == 0)
                                pc += 4;
                            else
                                pc += 2;
                            break;
                        default:
                            errorList.Add("Unknown opcode: " + (opcode).ToString("X") + " \n");
                            break;
                    }
                    break;

                case 0xF000:
                    switch (opcode & 0x00FF)
                    {
                        case 0x0007: // FX07: Sets VX to the value of the delay timer

                            break;

                        case 0x000A: // FX0A: A key press is awaited, and then stored in VX		

                            break;

                        case 0x0015: // FX15: Sets the delay timer to VX

                            break;

                        case 0x0018: // FX18: Sets the sound timer to VX

                            break;

                        case 0x001E: // FX1E: Adds VX to I


                            break;

                        case 0x0029: // FX29: Sets I to the location of the sprite for the character in VX.

                            break;

                        case 0x0033: // FX33: Stores the Binary-coded decimal representation of VX at the addresses I, I plus 1, and I plus 2

                            break;

                        case 0x0055: // FX55: Stores V0 to VX in memory starting at address I					


                            break;

                        case 0x0065: // FX65: Fills V0 to VX with values from memory starting at address I					

                            break;

                        default:
                            errorList.Add("Unknown opcode: " + (opcode).ToString("X") + " \n");
                            break;
                    }
                    break;


                default:
                    errorList.Add("Unknown opcode: " + (opcode).ToString("X") + " \n");
                    pc += 2;
                    break;
            }

            if (delay_timer > 0)
                --delay_timer;

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