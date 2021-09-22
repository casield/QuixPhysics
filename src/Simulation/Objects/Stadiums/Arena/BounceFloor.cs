using System;
using System.Numerics;
using BepuUtilities;

namespace QuixPhysics
{
    public class BounceFloor : PhyObject
    {
        PhyObject lastBounced;
        PhyWaiter waiter;
        private Vector3 maxVelocity = new Vector3(100);

        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            base.Load(bodyHandle, connectionState, simulator, state, guid, room);
            room.factory.OnContactListeners.Add(guid, this);
            QuixConsole.Log("Bounce floor");
            waiter = new PhyWaiter(3000);
        }
        public override void OnContact<TManifold>(PhyObject obj, TManifold manifold)
        {
            if (lastBounced == obj)
            {
                if (!waiter.Tick())
                {
                    waiter.Reset();
                    lastBounced = null;
                }
            }
            else
            {
                if (obj.state.mass != 0)
                {
                    var normal = (manifold.GetNormal(ref manifold, 0));
                    if (obj is Player2)
                    {
                        Player2 pl = obj as Player2;
                        pl.actionsManager.jumpAction.ResetNormal();
                    }
                    Bounce(obj, normal);
                    lastBounced = obj;
                }
            }


        }
        private void Bounce(PhyObject obj, Vector3 normal)
        {
            if (normal != Vector3.Zero)
            {
                obj.bodyReference.Velocity.Linear += Vector3.Normalize(normal) * (obj.state.mass * 3);
                obj.bodyReference.Velocity.Linear.X = Math.Clamp(obj.bodyReference.Velocity.Linear.X,-maxVelocity.X,maxVelocity.X);
                obj.bodyReference.Velocity.Linear.Y = Math.Clamp(obj.bodyReference.Velocity.Linear.Y,-maxVelocity.Y,maxVelocity.Y);
                obj.bodyReference.Velocity.Linear.Z = Math.Clamp(obj.bodyReference.Velocity.Linear.Z,-maxVelocity.Z,maxVelocity.Z);
              //  QuixConsole.Log("Bounce", normal, obj.bodyReference.Velocity.Linear.Length());
            }


        }
    }
}