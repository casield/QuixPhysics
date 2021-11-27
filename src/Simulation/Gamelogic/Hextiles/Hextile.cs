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

        /// <summary>
        /// Get HexSide of some position
        /// /// </summary>
        /// <returns></returns>
        public Vector3 GetCornerPoint(float cornerPoint, Hexagon hexagon)
        {
            var _size = size / 2;
            var angle_deg = 60 * cornerPoint;
            var angle_rad = (MathF.PI / 180) * angle_deg;
            var hexagon_pos = hexagon.GetPosition();

            return new Vector3(hexagon_pos.X + _size * MathF.Cos(angle_rad),
            hexagon_pos.Y,
            hexagon_pos.Z + _size * MathF.Sin(angle_rad));
        }

        public Vector3 GetSidePoint(float cornerPoint, Hexagon hexagon)
        {
            var _size = size;
            var angle_deg = 60 * cornerPoint - 30;
            var angle_rad = (MathF.PI / 180) * angle_deg;
            var hexagon_pos = hexagon.GetPosition();

            return new Vector3(hexagon_pos.X + _size * MathF.Cos(angle_rad),
            hexagon_pos.Y,
            hexagon_pos.Z + _size * MathF.Sin(angle_rad));
        }

        public Vector3 GetPointDirection(Vector3 point, Hexagon hexagon)
        {
            return Vector3.Normalize(point - hexagon.GetPosition());
        }
        /// <summary>
        /// Checks if some point is inside this Hextile
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool IsInside(Vector3 position)
        {
            //-sqrt(3)x - y + sqrt(3)/2
            float point = MathF.Sqrt(3) * position.X - position.Y + MathF.Sqrt(3) / 2;
            QuixConsole.Log("Point", point);
            return false;
        }
    }
}