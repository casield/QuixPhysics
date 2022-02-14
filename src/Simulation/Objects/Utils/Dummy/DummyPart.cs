using System;
using System.Numerics;
using BepuPhysics.Constraints;
using BepuUtilities;

namespace QuixPhysics
{
    public class DummyPart : PhyObject
    {
        public ObjectAnimation animation;
        private Dummy parent;
        private Raycast raycast;
        public bool isActive = true;

        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            base.Load(bodyHandle, connectionState, simulator, state, guid, room);
            SetCollidable(true);
            animation = new ObjectAnimation(this);
            animation.SetAnimation(new Vector3[] { new Vector3(-30,0,-30),new Vector3(-30,0,-40) });
            animation.SetVelocity(1f);
            animation.Start();
            AddWorker(new PhyInterval(1, simulator)).Completed += Update;

        }

        private void Update()
        {
            if (isActive)
            {
                animation.Update();
            }
            else
            {
                animation.animationPosition = 0;

            }
            SetPosition(parent.GetPosition() + animation.RotateToDirection(parent.added.state.quaternion));
            //


        }
        public bool HasFinishAnimation()
        {
            return animation.animationPosition == animation.Animation.Length - 1;
        }
        public void AddParent(Dummy dummy)
        {
            this.parent = dummy;
        }
        public static SphereState Build()
        {
            return new SphereState() { radius = 5, instantiate = true, type = "DummyPart", mass = 10 };
        }

    }
}