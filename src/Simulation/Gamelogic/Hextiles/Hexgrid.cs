using System.Collections.Generic;
using System.Numerics;

namespace QuixPhysics.Hextiles
{
    public class Hexgrid
    {
        public float hexagonSize = 100;
        public static int _GRID_SIZE = 10;

        public Hextile[] hextiles = new Hextile[(int)(_GRID_SIZE * _GRID_SIZE)];

        public Hexgrid(float size)
        {
            hexagonSize = size;
        }

        public Vector2 GetXY(int index)
        {
            int y = (int)(index / _GRID_SIZE);
            int x = (int)(index % _GRID_SIZE);
            return new Vector2(x, y);
        }
        public int GetIndex(int x, int y)
        {
            int i = (int)(x + _GRID_SIZE * y);

            return i;
        }

        public Hextile GetHextile(int x, int y)
        {
            if(x <0 || y <0) return null;
            if(x>=_GRID_SIZE || y>=_GRID_SIZE)return null;
            int index = GetIndex(x, y);
            if (index < hextiles.Length)
            {
                Hextile tile = hextiles[index];
                if (tile != null)
                {
                    return tile;
                }
            }

            return null;

        }

        public Hextile AddHextile(int x, int y)
        {
            int index = GetIndex(x, y);
            Hextile tile = hextiles[index] = new Hextile(this, index);

            return tile;

        }
    }
}