
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
        public static BoxState Build(Vector3 position)
        {
            BoxState state = new BoxState()
            {
                halfSize = new Vector3(_SIZE),
                mesh = "Stadiums/Hexagons/Hextile",
                instantiate = true,
                position = position-Hextile._PADDDING,
                type = "Hexagon"
            };

            return state;
        }

        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            base.Load(bodyHandle, connectionState, simulator, state, guid, room);

        }

        public void AddHextile(Hextile tile)
        {
            hextile = tile;

            AddWorker(new PhyInterval(100, simulator)).Completed += () =>
            {
                var gridpos =  hextile.getXY();
                var oddX = tile.IsOdd((int)gridpos.X);
                var oddY = tile.IsOdd((int)gridpos.Y);
                SendObjectMessage("(" + hextile.getXY() + ") - OddX:"+oddX+" / oddY:"+oddY);
            };
            if(hextile.IsOdd((int)hextile.getXY().Y)){
                ChangeColor("#FF0000");
            }

        }
        #region Hexagon Calculation
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

        public Vector3 GetSidePoint(float point)
        {
            var _size = _SIZE / 2;
            var angle_deg = 60 * point - 30;
            var angle_rad = (MathF.PI / 180) * angle_deg;
            var hexagon_pos = GetPosition();

            return new Vector3(hexagon_pos.X + _size * MathF.Cos(angle_rad),
            hexagon_pos.Y,
            hexagon_pos.Z + _size * MathF.Sin(angle_rad));
        }

        public Vector3 GetPointDirection(Vector3 point)
        {
            return -(Vector3.Normalize(point - GetPosition()));
        }
        #endregion

        #region Game Functions
        public PhyObject[] AddWalls(int[] sides)
        {

            List<PhyObject> wall = new List<PhyObject>();
            foreach (var index in sides)
            {
                var XY = hextile.getXY();
                var pos = GetSidePoint(index) + new Vector3(0, Hexagon._SIZE / 2, 0);
                var dir = GetPointDirection(pos);
                var rot = MathF.Atan2(dir.X, dir.Z);
                var quat = Quaternion.CreateFromYawPitchRoll(rot, 0, 0);


                var boxState = new BoxState()
                {

                    halfSize = new Vector3(hextile.GetWidth() * 1.15f, Hexagon._SIZE / 2, 20),
                    instantiate = true,
                    position = pos,
                    type = "Hexwall" + index + ("-x" + XY.X + "-y" + XY.Y),
                    quaternion = quat
                };

                wall.Add(room.factory.Create(boxState, room));


            }
            return wall.ToArray();
        }

        public void ChangeColor(string color){
            SendObjectMessage("c:"+color);
        }

        #endregion



    }
}