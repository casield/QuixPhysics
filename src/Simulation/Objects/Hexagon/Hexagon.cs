
using System;
using System.Collections.Generic;
using System.Numerics;
using QuixPhysics.Hextiles;

namespace QuixPhysics
{
    public class Hexagon : MeshBox
    {

        public static float _SIZE = 1000;
        public Hextile hextile;
        public static BoxState Build(Vector3 position, bool isBlue)
        {
            string color = isBlue ? "Blue" : "";
            BoxState state = new BoxState()
            {
                halfSize = new Vector3(_SIZE),
                mesh = "Stadiums/Hexagons/Hextile" + color,
                instantiate = true,
                position = position,
                type = "Hexagon"
            };

            return state;
        }

        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            base.Load(bodyHandle, connectionState, simulator, state, guid, room);

        }

        /// <summary>
        /// Get HexSide of some position
        /// /// </summary>
        /// <returns></returns>
        public Vector3 GetCornerPoint(float cornerPoint)
        {
            var _size = _SIZE / 2;
            var angle_deg = 60 * cornerPoint;
            var angle_rad = (MathF.PI / 180) * angle_deg;
            var hexagon_pos = this.GetPosition();

            return new Vector3(hexagon_pos.X + _size * MathF.Cos(angle_rad),
            hexagon_pos.Y,
            hexagon_pos.Z + _size * MathF.Sin(angle_rad));
        }

        public Vector3 GetSidePoint(float cornerPoint)
        {
            var _size = _SIZE / 2;
            var angle_deg = 60 * cornerPoint - 30;
            var angle_rad = (MathF.PI / 180) * angle_deg;
            var hexagon_pos = GetPosition();

            return new Vector3(hexagon_pos.X + _size * MathF.Cos(angle_rad),
            hexagon_pos.Y,
            hexagon_pos.Z + _size * MathF.Sin(angle_rad));
        }

        public Vector3 GetPointDirection(Vector3 point)
        {
            return Vector3.Normalize(point - GetPosition());
        }

        public PhyObject[] AddWalls(int[] sides)
        {

            PhyObject[] wall = new PhyObject[sides.Length];
            int added = 0;
            foreach (var item in sides)
            {
                var pos = GetSidePoint(item) + new Vector3(0, Hexagon._SIZE / 2, 0);
                var dir = GetPointDirection(pos);
                var rot = MathF.Atan2(dir.X, dir.Z);
                var quat = Quaternion.CreateFromYawPitchRoll(rot, 0, 0);
                var boxState = new BoxState()
                {
                    halfSize = new Vector3(hextile.GetWidth() * 1.15f, Hexagon._SIZE / 2, 20),
                    instantiate = true,
                    position = pos,
                    type = "Hexwall" + item+("-x"+hextile.getXY().X+"-y"+hextile.getXY().Y),
                    quaternion = quat
                };

                wall[added] = room.factory.Create(boxState, room);
                added++;
            }
            return wall;
        }



    }
}