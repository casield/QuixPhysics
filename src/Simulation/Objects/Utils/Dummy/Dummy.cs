using System;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Constraints;
using BepuUtilities;

namespace QuixPhysics
{
    public class Dummy : PhyObject
    {
        public delegate void DummyContactAction(PhyObject obj);
        public event DummyContactAction onContactListener;
        public PhyObject added;
        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            base.Load(bodyHandle, connectionState, simulator, state, guid, room);
            SetCollidable(false);
            AddToContactListener();

        }

        public override void OnContact<TManifold>(PhyObject obj, TManifold manifold)
        {
            onContactListener?.Invoke(obj);
        }

        public void AddToObject(PhyObject obj)
        {
            added = obj;

            AddWorker(new PhyInterval(1, simulator)).Completed += Update;
        }

        public static BoxState Build()
        {
            return new BoxState()
            {
                halfSize = new Vector3(20, 40, 20),
                instantiate = false,
                type = "Dummy"
            };

        }

        private void Update()
        {
            SetPosition(added.GetPosition() + new Vector3(0, 30, 0));
        }
    }
}