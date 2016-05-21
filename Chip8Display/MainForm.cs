using Chip8;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chip8Display
{
    public partial class MainForm : Form
    {
        EmulatorCore emulator = new EmulatorCore();
        public MainForm()
        {
            InitializeComponent();
            MainLoop.Start();
        }

        private void MainLoop_Tick(object sender, EventArgs e)
        {
            if (!emulator.Initialized)
                return;
            for (int i = 0; i < 8; i++)
                emulator.emulateCycle();

            drawGfx(Canvas);
        }
        public void clearGfx(PictureBox screen)
        {
            screen.Image = new Bitmap(64, 32);
            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 32; y++)
                {
                    ((Bitmap)screen.Image).SetPixel(x, y, Color.Black);
                }
            }
            screen.SizeMode = PictureBoxSizeMode.StretchImage;
            screen.Invalidate();
        }

        public void drawGfx(PictureBox screen)
        {
            screen.Image = new Bitmap(64, 32);
            int x, y;

            for (x = 0; x < 64; x++)
            {
                for (y = 0; y < 32; y++)
                {
                    if (emulator.Gfx[(y * 64) + x] == 0)
                        ((Bitmap)screen.Image).SetPixel(x, y, Color.Black);
                    else
                        ((Bitmap)screen.Image).SetPixel(x, y, Color.White);
                }

                screen.SizeMode = PictureBoxSizeMode.StretchImage;
                screen.Invalidate();

            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            int key = translateKeyCode(e.KeyCode);
            if (key < 0)
                return;
            emulator.Input(key);
        }

        private int translateKeyCode(Keys code)
        {
            int key = -1;
            switch (code)
            {
                case Keys.Q:
                    key = 0;
                    break;
                case Keys.W:
                    key = 1;
                    break;
                case Keys.E:
                    key = 2;
                    break;
                case Keys.R:
                    key = 3;
                    break;
                case Keys.T:
                    key = 4;
                    break;
                case Keys.Y:
                    key = 5;
                    break;
                case Keys.A:
                    key = 6;
                    break;
                case Keys.S:
                    key = 7;
                    break;
                case Keys.D:
                    key = 8;
                    break;
                case Keys.F:
                    key = 9;
                    break;
                case Keys.G:
                    key = 10;
                    break;
                case Keys.H:
                    key = 11;
                    break;
                case Keys.Z:
                    key = 12;
                    break;
                case Keys.X:
                    key = 13;
                    break;
                case Keys.C:
                    key = 14;
                    break;
                case Keys.V:
                    key = 15;
                    break;
            }
            return key;
        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            int key = translateKeyCode(e.KeyCode);
            if (key < 0)
                return;
            emulator.EndInput(key);
        }

        private void openRomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            clearGfx(Canvas);  
            emulator.LoadProgram(openFileDialog1.FileName);
           
        }
    }
}
