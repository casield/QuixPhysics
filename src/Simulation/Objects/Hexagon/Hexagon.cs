
using System;
using System.Numerics;
using QuixPhysics.Hextiles;

namespace QuixPhysics
{
    public class Hexagon : MeshBox
    {

        public static float _SIZE = 1000;
        public static BoxState Build(Vector3 position){
            BoxState state = new BoxState(){
                halfSize=new Vector3(_SIZE),
                mesh="Stadiums/Hexagons/Hextile",
                instantiate=true,
                position=position,
                type="Hexagon"
            };

            return state;
        }

        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            base.Load(bodyHandle, connectionState, simulator, state, guid, room);

        }

    

    } 
}