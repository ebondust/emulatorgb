using GBC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GbcDebuger
{
    public partial class Form1 : Form
    {
        EmulatorCore emu = new EmulatorCore();
        byte[] programData;
        public Form1()
        {
            InitializeComponent();
            LoadProgram("tetris.gb");
            UpdateState();

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
            emu.LoadRom(programData);
        }

        public void UpdateState()
        {
            Registers.Items.Clear();
            Registers.Items.Add("A = " + emu.cpu.A);
            Registers.Items.Add("B = " + emu.cpu.B);
            Registers.Items.Add("C = " + emu.cpu.C);
            Registers.Items.Add("D = " + emu.cpu.D);
            Registers.Items.Add("E = " + emu.cpu.E);
            Registers.Items.Add("F = " + emu.cpu.F);
            Registers.Items.Add("H = " + emu.cpu.H);
            Registers.Items.Add("L = " + emu.cpu.L);
            Registers.Items.Add("AF = " + emu.cpu.AF);
            Registers.Items.Add("BC = " + emu.cpu.BC);
            Registers.Items.Add("DE = " + emu.cpu.DE);
            Registers.Items.Add("HL = " + emu.cpu.HL);
            Registers.Items.Add("M = " + emu.cpu.m);
            Registers.Items.Add("T = " + emu.cpu.t);
            Registers.Items.Add("IP = " + emu.cpu.IP);

            int pc = 0;
            while (pc < programData.Length)
            {
                byte opcode = programData[pc];
                pc++;
                switch (opcode)
                {
                    case 0x06:
                         ByteCode.Items.Add(string.Format("0x{0:X2} {1:X2}", opcode, programData[pc]));
                        pc++;
                        break;
                    default:
                        ByteCode.Items.Add(string.Format("0x{0:X2}", opcode));
                        break;
                     
                   
                }

              
            }
        }

    

    }
}
