using System;
using BepuPhysics;

namespace QuixPhysics
{
    public class Hole : PhyObject
    {

        public Hole()
        {

        }
        public override void OnContact<TManifold>(PhyObject obj, TManifold manifold)
        {
            if (obj is GolfBall2)
            {
                QuixConsole.Log("Oh si la pelota entro!");
            }
        }
        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            base.Load(bodyHandle, connectionState, simulator, state, guid, room);
            room.factory.OnContactListeners.Add(guid, this);


        }





    }

}