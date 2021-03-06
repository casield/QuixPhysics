using System;
using System.Collections.Generic;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Trees;
using BepuUtilities;
using BepuUtilities.Memory;

namespace QuixPhysics
{
    public struct RayHit
    {
        public Vector3 Normal;
        public float T;
        public CollidableReference Collidable;
        public bool Hit;
    }
    public class HitHandler : IRayHitHandler
    {
        public List<RayHit> Hits;
        private Raycast raycast;
        public int IntersectionCount;

        public HitHandler(List<RayHit> hist, Raycast raycast)
        {
            Hits = hist;
            this.raycast = raycast;
        }
        public bool AllowTest(CollidableReference collidable)
        {
            return true;
        }

        public bool AllowTest(CollidableReference collidable, int childIndex)
        {
            return true;
        }
        public void OnRayHit(in RayData ray, ref float maximumT, float t, in Vector3 normal, CollidableReference collidable, int childIndex)
        {

            maximumT = t;
            var hit = Hits[ray.Id];
            
            if (t < hit.T)
            {
                var savedCollidable = hit.Collidable;
                if (hit.T == float.MaxValue)
                { ++IntersectionCount; }
                hit.Normal = normal;
                hit.T = t;
                hit.Collidable = collidable;
                hit.Hit = true;
                raycast.OnHit(hit);

            }

        }
    }
    class RayCastObject : PhyObject
    {
        //PhyObject broder;
        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            base.Load(bodyHandle, connectionState, simulator, state, guid, room);
            // broder = room.Create(new SphereState() { position = state.position, instantiate = true, radius = ((SphereState)state).radius });
            simulator.collidableMaterials[bodyHandle.staticHandle].collidable = false;
        }
    }
    public class Raycast
    {
        private Simulator simulator;
        public HitHandler hitHandler;
        private Room room;
        public delegate void ObjectHit(PhyObject obj, Vector3 normal);
        public event ObjectHit ObjectHitListeners;

        private IRayShape rayShape;
        public bool debugRayShape = false;
        private List<RayCastObject> debugObjects = new List<RayCastObject>();
        float debugTime = 0;
        public float distance = 1000;




        public Raycast(Simulator simulation, Room room)
        {

            this.simulator = simulation;
            this.room = room;
        }
        public void SetRayShape(IRayShape rayShape)
        {
            this.rayShape = rayShape;


            hitHandler = new HitHandler(CreateHits(), this);

        }

        private List<RayHit> CreateHits()
        {
            var hits = new List<RayHit>();
            for (int i = 0; i < rayShape.rayHits; i++)
            {
                if (debugRayShape)
                {
                    var obj = (RayCastObject)room.Create(new SphereState() { type = "RayCastObject", mass = 0, instantiate = true, radius = 10 });
                    debugObjects.Add(obj);
                }
                hits.Add(new RayHit() { T = 200000 });
            }

            return hits;
        }
        public void OnHit(RayHit hit)
        {

            if (hit.Hit)
            {
                PhyObject phy;
                if (hit.Collidable.Mobility == CollidableMobility.Dynamic)
                {
                    phy = room.factory.objectsHandlers[hit.Collidable.BodyHandle];
                }
                else
                {
                    phy = room.factory.staticObjectsHandlers[hit.Collidable.StaticHandle];
                }
                if (phy != null && !(phy is RayCastObject))
                {
                    
                    ObjectHitListeners?.Invoke(phy,hit.Normal);
                }
            }

        }

        public void Update(in Vector3 origin, Vector3 direction, Quaternion quaternion)
        {
            rayShape.SetOriginDirection(origin, direction, quaternion);
            for (int i = 0; i < hitHandler.Hits.Count; i++)
            {
                Vector3 m_origin;
                Vector3 m_dir;

                rayShape.SetRay(i, out m_origin, out m_dir);
                simulator.Simulation.RayCast<HitHandler>(m_origin, m_dir, distance, ref hitHandler, i);
                if (debugRayShape)
                {
                    if (debugTime > distance)
                    {
                        debugTime = 0;
                    }

                    debugTime += 0.1f;
                    //QuixConsole.Log("mdir", m_dir);
                    if (!(float.IsNaN(m_dir.X) && float.IsNaN(m_dir.Z)))
                    {
                        debugObjects[i].SetPosition(m_origin + (m_dir));
                    }

                }

            }
        }

    }
    public interface IRayShape
    {
        int rayHits { get; set; }
        Vector3 origin { get; set; }
        Vector3 direction { get; set; }
        Quaternion rotation { get; set; }

        void SetOriginDirection(Vector3 origin, Vector3 direction, Quaternion rotation)
        {
            this.origin = origin;
            this.direction = direction;
            this.rotation = rotation;
        }
        /// <summary>
        /// Set the raycast
        /// </summary>
        /// <param name="index"></param>
        /// <param name="origin"></param>
        /// <param name="direction"></param>
        void SetRay(int index, out Vector3 origin, out Vector3 direction);
    }
    class CircleRayShape : IRayShape
    {
        public int rayHits { get; set; }
        public Vector3 origin { get; set; }
        public Vector3 direction { get; set; }
        public Quaternion rotation { get; set; }

        private float circleDistance;
        private Raycast rayCast;

        public CircleRayShape(int _rayHits, float circleDistance, Raycast raycast)
        {
            rayHits = _rayHits;
            this.circleDistance = circleDistance;
            this.rayCast = raycast;
        }

        public void SetRay(int index, out Vector3 origin, out Vector3 direction)
        {
            var angle = (MathF.PI * 2 / rayHits) * index;
            var transform = Matrix3x3.CreateFromQuaternion(rotation * new Quaternion(0, 0, 0.707f, 0.707f));

            var x = MathF.Cos(angle);
            var y = MathF.Sin(angle);
            var newOr = new Vector3(x, 0, y);

            Matrix3x3.Transform(newOr, transform, out Vector3 outOrigin);

            origin = this.origin + (Vector3.Normalize(outOrigin) * (circleDistance));
            direction = Vector3.Normalize(this.direction) * (rayCast.distance);
        }

    }

    public class SpiralRayShape : IRayShape
    {
        public int rayHits { get; set; }
        public Vector3 origin { get; set; }
        public Vector3 direction { get; set; }
        public Quaternion rotation { get; set; }

        private float circleDistance;
        private Raycast rayCast;

        public SpiralRayShape(int _rayHits, float circleDistance, Raycast raycast)
        {
            rayHits = _rayHits;
            this.circleDistance = circleDistance;
            this.rayCast = raycast;
        }

        public void SetRay(int index, out Vector3 origin, out Vector3 direction)
        {
            var spiralDistance = circleDistance*MathF.Sqrt(index);
            var angle = index;
            var transform = Matrix3x3.CreateFromQuaternion(rotation * new Quaternion(0, 0, 0.707f, 0.707f));

            var x = MathF.Cos(angle);
            var y = MathF.Sin(angle);
            var newOr = new Vector3(x, 0, y);

            Matrix3x3.Transform(newOr, transform, out Vector3 outOrigin);

            origin = this.origin + (Vector3.Normalize(outOrigin) * (spiralDistance));
            direction = Vector3.Normalize(this.direction) * (rayCast.distance);
        }
    }
    class OneRayShape : IRayShape
    {
        public int rayHits { get; set; }

        private Raycast raycast;

        public Vector3 origin { get; set; }
        public Vector3 direction { get; set; }
        public Quaternion rotation { get; set ; }

        public OneRayShape(Raycast raycast){
            rayHits = 1;
            this.raycast = raycast;
        }

        public void SetRay(int index, out Vector3 origin, out Vector3 direction)
        {
           origin = this.origin;
           direction = Vector3.Normalize(this.direction)*raycast.distance;
        }
    }
}