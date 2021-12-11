using System;
using System.Numerics;
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
            SetCollidable(false);
            AddToContactListener();
            animation = new ObjectAnimation(this);
            animation.SetAnimation(new Vector3[] { new Vector3(0,-150,0), new Vector3(300,150 , 0) });
            animation.SetVelocity(1f);
            animation.Start();

            raycast = new Raycast(simulator, room);
            raycast.debugRayShape = true;
            raycast.distance = 0;
            raycast.SetRayShape(new PartRayShape());
            raycast.ObjectHitListeners += OnRayContact;

        }

        private void OnRayContact(PhyObject obj, Vector3 normal)
        {
            parent.OnDummyPartHit(this, obj, normal);
        }
        public void AddParent(Dummy dummy)
        {
            this.parent = dummy;
        }
        public static SphereState Build()
        {
            return new SphereState() { radius = 5, instantiate = false, type = "DummyPart" };
        }

        public void Update()
        {

            //SetPosition(parent.GetPosition());
            animation.Update();
            var pos = animation.RotateToDirection(parent.added.state.quaternion);
            raycast.Update(parent.GetPosition()+pos, new Vector3(0, 0, 0), parent.added.state.quaternion);




        }

    }
}