using System;
using System.Collections.Generic;
using System.Numerics;

namespace QuixPhysics.Hextiles
{
    public class Hexgrid
    {
        public float hexagonSize = 100;
        public static int _GRID_SIZE = 4;

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

        //TODO: Add onylflat param
        public Hextile GetRandomHextile(bool onlyFlat)
        {
            Random rnd = new Random();
            Hextile tile = null;
            Hextile firstOne = null;
            while (tile == null)
            {
                var x = rnd.Next(0, Hexgrid._GRID_SIZE - 1);
                var y = rnd.Next(0, Hexgrid._GRID_SIZE - 1);
                var index = GetIndex(x, y);
                if (hextiles[index] != null)
                {
                    tile = hextiles[index];
                }
            }

            if (tile == null)
            {
                return firstOne;
            }
            return tile;
        }

        public Hextile GetRandomHextile()
        {
            return GetRandomHextile(true);
        }

        public Hextile GetHextile(int x, int y)
        {
            if (x < 0 || y < 0) return null;
            if (x >= _GRID_SIZE) return null;
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
            if (index < hextiles.Length)
            {
                Hextile tile = hextiles[index] = new Hextile(this, index);

                return tile;
            }
            return null;

        }

        public void CubeRound()
        {

        }
    }
}