using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBC
{
    public class Sprite
    {
        public byte X;
        public byte Y;
        public byte TileIndex;

        public bool UnderBackGround;
        public bool FlipX;
        public bool FlipY;
        public bool SecondPallete;

        public static Sprite[] CreateSpritesArray(byte[] oam)
        {
            Sprite[] sprites = new Sprite[40];
            for(int i = 0; i<160;i+=4)
            {
                sprites[i / 4] = new Sprite(i,oam);
            }
            return sprites;
        }
        public Sprite(int startIndex, byte[] oam)
        {
            Y = oam[startIndex];
            X = oam[startIndex + 1];
            TileIndex = oam[startIndex + 2];

            UnderBackGround = (oam[startIndex + 3] & 0x10)!=0;
            FlipX = (oam[startIndex + 3] & 0x20) != 0;
            FlipY = (oam[startIndex + 3] & 0x40) != 0;
            SecondPallete = (oam[startIndex + 3] & 0x80) != 0;

        }

    }
}
