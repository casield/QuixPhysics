using System;
using BepuPhysics;
using BepuPhysics.Constraints;

namespace QuixPhysics
{
    public class GolfBall2 : PhyObject
    {

        public BodyReference reference;

        public GolfBall2()
        {

        }
        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state,Guid guid)
        {
            base.Load(bodyHandle, connectionState, simulator, state,guid);

            simulator.collidableMaterials[bodyHandle.bodyHandle].collidable = true;
            simulator.collidableMaterials[bodyHandle.bodyHandle].SpringSettings = new SpringSettings(5f, .000001f);


            reference = GetReference();
        }
    }
}