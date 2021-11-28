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

        int position;

        public Hextile(Hexgrid hexagon, int position)
        {
            this._hexgrid = hexagon;
            this.position = position;
        }

        private bool IsOdd(int value)
        {
            return value % 2 != 0;
        }
        public Vector2 getXY(){
            return _hexgrid.GetXY(position);
        }

        /// <summary>
        /// Gets worlds position
        /// </summary>
        /// <returns></returns>
        public Vector3 GetPosition()
        {
            Vector2 pos = _hexgrid.GetXY(position);
            float size = _hexgrid.hexagonSize/2;
            float width = MathF.Sqrt(3) * size;
            float odd = IsOdd(position)?size:0;
            float h = size*2;

            return new Vector3((pos.X*(width)), 0, (pos.Y*h)+odd);
        }
        public float GetWidth(){
            return  Hexagon._SIZE/2;
        }
    }
}