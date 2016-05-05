using Chip8;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
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
            emulator.LoadProgram("ufo");
            MainLoop.Start();
        }

        private void MainLoop_Tick(object sender, EventArgs e)
        {
            emulator.emulateCycle();
            drawGfx(Canvas);
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
                // System.Threading.Thread.Sleep(10);
                screen.Invalidate();

            }
        }
    }
}
