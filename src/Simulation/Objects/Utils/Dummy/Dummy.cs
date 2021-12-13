using System;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Constraints;
using BepuUtilities;

namespace QuixPhysics
{
    public class DummyBody
    {
        public DummyPart arm;
        Dummy dummy;
        public void Create(Dummy dummy)
        {
            this.dummy = dummy;
            arm = (DummyPart)dummy.room.factory.Create(DummyPart.Build(), dummy.room, null, true);
            arm.AddParent(dummy);
        }
    }
    public class Dummy : PhyObject
    {
        public delegate void DummyContactAction(PhyObject obj);
        public event DummyContactAction onContactListener;
        public PhyObject added;
        private ObjectAnimation animation;
        private DummyBody dummyBody;
        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            base.Load(bodyHandle, connectionState, simulator, state, guid, room);
            //SetCollidable(false);
            AddToContactListener();
            CreateDummyBody();
        }

        private void CreateDummyBody()
        {
            QuixConsole.Log("Create DymmyBody");
            dummyBody = new DummyBody();
            dummyBody.Create(this);
        }

        public override void OnContact<TManifold>(PhyObject obj, TManifold manifold)
        {
            onContactListener?.Invoke(obj);
        }

        public void AddToObject(PhyObject obj)
        {
            added = obj;
            QuixConsole.Log("height", added.GetHeight());
            room.simulator.Simulation.Solver.Add(handle.bodyHandle, obj.handle.bodyHandle,
             new Weld
             {
                 LocalOffset = new Vector3(0, -(added.GetHeight() * 1.5f), 0),
                 LocalOrientation = Quaternion.Identity,
                 SpringSettings = new SpringSettings(30, 1)
             });
        }

        public static BoxState Build()
        {
            return new BoxState()
            {
                halfSize = new Vector3(20, 40, 20),
                instantiate = false,
                mass = 1,
                type = "Dummy"
            };

        }

        public void OnDummyPartHit(DummyPart part, PhyObject hitObj, Vector3 normal)
        {
            if (this.added is Player2)
            {
                QuixConsole.Log(hitObj.state.type, hitObj.state.uID);
            }
        }
    }
}