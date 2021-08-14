using System;
using System.Numerics;

namespace QuixPhysics
{
    public class ShapeShifterItem : Item
    {
        public string Mesh = "Objects/Items/Gematorium";
        public override void Instantiate(Room room, Vector3 position)
        {
            var sp = new BoxState() { instantiate = true, mass = 0, halfSize=new Vector3(30,30,30), type = "ShapeShifterItem", mesh =Mesh, position = position };
            room.factory.Create(sp, room, this);
        }

        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            base.Load(bodyHandle, connectionState, simulator, state, guid, room);
            simulator.collidableMaterials[bodyHandle.staticHandle].collidable = false;
        }
    }
}