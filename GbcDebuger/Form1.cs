using GBC;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static GBC.KeyPad;

namespace GbcDebuger
{
    public partial class Form1 : Form
    {

        public ObservableCollection<string> breakPoints = new ObservableCollection<string>();

        EmulatorCore emu = new EmulatorCore();
        EmulatorCore emuDebug = new EmulatorCore();
        byte[] programData;
       
        public Form1()
        {
            InitializeComponent();
            //Name = "Emulator GB";
             LoadProgram("tetris.gb");
            //LoadProgram("opus5.gb");
           // LoadProgram("mario.gb");
             //LoadProgram("cpu_instrs.gb");
            UpdateState();
          
            Dipsach.Tick += Dipsach_Tick;
            Dipsach.Interval = 100;
            Dipsach.Start();
            //Break.DataSource = breakPoints;

        }

        private void Dipsach_Tick(object sender, EventArgs e)
        {
            //Canvas.Image = drawGfxFull();
            Canvas.Image = drawGfx();
            Canvas.SizeMode = PictureBoxSizeMode.StretchImage;
            registersUpdate();
            Canvas.Invalidate();
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
            //emuDebug.LoadRom(programData);
            Task.Run(() =>
            {
                emu.EmulationLoop();
            });
            FillData();
           // breakPoints.Add("0x0100");
            breakPoints.Add("0x20DC");
        }

        public void registersUpdate()
        {
            Registers.Items.Clear();

            Registers.Items.Add(string.Format("AF = 0x{0:X4}", emu.cpu.AF));
            Registers.Items.Add(string.Format("BC = 0x{0:X4}", emu.cpu.BC));
            Registers.Items.Add(string.Format("DE = 0x{0:X4}", emu.cpu.DE));
            Registers.Items.Add(string.Format("HL = 0x{0:X4}", emu.cpu.HL));
            Registers.Items.Add(string.Format("SP = 0x{0:X4}", emu.cpu.SP));
            Registers.Items.Add(string.Format("IP = 0x{0:X4}", emu.cpu.IP));

            Registers.Items.Add(string.Format("A = 0x{0:X2}", emu.cpu.A));
            Registers.Items.Add(string.Format("B = 0x{0:X2}", emu.cpu.B));
            Registers.Items.Add(string.Format("C = 0x{0:X2}", emu.cpu.C));
            Registers.Items.Add(string.Format("D = 0x{0:X2}", emu.cpu.D));
            Registers.Items.Add(string.Format("E = 0x{0:X2}", emu.cpu.E));
            Registers.Items.Add(string.Format("F = 0x{0:X2}", emu.cpu.F));
            Registers.Items.Add(string.Format("H = 0x{0:X2}", emu.cpu.H));
            Registers.Items.Add(string.Format("L = 0x{0:X2}", emu.cpu.L));
        
            Registers.Items.Add("M = " + emu.cpu.m);
            Registers.Items.Add("T = " + emu.cpu.t);
            Registers.Items.Add("Scanline = " + emu.gpu.gpu.scanline);
            Registers.Items.Add("SCY = " + emu.gpu.gpu.scrollY);
            Registers.Items.Add("SCx = " + emu.gpu.gpu.scrollX);
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
            //while (pc < programData.Length)
            //{
            //    byte opcode = programData[pc];
            //    pc++;
            //    switch (opcode)
            //    {
            //        case 0x06:
            //             ByteCode.Items.Add(string.Format("0x{0:X2} {1:X2}", opcode, programData[pc]));
            //            pc++;
            //            break;
            //        default:
            //            ByteCode.Items.Add(string.Format("0x{0:X2}", opcode));
            //            break;
                     
                   
            //    }             
            //}
            //for(int i = 0;i<255;i++)
            //{
            //    testBox.Text += (string.Format("0x{0:X2}", i)+"\n");
            //}
        }

        public Bitmap drawGfx()
        {
            Bitmap sc = new Bitmap(160, 144);
            int x, y;

            for (x = 0; x < 160; x++)
            {
                for (y = 0; y < 144; y++)
                {
                  var pixel = emu.gpu.Screen[x, y];
                  sc.SetPixel(x, y, Color.FromArgb(pixel.R, pixel.G, pixel.B));
                }
            }
            return sc;
        }
        public Bitmap drawGfxFull()
        {
            Bitmap sc = new Bitmap(256, 256);
            int x, y;

            for (x = 0; x < 256; x++)
            {
                for (y = 0; y < 256; y++)
                {
                    var pixel = emu.gpu.ScreenFull[x, y];
                    sc.SetPixel(x, y, Color.FromArgb(pixel.R, pixel.G, pixel.B));
                }
            }
            return sc;
        }

        private void Step_Click(object sender, EventArgs e)
        {
            emu.EmulateCycle();
            emuDebug.EmulateCycle();
            //InstIP.Items.RemoveAt(0);
            //Instruction.Items.RemoveAt(0);
            //InstIP.Items.Add(string.Format("0x{0:X4}", emu.cpu.IP));
            //Instruction.Items.Add((string.Format("0x{0:X2} {1:X2}", programData[emu.cpu.IP], programData[emuDebug.cpu.IP + 1])));
            emu.gpu.RenderScreen();

        }

        private void Run_Click(object sender, EventArgs e)
        {
            while (!breakPoints.Any(x => x == (string.Format("0x{0:X4}",emu.cpu.IP))))
            {
                emu.EmulateCycle();
               // emuDebug.EmulateCycle();
            }
            FillData();
        }

        private void Toggle_Click(object sender, EventArgs e)
        {
            //breakPoints.Add(InstIP.SelectedItem as string);
            //Break.DataSource = new ObservableCollection<string> (breakPoints);
            
            //Break.Refresh();
            //Break.Invalidate();
        }


        private void FillData()
        {
           // InstIP.Items.Clear();
           // Instruction.Items.Clear();
           //// for (int i = 0; i<10; i ++)
           // {
           //     InstIP.Items.Add(string.Format("0x{0:X4}", emu.cpu.IP));
           //     Instruction.Items.Add((string.Format("0x{0:X2} {1:X2}", programData[emu.cpu.IP], programData[emu.cpu.IP+1])));
               // emuDebug.EmulateCycle();
            //}
        }

        private void DeleteBreak_Click(object sender, EventArgs e)
        {
            //breakPoints.RemoveAt(Break.SelectedIndex);
        }

        private void StartButton_MouseDown(object sender, MouseEventArgs e)
        {
            emu.mmu.Keys.KeyDown(KeyCode.start);
        }

        private void StartButton_MouseUp(object sender, MouseEventArgs e)
        {
            emu.mmu.Keys.KeyUp(KeyCode.start);
        }

        private void AButton_MouseDown(object sender, MouseEventArgs e)
        {
            emu.mmu.Keys.KeyDown(KeyCode.a);
        }

        private void AButton_MouseUp(object sender, MouseEventArgs e)
        {
            emu.mmu.Keys.KeyUp(KeyCode.a);
        }

        private void BButton_MouseDown(object sender, MouseEventArgs e)
        {
            emu.mmu.Keys.KeyDown(KeyCode.b);
        }

        private void BButton_MouseUp(object sender, MouseEventArgs e)
        {
            emu.mmu.Keys.KeyUp(KeyCode.b);
        }

        private void LeftButton_MouseDown(object sender, MouseEventArgs e)
        {
            emu.mmu.Keys.KeyDown(KeyCode.left);
        }

        private void LeftButton_MouseUp(object sender, MouseEventArgs e)
        {
            emu.mmu.Keys.KeyUp(KeyCode.left);
        }

        private void RightButton_MouseDown(object sender, MouseEventArgs e)
        {
            emu.mmu.Keys.KeyDown(KeyCode.right);
        }

        private void RightButton_MouseUp(object sender, MouseEventArgs e)
        {
            emu.mmu.Keys.KeyUp(KeyCode.right);
        }
    }
}
