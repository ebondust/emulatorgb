using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBC
{
    public static class Extensions
    {
        public static byte[] SliceRow(this byte[,,] array, int x, int y)
        {
            byte[] result = new byte[array.GetLength(2)];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = array[x, y, i];
            }
            return result;
        }
    }
}
