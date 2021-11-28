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

        public Hextile(Hexgrid hexagon, int position)
        {
            this._hexgrid = hexagon;
            this.index = position;
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

        /// <summary>
        /// Gets worlds position
        /// </summary>
        /// <returns></returns>
        public Vector3 GetPosition()
        {
            Vector2 pos = _hexgrid.GetXY(index);
            float size = _hexgrid.hexagonSize / 2;
            float width = MathF.Sqrt(3) * size;
            float odd = IsOdd(index) ? size : 0;
            float h = (size * 2);

            return new Vector3((pos.X * (-width)), 0, (pos.Y * h) + odd);
        }
        public Vector2 GetSide(int position)
        {
            var pos = getXY();
            var isOdd = IsOdd(index);

            var oddSides = new Vector2[]{new Vector2(-1, 0),new Vector2(-1, -1), new Vector2(0, 1),new Vector2(1, 0),new Vector2(1, -1),new Vector2(0,-1)};
            var evenSides = new Vector2[]{new Vector2(-1, -1),new Vector2(-1, 0), new Vector2(0, 1),new Vector2(1, 0),new Vector2(1, -1),new Vector2(0,-1)};
            Vector2 side =isOdd?oddSides[position]: evenSides[position];
           

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