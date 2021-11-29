using System;
using System.Numerics;

namespace QuixPhysics.Hextiles
{
    public struct HexSide
    {

    }
    public class Hextile
    {
        float size = Hexagon._SIZE;
        Hexgrid _hexgrid;

        int index;

        public static Vector3 _PADDDING = new Vector3(0, Hexagon._SIZE, 0);

        public float randomHeight = 0;

        public Hextile(Hexgrid hexagon, int position)
        {
            this._hexgrid = hexagon;
            this.index = position;

            Random random = new Random();
            if (random.Next(0, 100) > 90)
            {
                randomHeight = random.Next(0, 400);
            }

        }

        public bool IsOdd(int value)
        {
            return value % 2 != 0;
        }
        public Vector2 getXY()
        {
            return _hexgrid.GetXY(index);
        }
        public int GetIndex()
        {
            return index;
        }

        public Vector3 XYToCube(Vector2 XY)
        {
            var q = XY.X;
            var r = XY.Y;
            var s = -q - r;

            return new Vector3(XY.Y, XY.Y, s);
        }

        /// <summary>
        /// Gets worlds position
        /// 
        /// It returns the top of the hexagon.
        /// </summary>
        /// <returns></returns>
        public Vector3 GetPosition()
        {
            Vector2 pos = _hexgrid.GetXY(index);
            float size = _hexgrid.hexagonSize / 2;
            float width = MathF.Sqrt(3) * size;
            float odd = IsOdd(index) ? size : 0;
            float h = -(size * 2);

            return new Vector3((pos.X * (width)), 0, (pos.Y * h) + odd) + (GetFullPadding());
        }
        /// <summary>
        /// Gets this hextile full height, included randomHeight
        /// </summary>
        /// <returns></returns>
        public Vector3 GetFullPadding()
        {
            return _PADDDING + new Vector3(0, randomHeight, 0);
        }
        public Vector2 GetSide(int position)
        {
            var pos = getXY();
            var isOdd = IsOdd(pos.Y);

            var evenSides = new Vector2[] {
           new Vector2(1, 0),
            new Vector2(1, -1),
            new Vector2(0, -1),
            new Vector2(-1, -1),
            new Vector2(-1, 0),
            new Vector2(0, 1), };

            var oddSides = new Vector2[] {
           new Vector2(1, 1),
            new Vector2(1, 0),
            new Vector2(0, -1),
            new Vector2(-1, 0),
            new Vector2(-1, 1),
            new Vector2(0, 1), };

            var parity = (int)pos.X & 1;
            Vector2 side = parity == 0 ? oddSides[position] : evenSides[position];


            return side;
        }

        private bool IsOdd(float x)
        {
            return IsOdd((int)x);
        }

        public float GetWidth()
        {
            return Hexagon._SIZE / 2;
        }
    }
}