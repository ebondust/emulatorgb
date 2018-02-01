using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBC
{
    public class GPU
    {
        public struct RGB
        {
            public byte R;
            public byte G;
            public byte B;
        }
        public struct Gpu
        {
            public byte control;
            public byte scrollX;
            public byte scrollY;
            public byte scanline;
            public ushort tick;
        }
        public RGB[] palette = new RGB[4]
        {
            new RGB { R= 255,G= 255,B = 255 },
            new RGB { R= 192,G= 192,B = 192 },
            new RGB { R= 96,G= 96,B = 96 },
            new RGB { R= 0,G= 0,B = 0 }
        };
        public Gpu gpu = new Gpu();

        byte[,,] tiles = new byte[384, 8, 8];
        public RGB[] backgroundPalette = new RGB[4];
        public RGB[,] spritePalette = new RGB[2, 4];

        enum GpuMode
        {
            GPU_MODE_HBLANK = 0,
            GPU_MODE_VBLANK = 1,
            GPU_MODE_OAM = 2,
            GPU_MODE_VRAM = 3,
        }
        GpuMode gpuMode = GpuMode.GPU_MODE_HBLANK;
        int lastTicks = 0;

        MMU mmu;
        private RGB[] scrn = new RGB[160 * 144];
        public RGB[,] Screen = new RGB[160, 144];
        public RGB[,] ScreenFull = new RGB[256, 256];

        private bool lcdOn { get { return ((gpu.control & 0x80) != 0); } }
        private bool bgOn { get { return ((gpu.control & 0x01) != 0); } }
        private bool spritesOn { get { return ((gpu.control & 0x02) != 0); } }

        byte GPU_CONTROL_BGENABLE = (1 << 0);
        byte GPU_CONTROL_SPRITEENABLE = (1 << 1);
        byte GPU_CONTROL_SPRITEVDOUBLE = (1 << 2);
        byte GPU_CONTROL_TILEMAP = (1 << 3);
        byte GPU_CONTROL_TILESET = (1 << 4);
        byte GPU_CONTROL_WINDOWENABLE = (1 << 5);
        byte GPU_CONTROL_WINDOWTILEMAP = (1 << 6);
        byte GPU_CONTROL_DISPLAYENABLE = (1 << 7);
        byte INTERRUPTS_VBLANK = (1 << 0);
        byte INTERRUPTS_LCDSTAT = (1 << 1);
        byte INTERRUPTS_TIMER = (1 << 2);
        byte INTERRUPTS_SERIAL = (1 << 3);
        byte INTERRUPTS_JOYPAD = (1 << 4);

        public void mapFrameData()
        {

            //for (int i = 0; i < 160 * 2; i++)
            //{
            //    scrn[i] = new RGB() { R = 122, G = 10, B = 10 };

            //}
            //if (lcdOn)
            //    Thread.Sleep(50);
            for (int i = 0; i < 144; i++)
            {
                for (int j = 0; j < 160; j++)
                {
                    Screen[j, i] = scrn[(i * 144) + j];
                }
            }
            //for (int i = 0; i < 160; i++)
            //{
            //    for (int j = 0; j < 144; j++)
            //    {
            //        Screen[i, j] = scrn[(i * 144) + j];
            //    }
            //}
        }
        public GPU(MMU mmu)
        {
            this.mmu = mmu;
            for (int i = 0; i < scrn.Length; i++)
                scrn[i] = new RGB() { R = 255, G = 155, B = 155 };
            mapFrameData();
        }

        public void gpuStep(ushort m)
        {
            if (!lcdOn)
                return;

            gpu.tick += m;

            switch (gpuMode)
            {
                case GpuMode.GPU_MODE_HBLANK:
                    if (gpu.tick >= 204)
                    {

                        hblank();

                        if (gpu.scanline == 143)
                        {
                            if ((mmu.InterruptEnable & INTERRUPTS_VBLANK) != 0)
                                mmu.InterruptFlags |= INTERRUPTS_VBLANK;

                            gpuMode = GpuMode.GPU_MODE_VBLANK;
                        }

                        else gpuMode = GpuMode.GPU_MODE_OAM;

                        gpu.tick -= 204;
                    }

                    break;

                case GpuMode.GPU_MODE_VBLANK:
                    if (gpu.tick >= 456)
                    {
                        gpu.scanline++;

                        if (gpu.scanline > 153)
                        {
                            gpu.scanline = 0;
                            gpuMode = GpuMode.GPU_MODE_OAM;
                            RenderScreen();
                            //RenderTiles();
                             //Task.Delay(10).Wait();
                        }

                        gpu.tick -= 456;
                    }

                    break;

                case GpuMode.GPU_MODE_OAM:
                    if (gpu.tick >= 80)
                    {
                        gpuMode = GpuMode.GPU_MODE_VRAM;

                        gpu.tick -= 80;
                    }

                    break;

                case GpuMode.GPU_MODE_VRAM:
                    if (gpu.tick >= 172)
                    {
                        gpuMode = GpuMode.GPU_MODE_HBLANK;


                        //renderScanline();

                        gpu.tick -= 172;
                    }

                    break;
            }
        }

        void hblank()
        {
            gpu.scanline++;
        }

        public void updatetile(ushort addr, byte val)
        {
            addr &= 0x1ffe;
            ushort tile = (ushort)((addr >> 4) & 511);
            ushort y = (ushort)((addr >> 1) & 7);

            byte x, bitIndex = 0;
            for (x = 0; x < 8; x++)
            {
                bitIndex = (byte)(1 << (7 - x));
                tiles[tile, y, x] = (byte)((((mmu.vram[addr] & bitIndex) != 0) ? (byte)1 : (byte)0) + (((mmu.vram[addr + 1] & bitIndex) != 0) ? (byte)2 : (byte)0));
            }

        }

        public void RenderScreen()
        {
            int mapOffset = ((gpu.control & GPU_CONTROL_TILEMAP) != 0) ? 0x1c00 : 0x1800;
            int tileOffset = ((gpu.control & GPU_CONTROL_TILESET) != 0) ? 0x0000 : 0x0800;
            //mapOffset = 0x1c00;
            RGB[] fScrn = new RGB[256 * 256];
            if (bgOn)
            {
                for (int i = 0; i < 32 * 32; i++)
                {
                    int yOff = (i / 32) * 8;
                    int even = (i / 32) * 32;


                    int index = i - even;


                    ushort tile = (ushort)mmu.vram[mapOffset + i];
                    if (tileOffset != 0 && tile < 128)
                        tile += 256;



                    for (int x = 0; x < 8; x++)
                    {
                        for (int y = 0; y < 8; y++)
                        {
                            byte colour = tiles[tile, y, x];
                            int fIndex = (yOff) * 256 + y * 256 + ((i - even) * 8) + x;
                            fScrn[fIndex].R = backgroundPalette[colour].R;
                            fScrn[fIndex].G = backgroundPalette[colour].G;
                            fScrn[fIndex].B = backgroundPalette[colour].B;
                        }
                    }
                }
            }
            if (spritesOn)
            {
                var sprites = Sprite.CreateSpritesArray(mmu.oam);

                for (int i = 0; i < sprites.Length; i++)
                {
                    ushort tile = sprites[i].TileIndex;
                    if (sprites[i].SecondPallete && tile < 128)
                        tile += 256;
                    int palleteindex = (sprites[i].SecondPallete) ? 1 : 0;

                    int yOff = sprites[i].Y - 16;
                    int xOff = sprites[i].X - 8;
                    if (xOff < 0 || yOff < 0)
                        continue;

                    for (int x = 0; x < 8; x++)
                    {
                        for (int y = 0; y < 8; y++)
                        {
                            byte colour = tiles[tile, y, x];
                            int fIndex = (yOff) * 256 + y * 256 + xOff + x;
                            fScrn[fIndex].R = spritePalette[palleteindex, colour].R;
                            fScrn[fIndex].G = spritePalette[palleteindex, colour].G;
                            fScrn[fIndex].B = spritePalette[palleteindex, colour].B;
                        }
                    }

                }

            }

            for (int i = 0; i < 256; i++)
            {
                for (int k = 0; k < 256; k++)
                {
                    ScreenFull[k, i] = fScrn[i * 256 + k];
                }
            }

            for (int i = 0; i < 160; i++)
            {
                for (int k = 0; k < 144; k++)
                {
                    int yPix = k + gpu.scrollY;
                    if (yPix >= 0x0100)
                        yPix = 255;
                    Screen[i, k] = ScreenFull[i + gpu.scrollX, yPix];
                }
            }

        }

        public void RenderTiles()
        {
            int xOff = 0;
            int yOff = 0;
            for (int i = 0; i < tiles.GetLength(0); i++)
            {
                for (int x = 0; x < 8; x++)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        byte colour = tiles[i, y, x];
                        Screen[x + xOff, y + yOff].R = backgroundPalette[colour].R;
                        Screen[x + xOff, y + yOff].G = backgroundPalette[colour].G;
                        Screen[x + xOff, y + yOff].B = backgroundPalette[colour].B;
                    }
                }
                xOff += 8;
                if (xOff >= 160)
                {
                    xOff = 0;
                    yOff += 8;
                }
                if (yOff == 0x090)
                    return;
            }
        }
    }
}

