using System;
using System.Numerics;
using BepuPhysics.Constraints;
using BepuUtilities;

namespace QuixPhysics
{
    class PartRayShape : IRayShape
    {
        public int rayHits { get; set; }
        public Vector3 origin { get; set; }
        public Vector3 direction { get; set; }
        public Quaternion rotation { get; set; }

        public PartRayShape()
        {
            rayHits = 1;
            origin = new Vector3();
            direction = new Vector3();
        }

        public void SetRay(int index, out Vector3 origin, out Vector3 direction)
        {
            var transform = Matrix3x3.CreateFromQuaternion(rotation);

            Matrix3x3.Transform(this.origin, transform, out Vector3 outOrigin);
            origin = this.origin + (Vector3.Normalize(outOrigin));
            direction = Vector3.Normalize(this.direction) * (10);
        }
    }
    public class DummyPart : PhyObject
    {
        private ObjectAnimation animation;
        private Dummy parent;
        private Raycast raycast;

        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            base.Load(bodyHandle, connectionState, simulator, state, guid, room);
            SetCollidable(true);
            // AddToContactListener();
            animation = new ObjectAnimation(this);
            animation.SetAnimation(new Vector3[] { new Vector3(70, 0, 0), new Vector3(150, 0, 0) });
            animation.SetVelocity(1f);
            animation.Start();
            AddWorker(new PhyInterval(1, simulator)).Completed += Update;

        }

        private void Update()
        {
            animation.Update();

           //  bodyReference.ApplyLinearImpulse(new Vector3(300,0,0));

              SetPosition(parent.GetPosition()+animation.RotateToDirection(parent.added.state.quaternion));

        }

        public override void OnContact<TManifold>(PhyObject obj, TManifold manifold)
        {
        }
        public void AddParent(Dummy dummy)
        {
            this.parent = dummy;
          /* var constrain = new BallSocket
            {
                LocalOffsetA = new Vector3(60, -80, 0),
                LocalOffsetB = new Vector3(0, 0, 0),
                SpringSettings = new SpringSettings(30, 1)
            };
            room.simulator.Simulation.Solver.Add(handle.bodyHandle, parent.handle.bodyHandle, constrain);*/

        }
        public static SphereState Build()
        {
            return new SphereState() { radius = 5, instantiate = true, type = "DummyPart", mass = 0 };
        }

    }
}